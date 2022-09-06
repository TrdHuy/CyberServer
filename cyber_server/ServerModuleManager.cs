using cyber_server.@base;
using cyber_server.implements.db_manager;
using cyber_server.implements.http_server;
using cyber_server.implements.log_manager;
using cyber_server.implements.plugin_manager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server
{
    internal class ServerModuleManager
    {
        private static Collection<IServerModule> _Modules;

        private static IServerModule _DBM_Instance;
        private static IServerModule _CPM_Instance;
        private static IServerModule _CHS_Instance;
        private static IServerModule _SLM_Instance;

        public static ServerLogManager SLM_Instance
        {
            get
            {
                if (_SLM_Instance == null)
                {
                    _SLM_Instance = Activator.CreateInstance(typeof(ServerLogManager), true) as ServerLogManager;
                }
                return (ServerLogManager)_SLM_Instance;
            }

        }

        public static CyberHttpServer CHS_Instance
        {
            get
            {
                if (_CHS_Instance == null)
                {
                    _CHS_Instance = Activator.CreateInstance(typeof(CyberHttpServer), true) as CyberHttpServer;
                }
                return (CyberHttpServer)_CHS_Instance;
            }

        }

        public static CyberDbManager DBM_Instance
        {
            get
            {
                if (_DBM_Instance == null)
                {
                    _DBM_Instance = Activator.CreateInstance(typeof(CyberDbManager), true) as CyberDbManager;
                }
                return (CyberDbManager)_DBM_Instance;
            }
        }

        public static CyberPluginManager CPM_Instance
        {
            get
            {
                if (_CPM_Instance == null)
                {
                    _CPM_Instance = Activator.CreateInstance(typeof(CyberPluginManager), true) as CyberPluginManager;
                }
                return (CyberPluginManager)_CPM_Instance;
            }
        }

        static ServerModuleManager()
        {
            _Modules = new Collection<IServerModule>();
        }

        public static void Init()
        {
            _Modules.Clear();
            _Modules.Add(CPM_Instance);
            _Modules.Add(DBM_Instance);
            _Modules.Add(CHS_Instance);
            _Modules.Add(SLM_Instance);

            foreach (var module in _Modules)
            {
                module.OnModuleInit();
            }
        }

        public static void Destroy()
        {
            foreach (var module in _Modules)
            {
                module.Dispose();
            }
        }
    }
}
