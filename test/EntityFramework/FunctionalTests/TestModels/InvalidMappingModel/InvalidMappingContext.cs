// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace ProductivityApiTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;

    public class InvalidProduct
    {
        public float? Id { get; set; }
        public string Name { get; set; }
    }

    public class InvalidDerivedProductDependent : InvalidProduct
    {
        public string SalePrice { get; set; }
        public Guid Quantity { get; set; }
        public Guid? PrincipalId { get; set; }

        [ForeignKey("PrincipalId")]
        public InvalidPrincipal PrincipalNavigation { get; set; }
    }

    public class InvalidPrincipal
    {
        public Guid Id { get; set; }
        public byte[] Description { get; set; }
        public ICollection<InvalidDerivedProductDependent> DerivedProductDependentNavigation { get; set; }
    }


    /// <summary>
    /// Validation of this model will fail. Currently this happens after the model has been
    /// built when it is passed to WriteEdmx or Compile. In the future the pipeline may be fixed
    /// such that it fails when Build is called.
    /// </summary>
    public class InvalidMappingContext : DbContext
    {
        public InvalidMappingContext()
        {
            Database.SetInitializer<InvalidMappingContext>(null);
        }

        public DbSet<InvalidProduct> Products { get; set; }
        public DbSet<InvalidPrincipal> Principals { get; set; }

        protected override void OnModelCreating(DbModelBuilder builder)
        {
            builder.Entity<InvalidProduct>();
            builder.Entity<InvalidPrincipal>();
            builder.Entity<InvalidProduct>()
                .Map(mapping => mapping.Requires("PrincipalId").HasValue(null))
                .Map<InvalidDerivedProductDependent>(mapping => mapping.Requires(e => e.PrincipalId).HasValue())
                .ToTable("Product");
            builder.Entity<InvalidDerivedProductDependent>().HasOptional(e => e.PrincipalNavigation).WithMany(e => e.DerivedProductDependentNavigation);
            builder.Entity<InvalidPrincipal>().HasMany(e => e.DerivedProductDependentNavigation).WithOptional(e => e.PrincipalNavigation);
            builder.Entity<InvalidProduct>().Property(p => p.Id).HasColumnType("real");
            builder.Entity<InvalidProduct>().Property(p => p.Name).IsRequired().HasColumnType("xml");
            builder.Entity<InvalidDerivedProductDependent>().Property(p => p.SalePrice).IsRequired().HasColumnType("char").HasMaxLength(20).IsFixedLength();
            builder.Entity<InvalidDerivedProductDependent>().Property(p => p.Quantity).HasColumnType("uniqueidentifier");
            builder.Entity<InvalidDerivedProductDependent>().Property(p => p.PrincipalId).HasColumnType("uniqueidentifier");
            builder.Entity<InvalidPrincipal>().Property(p => p.Id).HasColumnType("uniqueidentifier");
            builder.Entity<InvalidPrincipal>().Property(p => p.Description).HasColumnType("binary").HasMaxLength(10).IsFixedLength();
        }
    }
}