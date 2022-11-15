using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.view_models.tool_item
{
    public class ToolItemViewModel : BaseViewModel
    {
        private ToolVersionItemViewModel _selectedToolVersionItem;
        private Tool _baseModel;

        public Tool RawModel => _baseModel;

        [Bindable(true)]
        public ToolVersionItemViewModel SelectedToolVersionItemForEditting
        {
            get
            {
                return _selectedToolVersionItem;
            }
            set
            {
                _selectedToolVersionItem = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string StringId
        {
            get
            {
                return _baseModel.StringId;
            }
            set
            {
                _baseModel.StringId = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string Name
        {
            get => _baseModel.Name;
            set
            {
                _baseModel.Name = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string Author
        {
            get => _baseModel.Author;
            set
            {
                _baseModel.Author = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string Description
        {
            get => _baseModel.Description;
            set
            {
                _baseModel.Description = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string ProjectUrl
        {
            get => _baseModel.ProjectURL;
            set
            {
                _baseModel.ProjectURL = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string IconSource
        {
            get => _baseModel.IconSource;
            set
            {
                _baseModel.IconSource = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public bool IsPreReleased
        {
            get => _baseModel.IsPreReleased;
            set
            {
                _baseModel.IsPreReleased = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public bool IsRequireLatestVersionToRun
        {
            get => _baseModel.IsRequireLatestVersionToRun;
            set
            {
                _baseModel.IsRequireLatestVersionToRun = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public bool IsAuthenticated
        {
            get => _baseModel.IsAuthenticated;
            set
            {
                _baseModel.IsAuthenticated = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public int Downloads
        {
            get => _baseModel.Downloads;
        }

        public ObservableCollection<ToolVersionItemViewModel> VersionSource { get; set; }
            = new ObservableCollection<ToolVersionItemViewModel>();


        public ToolItemViewModel(Tool baseModel)
        {
            if (baseModel != null)
            {
                _baseModel = baseModel;
                InitOtherPropertiesOfPluginItem();
            }
            else
            {
                _baseModel = new Tool();
            }
        }

        private async void InitOtherPropertiesOfPluginItem()
        {
            await DoInitOtherPropertiesTask();
        }

        private async Task DoInitOtherPropertiesTask()
        {
            await Task.Delay(100);

            foreach (var pluginVerison in
                _baseModel.ToolVersions.OrderByDescending(v => Version.Parse(v.Version)))
            {
                VersionSource.Add(new ToolVersionItemViewModel(pluginVerison));
            }
        }
    }
}
