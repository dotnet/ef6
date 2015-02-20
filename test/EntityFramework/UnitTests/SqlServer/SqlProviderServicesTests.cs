// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer.Resources;
    using System.Data.Entity.Utilities;
    using System.Data.SqlClient;
    using System.Data.SqlServerCe;
    using System.Linq;
    using Moq;
    using Moq.Protected;
    using SimpleModel;
    using Xunit;

    public class SqlProviderServicesTests
    {
        public class RegisterInfoMessageHandler : TestBase
        {
            [Fact]
            public void Validates_pre_conditions()
            {
                Assert.Equal(
                    "connection",
                    Assert.Throws<ArgumentNullException>(
                        () => SqlProviderServices.Instance.RegisterInfoMessageHandler(null, null)).ParamName);
                Assert.Equal(
                    "handler",
                    Assert.Throws<ArgumentNullException>(
                        () => SqlProviderServices.Instance.RegisterInfoMessageHandler(new SqlConnection(), null)).ParamName);
            }

            [Fact]
            public void Throws_when_wrong_connection_type()
            {
                Assert.Equal(
                    Strings.Mapping_Provider_WrongConnectionType(typeof(SqlConnection)),
                    Assert.Throws<ArgumentException>(
                        () => SqlProviderServices.Instance.RegisterInfoMessageHandler(new SqlCeConnection(), _ => { })).Message);
            }
        }

        public class GetDbProviderManifest : TestBase
        {
            internal const string TokenSql8 = "2000";
            internal const string TokenSql9 = "2005";
            internal const string TokenSql10 = "2008";
            internal const string TokenSql11 = "2012";
            internal const string TokenAzure11 = "2012.Azure";

            [Fact]
            public void Returns_cached_objects()
            {
                var manifest8 = SqlProviderServices.Instance.GetProviderManifest(TokenSql8);
                var manifest9 = SqlProviderServices.Instance.GetProviderManifest(TokenSql9);
                var manifest10 = SqlProviderServices.Instance.GetProviderManifest(TokenSql10);
                var manifest11 = SqlProviderServices.Instance.GetProviderManifest(TokenSql11);
                var manifestAzure = SqlProviderServices.Instance.GetProviderManifest(TokenAzure11);

                Assert.Same(manifest8, SqlProviderServices.Instance.GetProviderManifest(TokenSql8));
                Assert.Same(manifest9, SqlProviderServices.Instance.GetProviderManifest(TokenSql9));
                Assert.Same(manifest10, SqlProviderServices.Instance.GetProviderManifest(TokenSql10));
                Assert.Same(manifest11, SqlProviderServices.Instance.GetProviderManifest(TokenSql11));
                Assert.Same(manifestAzure, SqlProviderServices.Instance.GetProviderManifest(TokenAzure11));

                Assert.NotSame(manifest8, manifest9);
                Assert.NotSame(manifest9, manifest10); 
                Assert.NotSame(manifest10, manifest11); 
                Assert.NotSame(manifest11, manifestAzure);
            }
        }

        public class GetDbProviderManifestToken : TestBase
        {
            [Fact]
            public void GetDbProviderManifestToken_can_return_Azure_manifest_token_from_given_connection()
            {
                Assert.Equal(
                    "2012.Azure",
                    SqlProviderServices.Instance.GetProviderManifestToken(CreateConnectionForTokenLookup("11", azure: true).Object));
            }

            [Fact]
            public void GetDbProviderManifestToken_can_return_Azure_manifest_token_from_master_connection()
            {
                var mockMasterConnection = CreateConnectionForTokenLookup("11", azure: true);

                var mockConnection = CreateConnectionForTokenLookup("11", azure: false, master: mockMasterConnection.Object);
                mockConnection.Setup(m => m.State).Returns(ConnectionState.Closed);
                mockConnection.Setup(m => m.Open()).Throws(new Exception());

                Assert.Equal("2012.Azure", SqlProviderServices.Instance.GetProviderManifestToken(mockConnection.Object));
            }

            [Fact]
            public void GetDbProviderManifestToken_can_return_non_Azure_manifest_token_from_given_connection()
            {
                Assert.Equal(
                    "2012", SqlProviderServices.Instance.GetProviderManifestToken(CreateConnectionForTokenLookup("11", azure: false).Object));
            }

            [Fact]
            public void GetDbProviderManifestToken_can_return_non_Azure_manifest_token_from_master_connection()
            {
                var mockMasterConnection = CreateConnectionForTokenLookup("11", azure: false);

                var mockConnection = CreateConnectionForTokenLookup("11", azure: true, master: mockMasterConnection.Object);
                mockConnection.Setup(m => m.State).Returns(ConnectionState.Closed);
                mockConnection.Setup(m => m.Open()).Throws(new Exception());

                Assert.Equal("2012", SqlProviderServices.Instance.GetProviderManifestToken(mockConnection.Object));
            }

            [Fact]
            public void GetDbProviderManifestToken_can_return_non_Azure_manifest_token_for_SQL_Server_2008()
            {
                Assert.Equal(
                    "2008", SqlProviderServices.Instance.GetProviderManifestToken(CreateConnectionForTokenLookup("10", azure: false).Object));
            }

            [Fact]
            public void GetDbProviderManifestToken_can_return_non_Azure_manifest_token_for_SQL_Server_2005()
            {
                Assert.Equal(
                    "2005", SqlProviderServices.Instance.GetProviderManifestToken(CreateConnectionForTokenLookup("09", azure: false).Object));
            }

            [Fact]
            public void GetDbProviderManifestToken_can_return_non_Azure_manifest_token_for_SQL_Server_2000()
            {
                Assert.Equal(
                    "2000", SqlProviderServices.Instance.GetProviderManifestToken(CreateConnectionForTokenLookup("08", azure: false).Object));
            }

            [Fact]
            public void GetDbProviderManifestToken_dispatches_to_interceptors()
            {
                var dbDataReaderMock = new Mock<DbDataReader>();
                
                var dbCommandMock = new Mock<DbCommand>();
                dbCommandMock.Protected()
                    .Setup<DbDataReader>("ExecuteDbDataReader", ItExpr.IsAny<CommandBehavior>())
                    .Returns(dbDataReaderMock.Object);

                var connectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                connectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(dbCommandMock.Object);
                connectionMock.Setup(m => m.ConnectionString).Returns(@"Data Source=.\SQLEXPRESS; Integrated Security=True; Database=master;");
                var state = ConnectionState.Closed;
                connectionMock.Setup(m => m.State).Returns(() => state);
                connectionMock.Setup(m => m.Open()).Callback(() => state = ConnectionState.Open);
                connectionMock.Setup(m => m.Close()).Callback(() => state = ConnectionState.Closed);

                connectionMock.Setup(m => m.DataSource).Returns(() => @".\SQLEXPRESS");
                connectionMock.Setup(m => m.ServerVersion).Returns(() => "11");
                connectionMock.Setup(m => m.ToString()).Returns("Mock DbConnection");
                connectionMock.Protected().Setup<DbProviderFactory>("DbProviderFactory").Returns(SqlClientFactory.Instance);
                
                var dbConnectionInterceptorMock = new Mock<IDbConnectionInterceptor>();
                DbInterception.Add(dbConnectionInterceptorMock.Object);

                var dbCommandInterceptorMock = new Mock<DbCommandInterceptor>();
                DbInterception.Add(dbCommandInterceptorMock.Object);
                try
                {
                    SqlProviderServices.Instance.GetProviderManifestToken(connectionMock.Object);

                    dbConnectionInterceptorMock.Verify(
                        m => m.ConnectionStringGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Exactly(4));
                    dbConnectionInterceptorMock.Verify(
                        m => m.ConnectionStringGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Exactly(4));
                    connectionMock.Verify(m => m.ConnectionString, Times.Exactly(4));

                    dbConnectionInterceptorMock.Verify(
                        m => m.StateGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<ConnectionState>>()),
                        Times.Exactly(3));
                    dbConnectionInterceptorMock.Verify(
                        m => m.StateGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<ConnectionState>>()),
                        Times.Exactly(3));
#if (DEBUG)
                    connectionMock.Verify(m => m.State, Times.Exactly(5));
#else
                    connectionMock.Verify(m => m.State, Times.Exactly(3));
#endif

                    dbConnectionInterceptorMock.Verify(
                        m => m.DataSourceGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Once());
                    dbConnectionInterceptorMock.Verify(
                        m => m.DataSourceGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Once());
                    connectionMock.Verify(m => m.DataSource, Times.Once());

                    dbConnectionInterceptorMock.Verify(
                        m => m.ServerVersionGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Once());
                    dbConnectionInterceptorMock.Verify(
                        m => m.ServerVersionGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Once());
                    connectionMock.Verify(m => m.ServerVersion, Times.Once());

                    dbConnectionInterceptorMock.Verify(
                        m => m.Opening(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                        Times.Once());
                    dbConnectionInterceptorMock.Verify(
                        m => m.Opened(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                        Times.Once());
                    connectionMock.Verify(m => m.Close(), Times.Once());

                    dbConnectionInterceptorMock.Verify(
                        m => m.Closing(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                        Times.Once());
                    dbConnectionInterceptorMock.Verify(
                        m => m.Closed(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                        Times.Once());
                    connectionMock.Verify(m => m.Close(), Times.Once());

                    dbCommandInterceptorMock.Verify(
                        m => m.ReaderExecuting(It.IsAny<DbCommand>(), It.IsAny<DbCommandInterceptionContext<DbDataReader>>()),
                        Times.Once());
                    dbCommandInterceptorMock.Verify(
                        m => m.ReaderExecuted(It.IsAny<DbCommand>(), It.IsAny<DbCommandInterceptionContext<DbDataReader>>()),
                        Times.Once());
                    dbCommandMock.Protected()
                        .Verify<DbDataReader>("ExecuteDbDataReader", Times.Once(), ItExpr.IsAny<CommandBehavior>());
                }
                finally
                {
                    DbInterception.Remove(dbConnectionInterceptorMock.Object);
                    DbInterception.Remove(dbCommandInterceptorMock.Object);
                }
            }
        }

        public class CreateDatabaseFromScript : TestBase
        {
            [Fact]
            public void CreateDatabaseFromScript_returns_expected_SQL_Version_from_master_connection()
            {
                var mockMasterConnection = CreateConnectionForTokenLookup("10", azure: false);
                var mockConnection = CreateConnectionForTokenLookup("00", azure: false, master: mockMasterConnection.Object);

                Assert.Equal(
                    SqlVersion.Sql10,
                    SqlProviderServices.Instance.CreateDatabaseFromScript(null, mockConnection.Object, ""));
            }
        }

        private static Mock<DbConnection> CreateConnectionForTokenLookup(string majorVersion, bool azure, DbConnection master = null)
        {
            var mockReader = new Mock<DbDataReader>();
            mockReader.Setup(m => m.GetInt32(0)).Returns(azure ? 5 : 2);

            var mockCommand = new Mock<DbCommand>();
            mockCommand.Protected().Setup<DbDataReader>("ExecuteDbDataReader", ItExpr.IsAny<CommandBehavior>()).Returns(mockReader.Object);

            var mockFactory = new Mock<DbProviderFactory>();

            var mockConnection = new Mock<DbConnection>();
            mockConnection.Setup(m => m.State).Returns(ConnectionState.Open);
            mockConnection.Protected().Setup<DbCommand>("CreateDbCommand").Returns(mockCommand.Object);
            mockConnection.Setup(m => m.ConnectionString).Returns("Database=MockDatabase");
            mockConnection.Protected().SetupGet<DbProviderFactory>("DbProviderFactory").Returns(mockFactory.Object);
            mockConnection.Setup(m => m.ServerVersion).Returns(majorVersion);

            mockFactory.Setup(m => m.CreateConnection()).Returns(master);

            return mockConnection;
        }

        public class GetService
        {
            [Fact]
            public void GetService_resolves_the_SQL_Server_Migrations_SQL_generator()
            {
                Assert.IsType<SqlServerMigrationSqlGenerator>(
                    SqlProviderServices.Instance.GetService<Func<MigrationSqlGenerator>>("System.Data.SqlClient")());
            }

            [Fact]
            public void GetService_returns_null_for_SQL_generators_for_other_invariant_names()
            {
                Assert.Null(SqlProviderServices.Instance.GetService<Func<MigrationSqlGenerator>>("System.Data.SqlServerCe.4.0"));
            }

            [Fact]
            public void GetService_resolves_the_default_SQL_Express_connection_factory()
            {
                Assert.IsType<SqlConnectionFactory>(SqlProviderServices.Instance.GetService<IDbConnectionFactory>());
            }

            [Fact]
            public void GetService_resolves_the_default_SQL_Server_execution_strategy_factory_for_any_server()
            {
                Assert.IsType<DefaultSqlExecutionStrategy>(
                    SqlProviderServices.Instance.GetService<Func<IDbExecutionStrategy>>(
                        new ExecutionStrategyKey("System.Data.SqlClient", "Elmo"))());
            }

            [Fact]
            public void GetService_returns_null_for_execution_strategy_factory_for_other_invariant_names()
            {
                Assert.Null(
                    SqlProviderServices.Instance.GetService<Func<IDbExecutionStrategy>>(
                        new ExecutionStrategyKey("System.Data.SqlServerCe.4.0", "Elmo")));
            }

            [Fact]
            public void GetService_resolves_the_SQL_Server_spatial_services_for_manifest_tokens_that_support_spatial()
            {
                Assert.Same(
                    SqlSpatialServices.Instance,
                    SqlProviderServices.Instance.GetService<DbSpatialServices>(new DbProviderInfo("System.Data.SqlClient", "2008")));

                Assert.Same(
                    SqlSpatialServices.Instance,
                    SqlProviderServices.Instance.GetService<DbSpatialServices>(new DbProviderInfo("System.Data.SqlClient", "2012")));

                Assert.Same(
                    SqlSpatialServices.Instance,
                    SqlProviderServices.Instance.GetService<DbSpatialServices>(new DbProviderInfo("System.Data.SqlClient", "2012.Azure")));
            }

            [Fact]
            public void GetService_returns_null_for_manifest_tokens_that_dont_support_spatial()
            {
                var key = new DbProviderInfo("System.Data.SqlClient", "2005");
                Assert.Null(SqlProviderServices.Instance.GetService<DbSpatialServices>(key));
            }

            [Fact]
            public void GetService_throws_for_unrecognized_manifest_tokens()
            {
                var key = new DbProviderInfo("System.Data.SqlClient", "Dingo");
                Assert.Equal(
                    Strings.UnableToDetermineStoreVersion,
                    Assert.Throws<ArgumentException>(() => SqlProviderServices.Instance.GetService<DbSpatialServices>(key)).Message);
            }

            [Fact]
            public void GetService_resolves_the_SQL_Server_spatial_services_as_the_default_resolver()
            {
                Assert.Same(SqlSpatialServices.Instance, SqlProviderServices.Instance.GetService<DbSpatialServices>());
            }

            [Fact]
            public void GetService_returns_null_for_spatail_services_for_other_providers()
            {
                Assert.Null(
                    SqlProviderServices.Instance.GetService<DbSpatialServices>(new DbProviderInfo("System.Data.SqlServerCe.4.0", "")));
            }
        }

        public class DbCreateDatabase : TestBase
        {
            [Fact]
            public void DbCreateDatabase_dispatches_to_interceptors()
            {
                using (var context = new DdlDatabaseContext())
                {
                    var storeItemCollection =
                        (StoreItemCollection)
                        ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);

                    context.Database.Delete();

                    var interceptor = new TestNonQueryInterceptor();
                    DbInterception.Add(interceptor);
                    var dbConnectionInterceptorMock = new Mock<IDbConnectionInterceptor>();
                    DbInterception.Add(dbConnectionInterceptorMock.Object);
                    try
                    {
                        SqlProviderServices.Instance.CreateDatabase(context.Database.Connection, null, storeItemCollection);
                    }
                    finally
                    {
                        DbInterception.Remove(interceptor);
                        DbInterception.Remove(dbConnectionInterceptorMock.Object);
                    }

                    Assert.Equal(3, interceptor.Commands.Count);

                    var commandTexts = interceptor.Commands.Select(c => c.CommandText);
                    Assert.True(commandTexts.Any(t => t.StartsWith("create database ")));
                    Assert.True(commandTexts.Any(t => t.StartsWith("if serverproperty('EngineEdition') <> 5 execute sp_executesql ")));
                    Assert.True(commandTexts.Any(t => t.StartsWith("create table [dbo].[Categories] ")));

                    dbConnectionInterceptorMock.Verify(
                        m => m.ConnectionStringGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Exactly(12));
                    dbConnectionInterceptorMock.Verify(
                        m => m.ConnectionStringGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Exactly(12));

                    dbConnectionInterceptorMock.Verify(
                        m => m.StateGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<ConnectionState>>()),
                        Times.Exactly(9));
                    dbConnectionInterceptorMock.Verify(
                        m => m.StateGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<ConnectionState>>()),
                        Times.Exactly(9));

                    dbConnectionInterceptorMock.Verify(
                        m => m.DataSourceGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Exactly(3));
                    dbConnectionInterceptorMock.Verify(
                        m => m.DataSourceGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Exactly(3));

                    dbConnectionInterceptorMock.Verify(
                        m => m.ServerVersionGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Once());
                    dbConnectionInterceptorMock.Verify(
                        m => m.ServerVersionGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Once());

                    dbConnectionInterceptorMock.Verify(
                        m => m.Opening(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                        Times.Exactly(3));
                    dbConnectionInterceptorMock.Verify(
                        m => m.Opened(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                        Times.Exactly(3));

                    dbConnectionInterceptorMock.Verify(
                        m => m.Closing(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                        Times.Exactly(3));
                    dbConnectionInterceptorMock.Verify(
                        m => m.Closed(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                        Times.Exactly(3));
                }
            }

            [Fact]
            public void GetDbProviderManifestToken_dispatches_to_interceptors()
            {
                var dbDataReaderMock = new Mock<DbDataReader>();

                var dbCommandMock = new Mock<DbCommand>();
                dbCommandMock.Protected()
                    .Setup<DbDataReader>("ExecuteDbDataReader", ItExpr.IsAny<CommandBehavior>())
                    .Returns(dbDataReaderMock.Object);

                var connectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                connectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(dbCommandMock.Object);
                connectionMock.Setup(m => m.ConnectionString).Returns(@"Data Source=.\SQLEXPRESS; Integrated Security=True; Database=master;");
                var state = ConnectionState.Closed;
                connectionMock.Setup(m => m.State).Returns(() => state);
                connectionMock.Setup(m => m.Open()).Callback(() => state = ConnectionState.Open);
                connectionMock.Setup(m => m.Close()).Callback(() => state = ConnectionState.Closed);

                connectionMock.Setup(m => m.DataSource).Returns(() => @".\SQLEXPRESS");
                connectionMock.Setup(m => m.ServerVersion).Returns(() => "11");
                connectionMock.Setup(m => m.ToString()).Returns("Mock DbConnection");
                connectionMock.Protected().Setup<DbProviderFactory>("DbProviderFactory").Returns(SqlClientFactory.Instance);

                var dbConnectionInterceptorMock = new Mock<IDbConnectionInterceptor>();
                DbInterception.Add(dbConnectionInterceptorMock.Object);

                var dbCommandInterceptorMock = new Mock<DbCommandInterceptor>();
                DbInterception.Add(dbCommandInterceptorMock.Object);
                try
                {
                    SqlProviderServices.Instance.GetProviderManifestToken(connectionMock.Object);

                    dbConnectionInterceptorMock.Verify(
                        m => m.ConnectionStringGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Exactly(4));
                    dbConnectionInterceptorMock.Verify(
                        m => m.ConnectionStringGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Exactly(4));

                    dbConnectionInterceptorMock.Verify(
                        m => m.StateGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<ConnectionState>>()),
                        Times.Exactly(3));
                    dbConnectionInterceptorMock.Verify(
                        m => m.StateGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<ConnectionState>>()),
                        Times.Exactly(3));

                    dbConnectionInterceptorMock.Verify(
                        m => m.DataSourceGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Once());
                    dbConnectionInterceptorMock.Verify(
                        m => m.DataSourceGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Once());
                    connectionMock.Verify(m => m.DataSource, Times.Once());

                    dbConnectionInterceptorMock.Verify(
                        m => m.ServerVersionGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Once());
                    dbConnectionInterceptorMock.Verify(
                        m => m.ServerVersionGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Once());
                    connectionMock.Verify(m => m.ServerVersion, Times.Once());

                    dbConnectionInterceptorMock.Verify(
                        m => m.Opening(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                        Times.Once());
                    dbConnectionInterceptorMock.Verify(
                        m => m.Opened(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                        Times.Once());
                    connectionMock.Verify(m => m.Close(), Times.Once());

                    dbConnectionInterceptorMock.Verify(
                        m => m.Closing(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                        Times.Once());
                    dbConnectionInterceptorMock.Verify(
                        m => m.Closed(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                        Times.Once());
                    connectionMock.Verify(m => m.Close(), Times.Once());

                    dbCommandInterceptorMock.Verify(
                        m => m.ReaderExecuting(It.IsAny<DbCommand>(), It.IsAny<DbCommandInterceptionContext<DbDataReader>>()),
                        Times.Once());
                    dbCommandInterceptorMock.Verify(
                        m => m.ReaderExecuted(It.IsAny<DbCommand>(), It.IsAny<DbCommandInterceptionContext<DbDataReader>>()),
                        Times.Once());
                    dbCommandMock.Protected()
                        .Verify<DbDataReader>("ExecuteDbDataReader", Times.Once(), ItExpr.IsAny<CommandBehavior>());
                }
                finally
                {
                    DbInterception.Remove(dbConnectionInterceptorMock.Object);
                    DbInterception.Remove(dbCommandInterceptorMock.Object);
                }
            }
        }

        public class DbDatabaseExists : TestBase
        {
            [Fact]
            public void DbDatabaseExists_dispatches_commands_to_interceptors_for_connections_with_initial_catalog()
            {
                var interceptor = new TestScalarInterceptor();
                DbInterception.Add(interceptor);
                var dbConnectionInterceptorMock = new Mock<IDbConnectionInterceptor>();
                DbInterception.Add(dbConnectionInterceptorMock.Object);
                try
                {
                    using (var connection = new SqlConnection(ModelHelpers.SimpleAttachConnectionString("I.Do.Not.Exist")))
                    {
                        SqlProviderServices.Instance.DatabaseExists(connection, null, new StoreItemCollection());
                    }
                }
                finally
                {
                    DbInterception.Remove(interceptor);
                    DbInterception.Remove(dbConnectionInterceptorMock.Object);
                }

                Assert.Equal(2, interceptor.Commands.Count);

                Assert.True(
                    interceptor.Commands.Select(c => c.CommandText).All(
                        t => t == "IF db_id(N'I.Do.Not.Exist') IS NOT NULL SELECT 1 ELSE SELECT Count(*) FROM sys.databases WHERE [name]=N'I.Do.Not.Exist'"));

                dbConnectionInterceptorMock.Verify(
                    m => m.ConnectionStringGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                    Times.Exactly(11));
                dbConnectionInterceptorMock.Verify(
                    m => m.ConnectionStringGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                    Times.Exactly(11));

                dbConnectionInterceptorMock.Verify(
                    m => m.StateGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<ConnectionState>>()),
                    Times.Exactly(8));
                dbConnectionInterceptorMock.Verify(
                    m => m.StateGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<ConnectionState>>()),
                    Times.Exactly(8));

                dbConnectionInterceptorMock.Verify(
                    m => m.DataSourceGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                    Times.Exactly(3));
                dbConnectionInterceptorMock.Verify(
                    m => m.DataSourceGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                    Times.Exactly(3));

                dbConnectionInterceptorMock.Verify(
                    m => m.ServerVersionGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                    Times.Exactly(0));
                dbConnectionInterceptorMock.Verify(
                    m => m.ServerVersionGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                    Times.Exactly(0));

                dbConnectionInterceptorMock.Verify(
                    m => m.Opening(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                    Times.Exactly(3));
                dbConnectionInterceptorMock.Verify(
                    m => m.Opened(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                    Times.Exactly(3));

                dbConnectionInterceptorMock.Verify(
                    m => m.Closing(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                    Times.Exactly(2));
                dbConnectionInterceptorMock.Verify(
                    m => m.Closed(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                    Times.Exactly(2));
            }

            [ExtendedFact(SkipForLocalDb = true, SkipForSqlAzure = true, Justification = "User Instance is not supported for SqlAzure and LocalDb")]
            public void DbDatabaseExists_dispatches_commands_to_interceptors_for_connections_with_no_initial_catalog()
            {
                // See CodePlex 1554 - Handle User Instance flakiness
                MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(new ExecutionStrategyResolver<IDbExecutionStrategy>(
                                    SqlProviderServices.ProviderInvariantName, null, () => new SqlAzureExecutionStrategy()));
                var interceptor = new TestScalarInterceptor();
                DbInterception.Add(interceptor);
                var dbConnectionInterceptorMock = new Mock<IDbConnectionInterceptor>();
                DbInterception.Add(dbConnectionInterceptorMock.Object);
                try
                {
                    using (var connection =
                        new SqlConnection(ModelHelpers.SimpleAttachConnectionString("I.Do.Not.Exist", useInitialCatalog: false)))
                    {
                        SqlProviderServices.Instance.DatabaseExists(connection, null, new StoreItemCollection());
                    }
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                    DbInterception.Remove(interceptor);
                    DbInterception.Remove(dbConnectionInterceptorMock.Object);
                }

                Assert.Equal(1, interceptor.Commands.Count);

                Assert.True(
                    interceptor.Commands.Select(c => c.CommandText)
                        .Single()
                        .StartsWith("SELECT Count(*) FROM sys.master_files WHERE [physical_name]=N'"));

                dbConnectionInterceptorMock.Verify(
                    m => m.ConnectionStringGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                    Times.Exactly(7));
                dbConnectionInterceptorMock.Verify(
                    m => m.ConnectionStringGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                    Times.Exactly(7));

                dbConnectionInterceptorMock.Verify(
                    m => m.StateGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<ConnectionState>>()),
                    Times.Exactly(5));
                dbConnectionInterceptorMock.Verify(
                    m => m.StateGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<ConnectionState>>()),
                    Times.Exactly(5));

                dbConnectionInterceptorMock.Verify(
                    m => m.DataSourceGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                    Times.Exactly(2));
                dbConnectionInterceptorMock.Verify(
                    m => m.DataSourceGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                    Times.Exactly(2));

                dbConnectionInterceptorMock.Verify(
                    m => m.ServerVersionGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                    Times.Once());
                dbConnectionInterceptorMock.Verify(
                    m => m.ServerVersionGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                    Times.Once());

                dbConnectionInterceptorMock.Verify(
                    m => m.Opening(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                    Times.Exactly(2));
                dbConnectionInterceptorMock.Verify(
                    m => m.Opened(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                    Times.Exactly(2));

                dbConnectionInterceptorMock.Verify(
                    m => m.Closing(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                    Times.Once());
                dbConnectionInterceptorMock.Verify(
                    m => m.Closed(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                    Times.Once());
            }
        }

        public class DbDeleteDatabase : TestBase
        {
            [Fact]
            public void DbDeleteDatabase_dispatches_to_interceptors_for_connections_with_initial_catalog()
            {
                using (var context = new DdlDatabaseContext())
                {
                    context.Database.CreateIfNotExists();
                }

                var interceptor = new TestNonQueryInterceptor();
                DbInterception.Add(interceptor);
                var dbConnectionInterceptorMock = new Mock<IDbConnectionInterceptor>();
                DbInterception.Add(dbConnectionInterceptorMock.Object);
                try
                {
                    using (var connection = new SqlConnection(SimpleAttachConnectionString<DdlDatabaseContext>()))
                    {
                        SqlProviderServices.Instance.DeleteDatabase(connection, null, new StoreItemCollection());
                    }
                }
                finally
                {
                    DbInterception.Remove(interceptor);
                    DbInterception.Remove(dbConnectionInterceptorMock.Object);
                }

                Assert.Equal(1, interceptor.Commands.Count);

                Assert.Equal(
                    "drop database [System.Data.Entity.SqlServer.SqlProviderServicesTests+DdlDatabaseContext]",
                    interceptor.Commands.Select(c => c.CommandText).Single());

                dbConnectionInterceptorMock.Verify(
                    m => m.ConnectionStringGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                    Times.Exactly(5));
                dbConnectionInterceptorMock.Verify(
                    m => m.ConnectionStringGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                    Times.Exactly(5));

                dbConnectionInterceptorMock.Verify(
                    m => m.StateGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<ConnectionState>>()),
                    Times.Exactly(3));
                dbConnectionInterceptorMock.Verify(
                    m => m.StateGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<ConnectionState>>()),
                    Times.Exactly(3));

                dbConnectionInterceptorMock.Verify(
                    m => m.DataSourceGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                    Times.Once());
                dbConnectionInterceptorMock.Verify(
                    m => m.DataSourceGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
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
            }

            [ExtendedFact(SkipForLocalDb = true, SkipForSqlAzure = true, Justification = "User Instance is not supported for SqlAzure and LocalDb")]
            public void DbDeleteDatabase_dispatches_to_interceptors_for_connections_without_initial_catalog()
            {
                StoreItemCollection storeItemCollection;
                using (var context = new DdlDatabaseContext())
                {
                    storeItemCollection =
                        (StoreItemCollection)
                        ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
                }

                using (var connection = new SqlConnection(SimpleAttachConnectionString<DdlDatabaseContext>(useInitialCatalog: false)))
                {
                    var nonQueryInterceptor = new TestNonQueryInterceptor();
                    var readerInterceptor = new TestReaderInterceptor();
                    var dbConnectionInterceptorMock = new Mock<IDbConnectionInterceptor>();

                    // See CodePlex 1554 - Handle User Instance flakiness
                    MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(new ExecutionStrategyResolver<IDbExecutionStrategy>(
                                        SqlProviderServices.ProviderInvariantName, null, () => new SqlAzureExecutionStrategy())); 
                    try
                    {
                        if (!SqlProviderServices.Instance.DatabaseExists(connection, null, storeItemCollection))
                        {
                            SqlProviderServices.Instance.CreateDatabase(connection, null, storeItemCollection);
                        }

                        DbInterception.Add(nonQueryInterceptor);
                        DbInterception.Add(readerInterceptor);
                        DbInterception.Add(dbConnectionInterceptorMock.Object);
                        try
                        {
                            SqlProviderServices.Instance.DeleteDatabase(connection, null, storeItemCollection);
                        }
                        finally
                        {
                            DbInterception.Remove(nonQueryInterceptor);
                            DbInterception.Remove(readerInterceptor);
                            DbInterception.Remove(dbConnectionInterceptorMock.Object);
                        }
                    }
                    finally
                    {
                        MutableResolver.ClearResolvers();
                    }

                    Assert.Equal(2, nonQueryInterceptor.Commands.Count);

                    var commandTexts = nonQueryInterceptor.Commands.Select(c => c.CommandText);
                    Assert.True(commandTexts.Any(t => t.StartsWith("drop database [SYSTEM_DATA_ENTITY_SQLSERVER")));
                    Assert.True(
                        commandTexts.Any(t => t.Contains("SYSTEM.DATA.ENTITY.SQLSERVER.SQLPROVIDERSERVICESTESTS+DDLDATABASECONTEXT.MDF")));

                    Assert.Equal(1, readerInterceptor.Commands.Count);

                    Assert.True(
                        readerInterceptor.Commands.Select(
                            c => c.CommandText).Single().StartsWith("SELECT [d].[name] FROM sys.databases "));

                    dbConnectionInterceptorMock.Verify(
                        m => m.ConnectionStringGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Exactly(13));
                    dbConnectionInterceptorMock.Verify(
                        m => m.ConnectionStringGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Exactly(13));

                    dbConnectionInterceptorMock.Verify(
                        m => m.StateGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<ConnectionState>>()),
                        Times.Exactly(9));
                    dbConnectionInterceptorMock.Verify(
                        m => m.StateGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<ConnectionState>>()),
                        Times.Exactly(9));

                    dbConnectionInterceptorMock.Verify(
                        m => m.DataSourceGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Exactly(3));
                    dbConnectionInterceptorMock.Verify(
                        m => m.DataSourceGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Exactly(3));

                    dbConnectionInterceptorMock.Verify(
                        m => m.ServerVersionGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Once());
                    dbConnectionInterceptorMock.Verify(
                        m => m.ServerVersionGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                        Times.Once());

                    dbConnectionInterceptorMock.Verify(
                        m => m.Opening(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                        Times.Exactly(3));
                    dbConnectionInterceptorMock.Verify(
                        m => m.Opened(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                        Times.Exactly(3));

                    dbConnectionInterceptorMock.Verify(
                        m => m.Closing(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                        Times.Exactly(3));
                    dbConnectionInterceptorMock.Verify(
                        m => m.Closed(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()),
                        Times.Exactly(3));
                }
            }
        }

        public class DdlDatabaseContext : SimpleModelContext
        {
            static DdlDatabaseContext()
            {
                Database.SetInitializer<DdlDatabaseContext>(null);
            }
        }

        public class TestNonQueryInterceptor : DbCommandInterceptor
        {
            public readonly List<DbCommand> Commands = new List<DbCommand>();

            public override void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
            {
                Commands.Add(command);

                Assert.Empty(interceptionContext.DbContexts);
                Assert.Empty(interceptionContext.ObjectContexts);
            }
        }

        public class TestScalarInterceptor : DbCommandInterceptor
        {
            public readonly List<DbCommand> Commands = new List<DbCommand>();

            public override void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
            {
                Commands.Add(command);

                Assert.Empty(interceptionContext.DbContexts);
                Assert.Empty(interceptionContext.ObjectContexts);
            }
        }

        public class TestReaderInterceptor : DbCommandInterceptor
        {
            public readonly List<DbCommand> Commands = new List<DbCommand>();

            public override void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
            {
                Commands.Add(command);

                Assert.Empty(interceptionContext.DbContexts);
                Assert.Empty(interceptionContext.ObjectContexts);
            }
        }

        public class CreateSqlParameter : TestBase
        {
            [Fact]
            public void CreateSqlParameter_does_not_set_scale_or_precision_only_when_creating_input_parameter_with_truncate_flag_cleared()
            {
                var oldValue = SqlProviderServices.TruncateDecimalsToScale;
                SqlProviderServices.TruncateDecimalsToScale = false;
                try
                {
                    Assert.Equal(0, CreateDecimalParameter(10, 4, ParameterMode.In).Scale);
                    Assert.Equal(4, CreateDecimalParameter(10, 4, ParameterMode.Out).Scale);
                    Assert.Equal(4, CreateDecimalParameter(10, 4, ParameterMode.InOut).Scale);
                    Assert.Equal(4, CreateDecimalParameter(10, 4, ParameterMode.ReturnValue).Scale);

                    Assert.Equal(0, CreateDecimalParameter(10, 4, ParameterMode.In).Precision);
                    Assert.Equal(10, CreateDecimalParameter(10, 4, ParameterMode.Out).Precision);
                    Assert.Equal(10, CreateDecimalParameter(10, 4, ParameterMode.InOut).Precision);
                    Assert.Equal(10, CreateDecimalParameter(10, 4, ParameterMode.ReturnValue).Precision);
                }
                finally
                {
                    SqlProviderServices.TruncateDecimalsToScale = oldValue;
                }
            }

            [Fact]
            public void CreateSqlParameter_sets_scale_and_precision_when_creating_any_parameter_with_truncate_flag_set()
            {
                Assert.Equal(4, CreateDecimalParameter(10, 4, ParameterMode.In).Scale);
                Assert.Equal(4, CreateDecimalParameter(10, 4, ParameterMode.Out).Scale);
                Assert.Equal(4, CreateDecimalParameter(10, 4, ParameterMode.InOut).Scale);
                Assert.Equal(4, CreateDecimalParameter(10, 4, ParameterMode.ReturnValue).Scale);

                Assert.Equal(10, CreateDecimalParameter(10, 4, ParameterMode.In).Precision);
                Assert.Equal(10, CreateDecimalParameter(10, 4, ParameterMode.Out).Precision);
                Assert.Equal(10, CreateDecimalParameter(10, 4, ParameterMode.InOut).Precision);
                Assert.Equal(10, CreateDecimalParameter(10, 4, ParameterMode.ReturnValue).Precision);
            }

            private static SqlParameter CreateDecimalParameter(byte precision, byte scale, ParameterMode parameterMode)
            {
                return SqlProviderServices.CreateSqlParameter(
                    "Lily",
                    TypeUsage.CreateDecimalTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Decimal), precision, scale),
                    parameterMode,
                    99999999.88888888m,
                    true,
                    SqlVersion.Sql11);
            }
        }
    }
}
