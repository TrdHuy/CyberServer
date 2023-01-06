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
        public const string DB_FILE_NAME = "CyberDB.mdf";
        public static readonly string DB_FILE_PATH = AppDomain.CurrentDomain.BaseDirectory
           + DB_FILE_NAME;

        public const string SERVER_REMOTE_ADDRESS = "http://107.98.32.108:8080/";
        public const string SSL_SERVER_REMOTE_ADDRESS = "https://107.98.32.108:8088/";

        public const string NEW_BUILD_CONCEPT_SOFTWARE_VERSION_BUILD_INFO_FILE_NAME = "build.info.json";
        public const string NEW_BUILD_CONCEPT_SOFTWARE_VERSION_BUILD_FILE_NAME = "build.zip";
    }
}
