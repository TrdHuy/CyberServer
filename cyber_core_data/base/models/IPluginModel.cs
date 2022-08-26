using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_core_data.@base.models
{
    public interface IPluginModel
    {
        int PluginId { get; set; }
        string StringId { get; set; }
        string Name { get; set; }
        string Author { get; set; }
        string Description { get; set; }
        string ProjectURL { get; set; }
        string IconSource { get; set; }
        bool IsAuthenticated { get; set; }
        int Downloads { get; set; }

        ICollection<IPluginVersionModel> PluginVersions { get; set; }
        ICollection<ITagModel> Tags { get; set; }
        ICollection<IVoteModel> Votes { get; set; }
    }
}
