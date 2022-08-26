using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_core_data.@base.models
{
    public interface IPluginVersionModel
    {
        int VersionId { get; set; }
        string Version { get; set; }
        string Description { get; set; }
        int PluginId { get; set; }
        System.DateTime DatePublished { get; set; }
        string FilePath { get; set; }

        IPluginModel Plugin { get; set; }
    }
}
