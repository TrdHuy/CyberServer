using cyber_server.models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.view_models.list_view_item
{
    public abstract class BaseObjectSwItemViewModel : BaseViewModel
    {
        private BaseObjectVersionItemViewModel _selectedSwVersionItem;
        protected BaseObjectSwModel _vo;

        public BaseObjectSwModel RawModel => _vo;

        [Bindable(true)]
        public BaseObjectVersionItemViewModel SelectedSwVersionItemForEditting
        {
            get
            {
                return _selectedSwVersionItem;
            }
            set
            {
                _selectedSwVersionItem = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string StringId
        {
            get
            {
                return _vo.StringId;
            }
            set
            {
                _vo.StringId = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string Name
        {
            get => _vo.Name;
            set
            {
                _vo.Name = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string Author
        {
            get => _vo.Author;
            set
            {
                _vo.Author = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string Description
        {
            get => _vo.Description;
            set
            {
                _vo.Description = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string ProjectUrl
        {
            get => _vo.ProjectURL;
            set
            {
                _vo.ProjectURL = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string IconSource
        {
            get => _vo.IconSource;
            set
            {
                _vo.IconSource = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public bool IsPreReleased
        {
            get => _vo.IsPreReleased;
            set
            {
                _vo.IsPreReleased = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public bool IsRequireLatestVersionToRun
        {
            get => _vo.IsRequireLatestVersionToRun;
            set
            {
                _vo.IsRequireLatestVersionToRun = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public bool IsAuthenticated
        {
            get => _vo.IsAuthenticated;
            set
            {
                _vo.IsAuthenticated = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public int Downloads
        {
            get => _vo.Downloads;
        }

        public ObservableCollection<BaseObjectVersionItemViewModel> VersionSource { get; set; }
            = new ObservableCollection<BaseObjectVersionItemViewModel>();


        public BaseObjectSwItemViewModel(BaseObjectSwModel baseModel)
        {
            if (baseModel != null)
            {
                _vo = baseModel;
                InitOtherPropertiesOfItem();
            }
            else
            {
                _vo = new BaseObjectSwModel();
            }
        }

        private async void InitOtherPropertiesOfItem()
        {
            await DoInitOtherPropertiesTask();
        }

        protected abstract Task DoInitOtherPropertiesTask();
        
    }
}
