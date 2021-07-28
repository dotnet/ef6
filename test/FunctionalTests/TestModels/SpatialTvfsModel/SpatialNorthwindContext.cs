// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Functionals.Utilities;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Spatial;
    using System.Linq;

    public class SupplierWithLocation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DbGeography Location { get; set; }
    }

    public class WidgetWithGeometry
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DbGeometry SomeGeometry { get; set; }
        public ComplexWithGeometry Complex { get; set; }
    }

    public class ComplexWithGeometry
    {
        public string NotGeometry { get; set; }
        public DbGeometry SomeMoreGeometry { get; set; }
    }

    public class WidgetWithLineString
    {
        public int Id { get; set; }
        public DbGeometry AGeometricLineString { get; set; }
    }

    public class WidgetWithPolygon
    {
        public int Id { get; set; }
        public DbGeometry AGeometricPolygon { get; set; }
    }

    public class SpatialNorthwindContext : DbContext
    {
        public SpatialNorthwindContext(string connectionString)
            : base(connectionString)
        {
            Database.SetInitializer(new SpatialNorthwindInitializer());
        }

        public DbSet<SupplierWithLocation> Suppliers { get; set; }
        public DbSet<WidgetWithGeometry> Widgets { get; set; }
        public DbSet<WidgetWithLineString> LineStringWidgets { get; set; }
        public DbSet<WidgetWithPolygon> PolygonWidgets { get; set; }

        [DbFunction("SpatialNorthwindContext", "SuppliersWithinRange")]
        public virtual IQueryable<SupplierWithLocation> SuppliersWithinRange(int? miles, DbGeography location)
        {
            var objectContext = ((IObjectContextAdapter)this).ObjectContext;
            objectContext.MetadataWorkspace.LoadFromAssembly(typeof(SupplierWithLocation).Assembly());

            return objectContext.CreateQuery<SupplierWithLocation>(
                "[SpatialNorthwindContext].[SuppliersWithinRange](@miles, @location)",
                new ObjectParameter("miles", miles),
                new ObjectParameter("location", location));
        }

        [DbFunction("SpatialNorthwindContext", "SuppliersWithinRange")]
        public static IQueryable<SupplierWithLocation> StaticSuppliersWithinRange(int? miles, DbGeography location)
        {
            throw new NotImplementedException("Should not be called by client code.");
        }

        [DbFunction("SpatialNorthwindContext", "SuppliersWithinRangeUsingPoint")]
        public virtual IQueryable<SupplierWithLocation> SuppliersWithinRangeUsingPoint(int? miles, DbGeography location)
        {
            var objectContext = ((IObjectContextAdapter)this).ObjectContext;
            objectContext.MetadataWorkspace.LoadFromAssembly(typeof(SupplierWithLocation).Assembly());

            return
                objectContext.CreateQuery<SupplierWithLocation>(
                    "[SpatialNorthwindContext].[SuppliersWithinRangeUsingPoint](@miles, @location)",
                    new ObjectParameter("miles", miles),
                    new ObjectParameter("location", location));
        }

        [DbFunction("SpatialNorthwindContext", "SupplierLocationsWithinRange")]
        public virtual IQueryable<DbGeography> SupplierLocationsWithinRange(int? miles, DbGeography location)
        {
            var objectContext = ((IObjectContextAdapter)this).ObjectContext;
            objectContext.MetadataWorkspace.LoadFromAssembly(typeof(SupplierWithLocation).Assembly());

            return objectContext.CreateQuery<DbGeography>(
                "[SpatialNorthwindContext].[SupplierLocationsWithinRange](@miles, @location)",
                new ObjectParameter("miles", miles),
                new ObjectParameter("location", location));
        }
    }
}
