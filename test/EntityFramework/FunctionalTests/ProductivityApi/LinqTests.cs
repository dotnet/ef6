namespace ProductivityApiTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    using System.Linq.Expressions;
    using ConcurrencyModel;
    using SimpleModel;
    using Xunit;
    using Xunit.Sdk;

    /// <summary>
    /// Functional tests for LINQ to Entities using DbQuery.
    /// </summary>
    public class LinqTests : FunctionalTestBase
    {
        #region Tests for mismatch between TElement and ElementType (Dev11 254425)

        [Fact]
        public void Generic_CreateQuery_on_generic_DbQuery_uses_ElementType_when_ElementType_differs_from_generic_type()
        {
            using (var context = new F1Context())
            {
                var newQuery =
                    ((IQueryable)context.Set<Driver>()).Provider.CreateQuery<object>(LewisHamiltonExpression(context));
                Assert.IsType<DbQuery<Driver>>(newQuery);
                Assert.IsAssignableFrom<IQueryable<object>>(newQuery);

                newQuery.Load();
                Assert.Equal("Lewis Hamilton", context.Drivers.Local.Single().Name);
            }
        }

        [Fact]
        public void
            Generic_CreateQuery_on_non_generic_DbQuery_uses_ElementType_when_ElementType_differs_from_generic_type()
        {
            using (var context = new F1Context())
            {
                var newQuery =
                    ((IQueryable)context.Set(typeof(Driver))).Provider.CreateQuery<object>(
                        LewisHamiltonExpression(context));
                Assert.IsAssignableFrom<DbQuery>(newQuery);
                Assert.IsAssignableFrom<IQueryable<Driver>>(newQuery);
                Assert.IsAssignableFrom<IQueryable<object>>(newQuery);

                newQuery.Load();
                Assert.Equal("Lewis Hamilton", context.Drivers.Local.Single().Name);
            }
        }

        [Fact]
        public void Generic_CreateQuery_on_generic_DbQuery_uses_ElementType_when_ElementType_differs_from_generic_type_even_when_using_dynamic()
        {
            using (var context = new F1Context())
            {
                var newQuery =
                    ((IQueryable)context.Set<Driver>()).Provider.CreateQuery<dynamic>(LewisHamiltonExpression(context));
                Assert.IsAssignableFrom<DbQuery<Driver>>(newQuery);
                Assert.IsAssignableFrom<IQueryable<dynamic>>(newQuery);

                newQuery.Load();
                Assert.Equal("Lewis Hamilton", context.Drivers.Local.Single().Name);
            }
        }

        [Fact]
        public void Generic_CreateQuery_on_non_generic_DbQuery_uses_ElementType_when_ElementType_differs_from_generic_type_even_when_using_dynamic()
        {
            using (var context = new F1Context())
            {
                var newQuery =
                    ((IQueryable)context.Set(typeof(Driver))).Provider.CreateQuery<dynamic>(
                        LewisHamiltonExpression(context));
                Assert.IsAssignableFrom<DbQuery>(newQuery);
                Assert.IsAssignableFrom<IQueryable<Driver>>(newQuery);
                Assert.IsAssignableFrom<IQueryable<dynamic>>(newQuery);

                newQuery.Load();
                Assert.Equal("Lewis Hamilton", context.Drivers.Local.Single().Name);
            }
        }

        [Fact]
        public void Generic_CreateQuery_on_generic_DbQuery_with_incompatible_TElement_and_ElementType_throws()
        {
            using (var context = new F1Context())
            {
                Assert.Throws<InvalidCastException>(
                    () => ((IQueryable)context.Set<Driver>()).Provider.CreateQuery<Team>(LewisHamiltonExpression(context)));
            }
        }

        [Fact]
        public void Generic_CreateQuery_on_non_generic_DbQuery_with_incompatible_TElement_and_ElementType_throws()
        {
            using (var context = new F1Context())
            {
                Assert.Throws<InvalidCastException>(
                    () => ((IQueryable)context.Set(typeof(Driver))).Provider.CreateQuery<Team>(LewisHamiltonExpression(context)));
            }
        }

        #endregion

        #region Tests for using ToString to get a query trace

        [Fact]
        public void DbSet_ToString_returns_a_query_string_equivalent_to_ObjectSet_ToTraceString()
        {
            using (var context = new F1Context())
            {
                var dbString = context.Drivers.ToString();
                var oqString = GetObjectContext(context).CreateObjectSet<Driver>().ToTraceString();

                Assert.Equal(dbString, oqString);
            }
        }

        [Fact]
        public void DbQuery_ToString_returns_a_query_string_equivalent_to_ObjectQuery_ToTraceString()
        {
            using (var context = new F1Context())
            {
                var dbString = context.Drivers.ToString();
                var oqString = GetObjectContext(context).CreateObjectSet<Driver>().ToTraceString();

                Assert.Equal(dbString, oqString);
            }
        }

        [Fact]
        public void DbQuery_ToString_after_LINQ_returns_a_query_string_equivalent_to_ObjectQuery_ToTraceString()
        {
            using (var context = new F1Context())
            {
                var dbString = context.Drivers.Where(d => d.Name == "Jenson Button").ToString();
                var oqString =
                    ((ObjectQuery<Driver>)
                     GetObjectContext(context).CreateObjectSet<Driver>().Where(d => d.Name == "Jenson Button")).
                        ToTraceString();

                Assert.Equal(dbString, oqString);
            }
        }

        [Fact]
        public void
            DbQuery_ToString_without_cast_after_LINQ_returns_a_query_string_equivalent_to_ObjectQuery_ToTraceString()
        {
            using (var context = new F1Context())
            {
                var dbString = context.Drivers.Where(d => d.Name == "Jenson Button").ToString();
                var oqString =
                    ((ObjectQuery<Driver>)
                     GetObjectContext(context).CreateObjectSet<Driver>().Where(d => d.Name == "Jenson Button")).
                        ToTraceString();

                Assert.Equal(dbString, oqString);
            }
        }

        [Fact]
        public void Non_generic_DbSet_ToString_returns_a_query_string_equivalent_to_ObjectSet_ToTraceString()
        {
            using (var context = new F1Context())
            {
                var dbString = context.Set(typeof(Driver)).ToString();
                var oqString = GetObjectContext(context).CreateObjectSet<Driver>().ToTraceString();

                Assert.Equal(dbString, oqString);
            }
        }

        [Fact]
        public void Non_generic_DbQuery_ToString_returns_a_query_string_equivalent_to_ObjectQuery_ToTraceString()
        {
            using (var context = new F1Context())
            {
                var dbString = context.Set(typeof(Driver)).ToString();
                var oqString = GetObjectContext(context).CreateObjectSet<Driver>().ToTraceString();

                Assert.Equal(dbString, oqString);
            }
        }

        [Fact]
        public void
            Non_generic_DbQuery_ToString_after_LINQ_returns_a_query_string_equivalent_to_ObjectQuery_ToTraceString()
        {
            using (var context = new F1Context())
            {
                var expression =
                    GetObjectContext(context).CreateObjectSet<Driver>().Where(d => d.Name == "Jenson Button").Expression;
                var query = ((IQueryable)context.Set(typeof(Driver))).Provider.CreateQuery(expression);

                var dbString = query.ToString();
                var oqString =
                    ((ObjectQuery<Driver>)
                     GetObjectContext(context).CreateObjectSet<Driver>().Where(d => d.Name == "Jenson Button")).
                        ToTraceString();

                Assert.Equal(dbString, oqString);
            }
        }

        [Fact]
        public void Non_generic_DbQuery_ToString_without_cast_after_LINQ_returns_a_query_string_equivalent_to_ObjectQuery_ToTraceString()
        {
            using (var context = new F1Context())
            {
                var expression =
                    GetObjectContext(context).CreateObjectSet<Driver>().Where(d => d.Name == "Jenson Button").Expression;
                var query = ((IQueryable)context.Set(typeof(Driver))).Provider.CreateQuery(expression);

                var dbString = query.ToString();
                var oqString =
                    ((ObjectQuery<Driver>)
                     GetObjectContext(context).CreateObjectSet<Driver>().Where(d => d.Name == "Jenson Button")).
                        ToTraceString();

                Assert.Equal(dbString, oqString);
            }
        }

        #endregion

        #region Tests to check that LINQ against DbSets creates DbQueries not ObjectQueries

        [Fact]
        public void LINQ_using_Set_method_results_in_IQueryable_that_is_DbQuery()
        {
            using (var context = new F1Context())
            {
                var query = context.Set<Driver>();
                Assert.IsAssignableFrom<DbQuery<Driver>>(query);
                query.Load(); // Sanity test that it doesn't throw
            }
        }

        [Fact]
        public void LINQ_with_Where_results_in_IQueryable_that_is_DbQuery()
        {
            using (var context = new F1Context())
            {
                var query = context.Set<Driver>().Where(d => d.Name.StartsWith("L"));
                Assert.IsType<DbQuery<Driver>>(query);
                query.Load(); // Sanity test that it doesn't throw
            }
        }

        public class DriverProjectionClass
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        public void LINQ_with_Select_results_in_IQueryable_that_is_DbQuery()
        {
            using (var context = new F1Context())
            {
                var query =
                    context.Drivers.Where(d => d.Name.StartsWith("L")).Select(
                        d => new DriverProjectionClass { Id = d.Id, Name = d.Name });
                Assert.IsType<DbQuery<DriverProjectionClass>>(query);
                query.Load(); // Sanity test that it doesn't throw
            }
        }

        public class NumberProductProjectionClass
        {
            public int Value { get; set; }
            public int UnitsInStock { get; set; }
        }

        [Fact]
        public void LINQ_with_SelectMany_results_in_IQueryable_that_is_DbQuery()
        {
            using (var context = new SimpleModelForLinq())
            {
                var query = from n in context.Numbers
                            from p in context.Products
                            where n.Value > p.UnitsInStock
                            select new NumberProductProjectionClass { Value = n.Value, UnitsInStock = p.UnitsInStock };
                Assert.IsType<DbQuery<NumberProductProjectionClass>>(query);
                query.Load(); // Sanity test that it doesn't throw
            }
        }

        [Fact]
        public void LINQ_with_SelectMany_using_Set_method_results_in_IQueryable_that_is_DbQuery()
        {
            using (var context = new SimpleModelForLinq())
            {
                var query = from n in context.Set<NumberForLinq>()
                            from p in context.Set<ProductForLinq>()
                            where n.Value > p.UnitsInStock
                            select new NumberProductProjectionClass { Value = n.Value, UnitsInStock = p.UnitsInStock };
                Assert.IsType<DbQuery<NumberProductProjectionClass>>(query);
                query.Load(); // Sanity test that it doesn't throw
            }
        }

        [Fact]
        public void LINQ_with_SelectMany_and_closure_parameter_results_in_IQueryable_that_is_DbQuery()
        {
            using (var context = new SimpleModelForLinq())
            {
                var parameter = 1;
                var query = from n in context.Numbers
                            from p in context.Products
                            where n.Value > p.UnitsInStock && n.Value == parameter
                            select new NumberProductProjectionClass { Value = n.Value, UnitsInStock = p.UnitsInStock };
                Assert.IsType<DbQuery<NumberProductProjectionClass>>(query);
                query.Load(); // Sanity test that it doesn't throw
            }
        }

        [Fact]
        public void LINQ_with_SelectMany_and_closure_parameter_using_Set_method_results_in_IQueryable_that_is_DbQuery()
        {
            using (var context = new SimpleModelForLinq())
            {
                var parameter = 1;
                var query = from n in context.Set<NumberForLinq>()
                            from p in context.Set<ProductForLinq>()
                            where n.Value > p.UnitsInStock && n.Value == parameter
                            select new NumberProductProjectionClass { Value = n.Value, UnitsInStock = p.UnitsInStock };
                Assert.IsType<DbQuery<NumberProductProjectionClass>>(query);
                query.Load(); // Sanity test that it doesn't throw
            }
        }

        [Fact]
        public void LINQ_with_SelectMany_and_closure_parameter_using_custom_DbQuery_methods_results_in_IQueryable_that_is_DbQuery()
        {
            using (var context = new SimpleModelForLinq())
            {
                var parameter = 1;
                var query = from n in context.NumbersGreaterThanTen()
                            from p in context.ProductsStartingWithP
                            where n.Value > p.UnitsInStock && n.Value == parameter
                            select new NumberProductProjectionClass { Value = n.Value, UnitsInStock = p.UnitsInStock };
                Assert.IsType<DbQuery<NumberProductProjectionClass>>(query);
                query.Load(); // Sanity test that it doesn't throw
            }
        }

        [Fact]
        public void LINQ_with_OrderBy_and_Skip_results_in_IQueryable_that_is_DbQuery()
        {
            using (var context = new SimpleModelForLinq())
            {
                var query = context.Numbers.OrderBy(n => n.Id).Skip(4);
                Assert.IsType<DbQuery<NumberForLinq>>(query);
                query.Load(); // Sanity test that it doesn't throw
            }
        }

        #endregion

        #region Tests for generic and non-generic calls to CreateQuery

        // See DevDiv2 Bug 112305
        [Fact]
        public void Call_to_generic_CreateQuery_method_from_generic_DbQuery_returns_usable_generic_DbQuery()
        {
            using (var context = new F1Context())
            {
                var newQuery =
                    ((IQueryable)context.Set<Driver>()).Provider.CreateQuery<Driver>(LewisHamiltonExpression(context));
                Assert.IsType<DbQuery<Driver>>(newQuery);

                newQuery.Load();
                Assert.Equal("Lewis Hamilton", context.Drivers.Local.Single().Name);
            }
        }

        // See DevDiv2 Bug 112305
        [Fact]
        public void Call_to_generic_CreateQuery_method_from_non_generic_DbQuery_returns_usable_non_generic_DbQuery()
        {
            using (var context = new F1Context())
            {
                var newQuery =
                    ((IQueryable)context.Set(typeof(Driver))).Provider.CreateQuery<Driver>(
                        LewisHamiltonExpression(context));
                Assert.IsAssignableFrom<DbQuery>(newQuery);
                Assert.IsAssignableFrom<IQueryable<Driver>>(newQuery);

                newQuery.Load();
                Assert.Equal("Lewis Hamilton", context.Drivers.Local.Single().Name);
            }
        }

        // See DevDiv2 Bug 112305
        [Fact]
        public void Call_to_non_generic_CreateQuery_method_from_generic_DbQuery_returns_usable_generic_DbQuery()
        {
            using (var context = new F1Context())
            {
                var newQuery = ((IQueryable)context.Set<Driver>()).Provider.CreateQuery(LewisHamiltonExpression(context));
                Assert.IsType<DbQuery<Driver>>(newQuery);

                newQuery.Load();
                Assert.Equal("Lewis Hamilton", context.Drivers.Local.Single().Name);
            }
        }

        // See DevDiv2 Bug 112305
        [Fact]
        public void Call_to_non_generic_CreateQuery_method_from_non_generic_DbQuery_returns_usable_non_generic_DbQuery()
        {
            using (var context = new F1Context())
            {
                var newQuery =
                    ((IQueryable)context.Set(typeof(Driver))).Provider.CreateQuery(LewisHamiltonExpression(context));
                Assert.IsAssignableFrom<DbQuery>(newQuery);
                Assert.IsAssignableFrom<IQueryable<Driver>>(newQuery);

                newQuery.Load();
                Assert.Equal("Lewis Hamilton", context.Drivers.Local.Single().Name);
            }
        }

        private Expression LewisHamiltonExpression(F1Context context)
        {
            return GetObjectContext(context).CreateObjectSet<Driver>().Where(d => d.Name == "Lewis Hamilton").Expression;
        }

        #endregion

        #region Test null exceptions from wrapping provider

        [Fact]
        public void DbQueryProvider_CreateQuery_throws_when_given_null_expression()
        {
            using (var context = new F1Context())
            {
                var queryProvider = ((IQueryable)context.Drivers).Provider;
                Assert.Equal("expression",
                             Assert.Throws<ArgumentNullException>(() => queryProvider.CreateQuery(null)).ParamName);
            }
        }

        [Fact]
        public void DbQueryProvider_CreateQuery_T_throws_when_given_null_expression()
        {
            using (var context = new F1Context())
            {
                var queryProvider = ((IQueryable)context.Drivers).Provider;
                Assert.Equal("expression",
                             Assert.Throws<ArgumentNullException>(() => queryProvider.CreateQuery<Driver>(null)).
                                 ParamName);
            }
        }

        [Fact]
        public void DbQueryProvider_Execute_throws_when_given_null_expression()
        {
            using (var context = new F1Context())
            {
                var queryProvider = ((IQueryable)context.Drivers).Provider;
                Assert.Equal("expression",
                             Assert.Throws<ArgumentNullException>(() => queryProvider.Execute(null)).ParamName);
            }
        }

        [Fact]
        public void DbQueryProvider_Execute_T_throws_when_given_null_expression()
        {
            using (var context = new F1Context())
            {
                var queryProvider = ((IQueryable)context.Drivers).Provider;
                Assert.Equal("expression",
                             Assert.Throws<ArgumentNullException>(() => queryProvider.Execute<Driver>(null)).ParamName);
            }
        }

        [Fact]
        public void NonGenericDbQueryProvider_CreateQuery_throws_when_given_null_expression()
        {
            using (var context = new F1Context())
            {
                var queryProvider = ((IQueryable)context.Set(typeof(Driver))).Provider;
                Assert.Equal("expression",
                             Assert.Throws<ArgumentNullException>(() => queryProvider.CreateQuery(null)).ParamName);
            }
        }

        [Fact]
        public void NonGenericDbQueryProvider_CreateQuery_T_throws_when_given_null_expression()
        {
            using (var context = new F1Context())
            {
                var queryProvider = ((IQueryable)context.Set(typeof(Driver))).Provider;
                Assert.Equal("expression",
                             Assert.Throws<ArgumentNullException>(() => queryProvider.CreateQuery<Driver>(null)).
                                 ParamName);
            }
        }

        [Fact]
        public void NonGenericDbQueryProvider_Execute_throws_when_given_null_expression()
        {
            using (var context = new F1Context())
            {
                var queryProvider = ((IQueryable)context.Set(typeof(Driver))).Provider;
                Assert.Equal("expression",
                             Assert.Throws<ArgumentNullException>(() => queryProvider.Execute(null)).ParamName);
            }
        }

        [Fact]
        public void NonGenericDbQueryProvider_Execute_T_throws_when_given_null_expression()
        {
            using (var context = new F1Context())
            {
                var queryProvider = ((IQueryable)context.Set(typeof(Driver))).Provider;
                Assert.Equal("expression",
                             Assert.Throws<ArgumentNullException>(() => queryProvider.Execute<Driver>(null)).ParamName);
            }
        }

        #endregion

        #region Where

        [Fact]
        public void Where_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Where_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Where_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Where_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Where_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Where_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Where_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Where_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Where_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from n in q where n.Value < 5 select n, null);
        }

        [Fact]
        public void Where_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Where_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Where_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Where_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Where_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Where_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Where_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Where_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Where_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from p in q where p.UnitsInStock == 0 select p, null);
        }

        [Fact]
        public void Where_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Where_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Where_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Where_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Where_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Where_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Where_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Where_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Where_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from p in q where p.UnitsInStock > 0 && p.UnitPrice > 3.00M select p, null);
        }

        [Fact]
        public void Where_Drilldown_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Where_Drilldown_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Where_Drilldown_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Where_Drilldown_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Where_Drilldown_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Where_Drilldown_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Where_Drilldown_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Where_Drilldown_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Where_Drilldown_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<CustomerForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from c in q where c.Region == "WA" select new { c.Id, c.Orders }, null);
        }

        [Fact]
        public void Where_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Where_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunNotSupportedLinqTest);
        }

        [Fact]
        public void Where_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Where_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void Where_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Where_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Where_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Where_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Where_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.Where((digit, index) => digit.Name.Length < index), null);
        }

        #endregion

        #region Select, SelectMany

        [Fact]
        public void Select_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Select_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Select_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Select_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Select_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Select_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Select_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Select_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Select_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from n in q select n.Value + 1, null);
        }

        [Fact]
        public void Select_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Select_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Select_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Select_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Select_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Select_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Select_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Select_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Select_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from p in q select p.ProductName, null);
        }

        [Fact]
        public void Select_Transformation_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Select_Transformation_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTest);
        }

        [Fact]
        public void Select_Transformation_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Select_Transformation_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void Select_Transformation_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Select_Transformation_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Select_Transformation_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Select_Transformation_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Select_Transformation_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            string[] strings = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };

            runner(q => from n in q select strings[n.Value], null);
        }

        [Fact]
        public void Select_Anonymous_Types_1_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Select_Anonymous_Types_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Select_Anonymous_Types_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Select_Anonymous_Types_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGeneric);
        }

        [Fact]
        public void
            Select_Anonymous_Types_1_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Select_Anonymous_Types_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Select_Anonymous_Types_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Select_Anonymous_Types_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Select_Anonymous_Types_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from w in q select new { Upper = w.Name.ToUpper(), Lower = w.Name.ToLower() }, null);
        }

        [Fact]
        public void Select_Anonymous_Types_2_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Select_Anonymous_Types_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTest);
        }

        [Fact]
        public void Select_Anonymous_Types_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Select_Anonymous_Types_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void
            Select_Anonymous_Types_2_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Select_Anonymous_Types_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Select_Anonymous_Types_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Select_Anonymous_Types_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Select_Anonymous_Types_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            string[] strings = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };

            runner(q => from n in q select new { Digit = strings[n.Value], Even = (n.Value%2 == 0) }, null);
        }

        [Fact]
        public void Select_Anonymous_Types_3_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Select_Anonymous_Types_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Select_Anonymous_Types_3_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Select_Anonymous_Types_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGeneric);
        }

        [Fact]
        public void
            Select_Anonymous_Types_3_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Select_Anonymous_Types_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Select_Anonymous_Types_3_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Select_Anonymous_Types_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Select_Anonymous_Types_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from p in q select new { p.ProductName, p.Category, Price = p.UnitPrice }, null);
        }

        [Fact]
        public void Select_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Select_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunNotSupportedLinqTest);
        }

        [Fact]
        public void Select_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Select_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void Select_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Select_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Select_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Select_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Select_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.Select((num, index) => new { Num = num.Value, InPlace = (num.Value == index) }), null);
        }

        [Fact]
        public void SelectMany_Compound_from_1_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            SelectMany_Compound_from_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void SelectMany_Compound_from_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            SelectMany_Compound_from_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGeneric);
        }

        [Fact]
        public void
            SelectMany_Compound_from_1_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            SelectMany_Compound_from_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void SelectMany_Compound_from_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            SelectMany_Compound_from_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void SelectMany_Compound_from_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((nq, pq) => from n in nq
                               from p in pq
                               where n.Value > p.UnitsInStock
                               select new { n.Value, p.UnitsInStock }, null);
        }

        [Fact]
        public void SelectMany_Compound_from_2_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            SelectMany_Compound_from_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void SelectMany_Compound_from_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            SelectMany_Compound_from_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGeneric);
        }

        [Fact]
        public void
            SelectMany_Compound_from_2_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            SelectMany_Compound_from_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void SelectMany_Compound_from_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            SelectMany_Compound_from_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void SelectMany_Compound_from_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<CustomerForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from c in q
                        from o in c.Orders
                        where o.Total < 500.00M
                        select new { CustomerId = c.Id, OrderId = o.Id, o.Total }, null);
        }

        [Fact]
        public void SelectMany_Compound_from_3_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            SelectMany_Compound_from_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void SelectMany_Compound_from_3_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            SelectMany_Compound_from_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGeneric);
        }

        [Fact]
        public void
            SelectMany_Compound_from_3_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            SelectMany_Compound_from_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void SelectMany_Compound_from_3_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            SelectMany_Compound_from_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void SelectMany_Compound_from_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<CustomerForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from c in q
                        from o in c.Orders
                        where o.OrderDate >= new DateTime(1998, 1, 1)
                        select new { CustomerId = c.Id, OrderId = o.Id, o.OrderDate }, null);
        }

        [Fact]
        public void SelectMany_from_Assignment_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            SelectMany_from_Assignment_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void SelectMany_from_Assignment_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            SelectMany_from_Assignment_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGeneric);
        }

        [Fact]
        public void
            SelectMany_from_Assignment_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            SelectMany_from_Assignment_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void SelectMany_from_Assignment_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            SelectMany_from_Assignment_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void SelectMany_from_Assignment_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<CustomerForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from c in q
                        from o in c.Orders
                        where o.Total >= 2000.0M
                        select new { CustomerId = c.Id, OrderId = o.Id, o.Total }, null);
        }

        [Fact]
        public void SelectMany_Multiple_from_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            SelectMany_Multiple_from_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void SelectMany_Multiple_from_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            SelectMany_Multiple_from_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGeneric);
        }

        [Fact]
        public void
            SelectMany_Multiple_from_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            SelectMany_Multiple_from_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void SelectMany_Multiple_from_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            SelectMany_Multiple_from_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void SelectMany_Multiple_from_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<CustomerForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            var cutoffDate = new DateTime(1997, 1, 1);

            runner(q => from c in q
                        where c.Region == "WA"
                        from o in c.Orders
                        where o.OrderDate >= cutoffDate
                        select new { CustomerId = c.Id, OrderId = o.Id }, null);
        }

        [Fact]
        public void SelectMany_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            SelectMany_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunNotSupportedLinqTest);
        }

        [Fact]
        public void SelectMany_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            SelectMany_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void SelectMany_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            SelectMany_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void SelectMany_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            SelectMany_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void SelectMany_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<CustomerForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.SelectMany((cust, custIndex) =>
                                     cust.Orders.Select(
                                         o => "Customer #" + (custIndex + 1) + " has an order with OrderID " + o.Id)),
                   null);
        }

        #endregion

        #region Take, Skip, TakeWhile, SkipWhile

        [Fact]
        public void Take_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Take_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Take_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Take_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Take_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Take_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Take_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Take_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Take_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.Take(3), null);
        }

        [Fact]
        public void Take_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Take_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Take_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Take_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Take_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Take_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Take_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Take_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        public void Take_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<CustomerForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => (from c in q
                         from o in c.Orders
                         where c.Region == "WA"
                         select new { c.Id, OrderId = o.Id, o.OrderDate }).Take(3), null);
        }

        [Fact]
        public void Skip_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Skip_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Skip_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Skip_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Skip_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Skip_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Skip_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Skip_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Skip_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.OrderBy(n => n.Id).Skip(4), null);
        }

        [Fact]
        public void Skip_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Skip_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Skip_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Skip_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Skip_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Skip_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Skip_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Skip_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Skip_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<CustomerForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => (from c in q
                         from o in c.Orders
                         where c.Region == "WA"
                         orderby c.Id
                         select new { c.Id, OrderId = o.Id, o.OrderDate }).Skip(2), null);
        }

        [Fact]
        public void TakeWhile_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            TakeWhile_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunNotSupportedLinqTest);
        }

        [Fact]
        public void TakeWhile_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            TakeWhile_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void TakeWhile_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            TakeWhile_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void TakeWhile_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            TakeWhile_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void TakeWhile_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.OrderBy(n => n.Id).TakeWhile(n => n.Value < 6), null);
        }

        [Fact]
        public void TakeWhile_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            TakeWhile_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunNotSupportedLinqTest);
        }

        [Fact]
        public void TakeWhile_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            TakeWhile_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void TakeWhile_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            TakeWhile_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void TakeWhile_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            TakeWhile_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void TakeWhile_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.OrderBy(n => n.Id).TakeWhile((n, index) => n.Value >= index), null);
        }

        [Fact]
        public void SkipWhile_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            SkipWhile_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunNotSupportedLinqTest);
        }

        [Fact]
        public void SkipWhile_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            SkipWhile_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void SkipWhile_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            SkipWhile_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void SkipWhile_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            SkipWhile_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void SkipWhile_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.OrderBy(n => n.Id).SkipWhile(n => n.Value%3 != 0), null);
        }

        [Fact]
        public void SkipWhile_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            SkipWhile_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunNotSupportedLinqTest);
        }

        [Fact]
        public void SkipWhile_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            SkipWhile_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void SkipWhile_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            SkipWhile_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void SkipWhile_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            SkipWhile_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void SkipWhile_Indexed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.OrderBy(n => n.Id).SkipWhile((n, index) => n.Value >= index), null);
        }

        #endregion

        #region OrderBy, OrderByDescending, ThenBy, ThenByDescending, Reverse

        [Fact]
        public void OrderBy_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            OrderBy_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void OrderBy_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            OrderBy_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void OrderBy_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            OrderBy_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void OrderBy_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            OrderBy_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void OrderBy_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from w in q orderby w.Name select w, null);
        }

        [Fact]
        public void OrderBy_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            OrderBy_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void OrderBy_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            OrderBy_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void OrderBy_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            OrderBy_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void OrderBy_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            OrderBy_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void OrderBy_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from w in q orderby w.Name.Length select w, null);
        }

        [Fact]
        public void OrderBy_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            OrderBy_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void OrderBy_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            OrderBy_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void OrderBy_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            OrderBy_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void OrderBy_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            OrderBy_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void OrderBy_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from p in q orderby p.ProductName select p, null);
        }

        public class CaseInsensitiveNumberComparer : IComparer<NumberForLinq>
        {
            public int Compare(NumberForLinq x, NumberForLinq y)
            {
                return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void OrderBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            OrderBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunNotSupportedLinqTest);
        }

        [Fact]
        public void OrderBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            OrderBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void OrderBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            OrderBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void OrderBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            OrderBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void OrderBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.OrderBy(a => a, new CaseInsensitiveNumberComparer()), null);
        }

        [Fact]
        public void OrderByDescending_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            OrderByDescending_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void OrderByDescending_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            OrderByDescending_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGeneric);
        }

        [Fact]
        public void
            OrderByDescending_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            OrderByDescending_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void OrderByDescending_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            OrderByDescending_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void OrderByDescending_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from d in q orderby d.Value descending select d, null);
        }

        [Fact]
        public void OrderByDescending_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            OrderByDescending_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void OrderByDescending_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            OrderByDescending_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGeneric);
        }

        [Fact]
        public void
            OrderByDescending_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            OrderByDescending_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void OrderByDescending_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            OrderByDescending_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void OrderByDescending_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from p in q orderby p.UnitsInStock descending select p, null);
        }

        [Fact]
        public void OrderByDescending_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            OrderByDescending_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTest);
        }

        [Fact]
        public void OrderByDescending_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            OrderByDescending_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void
            OrderByDescending_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            OrderByDescending_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void OrderByDescending_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            OrderByDescending_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void OrderByDescending_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.OrderByDescending(a => a, new CaseInsensitiveNumberComparer()), null);
        }

        [Fact]
        public void ThenBy_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            ThenBy_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void ThenBy_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            ThenBy_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void ThenBy_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            ThenBy_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void ThenBy_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            ThenBy_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void ThenBy_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from d in q orderby d.Name.Length , d.Value select d, null);
        }

        [Fact]
        public void ThenBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            ThenBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunNotSupportedLinqTest);
        }

        [Fact]
        public void ThenBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            ThenBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void ThenBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            ThenBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void ThenBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            ThenBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void ThenBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.OrderBy(a => a.Name.Length).ThenBy(a => a, new CaseInsensitiveNumberComparer()), null);
        }

        [Fact]
        public void ThenByDescending_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            ThenByDescending_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void ThenByDescending_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            ThenByDescending_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGeneric);
        }

        [Fact]
        public void
            ThenByDescending_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            ThenByDescending_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void ThenByDescending_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            ThenByDescending_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void ThenByDescending_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from p in q orderby p.Category , p.UnitPrice descending select p, null);
        }

        [Fact]
        public void ThenByDescending_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            ThenByDescending_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTest);
        }

        [Fact]
        public void ThenByDescending_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            ThenByDescending_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void
            ThenByDescending_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            ThenByDescending_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void ThenByDescending_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            ThenByDescending_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void ThenByDescending_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.OrderBy(a => a.Name.Length).ThenByDescending(a => a, new CaseInsensitiveNumberComparer()),
                   null);
        }

        [Fact]
        public void Reverse_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Reverse_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunNotSupportedLinqTest);
        }

        [Fact]
        public void Reverse_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Reverse_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void Reverse_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Reverse_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Reverse_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Reverse_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Reverse_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => (from d in q where d.Name.StartsWith("i") select d).Reverse(), null);
        }

        #endregion

        #region GroupBy

        [Fact]
        public void GroupBy_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            GroupBy_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void GroupBy_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            GroupBy_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void GroupBy_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            GroupBy_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void GroupBy_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            GroupBy_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void GroupBy_Simple_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from n in q
                        group n by n.Value%5
                        into g
                        select new { Remainder = g.Key, Numbers = g }, null);
        }

        [Fact]
        public void GroupBy_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            GroupBy_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void GroupBy_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            GroupBy_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void GroupBy_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            GroupBy_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void GroupBy_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            GroupBy_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void GroupBy_Simple_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from w in q
                        group w by w.Name.Length
                        into g
                        select new { FirstLetter = g.Key, Words = g }, null);
        }

        [Fact]
        public void GroupBy_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            GroupBy_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void GroupBy_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            GroupBy_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void GroupBy_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            GroupBy_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void GroupBy_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            GroupBy_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void GroupBy_Simple_3_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from p in q
                        group p by p.Category
                        into g
                        select new { Category = g.Key, Products = g }, null);
        }

        [Fact]
        public void GroupBy_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            GroupBy_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void GroupBy_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            GroupBy_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void GroupBy_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            GroupBy_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void GroupBy_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            GroupBy_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void GroupBy_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<CustomerForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from c in q
                        select new
                               {
                                   c.CompanyName,
                                   YearGroups = from o in c.Orders
                                                group o by o.OrderDate.Year
                                                into yg
                                                select new
                                                       {
                                                           Year = yg.Key,
                                                           MonthGroups = from o in yg
                                                                         group o by o.OrderDate.Month
                                                                         into mg
                                                                         select
                                                                             new { Month = mg.Key, Orders = mg }
                                                       }
                               }, null);
        }

        public class CaseInsensitiveStringComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return string.Compare(x, y, StringComparison.OrdinalIgnoreCase) != 0;
            }

            public int GetHashCode(string obj)
            {
                return obj.ToLower().GetHashCode();
            }
        }

        [Fact]
        public void GroupBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            GroupBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunNotSupportedLinqTest);
        }

        [Fact]
        public void GroupBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            GroupBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void GroupBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            GroupBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void GroupBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            GroupBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void GroupBy_Comparer_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.GroupBy(w => w.Name.Substring(0, 2), new CaseInsensitiveStringComparer()), null);
        }

        [Fact]
        public void GroupBy_Comparer_Mapped_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            GroupBy_Comparer_Mapped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTest);
        }

        [Fact]
        public void GroupBy_Comparer_Mapped_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            GroupBy_Comparer_Mapped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void
            GroupBy_Comparer_Mapped_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            GroupBy_Comparer_Mapped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void GroupBy_Comparer_Mapped_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            GroupBy_Comparer_Mapped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void GroupBy_Comparer_Mapped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(
                q => q.GroupBy(w => w.Name.Substring(0, 2), a => a.Name.ToUpper(), new CaseInsensitiveStringComparer()),
                null);
        }

        #endregion

        #region Distinct, Union, Intersect, Except

        [Fact]
        public void Distinct_1_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Distinct_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Distinct_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Distinct_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Distinct_1_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Distinct_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Distinct_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Distinct_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Distinct_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.Distinct(), null);
        }

        [Fact]
        public void Distinct_2_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Distinct_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Distinct_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Distinct_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Distinct_2_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Distinct_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Distinct_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Distinct_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Distinct_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => (from p in q select p.Category).Distinct(), null);
        }

        [Fact]
        public void Union_1_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Union_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Union_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Union_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Union_1_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Union_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Union_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Union_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Union_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((q1, q2) => q1.Union(q2), null);
        }

        [Fact]
        public void Union_2_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Union_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Union_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Union_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Union_2_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Union_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Union_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Union_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Union_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable<CustomerForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((pq, cq) => (from p in pq
                                select p.ProductName.Substring(0, 1)).Union(from c in cq
                                                                            select c.CompanyName.Substring(0, 1)), null);
        }

        [Fact]
        public void Intersect_1_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Intersect_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Intersect_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Intersect_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Intersect_1_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Intersect_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Intersect_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Intersect_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Intersect_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((q1, q2) => q1.Intersect(q2), null);
        }

        [Fact]
        public void Intersect_2_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Intersect_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Intersect_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Intersect_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Intersect_2_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Intersect_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Intersect_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Intersect_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Intersect_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable<CustomerForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((pq, cq) => (from p in pq
                                select p.ProductName.Substring(0, 1)).Intersect(from c in cq
                                                                                select c.CompanyName.Substring(0, 1)),
                   null);
        }

        [Fact]
        public void Except_1_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Except_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Except_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Except_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Except_1_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Except_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Except_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Except_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Except_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((q1, q2) => q1.Except(q2), null);
        }

        [Fact]
        public void Except_2_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Except_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Except_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Except_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Except_2_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Except_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Except_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Except_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Except_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable<CustomerForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((pq, cq) => (from p in pq
                                select p.ProductName.Substring(0, 1)).Except(from c in cq
                                                                             select c.CompanyName.Substring(0, 1)), null);
        }

        #endregion

        #region ToArray, ToList, ToDictionary, OfType

        [Fact]
        public void ToArray_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            ToArray_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void ToArray_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            ToArray_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void ToArray_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            ToArray_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void ToArray_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            ToArray_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void ToArray_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(SimpleQuery, q => ((IQueryable<NumberForLinq>)q).ToArray());
        }

        [Fact]
        public void ToList_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            ToList_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void ToList_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            ToList_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void ToList_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            ToList_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void ToList_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            ToList_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void ToList_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(SimpleQuery, q => ((IQueryable<NumberForLinq>)q).ToList());
        }

        [Fact]
        public void ToDictionary_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            ToDictionary_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void ToDictionary_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            ToDictionary_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void ToDictionary_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            ToDictionary_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void ToDictionary_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            ToDictionary_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void ToDictionary_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(SimpleQuery, q => ((IQueryable<NumberForLinq>)q).ToDictionary(n => n.Name));
        }

        private IQueryable<NumberForLinq> SimpleQuery(IQueryable<NumberForLinq> q)
        {
            return from d in q orderby d.Value descending select d;
        }

        [Fact]
        public void OfType_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            OfType_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void OfType_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            OfType_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void OfType_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            OfType_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void OfType_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            OfType_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void OfType_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.OfType<FeaturedProductForLinq>(), null);
        }

        [Fact]
        public void OfType_directly_on_non_generic_DbSet_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            RunLinqTest(c => c.Set(typeof(ProductForLinq)).OfType<FeaturedProductForLinq>().ToList(),
                        c => c.CreateObjectSet<ProductForLinq>().OfType<FeaturedProductForLinq>().ToList());
        }

        #endregion

        #region First, FirstOrDefault, ElementAt

        [Fact]
        public void First_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            First_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void First_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            First_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void First_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            First_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void First_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            First_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void First_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from p in q where p.Id == 12 select p, q => ((IQueryable<ProductForLinq>)q).First());
        }

        [Fact]
        public void First_Condition_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            First_Condition_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void First_Condition_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            First_Condition_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void First_Condition_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            First_Condition_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void First_Condition_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            First_Condition_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void First_Condition_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q, q => ((IQueryable<NumberForLinq>)q).First(s => s.Name.Substring(0, 1) == "T"));
        }

        [Fact]
        public void FirstOrDefault_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            FirstOrDefault_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void FirstOrDefault_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            FirstOrDefault_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void FirstOrDefault_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            FirstOrDefault_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void FirstOrDefault_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            FirstOrDefault_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void FirstOrDefault_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from p in q where p.ProductName == "Office Space" select p,
                   q => ((IQueryable<ProductForLinq>)q).FirstOrDefault());
        }

        [Fact]
        public void FirstOrDefault_Condition_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            FirstOrDefault_Condition_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void FirstOrDefault_Condition_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            FirstOrDefault_Condition_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGeneric);
        }

        [Fact]
        public void
            FirstOrDefault_Condition_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            FirstOrDefault_Condition_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void FirstOrDefault_Condition_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            FirstOrDefault_Condition_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void FirstOrDefault_Condition_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q, q => ((IQueryable<NumberForLinq>)q).FirstOrDefault(s => s.Name.Substring(0, 1) == "Q"));
        }

        [Fact]
        public void ElementAt_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            ElementAt_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunNotSupportedLinqTest);
        }

        [Fact]
        public void ElementAt_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            ElementAt_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void ElementAt_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            ElementAt_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void ElementAt_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            ElementAt_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void ElementAt_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from n in q where n.Value > 5 select n, q => ((IQueryable<NumberForLinq>)q).ElementAt(1));
        }

        #endregion

        #region Any, All

        [Fact]
        public void Any_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Any_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Any_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Any_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Any_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Any_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Any_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Any_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Any_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q, q => ((IQueryable<NumberForLinq>)q).Any(w => w.Name.Contains("e")));
        }

        [Fact]
        public void Any_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Any_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Any_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Any_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        [Fact]
        public void Any_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Any_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Any_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Any_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        public void Any_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from p in q
                        group p by p.Category
                        into g
                        where g.Any(p => p.UnitsInStock == 0)
                        select new { Category = g.Key, Products = g }, null);
        }

        [Fact]
        public void All_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            All_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void All_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            All_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void All_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            All_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void All_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            All_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void All_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            RunLinqTest<NumberForLinq>(q => q, q => ((IQueryable<NumberForLinq>)q).All(n => n.Value%2 == 1));
        }

        [Fact]
        public void All_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            All_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void All_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            All_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void All_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            All_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void All_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            All_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void All_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from p in q
                        group p by p.Category
                        into g
                        where g.All(p => p.UnitsInStock > 0)
                        select new { Category = g.Key, Products = g }, null);
        }

        #endregion

        #region Count, Sum, Min, Max, Average, Aggregate

        [Fact]
        public void Count_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Count_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Count_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Count_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Count_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Count_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Count_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Count_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Count_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.Distinct(), q => ((IQueryable<NumberForLinq>)q).Count());
        }

        [Fact]
        public void Count_Conditional_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Count_Conditional_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Count_Conditional_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Count_Conditional_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Count_Conditional_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Count_Conditional_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Count_Conditional_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Count_Conditional_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Count_Conditional_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q, q => ((IQueryable<NumberForLinq>)q).Count(n => n.Value%2 == 1));
        }

        [Fact]
        public void Count_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Count_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Count_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Count_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Count_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Count_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Count_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Count_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Count_Nested_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<CustomerForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from c in q select new { c.Id, OrderCount = c.Orders.Count() }, null);
        }

        [Fact]
        public void Count_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Count_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Count_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Count_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Count_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Count_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Count_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Count_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Count_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from p in q
                        group p by p.Category
                        into g
                        select new { Category = g.Key, ProductCount = g.Count() }, null);
        }

        [Fact]
        public void Sum_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Sum_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Sum_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Sum_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Sum_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Sum_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Sum_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Sum_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Sum_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.Select(n => n.Value), q => ((IQueryable<int>)q).Sum());
        }

        [Fact]
        public void Sum_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Sum_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Sum_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Sum_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Sum_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Sum_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Sum_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Sum_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Sum_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q, q => ((IQueryable<NumberForLinq>)q).Sum(n => n.Name.Length));
        }

        [Fact]
        public void Sum_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Sum_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Sum_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Sum_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Sum_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Sum_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Sum_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Sum_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Sum_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from p in q
                        group p by p.Category
                        into g
                        select new { Category = g.Key, TotalUnitsInStock = g.Sum(p => p.UnitsInStock) }, null);
        }

        [Fact]
        public void Min_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Min_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Min_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Min_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Min_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Min_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Min_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Min_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Min_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.Select(n => n.Value), q => ((IQueryable<int>)q).Min());
        }

        [Fact]
        public void Min_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Min_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Min_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Min_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Min_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Min_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Min_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Min_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Min_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q, q => ((IQueryable<NumberForLinq>)q).Min(n => n.Name.Length));
        }

        [Fact]
        public void Min_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Min_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Min_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Min_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Min_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Min_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Min_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Min_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Min_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from p in q
                        group p by p.Category
                        into g
                        select new { Category = g.Key, CheapestPrice = g.Min(p => p.UnitPrice) }, null);
        }

        [Fact]
        public void Min_Elements_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Min_Elements_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Min_Elements_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Min_Elements_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Min_Elements_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Min_Elements_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Min_Elements_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Min_Elements_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Min_Elements_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from p in q
                        group p by p.Category
                        into g
                        let minPrice = g.Min(p => p.UnitPrice)
                        select new { Category = g.Key, CheapestProducts = g.Where(p => p.UnitPrice == minPrice) }, null);
        }

        [Fact]
        public void Max_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Max_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Max_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Max_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Max_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Max_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Max_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Max_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Max_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.Select(n => n.Value), q => ((IQueryable<int>)q).Max());
        }

        [Fact]
        public void Max_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Max_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Max_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Max_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Max_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Max_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Max_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Max_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Max_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q, q => ((IQueryable<NumberForLinq>)q).Max(n => n.Name.Length));
        }

        [Fact]
        public void Max_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Max_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Max_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Max_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Max_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Max_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Max_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Max_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Max_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from p in q
                        group p by p.Category
                        into g
                        select new { Category = g.Key, MostExpensivePrice = g.Max(p => p.UnitPrice) }, null);
        }

        [Fact]
        public void Max_Elements_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Max_Elements_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Max_Elements_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Max_Elements_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Max_Elements_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Max_Elements_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Max_Elements_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Max_Elements_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Max_Elements_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from p in q
                        group p by p.Category
                        into g
                        let minPrice = g.Max(p => p.UnitPrice)
                        select new { Category = g.Key, MostExpensiveProducts = g.Where(p => p.UnitPrice == minPrice) },
                   null);
        }

        [Fact]
        public void Average_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Average_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Average_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Average_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Average_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Average_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Average_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Average_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Average_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.Select(n => n.Value), q => ((IQueryable<int>)q).Average());
        }

        [Fact]
        public void Average_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Average_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Average_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Average_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Average_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Average_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Average_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Average_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Average_Projection_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q, q => ((IQueryable<NumberForLinq>)q).Average(n => n.Name.Length));
        }

        [Fact]
        public void Average_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Average_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Average_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Average_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Average_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Average_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Average_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Average_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Average_Grouped_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => from p in q
                        group p by p.Category
                        into g
                        select new { Category = g.Key, AveragePrice = g.Average(p => p.UnitPrice) },
                   null);
        }

        [Fact]
        public void Aggregate_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Aggregate_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunNotSupportedLinqTest);
        }

        [Fact]
        public void Aggregate_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Aggregate_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void Aggregate_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Aggregate_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Aggregate_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Aggregate_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Aggregate_Simple_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            runner(q => q.Select(n => n.Value),
                   q => ((IQueryable<int>)q).Aggregate((runningProduct, nextFactor) => runningProduct*nextFactor));
        }

        [Fact]
        public void Aggregate_Seed_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Aggregate_Seed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunNotSupportedLinqTest);
        }

        [Fact]
        public void Aggregate_Seed_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Aggregate_Seed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void Aggregate_Seed_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Aggregate_Seed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Aggregate_Seed_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Aggregate_Seed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Aggregate_Seed_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>> runner)
        {
            var startBalance = 100;
            runner(q => q.Select(n => n.Value), q => ((IQueryable<int>)q).Aggregate(startBalance,
                                                                                    (balance, nextWithdrawal) =>
                                                                                    ((nextWithdrawal <= balance)
                                                                                         ? (balance - nextWithdrawal)
                                                                                         : balance)));
        }

        #endregion

        #region Concat, SequenceEqual

        [Fact]
        public void Concat_1_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Concat_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Concat_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Concat_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Concat_1_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Concat_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Concat_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Concat_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Concat_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((q1, q2) => q1.Concat(q2), null);
        }

        [Fact]
        public void Concat_2_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Concat_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Concat_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Concat_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Concat_2_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Concat_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Concat_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Concat_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Concat_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<CustomerForLinq>, IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((q1, q2) => (from c in q1 select c.CompanyName).Concat(from p in q2 select p.ProductName), null);
        }

        [Fact]
        public void EqualAll_1_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            EqualAll_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunNotSupportedLinqTest);
        }

        [Fact]
        public void EqualAll_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            EqualAll_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void EqualAll_1_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            EqualAll_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void EqualAll_1_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            EqualAll_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void EqualAll_1_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((q1, q2) => new[] { q1.SequenceEqual(q2) }.AsQueryable(), null);
        }

        [Fact]
        public void EqualAll_1b_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            EqualAll_1b_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunNotSupportedLinqTest);
        }

        [Fact]
        public void EqualAll_1b_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            EqualAll_1b_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void EqualAll_1b_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            EqualAll_1b_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void EqualAll_1b_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            EqualAll_1b_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void EqualAll_1b_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((q1, q2) => new[] { q1.Select(n => n.Value).SequenceEqual(q2.Select(n => n.Value)) }.AsQueryable(),
                   null);
        }

        [Fact]
        public void EqualAll_2_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            EqualAll_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunNotSupportedLinqTest);
        }

        [Fact]
        public void EqualAll_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            EqualAll_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGeneric);
        }

        [Fact]
        public void EqualAll_2_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            EqualAll_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void EqualAll_2_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            EqualAll_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void EqualAll_2_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable<NumberForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((q1, q2) => new[] { q1.OrderBy(n => n.Value).SequenceEqual(q2) }.AsQueryable(), null);
        }

        #endregion

        #region Join

        [Fact]
        public void Cross_Join_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Cross_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Cross_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Cross_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Cross_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Cross_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Cross_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Cross_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Cross_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<CustomerForLinq>, IQueryable<OrderForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((q1, q2) => from c in q1
                               join o in q2 on c equals o.Customer
                               select new { Customer = c, o.Id }, null);
        }

        [Fact]
        public void Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<CustomerForLinq>, IQueryable<OrderForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((q1, q2) => from c in q1
                               join o in q2 on c equals o.Customer into ps
                               select new { Customer = c, Products = ps }, null);
        }

        [Fact]
        public void Cross_Join_with_Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Cross_Join_with_Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void Cross_Join_with_Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Cross_Join_with_Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGeneric);
        }

        [Fact]
        public void
            Cross_Join_with_Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Cross_Join_with_Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Cross_Join_with_Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Cross_Join_with_Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Cross_Join_with_Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<CustomerForLinq>, IQueryable<OrderForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((q1, q2) => from c in q1
                               join o in q2 on c equals o.Customer into ps
                               from o in ps
                               select new { Customer = c, o.Id }, null);
        }

        [Fact]
        public void Left_Outer_Join_with_Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery()
        {
            Left_Outer_Join_with_Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(RunLinqTest);
        }

        [Fact]
        public void
            Left_Outer_Join_with_Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet()
        {
            Left_Outer_Join_with_Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGeneric);
        }

        [Fact]
        public void Left_Outer_Join_with_Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_with_non_generic_CreateQuery()
        {
            Left_Outer_Join_with_Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Left_Outer_Join_with_Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Left_Outer_Join_with_Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Left_Outer_Join_with_Group_Join_from_LINQ_101_returns_same_results_as_ObjectQuery_implementation(
            Action<Func<IQueryable<CustomerForLinq>, IQueryable<OrderForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((q1, q2) => from c in q1
                               join o in q2 on c equals o.Customer into ps
                               from o in ps.DefaultIfEmpty()
                               select new { Customer = c, OrderId = o == null ? -1 : o.Id }, null);
        }

        #endregion

        #region Tests that potentially require lambda translation

        [Fact]
        public void Very_simple_SelectMany_1_works()
        {
            Very_simple_SelectMany_1_works_implementation(RunLinqTest);
        }

        [Fact]
        public void Very_simple_SelectMany_1_works_using_non_generic_DbSet()
        {
            Very_simple_SelectMany_1_works_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Very_simple_SelectMany_1_works_with_non_generic_CreateQuery()
        {
            Very_simple_SelectMany_1_works_implementation(RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Very_simple_SelectMany_1_works_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Very_simple_SelectMany_1_works_implementation(RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Very_simple_SelectMany_1_works_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((nq, pq) => from n in nq
                               from p in pq
                               select n, null);
        }

        [Fact]
        public void Very_simple_SelectMany_2_works()
        {
            Very_simple_SelectMany_2_works_implementation(RunLinqTest);
        }

        [Fact]
        public void Very_simple_SelectMany_2_works_using_non_generic_DbSet()
        {
            Very_simple_SelectMany_2_works_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Very_simple_SelectMany_2_works_with_non_generic_CreateQuery()
        {
            Very_simple_SelectMany_2_works_implementation(RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Very_simple_SelectMany_2_works_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Very_simple_SelectMany_2_works_implementation(RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Very_simple_SelectMany_2_works_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((nq, pq) => from n in nq
                               from p in pq
                               select p, null);
        }

        [Fact]
        public void SelectMany_with_aggregate_works()
        {
            SelectMany_with_aggregate_works_implementation(RunLinqTest);
        }

        [Fact]
        public void SelectMany_with_aggregate_works_using_non_generic_DbSet()
        {
            SelectMany_with_aggregate_works_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void SelectMany_with_aggregate_works_with_non_generic_CreateQuery()
        {
            SelectMany_with_aggregate_works_implementation(RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void SelectMany_with_aggregate_works_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            SelectMany_with_aggregate_works_implementation(RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void SelectMany_with_aggregate_works_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((nq, pq) => from n in nq
                               from p in pq
                               select n, q => ((IQueryable<NumberForLinq>)q).Count());
        }

        [Fact]
        public void SelectMany_with_additional_predicate_in_lambda_works()
        {
            SelectMany_with_additional_predicate_in_lambda_works_implementation(RunLinqTest);
        }

        [Fact]
        public void SelectMany_with_additional_predicate_in_lambda_works_using_non_generic_DbSet()
        {
            SelectMany_with_additional_predicate_in_lambda_works_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void SelectMany_with_additional_predicate_in_lambda_works_with_non_generic_CreateQuery()
        {
            SelectMany_with_additional_predicate_in_lambda_works_implementation(RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void
            SelectMany_with_additional_predicate_in_lambda_works_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            SelectMany_with_additional_predicate_in_lambda_works_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void SelectMany_with_additional_predicate_in_lambda_works_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((nq, pq) => from n in nq
                               from p in pq.Where(r => r.Id > 0)
                               select n, null);
        }

        [Fact]
        public void SelectMany_with_additional_predicate_in_lambda_and_aggregate_works()
        {
            SelectMany_with_additional_predicate_in_lambda_and_aggregate_works_implementation(RunLinqTest);
        }

        [Fact]
        public void SelectMany_with_additional_predicate_in_lambda_and_aggregate_works_using_non_generic_DbSet()
        {
            SelectMany_with_additional_predicate_in_lambda_and_aggregate_works_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void SelectMany_with_additional_predicate_in_lambda_and_aggregate_works_with_non_generic_CreateQuery()
        {
            SelectMany_with_additional_predicate_in_lambda_and_aggregate_works_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void SelectMany_with_additional_predicate_in_lambda_and_aggregate_works_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            SelectMany_with_additional_predicate_in_lambda_and_aggregate_works_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void SelectMany_with_additional_predicate_in_lambda_and_aggregate_works_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((nq, pq) => from n in nq
                               from p in pq.Where(r => r.Id > 0)
                               select n, q => ((IQueryable<NumberForLinq>)q).Count());
        }

        [Fact]
        public void SelectMany_with_use_of_second_root_in_projection_works()
        {
            SelectMany_with_use_of_second_root_in_projection_works_implementation(RunLinqTest);
        }

        [Fact]
        public void SelectMany_with_use_of_second_root_in_projection_works_using_non_generic_DbSet()
        {
            SelectMany_with_use_of_second_root_in_projection_works_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void SelectMany_with_use_of_second_root_in_projection_works_with_non_generic_CreateQuery()
        {
            SelectMany_with_use_of_second_root_in_projection_works_implementation(RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void
            SelectMany_with_use_of_second_root_in_projection_works_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            SelectMany_with_use_of_second_root_in_projection_works_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void SelectMany_with_use_of_second_root_in_projection_works_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((nq, pq) => from n in nq
                               from p in pq.Where(r => r.Id > 0)
                               select new { n, foo = pq.Where(z => n.Id == z.Id) }, null);
        }

        // See DevDiv2 Bug 113471
        [Fact]
        public void Query_with_nested_query_in_select_clause_works()
        {
            Query_with_nested_query_in_select_clause_works_implementation(RunLinqTest);
        }

        // See DevDiv2 Bug 113471
        [Fact]
        public void Query_with_nested_query_in_select_clause_works_using_non_generic_DbSet()
        {
            Query_with_nested_query_in_select_clause_works_implementation(RunLinqTestNonGeneric);
        }

        // See DevDiv2 Bug 113471
        [Fact]
        public void Query_with_nested_query_in_select_clause_works_with_non_generic_CreateQuery()
        {
            Query_with_nested_query_in_select_clause_works_implementation(RunLinqTestWithNonGenericCreateQuery);
        }

        // See DevDiv2 Bug 113471
        [Fact]
        public void Query_with_nested_query_in_select_clause_works_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Query_with_nested_query_in_select_clause_works_implementation(RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Query_with_nested_query_in_select_clause_works_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            var parameter = 1;
            runner((nq, pq) => from p in pq
                               where p.Id == parameter
                               select nq.Where(q => q.Id == p.Id), null);
        }

        [Fact]
        public void Query_with_multiple_things_in_the_closure_works()
        {
            Query_with_multiple_things_in_the_closure_works_implementation(RunLinqTest);
        }

        [Fact]
        public void Query_with_multiple_things_in_the_closure_works_using_non_generic_DbSet()
        {
            Query_with_multiple_things_in_the_closure_works_implementation(RunLinqTestNonGeneric);
        }

        [Fact]
        public void Query_with_multiple_things_in_the_closure_works_with_non_generic_CreateQuery()
        {
            Query_with_multiple_things_in_the_closure_works_implementation(RunLinqTestWithNonGenericCreateQuery);
        }

        [Fact]
        public void Query_with_multiple_things_in_the_closure_works_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Query_with_multiple_things_in_the_closure_works_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Query_with_multiple_things_in_the_closure_works_implementation(
            Action<Func<IQueryable<NumberForLinq>, IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            var parameter1 = 1;
            var parameter2 = "Name";
            runner((nq, pq) => from p in pq
                               where p.Id == parameter1
                               select nq.Where(q => q.Id == p.Id && q.Name == parameter2), null);
        }

        // See DevDiv2 Bug 113464
        [Fact]
        public void Query_with_nested_query_and_Contains_call_in_where_clause_works()
        {
            Query_with_nested_query_and_Contains_call_in_where_clause_works_implementation(RunLinqTest);
        }

        // See DevDiv2 Bug 113464
        [Fact]
        public void Query_with_nested_query_and_Contains_call_in_where_clause_works_using_non_generic_DbSet()
        {
            Query_with_nested_query_and_Contains_call_in_where_clause_works_implementation(RunLinqTestNonGeneric);
        }

        // See DevDiv2 Bug 113464
        [Fact]
        public void Query_with_nested_query_and_Contains_call_in_where_clause_works_with_non_generic_CreateQuery()
        {
            Query_with_nested_query_and_Contains_call_in_where_clause_works_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        // See DevDiv2 Bug 113464
        [Fact]
        public void Query_with_nested_query_and_Contains_call_in_where_clause_works_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Query_with_nested_query_and_Contains_call_in_where_clause_works_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Query_with_nested_query_and_Contains_call_in_where_clause_works_implementation(
            Action<Func<IQueryable<ProductForLinq>, IQueryable<ProductForLinq>, IQueryable>, Func<IQueryable, object>>
                runner)
        {
            runner((pq1, pq2) => from c1 in pq1
                                 where (from c2 in pq2 select c2.Id).Contains(c1.Id)
                                 orderby c1.ProductName descending
                                 select c1, null);
        }

        // See DevDiv2 Bug 113464
        [Fact]
        public void Query_with_extracted_nested_query_and_Contains_call_in_where_clause_works()
        {
            Query_with_extracted_nested_query_and_Contains_call_in_where_clause_works_implementation(RunLinqTest);
        }

        // See DevDiv2 Bug 113464
        [Fact]
        public void Query_with_extracted_nested_query_and_Contains_call_in_where_clause_works_using_non_generic_DbSet()
        {
            Query_with_extracted_nested_query_and_Contains_call_in_where_clause_works_implementation(
                RunLinqTestNonGeneric);
        }

        // See DevDiv2 Bug 113464
        [Fact]
        public void
            Query_with_extracted_nested_query_and_Contains_call_in_where_clause_works_with_non_generic_CreateQuery()
        {
            Query_with_extracted_nested_query_and_Contains_call_in_where_clause_works_implementation(
                RunLinqTestWithNonGenericCreateQuery);
        }

        // See DevDiv2 Bug 113464
        [Fact]
        public void Query_with_extracted_nested_query_and_Contains_call_in_where_clause_works_using_non_generic_DbSet_with_non_generic_CreateQuery()
        {
            Query_with_extracted_nested_query_and_Contains_call_in_where_clause_works_implementation(
                RunLinqTestNonGenericWithNonGenericCreateQuery);
        }

        private void Query_with_extracted_nested_query_and_Contains_call_in_where_clause_works_implementation(
            Action
                <Func<IQueryable<ProductForLinq>, IQueryable<int>, IQueryable>,
                Func<IQueryable<NumberForLinq>, IQueryable<int>>, Func<IQueryable, object>> runner)
        {
            runner((pq1, nq) => from c in pq1
                                where nq.Contains(c.Id)
                                orderby c.ProductName descending
                                select c,
                   pq2 => from c in pq2 select c.Id, null);
        }

        // See Dev11 Bug 142974
        [Fact]
        public void Query_with_top_level_nested_query_in_select_works_if_IQueryable_workaround_is_used()
        {
            using (var context = new SimpleModelContext())
            {
                var results = context.Products.Select(p => (IQueryable<Product>)context.Products).ToList();
                Verify142974Results(results);
            }
        }

        private void Verify142974Results(List<IQueryable<Product>> results)
        {
            Assert.Equal(7, results.Count);
            foreach (var result in results)
            {
                var innerResults = result.ToList().Select(p => p.Name).ToList();
                Assert.Equal(7, innerResults.Count);
                Assert.True(innerResults.Contains("Marmite"));
                Assert.True(innerResults.Contains("Cadillac"));
            }
        }

        // See Dev11 Bug 142974
        [Fact]
        public void Query_with_top_level_nested_query_as_Set_method_in_select_works_with_workaround()
        {
            using (var context = new SimpleModelContext())
            {
                var results = context.Products.Select(p => (IQueryable<Product>)context.Set<Product>()).ToList();
                Verify142974Results(results);
            }
        }

        // See Dev11 Bug 142979
        public class ClassWithContextField
        {
            private SimpleModelContext _context;

            public List<IQueryable<int>> Query_with_top_level_nested_query_obtained_from_context_field_in_select_works()
            {
                using (_context = new SimpleModelContext())
                {
                    return _context.Products.Select(p => _context.Products.Select(p2 => p2.Id)).ToList();
                }
            }

            public List<IQueryable<int>>
                Query_with_top_level_nested_query_using_Set_method_obtained_from_context_field_in_select_works()
            {
                using (_context = new SimpleModelContext())
                {
                    return _context.Products.Select(p => _context.Set<Product>().Select(p2 => p2.Id)).ToList();
                }
            }
        }

        private void Verify142979Results(List<IQueryable<int>> results)
        {
            Assert.Equal(7, results.Count);
            foreach (var result in results)
            {
                var innerResults = result.ToList();
                Assert.Equal(7, innerResults.Count);
                Assert.True(innerResults.Contains(1));
                Assert.True(innerResults.Contains(7));
            }
        }

        // See Dev11 Bug 142979
        [Fact]
        public void Query_with_top_level_nested_query_obtained_from_context_field_in_select_works()
        {
            Verify142979Results(
                new ClassWithContextField().
                    Query_with_top_level_nested_query_obtained_from_context_field_in_select_works());
        }

        // See Dev11 Bug 142979
        [Fact]
        public void Query_with_top_level_nested_query_using_Set_method_obtained_from_context_field_in_select_works()
        {
            Verify142979Results(
                new ClassWithContextField().
                    Query_with_top_level_nested_query_using_Set_method_obtained_from_context_field_in_select_works());
        }

        // See Dev11 Bug 142979
        public class ClassWithContextProperty
        {
            private SimpleModelContext Context { get; set; }

            public List<IQueryable<int>>
                Query_with_top_level_nested_query_obtained_from_context_property_in_select_works()
            {
                using (Context = new SimpleModelContext())
                {
                    return Context.Products.Select(p => Context.Products.Select(p2 => p2.Id)).ToList();
                }
            }

            public List<IQueryable<int>>
                Query_with_top_level_nested_query_using_Set_method_obtained_from_context_property_in_select_works()
            {
                using (Context = new SimpleModelContext())
                {
                    return Context.Products.Select(p => Context.Set<Product>().Select(p2 => p2.Id)).ToList();
                }
            }
        }

        // See Dev11 Bug 142979
        [Fact]
        public void Query_with_top_level_nested_query_obtained_from_context_property_in_select_works()
        {
            Verify142979Results(
                new ClassWithContextProperty().
                    Query_with_top_level_nested_query_obtained_from_context_property_in_select_works());
        }

        // See Dev11 Bug 142979
        [Fact]
        public void Query_with_top_level_nested_query_using_Set_method_obtained_from_context_property_in_select_works()
        {
            Verify142979Results(
                new ClassWithContextProperty().
                    Query_with_top_level_nested_query_using_Set_method_obtained_from_context_property_in_select_works());
        }

        // See Dev11 Bug 142979
        public class ClassWithContextFieldAndOtherFields
        {
            private int _someInt;
            private string _someString;
            private SimpleModelContext _context1;
            private SimpleModelContext _context2;
            private SimpleModelContext _context3;
            private static int _someIntStatic;
            private static string _someStringStatic;
            private static SimpleModelContext _context1Static;
            private static SimpleModelContext _context2Static;
            private static SimpleModelContext _context3Static;

            public List<IQueryable<int>> Query_with_top_level_nested_query_obtained_from_context_field_in_select_works()
            {
                using (_context2 = new SimpleModelContext())
                {
                    return _context2.Products.Select(p => _context2.Products.Select(p2 => p2.Id)).ToList();
                }
            }

            public List<IQueryable<int>>
                Query_with_top_level_nested_query_using_Set_method_obtained_from_context_field_in_select_works()
            {
                using (_context2 = new SimpleModelContext())
                {
                    return _context2.Products.Select(p => _context2.Set<Product>().Select(p2 => p2.Id)).ToList();
                }
            }

            public List<IQueryable<int>>
                Query_with_top_level_nested_query_obtained_from_static_context_field_in_select_works()
            {
                using (_context2Static = new SimpleModelContext())
                {
                    return _context2Static.Products.Select(p => _context2Static.Products.Select(p2 => p2.Id)).ToList();
                }
            }

            public List<IQueryable<int>>
                Query_with_top_level_nested_query_using_Set_method_obtained_from_static_context_field_in_select_works()
            {
                using (_context3Static = new SimpleModelContext())
                {
                    return
                        _context3Static.Products.Select(p => _context3Static.Set<Product>().Select(p2 => p2.Id)).ToList();
                }
            }
        }

        // See Dev11 Bug 142979
        [Fact]
        public void Query_with_top_level_nested_query_obtained_from_context_field_with_in_select_with_other_fields_also_in_class_works()
        {
            Verify142979Results(
                new ClassWithContextFieldAndOtherFields().
                    Query_with_top_level_nested_query_obtained_from_context_field_in_select_works());
        }

        // See Dev11 Bug 142979
        [Fact]
        public void Query_with_top_level_nested_query_using_Set_method_obtained_from_context_field_with_other_fields_also_in_classin_select_with_other_fields_also_in_class_works()
        {
            Verify142979Results(
                new ClassWithContextFieldAndOtherFields().
                    Query_with_top_level_nested_query_using_Set_method_obtained_from_context_field_in_select_works());
        }

        // See Dev11 Bug 142979
        [Fact]
        public void Query_with_top_level_nested_query_obtained_from_static_context_field_with_in_select_with_other_fields_also_in_class_works()
        {
            Verify142979Results(
                new ClassWithContextFieldAndOtherFields().
                    Query_with_top_level_nested_query_obtained_from_static_context_field_in_select_works());
        }

        // See Dev11 Bug 142979
        [Fact]
        public void Query_with_top_level_nested_query_using_Set_method_obtained_from_static_context_field_with_other_fields_also_in_classin_select_with_other_fields_also_in_class_works()
        {
            Verify142979Results(
                new ClassWithContextFieldAndOtherFields().
                    Query_with_top_level_nested_query_using_Set_method_obtained_from_static_context_field_in_select_works());
        }

        // See Dev11 Bug 142979
        public class ClassWithContextPropertyAndOtherProperties
        {
            private int SomeInt { get; set; }
            private string SomeString { get; set; }
            private SimpleModelContext Context1 { get; set; }
            private SimpleModelContext Context2 { get; set; }
            private SimpleModelContext Context3 { get; set; }
            private static int SomeIntStatic { get; set; }
            private static string SomeStringStatic { get; set; }
            private static SimpleModelContext Context1Static { get; set; }
            private static SimpleModelContext Context2Static { get; set; }
            private static SimpleModelContext Context3Static { get; set; }

            public List<IQueryable<int>>
                Query_with_top_level_nested_query_obtained_from_context_property_in_select_works()
            {
                using (Context2 = new SimpleModelContext())
                {
                    return Context2.Products.Select(p => Context2.Products.Select(p2 => p2.Id)).ToList();
                }
            }

            public List<IQueryable<int>>
                Query_with_top_level_nested_query_using_Set_method_obtained_from_context_property_in_select_works()
            {
                using (Context2 = new SimpleModelContext())
                {
                    return Context2.Products.Select(p => Context2.Set<Product>().Select(p2 => p2.Id)).ToList();
                }
            }

            public List<IQueryable<int>>
                Query_with_top_level_nested_query_obtained_from_static_context_property_in_select_works()
            {
                using (Context2Static = new SimpleModelContext())
                {
                    return Context2Static.Products.Select(p => Context2Static.Products.Select(p2 => p2.Id)).ToList();
                }
            }

            public List<IQueryable<int>>
                Query_with_top_level_nested_query_using_Set_method_obtained_from_static_context_property_in_select_works()
            {
                using (Context3Static = new SimpleModelContext())
                {
                    return
                        Context3Static.Products.Select(p => Context3Static.Set<Product>().Select(p2 => p2.Id)).ToList();
                }
            }
        }

        // See Dev11 Bug 142979
        [Fact]
        public void Query_with_top_level_nested_query_obtained_from_context_property_with_in_select_with_other_properties_also_in_class_works()
        {
            Verify142979Results(
                new ClassWithContextPropertyAndOtherProperties().
                    Query_with_top_level_nested_query_obtained_from_context_property_in_select_works());
        }

        // See Dev11 Bug 142979
        [Fact]
        public void Query_with_top_level_nested_query_using_Set_method_obtained_from_context_property_with_other_properties_also_in_classin_select_with_other_fields_also_in_class_works()
        {
            Verify142979Results(
                new ClassWithContextPropertyAndOtherProperties().
                    Query_with_top_level_nested_query_using_Set_method_obtained_from_context_property_in_select_works());
        }

        // See Dev11 Bug 142979
        [Fact]
        public void Query_with_top_level_nested_query_obtained_from_static_context_property_with_in_select_with_other_properties_also_in_class_works()
        {
            Verify142979Results(
                new ClassWithContextPropertyAndOtherProperties().
                    Query_with_top_level_nested_query_obtained_from_static_context_property_in_select_works());
        }

        // See Dev11 Bug 142979
        [Fact]
        public void Query_with_top_level_nested_query_using_Set_method_obtained_from_static_context_property_with_other_properties_also_in_classin_select_with_other_fields_also_in_class_works()
        {
            Verify142979Results(
                new ClassWithContextPropertyAndOtherProperties().
                    Query_with_top_level_nested_query_using_Set_method_obtained_from_static_context_property_in_select_works());
        }

        #endregion

        #region Mixing ObjectQuery and DbQuery

        [Fact]
        public void Nesting_ObjectQuery_inside_a_DbQuery_Select_works()
        {
            RunWithObjectQueryNestedInsideDbQuery((l, r) => l.Select(le => r).ToList());
        }

        [Fact]
        public void Nesting_ObjectQuery_inside_a_DbQuery_SelectMany_works()
        {
            RunWithObjectQueryNestedInsideDbQuery((l, r) => l.SelectMany(le => r).ToList());
        }

        [Fact]
        public void Nesting_ObjectQuery_inside_a_DbQuery_Join_works()
        {
            RunWithObjectQueryNestedInsideDbQuery(
                (l, r) => l.Join(r, le => le.Id, re => re.Id, (le, re) => new { le, re }).ToList());
        }

        [Fact]
        public void Nesting_ObjectQuery_with_Concat_inside_DbQuery_works()
        {
            RunWithObjectQueryNestedInsideDbQuery((l, r) => l.Concat(r).ToList());
        }

        [Fact]
        public void Nesting_ObjectQuery_with_Except_inside_DbQuery_works()
        {
            RunWithObjectQueryNestedInsideDbQuery((l, r) => l.Except(r).ToList());
        }

        [Fact]
        public void Nesting_ObjectQuery_with_Intersect_inside_DbQuery_works()
        {
            RunWithObjectQueryNestedInsideDbQuery((l, r) => l.Intersect(r).ToList());
        }

        [Fact]
        public void Nesting_ObjectQuery_with_Union_inside_DbQuery_works()
        {
            RunWithObjectQueryNestedInsideDbQuery((l, r) => l.Union(r).ToList());
        }

        [Fact]
        public void Nesting_ObjectQuery_with_Contains_inside_DbQuery_works()
        {
            RunWithObjectQueryNestedInsideDbQuery((l, r) => l.Where(le => r.Contains(le)));
        }

        /// <summary>
        /// Tests that using a <see cref="DbQuery"/> with an <see cref="ObjectQuery"/> in its expression tree works.
        /// Note that the reverse (using a DbQuery inside an ObjectQuery) may not work if the expression
        /// tree given to the ObjectQuery provider contains a DbQuery node.
        /// </summary>
        /// <param name="query">The query.</param>
        private void RunWithObjectQueryNestedInsideDbQuery(
            Func<IQueryable<ProductForLinq>, IQueryable<ProductForLinq>, object> query)
        {
            using (var context = new SimpleModelForLinq())
            {
                var dbQueryProducts = context.Products;
                var objectQueryProducts =
                    ((IObjectContextAdapter)context).ObjectContext.CreateQuery<ProductForLinq>("Products");

                var dbContextResult = query(dbQueryProducts, objectQueryProducts);
                var objectContextResult = query(objectQueryProducts, objectQueryProducts);

                AssertResultsEqual(dbContextResult, objectContextResult);
            }
        }

        [Fact]
        public void Nesting_ObjectQuery_with_constants_and_Contains_inside_DbQuery_works()
        {
            using (var ctx = new SimpleModelForLinq())
            {
                var list = ((IObjectContextAdapter)ctx).ObjectContext.CreateQuery<int>("{ 1, 2, 3, 4 }");

                var dbContextResult = ctx.Products.Where(p => list.Contains(p.Id)).ToList();
                var objectContextResult =
                    GetObjectContext(ctx).CreateQuery<ProductForLinq>("Products").Where(p => list.Contains(p.Id)).ToList();

                AssertResultsEqual(dbContextResult, objectContextResult);
            }
        }

        #endregion

        #region Test runner sanity checks

        [Fact]
        public void Test_runner_detects_different_results_for_simple_scalar_results()
        {
            Test_runner_detects_different_results_for_simple_scalar_results_implementation(useNonGeneric: false);
        }

        [Fact]
        public void Test_runner_detects_different_results_for_simple_scalar_results_using_non_generic_DbSet()
        {
            Test_runner_detects_different_results_for_simple_scalar_results_implementation(useNonGeneric: true);
        }

        private void Test_runner_detects_different_results_for_simple_scalar_results_implementation(bool useNonGeneric)
        {
            try
            {
                RunLinqTest(c => (from n in (useNonGeneric
                                                 ? (IQueryable<NumberForLinq>)c.Set(typeof(NumberForLinq))
                                                 : c.Set<NumberForLinq>())
                                  orderby n.Id
                                  select n.Value).First(),
                            c =>
                            (from n in c.CreateObjectSet<NumberForLinq>() orderby n.Id select n.Value).Skip(1).First());
                Assert.True(false);
            }
            catch (AssertException ex)
            {
                Assert.Equal("Assert.Equal() Failure\r\nExpected: 5\r\nActual:   4", ex.Message);
            }
        }

        public void Test_runner_detects_different_results_for_simple_IQueryable_results()
        {
            Test_runner_detects_different_results_for_simple_IQueryable_results_implementation(useNonGeneric: false);
        }

        [Fact]
        public void Test_runner_detects_different_results_for_simple_IQueryable_results_using_non_generic_DbSet()
        {
            Test_runner_detects_different_results_for_simple_IQueryable_results_implementation(useNonGeneric: true);
        }

        private void Test_runner_detects_different_results_for_simple_IQueryable_results_implementation(
            bool useNonGeneric)
        {
            try
            {
                RunLinqTest(c => (from n in (useNonGeneric
                                                 ? (IQueryable<NumberForLinq>)c.Set(typeof(NumberForLinq))
                                                 : c.Set<NumberForLinq>())
                                  orderby n.Id
                                  select n).ToList(),
                            c =>
                            (from n in c.CreateObjectSet<NumberForLinq>() orderby n.Id where n.Id != 7 select n).ToList());
                Assert.True(false);
            }
            catch (AssertException ex)
            {
                Assert.Equal(
                    "Left 'ID: 7, Value: 6, Name: Six' different from right 'ID: 8, Value: 7, Name: Seven'",
                    ex.Message);
            }
        }

        public void Test_runner_detects_different_results_for_included_collections()
        {
            Test_runner_detects_different_results_for_included_collections_implementation(useNonGeneric: false);
        }

        [Fact]
        public void Test_runner_detects_different_results_for_included_collections_using_non_generic_DbSet()
        {
            Test_runner_detects_different_results_for_included_collections_implementation(useNonGeneric: true);
        }

        private void Test_runner_detects_different_results_for_included_collections_implementation(bool useNonGeneric)
        {
            try
            {
                RunLinqTest(c => (from n in (useNonGeneric
                                                 ? (IQueryable<CustomerForLinq>)c.Set(typeof(CustomerForLinq))
                                                 : c.Set<CustomerForLinq>())
                                  orderby n.Id
                                  select n).ToList(),
                            c =>
                            (from n in c.CreateObjectSet<CustomerForLinq>().Include("Orders") orderby n.Id select n).
                                ToList());
                Assert.True(false);
            }
            catch (AssertException ex)
            {
                Assert.Equal(
                    "Left 'ID: 1, Region: WA, CompanyName: Microsoft\r\n' different from right 'ID: 1, Region: WA, CompanyName: Microsoft\r\n  ID: 1, Total: 111.00, OrderDate: 09/03/1997 00:00:00\r\n  ID: 3, Total: 333.00, OrderDate: 09/03/1999 00:00:00\r\n'",
                    ex.Message);
            }
        }

        public void Test_runner_detects_different_results_for_projections()
        {
            Test_runner_detects_different_results_for_projections(useNonGeneric: false);
        }

        [Fact]
        public void Test_runner_detects_different_results_for_projections_using_non_generic_DbSet()
        {
            Test_runner_detects_different_results_for_projections(useNonGeneric: true);
        }

        private void Test_runner_detects_different_results_for_projections(bool useNonGeneric)
        {
            try
            {
                RunLinqTest(c => (from n in (useNonGeneric
                                                 ? (IQueryable<CustomerForLinq>)c.Set(typeof(CustomerForLinq))
                                                 : c.Set<CustomerForLinq>())
                                  where n.Id == 0
                                  select new { n.Region, Orders = n.Orders }).ToList(),
                            c =>
                            (from n in c.CreateObjectSet<CustomerForLinq>()
                             where n.Id == 1
                             select new { n.Region, Orders = n.Orders }).ToList());
                Assert.True(false);
            }
            catch (AssertException ex)
            {
                Assert.Equal("Left list had fewer elements than right list.", ex.Message);
            }
        }

        #endregion

        #region LINQ test runner

        #region RunLinqTest methods for queries with one root

        /// <summary>
        /// This is the default LINQ query executor that does (basically) a ToList.
        /// Some tests use First, Single(), etc instead.
        /// </summary>
        private static object ToListExecutor(IQueryable query)
        {
            return query.ToList<object>();
        }

        /// <summary>
        /// First, creates an DbSet object for the given entity type and then runs the given LINQ query this set as
        /// the root for the query.  Next creates an ObjectSet object for the given entity type and then runs the same LINQ
        /// query using this set as the root for the query.  The query must return an IQueryable itself, which is then
        /// evaluated with a call to ToList.  Finally, asserts that the results of the two queries are the same.
        /// </summary>
        private void RunLinqTest<TEntity>(Func<IQueryable<TEntity>, IQueryable> query,
                                          Func<IQueryable, object> executor = null) where TEntity : class
        {
            executor = executor ?? ToListExecutor;
            RunLinqTest(c => executor(query(c.Set<TEntity>())), c => executor(query(c.CreateObjectSet<TEntity>())));
        }

        /// <summary>
        /// Version of RunLinqTest that starts with a non-generic DbSet and treats is as a generic IQueryable.
        /// </summary>
        private void RunLinqTestNonGeneric<TEntity>(Func<IQueryable<TEntity>, IQueryable> query,
                                                    Func<IQueryable, object> executor = null) where TEntity : class
        {
            executor = executor ?? ToListExecutor;
            RunLinqTest(c => executor(query((IQueryable<TEntity>)(c.Set(typeof(TEntity))))),
                        c => executor(query(c.CreateObjectSet<TEntity>())));
        }

        /// <summary>
        /// Version of RunLinqTest that starts with a generic DbSet and passes built expressions to non-generic CreateQuery.
        /// </summary>
        private void RunLinqTestWithNonGenericCreateQuery<TEntity>(Func<IQueryable<TEntity>, IQueryable> query,
                                                                   Func<IQueryable, object> executor = null)
            where TEntity : class
        {
            executor = executor ?? ToListExecutor;
            RunLinqTest(c => executor(CreateQueryWithNonGenericCreateQuery(c, query)),
                        c => executor(query(c.CreateObjectSet<TEntity>())));
        }

        /// <summary>
        /// Version of RunLinqTest that starts with a non-generic DbSet and passes built expressions to non-generic CreateQuery.
        /// </summary>
        private void RunLinqTestNonGenericWithNonGenericCreateQuery<TEntity>(
            Func<IQueryable<TEntity>, IQueryable> query, Func<IQueryable, object> executor = null) where TEntity : class
        {
            executor = executor ?? ToListExecutor;
            RunLinqTest(c => executor(CreateQueryNonGenericWithNonGenericCreateQuery(c, query)),
                        c => executor(query(c.CreateObjectSet<TEntity>())));
        }

        private IQueryable CreateQueryWithNonGenericCreateQuery<TEntity>(DbContext context,
                                                                         Func<IQueryable<TEntity>, IQueryable> query)
            where TEntity : class
        {
            var set = context.Set<TEntity>();
            return ((IQueryable)set).Provider.CreateQuery(query(set).Expression);
        }

        private IQueryable CreateQueryNonGenericWithNonGenericCreateQuery<TEntity>(DbContext context,
                                                                                   Func<IQueryable<TEntity>, IQueryable>
                                                                                       query) where TEntity : class
        {
            var expression = query((IQueryable<TEntity>)context.Set(typeof(TEntity))).Expression;
            return ((IQueryable)context.Set(typeof(TEntity))).Provider.CreateQuery(expression);
        }

        #endregion

        #region RunLinqTest methods for queries with two roots

        /// <summary>
        /// First, creates DbSet objects for the given entity types and then runs the given LINQ query using these sets as
        /// the roots for the query.  Next creates ObjectSet objects for the given entity types and then runs the same LINQ
        /// query using these sets as the roots for the query.  The query must return an IQueryable itself, which is then
        /// evaluated with a call to ToList.  Finally, asserts that the results of the two queries are the same.
        /// </summary>
        private void RunLinqTest<TEntity1, TEntity2>(Func<IQueryable<TEntity1>, IQueryable<TEntity2>, IQueryable> query,
                                                     Func<IQueryable, object> executor = null)
            where TEntity1 : class
            where TEntity2 : class
        {
            executor = executor ?? ToListExecutor;
            RunLinqTest(c => executor(query(c.Set<TEntity1>(), c.Set<TEntity2>())),
                        c => executor(query(c.CreateObjectSet<TEntity1>(), c.CreateObjectSet<TEntity2>())));
        }

        /// <summary>
        /// Version of RunLinqTest that starts with a non-generic DbSet.
        /// </summary>
        private void RunLinqTestNonGeneric<TEntity1, TEntity2>(
            Func<IQueryable<TEntity1>, IQueryable<TEntity2>, IQueryable> query, Func<IQueryable, object> executor = null)
            where TEntity1 : class
            where TEntity2 : class
        {
            executor = executor ?? ToListExecutor;
            RunLinqTest(
                c =>
                executor(query((IQueryable<TEntity1>)c.Set(typeof(TEntity1)),
                               (IQueryable<TEntity2>)c.Set(typeof(TEntity2)))),
                c => executor(query(c.CreateObjectSet<TEntity1>(), c.CreateObjectSet<TEntity2>())));
        }

        /// <summary>
        /// Version of RunLinqTest that starts with a generic DbSet and passes built expressions to non-generic CreateQuery.
        /// </summary>
        private void RunLinqTestWithNonGenericCreateQuery<TEntity1, TEntity2>(
            Func<IQueryable<TEntity1>, IQueryable<TEntity2>, IQueryable> query, Func<IQueryable, object> executor = null)
            where TEntity1 : class
            where TEntity2 : class
        {
            executor = executor ?? ToListExecutor;
            RunLinqTest(c => executor(CreateQueryWithNonGenericCreateQuery(c, query)),
                        c => executor(query(c.CreateObjectSet<TEntity1>(), c.CreateObjectSet<TEntity2>())));
        }

        /// <summary>
        /// Version of RunLinqTest that starts with a non-generic DbSet and passes built expressions to non-generic CreateQuery.
        /// </summary>
        private void RunLinqTestNonGenericWithNonGenericCreateQuery<TEntity1, TEntity2>(
            Func<IQueryable<TEntity1>, IQueryable<TEntity2>, IQueryable> query, Func<IQueryable, object> executor = null)
            where TEntity1 : class
            where TEntity2 : class
        {
            executor = executor ?? ToListExecutor;
            RunLinqTest(c => executor(CreateQueryNonGenericWithNonGenericCreateQuery(c, query)),
                        c => executor(query(c.CreateObjectSet<TEntity1>(), c.CreateObjectSet<TEntity2>())));
        }

        private IQueryable CreateQueryWithNonGenericCreateQuery<TEntity1, TEntity2>(DbContext context,
                                                                                    Func
                                                                                        <IQueryable<TEntity1>,
                                                                                        IQueryable<TEntity2>, IQueryable
                                                                                        > query)
            where TEntity1 : class
            where TEntity2 : class
        {
            var set1 = context.Set<TEntity1>();
            var set2 = context.Set<TEntity2>();
            return ((IQueryable)set1).Provider.CreateQuery(query(set1, set2).Expression);
        }

        private IQueryable CreateQueryNonGenericWithNonGenericCreateQuery<TEntity1, TEntity2>(DbContext context,
                                                                                              Func
                                                                                                  <IQueryable<TEntity1>,
                                                                                                  IQueryable<TEntity2>,
                                                                                                  IQueryable> query)
            where TEntity1 : class
            where TEntity2 : class
        {
            var set1 = context.Set(typeof(TEntity1));
            var set2 = context.Set(typeof(TEntity2));
            var expression = query((IQueryable<TEntity1>)set1, (IQueryable<TEntity2>)set2).Expression;
            return ((IQueryable)set1).Provider.CreateQuery(expression);
        }

        #endregion

        #region RunLinqTest methods for queries with an extracted nested query

        /// <summary>
        /// First, creates DbSet objects for the given entity types and then runs the given LINQ query using these sets as
        /// the roots for the query.  Next creates ObjectSet objects for the given entity types and then runs the same LINQ
        /// query using these sets as the roots for the query.  The query must return an IQueryable itself, which is then
        /// evaluated with a call to ToList.  Finally, asserts that the results of the two queries are the same.
        /// </summary>
        private void RunLinqTest<TEntity1, TEntity2, TNested>(
            Func<IQueryable<TEntity1>, IQueryable<TNested>, IQueryable> query,
            Func<IQueryable<TEntity2>, IQueryable<TNested>> nestedQuery, Func<IQueryable, object> executor = null)
            where TEntity1 : class
            where TEntity2 : class
        {
            executor = executor ?? ToListExecutor;
            RunLinqTest(c => executor(query(c.Set<TEntity1>(), nestedQuery(c.Set<TEntity2>()))),
                        c => executor(query(c.CreateObjectSet<TEntity1>(), nestedQuery(c.CreateObjectSet<TEntity2>()))));
        }

        /// <summary>
        /// Version of RunLinqTest that starts with a non-generic DbSet.
        /// </summary>
        private void RunLinqTestNonGeneric<TEntity1, TEntity2, TNested>(
            Func<IQueryable<TEntity1>, IQueryable<TNested>, IQueryable> query,
            Func<IQueryable<TEntity2>, IQueryable<TNested>> nestedQuery, Func<IQueryable, object> executor = null)
            where TEntity1 : class
            where TEntity2 : class
        {
            executor = executor ?? ToListExecutor;
            RunLinqTest(
                c =>
                executor(query((IQueryable<TEntity1>)c.Set(typeof(TEntity1)),
                               nestedQuery((IQueryable<TEntity2>)c.Set(typeof(TEntity2))))),
                c => executor(query(c.CreateObjectSet<TEntity1>(), nestedQuery(c.CreateObjectSet<TEntity2>()))));
        }

        /// <summary>
        /// Version of RunLinqTest that starts with a generic DbSet and passes built expressions to non-generic CreateQuery.
        /// </summary>
        private void RunLinqTestWithNonGenericCreateQuery<TEntity1, TEntity2, TNested>(
            Func<IQueryable<TEntity1>, IQueryable<TNested>, IQueryable> query,
            Func<IQueryable<TEntity2>, IQueryable<TNested>> nestedQuery, Func<IQueryable, object> executor = null)
            where TEntity1 : class
            where TEntity2 : class
        {
            executor = executor ?? ToListExecutor;
            RunLinqTest(c => executor(CreateQueryWithNonGenericCreateQuery(c, query, nestedQuery)),
                        c => executor(query(c.CreateObjectSet<TEntity1>(), nestedQuery(c.CreateObjectSet<TEntity2>()))));
        }

        /// <summary>
        /// Version of RunLinqTest that starts with a non-generic DbSet and passes built expressions to non-generic CreateQuery.
        /// </summary>
        private void RunLinqTestNonGenericWithNonGenericCreateQuery<TEntity1, TEntity2, TNested>(
            Func<IQueryable<TEntity1>, IQueryable<TNested>, IQueryable> query,
            Func<IQueryable<TEntity2>, IQueryable<TNested>> nestedQuery, Func<IQueryable, object> executor = null)
            where TEntity1 : class
            where TEntity2 : class
        {
            executor = executor ?? ToListExecutor;
            RunLinqTest(c => executor(CreateQueryNonGenericWithNonGenericCreateQuery(c, query, nestedQuery)),
                        c => executor(query(c.CreateObjectSet<TEntity1>(), nestedQuery(c.CreateObjectSet<TEntity2>()))));
        }

        private IQueryable CreateQueryWithNonGenericCreateQuery<TEntity1, TEntity2, TNested>(DbContext context,
                                                                                             Func
                                                                                                 <IQueryable<TEntity1>,
                                                                                                 IQueryable<TNested>,
                                                                                                 IQueryable> query,
                                                                                             Func
                                                                                                 <IQueryable<TEntity2>,
                                                                                                 IQueryable<TNested>>
                                                                                                 nestedQuery)
            where TEntity1 : class
            where TEntity2 : class
        {
            var set1 = context.Set<TEntity1>();
            var set2 = context.Set<TEntity2>();
            return ((IQueryable)set1).Provider.CreateQuery(query(set1, nestedQuery(set2)).Expression);
        }

        private IQueryable CreateQueryNonGenericWithNonGenericCreateQuery<TEntity1, TEntity2, TNested>(
            DbContext context, Func<IQueryable<TEntity1>, IQueryable<TNested>, IQueryable> query,
            Func<IQueryable<TEntity2>, IQueryable<TNested>> nestedQuery)
            where TEntity1 : class
            where TEntity2 : class
        {
            var set1 = context.Set(typeof(TEntity1));
            var set2 = context.Set(typeof(TEntity2));
            var expression = query((IQueryable<TEntity1>)set1, nestedQuery((IQueryable<TEntity2>)set2)).Expression;
            return ((IQueryable)set1).Provider.CreateQuery(expression);
        }

        #endregion

        #region RunNotSupportedLinqTest for queries with one root

        /// <summary>
        /// First, creates an DbSet object for the given entity type and then runs the given LINQ query this set as
        /// the root for the query.  Next creates an ObjectSet object for the given entity type and then runs the same LINQ
        /// query using this set as the root for the query.  The query must return an IQueryable itself, which is then
        /// evaluated with a call to ToList.
        /// It is expected (and asserted) that both these queries will throw a NotSupportedException.
        /// </summary>
        private void RunNotSupportedLinqTest<TEntity>(Func<IQueryable<TEntity>, IQueryable> query,
                                                      Func<IQueryable, object> executor = null) where TEntity : class
        {
            executor = executor ?? ToListExecutor;
            RunNotSupportedLinqTest(c => executor(query(c.Set<TEntity>())),
                                    c => executor(query(c.CreateObjectSet<TEntity>())));
        }

        /// <summary>
        /// Version of RunNotSupportedLinqTest that starts with a non-generic DbSet.
        /// </summary>
        private void RunNotSupportedLinqTestNonGeneric<TEntity>(Func<IQueryable<TEntity>, IQueryable> query,
                                                                Func<IQueryable, object> executor = null)
            where TEntity : class
        {
            executor = executor ?? ToListExecutor;
            RunNotSupportedLinqTest(c => executor(query((IQueryable<TEntity>)c.Set(typeof(TEntity)))),
                                    c => executor(query(c.CreateObjectSet<TEntity>())));
        }

        /// <summary>
        /// Version of RunNotSupportedLinqTest that starts with a generic DbSet and passes built expressions to non-generic CreateQuery.
        /// </summary>
        private void RunNotSupportedLinqTestWithNonGenericCreateQuery<TEntity>(
            Func<IQueryable<TEntity>, IQueryable> query, Func<IQueryable, object> executor = null) where TEntity : class
        {
            executor = executor ?? ToListExecutor;
            RunNotSupportedLinqTest(c => executor(CreateQueryWithNonGenericCreateQuery(c, query)),
                                    c => executor(query(c.CreateObjectSet<TEntity>())));
        }

        /// <summary>
        /// Version of RunNotSupportedLinqTest that starts with a non-generic DbSet and passes built expressions to non-generic CreateQuery.
        /// </summary>
        private void RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery<TEntity>(
            Func<IQueryable<TEntity>, IQueryable> query, Func<IQueryable, object> executor = null) where TEntity : class
        {
            executor = executor ?? ToListExecutor;
            RunNotSupportedLinqTest(c => executor(CreateQueryNonGenericWithNonGenericCreateQuery(c, query)),
                                    c => executor(query(c.CreateObjectSet<TEntity>())));
        }

        #endregion

        #region RunNotSupportedLinqTest methods for queries with two roots

        /// <summary>
        /// First, creates DbSet objects for the given entity types and then runs the given LINQ query using these sets as
        /// the roots for the query.  Next creates ObjectSet objects for the given entity types and then runs the same LINQ
        /// query using these sets as the roots for the query.  The query returns an object rather than an IQueryable.
        /// It is expected (and asserted) that both these queries will throw a NotSupportedException.
        /// </summary>
        private void RunNotSupportedLinqTest<TEntity1, TEntity2>(
            Func<IQueryable<TEntity1>, IQueryable<TEntity2>, IQueryable> query, Func<IQueryable, object> executor = null)
            where TEntity1 : class
            where TEntity2 : class
        {
            executor = executor ?? ToListExecutor;
            RunNotSupportedLinqTest(c => executor(query(c.Set<TEntity1>(), c.Set<TEntity2>())),
                                    c => executor(query(c.CreateObjectSet<TEntity1>(), c.CreateObjectSet<TEntity2>())));
        }

        /// <summary>
        /// Version of RunNotSupportedLinqTest that starts with a non-generic DbSet.
        /// </summary>
        private void RunNotSupportedLinqTestNonGeneric<TEntity1, TEntity2>(
            Func<IQueryable<TEntity1>, IQueryable<TEntity2>, IQueryable> query, Func<IQueryable, object> executor = null)
            where TEntity1 : class
            where TEntity2 : class
        {
            executor = executor ?? ToListExecutor;
            RunNotSupportedLinqTest(
                c =>
                executor(query((IQueryable<TEntity1>)c.Set(typeof(TEntity1)),
                               (IQueryable<TEntity2>)c.Set(typeof(TEntity2)))),
                c => executor(query(c.CreateObjectSet<TEntity1>(), c.CreateObjectSet<TEntity2>())));
        }

        /// <summary>
        /// Version of RunNotSupportedLinqTest that starts with a generic DbSet and passes built expressions to non-generic CreateQuery.
        /// </summary>
        private void RunNotSupportedLinqTestWithNonGenericCreateQuery<TEntity1, TEntity2>(
            Func<IQueryable<TEntity1>, IQueryable<TEntity2>, IQueryable> query, Func<IQueryable, object> executor = null)
            where TEntity1 : class
            where TEntity2 : class
        {
            executor = executor ?? ToListExecutor;
            RunNotSupportedLinqTest(c => executor(CreateQueryWithNonGenericCreateQuery(c, query)),
                                    c => executor(query(c.CreateObjectSet<TEntity1>(), c.CreateObjectSet<TEntity2>())));
        }

        /// <summary>
        /// Version of RunNotSupportedLinqTest that starts with a non-generic DbSet and passes built expressions to non-generic CreateQuery.
        /// </summary>
        private void RunNotSupportedLinqTestNonGenericWithNonGenericCreateQuery<TEntity1, TEntity2>(
            Func<IQueryable<TEntity1>, IQueryable<TEntity2>, IQueryable> query, Func<IQueryable, object> executor = null)
            where TEntity1 : class
            where TEntity2 : class
        {
            executor = executor ?? ToListExecutor;
            RunNotSupportedLinqTest(c => executor(CreateQueryNonGenericWithNonGenericCreateQuery(c, query)),
                                    c => executor(query(c.CreateObjectSet<TEntity1>(), c.CreateObjectSet<TEntity2>())));
        }

        #endregion

        #region Low-level RunLinqTest and RunNotSupportedLinqTest methods

        /// <summary>
        /// Executes one delegate against a DbContext and the other against an ObjectContext and asserts that
        /// a NotSupportedException with the same message is thrown in both cases.
        /// </summary>
        private void RunNotSupportedLinqTest(Func<DbContext, object> dbQuery, Func<ObjectContext, object> obQuery)
        {
            string dbException = null;
            try
            {
                RunAgainstDbQuery(dbQuery);
                Assert.True(false);
            }
            catch (NotSupportedException ex)
            {
                dbException = ex.Message;
            }

            string obException = null;
            try
            {
                RunAgainstObQuery(obQuery);
                Assert.True(false);
            }
            catch (NotSupportedException ex)
            {
                obException = ex.Message;
            }

            Assert.Equal(obException, dbException);
        }

        /// <summary>
        /// Executes one delegate against a DbContext and the other against an ObjectContext, and then asserts that
        /// the results are the same.
        /// </summary>
        private void RunLinqTest(Func<DbContext, object> dbQuery, Func<ObjectContext, object> obQuery)
        {
            //RunAgainstObQuery(obQuery);
            AssertResultsEqual(RunAgainstDbQuery(dbQuery), RunAgainstObQuery(obQuery));
        }

        /// <summary>
        /// Executes the given delegate, which is assumed to contain a LINQ query, on a new instance
        /// of DbContext and then returns the result.
        /// </summary>
        private object RunAgainstDbQuery(Func<DbContext, object> query)
        {
            using (var context = new SimpleModelForLinq())
            {
                return query(context);
            }
        }

        /// <summary>
        /// Executes the given delegate, which is assumed to contain a LINQ query, on a new instance
        /// of ObjectContext and then returns the result.
        /// </summary>
        private object RunAgainstObQuery(Func<ObjectContext, object> query)
        {
            using (var context = new SimpleModelForLinq())
            {
                return query(GetObjectContext(context));
            }
        }

        #endregion

        #region Assertion helpers

        /// <summary>
        /// Asserts that the two objects are equal using the following heuristics:
        /// - If one is null, then the other must be null
        /// - Else if one is IEnumerable, then the other must be IEnumerable and the two enumerations are
        ///   passed to AssertEnumerationsEqual to tests their elements
        /// - Else the types of the two objects must be exactly the same
        /// - And if they are instances of BaseTypeForLinq, then the EntityEquals method is used
        /// - Else if Object.Equals indicates that they are equal, then this is trusted
        /// - Else each property is tested for equality with a recursive call to AssertResultsEqual
        /// </summary>
        private void AssertResultsEqual(object left, object right)
        {
            // To look at both query results, uncomment this:
            // Console.WriteLine("Left: {0}; Right: {1}", left, right);

            // To look at just the output of one query, uncomment this:
            // Console.WriteLine(left);

            if (left == null)
            {
                Assert.Null(right);
            }
            else if (left is IEnumerable)
            {
                // Compare each element of the enumerations
                Assert.True(right is IEnumerable);
                AssertEnumerationsEqual((IEnumerable)left, (IEnumerable)right);
            }
            else
            {
                // Types must be exactly the same
                Assert.Same(left.GetType(), right.GetType());
                if (left is BaseTypeForLinq)
                {
                    Assert.True(((BaseTypeForLinq)left).EntityEquals((BaseTypeForLinq)right),
                                String.Format("Left '{0}' different from right '{1}'", left, right));
                }
                else
                {
                    // Only do property-based comparison if Object.Equals says they are not equal
                    if (!Object.Equals(left, right))
                    {
                        var properties = left.GetType().GetProperties();
                        if (properties.Any())
                        {
                            foreach (var property in properties)
                            {
                                // Recursive call for each property value
                                AssertResultsEqual(property.GetValue(left, null), property.GetValue(right, null));
                            }
                        }
                        else
                        {
                            Assert.Equal(left, right);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Asserts that each enumeration has the same number of elements and that each element tests
        /// equal with the AssertResultsEqual method.
        /// </summary>
        private void AssertEnumerationsEqual(IEnumerable leftResult, IEnumerable rightResult)
        {
            var leftIter = leftResult.GetEnumerator();
            var rightIter = rightResult.GetEnumerator();
            while (leftIter.MoveNext())
            {
                Assert.True(rightIter.MoveNext(), "Left list had more elements than right list.");

                AssertResultsEqual(leftIter.Current, rightIter.Current);
            }
            Assert.False(rightIter.MoveNext(), "Left list had fewer elements than right list.");
        }

        #endregion

        #endregion
    }
}