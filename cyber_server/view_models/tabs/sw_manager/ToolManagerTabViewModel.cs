using cyber_server.definition;
using cyber_server.implements.db_manager;
using cyber_server.implements.plugin_manager;
using cyber_server.models;
using cyber_server.view_models.list_view_item;
using cyber_server.view_models.list_view_item.tool_item;
using cyber_server.views.usercontrols.tabs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace cyber_server.view_models.tabs.sw_manager
{
    internal class ToolManagerTabViewModel : BaseSwManagerTabViewModel
    {
        public ToolManagerTabViewModel()
        {
        }

        public override async Task ReloadSwSource()
        {
            SwSource.Clear();
            await CyberDbManager.Current.RequestDbContextAsync((context) =>
            {
                foreach (var tool in context.Tools)
                {
                    var vm = new ToolItemViewModel(tool);
                    SwSource.Add(vm);
                }
            });
        }

        public override async Task<bool> DeleteSwFromDb(BaseObjectSwItemViewModel toolItemVM)
        {
            var toolModel = toolItemVM.RawModel as Tool;
            if (toolModel == null) return false;

            var sucess = await CyberDbManager.Current.RequestDbContextAsync((context) =>
            {
                context.ToolVersions.RemoveRange(toolModel.ToolVersions);
                context.Tools.Remove(toolModel);
                context.SaveChanges();
            });

            if (sucess)
            {
                SwSource.Remove(toolItemVM);
            }
            return sucess;
        }

        [Obsolete("Method is deprecated, since using byte array instead saving to folder physically")]
        public override async Task<bool> SyncSwFolderWithDb()
        {
            string message = "Đã xóa:\n";
            var isShouldNotify = false;

            var toolKeys = CyberPluginAndToolManager.Current.GetAllToolKeyInToolStorageFolder();

            var sucess = await CyberDbManager.Current.RequestDbContextAsync((context) =>
            {
                var deleteKeys = "";
                var totalDelete = 0;
                var deleteVersions = "";
                var totalDeleteVersion = 0;

                foreach (var key in toolKeys)
                {
                    var tool = context.Tools.Where(t => t.StringId == key)
                        .FirstOrDefault();
                    if (tool == null)
                    {
                        CyberPluginAndToolManager.Current.DeleteToolDirectory(key, true);
                        deleteKeys = deleteKeys + key + "\n";
                        totalDelete++;
                        isShouldNotify = true;
                    }
                    else
                    {
                        var toolVersion = CyberPluginAndToolManager.Current.GetAllToolVersionInStorageFolder(key);
                        foreach (var version in toolVersion)
                        {
                            if (tool.ToolVersions.Where(v => Version.Parse(v.Version) == Version.Parse(version))
                                .FirstOrDefault() == null)
                            {
                                CyberPluginAndToolManager.Current.DeleteToolVersionDirectory(key, version, true);
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
            , BaseObjectVersionModel toolVer)
        {
            var toolModel = modifiedItemViewModel.RawModel as Tool;
            var toolVerCast = toolVer as ToolVersion;
            if (toolModel == null && toolVerCast == null) return false;

            return await CyberDbManager.Current.RequestDbContextAsync((dbContext) =>
            {
                toolModel.ToolVersions.Add(toolVerCast);
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
            var tool = new Tool();
            tool.StringId = swKey;
            tool.Name = swName;
            tool.Author = swAuthor;
            tool.Description = swDes;
            tool.ProjectURL = swUrl;
            tool.IconSource = swIconSource;
            tool.IsPreReleased = isPreReleased;
            tool.IsAuthenticated = isAuthenticated;
            tool.Downloads = 0;

            // Build version source
            foreach (var version in versionSource)
            {
                var localFilePath = version.GetVersionSourceLocalFilePath();
                var tv = version.BuildToolVersionFromViewModel(tool.StringId) as ToolVersion;
                if (tv != null && File.Exists(localFilePath))
                {
                    tv.File = File.ReadAllBytes(localFilePath);
                    tool.ToolVersions.Add(tv);
                }
            }

            tool.IconSource = BuildSwIconSource(swKey, swIconSource);

            return tool;
        }

        protected override async Task<BaseObjectSwItemViewModel> AddNewSwToDatabase(BaseObjectSwModel swModel)
        {
            ToolItemViewModel vm = null;
            var tool = swModel as Tool;
            if (tool == null) return null;
            await CyberDbManager.Current.RequestDbContextAsync((context) =>
            {
                context.Tools.Add(tool);
                context.SaveChanges();
                vm = new ToolItemViewModel(tool);
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
                            .CopyToolIconToServerLocation(swIconSource, swKey);
                        return CyberServerDefinition.SERVER_REMOTE_ADDRESS
                            + "/toolresource/"
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
                var tool = modifingContext.RawModel as Tool;
                var toolVersion = context.RawModel as ToolVersion;
                if (tool != null && toolVersion != null && tool.ToolVersions.Count > 1)
                {
                    dbcontext.ToolVersions.Remove(toolVersion);
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
            var isToolKeyExist = false;
            await CyberDbManager.Current.RequestDbContextAsync((context) =>
            {
                isToolKeyExist = context
                 .Tools
                 .Any<Tool>(t =>
                     t.StringId.Equals(swKey, StringComparison.CurrentCultureIgnoreCase));
            });
            return isToolKeyExist;
        }
    }
}
