using cyber_server.implements.log_manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.view_models.tabs
{
    internal class ServerControllerTabViewModel : BaseViewModel
    {
        private string _consoleLogText = "";

        public string ConsoleLogText
        {
            get => _consoleLogText;
            set
            {
                _consoleLogText = value;
                InvalidateOwn();
            }
        }
        public ServerControllerTabViewModel()
        {
            ServerLogManager.Current.ServerLogChanged += (s, e) =>
            {
                ConsoleLogText = e;
            };
        }
    }
}
