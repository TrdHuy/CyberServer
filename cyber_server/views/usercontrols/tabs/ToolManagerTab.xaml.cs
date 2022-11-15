using cyber_server.definition;
using cyber_server.implements.db_manager;
using cyber_server.implements.plugin_manager;
using cyber_server.implements.task_handler;
using cyber_server.view_models.plugin_version_item;
using cyber_server.view_models.tool_item;
using cyber_server.view_models.windows;
using cyber_server.views.usercontrols.others;
using cyber_server.views.windows.others;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using System.Text;
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
using static cyber_server.view_models.tabs.ToolManagerTabViewModel;

namespace cyber_server.views.usercontrols.tabs
{
    /// <summary>
    /// Interaction logic for ToolManagerTab.xaml
    /// </summary>
    public enum EditorMode
    {
        ADD_NEW_EDITOR_MODE = 0,
        MODIFY_EDITOR_MODE = 1,
    }

    public partial class ToolManagerTab : UserControl
    {
        #region ToolEditorMode
        public static readonly DependencyProperty ToolEditorModeProperty =
            DependencyProperty.Register("ToolEditorMode",
                typeof(EditorMode),
                typeof(ToolManagerTab),
                new PropertyMetadata(EditorMode.ADD_NEW_EDITOR_MODE, new PropertyChangedCallback(OnToolEditorModeChangedCallback)));

        private static void OnToolEditorModeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ToolManagerTab)?.HandleToolItemEditorModeChanged((EditorMode)e.NewValue);
        }

        public EditorMode ToolEditorMode
        {
            get { return (EditorMode)GetValue(ToolEditorModeProperty); }
            set { SetValue(ToolEditorModeProperty, value); }
        }
        #endregion

        #region ToolVersionEditorMode
        public static readonly DependencyProperty ToolVersionEditorModeProperty =
            DependencyProperty.Register("ToolVersionEditorMode",
                typeof(EditorMode),
                typeof(ToolManagerTab),
                new PropertyMetadata(EditorMode.ADD_NEW_EDITOR_MODE, new PropertyChangedCallback(OnToolVersionEditorModeChangedCallback)));

        private static void OnToolVersionEditorModeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ToolManagerTab)?.HandleToolVersionItemEditorModeChanged((EditorMode)e.NewValue);
        }

        public EditorMode ToolVersionEditorMode
        {
            get { return (EditorMode)GetValue(ToolVersionEditorModeProperty); }
            set { SetValue(ToolVersionEditorModeProperty, value); }
        }
        #endregion

        public ToolItemViewModel ModifingContext { get; set; }
        public ToolItemViewModel AddingContext { get; set; } = new ToolItemViewModel(null);

        private long _compressToolSizeCache = 0;
        private long _rawToolSizeCache = 0;
        private string _oldToolKeyCache = "";
        private string _oldVersionCache = "";

        private bool _isToolEditorOpen;
        private bool IsToolEditorOpen
        {
            get
            {
                return _isToolEditorOpen;
            }
            set
            {
                _isToolEditorOpen = value;
                PART_ToolEditorPanel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                PART_OpenToolEditorButton.Content = value ? "v" : ">";
            }
        }


        public ToolManagerTab()
        {
            InitializeComponent();
            SetBinding(ToolManagerTab.ToolEditorModeProperty, new Binding()
            {
                Source = PART_ToolManagerTabViewModel,
                Mode = BindingMode.TwoWay,
                Path = new PropertyPath(nameof(PART_ToolManagerTabViewModel.CurrentEditorMode)),
                NotifyOnSourceUpdated = true,
            });
            HandleToolItemEditorModeChanged(ToolEditorMode);
            HandleToolVersionItemEditorModeChanged(ToolVersionEditorMode);
        }

        private void HandleToolVersionItemEditorModeChanged(EditorMode newValue)
        {
            switch (newValue)
            {
                case EditorMode.ADD_NEW_EDITOR_MODE:
                    PART_CreateNewVersionBtn.Visibility = Visibility.Visible;
                    PART_ExitEditVersionBtn.Visibility = Visibility.Collapsed;
                    PART_SaveEditVersionBtn.Visibility = Visibility.Collapsed;
                    _oldVersionCache = "";
                    break;
                case EditorMode.MODIFY_EDITOR_MODE:
                    if (ToolEditorMode == EditorMode.MODIFY_EDITOR_MODE)
                    {
                        PART_ExitEditVersionBtn.Visibility = Visibility.Visible;
                        PART_SaveEditVersionBtn.Visibility = Visibility.Visible;
                        _oldVersionCache = ModifingContext.SelectedToolVersionItemForEditting.Version;
                    }
                    else
                    {
                        PART_ExitEditVersionBtn.Visibility = Visibility.Visible;
                        PART_SaveEditVersionBtn.Visibility = Visibility.Collapsed;
                        _oldVersionCache = AddingContext.SelectedToolVersionItemForEditting.Version;
                    }
                    PART_CreateNewVersionBtn.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void HandleToolItemEditorModeChanged(EditorMode newValue)
        {
            switch (newValue)
            {
                case EditorMode.ADD_NEW_EDITOR_MODE:
                    PART_ToolEditorPanel.DataContext = AddingContext;
                    PART_SaveToolToDb.Visibility = Visibility.Collapsed;
                    PART_AddToolToDb.Visibility = Visibility.Visible;
                    PART_ExitModifingTool.Visibility = Visibility.Collapsed;
                    PART_ToolKeyTb.IsEnabled = true;
                    _oldToolKeyCache = "";
                    break;
                case EditorMode.MODIFY_EDITOR_MODE:
                    IsToolEditorOpen = true;
                    PART_ToolKeyTb.IsEnabled = false;
                    PART_ToolEditorPanel.DataContext = ModifingContext;
                    PART_SaveToolToDb.Visibility = Visibility.Visible;
                    PART_AddToolToDb.Visibility = Visibility.Collapsed;
                    PART_ExitModifingTool.Visibility = Visibility.Visible;
                    _oldToolKeyCache = ModifingContext.StringId;
                    break;
            }
        }

        private void HandleLoadedEvent(object sender, RoutedEventArgs e)
        {
            var fw = sender as FrameworkElement;
            if (fw == null) return;
            switch (fw.Name)
            {
                case "PART_ToolManagerTabUC":
                    {
                        IsToolEditorOpen = false;
                        break;
                    }
            }
        }

        private async void HandleButtonClickEvent(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            switch (btn?.Name)
            {
                case "PART_ClearAddToolTab":
                    {
                        RefreshTab();
                        break;
                    }
                case "PART_OpenToolEditorButton":
                    {
                        IsToolEditorOpen = !IsToolEditorOpen;
                        break;
                    }
                case "PART_ReloadToolFromDb":
                    {
                        var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                        if (handler == null) return;
                        await handler.ExecuteTask(CurrentTaskManager.RELOAD_TOOL_TASK_TYPE_KEY,
                           mainFunc: async () =>
                           {
                               await PART_ToolManagerTabViewModel.ReloadToolSource();
                           },
                           executeTime: 1000,
                           bypassIfSemaphoreNotAvaild: true);
                        break;
                    }
                case "PART_OpenToolFileChooser":
                    {
                        var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                        if (handler == null) return;

                        await handler.ExecuteTask(CurrentTaskManager.IMPORT_TOOL_TASK_TYPE_KEY,
                            mainFunc: async () =>
                            {
                                var ofd = new OpenFileDialog();
                                ofd.Filter = "Zip files (*.zip)|*.zip";
                                if (ofd.ShowDialog() == true)
                                {
                                    await Task.Delay(10);
                                    _compressToolSizeCache = 0;
                                    _rawToolSizeCache = 0;
                                    using (ZipArchive archive = ZipFile.OpenRead(ofd.FileName))
                                    {
                                        foreach (ZipArchiveEntry entry in archive.Entries)
                                        {
                                            _compressToolSizeCache += entry.CompressedLength;
                                            _rawToolSizeCache += entry.Length;
                                        }
                                    }
                                    PART_PathToToolTextbox.Text = ofd.FileName;
                                    PART_CompressLengthRun.Text = Math.Round(_compressToolSizeCache / Math.Pow(2, 20), 2) + "MB";
                                    PART_RawLengthRun.Text = Math.Round(_rawToolSizeCache / Math.Pow(2, 20), 2) + "MB";
                                }
                            },
                            executeTime: 0,
                            bypassIfSemaphoreNotAvaild: true);
                        break;
                    }
                case "PART_CreateNewVersionBtn":
                    {
                        var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                        if (handler == null) return;
                        await handler.ExecuteTask(CurrentTaskManager.ADD_VERSION_TASK_TYPE_KEY,
                            mainFunc: async () =>
                            {
                                if (ToolEditorMode == EditorMode.ADD_NEW_EDITOR_MODE)
                                {
                                    var isSuccess = await PART_ToolManagerTabViewModel.CreateNewToolVersion(
                                        PART_ToolVersionTb.Text
                                        , PART_PathToToolTextbox.Text
                                        , PART_DatePublisedDP.Text
                                        , PART_VersionDesTb.Text
                                        , PART_ExecutePathTextbox.Text
                                        , _compressToolSizeCache
                                        , _rawToolSizeCache
                                        , PART_ListVersionCbx.ItemsSource as ObservableCollection<ToolVersionItemViewModel>);
                                    if (isSuccess) PART_ListVersionCbx.SelectedIndex = 0;
                                }
                                else if (ToolEditorMode == EditorMode.MODIFY_EDITOR_MODE)
                                {
                                    var isSuccess = await PART_ToolManagerTabViewModel.AddNewToolVersion(
                                       PART_ToolVersionTb.Text
                                       , PART_PathToToolTextbox.Text
                                       , PART_DatePublisedDP.Text
                                       , PART_VersionDesTb.Text
                                       , PART_ExecutePathTextbox.Text
                                       , _compressToolSizeCache
                                       , _rawToolSizeCache
                                       , ModifingContext);
                                    if (isSuccess) PART_ListVersionCbx.SelectedIndex = 0;
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
                        await handler.ExecuteTask(CurrentTaskManager.ADD_TOOL_TASK_TYPE_KEY,
                        mainFunc: async () =>
                        {
                            if (ToolEditorMode == EditorMode.ADD_NEW_EDITOR_MODE)
                            {
                                var success = await PART_ToolManagerTabViewModel.AddNewToolToDb(
                                   PART_ToolKeyTb.Text
                                   , PART_ToolNameTb.Text
                                   , PART_ToolAuthorTb.Text
                                   , PART_ToolDesTb.Text
                                   , PART_ToolURLTb.Text
                                   , PART_ToolIconSourceTb.Text
                                   , PART_ToolIsPrereleasedCb.IsChecked ?? false
                                   , PART_ToolIsAuthenticatedCb.IsChecked ?? false
                                   , PART_ListVersionCbx.ItemsSource as ObservableCollection<ToolVersionItemViewModel>);
                                if (success) RefreshTab();
                            }

                        },
                        executeTime: 1000,
                        bypassIfSemaphoreNotAvaild: true);
                        break;
                    }
                case "PART_RefreshVersionFieldBtn":
                    {
                        RefreshVersionField();
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
                                    PART_ToolIconSourceTb.Text = ofd.FileName;
                                }
                            }
                            catch
                            {
                                MessageBox.Show("Not support this format!~");
                            }
                        }
                        break;
                    }
                case "PART_ExitModifingTool":
                    {
                        var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                        if (handler == null) return;
                        await handler.ExecuteTask(CurrentTaskManager.RELOAD_TOOL_TASK_TYPE_KEY,
                           mainFunc: async () =>
                           {
                               await CyberDbManager.Current.RequestDbContextAsync((dbcontext) =>
                               {
                                   CyberDbManager.Current.RollBack(force: false);
                               });
                               await PART_ToolManagerTabViewModel.ReloadToolSource();
                           },
                           executeTime: 0,
                           bypassIfSemaphoreNotAvaild: true);
                        ToolEditorMode = EditorMode.ADD_NEW_EDITOR_MODE;
                        break;
                    }
                case "PART_SaveToolToDb":
                    {
                        var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                        if (handler == null) return;
                        await handler.ExecuteTask(CurrentTaskManager.MODIFI_TOOL_TASK_TYPE_KEY,
                           mainFunc: async () =>
                           {
                               if (ToolEditorMode == EditorMode.MODIFY_EDITOR_MODE && ModifingContext != null)
                               {
                                   var success = await PART_ToolManagerTabViewModel.SaveModifyingToolToDb(
                                   PART_ToolKeyTb.Text
                                   , PART_ToolNameTb.Text
                                   , PART_ToolAuthorTb.Text
                                   , PART_ToolIconSourceTb.Text
                                   , PART_ListVersionCbx.ItemsSource as ObservableCollection<ToolVersionItemViewModel>
                                   , ModifingContext);
                                   if (success)
                                   {
                                       ModifingContext = null;
                                       ToolEditorMode = EditorMode.ADD_NEW_EDITOR_MODE;
                                   }
                               }

                           },
                           executeTime: 0,
                           bypassIfSemaphoreNotAvaild: true);
                        break;
                    }
                case "PART_SyncToolFolderWithDbButton":
                    {
                        var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                        if (handler == null) return;
                        await handler.ExecuteTask(CurrentTaskManager.SYNC_TASK_TYPE_KEY,
                            mainFunc: async () =>
                            {
                                await PART_ToolManagerTabViewModel.SyncToolFolderWithDb();
                            },
                            executeTime: 2000,
                            bypassIfSemaphoreNotAvaild: true);
                        break;
                    }
                case "PART_AccessBaseFolderTab":
                    {
                        Process.Start(CyberServerDefinition.TOOL_BASE_FOLDER_PATH);
                        break;
                    }
                case "PART_SaveEditVersionBtn":
                    {
                        if (ToolEditorMode == EditorMode.MODIFY_EDITOR_MODE)
                        {
                            var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                            if (handler == null) return;
                            await handler.ExecuteTask(CurrentTaskManager.MODIFI_VERSION_TASK_TYPE_KEY,
                              mainFunc: async () =>
                              {
                                  var success = await PART_ToolManagerTabViewModel.SaveEdittedToolVersionToDb(
                                    _oldToolKeyCache
                                    , _oldVersionCache.Trim()
                                    , PART_ToolVersionTb.Text.Trim()
                                    , PART_PathToToolTextbox.Text
                                    , PART_DatePublisedDP.Text
                                    , PART_VersionDesTb.Text
                                    , PART_ExecutePathTextbox.Text
                                    , _compressToolSizeCache
                                    , _rawToolSizeCache
                                    , ModifingContext
                                    , ModifingContext.SelectedToolVersionItemForEditting);
                                  if (success)
                                  {
                                      ModifingContext.SelectedToolVersionItemForEditting = null;
                                      ToolVersionEditorMode = EditorMode.ADD_NEW_EDITOR_MODE;
                                  }
                              },
                              executeTime: 0,
                              bypassIfSemaphoreNotAvaild: true);
                        }

                        break;
                    }
                case "PART_ExitEditVersionBtn":
                    {
                        if (ToolEditorMode == EditorMode.MODIFY_EDITOR_MODE)
                        {
                            var currentEditToolVM = PART_ToolEditorPanel.DataContext as ToolItemViewModel;
                            if (currentEditToolVM != null)
                            {
                                if (CyberDbManager.Current.RollBack(force: false))
                                {
                                    currentEditToolVM.SelectedToolVersionItemForEditting = null;
                                    currentEditToolVM.SelectedToolVersionItemForEditting?.RefreshViewModel();
                                    ToolVersionEditorMode = EditorMode.ADD_NEW_EDITOR_MODE;
                                }
                            }
                        }
                        else if (ToolEditorMode == EditorMode.ADD_NEW_EDITOR_MODE)
                        {
                            var selectedVersionVM = AddingContext.SelectedToolVersionItemForEditting;

                            if (selectedVersionVM != null && PART_ToolManagerTabViewModel.RenewToolVersionIndexForAddingMode(PART_ToolVersionTb.Text
                                        , PART_PathToToolTextbox.Text
                                        , PART_DatePublisedDP.Text
                                        , PART_VersionDesTb.Text
                                        , PART_ExecutePathTextbox.Text
                                        , _compressToolSizeCache
                                        , _rawToolSizeCache
                                        , selectedVersionVM
                                        , PART_ListVersionCbx.ItemsSource as ObservableCollection<ToolVersionItemViewModel>))
                            {
                                ToolVersionEditorMode = EditorMode.ADD_NEW_EDITOR_MODE;
                                AddingContext.SelectedToolVersionItemForEditting = null;
                            }
                        }

                        break;
                    }
                case "ModifyToolItemButton":
                    {
                        ModifingContext = btn.DataContext as ToolItemViewModel;
                        ToolEditorMode = EditorMode.MODIFY_EDITOR_MODE;
                        break;
                    }
                case "DeleteToolItemButton":
                    {
                        var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                        if (handler == null) return;
                        var itemViewModel = btn.DataContext as ToolItemViewModel;
                        if (itemViewModel != null)
                        {
                            var confirm = MessageBox.Show("Bạn có chắc xóa dữ liệu này!", "Xác nhận", MessageBoxButton.YesNo);
                            if (confirm == MessageBoxResult.Yes)
                            {
                                await handler.ExecuteTask(CurrentTaskManager.DELETE_TOOL_TASK_TYPE_KEY,
                                   mainFunc: async () =>
                                   {
                                       var success = await PART_ToolManagerTabViewModel.DeleteToolFromDb(itemViewModel);
                                       MessageBox.Show(success ? "Xóa dữ liệu thành công" : "Lỗi thao tác xóa dữ liệu");
                                   },
                                   executeTime: 0,
                                   bypassIfSemaphoreNotAvaild: true);
                            }
                        }
                        break;
                    }
                case "SUB_DeleteVersionItem":
                    {
                        var tcontext = btn.DataContext as ToolVersionItemViewModel;
                        var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                        if (handler == null || tcontext == null) return;
                        await handler.ExecuteTask(CurrentTaskManager.DELETE_VERSION_TASK_TYPE_KEY,
                            mainFunc: async () =>
                            {
                                if (ToolEditorMode == EditorMode.ADD_NEW_EDITOR_MODE)
                                {
                                    var isSuccess = await PART_ToolManagerTabViewModel.DeleteVerionInAddingMode(
                                        tcontext
                                        , PART_ListVersionCbx.ItemsSource as ObservableCollection<ToolVersionItemViewModel>);
                                }
                                else if (ToolEditorMode == EditorMode.MODIFY_EDITOR_MODE)
                                {
                                    if (ToolVersionEditorMode == EditorMode.MODIFY_EDITOR_MODE)
                                    {
                                        MessageBox.Show("Thooát chế độ chỉnh sửa version trước!");
                                        return;
                                    }
                                    var isSuccess = await PART_ToolManagerTabViewModel.DeleteVerionInModifyingMode(
                                       tcontext
                                       , ModifingContext);
                                }
                            },
                            executeTime: 0,
                            bypassIfSemaphoreNotAvaild: true);
                        break;
                    }
                case "SUB_ModifyVersionItem":
                    {
                        var selectedModifyVersionVM = btn.DataContext as ToolVersionItemViewModel;
                        var currentEditToolVM = PART_ToolEditorPanel.DataContext as ToolItemViewModel;
                        if (selectedModifyVersionVM != null && currentEditToolVM != null)
                        {
                            currentEditToolVM.SelectedToolVersionItemForEditting = selectedModifyVersionVM;
                            ToolVersionEditorMode = EditorMode.MODIFY_EDITOR_MODE;
                        }
                        break;
                    }
            }

            var checkBox = sender as CheckBox;
            switch (checkBox?.Name)
            {
                case "MarkRequireLatestVersionCheckBox":
                    {
                        var context = checkBox.DataContext as ToolItemViewModel;
                        var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                        if (handler == null || context == null) return;
                        var isEditable = true;
                        if (ToolEditorMode == EditorMode.MODIFY_EDITOR_MODE)
                        {
                            MessageBox.Show("Thoát chế độ chỉnh sửa để thực hiện thao tác này!");
                            isEditable = false;
                        }
                        await handler.ExecuteTask(CurrentTaskManager.MODIFI_TOOL_TASK_TYPE_KEY,
                           mainFunc: async () =>
                           {
                               await CyberDbManager.Current.RequestDbContextAsync((dbcontext) =>
                               {
                                   if (isEditable)
                                   {
                                       dbcontext.SaveChanges();
                                   }
                                   else
                                   {
                                       CyberDbManager.Current.RollBack();
                                       context.RefreshViewModel();
                                   }
                               });

                               if (isEditable) await Task.Delay(1000);
                           },
                           executeTime: 0,
                           bypassIfSemaphoreNotAvaild: true);
                        break;
                    }
            }
        }

        private void RefreshTab()
        {
            _compressToolSizeCache = 0;
            _rawToolSizeCache = 0;
            (PART_ListVersionCbx.ItemsSource as IList)?.Clear();
            PART_CompressLengthRun.Text = "";
            PART_RawLengthRun.Text = "";
            PART_ToolKeyTb.Text = "";
            PART_ToolNameTb.Text = "";
            PART_ToolAuthorTb.Text = "";
            PART_ToolDesTb.Text = "";
            PART_ToolVersionTb.Text = "";
            PART_PathToToolTextbox.Text = "";
            PART_ExecutePathTextbox.Text = "";
            PART_DatePublisedDP.SelectedDate = null;
            PART_VersionDesTb.Text = "";
            PART_ToolURLTb.Text = "";
            PART_ToolIconSourceTb.Text = "";
            PART_ToolIsAuthenticatedCb.IsChecked = false;
            PART_ToolIsPrereleasedCb.IsChecked = false;
        }

        private void RefreshVersionField()
        {
            _compressToolSizeCache = 0;
            _rawToolSizeCache = 0;
            PART_ToolVersionTb.Text = "";
            PART_PathToToolTextbox.Text = "";
            PART_ExecutePathTextbox.Text = "";
            PART_DatePublisedDP.SelectedDate = null;
            PART_VersionDesTb.Text = "";
        }
    }
}
