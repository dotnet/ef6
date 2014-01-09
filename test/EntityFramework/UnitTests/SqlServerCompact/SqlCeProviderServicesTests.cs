// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.SqlServerCompact.Resources;
    using System.Data.SqlClient;
    using System.Data.SqlServerCe;
    using System.Linq;
    using Moq;
    using SimpleModel;
    using Xunit;

    public class SqlCeProviderServicesTests : TestBase
    {
        public class RegisterInfoMessageHandler : TestBase
        {
            [Fact]
            public void Validates_pre_conditions()
            {
                Assert.Equal(
                    "connection",
                    Assert.Throws<ArgumentNullException>(
                        () => SqlCeProviderServices.Instance.RegisterInfoMessageHandler(null, null)).ParamName);
                Assert.Equal(
                    "handler",
                    Assert.Throws<ArgumentNullException>(
                        () => SqlCeProviderServices.Instance.RegisterInfoMessageHandler(new SqlCeConnection(), null)).ParamName);
            }

            [Fact]
            public void Throws_when_wrong_connection_type()
            {
                Assert.Equal(
                    Strings.Mapping_Provider_WrongConnectionType(typeof(SqlCeConnection)),
                    Assert.Throws<ArgumentException>(
                        () => SqlCeProviderServices.Instance.RegisterInfoMessageHandler(new SqlConnection(), _ => { })).Message);
            }
        }

        [Fact]
        public void GetProviderManifest_throws_when_empty()
        {
            var ex = Assert.Throws<ProviderIncompatibleException>(
                () => SqlCeProviderServices.Instance.GetProviderManifest(string.Empty));

            // NOTE: Verifying base exception since DbProviderServices wraps errors
            var baseException = ex.GetBaseException();

            Assert.IsType<ArgumentException>(baseException);
            Assert.Equal(Strings.UnableToDetermineStoreVersion, baseException.Message);
        }

        [Fact]
        public void GetProviderManifest_returns_cached_object()
        {
            var localManifest = SqlCeProviderServices.Instance.GetProviderManifest("2008");
            Assert.Same(localManifest, SqlCeProviderServices.Instance.GetProviderManifest("2010"));
        }

        public class GetService
        {
            [Fact]
            public void GetService_resolves_the_SQL_CE_Migrations_SQL_generator()
            {
                Assert.IsType<SqlCeMigrationSqlGenerator>(
                    SqlCeProviderServices.Instance.GetService<Func<MigrationSqlGenerator>>("System.Data.SqlServerCe.4.0")());
            }

            [Fact]
            public void GetService_returns_null_for_SQL_generators_for_other_invariant_names()
            {
                Assert.Null(SqlCeProviderServices.Instance.GetService<Func<MigrationSqlGenerator>>("System.Data.SqlClient"));
            }

            [Fact]
            public void GetService_resolves_the_default_SQL_Compact_connection_factory()
            {
                Assert.IsType<SqlCeConnectionFactory>(SqlCeProviderServices.Instance.GetService<IDbConnectionFactory>());
            }
        }

        public class DbCreateDatabase : TestBase
        {
            [Fact]
            public void DbCreateDatabase_dispatches_to_interceptors()
            {
                using (var context = new DdlDatabaseContext(SimpleCeConnection<DdlDatabaseContext>()))
                {
                    var storeItemCollection =
                        (StoreItemCollection)
                            ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);

                    context.Database.Delete();

                    var interceptor = new TestNonQueryInterceptor();
                    DbInterception.Add(interceptor);
                    var dbConnectionInterceptorMock = new Mock<IDbConnectionInterceptor>();
                    DbInterception.Add(dbConnectionInterceptorMock.Object);
                    var transactionInterceptorMock = new Mock<IDbTransactionInterceptor>();
                    DbInterception.Add(transactionInterceptorMock.Object);
                    try
                    {
                        SqlCeProviderServices.Instance.CreateDatabase(context.Database.Connection, null, storeItemCollection);
                    }
                    finally
                    {
                        DbInterception.Remove(interceptor);
                        DbInterception.Remove(dbConnectionInterceptorMock.Object);
                        DbInterception.Remove(transactionInterceptorMock.Object);
                    }

                    Assert.Equal(3, interceptor.CommandTexts.Count);

                    Assert.True(interceptor.CommandTexts.Any(t => t.StartsWith("CREATE TABLE \"Products\" ")));
                    Assert.True(interceptor.CommandTexts.Any(t => t.StartsWith("CREATE TABLE \"Categories\" ")));
                    Assert.True(interceptor.CommandTexts.Any(t => t.StartsWith("ALTER TABLE \"Products\" ADD CONSTRAINT ")));
                    
                    dbConnectionInterceptorMock.Verify(
                        m => m.ConnectionStringGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Once());
                    dbConnectionInterceptorMock.Verify(
                        m => m.ConnectionStringGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Once());

                    dbConnectionInterceptorMock.Verify(
                        m => m.BeginningTransaction(It.IsAny<DbConnection>(), It.IsAny<BeginTransactionInterceptionContext>()),
                        Times.Once());
                    dbConnectionInterceptorMock.Verify(
                        m => m.BeganTransaction(It.IsAny<DbConnection>(), It.IsAny<BeginTransactionInterceptionContext>()),
                        Times.Once());

                    dbConnectionInterceptorMock.Verify(
                        m => m.Opening(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                        Times.Once());
                    dbConnectionInterceptorMock.Verify(
                        m => m.Opened(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                        Times.Once());

                    dbConnectionInterceptorMock.Verify(
                        m => m.Closing(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                        Times.Once());
                    dbConnectionInterceptorMock.Verify(
                        m => m.Closed(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                        Times.Once());

                    transactionInterceptorMock.Verify(
                        m => m.Committing(It.IsAny<DbTransaction>(), It.IsAny<DbTransactionInterceptionContext>()),
                        Times.Once());
                    transactionInterceptorMock.Verify(
                        m => m.Committed(It.IsAny<DbTransaction>(), It.IsAny<DbTransactionInterceptionContext>()),
                        Times.Once());
                }
            }

            public class TestNonQueryInterceptor : DbCommandInterceptor
            {
                public readonly List<string> CommandTexts = new List<string>();

                public override void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
                {
                    CommandTexts.Add(command.CommandText);

                    Assert.Empty(interceptionContext.DbContexts);
                    Assert.Empty(interceptionContext.ObjectContexts);
                }
            }
        }

        public class DbDeleteDatabase : TestBase
        {
            [Fact]
            public void DbDeleteDatabase_dispatches_to_interceptors()
            {
                using (var context = new DdlDatabaseContext(SimpleCeConnection<DdlDatabaseContext>()))
                {
                    context.Database.CreateIfNotExists();

                    var dbConnectionInterceptorMock = new Mock<IDbConnectionInterceptor>();
                    DbInterception.Add(dbConnectionInterceptorMock.Object);
                    try
                    {
                        SqlCeProviderServices.Instance.DeleteDatabase(context.Database.Connection, null, new StoreItemCollection());
                    }
                    finally
                    {
                        DbInterception.Remove(dbConnectionInterceptorMock.Object);
                    }

                    dbConnectionInterceptorMock.Verify(
                        m => m.StateGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<ConnectionState>>()),
                        Times.Once());
                    dbConnectionInterceptorMock.Verify(
                        m => m.StateGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<ConnectionState>>()),
                        Times.Once());

                    dbConnectionInterceptorMock.Verify(
                        m => m.DataSourceGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Once());
                    dbConnectionInterceptorMock.Verify(
                        m => m.DataSourceGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Once());
                }
            }
        }

        public class DbDatabaseExists : TestBase
        {
            [Fact]
            public void DbDatabaseExists_dispatches_to_interceptors()
            {
                var dbConnectionInterceptorMock = new Mock<IDbConnectionInterceptor>();
                DbInterception.Add(dbConnectionInterceptorMock.Object);
                try
                {
                    using (var connection = new SqlCeConnection(ModelHelpers.SimpleCeConnectionString("I.Do.Not.Exist")))
                    {
                        Assert.False(SqlCeProviderServices.Instance.DatabaseExists(connection, null, new StoreItemCollection()));
                    }
                }
                finally
                {
                    DbInterception.Remove(dbConnectionInterceptorMock.Object);
                }

                dbConnectionInterceptorMock.Verify(
                    m => m.DataSourceGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                    Times.Once());
                dbConnectionInterceptorMock.Verify(
                    m => m.DataSourceGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                    Times.Once());
            }
        }

        public class DdlDatabaseContext : SimpleModelContext
        {
            static DdlDatabaseContext()
            {
                Database.SetInitializer<DdlDatabaseContext>(null);
            }

            public DdlDatabaseContext(DbConnection connection)
                : base(connection, contextOwnsConnection: true)
            {
            }
        }
    }
}
