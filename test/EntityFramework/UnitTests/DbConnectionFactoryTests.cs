// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace ProductivityApiUnitTests
{
    using System;
    using System.Data.Entity.Core;
    using System.Data;
    using System.Data.Entity.Core.Common;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Data.SqlClient;
    using System.IO;
    using Moq;
    using Moq.Protected;
    using Xunit;

    /// <summary>
    /// Unit tests for implementations of IDbConnectionFactory.
    /// </summary> 
    public class DbConnectionFactoryTests : TestBase
    {
        #region Test setup

        static DbConnectionFactoryTests()
        {
            FakeSqlProviderFactory.Initialize();
        }

        #endregion

        #region Positive SqlCeConnectionFactory tests

        [Fact]
        public void SqlCeConnectionFactory_uses_database_name_as_sdf_filename()
        {
            var factory = new SqlCeConnectionFactory("System.Data.FakeSqlClient");

            var connection = factory.CreateConnection("FakeDatabaseName");

            Assert.Equal("Data Source=|DataDirectory|FakeDatabaseName.sdf; ", connection.ConnectionString);
        }

        [Fact]
        public void SqlCeConnectionFactory_uses_database_name_as_sdf_filename_even_when_database_name_already_ends_in_sdf()
        {
            var factory = new SqlCeConnectionFactory("System.Data.FakeSqlClient");

            var connection = factory.CreateConnection("FakeDatabaseName.sdf");

            Assert.Equal("Data Source=|DataDirectory|FakeDatabaseName.sdf; ", connection.ConnectionString);
        }

        [Fact]
        public void SqlCeConnectionFactory_default_database_path_is_DataDirectory()
        {
            Assert.Equal("|DataDirectory|", new SqlCeConnectionFactory("System.Data.FakeSqlClient").DatabaseDirectory);
        }

        [Fact]
        public void SqlCeConnectionFactory_default_base_connection_string_is_empty()
        {
            Assert.Equal("", new SqlCeConnectionFactory("System.Data.FakeSqlClient").BaseConnectionString);
        }

        [Fact]
        public void SqlCeConnectionFactory_sets_provider_invariant_name_to_value_from_constructor()
        {
            var factory = new SqlCeConnectionFactory("System.Data.FakeSqlClient");

            Assert.Equal("System.Data.FakeSqlClient", factory.ProviderInvariantName);
        }

        [Fact]
        public void SqlCeConnectionFactory_sets_provider_invariant_name_to_value_from_long_constructor()
        {
            var factory = new SqlCeConnectionFactory("System.Data.FakeSqlClient", "", "");

            Assert.Equal("System.Data.FakeSqlClient", factory.ProviderInvariantName);
        }

        [Fact]
        public void SqlCeConnectionFactory_uses_changed_database_path_when_creating_connection_string()
        {
            var factory = new SqlCeConnectionFactory("System.Data.FakeSqlClient", @"C:\VicAndBobs\Novelty Island", "");

            Assert.Equal(@"C:\VicAndBobs\Novelty Island", factory.DatabaseDirectory);

            var connection = factory.CreateConnection("FakeDatabaseName");

            Assert.Equal(@"Data Source=C:\VicAndBobs\Novelty Island\FakeDatabaseName.sdf; ", connection.ConnectionString);
        }

        [Fact]
        public void SqlCeConnectionFactory_with_properly_formed_environment_style_is_concatenated_correctly()
        {
            var factory = new SqlCeConnectionFactory("System.Data.FakeSqlClient", @"|BuffyLovesAngel|", "");

            var connection = factory.CreateConnection("FakeDatabaseName");

            Assert.Equal(@"Data Source=|BuffyLovesAngel|FakeDatabaseName.sdf; ", connection.ConnectionString);
        }

        [Fact]
        public void SqlCeConnectionFactory_uses_changed_base_connection_string_when_creating_connection_string()
        {
            var factory = new SqlCeConnectionFactory("System.Data.FakeSqlClient", "", "Persist Security Info=False");

            Assert.Equal("Persist Security Info=False", factory.BaseConnectionString);

            var connection = factory.CreateConnection("FakeDatabaseName");

            Assert.Equal("Data Source=FakeDatabaseName.sdf; Persist Security Info=False", connection.ConnectionString);
        }

        #endregion

        #region Negative SqlCeConnectionFactory tests

        [Fact]
        public void SqlCeConnectionFactory_treats_improperly_end_formed_environment_style_as_path_which_then_throws()
        {
            var factory = new SqlCeConnectionFactory("System.Data.FakeSqlClient", @"|SpikeLovesDrusila", "");

            Assert.Equal(GenerateException(() => Path.Combine("|", "Willow")).Message, Assert.Throws<ArgumentException>(() => factory.CreateConnection("FakeDatabaseName")).Message);
        }

        [Fact]
        public void SqlCeConnectionFactory_treats_improperly_start_formed_environment_style_as_path_which_then_throws()
        {
            var factory = new SqlCeConnectionFactory("System.Data.FakeSqlClient", @"AnyaLovesXander|", "");

            Assert.Equal(GenerateException(() => Path.Combine("|", "Willow")).Message, Assert.Throws<ArgumentException>(() => factory.CreateConnection("FakeDatabaseName")).Message);
        }

        [Fact]
        public void SqlCeConnectionFactory_throws_when_given_null_database_name()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"), Assert.Throws<ArgumentException>(() => new SqlCeConnectionFactory("System.Data.FakeSqlClient").CreateConnection(null)).Message);
        }

        [Fact]
        public void SqlCeConnectionFactory_throws_when_given_empty_database_name()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"), Assert.Throws<ArgumentException>(() => new SqlCeConnectionFactory("System.Data.FakeSqlClient").CreateConnection("")).Message);
        }

        [Fact]
        public void SqlCeConnectionFactory_throws_when_given_whitespace_database_name()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"), Assert.Throws<ArgumentException>(() => new SqlCeConnectionFactory("System.Data.FakeSqlClient").CreateConnection(" ")).Message);
        }

        [Fact]
        public void SqlCeConnectionFactory_throws_when_given_null_provider_invariant_name()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("providerInvariantName"), Assert.Throws<ArgumentException>(() => new SqlCeConnectionFactory(null)).Message);
        }

        [Fact]
        public void SqlCeConnectionFactory_throws_when_given_empty_provider_invariant_name()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("providerInvariantName"), Assert.Throws<ArgumentException>(() => new SqlCeConnectionFactory("")).Message);
        }

        [Fact]
        public void SqlCeConnectionFactory_throws_when_given_whitespace_provider_invariant_name()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("providerInvariantName"), Assert.Throws<ArgumentException>(() => new SqlCeConnectionFactory(" ")).Message);
        }

        [Fact]
        public void SqlCeConnectionFactory_throws_when_given_null_provider_invariant_name_to_long_constructor()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("providerInvariantName"), Assert.Throws<ArgumentException>(() => new SqlCeConnectionFactory(null, "", "")).Message);
        }

        [Fact]
        public void SqlCeConnectionFactory_throws_when_given_empty_provider_invariant_name_to_long_constructor()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("providerInvariantName"), Assert.Throws<ArgumentException>(() => new SqlCeConnectionFactory("", "", "")).Message);
        }

        [Fact]
        public void SqlCeConnectionFactory_throws_when_given_whitespace_provider_invariant_name_to_long_constructor()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("providerInvariantName"), Assert.Throws<ArgumentException>(() => new SqlCeConnectionFactory(" ", "", "")).Message);
        }

        [Fact]
        public void SqlCeConnectionFactory_throws_when_given_null_base_connection_string()
        {
            Assert.Equal("baseConnectionString", Assert.Throws<ArgumentNullException>(() => new SqlCeConnectionFactory("System.Data.FakeSqlClient", "", null)).ParamName);
        }

        [Fact]
        public void SqlCeConnectionFactory_throws_when_given_null_database_path()
        {
            Assert.Equal("databaseDirectory", Assert.Throws<ArgumentNullException>(() => new SqlCeConnectionFactory("System.Data.FakeSqlClient", null, "")).ParamName);
        }

        [Fact]
        public void SqlCeConnectionFactory_throws_when_provider_returns_null_connection()
        {
            try
            {
                FakeSqlProviderFactory.Instance.ForceNullConnection = true;
                var factory = new SqlCeConnectionFactory("System.Data.FakeSqlClient");

                Assert.Equal(Strings.DbContext_ProviderReturnedNullConnection, Assert.Throws<InvalidOperationException>(() => factory.CreateConnection("FakeDatabaseName")).Message);
            }
            finally
            {
                FakeSqlProviderFactory.Instance.ForceNullConnection = false;
            }
        }

        #endregion

        #region Positive SqlConnectionFactory tests

        [Fact]
        public void SqlConnectionFactory_creates_SqlConnection()
        {
            var factory = new SqlConnectionFactory();

            var connection = factory.CreateConnection("FakeDatabaseName");

            Assert.IsType<SqlConnection>(connection);
        }

        [Fact]
        public void SqlConnectionFactory_uses_database_name_as_initial_catalog()
        {
            var factory = new SqlConnectionFactory();

            var connection = factory.CreateConnection("FakeDatabaseName");

            Assert.True(connection.ConnectionString.Contains("Initial Catalog=FakeDatabaseName"));
            AssertConnectionStringHasDefaultParts(connection.ConnectionString);
        }

        private void AssertConnectionStringHasDefaultParts(string connectionString)
        {
            Assert.True(connectionString.Contains(@"Data Source=.\SQLEXPRESS"));
            Assert.True(connectionString.Contains("Integrated Security=True"));
            Assert.True(connectionString.Contains("MultipleActiveResultSets=True"));
        }

        [Fact]
        public void SqlConnectionFactory_throws_if_given_MDF_filename()
        {
            var factory = new SqlConnectionFactory();

            Assert.Equal(Strings.SqlConnectionFactory_MdfNotSupported("FakeDatabaseName.mdf"), Assert.Throws<NotSupportedException>(() => factory.CreateConnection("FakeDatabaseName.mdf")).Message);
        }

        [Fact]
        public void SqlConnectionFactory_default_base_connection_string_is_as_expected()
        {
            AssertConnectionStringHasDefaultParts(new SqlConnectionFactory().BaseConnectionString);
        }

        [Fact]
        public void SqlConnectionFactory_uses_changed_base_connection_string_when_creating_connection_string()
        {
            var factory = new SqlConnectionFactory("Data Source=190.190.200.100,1433");

            Assert.Equal("Data Source=190.190.200.100,1433", factory.BaseConnectionString);

            var connection = factory.CreateConnection("FakeDatabaseName");

            Assert.True(connection.ConnectionString.Contains("Data Source=190.190.200.100,1433"));
            Assert.True(connection.ConnectionString.Contains("Initial Catalog=FakeDatabaseName"));
        }

        [Fact]
        public void SqlConnectionFactory_replaces_any_initial_catalog_set_in_the_base_connection()
        {
            var factory = new SqlConnectionFactory(@"Data Source=.\SQLEXPRESS; Integrated Security=True; Initial Catalog=TheManWithTheStick");

            var connection = factory.CreateConnection("FakeDatabaseName");

            Assert.False(connection.ConnectionString.Contains("Initial Catalog=TheManWithTheStick"));
            Assert.True(connection.ConnectionString.Contains("Initial Catalog=FakeDatabaseName"));
        }

        [Fact]
        public void SqlConnectionFactory_default_db_provider_factory_creates_sql_connection()
        {
            var factory = new SqlConnectionFactory();

            var providerFactory = factory.ProviderFactory("System.Data.SqlClient");

            Assert.Equal(typeof(SqlClientFactory), providerFactory.GetType());
        }

        [Fact]
        public void SqlConnectionFactory_uses_db_provider_factory()
        {
            var mockProviderFactory = new Mock<DbProviderFactory>();
            var mockConnection = new Mock<DbConnection>();
            mockConnection.SetupProperty(c => c.ConnectionString);
            mockProviderFactory.Setup(m => m.CreateConnection()).Returns(mockConnection.Object);

            var factory = new SqlConnectionFactory { ProviderFactory = name => mockProviderFactory.Object };

            var connection = factory.CreateConnection("FakeDatabaseName");

            Assert.Same(mockConnection.Object, connection);
            Assert.True(connection.ConnectionString.Contains("Initial Catalog=FakeDatabaseName"));
        }

        [Fact]
        public void SqlConnectionFactory_handles_no_provider_factory_found()
        {
            var factory = new SqlConnectionFactory { ProviderFactory = name => null };

            var connection = factory.CreateConnection("FakeDatabaseName");

            Assert.Equal(typeof(SqlConnection), connection.GetType());
            Assert.True(connection.ConnectionString.Contains("Initial Catalog=FakeDatabaseName"));
        }

        [Fact]
        public void SqlConnectionFactory_handles_provider_factory_returning_null_connection()
        {
            var mockProviderFactory = new Mock<DbProviderFactory>();
            mockProviderFactory.Setup(m => m.CreateConnection()).Returns<DbConnection>(null);

            var factory = new SqlConnectionFactory { ProviderFactory = name => mockProviderFactory.Object };

            var connection = factory.CreateConnection("FakeDatabaseName");

            Assert.Equal(typeof(SqlConnection), connection.GetType());
            Assert.True(connection.ConnectionString.Contains("Initial Catalog=FakeDatabaseName"));
        }

        #endregion

        #region Negative SqlConnectionFactory tests

        [Fact]
        public void SqlConnectionFactory_throws_when_given_null_database_name()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"), Assert.Throws<ArgumentException>(() => new SqlConnectionFactory().CreateConnection(null)).Message);
        }

        [Fact]
        public void SqlConnectionFactory_throws_when_given_empty_database_name()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"), Assert.Throws<ArgumentException>(() => new SqlConnectionFactory().CreateConnection("")).Message);
        }

        [Fact]
        public void SqlConnectionFactory_throws_when_given_whitespace_database_name()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"), Assert.Throws<ArgumentException>(() => new SqlConnectionFactory().CreateConnection(" ")).Message);
        }

        [Fact]
        public void SqlConnectionFactory_throws_when_given_null_base_connection_string()
        {
            Assert.Equal("baseConnectionString", Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(null)).ParamName);
        }

        #endregion

        #region Exception thrown for bad connection (Dev11 364657)

        [Fact]
        public void Useful_exception_is_thrown_by_GetProviderManifestTokenChecked_if_bad_MVC4_connection_string_is_used()
        {
            Useful_exception_is_thrown_by_GetProviderManifestTokenChecked_if_bad_connection_string_is_used(
                "Data Source=(localdb)\v11.0", Strings.BadLocalDBDatabaseName);
        }

        [Fact]
        public void Useful_exception_is_thrown_by_GetProviderManifestTokenChecked_if_general_bad_connection_string_is_used()
        {
            Useful_exception_is_thrown_by_GetProviderManifestTokenChecked_if_bad_connection_string_is_used(
                "Data Source=WotNoServer", Strings.FailedToGetProviderInformation);
        }

        [Fact]
        public void Useful_exception_is_thrown_by_GetProviderManifestTokenChecked_if_correct_LocalDB_name_is_used_but_it_still_fails()
        {
            Useful_exception_is_thrown_by_GetProviderManifestTokenChecked_if_bad_connection_string_is_used(
                "Data Source=(localdb)\\v11.0", Strings.FailedToGetProviderInformation);
        }

        private void Useful_exception_is_thrown_by_GetProviderManifestTokenChecked_if_bad_connection_string_is_used(string connectionString, string expectedMessage)
        {
            var fakeConnection = new SqlConnection(connectionString);
            var innerException = new ProviderIncompatibleException();

            var mockProviderServices = new Mock<DbProviderServices>();
            mockProviderServices
                .Protected()
                .Setup("GetDbProviderManifestToken", fakeConnection)
                .Throws(innerException);

            var ex = Assert.Throws<ProviderIncompatibleException>(() => mockProviderServices.Object.GetProviderManifestTokenChecked(fakeConnection));
            Assert.Equal(expectedMessage, ex.Message);
            Assert.Same(innerException, ex.InnerException);
        }

        #endregion

        #region LocalDbConnectionFactory tests

        [Fact]
        public void LocalDbConnectionFactory_throws_when_null_base_connection_string_is_used()
        {
            Assert.Equal(
                "baseConnectionString",
                Assert.Throws<ArgumentNullException>(() => new LocalDbConnectionFactory("v11.0", null)).ParamName);
        }

        [Fact]
        public void LocalDbConnectionFactory_throws_when_null_or_empty_LocalDb_version_is_used()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("localDbVersion"),
                Assert.Throws<ArgumentException>(() => new LocalDbConnectionFactory(null)).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("localDbVersion"),
                Assert.Throws<ArgumentException>(() => new LocalDbConnectionFactory("")).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("localDbVersion"),
                Assert.Throws<ArgumentException>(() => new LocalDbConnectionFactory(" ")).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("localDbVersion"),
                Assert.Throws<ArgumentException>(() => new LocalDbConnectionFactory(null, "")).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("localDbVersion"),
                Assert.Throws<ArgumentException>(() => new LocalDbConnectionFactory("", "")).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("localDbVersion"),
                Assert.Throws<ArgumentException>(() => new LocalDbConnectionFactory(" ", "")).Message);
        }

        [Fact]
        public void LocalDbConnectionFactory_creates_a_LocalDb_connection_from_a_database_name()
        {
            using (var connection = new LocalDbConnectionFactory("v99").CreateConnection("MyDatabase"))
            {
                Assert.Equal(@"(localdb)\v99", connection.DataSource);
                Assert.Equal(@"MyDatabase", connection.Database);
            }
        }

        [Fact]
        public void LocalDbConnectionFactory_just_uses_full_connection_string_if_given_one()
        {
            using (var connection = new LocalDbConnectionFactory("v99").CreateConnection("Database=AndBingoWasHisNameo"))
            {
                Assert.Equal("Database=AndBingoWasHisNameo", connection.ConnectionString);
            }
        }

        [Fact]
        public void LocalDbConnectionFactory_uses_database_name_as_mdf_filename_if_DataDirectory_is_set()
        {
            WithDataDirectory(@"C:\Some\Data\Directory",
                () =>
                {
                    using (var connection = new LocalDbConnectionFactory("v99").CreateConnection("MyDatabase"))
                    {
                        Assert.Equal(
                            @"Data Source=(localdb)\v99;AttachDbFilename=|DataDirectory|MyDatabase.mdf;Initial Catalog=MyDatabase;Integrated Security=True;MultipleActiveResultSets=True",
                            connection.ConnectionString);
                    }
                });
        }

        [Fact]
        public void LocalDbConnectionFactory_does_not_set_AttachDbFileName_if_DataDirectory_is_not_set()
        {
            WithDataDirectory(null,
                () =>
                {
                    using (var connection = new LocalDbConnectionFactory("v99").CreateConnection("MyDatabase"))
                    {
                        Assert.Equal(
                            @"Data Source=(localdb)\v99;Initial Catalog=MyDatabase;Integrated Security=True;MultipleActiveResultSets=True",
                            connection.ConnectionString);
                    }
                });
        }

        [Fact]
        public void LocalDbConnectionFactory_creates_a_LocalDb_connection_from_a_database_name_using_given_base_connection_string()
        {
            WithDataDirectory(null,
                () =>
                {
                    using (var connection = new LocalDbConnectionFactory("v99", "Integrated Security=True").CreateConnection("MyDatabase"))
                    {
                        Assert.Equal(
                            @"Data Source=(localdb)\v99;Initial Catalog=MyDatabase;Integrated Security=True",
                            connection.ConnectionString);
                    }
                });
        }

        [Fact]
        public void LocalDbConnectionFactory_creates_a_LocalDb_connection_with_AttachDbFileName_from_a_database_name_using_given_base_connection_string_if_DataDirectory_is_set()
        {
            WithDataDirectory(@"C:\Some\Data\Directory",
                () =>
                {
                    using (var connection = new LocalDbConnectionFactory("v99", "Integrated Security=True").CreateConnection("MyDatabase"))
                    {
                        Assert.Equal(
                            @"Data Source=(localdb)\v99;AttachDbFilename=|DataDirectory|MyDatabase.mdf;Initial Catalog=MyDatabase;Integrated Security=True",
                            connection.ConnectionString);
                    }
                });
        }

        [Fact]
        public void LocalDbConnectionFactory_uses_Data_Source_set_in_base_connection_string_if_set()
        {
            using (var connection = new LocalDbConnectionFactory("v99", "Data Source=NotLocalDb").CreateConnection("MyDatabase"))
            {
                Assert.Equal("NotLocalDb", connection.DataSource);
            }
        }

        [Fact]
        public void LocalDbConnectionFactory_uses_AttachDbFilename_set_by_factory_even_if_set_in_base_connection_string()
        {
            WithDataDirectory(@"C:\Some\Data\Directory",
                () =>
                {
                    using (var connection = new LocalDbConnectionFactory("v99", "AttachDbFilename=|DataDirectory|ADifferent.mdf;")
                                .CreateConnection("MyDatabase"))
                    {
                        Assert.Equal(
                            "|DataDirectory|MyDatabase.mdf",
                            new DbConnectionStringBuilder { ConnectionString = connection.ConnectionString }["AttachDbFilename"]);
                    }
                });
        }

        [Fact]
        public void LocalDbConnectionFactory_uses_Initial_Catalog_set_by_factory_even_if_set_in_base_connection_string()
        {
            using (var connection = new LocalDbConnectionFactory("v99", "Initial Catalog=YourDatabase").CreateConnection("MyDatabase"))
            {
                Assert.Equal("MyDatabase", connection.Database);
            }
        }

        private static void WithDataDirectory(string dataDirectory, Action test)
        {
            var previousDataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory");
            try
            {
                AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory);
                test();
            }
            finally
            {
                AppDomain.CurrentDomain.SetData("DataDirectory", previousDataDirectory);
            }
        }

        #endregion
    }
}