using cyber_server.@base;
using cyber_server.implements.db_manager;
using cyber_server.implements.log_manager;
using cyber_server.implements.plugin_manager;
using cyber_server.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.implements.http_server.handlers
{
    internal class RequestDownloadToolHttpHandler : ICyberHttpRequestHandler
    {
        public const string REQUEST_CHECK_TOOL_DOWNLOADABLE_HEADER_ID = "CHECK_DOWNLOADABLE_TOOL";
        public const string REQUEST_KEY_TO_CHECK_DOWNLOADABLE_HEADER_ID = "CHECK_DOWNLOADABLE_TOOL__TOOL_KEY";
        public const string REQUEST_VERSION_TO_CHECK_DOWNLOADABLE_HEADER_ID = "CHECK_DOWNLOADABLE_TOOL__TOOL_VERSION";
        public const string RESPONSE_IS_TOOL_DOWNLOADABLE_HEADER_ID = "CHECK_DOWNLOADABLE_TOOL__IS_DOWNLOADABLE";
        public const string RESPONSE_TOOL_FILE_NAME_HEADER_ID = "CHECK_DOWNLOADABLE_TOOL__TOOL_FILE_NAME";
        public const string RESPONSE_TOOL_EXECUTE_PATH_HEADER_ID = "CHECK_DOWNLOADABLE_TOOL__TOOL_EXECUTE_PATH";

        public const string REQUEST_DOWNLOAD_TOOL_HEADER_ID = "DOWNLOAD_TOOL";
        public const string REQUEST_DOWNLOAD_TOOL_KEY_HEADER_ID = "DOWNLOAD_TOOL__TOOL_KEY";
        public const string REQUEST_DOWNLOAD_TOOL_VERSION_HEADER_ID = "DOWNLOAD_TOOL__TOOL_VERSION";

        public RequestDownloadToolHttpHandler()
        {
        }

        public async Task<byte[]> Handle(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.Headers.AllKeys.Contains(CyberHttpServer.REQUEST_DOWNLOAD_TOOL_HEADER_KEY))
            {
                switch (request.Headers[CyberHttpServer.REQUEST_DOWNLOAD_TOOL_HEADER_KEY])
                {
                    case REQUEST_CHECK_TOOL_DOWNLOADABLE_HEADER_ID:
                        {
                            string responseString = "SUCCESS";
                            string requestToolKey = "";
                            Version requestToolVersion;
                            try
                            {

                                if (!string.IsNullOrEmpty(request.Headers[REQUEST_KEY_TO_CHECK_DOWNLOADABLE_HEADER_ID])
                                    && !string.IsNullOrEmpty(request.Headers[REQUEST_VERSION_TO_CHECK_DOWNLOADABLE_HEADER_ID]))
                                {
                                    requestToolKey = request.Headers[REQUEST_KEY_TO_CHECK_DOWNLOADABLE_HEADER_ID];
                                    requestToolVersion = Version.Parse(request.Headers[REQUEST_VERSION_TO_CHECK_DOWNLOADABLE_HEADER_ID]);

                                    ServerLogManager.Current.I("Request to check TOOL downloadable: key=" + requestToolKey + ", version=" + requestToolVersion);

                                    ToolVersion query = null;
                                    await CyberDbManager.Current.RequestDbContextAsync((dbContext) =>
                                    {
                                        query = dbContext.Tools
                                                .Where(t => t.StringId == requestToolKey)
                                                .FirstOrDefault()?
                                                .ToolVersions
                                                .Where(v => Version.Parse(v.Version) == requestToolVersion)
                                                .FirstOrDefault();

                                    });

                                    if (query != null)
                                    {
                                        var setting = new JsonSerializerSettings
                                        {
                                            ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                                        };
                                        responseString = JsonConvert.SerializeObject(query, Formatting.Indented, setting);
                                        response.StatusCode = (int)HttpStatusCode.OK;
                                        response.Headers.Add(RESPONSE_IS_TOOL_DOWNLOADABLE_HEADER_ID, "1");
                                        response.Headers.Add(RESPONSE_TOOL_FILE_NAME_HEADER_ID, query.FileName);
                                        response.Headers.Add(RESPONSE_TOOL_EXECUTE_PATH_HEADER_ID, query.ExecutePath);
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

                            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                            response.ContentLength64 = buffer.Length;
                            return buffer;
                        }
                    case REQUEST_DOWNLOAD_TOOL_HEADER_ID:
                        {
                            string requestToolKey = "";
                            Version requestToolVersion;
                            if (!string.IsNullOrEmpty(request.Headers[REQUEST_DOWNLOAD_TOOL_KEY_HEADER_ID])
                                    && !string.IsNullOrEmpty(request.Headers[REQUEST_DOWNLOAD_TOOL_VERSION_HEADER_ID]))
                            {
                                requestToolKey = request.Headers[REQUEST_DOWNLOAD_TOOL_KEY_HEADER_ID];
                                requestToolVersion = Version.Parse(request.Headers[REQUEST_DOWNLOAD_TOOL_VERSION_HEADER_ID]);

                                ServerLogManager.Current.I("Request to download TOOL: key=" + requestToolKey + ", version=" + requestToolVersion);

                                ToolVersion query = null;
                                await CyberDbManager.Current.RequestDbContextAsync((dbContext) =>
                                {
                                    query = dbContext.Tools
                                            .Where(p => p.StringId == requestToolKey)
                                            .FirstOrDefault()?
                                            .ToolVersions
                                            .Where(v => Version.Parse(v.Version) == requestToolVersion)
                                            .FirstOrDefault();
                                });

                                if (query != null)
                                {
                                    response.StatusCode = (int)HttpStatusCode.OK;

                                    if (query.File != null && query.File.Length > 0)
                                    {
                                        byte[] buffer = query.File;
                                        response.ContentLength64 = buffer.Length;

                                        await CyberDbManager.Current.RequestDbContextAsync((dbContext) =>
                                        {
                                            var tool = dbContext.Tools
                                                .Where(t => t.StringId == requestToolKey)
                                                .FirstOrDefault();
                                            tool.Downloads++;
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
