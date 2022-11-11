using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public string Name
        {
            get => _baseModel.Name;
            set
            {
                _baseModel.Name = value;
                InvalidateOwn();
            }
        }

        public string Author
        {
            get => _baseModel.Author;
            set
            {
                _baseModel.Author = value;
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

        public string ProjectUrl
        {
            get => _baseModel.ProjectURL;
            set
            {
                _baseModel.ProjectURL = value;
                InvalidateOwn();
            }
        }

        public string IconSource
        {
            get => _baseModel.IconSource;
            set
            {
                _baseModel.IconSource = value;
                InvalidateOwn();
            }
        }

        public bool IsPreReleased
        {
            get => _baseModel.IsPreReleased;
            set
            {
                _baseModel.IsPreReleased = value;
                InvalidateOwn();
            }
        }

        public bool IsAuthenticated
        {
            get => _baseModel.IsAuthenticated;
            set
            {
                _baseModel.IsAuthenticated = value;
                InvalidateOwn();
            }
        }

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
