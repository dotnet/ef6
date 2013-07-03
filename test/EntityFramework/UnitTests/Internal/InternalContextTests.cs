// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.History;
    using System.IO;
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

        [Fact]
        public void MigrationsConfigurationDiscovered_returns_true_if_configuration_discovered()
        {
            Assert.True(new ContextWithMigrations().InternalContext.MigrationsConfigurationDiscovered);
        }

        [Fact]
        public void MigrationsConfigurationDiscovered_returns_false_if_configuration_not_discovered()
        {
            Assert.False(new ContextWithoutMigrations().InternalContext.MigrationsConfigurationDiscovered);
        }

        [Fact]
        public void ContextKey_returns_key_from_Migrations_configuration_if_discovered()
        {
            Assert.Equal("My Key", new ContextWithMigrations().InternalContext.ContextKey);
        }

        public class DiscoverableConfiguration : DbMigrationsConfiguration<ContextWithMigrations>
        {
            public DiscoverableConfiguration()
            {
                ContextKey = "My Key";
            }
        }

        public class ContextWithMigrations : DbContext
        {
            static ContextWithMigrations()
            {
                Database.SetInitializer<ContextWithMigrations>(null);
            }
        }

        public class ContextWithoutMigrations : DbContext
        {
            static ContextWithoutMigrations()
            {
                Database.SetInitializer<ContextWithoutMigrations>(null);
            }
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

        private static void Generic_ExecuteSqlQuery_delegates_lazily_to_ExecuteStoreQuery(bool streaming)
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

        private static void NonGeneric_ExecuteSqlQuery_delegates_lazily_to_ExecuteStoreQuery(bool streaming)
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

        private static void Generic_ExecuteSqlQueryAsync_delegates_lazily_to_ExecuteStoreQuery(bool streaming)
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

        private static void NonGeneric_ExecuteSqlQueryAsync_delegates_lazily_to_ExecuteStoreQuery(bool streaming)
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
                m => m.ExecuteStoreCommand(It.IsAny<TransactionalBehavior>(), It.IsAny<string>(), It.IsAny<object[]>()));

            var parameters = new object[] { "param" };

            internalContext.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, "sql", parameters);

            objectContextMock.Verify(
                m => m.ExecuteStoreCommand(TransactionalBehavior.DoNotEnsureTransaction, "sql", parameters),
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
                m => m.ExecuteStoreCommandAsync(It.IsAny<TransactionalBehavior>(),
                    It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<object[]>())).Returns(Task.FromResult(0));
            var parameters = new object[] { "param" };
            var cancellationToken = new CancellationTokenSource().Token;

            internalContext.ExecuteSqlCommandAsync(TransactionalBehavior.DoNotEnsureTransaction, "sql", cancellationToken, parameters);

            objectContextMock.Verify(
                m => m.ExecuteStoreCommandAsync(TransactionalBehavior.DoNotEnsureTransaction, "sql", cancellationToken, parameters),
                Times.Once());
        }
#endif

        [Fact]
        public void Log_can_be_set_to_log_to_a_new_sink()
        {
            var mockDispatchers = new Mock<Dispatchers>(null);

            var context = new Mock<DbContext>().Object;
            var internalContext = new LazyInternalContext(
                context, new Mock<IInternalConnection>().Object, null, null, null, new Lazy<Dispatchers>(() => mockDispatchers.Object));

            Action<string> sink = new StringWriter().Write;
            internalContext.Log = sink;

            mockDispatchers.Verify(
                m => m.AddInterceptor(It.Is<DbCommandLogger>(l => l.Context == context && l.Sink == sink)),
                Times.Once());

            // Setting same sink again is a no-op
            internalContext.Log = sink;

            mockDispatchers.Verify(
                m => m.AddInterceptor(It.Is<DbCommandLogger>(l => l.Context == context && l.Sink == sink)),
                Times.Once());
        }

        [Fact]
        public void Setting_log_again_reoplaces_the_existing_sink()
        {
            var mockDispatchers = new Mock<Dispatchers>(null);

            var context = new Mock<DbContext>().Object;
            var internalContext = new LazyInternalContext(
                context, new Mock<IInternalConnection>().Object, null, null, null, new Lazy<Dispatchers>(() => mockDispatchers.Object));

            Action<string> sink = new StringWriter().Write;
            internalContext.Log = sink;

            Action<string> newSink = new StringWriter().Write;
            internalContext.Log = newSink;

            mockDispatchers.Verify(
                m => m.RemoveInterceptor(It.Is<DbCommandLogger>(l => l.Context == context && l.Sink == sink)),
                Times.Once());
            
            mockDispatchers.Verify(
                m => m.AddInterceptor(It.Is<DbCommandLogger>(l => l.Context == context && l.Sink == newSink)),
                Times.Once());
            
            mockDispatchers.Verify(
                m => m.AddInterceptor(It.Is<DbCommandLogger>(l => l.Context == context)),
                Times.Exactly(2));
        }

        [Fact]
        public void Log_can_be_cleared_by_setting_it_to_null()
        {
            var mockDispatchers = new Mock<Dispatchers>(null);

            var context = new Mock<DbContext>().Object;
            var internalContext = new LazyInternalContext(
                context, new Mock<IInternalConnection>().Object, null, null, null, new Lazy<Dispatchers>(() => mockDispatchers.Object));

            Action<string> sink = new StringWriter().Write;
            internalContext.Log = sink;
            internalContext.Log = null;

            mockDispatchers.Verify(
                m => m.RemoveInterceptor(It.Is<DbCommandLogger>(l => l.Context == context && l.Sink == sink)),
                Times.Once());
        }

        [Fact]
        public void Log_returns_the_current_sink_in_use_or_null()
        {
            var mockDispatchers = new Mock<Dispatchers>(null);

            var context = new Mock<DbContext>().Object;
            var internalContext = new LazyInternalContext(
                context, new Mock<IInternalConnection>().Object, null, null, null, new Lazy<Dispatchers>(() => mockDispatchers.Object));

            Assert.Null(internalContext.Log);

            Action<string> sink = new StringWriter().Write;
            internalContext.Log = sink;
            Assert.Same(sink, internalContext.Log);

            internalContext.Log = null;
            Assert.Null(internalContext.Log);
        }

        [Fact]
        public void Log_is_cleared_when_context_is_disposed()
        {
            var mockDispatchers = new Mock<Dispatchers>(null);

            var context = new Mock<DbContext>().Object;
            var internalContext = new LazyInternalContext(
                context, new Mock<IInternalConnection>().Object, null, null, null, new Lazy<Dispatchers>(() => mockDispatchers.Object));

            Action<string> sink = new StringWriter().Write;
            internalContext.Log = sink;

            internalContext.Dispose();

            mockDispatchers.Verify(
                m => m.RemoveInterceptor(It.Is<DbCommandLogger>(l => l.Context == context && l.Sink == sink)),
                Times.Once());
        }
    }
}
