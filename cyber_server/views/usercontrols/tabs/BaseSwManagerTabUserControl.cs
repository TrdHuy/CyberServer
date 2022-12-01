using cyber_server.definition;
using cyber_server.implements.db_manager;
using cyber_server.implements.task_handler;
using cyber_server.view_models.list_view_item;
using cyber_server.view_models.tabs;
using cyber_server.views.usercontrols.others;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace cyber_server.views.usercontrols.tabs
{
    public enum SwManagerViewElementTagId
    {
        ExpandSwEditorButton = 1,
        SaveSwToDbButton = 2,
        AddSwToDbButton = 3,
        ExitModifingSwButton = 4,
        CreateNewVersionButton = 5,
        ExitEditVersionButton = 6,
        ClearAddSwButton = 7,
        SaveEditVersionButton = 8,
        ReloadSwFromDbButton = 9,
        OpenSwFileChooserButton = 10,
        RefreshVersionFieldButton = 11,
        OpenIconFileChooserButton = 12,
        SyncSwFolderWithDbButton = 13,
        AccessBaseFolderTabButton = 14,
        ModifySwItemButton = 15,
        DeleteSwItemButton = 16,
        ModifyVersionItem = 17,
        DeleteVersionItem = 18,
        MarkRequireLatestVersionCheckBox = 19,
    }

    public enum EditorMode
    {
        ADD_NEW_EDITOR_MODE = 0,
        MODIFY_EDITOR_MODE = 1,
    }

    public abstract class BaseSwManagerTabUserControl : UserControl
    {
        #region SwEditorMode
        public static readonly DependencyProperty SwEditorModeProperty =
            DependencyProperty.Register("SwEditorMode",
                typeof(EditorMode),
                typeof(BaseSwManagerTabUserControl),
                new PropertyMetadata(EditorMode.ADD_NEW_EDITOR_MODE, new PropertyChangedCallback(OnSwEditorModeChangedCallback)));

        private static void OnSwEditorModeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as BaseSwManagerTabUserControl)?.HandleSwItemEditorModeChanged((EditorMode)e.NewValue);
        }

        public EditorMode SwEditorMode
        {
            get { return (EditorMode)GetValue(SwEditorModeProperty); }
            set { SetValue(SwEditorModeProperty, value); }
        }
        #endregion

        #region SwVersionEditorMode
        public static readonly DependencyProperty SwVersionEditorModeProperty =
            DependencyProperty.Register("SwVersionEditorMode",
                typeof(EditorMode),
                typeof(BaseSwManagerTabUserControl),
                new PropertyMetadata(EditorMode.ADD_NEW_EDITOR_MODE, new PropertyChangedCallback(OnSwVersionEditorModeChangedCallback)));

        private static void OnSwVersionEditorModeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as BaseSwManagerTabUserControl)?.HandleSwVersionItemEditorModeChanged((EditorMode)e.NewValue);
        }

        public EditorMode SwVersionEditorMode
        {
            get { return (EditorMode)GetValue(SwVersionEditorModeProperty); }
            set { SetValue(SwVersionEditorModeProperty, value); }
        }
        #endregion

        public abstract BaseObjectSwItemViewModel ModifingContext { get; set; }
        public abstract BaseObjectSwItemViewModel AddingContext { get; set; }

        protected long _compressToolSizeCache { get; private set; } = 0;
        protected long _rawToolSizeCache { get; private set; } = 0;
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
                SwEditorPanel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                ExpandSwEditorButton.Content = value ? "v" : ">";
            }
        }

        protected abstract BaseSwManagerTabViewModel SwManagerTabViewModel { get; }

        protected abstract Grid SwEditorPanel { get; }
        protected abstract Button AccessBaseFolderTabButton { get; }
        protected abstract Button ExpandSwEditorButton { get; }
        protected abstract Button SaveSwToDbButton { get; }
        protected abstract Button AddSwToDbButton { get; }
        protected abstract Button ExitModifingSwButton { get; }
        protected abstract Button ReloadSwFromDbButton { get; }
        protected abstract Button CreateNewVersionButton { get; }
        protected abstract Button ExitEditVersionButton { get; }
        protected abstract Button ClearAddSwButton { get; }
        protected abstract Button SaveEditVersionButton { get; }
        protected abstract Button OpenSwFileChooserButton { get; }
        protected abstract Button RefreshVersionFieldButton { get; }
        protected abstract Button OpenIconFileChooserButton { get; }
        protected abstract Button SyncSwFolderWithDbButton { get; }

        protected abstract TextBox SwKeyTextbox { get; }
        protected abstract TextBox SwNameTextbox { get; }
        protected abstract TextBox SwAuthorTextbox { get; }
        protected abstract TextBox SwVersionTextBox { get; }
        protected abstract ComboBox SwListVersionCombobox { get; }
        protected abstract Run CompressLengthRun { get; }
        protected abstract Run RawLengthRun { get; }
        protected abstract TextBox SwDescriptionTextBox { get; }
        protected abstract TextBox SwPathTextBox { get; }
        protected abstract TextBox SwExecutePathTextBox { get; }
        protected abstract DatePicker SwatePublisedDatePicker { get; }
        protected abstract TextBox SwVersionDescriptionTextBox { get; }
        protected abstract TextBox SwURLTextBox { get; }
        protected abstract TextBox SwIconSourceTextBox { get; }
        protected abstract CheckBox SwIsAuthenticatedCheckBox { get; }
        protected abstract CheckBox SwIsPrereleasedCheckBox { get; }

        public BaseSwManagerTabUserControl() : base()
        {
            Initialized += BaseHandleSwManagerTabInitialized;
            Loaded += BaseHandleLoadedEvent;
        }

        protected async Task BaseHandleButtonClickEvent(object sender, RoutedEventArgs e)
        {
            if (e.Handled) return;
            var tag = (sender as FrameworkElement)?.Tag;
            var taskKey = BuildTaskTypeKey(tag);

            var btn = sender as Button;
            switch (btn?.Tag)
            {
                case SwManagerViewElementTagId.ClearAddSwButton:
                    {
                        RefreshTab();
                        break;
                    }
                case SwManagerViewElementTagId.ExpandSwEditorButton:
                    {
                        IsToolEditorOpen = !IsToolEditorOpen;
                        break;
                    }
                case SwManagerViewElementTagId.ReloadSwFromDbButton:
                    {
                        var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                        if (handler == null) return;
                        await handler.ExecuteTask(taskKey,
                           mainFunc: async () =>
                           {
                               await SwManagerTabViewModel.ReloadSwSource();
                           },
                           executeTime: 1000,
                           bypassIfSemaphoreNotAvaild: true);
                        break;
                    }
                case SwManagerViewElementTagId.OpenSwFileChooserButton:
                    {
                        var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                        if (handler == null) return;

                        await handler.ExecuteTask(taskKey,
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
                                    SwPathTextBox.Text = ofd.FileName;
                                    CompressLengthRun.Text = Math.Round(_compressToolSizeCache / Math.Pow(2, 20), 2) + "MB";
                                    RawLengthRun.Text = Math.Round(_rawToolSizeCache / Math.Pow(2, 20), 2) + "MB";
                                }
                            },
                            executeTime: 0,
                            bypassIfSemaphoreNotAvaild: true);
                        break;
                    }
                case SwManagerViewElementTagId.CreateNewVersionButton:
                    {
                        var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                        if (handler == null) return;
                        await handler.ExecuteTask(taskKey,
                            mainFunc: async () =>
                            {
                                if (SwEditorMode == EditorMode.ADD_NEW_EDITOR_MODE)
                                {
                                    var versionItemVM = BuildSwVersionItemViewModelFromViewElement();
                                    var isSuccess = await SwManagerTabViewModel.CreateNewSwVersion(versionItemVM
                                        , SwListVersionCombobox.ItemsSource as ObservableCollection<BaseObjectVersionItemViewModel>);
                                    if (isSuccess) SwListVersionCombobox.SelectedIndex = 0;
                                }
                                else if (SwEditorMode == EditorMode.MODIFY_EDITOR_MODE)
                                {
                                    var versionItemVM = BuildSwVersionItemViewModelFromViewElement();

                                    var isSuccess = await SwManagerTabViewModel.AddNewSwVersionForEdittingMode(versionItemVM
                                       , ModifingContext);
                                    if (isSuccess)
                                    {
                                        SwListVersionCombobox.SelectedIndex = 0;
                                        MessageBox.Show("Thêm version mới thành công!", "Thông báo");
                                    }
                                    else
                                    {
                                        MessageBox.Show("Thêm version mới thất bại!", "Thông báo");
                                    }
                                }
                            },
                            executeTime: 0,
                            bypassIfSemaphoreNotAvaild: true);
                        break;
                    }
                case SwManagerViewElementTagId.AddSwToDbButton:
                    {
                        var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                        if (handler == null) return;
                        await handler.ExecuteTask(taskKey,
                        mainFunc: async () =>
                        {
                            if (SwEditorMode == EditorMode.ADD_NEW_EDITOR_MODE)
                            {
                                var success = await SwManagerTabViewModel.AddNewSwToDb(
                                   SwKeyTextbox.Text
                                   , SwNameTextbox.Text
                                   , SwAuthorTextbox.Text
                                   , SwDescriptionTextBox.Text
                                   , SwURLTextBox.Text
                                   , SwIconSourceTextBox.Text
                                   , SwIsPrereleasedCheckBox.IsChecked ?? false
                                   , SwIsAuthenticatedCheckBox.IsChecked ?? false
                                   , SwListVersionCombobox.ItemsSource as ObservableCollection<BaseObjectVersionItemViewModel>);
                                if (success) RefreshTab();
                            }

                        },
                        executeTime: 1000,
                        bypassIfSemaphoreNotAvaild: true);
                        break;
                    }
                case SwManagerViewElementTagId.RefreshVersionFieldButton:
                    {
                        RefreshVersionField();
                        break;
                    }
                case SwManagerViewElementTagId.OpenIconFileChooserButton:
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
                                    SwIconSourceTextBox.Text = ofd.FileName;
                                }
                            }
                            catch
                            {
                                MessageBox.Show("Not support this format!~");
                            }
                        }
                        break;
                    }
                case SwManagerViewElementTagId.ExitModifingSwButton:
                    {
                        var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                        if (handler == null) return;
                        await handler.ExecuteTask(taskKey,
                           mainFunc: async () =>
                           {
                               await CyberDbManager.Current.RequestDbContextAsync((dbcontext) =>
                               {
                                   CyberDbManager.Current.RollBack(force: false);
                               });
                               await SwManagerTabViewModel.ReloadSwSource();
                           },
                           executeTime: 0,
                           bypassIfSemaphoreNotAvaild: true);
                        SwEditorMode = EditorMode.ADD_NEW_EDITOR_MODE;
                        break;
                    }
                case SwManagerViewElementTagId.SaveSwToDbButton:
                    {
                        var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                        if (handler == null) return;
                        await handler.ExecuteTask(taskKey,
                           mainFunc: async () =>
                           {
                               if (SwEditorMode == EditorMode.MODIFY_EDITOR_MODE && ModifingContext != null)
                               {
                                   var success = await SwManagerTabViewModel.SaveModifyingSwToDb(
                                   SwKeyTextbox.Text
                                   , SwNameTextbox.Text
                                   , SwAuthorTextbox.Text
                                   , SwIconSourceTextBox.Text
                                   , SwListVersionCombobox.ItemsSource as ObservableCollection<BaseObjectVersionItemViewModel>
                                   , ModifingContext);
                                   if (success)
                                   {
                                       ModifingContext = null;
                                       SwEditorMode = EditorMode.ADD_NEW_EDITOR_MODE;
                                   }
                               }

                           },
                           executeTime: 0,
                           bypassIfSemaphoreNotAvaild: true);
                        break;
                    }
                case SwManagerViewElementTagId.SyncSwFolderWithDbButton:
                    {
                        var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                        if (handler == null) return;
                        await handler.ExecuteTask(taskKey,
                            mainFunc: async () =>
                            {
                                // await PART_ToolManagerTabViewModel.SyncSwFolderWithDb();
                            },
                            executeTime: 2000,
                            bypassIfSemaphoreNotAvaild: true);
                        break;
                    }
                case SwManagerViewElementTagId.AccessBaseFolderTabButton:
                    {
                        Process.Start(CyberServerDefinition.TOOL_BASE_FOLDER_PATH);
                        break;
                    }
                case SwManagerViewElementTagId.SaveEditVersionButton:
                    {
                        if (SwEditorMode == EditorMode.MODIFY_EDITOR_MODE)
                        {
                            var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                            if (handler == null) return;
                            await handler.ExecuteTask(taskKey,
                              mainFunc: async () =>
                              {
                                  var success = await SwManagerTabViewModel.SaveEdittedToolVersionToDb(_oldVersionCache.Trim()
                                    , ModifingContext
                                    , ModifingContext.SelectedSwVersionItemForEditting);
                                  if (success)
                                  {
                                      ModifingContext.SelectedSwVersionItemForEditting = null;
                                      SwVersionEditorMode = EditorMode.ADD_NEW_EDITOR_MODE;
                                  }
                              },
                              executeTime: 0,
                              bypassIfSemaphoreNotAvaild: true);
                        }

                        break;
                    }
                case SwManagerViewElementTagId.ExitEditVersionButton:
                    {
                        if (SwEditorMode == EditorMode.MODIFY_EDITOR_MODE)
                        {
                            var currentEditToolVM = SwEditorPanel.DataContext as BaseObjectSwItemViewModel;
                            if (currentEditToolVM != null)
                            {
                                if (CyberDbManager.Current.RollBack(force: false))
                                {
                                    currentEditToolVM.SelectedSwVersionItemForEditting = null;
                                    currentEditToolVM.SelectedSwVersionItemForEditting?.RefreshViewModel();
                                    SwVersionEditorMode = EditorMode.ADD_NEW_EDITOR_MODE;
                                }
                            }
                        }
                        else if (SwEditorMode == EditorMode.ADD_NEW_EDITOR_MODE)
                        {
                            var selectedVersionVM = AddingContext.SelectedSwVersionItemForEditting;
                            var versionItemVM = BuildSwVersionItemViewModelFromViewElement();

                            if (selectedVersionVM != null && SwManagerTabViewModel.RenewSwVersionIndexForAddingMode(versionItemVM
                                        , selectedVersionVM
                                        , SwListVersionCombobox.ItemsSource as ObservableCollection<BaseObjectVersionItemViewModel>))
                            {
                                SwVersionEditorMode = EditorMode.ADD_NEW_EDITOR_MODE;
                                AddingContext.SelectedSwVersionItemForEditting = null;
                            }
                        }

                        break;
                    }
                case SwManagerViewElementTagId.ModifySwItemButton:
                    {
                        ModifingContext = btn.DataContext as BaseObjectSwItemViewModel;
                        SwEditorMode = EditorMode.MODIFY_EDITOR_MODE;
                        break;
                    }
                case SwManagerViewElementTagId.DeleteSwItemButton:
                    {
                        var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                        if (handler == null) return;
                        var itemViewModel = btn.DataContext as BaseObjectSwItemViewModel;
                        if (itemViewModel != null)
                        {
                            var confirm = MessageBox.Show("Bạn có chắc xóa dữ liệu này!", "Xác nhận", MessageBoxButton.YesNo);
                            if (confirm == MessageBoxResult.Yes)
                            {
                                await handler.ExecuteTask(taskKey,
                                   mainFunc: async () =>
                                   {
                                       var success = await SwManagerTabViewModel.DeleteSwFromDb(itemViewModel);
                                       MessageBox.Show(success ? "Xóa dữ liệu thành công" : "Lỗi thao tác xóa dữ liệu");
                                   },
                                   executeTime: 0,
                                   bypassIfSemaphoreNotAvaild: true);
                            }
                        }
                        break;
                    }
                case SwManagerViewElementTagId.DeleteVersionItem:
                    {
                        var tcontext = btn.DataContext as BaseObjectVersionItemViewModel;
                        var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                        if (handler == null || tcontext == null) return;
                        await handler.ExecuteTask(taskKey,
                            mainFunc: async () =>
                            {
                                if (SwEditorMode == EditorMode.ADD_NEW_EDITOR_MODE)
                                {
                                    var isSuccess = await SwManagerTabViewModel.DeleteVerionInAddingMode(
                                        tcontext
                                        , SwListVersionCombobox.ItemsSource as ObservableCollection<BaseObjectVersionItemViewModel>);
                                }
                                else if (SwEditorMode == EditorMode.MODIFY_EDITOR_MODE)
                                {
                                    if (SwVersionEditorMode == EditorMode.MODIFY_EDITOR_MODE)
                                    {
                                        MessageBox.Show("Thooát chế độ chỉnh sửa version trước!");
                                        return;
                                    }
                                    var isSuccess = await SwManagerTabViewModel.DeleteVerionInModifyingMode(
                                       tcontext
                                       , ModifingContext);
                                }
                            },
                            executeTime: 0,
                            bypassIfSemaphoreNotAvaild: true);
                        break;
                    }
                case SwManagerViewElementTagId.ModifyVersionItem:
                    {
                        var selectedModifyVersionVM = btn.DataContext as BaseObjectVersionItemViewModel;
                        var currentEditToolVM = SwEditorPanel.DataContext as BaseObjectSwItemViewModel;
                        if (selectedModifyVersionVM != null && currentEditToolVM != null)
                        {
                            currentEditToolVM.SelectedSwVersionItemForEditting = selectedModifyVersionVM;
                            SwVersionEditorMode = EditorMode.MODIFY_EDITOR_MODE;
                        }
                        break;
                    }
            }

            var checkBox = sender as CheckBox;
            switch (checkBox?.Tag)
            {
                case SwManagerViewElementTagId.MarkRequireLatestVersionCheckBox:
                    {
                        var context = checkBox.DataContext as BaseObjectSwItemViewModel;
                        var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                        if (handler == null || context == null) return;
                        var isEditable = true;
                        if (SwEditorMode == EditorMode.MODIFY_EDITOR_MODE)
                        {
                            MessageBox.Show("Thoát chế độ chỉnh sửa để thực hiện thao tác này!");
                            isEditable = false;
                        }
                        await handler.ExecuteTask(taskKey,
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


        private void HandleSwItemEditorModeChanged(EditorMode newValue)
        {
            switch (newValue)
            {
                case EditorMode.ADD_NEW_EDITOR_MODE:
                    SwEditorPanel.DataContext = AddingContext;
                    SaveSwToDbButton.Visibility = Visibility.Collapsed;
                    AddSwToDbButton.Visibility = Visibility.Visible;
                    ExitModifingSwButton.Visibility = Visibility.Collapsed;
                    SwKeyTextbox.IsEnabled = true;
                    _oldToolKeyCache = "";
                    break;
                case EditorMode.MODIFY_EDITOR_MODE:
                    IsToolEditorOpen = true;
                    SwKeyTextbox.IsEnabled = false;
                    SwEditorPanel.DataContext = ModifingContext;
                    SaveSwToDbButton.Visibility = Visibility.Visible;
                    AddSwToDbButton.Visibility = Visibility.Collapsed;
                    ExitModifingSwButton.Visibility = Visibility.Visible;
                    _oldToolKeyCache = ModifingContext.StringId;
                    break;
            }
        }

        private void HandleSwVersionItemEditorModeChanged(EditorMode newValue)
        {
            switch (newValue)
            {
                case EditorMode.ADD_NEW_EDITOR_MODE:
                    CreateNewVersionButton.Visibility = Visibility.Visible;
                    ExitEditVersionButton.Visibility = Visibility.Collapsed;
                    SaveEditVersionButton.Visibility = Visibility.Collapsed;
                    _oldVersionCache = "";
                    break;
                case EditorMode.MODIFY_EDITOR_MODE:
                    if (SwEditorMode == EditorMode.MODIFY_EDITOR_MODE)
                    {
                        ExitEditVersionButton.Visibility = Visibility.Visible;
                        SaveEditVersionButton.Visibility = Visibility.Visible;
                        _oldVersionCache = ModifingContext.SelectedSwVersionItemForEditting.Version;
                    }
                    else
                    {
                        ExitEditVersionButton.Visibility = Visibility.Visible;
                        SaveEditVersionButton.Visibility = Visibility.Collapsed;
                        _oldVersionCache = AddingContext.SelectedSwVersionItemForEditting.Version;
                    }
                    CreateNewVersionButton.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void BaseHandleLoadedEvent(object sender, RoutedEventArgs e)
        {
            IsToolEditorOpen = false;
            OnSwManagerTabLoaded();
        }

        private void BaseHandleSwManagerTabInitialized(object sender, EventArgs e)
        {
            SetBinding(BaseSwManagerTabUserControl.SwEditorModeProperty, new Binding()
            {
                Source = SwManagerTabViewModel,
                Mode = BindingMode.TwoWay,
                Path = new PropertyPath(nameof(SwManagerTabViewModel.CurrentEditorMode)),
                NotifyOnSourceUpdated = true,
            });
            OnSwManagerTabInitialized();

            HandleSwItemEditorModeChanged(SwEditorMode);
            HandleSwVersionItemEditorModeChanged(SwVersionEditorMode);
        }

        private void RefreshTab()
        {
            _compressToolSizeCache = 0;
            _rawToolSizeCache = 0;
            (SwListVersionCombobox.ItemsSource as IList)?.Clear();
            CompressLengthRun.Text = "";
            RawLengthRun.Text = "";
            SwKeyTextbox.Text = "";
            SwNameTextbox.Text = "";
            SwAuthorTextbox.Text = "";
            SwDescriptionTextBox.Text = "";
            SwVersionTextBox.Text = "";
            SwPathTextBox.Text = "";
            SwExecutePathTextBox.Text = "";
            SwatePublisedDatePicker.SelectedDate = null;
            SwVersionDescriptionTextBox.Text = "";
            SwURLTextBox.Text = "";
            SwIconSourceTextBox.Text = "";
            SwIsAuthenticatedCheckBox.IsChecked = false;
            SwIsPrereleasedCheckBox.IsChecked = false;
        }

        private void RefreshVersionField()
        {
            _compressToolSizeCache = 0;
            _rawToolSizeCache = 0;
            SwVersionTextBox.Text = "";
            SwPathTextBox.Text = "";
            SwExecutePathTextBox.Text = "";
            SwatePublisedDatePicker.SelectedDate = null;
            SwVersionDescriptionTextBox.Text = "";
        }

        protected abstract BaseObjectVersionItemViewModel BuildSwVersionItemViewModelFromViewElement();
        protected abstract void OnSwManagerTabInitialized();
        protected abstract void OnSwManagerTabLoaded();
        protected abstract void HandleButtonClickEvent(object sender, RoutedEventArgs e);
        protected abstract string BuildTaskTypeKey(object tag);

    }
}
