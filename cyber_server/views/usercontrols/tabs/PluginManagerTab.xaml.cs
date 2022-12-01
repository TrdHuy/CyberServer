using cyber_server.implements.task_handler;
using cyber_server.view_models.list_view_item;
using cyber_server.view_models.list_view_item.plugin_item;
using cyber_server.view_models.tabs;
using cyber_server.views.usercontrols.others;
using cyber_server.views.windows.others;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for PluginManagerTab.xaml
    /// </summary>
    public partial class PluginManagerTab : BaseSwManagerTabUserControl
    {
        private PluginItemViewModel _modifingContext;
        private PluginItemViewModel _addingContext;

        public PluginManagerTab()
        {
            InitializeComponent();
        }

        public override BaseObjectSwItemViewModel ModifingContext { get => _modifingContext; set => _modifingContext = value as PluginItemViewModel; }
        public override BaseObjectSwItemViewModel AddingContext { get => _addingContext; set => _addingContext = value as PluginItemViewModel; }

        protected override BaseSwManagerTabViewModel SwManagerTabViewModel => PART_PluginManagerTabViewModel;

        protected override Grid SwEditorPanel => PART_PluginEditorPanel;

        protected override Button AccessBaseFolderTabButton => PART_AccessBaseFolderTabButton;

        protected override Button ExpandSwEditorButton => PART_ExpandPluginEditorButton;

        protected override Button SaveSwToDbButton => PART_SavePluginToDb;

        protected override Button AddSwToDbButton => PART_AddPluginToDb;

        protected override Button ExitModifingSwButton => PART_ExitModifingTool;

        protected override Button ReloadSwFromDbButton => PART_ReloadPluginFromDb;

        protected override Button CreateNewVersionButton => PART_CreateNewVersionBtn;

        protected override Button ExitEditVersionButton => PART_ExitEditVersionBtn;

        protected override Button ClearAddSwButton => PART_ClearAddPluginTab;

        protected override Button SaveEditVersionButton => PART_SaveEditVersionBtn;

        protected override Button OpenSwFileChooserButton => PART_OpenPluginFileChooser;

        protected override Button RefreshVersionFieldButton => PART_RefreshVersionFieldBtn;

        protected override Button OpenIconFileChooserButton => PART_OpenIconFileChooser;

        protected override Button SyncSwFolderWithDbButton => PART_SyncPluginFolderWithDbButton;

        protected override TextBox SwKeyTextbox => PART_PluginKeyTb;

        protected override TextBox SwNameTextbox => PART_PluginNameTb;

        protected override TextBox SwAuthorTextbox => PART_PluginAuthorTb;

        protected override TextBox SwVersionTextBox => PART_PluginVersionTb;

        protected override ComboBox SwListVersionCombobox => PART_ListVersionCbx;

        protected override Run CompressLengthRun => PART_CompressLengthRun;

        protected override Run RawLengthRun => PART_RawLengthRun;

        protected override TextBox SwDescriptionTextBox => PART_PluginDesTb;

        protected override TextBox SwPathTextBox => PART_PathToPLuginTextbox;

        protected override TextBox SwExecutePathTextBox => PART_ExecutePathTextbox;

        protected override DatePicker SwatePublisedDatePicker => PART_DatePublisedDP;

        protected override TextBox SwVersionDescriptionTextBox => PART_VersionDesTb;

        protected override TextBox SwURLTextBox => PART_PluginURLTb;

        protected override TextBox SwIconSourceTextBox => PART_PluginIconSourceTb;

        protected override CheckBox SwIsAuthenticatedCheckBox => PART_PluginIsAuthenticatedCb;

        protected override CheckBox SwIsPrereleasedCheckBox => PART_PluginIsPrereleasedCb;

        protected override string BuildTaskTypeKey(object tag)
        {
            switch (tag)
            {
                case SwManagerViewElementTagId.SaveSwToDbButton:
                case SwManagerViewElementTagId.MarkRequireLatestVersionCheckBox:
                    return CurrentTaskManager.MODIFI_PLUGIN_TASK_TYPE_KEY;
                case SwManagerViewElementTagId.DeleteVersionItem:
                    return CurrentTaskManager.DELETE_VERSION_TASK_TYPE_KEY;
                case SwManagerViewElementTagId.DeleteSwItemButton:
                    return CurrentTaskManager.DELETE_PLUGIN_TASK_TYPE_KEY;
                case SwManagerViewElementTagId.SaveEditVersionButton:
                    return CurrentTaskManager.MODIFI_VERSION_TASK_TYPE_KEY;
                case SwManagerViewElementTagId.SyncSwFolderWithDbButton:
                    return CurrentTaskManager.SYNC_TASK_TYPE_KEY;
                case SwManagerViewElementTagId.ReloadSwFromDbButton:
                    return CurrentTaskManager.RELOAD_PLUGIN_TASK_TYPE_KEY;
                case SwManagerViewElementTagId.OpenSwFileChooserButton:
                    return CurrentTaskManager.IMPORT_PLUGIN_TASK_TYPE_KEY;
                case SwManagerViewElementTagId.CreateNewVersionButton:
                    return CurrentTaskManager.ADD_VERSION_TASK_TYPE_KEY;
                case SwManagerViewElementTagId.AddSwToDbButton:
                    return CurrentTaskManager.ADD_PLUGIN_TASK_TYPE_KEY;
                case SwManagerViewElementTagId.ExitModifingSwButton:
                    return CurrentTaskManager.RELOAD_PLUGIN_TASK_TYPE_KEY;
                default: return "";
            }
        }

        protected override BaseObjectVersionItemViewModel BuildSwVersionItemViewModelFromViewElement()
        {
            return new PluginVersionItemViewModel()
            {
                Version = PART_PluginVersionTb.Text,
                FilePath = PART_PathToPLuginTextbox.Text,
                DatePublished = PART_DatePublisedDP.Text,
                Description = PART_VersionDesTb.Text,
                ExecutePath = PART_ExecutePathTextbox.Text,
                CompressLength = _compressToolSizeCache + "",
                RawLength = _rawToolSizeCache + "",
                MainClassName = PART_PathMainClassNameTextbox.Text
            };
        }

        protected async override void HandleButtonClickEvent(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            switch (btn?.Name)
            {
                case "PART_QuickFillExecutePathButton":
                    {
                        bool IsMeetConditionToCreateExecutePath()
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
                            eBW.Owner = App.Current.ServerWindow;
                            var pathToExecute = eBW.Show();
                            if (!eBW.IsCanceled)
                            {
                                PART_ExecutePathTextbox.Text = pathToExecute;
                            }
                        }
                        return;
                    }
            }

            await base.BaseHandleButtonClickEvent(sender, e);
        }
        protected override void OnSwManagerTabInitialized()
        {
            _addingContext = new PluginItemViewModel(null);
        }

        protected override void OnSwManagerTabLoaded()
        {
        }
    }
}
