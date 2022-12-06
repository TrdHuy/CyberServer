using cyber_server.implements.plugin_manager;
using cyber_server.models;
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
        private PluginVersion _vo;
        public override BaseObjectVersionModel RawModel => _vo;

        [Bindable(true)]
        public string MainClassName
        {
            get
            {
                    return _vo.MainClassName;
            }
            set
            {
                _vo.SetMainClassName(value);
                InvalidateOwn();
            }
        }

        public PluginVersionItemViewModel(PluginVersion vo)
        {
            _vo = vo ?? new PluginVersion();
        }

    }
}
