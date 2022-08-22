using cyber_server.@base;
using cyber_server.definition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.implements.plugin_manager
{
    internal class CyberPluginManager : IServerModule
    {
        private string pluginFolderLocation = CyberServerDefinition.PLUGIN_BASE_FOLDER_PATH;

        public static CyberPluginManager Current
        {
            get => ServerModuleManager.CPM_Instance;
        }

        public void Dispose()
        {
        }

        public void OnModuleInit()
        {
            if (!Directory.Exists(CyberServerDefinition.PLUGIN_BASE_FOLDER_PATH))
            {
                Directory.CreateDirectory(CyberServerDefinition.PLUGIN_BASE_FOLDER_PATH);
            }
        }

        public void DeletePluginDirectory(string pluginName)
        {
            if (Directory.Exists(pluginFolderLocation + "\\" + pluginName))
            {
                Directory.Delete(pluginFolderLocation + "\\" + pluginName);
            }
        }

        public bool CopyPluginToServerLocation(string sourceFile, string destination)
        {
            if (File.Exists(sourceFile))
            {
                var fileName = Path.GetFileName(sourceFile);
                if (!Directory.Exists(pluginFolderLocation + "\\" + destination))
                {
                    Directory.CreateDirectory(pluginFolderLocation + "\\" + destination);
                }
                File.Copy(sourceFile, pluginFolderLocation + "\\" + destination + "\\" + fileName, true);
                return true;
            }
            return false;
        }

        public bool MovePluginToServerLocation(string sourceFile)
        {
            if (File.Exists(sourceFile))
            {
                File.Move(sourceFile, pluginFolderLocation);
                return true;
            }
            return false;
        }

        public async Task<bool> CopyPluginToServerLocationAsync(string sourceFile, string destination)
        {
            if (File.Exists(sourceFile))
            {
                await Task.Delay(1);
                File.Copy(sourceFile, pluginFolderLocation + "\\" + destination, true);
                return true;
            }
            return false;
        }

        public async Task<bool> MovePluginToServerLocationAsync(string sourceFile)
        {
            if (File.Exists(sourceFile))
            {
                await Task.Delay(1);
                File.Move(sourceFile, pluginFolderLocation);
                return true;
            }
            return false;
        }
    }
}
