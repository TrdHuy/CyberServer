using cyber_server.@base;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace cyber_server.implements.log_manager
{

    internal class ServerLogManager : IServerModule
    {
        public enum ConsoleLogActionChanged
        {
            ADDED, CLEAR
        }
        private static readonly Brush DEFAULT_CONSOLE_LOG_COLOR = Brushes.White;

        private string _consoleLogCache;

        private StringBuilder _rawLogBuilder;
        private string _logFileName;
        private ObservableQueue<Func<bool>> _taskQueue;
        private SemaphoreSlim _mutex = new SemaphoreSlim(1, 1);

        public delegate void ServerLogChangedHandler(object sender, ConsoleLogActionChanged action, string newLog, string newLine);

        public event ServerLogChangedHandler ServerLogChanged;

        public Brush ConsoleLogColor { get; private set; } = DEFAULT_CONSOLE_LOG_COLOR;

        public static ServerLogManager Current
        {
            get
            {
                return ServerModuleManager.SLM_Instance;
            }
        }

        private ServerLogManager()
        {

        }

        public void Dispose()
        {
            _taskQueue.CollectionChanged -= HandleTaskQueueChange;
        }

        public void OnModuleInit()
        {
            var dateTimeNow = DateTime.Now.ToString("ddMMyyHHmmss");
            _logFileName =
                   Assembly.GetCallingAssembly().GetName().Name + "_" +
                   Assembly.GetCallingAssembly().GetName().Version + "_" +
                   dateTimeNow + ".txt";

            _consoleLogCache = "";
            _rawLogBuilder = new StringBuilder();

            _taskQueue = new ObservableQueue<Func<bool>>();
            _taskQueue.CollectionChanged += HandleTaskQueueChange;

            AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        }

        public void AppendConsoleErrorLine(string newLine, int tab = 0)
        {
            ConsoleLogColor = Brushes.Red;
            AppendConsoleLogLine(newLine, tab);
            ConsoleLogColor = DEFAULT_CONSOLE_LOG_COLOR;
        }

        public void AppendConsoleWarningLine(string newLine, int tab = 0)
        {
            ConsoleLogColor = Brushes.OrangeRed;
            AppendConsoleLogLine(newLine, tab);
            ConsoleLogColor = DEFAULT_CONSOLE_LOG_COLOR;
        }

        public void AppendConsoleDebugLine(string newLine, int tab = 0)
        {
            ConsoleLogColor = Brushes.GreenYellow;
            AppendConsoleLogLine(newLine, tab);
            ConsoleLogColor = DEFAULT_CONSOLE_LOG_COLOR;
        }

        public void AppendConsoleHyperLine(string newLine, int tab = 0)
        {
            ConsoleLogColor = Brushes.DeepSkyBlue;
            AppendConsoleLogLine(newLine, tab);
            ConsoleLogColor = DEFAULT_CONSOLE_LOG_COLOR;
        }

        public void AppendConsoleLogLine(string newLine, Brush logColor, int tab = 0)
        {
            ConsoleLogColor = logColor;
            AppendConsoleLogLine(newLine, tab);
            ConsoleLogColor = DEFAULT_CONSOLE_LOG_COLOR;
        }

        public void AppendConsoleLogLine(string newLine, int tab = 0)
        {
            var newLogLine = "";
            if (tab > 0)
            {
                newLogLine += "|";
                for (int i = 0; i < tab; i++)
                {
                    newLogLine += "__";
                }
            }
            newLogLine += newLine + "\n";
            _consoleLogCache = _consoleLogCache + newLogLine;

            ServerLogChanged?.Invoke(this, ConsoleLogActionChanged.ADDED, _consoleLogCache, newLogLine);
        }

        public void ClearConsoleLog()
        {
            _consoleLogCache = "";
            ServerLogChanged?.Invoke(this, ConsoleLogActionChanged.CLEAR, _consoleLogCache, "");
        }

        public void D(string log, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "")
        {
            var task = GenerateTask("D", filePath, caller, log);
            _taskQueue.Enqueue(task);
        }

        public void E(string log, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "")
        {
            var task = GenerateTask("E", filePath, caller, log);
            _taskQueue.Enqueue(task);
        }

        public void I(string log, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "")
        {
            var task = GenerateTask("I", filePath, caller, log);
            _taskQueue.Enqueue(task);
        }

        public void W(string log, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "")
        {
            var task = GenerateTask("W", filePath, caller, log);
            _taskQueue.Enqueue(task);
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            ExportLogFile();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var task1 = GenerateTask("F", "\\ServerLogManager.cs", "[UnhandledException]", ":" + e.ExceptionObject.ToString());
            _taskQueue.Enqueue(task1);
            var task2 = GenerateTask("", "", "", "", true);
            _taskQueue.Enqueue(task2);
        }

        private async void HandleTaskQueueChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                await ProcessQueue();
            }
        }

        private async Task ProcessQueue()
        {
            await _mutex.WaitAsync();
            try
            {
                int reDoWorkCounter = 0;

                while (_taskQueue.Count >= 1)
                {
                    var taskFormQueue = _taskQueue.Peek();
                    reDoWorkCounter++;
                    var success = taskFormQueue.Invoke();
                    if (success || reDoWorkCounter >= 3)
                    {
                        var removeTask = _taskQueue.Dequeue();
                        removeTask = null;
                        reDoWorkCounter = 0;
                    }
                }
            }
            finally
            {
                _mutex.Release();
            }
        }

        private Func<bool> GenerateTask(string logLV,
            string filePath,
            string callMemberName,
            string message,
            bool isExportLogFile = false)
        {
            var task = !isExportLogFile ?
                new Func<bool>(() =>
                {
                    return WriteLog(logLV, filePath, callMemberName, message);
                }) :
             new Func<bool>(() =>
             {
                 var resExportLog = ExportLogFile();
                 return resExportLog;
             });

            return task;

        }

        private bool WriteLog(string logLv, string classFilePath, string methodName, string message)
        {
            try
            {
                var classFileName = classFilePath.Substring(classFilePath.LastIndexOf("\\") + 1);
                var className = classFileName.Substring(0, classFileName.IndexOf("."));
                var dateTimeNow = DateTime.Now.ToString("dd-MM HH:mm:ss:ffffff");

                var newLogLine = dateTimeNow + " " +
                    logLv + " " +
                    className + " " + methodName + ":" + message;
                _rawLogBuilder.AppendLine(newLogLine);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool ExportLogFile()
        {
            try
            {
                var filePath = "logs\\" + _logFileName;
                if (!Directory.Exists("logs"))
                {
                    Directory.CreateDirectory("logs");
                }
                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Dispose();
                }
                var logContent = _rawLogBuilder?.ToString();
                if (!string.IsNullOrEmpty(logContent))
                {
                    File.AppendAllText(filePath, logContent);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

    }

    internal class ObservableQueue<T> : Queue<T>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public new void Enqueue(T item)
        {
            base.Enqueue(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public new T Dequeue()
        {
            var x = base.Dequeue();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, x));
            return x;
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            RaiseCollectionChanged(e);
        }

        private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }
    }
}
