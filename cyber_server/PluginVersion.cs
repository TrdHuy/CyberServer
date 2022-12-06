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
    using cyber_server.models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    public partial class PluginVersion : BaseObjectVersionModel
    {
        public PluginVersion()
        {

        }

        public int PluginId { get; set; }
        public string MainClassName { get; set; }

        [JsonIgnore]
        public virtual Plugin Plugin { get; set; }

        public override object Clone()
        {
            byte[] newFile = new byte[File.Length];
            Array.Copy(this.File, newFile, File.Length);
            return new PluginVersion()
            {
                PluginId = -1,
                MainClassName = MainClassName,
                Version = Version,
                RawLength = RawLength,
                CompressLength = CompressLength,
                DatePublished = DatePublished,
                Description = Description,
                ExecutePath = ExecutePath,
                FileName = FileName,
                File = newFile,
                VersionId = -1,
                Plugin = null,
            };
        }

        public void SetMainClassName(string mainClassName)
        {
            MainClassName = mainClassName;
        }
    }
}
