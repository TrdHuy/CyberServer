using cyber_server.implements.plugin_manager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.view_models.tool_item
{
    public class ToolVersionItemViewModel : BaseViewModel
    {
        private ToolVersion _vo;
        private string _localFilePath = "";

        public ToolVersion RawModel => _vo;

        [Bindable(true)]
        public string CompressLength
        {
            get
            {
                return Math.Round(_vo.CompressLength / Math.Pow(2, 20), 2) + "MB";
            }
            set
            {
                _vo.CompressLength = Convert.ToInt64(value);
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string RawLength
        {
            get
            {
                return Math.Round(_vo.RawLength / Math.Pow(2, 20), 2) + "MB";
            }
            set
            {
                _vo.RawLength = Convert.ToInt64(value);
                InvalidateOwn();
            }
        }


        [Bindable(true)]
        public string Version
        {
            get
            {
                return _vo.Version;
            }
            set
            {
                _vo.Version = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string DatePublished
        {
            get
            {
                return _vo.DatePublished.ToString("dd-MM-yyyy");
            }
            set
            {
                _vo.DatePublished = DateTime.Parse(value);
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string Description
        {
            get
            {
                return _vo.Description;
            }
            set
            {
                _vo.Description = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string FilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(_localFilePath)) return _localFilePath;
                return _vo.FolderPath + "\\" + _vo.FileName;
            }
            set
            {
                _localFilePath = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string ExecutePath
        {
            get
            {
                return _vo.ExecutePath;
            }
            set
            {
                _vo.ExecutePath = value;
                InvalidateOwn();
            }
        }
        public ToolVersionItemViewModel(ToolVersion vo)
        {
            _vo = vo;
        }

        public ToolVersionItemViewModel()
        {
            _vo = new ToolVersion();
        }

        public ToolVersion BuildToolVersionFromViewModel(string toolKey)
        {
            _vo.FolderPath = CyberPluginAndToolManager.Current.BuildToolVersionFolderPath(toolKey, _vo.Version);
            if (!string.IsNullOrEmpty(_localFilePath))
            {
                _vo.FileName = Path.GetFileName(_localFilePath);
                _localFilePath = "";
            }
            return _vo;
        }

        public bool IsThisVersionAddedNewly()
        {
            if (_vo != null)
            {
                return _vo.VersionId == -1;
            }
            return true;
        }

        public string GetVersionSourceLocalFilePath()
        {
            return _localFilePath;
        }
    }
}
