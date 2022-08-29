using cyber_server.view_models.plugin_version_item;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.view_models.windows
{
    internal class ModifyPluginWindowViewModel : BaseViewModel
    {
        private Plugin _rawModel;
        private ObservableCollection<PluginVersionItemViewModel> _versionSource = new ObservableCollection<PluginVersionItemViewModel>();
        private string _projectURL;
        private string _name;
        private string _key;
        private string _author;
        private string _des;
        private string _iconSource;
        private bool _isAuthenticated;
        private int _selectedVersionIndex;
        public Plugin RawModel => _rawModel;

        [Bindable(true)]
        public int SelectedVersionIndex
        {
            get => _selectedVersionIndex;
            set
            {
                _selectedVersionIndex = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public ObservableCollection<PluginVersionItemViewModel> VersionSource
        {
            get => _versionSource;
            set
            {
                _versionSource = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string ProjectURL
        {
            get => _projectURL;
            set
            {
                _projectURL = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set
            {
                _isAuthenticated = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string IconSource
        {
            get => _iconSource;
            set
            {
                _iconSource = value;
                InvalidateOwn();
            }
        }


        [Bindable(true)]
        public string Description
        {
            get => _des;
            set
            {
                _des = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string Author
        {
            get => _author;
            set
            {
                _author = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string PluginName
        {
            get => _name;
            set
            {
                _name = value;
                InvalidateOwn();
            }
        }

        [Bindable(true)]
        public string PluginKey
        {
            get => _key;
            set
            {
                _key = value;
                InvalidateOwn();
            }
        }


        public ModifyPluginWindowViewModel()
        {

        }

        public void SetRawModel(Plugin rawModel)
        {
            _rawModel = rawModel;
            PluginName = rawModel.Name;
            Author = rawModel.Author;
            PluginKey = rawModel.StringId;
            IsAuthenticated = rawModel.IsAuthenticated;
            IconSource = rawModel.IconSource;
            Description = rawModel.Description;
            ProjectURL = rawModel.ProjectURL;

            _versionSource.Clear();

            InitVersionSource();
            
        }

        private async void InitVersionSource()
        {
            await DoTaskInitVersionSource();
        }

        private async Task DoTaskInitVersionSource()
        {
            await Task.Delay(100);
            var inOrderSource = _rawModel.PluginVersions.OrderByDescending(v => Version.Parse(v.Version));
            foreach (var version in inOrderSource)
            {
                _versionSource.Add(new PluginVersionItemViewModel(version));
            }

            if (_versionSource.Count > 0)
            {
                SelectedVersionIndex = 0;
            }
        }
    }
}
