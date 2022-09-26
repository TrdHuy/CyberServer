using cyber_server.definition;
using cyber_server.implements.db_manager;
using cyber_server.implements.plugin_manager;
using cyber_server.implements.task_handler;
using cyber_server.view_models.tool_item;
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
    /// Interaction logic for ModifyToolWindow.xaml
    /// </summary>
    public partial class ModifyToolWindow : Window
    {
        private ModifyToolWindowViewModel _viewModel;

        public ModifyToolWindow()
        {
            InitializeComponent();
            TaskHandlerManager.Current.RegisterHandler(TaskHandlerManager.TOOL_MODIFY_WINDOW_HANDLER_KEY, PART_TaskHandlingPanel);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.MODIFI_TOOL_TASK_TYPE_KEY, "Saving data", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.ADD_VERSION_TASK_TYPE_KEY, "Adding new version", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.DELETE_VERSION_TASK_TYPE_KEY, "Deleting old version", 1, 1);
            _viewModel = DataContext as ModifyToolWindowViewModel;
        }

        protected override void OnClosed(EventArgs e)
        {
            TaskHandlerManager.Current.UnregisterHandler(TaskHandlerManager.TOOL_MODIFY_WINDOW_HANDLER_KEY);
            base.OnClosed(e);

        }

        private async void HandleButtonEvent(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                switch (btn.Name)
                {
                    case "SUB_DeleteVersionItem":
                        {
                            await PART_TaskHandlingPanel.ExecuteTask(CurrentTaskManager.DELETE_VERSION_TASK_TYPE_KEY,
                                mainFunc: async () =>
                                {
                                    var confirm = MessageBox.Show("Bạn có muốn xóa version này?", "", MessageBoxButton.YesNo);
                                    if (confirm == MessageBoxResult.Yes)
                                    {
                                        var picontext = btn.DataContext as ToolVersionItemViewModel;
                                        if (picontext != null && picontext.RawModel != null)
                                        {
                                            var toolId = _viewModel.RawModel.ToolId;
                                            await CyberDbManager.Current.RequestDbContextAsync((context) =>
                                            {
                                                var tool = context
                                                    .Tools
                                                    .Where<Tool>(t => t.ToolId == toolId)
                                                    .FirstOrDefault();
                                                context.ToolVersions.Remove(picontext.RawModel);
                                                tool.ToolVersions.Remove(picontext.RawModel);
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
                                            var toolId = _viewModel.RawModel.ToolId;
                                            var toolVer = new ToolVersionItemViewModel()
                                            {
                                                Version = PART_ToolVersionTb.Text,
                                                FilePath = PART_PathToToolTextbox.Text,
                                                DatePublished = PART_DatePublisedDP.Text,
                                                Description = PART_VersionDesTb.Text,
                                                ExecutePath = PART_ExecutePathTextbox.Text,
                                            };

                                            _viewModel.VersionSource.Insert(newIndex, toolVer);
                                            if (_viewModel.VersionSource.Count > 0)
                                            {
                                                PART_ListVersionCbx.SelectedIndex = 0;
                                            }

                                            await CyberDbManager.Current.RequestDbContextAsync((context) =>
                                            {
                                                var tool = context
                                                    .Tools
                                                    .Where<Tool>(t => t.ToolId == toolId)
                                                    .FirstOrDefault();
                                                var tv = toolVer.BuildToolVersionFromViewModel(tool.StringId);

                                                var isCopFileSuccess = CyberPluginAndToolManager
                                                    .Current
                                                    .CopyPluginToServerLocation(toolVer.GetVersionSourceFilePath()
                                                        , tv.FolderPath);
                                                if (isCopFileSuccess)
                                                {
                                                    tool.ToolVersions.Add(tv);
                                                    context.SaveChanges();
                                                    MessageBox.Show("Thêm version mới thành công!");
                                                }
                                                else
                                                {
                                                    CyberPluginAndToolManager.Current.DeletePluginDirectory(tool.StringId);
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
                    case "PART_SaveToolToDb":
                        {
                            var confirm = MessageBox.Show("Bạn có chắc thay đổi trên?", "", MessageBoxButton.YesNo);
                            if (confirm == MessageBoxResult.Yes)
                            {
                                await PART_TaskHandlingPanel.ExecuteTask(CurrentTaskManager.MODIFI_TOOL_TASK_TYPE_KEY,
                                    mainFunc: async () =>
                                    {
                                        if (IsMeetConditionToAddToolToDb())
                                        {
                                            var toolId = _viewModel.RawModel.ToolId;

                                            if (_viewModel.ToolKey != _viewModel.RawModel.StringId)
                                            {
                                                bool isPluginKeyExist = false;
                                                await CyberDbManager.Current.RequestDbContextAsync((context) =>
                                                {
                                                    isPluginKeyExist = context
                                                     .Plugins
                                                     .Any<Plugin>(p =>
                                                         p.StringId.Equals(_viewModel.ToolKey, StringComparison.CurrentCultureIgnoreCase));
                                                });
                                                if (isPluginKeyExist)
                                                {
                                                    MessageBox.Show("Key này đã tồn tại\nHãy chọn key khác!");
                                                    return;
                                                }
                                            }


                                            await CyberDbManager.Current.RequestDbContextAsync((context) =>
                                            {
                                                var tool = context
                                                    .Tools
                                                    .Where<Tool>(t => t.ToolId == toolId)
                                                    .FirstOrDefault();
                                                tool.Name = _viewModel.ToolName;
                                                tool.Author = _viewModel.Author;
                                                tool.Description = _viewModel.Description;
                                                tool.ProjectURL = _viewModel.ProjectURL;
                                                tool.IsAuthenticated = _viewModel.IsAuthenticated;
                                                tool.IsPreReleased = _viewModel.IsPreReleased;
                                                var success = true;

                                                //Rename plugin folder
                                                if (_viewModel.ToolKey != _viewModel.RawModel.StringId)
                                                {
                                                    success = CyberPluginAndToolManager.Current.RenamePluginFolder(tool.StringId
                                                        , _viewModel.ToolKey);
                                                }

                                                if (success)
                                                {
                                                    tool.StringId = _viewModel.ToolKey;

                                                    //Build plugin version source
                                                    foreach (var version in _viewModel.VersionSource)
                                                    {
                                                        if (version.IsThisVersionAddedNewly())
                                                        {
                                                            var tv = version.BuildToolVersionFromViewModel(tool.StringId);
                                                            success = CyberPluginAndToolManager
                                                                .Current
                                                                .CopyPluginToServerLocation(version.GetVersionSourceFilePath()
                                                                    , tv.FolderPath);
                                                            tool.ToolVersions.Add(tv);
                                                            if (success)
                                                            {
                                                                CyberPluginAndToolManager.Current.DeletePluginDirectory(tool.StringId);
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
                                                                    .CopyPluginIconToServerLocation(_viewModel.IconSource, tool.StringId);
                                                                tool.IconSource = CyberServerDefinition.SERVER_REMOTE_ADDRESS
                                                                    + "/toolresource/"
                                                                    + tool.StringId + "/" + System.IO.Path.GetFileName(_viewModel.IconSource);
                                                            }
                                                            else
                                                            {
                                                                tool.IconSource = _viewModel.IconSource;
                                                            }
                                                        }
                                                    }
                                                    catch
                                                    {
                                                        tool.IconSource = "";
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
                    case "PART_OpenToolFileChooser":
                        {
                            var ofd = new OpenFileDialog();
                            ofd.Filter = "Zip files (*.zip)|*.zip";
                            if (ofd.ShowDialog() == true)
                                PART_PathToToolTextbox.Text = ofd.FileName;
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

        private bool IsMeetConditionToAddToolToDb()
        {

            if (PART_ToolNameTb.Text == ""
                || PART_ToolAuthorTb.Text == ""
                || PART_ToolURLTb.Text == ""
                || PART_ToolKeyTb.Text == ""
                || _viewModel.VersionSource.Count == 0) return false;
            if (!string.IsNullOrEmpty(PART_ToolKeyTb.Text))
            {
                var regexItem = new Regex(@"^[a-zA-Z0-9_]*$");
                if (!regexItem.IsMatch(PART_ToolKeyTb.Text))
                {
                    MessageBox.Show("tool key không được chứ ký tự đặc biệt");
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
                newVersion = Version.Parse(PART_ToolVersionTb.Text);
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

            if (PART_ToolVersionTb.Text == ""
                || PART_DatePublisedDP.SelectedDate == null
                || PART_VersionDesTb.Text == ""
                || PART_PathToToolTextbox.Text == ""
                || PART_ExecutePathTextbox.Text == "")
            {
                MessageBox.Show("Điền các trường còn thiếu!");
                return -1;
            }
            return index;
        }
    }
}
