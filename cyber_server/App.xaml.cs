using cyber_server.views.windows;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace cyber_server
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static App _instance;

        public static new App Current
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new App();
                }
                return _instance;
            }
        }

        private App() : base()
        {
            _instance = this;
        }

        public CyberServerWindow ServerWindow { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            ServerModuleManager.Init();
            base.OnStartup(e);

            ServerWindow = new CyberServerWindow(); 
            ServerWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ServerModuleManager.Destroy();
            base.OnExit(e);
        }
    }
}
