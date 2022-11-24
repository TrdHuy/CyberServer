using cyber_server.definition;
using cyber_server.implements.db_manager;
using cyber_server.implements.plugin_manager;
using cyber_server.view_models.tool_item;
using cyber_server.views.usercontrols.tabs;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace cyber_server.view_models.tabs
{
    internal class ToolManagerTabViewModel : BaseViewModel
    {
        private ToolItemViewModel _selectedModifyToolItem;
        private string _previewSizeContent;
        private string _pathToTool;
        private EditorMode _currentEditorMode;


        [Bindable(true)]
        public EditorMode CurrentEditorMode
        {
            get
            {
                return _currentEditorMode;
            }
            set
            {
                _currentEditorMode = value;
                InvalidateOwn();

            }
        }

        [Bindable(true)]
        public ObservableCollection<ToolItemViewModel> ToolsSource { get; set; }
            = new ObservableCollection<ToolItemViewModel>();

        [Bindable(true)]
        public ToolItemViewModel SelectedModifyToolItem
        {
            get
            {
                return _selectedModifyToolItem;
            }
            set
            {
                _selectedModifyToolItem = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string PreviewSizeContent
        {
            get
            {
                return _previewSizeContent;
            }
            set
            {
                _previewSizeContent = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string PathToToolString
        {
            get
            {
                return _pathToTool;
            }
            set
            {
                _pathToTool = value;
                InvalidateOwn();
            }
        }

        public ToolManagerTabViewModel()
        {
        }

        public async Task ReloadToolSource()
        {
            ToolsSource.Clear();
            await CyberDbManager.Current.RequestDbContextAsync((context) =>
            {
                foreach (var tool in context.Tools)
                {
                    var vm = new ToolItemViewModel(tool);
                    ToolsSource.Add(vm);
                }
            });
        }

        public async Task<bool> DeleteToolFromDb(ToolItemViewModel toolItemVM)
        {
            var sucess = await CyberDbManager.Current.RequestDbContextAsync((context) =>
            {
                context.ToolVersions.RemoveRange(toolItemVM.RawModel.ToolVersions);
                context.Tools.Remove(toolItemVM.RawModel);
                context.SaveChanges();
            });

            if (sucess)
            {
                ToolsSource.Remove(toolItemVM);
            }
            return sucess;
        }

        [Obsolete("Method is deprecated, since using byte array instead saving to folder physically")]
        public async Task<bool> SyncToolFolderWithDb()
        {
            string message = "Đã xóa:\n";
            var isShouldNotify = false;

            var toolKeys = CyberPluginAndToolManager.Current.GetAllToolKeyInToolStorageFolder();

            var sucess = await CyberDbManager.Current.RequestDbContextAsync((context) =>
            {
                var deleteKeys = "";
                var totalDelete = 0;
                var deleteVersions = "";
                var totalDeleteVersion = 0;

                foreach (var key in toolKeys)
                {
                    var tool = context.Tools.Where(t => t.StringId == key)
                        .FirstOrDefault();
                    if (tool == null)
                    {
                        CyberPluginAndToolManager.Current.DeleteToolDirectory(key, true);
                        deleteKeys = deleteKeys + key + "\n";
                        totalDelete++;
                        isShouldNotify = true;
                    }
                    else
                    {
                        var toolVersion = CyberPluginAndToolManager.Current.GetAllToolVersionInStorageFolder(key);
                        foreach (var version in toolVersion)
                        {
                            if (tool.ToolVersions.Where(v => Version.Parse(v.Version) == Version.Parse(version))
                                .FirstOrDefault() == null)
                            {
                                CyberPluginAndToolManager.Current.DeleteToolVersionDirectory(key, version, true);
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
            if (isShouldNotify)
            {
                MessageBox.Show(message);
            }

            return true;
        }

        public async Task<bool> CreateNewToolVersion(string version, string toolPath, string datePublished
            , string versionDes, string executePath, long compressToolSize, long rawToolSize
            , ObservableCollection<ToolVersionItemViewModel> versionSource)
        {
            var newIndex = GetIndexOfNewVersion(version, datePublished, toolPath
                , versionDes, executePath, versionSource);
            if (newIndex != -1)
            {
                await Task.Delay(1000);
                var pluginVer = new ToolVersionItemViewModel()
                {
                    Version = version,
                    FilePath = toolPath,
                    DatePublished = datePublished,
                    Description = versionDes,
                    ExecutePath = executePath,
                    CompressLength = compressToolSize + "",
                    RawLength = rawToolSize + "",
                };

                versionSource.Insert(newIndex, pluginVer);
                return true;
            }
            return false;
        }

        public bool RenewToolVersionIndexForAddingMode(string version, string toolPath, string datePublished
            , string versionDes, string executePath, long compressToolSize, long rawToolSize
            , ToolVersionItemViewModel selectedVersionVM
            , ObservableCollection<ToolVersionItemViewModel> versionSource)
        {
            var oldIndex = versionSource.IndexOf(selectedVersionVM);
            versionSource.RemoveAt(oldIndex);
            var newIndex = GetIndexOfNewVersion(version, datePublished, toolPath
                , versionDes, executePath, versionSource);
            var success = newIndex != -1 && compressToolSize != 0 && rawToolSize != 0;
            if (success)
            {
                versionSource.Insert(newIndex, selectedVersionVM);
            }
            else
            {
                versionSource.Insert(oldIndex, selectedVersionVM);
            }
            return success;
        }

        public async Task<bool> AddNewToolVersionForEdittingMode(string version, string toolPath, string datePublished
        , string versionDes, string executePath, long compressToolSize, long rawToolSize
        , ToolItemViewModel modifiedItemViewModel)
        {
            var newIndex = GetIndexOfNewVersion(version, datePublished, toolPath
                , versionDes, executePath, modifiedItemViewModel.VersionSource);
            if (newIndex != -1)
            {
                await Task.Delay(1000);
                var toolVerVM = new ToolVersionItemViewModel()
                {
                    Version = version,
                    FilePath = toolPath,
                    DatePublished = datePublished,
                    Description = versionDes,
                    ExecutePath = executePath,
                    CompressLength = compressToolSize + "",
                    RawLength = rawToolSize + "",
                };

                var toolVer = toolVerVM.BuildToolVersionFromViewModel(modifiedItemViewModel.StringId);

                toolVer.File = File.ReadAllBytes(toolPath);

                modifiedItemViewModel.VersionSource.Insert(newIndex, toolVerVM);
                await CyberDbManager.Current.RequestDbContextAsync((dbContext) =>
                {
                    modifiedItemViewModel.RawModel.ToolVersions.Add(toolVer);
                    dbContext.SaveChanges();
                });

                return true;
            }
            return false;
        }

        public async Task<bool> SaveEdittedToolVersionToDb(string oldToolKey, string oldVersion, string newVersion, string toolPath, string datePublished
            , string versionDes, string executePath, long compressToolSize, long rawToolSize
            , ToolItemViewModel modifiedItemViewModel
            , ToolVersionItemViewModel modifiedVersionItemViewModel)
        {
            // Remove temporaly
            var oldIndex = modifiedItemViewModel.VersionSource.IndexOf(modifiedVersionItemViewModel);
            modifiedItemViewModel.VersionSource.RemoveAt(oldIndex);

            var newIndex = GetIndexOfNewVersion(newVersion, datePublished, toolPath
                , versionDes, executePath, modifiedItemViewModel.VersionSource);
            if (newIndex != -1)
            {
                var oVer = Version.Parse(oldVersion);
                var nVer = Version.Parse(newVersion);
                await Task.Delay(100);

                var localVersionFilePath = modifiedVersionItemViewModel.GetVersionSourceLocalFilePath();
                var success = true;
                var toolVer = modifiedVersionItemViewModel.BuildToolVersionFromViewModel(modifiedItemViewModel.StringId);

                toolVer.File = File.ReadAllBytes(toolPath);

                modifiedItemViewModel.VersionSource.Insert(newIndex, modifiedVersionItemViewModel);
                await CyberDbManager.Current.RequestDbContextAsync((dbContext) =>
                {
                    if (success)
                    {
                        dbContext.SaveChanges();
                    }
                    else
                    {
                        CyberDbManager.Current.RollBack();
                    }
                });
                return true;
            }
            else
            {
                modifiedItemViewModel.VersionSource.Insert(oldIndex, modifiedVersionItemViewModel);
            }
            return false;
        }

        public async Task<bool> AddNewToolToDb(string toolKey, string toolName, string toolAuthor, string toolDes
            , string toolURL, string toolIconSource, bool isPreReleased, bool isAuthenticated
            , ObservableCollection<ToolVersionItemViewModel> versionSource)
        {
            var success = false;
            if (await IsMeetConditionToAddToolToDb(toolKey, toolName, toolAuthor, versionSource))
            {
                await Task.Delay(1000);
                var tool = new Tool();
                tool.StringId = toolKey;
                tool.Name = toolName;
                tool.Author = toolAuthor;
                tool.Description = toolDes;
                tool.ProjectURL = toolURL;
                tool.IconSource = toolIconSource;
                tool.IsPreReleased = isPreReleased;
                tool.IsAuthenticated = isAuthenticated;
                tool.Downloads = 0;

                // Build version source
                foreach (var version in versionSource)
                {
                    var localFilePath = version.GetVersionSourceLocalFilePath();
                    var tv = version.BuildToolVersionFromViewModel(tool.StringId);
                    if (File.Exists(localFilePath))
                    {
                        tv.File = File.ReadAllBytes(localFilePath);
                        tool.ToolVersions.Add(tv);
                    }
                }


                //Build icon source
                try
                {
                    if (toolIconSource != "")
                    {
                        var isLocalFile = new Uri(toolIconSource).IsFile;
                        if (isLocalFile)
                        {
                            CyberPluginAndToolManager
                                .Current
                                .CopyToolIconToServerLocation(toolIconSource, tool.StringId);
                            tool.IconSource = CyberServerDefinition.SERVER_REMOTE_ADDRESS
                                + "/toolresource/"
                                + tool.StringId + "/" + System.IO.Path.GetFileName(toolIconSource);
                        }
                        else
                        {
                            tool.IconSource = toolIconSource;
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
                    var vm = new ToolItemViewModel(tool);
                    ToolsSource.Add(vm);
                });
                success = true;
                MessageBox.Show("Thêm mới tool thành công!");


            }
            else
            {
                MessageBox.Show("Điền các trường còn thiếu!");
            }
            return success;
        }

        public async Task<bool> SaveModifyingToolToDb(string toolKey, string toolName, string toolAuthor
            , string toolIconSource
            , ObservableCollection<ToolVersionItemViewModel> versionSource
            , ToolItemViewModel modifiedItemVM)
        {
            var confirm = MessageBox.Show("Bạn có chắc thay đổi trên?", "", MessageBoxButton.YesNo);
            if (confirm == MessageBoxResult.Yes)
            {
                if (IsMeetConditionToSaveEditedToolToDb(toolName, toolAuthor, versionSource))
                {
                    var success = true;
                    await CyberDbManager.Current.RequestDbContextAsync((context) =>
                    {
                        //Build tool version source
                        foreach (var version in versionSource)
                        {
                            if (version.IsThisVersionAddedNewly())
                            {
                                var localFilePath = version.GetVersionSourceLocalFilePath();
                                var tv = version.BuildToolVersionFromViewModel(toolKey);
                                if (File.Exists(localFilePath))
                                {
                                    tv.File = File.ReadAllBytes(localFilePath);
                                    modifiedItemVM.RawModel.ToolVersions.Add(tv);
                                }
                            }
                        }

                        //Build icon source
                        try
                        {
                            if (toolIconSource != "")
                            {
                                var isLocalFile = new Uri(toolIconSource).IsFile;
                                if (isLocalFile)
                                {
                                    CyberPluginAndToolManager
                                        .Current
                                        .CopyToolIconToServerLocation(toolIconSource, toolKey);
                                    modifiedItemVM.RawModel.IconSource = CyberServerDefinition.SERVER_REMOTE_ADDRESS
                                        + "/toolresource/"
                                        + toolKey + "/" + System.IO.Path.GetFileName(toolIconSource);
                                }
                            }
                        }
                        catch
                        {
                            modifiedItemVM.RawModel.IconSource = "";
                        }

                        if (success)
                        {
                            context.SaveChanges();
                        }
                        else
                        {
                            CyberDbManager.Current.RollBack();
                        }
                    });
                    return success;
                }
                else
                {
                    MessageBox.Show("Điền các trường còn thiếu!");
                }
            }

            return false;
        }

        private bool IsMeetConditionToSaveEditedToolToDb(string toolName, string toolAuthor
            , ObservableCollection<ToolVersionItemViewModel> versionSource)
        {

            if (string.IsNullOrEmpty(toolName)
                || string.IsNullOrEmpty(toolAuthor)
                || versionSource.Count == 0) return false;

            return true;
        }

        private async Task<bool> IsMeetConditionToAddToolToDb(string toolKey, string toolName, string toolAuthor
            , ObservableCollection<ToolVersionItemViewModel> versionSource)
        {

            if (string.IsNullOrEmpty(toolName)
                || string.IsNullOrEmpty(toolKey)
                || string.IsNullOrEmpty(toolAuthor)
                || versionSource.Count == 0) return false;
            if (!string.IsNullOrEmpty(toolKey))
            {
                var regexItem = new Regex(@"^[a-zA-Z0-9_]*$");
                if (!regexItem.IsMatch(toolKey))
                {
                    MessageBox.Show("Tool key không được chứ ký tự đặc biệt");
                    return false;
                }
            }

            var isToolKeyExist = false;
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
                return false;
            }
            return true;
        }

        private int GetIndexOfNewVersion(string version, string datePublished, string toolPath
            , string versionDes, string executePath, ObservableCollection<ToolVersionItemViewModel> versionSource)
        {
            Version newVersion = new Version();
            try
            {
                newVersion = Version.Parse(version);
            }
            catch
            {
                MessageBox.Show("Version ko đúng format!\nFormat: [Major].[Minor].[Build].[Revision]\nVí dụ: 1.1.1.1");
                return -1;
            }

            int index = 0;
            for (int i = 0; i < versionSource.Count; i++)
            {
                var ver = versionSource[i];
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

            if (string.IsNullOrEmpty(version)
                || string.IsNullOrEmpty(datePublished)
                || string.IsNullOrEmpty(versionDes)
                || string.IsNullOrEmpty(executePath))
            {
                MessageBox.Show("Điền các trường còn thiếu!");
                return -1;
            }

            var isExistExePath = false;
            if (File.Exists(toolPath))
            {
                using (ZipArchive archive = ZipFile.OpenRead(toolPath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName == executePath)
                        {
                            isExistExePath = true;
                            break;
                        }
                    }
                }

                if (!isExistExePath)
                {
                    MessageBox.Show("Đường dẫn tới file exe không tồn tại!");
                    return -1;
                }
            }
            else if (!CyberPluginAndToolManager.Current.CheckToolPathExistOnServer(toolPath))
            {
                MessageBox.Show("File tool không tồn tại!");
                return -1;
            }
            return index;
        }

        public async Task<bool> DeleteVerionInModifyingMode(ToolVersionItemViewModel context, ToolItemViewModel modifingContext)
        {
            var confirm = MessageBox.Show("Bạn có muốn xóa phiên bản này?", "", MessageBoxButton.YesNo);
            if (confirm == MessageBoxResult.Yes)
            {
                if (context != null && context.RawModel != null)
                {
                    await CyberDbManager.Current.RequestDbContextAsync((dbcontext) =>
                    {
                        var tool = modifingContext.RawModel;
                        if (tool.ToolVersions.Count > 1)
                        {
                            dbcontext.ToolVersions.Remove(context.RawModel);
                            tool.ToolVersions.Remove(context.RawModel);
                            dbcontext.SaveChanges();
                            modifingContext.VersionSource.Remove(context);
                        }
                        else
                        {
                            MessageBox.Show("Không thể xóa phiên bản duy nhất!\nVui lòng thêm các phiên bản khác để tiến hành thao tác này!", "Thông báo");
                        }

                    });
                }
            }
            return true;
        }

        public async Task<bool> DeleteVerionInAddingMode(ToolVersionItemViewModel context, ObservableCollection<ToolVersionItemViewModel> toolVersionItemViewModels)
        {
            await Task.Delay(10);
            return toolVersionItemViewModels.Remove(context);
        }
    }
}
