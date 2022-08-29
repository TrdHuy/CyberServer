using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.@base
{
    internal interface ICyberHttpRequestHandler
    {
        Task<byte[]> Handle(HttpListenerRequest request, HttpListenerResponse response);
    }
}
