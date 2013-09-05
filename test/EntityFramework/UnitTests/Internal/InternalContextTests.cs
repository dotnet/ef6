// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Resources;
    using System.Data.Entity.SqlServer;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DaFunc;
    using Moq;
    using Xunit;

    public class InternalContextTests : TestBase
    {
        [Fact]
        public void MethodInfo_fields_are_initialized()
        {
            Assert.NotNull(InternalContext.CreateInitializationActionMethod);
            Assert.NotNull(InternalContext.CreateObjectAsObjectMethod);
#if !NET40
            Assert.NotNull(InternalContext.ExecuteSqlQueryAsIDbAsyncEnumeratorMethod);
#endif
            Assert.NotNull(InternalContext.ExecuteSqlQueryAsIEnumeratorMethod);
        }

        public class OnDisposing : TestBase
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
        }

        public class MigrationsConfiguration : TestBase
        {
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

            [Fact]
            public void DefaultContextKey_returns_context_type_string_even_if_configuration_is_discovered()
            {
                Assert.Equal(
                    "System.Data.Entity.Internal.InternalContextTests+MigrationsConfiguration+ContextWithMigrations",
                    new ContextWithMigrations().InternalContext.DefaultContextKey);
            }

            public class DiscoverableConfiguration : DbMigrationsConfiguration<ContextWithMigrations>
            {
                public static readonly Func<DbConnection, string, HistoryContext> NewFactory = (c, s) => new HistoryContext(c, s);

                public DiscoverableConfiguration()
                {
                    ContextKey = "My Key";
                    SetHistoryContextFactory(SqlProviderServices.ProviderInvariantName, NewFactory);
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
        }

        public class HistoryContextFactory : TestBase
        {
            [Fact]
            public void HistoryContextFactory_returns_factory_from_Migrations_configuration_if_discovered()
            {
                Assert.Same(
                    MigrationsConfiguration.DiscoverableConfiguration.NewFactory,
                    new MigrationsConfiguration.ContextWithMigrations().InternalContext.HistoryContextFactory);
            }

            [Fact]
            public void HistoryContextFactory_returns_factory_for_given_provider_invariant_name_if_registered()
            {
                Func<DbConnection, string, HistoryContext> newFactory = (c, s) => new HistoryContext(c, s);
                try
                {
                    MutableResolver.AddResolver<Func<DbConnection, string, HistoryContext>>(
                        new SingletonDependencyResolver<Func<DbConnection, string, HistoryContext>>(
                            newFactory, SqlProviderServices.ProviderInvariantName));

                    Assert.Same(newFactory, new MigrationsConfiguration.ContextWithoutMigrations().InternalContext.HistoryContextFactory);
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                }
            }

            [Fact]
            public void HistoryContextFactory_returns_default_factory_if_none_other_found()
            {
                Assert.Same(
                    HistoryContext.DefaultFactory,
                    new MigrationsConfiguration.ContextWithoutMigrations().InternalContext.HistoryContextFactory);
            }
        }

        public class ExecuteSqlQuery : TestBase
        {
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
        }

        public class ExecuteSqlCommand : TestBase
        {
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
                    m => m.ExecuteStoreCommandAsync(
                        It.IsAny<TransactionalBehavior>(),
                        It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<object[]>())).Returns(Task.FromResult(0));
                var parameters = new object[] { "param" };
                var cancellationToken = new CancellationTokenSource().Token;

                internalContext.ExecuteSqlCommandAsync(TransactionalBehavior.DoNotEnsureTransaction, "sql", cancellationToken, parameters);

                objectContextMock.Verify(
                    m => m.ExecuteStoreCommandAsync(TransactionalBehavior.DoNotEnsureTransaction, "sql", cancellationToken, parameters),
                    Times.Once());
            }
#endif
        }

        public class Log : TestBase
        {
            [Fact]
            public void Log_can_be_set_to_log_to_a_new_sink()
            {
                var mockDispatchers = new Mock<DbDispatchers>();

                var context = new Mock<DbContext>().Object;
                var internalContext = new LazyInternalContext(
                    context, new Mock<IInternalConnection>().Object, null, null, null, new Lazy<DbDispatchers>(() => mockDispatchers.Object));

                Action<string> sink = new StringWriter().Write;
                internalContext.Log = sink;

                mockDispatchers.Verify(
                    m => m.AddInterceptor(It.Is<DatabaseLogFormatter>(l => l.Context == context && l.WriteAction == sink)),
                    Times.Once());

                // Setting same writeAction again is a no-op
                internalContext.Log = sink;

                mockDispatchers.Verify(
                    m => m.AddInterceptor(It.Is<DatabaseLogFormatter>(l => l.Context == context && l.WriteAction == sink)),
                    Times.Once());
            }

            [Fact]
            public void Setting_log_again_reoplaces_the_existing_sink()
            {
                var mockDispatchers = new Mock<DbDispatchers>();

                var context = new Mock<DbContext>().Object;
                var internalContext = new LazyInternalContext(
                    context, new Mock<IInternalConnection>().Object, null, null, null, new Lazy<DbDispatchers>(() => mockDispatchers.Object));

                Action<string> sink = new StringWriter().Write;
                internalContext.Log = sink;

                Action<string> newSink = new StringWriter().Write;
                internalContext.Log = newSink;

                mockDispatchers.Verify(
                    m => m.RemoveInterceptor(It.Is<DatabaseLogFormatter>(l => l.Context == context && l.WriteAction == sink)),
                    Times.Once());

                mockDispatchers.Verify(
                    m => m.AddInterceptor(It.Is<DatabaseLogFormatter>(l => l.Context == context && l.WriteAction == newSink)),
                    Times.Once());

                mockDispatchers.Verify(
                    m => m.AddInterceptor(It.Is<DatabaseLogFormatter>(l => l.Context == context)),
                    Times.Exactly(2));
            }

            [Fact]
            public void Log_can_be_cleared_by_setting_it_to_null()
            {
                var mockDispatchers = new Mock<DbDispatchers>();

                var context = new Mock<DbContext>().Object;
                var internalContext = new LazyInternalContext(
                    context, new Mock<IInternalConnection>().Object, null, null, null, new Lazy<DbDispatchers>(() => mockDispatchers.Object));

                Action<string> sink = new StringWriter().Write;
                internalContext.Log = sink;
                internalContext.Log = null;

                mockDispatchers.Verify(
                    m => m.RemoveInterceptor(It.Is<DatabaseLogFormatter>(l => l.Context == context && l.WriteAction == sink)),
                    Times.Once());
            }

            [Fact]
            public void Log_returns_the_current_sink_in_use_or_null()
            {
                var mockDispatchers = new Mock<DbDispatchers>();

                var context = new Mock<DbContext>().Object;
                var internalContext = new LazyInternalContext(
                    context, new Mock<IInternalConnection>().Object, null, null, null, new Lazy<DbDispatchers>(() => mockDispatchers.Object));

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
                var mockDispatchers = new Mock<DbDispatchers>();

                var context = new Mock<DbContext>().Object;
                var internalContext = new LazyInternalContext(
                    context, new Mock<IInternalConnection>().Object, null, null, null, new Lazy<DbDispatchers>(() => mockDispatchers.Object));

                Action<string> sink = new StringWriter().Write;
                internalContext.Log = sink;

                internalContext.Dispose();

                mockDispatchers.Verify(
                    m => m.RemoveInterceptor(It.Is<DatabaseLogFormatter>(l => l.Context == context && l.WriteAction == sink)),
                    Times.Once());
            }
        }

        public class WrapUpdateException : TestBase
        {
            [Fact]
            public void UpdateException_with_relationship_entries_is_wrapped_as_IA_update_exception()
            {
                var original = new UpdateException("Bang!", null, new[] { CreateEntityEntry(), CreateRelationshipEntry() });

                var wrapped = new EagerInternalContext(new Mock<DbContext>().Object).WrapUpdateException(original);

                Assert.IsType<DbUpdateException>(wrapped);
                Assert.Equal(Strings.DbContext_IndependentAssociationUpdateException, wrapped.Message);
                Assert.Empty(wrapped.Entries);
                Assert.Same(original, wrapped.InnerException);
            }

            [Fact]
            public void OptimisticConcurrencyException_with_relationship_entries_is_wrapped_as_IA_update_exception()
            {
                var original = new OptimisticConcurrencyException("Bang!", null, new[] { CreateEntityEntry(), CreateRelationshipEntry() });

                var wrapped = new EagerInternalContext(new Mock<DbContext>().Object).WrapUpdateException(original);

                Assert.IsType<DbUpdateException>(wrapped);
                Assert.Equal(Strings.DbContext_IndependentAssociationUpdateException, wrapped.Message);
                Assert.Empty(wrapped.Entries);
                Assert.Same(original, wrapped.InnerException);
            }

            [Fact]
            public void UpdateException_without_relationship_entries_is_wrapped_as_normal_update_exception()
            {
                var entity1 = new object();
                var entity2 = new object();
                var original = new UpdateException("Bang!", null, new[] { CreateEntityEntry(entity1), CreateEntityEntry(entity2) });

                var wrapped = new EagerInternalContext(new Mock<DbContext>().Object).WrapUpdateException(original);

                Assert.IsType<DbUpdateException>(wrapped);
                Assert.Equal("Bang!", wrapped.Message);
                Assert.Equal(2, wrapped.Entries.Count());
                Assert.Same(entity1, wrapped.Entries.First().Entity);
                Assert.Same(entity2, wrapped.Entries.Skip(1).First().Entity);
                Assert.Same(original, wrapped.InnerException);
            }

            [Fact]
            public void OptimisticConcurrencyException_without_relationship_entries_is_wrapped_as_normal_update_exception()
            {
                var entity1 = new object();
                var entity2 = new object();
                var original = new OptimisticConcurrencyException(
                    "Bang!", null, new[] { CreateEntityEntry(entity1), CreateEntityEntry(entity2) });

                var wrapped = new EagerInternalContext(new Mock<DbContext>().Object).WrapUpdateException(original);

                Assert.IsType<DbUpdateConcurrencyException>(wrapped);
                Assert.Equal("Bang!", wrapped.Message);
                Assert.Equal(2, wrapped.Entries.Count());
                Assert.Same(entity1, wrapped.Entries.First().Entity);
                Assert.Same(entity2, wrapped.Entries.Skip(1).First().Entity);
                Assert.Same(original, wrapped.InnerException);
            }

            [Fact]
            public void UpdateException_with_no_entries_is_wrapped_as_normal_update_exception()
            {
                var original = new UpdateException("Bang!", null, new ObjectStateEntry[0] );

                var wrapped = new EagerInternalContext(new Mock<DbContext>().Object).WrapUpdateException(original);

                Assert.IsType<DbUpdateException>(wrapped);
                Assert.Equal("Bang!", wrapped.Message);
                Assert.Empty(wrapped.Entries);
                Assert.Same(original, wrapped.InnerException);
            }

            [Fact]
            public void OptimisticConcurrencyException_with_no_entries_is_wrapped_as_normal_update_exception()
            {
                var original = new OptimisticConcurrencyException("Bang!", null, new ObjectStateEntry[0]);

                var wrapped = new EagerInternalContext(new Mock<DbContext>().Object).WrapUpdateException(original);

                Assert.IsType<DbUpdateConcurrencyException>(wrapped);
                Assert.Equal("Bang!", wrapped.Message);
                Assert.Empty(wrapped.Entries);
                Assert.Same(original, wrapped.InnerException);
            }

            [Fact]
            public void UpdateException_with_null_entries_is_wrapped_as_normal_update_exception()
            {
                var original = new UpdateException("Bang!");

                var wrapped = new EagerInternalContext(new Mock<DbContext>().Object).WrapUpdateException(original);

                Assert.IsType<DbUpdateException>(wrapped);
                Assert.Equal("Bang!", wrapped.Message);
                Assert.Empty(wrapped.Entries);
                Assert.Same(original, wrapped.InnerException);
            }

            [Fact]
            public void OptimisticConcurrencyException_with_null_entries_is_wrapped_as_normal_update_exception()
            {
                var original = new OptimisticConcurrencyException("Bang!");

                var wrapped = new EagerInternalContext(new Mock<DbContext>().Object).WrapUpdateException(original);

                Assert.IsType<DbUpdateConcurrencyException>(wrapped);
                Assert.Equal("Bang!", wrapped.Message);
                Assert.Empty(wrapped.Entries);
                Assert.Same(original, wrapped.InnerException);
            }

            private static ObjectStateEntry CreateRelationshipEntry()
            {
                return new Mock<ObjectStateEntry>().Object;
            }

            private static ObjectStateEntry CreateEntityEntry(object entity = null)
            {
                var mockEntityEntry = new Mock<ObjectStateEntry>();
                mockEntityEntry.Setup(m => m.Entity).Returns(entity ?? new object());
                return mockEntityEntry.Object;
            }
        }
    }
}
