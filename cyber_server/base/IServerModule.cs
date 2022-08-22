using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.@base
{
    internal interface IServerModule : IDisposable
    {
        void OnModuleInit();
    }
}
