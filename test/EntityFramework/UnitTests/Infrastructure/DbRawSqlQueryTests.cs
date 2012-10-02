// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Threading;
    using Xunit;

    /// <summary>
    ///     Unit tests for <see cref="DbRawSqlQuery" /> and <see cref="DbRawSqlQuery{TElement}" />.
    /// </summary>
    public class DbRawSqlQueryTests : TestBase
    {
        #region ToString tests

        [Fact]
        public void Generic_non_entity_SQL_query_ToString_returns_the_query()
        {
            var query = new DbRawSqlQuery<FakeEntity>(MockHelper.CreateInternalSqlNonSetQuery("select * from products"));

            Assert.Equal("select * from products", query.ToString());
        }

        [Fact]
        public void Generic_non_entity_SQL_query_ToString_returns_the_query_but_not_the_parameters()
        {
            var query =
                new DbRawSqlQuery<FakeEntity>(
                    MockHelper.CreateInternalSqlNonSetQuery("select * from Products where Id < {0} and CategoryId = '{1}'", 4, "Beverages"));

            Assert.Equal("select * from Products where Id < {0} and CategoryId = '{1}'", query.ToString());
        }

        [Fact]
        public void Non_generic_DbSqlQuery_ToString_returns_the_query()
        {
            var query = new DbRawSqlQuery(MockHelper.CreateInternalSqlSetQuery("select * from products"));

            Assert.Equal("select * from products", query.ToString());
        }

        [Fact]
        public void Non_generic_DbSqlQuery_ToString_returns_the_query_but_not_the_parameters()
        {
            var query =
                new DbRawSqlQuery(
                    MockHelper.CreateInternalSqlSetQuery("select * from Products where Id < {0} and CategoryId = '{1}'", 4, "Beverages"));

            Assert.Equal("select * from Products where Id < {0} and CategoryId = '{1}'", query.ToString());
        }

        [Fact]
        public void Generic_DbSqlQuery_ToString_returns_the_query()
        {
            var query = new DbRawSqlQuery<FakeEntity>(MockHelper.CreateInternalSqlSetQuery("select * from products"));

            Assert.Equal("select * from products", query.ToString());
        }

        [Fact]
        public void Generic_DbSqlQuery_ToString_returns_the_query_but_not_the_parameters()
        {
            var query =
                new DbRawSqlQuery<FakeEntity>(
                    MockHelper.CreateInternalSqlSetQuery("select * from Products where Id < {0} and CategoryId = '{1}'", 4, "Beverages"));

            Assert.Equal("select * from Products where Id < {0} and CategoryId = '{1}'", query.ToString());
        }

        #endregion

        #region DbRawSqlQuery as IListSource tests

        [Fact]
        public void Non_entity_SQL_query_ContainsListCollection_returns_false()
        {
            var query = new DbRawSqlQuery<FakeEntity>(MockHelper.CreateInternalSqlNonSetQuery("query"));

            Assert.False(((IListSource)query).ContainsListCollection);
        }

        [Fact]
        public void Non_entity_SQL_query_GetList_throws_indicating_that_binding_to_queries_is_not_allowed()
        {
            var query = new DbRawSqlQuery<Random>(MockHelper.CreateInternalSqlNonSetQuery("query"));

            Assert.Equal(
                Strings.DbQuery_BindingToDbQueryNotSupported,
                Assert.Throws<NotSupportedException>(() => ((IListSource)query).GetList()).Message);
        }

        [Fact]
        public void DbSqlQuery_ContainsListCollection_returns_false()
        {
            var query = new DbRawSqlQuery<FakeEntity>(MockHelper.CreateInternalSqlSetQuery("query"));

            Assert.False(((IListSource)query).ContainsListCollection);
        }

        [Fact]
        public void Non_generic_DbSqlQuery_ContainsListCollection_returns_false()
        {
            var query = new DbRawSqlQuery(MockHelper.CreateInternalSqlSetQuery("query"));

            Assert.False(((IListSource)query).ContainsListCollection);
        }

        [Fact]
        public void DbSqlQuery_GetList_throws_indicating_that_binding_to_queries_is_not_allowed()
        {
            var query = new DbRawSqlQuery<FakeEntity>(MockHelper.CreateInternalSqlSetQuery("query"));

            Assert.Equal(
                Strings.DbQuery_BindingToDbQueryNotSupported,
                Assert.Throws<NotSupportedException>(() => ((IListSource)query).GetList()).Message);
        }

        [Fact]
        public void Non_generic_DbSqlQuery_GetList_throws_indicating_that_binding_to_queries_is_not_allowed()
        {
            var query = new DbRawSqlQuery(MockHelper.CreateInternalSqlSetQuery("query"));

            Assert.Equal(
                Strings.DbQuery_BindingToDbQueryNotSupported,
                Assert.Throws<NotSupportedException>(() => ((IListSource)query).GetList()).Message);
        }

        #endregion

        #region IDbAsyncEnumerable extension methods

#if !NET40

        [Fact]
        public void Generic_IDbAsyncEnumerable_extension_method_equivalents_produce_the_same_results_asIEnumerable_extension_methods()
        {
            IEnumerable<int>[] testCases = new[]
                                               {
                                                   new int[] { },
                                                   new[] { 1 },
                                                   new[] { 2 },
                                                   new[] { 1, 2 },
                                                   new[] { 1, 2, 3 }
                                               };

            foreach (var testCase in testCases)
            {
                AssertSameResult(
                    testCase,
                    q => q.AllAsync(i => i < 2).Result,
                    e => e.All(i => i < 2));
                AssertSameResult(
                    testCase,
                    q => q.AnyAsync().Result,
                    e => e.Any());
                AssertSameResult(
                    testCase,
                    q => q.AnyAsync(i => i < 2).Result,
                    e => e.Any(i => i < 2));
                AssertSameResult(
                    testCase,
                    q => q.ContainsAsync(1).Result,
                    e => e.Contains(1));
                AssertSameResult(
                    testCase,
                    q => q.CountAsync().Result,
                    e => e.Count());
                AssertSameResult(
                    testCase,
                    q => q.CountAsync(i => i < 2).Result,
                    e => e.Count(i => i < 2));
                AssertSameResult(
                    testCase,
                    q => q.LongCountAsync().Result,
                    e => e.LongCount());
                AssertSameResult(
                    testCase,
                    q => q.LongCountAsync(i => i < 2).Result,
                    e => e.LongCount(i => i < 2));
                AssertSameResult(
                    testCase,
                    q => q.FirstAsync().Result,
                    e => e.First());
                AssertSameResult(
                    testCase,
                    q => q.FirstAsync(i => i < 2).Result,
                    e => e.First(i => i < 2));
                AssertSameResult(
                    testCase,
                    q =>
                        {
                            var sum = 0;
                            q.ForEachAsync(e => sum = +e).Wait();
                            return sum;
                        },
                    e =>
                        {
                            var sum = 0;
                            foreach (var i in e)
                            {
                                sum = +i;
                            }
                            return sum;
                        });
                AssertSameResult(
                    testCase,
                    q => q.MaxAsync().Result,
                    e => e.Max());
                AssertSameResult(
                    testCase,
                    q => q.MinAsync().Result,
                    e => e.Min());
                AssertSameResult(
                    testCase,
                    q => q.SingleAsync().Result,
                    e => e.Single());
                AssertSameResult(
                    testCase,
                    q => q.SingleAsync(i => i < 2).Result,
                    e => e.Single(i => i < 2));
                AssertSameResult(
                    testCase,
                    q => q.SingleOrDefaultAsync().Result,
                    e => e.SingleOrDefault());
                AssertSameResult(
                    testCase,
                    q => q.SingleOrDefaultAsync(i => i < 2).Result,
                    e => e.SingleOrDefault(i => i < 2));
                AssertSameResult(
                    testCase,
                    q => q.ToArrayAsync().Result,
                    e => e.ToArray());
                AssertSameResult(
                    testCase,
                    q => q.ToDictionaryAsync(i => i + 2).Result,
                    e => e.ToDictionary(i => i + 2));
                AssertSameResult(
                    testCase,
                    q => q.ToDictionaryAsync(i => i + 2, i => i - 1).Result,
                    e => e.ToDictionary(i => i + 2, i => i - 1));
                AssertSameResult(
                    testCase,
                    q => q.ToDictionaryAsync(i => i + 2, i => i - 1, new ModuloEqualityComparer(2)).Result,
                    e => e.ToDictionary(i => i + 2, i => i - 1, new ModuloEqualityComparer(2)));
                AssertSameResult(
                    testCase,
                    q => q.ToDictionaryAsync(i => i + 2, new ModuloEqualityComparer(2)).Result,
                    e => e.ToDictionary(i => i + 2, new ModuloEqualityComparer(2)));
                AssertSameResult(
                    testCase,
                    q => q.ToListAsync().Result,
                    e => e.ToList());

                //CancellationToken overloads
                AssertSameResult(
                    testCase,
                    q => q.AllAsync(i => i < 2, CancellationToken.None).Result,
                    e => e.All(i => i < 2));
                AssertSameResult(
                    testCase,
                    q => q.AnyAsync(CancellationToken.None).Result,
                    e => e.Any());
                AssertSameResult(
                    testCase,
                    q => q.AnyAsync(i => i < 2, CancellationToken.None).Result,
                    e => e.Any(i => i < 2));
                AssertSameResult(
                    testCase,
                    q => q.ContainsAsync(1, CancellationToken.None).Result,
                    e => e.Contains(1));
                AssertSameResult(
                    testCase,
                    q => q.CountAsync(CancellationToken.None).Result,
                    e => e.Count());
                AssertSameResult(
                    testCase,
                    q => q.CountAsync(i => i < 2, CancellationToken.None).Result,
                    e => e.Count(i => i < 2));
                AssertSameResult(
                    testCase,
                    q => q.LongCountAsync(CancellationToken.None).Result,
                    e => e.LongCount());
                AssertSameResult(
                    testCase,
                    q => q.LongCountAsync(i => i < 2, CancellationToken.None).Result,
                    e => e.LongCount(i => i < 2));
                AssertSameResult(
                    testCase,
                    q => q.FirstAsync(CancellationToken.None).Result,
                    e => e.First());
                AssertSameResult(
                    testCase,
                    q => q.FirstAsync(i => i < 2, CancellationToken.None).Result,
                    e => e.First(i => i < 2));
                AssertSameResult(
                    testCase,
                    q =>
                        {
                            var sum = 0;
                            q.ForEachAsync(e => sum = +e, CancellationToken.None).Wait();
                            return sum;
                        },
                    e =>
                        {
                            var sum = 0;
                            foreach (var i in e)
                            {
                                sum = +i;
                            }
                            return sum;
                        });
                AssertSameResult(
                    testCase,
                    q => q.MaxAsync(CancellationToken.None).Result,
                    e => e.Max());
                AssertSameResult(
                    testCase,
                    q => q.MinAsync(CancellationToken.None).Result,
                    e => e.Min());
                AssertSameResult(
                    testCase,
                    q => q.SingleAsync(CancellationToken.None).Result,
                    e => e.Single());
                AssertSameResult(
                    testCase,
                    q => q.SingleAsync(i => i < 2, CancellationToken.None).Result,
                    e => e.Single(i => i < 2));
                AssertSameResult(
                    testCase,
                    q => q.SingleOrDefaultAsync(CancellationToken.None).Result,
                    e => e.SingleOrDefault());
                AssertSameResult(
                    testCase,
                    q => q.SingleOrDefaultAsync(i => i < 2, CancellationToken.None).Result,
                    e => e.SingleOrDefault(i => i < 2));
                AssertSameResult(
                    testCase,
                    q => q.ToArrayAsync(CancellationToken.None).Result,
                    e => e.ToArray());
                AssertSameResult(
                    testCase,
                    q => q.ToDictionaryAsync(i => i + 2, CancellationToken.None).Result,
                    e => e.ToDictionary(i => i + 2));
                AssertSameResult(
                    testCase,
                    q => q.ToDictionaryAsync(i => i + 2, i => i - 1, CancellationToken.None).Result,
                    e => e.ToDictionary(i => i + 2, i => i - 1));
                AssertSameResult(
                    testCase,
                    q => q.ToDictionaryAsync(i => i + 2, i => i - 1, new ModuloEqualityComparer(2), CancellationToken.None).Result,
                    e => e.ToDictionary(i => i + 2, i => i - 1, new ModuloEqualityComparer(2)));
                AssertSameResult(
                    testCase,
                    q => q.ToDictionaryAsync(i => i + 2, new ModuloEqualityComparer(2), CancellationToken.None).Result,
                    e => e.ToDictionary(i => i + 2, new ModuloEqualityComparer(2)));
                AssertSameResult(
                    testCase,
                    q => q.ToListAsync(CancellationToken.None).Result,
                    e => e.ToList());
            }
        }

        [Fact]
        public void NonGeneric_IDbAsyncEnumerable_extension_method_equivalents_produce_the_same_results_asIEnumerable_extension_methods()
        {
            IEnumerable<object>[] testCases = new[]
                                                  {
                                                      new object[] { },
                                                      new object[] { 1 },
                                                      new object[] { 2 },
                                                      new object[] { 1, 2 },
                                                      new object[] { 1, 2, 3 }
                                                  };

            foreach (var testCase in testCases)
            {
                AssertSameResult<int>(
                    testCase,
                    q =>
                        {
                            var sum = 0;
                            q.ForEachAsync(e => sum = +(int)e).Wait();
                            return sum;
                        },
                    e =>
                        {
                            var sum = 0;
                            foreach (var i in e)
                            {
                                sum = +(int)i;
                            }
                            return sum;
                        });
                AssertSameResult(
                    testCase,
                    q => q.ToArrayAsync().Result,
                    e => e.ToArray());
                AssertSameResult(
                    testCase,
                    q => q.ToListAsync().Result,
                    e => e.ToList());

                //CancellationToken overloads
                AssertSameResult<int>(
                    testCase,
                    q =>
                        {
                            var sum = 0;
                            q.ForEachAsync(e => sum = +(int)e, CancellationToken.None).Wait();
                            return sum;
                        },
                    e =>
                        {
                            var sum = 0;
                            foreach (var i in e)
                            {
                                sum = +(int)i;
                            }
                            return sum;
                        });
                AssertSameResult(
                    testCase,
                    q => q.ToArrayAsync(CancellationToken.None).Result,
                    e => e.ToArray());
                AssertSameResult(
                    testCase,
                    q => q.ToListAsync(CancellationToken.None).Result,
                    e => e.ToList());
            }
        }

        [Fact]
        public void Generic_IDbAsyncEnumerable_extension_method_equivalents_validate_arguments()
        {
            ArgumentNullTest<int, int>(
                "action", q =>
                              {
                                  q.ForEachAsync(null);
                                  return 0;
                              });
            ArgumentNullTest<int, int>(
                "action", q =>
                              {
                                  q.ForEachAsync(null, new CancellationToken());
                                  return 0;
                              });

            ArgumentNullTest<int, Dictionary<int, int>>(
                "keySelector",
                q => q.ToDictionaryAsync<int>(null).Result);
            ArgumentNullTest<int, Dictionary<int, int>>(
                "keySelector",
                q => q.ToDictionaryAsync<int>(null, new CancellationToken()).Result);

            ArgumentNullTest<int, Dictionary<int, int>>(
                "keySelector",
                q => q.ToDictionaryAsync<int>(null, comparer: null).Result);
            ArgumentNullTest<int, Dictionary<int, int>>(
                "keySelector",
                q => q.ToDictionaryAsync<int>(null, comparer: null, cancellationToken: new CancellationToken()).Result);

            ArgumentNullTest<int, Dictionary<int, int>>(
                "keySelector",
                q => q.ToDictionaryAsync<int>(null, elementSelector: e => e).Result);
            ArgumentNullTest<int, Dictionary<int, int>>(
                "keySelector",
                q => q.ToDictionaryAsync<int>(null, elementSelector: e => e, cancellationToken: new CancellationToken()).Result);
            ArgumentNullTest<int, Dictionary<int, int>>(
                "elementSelector",
                q => q.ToDictionaryAsync(e => e, elementSelector: null).Result);
            ArgumentNullTest<int, Dictionary<int, int>>(
                "elementSelector",
                q => q.ToDictionaryAsync(e => e, elementSelector: null, cancellationToken: new CancellationToken()).Result);

            ArgumentNullTest<int, Dictionary<int, int>>(
                "keySelector",
                q => q.ToDictionaryAsync<int>(null, e => e, null).Result);
            ArgumentNullTest<int, Dictionary<int, int>>(
                "keySelector",
                q => q.ToDictionaryAsync<int>(null, e => e, null, new CancellationToken()).Result);
            ArgumentNullTest<int, Dictionary<int, int>>(
                "elementSelector",
                q => q.ToDictionaryAsync(e => e, null, null).Result);
            ArgumentNullTest<int, Dictionary<int, int>>(
                "elementSelector",
                q => q.ToDictionaryAsync(e => e, null, null, new CancellationToken()).Result);

            ArgumentNullTest<int, int>("predicate", q => q.FirstOrDefaultAsync(null).Result);
            ArgumentNullTest<int, int>("predicate", q => q.FirstOrDefaultAsync(null, new CancellationToken()).Result);

            ArgumentNullTest<int, int>("predicate", q => q.SingleAsync(null).Result);
            ArgumentNullTest<int, int>("predicate", q => q.SingleAsync(null, new CancellationToken()).Result);

            ArgumentNullTest<int, int>("predicate", q => q.SingleOrDefaultAsync(null).Result);
            ArgumentNullTest<int, int>("predicate", q => q.SingleOrDefaultAsync(null, new CancellationToken()).Result);

            ArgumentNullTest<int, bool>("predicate", q => q.AnyAsync(null).Result);
            ArgumentNullTest<int, bool>("predicate", q => q.AnyAsync(null, new CancellationToken()).Result);

            ArgumentNullTest<int, bool>("predicate", q => q.AllAsync(null).Result);
            ArgumentNullTest<int, bool>("predicate", q => q.AllAsync(null, new CancellationToken()).Result);

            ArgumentNullTest<int, int>("predicate", q => q.CountAsync(null).Result);
            ArgumentNullTest<int, int>("predicate", q => q.CountAsync(null, new CancellationToken()).Result);

            ArgumentNullTest<int, long>("predicate", q => q.LongCountAsync(null).Result);
            ArgumentNullTest<int, long>("predicate", q => q.LongCountAsync(null, new CancellationToken()).Result);
        }

        [Fact]
        public void NonGeneric_IDbAsyncEnumerable_extension_method_equivalents_validate_arguments()
        {
            ArgumentNullTest("action", q => q.ForEachAsync(null));
            ArgumentNullTest("action", q => q.ForEachAsync(null, new CancellationToken()));
        }

        private static void AssertSameResult<TElement, TResult>(
            IEnumerable<TElement> sourceEnumerable,
            Func<DbRawSqlQuery<TElement>, TResult> invokeMethodUnderTest, Func<IEnumerable<TElement>, TResult> invokeOracleMethod)
        {
            var query = CreateDbRawSqlQuery(sourceEnumerable);

            var expectedResult = default(TResult);
            Exception expectedException = null;
            try
            {
                expectedResult = invokeOracleMethod(sourceEnumerable);
            }
            catch (Exception e)
            {
                expectedException = e;
            }

            var actualResult = default(TResult);
            Exception actualException = null;
            try
            {
                actualResult = ExceptionHelpers.UnwrapAggregateExceptions(() => invokeMethodUnderTest(query));
            }
            catch (Exception e)
            {
                actualException = e;
            }

            if (expectedException != null)
            {
                Assert.NotNull(actualException);
                Assert.Equal(expectedException.GetType(), actualException.GetType());
                Assert.Equal(expectedException.Message, actualException.Message);
            }
            else
            {
                Assert.Null(actualException);
                Assert.Equal(expectedResult, actualResult);
            }
        }

        private static void AssertSameResult<TResult>(
            IEnumerable<object> sourceEnumerable,
            Func<DbRawSqlQuery, TResult> invokeMethodUnderTest, Func<IEnumerable, TResult> invokeOracleMethod)
        {
            var query = CreateDbRawSqlQuery(sourceEnumerable);

            var expectedResult = default(TResult);
            Exception expectedException = null;
            try
            {
                expectedResult = invokeOracleMethod(sourceEnumerable);
            }
            catch (Exception e)
            {
                expectedException = e;
            }

            var actualResult = default(TResult);
            Exception actualException = null;
            try
            {
                actualResult = ExceptionHelpers.UnwrapAggregateExceptions(() => invokeMethodUnderTest(query));
            }
            catch (Exception e)
            {
                actualException = e;
            }

            if (expectedException != null)
            {
                Assert.NotNull(actualException);
                Assert.Equal(expectedException.GetType(), actualException.GetType());
                Assert.Equal(expectedException.Message, actualException.Message);
            }
            else
            {
                Assert.Null(actualException);
                Assert.Equal(expectedResult, actualResult);
            }
        }

        private static void ArgumentNullTest<TElement, TResult>(
            string paramName, Func<DbRawSqlQuery<TElement>, TResult> invokeMethodUnderTest)
        {
            var query = CreateDbRawSqlQuery(new TElement[0]);
            Assert.Equal(paramName, Assert.Throws<ArgumentNullException>(() => invokeMethodUnderTest(query)).ParamName);
        }

        private static void ArgumentNullTest<TResult>(string paramName, Func<DbRawSqlQuery, TResult> invokeMethodUnderTest)
        {
            var query = CreateDbRawSqlQuery(new object[0]);
            Assert.Equal(paramName, Assert.Throws<ArgumentNullException>(() => invokeMethodUnderTest(query)).ParamName);
        }

        private static DbRawSqlQuery<TElement> CreateDbRawSqlQuery<TElement>(IEnumerable<TElement> sourceEnumerable)
        {
            var internalSqlNonSetQueryMock = MockHelper.CreateMockInternalSqlNonSetQuery("");
            var shimEnumerator = new DbEnumeratorShim<TElement>(sourceEnumerable.GetEnumerator());
            internalSqlNonSetQueryMock.Setup(m => m.GetAsyncEnumerator()).Returns(shimEnumerator);
            return new DbRawSqlQuery<TElement>(internalSqlNonSetQueryMock.Object);
        }

        private static DbRawSqlQuery CreateDbRawSqlQuery(IEnumerable<object> sourceEnumerable)
        {
            var internalSqlNonSetQueryMock = MockHelper.CreateMockInternalSqlNonSetQuery("");
            var shimEnumerator = new DbEnumeratorShim<object>(sourceEnumerable.GetEnumerator());
            internalSqlNonSetQueryMock.Setup(m => m.GetAsyncEnumerator()).Returns(shimEnumerator);
            return new DbRawSqlQuery(internalSqlNonSetQueryMock.Object);
        }

        private class ModuloEqualityComparer : IEqualityComparer<int>
        {
            private readonly int _modulo;

            public ModuloEqualityComparer(int modulo)
            {
                _modulo = modulo;
            }

            public bool Equals(int left, int right)
            {
                return left % _modulo == right % _modulo;
            }

            public int GetHashCode(int value)
            {
                return value.GetHashCode();
            }
        }

#endif

        #endregion
    }
}
