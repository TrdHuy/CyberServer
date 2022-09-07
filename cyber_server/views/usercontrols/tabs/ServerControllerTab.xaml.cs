using cyber_base.implement.async_task;
using cyber_server.implements.http_server;
using cyber_server.views.windows.others;
using cyber_server_base.async_task.core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
