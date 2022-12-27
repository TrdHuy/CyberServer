using cyber_server.implements.db_manager;
using cyber_server.implements.plugin_manager;
using cyber_server.models;
using cyber_server.view_models.list_view_item;
using cyber_server.views.usercontrols.tabs;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace cyber_server.view_models.tabs
{
    public abstract class BaseSwManagerTabViewModel : BaseViewModel
    {
        private BaseObjectSwItemViewModel _selectedModifyToolItem;
        private string _previewSizeContent;
        private string _pathToSw;
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
        public ObservableCollection<BaseObjectSwItemViewModel> SwSource { get; set; }
            = new ObservableCollection<BaseObjectSwItemViewModel>();

        [Bindable(true)]
        public BaseObjectSwItemViewModel SelectedModifyToolItem
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
                return _pathToSw;
            }
            set
            {
                _pathToSw = value;
                InvalidateOwn();
            }
        }

        public async Task<bool> DeleteVerionInModifyingMode(BaseObjectVersionItemViewModel context, BaseObjectSwItemViewModel modifingContext)
        {
            var confirm = MessageBox.Show("Bạn có muốn xóa phiên bản này?", "", MessageBoxButton.YesNo);
            if (confirm == MessageBoxResult.Yes)
            {
                if (context != null && context.RawModel != null)
                {
                    return await DeleteSwVersionInDatabase(context, modifingContext);
                }
            }
            return true;
        }

        public async Task<bool> DeleteVerionInAddingMode(BaseObjectVersionItemViewModel context
            , ObservableCollection<BaseObjectVersionItemViewModel> versionItemViewModels)
        {
            await Task.Delay(10);
            return versionItemViewModels.Remove(context);
        }

        public async Task<bool> CreateNewSwVersion(BaseObjectVersionItemViewModel swVersionVM
            , ObservableCollection<BaseObjectVersionItemViewModel> versionSource)
        {
            {
                var newIndex = GetIndexOfNewVersion(swVersionVM, versionSource);

                if (newIndex != -1)
                {
                    await Task.Delay(1000);
                    versionSource.Insert(newIndex, swVersionVM);
                    return true;
                }
                return false;
            }
        }

        public bool RenewSwVersionIndexForAddingMode(BaseObjectVersionItemViewModel swVersionVM
            , BaseObjectVersionItemViewModel selectedVersionVM
            , ObservableCollection<BaseObjectVersionItemViewModel> versionSource)
        {
            var oldIndex = versionSource.IndexOf(selectedVersionVM);
            versionSource.RemoveAt(oldIndex);
            var newIndex = GetIndexOfNewVersion(swVersionVM, versionSource);
            var success = newIndex != -1;
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

        public async Task<bool> AddNewSwVersionForEdittingMode(BaseObjectVersionItemViewModel swVersionVM
        , BaseObjectSwItemViewModel modifiedItemViewModel)
        {
            var newIndex = GetIndexOfNewVersion(swVersionVM, modifiedItemViewModel.VersionSource);
            if (newIndex != -1)
            {
                await Task.Delay(1000);

                var toolVer = swVersionVM.RawModel;

                toolVer.File = File.ReadAllBytes(swVersionVM.FilePath);

                var success = await AddSwVersionToDatabase(modifiedItemViewModel, toolVer);
                if (success)
                {
                    modifiedItemViewModel.VersionSource.Insert(newIndex, swVersionVM);
                }
                return success;
            }
            return false;
        }

        public async Task<bool> SaveEdittedToolVersionToDb(string oldVersion
            , BaseObjectSwItemViewModel modifiedItemViewModel
            , BaseObjectVersionItemViewModel modifiedVersionItemViewModel)
        {
            // Remove temporaly
            var oldIndex = modifiedItemViewModel.VersionSource.IndexOf(modifiedVersionItemViewModel);
            modifiedItemViewModel.VersionSource.RemoveAt(oldIndex);

            var newIndex = GetIndexOfNewVersion(modifiedVersionItemViewModel, modifiedItemViewModel.VersionSource);
            if (newIndex != -1)
            {
                var oVer = Version.Parse(oldVersion);
                var nVer = Version.Parse(modifiedVersionItemViewModel.Version);
                await Task.Delay(100);

                var localVersionFilePath = modifiedVersionItemViewModel.FilePath;
                var success = true;
                var swVersionModel = modifiedVersionItemViewModel.RawModel;

                if (!string.IsNullOrEmpty(localVersionFilePath))
                    swVersionModel.File = File.ReadAllBytes(localVersionFilePath);

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

        public async Task<bool> AddNewSwToDb(BaseObjectSwItemViewModel newItemViewModel)
        {
            var success = false;
            if (await IsMeetConditionToAddSwToDb(newItemViewModel))
            {
                await Task.Delay(1000);
                var swModel = await BuildNewSwModelFromViewModel(newItemViewModel);

                var vm = await AddNewSwToDatabase(swModel);
                if (vm != null)
                {
                    SwSource.Add(vm);
                    MessageBox.Show("Thêm mới sw thành công!");
                }
                else
                {
                    MessageBox.Show("Thêm mới sw thất bại!");
                }

            }
            else
            {
                MessageBox.Show("Điền các trường còn thiếu!");
            }
            return success;
        }

        public async Task<bool> SaveModifyingSwToDb(BaseObjectSwItemViewModel modifiedItemVM)
        {
            var confirm = MessageBox.Show("Bạn có chắc thay đổi trên?", "", MessageBoxButton.YesNo);
            if (confirm == MessageBoxResult.Yes)
            {
                if (IsMeetConditionToSaveEditedSwToDb(modifiedItemVM))
                {
                    var success = true;
                    await modifiedItemVM.RebuildSwModel();
                    success = await CyberDbManager.Current.RequestDbContextAsync((context) =>
                    {
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

        public abstract Task ReloadSwSource();

        public abstract Task<bool> DeleteSwFromDb(BaseObjectSwItemViewModel toolItemVM);

        [Obsolete("Method is deprecated, since using byte array instead saving to folder physically")]
        public abstract Task<bool> SyncSwFolderWithDb();

        protected abstract Task<BaseObjectSwItemViewModel> AddNewSwToDatabase(BaseObjectSwModel swModel);

        protected abstract Task<bool> AddSwVersionToDatabase(BaseObjectSwItemViewModel modifiedItemViewModel, BaseObjectVersionModel toolVer);

        protected abstract Task<BaseObjectSwModel> BuildNewSwModelFromViewModel(BaseObjectSwItemViewModel swItemViewModel);

        protected abstract Task<bool> IsSwKeyExistInDatabase(string swKey);

        protected abstract Task<bool> DeleteSwVersionInDatabase(BaseObjectVersionItemViewModel context, BaseObjectSwItemViewModel modifingContext);

        protected bool IsMeetConditionToSaveEditedSwToDb(BaseObjectSwItemViewModel modifiedItemViewModel)
        {

            if (string.IsNullOrEmpty(modifiedItemViewModel.Name)
                || string.IsNullOrEmpty(modifiedItemViewModel.Author)
                || modifiedItemViewModel.VersionSource.Count == 0) return false;

            return true;
        }

        protected async Task<bool> IsMeetConditionToAddSwToDb(BaseObjectSwItemViewModel newItemViewModel)
        {

            if (string.IsNullOrEmpty(newItemViewModel.Name)
                || string.IsNullOrEmpty(newItemViewModel.StringId)
                || string.IsNullOrEmpty(newItemViewModel.Author)
                || newItemViewModel.VersionSource.Count == 0) return false;
            if (!string.IsNullOrEmpty(newItemViewModel.StringId))
            {
                var regexItem = new Regex(@"^[a-zA-Z0-9_]*$");
                if (!regexItem.IsMatch(newItemViewModel.StringId))
                {
                    MessageBox.Show("Tool key không được chứ ký tự đặc biệt");
                    return false;
                }
            }

            var isSwKeyExist = await IsSwKeyExistInDatabase(newItemViewModel.StringId);

            if (isSwKeyExist)
            {
                MessageBox.Show("Key này đã tồn tại\nHãy chọn key khác!");
                return false;
            }
            return true;
        }

        protected virtual int GetIndexOfNewVersion(BaseObjectVersionItemViewModel swVersionVM
            , ObservableCollection<BaseObjectVersionItemViewModel> versionSource)
        {
            Version newVersion = new Version();
            try
            {
                newVersion = Version.Parse(swVersionVM.Version);
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

            if (string.IsNullOrEmpty(swVersionVM.Version)
                || string.IsNullOrEmpty(swVersionVM.DatePublished)
                || string.IsNullOrEmpty(swVersionVM.Description)
                || string.IsNullOrEmpty(swVersionVM.AssemblyName)
                || string.IsNullOrEmpty(swVersionVM.ExecutePath))
            {
                MessageBox.Show("Điền các trường còn thiếu!");
                return -1;
            }

            var isExistExePath = false;
            if (File.Exists(swVersionVM.FilePath))
            {
                using (ZipArchive archive = ZipFile.OpenRead(swVersionVM.FilePath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName == swVersionVM.ExecutePath)
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
            else if (!CyberPluginAndToolManager.Current.CheckToolPathExistOnServer(swVersionVM))
            {
                MessageBox.Show("File tool không tồn tại!");
                return -1;
            }
            return index;
        }

    }
}
