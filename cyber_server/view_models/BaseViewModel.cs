using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.view_models
{

    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnChanged(object viewModel, string propertyName)
        {
            VerifyPropertyName(propertyName);
            PropertyChanged?.Invoke(viewModel, new PropertyChangedEventArgs(propertyName));
        }

        [Conditional("DEBUG")]
        private void VerifyPropertyName(string propertyName)
        {
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
                throw new ArgumentNullException(GetType().Name + " does not contain property: " + propertyName);
        }

        public void Invalidate(string propName)
        {
            OnChanged(this, propName);
        }

        public void InvalidateOwn([CallerMemberName()] string propName = "")
        {
            OnChanged(this, propName);
        }

        public void RefreshViewModel()
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this);

            foreach (PropertyDescriptor property in properties)
            {
                var attr = property.Attributes;

                if (attr[typeof(BindableAttribute)]?.Equals(BindableAttribute.Yes) ?? false)
                {
                    Invalidate(property.Name);
                }
            }
        }

    }
}
