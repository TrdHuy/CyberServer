﻿using cyber_server.@base;
using cyber_server.definition;
using cyber_server.implements.db_manager;
using cyber_server.utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.implements.http_server.handlers
{
    internal class RequestInfoHttpHandler : ICyberHttpRequestHandler
    {
        public const string REQUEST_ALL_PLUGIN_INFO_HEADER_ID = "GET_ALL_PLUGIN_DATA";
        public const string REQUEST_ALL_PLUGIN_INFO_MAXIMUM_AMOUNT_HEADER_ID = "GET_ALL_PLUGIN_DATA__MAXIMUM_AMOUNT";
        public const string REQUEST_ALL_PLUGIN_INFO_START_INDEX_HEADER_ID = "GET_ALL_PLUGIN_DATA__START_INDEX";
        public const string RESPONSE_GET_ALL_PLUGIN_INFO_END_OF_DBSET_HEADER_ID = "GET_ALL_PLUGIN_DATA__IS_END_OF_DBSET";

        public const string REQUEST_ALL_SOFTWARE_INFO_HEADER_ID = "GET_ALL_SOFTWARE_DATA";
        public const string REQUEST_SOFTWARE_INFO_MAXIMUM_AMOUNT_HEADER_ID = "GET_ALL_SOFTWARE_DATA__MAXIMUM_AMOUNT";
        public const string REQUEST_SOFTWARE_INFO_START_INDEX_HEADER_ID = "GET_ALL_SOFTWARE_DATA__START_INDEX";
        public const string RESPONSE_SOFTWARE_INFO_END_OF_DBSET_HEADER_ID = "GET_ALL_SOFTWARE_DATA__IS_END_OF_DBSET";

        public const string REQUEST_SOFTWARE_INFO_HEADER_ID = "GET_SOFTWARE_DATA";
        public const string REQUEST_SOFTWARE_KEY_HEADER_ID = "GET_SOFTWARE_DATA__SOFTWARE_KEY";

        public const string REQUEST_CYBER_SW_PACKAGE_BUILD_PARAM_HEADER_ID = "GET_CYBER_SW_PACKAGE_BUILD_PARAM";

        public RequestInfoHttpHandler()
        {
        }

        public async Task<byte[]> Handle(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.Headers.AllKeys.Contains(CyberHttpServer.REQUEST_INFO_HEADER_KEY))
            {
                switch (request.Headers[CyberHttpServer.REQUEST_INFO_HEADER_KEY])
                {
                    case REQUEST_ALL_PLUGIN_INFO_HEADER_ID:
                        {
                            response.Headers.Add("Content-Type", "application/json");
                            response.StatusCode = (int)HttpStatusCode.OK;
                            string jsonstring = "";
                            var maximumElement = -1;
                            var currentStartIndex = 0;
                            var isEndOfDbset = 0;

                            if (!string.IsNullOrEmpty(request.Headers[REQUEST_ALL_PLUGIN_INFO_MAXIMUM_AMOUNT_HEADER_ID])
                                && !string.IsNullOrEmpty(request.Headers[REQUEST_ALL_PLUGIN_INFO_START_INDEX_HEADER_ID]))
                            {
                                maximumElement = Convert.ToInt32(request.Headers[REQUEST_ALL_PLUGIN_INFO_MAXIMUM_AMOUNT_HEADER_ID]);
                                currentStartIndex = Convert.ToInt32(request.Headers[REQUEST_ALL_PLUGIN_INFO_START_INDEX_HEADER_ID]);
                            }

                            await CyberDbManager.Current.RequestDbContextAsync((dbContext) =>
                            {
                                var setting = new JsonSerializerSettings
                                {
                                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                                };

                                List<Plugin> queryResult = null;
                                if (maximumElement == -1)
                                {
                                    queryResult = dbContext
                                        .Plugins
                                        .JsonClone();
                                    isEndOfDbset = 1;
                                }
                                else
                                {
                                    queryResult = dbContext.Plugins
                                            .OrderBy(p => p.PluginId)
                                            .Skip(currentStartIndex)
                                            .Take(maximumElement)
                                            .JsonClone();
                                    if (queryResult.Count() < maximumElement)
                                    {
                                        isEndOfDbset = 1;
                                    }
                                }
                                jsonstring = JsonConvert.SerializeObject(queryResult, Formatting.Indented, setting);

                            });
                            // Gửi thông tin về cho Client
                            byte[] buf = System.Text.Encoding.UTF8.GetBytes(jsonstring);
                            response.ContentLength64 = buf.Length;
                            response.Headers.Add(RESPONSE_GET_ALL_PLUGIN_INFO_END_OF_DBSET_HEADER_ID, isEndOfDbset + "");
                            return buf;
                        }
                    case REQUEST_ALL_SOFTWARE_INFO_HEADER_ID:
                        {
                            response.Headers.Add("Content-Type", "application/json");
                            response.StatusCode = (int)HttpStatusCode.OK;
                            string jsonstring = "";
                            var maximumElement = -1;
                            var currentStartIndex = 0;
                            var isEndOfDbset = 0;

                            if (!string.IsNullOrEmpty(request.Headers[REQUEST_SOFTWARE_INFO_MAXIMUM_AMOUNT_HEADER_ID])
                                && !string.IsNullOrEmpty(request.Headers[REQUEST_SOFTWARE_INFO_START_INDEX_HEADER_ID]))
                            {
                                maximumElement = Convert.ToInt32(request.Headers[REQUEST_SOFTWARE_INFO_MAXIMUM_AMOUNT_HEADER_ID]);
                                currentStartIndex = Convert.ToInt32(request.Headers[REQUEST_SOFTWARE_INFO_START_INDEX_HEADER_ID]);
                            }
                            List<Tool> queryResult = null;

                            await CyberDbManager.Current.RequestDbContextAsync((dbContext) =>
                            {
                                if (maximumElement == -1)
                                {
                                    queryResult = dbContext
                                            .Tools
                                            .Where(t => t.IsShowOnCyberInstaller)
                                            .JsonClone();
                                    isEndOfDbset = 1;
                                }
                                else
                                {
                                    var tempQuery = dbContext.Tools
                                            .Where(t => t.IsShowOnCyberInstaller)
                                            .OrderBy(p => p.ToolId)
                                            .Skip(currentStartIndex)
                                            .Take(maximumElement)
                                            .JsonClone();
                                    if (tempQuery.Count() < maximumElement)
                                    {
                                        isEndOfDbset = 1;
                                    }
                                    queryResult = tempQuery;
                                }

                                if (queryResult != null)
                                {
                                    foreach (var tool in queryResult)
                                    {
                                        var selectedVersionSource = tool
                                            .ToolVersions
                                            .Where(tv => tv.IsDisable == false)
                                            .ToList();
                                        tool.ToolVersions = selectedVersionSource;
                                    }
                                }
                            });
                            var setting = new JsonSerializerSettings
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                            };
                            jsonstring = JsonConvert.SerializeObject(queryResult, Formatting.Indented, setting);

                            // Gửi thông tin về cho Client
                            byte[] buf = System.Text.Encoding.UTF8.GetBytes(jsonstring);
                            response.ContentLength64 = buf.Length;
                            response.Headers.Add(RESPONSE_SOFTWARE_INFO_END_OF_DBSET_HEADER_ID, isEndOfDbset + "");
                            return buf;
                        }
                    case REQUEST_SOFTWARE_INFO_HEADER_ID:
                        {
                            response.Headers.Add("Content-Type", "application/json");
                            response.StatusCode = (int)HttpStatusCode.OK;
                            string jsonstring = "";
                            var swKey = "";

                            if (!string.IsNullOrEmpty(request.Headers[REQUEST_SOFTWARE_KEY_HEADER_ID]))
                            {
                                swKey = request.Headers[REQUEST_SOFTWARE_KEY_HEADER_ID].ToString();
                            }

                            object queryResult = null;

                            await CyberDbManager.Current.RequestDbContextAsync((dbContext) =>
                            {
                                var queryTool = dbContext
                                        .Tools
                                        .Where(t => t.StringId.Equals(swKey))
                                        .FirstOrDefault()
                                        .JsonClone() as Tool;
                                var enabledVersions = queryTool.ToolVersions.Where(v => v.IsDisable == false).ToList();
                                queryTool.ToolVersions = enabledVersions;
                                queryResult = queryTool;
                            });
                            var setting = new JsonSerializerSettings
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                            };
                            jsonstring = JsonConvert.SerializeObject(queryResult, Formatting.Indented, setting);

                            // Gửi thông tin về cho Client
                            byte[] buf = System.Text.Encoding.UTF8.GetBytes(jsonstring);
                            response.ContentLength64 = buf.Length;
                            return buf;
                        }
                    case REQUEST_CYBER_SW_PACKAGE_BUILD_PARAM_HEADER_ID:
                        {
                            response.Headers.Add("Content-Type", "application/json");
                            response.StatusCode = (int)HttpStatusCode.OK;
                            string jsonstring = "";
                            object result = new
                            {
                                InfoFileName = CyberServerDefinition.NEW_BUILD_CONCEPT_SOFTWARE_VERSION_BUILD_INFO_FILE_NAME,
                                MainBuildFileName = CyberServerDefinition.NEW_BUILD_CONCEPT_SOFTWARE_VERSION_BUILD_FILE_NAME,
                            };


                            jsonstring = JsonConvert.SerializeObject(result, Formatting.Indented);
                            byte[] buf = Encoding.UTF8.GetBytes(jsonstring);
                            response.ContentLength64 = buf.Length;
                            return buf;
                        }
                }
            }

            return null;
        }
    }
}
