// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Infrastructure;
    using System.Linq.Expressions;
    using System.Threading;
    using Moq;
    using Xunit;

    public class ObjectQueryTests
    {
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
                () =>
                new DbEnumeratorShim<object>(((IEnumerable<object>)new[] { new object() }).GetEnumerator()));
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
                () =>
                new DbEnumeratorShim<object>(((IEnumerable<object>)new[] { new object() }).GetEnumerator()));
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
        public void Execute_calls_ObjectQueryExecutionPlan_Execute()
        {
            Execute_calls_ObjectQueryExecutionPlan_Execute_implementation(
                q => q.Execute(MergeOption.NoTracking),
                m => m.Execute<object>(It.IsAny<ObjectContext>(), It.IsAny<ObjectParameterCollection>()));
            Execute_calls_ObjectQueryExecutionPlan_Execute_implementation(
                q => ((ObjectQuery)q).Execute(MergeOption.NoTracking),
                m => m.Execute<object>(It.IsAny<ObjectContext>(), It.IsAny<ObjectParameterCollection>()));
#if !NET40
            Execute_calls_ObjectQueryExecutionPlan_Execute_implementation(
                q => q.ExecuteAsync(MergeOption.NoTracking).Result,
                m => m.ExecuteAsync<object>(It.IsAny<ObjectContext>(), It.IsAny<ObjectParameterCollection>(), It.IsAny<CancellationToken>()));
            Execute_calls_ObjectQueryExecutionPlan_Execute_implementation(
                q => ((ObjectQuery)q).ExecuteAsync(MergeOption.NoTracking).Result,
                m => m.ExecuteAsync<object>(It.IsAny<ObjectContext>(), It.IsAny<ObjectParameterCollection>(), It.IsAny<CancellationToken>()));
#endif
        }

        private void Execute_calls_ObjectQueryExecutionPlan_Execute_implementation<T>(
            Func<ObjectQuery<object>, ObjectResult> execute,
            Expression<Func<ObjectQueryExecutionPlan, T>> mockCall)
        {
            var objectQuery = MockHelper.CreateMockObjectQuery((object)null).Object;
            var executionPlanMock = Mock.Get(objectQuery.QueryState.GetExecutionPlan(MergeOption.AppendOnly));

            execute(objectQuery);

            executionPlanMock.Verify(mockCall, Times.Once());
        }
    }
}
