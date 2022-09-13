using cyber_server.@base;
using cyber_server.implements.db_manager;
using cyber_server.implements.log_manager;
using cyber_server.implements.plugin_manager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.implements.http_server.handlers
{
    internal class RequestDownloadPluginHttpHandler : ICyberHttpRequestHandler
    {
        private const string REQUEST_CHECK_PLUGIN_DOWNLOADABLE_HEADER_ID = "CHECK_DOWNLOADABLE_PLUGIN";
        private const string REQUEST_KEY_TO_CHECK_DOWNLOADABLE_HEADER_ID = "CHECK_DOWNLOADABLE_PLUGIN__PLUGIN_KEY";
        private const string REQUEST_VERSION_TO_CHECK_DOWNLOADABLE_HEADER_ID = "CHECK_DOWNLOADABLE_PLUGIN__PLUGIN_VERSION";
        private const string RESPONSE_IS_PLUGIN_DOWNLOADABLE_HEADER_ID = "CHECK_DOWNLOADABLE_PLUGIN__IS_DOWNLOADABLE";
        private const string RESPONSE_PLUGIN_FILE_NAME_HEADER_ID = "CHECK_DOWNLOADABLE_PLUGIN__PLUGIN_FILE_NAME";
        private const string RESPONSE_PLUGIN_EXECUTE_PATH_HEADER_ID = "CHECK_DOWNLOADABLE_PLUGIN__PLUGIN_EXECUTE_PATH";
        private const string RESPONSE_PLUGIN_MAIN_CLASS_NAME_HEADER_ID = "CHECK_DOWNLOADABLE_PLUGIN__PLUGIN_MAIN_CLASS_NAME";

        private const string REQUEST_DOWNLOAD_PLUGIN_HEADER_ID = "DOWNLOAD_PLUGIN";
        private const string REQUEST_DOWNLOAD_PLUGIN_KEY_HEADER_ID = "DOWNLOAD_PLUGIN__PLUGIN_KEY";
        private const string REQUEST_DOWNLOAD_PLUGIN_VERSION_HEADER_ID = "DOWNLOAD_PLUGIN__PLUGIN_VERSION";

        public RequestDownloadPluginHttpHandler()
        {
        }

        public async Task<byte[]> Handle(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.Headers.AllKeys.Contains(CyberHttpServer.REQUEST_DOWNLOAD_PLUGIN_HEADER_KEY))
            {
                switch (request.Headers[CyberHttpServer.REQUEST_DOWNLOAD_PLUGIN_HEADER_KEY])
                {
                    case REQUEST_CHECK_PLUGIN_DOWNLOADABLE_HEADER_ID:
                        {
                            string responseString = "SUCCESS";
                            string requestPluginKey = "";
                            Version requestPluginVersion;
                            try
                            {

                                if (!string.IsNullOrEmpty(request.Headers[REQUEST_KEY_TO_CHECK_DOWNLOADABLE_HEADER_ID])
                                    && !string.IsNullOrEmpty(request.Headers[REQUEST_VERSION_TO_CHECK_DOWNLOADABLE_HEADER_ID]))
                                {
                                    requestPluginKey = request.Headers[REQUEST_KEY_TO_CHECK_DOWNLOADABLE_HEADER_ID];
                                    requestPluginVersion = Version.Parse(request.Headers[REQUEST_VERSION_TO_CHECK_DOWNLOADABLE_HEADER_ID]);

                                    ServerLogManager.Current.I("Request to check plugin downloadable: key=" + requestPluginKey + ", version=" + requestPluginVersion);

                                    PluginVersion query = null;
                                    await CyberDbManager.Current.RequestDbContextAsync((dbContext) =>
                                    {
                                        query = dbContext.Plugins
                                                .Where(p => p.StringId == requestPluginKey)
                                                .FirstOrDefault()?
                                                .PluginVersions
                                                .Where(v => Version.Parse(v.Version) == requestPluginVersion)
                                                .FirstOrDefault();

                                    });

                                    if (query != null)
                                    {
                                        response.StatusCode = (int)HttpStatusCode.OK;
                                        response.Headers.Add(RESPONSE_IS_PLUGIN_DOWNLOADABLE_HEADER_ID, "1");
                                        response.Headers.Add(RESPONSE_PLUGIN_FILE_NAME_HEADER_ID, query.FileName);
                                        response.Headers.Add(RESPONSE_PLUGIN_EXECUTE_PATH_HEADER_ID, query.ExecutePath);
                                        response.Headers.Add(RESPONSE_PLUGIN_MAIN_CLASS_NAME_HEADER_ID, query.MainClassName);
                                    }
                                    else
                                    {
                                        response.StatusCode = (int)HttpStatusCode.NotFound;
                                        responseString = "NOT FOUND";
                                    }
                                }
                                else
                                {
                                    response.StatusCode = (int)HttpStatusCode.NotFound;
                                    responseString = "NOT FOUND";
                                }
                            }
                            catch (Exception ex)
                            {
                                ServerLogManager.Current.E(ex.ToString());
                                response.StatusCode = (int)HttpStatusCode.NotFound;
                                responseString = "NOT FOUND";
                            }

                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                            response.ContentLength64 = buffer.Length;
                            return buffer;
                        }
                    case REQUEST_DOWNLOAD_PLUGIN_HEADER_ID:
                        {
                            string requestPluginKey = "";
                            Version requestPluginVersion;
                            if (!string.IsNullOrEmpty(request.Headers[REQUEST_DOWNLOAD_PLUGIN_KEY_HEADER_ID])
                                    && !string.IsNullOrEmpty(request.Headers[REQUEST_DOWNLOAD_PLUGIN_VERSION_HEADER_ID]))
                            {
                                requestPluginKey = request.Headers[REQUEST_DOWNLOAD_PLUGIN_KEY_HEADER_ID];
                                requestPluginVersion = Version.Parse(request.Headers[REQUEST_DOWNLOAD_PLUGIN_VERSION_HEADER_ID]);

                                ServerLogManager.Current.I("Request to download plugin: key=" + requestPluginKey + ", version=" + requestPluginVersion);

                                PluginVersion query = null;
                                await CyberDbManager.Current.RequestDbContextAsync((dbContext) =>
                                {
                                    query = dbContext.Plugins
                                            .Where(p => p.StringId == requestPluginKey)
                                            .FirstOrDefault()?
                                            .PluginVersions
                                            .Where(v => Version.Parse(v.Version) == requestPluginVersion)
                                            .FirstOrDefault();

                                });

                                if (query != null)
                                {
                                    var zipFilePath = CyberPluginManager.Current.GetSetupZipFilePathByPluginVersion(
                                            requestPluginKey,
                                            requestPluginVersion.ToString(),
                                            query.FileName);
                                    response.StatusCode = (int)HttpStatusCode.OK;

                                    if (File.Exists(zipFilePath))
                                    {
                                        byte[] buffer;
                                        using (FileStream stream = File.Open(zipFilePath, FileMode.Open))
                                        {
                                            buffer = new byte[stream.Length];
                                            await stream.ReadAsync(buffer, 0, (int)stream.Length);
                                        }
                                        response.ContentLength64 = buffer.Length;

                                        await CyberDbManager.Current.RequestDbContextAsync((dbContext) =>
                                        {
                                            var plugin = dbContext.Plugins
                                                .Where(p => p.StringId == requestPluginKey)
                                                .FirstOrDefault();
                                            plugin.Downloads++;
                                            dbContext.SaveChanges();
                                        });
                                        return buffer;
                                    }
                                    else
                                    {
                                        response.StatusCode = (int)HttpStatusCode.NotFound;
                                    }
                                }
                                else
                                {
                                    response.StatusCode = (int)HttpStatusCode.NotFound;
                                }
                            }
                            break;
                        }
                }
            }

            return null;
        }
    }
}
