namespace ProductivityApiTests
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.Spatial;

    public class SpatialNorthwindInitializer : DropCreateDatabaseAlways<SpatialNorthwindContext>
    {
        protected override void Seed(SpatialNorthwindContext context)
        {
            context.Database.ExecuteSqlCommand(@"

                CREATE FUNCTION [dbo].[fx_SuppliersWithinRange]
                (	
	                @miles int, 
	                @location geography
                )
                RETURNS TABLE 
                AS
                RETURN 
                (
	                SELECT [Id],[Name],[Location]
	                FROM [ProductivityApiTests.SpatialNorthwindContext].[dbo].[SupplierWithLocations] as supplier
	                Where supplier.Location.STIntersects(@location.STBuffer(@miles * 1609.344)) = 1
                )"
                );

            context.Database.ExecuteSqlCommand(@"

                CREATE FUNCTION [dbo].[fx_SupplierLocationsWithinRange]
                (	
	                @miles int, 
	                @location geography
                )
                RETURNS TABLE 
                AS
                RETURN 
                (
	                SELECT [Location]
	                FROM [ProductivityApiTests.SpatialNorthwindContext].[dbo].[SupplierWithLocations] as supplier
	                Where supplier.Location.STIntersects(@location.STBuffer(@miles * 1609.344)) = 1
                )"
                );

            new List<SupplierWithLocation>
            {
                new SupplierWithLocation { Name = "Supplier1", Location = DbGeography.FromText("POINT(-122.31946 47.625112)") },
                new SupplierWithLocation { Name = "Supplier2", Location = DbGeography.FromText("POINT(-122.296623 47.640405)") },
                new SupplierWithLocation { Name = "Supplier3", Location = DbGeography.FromText("POINT(-122.334571 47.604009)") },
                new SupplierWithLocation { Name = "Supplier4", Location = DbGeography.FromText("POINT(-122.336124 47.610267)") },
                new SupplierWithLocation { Name = "Supplier5", Location = DbGeography.FromText("POINT(-122.338711 47.610753)") },
                new SupplierWithLocation { Name = "Supplier6", Location = DbGeography.FromText("POINT(-122.335576 47.610676)") },
                new SupplierWithLocation { Name = "Supplier7", Location = DbGeography.FromText("POINT(-122.349755 47.647494)") },
                new SupplierWithLocation { Name = "Supplier8", Location = DbGeography.FromText("POINT(-122.335197 47.646711)") },
                new SupplierWithLocation { Name = "Supplier9", Location = DbGeography.FromText("POINT(-122.304482 47.647295)") },
                new SupplierWithLocation { Name = "Supplier10", Location = DbGeography.FromText("POINT(-122.341529 47.611693)") },
                new SupplierWithLocation { Name = "Supplier11", Location = DbGeography.FromText("POINT(-122.352842 47.6186)") },
                new SupplierWithLocation { Name = "Supplier12", Location = DbGeography.FromText("POINT(-122.255949 47.549068)") },
                new SupplierWithLocation { Name = "Supplier13", Location = DbGeography.FromText("POINT(-122.349074 47.619589)") },
                new SupplierWithLocation { Name = "Supplier14", Location = DbGeography.FromText("POINT(-122.3381 47.612467)") },
                new SupplierWithLocation { Name = "Supplier15", Location = DbGeography.FromText("POINT(-122.317575 47.665229)") },
                new SupplierWithLocation { Name = "Supplier16", Location = DbGeography.FromText("POINT(-122.31249 47.632342)") },
            }.ForEach(s => context.Suppliers.Add(s));

            new List<WidgetWithGeometry>
            {
                new WidgetWithGeometry { Name = "Widget1", SomeGeometry = DbGeometry.FromText("POINT(-122.31946 47.625112)"), Complex = new ComplexWithGeometry { NotGeometry = "1", SomeMoreGeometry = DbGeometry.FromText("POINT(-122.31946 47.625112)") } },
                new WidgetWithGeometry { Name = "Widget2", SomeGeometry = DbGeometry.FromText("POINT(-122.296623 47.640405)"), Complex = new ComplexWithGeometry { NotGeometry = "1", SomeMoreGeometry = DbGeometry.FromText("POINT(-122.31946 47.625112)") } },
                new WidgetWithGeometry { Name = "Widget3", SomeGeometry = DbGeometry.FromText("POINT(-122.334571 47.604009)"), Complex = new ComplexWithGeometry { NotGeometry = "1", SomeMoreGeometry = DbGeometry.FromText("POINT(-122.31946 47.625112)") } },
                new WidgetWithGeometry { Name = "Widget4", SomeGeometry = DbGeometry.FromText("POINT(-122.336124 47.610267)"), Complex = new ComplexWithGeometry { NotGeometry = "1", SomeMoreGeometry = DbGeometry.FromText("POINT(-122.31946 47.625112)") } },
            }.ForEach(w => context.Widgets.Add(w));

            new List<WidgetWithLineString>
            {
                new WidgetWithLineString { AGeometricLineString = DbGeometry.FromText("LINESTRING (10 10, 100 100)") },
                new WidgetWithLineString { AGeometricLineString = DbGeometry.FromText("LINESTRING (100 100, 200 200)") },
            }.ForEach(w => context.LineStringWidgets.Add(w));

            new List<WidgetWithPolygon>
            {
                new WidgetWithPolygon { AGeometricPolygon = DbGeometry.FromText("POLYGON ((50 70, 100 70, 100 20, 50 20, 50 70))") },
                new WidgetWithPolygon { AGeometricPolygon = DbGeometry.FromText("POLYGON ((150 170, 200 170, 200 120, 150 120, 150 170))") },
            }.ForEach(w => context.PolygonWidgets.Add(w));
        }
    }
}

