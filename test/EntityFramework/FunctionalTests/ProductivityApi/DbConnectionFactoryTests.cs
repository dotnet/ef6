// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Functionals.Utilities;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;
    using System.IO;
    using System.Reflection;
    using Xunit;

    /// <summary>
    /// Functional tests for implementations of IDbConnectionFactory.
    /// </summary>
    public class DbConnectionFactoryTests : FunctionalTestBase
    {
        #region Infrastructure/setup

        private static readonly Lazy<Assembly> _sqlCeAssembly
            =
            new Lazy<Assembly>(() => new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0").CreateConnection("Dummy").GetType().Assembly());

        #endregion

        #region Positive SqlCeConnectionFactory tests

        [Fact]
        public void SqlCeConnectionFactory_creates_a_SQL_CE_connection_from_a_database_name()
        {
            using (
                var connection =
                    new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0").CreateConnection("FakeDatabaseName"))
            {
                var sqlCeExceptionType = _sqlCeAssembly.Value.GetType("System.Data.SqlServerCe.SqlCeConnection");
                Assert.IsType(sqlCeExceptionType, connection);
                Assert.Equal("Data Source=|DataDirectory|FakeDatabaseName.sdf; ", connection.ConnectionString);
            }
        }

        [Fact]
        public void
            SqlCeConnectionFactory_creates_a_SQL_CE_connection_using_changed_database_path_and_base_connection_string()
        {
            var factory = new SqlCeConnectionFactory(
                "System.Data.SqlServerCe.4.0", @"C:\VicAndBob\",
                "Persist Security Info=False");
            using (var connection = factory.CreateConnection("FakeDatabaseName"))
            {
                var sqlCeExceptionType = _sqlCeAssembly.Value.GetType("System.Data.SqlServerCe.SqlCeConnection");
                Assert.IsType(sqlCeExceptionType, connection);
                Assert.Equal(
                    @"Data Source=C:\VicAndBob\FakeDatabaseName.sdf; Persist Security Info=False",
                    connection.ConnectionString);
            }
        }

        #endregion

        #region Negative SqlCeConnectionFactory tests

        [Fact]
        public void SqlCeConnectionFactory_throws_when_a_connection_with_bad_database_path_is_used()
        {
            var factory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0", @"//C:::\\\D:\\D::::D\\\", "");
            using (var connection = factory.CreateConnection("FakeDatabaseName"))
            {
                Assert.Throws<NotSupportedException>(() => connection.Open()).ValidateMessage(
                    typeof(File).Assembly(),
                    "Argument_PathFormatNotSupported",
                    null);
            }
        }

        [Fact]
        public void SqlCeConnectionFactory_throws_when_a_base_connection_already_containing_a_data_source_is_used()
        {
            var factory = new SqlCeConnectionFactory(
                "System.Data.SqlServerCe.4.0", "",
                "Data Source=VicAndBobsDatabase.sdf");
            using (var connection = factory.CreateConnection("FakeDatabaseName"))
            {
                var sqlCeExceptionType = _sqlCeAssembly.Value.GetType("System.Data.SqlServerCe.SqlCeException");
                try
                {
                    connection.Open();
                    Assert.True(false);
                }
                catch (Exception ex)
                {
                    Assert.IsType(sqlCeExceptionType, ex);
                    Assert.True(ex.Message.Contains("VicAndBobsDatabase.sdf"));
                }
            }
        }

        [Fact]
        public void SqlCeConnectionFactory_throws_when_a_bad_base_connection_string_is_used()
        {
            var factory = new SqlCeConnectionFactory(
                "System.Data.SqlServerCe.4.0", "",
                "Whats On The End Of The Stick Vic=Admiral Nelsons Final Flannel");

            Assert.Throws<ArgumentException>(() => factory.CreateConnection("Something")).ValidateMessage(
                _sqlCeAssembly.Value, "ADP_KeywordNotSupported", null, "whats on the end of the stick vic");
        }

        #endregion

        #region Negative SqlConnectionFactory tests

        [Fact]
        public void SqlConnectionFactory_throws_when_a_bad_base_connection_string_is_used()
        {
            var factory = new SqlConnectionFactory("You Wouldnt Let It Lie=True");
            Assert.Throws<ArgumentException>(() => factory.CreateConnection("FakeDatabaseName")).ValidateMessage(
                typeof(SqlConnection).Assembly(), "ADP_KeywordNotSupported", null, "you wouldnt let it lie");
        }

        #endregion
    }
}
