using cyber_server.implements.attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.models
{
    public abstract class BaseObjectSwModel : BaseCloneableObject
    {
        [Cloneable(true)]
        public string StringId { get; set; } = "";

        [Cloneable(true)]
        public string Name { get; set; } = "";

        [Cloneable(true)]
        public string Author { get; set; } = "";

        [Cloneable(true)]
        public string Description { get; set; } = "";

        [Cloneable(true)]
        public string ProjectURL { get; set; } = "";

        [Cloneable(true)]
        public string IconSource { get; set; } = "";

        [Cloneable(true)]
        public bool IsAuthenticated { get; set; }

        [Cloneable(true)]
        public bool IsPreReleased { get; set; }

        [Cloneable(true)]
        public bool IsRequireLatestVersionToRun { get; set; }

        [Cloneable(true)]
        public int Downloads { get; set; }

        [Cloneable(true)]
        [JsonIgnore]
        public byte[] IconFile { get; set; }

    }
}
