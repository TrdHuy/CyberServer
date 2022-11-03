using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.view_models.list_view_item.certificate_item
{
    public class CertificateItemViewModel : BaseViewModel
    {
        private Certificate _baseModel;
        private bool _modifingModeEnable;

        public Certificate RawModel => _baseModel;
        
        public string CertKey
        {
            get => _baseModel.StringId;
            set
            {
                _baseModel.StringId = value;
                InvalidateOwn();
            }
        }

        public string Description
        {
            get => _baseModel.Description;
            set
            {
                _baseModel.Description = value;
                InvalidateOwn();
            }
        }

        public string Expiration
        {
            get
            {
                return _baseModel.Expiration.ToString("dd-MM-yyy");
            }
            set
            {
                _baseModel.Expiration = DateTime.Parse(value);
                InvalidateOwn();
            }
        }

        public bool ModifingModeEnable
        {
            get
            {
                return _modifingModeEnable;
            }
            set
            {
                _modifingModeEnable = value;
                InvalidateOwn();
            }
        }

        public int Downloads
        {
            get => _baseModel.Downloads;
        }


        
        public CertificateItemViewModel(Certificate baseModel)
        {
            _baseModel = baseModel;
        }

    }
}

