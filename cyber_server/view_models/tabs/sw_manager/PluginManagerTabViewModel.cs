using cyber_server.definition;
using cyber_server.implements.db_manager;
using cyber_server.implements.plugin_manager;
using cyber_server.models;
using cyber_server.view_models.list_view_item;
using cyber_server.view_models.list_view_item.plugin_item;
using cyber_server.view_models.list_view_item.tool_item;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace cyber_server.view_models.tabs.sw_manager
{
    internal class PluginManagerTabViewModel : BaseSwManagerTabViewModel
    {
        public PluginManagerTabViewModel()
        {
        }

        public override async Task ReloadSwSource()
        {
            SwSource.Clear();
            await CyberDbManager.Current.RequestDbContextAsync((context) =>
            {
                foreach (var plugin in context.Plugins)
                {
                    var vm = new PluginItemViewModel(plugin);
                    SwSource.Add(vm);
                }
            });
        }

        public override async Task<bool> DeleteSwFromDb(BaseObjectSwItemViewModel pluginItemVM)
        {
            var pluginModel = pluginItemVM.RawModel as Plugin;
            if (pluginModel == null) return false;

            var sucess = await CyberDbManager.Current.RequestDbContextAsync((context) =>
            {
                context.PluginVersions.RemoveRange(pluginModel.PluginVersions);
                context.Plugins.Remove(pluginModel);
                context.SaveChanges();
            });

            if (sucess)
            {
                SwSource.Remove(pluginItemVM);
            }
            return sucess;
        }

        [Obsolete("Method is deprecated, since using byte array instead saving to folder physically")]
        public override async Task<bool> SyncSwFolderWithDb()
        {
            string message = "Đã xóa:\n";
            var isShouldNotify = false;

            var pluginKeys = CyberPluginAndToolManager.Current.GetAllPluginKeyInPluginStorageFolder();

            var sucess = await CyberDbManager.Current.RequestDbContextAsync((context) =>
            {
                var deleteKeys = "";
                var totalDelete = 0;
                var deleteVersions = "";
                var totalDeleteVersion = 0;

                foreach (var key in pluginKeys)
                {
                    var plugin = context.Plugins.Where(t => t.StringId == key)
                        .FirstOrDefault();
                    if (plugin == null)
                    {
                        CyberPluginAndToolManager.Current.DeletePluginDirectory(key, true);
                        deleteKeys = deleteKeys + key + "\n";
                        totalDelete++;
                        isShouldNotify = true;
                    }
                    else
                    {
                        var pluginVersion = CyberPluginAndToolManager.Current.GetAllPluginVersionInStorageFolder(key);
                        foreach (var version in pluginVersion)
                        {
                            if (plugin.PluginVersions.Where(v => Version.Parse(v.Version) == Version.Parse(version))
                                .FirstOrDefault() == null)
                            {
                                CyberPluginAndToolManager.Current.DeletePluginVersionDirectory(key, version, true);
                                deleteVersions = deleteVersions + key + " version=" + version + "\n";
                                totalDeleteVersion++;
                                isShouldNotify = true;
                            }
                        }
                    }
                }

                message = "Đã xóa plugin:" + totalDelete + "\n"
                    + deleteKeys + "\n"
                    + "Đã xóa plugin version:" + totalDeleteVersion + "\n"
                    + deleteVersions;
            });
            if (isShouldNotify)
            {
                MessageBox.Show(message);
            }

            return true;
        }

        protected override async Task<bool> AddSwVersionToDatabase(BaseObjectSwItemViewModel modifiedItemViewModel
            , BaseObjectVersionModel pluginVer)
        {
            var pluginModel = modifiedItemViewModel.RawModel as Plugin;
            var pluginVerCast = pluginVer as PluginVersion;
            if (pluginModel == null && pluginVerCast == null) return false;

            return await CyberDbManager.Current.RequestDbContextAsync((dbContext) =>
            {
                pluginModel.PluginVersions.Add(pluginVerCast);
                dbContext.SaveChanges();
            });
        }

        protected override BaseObjectSwModel BuildSwModel(string swKey
           , string swName
           , string swAuthor
           , string swDes
           , string swUrl
           , string swIconSource
           , bool isPreReleased
           , bool isAuthenticated
           , ObservableCollection<BaseObjectVersionItemViewModel> versionSource)
        {
            var plugin = new Plugin();
            plugin.StringId = swKey;
            plugin.Name = swName;
            plugin.Author = swAuthor;
            plugin.Description = swDes;
            plugin.ProjectURL = swUrl;
            plugin.IconSource = swIconSource;
            plugin.IsPreReleased = isPreReleased;
            plugin.IsAuthenticated = isAuthenticated;
            plugin.Downloads = 0;

            // Build version source
            foreach (var version in versionSource)
            {
                var localFilePath = version.GetVersionSourceLocalFilePath();
                var pv = version.BuildToolVersionFromViewModel(plugin.StringId) as PluginVersion;
                if (pv != null && File.Exists(localFilePath))
                {
                    pv.File = File.ReadAllBytes(localFilePath);
                    plugin.PluginVersions.Add(pv);
                }
            }

            plugin.IconSource = BuildSwIconSource(swKey, swIconSource);

            return plugin;
        }

        protected override async Task<BaseObjectSwItemViewModel> AddNewSwToDatabase(BaseObjectSwModel swModel)
        {
            PluginItemViewModel vm = null;
            var plugin = swModel as Plugin;
            if (plugin == null) return null;
            await CyberDbManager.Current.RequestDbContextAsync((context) =>
            {
                context.Plugins.Add(plugin);
                context.SaveChanges();
                vm = new PluginItemViewModel(plugin);
            });
            return vm;

        }

        protected override string BuildSwIconSource(string swKey, string swIconSource)
        {
            //Build icon source
            try
            {
                if (swIconSource != "")
                {
                    var isLocalFile = new Uri(swIconSource).IsFile;
                    if (isLocalFile)
                    {
                        CyberPluginAndToolManager
                            .Current
                            .CopyPluginIconToServerLocation(swIconSource, swKey);
                        return CyberServerDefinition.SERVER_REMOTE_ADDRESS
                            + "/pluginresource/"
                            + swKey + "/" + System.IO.Path.GetFileName(swIconSource);
                    }
                }
            }
            catch
            {
            }

            return base.BuildSwIconSource(swKey, swIconSource);
        }

        protected override async Task<bool> DeleteSwVersionInDatabase(BaseObjectVersionItemViewModel context
            , BaseObjectSwItemViewModel modifingContext)
        {
            var success = false;
            await CyberDbManager.Current.RequestDbContextAsync((dbcontext) =>
            {
                var plugin = modifingContext.RawModel as Plugin;
                var pluginVersion = context.RawModel as PluginVersion;
                if (plugin != null && pluginVersion != null && plugin.PluginVersions.Count > 1)
                {
                    dbcontext.PluginVersions.Remove(pluginVersion);
                    dbcontext.SaveChanges();
                    modifingContext.VersionSource.Remove(context);
                    success = true;
                }
                else
                {
                    MessageBox.Show("Không thể xóa phiên bản duy nhất!\nVui lòng thêm các phiên bản khác để tiến hành thao tác này!", "Thông báo");
                }

            });
            return success;
        }

        protected override async Task<bool> IsSwKeyExistInDatabase(string swKey)
        {
            var isPluginExist = false;
            await CyberDbManager.Current.RequestDbContextAsync((context) =>
            {
                isPluginExist = context
                 .Plugins
                 .Any<Plugin>(t =>
                     t.StringId.Equals(swKey, StringComparison.CurrentCultureIgnoreCase));
            });
            return isPluginExist;
        }
    }
}
