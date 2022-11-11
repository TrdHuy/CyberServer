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

    public partial class Tool
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Tool()
        {
            this.ToolVersions = new HashSet<ToolVersion>();
        }

        public int ToolId { get; set; } = -1;
        public string StringId { get; set; } = "";
        public string Name { get; set; } = "";
        public string Author { get; set; } = "";
        public string Description { get; set; } = "";
        public string ProjectURL { get; set; } = "";
        public string IconSource { get; set; } = "";
        public bool IsAuthenticated { get; set; }
        public bool IsPreReleased { get; set; }
        public int Downloads { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ToolVersion> ToolVersions { get; set; }
    }
}
