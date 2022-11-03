using cyber_server.@base;
using cyber_server.implements.db_manager;
using cyber_server.implements.http_server.handlers;
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
        public const string END_POINT1 = "http://107.127.131.89:8080/";
        public const string END_POINT2 = "https://107.127.131.89:8088/";
        public const string REQUEST_PLUGIN_RESOURCE_PATH = "/pluginresource";
        public const string REQUEST_TOOL_RESOURCE_PATH = "/toolresource";
        public const string DOWNLOAD_PLUGIN_API_PATH = "/downloadplugin";
        public const string DOWNLOAD_TOOL_API_PATH = "/downloadtool";
        public const string DOWNLOAD_CERTIFICATE_API_PATH = "/certificate";
        public const string REQUEST_INFO_API_PATH = "/requestinfo";
        public const string REQUEST_INFO_HEADER_KEY = "h2sw-request-info";
        public const string REQUEST_DOWNLOAD_PLUGIN_HEADER_KEY = "h2sw-download-plugin";
        public const string REQUEST_DOWNLOAD_TOOL_HEADER_KEY = "h2sw-download-tool";
        public const string REQUEST_DOWNLOAD_CERTIFICATE_HEADER_KEY = "h2sw-download-certificate";

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
            listener.Prefixes.Add(END_POINT1);
            listener.Prefixes.Add(END_POINT2);
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
            ServerLogManager.Current.AppendConsoleLogLine($"{DateTime.Now.ToLongTimeString()} : Start server successfully, waiting a client connect");
            listener.Start();
            do
            {

                try
                {
                    // Một client kết nối đến
                    HttpListenerContext context = await listener.GetContextAsync();
                    ProcessRequest(context);
                }
                catch (Exception ex)
                {
                    ServerLogManager.Current.AppendConsoleLogLine(ex.Message);
                    ServerLogManager.Current.E(ex.ToString());
                }

            }
            while (listener.IsListening);
        }

        public void Stop()
        {
            listener.Stop();
            ServerLogManager.Current.AppendConsoleLogLine("Server was stopped!~");
        }

        // Xử lý trả về nội dung tùy thuộc vào URL truy cập
        //      /               hiện thị dòng Hello World
        //      /stop           dừng máy chủ
        //      /json           trả về một nội dung json
        //      /anh2.png       trả về một file ảnh 
        //      /requestinfo    thông tin truy vấn
        private async void ProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            var outputstream = response.OutputStream;

            try
            {
               
                ServerLogManager.Current.AppendConsoleLogLine($"{request.HttpMethod} {request.RawUrl} {request.Url.AbsolutePath} {request.RemoteEndPoint}");


                if (request.Url.AbsolutePath.Contains(REQUEST_PLUGIN_RESOURCE_PATH))
                {
                    byte[] buffer = await new RequestResourceHttpHandler(request.Url.AbsolutePath, resMode: ResourceMode.Plugin)
                        .Handle(request, response);
                    await outputstream.WriteAsync(buffer, 0, buffer.Length);
                }
                else if (request.Url.AbsolutePath.Contains(REQUEST_TOOL_RESOURCE_PATH))
                {
                    byte[] buffer = await new RequestResourceHttpHandler(request.Url.AbsolutePath, resMode: ResourceMode.Tool)
                        .Handle(request, response);
                    await outputstream.WriteAsync(buffer, 0, buffer.Length);
                }
                else
                {
                    switch (request.Url.AbsolutePath)
                    {
                        case REQUEST_INFO_API_PATH:
                            {
                                byte[] buffer = await new RequestInfoHttpHandler().Handle(request, response);
                                await outputstream.WriteAsync(buffer, 0, buffer.Length);
                                break;
                            }
                        case DOWNLOAD_PLUGIN_API_PATH:
                            {
                                byte[] buffer = await new RequestDownloadPluginHttpHandler().Handle(request, response);
                                await outputstream.WriteAsync(buffer, 0, buffer.Length);
                                break;
                            }
                        case DOWNLOAD_TOOL_API_PATH:
                            {
                                byte[] buffer = await new RequestDownloadToolHttpHandler().Handle(request, response);
                                await outputstream.WriteAsync(buffer, 0, buffer.Length);
                                break;
                            }
                        case DOWNLOAD_CERTIFICATE_API_PATH:
                            {
                                byte[] buffer = await new RequestDownloadCertificateHttpHandler().Handle(request, response);
                                await outputstream.WriteAsync(buffer, 0, buffer.Length);
                                break;
                            }
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
                                var requestPathArr = request.Url.AbsolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                                if (requestPathArr.Length > 0)
                                {
                                    switch (requestPathArr[0])
                                    {
                                        case "htdocs":
                                            {
                                                var filePath = String.Join("/", requestPathArr);
                                                if (File.Exists(filePath))
                                                {
                                                    var jsCode = File.ReadAllText(filePath);
                                                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(jsCode);
                                                    response.ContentLength64 = buffer.Length;
                                                    await outputstream.WriteAsync(buffer, 0, buffer.Length);
                                                }
                                                else
                                                {
                                                    response.StatusCode = (int)HttpStatusCode.NotFound;
                                                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes("NOT FOUND!");
                                                    response.ContentLength64 = buffer.Length;
                                                    await outputstream.WriteAsync(buffer, 0, buffer.Length);
                                                }
                                                break;
                                            }
                                    }
                                }
                                else
                                {
                                    response.StatusCode = (int)HttpStatusCode.NotFound;
                                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes("NOT FOUND!");
                                    response.ContentLength64 = buffer.Length;
                                    await outputstream.WriteAsync(buffer, 0, buffer.Length);
                                }

                            }
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                ServerLogManager.Current.AppendConsoleLogLine(ex.Message);
                ServerLogManager.Current.E(ex.ToString());
            }
            finally
            {
                // Đóng stream để hoàn thành gửi về client
                outputstream.Close();
            }

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
