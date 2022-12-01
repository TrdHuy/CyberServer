using cyber_server.models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.view_models.list_view_item
{
    public class BaseObjectVersionItemViewModel : BaseViewModel
    {
        protected BaseObjectVersionModel _vo;
        private string _localFilePath = "";

        public virtual BaseObjectVersionModel RawModel => _vo;

        public byte[] File
        {
            get
            {
                return _vo.File;
            }
            set
            {
                _vo.File = value;
            }
        }

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
                return _localFilePath;
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

        public BaseObjectVersionItemViewModel(BaseObjectVersionModel vo)
        {
            _vo = vo;
        }

        public BaseObjectVersionItemViewModel()
        {
            _vo = new BaseObjectVersionModel();
        }

        public BaseObjectVersionModel BuildToolVersionFromViewModel(string toolKey)
        {
            if (!string.IsNullOrEmpty(_localFilePath))
            {
                _vo.FileName = Path.GetFileName(_localFilePath);
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
