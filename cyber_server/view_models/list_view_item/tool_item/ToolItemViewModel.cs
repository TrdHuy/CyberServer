using cyber_server.definition;
using cyber_server.implements.plugin_manager;
using cyber_server.models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.view_models.list_view_item.tool_item
{
    public class ToolItemViewModel : BaseObjectSwItemViewModel
    {
        private Tool _vo;

        public override BaseObjectSwModel RawModel => _vo;

        public ToolItemViewModel(Tool baseModel) : base(baseModel)
        {
            _vo = baseModel ?? new Tool();
        }


        protected override async Task DoInitOtherPropertiesTask(BaseObjectSwModel model)
        {
            var cast = model as Tool;
            if (cast == null) return;

            await Task.Delay(100);

            foreach (var pluginVerison in
                cast.ToolVersions.OrderByDescending(v => Version.Parse(v.Version)))
            {
                VersionSource.Add(new ToolVersionItemViewModel(pluginVerison));
            }
        }

        protected override void AddNewVersionModelToRawModel(BaseObjectVersionModel versionModel)
        {
            var cast = versionModel as ToolVersion;
            if (cast != null)
            {
                _vo.ToolVersions.Add(cast);
            }
        }

        protected override bool IsNewModel()
        {
            return _vo.ToolId == -1;
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
                            + "toolresource/"
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
