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
    public abstract class BaseObjectVersionItemViewModel : BaseViewModel
    {
        private string _localFilePath = "";

        public abstract BaseObjectVersionModel RawModel { get; }

        public byte[] File
        {
            get
            {
                return RawModel.File;
            }
            set
            {
                RawModel.File = value;
            }
        }

        [Bindable(true)]
        public string CompressLength
        {
            get
            {
                return Math.Round(RawModel.CompressLength / Math.Pow(2, 20), 2) + "MB";
            }
            set
            {
                RawModel.CompressLength = Convert.ToInt64(value);
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string RawLength
        {
            get
            {
                return Math.Round(RawModel.RawLength / Math.Pow(2, 20), 2) + "MB";
            }
            set
            {
                RawModel.RawLength = Convert.ToInt64(value);
                InvalidateOwn();
            }
        }


        [Bindable(true)]
        public string Version
        {
            get
            {
                return RawModel.Version;
            }
            set
            {
                RawModel.Version = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string DatePublished
        {
            get
            {
                return RawModel.DatePublished.ToString("dd-MM-yyyy");
            }
            set
            {
                RawModel.DatePublished = DateTime.Parse(value);
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string Description
        {
            get
            {
                return RawModel.Description;
            }
            set
            {
                RawModel.Description = value;
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
                RawModel.FileName = Path.GetFileName(_localFilePath);
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string ExecutePath
        {
            get
            {
                return RawModel.ExecutePath;
            }
            set
            {
                RawModel.ExecutePath = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string AssemblyName
        {
            get
            {
                return RawModel.AssemblyName;
            }
            set
            {
                RawModel.AssemblyName = value;
                InvalidateOwn();
            }
        }

        public bool IsThisVersionAddedNewly()
        {
            if (RawModel != null)
            {
                return RawModel.VersionId == -1;
            }
            return true;
        }

        public async Task<BaseObjectVersionModel> BuildNewVersionModel()
        {
            if (RawModel.VersionId == -1)
            {
                if (System.IO.File.Exists(_localFilePath))
                {
                    using (FileStream stream = System.IO.File.Open(_localFilePath, FileMode.Open))
                    {
                        RawModel.File = new byte[stream.Length];
                        await stream.ReadAsync(RawModel.File, 0, (int)stream.Length);
                    }
                    return RawModel;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return RawModel.Clone() as BaseObjectVersionModel;
            }
        }
    }
}
