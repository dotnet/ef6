// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;
    using System.Linq;
    using AdvancedPatternsModel;
    using Moq;
    using SimpleModel;
    using Xunit;
    using Xunit.Extensions;

    /// <summary>
    /// Functional tests for DbSqlQuery and other raw SQL functionality. 
    /// Unit tests also exist in the unit tests project.
    /// </summary>
    public class DbSqlQueryTests : FunctionalTestBase
    {
        #region Infrastructure/setup

        static DbSqlQueryTests()
        {
            using (var context = new SimpleModelContext())
            {
                context.Database.Initialize(force: false);
            }
        }

        #endregion

        #region SQL queries for entities

        [Fact]
        public void SQL_query_can_be_used_to_materialize_entities_into_a_set()
        {
            using (var context = new SimpleModelContext())
            {
                var productsQuery = context.Products.SqlQuery("select * from Products");

                // Quick check that creating the query object does not execute the query.
                Assert.Equal(0, context.Products.Local.Count);

                var products = productsQuery.ToList();

                Assert.Equal(7, products.Count);
                Assert.Equal(products.Count, context.Products.Local.Count);

                ValidateBovril(products.Single(d => d.Name == "Bovril"));
                CadillacIsNotFeaturedProduct(products.Single(d => d.Name == "Cadillac"));
            }
        }

        [Fact]
        public void Non_generic_SQL_query_can_be_used_to_materialize_entities_into_a_set()
        {
            using (var context = new SimpleModelContext())
            {
                var productsQuery = context.Set(typeof(Product)).SqlQuery("select * from Products");

                // Quick check that creating the query object does not execute the query.
                Assert.Equal(0, context.Products.Local.Count);

                var products = productsQuery.ToList<Product>();

                Assert.Equal(7, products.Count);
                Assert.Equal(products.Count, context.Products.Local.Count);

                ValidateBovril(products.Single(d => d.Name == "Bovril"));
                CadillacIsNotFeaturedProduct(products.Single(d => d.Name == "Cadillac"));
            }
        }

        [Fact]
        public void SQL_query_uses_identity_resolution()
        {
            using (var context = new SimpleModelContext())
            {
                var trackedProducts = context.Products.Where(p => !(p is FeaturedProduct)).OrderBy(p => p.Id).ToList();
                var products = context.Set<Product>().SqlQuery("select * from Products").OrderBy(p => p.Id).ToList();

                Assert.Equal(6, trackedProducts.Count);
                Assert.Equal(7, products.Count);
                Assert.Equal(products.Count, context.Products.Local.Count);

                products.Remove(products.Single(d => d.Name == "Cadillac"));
                Assert.True(products.SequenceEqual(trackedProducts));
            }
        }

        [Fact]
        public void
            SQL_query_identity_resolution_fails_when_type_returned_from_query_is_different_from_type_in_state_manager()
        {
            using (var context = new SimpleModelContext())
            {
                context.Products.Load();

                var query = context.Set<Product>().SqlQuery("select * from Products");
                Assert.Throws<NotSupportedException>(() => query.ToList()).ValidateMessage(
                    "Materializer_RecyclingEntity",
                    "SimpleModelContext.Products",
                    "SimpleModel.Product",
                    "SimpleModel.FeaturedProduct",
                    "EntitySet=Products;Id=7");
            }
        }

        [Fact]
        public void SQL_query_with_parameters_can_be_used_to_materialize_entities_into_a_set()
        {
            using (var context = new SimpleModelContext())
            {
                var products =
                    context.Products.SqlQuery(
                        "select * from Products where Id < {0} and CategoryId = {1}", 4,
                        "Beverages").ToList();

                Assert.Equal(1, context.Products.Local.Count);
                ValidateBovril(products);
            }
        }

        [Fact]
        public void Non_generic_SQL_query_with_parameters_can_be_used_to_materialize_entities_into_a_set()
        {
            using (var context = new SimpleModelContext())
            {
                var products =
                    context.Set(typeof(Product)).SqlQuery(
                        "select * from Products where Id < {0} and CategoryId = {1}",
                        4, "Beverages").ToList<Product>();

                Assert.Equal(1, context.Products.Local.Count);
                ValidateBovril(products);
            }
        }

        [Fact]
        public void SQL_query_with_SqlParameter_parameters_can_be_used_to_materialize_entities_into_a_set()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.SqlQuery(
                    "select * from Products where Id < @p0 and CategoryId = @p1",
                    new SqlParameter
                        {
                            ParameterName = "p0",
                            Value = 4
                        },
                    new SqlParameter
                        {
                            ParameterName = "p1",
                            Value = "Beverages"
                        })
                    .ToList();

                Assert.Equal(1, context.Products.Local.Count);
                ValidateBovril(products);
            }
        }

        [Fact]
        public void Non_generic_SQL_query_with_SqlParameter_parameters_can_be_used_to_materialize_entities_into_a_set()
        {
            using (var context = new SimpleModelContext())
            {
                var products =
                    context.Set(typeof(Product)).SqlQuery(
                        "select * from Products where Id < @p0 and CategoryId = @p1",
                        new SqlParameter
                            {
                                ParameterName = "p0",
                                Value = 4
                            },
                        new SqlParameter
                            {
                                ParameterName = "p1",
                                Value = "Beverages"
                            })
                        .ToList<Product>();

                Assert.Equal(1, context.Products.Local.Count);
                ValidateBovril(products);
            }
        }

        [Fact]
        public void SQL_query_can_be_used_to_materialize_entities_with_AsNoTracking()
        {
            SQL_query_can_be_used_to_materialize_entities_without_tracking(
                (c, s) => c.Products.SqlQuery(s).AsNoTracking().ToList());
        }

        [Fact]
        public void SQL_query_can_be_used_to_materialize_entities_with_AsNoTracking_and_AsStreaming()
        {
            SQL_query_can_be_used_to_materialize_entities_without_tracking(
                (c, s) => c.Products.SqlQuery(s).AsStreaming().AsNoTracking().ToList());
        }

        [Fact]
        public void SQL_query_can_be_used_to_materialize_entities_without_tracking_by_using_Database_SqlQuery()
        {
            SQL_query_can_be_used_to_materialize_entities_without_tracking(
                (c, s) => c.Database.SqlQuery<Product>(s).ToList());
        }

        [Fact]
        public void SQL_query_can_be_used_to_materialize_entities_without_tracking_by_using_Database_SqlQuery_with_AsStreaming()
        {
            SQL_query_can_be_used_to_materialize_entities_without_tracking(
                (c, s) => c.Database.SqlQuery<Product>(s).AsStreaming().ToList());
        }

        [Fact]
        public void Non_generic_SQL_query_can_be_used_to_materialize_entities_with_AsNoTracking()
        {
            SQL_query_can_be_used_to_materialize_entities_without_tracking(
                (c, s) => c.Set(typeof(Product)).SqlQuery(s).AsNoTracking().ToList<Product>());
        }

        [Fact]
        public void Non_generic_SQL_query_can_be_used_to_materialize_entities_with_AsNoTracking_and_AsStreaming()
        {
            SQL_query_can_be_used_to_materialize_entities_without_tracking(
                (c, s) => c.Set(typeof(Product)).SqlQuery(s).AsStreaming().AsNoTracking().ToList<Product>());
        }

        [Fact]
        public void
            Non_generic_SQL_query_can_be_used_to_materialize_entities_without_tracking_by_using_Database_SqlQuery()
        {
            SQL_query_can_be_used_to_materialize_entities_without_tracking(
                (c, s) => c.Database.SqlQuery(typeof(Product), s).ToList<Product>());
        }

        [Fact]
        public void
            Non_generic_SQL_query_can_be_used_to_materialize_entities_without_tracking_by_using_Database_SqlQuery_with_AsStreaming()
        {
            SQL_query_can_be_used_to_materialize_entities_without_tracking(
                (c, s) => c.Database.SqlQuery(typeof(Product), s).AsStreaming().ToList<Product>());
        }

        private void SQL_query_can_be_used_to_materialize_entities_without_tracking(
            Func<SimpleModelContext, string, List<Product>> query)
        {
            using (var context = new SimpleModelContext())
            {
                var products = query(context, "select * from Products");

                Assert.Equal(7, products.Count);
                Assert.Equal(0, context.Products.Local.Count);

                ValidateBovril(products.Single(d => d.Name == "Bovril"));
                CadillacIsNotFeaturedProduct(products.Single(d => d.Name == "Cadillac"));
            }
        }

        [Fact]
        public void SQL_query_with_parameters_can_be_used_to_materialize_entities_with_AsNoTracking()
        {
            SQL_query_with_parameters_can_be_used_to_materialize_entities_without_tracking(
                (c, s, p) => c.Products.SqlQuery(s, p).AsNoTracking().ToList());
        }

        [Fact]
        public void SQL_query_with_parameters_can_be_used_to_materialize_entities_with_AsNoTracking_and_AsStreaming()
        {
            SQL_query_with_parameters_can_be_used_to_materialize_entities_without_tracking(
                (c, s, p) => c.Products.SqlQuery(s, p).AsStreaming().AsNoTracking().ToList());
        }

        [Fact]
        public void
            SQL_query_with_parameters_can_be_used_to_materialize_entities_without_tracking_by_using_Database_SqlQuery()
        {
            SQL_query_with_parameters_can_be_used_to_materialize_entities_without_tracking(
                (c, s, p) => c.Database.SqlQuery<Product>(s, p).ToList());
        }

        [Fact]
        public void
            SQL_query_with_parameters_can_be_used_to_materialize_entities_without_tracking_by_using_Database_SqlQuery_with_AsStreaming()
        {
            SQL_query_with_parameters_can_be_used_to_materialize_entities_without_tracking(
                (c, s, p) => c.Database.SqlQuery<Product>(s, p).AsStreaming().ToList());
        }

        [Fact]
        public void Non_generic_SQL_query_with_parameters_can_be_used_to_materialize_entities_with_AsNoTracking()
        {
            SQL_query_with_parameters_can_be_used_to_materialize_entities_without_tracking(
                (c, s, p) => c.Set(typeof(Product)).SqlQuery(s, p).AsNoTracking().ToList<Product>());
        }

        [Fact]
        public void Non_generic_SQL_query_with_parameters_can_be_used_to_materialize_entities_with_AsNoTracking_and_AsStreaming()
        {
            SQL_query_with_parameters_can_be_used_to_materialize_entities_without_tracking(
                (c, s, p) => c.Set(typeof(Product)).SqlQuery(s, p).AsStreaming().AsNoTracking().ToList<Product>());
        }

        [Fact]
        public void Non_generic_SQL_query_with_parameters_can_be_used_to_materialize_entities_without_tracking_by_using_Database_SqlQuery()
        {
            SQL_query_with_parameters_can_be_used_to_materialize_entities_without_tracking(
                (c, s, p) => c.Database.SqlQuery(typeof(Product), s, p).ToList<Product>());
        }

        [Fact]
        public void Non_generic_SQL_query_with_parameters_can_be_used_to_materialize_entities_without_tracking_by_using_Database_SqlQuery_with_AsStreaming()
        {
            SQL_query_with_parameters_can_be_used_to_materialize_entities_without_tracking(
                (c, s, p) => c.Database.SqlQuery(typeof(Product), s, p).AsStreaming().ToList<Product>());
        }

        private void SQL_query_with_parameters_can_be_used_to_materialize_entities_without_tracking(
            Func<SimpleModelContext, string, object[], List<Product>> query)
        {
            using (var context = new SimpleModelContext())
            {
                var products = query(
                    context, "select * from Products where Id < {0} and CategoryId = {1}",
                    new object[] { 4, "Beverages" });

                Assert.Equal(0, context.Products.Local.Count);
                ValidateBovril(products);
            }
        }

        [Fact]
        public void SQL_query_can_be_used_to_materialize_derived_entities_into_a_set()
        {
            using (var context = new SimpleModelContext())
            {
                var products =
                    context.Set<FeaturedProduct>().SqlQuery(
                        "select * from Products where Discriminator = 'FeaturedProduct'").ToList();

                Assert.Equal(1, products.Count);
                Assert.Equal(products.Count, context.Products.Local.Count);

                ValidateCadillac(products.Single());
            }
        }

        [Fact]
        public void
            SQL_query_can_be_used_to_materialize_derived_entities_into_a_set_even_when_base_entities_are_returned()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Set<FeaturedProduct>().SqlQuery("select * from Products").ToList();

                Assert.Equal(7, products.Count);
                Assert.Equal(products.Count, context.Products.Local.Count);

                ValidateBovril(products.Single(d => d.Name == "Bovril"));
                ValidateCadillac(products.Single(d => d.Name == "Cadillac"));
            }
        }

        [Fact]
        public void SQL_query_for_entity_where_columns_dont_map_throws()
        {
            using (var context = new SimpleModelContext())
            {
                var query = context.Products.SqlQuery("select * from Categories");

                Assert.Throws<EntityCommandExecutionException>(() => query.ToList()).ValidateMessage(
                    "ADP_InvalidDataReaderMissingColumnForType",
                    "SimpleModel.Product", "CategoryId");
            }
        }

        [Fact]
        public void SQL_query_for_entity_can_be_executed_multiple_times()
        {
            using (var context = new SimpleModelContext())
            {
                var query = context.Products.SqlQuery("select * from Products");

                Assert.True(query.ToList().SequenceEqual(query.ToList()));
            }
        }

        [Fact]
        public void Non_generic_SQL_query_for_entity_can_be_executed_multiple_times()
        {
            using (var context = new SimpleModelContext())
            {
                var query = context.Set(typeof(Product)).SqlQuery("select * from Products");

                Assert.True(query.ToList<Product>().SequenceEqual(query.ToList<Product>()));
            }
        }

        private static void ValidateBovril(List<Product> products)
        {
            Assert.Equal(1, products.Count);
            ValidateBovril(products.Single());
        }

        private static void ValidateBovril(dynamic bovril)
        {
            Assert.Equal(2, bovril.Id);
            Assert.Equal("Bovril", bovril.Name);
            Assert.Equal("Beverages", bovril.CategoryId);
        }

        private static void ValidateCadillac(Product cadillac)
        {
            Assert.IsType<FeaturedProduct>(cadillac);
            var asFeaturedProduct = (FeaturedProduct)cadillac;

            Assert.Equal(7, asFeaturedProduct.Id);
            Assert.Equal("Cadillac", asFeaturedProduct.Name);
            Assert.Equal("Cars", asFeaturedProduct.CategoryId);
            Assert.Equal("Ed Wood", asFeaturedProduct.PromotionalCode);
        }

        private static void CadillacIsNotFeaturedProduct(Product cadillac)
        {
            Assert.IsNotType<FeaturedProduct>(cadillac);

            Assert.Equal(7, cadillac.Id);
            Assert.Equal("Cadillac", cadillac.Name);
            Assert.Equal("Cars", cadillac.CategoryId);
        }

        [Fact]
        public void SQL_query_for_entity_is_streaming_by_default()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.SqlQuery("select * from Products");
                using (var enumerator = products.GetEnumerator())
                {
                    enumerator.MoveNext();

                    Assert.Equal(ConnectionState.Open, context.Database.Connection.State);
                }
            }
        }

        [Fact]
        public void SQL_query_for_entity_is_buffered_if_execution_strategy_is_used()
        {
            var executionStrategyMock = new Mock<IDbExecutionStrategy>();
            executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);
            executionStrategyMock.Setup(m => m.Execute(It.IsAny<Func<ObjectResult<Product>>>()))
                 .Returns<Func<ObjectResult<Product>>>(f => f());
            executionStrategyMock.Setup(m => m.Execute(It.IsAny<Action>())).Callback<Action>(f => f());

            MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
            try
            {
                using (var context = new SimpleModelContext())
                {
                    var products = context.Products.SqlQuery("select * from Products");
                    using (var enumerator = products.GetEnumerator())
                    {
                        enumerator.MoveNext();

                        Assert.Equal(ConnectionState.Closed, context.Database.Connection.State);
                    }
                }
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }
        }

        #endregion

        #region SQL queries for non-entities

        public class UnMappedProduct
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string CategoryId { get; set; }
        }

        [Fact]
        public void SQL_query_can_be_used_to_materialize_unmapped_types()
        {
            SQL_query_can_be_used_to_materialize_unmapped_types_implementation(
                (c, s) => c.Database.SqlQuery<UnMappedProduct>(s).ToList());
        }

        [Fact]
        public void Non_generic_SQL_query_can_be_used_to_materialize_unmapped_types()
        {
            SQL_query_can_be_used_to_materialize_unmapped_types_implementation(
                (c, s) => c.Database.SqlQuery(typeof(UnMappedProduct), s).ToList<UnMappedProduct>());
        }

#if !NET40

        [Fact]
        public void SQL_query_can_be_used_to_materialize_unmapped_types_async()
        {
            SQL_query_can_be_used_to_materialize_unmapped_types_implementation(
                (c, s) => c.Database.SqlQuery<UnMappedProduct>(s).ToListAsync().Result);
        }

        [Fact]
        public void Non_generic_SQL_query_can_be_used_to_materialize_unmapped_types_async()
        {
            SQL_query_can_be_used_to_materialize_unmapped_types_implementation(
                (c, s) => c.Database.SqlQuery(typeof(UnMappedProduct), s).ToListAsync().Result.ToList<UnMappedProduct>());
        }

#endif

        private void SQL_query_can_be_used_to_materialize_unmapped_types_implementation(
            Func<SimpleModelContext, string, List<UnMappedProduct>> query)
        {
            using (var context = new SimpleModelContext())
            {
                var products = query(context, "select * from Products");

                Assert.Equal(7, products.Count);
                Assert.Equal(0, context.Products.Local.Count);

                ValidateBovril(products.Single(d => d.Name == "Bovril"));
            }
        }

        [Fact]
        public void SQL_query_with_parameters_can_be_used_to_materialize_unmapped_types()
        {
            SQL_query_with_parameters_can_be_used_to_materialize_unmapped_types_implementation(
                (c, s, p) => c.Database.SqlQuery<UnMappedProduct>(s, p).ToList());
        }

        [Fact]
        public void Non_generic_SQL_query_with_parameters_can_be_used_to_materialize_unmapped_types()
        {
            SQL_query_with_parameters_can_be_used_to_materialize_unmapped_types_implementation(
                (c, s, p) => c.Database.SqlQuery(typeof(UnMappedProduct), s, p).ToList<UnMappedProduct>());
        }

#if !NET40

        [Fact]
        public void SQL_query_with_parameters_can_be_used_to_materialize_unmapped_types_async()
        {
            SQL_query_with_parameters_can_be_used_to_materialize_unmapped_types_implementation(
                (c, s, p) => c.Database.SqlQuery<UnMappedProduct>(s, p).ToListAsync().Result);
        }

        [Fact]
        public void Non_generic_SQL_query_with_parameters_can_be_used_to_materialize_unmapped_types_async()
        {
            SQL_query_with_parameters_can_be_used_to_materialize_unmapped_types_implementation(
                (c, s, p) => c.Database.SqlQuery(typeof(UnMappedProduct), s, p).ToListAsync().Result.ToList<UnMappedProduct>());
        }

#endif

        private void SQL_query_with_parameters_can_be_used_to_materialize_unmapped_types_implementation(
            Func<SimpleModelContext, string, object[], List<UnMappedProduct>> query)
        {
            using (var context = new SimpleModelContext())
            {
                var products = query(
                    context, "select * from Products where Id < {0} and CategoryId = {1}",
                    new object[] { 4, "Beverages" });

                Assert.Equal(1, products.Count);
                Assert.Equal(0, context.Products.Local.Count);

                ValidateBovril(products.Single());
            }
        }

        [Fact]
        public void SQL_query_for_non_entity_where_columns_dont_map_throws()
        {
            using (var context = new SimpleModelContext())
            {
                var query = context.Database.SqlQuery<UnMappedProduct>("select * from Categories");

                Assert.Throws<InvalidOperationException>(() => query.ToList()).ValidateMessage(
                    "Materializer_InvalidCastReference", "System.String",
                    "System.Int32");
            }
        }

        [Fact]
        public void SQL_query_for_non_entity_where_columns_dont_map_throws_when_streaming()
        {
            using (var context = new SimpleModelContext())
            {
                var query = context.Database.SqlQuery<UnMappedProduct>("select * from Categories").AsStreaming();

                Assert.Throws<InvalidOperationException>(() => query.ToList()).ValidateMessage(
                    "Materializer_InvalidCastReference", "System.String",
                    "System.Int32");
            }
        }

#if !NET40

        [Fact]
        public void SQL_query_for_non_entity_where_columns_dont_map_throws_async()
        {
            using (var context = new SimpleModelContext())
            {
                var query = context.Database.SqlQuery<UnMappedProduct>("select * from Categories");

                Assert.Throws<InvalidOperationException>(
                    () => ExceptionHelpers.UnwrapAggregateExceptions(
                        () =>
                        query.ToListAsync().Result)).ValidateMessage(
                            "Materializer_InvalidCastReference", "System.String",
                            "System.Int32");
            }
        }

        [Fact]
        public void SQL_query_for_non_entity_where_columns_dont_map_throws_when_streaming_async()
        {
            using (var context = new SimpleModelContext())
            {
                var query = context.Database.SqlQuery<UnMappedProduct>("select * from Categories").AsStreaming();

                Assert.Throws<InvalidOperationException>(
                    () => ExceptionHelpers.UnwrapAggregateExceptions(
                        () =>
                        query.ToListAsync().Result)).ValidateMessage(
                            "Materializer_InvalidCastReference", "System.String",
                            "System.Int32");
            }
        }

#endif

        [Fact]
        public void SQL_query_cannot_be_used_to_materialize_anonymous_types()
        {
            SQL_query_cannot_be_used_to_materialize_anonymous_types_implementation(
                new
                    {
                        Id = 2,
                        Name = "Bovril",
                        CategoryId = "Foods"
                    }, q => q.ToList());
        }

#if !NET40

        [Fact]
        public void SQL_query_cannot_be_used_to_materialize_anonymous_types_async()
        {
            SQL_query_cannot_be_used_to_materialize_anonymous_types_implementation(
                new
                    {
                        Id = 2,
                        Name = "Bovril",
                        CategoryId = "Foods"
                    }, q => ExceptionHelpers.UnwrapAggregateExceptions(() => q.ToListAsync().Result));
        }

#endif

        private void SQL_query_cannot_be_used_to_materialize_anonymous_types_implementation<TElement>(
            TElement _,
            Func<DbRawSqlQuery<TElement>, List<TElement>> execute)
        {
            using (var context = new SimpleModelContext())
            {
                var query = context.Database.SqlQuery<TElement>("select * from Products");
                Assert.Throws<InvalidOperationException>(() => execute(query)).ValidateMessage(
                    "ObjectContext_InvalidTypeForStoreQuery",
                    typeof(TElement).ToString());
            }
        }

        [Fact]
        public void SQL_query_can_be_used_to_materialize_value_types()
        {
            SQL_query_can_be_used_to_materialize_value_types_implementation(
                (c, s) => c.Database.SqlQuery<int>(s).ToList());
        }

        [Fact]
        public void Non_generic_SQL_query_can_be_used_to_materialize_value_types()
        {
            SQL_query_can_be_used_to_materialize_value_types_implementation(
                (c, s) => c.Database.SqlQuery(typeof(int), s).ToList<int>());
        }

#if !NET40

        [Fact]
        public void SQL_query_can_be_used_to_materialize_value_types_async()
        {
            SQL_query_can_be_used_to_materialize_value_types_implementation(
                (c, s) => c.Database.SqlQuery<int>(s).ToListAsync().Result);
        }

        [Fact]
        public void Non_generic_SQL_query_can_be_used_to_materialize_value_types_async()
        {
            SQL_query_can_be_used_to_materialize_value_types_implementation(
                (c, s) => c.Database.SqlQuery(typeof(int), s).ToListAsync().Result.ToList<int>());
        }

#endif

        private void SQL_query_can_be_used_to_materialize_value_types_implementation(
            Func<SimpleModelContext, string, List<int>> query)
        {
            using (var context = new SimpleModelContext())
            {
                var products = query(context, "select Id from Products");

                Assert.Equal(7, products.Count);
                Assert.Equal(0, context.Products.Local.Count);

                Assert.True(products.Contains(2));
            }
        }

        [Fact]
        public void SQL_query_can_be_used_to_materialize_complex_types()
        {
            SQL_query_can_be_used_to_materialize_complex_types_implementation(
                (c, s) => c.Database.SqlQuery<SiteInfo>(s).ToList());
        }

        [Fact]
        public void Non_generic_SQL_query_can_be_used_to_materialize_complex_types()
        {
            SQL_query_can_be_used_to_materialize_complex_types_implementation(
                (c, s) => c.Database.SqlQuery(typeof(SiteInfo), s).ToList<SiteInfo>());
        }

#if !NET40

        [Fact]
        public void SQL_query_can_be_used_to_materialize_complex_types_async()
        {
            SQL_query_can_be_used_to_materialize_complex_types_implementation(
                (c, s) => c.Database.SqlQuery<SiteInfo>(s).ToListAsync().Result);
        }

        [Fact]
        public void Non_generic_SQL_query_can_be_used_to_materialize_complex_types_async()
        {
            SQL_query_can_be_used_to_materialize_complex_types_implementation(
                (c, s) => c.Database.SqlQuery(typeof(SiteInfo), s).ToListAsync().Result.ToList<SiteInfo>());
        }

#endif

        private void SQL_query_can_be_used_to_materialize_complex_types_implementation(
            Func<AdvancedPatternsMasterContext, string, List<SiteInfo>> query)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var siteInfos = query(
                    context,
                    "select Address_SiteInfo_Zone as Zone, Address_SiteInfo_Environment as Environment from Buildings");

                Assert.Equal(2, siteInfos.Count);
            }
        }

        [Fact]
        public void SQL_query_for_non_entity_can_be_executed_multiple_times()
        {
            using (var context = new SimpleModelContext())
            {
                var query = context.Database.SqlQuery<int>("select Id from Products");

                Assert.True(query.ToList().SequenceEqual(query.ToList()));
            }
        }

        [Fact]
        public void Non_generic_SQL_query_for_non_entity_can_be_executed_multiple_times()
        {
            using (var context = new SimpleModelContext())
            {
                var query = context.Database.SqlQuery(typeof(int), "select Id from Products");

                Assert.True(query.ToList<int>().SequenceEqual(query.ToList<int>()));
            }
        }

#if !NET40

        [Fact]
        public void SQL_query_for_non_entity_can_be_executed_multiple_times_async()
        {
            using (var context = new SimpleModelContext())
            {
                var query = context.Database.SqlQuery<int>("select Id from Products");

                Assert.True(query.ToListAsync().Result.SequenceEqual(query.ToListAsync().Result));
            }
        }

        [Fact]
        public void Non_generic_SQL_query_for_non_entity_can_be_executed_multiple_times_async()
        {
            using (var context = new SimpleModelContext())
            {
                var query = context.Database.SqlQuery(typeof(int), "select Id from Products");

                Assert.True(query.ToListAsync().Result.SequenceEqual(query.ToListAsync().Result));
            }
        }

#endif

        [Fact]
        public void SQL_query_is_streaming_by_default()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Database.SqlQuery<int>("select Id from Products");
                using (var enumerator = products.GetEnumerator())
                {
                    enumerator.MoveNext();

                    Assert.Equal(ConnectionState.Open, context.Database.Connection.State);
                }
            }
        }

        [Fact]
        public void SQL_query_is_buffered_if_execution_strategy_is_used()
        {
            var executionStrategyMock = new Mock<IDbExecutionStrategy>();
            executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);
            executionStrategyMock.Setup(m => m.Execute(It.IsAny<Func<ObjectResult<int>>>()))
                 .Returns<Func<ObjectResult<int>>>(f => f());
            executionStrategyMock.Setup(m => m.Execute(It.IsAny<Action>())).Callback<Action>(f => f());

            MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
            try
            {
                using (var context = new SimpleModelContext())
                {
                    var products = context.Database.SqlQuery<int>("select Id from Products");
                    using (var enumerator = products.GetEnumerator())
                    {
                        enumerator.MoveNext();

                        Assert.Equal(ConnectionState.Closed, context.Database.Connection.State);
                    }
                }
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }
        }

        [Fact]
        public void Nongeneric_SQL_query_is_streaming_by_default()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Database.SqlQuery(typeof(int), "select Id from Products");
                var enumerator = products.GetEnumerator();
                enumerator.MoveNext();

                Assert.Equal(ConnectionState.Open, context.Database.Connection.State);
            }
        }

        [Fact]
        public void Nongeneric_SQL_query_is_buffered_if_execution_strategy_is_used()
        {
            var executionStrategyMock = new Mock<IDbExecutionStrategy>();
            executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);
            executionStrategyMock.Setup(m => m.Execute(It.IsAny<Func<ObjectResult<int>>>()))
                 .Returns<Func<ObjectResult<int>>>(f => f());
            executionStrategyMock.Setup(m => m.Execute(It.IsAny<Action>())).Callback<Action>(f => f());

            MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
            try
            {
                using (var context = new SimpleModelContext())
                {
                    var products = context.Database.SqlQuery(typeof(int), "select Id from Products");
                    var enumerator = products.GetEnumerator();
                    enumerator.MoveNext();

                    Assert.Equal(ConnectionState.Closed, context.Database.Connection.State);
                }
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }
        }

        #endregion

        #region SQL command tests

        [Fact]
        [AutoRollback]
        public void SQL_commands_can_be_executed_against_the_database()
        {
            SQL_commands_can_be_executed_against_the_database_implementation((d, q) => d.ExecuteSqlCommand(q));
        }

#if !NET40

        [Fact]
        [AutoRollback]
        public void SQL_commands_can_be_executed_against_the_database_async()
        {
            SQL_commands_can_be_executed_against_the_database_implementation((d, q) => d.ExecuteSqlCommandAsync(q).Result);
        }

#endif

        private void SQL_commands_can_be_executed_against_the_database_implementation(Func<Database, string, int> execute)
        {
            using (var context = new SimpleModelContext())
            {
                var result = execute(
                    context.Database,
                    "update Products set Name = 'Vegemite' where Name = 'Marmite'");

                Assert.Equal(1, result);

                Assert.NotNull(context.Products.SingleOrDefault(p => p.Name == "Vegemite"));
            }
        }

        [Fact]
        [AutoRollback]
        public void SQL_commands_with_parameters_can_be_executed_against_the_database()
        {
            SQL_commands_with_parameters_can_be_executed_against_the_database_implementation((d, q, p) => d.ExecuteSqlCommand(q, p));
        }

#if !NET40

        [Fact]
        [AutoRollback]
        public void SQL_commands_with_parameters_can_be_executed_against_the_database_async()
        {
            SQL_commands_with_parameters_can_be_executed_against_the_database_implementation(
                (d, q, p) => d.ExecuteSqlCommandAsync(q, p).Result);
        }

#endif

        private void SQL_commands_with_parameters_can_be_executed_against_the_database_implementation(
            Func<Database, string, object[], int> execute)
        {
            using (var context = new SimpleModelContext())
            {
                var result = execute(
                    context.Database,
                    "update Products set Name = {0} where Name = {1}",
                    new object[] { "Vegemite", "Marmite" });

                Assert.Equal(1, result);

                Assert.NotNull(context.Products.SingleOrDefault(p => p.Name == "Vegemite"));
            }
        }

        #endregion
    }
}
