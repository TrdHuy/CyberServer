using cyber_server.@base;
using cyber_server.implements.db_manager;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.implements.http_server.handlers
{
    internal class RequestPluginInfoHttpHandler : ICyberHttpRequestHandler
    {
        public RequestPluginInfoHttpHandler()
        {
        }

        public async Task<byte[]> Handle(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.Headers.AllKeys.Contains(CyberHttpServer.REQUEST_INFO_HEADER_KEY))
            {
                switch (request.Headers[CyberHttpServer.REQUEST_INFO_HEADER_KEY])
                {
                    case CyberHttpServer.REQUEST_PLUGIN_INFO_HEADER_ID:
                        {
                            response.Headers.Add("Content-Type", "application/json");
                            response.StatusCode = (int)HttpStatusCode.OK;
                            string jsonstring = "";
                            var maximumElement = -1;
                            var currentStartIndex = 0;
                            var isEndOfDbset = 0;

                            if (!string.IsNullOrEmpty(request.Headers[CyberHttpServer.REQUEST_PLUGIN_INFO_MAXIMUM_AMOUNT_HEADER_ID])
                                && !string.IsNullOrEmpty(request.Headers[CyberHttpServer.REQUEST_PLUGIN_INFO_START_INDEX_HEADER_ID]))
                            {
                                maximumElement = Convert.ToInt32(request.Headers[CyberHttpServer.REQUEST_PLUGIN_INFO_MAXIMUM_AMOUNT_HEADER_ID]);
                                currentStartIndex = Convert.ToInt32(request.Headers[CyberHttpServer.REQUEST_PLUGIN_INFO_START_INDEX_HEADER_ID]);
                            }

                            await CyberDbManager.Current.RequestDbContextAsync((dbContext) =>
                            {
                                var setting = new JsonSerializerSettings
                                {
                                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                                };

                                if (maximumElement == -1)
                                {
                                    jsonstring = JsonConvert.SerializeObject(dbContext.Plugins, Formatting.Indented, setting);
                                    isEndOfDbset = 1;
                                }
                                else
                                {
                                    var query = dbContext.Plugins
                                            .OrderBy(p => p.PluginId)
                                            .Skip(currentStartIndex)
                                            .Take(maximumElement);
                                    if (query.Count() < maximumElement)
                                    {
                                        isEndOfDbset = 1;
                                    }
                                    jsonstring = JsonConvert.SerializeObject(query, Formatting.Indented, setting);
                                }

                            });
                            // Gửi thông tin về cho Client
                            byte[] buf = System.Text.Encoding.UTF8.GetBytes(jsonstring);
                            response.ContentLength64 = buf.Length;
                            response.Headers.Add(CyberHttpServer.RESPONSE_PLUGIN_INFO_END_OF_DBSET_HEADER_ID, isEndOfDbset + "");
                            return buf;
                        }
                }
            }

            return null;
        }
    }
}
