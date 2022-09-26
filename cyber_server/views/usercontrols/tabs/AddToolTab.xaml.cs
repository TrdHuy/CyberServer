using cyber_server.definition;
using cyber_server.implements.db_manager;
using cyber_server.implements.log_manager;
using cyber_server.implements.plugin_manager;
using cyber_server.implements.task_handler;
using cyber_server.view_models.tool_item;
using cyber_server.views.usercontrols.others;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace cyber_server.views.usercontrols.tabs
{
    /// <summary>
    /// Interaction logic for AddToolTab.xaml
    /// </summary>
    public partial class AddToolTab : UserControl
    {
        public ObservableCollection<ToolVersionItemViewModel> VersionSource { get; set; }
            = new ObservableCollection<ToolVersionItemViewModel>();

        public AddToolTab()
        {
            InitializeComponent();
        }

        private async void HandleAddToolTabButtonEvent(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            ServerLogManager.Current.D("On handling: " + btn.Name);
            switch (btn.Name)
            {
                case "PART_AccessBaseFolderTab":
                    {
                        Process.Start(CyberServerDefinition.TOOL_BASE_FOLDER_PATH);
                        break;
                    }
                case "SUB_DeleteVersionItem":
                    {
                        var picontext = btn.DataContext as ToolVersionItemViewModel;
                        if (picontext != null)
                        {
                            VersionSource.Remove(picontext);
                        }
                        break;
                    }
                case "PART_CreateNewVersionBtn":
                    {
                        var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                        if (handler == null) return;
                        await handler.ExecuteTask(CurrentTaskManager.ADD_VERSION_TASK_TYPE_KEY,
                            mainFunc: async () =>
                            {
                                var newIndex = GetIndexOfNewVersion();
                                if (newIndex != -1)
                                {
                                    await Task.Delay(1000);
                                    var pluginVer = new ToolVersionItemViewModel()
                                    {
                                        Version = PART_ToolVersionTb.Text,
                                        FilePath = PART_PathToToolTextbox.Text,
                                        DatePublished = PART_DatePublisedDP.Text,
                                        Description = PART_VersionDesTb.Text,
                                        ExecutePath = PART_ExecutePathTextbox.Text,
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
                case "PART_AddToolToDb":
                    {
                        var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                        if (handler == null) return;
                        await handler.ExecuteTask(CurrentTaskManager.ADD_PLUGIN_TASK_TYPE_KEY,
                        mainFunc: async () =>
                        {
                            if (IsMeetConditionToAddToolToDb())
                            {
                                var isToolKeyExist = false;
                                var toolKey = PART_ToolKeyTb.Text;
                                await CyberDbManager.Current.RequestDbContextAsync((context) =>
                                {
                                    isToolKeyExist = context
                                     .Tools
                                     .Any<Tool>(t =>
                                         t.StringId.Equals(toolKey, StringComparison.CurrentCultureIgnoreCase));
                                });
                                if (isToolKeyExist)
                                {
                                    MessageBox.Show("Key này đã tồn tại\nHãy chọn key khác!");
                                    return;
                                }
                                await Task.Delay(1000);
                                var tool = new Tool();
                                tool.StringId = toolKey;
                                tool.Name = PART_ToolNameTb.Text;
                                tool.Author = PART_ToolAuthorTb.Text;
                                tool.Description = PART_ToolDesTb.Text;
                                tool.ProjectURL = PART_ToolURLTb.Text;
                                tool.IconSource = PART_PluginIconSourceTb.Text;
                                tool.IsPreReleased = PART_ToolIsPrereleasedCb.IsChecked ?? false;
                                tool.IsAuthenticated = PART_ToolIsAuthenticatedCb.IsChecked ?? false;
                                tool.Downloads = 0;
                                var isCopFileSuccess = true;

                                // Build version source
                                foreach (var version in VersionSource)
                                {
                                    var tv = version.BuildToolVersionFromViewModel(tool.StringId);
                                    isCopFileSuccess = CyberPluginAndToolManager
                                         .Current
                                         .CopyToolToServerLocation(version.GetVersionSourceFilePath()
                                             , tv.FolderPath);
                                    tool.ToolVersions.Add(tv);

                                    if (!isCopFileSuccess)
                                    {
                                        CyberPluginAndToolManager.Current.DeletePluginDirectory(tool.StringId);
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
                                                    .CopyToolIconToServerLocation(PART_PluginIconSourceTb.Text, tool.StringId);
                                                tool.IconSource = CyberServerDefinition.SERVER_REMOTE_ADDRESS
                                                    + "/toolresource/"
                                                    + tool.StringId + "/" + System.IO.Path.GetFileName(PART_PluginIconSourceTb.Text);
                                            }
                                            else
                                            {
                                                tool.IconSource = PART_PluginIconSourceTb.Text;
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        tool.IconSource = "";
                                    }

                                    await CyberDbManager.Current.RequestDbContextAsync((context) =>
                                    {
                                        context.Tools.Add(tool);
                                        context.SaveChanges();
                                    });
                                }

                                MessageBox.Show("Thêm mới tool thành công!");
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

        private bool IsMeetConditionToAddToolToDb()
        {

            if (PART_ToolNameTb.Text == ""
                || PART_ToolAuthorTb.Text == ""
                || PART_ToolURLTb.Text == ""
                || PART_ToolKeyTb.Text == ""
                || VersionSource.Count == 0) return false;
            if (!string.IsNullOrEmpty(PART_ToolKeyTb.Text))
            {
                var regexItem = new Regex(@"^[a-zA-Z0-9_]*$");
                if (!regexItem.IsMatch(PART_ToolKeyTb.Text))
                {
                    MessageBox.Show("Tool key không được chứ ký tự đặc biệt");
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

            if (PART_ToolVersionTb.Text == ""
                || PART_DatePublisedDP.SelectedDate == null
                || PART_VersionDesTb.Text == ""
                || PART_ExecutePathTextbox.Text == "")
            {
                MessageBox.Show("Điền các trường còn thiếu!");
                return -1;
            }
            return index;
        }
    }
}
