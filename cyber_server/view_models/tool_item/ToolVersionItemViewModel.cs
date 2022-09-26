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
        private string _version;
        private string _description;
        private string _filePath;
        private string _executePath;
        private DateTime _datePublised;
        public ToolVersion RawModel => _vo;

        [Bindable(true)]
        public string Version
        {
            get
            {
                if (_vo != null)
                    return _vo.Version;
                return _version;
            }
            set
            {
                _version = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string DatePublished
        {
            get
            {
                if (_vo != null)
                {
                    return _vo.DatePublished.ToString("dd-MM-yyyy");
                }
                return _datePublised.ToString("dd-MM-yyyy");
            }
            set
            {
                _datePublised = DateTime.Parse(value);
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string Description
        {
            get
            {
                if (_vo != null)
                {
                    return _vo.Description;
                }
                return _description;
            }
            set
            {
                _description = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string FilePath
        {
            get
            {
                if (_vo != null)
                {
                    return _vo.FolderPath;
                }
                return _filePath;
            }
            set
            {
                _filePath = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string ExecutePath
        {
            get
            {
                if (_vo != null)
                {
                    return _vo.ExecutePath;
                }
                return _executePath;
            }
            set
            {
                _executePath = value;
                InvalidateOwn();
            }
        }
        public ToolVersionItemViewModel(ToolVersion vo)
        {
            _vo = vo;
        }

        public ToolVersionItemViewModel()
        {
        }

        public ToolVersion BuildToolVersionFromViewModel(string toolKey)
        {
            if (_vo != null)
            {
                return _vo;
            }
            _vo = new ToolVersion();
            _vo.Version = _version;
            _vo.Description = _description;
            _vo.FolderPath = CyberPluginAndToolManager.Current.BuildToolVersionFolderPath(toolKey, _version);
            _vo.DatePublished = _datePublised;
            _vo.ExecutePath = _executePath;
            _vo.FileName = Path.GetFileName(_filePath);
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

        public string GetVersionSourceFilePath()
        {
            return _filePath;
        }
    }
}
