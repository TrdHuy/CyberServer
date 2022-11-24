using cyber_base.implement.async_task;
using cyber_server.implements.db_manager;
using cyber_server.implements.http_server;
using cyber_server.implements.http_server.handlers;
using cyber_server.implements.task_handler;
using cyber_server.views.usercontrols.others;
using cyber_server.views.windows.others;
using cyber_server_base.async_task.core;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
    /// Interaction logic for ServerControllerTab.xaml
    /// </summary>
    public partial class ServerControllerTab : UserControl
    {
        public ServerControllerTab()
        {
            InitializeComponent();
        }

        private async void HandleButtonClickEvent(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                switch (btn.Name)
                {
                    case "PART_StartServerButton":
                        {
                            await CyberHttpServer.Current.StartAsync();
                            break;
                        }
                    case "PART_StopServerButton":
                        {
                            CyberHttpServer.Current.Stop();
                            break;
                        }
                    case "PART_BackupDbButton":
                        {
                            var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                            if (handler == null) return;
                            await handler.ExecuteTask(CurrentTaskManager.BACKUP_DATABASE_TYPE_KEY,
                            mainFunc: async () =>
                            {
                                try
                                {
                                    using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                                    {
                                        dialog.Description = "Hãy chọn thư mục lưu file backup!";
                                        System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                                        if (result == System.Windows.Forms.DialogResult.OK
                                            && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                                        {
                                            var path = await CyberDbManager.Current.Backup(dialog.SelectedPath);
                                            if (path != null && path.Count > 0)
                                                MessageBox.Show("Xuất file csv thành công!");
                                        }

                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Xuất file csv thất bại!\n" + ex.Message);
                                }
                            },
                            executeTime: 0,
                            bypassIfSemaphoreNotAvaild: true);
                            break;
                        }
                    case "PART_ImportCsvToDbButton":
                        {
                            var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                            if (handler == null) return;
                            await handler.ExecuteTask(CurrentTaskManager.IMPORT_CSV_TO_DATABASE_TYPE_KEY,
                            mainFunc: async () =>
                            {
                                try
                                {
                                    var ofd = new OpenFileDialog();
                                    ofd.Filter = "Csv files (*.csv)|*.csv";
                                    ofd.Multiselect = true;
                                    var result = ofd.ShowDialog();
                                    await CyberDbManager.Current.ImportCSVToDb(ofd.FileNames);
                                    if (ofd.FileNames.Length > 0)
                                    {
                                        MessageBox.Show("Import csv thành công!");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Import csv thất bại!\n" + ex.Message);
                                }
                            },
                            executeTime: 0,
                            bypassIfSemaphoreNotAvaild: true);
                            break;
                        }
                    case "PART_ShowDatabseTableButton":
                        {
                            var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                            if (handler == null) return;
                            await handler.ExecuteTask(CurrentTaskManager.GET_DATABASE_TABLE_DATA,
                            mainFunc: async () =>
                            {
                                await CyberDbManager.Current.ShowDatabaseTable2();
                            },
                            executeTime: 0,
                            bypassIfSemaphoreNotAvaild: true);
                            break;
                        }
                    case "PART_DropAllTableButton":
                        {
                            var handler = TaskHandlerManager.Current.GetHandlerByKey(TaskHandlerManager.SERVER_WINDOW_HANDLER_KEY);
                            if (handler == null) return;
                            await handler.ExecuteTask(CurrentTaskManager.GET_DATABASE_TABLE_DATA,
                            mainFunc: async () =>
                            {
                                await CyberDbManager.Current.DropAllTable();
                                MessageBox.Show("Reset dữ liệu thành công", "Thông báo");
                            },
                            executeTime: 0,
                            bypassIfSemaphoreNotAvaild: true);
                            break;
                        }
                }
            }
        }

        private async void testRequest(object sender, RoutedEventArgs e)
        {
            switch (PART_TestRequestOptionBox.SelectedIndex)
            {
                case 0:
                    {
                        var httpContent = new StringContent("HelloWorld222");

                        httpContent.Headers.Add("h2sw-request-info", "GET_ALL_PLUGIN_DATA");

                        using (HttpClient client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Add("h2sw-request-info", "GET_ALL_PLUGIN_DATA");
                            client.DefaultRequestHeaders.Add("GET_ALL_PLUGIN_DATA__MAXIMUM_AMOUNT", "10");
                            client.DefaultRequestHeaders.Add("GET_ALL_PLUGIN_DATA__START_INDEX", "0");
                            var response = await client.GetAsync("http://107.127.131.89:8080/requestinfo");

                            var responseContent = await response.Content.ReadAsStringAsync();
                            var w = new TextWindow(responseContent);
                            w.ShowDialog();
                        }
                        break;
                    }
                case 3:
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            var toolKey = "cyber_tool";
                            var toolVersion = "1.0.0.2";
                            client.DefaultRequestHeaders.Add(CyberHttpServer.REQUEST_DOWNLOAD_TOOL_HEADER_KEY
                                , RequestDownloadToolHttpHandler.REQUEST_CHECK_TOOL_DOWNLOADABLE_HEADER_ID);
                            client.DefaultRequestHeaders.Add(RequestDownloadToolHttpHandler.REQUEST_KEY_TO_CHECK_DOWNLOADABLE_HEADER_ID
                                , toolKey);
                            client.DefaultRequestHeaders.Add(RequestDownloadToolHttpHandler.REQUEST_VERSION_TO_CHECK_DOWNLOADABLE_HEADER_ID
                                , toolVersion);
                            var response = await client.GetAsync("http://107.127.131.89:8080" + CyberHttpServer.DOWNLOAD_TOOL_API_PATH);

                            var responseContent = await response.Content.ReadAsStringAsync();
                            var w = new TextWindow(responseContent);
                            w.ShowDialog();

                            var isDownloadable = false;
                            var fileName = "";
                            var executePath = "";
                            try
                            {
                                isDownloadable = response.Headers.GetValues(RequestDownloadToolHttpHandler.RESPONSE_IS_TOOL_DOWNLOADABLE_HEADER_ID)
                                    .FirstOrDefault() == "1";
                                fileName = response.Headers.GetValues(RequestDownloadToolHttpHandler.RESPONSE_TOOL_FILE_NAME_HEADER_ID)
                                    .FirstOrDefault();
                                executePath = response.Headers.GetValues(RequestDownloadToolHttpHandler.RESPONSE_TOOL_EXECUTE_PATH_HEADER_ID)
                                    .FirstOrDefault();
                            }
                            catch
                            {
                                isDownloadable = false;
                            }

                            if (!isDownloadable)
                            {
                                return;
                            }
                            else
                            {
                                var downloadFileFolder = "temp\\" + "tools"
                                        + "\\" + toolKey + "\\" + toolVersion;
                                var versionExecutePath = "temp\\" + "tools"
                                        + "\\" + toolKey + "\\" + toolVersion + "\\" + executePath;
                                if (!Directory.Exists(downloadFileFolder))
                                {
                                    Directory.CreateDirectory(downloadFileFolder);
                                }
                                var downloadFilePath = downloadFileFolder + "\\" + fileName;
                                var requestHeaderContentMap = new Dictionary<string, string>();
                                requestHeaderContentMap.Add(CyberHttpServer.REQUEST_DOWNLOAD_TOOL_HEADER_KEY
                                    , RequestDownloadToolHttpHandler.REQUEST_DOWNLOAD_TOOL_HEADER_ID);
                                requestHeaderContentMap.Add(RequestDownloadToolHttpHandler.REQUEST_DOWNLOAD_TOOL_KEY_HEADER_ID
                                    , toolKey);
                                requestHeaderContentMap.Add(RequestDownloadToolHttpHandler.REQUEST_DOWNLOAD_TOOL_VERSION_HEADER_ID
                                    , toolVersion);
                                var param = new object[] { "http://107.127.131.89:8080/downloadtool"
                                    , downloadFilePath
                                    , requestHeaderContentMap};
                                var downloadTask = new DownloadPluginTask(param);
                                downloadTask.ProgressChanged += (s, e2) =>
                                {
                                };
                                await downloadTask.Execute();

                                if (downloadTask.IsCompleted)
                                {

                                }
                            }

                        }
                        break;
                    }
                case 2:
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Add("h2sw-request-info", "GET_ALL_SOFTWARE_DATA");
                            client.DefaultRequestHeaders.Add("GET_ALL_SOFTWARE_DATA__MAXIMUM_AMOUNT", "10");
                            client.DefaultRequestHeaders.Add("GET_ALL_SOFTWARE_DATA__START_INDEX", "0");
                            var response = await client.GetAsync("http://107.127.131.89:8080/requestinfo");

                            var responseContent = await response.Content.ReadAsStringAsync();
                            var w = new TextWindow(responseContent);
                            w.ShowDialog();
                        }
                        break;
                    }
                case 1:
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            var pluginKey = "progtroll";
                            var pluginVersion = "1.0.0";
                            client.DefaultRequestHeaders.Add("h2sw-download-plugin", "CHECK_DOWNLOADABLE_PLUGIN");
                            client.DefaultRequestHeaders.Add("CHECK_DOWNLOADABLE_PLUGIN__PLUGIN_KEY", pluginKey);
                            client.DefaultRequestHeaders.Add("CHECK_DOWNLOADABLE_PLUGIN__PLUGIN_VERSION", pluginVersion);
                            var response = await client.GetAsync("http://107.127.131.89:8080/downloadplugin");

                            var responseContent = await response.Content.ReadAsStringAsync();
                            var w = new TextWindow(responseContent);
                            w.ShowDialog();

                            var isDownloadable = false;
                            var fileName = "";
                            var executePath = "";
                            try
                            {
                                isDownloadable = response.Headers.GetValues("CHECK_DOWNLOADABLE_PLUGIN__IS_DOWNLOADABLE")
                                    .FirstOrDefault() == "1";
                                fileName = response.Headers.GetValues("CHECK_DOWNLOADABLE_PLUGIN__PLUGIN_FILE_NAME")
                                    .FirstOrDefault();
                                executePath = response.Headers.GetValues("CHECK_DOWNLOADABLE_PLUGIN__PLUGIN_EXECUTE_PATH")
                                    .FirstOrDefault();
                            }
                            catch
                            {
                                isDownloadable = false;
                            }

                            if (!isDownloadable)
                            {
                                return;
                            }
                            else
                            {
                                var downloadFileFolder = "temp\\" + "plugins"
                                        + "\\" + pluginKey + "\\" + pluginVersion;
                                var versionExecutePath = "temp\\" + "plugins"
                                        + "\\" + pluginKey + "\\" + pluginVersion + "\\" + executePath;
                                if (!Directory.Exists(downloadFileFolder))
                                {
                                    Directory.CreateDirectory(downloadFileFolder);
                                }
                                var downloadFilePath = downloadFileFolder + "\\" + fileName;
                                var requestHeaderContentMap = new Dictionary<string, string>();
                                requestHeaderContentMap.Add("h2sw-download-plugin", "DOWNLOAD_PLUGIN");
                                requestHeaderContentMap.Add("DOWNLOAD_PLUGIN__PLUGIN_KEY", pluginKey);
                                requestHeaderContentMap.Add("DOWNLOAD_PLUGIN__PLUGIN_VERSION", pluginVersion);
                                var param = new object[] { "http://107.127.131.89:8080/downloadplugin"
                                    , downloadFilePath
                                    , requestHeaderContentMap};
                                var downloadTask = new DownloadPluginTask(param);
                                downloadTask.ProgressChanged += (s, e2) =>
                                {
                                };
                                await downloadTask.Execute();

                                if (downloadTask.IsCompleted)
                                {

                                }
                            }

                        }
                        break;
                    }
                case 4:
                    {
                        try
                        {
                            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                            {
                                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                                if (result == System.Windows.Forms.DialogResult.OK
                                    && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                                {
                                    var path = await CyberDbManager.Current.Backup(dialog.SelectedPath);
                                    if (path.Count > 0)
                                        MessageBox.Show("Xuất file csv thành công!");
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Xuất file csv thất bại!\n" + ex.Message);
                        }

                        break;
                    }
                case 5:
                    {
                        try
                        {
                            var ofd = new OpenFileDialog();
                            ofd.Filter = "Csv files (*.csv)|*.csv";
                            ofd.Multiselect = true;
                            var result = ofd.ShowDialog();
                            await CyberDbManager.Current.ImportCSVToDb(ofd.FileNames);
                            if (ofd.FileNames.Length > 0)
                            {
                                MessageBox.Show("Import csv thành công!");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Import csv thất bại!\n" + ex.Message);
                        }

                        break;
                    }
            }

        }
    }

    internal abstract class BaseEMSParamAsyncTask : ParamAsyncTask
    {
        private Action<AsyncTaskResult> _completedCallback;
        private Func<object, bool> _baseRTTaskCanExecute;
        public BaseEMSParamAsyncTask(object param
            , string name
            , [Optional] Action<AsyncTaskResult> completedCallback
            , [Optional] CancellationTokenSource cancellationTokenSource
            , [Optional] Func<object, AsyncTaskResult, CancellationTokenSource, Task<AsyncTaskResult>> mainFunc
            , Func<object, bool> canExecute = null
            , Func<object, AsyncTaskResult, Task<AsyncTaskResult>> callback = null
            , ulong estimatedTime = 0
            , ulong delayTime = 0
            , int reportDelay = 1000)
            : base(mainFunc, cancellationTokenSource, param, canExecute, callback, name, estimatedTime, delayTime, reportDelay)
        {
            _mainFunc = _DoMainTask;
            _canExecute = _IsTaskPossile;
            _baseRTTaskCanExecute = canExecute;
            _callback = _DoCallback;
            _cancellationTokenSource = new CancellationTokenSource();
            _completedCallback = completedCallback;
        }

        private async Task<AsyncTaskResult> _DoCallback(object param, AsyncTaskResult result)
        {
            DoCallback(param, result);
            await DoAsyncCallback(param, result);
            _completedCallback?.Invoke(result);
            return result;
        }

        private async Task<AsyncTaskResult> _DoMainTask(object param, AsyncTaskResult result, CancellationTokenSource token)
        {
            DoMainTask(param, result, token);
            await DoAsyncMainTask(param, result, token);
            return result;
        }

        private bool _IsTaskPossile(object param)
        {
            return IsTaskPossible(param)
                && (_baseRTTaskCanExecute?.Invoke(param) ?? true);
        }

        protected virtual void DoCallback(object param, AsyncTaskResult result) { }
        protected virtual void DoMainTask(object param, AsyncTaskResult result, CancellationTokenSource token) { }

        protected virtual Task DoAsyncCallback(object param, AsyncTaskResult result)
        {
            var t = new Task(() => { });
            t.RunSynchronously();
            return t;
        }
        protected virtual Task DoAsyncMainTask(object param, AsyncTaskResult result, CancellationTokenSource token)
        {
            var t = new Task(() => { });
            t.RunSynchronously();
            return t;
        }

        protected abstract bool IsTaskPossible(object param);
    }

    internal class DownloadPluginTask : BaseEMSParamAsyncTask
    {
        private string _sourceToDownload;
        private string _filePathDestination;
        private Uri _sourceUri;
        private Dictionary<string, string> _requestHeaderContentMap;

        public DownloadPluginTask(object param
          , Action<AsyncTaskResult> callback = null
          , string name = "Downloading") : base(param, name, callback)
        {
            switch (param)
            {
                case object[] data:
                    if (data.Length == 3)
                    {
                        _sourceToDownload = data[0].ToString()
                            ?? throw new ArgumentNullException("Source to download not found in params!");
                        _filePathDestination = data[1].ToString()
                            ?? throw new ArgumentNullException("File path destination not found in params!");
                        _requestHeaderContentMap = data[2] as Dictionary<string, string>
                            ?? throw new ArgumentNullException("Request header content map not found in params!");
                    }
                    break;
                default:
                    throw new InvalidDataException("Param must be an array with 3 elements");
            }
            _isEnableReport = false;
        }

        protected override async Task DoAsyncMainTask(object param, AsyncTaskResult result, CancellationTokenSource token)
        {
            if (_sourceUri != null && _filePathDestination != null && _requestHeaderContentMap != null)
            {
                using (WebClient myWebClient = new WebClient())
                {
                    foreach (var kv in _requestHeaderContentMap)
                    {
                        myWebClient.Headers.Add(kv.Key, kv.Value);
                    }

                    myWebClient.DownloadProgressChanged += (s, e) =>
                    {
                        var rate = e.BytesReceived / e.TotalBytesToReceive * 100;
                        CurrentProgress = rate;
                    };
                    await myWebClient.DownloadFileTaskAsync(_sourceUri, _filePathDestination);
                }
            }

        }
        protected override bool IsTaskPossible(object param)
        {
            if (_sourceToDownload == null)
            {
                return false;
            }

            try
            {
                _sourceUri = new Uri(_sourceToDownload);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}
