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
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    
    public partial class ToolVersion
    {
        public int VersionId { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public int ToolId { get; set; }
        public System.DateTime DatePublished { get; set; }
        public string FolderPath { get; set; }
        public string FileName { get; set; }
        public string ExecutePath { get; set; }
    
        [JsonIgnore]
        public virtual Tool Tool { get; set; }
    }
}
