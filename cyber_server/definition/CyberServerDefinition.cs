using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.definition
{
    internal class CyberServerDefinition
    {
        public static readonly string PLUGIN_BASE_FOLDER_PATH = AppDomain.CurrentDomain.BaseDirectory
            + "plugins";
        public static readonly string TOOL_BASE_FOLDER_PATH = AppDomain.CurrentDomain.BaseDirectory
            + "tools";

        public static readonly string SERVER_REMOTE_ADDRESS = "http://107.127.131.89:8080";
    }
}
