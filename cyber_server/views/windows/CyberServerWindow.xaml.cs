using cyber_server.definition;
using cyber_server.implements.db_manager;
using cyber_server.implements.log_manager;
using cyber_server.implements.plugin_manager;
using cyber_server.implements.task_handler;
using cyber_server.view_models.plugin_version_item;
using cyber_server.view_models.tool_item;
using cyber_server.view_models.windows;
using cyber_server.views.usercontrols.others;
using cyber_server.views.windows.others;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// <summary>
    /// Interaction logic for CyberServerWindow.xaml
    /// </summary>
    public partial class CyberServerWindow : Window
    {
        public ObservableCollection<PluginVersionItemViewModel> VersionSource { get; set; }
            = new ObservableCollection<PluginVersionItemViewModel>();

        public ObservableCollection<PluginItemViewModel> PluginSource { get; set; }
            = new ObservableCollection<PluginItemViewModel>();

        public ObservableCollection<ToolItemViewModel> ToolSource { get; set; }
            = new ObservableCollection<ToolItemViewModel>();

        public CyberServerWindow()
        {
            InitializeComponent();
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.ADD_VERSION_TASK_TYPE_KEY, "Adding new version", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.ADD_PLUGIN_TASK_TYPE_KEY, "Adding new plugin", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.ADD_TOOL_TASK_TYPE_KEY, "Adding new tool", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.RELOAD_TOOL_TASK_TYPE_KEY, "Reloading tools", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.RELOAD_PLUGIN_TASK_TYPE_KEY, "Reloading plugins", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.DELETE_PLUGIN_TASK_TYPE_KEY, "Deleting plugin", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.DELETE_TOOL_TASK_TYPE_KEY, "Deleting tool", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.SYNC_TASK_TYPE_KEY, "Syncing", 1, 1);
            TaskHandlerManager.Current.RegisterHandler(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY, PART_TaskHandlingPanel);
        }

        #region AddPluginTab
        private async void HandleAddPluginTabButtonEvent(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            ServerLogManager.Current.D(btn.Name);
            switch (btn.Name)
            {
                case "PART_QuickFillExecutePathButton":
                    {
                        if (IsMeetConditionToCreateExecutePath())
                        {
                            var eBW = new EditBoxWindow(
                            uneditableText: "plugins\\" + PART_PluginKeyTb.Text + "\\" + PART_PluginVersionTb.Text + "\\"
                            , editableText: "[plugin dll name]"
                            , checkConditionSatisfyToCloseWindow: (editedText) =>
                            {
                                System.IO.FileInfo fi = null;
                                try
                                {
                                    fi = new System.IO.FileInfo(editedText);
                                }
                                catch (ArgumentException) { }
                                catch (System.IO.PathTooLongException) { }
                                catch (NotSupportedException) { }
                                if (ReferenceEquals(fi, null))
                                {
                                    MessageBox.Show("File name is not vaild!");
                                    return false;
                                }
                                else
                                {
                                    var splits = editedText.Split('.');

                                    if (splits.Length == 2)
                                    {
                                        if (splits[1] == "dll")
                                        {
                                            return true;
                                        }
                                        MessageBox.Show("File extension must be dll");
                                    }
                                    else
                                    {
                                        MessageBox.Show("File name is not vaild!");
                                    }
                                }
                                return false;
                            });
                            eBW.Owner = this;
                            var pathToExecute = eBW.Show();
                            if (!eBW.IsCanceled)
                            {
                                PART_ExecutePathTextbox.Text = pathToExecute;
                            }
                        }



                        break;
                    }
                case "PART_AccessBaseFolderTab":
                    {
                        Process.Start(CyberServerDefinition.PLUGIN_BASE_FOLDER_PATH);
                        break;
                    }
                case "SUB_DeleteVersionItem":
                    {
                        var picontext = btn.DataContext as PluginVersionItemViewModel;
                        if (picontext != null)
                        {
                            VersionSource.Remove(picontext);
                        }
                        break;
                    }
                case "PART_CreateNewVersionBtn":
                    {
                        await PART_TaskHandlingPanel.ExecuteTask(CurrentTaskManager.ADD_VERSION_TASK_TYPE_KEY,
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
                                        ExecutePath = PART_ExecutePathTextbox.Text,
                                        MainClassName = PART_PathMainClassNameTextbox.Text,
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
                    }
                case "PART_AddPluginToDb":
                    {
                        await PART_TaskHandlingPanel.ExecuteTask(CurrentTaskManager.ADD_PLUGIN_TASK_TYPE_KEY,
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
                                await Task.Delay(1000);
                                var plugin = new Plugin();
                                plugin.StringId = pluginKey;
                                plugin.Name = PART_PluginNameTb.Text;
                                plugin.Author = PART_PluginAuthorTb.Text;
                                plugin.Description = PART_PluginDesTb.Text;
                                plugin.ProjectURL = PART_PluginURLTb.Text;
                                plugin.IconSource = PART_PluginIconSourceTb.Text;
                                plugin.IsPreReleased = PART_PluginIsPrereleasedCb.IsChecked ?? false;
                                plugin.IsAuthenticated = PART_PluginIsAuthenticatedCb.IsChecked ?? false;
                                plugin.Downloads = 0;
                                var isCopFileSuccess = true;

                                // Build version source
                                foreach (var version in VersionSource)
                                {
                                    var pv = version.BuildPluginVersionFromViewModel(plugin.StringId);
                                    isCopFileSuccess = CyberPluginAndToolManager
                                         .Current
                                         .CopyPluginToServerLocation(version.GetVersionSourceFilePath()
                                             , pv.FolderPath);
                                    plugin.PluginVersions.Add(pv);

                                    if (!isCopFileSuccess)
                                    {
                                        CyberPluginAndToolManager.Current.DeletePluginDirectory(plugin.StringId);
                                        break;
                                    }
                                }

                                if (isCopFileSuccess)
                                {
                                    //Build icon source
                                    try
                                    {
                                        if (PART_PluginIconSourceTb.Text != "")
                                        {
                                            var isLocalFile = new Uri(PART_PluginIconSourceTb.Text).IsFile;
                                            if (isLocalFile)
                                            {
                                                CyberPluginAndToolManager
                                                    .Current
                                                    .CopyPluginIconToServerLocation(PART_PluginIconSourceTb.Text, plugin.StringId);
                                                plugin.IconSource = CyberServerDefinition.SERVER_REMOTE_ADDRESS
                                                    + "/pluginresource/"
                                                    + plugin.StringId + "/" + System.IO.Path.GetFileName(PART_PluginIconSourceTb.Text);
                                            }
                                            else
                                            {
                                                plugin.IconSource = PART_PluginIconSourceTb.Text;
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        plugin.IconSource = "";
                                    }

                                    await CyberDbManager.Current.RequestDbContextAsync((context) =>
                                    {
                                        context.Plugins.Add(plugin);
                                        context.SaveChanges();
                                    });
                                }

                                MessageBox.Show("Thêm mới plugin thành công!");
                            }
                            else
                            {
                                MessageBox.Show("Điền các trường còn thiếu!");
                            }

                        },
                        executeTime: 3000,
                        bypassIfSemaphoreNotAvaild: true);
                        break;
                    }
                case "PART_OpenPluginFileChooser":
                    {
                        var ofd = new OpenFileDialog();
                        ofd.Filter = "Zip files (*.zip)|*.zip";
                        if (ofd.ShowDialog() == true)
                            PART_PathToPluginTextbox.Text = ofd.FileName;
                        break;
                    }
                case "PART_OpenIconFileChooser":
                    {
                        var ofd = new OpenFileDialog();
                        ofd.Filter = "Image files (*.png)|*.png";
                        if (ofd.ShowDialog() == true)
                        {
                            try
                            {
                                Bitmap img = new Bitmap(ofd.FileName);

                                var imageHeight = img.Height;
                                var imageWidth = img.Width;
                                if (imageHeight > 200 || imageWidth > 200)
                                {
                                    MessageBox.Show("Please select icon with size < 100 pixels");
                                }
                                else
                                {
                                    PART_PluginIconSourceTb.Text = ofd.FileName;
                                }

                            }
                            catch
                            {
                                MessageBox.Show("Not support this format!~");
                            }
                        }
                        break;
                    }
            }
        }

        private bool IsMeetConditionToAddPlugToDb()
        {

            if (PART_PluginNameTb.Text == ""
                || PART_PluginAuthorTb.Text == ""
                || PART_PluginURLTb.Text == ""
                || PART_PluginKeyTb.Text == ""
                || VersionSource.Count == 0) return false;
            if (!string.IsNullOrEmpty(PART_PluginKeyTb.Text))
            {
                var regexItem = new Regex(@"^[a-zA-Z0-9_]*$");
                if (!regexItem.IsMatch(PART_PluginKeyTb.Text))
                {
                    MessageBox.Show("Plugin key không được chứ ký tự đặc biệt");
                    return false;
                }
            }
            return true;
        }

        private bool IsMeetConditionToCreateExecutePath()
        {

            if (PART_PluginKeyTb.Text == "")
            {
                MessageBox.Show("Điền plugin key trước!");
                return false;
            }
            if (PART_PluginVersionTb.Text == "")
            {
                MessageBox.Show("Điền plugin version trước!");
                return false;
            }

            if (!string.IsNullOrEmpty(PART_PluginKeyTb.Text))
            {
                var regexItem = new Regex(@"^[a-zA-Z0-9_]*$");
                if (!regexItem.IsMatch(PART_PluginKeyTb.Text))
                {
                    MessageBox.Show("Plugin key không được chứ ký tự đặc biệt");
                    return false;
                }
            }
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
                || PART_PathMainClassNameTextbox.Text == ""
                || PART_PathToPluginTextbox.Text == ""
                || PART_ExecutePathTextbox.Text == "")
            {
                MessageBox.Show("Điền các trường còn thiếu!");
                return -1;
            }
            return index;
        }
        #endregion

        #region ManageTool&PluginTab
        private async void HandleManagerPluginTabButtonEvent(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            switch (btn.Name)
            {
                case "PART_ReloadPluginFromDb":
                    {
                        await PART_TaskHandlingPanel.ExecuteTask(CurrentTaskManager.RELOAD_PLUGIN_TASK_TYPE_KEY,
                           mainFunc: async () =>
                           {
                               PluginSource.Clear();
                               await CyberDbManager.Current.RequestDbContextAsync((context) =>
                               {
                                   foreach (var plugin in context.Plugins)
                                   {
                                       var vm = new PluginItemViewModel(plugin);
                                       PluginSource.Add(vm);
                                   }
                               });
                           },
                           executeTime: 1000,
                           bypassIfSemaphoreNotAvaild: true);
                        break;
                    }
                case "PART_ReloadToolFromDb":
                    {
                        await PART_TaskHandlingPanel.ExecuteTask(CurrentTaskManager.RELOAD_TOOL_TASK_TYPE_KEY,
                           mainFunc: async () =>
                           {
                               ToolSource.Clear();
                               await CyberDbManager.Current.RequestDbContextAsync((context) =>
                               {
                                   foreach (var tool in context.Tools)
                                   {
                                       var vm = new ToolItemViewModel(tool);
                                       ToolSource.Add(vm);
                                   }
                               });
                           },
                           executeTime: 1000,
                           bypassIfSemaphoreNotAvaild: true);
                        break;
                    }
                case "DeleteToolItemButton":
                    {
                        var itemViewModel = btn.DataContext as ToolItemViewModel;
                        if (itemViewModel != null)
                        {
                            var confirm = MessageBox.Show("Bạn có chắc xóa dữ liệu này!", "Xác nhận", MessageBoxButton.YesNo);
                            if (confirm == MessageBoxResult.Yes)
                            {
                                await PART_TaskHandlingPanel.ExecuteTask(CurrentTaskManager.DELETE_TOOL_TASK_TYPE_KEY,
                                   mainFunc: async () =>
                                   {
                                       var sucess = await CyberDbManager.Current.RequestDbContextAsync((context) =>
                                       {
                                           context.ToolVersions.RemoveRange(itemViewModel.RawModel.ToolVersions);
                                           context.Tools.Remove(itemViewModel.RawModel);
                                           context.SaveChanges();
                                       });

                                       if (sucess)
                                       {
                                           CyberPluginAndToolManager.Current.DeleteToolDirectory(itemViewModel.RawModel.StringId, true);
                                           ToolSource.Remove(itemViewModel);
                                       }
                                   },
                                   executeTime: 0,
                                   bypassIfSemaphoreNotAvaild: true);
                            }
                        }
                        break;
                    }
                case "DeletePluginItemButton":
                    {
                        var itemViewModel = btn.DataContext as PluginItemViewModel;
                        if (itemViewModel != null)
                        {
                            var confirm = MessageBox.Show("Bạn có chắc xóa dữ liệu này!", "Xác nhận", MessageBoxButton.YesNo);
                            if (confirm == MessageBoxResult.Yes)
                            {
                                await PART_TaskHandlingPanel.ExecuteTask(CurrentTaskManager.DELETE_PLUGIN_TASK_TYPE_KEY,
                                   mainFunc: async () =>
                                   {
                                       var sucess = await CyberDbManager.Current.RequestDbContextAsync((context) =>
                                       {
                                           context.PluginVersions.RemoveRange(itemViewModel.RawModel.PluginVersions);
                                           context.Tags.RemoveRange(itemViewModel.RawModel.Tags);
                                           context.Votes.RemoveRange(itemViewModel.RawModel.Votes);
                                           context.Plugins.Remove(itemViewModel.RawModel);
                                           context.SaveChanges();
                                       });

                                       if (sucess)
                                       {
                                           CyberPluginAndToolManager.Current.DeletePluginDirectory(itemViewModel.RawModel.StringId, true);
                                           PluginSource.Remove(itemViewModel);
                                       }
                                   },
                                   executeTime: 0,
                                   bypassIfSemaphoreNotAvaild: true);

                            }
                        }
                        break;
                    }
                case "ModifyToolItemButton":
                    {
                        var itemViewModel = btn.DataContext as ToolItemViewModel;
                        var modifyToolWindow = new ModifyToolWindow();
                        modifyToolWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        modifyToolWindow.Owner = this;
                        var modifyToolWindowContext = modifyToolWindow.DataContext as ModifyToolWindowViewModel;
                        if (modifyToolWindowContext != null)
                        {
                            modifyToolWindowContext.SetRawModel(itemViewModel.RawModel);
                            modifyToolWindow.ShowDialog();
                        }
                        break;
                    }
                case "ModifyPluginItemButton":
                    {
                        var itemViewModel = btn.DataContext as PluginItemViewModel;
                        var modifyPluginWindow = new ModifyPluginWindow();
                        modifyPluginWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        modifyPluginWindow.Owner = this;
                        var modifyPluginWindowContext = modifyPluginWindow.DataContext as ModifyPluginWindowViewModel;
                        if (modifyPluginWindowContext != null)
                        {
                            modifyPluginWindowContext.SetRawModel(itemViewModel.RawModel);
                            modifyPluginWindow.ShowDialog();
                        }
                        break;
                    }
                case "PART_SyncPluginFolderWithDbButton":
                    {
                        string message = "Đã xóa:\n";
                        var isShouldNotify = false;
                        await PART_TaskHandlingPanel.ExecuteTask(CurrentTaskManager.SYNC_TASK_TYPE_KEY,
                            mainFunc: async () =>
                            {
                                var pluginKeys = CyberPluginAndToolManager.Current.GetAllPluginKeyInPluginStorageFolder();

                                var sucess = await CyberDbManager.Current.RequestDbContextAsync((context) =>
                                {
                                    var deleteKeys = "";
                                    var totalDelete = 0;
                                    var deleteVersions = "";
                                    var totalDeleteVersion = 0;

                                    foreach (var key in pluginKeys)
                                    {
                                        var plugin = context.Plugins.Where(p => p.StringId == key)
                                            .FirstOrDefault();
                                        if (plugin == null)
                                        {
                                            CyberPluginAndToolManager.Current.DeletePluginDirectory(key, true);
                                            deleteKeys = deleteKeys + key + "\n";
                                            totalDelete++;
                                            isShouldNotify = true;
                                        }
                                        else
                                        {
                                            var pluginVersions = CyberPluginAndToolManager.Current.GetAllPluginVersionInStorageFolder(key);
                                            foreach (var version in pluginVersions)
                                            {
                                                if (plugin.PluginVersions.Where(v => Version.Parse(v.Version) == Version.Parse(version))
                                                    .FirstOrDefault() == null)
                                                {
                                                    CyberPluginAndToolManager.Current.DeletePluginVersionDirectory(key, version, true);
                                                    deleteVersions = deleteVersions + key + " version=" + version + "\n";
                                                    totalDeleteVersion++;
                                                    isShouldNotify = true;
                                                }
                                            }
                                        }
                                    }

                                    message = "Đã xóa plugin:" + totalDelete + "\n"
                                        + deleteKeys + "\n"
                                        + "Đã xóa plugin version:" + totalDeleteVersion + "\n"
                                        + deleteVersions;
                                });

                            },
                            executeTime: 2000,
                            bypassIfSemaphoreNotAvaild: true);

                        if (isShouldNotify)
                        {
                            MessageBox.Show(message);
                        }
                        break;
                    }
            }
        }
        #endregion
    }
}
