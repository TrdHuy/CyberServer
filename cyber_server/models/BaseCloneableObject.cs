using cyber_server.implements.attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.models
{
    public abstract class BaseCloneableObject : ICloneable, IJsonCloneable
    {
        public object Clone()
        {
            var cloneResult = GenerateNewCloneObjectInstance() ;
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(cloneResult);
            foreach (PropertyDescriptor property in properties)
            {
                var attr = property.Attributes;
                var clattr = attr[typeof(CloneableAttribute)] as CloneableAttribute;
                if (clattr != null && clattr.IsCloneable)
                {
                    if (clattr.CloneOption == CloneOption.Manual)
                    {
                        // clone an array
                        var cloneValue = (this.GetType().GetProperty(property.Name)?.GetValue(this) as ICloneable)?.Clone()
                            ?? this.GetType().GetProperty(property.Name)?.GetValue(this);

                        property.SetValue(cloneResult, cloneValue);
                    }
                    else if (clattr.CloneOption == CloneOption.Collection && clattr.CollectionType != null)
                    {
                        var currentValue = this.GetType().GetProperty(property.Name)?.GetValue(this) as System.Collections.IEnumerable;
                        var collectionType = typeof(List<>).MakeGenericType(clattr.CollectionType);
                        var newCollection = Activator.CreateInstance(collectionType);
                        MethodInfo addMethod = collectionType.GetMethod("Add");
                        if (currentValue != null)
                        {
                            foreach (ICloneable item in currentValue)
                            {
                                object[] parametersArray = new object[] { item.Clone() };
                                addMethod?.Invoke(newCollection, parametersArray);
                            }
                        }
                        property.SetValue(cloneResult, newCollection);
                    }
                }
                else if (clattr != null && !clattr.IsCloneable)
                {
                    var defaultValue = clattr.DefaultValue;
                    property.SetValue(cloneResult, defaultValue);
                }
            }

            return cloneResult;
        }

        protected abstract object GenerateNewCloneObjectInstance();

        public object JsonClone()
        {
            var cloneResult = GenerateNewCloneObjectInstance();
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(cloneResult);
            foreach (PropertyDescriptor property in properties)
            {
                var attr = property.Attributes;
                var clattr = attr[typeof(CloneableAttribute)] as CloneableAttribute;
                var jsonIAttr = attr[typeof(JsonIgnoreAttribute)];
                if (jsonIAttr == null)
                {
                    if (clattr != null && clattr.IsCloneable)
                    {
                        if (clattr.CloneOption == CloneOption.Manual)
                        {
                            // clone an array
                            var cloneValue = (this.GetType().GetProperty(property.Name)?.GetValue(this) as ICloneable)?.Clone()
                                ?? this.GetType().GetProperty(property.Name)?.GetValue(this);

                            property.SetValue(cloneResult, cloneValue);
                        }
                        else if (clattr.CloneOption == CloneOption.Collection && clattr.CollectionType != null)
                        {
                            var currentValue = this.GetType().GetProperty(property.Name)?.GetValue(this) as System.Collections.IEnumerable;
                            var collectionType = typeof(List<>).MakeGenericType(clattr.CollectionType);
                            var newCollection = Activator.CreateInstance(collectionType);
                            MethodInfo addMethod = collectionType.GetMethod("Add");
                            if (currentValue != null)
                            {
                                foreach (IJsonCloneable item in currentValue)
                                {
                                    object[] parametersArray = new object[] { item.JsonClone() };
                                    addMethod?.Invoke(newCollection, parametersArray);
                                }
                            }
                            property.SetValue(cloneResult, newCollection);
                        }
                    }
                    else if (clattr != null && !clattr.IsCloneable)
                    {
                        var defaultValue = clattr.DefaultValue;
                        property.SetValue(cloneResult, defaultValue);
                    }
                }
            }
            return cloneResult;
        }
    }

    public interface IJsonCloneable
    {
        object JsonClone();
    }
}
