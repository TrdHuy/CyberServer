//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace cyber_server
{
    using System;
    using System.Collections.Generic;
    
    public partial class Tag
    {
        public string Content { get; set; }
        public int TagId { get; set; }
        public int PluginId { get; set; }
    
        public virtual Plugin Plugin { get; set; }
    }
}
