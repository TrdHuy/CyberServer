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
        public ToolVersionItemViewModel(ToolVersion vo) : base(vo)
        {
        }

        public ToolVersionItemViewModel()
        {
            _vo = new ToolVersion();
        }
    }
}

