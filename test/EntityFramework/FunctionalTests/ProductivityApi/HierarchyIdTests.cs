// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Hierarchy;
    using System.Data.Entity.Infrastructure;
    using System.Globalization;
    using System.Linq;
    using Xunit;

    /// <summary>
    ///     Tests for hierarchyid and DbContext.
    /// </summary>
    public class HierarchyIdTests : FunctionalTestBase
    {
        #region Infrastructure/setup

        private static readonly string _connectionString;

        static HierarchyIdTests()
        {
            const string prefix = "FunctionalTests.ProductivityApi.HierarchyIdTvfsModel.";
            ResourceUtilities.CopyEmbeddedResourcesToCurrentDir(
                typeof(HierarchyIdTests).Assembly, prefix, /*overwrite*/ true,
                "226644HierarchyIdModel.csdl", "226644HierarchyIdModel.msl",
                "226644HierarchyIdModel.ssdl");

            const string baseConnectionString =
                @"metadata=.\226644HierarchyIdModel.csdl|.\226644HierarchyIdModel.ssdl|.\226644HierarchyIdModel.msl;
                                                  provider=System.Data.SqlClient;provider connection string='{0}'";
            _connectionString = String.Format(
                CultureInfo.InvariantCulture, baseConnectionString,
                SimpleConnectionString<HierarchyIdNorthwindContext>());
        }

        #endregion

        #region Tests for TVFs with hierarchyid types

        // Dev11 226644
        [Fact]
        public void DbQuery_with_TVFs_mapped_to_context_instance_methods_involving_hierarchyid_types_works_sync()
        {
            DbQuery_with_TVFs_mapped_to_context_instance_methods_involving_hierarchyid_types_works(ToList);
        }

#if !NET40

        [Fact]
        public void DbQuery_with_TVFs_mapped_to_context_instance_methods_involving_hierarchyid_types_works_async()
        {
            DbQuery_with_TVFs_mapped_to_context_instance_methods_involving_hierarchyid_types_works(ToListAsync);
        }

#endif

        private void DbQuery_with_TVFs_mapped_to_context_instance_methods_involving_hierarchyid_types_works(
            Func<IQueryable<IQueryable<SupplierWithHierarchyId>>, List<IQueryable<SupplierWithHierarchyId>>> toList)
        {
            using (var context = new HierarchyIdNorthwindContext(_connectionString))
            {
                var suppliers = toList(
                    from x in context.Suppliers
                    select
                        context.SuppliersWithinRange(
                            HierarchyId.Parse("/1/"),
                            HierarchyId.Parse("/10/")));

                Assert.Equal(16, suppliers.Count);
            }
        }

        [Fact]
        public void DbQuery_with_TVFs_mapped_to_static_methods_involving_hierarchyid_types_works_sync()
        {
            DbQuery_with_TVFs_mapped_to_static_methods_involving_hierarchyid_types_works(ToList);
        }

#if !NET40

        [Fact]
        public void DbQuery_with_TVFs_mapped_to_static_methods_involving_hierarchyid_types_works_async()
        {
            DbQuery_with_TVFs_mapped_to_static_methods_involving_hierarchyid_types_works(ToListAsync);
        }

#endif

        private void DbQuery_with_TVFs_mapped_to_static_methods_involving_hierarchyid_types_works(
            Func<IQueryable<IQueryable<SupplierWithHierarchyId>>, List<IQueryable<SupplierWithHierarchyId>>> toList)
        {
            using (var context = new HierarchyIdNorthwindContext(_connectionString))
            {
                var suppliers = toList(
                    from x in context.Suppliers
                    select
                        HierarchyIdNorthwindContext.StaticSuppliersWithinRange(
                            HierarchyId.Parse("/1/"),
                            HierarchyId.Parse("/10/")));

                Assert.Equal(16, suppliers.Count);
            }
        }

        [DbFunction("HierarchyIdNorthwindContext", "SuppliersWithinRange")]
        public static IQueryable<SupplierWithHierarchyId> ArbitrarySuppliersWithinRange(HierarchyId path1, HierarchyId path2)
        {
            throw new NotImplementedException("Should not be called by client code.");
        }

        [Fact]
        public void DbQuery_with_TVFs_mapped_to_arbitrary_instance_methods_involving_hierarchyid_types_works_sync()
        {
            DbQuery_with_TVFs_mapped_to_arbitrary_instance_methods_involving_hierarchyid_types_works(ToList);
        }

#if !NET40

        [Fact]
        public void DbQuery_with_TVFs_mapped_to_arbitrary_instance_methods_involving_hierarchyid_types_works_async()
        {
            DbQuery_with_TVFs_mapped_to_arbitrary_instance_methods_involving_hierarchyid_types_works(ToListAsync);
        }

#endif

        private void DbQuery_with_TVFs_mapped_to_arbitrary_instance_methods_involving_hierarchyid_types_works(
            Func<IQueryable<IQueryable<SupplierWithHierarchyId>>, List<IQueryable<SupplierWithHierarchyId>>> toList)
        {
            using (var context = new HierarchyIdNorthwindContext(_connectionString))
            {
                var suppliers = (from x in context.Suppliers
                                 select
                                     ArbitrarySuppliersWithinRange(
                                         HierarchyId.Parse("/1/"),
                                         HierarchyId.Parse("/10/"))).ToList();

                Assert.Equal(16, suppliers.Count);
            }
        }

        [Fact]
        public void DbQuery_SelectMany_with_TVFs_and_hierarchyid_types_works()
        {
            using (var context = new HierarchyIdNorthwindContext(_connectionString))
            {
                var results =
                    (from s1 in
                         context.SuppliersWithinRange(HierarchyId.Parse("/-100/"), HierarchyId.Parse("/100/"))
                     from s2 in
                         context.SuppliersWithinRange(HierarchyId.Parse("/-100/"), HierarchyId.Parse("/100/"))
                     where s1.Name == s2.Name
                     select new
                                {
                                    s1,
                                    s2
                                }).ToList();


                Assert.Equal(16, results.Count);
            }
        }

        [Fact]
        public void DbQuery_SelectMany_with_TVFs_and_hierarchyid_types_using_Point_and_Point_return_in_function_import_works()
        {
            using (var context = new HierarchyIdNorthwindContext(_connectionString))
            {
                context.Database.Initialize(force: false);

                var results =
                    (from s1 in
                         context.SupplierHierarchyIdsWithinRange(
                             HierarchyId.Parse("/-100/"),
                             HierarchyId.Parse("/100/"))
                     from s2 in
                         context.SupplierHierarchyIdsWithinRange(
                             HierarchyId.Parse("/-100/"),
                             HierarchyId.Parse("/100/"))
                     select new
                                {
                                    s1,
                                    s2
                                }).ToList();

                Assert.Equal(256, results.Count);
            }
        }

        #endregion

        #region Tests for strongly typed hierarchyid values for type construction (Dev11 254822)

        [Fact]
        public void Can_query_for_strongly_typed_hierarchyid_using_type_construction()
        {
            using (var context = new HierarchyIdNorthwindContext(_connectionString))
            {
                var query =
                    @"select value ProductivityApiTests.SupplierWithHierarchyId(-77, N'MyName', Edm.HierarchyIdParse(""/1/"")) 
                              from [HierarchyIdNorthwindContext].[Suppliers] as SupplierWithHierarchyId";
                Assert.Equal(16, TestWithReader(context, query, r => Assert.IsType<HierarchyId>(r.GetValue(2))));
            }
        }

        [Fact]
        public void Can_query_for_strongly_typed_hierarchyid_using_type_construction2()
        {
            using (var context = new HierarchyIdNorthwindContext(_connectionString))
            {
                var query =
                    @"select value ProductivityApiTests.WidgetWithHierarchyId(-77, N'MyName', Edm.HierarchyIdParse(""/1/""), ProductivityApiTests.ComplexWithHierarchyId(N'A', Edm.HierarchyIdParse(""/1/""))) 
                              from [HierarchyIdNorthwindContext].[Widgets] as WidgetWithHierarchyId";
                Assert.Equal(
                    4, TestWithReader(
                        context, query, r =>
                                            {
                                                Assert.Equal(-77, r.GetInt32(0));
                                                Assert.IsType<HierarchyId>(r.GetValue(2));
                                                var nestedRecord = r.GetDataRecord(3);
                                                Assert.IsType<HierarchyId>(nestedRecord.GetValue(1));
                                            }));
            }
        }

        [Fact]
        public void Can_query_for_strongly_typed_hierarchyid_using_complex_type_type_construction()
        {
            using (var context = new HierarchyIdNorthwindContext(_connectionString))
            {
                var query =
                    @"select value ProductivityApiTests.ComplexWithHierarchyId(N'A', Edm.HierarchyIdParse(""/1/"")) 
                              from [HierarchyIdNorthwindContext].[Widgets] as WidgetWithHierarchyId";
                Assert.Equal(
                    4, TestWithReader(
                        context, query, r =>
                                            {
                                                Assert.Equal("A", r.GetString(0));
                                                Assert.IsType<HierarchyId>(r.GetValue(1));
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
                var count = 0;
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

        #region Tests for materializing hierarchyid types using eSQL

        // Dev11 260655

        [Fact]
        public void
            Can_materialize_record_containing_hierarchyid_types_and_get_names_of_the_types_without_null_arg_exception_sync()
        {
            Can_materialize_record_containing_hierarchyid_types_and_get_names_of_the_types_without_null_arg_exception(ToList);
        }

#if !NET40

        [Fact]
        public void
            Can_materialize_record_containing_hierarchyid_types_and_get_names_of_the_types_without_null_arg_exception_async()
        {
            Can_materialize_record_containing_hierarchyid_types_and_get_names_of_the_types_without_null_arg_exception(ToListAsync);
        }

#endif

        private void
            Can_materialize_record_containing_hierarchyid_types_and_get_names_of_the_types_without_null_arg_exception(
            Func<IQueryable<DbDataRecord>, List<DbDataRecord>> toList)
        {
            using (var context = new HierarchyIdNorthwindContext(_connectionString))
            {
                var query =
                    @"select o.[Path]
                              from [HierarchyIdNorthwindContext].[Suppliers] as [o]";

                var results = ExecuteESqlQuery(context, query, toList);

                Assert.Equal(16, results.Count);
                foreach (var result in results)
                {
                    Assert.Equal("Path", result.GetName(0)); // GetName would throw
                    Assert.Same(typeof(HierarchyId), result.GetFieldType(0));
                }
            }
        }

        private List<DbDataRecord> ExecuteESqlQuery(
            DbContext context, string query,
            Func<IQueryable<DbDataRecord>, List<DbDataRecord>> toList)
        {
            var objectContext = ((IObjectContextAdapter)context).ObjectContext;
            objectContext.MetadataWorkspace.LoadFromAssembly(typeof(ComplexWithHierarchyId).Assembly);

            return toList(objectContext.CreateQuery<DbDataRecord>(query));
        }

#if !NET40

        private List<DbDataRecord> ExecuteESqlQueryAsync(
            DbContext context, string query,
            Func<IQueryable<DbDataRecord>, List<DbDataRecord>> toList)
        {
            var objectContext = ((IObjectContextAdapter)context).ObjectContext;
            objectContext.MetadataWorkspace.LoadFromAssembly(typeof(ComplexWithHierarchyId).Assembly);

            return toList(objectContext.CreateQuery<DbDataRecord>(query));
        }

#endif

        private List<T> ToList<T>(IQueryable<T> query)
        {
            return query.ToList();
        }

#if !NET40

        private List<T> ToListAsync<T>(IQueryable<T> query)
        {
            return query.ToListAsync().Result;
        }

#endif

        #endregion
    }
}
