using cyber_server.definition;
using cyber_server.implements.db_manager;
using cyber_server.implements.plugin_manager;
using cyber_server.implements.task_handler;
using cyber_server.view_models.plugin_version_item;
using cyber_server.view_models.windows;
using cyber_server.views.usercontrols.others;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace cyber_server.views.windows.others
{
    /// <summary>
    /// Interaction logic for ModifyPluginWindow.xaml
    /// </summary>
    public partial class ModifyPluginWindow : Window
    {
        private ModifyPluginWindowViewModel _viewModel;
        public ModifyPluginWindow()
        {
            InitializeComponent();
            TaskHandlerManager.Current.RegisterHandler(TaskHandlerManager.PLUGIN_MODIFY_WINDOW_HANDLER_KEY, PART_TaskHandlingPanel); 
            _viewModel = DataContext as ModifyPluginWindowViewModel;
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.MODIFI_PLUGIN_TASK_TYPE_KEY, "Saving data", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.ADD_VERSION_TASK_TYPE_KEY, "Adding new version", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.DELETE_VERSION_TASK_TYPE_KEY, "Deleting old version", 1, 1);
        }

        private async void HandleButtonEvent(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
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
                    case "SUB_DeleteVersionItem":
                        {
                            await PART_TaskHandlingPanel.ExecuteTask(CurrentTaskManager.DELETE_VERSION_TASK_TYPE_KEY,
                                                            mainFunc: async () =>
                                                            {
                                                                var confirm = MessageBox.Show("Bạn có muốn xóa version này?", "", MessageBoxButton.YesNo);
                                                                if (confirm == MessageBoxResult.Yes)
                                                                {
                                                                    var picontext = btn.DataContext as PluginVersionItemViewModel;
                                                                    if (picontext != null && picontext.RawModel != null)
                                                                    {
                                                                        var pluginId = _viewModel.RawModel.PluginId;
                                                                        await CyberDbManager.Current.RequestDbContextAsync((context) =>
                                                                        {
                                                                            var plugin = context
                                                                                .Plugins
                                                                                .Where<Plugin>(p => p.PluginId == pluginId)
                                                                                .FirstOrDefault();
                                                                            context.PluginVersions.Remove(picontext.RawModel);
                                                                            plugin.PluginVersions.Remove(picontext.RawModel);
                                                                            context.SaveChanges();
                                                                            _viewModel.VersionSource.Remove(picontext);
                                                                        });
                                                                    }
                                                                }
                                                            },
                                                            executeTime: 1000,
                                                            bypassIfSemaphoreNotAvaild: true);
                            break;
                        }
                    case "PART_CreateNewVersionBtn":
                        {
                            await PART_TaskHandlingPanel.ExecuteTask(CurrentTaskManager.ADD_VERSION_TASK_TYPE_KEY,
                                mainFunc: async () =>
                                {
                                    var confirm = MessageBox.Show("Bạn có muốn thêm version này?", "", MessageBoxButton.YesNo);
                                    if (confirm == MessageBoxResult.Yes)
                                    {
                                        await Task.Delay(100);
                                        var newIndex = GetIndexOfNewVersion();
                                        if (newIndex != -1)
                                        {
                                            var pluginId = _viewModel.RawModel.PluginId;
                                            var pluginVer = new PluginVersionItemViewModel()
                                            {
                                                Version = PART_PluginVersionTb.Text,
                                                FilePath = PART_PathToPluginTextbox.Text,
                                                DatePublished = PART_DatePublisedDP.Text,
                                                Description = PART_VersionDesTb.Text,
                                                ExecutePath = PART_ExecutePathTextbox.Text,
                                                MainClassName = PART_PathMainClassNameTextbox.Text,
                                            };

                                            _viewModel.VersionSource.Insert(newIndex, pluginVer);
                                            if (_viewModel.VersionSource.Count > 0)
                                            {
                                                PART_ListVersionCbx.SelectedIndex = 0;
                                            }

                                            await CyberDbManager.Current.RequestDbContextAsync((context) =>
                                            {
                                                var plugin = context
                                                    .Plugins
                                                    .Where<Plugin>(p => p.PluginId == pluginId)
                                                    .FirstOrDefault();
                                                var pv = pluginVer.BuildPluginVersionFromViewModel(plugin.StringId);

                                                var isCopFileSuccess = CyberPluginAndToolManager
                                                    .Current
                                                    .CopyPluginToServerLocation(pluginVer.GetVersionSourceFilePath()
                                                        , pv.FolderPath);
                                                if (isCopFileSuccess)
                                                {
                                                    plugin.PluginVersions.Add(pv);
                                                    context.SaveChanges();
                                                    MessageBox.Show("Thêm version mới thành công!");
                                                }
                                                else
                                                {
                                                    CyberPluginAndToolManager.Current.DeletePluginDirectory(plugin.StringId);
                                                    CyberDbManager.Current.RollBack();
                                                }

                                            });
                                        }
                                    }
                                },
                                executeTime: 1000,
                                bypassIfSemaphoreNotAvaild: true);
                            break;
                        }
                    case "PART_SavePluginToDb":
                        {
                            var confirm = MessageBox.Show("Bạn có chắc thay đổi trên?", "", MessageBoxButton.YesNo);
                            if (confirm == MessageBoxResult.Yes)
                            {
                                await PART_TaskHandlingPanel.ExecuteTask(CurrentTaskManager.MODIFI_PLUGIN_TASK_TYPE_KEY,
                                    mainFunc: async () =>
                                    {
                                        if (IsMeetConditionToAddPlugToDb())
                                        {
                                            var pluginId = _viewModel.RawModel.PluginId;

                                            if (_viewModel.PluginKey != _viewModel.RawModel.StringId)
                                            {
                                                bool isPluginKeyExist = false;
                                                await CyberDbManager.Current.RequestDbContextAsync((context) =>
                                                {
                                                    isPluginKeyExist = context
                                                     .Plugins
                                                     .Any<Plugin>(p =>
                                                         p.StringId.Equals(_viewModel.PluginKey, StringComparison.CurrentCultureIgnoreCase));
                                                });
                                                if (isPluginKeyExist)
                                                {
                                                    MessageBox.Show("Key này đã tồn tại\nHãy chọn key khác!");
                                                    return;
                                                }
                                            }


                                            await CyberDbManager.Current.RequestDbContextAsync((context) =>
                                            {
                                                var plugin = context
                                                    .Plugins
                                                    .Where<Plugin>(p => p.PluginId == pluginId)
                                                    .FirstOrDefault();
                                                plugin.Name = _viewModel.PluginName;
                                                plugin.Author = _viewModel.Author;
                                                plugin.Description = _viewModel.Description;
                                                plugin.ProjectURL = _viewModel.ProjectURL;
                                                plugin.IsAuthenticated = _viewModel.IsAuthenticated;
                                                plugin.IsPreReleased = _viewModel.IsPreReleased;
                                                var success = true;

                                                //Rename plugin folder
                                                if (_viewModel.PluginKey != _viewModel.RawModel.StringId)
                                                {
                                                    success = CyberPluginAndToolManager.Current.RenamePluginFolder(plugin.StringId
                                                        , _viewModel.PluginKey);
                                                }

                                                if (success)
                                                {
                                                    plugin.StringId = _viewModel.PluginKey;

                                                    //Build plugin version source
                                                    foreach (var version in _viewModel.VersionSource)
                                                    {
                                                        if (version.IsThisVersionAddedNewly())
                                                        {
                                                            var pv = version.BuildPluginVersionFromViewModel(plugin.StringId);
                                                            success = CyberPluginAndToolManager
                                                                .Current
                                                                .CopyPluginToServerLocation(version.GetVersionSourceFilePath()
                                                                    , pv.FolderPath);
                                                            plugin.PluginVersions.Add(pv);
                                                            if (success)
                                                            {
                                                                CyberPluginAndToolManager.Current.DeletePluginDirectory(plugin.StringId);
                                                                break;
                                                            }
                                                        }
                                                    }

                                                    //Build icon source
                                                    try
                                                    {
                                                        if (_viewModel.IconSource != "")
                                                        {
                                                            var isLocalFile = new Uri(_viewModel.IconSource).IsFile;
                                                            if (isLocalFile)
                                                            {
                                                                CyberPluginAndToolManager
                                                                    .Current
                                                                    .CopyPluginIconToServerLocation(_viewModel.IconSource, plugin.StringId);
                                                                plugin.IconSource = CyberServerDefinition.SERVER_REMOTE_ADDRESS
                                                                    + "/pluginresource/"
                                                                    + plugin.StringId + "/" + System.IO.Path.GetFileName(_viewModel.IconSource);
                                                            }
                                                            else
                                                            {
                                                                plugin.IconSource = _viewModel.IconSource;
                                                            }
                                                        }
                                                    }
                                                    catch
                                                    {
                                                        plugin.IconSource = "";
                                                    }

                                                    if (success)
                                                    {
                                                        context.SaveChanges();
                                                        this.Close();
                                                    }
                                                    else
                                                    {
                                                        CyberDbManager.Current.RollBack();
                                                    }
                                                }
                                                else
                                                {
                                                    CyberDbManager.Current.RollBack();
                                                }
                                            });
                                        }
                                        else
                                        {
                                            MessageBox.Show("Điền các trường còn thiếu!");
                                        }
                                    },
                                    executeTime: 0,
                                    bypassIfSemaphoreNotAvaild: true);
                            }
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
                                        _viewModel.IconSource = ofd.FileName;
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
        }

        private bool IsMeetConditionToAddPlugToDb()
        {

            if (PART_PluginNameTb.Text == ""
                || PART_PluginAuthorTb.Text == ""
                || PART_PluginURLTb.Text == ""
                || PART_PluginKeyTb.Text == ""
                || _viewModel.VersionSource.Count == 0) return false;
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
            for (int i = 0; i < _viewModel.VersionSource.Count; i++)
            {
                var ver = _viewModel.VersionSource[i];
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
                || PART_PathMainClassNameTextbox.Text == ""
                || PART_VersionDesTb.Text == ""
                || PART_PathToPluginTextbox.Text == ""
                || PART_ExecutePathTextbox.Text == "")
            {
                MessageBox.Show("Điền các trường còn thiếu!");
                return -1;
            }
            return index;
        }

        protected override void OnClosed(EventArgs e)
        {
            TaskHandlerManager.Current.UnregisterHandler(TaskHandlerManager.PLUGIN_MODIFY_WINDOW_HANDLER_KEY);
            base.OnClosed(e);
        }
    }
}
