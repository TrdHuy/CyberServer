using cyber_server.implements.log_manager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace cyber_server.views.usercontrols.others
{
    public class CurrentTaskManager
    {
        public const string ADD_VERSION_TASK_TYPE_KEY = "ADD_VERSION_TASK_TYPE_KEY";
        public const string DELETE_VERSION_TASK_TYPE_KEY = "DELETE_VERSION_TASK_TYPE_KEY";
        public const string ADD_PLUGIN_TASK_TYPE_KEY = "ADD_PLUGIN_TASK_TYPE_KEY";
        public const string ADD_TOOL_TASK_TYPE_KEY = "ADD_TOOL_TASK_TYPE_KEY";
        public const string RELOAD_TOOL_TASK_TYPE_KEY = "RELOAD_TOOL_TASK_TYPE_KEY";
        public const string RELOAD_PLUGIN_TASK_TYPE_KEY = "RELOAD_PLUGIN_TASK_TYPE_KEY";
        public const string DELETE_PLUGIN_TASK_TYPE_KEY = "DELETE_PLUGIN_TASK_TYPE_KEY";
        public const string DELETE_TOOL_TASK_TYPE_KEY = "DELETE_TOOL_TASK_TYPE_KEY";
        public const string IMPORT_TOOL_TASK_TYPE_KEY = "IMPORT_TOOL_TASK_TYPE_KEY";
        public const string MODIFI_PLUGIN_TASK_TYPE_KEY = "MODIFI_PLUGIN_TASK_TYPE_KEY";
        public const string MODIFI_VERSION_TASK_TYPE_KEY = "MODIFI_VERSION_TASK_TYPE_KEY";
        public const string MODIFI_TOOL_TASK_TYPE_KEY = "MODIFI_TOOL_TASK_TYPE_KEY";
        public const string SYNC_TASK_TYPE_KEY = "SYNC_TASK_TYPE_KEY";
        public const string SAVE_CERTIFICATE_TO_DB_TASK_TYPE_KEY = "SAVE_CERTIFICATE_TO_DB_TASK_TYPE_KEY";
        public const string CHECK_VALIDATION_CERTIFICATE_TYPE_KEY = "CHECK_CERTIFICATE_INVAILD_TYPE_KEY";
        public const string RELOAD_CERTIFICATE_FROM_DB_TASK_TYPE_KEY = "RELOAD_CERTIFICATE_FROM_DB_TASK_TYPE_KEY";
        public const string DELETE_CERTIFICATE_FROM_DB_TASK_TYPE_KEY = "DELETE_CERTIFICATE_FROM_DB_TASK_TYPE_KEY";
        
        private TextBlock _currentTaskNameTb;
        private Path _waitingIconPath;
        private class TaskInfo
        {
            public SemaphoreSlim semaphoreSlim { get; set; }
            public string Name { get; set; }
        }

        private Dictionary<string, TaskInfo> _taskSemaphoreMap = new Dictionary<string, TaskInfo>();
        private int _currentTaskCount = 0;
        private List<TaskInfo> _handlingTaskQueue = new List<TaskInfo>();
        private int CurrentTaskCount
        {
            get => _currentTaskCount;
            set
            {
                _currentTaskCount = value;
                if (_currentTaskCount == 1)
                {
                    _waitingIconPath.Visibility = Visibility.Visible;
                    _currentTaskNameTb.Text = "'" + _handlingTaskQueue[0].Name + "'";
                }
                else if (_currentTaskCount > 1)
                {
                    _waitingIconPath.Visibility = Visibility.Visible;
                    _currentTaskNameTb.Text = "Handling " + _currentTaskCount + " tasks";
                }
                else
                {
                    _waitingIconPath.Visibility = Visibility.Hidden;
                    _currentTaskNameTb.Text = "";
                }

            }
        }

        public CurrentTaskManager(TextBlock tb, Path wIP)
        {
            _currentTaskNameTb = tb;
            _waitingIconPath = wIP;
            wIP.Visibility = Visibility.Hidden;
        }

        public void GenerateNewTaskSemaphore(string taskTypeKey, string taskName, int maxCore, int initCore)
        {
            if (!_taskSemaphoreMap.ContainsKey(taskTypeKey))
            {
                var smp = new SemaphoreSlim(initCore, maxCore);
                var task = new TaskInfo()
                {
                    Name = taskName,
                    semaphoreSlim = smp
                };
                _taskSemaphoreMap.Add(taskTypeKey, task);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskTypeKey"></param>
        /// <param name="mainFunc"></param>
        /// <param name="delay">Thời gian delay sau khi hoàn thành task</param>
        /// <param name="executeTime">Thời gian dự kiến hoàn thành của task, nếu thời gian thực hiện ít hơn dự kiến, nó sẽ delay khoảng thời gian còn dư lại</param>
        /// <param name="bypassIfSemaphoreNotAvaild">Bỏ qua task ko thực hiện khi semaphore ko có sẵn</param>
        /// <returns></returns>
        public async Task ExecuteTask(string taskTypeKey
            , Action mainFunc
            , int delay = 0
            , int executeTime = 0
            , bool bypassIfSemaphoreNotAvaild = false)
        {
            if (_taskSemaphoreMap.ContainsKey(taskTypeKey))
            {
                var smp = _taskSemaphoreMap[taskTypeKey].semaphoreSlim;
                if (bypassIfSemaphoreNotAvaild)
                {
                    if (smp.CurrentCount == 0)
                    {
                        return;
                    }
                }

                Stopwatch watch = Stopwatch.StartNew();
                await smp.WaitAsync();
                try
                {
                    _handlingTaskQueue.Insert(0, _taskSemaphoreMap[taskTypeKey]);
                    CurrentTaskCount++;
                    mainFunc?.Invoke();
                }
                catch
                {

                }
                finally
                {
                    watch.Stop();
                    var executedTime = watch.ElapsedMilliseconds;
                    var timeLeft = (int)(executeTime - executedTime);
                    if (timeLeft > 0)
                    {
                        await Task.Delay(timeLeft);
                    }
                    await Task.Delay(delay);

                    _handlingTaskQueue.Remove(_taskSemaphoreMap[taskTypeKey]);
                    CurrentTaskCount--;
                    smp.Release();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskTypeKey"></param>
        /// <param name="mainFunc"></param>
        /// <param name="delay">Thời gian delay sau khi hoàn thành task</param>
        /// <param name="executeTime">Thời gian dự kiến hoàn thành của task, nếu thời gian thực hiện ít hơn dự kiến, nó sẽ delay khoảng thời gian còn dư lại</param>
        /// <param name="bypassIfSemaphoreNotAvaild">Bỏ qua task ko thực hiện khi semaphore ko có sẵn</param>
        /// <returns></returns>
        public async Task ExecuteTask(string taskTypeKey
            , Func<Task> mainFunc
            , int delay = 0
            , int executeTime = 0
            , bool bypassIfSemaphoreNotAvaild = false)
        {
            if (_taskSemaphoreMap.ContainsKey(taskTypeKey))
            {
                var smp = _taskSemaphoreMap[taskTypeKey].semaphoreSlim;
                if (bypassIfSemaphoreNotAvaild)
                {
                    if (smp.CurrentCount == 0)
                    {
                        return;
                    }
                }

                Stopwatch watch = Stopwatch.StartNew();
                await smp.WaitAsync();
                try
                {
                    _handlingTaskQueue.Insert(0, _taskSemaphoreMap[taskTypeKey]);
                    CurrentTaskCount++;
                    await mainFunc?.Invoke();
                }
                catch (Exception ex)
                {
                    ServerLogManager.Current.E(ex.ToString());
                }
                finally
                {
                    watch.Stop();
                    var executedTime = watch.ElapsedMilliseconds;
                    var timeLeft = (int)(executeTime - executedTime);
                    if (timeLeft > 0)
                    {
                        await Task.Delay(timeLeft);
                    }
                    await Task.Delay(delay);

                    _handlingTaskQueue.Remove(_taskSemaphoreMap[taskTypeKey]);
                    CurrentTaskCount--;
                    smp.Release();
                }
            }
        }
    }

    /// <summary>
    /// Interaction logic for TaskHandlingPanel.xaml
    /// </summary>
    public partial class TaskHandlingPanel : UserControl
    {
        private CurrentTaskManager _taskManager;
        public TaskHandlingPanel()
        {
            InitializeComponent();
            _taskManager = new CurrentTaskManager(PART_CurrentTaskNameTb, PART_WaitingIconPath);
        }

        public void GenerateTaskSemaphore(string taskKey, string taskName, int maxCore, int initCore)
        {
            _taskManager.GenerateNewTaskSemaphore(taskKey, taskName, maxCore, initCore);
        }

        public async Task ExecuteTask(string taskTypeKey
            , Func<Task> mainFunc
            , int delay = 0
            , int executeTime = 0
            , bool bypassIfSemaphoreNotAvaild = false)
        {
            await _taskManager.ExecuteTask(taskTypeKey, mainFunc, delay, executeTime, bypassIfSemaphoreNotAvaild);
        }
    }
}
