using cyber_server.implements.attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.models
{
    public abstract class BaseObjectVersionModel : BaseCloneableObject, IComparable<BaseObjectVersionModel>
    {
        [Cloneable(false, -1)]
        [Editable(false)]
        public int VersionId { get; set; } = -1;

        [Cloneable(true)]
        public string Version { get; set; }

        [Cloneable(true)]
        public string Description { get; set; }

        [Cloneable(true)]
        public string ExecutePath { get; set; }

        [Cloneable(true)]
        public System.DateTime DatePublished { get; set; }

        [Cloneable(true)]
        [JsonIgnore]
        public byte[] File { get; set; }


        [Cloneable(true)]
        public string FileName { get; set; }

        [Cloneable(true)]
        public long CompressLength { get; set; }

        [Cloneable(true)]
        public long RawLength { get; set; }

        [Cloneable(true)]
        public string AssemblyName { get; set; }

        [Cloneable(true)]
        public Nullable<bool> IsDisable { get; set; } = false;

        public int CompareTo(BaseObjectVersionModel other)
        {
            if (System.Version.Parse(this.Version) > System.Version.Parse(other.Version))
            {
                return 1;
            }
            if (System.Version.Parse(this.Version) < System.Version.Parse(other.Version))
            {
                return -1;
            }
            return 0;
        }
    }
}
