// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Hierarchy;
    using System.Data.Entity.Infrastructure;
    using System.Linq;

    public class SupplierWithHierarchyId
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public HierarchyId Path { get; set; } 
    }

    public class WidgetWithHierarchyId
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public HierarchyId SomeHierarchyId { get; set; }
        public ComplexWithHierarchyId Complex { get; set; }
    }

    public class ComplexWithHierarchyId
    {
        public string NotHierarchyId { get; set; }
        public HierarchyId SomeMoreHierarchyId { get; set; }
    }

    public class HierarchyIdNorthwindContext : DbContext
    {
        public HierarchyIdNorthwindContext(string connectionString)
            : base(connectionString)
        {
            Database.SetInitializer(new HierarchyIdNorthwindInitializer());
        }

        public DbSet<SupplierWithHierarchyId> Suppliers { get; set; }
        public DbSet<WidgetWithHierarchyId> Widgets { get; set; }

        [DbFunction("HierarchyIdNorthwindContext", "SuppliersWithinRange")]
        public virtual IQueryable<SupplierWithHierarchyId> SuppliersWithinRange(HierarchyId path1, HierarchyId path2)
        {
            var objectContext = ((IObjectContextAdapter)this).ObjectContext;
            objectContext.MetadataWorkspace.LoadFromAssembly(typeof(SupplierWithHierarchyId).Assembly);

            return objectContext.CreateQuery<SupplierWithHierarchyId>(
                "[HierarchyIdNorthwindContext].[SuppliersWithinRange](@path1, @path2)",
                new ObjectParameter("path1", path1),
                new ObjectParameter("path2", path2));
        }

        [DbFunction("HierarchyIdNorthwindContext", "SuppliersWithinRange")]
        public static IQueryable<SupplierWithHierarchyId> StaticSuppliersWithinRange(HierarchyId path1, HierarchyId path2)
        {
            throw new NotImplementedException("Should not be called by client code.");
        }

        [DbFunction("HierarchyIdNorthwindContext", "SupplierHierarchyIdsWithinRange")]
        public virtual IQueryable<HierarchyId> SupplierHierarchyIdsWithinRange(HierarchyId path1, HierarchyId path2)
        {
            var objectContext = ((IObjectContextAdapter)this).ObjectContext;
            objectContext.MetadataWorkspace.LoadFromAssembly(typeof(SupplierWithHierarchyId).Assembly);

            return objectContext.CreateQuery<HierarchyId>(
                "[HierarchyIdNorthwindContext].[SupplierHierarchyIdsWithinRange](@path1, @path2)",
                new ObjectParameter("path1", path1),
                new ObjectParameter("path2", path2));
        }
    }
}
