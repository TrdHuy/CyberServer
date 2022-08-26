﻿using cyber_server.@base;
using cyber_server.implements.db_manager;
using cyber_server.implements.log_manager;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.implements.http_server
{
    internal class CyberHttpServer : IServerModule
    {
        private const string END_POINT = "http://107.127.131.89:8080/";
        private const string REQUEST_INFO_API_PATH = "requestinfo";
        private const string REQUEST_INFO_HEADER_KEY = "h2sw-request-info";
        private const string REQUEST_PLUGIN_INFO_HEADER_ID = "GET_ALL_PLUGIN_DATA";
        private const string REQUEST_PLUGIN_INFO_MAXIMUM_AMOUNT_HEADER_ID = "GET_ALL_PLUGIN_DATA__MAXIMUM_AMOUNT";
        private const string REQUEST_PLUGIN_INFO_START_INDEX_HEADER_ID = "GET_ALL_PLUGIN_DATA__START_INDEX";
        private const string RESPONSE_PLUGIN_INFO_END_OF_DBSET_HEADER_ID = "GET_ALL_PLUGIN_DATA__IS_END_OF_DBSET";

        private HttpListener listener;

        public static CyberHttpServer Current
        {
            get
            {
                return ServerModuleManager.CHS_Instance;
            }
        }

        private CyberHttpServer()
        {

        }

        public void OnModuleInit()
        {
            if (!HttpListener.IsSupported)
                throw new Exception("Máy không hỗ trợ HttpListener.");

            // Khởi tạo HttpListener
            listener = new HttpListener();
            listener.Prefixes.Add(END_POINT);
        }

        public void Dispose()
        {
            listener.Stop();
        }

        public async Task StartAsync()
        {
            if (listener.IsListening)
            {
                return;
            }

            // Bắt đầu lắng nghe kết nối HTTP
            ServerLogManager.Current.AppendLogLine($"{DateTime.Now.ToLongTimeString()} : Start server successfully, waiting a client connect");
            listener.Start();
            do
            {

                try
                {
                    // Một client kết nối đến
                    HttpListenerContext context = await listener.GetContextAsync();
                    await ProcessRequest(context);

                }
                catch (Exception ex)
                {
                    ServerLogManager.Current.AppendLogLine(ex.Message);
                }

            }
            while (listener.IsListening);
        }

        public void Stop()
        {
            listener.Stop();
            ServerLogManager.Current.AppendLogLine("Server was stopped!~");
        }

        // Xử lý trả về nội dung tùy thuộc vào URL truy cập
        //      /               hiện thị dòng Hello World
        //      /stop           dừng máy chủ
        //      /json           trả về một nội dung json
        //      /anh2.png       trả về một file ảnh 
        //      /requestinfo    thông tin truy vấn

        async Task ProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            ServerLogManager.Current.AppendLogLine($"{request.HttpMethod} {request.RawUrl} {request.Url.AbsolutePath} {request.RemoteEndPoint}");

            var outputstream = response.OutputStream;

            var requestPaths = request.Url.AbsolutePath.Split('/');

            switch (request.Url.AbsolutePath)
            {
                case "/requestinfo":
                    {
                        if (request.Headers.AllKeys.Contains(REQUEST_INFO_HEADER_KEY))
                        {
                            switch (request.Headers[REQUEST_INFO_HEADER_KEY])
                            {
                                case REQUEST_PLUGIN_INFO_HEADER_ID:
                                    response.Headers.Add("Content-Type", "application/json");
                                    string jsonstring = "";
                                    var maximumElement = -1;
                                    var currentStartIndex = 0;
                                    var isEndOfDbset = 0;

                                    if (!string.IsNullOrEmpty(request.Headers[REQUEST_PLUGIN_INFO_MAXIMUM_AMOUNT_HEADER_ID])
                                        && !string.IsNullOrEmpty(request.Headers[REQUEST_PLUGIN_INFO_START_INDEX_HEADER_ID]))
                                    {
                                        maximumElement = Convert.ToInt32(request.Headers[REQUEST_PLUGIN_INFO_MAXIMUM_AMOUNT_HEADER_ID]);
                                        currentStartIndex = Convert.ToInt32(request.Headers[REQUEST_PLUGIN_INFO_START_INDEX_HEADER_ID]);
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
                                    byte[] buf = System.Text.Encoding.UTF8.GetBytes(jsonstring);
                                    response.ContentLength64 = buf.Length;
                                    response.Headers.Add(RESPONSE_PLUGIN_INFO_END_OF_DBSET_HEADER_ID, isEndOfDbset + "");
                                    await outputstream.WriteAsync(buf, 0, buf.Length);
                                    break;
                            }
                            break;
                        }
                        // Gửi thông tin về cho Client
                        context.Response.Headers.Add("content-type", "text/html");
                        context.Response.StatusCode = (int)HttpStatusCode.OK;

                        string responseString = this.GenerateHTML(request);
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                        response.ContentLength64 = buffer.Length;
                        await outputstream.WriteAsync(buffer, 0, buffer.Length);
                    }
                    break;
                case "/":
                    {
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes("Hello world!");
                        response.ContentLength64 = buffer.Length;
                        await outputstream.WriteAsync(buffer, 0, buffer.Length);
                    }
                    break;

                case "/stop":
                    {
                        listener.Stop();
                        Console.WriteLine("stop http");
                    }
                    break;

                case "/json":
                    {
                        response.Headers.Add("Content-Type", "application/json");
                        var product = new
                        {
                            Name = "Macbook Pro",
                            Price = 2000,
                            Manufacturer = "Apple"
                        };
                        string jsonstring = JsonConvert.SerializeObject(product);
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(jsonstring);
                        response.ContentLength64 = buffer.Length;
                        await outputstream.WriteAsync(buffer, 0, buffer.Length);

                    }
                    break;
                case "/anh2.png":
                    {
                        response.Headers.Add("Content-Type", "image/png");

                        byte[] buffer;
                        using (FileStream stream = File.Open("anh2.png", FileMode.Open))
                        {
                            buffer = new byte[stream.Length];
                            await stream.ReadAsync(buffer, 0, (int)stream.Length);
                        }
                        response.ContentLength64 = buffer.Length;
                        await outputstream.WriteAsync(buffer, 0, buffer.Length);

                    }
                    break;

                default:
                    {
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes("NOT FOUND!");
                        response.ContentLength64 = buffer.Length;
                        await outputstream.WriteAsync(buffer, 0, buffer.Length);
                    }
                    break;
            }

            // switch (request.Url.AbsolutePath)


            // Đóng stream để hoàn thành gửi về client
            outputstream.Close();
        }

        private void ExtractRequest(HttpListenerRequest request)
        {
            var contentType = request.ContentType;

            var reqMes = new HttpRequestMessage()
            {

            };

            //if (contentType?.Contains("multipart/form-data") ?? false)
            {
                using (System.IO.Stream body = request.InputStream) // here we have data
                {
                    using (var reader = new System.IO.StreamReader(body, request.ContentEncoding))
                    {
                        var data = reader.ReadToEnd();

                    }
                }
            }
        }

        // Tạo nội dung HTML trả về cho Client (HTML chứa thông tin về Request)
        public string GenerateHTML(HttpListenerRequest request)
        {
            string format = @"<!DOCTYPE html>
                            <html lang=""en""> 
                                <head>
                                    <meta charset=""UTF-8"">
                                    {0}
                                 </head> 
                                <body>
                                    {1}
                                </body> 
                            </html>";
            string head = "<title>Test WebServer</title>";
            var body = new StringBuilder();
            body.Append("<h1>Request Info</h1>");
            body.Append("<h2>Request Header:</h2>");

            // Header infomation
            var headers = from key in request.Headers.AllKeys
                          select $"<div>{key} : {string.Join(",", request.Headers.GetValues(key))}</div>";
            body.Append(string.Join("", headers));

            //Extract request properties
            body.Append("<h2>Request properties:</h2>");
            var properties = request.GetType().GetProperties();
            foreach (var property in properties)
            {
                var name_pro = property.Name;
                string value_pro;
                try
                {
                    value_pro = property.GetValue(request).ToString();
                }
                catch (Exception e)
                {
                    value_pro = e.Message;
                }
                body.Append($"<div>{name_pro} : {value_pro}</div>");

            };
            string html = string.Format(format, head, body.ToString());
            return html;
        }
    }
}