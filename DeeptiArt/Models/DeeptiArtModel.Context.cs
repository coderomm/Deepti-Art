﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DeeptiArt.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class dbdeeptiartsEntities : DbContext
    {
        public dbdeeptiartsEntities()
            : base("name=dbdeeptiartsEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<AboutDetailsTbl> AboutDetailsTbls { get; set; }
        public DbSet<FrameTbl> FrameTbls { get; set; }
        public DbSet<OurTeamTbl> OurTeamTbls { get; set; }
        public DbSet<RegisteredUsersTbl> RegisteredUsersTbls { get; set; }
        public DbSet<AdminTbl> AdminTbls { get; set; }
        public DbSet<MainCategoryTbl> MainCategoryTbls { get; set; }
        public DbSet<ProductTbl> ProductTbls { get; set; }
        public DbSet<SubCategoryTbl> SubCategoryTbls { get; set; }
        public DbSet<ReviewTable> ReviewTables { get; set; }
        public DbSet<CartTbl> CartTbls { get; set; }
        public DbSet<ContactEnquiryTbl> ContactEnquiryTbls { get; set; }
        public DbSet<city> cities { get; set; }
        public DbSet<district> districts { get; set; }
        public DbSet<state> states { get; set; }
        public DbSet<WishlistTbl> WishlistTbls { get; set; }
        public DbSet<BillingDetailsTbl> BillingDetailsTbls { get; set; }
        public DbSet<OrderDetailsTbl> OrderDetailsTbls { get; set; }
        public DbSet<OrderTbl> OrderTbls { get; set; }
        public DbSet<ShippingDetailsTbl> ShippingDetailsTbls { get; set; }
    }
}
