﻿using cyber_server.@base;
using cyber_server.definition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.implements.http_server.handlers
{
    internal enum ResourceMode
    {
        Tool = 1,
        Plugin = 2,
    }
    internal class RequestResourceHttpHandler : ICyberHttpRequestHandler
    {
        private string _pluginResourceApiPathSub = CyberHttpServer.REQUEST_PLUGIN_RESOURCE_PATH.Trim('/');
        private string _toolnResourceApiPathSub = CyberHttpServer.REQUEST_TOOL_RESOURCE_PATH.Trim('/');
        private string _key = "";
        private string _fileName = "";
        private string _filePath = "";

        public RequestResourceHttpHandler(string uriRequestPath, ResourceMode resMode = ResourceMode.Plugin)
        {
            var split = uriRequestPath.Split('/');
            int starDuet = 0;
            foreach (var pram in split)
            {
                if (starDuet == 2)
                {
                    _fileName = pram;
                    break;
                }

                if (starDuet == 1)
                {
                    _key = pram;
                    starDuet = 2;
                }

                if (pram == _pluginResourceApiPathSub || pram == _toolnResourceApiPathSub)
                {
                    starDuet = 1;
                }
            }

            if (_key == "" || _fileName == "")
            {
                throw new ArgumentException("Invaild request plugin resource!~");
            }
            else
            {
                if (resMode == ResourceMode.Plugin)
                {
                    _filePath = CyberServerDefinition.PLUGIN_BASE_FOLDER_PATH + "\\" + _key + "\\resources\\" + _fileName;
                }
                else if (resMode == ResourceMode.Tool)
                {
                    _filePath = CyberServerDefinition.TOOL_BASE_FOLDER_PATH + "\\" + _key + "\\resources\\" + _fileName;
                }
                if (!File.Exists(_filePath))
                {
                    throw new ArgumentException("Invaild request plugin resource!~");
                }
            }
        }

        public async Task<byte[]> Handle(HttpListenerRequest request, HttpListenerResponse response)
        {
            response.Headers.Add("Content-Type", "image/png");

            byte[] buffer;
            using (FileStream stream = File.Open(_filePath, FileMode.Open))
            {
                buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, (int)stream.Length);
            }
            response.ContentLength64 = buffer.Length;
            return buffer;
        }
    }
}