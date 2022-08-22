using cyber_server.implements.db_manager;
using cyber_server.implements.plugin_manager;
using cyber_server.view_models.plugin_version_item;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace cyber_server.views.windows
{
    public class CurrentTaskManager
    {
        public const string ADD_VERSION_TASK_TYPE_KEY = "ADD_VERSION_TASK_TYPE_KEY";
        public const string ADD_PLUGIN_TASK_TYPE_KEY = "ADD_PLUGIN_TASK_TYPE_KEY";

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
    }
    /// <summary>
    /// Interaction logic for CyberServerWindow.xaml
    /// </summary>
    public partial class CyberServerWindow : Window
    {
        private CurrentTaskManager _taskManager;
        public ObservableCollection<PluginVersionItemViewModel> VersionSource { get; set; }
            = new ObservableCollection<PluginVersionItemViewModel>();

        public CyberServerWindow()
        {
            InitializeComponent();
            _taskManager = new CurrentTaskManager(PART_CurrentTaskNameTb, PART_WaitingIconPath);
            _taskManager.GenerateNewTaskSemaphore(CurrentTaskManager.ADD_VERSION_TASK_TYPE_KEY, "Adding new version", 1, 1);
            _taskManager.GenerateNewTaskSemaphore(CurrentTaskManager.ADD_PLUGIN_TASK_TYPE_KEY, "Adding new plugin", 1, 1);
        }

        private async void HandleAddPluginTabButtonEvent(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            switch (btn.Name)
            {
                case "SUB_DeleteVersionItem":
                    var picontext = btn.DataContext as PluginVersionItemViewModel;
                    if (picontext != null)
                    {
                        VersionSource.Remove(picontext);
                    }
                    break;
                case "PART_CreateNewVersionBtn":
                    await _taskManager.ExecuteTask(CurrentTaskManager.ADD_VERSION_TASK_TYPE_KEY,
                        mainFunc: async () =>
                        {
                            var newIndex = GetIndexOfNewVersion();
                            if (newIndex != -1)
                            {
                                await Task.Delay(1000);
                                var pluginVer = new PluginVersionItemViewModel()
                                {
                                    Version = PART_PluginVersionTb.Text,
                                    FilePath = PART_PathToPluginTextbox.Text,
                                    DatePublished = PART_DatePublisedDP.Text,
                                    Description = PART_VersionDesTb.Text,
                                };

                                VersionSource.Insert(newIndex, pluginVer);
                                if (VersionSource.Count > 0)
                                {
                                    PART_ListVersionCbx.SelectedIndex = 0;
                                }
                            }
                        },
                        executeTime: 0,
                        bypassIfSemaphoreNotAvaild: true);
                    break;
                case "PART_AddPluginToDb":
                    await _taskManager.ExecuteTask(CurrentTaskManager.ADD_PLUGIN_TASK_TYPE_KEY,
                       mainFunc: async () =>
                       {
                           if (IsMeetConditionToAddPlugToDb())
                           {
                               var isPluginKeyExist = false;
                               var pluginKey = PART_PluginKeyTb.Text;
                               await CyberDbManager.Current.RequestDbContextAsync((context) =>
                               {
                                   isPluginKeyExist = context
                                    .Plugins
                                    .Any<Plugin>(p =>
                                        p.StringId.Equals(pluginKey, StringComparison.CurrentCultureIgnoreCase));
                               });
                               if (isPluginKeyExist)
                               {
                                   MessageBox.Show("Key này đã tồn tại\nHãy chọn key khác!");
                                   return;
                               }

                               var plugin = new Plugin();
                               plugin.StringId = pluginKey;
                               plugin.Name = PART_PluginNameTb.Text;
                               plugin.Author = PART_PluginAuthorTb.Text;
                               plugin.Description = PART_PluginDesTb.Text;
                               plugin.ProjectURL = PART_PluginURLTb.Text;
                               plugin.IconSource = PART_PluginIconSourceTb.Text;
                               plugin.IsAuthenticated = PART_PluginIsAuthenticatedCb.IsChecked ?? false;
                               plugin.Downloads = 0;
                               var isCopFileSuccess = true;

                               foreach (var version in VersionSource)
                               {
                                   var pv = version.BuildPluginVersionFromViewModel(plugin.StringId);
                                   isCopFileSuccess = CyberPluginManager
                                        .Current
                                        .CopyPluginToServerLocation(version.GetVersionSourceFilePath()
                                            , pv.FilePath);
                                   if (!isCopFileSuccess)
                                   {
                                       CyberPluginManager.Current.DeletePluginDirectory(plugin.Name);
                                       break;
                                   }
                               }

                               if (isCopFileSuccess)
                               {
                                   await CyberDbManager.Current.RequestDbContextAsync((context) =>
                                   {
                                       context.Plugins.Add(plugin);
                                       context.SaveChanges();
                                   });
                               }
                           }
                           else
                           {
                               MessageBox.Show("Điền các trường còn thiếu!");
                           }

                       },
                       executeTime: 1000,
                       bypassIfSemaphoreNotAvaild: true);
                    break;
                case "PART_OpenPluginFileChooser":
                    var ofd = new OpenFileDialog();
                    if (ofd.ShowDialog() == true)
                        PART_PathToPluginTextbox.Text = ofd.FileName;
                    break;
            }
        }

        private bool IsMeetConditionToAddPlugToDb()
        {
            if (PART_PluginNameTb.Text == ""
                || PART_PluginAuthorTb.Text == ""
                || PART_PluginURLTb.Text == ""
                || PART_PluginKeyTb.Text == ""
                || VersionSource.Count == 0) return false;

            return true;
        }

        private int GetIndexOfNewVersion()
        {
            Version newVersion = new Version();
            try
            {
                newVersion = Version.Parse(PART_PluginVersionTb.Text);
            }
            catch
            {
                MessageBox.Show("Version ko đúng format!\nFormat: [Major].[Minor].[Build].[Revision]\nVí dụ: 1.1.1.1");
                return -1;
            }

            int index = 0;
            for (int i = 0; i < VersionSource.Count; i++)
            {
                var ver = VersionSource[i];
                Version v = Version.Parse(ver.Version);
                if (newVersion == v)
                {
                    MessageBox.Show("Đã có version này!");
                    return -1;
                }

                if (i == 0 && newVersion > v)
                {
                    index = 0;
                    break;
                }

                if (newVersion < v)
                {
                    index = i + 1;
                }
            }

            if (PART_PluginVersionTb.Text == ""
                || PART_DatePublisedDP.SelectedDate == null
                || PART_VersionDesTb.Text == ""
                || PART_PathToPluginTextbox.Text == "")
            {
                MessageBox.Show("Điền các trường còn thiếu!");
                return -1;
            }
            return index;
        }

    }
}
