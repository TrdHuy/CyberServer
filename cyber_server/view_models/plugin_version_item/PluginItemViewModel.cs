using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.view_models.plugin_version_item
{
    public class PluginItemViewModel : BaseViewModel
    {
        private Plugin _baseModel;
        private double _rates;
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

        public int Downloads
        {
            get => _baseModel.Downloads;
        }

        public double Rates
        {
            get => _rates;
        }

        public ObservableCollection<PluginVersionItemViewModel> VersionSource { get; set; }
            = new ObservableCollection<PluginVersionItemViewModel>();
        public PluginItemViewModel(Plugin baseModel)
        {
            _baseModel = baseModel;
            if (baseModel.Votes.Count != 0)
            {
                _rates = baseModel.Votes.Average(v => v.Stars);
            }
            foreach(var pluginVerison in baseModel.PluginVersions)
            {
                VersionSource.Add(new PluginVersionItemViewModel(pluginVerison));
            }
        }
    }
}
