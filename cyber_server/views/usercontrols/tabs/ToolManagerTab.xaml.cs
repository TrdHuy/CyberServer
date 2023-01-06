using cyber_server.definition;
using cyber_server.implements.db_manager;
using cyber_server.implements.task_handler;
using cyber_server.view_models.list_view_item;
using cyber_server.view_models.list_view_item.tool_item;
using cyber_server.view_models.tabs;
using cyber_server.views.usercontrols.others;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace cyber_server.views.usercontrols.tabs
{
    /// <summary>
    /// Interaction logic for ToolManagerTab.xaml
    /// </summary>

    public partial class ToolManagerTab : BaseSwManagerTabUserControl
    {
        private ToolItemViewModel _modifingContext;
        private ToolItemViewModel _addingContext;

        protected override BaseSwManagerTabViewModel SwManagerTabViewModel => PART_ToolManagerTabViewModel;

        protected override Grid SwEditorPanel => PART_ToolEditorPanel;
        protected override Button ExpandSwEditorButton => PART_OpenToolEditorButton;
        protected override Button SaveSwToDbButton => PART_SaveToolToDb;
        protected override Button AddSwToDbButton => PART_AddToolToDb;
        protected override Button ExitModifingSwButton => PART_ExitModifingTool;
        protected override TextBox SwKeyTextbox => PART_ToolKeyTb;
        protected override Button CreateNewVersionButton => PART_CreateNewVersionBtn;
        protected override Button ExitEditVersionButton => PART_ExitEditVersionBtn;
        protected override Button SaveEditVersionButton => PART_SaveEditVersionBtn;
        protected override TextBox SwNameTextbox => PART_ToolNameTb;
        protected override TextBox SwAuthorTextbox => PART_ToolAuthorTb;
        protected override TextBox SwVersionTextBox => PART_ToolVersionTb;
        protected override ComboBox SwListVersionCombobox => PART_ListVersionCbx;
        protected override Run CompressLengthRun => PART_CompressLengthRun;
        protected override Run RawLengthRun => PART_RawLengthRun;
        protected override TextBox SwDescriptionTextBox => PART_ToolDesTb;
        protected override TextBox SwPathTextBox => PART_PathToToolTextbox;
        protected override TextBox SwExecutePathTextBox => PART_ExecutePathTextbox;
        protected override DatePicker SwatePublisedDatePicker => PART_DatePublisedDP;
        protected override TextBox SwVersionDescriptionTextBox => PART_VersionDesTb;
        protected override TextBox SwURLTextBox => PART_ToolURLTb;
        protected override TextBox SwIconSourceTextBox => PART_ToolIconSourceTb;
        protected override CheckBox SwIsAuthenticatedCheckBox => PART_ToolIsAuthenticatedCb;
        protected override CheckBox SwIsPrereleasedCheckBox => PART_ToolIsPrereleasedCb;
        protected override Button ClearAddSwButton => PART_ClearAddToolTab;
        protected override Button ReloadSwFromDbButton => PART_ReloadToolFromDb;
        protected override Button OpenSwFileChooserButton => PART_OpenToolFileChooser;
        protected override Button RefreshVersionFieldButton => PART_RefreshVersionFieldBtn;
        protected override Button OpenIconFileChooserButton => PART_OpenIconFileChooser;
        protected override Button SyncSwFolderWithDbButton => PART_SyncToolFolderWithDbButton;
        protected override Button AccessBaseFolderTabButton => PART_AccessBaseFolderTab;
        protected override TextBox SwVersionAssemblyNameTextBox => PART_ToolVersionAssemblyNameTb;

        protected override string BuildTaskTypeKey(object tag)
        {
            switch (tag)
            {
                case SwManagerViewElementTagId.SaveSwToDbButton:
                case SwManagerViewElementTagId.MarkRequireLatestVersionCheckBox:
                    return CurrentTaskManager.MODIFI_TOOL_TASK_TYPE_KEY;
                case SwManagerViewElementTagId.DeleteVersionItem:
                    return CurrentTaskManager.DELETE_VERSION_TASK_TYPE_KEY;
                case SwManagerViewElementTagId.DeleteSwItemButton:
                    return CurrentTaskManager.DELETE_TOOL_TASK_TYPE_KEY;
                case SwManagerViewElementTagId.SaveEditVersionButton:
                    return CurrentTaskManager.MODIFI_VERSION_TASK_TYPE_KEY;
                case SwManagerViewElementTagId.SyncSwFolderWithDbButton:
                    return CurrentTaskManager.SYNC_TASK_TYPE_KEY;
                case SwManagerViewElementTagId.ReloadSwFromDbButton:
                    return CurrentTaskManager.RELOAD_TOOL_TASK_TYPE_KEY;
                case SwManagerViewElementTagId.OpenSwFileChooserButton:
                    return CurrentTaskManager.IMPORT_TOOL_TASK_TYPE_KEY;
                case SwManagerViewElementTagId.CreateNewVersionButton:
                    return CurrentTaskManager.ADD_VERSION_TASK_TYPE_KEY;
                case SwManagerViewElementTagId.AddSwToDbButton:
                    return CurrentTaskManager.ADD_TOOL_TASK_TYPE_KEY;
                case SwManagerViewElementTagId.ExitModifingSwButton:
                    return CurrentTaskManager.RELOAD_TOOL_TASK_TYPE_KEY;
                case SwManagerViewElementTagId.ExtractVersionItemToFile:
                    return CurrentTaskManager.OTHER_TOOL_TASK_TYPE_KEY;
                default: return "";
            }
        }
        public override BaseObjectSwItemViewModel ModifingContext
        {
            get
            {
                return _modifingContext;
            }
            set
            {
                _modifingContext = (value as ToolItemViewModel);

            }
        }

        public override BaseObjectSwItemViewModel AddingContext
        {
            get
            {
                return _addingContext;
            }
            set
            {
                _addingContext = (value as ToolItemViewModel);

            }
        }

        public ToolManagerTab()
        {
            InitializeComponent();
        }


        protected override void OnSwManagerTabInitialized()
        {
            _addingContext = new ToolItemViewModel(null);
        }

        protected override void OnSwManagerTabLoaded()
        {
        }

        protected override async void HandleButtonClickEvent(object sender, RoutedEventArgs e)
        {
            await base.BaseHandleButtonClickEvent(sender, e);
        }

        protected override BaseObjectVersionItemViewModel BuildSwVersionItemViewModelFromViewElement()
        {
            return new ToolVersionItemViewModel(null)
            {
                IsNewConceptSwVersionBuild = _isNewConceptSwVersionBuild,
                NewConceptBuildInfo = _newBuildConceptSwVersionBuildInfo,
                Version = PART_ToolVersionTb.Text,
                FilePath = PART_PathToToolTextbox.Text,
                DatePublished = PART_DatePublisedDP.Text,
                Description = PART_VersionDesTb.Text,
                ExecutePath = PART_ExecutePathTextbox.Text,
                CompressLength = _compressToolSizeCache + "",
                AssemblyName = PART_ToolVersionAssemblyNameTb.Text,
                RawLength = _rawToolSizeCache + "",
            };
        }
    }
}
