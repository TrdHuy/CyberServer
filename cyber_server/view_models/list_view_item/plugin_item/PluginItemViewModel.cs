using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.view_models.list_view_item.plugin_item
{
    public class PluginItemViewModel : BaseObjectSwItemViewModel
    {
        private double _rates;

        public double Rates
        {
            get => _rates;
            set
            {
                _rates = value;
                InvalidateOwn();
            }
        }

        public PluginItemViewModel(Plugin baseModel) : base(baseModel)
        {
            if (baseModel == null)
            {
                _vo = new Plugin();
            };
        }

        protected override async Task DoInitOtherPropertiesTask()
        {
            var cast = _vo as Plugin;
            if (cast == null) return;

            await Task.Delay(100);
            if (cast.Votes.Count != 0)
            {
                Rates = cast.Votes.Average(v => v.Stars);
            }
            foreach (var pluginVerison in
                cast.PluginVersions.OrderByDescending(v => Version.Parse(v.Version)))
            {
                VersionSource.Add(new PluginVersionItemViewModel(pluginVerison));
            }
        }
    }
}

