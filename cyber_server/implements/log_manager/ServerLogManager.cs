using cyber_server.@base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.implements.log_manager
{
    internal class ServerLogManager : IServerModule
    {
        private string _logCache;

        public delegate void ServerLogChangedHandler(object sender, string newLog);

        public event ServerLogChangedHandler ServerLogChanged;

        public static ServerLogManager Current
        {
            get
            {
                return ServerModuleManager.SLM_Instance;
            }
        }

        private ServerLogManager()
        {

        }

        public void Dispose()
        {
        }

        public void OnModuleInit()
        {
            _logCache = "";
        }

        public void AppendLogLine(string newLine)
        {
            _logCache = _logCache + newLine + "\n";
            ServerLogChanged?.Invoke(this, _logCache);
        }

        public void ClearLog()
        {
            _logCache = "";
            ServerLogChanged?.Invoke(this, _logCache);
        }
    }
}
