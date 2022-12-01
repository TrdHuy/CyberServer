using cyber_server.implements.plugin_manager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.view_models.list_view_item.plugin_item
{
    public class PluginVersionItemViewModel : BaseObjectVersionItemViewModel
    {
        [Bindable(true)]
        public string MainClassName
        {
            get
            {
                    return (_vo as PluginVersion)?.MainClassName;
            }
            set
            {

                (_vo as PluginVersion)?.SetMainClassName(value);
                InvalidateOwn();
            }
        }

        public PluginVersionItemViewModel(PluginVersion vo) : base(vo)
        {
        }

        public PluginVersionItemViewModel()
        {
            _vo = new PluginVersion();
        }

    }
}
