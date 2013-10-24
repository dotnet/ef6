// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;

    public class ObjectQueryTests : TestBase
    {
        [Fact]
        public void Is_streaming_by_default()
        {
            var objectQuery = MockHelper.CreateMockObjectQuery((object)null).Object;
            Assert.True(objectQuery.Streaming);
        }

        [Fact]
        public void Is_buffered_if_execution_strategy_is_used()
        {
            var executionStrategyMock = new Mock<IDbExecutionStrategy>();
            executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);
            executionStrategyMock.Setup(m => m.Execute(It.IsAny<Func<ObjectResult<object>>>()))
                 .Returns<Func<ObjectResult<object>>>(f => f());

            MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
            try
            {
                var objectQuery = MockHelper.CreateMockObjectQuery((object)null).Object;
                Assert.False(objectQuery.Streaming);
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }
        }

        [Fact]
        public void MethodInfo_fields_are_initialized()
        {
            Assert.NotNull(ObjectQuery<int>.IncludeSpanMethod);
            Assert.NotNull(ObjectQuery<string>.MergeAsMethod);
        }

        [Fact]
        public void GetEnumerator_calls_Shaper_GetEnumerator_lazily()
        {
            GetEnumerator_calls_Shaper_GetEnumerator_lazily_implementation(q => ((IEnumerable<object>)q).GetEnumerator());
            GetEnumerator_calls_Shaper_GetEnumerator_lazily_implementation(q => ((IEnumerable)q).GetEnumerator());
        }

        private void GetEnumerator_calls_Shaper_GetEnumerator_lazily_implementation(Func<ObjectQuery<object>, IEnumerator> getEnumerator)
        {
            var shaperMock = MockHelper.CreateShaperMock<object>();
            shaperMock.Setup(m => m.GetEnumerator()).Returns(
                () => new DbEnumeratorShim<object>(((IEnumerable<object>)new[] { new object() }).GetEnumerator()));
            var objectQuery = MockHelper.CreateMockObjectQuery(null, shaperMock.Object).Object;

            var enumerator = getEnumerator(objectQuery);

            shaperMock.Verify(m => m.GetEnumerator(), Times.Never());

            enumerator.MoveNext();

            shaperMock.Verify(m => m.GetEnumerator(), Times.Once());
        }

#if !NET40

        [Fact]
        public void GetEnumeratorAsync_calls_Shaper_GetEnumerator_lazily()
        {
            GetEnumeratorAsync_calls_Shaper_GetEnumerator_lazily_implementation(q => ((IDbAsyncEnumerable<object>)q).GetAsyncEnumerator());
            GetEnumeratorAsync_calls_Shaper_GetEnumerator_lazily_implementation(q => ((IDbAsyncEnumerable)q).GetAsyncEnumerator());
        }

        private void GetEnumeratorAsync_calls_Shaper_GetEnumerator_lazily_implementation(
            Func<ObjectQuery<object>, IDbAsyncEnumerator> getEnumerator)
        {
            var shaperMock = MockHelper.CreateShaperMock<object>();
            shaperMock.Setup(m => m.GetEnumerator()).Returns(
                () => new DbEnumeratorShim<object>(((IEnumerable<object>)new[] { new object() }).GetEnumerator()));
            var objectQuery = MockHelper.CreateMockObjectQuery(null, shaperMock.Object).Object;

            var enumerator = getEnumerator(objectQuery);

            shaperMock.Verify(m => m.GetEnumerator(), Times.Never());

            enumerator.MoveNextAsync().Wait();

            shaperMock.Verify(m => m.GetEnumerator(), Times.Once());
        }

#endif

        [Fact]
        public void Foreach_calls_generic_GetEnumerator()
        {
            var shaperMock = MockHelper.CreateShaperMock<string>();
            shaperMock.Setup(m => m.GetEnumerator()).Returns(
                () =>
                new DbEnumeratorShim<string>(((IEnumerable<string>)new[] { "foo" }).GetEnumerator()));
            var objectQuery = MockHelper.CreateMockObjectQuery(refreshedValue: null, shaper: shaperMock.Object).Object;

            foreach (var element in objectQuery)
            {
                Assert.True(element.StartsWith("foo"));
            }
        }

        [Fact]
        public void Execute_throws_on_streaming_with_retrying_strategy()
        {
            var objectQuery = MockHelper.CreateMockObjectQuery((object)null).Object;
            objectQuery.Streaming = true;

            var executionStrategyMock = new Mock<IDbExecutionStrategy>();
            executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);

            MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
            try
            {
                Assert.Equal(
                    Strings.ExecutionStrategy_StreamingNotSupported(executionStrategyMock.Object.GetType().Name),
                    Assert.Throws<InvalidOperationException>(() => objectQuery.Execute(MergeOption.NoTracking)).Message);
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }
        }

#if !NET40

        [Fact]
        public void ExecuteAsync_throws_on_streaming_with_retrying_strategy()
        {
            var objectQuery = MockHelper.CreateMockObjectQuery((object)null).Object;
            objectQuery.Streaming = true;

            var executionStrategyMock = new Mock<IDbExecutionStrategy>();
            executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);

            MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
            try
            {
                Assert.Equal(
                    Strings.ExecutionStrategy_StreamingNotSupported(executionStrategyMock.Object.GetType().Name),
                    Assert.Throws<InvalidOperationException>(() => objectQuery.ExecuteAsync(MergeOption.NoTracking).Wait()).Message);
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }
        }

#endif

        [Fact]
        public void Execute_calls_ObjectQueryExecutionPlan_Execute_in_a_transaction_using_ExecutionStrategy()
        {
            Execute_calls_ObjectQueryExecutionPlan_Execute_in_a_transaction_using_ExecutionStrategy_implementation(
                q => q.Execute(MergeOption.NoTracking),
                async: false);
            Execute_calls_ObjectQueryExecutionPlan_Execute_in_a_transaction_using_ExecutionStrategy_implementation(
                q => ((ObjectQuery)q).Execute(MergeOption.NoTracking),
                async: false);
#if !NET40
            Execute_calls_ObjectQueryExecutionPlan_Execute_in_a_transaction_using_ExecutionStrategy_implementation(
                q => q.ExecuteAsync(MergeOption.NoTracking).Result,
                async: true);
            Execute_calls_ObjectQueryExecutionPlan_Execute_in_a_transaction_using_ExecutionStrategy_implementation(
                q => ((ObjectQuery)q).ExecuteAsync(MergeOption.NoTracking).Result,
                async: true);
#endif
        }

        private void Execute_calls_ObjectQueryExecutionPlan_Execute_in_a_transaction_using_ExecutionStrategy_implementation(
            Func<ObjectQuery<object>, ObjectResult> execute, bool async)
        {
            var objectQuery = MockHelper.CreateMockObjectQuery((object)null).Object;
            var executionPlanMock = Mock.Get(objectQuery.QueryState.GetExecutionPlan(MergeOption.AppendOnly));

            var executionStrategyMock = new Mock<IDbExecutionStrategy>();
            var objectContextMock = Mock.Get(objectQuery.QueryState.ObjectContext);

            // Verify that ExecuteInTransaction calls ObjectQueryExecutionPlan.Execute
            if (async)
            {
#if !NET40
                objectContextMock.Setup(
                    m =>
                    m.ExecuteInTransactionAsync(
                        It.IsAny<Func<Task<ObjectResult<object>>>>(), It.IsAny<IDbExecutionStrategy>(), It.IsAny<bool>(), It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                    .Returns<Func<Task<ObjectResult<object>>>, IDbExecutionStrategy, bool, bool, CancellationToken>(
                        (f, t, s, r, c) =>
                            {
                                executionPlanMock.Verify(
                                    m =>
                                    m.ExecuteAsync<object>(
                                        It.IsAny<ObjectContext>(), It.IsAny<ObjectParameterCollection>(),
                                        It.IsAny<CancellationToken>()), Times.Never());
                                var result = f().Result;
                                executionPlanMock.Verify(
                                    m =>
                                    m.ExecuteAsync<object>(
                                        It.IsAny<ObjectContext>(), It.IsAny<ObjectParameterCollection>(),
                                        It.IsAny<CancellationToken>()), Times.Once());
                                return Task.FromResult(result);
                            });
#endif
            }
            else
            {
                objectContextMock.Setup(
                    m =>
                    m.ExecuteInTransaction(
                        It.IsAny<Func<ObjectResult<object>>>(), It.IsAny<IDbExecutionStrategy>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Returns<Func<ObjectResult<object>>, IDbExecutionStrategy, bool, bool>(
                        (f, t, s, r) =>
                            {
                                executionPlanMock.Verify(
                                    m =>
                                    m.Execute<object>(It.IsAny<ObjectContext>(), It.IsAny<ObjectParameterCollection>()),
                                    Times.Never());
                                var result = f();
                                executionPlanMock.Verify(
                                    m =>
                                    m.Execute<object>(It.IsAny<ObjectContext>(), It.IsAny<ObjectParameterCollection>()),
                                    Times.Once());
                                return result;
                            });
            }

            // Verify that ExecutionStrategy.Execute calls ExecuteInTransaction
            if (async)
            {
#if !NET40
                executionStrategyMock.Setup(
                    m => m.ExecuteAsync(It.IsAny<Func<Task<ObjectResult<object>>>>(), It.IsAny<CancellationToken>()))
                    .Returns<Func<Task<ObjectResult<object>>>, CancellationToken>(
                        (f, c) =>
                            {
                                objectContextMock.Verify(
                                    m =>
                                    m.ExecuteInTransactionAsync(
                                        It.IsAny<Func<Task<ObjectResult<object>>>>(), It.IsAny<IDbExecutionStrategy>(),
                                        false, It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                                    Times.Never());
                                var result = f().Result;
                                objectContextMock.Verify(
                                    m =>
                                    m.ExecuteInTransactionAsync(
                                        It.IsAny<Func<Task<ObjectResult<object>>>>(), It.IsAny<IDbExecutionStrategy>(),
                                        false, It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                                    Times.Once());
                                return Task.FromResult(result);
                            });
#endif
            }
            else
            {
                executionStrategyMock.Setup(m => m.Execute(It.IsAny<Func<ObjectResult<object>>>()))
                    .Returns<Func<ObjectResult<object>>>(
                        f =>
                            {
                                objectContextMock.Verify(
                                    m =>
                                    m.ExecuteInTransaction(
                                        It.IsAny<Func<ObjectResult<object>>>(), It.IsAny<IDbExecutionStrategy>(), false, It.IsAny<bool>()),
                                    Times.Never());
                                var result = f();
                                objectContextMock.Verify(
                                    m =>
                                    m.ExecuteInTransaction(
                                        It.IsAny<Func<ObjectResult<object>>>(), It.IsAny<IDbExecutionStrategy>(), false, It.IsAny<bool>()),
                                    Times.Once());
                                return result;
                            });
            }

            MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
            try
            {
                execute(objectQuery);
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }

            // Finally verify that ExecutionStrategy.Execute was called
            if (async)
            {
#if !NET40
                executionStrategyMock.Verify(
                    m => m.ExecuteAsync(It.IsAny<Func<Task<ObjectResult<object>>>>(), It.IsAny<CancellationToken>()), Times.Once());
#endif
            }
            else
            {
                executionStrategyMock.Verify(m => m.Execute(It.IsAny<Func<ObjectResult<object>>>()), Times.Once());
            }
        }

#if !NET40

        [Fact]
        public void Non_generic_ObjectQuery_ExecuteAsync_throws_OperationCanceledException_if_task_is_cancelled()
        {
            var objectQuery = new Mock<ObjectQuery>{ CallBase = true }.Object;

            Assert.Throws<OperationCanceledException>(
                () => objectQuery.ExecuteAsync(0, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void Generic_ObjectQuery_ExecuteAsync_throws_OperationCanceledException_if_task_is_cancelled()
        {
            Assert.Throws<OperationCanceledException>(
                () => new ObjectQuery<int>().ExecuteAsync(MergeOption.NoTracking, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void Generic_ObjectQuery_ExecuteInternalAsync_throws_OperationCanceledException_if_task_is_cancelled()
        {
            Assert.Throws<OperationCanceledException>(
                () => new ObjectQuery<int>().ExecuteInternalAsync(MergeOption.NoTracking, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

#endif
    }
}
