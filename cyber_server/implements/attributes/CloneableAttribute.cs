using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.implements.attributes
{
    public enum CloneOption
    {
        Manual = 1,
        Collection = 2,
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class CloneableAttribute : Attribute
    {
        private object _defaultValue;
        public bool IsCloneable { get; private set; }
        public CloneOption CloneOption { get; private set; }
        public Type CollectionType { get; private set; }
        public object DefaultValue
        {
            get
            {
                return _defaultValue;
            }
        }

        public CloneableAttribute(bool isCloneable
            , object defaultValue = null
            , CloneOption cloneOption = CloneOption.Manual
            , Type collectionType = null)
        {
            this.IsCloneable = isCloneable;
            this._defaultValue = defaultValue;
            this.CloneOption = cloneOption;
            this.CollectionType = collectionType;
            if (cloneOption == CloneOption.Collection && collectionType == null)
                throw new ArgumentNullException("Cannot use CloneOption.Collection with null collection type");
        }


    }
}
