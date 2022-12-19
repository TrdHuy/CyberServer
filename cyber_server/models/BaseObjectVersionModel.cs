using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.models
{
    public abstract class BaseObjectVersionModel: ICloneable
    {
        [Editable(false)]
        public int VersionId { get; set; } = -1;
        public string Version { get; set; }
        public string Description { get; set; }
        public string ExecutePath { get; set; }
        public System.DateTime DatePublished { get; set; }
        
        [JsonIgnore]
        public byte[] File { get; set; }
        public string FileName { get; set; }
        public long CompressLength { get; set; }
        public long RawLength { get; set; }
        public string AssemblyName { get; set; }

        public abstract object Clone();
    }
}
