// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class SqlProviderServicesTests
    {
        public class GetExecutionStrategyFactory : TestBase
        {
            [Fact]
            public void GetExecutionStrategyFactory_returns_DefaultSqlExecutionStrategy()
            {
                Assert.IsType<DefaultSqlExecutionStrategy>(SqlProviderServices.Instance.GetExecutionStrategyFactory()());
            }
        }

        public class ProviderInvariantNameAttribute : TestBase
        {
            [Fact]
            public void Has_ProviderInvariantNameAttribute()
            {
                Assert.Equal(
                    "System.Data.SqlClient",
                    DbProviderNameAttribute.GetFromType(typeof(SqlProviderServices)).Single().Name);
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
        }

        public class CreateDatabaseFromScript: TestBase
        {
            [Fact]
            public void CreateDatabaseFromScript_returns_expected_SQL_Version_from_master_connection()
            {
                var mockMasterConnection = CreateConnectionForTokenLookup("10", azure: false);
                var mockConnection = CreateConnectionForTokenLookup("00", azure: false, master: mockMasterConnection.Object);

                Assert.Equal(
                    SqlVersion.Sql10,
                    SqlProviderServices.CreateDatabaseFromScript(null, mockConnection.Object, ""));
            }
        }

        private static Mock<DbConnection> CreateConnectionForTokenLookup(string majorVersion, bool azure, DbConnection master = null)
        {
            var mockReader = new Mock<DbDataReader>();
            mockReader.Setup(m => m.GetInt32(0)).Returns(azure ? 5 : 2);

            var mockCommand = new Mock<DbCommand>();
            mockCommand.Protected().Setup<DbDataReader>("ExecuteDbDataReader", CommandBehavior.Default).Returns(mockReader.Object);

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
    }
}
