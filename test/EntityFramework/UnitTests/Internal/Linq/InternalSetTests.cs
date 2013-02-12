// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.Linq
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;

    public class InternalSetTests : TestBase
    {
        [Fact]
        public void ExecuteSqlQuery_delegates_lazily_with_noTracking_and_buffering()
        {
            ExecuteSqlQuery_delegates_lazily(true, false);
        }

        [Fact]
        public void ExecuteSqlQuery_delegates_lazily_with_tracking_and_streaming()
        {
            ExecuteSqlQuery_delegates_lazily(false, true);
        }

        private void ExecuteSqlQuery_delegates_lazily(bool noTracking, bool streaming)
        {
            var objectContextMock = Mock.Get(MockHelper.CreateMockObjectContext<string>());
            var internalSet = CreateInternalSet(objectContextMock, "foo");

            var actualEnumerator = internalSet.ExecuteSqlQuery("", noTracking, streaming, new object[0]);

            // The query shouldn't have run yet
            objectContextMock
                .Verify(
                    m => m.ExecuteStoreQuery<string>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ExecutionOptions>(), It.IsAny<object[]>()),
                    Times.Never());

            actualEnumerator.MoveNext();

            objectContextMock
                .Verify(
                    m => m.ExecuteStoreQuery<string>(It.IsAny<string>(), It.IsAny<string>(),
                        new ExecutionOptions(noTracking? MergeOption.NoTracking : MergeOption.AppendOnly, streaming),
                        It.IsAny<object[]>()),
                    Times.Once());

            Assert.Equal("foo", actualEnumerator.Current);
        }

#if !NET40

        [Fact]
        public void ExecuteSqlQueryAsync_delegates_lazily_with_noTracking_and_buffering()
        {
            ExecuteSqlQueryAsync_delegates_lazily(true, false);
        }
        [Fact]
        public void ExecuteSqlQueryAsync_delegates_lazily_with_tracking_and_streaming()
        {
            ExecuteSqlQueryAsync_delegates_lazily(false, true);
        }

        private void ExecuteSqlQueryAsync_delegates_lazily(bool noTracking, bool streaming)
        {
            var objectContextMock = Mock.Get(MockHelper.CreateMockObjectContext<string>());
            var internalSet = CreateInternalSet(objectContextMock, "foo");

            var actualEnumerator = internalSet.ExecuteSqlQueryAsync("", noTracking, streaming, new object[0]);

            // The query shouldn't have run yet
            objectContextMock
                .Verify(
                    m =>
                    m.ExecuteStoreQueryAsync<string>(
                        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ExecutionOptions>(), It.IsAny<CancellationToken>(), It.IsAny<object[]>()),
                    Times.Never());

            actualEnumerator.MoveNextAsync(CancellationToken.None).Wait();

            objectContextMock
                .Verify(
                    m =>
                    m.ExecuteStoreQueryAsync<string>(
                        It.IsAny<string>(), It.IsAny<string>(),
                        new ExecutionOptions(noTracking ? MergeOption.NoTracking : MergeOption.AppendOnly, streaming),
                        It.IsAny<CancellationToken>(), It.IsAny<object[]>()),
                    Times.Once());

            Assert.Equal("foo", actualEnumerator.Current);
        }

#endif

        [Fact]
        public void GetEnumerator_calls_ObjectQuery_GetEnumerator()
        {
            var objectContextMock = Mock.Get(MockHelper.CreateMockObjectContext<string>());
            var internalSet = CreateInternalSet(objectContextMock, "foo");

            var enumerator = internalSet.GetEnumerator();

            var executionPlanMock =
                Mock.Get(objectContextMock.Object.CreateQuery<string>("").QueryState.GetExecutionPlan(MergeOption.NoTracking));
            executionPlanMock.Verify(
                m => m.Execute<string>(It.IsAny<ObjectContext>(), It.IsAny<ObjectParameterCollection>()), Times.Never());

            enumerator.MoveNext();

            executionPlanMock.Verify(m => m.Execute<string>(It.IsAny<ObjectContext>(), It.IsAny<ObjectParameterCollection>()), Times.Once());
        }

#if !NET40

        [Fact]
        public void GetAsyncEnumerator_calls_ObjectQuery_GetAsyncEnumerator()
        {
            var objectContextMock = Mock.Get(MockHelper.CreateMockObjectContext<string>());
            var internalSet = CreateInternalSet(objectContextMock, "foo");

            var enumerator = internalSet.GetAsyncEnumerator();

            var executionPlanMock =
                Mock.Get(objectContextMock.Object.CreateQuery<string>("").QueryState.GetExecutionPlan(MergeOption.NoTracking));
            executionPlanMock
                .Verify(
                    m =>
                    m.ExecuteAsync<string>(It.IsAny<ObjectContext>(), It.IsAny<ObjectParameterCollection>(), It.IsAny<CancellationToken>()),
                    Times.Never());

            enumerator.MoveNextAsync(CancellationToken.None).Wait();

            executionPlanMock
                .Verify(
                    m =>
                    m.ExecuteAsync<string>(It.IsAny<ObjectContext>(), It.IsAny<ObjectParameterCollection>(), It.IsAny<CancellationToken>()),
                    Times.Once());
        }

#endif

        private InternalSet<TEntity> CreateInternalSet<TEntity>(Mock<ObjectContextForMock> objectContextMock, TEntity value)
            where TEntity : class
        {
            var shaperMock = MockHelper.CreateShaperMock<TEntity>();
            shaperMock.Setup(m => m.GetEnumerator()).Returns(
                () =>
                new DbEnumeratorShim<TEntity>(((IEnumerable<TEntity>)new[] { value }).GetEnumerator()));

            var objectResultMock = new Mock<ObjectResult<TEntity>>(shaperMock.Object, null, null)
                                       {
                                           CallBase = true
                                       };

            objectContextMock
                .Setup(
                    m => m.ExecuteStoreQuery<TEntity>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ExecutionOptions>(), It.IsAny<object[]>()))
                .Returns(objectResultMock.Object);

#if !NET40
            objectContextMock
                .Setup(
                    m =>
                    m.ExecuteStoreQueryAsync<TEntity>(
                        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ExecutionOptions>(), It.IsAny<CancellationToken>(), It.IsAny<object[]>()))
                .Returns(Task.FromResult(objectResultMock.Object));
#endif

            var internalContextMock = new Mock<InternalContextForMock>(objectContextMock.Object)
                                          {
                                              CallBase = true
                                          };

            var entitySet = new EntitySet("set", "", "", "", new EntityType());
            entitySet.ChangeEntityContainerWithoutCollectionFixup(new EntityContainer("container", DataSpace.OSpace));
            internalContextMock.Setup(m => m.GetEntitySetAndBaseTypeForType(It.IsAny<Type>()))
                .Returns(new EntitySetTypePair(entitySet, typeof(TEntity)));

            return new InternalSet<TEntity>(internalContextMock.Object);
        }

        [Fact]
        public void AsStreaming_sets_Streaming_on_internal_query()
        {
            var objectContextMock = Mock.Get(MockHelper.CreateMockObjectContext<string>());
            var internalSet = CreateInternalSet(objectContextMock, "foo");
            
            Assert.False(internalSet.ObjectQuery.Streaming);
            var streamingQuery = internalSet.AsStreaming();
            Assert.True(streamingQuery.ObjectQuery.Streaming);
        }
    }
}
