// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.SqlServer.Resources;
    using System.Linq;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class SqlVersionUtilsTests
    {
        public class GetSqlVersion
        {
            [Fact]
            public void GetSqlVersion_returns_Sql11_for_server_version_string_greater_than_or_equal_to_11()
            {
                Assert.Equal(SqlVersion.Sql11, SqlVersionUtils.GetSqlVersion(CreateConnectionForVersion("11.12.1234")));
                Assert.Equal(SqlVersion.Sql11, SqlVersionUtils.GetSqlVersion(CreateConnectionForVersion("12.12.1234")));
            }

            [Fact]
            public void GetSqlVersion_returns_Sql10_for_server_version_string_greater_equal_to_10()
            {
                Assert.Equal(SqlVersion.Sql10, SqlVersionUtils.GetSqlVersion(CreateConnectionForVersion("10.12.1234")));
            }

            [Fact]
            public void GetSqlVersion_returns_Sql9_for_server_version_string_greater_equal_to_9()
            {
                Assert.Equal(SqlVersion.Sql9, SqlVersionUtils.GetSqlVersion(CreateConnectionForVersion("09.12.1234")));
            }

            [Fact]
            public void GetSqlVersion_returns_Sql8_for_server_version_string_greater_equal_to_8()
            {
                Assert.Equal(SqlVersion.Sql8, SqlVersionUtils.GetSqlVersion(CreateConnectionForVersion("08.12.1234")));
            }

            [Fact]
            public void GetSqlVersion_returns_correct_version_for_manifest_token()
            {
                Assert.Equal(SqlVersion.Sql8, SqlVersionUtils.GetSqlVersion("2000"));
                Assert.Equal(SqlVersion.Sql9, SqlVersionUtils.GetSqlVersion("2005"));
                Assert.Equal(SqlVersion.Sql10, SqlVersionUtils.GetSqlVersion("2008"));
                Assert.Equal(SqlVersion.Sql11, SqlVersionUtils.GetSqlVersion("2012"));
                Assert.Equal(SqlVersion.Sql11, SqlVersionUtils.GetSqlVersion("2012.Azure"));
            }

            [Fact]
            public void GetSqlVersion_throws_for_unknown_version()
            {
                Assert.Equal(
                    Strings.UnableToDetermineStoreVersion,
                    Assert.Throws<ArgumentException>(() => SqlVersionUtils.GetSqlVersion("2014")).Message);
            }

            private static DbConnection CreateConnectionForVersion(string version)
            {
                var mockConnection = new Mock<DbConnection>();
                mockConnection.Setup(m => m.State).Returns(ConnectionState.Open);
                mockConnection.Setup(m => m.ServerVersion).Returns(version);

                return mockConnection.Object;
            }
        }

        public class GetVersionHint
        {
            [Fact]
            public void GetVersionHint_returns_correct_manifest_token_for_version()
            {
                Assert.Equal("2000", SqlVersionUtils.GetVersionHint(SqlVersion.Sql8, ServerType.OnPremises));
                Assert.Equal("2005", SqlVersionUtils.GetVersionHint(SqlVersion.Sql9, ServerType.OnPremises));
                Assert.Equal("2008", SqlVersionUtils.GetVersionHint(SqlVersion.Sql10, ServerType.OnPremises));
                Assert.Equal("2012", SqlVersionUtils.GetVersionHint(SqlVersion.Sql11, ServerType.OnPremises));
                Assert.Equal("2012.Azure", SqlVersionUtils.GetVersionHint(SqlVersion.Sql11, ServerType.Cloud));
            }

            [Fact]
            public void GetVersionHint_throws_for_unknown_version()
            {
                Assert.Equal(
                    Strings.UnableToDetermineStoreVersion,
                    Assert.Throws<ArgumentException>(() => SqlVersionUtils.GetVersionHint((SqlVersion)120, ServerType.OnPremises)).Message);
            }
        }

        public class GetServerType
        {
            [Fact]
            public void GetServerType_returns_OnPremises_for_server_with_EngineEdtion_not_equal_to_five()
            {
                Assert.Equal(ServerType.OnPremises, SqlVersionUtils.GetServerType(CreateConnectionForAzureQuery(1).Object));
            }

            [Fact]
            public void GetServerType_returns_Cloud_for_server_with_EngineEdtion_equal_to_five()
            {
                Assert.Equal(ServerType.Cloud, SqlVersionUtils.GetServerType(CreateConnectionForAzureQuery(5).Object));
            }

            private static Mock<DbConnection> CreateConnectionForAzureQuery(int engineEdition)
            {
                var mockReader = new Mock<DbDataReader>();
                mockReader.Setup(m => m.GetInt32(0)).Returns(engineEdition);

                var mockCommand = new Mock<DbCommand>();
                mockCommand.Protected().Setup<DbDataReader>("ExecuteDbDataReader", CommandBehavior.Default).Returns(mockReader.Object);

                var mockConnection = new Mock<DbConnection>();
                mockConnection.Setup(m => m.State).Returns(ConnectionState.Open);
                mockConnection.Protected().Setup<DbCommand>("CreateDbCommand").Returns(mockCommand.Object);
                return mockConnection;
            }

            [Fact]
            public void GetServerType_returns_OnPremises_for_real_local_test_database()
            {
                using (var context = new RealDatabase())
                {
                    context.Database.CreateIfNotExists();

                    var connection = context.Database.Connection;
                    connection.Open();

                    Assert.Equal(ServerType.OnPremises, SqlVersionUtils.GetServerType(connection));

                    connection.Close();
                }
            }

            public class RealDatabase : DbContext 
            {
            }

            [Fact]
            public void GetServerType_dispatches_commands_to_interceptors()
            {
                var connection = CreateConnectionForAzureQuery(5).Object;

                var interceptor = new TestReaderInterceptor();
                DbInterception.Add(interceptor);
                try
                {
                    SqlVersionUtils.GetServerType(connection);
                }
                finally
                {
                    DbInterception.Remove(interceptor);
                }

                Assert.Equal(1, interceptor.Commands.Count);
                Assert.Same(connection.CreateCommand(), interceptor.Commands.Single());
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
        }
    }
}
