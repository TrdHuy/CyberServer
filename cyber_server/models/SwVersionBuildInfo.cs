using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.models
{
    public class SwVersionBuildInfo
    {
        public string Version { get; set; }
        public string MainAssemblyName { get; set; }
        public string PathToMainExe { get; set; }
        public string Description { get; set; }
        public string CompressedBuildFileName { get; set; }
        public string UninstallFilePath { get; set; }
        public string PathToMainDll { get; set; }
        public string MainClassName { get; set; }

    }
}
