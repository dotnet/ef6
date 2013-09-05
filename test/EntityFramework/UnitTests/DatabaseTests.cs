// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;

    public class DatabaseTests : TestBase
    {
        public class Exists : TestBase
        {
            [Fact]
            public void With_null_nameOrConnectionString_throws()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"),
                    Assert.Throws<ArgumentException>(() => Database.Exists((string)null)).Message);
            }

            [Fact]
            public void With_empty_nameOrConnectionString_throws()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"),
                    Assert.Throws<ArgumentException>(() => Database.Exists("")).Message);
            }

            [Fact]
            public void With_whitespace_nameOrConnectionString_throws()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"),
                    Assert.Throws<ArgumentException>(() => Database.Exists(" ")).Message);
            }

            [Fact]
            public void With_null_existingConnection_throws()
            {
                Assert.Equal(
                    "existingConnection", Assert.Throws<ArgumentNullException>(() => Database.Exists((DbConnection)null)).ParamName);
            }
        }

        public class Delete : TestBase
        {
            [Fact]
            public void With_null_nameOrConnectionString_throws()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"),
                    Assert.Throws<ArgumentException>(() => Database.Delete((string)null)).Message);
            }

            [Fact]
            public void With_empty_nameOrConnectionString_throws()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"),
                    Assert.Throws<ArgumentException>(() => Database.Delete("")).Message);
            }

            [Fact]
            public void With_whitespace_nameOrConnectionString_throws()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"),
                    Assert.Throws<ArgumentException>(() => Database.Delete(" ")).Message);
            }

            [Fact]
            public void With_null_existingConnection_throws()
            {
                Assert.Equal(
                    "existingConnection", Assert.Throws<ArgumentNullException>(() => Database.Delete((DbConnection)null)).ParamName);
            }
        }

        public class ExecuteSqlCommand : TestBase
        {
            [Fact]
            public void With_null_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.ExecuteSqlCommand(null)).Message);
            }

            [Fact]
            public void With_empty_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.ExecuteSqlCommand("")).Message);
            }

            [Fact]
            public void With_whitespace_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.ExecuteSqlCommand(" ")).Message);
            }

            [Fact]
            public void With_null_parameters_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal("parameters", Assert.Throws<ArgumentNullException>(() => database.ExecuteSqlCommand("query", null)).ParamName);
            }

            [Fact]
            public void With_valid_arguments_doesnt_throw()
            {
                var internalContextMock = new Mock<InternalContextForMock>();
                var database = new Database(internalContextMock.Object);
                var parameters = new object[1];

                Assert.NotNull(database.ExecuteSqlCommand("query", parameters));
                internalContextMock.Verify(m => m.ExecuteSqlCommand(TransactionalBehavior.EnsureTransaction, "query", parameters), Times.Once());

                Assert.NotNull(database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, "query", parameters));
                internalContextMock.Verify(
                    m => m.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, "query", parameters), Times.Once());
            }
        }

#if !NET40

        public class ExecuteSqlCommandAsync : TestBase
        {
            [Fact]
            public void With_null_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.ExecuteSqlCommandAsync(null).Result).Message);
            }

            [Fact]
            public void With_empty_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.ExecuteSqlCommandAsync("").Result).Message);
            }

            [Fact]
            public void With_whitespace_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.ExecuteSqlCommandAsync(" ").Result).Message);
            }

            [Fact]
            public void With_null_parameters_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    "parameters",
                    Assert.Throws<ArgumentNullException>(() => database.ExecuteSqlCommandAsync("query", null).Result).ParamName);
            }

            [Fact]
            public void With_valid_arguments_doesnt_throw()
            {
                var internalContextMock = new Mock<InternalContextForMock>();
                internalContextMock.Setup(
                    m =>
                    m.ExecuteSqlCommandAsync(It.IsAny<TransactionalBehavior>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<object[]>()))
                                   .Returns(Task.FromResult(1));
                var database = new Database(internalContextMock.Object);
                var cancellationToken = new CancellationTokenSource().Token;
                var parameters = new object[1];

                Assert.NotNull(database.ExecuteSqlCommandAsync("query", parameters).Result);
                internalContextMock.Verify(
                    m => m.ExecuteSqlCommandAsync(TransactionalBehavior.EnsureTransaction, "query", CancellationToken.None, parameters), Times.Once());

                Assert.NotNull(database.ExecuteSqlCommandAsync("query", cancellationToken, parameters).Result);
                internalContextMock.Verify(
                    m => m.ExecuteSqlCommandAsync(TransactionalBehavior.EnsureTransaction, "query", cancellationToken, parameters), Times.Once());

                Assert.NotNull(database.ExecuteSqlCommandAsync(TransactionalBehavior.DoNotEnsureTransaction, "query", parameters).Result);
                internalContextMock.Verify(
                    m => m.ExecuteSqlCommandAsync(TransactionalBehavior.EnsureTransaction, "query", CancellationToken.None, parameters), Times.Once());

                Assert.NotNull(database.ExecuteSqlCommandAsync(TransactionalBehavior.DoNotEnsureTransaction, "query", cancellationToken, parameters).Result);
                internalContextMock.Verify(
                    m => m.ExecuteSqlCommandAsync(TransactionalBehavior.EnsureTransaction, "query", cancellationToken, parameters), Times.Once());
            }
        }

#endif

        public class DefaultConnectionFactory : TestBase
        {
            [Fact]
            public void Default_is_SqlServerConnectionFactory()
            {
#pragma warning disable 612,618
                Assert.IsType<SqlConnectionFactory>(Database.DefaultConnectionFactory);
#pragma warning restore 612,618
                Assert.IsType<SqlConnectionFactory>(DbConfiguration.DependencyResolver.GetService<IDbConnectionFactory>());
            }

            private class FakeConnectionFactory : IDbConnectionFactory
            {
                public DbConnection CreateConnection(string nameOrConnectionString)
                {
                    throw new NotImplementedException();
                }
            }

            [Fact]
            public void Setting_DefaultConnectionFactory_after_configuration_override_is_in_place_has_no_effect()
            {
                try
                {
#pragma warning disable 612,618
                    // This call will have no effect because the functional tests are setup with a DbConfiguration
                    // that explicitly overrides this using an Loaded handler.
                    Database.DefaultConnectionFactory = new FakeConnectionFactory();

                    Assert.IsType<SqlConnectionFactory>(Database.DefaultConnectionFactory);
#pragma warning restore 612,618
                }
                finally
                {
                    typeof(Database).GetDeclaredMethod("ResetDefaultConnectionFactory").Invoke(null, null);
                    Database.ResetDefaultConnectionFactory();
                }
            }

            [Fact]
            public void Throws_when_set_to_null()
            {
#pragma warning disable 612,618
                Assert.Equal("value", Assert.Throws<ArgumentNullException>(() => Database.DefaultConnectionFactory = null).ParamName);
#pragma warning restore 612,618
            }
        }

        public class SqlQuery_Generic : TestBase
        {
            [Fact]
            public void With_null_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.SqlQuery<Random>(null)).Message);
            }

            [Fact]
            public void With_empty_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.SqlQuery<Random>("")).Message);
            }

            [Fact]
            public void With_whitespace_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.SqlQuery<Random>(" ")).Message);
            }

            [Fact]
            public void With_null_parameters_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    "parameters",
                    Assert.Throws<ArgumentNullException>(() => database.SqlQuery<Random>("query", null)).ParamName);
            }

            [Fact]
            public void With_valid_arguments_doesnt_throw()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                var query = database.SqlQuery<Random>("query");

                Assert.NotNull(query);
                Assert.False(query.InternalQuery.Streaming);
            }
        }

        public class SqlQuery_NonGeneric : TestBase
        {
            [Fact]
            public void With_null_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.SqlQuery(typeof(Random), null)).Message);
            }

            [Fact]
            public void With_empty_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.SqlQuery(typeof(Random), "")).Message);
            }

            [Fact]
            public void With_whitespace_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.SqlQuery(typeof(Random), " ")).Message);
            }

            [Fact]
            public void With_null_parameters_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    "parameters",
                    Assert.Throws<ArgumentNullException>(() => database.SqlQuery(typeof(Random), "query", null)).ParamName);
            }

            [Fact]
            public void With_null_type_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    "elementType",
                    Assert.Throws<ArgumentNullException>(() => database.SqlQuery(null, "query")).ParamName);
            }

            [Fact]
            public void With_valid_arguments_dont_throw()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                var query = database.SqlQuery(typeof(Random), "query");

                Assert.NotNull(query);
                Assert.False(query.InternalQuery.Streaming);
            }
        }

        public class CommandTimeout : TestBase
        {
            [Fact]
            public void Default_value_for_CommandTimeout_is_null_and_can_be_changed_including_setting_to_null()
            {
                using (var context = new TimeoutContext())
                {
                    Assert.Null(context.Database.CommandTimeout);

                    context.Database.CommandTimeout = 77;
                    Assert.Equal(77, context.Database.CommandTimeout);

                    context.Database.CommandTimeout = null;
                    Assert.Null(context.Database.CommandTimeout);
                }
            }

            [Fact]
            public void CommandTimeout_throws_for_negative_values()
            {
                using (var context = new TimeoutContext())
                {
                    Assert.Equal(
                        Strings.ObjectContext_InvalidCommandTimeout,
                        Assert.Throws<ArgumentException>(
                            () => context.Database.CommandTimeout = -1).Message);
                }
            }

            [Fact]
            public void CommandTimeout_can_be_set_in_constructor_and_changed_on_DbContext_without_triggering_initialization()
            {
                using (var context = new TimeoutContext(77))
                {
                    Assert.Equal(77, context.Database.CommandTimeout);
                    Assert.Null(((LazyInternalContext)context.InternalContext).ObjectContextInUse);

                    context.Database.CommandTimeout = 88;
                    Assert.Equal(88, context.Database.CommandTimeout);
                    Assert.Null(((LazyInternalContext)context.InternalContext).ObjectContextInUse);

                    Assert.Equal(88, ((IObjectContextAdapter)context).ObjectContext.CommandTimeout);
                }
            }

            public class TimeoutContext : DbContext
            {
                static TimeoutContext()
                {
                    Database.SetInitializer<TimeoutContext>(null);
                }

                public TimeoutContext()
                {
                }

                public TimeoutContext(int? commandTimeout)
                {
                    Database.CommandTimeout = commandTimeout;
                }
            }
        }

        public class Log : TestBase
        {
            public class LogContext : DbContext
            {
                static LogContext()
                {
                    Database.SetInitializer<LogContext>(null);
                }
            }

            [Fact]
            public void Log_is_null_by_default()
            {
                Assert.Null(new LogContext().Database.Log);
            }

            [Fact]
            public void Getting_and_setting_Log_delegates_to_Log_on_InternalContext()
            {
                var mockContext = new Mock<InternalContextForMock>();
                var database = new Database(mockContext.Object);

                Action<string> sink = new StringWriter().Write;
                database.Log = sink;
                mockContext.VerifySet(m => m.Log = sink);

                var _ = database.Log;
                mockContext.VerifyGet(m => m.Log);
            }
        }
    }
}
