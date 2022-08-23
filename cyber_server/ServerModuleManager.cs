using cyber_server.@base;
using cyber_server.implements.db_manager;
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
