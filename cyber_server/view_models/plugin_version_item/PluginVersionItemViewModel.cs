﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.view_models.plugin_version_item
{
    public class PluginVersionItemViewModel : BaseViewModel
    {
        private PluginVersion _vo;
        private string _version;
        private string _description;
        private string _filePath;
        private DateTime _datePublised;
        public PluginVersion RawModel => _vo;

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
                    return _vo.DatePublished.ToString("dd-MM-yyy");
                }
                return _datePublised.ToString("dd-MM-yyy");
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
                    return _vo.FilePath;
                }
                return _filePath;
            }
            set
            {
                _filePath = value;
                InvalidateOwn();
            }
        }

        public PluginVersionItemViewModel(PluginVersion vo)
        {
            _vo = vo;
        }

        public PluginVersionItemViewModel()
        {
        }

        public PluginVersion BuildPluginVersionFromViewModel(string pluginKey)
        {
            if (_vo != null)
            {
                return _vo;
            }
            _vo = new PluginVersion();
            _vo.Version = _version;
            _vo.Description = _description;
            _vo.FilePath = pluginKey + "\\" + _version;
            _vo.DatePublished = _datePublised;
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
