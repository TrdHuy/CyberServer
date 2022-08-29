using cyber_server.implements.db_manager;
using cyber_server.implements.plugin_manager;
using cyber_server.view_models.plugin_version_item;
using cyber_server.view_models.windows;
using cyber_server.views.usercontrols.others;
using cyber_server.views.windows.others;
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
    /// <summary>
    /// Interaction logic for CyberServerWindow.xaml
    /// </summary>
    public partial class CyberServerWindow : Window
    {
        public ObservableCollection<PluginVersionItemViewModel> VersionSource { get; set; }
            = new ObservableCollection<PluginVersionItemViewModel>();

        public ObservableCollection<PluginItemViewModel> PluginSource { get; set; }
            = new ObservableCollection<PluginItemViewModel>();

        public CyberServerWindow()
        {
            InitializeComponent();
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.ADD_VERSION_TASK_TYPE_KEY, "Adding new version", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.ADD_PLUGIN_TASK_TYPE_KEY, "Adding new plugin", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.RELOAD_PLUGIN_TASK_TYPE_KEY, "Reloading", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.DELETE_PLUGIN_TASK_TYPE_KEY, "Deleting plugin", 1, 1);
        }

        #region AddPluginTab
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
                                    plugin.PluginVersions.Add(pv);

                                    if (!isCopFileSuccess)
                                    {
                                        CyberPluginManager.Current.DeletePluginDirectory(plugin.StringId);
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

                                MessageBox.Show("Thêm mới plugin thành công!");
                            }
                            else
                            {
                                MessageBox.Show("Điền các trường còn thiếu!");
                            }

                        },
                        executeTime: 1000,
                        bypassIfSemaphoreNotAvaild: true);
                        break;
                    }
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
        #endregion

        #region ManagePluginTab
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
                                           CyberPluginManager.Current.DeletePluginDirectory(itemViewModel.RawModel.StringId);
                                           PluginSource.Remove(itemViewModel);
                                       }
                                   },
                                   executeTime: 0,
                                   bypassIfSemaphoreNotAvaild: true);

                            }
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
            }
        }
        #endregion
    }
}
