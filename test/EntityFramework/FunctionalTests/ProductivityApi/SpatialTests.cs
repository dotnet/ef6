namespace ProductivityApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data;
    using System.Data.Entity.Core.Common;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Spatial;
    using System.Globalization;
    using System.Linq;
    using Xunit;

    /// <summary>
    /// Tests for spatial and DbContext.
    /// </summary>
    public class SpatialTests : FunctionalTestBase
    {
        #region Infrastructure/setup

        private static readonly string ConnectionString;

        static SpatialTests()
        {
            const string prefix = "FunctionalTests.ProductivityApi.SpatialTvfsModel.";
            ResourceUtilities.CopyEmbeddedResourcesToCurrentDir(typeof(SpatialTests).Assembly, prefix, /*overwrite*/ true,
                                                              "226644SpatialModel.csdl", "226644SpatialModel.msl",
                                                              "226644SpatialModel.ssdl");

            const string baseConnectionString =
                @"metadata=.\226644SpatialModel.csdl|.\226644SpatialModel.ssdl|.\226644SpatialModel.msl;
                                                  provider=System.Data.SqlClient;provider connection string='{0}'";
            ConnectionString = String.Format(CultureInfo.InvariantCulture, baseConnectionString,
                                             SimpleConnectionString<SpatialNorthwindContext>());
        }

        #endregion

        #region Tests for TVFs with spatial types

        // Dev11 226644
        [Fact]
        public void DbQuery_with_TVFs_mapped_to_context_instance_methods_involving_spatial_types_works()
        {
            using (var context = new SpatialNorthwindContext(ConnectionString))
            {
                var suppliers = (from x in context.Suppliers
                                 select
                                     context.SuppliersWithinRange(1000,
                                                                  DbGeography.FromText("POINT(-122.335576 47.610676)",
                                                                                       4326))).ToList();

                Assert.Equal(16, suppliers.Count);
            }
        }

        [Fact]
        public void DbQuery_with_TVFs_mapped_to_static_methods_involving_spatial_types_works()
        {
            using (var context = new SpatialNorthwindContext(ConnectionString))
            {
                var suppliers = (from x in context.Suppliers
                                 select
                                     SpatialNorthwindContext.StaticSuppliersWithinRange(1000,
                                                                                        DbGeography.FromText(
                                                                                            "POINT(-122.335576 47.610676)",
                                                                                            4326))).ToList();

                Assert.Equal(16, suppliers.Count);
            }
        }

        [EdmFunction("SpatialNorthwindContext", "SuppliersWithinRange")]
        public static IQueryable<SupplierWithLocation> ArbitrarySuppliersWithinRange(int? miles, DbGeography location)
        {
            throw new NotImplementedException("Should not be called by client code.");
        }

        [Fact]
        public void DbQuery_with_TVFs_mapped_to_arbitrary_instance_methods_involving_spatial_types_works()
        {
            using (var context = new SpatialNorthwindContext(ConnectionString))
            {
                var suppliers = (from x in context.Suppliers
                                 select
                                     ArbitrarySuppliersWithinRange(1000,
                                                                   DbGeography.FromText("POINT(-122.335576 47.610676)",
                                                                                        4326))).ToList();

                Assert.Equal(16, suppliers.Count);
            }
        }

        [Fact]
        public void DbQuery_SelectMany_with_TVFs_and_spatial_types_works()
        {
            using (var context = new SpatialNorthwindContext(ConnectionString))
            {
                var results =
                    (from s1 in
                         context.SuppliersWithinRange(1000, DbGeography.FromText("POINT(-122.335576 47.610676)", 4326))
                     from s2 in
                         context.SuppliersWithinRange(1000, DbGeography.FromText("POINT(-122.335576 47.610676)", 4326))
                     where s1.Name == s2.Name
                     select new { s1, s2 }).ToList();

                Assert.Equal(16, results.Count);
            }
        }

        // Dev11 264624
        [Fact]
        public void DbQuery_SelectMany_with_TVFs_and_spatial_types_using_Point_in_function_import_works()
        {
            using (var context = new SpatialNorthwindContext(ConnectionString))
            {
                context.Database.Initialize(force: false);

                var results =
                    (from s1 in
                         context.SuppliersWithinRangeUsingPoint(1000,
                                                                DbGeography.FromText("POINT(-122.335576 47.610676)",
                                                                                     4326))
                     from s2 in
                         context.SuppliersWithinRangeUsingPoint(1000,
                                                                DbGeography.FromText("POINT(-122.335576 47.610676)",
                                                                                     4326))
                     where s1.Name == s2.Name
                     select new { s1, s2 }).ToList();

                Assert.Equal(16, results.Count);
            }
        }

        [Fact]
        public void DbQuery_SelectMany_with_TVFs_and_spatial_types_using_Point_and_Point_return_in_function_import_works()
        {
            using (var context = new SpatialNorthwindContext(ConnectionString))
            {
                context.Database.Initialize(force: false);

                var results =
                    (from s1 in
                         context.SupplierLocationsWithinRange(1000,
                                                              DbGeography.FromText("POINT(-122.335576 47.610676)", 4326))
                     from s2 in
                         context.SupplierLocationsWithinRange(1000,
                                                              DbGeography.FromText("POINT(-122.335576 47.610676)", 4326))
                     select new { s1, s2 }).ToList();

                Assert.Equal(256, results.Count);
            }
        }

        #endregion

        #region Tests for strongly typed spatial values for type construction (Dev11 254822)

        [Fact]
        public void Can_query_for_strongly_typed_geographic_point_using_type_construction()
        {
            using (var context = new SpatialNorthwindContext(ConnectionString))
            {
                var query =
                    @"select value ProductivityApiTests.SupplierWithLocation(-77, N'MyName', Edm.GeographyFromText(""POINT(-122.335576 47.610676)"")) 
                              from [SpatialNorthwindContext].[Suppliers] as SupplierWithLocation";
                Assert.Equal(16, TestWithReader(context, query, r => Assert.IsType<DbGeography>(r.GetValue(2))));
            }
        }

        [Fact]
        public void Can_query_for_strongly_typed_geometric_point_using_type_construction()
        {
            using (var context = new SpatialNorthwindContext(ConnectionString))
            {
                var query =
                    @"select value ProductivityApiTests.WidgetWithGeometry(-77, N'MyName', Edm.GeometryFromText(""POINT(-122.335576 47.610676)""), ProductivityApiTests.ComplexWithGeometry(N'A', Edm.GeometryFromText(""POINT(-122.335576 47.610676)""))) 
                              from [SpatialNorthwindContext].[Widgets] as WidgetWithGeometry";
                Assert.Equal(4, TestWithReader(context, query, r =>
                                                               {
                                                                   Assert.Equal(-77, r.GetInt32(0));
                                                                   Assert.IsType<DbGeometry>(r.GetValue(2));
                                                                   var nestedRecord = r.GetDataRecord(3);
                                                                   Assert.IsType<DbGeometry>(nestedRecord.GetValue(1));
                                                               }));
            }
        }

        [Fact]
        public void Can_query_for_strongly_typed_geometric_point_using_complex_type_type_construction()
        {
            using (var context = new SpatialNorthwindContext(ConnectionString))
            {
                var query =
                    @"select value ProductivityApiTests.ComplexWithGeometry(N'A', Edm.GeometryFromText(""POINT(-122.335576 47.610676)"")) 
                              from [SpatialNorthwindContext].[Widgets] as WidgetWithGeometry";
                Assert.Equal(4, TestWithReader(context, query, r =>
                                                               {
                                                                   Assert.Equal("A", r.GetString(0));
                                                                   Assert.IsType<DbGeometry>(r.GetValue(1));
                                                               }));
            }
        }

        private int TestWithReader(DbContext context, string query, Action<EntityDataReader> test)
        {
            var entityConnection = (EntityConnection)((IObjectContextAdapter)context).ObjectContext.Connection;
            using (var command = entityConnection.CreateCommand())
            {
                command.CommandText = query;
                entityConnection.Open();
                int count = 0;
                var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                while (reader.Read())
                {
                    count++;
                    test(reader);
                }
                return count;
            }
        }

        #endregion

        #region Tests for materializing spatial types using eSQL

        // Dev11 260655
        [Fact]
        public void
            Can_materialize_record_containing_geometric_types_and_get_names_of_the_types_without_null_arg_exception()
        {
            using (var context = new SpatialNorthwindContext(ConnectionString))
            {
                var query =
                    @"(select o.[AGeometricLineString], 
                                      i.[AGeometricPolygon]
                               from [SpatialNorthwindContext].[LineStringWidgets] as [o]
                               left outer join [SpatialNorthwindContext].[PolygonWidgets] as [i]
                               on Edm.SpatialCrosses(o.[AGeometricLineString],i.[AGeometricPolygon]))";

                var results = ExecuteESqlQuery(context, query);

                Assert.Equal(2, results.Count);
                foreach (var result in results)
                {
                    Assert.Equal("AGeometricLineString", result.GetName(0)); // GetName would throw
                    Assert.Equal("AGeometricPolygon", result.GetName(1));
                    Assert.Same(typeof(DbGeometry), result.GetFieldType(0));
                    Assert.Same(typeof(DbGeometry), result.GetFieldType(1));
                }
            }
        }

        [Fact]
        public void
            Can_materialize_record_containing_geographic_types_and_get_names_of_the_types_without_null_arg_exception()
        {
            using (var context = new SpatialNorthwindContext(ConnectionString))
            {
                var query =
                    @"select o.[Location]
                              from [SpatialNorthwindContext].[Suppliers] as [o]";

                var results = ExecuteESqlQuery(context, query);

                Assert.Equal(16, results.Count);
                foreach (var result in results)
                {
                    Assert.Equal("Location", result.GetName(0)); // GetName would throw
                    Assert.Same(typeof(DbGeography), result.GetFieldType(0));
                }
            }
        }

        private List<DbDataRecord> ExecuteESqlQuery(DbContext context, string query)
        {
            var objectContext = ((IObjectContextAdapter)context).ObjectContext;
            objectContext.MetadataWorkspace.LoadFromAssembly(typeof(WidgetWithLineString).Assembly);

            return objectContext.CreateQuery<DbDataRecord>(query).ToList();
        }

        #endregion
    }
}