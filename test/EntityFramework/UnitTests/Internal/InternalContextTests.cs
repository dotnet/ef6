// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Migrations.History;
    using System.Threading;
    using System.Threading.Tasks;
    using DaFunc;
    using Moq;
    using Xunit;

    public class InternalContextTests : TestBase
    {
        [Fact]
        public void OnDisposing_event_is_raised_when_once_when_context_is_disposed_and_never_again()
        {
            var eventCount = 0;
            var context = new EagerInternalContext(new Mock<DbContext>().Object);

            context.OnDisposing += (_, __) => eventCount++;

            context.Dispose();
            Assert.Equal(1, eventCount);

            context.Dispose();
            Assert.Equal(1, eventCount);
        }

        [Fact]
        public void ContextKey_returns_to_string_of_context_type()
        {
            var genericFuncy = new GT<NT, NT>.GenericFuncy<GT<GT<NT, NT>, NT>, NT>();

            var internalContext = new EagerInternalContext(genericFuncy);

            Assert.Equal(genericFuncy.GetType().ToString(), internalContext.ContextKey);
        }

        private class LongTypeNameInternalContext : EagerInternalContext
        {
            public LongTypeNameInternalContext(DbContext owner)
                : base(owner)
            {
            }

            internal override string OwnerShortTypeName
            {
                get { return new string('a', 600); }
            }
        }

        [Fact]
        public void ContextKey_restricts_value_to_max_length()
        {
            var internalContext = new LongTypeNameInternalContext(new Mock<DbContext>().Object);

            Assert.Equal(new string('a', HistoryContext.ContextKeyMaxLength), internalContext.ContextKey);
        }

        [Fact]
        public void Generic_ExecuteSqlQuery_delegates_lazily_to_ExecuteStoreQuery_with_buffering()
        {
            Generic_ExecuteSqlQuery_delegates_lazily_to_ExecuteStoreQuery(false);
        }

        [Fact]
        public void Generic_ExecuteSqlQuery_delegates_lazily_to_ExecuteStoreQuery_with_streaming()
        {
            Generic_ExecuteSqlQuery_delegates_lazily_to_ExecuteStoreQuery(true);
        }

        private void Generic_ExecuteSqlQuery_delegates_lazily_to_ExecuteStoreQuery(bool streaming)
        {
            var internalContext = new Mock<InternalContextForMock<DbContext>>
                {
                    CallBase = true
                }.Object;
            var objectContextMock = Mock.Get((ObjectContextForMock)internalContext.ObjectContext);
            objectContextMock.Setup(
                m => m.ExecuteStoreQuery<Random>(It.IsAny<string>(), It.IsAny<ExecutionOptions>(), It.IsAny<object[]>()))
                             .Returns(
                                 Core.Objects.MockHelper.CreateMockObjectQuery(new Random())
                                     .Object.Execute(MergeOption.AppendOnly));

            var results = internalContext.ExecuteSqlQuery<Random>("sql", streaming, new object[] { "param" });

            objectContextMock.Verify(
                m => m.ExecuteStoreQuery<Random>(
                    "sql",
                    new ExecutionOptions(MergeOption.AppendOnly, streaming), new object[] { "param" }),
                Times.Never());

            results.MoveNext();

            objectContextMock.Verify(
                m => m.ExecuteStoreQuery<Random>(
                    "sql",
                    new ExecutionOptions(MergeOption.AppendOnly, streaming), new object[] { "param" }),
                Times.Once());
        }

        [Fact]
        public void NonGeneric_ExecuteSqlQuery_delegates_lazily_to_ExecuteStoreQuery_with_buffering()
        {
            NonGeneric_ExecuteSqlQuery_delegates_lazily_to_ExecuteStoreQuery(false);
        }

        [Fact]
        public void NonGeneric_ExecuteSqlQuery_delegates_lazily_to_ExecuteStoreQuery_with_streaming()
        {
            NonGeneric_ExecuteSqlQuery_delegates_lazily_to_ExecuteStoreQuery(true);
        }

        private void NonGeneric_ExecuteSqlQuery_delegates_lazily_to_ExecuteStoreQuery(bool streaming)
        {
            var internalContext = new Mock<InternalContextForMock<DbContext>>
                {
                    CallBase = true
                }.Object;
            var objectContextMock = Mock.Get((ObjectContextForMock)internalContext.ObjectContext);
            objectContextMock.Setup(
                m => m.ExecuteStoreQuery<Random>(It.IsAny<string>(), It.IsAny<ExecutionOptions>(), It.IsAny<object[]>()))
                             .Returns(
                                 Core.Objects.MockHelper.CreateMockObjectQuery(new Random())
                                     .Object.Execute(MergeOption.AppendOnly));

            var results = internalContext.ExecuteSqlQuery(typeof(Random), "sql", streaming, new object[] { "param" });

            objectContextMock.Verify(
                m => m.ExecuteStoreQuery<Random>(
                    "sql",
                    new ExecutionOptions(MergeOption.AppendOnly, streaming), new object[] { "param" }),
                Times.Never());

            results.MoveNext();

            objectContextMock.Verify(
                m => m.ExecuteStoreQuery<Random>(
                    "sql",
                    new ExecutionOptions(MergeOption.AppendOnly, streaming), new object[] { "param" }),
                Times.Once());
        }

#if !NET40

        [Fact]
        public void Generic_ExecuteSqlQueryAsync_delegates_lazily_to_ExecuteStoreQuery_with_buffering()
        {
            Generic_ExecuteSqlQueryAsync_delegates_lazily_to_ExecuteStoreQuery(false);
        }

        [Fact]
        public void Generic_ExecuteSqlQueryAsync_delegates_lazily_to_ExecuteStoreQuery_with_streaming()
        {
            Generic_ExecuteSqlQueryAsync_delegates_lazily_to_ExecuteStoreQuery(true);
        }

        private void Generic_ExecuteSqlQueryAsync_delegates_lazily_to_ExecuteStoreQuery(bool streaming)
        {
            var internalContext = new Mock<InternalContextForMock<DbContext>>
                {
                    CallBase = true
                }.Object;
            var objectContextMock = Mock.Get((ObjectContextForMock)internalContext.ObjectContext);
            objectContextMock.Setup(
                m =>
                m.ExecuteStoreQueryAsync<Random>(
                    It.IsAny<string>(), It.IsAny<ExecutionOptions>(), It.IsAny<CancellationToken>(), It.IsAny<object[]>()))
                             .Returns(
                                 Core.Objects.MockHelper.CreateMockObjectQuery(new Random())
                                     .Object.ExecuteAsync(MergeOption.AppendOnly));

            var results = internalContext.ExecuteSqlQueryAsync<Random>("sql", streaming, new object[] { "param" });

            objectContextMock.Verify(
                m => m.ExecuteStoreQueryAsync<Random>(
                    "sql",
                    new ExecutionOptions(MergeOption.AppendOnly, streaming), CancellationToken.None, new object[] { "param" }),
                Times.Never());

            results.MoveNextAsync(CancellationToken.None);

            objectContextMock.Verify(
                m => m.ExecuteStoreQueryAsync<Random>(
                    "sql",
                    new ExecutionOptions(MergeOption.AppendOnly, streaming), CancellationToken.None, new object[] { "param" }),
                Times.Once());
        }

        [Fact]
        public void NonGeneric_ExecuteSqlQueryAsync_delegates_lazily_to_ExecuteStoreQuery_with_buffering()
        {
            NonGeneric_ExecuteSqlQueryAsync_delegates_lazily_to_ExecuteStoreQuery(false);
        }

        [Fact]
        public void NonGeneric_ExecuteSqlQueryAsync_delegates_lazily_to_ExecuteStoreQuery_with_streaming()
        {
            NonGeneric_ExecuteSqlQueryAsync_delegates_lazily_to_ExecuteStoreQuery(true);
        }

        private void NonGeneric_ExecuteSqlQueryAsync_delegates_lazily_to_ExecuteStoreQuery(bool streaming)
        {
            var internalContext = new Mock<InternalContextForMock<DbContext>>
                {
                    CallBase = true
                }.Object;
            var objectContextMock = Mock.Get((ObjectContextForMock)internalContext.ObjectContext);
            objectContextMock.Setup(
                m =>
                m.ExecuteStoreQueryAsync<Random>(
                    It.IsAny<string>(), It.IsAny<ExecutionOptions>(), It.IsAny<CancellationToken>(), It.IsAny<object[]>()))
                             .Returns(
                                 Core.Objects.MockHelper.CreateMockObjectQuery(new Random())
                                     .Object.ExecuteAsync(MergeOption.AppendOnly));

            var results = internalContext.ExecuteSqlQueryAsync(typeof(Random), "sql", streaming, new object[] { "param" });

            objectContextMock.Verify(
                m => m.ExecuteStoreQueryAsync<Random>(
                    "sql",
                    new ExecutionOptions(MergeOption.AppendOnly, streaming), CancellationToken.None, new object[] { "param" }),
                Times.Never());

            results.MoveNextAsync(CancellationToken.None);

            objectContextMock.Verify(
                m => m.ExecuteStoreQueryAsync<Random>(
                    "sql",
                    new ExecutionOptions(MergeOption.AppendOnly, streaming), CancellationToken.None, new object[] { "param" }),
                Times.Once());
        }
#endif

        [Fact]
        public void ExecuteSqlCommand_delegates_to_ExecuteStoreCommand()
        {
            var internalContext = new Mock<InternalContextForMock<DbContext>>
                {
                    CallBase = true
                }.Object;
            var objectContextMock = Mock.Get((ObjectContextForMock)internalContext.ObjectContext);
            objectContextMock.Setup(
                m => m.ExecuteStoreCommand(It.IsAny<TransactionBehavior>(), It.IsAny<string>(), It.IsAny<object[]>()));

            var parameters = new object[] { "param" };

            internalContext.ExecuteSqlCommand(TransactionBehavior.DoNotEnsureTransaction, "sql", parameters);

            objectContextMock.Verify(
                m => m.ExecuteStoreCommand(TransactionBehavior.DoNotEnsureTransaction, "sql", parameters),
                Times.Once());
        }

#if !NET40

        [Fact]
        public void ExecuteSqlCommandAsync_delegates_to_ExecuteStoreCommandAsync()
        {
            var internalContext = new Mock<InternalContextForMock<DbContext>>
                {
                    CallBase = true
                }.Object;
            var objectContextMock = Mock.Get((ObjectContextForMock)internalContext.ObjectContext);
            objectContextMock.Setup(
                m => m.ExecuteStoreCommandAsync(It.IsAny<TransactionBehavior>(),
                    It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<object[]>())).Returns(Task.FromResult(0));
            var parameters = new object[] { "param" };
            var cancellationToken = new CancellationTokenSource().Token;

            internalContext.ExecuteSqlCommandAsync(TransactionBehavior.DoNotEnsureTransaction, "sql", cancellationToken, parameters);

            objectContextMock.Verify(
                m => m.ExecuteStoreCommandAsync(TransactionBehavior.DoNotEnsureTransaction, "sql", cancellationToken, parameters),
                Times.Once());
        }
#endif
    }
}
