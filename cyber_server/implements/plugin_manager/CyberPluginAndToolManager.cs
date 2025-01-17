﻿using cyber_server.@base;
using cyber_server.definition;
using cyber_server.implements.log_manager;
using cyber_server.view_models.list_view_item;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.implements.plugin_manager
{
    internal class CyberPluginAndToolManager : IServerModule
    {
        private string pluginFolderLocation = CyberServerDefinition.PLUGIN_BASE_FOLDER_PATH;
        private string toolFolderLocation = CyberServerDefinition.TOOL_BASE_FOLDER_PATH;

        public static CyberPluginAndToolManager Current
        {
            get => ServerModuleManager.CPM_Instance;
        }

        public void Dispose()
        {
        }

        public void OnModuleInit()
        {
            if (!Directory.Exists(pluginFolderLocation))
            {
                Directory.CreateDirectory(pluginFolderLocation);
            }
        }

        public void DeletePluginDirectory(string pluginKey, bool rescursive = false)
        {
            if (Directory.Exists(pluginFolderLocation + "\\" + pluginKey))
            {
                Directory.Delete(pluginFolderLocation + "\\" + pluginKey, rescursive);
            }
        }

        [Obsolete("Method is deprecated, since using byte array instead saving to folder physically")]
        public void DeleteToolDirectory(string toolKey, bool rescursive = false)
        {
            if (Directory.Exists(toolFolderLocation + "\\" + toolKey))
            {
                Directory.Delete(toolFolderLocation + "\\" + toolKey, rescursive);
            }
        }

        public void DeletePluginVersionDirectory(string pluginKey, string version, bool rescursive = false)
        {
            var folder = pluginFolderLocation + "\\" + pluginKey + "\\versions\\" + version;
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, rescursive);
            }
        }

        [Obsolete("Method is deprecated, since using byte array instead saving to folder physically")]
        public void DeleteToolVersionDirectory(string toolKey, string version, bool rescursive = false)
        {
            var folder = toolFolderLocation + "\\" + toolKey + "\\versions\\" + version;
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, rescursive);
            }
        }

        public bool CheckToolPathExistOnServer(BaseObjectVersionItemViewModel swVersionViewModel)
        {
            return swVersionViewModel.RawModel.File != null && swVersionViewModel.RawModel.File.Length > 0;
        }

        public bool RenamePluginFolder(string oldPluginKey, string newPluginKey)
        {
            if (Directory.Exists(pluginFolderLocation + "\\" + oldPluginKey))
            {
                Directory.Move(pluginFolderLocation + "\\" + oldPluginKey,
                    pluginFolderLocation + "\\" + newPluginKey);

                return true;
            }
            ServerLogManager.Current.D("source file not found!");
            return false;
        }

        public bool CopyPluginToServerLocation(string sourceFile, string destination)
        {
            if (string.IsNullOrEmpty(sourceFile)
                || string.IsNullOrEmpty(destination)) return false;
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
            ServerLogManager.Current.D("source file not found!");
            return false;
        }

        [Obsolete("Method is deprecated, since using byte array instead saving to folder physically")]
        public bool CopyToolToServerLocation(string sourceFile, string destination)
        {
            if (string.IsNullOrEmpty(sourceFile)
                || string.IsNullOrEmpty(destination)) return false;
            if (File.Exists(sourceFile))
            {
                var fileName = Path.GetFileName(sourceFile);
                if (!Directory.Exists(toolFolderLocation + "\\" + destination))
                {
                    Directory.CreateDirectory(toolFolderLocation + "\\" + destination);
                }
                File.Copy(sourceFile, toolFolderLocation + "\\" + destination + "\\" + fileName, true);
                return true;
            }
            ServerLogManager.Current.D("source file not found!");
            return false;
        }

        [Obsolete("Method is deprecated, since using byte array instead saving to folder physically")]
        public bool CopyToolIconToServerLocation(string sourceFile, string toolKey)
        {
            if (File.Exists(sourceFile))
            {
                var fileName = Path.GetFileName(sourceFile);
                if (!Directory.Exists(toolFolderLocation + "\\" + toolKey + "\\resources"))
                {
                    Directory.CreateDirectory(toolFolderLocation + "\\" + toolKey + "\\resources");
                }
                File.Copy(sourceFile, toolFolderLocation + "\\" + toolKey + "\\resources" + "\\" + fileName, true);
                return true;
            }
            ServerLogManager.Current.D("source file not found!");
            return false;
        }

        public bool CopyPluginIconToServerLocation(string sourceFile, string pluginKey)
        {
            if (File.Exists(sourceFile))
            {
                var fileName = Path.GetFileName(sourceFile);
                if (!Directory.Exists(pluginFolderLocation + "\\" + pluginKey + "\\resources"))
                {
                    Directory.CreateDirectory(pluginFolderLocation + "\\" + pluginKey + "\\resources");
                }
                File.Copy(sourceFile, pluginFolderLocation + "\\" + pluginKey + "\\resources" + "\\" + fileName, true);
                return true;
            }
            ServerLogManager.Current.D("source file not found!");
            return false;
        }

        public bool MovePluginToServerLocation(string sourceFile)
        {
            if (File.Exists(sourceFile))
            {
                File.Move(sourceFile, pluginFolderLocation);
                return true;
            }
            ServerLogManager.Current.D("source file not found!");
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
            ServerLogManager.Current.D("source file not found!");
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
            ServerLogManager.Current.D("source file not found!");
            return false;
        }

        [Obsolete("Method is deprecated, since using byte array instead saving to folder physically")]
        public string GetSetupZipFilePathByToolVersion(string toolKey, string toolVersion, string toolZipFileName)
        {
            return GetToolVersionFolderPath(toolKey) + "\\" + toolVersion + "\\" + toolZipFileName;
        }

        [Obsolete("Method is deprecated, since using byte array instead saving to folder physically")]
        public string GetToolVersionFolderPath(string toolKey)
        {
            return toolFolderLocation + "\\" + toolKey + "\\" + "versions";
        }

        public string GetSetupZipFilePathByPluginVersion(string pluginKey, string pluginVersion, string pluginZipFileName)
        {
            return GetPluginVersionForderPath(pluginKey) + "\\" + pluginVersion + "\\" + pluginZipFileName;
        }

        public string GetPluginVersionForderPath(string pluginKey)
        {
            return pluginFolderLocation + "\\" + pluginKey + "\\" + "versions";
        }

        public string BuildPluginVersionFolderPath(string pluginKey, string version)
        {
            try
            {
                var v = Version.Parse(version);
            }
            catch
            {
                throw new InvalidOperationException("version is invaild");
            }
            return pluginKey + "\\" + "versions" + "\\" + version;
        }

        [Obsolete("Method is deprecated, since using byte array instead saving to folder physically")]
        public string BuildToolVersionFolderPath(string toolKey, string version)
        {
            try
            {
                var v = Version.Parse(version);
            }
            catch
            {
                throw new InvalidOperationException("version is invaild");
            }
            return toolKey + "\\" + "versions" + "\\" + version;

        }

        public string[] GetAllPluginDirectory()
        {
            return Directory.GetDirectories(pluginFolderLocation);
        }

        public string[] GetAllPluginVersionInStorageFolder(string pluginKey)
        {
            var folders = Directory.GetDirectories(pluginFolderLocation + "\\" + pluginKey + "\\" + "versions");
            var version = new string[folders.Length];
            int i = 0;
            foreach (var folder in folders)
            {
                version[i++] = new DirectoryInfo(folder).Name;
            }
            return version;
        }

        public string[] GetAllPluginKeyInPluginStorageFolder()
        {
            var folders = Directory.GetDirectories(pluginFolderLocation);
            var keys = new string[folders.Length];
            int i = 0;
            foreach (var folder in folders)
            {
                keys[i++] = new DirectoryInfo(folder).Name;
            }
            return keys;
        }

        [Obsolete("Method is deprecated, since using byte array instead saving to folder physically")]
        public string[] GetAllToolKeyInToolStorageFolder()
        {
            var folders = Directory.GetDirectories(toolFolderLocation);
            var keys = new string[folders.Length];
            int i = 0;
            foreach (var folder in folders)
            {
                keys[i++] = new DirectoryInfo(folder).Name;
            }
            return keys;
        }

        [Obsolete("Method is deprecated, since using byte array instead saving to folder physically")]
        public string[] GetAllToolVersionInStorageFolder(string toolKey)
        {
            var folders = Directory.GetDirectories(toolFolderLocation + "\\" + toolKey + "\\" + "versions");
            var version = new string[folders.Length];
            int i = 0;
            foreach (var folder in folders)
            {
                version[i++] = new DirectoryInfo(folder).Name;
            }
            return version;
        }

        [Obsolete("Method is deprecated, since using byte array instead saving to folder physically")]
        public bool RenameToolVersionFolder(string toolKey, string oldToolVersion, string newToolVersion)
        {
            var toolVersionFolderPath = GetToolVersionFolderPath(toolKey);
            if (Directory.Exists(toolVersionFolderPath + "\\" + oldToolVersion))
            {
                Directory.Move(toolVersionFolderPath + "\\" + oldToolVersion,
                    toolVersionFolderPath + "\\" + newToolVersion);
                return true;
            }
            ServerLogManager.Current.D("source file not found!");
            return false;
        }
    }
}
