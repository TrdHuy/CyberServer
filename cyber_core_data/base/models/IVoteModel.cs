using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_core_data.@base.models
{
    public interface IVoteModel
    {
         int Stars { get; set; }
         int VoteId { get; set; }
         int PluginId { get; set; }

          IPluginModel Plugin { get; set; }
    }
}
