using cyber_server.@base;
using cyber_server.implements.cert_manager;
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
    internal class RequestDownloadCertificateHttpHandler : ICyberHttpRequestHandler
    {

        private const string REQUEST_DOWNLOAD_CYBER_ROOT_CA_HEADER_ID = "DOWNLOAD_CYBER_ROOT_CA";

        private const string RESPONSE_DOWNLOAD_CERTIFICATE_NAME_HEADER_ID = "CERTIFICATE_NAME";
        private const string RESPONSE_DOWNLOAD_CERTIFICATE_PASSWORD_HEADER_ID = "CERTIFICATE_PASSWORD";

        public RequestDownloadCertificateHttpHandler()
        {
        }

        public async Task<byte[]> Handle(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.Headers.AllKeys.Contains(CyberHttpServer.REQUEST_DOWNLOAD_CERTIFICATE_HEADER_KEY))
            {
                switch (request.Headers[CyberHttpServer.REQUEST_DOWNLOAD_CERTIFICATE_HEADER_KEY])
                {
                    case REQUEST_DOWNLOAD_CYBER_ROOT_CA_HEADER_ID:
                        {
                            var certKey = "huy.td1_ca";
                            ServerLogManager.Current.I("Request to download cyber root ca: key=");

                            Certificate query = null;
                            await CyberDbManager.Current.RequestDbContextAsync((dbContext) =>
                            {
                                query = dbContext
                                    .Certificates
                                    .Where(cert => cert.StringId == certKey)
                                    .FirstOrDefault();
                            });

                            if (query != null)
                            {
                                response.StatusCode = (int)HttpStatusCode.OK;

                                byte[] buffer = query.File;

                                response.ContentLength64 = buffer.Length;
                                response.Headers.Add(RESPONSE_DOWNLOAD_CERTIFICATE_NAME_HEADER_ID, query.FileName);
                                if (query.Password != null)
                                {
                                    response.Headers.Add(RESPONSE_DOWNLOAD_CERTIFICATE_PASSWORD_HEADER_ID, DESCryptoManager.DecryptTextFromMemory(query.Password));
                                }
                                return buffer;
                            }
                            else
                            {
                                response.StatusCode = (int)HttpStatusCode.NotFound;
                            }
                            break;
                        }
                }
            }

            return null;
        }
    }
}
