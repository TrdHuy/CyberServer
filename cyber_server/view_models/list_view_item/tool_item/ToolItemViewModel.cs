using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.view_models.list_view_item.tool_item
{
    public class ToolItemViewModel : BaseObjectSwItemViewModel
    {

        public ToolItemViewModel(Tool baseModel) : base(baseModel)
        {
            if (baseModel == null)
            {
                _vo = new Tool();
            }
        }

        protected override async Task DoInitOtherPropertiesTask()
        {
            var cast = _vo as Tool;
            if (cast == null) return;

            await Task.Delay(100);

            foreach (var pluginVerison in
                cast.ToolVersions.OrderByDescending(v => Version.Parse(v.Version)))
            {
                VersionSource.Add(new ToolVersionItemViewModel(pluginVerison));
            }
        }
    }
}
