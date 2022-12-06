using cyber_server.definition;
using cyber_server.implements.plugin_manager;
using cyber_server.models;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.view_models.list_view_item
{
    public abstract class BaseObjectSwItemViewModel : BaseViewModel
    {
        private BaseObjectVersionItemViewModel _selectedSwVersionItem;
        public abstract BaseObjectSwModel RawModel { get; }

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
                return RawModel.StringId;
            }
            set
            {
                RawModel.StringId = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string Name
        {
            get => RawModel.Name;
            set
            {
                RawModel.Name = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string Author
        {
            get => RawModel.Author;
            set
            {
                RawModel.Author = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string Description
        {
            get => RawModel.Description;
            set
            {
                RawModel.Description = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string ProjectUrl
        {
            get => RawModel.ProjectURL;
            set
            {
                RawModel.ProjectURL = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string IconSource
        {
            get => RawModel.IconSource;
            set
            {
                RawModel.IconSource = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public bool IsPreReleased
        {
            get => RawModel.IsPreReleased;
            set
            {
                RawModel.IsPreReleased = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public bool IsRequireLatestVersionToRun
        {
            get => RawModel.IsRequireLatestVersionToRun;
            set
            {
                RawModel.IsRequireLatestVersionToRun = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public bool IsAuthenticated
        {
            get => RawModel.IsAuthenticated;
            set
            {
                RawModel.IsAuthenticated = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public int Downloads
        {
            get => RawModel.Downloads;
        }

        public ObservableCollection<BaseObjectVersionItemViewModel> VersionSource { get; set; }
            = new ObservableCollection<BaseObjectVersionItemViewModel>();


        public BaseObjectSwItemViewModel(BaseObjectSwModel model)
        {
            InitOtherPropertiesOfItem(model);
        }

        public async Task<BaseObjectSwModel> BuildNewSwModel()
        {
            if (!IsNewModel()) return null;

            foreach (var version in VersionSource)
            {
                var versionModel = await version.BuildNewVersionModel();
                if(versionModel != null)
                {
                    AddNewVersionModelToRawModel(versionModel);
                }
            }
            await BuildSwIconSource();
            return RawModel;
        }

        protected abstract Task<string> BuildSwIconSource();

        protected abstract bool IsNewModel();

        protected abstract void AddNewVersionModelToRawModel(BaseObjectVersionModel versionModel);

        private async void InitOtherPropertiesOfItem(BaseObjectSwModel model)
        {
            await DoInitOtherPropertiesTask(model);
        }

        protected abstract Task DoInitOtherPropertiesTask(BaseObjectSwModel model);

    }
}
