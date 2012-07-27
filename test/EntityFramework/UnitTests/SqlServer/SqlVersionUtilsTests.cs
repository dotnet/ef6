// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Common;
    using System.Data.Entity.SqlServer.Resources;
    using Moq;
    using Xunit;

    public class SqlVersionUtilsTests
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

        private static DbConnection CreateConnectionForVersion(string version)
        {
            var mockConnection = new Mock<DbConnection>();
            mockConnection.Setup(m => m.State).Returns(ConnectionState.Open);
            mockConnection.Setup(m => m.ServerVersion).Returns(version);

            return mockConnection.Object;
        }

        [Fact]
        public void GetVersionHint_returns_correct_manifest_token_for_version()
        {
            Assert.Equal("2000", SqlVersionUtils.GetVersionHint(SqlVersion.Sql8));
            Assert.Equal("2005", SqlVersionUtils.GetVersionHint(SqlVersion.Sql9));
            Assert.Equal("2008", SqlVersionUtils.GetVersionHint(SqlVersion.Sql10));
            Assert.Equal("2012", SqlVersionUtils.GetVersionHint(SqlVersion.Sql11));
        }

        [Fact]
        public void GetVersionHint_throws_for_unknown_version()
        {
            Assert.Equal(
                Strings.UnableToDetermineStoreVersion,
                Assert.Throws<ArgumentException>(() => SqlVersionUtils.GetVersionHint((SqlVersion)120)).Message);
        }

        [Fact]
        public void GetSqlVersion_returns_correct_version_for_manifest_token()
        {
            Assert.Equal(SqlVersion.Sql8, SqlVersionUtils.GetSqlVersion("2000"));
            Assert.Equal(SqlVersion.Sql9, SqlVersionUtils.GetSqlVersion("2005"));
            Assert.Equal(SqlVersion.Sql10, SqlVersionUtils.GetSqlVersion("2008"));
            Assert.Equal(SqlVersion.Sql11, SqlVersionUtils.GetSqlVersion("2012"));
        }

        [Fact]
        public void GetSqlVersion_throws_for_unknown_version()
        {
            Assert.Equal(
                Strings.UnableToDetermineStoreVersion,
                Assert.Throws<ArgumentException>(() => SqlVersionUtils.GetSqlVersion("2014")).Message);
        }
    }
}
