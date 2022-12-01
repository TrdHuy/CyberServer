using cyber_server.definition;
using cyber_server.implements.db_manager;
using cyber_server.implements.log_manager;
using cyber_server.implements.plugin_manager;
using cyber_server.implements.task_handler;
using cyber_server.views.usercontrols.others;
using cyber_server.views.windows.others;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public CyberServerWindow()
        {
            InitializeComponent();
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.BACKUP_DATABASE_TYPE_KEY, "Backing up database to csv!", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.IMPORT_CSV_TO_DATABASE_TYPE_KEY, "Importing csv to database!", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.MODIFI_PLUGIN_TASK_TYPE_KEY, "Modifying plugin data", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.MODIFI_TOOL_TASK_TYPE_KEY, "Modifying tool data", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.MODIFI_VERSION_TASK_TYPE_KEY, "Modifying tool version data", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.ADD_VERSION_TASK_TYPE_KEY, "Adding new version", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.ADD_PLUGIN_TASK_TYPE_KEY, "Adding new plugin", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.ADD_TOOL_TASK_TYPE_KEY, "Adding new tool", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.RELOAD_TOOL_TASK_TYPE_KEY, "Reloading tools", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.RELOAD_PLUGIN_TASK_TYPE_KEY, "Reloading plugins", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.DELETE_PLUGIN_TASK_TYPE_KEY, "Deleting plugin", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.DELETE_TOOL_TASK_TYPE_KEY, "Deleting tool", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.DELETE_VERSION_TASK_TYPE_KEY, "Deleting version", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.IMPORT_TOOL_TASK_TYPE_KEY, "Importing tool", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.IMPORT_PLUGIN_TASK_TYPE_KEY, "Importing plugin", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.SYNC_TASK_TYPE_KEY, "Syncing", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.SAVE_CERTIFICATE_TO_DB_TASK_TYPE_KEY, "Saving certificate", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.RELOAD_CERTIFICATE_FROM_DB_TASK_TYPE_KEY, "Reloading certificate", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.DELETE_CERTIFICATE_FROM_DB_TASK_TYPE_KEY, "Deleting certificate", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.CHECK_VALIDATION_CERTIFICATE_TYPE_KEY, "Checking validation", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.GET_DATABASE_TABLE_DATA, "Getting table data", 1, 1);
            PART_TaskHandlingPanel.GenerateTaskSemaphore(CurrentTaskManager.DROP_ALL_DATABASE_TABLE_DATA, "Dropping table data", 1, 1);
            TaskHandlerManager.Current.RegisterHandler(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY, PART_TaskHandlingPanel);
        }

        private void HandleMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var ti = sender as TabItem;
            switch (ti?.Name)
            {
                case "PART_CertificateManagerTabItem":
                    {
                        PART_CertificateManagerTabUC.OnTabReloaded(ti, e);
                        break;
                    }

            }
        }
    }
}
