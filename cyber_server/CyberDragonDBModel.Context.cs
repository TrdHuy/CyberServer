﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class CyberDragonDbContext : DbContext
    {
        public CyberDragonDbContext()
            : base("name=CyberDragonDbContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Plugin> Plugins { get; set; }
        public virtual DbSet<PluginVersion> PluginVersions { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<Vote> Votes { get; set; }
    }
}
