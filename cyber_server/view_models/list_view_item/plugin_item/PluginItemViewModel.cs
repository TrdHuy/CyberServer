using cyber_server.definition;
using cyber_server.models;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.view_models.list_view_item.plugin_item
{
    public class PluginItemViewModel : BaseObjectSwItemViewModel
    {
        private Plugin _vo;
        private double _rates;

        [Bindable(true)]
        public double Rates
        {
            get => _rates;
            set
            {
                _rates = value;
                InvalidateOwn();
            }
        }

        public override BaseObjectSwModel RawModel => _vo;

        public PluginItemViewModel(Plugin baseModel) : base(baseModel)
        {
            _vo = baseModel ?? new Plugin();
        }

        protected override async Task DoInitOtherPropertiesTask(BaseObjectSwModel model)
        {
            var cast = model as Plugin;
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

        protected override void AddNewVersionModelToRawModel(BaseObjectVersionModel versionModel)
        {
            var cast = versionModel as PluginVersion;
            if (cast != null)
            {
                _vo.PluginVersions.Add(cast);
            }
        }

        protected override bool IsNewModel()
        {
            return _vo.PluginId == -1;
        }

        protected override async Task<string> BuildSwIconSource()
        {
            try
            {
                if (IconSource != "")
                {
                    var isLocalFile = new Uri(IconSource).IsFile;
                    if (isLocalFile && System.IO.File.Exists(IconSource))
                    {
                        using (FileStream stream = System.IO.File.Open(IconSource, FileMode.Open))
                        {
                            RawModel.IconFile = new byte[stream.Length];
                            await stream.ReadAsync(RawModel.IconFile, 0, (int)stream.Length);
                        }
                        return CyberServerDefinition.SERVER_REMOTE_ADDRESS
                            + "pluginresource/"
                            + StringId + "/" + System.IO.Path.GetFileName(IconSource);
                    }
                }
            }
            catch
            {
            }
            return IconSource;
        }
    }
}

