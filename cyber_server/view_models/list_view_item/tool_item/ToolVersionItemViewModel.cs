using cyber_server.models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.view_models.list_view_item.tool_item
{
    public class ToolVersionItemViewModel : BaseObjectVersionItemViewModel
    {
        private ToolVersion _vo;
        public override BaseObjectVersionModel RawModel => _vo;

        public ToolVersionItemViewModel(ToolVersion vo)
        {
            _vo = vo ?? new ToolVersion();
        }

    }
}

