using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.models
{
    public class BaseObjectSwModel
    {
        public string StringId { get; set; } = "";
        public string Name { get; set; } = "";
        public string Author { get; set; } = "";
        public string Description { get; set; } = "";
        public string ProjectURL { get; set; } = "";
        public string IconSource { get; set; } = "";
        public bool IsAuthenticated { get; set; }
        public bool IsPreReleased { get; set; }
        public bool IsRequireLatestVersionToRun { get; set; }
        public int Downloads { get; set; }

        [JsonIgnore]
        public byte[] IconFile { get; set; }
    }
}
