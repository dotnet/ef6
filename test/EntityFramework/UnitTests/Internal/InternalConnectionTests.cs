// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System;
    using System.Configuration;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.SqlClient;
    using System.Data.SqlServerCe;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests for creating and managing connections using IInternalConnection.
    /// </summary>
    public class InternalConnectionTests : TestBase
    {
        #region Positive existing connection tests

        [Fact]
        public void Existing_connection_provided_is_returned()
        {
            using (var connection = new SqlConnection())
            {
                using (var internalConnection = new EagerInternalConnection(new DbContext(connection, false),
                    connection, connectionOwned: false))
                {
                    Assert.Same(connection, internalConnection.Connection);
                }
            }
        }

        #endregion

        #region Positive app.config connection tests

        [Fact]
        public void Full_name_of_context_is_found_in_app_config()
        {
            using (var internalConnection = new LazyInternalConnection("Couger35.Hubcap.FullNameDbContext"))
            {
                var connection = internalConnection.Connection;

                Assert.Equal("FullNameInAppConfig", connection.Database);
                Assert.IsType<SqlConnection>(connection);
            }
        }

        [Fact]
        public void Name_of_context_with_Context_stripped_is_not_found_in_app_config()
        {
            using (var internalConnection = new LazyInternalConnection("Couger35.Hubcap.FullNameStrippedContext"))
            {
                var connection = internalConnection.Connection;

                Assert.Equal("Couger35.Hubcap.FullNameStrippedContext", connection.Database);
                Assert.IsType<SqlConnection>(connection);
            }
        }

        [Fact]
        public void Name_of_context_with_DbContext_stripped_is_not_found_in_app_config()
        {
            using (var internalConnection = new LazyInternalConnection("Couger35.Hubcap.FullNameDbStrippedDbContext"))
            {
                var connection = internalConnection.Connection;

                Assert.Equal("Couger35.Hubcap.FullNameDbStrippedDbContext", connection.Database);
                Assert.IsType<SqlConnection>(connection);
            }
        }

        [Fact]
        public void Name_of_context_with_namespace_stripped_is_found_in_app_config()
        {
            using (var internalConnection = new LazyInternalConnection("Couger35.Hubcap.ShortNameDbContext"))
            {
                var connection = internalConnection.Connection;

                Assert.Equal("ShortNameInAppConfig", connection.Database);
                Assert.IsType<SqlConnection>(connection);
            }
        }

        [Fact]
        public void Name_of_context_with_namespace_and_Context_stripped_is_not_found_in_app_config()
        {
            using (var internalConnection = new LazyInternalConnection("Couger35.Hubcap.ShortNameStrippedContext"))
            {
                var connection = internalConnection.Connection;

                Assert.Equal("Couger35.Hubcap.ShortNameStrippedContext", connection.Database);
                Assert.IsType<SqlConnection>(connection);
            }
        }

        [Fact]
        public void Name_of_context_with_namespace_and_DbContext_stripped_is_found_in_app_config()
        {
            using (var internalConnection = new LazyInternalConnection("Couger35.Hubcap.ShortNameDbStrippedDbContext"))
            {
                var connection = internalConnection.Connection;

                Assert.Equal("Couger35.Hubcap.ShortNameDbStrippedDbContext", connection.Database);
                Assert.IsType<SqlConnection>(connection);
            }
        }

        [Fact]
        public void Connection_from_app_config_is_cached()
        {
            using (var internalConnection = new LazyInternalConnection("Couger35.Hubcap.FullNameDbContext"))
            {
                var connection = internalConnection.Connection;

                Assert.Same(connection, internalConnection.Connection);
                Assert.Same(connection, internalConnection.Connection);
            }
        }

        // See Dev11 bug 113586
        [Fact]
        public void Nice_exception_is_thrown_if_connection_string_in_app_config_does_not_contain_provider_name()
        {
            using (var internalConnection = new LazyInternalConnection("ConnectionWithoutProviderName"))
            {
                Assert.Equal(
                    Strings.DbContext_ProviderNameMissing("ConnectionWithoutProviderName"),
                    Assert.Throws<InvalidOperationException>(() => { var _ = internalConnection.Connection; }).Message);
            }
        }

        #endregion

        #region Positive hard-coded connection string tests

        [Fact]
        public void Connection_from_hard_coded_connection_string_is_cached()
        {
            using (var internalConnection = new LazyInternalConnection("Database=HardCodedConnectionString"))
            {
                var connection = internalConnection.Connection;

                Assert.Same(connection, internalConnection.Connection);
                Assert.Same(connection, internalConnection.Connection);
            }
        }

        [Fact]
        public void Default_connection_factory_is_used_to_create_connection_from_connection_string()
        {
            using (var internalConnection = new LazyInternalConnection("Database=HardCodedConnectionString"))
            {
                var connection = internalConnection.Connection;
                Assert.IsType<SqlConnection>(connection);
            }
        }

        [Fact]
        public void Changed_connection_factory_is_used_to_create_connection_from_connection_string()
        {
            try
            {
                var mockFactory = new Mock<IDbConnectionFactory>();
                mockFactory.Setup(f => f.CreateConnection("Database=HardCodedConnectionString")).Returns(new SqlConnection());

                MutableResolver.AddResolver<IDbConnectionFactory>(k => mockFactory.Object);
                using (var internalConnection = new LazyInternalConnection("Database=HardCodedConnectionString"))
                {
                    var _ = internalConnection.Connection;
                }

                mockFactory.Verify(f => f.CreateConnection("Database=HardCodedConnectionString"));
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }
        }

        #endregion

        #region Negative hard-coded connection string tests

        [Fact]
        public void Using_bad_connection_string_throws()
        {
            var expectedException = GenerateException(() => new SqlConnection("Step1=YouNeedToAsk?").Open());

            using (var internalConnection = new LazyInternalConnection("Step1=YouNeedToAsk?"))
            {
                Assert.Equal(
                    expectedException.Message, Assert.Throws<ArgumentException>(() => { var _ = internalConnection.Connection; }).Message);
            }
        }

        #endregion

        #region Positive IDbConnectionFactory tests

        [Fact]
        public void Default_IDbConnectionFactory_is_used_to_create_connection()
        {
            using (var internalConnection = new LazyInternalConnection("NameNotInAppConfig"))
            {
                var connection = internalConnection.Connection;

                Assert.Equal("NameNotInAppConfig", connection.Database);
                Assert.IsType<SqlConnection>(connection);
            }
        }

        [Fact]
        public void Changed_by_convention_IDbConnectionFactory_is_used_to_create_connection()
        {
            try
            {
                var mockFactory = new Mock<IDbConnectionFactory>();
                mockFactory.Setup(f => f.CreateConnection(It.IsAny<string>())).Returns(new SqlConnection());

                MutableResolver.AddResolver<IDbConnectionFactory>(k => mockFactory.Object);
                using (var internalConnection = new LazyInternalConnection("NameNotInAppConfig"))
                {
                    var connection = internalConnection.Connection;
                }

                mockFactory.Verify(f => f.CreateConnection("NameNotInAppConfig"));
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }
        }

        [Fact]
        public void Explicit_name_is_used_without_stripping_Context()
        {
            using (var internalConnection = new LazyInternalConnection("Couger35.Hubcap.NameForFactoryContext"))
            {
                var connection = internalConnection.Connection;

                Assert.Equal("Couger35.Hubcap.NameForFactoryContext", connection.Database);
                Assert.IsType<SqlConnection>(connection);
            }
        }

        [Fact]
        public void Explicit_name_is_used_without_stripping_DbContext()
        {
            using (var internalConnection = new LazyInternalConnection("Couger35.Hubcap.NameForFactoryDbContext"))
            {
                var connection = internalConnection.Connection;

                Assert.Equal("Couger35.Hubcap.NameForFactoryDbContext", connection.Database);
                Assert.IsType<SqlConnection>(connection);
            }
        }

        [Fact]
        public void By_convention_name_does_not_have_Context_stripped()
        {
            using (var internalConnection = new LazyInternalConnection("Couger35.Hubcap.NameForFactoryContext"))
            {
                var connection = internalConnection.Connection;

                Assert.Equal("Couger35.Hubcap.NameForFactoryContext", connection.Database);
                Assert.IsType<SqlConnection>(connection);
            }
        }

        [Fact]
        public void By_convention_name_does_not_have_DbContext_stripped()
        {
            using (var internalConnection = new LazyInternalConnection("Couger35.Hubcap.NameForFactoryDbContext"))
            {
                var connection = internalConnection.Connection;

                Assert.Equal("Couger35.Hubcap.NameForFactoryDbContext", connection.Database);
                Assert.IsType<SqlConnection>(connection);
            }
        }

        [Fact]
        public void Connection_from_factory_is_cached()
        {
            using (var internalConnection = new LazyInternalConnection("NameForFactory"))
            {
                var connection = internalConnection.Connection;

                Assert.Same(connection, internalConnection.Connection);
                Assert.Same(connection, internalConnection.Connection);
            }
        }

        #endregion

        #region Negative IDbConnectionFactory tests

        [Fact]
        public void Changed_default_connection_factory_that_results_in_null_connections_throws()
        {
            try
            {
                MutableResolver.AddResolver<IDbConnectionFactory>(k => new Mock<IDbConnectionFactory>().Object);
                using (var internalConnection = new LazyInternalConnection("NameNotInAppConfig"))
                {
                    Assert.Equal(
                        Strings.DbContext_ConnectionFactoryReturnedNullConnection,
                        Assert.Throws<InvalidOperationException>(() => { var _ = internalConnection.Connection; }).Message);
                }
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }
        }

        #endregion

        #region Positive connection dispose tests

        [Fact]
        public void Existing_connection_is_not_disposed_after_use()
        {
            var disposed = false;
            using (var connection = new SqlConnection())
            {
                connection.Disposed += (_, __) => disposed = true;

                using (var internalConnection = new EagerInternalConnection(new DbContext(connection, false)
                    , connection, connectionOwned: false))
                {
                    var _ = internalConnection.Connection;
                }

                Assert.False(disposed);
            }
            Assert.True(disposed);
        }

        [Fact]
        public void Existing_connection_is_disposed_after_use_if_it_is_owned_by_the_connection_object()
        {
            var disposed = false;
            var connection = new SqlConnection();

            connection.Disposed += (_, __) => disposed = true;

            using (var internalConnection = new EagerInternalConnection(new DbContext(connection, false),
                connection, connectionOwned: true))
            {
                var _ = internalConnection.Connection;
            }

            Assert.True(disposed);
        }

        [Fact]
        public void Connection_created_from_app_config_is_disposed_after_use()
        {
            var disposed = false;
            using (var internalConnection = new LazyInternalConnection("Couger35.Hubcap.FullNameDbContext"))
            {
                var connection = internalConnection.Connection;
                connection.Disposed += (_, __) => disposed = true;
            }
            Assert.True(disposed);
        }

        [Fact]
        public void Connection_created_from_hard_coded_connection_string_is_disposed_after_use()
        {
            var disposed = false;
            using (var internalConnection = new LazyInternalConnection("Database=HardCodedConnectionString"))
            {
                var connection = internalConnection.Connection;
                connection.Disposed += (_, __) => disposed = true;
            }
            Assert.True(disposed);
        }

        [Fact]
        public void Connection_created_from_factory_is_disposed_after_use()
        {
            var disposed = false;
            using (var internalConnection = new LazyInternalConnection("NameForFactory"))
            {
                var connection = internalConnection.Connection;
                connection.Disposed += (_, __) => disposed = true;
            }
            Assert.True(disposed);
        }

        [Fact]
        public void Disposed_LazyInternalConnection_can_be_reused()
        {
            var internalConnection = new LazyInternalConnection("NameForFactory");
            try
            {
                var disposed1 = 0;
                var disposed2 = 0;

                var connection1 = internalConnection.Connection;
                connection1.Disposed += (_, __) => disposed1++;

                internalConnection.Dispose();
                Assert.Equal(1, disposed1);
                Assert.Equal(0, disposed2);

                var connection2 = internalConnection.Connection;
                connection2.Disposed += (_, __) => disposed2++;

                internalConnection.Dispose();
                Assert.Equal(1, disposed1);
                Assert.Equal(1, disposed2);
            }
            finally
            {
                internalConnection.Dispose();
            }
        }

        [Fact]
        public void Disposed_EagerInternalConnection_created_with_existing_connection_can_be_reused()
        {
            var disposed = false;
            using (var connection = new SqlConnection())
            {
                connection.Disposed += (_, __) => disposed = true;

                var internalConnection = new EagerInternalConnection(new DbContext(connection, false),
                    connection, connectionOwned: false);
                try
                {
                    Assert.Same(connection, internalConnection.Connection);
                    internalConnection.Dispose();
                    Assert.False(disposed);

                    Assert.Same(connection, internalConnection.Connection);
                    internalConnection.Dispose();
                    Assert.False(disposed);
                }
                finally
                {
                    internalConnection.Dispose();
                }
            }
            Assert.True(disposed);
        }

        #endregion

        #region Lazy connection tests

        [Fact]
        public void LazyInternalConnection_can_create_connection_from_DbConnectionInfo_with_connection_string()
        {
            using (
                var connection =
                    new LazyInternalConnection(new DbContext("Database=DatabaseFromDbConnectionInfo"),
                        new DbConnectionInfo("Database=DatabaseFromDbConnectionInfo", "System.Data.SqlClient")))
            {
                Assert.IsType<SqlConnection>(connection.Connection);
                Assert.Equal("DatabaseFromDbConnectionInfo", connection.Connection.Database);
                Assert.Equal(null, connection.ConnectionStringName);
                Assert.Equal(DbConnectionStringOrigin.DbContextInfo, connection.ConnectionStringOrigin);
            }
        }

        [Fact]
        public void LazyInternalConnection_can_create_connection_from_DbConnectionInfo_from_config_file()
        {
            using (var connection = new LazyInternalConnection(new DbContext("LazyConnectionTest"), new DbConnectionInfo("LazyConnectionTest")))
            {
                Assert.IsType<SqlCeConnection>(connection.Connection);
                Assert.Equal("ConnectionFromAppConfig.sdf", connection.Connection.Database);
                Assert.Equal("LazyConnectionTest", connection.ConnectionStringName);
                Assert.Equal(DbConnectionStringOrigin.DbContextInfo, connection.ConnectionStringOrigin);
            }
        }

        [Fact]
        public void LazyInternalConnection_throws_when_cant_find_connection_from_DbConnectionInfo_in_config_file()
        {
            using (var connection = new LazyInternalConnection(new DbContext("YouWontFindMe"), new DbConnectionInfo("YouWontFindMe")))
            {
                Assert.Equal(
                    Strings.DbConnectionInfo_ConnectionStringNotFound("YouWontFindMe"),
                    Assert.Throws<InvalidOperationException>(() => { var temp = connection.Connection; }).Message);
            }
        }

        [Fact]
        public void LazyInternalConnection_can_create_connection_from_DbConnectionInfo_from_overridden_config_file()
        {
            using (var connection = new LazyInternalConnection(new DbContext("LazyConnectionTest"),
                new DbConnectionInfo("LazyConnectionTest")))
            {
                connection.AppConfig =
                    new AppConfig(
                        CreateEmptyConfig().AddConnectionString(
                            "LazyConnectionTest", "Database=FromOverridenConfig", "System.Data.SqlClient"));

                Assert.IsType<SqlConnection>(connection.Connection);
                Assert.Equal("FromOverridenConfig", connection.Connection.Database);
                Assert.Equal("LazyConnectionTest", connection.ConnectionStringName);
                Assert.Equal(DbConnectionStringOrigin.DbContextInfo, connection.ConnectionStringOrigin);
            }
        }

        [Fact]
        public void LazyInternalConnection_can_calculate_ConnectionHasModel_false_from_convention_without_initializing()
        {
            using (var connection = new LazyInternalConnection("MyName"))
            {
                Assert.False(connection.ConnectionHasModel);
                Assert.False(connection.IsInitialized);
            }
        }

        [Fact]
        public void LazyInternalConnection_can_calculate_ConnectionHasModel_false_from_config_file_without_initializing()
        {
            using (var connection = new LazyInternalConnection("name=LazyConnectionTest"))
            {
                Assert.False(connection.ConnectionHasModel);
                Assert.False(connection.IsInitialized);
            }
        }

        [Fact]
        public void LazyInternalConnection_can_calculate_ConnectionHasModel_true_from_config_file_without_initializing()
        {
            using (var connection = new LazyInternalConnection("name=EntityConnectionString"))
            {
                Assert.True(connection.ConnectionHasModel);
                Assert.False(connection.IsInitialized);
            }
        }

        [Fact]
        public void LazyInternalConnection_can_calculate_ConnectionHasModel_false_from_connection_string_without_initializing()
        {
            var efString = ConfigurationManager.ConnectionStrings["LazyConnectionTest"].ConnectionString;

            using (var connection = new LazyInternalConnection(efString))
            {
                Assert.False(connection.ConnectionHasModel);
                Assert.False(connection.IsInitialized);
            }
        }

        [Fact]
        public void LazyInternalConnection_can_calculate_ConnectionHasModel_true_from_connection_string_without_initializing()
        {
            var efString = ConfigurationManager.ConnectionStrings["EntityConnectionString"].ConnectionString;

            using (var connection = new LazyInternalConnection(efString))
            {
                Assert.True(connection.ConnectionHasModel);
                Assert.False(connection.IsInitialized);
            }
        }

        [Fact]
        public void LazyInternalConnection_can_calculate_ConnectionHasModel_false_from_DbConnectionInfo_without_initializing()
        {
            using (var connection = new LazyInternalConnection(new DbContext("LazyConnectionTest"),
                new DbConnectionInfo("LazyConnectionTest")))
            {
                Assert.False(connection.ConnectionHasModel);
                Assert.False(connection.IsInitialized);
            }
        }

        [Fact]
        public void LazyInternalConnection_can_calculate_ConnectionHasModel_true_from_DbConnectionInfo_without_initializing()
        {
            using (var connection = new LazyInternalConnection(new DbContext("EntityConnectionString"),
                new DbConnectionInfo("EntityConnectionString")))
            {
                Assert.True(connection.ConnectionHasModel);
                Assert.False(connection.IsInitialized);
            }
        }

        [Fact]
        public void LazyInternalConnection_can_calculate_ConnectionHasModel_false_after_initializing()
        {
            using (var connection = new LazyInternalConnection("name=LazyConnectionTest"))
            {
                var x = connection.Connection;
                Assert.True(connection.IsInitialized);

                Assert.False(connection.ConnectionHasModel);
            }
        }

        [Fact]
        public void LazyInternalConnection_can_calculate_ConnectionHasModel_true_after_initializing()
        {
            using (var connection = new LazyInternalConnection("name=EntityConnectionString"))
            {
                var x = connection.Connection;
                Assert.True(connection.IsInitialized);

                Assert.True(connection.ConnectionHasModel);
            }
        }

        [Fact]
        public void LazyInternalConnection_ConnectionHasModel_without_initializing_throws_when_connection_not_in_config()
        {
            using (var connection = new LazyInternalConnection("name=WontFindMe"))
            {
                Assert.Equal(
                    Strings.DbContext_ConnectionStringNotFound("WontFindMe"),
                    Assert.Throws<InvalidOperationException>(() => { var temp = connection.ConnectionHasModel; }).Message);
            }
        }

        [Fact]
        public void LazyInternalConnection_ConnectionHasModel_without_initializing_throws_when_connection_not_in_config_DbConnectionInfo()
        {
            using (var connection = new LazyInternalConnection(new DbContext("WontFindMe"), new DbConnectionInfo("WontFindMe")))
            {
                Assert.Equal(
                    Strings.DbConnectionInfo_ConnectionStringNotFound("WontFindMe"),
                    Assert.Throws<InvalidOperationException>(() => { var temp = connection.ConnectionHasModel; }).Message);
            }
        }

        [Fact]
        public void LazyInternalConnection_provider_name_calculated_when_connection_by_convention()
        {
            using (var connection = new LazyInternalConnection("ConnectionName"))
            {
                Assert.Equal("System.Data.SqlClient", connection.ProviderName);
            }
        }

        [Fact]
        public void LazyInternalConnection_provider_name_calculated_when_connection_from_config()
        {
            using (var connection = new LazyInternalConnection("name=LazyConnectionTest"))
            {
                Assert.Equal("System.Data.SqlServerCe.4.0", connection.ProviderName);
            }
        }

        [Fact]
        public void LazyInternalConnection_provider_name_calculated_when_connection_from_DbConnectionInfo()
        {
            using (
                var connection =
                    new LazyInternalConnection(
                        new DbContext("Data Source=ConnectionFromDbConnectionInfo.sdf"),
                        new DbConnectionInfo("Data Source=ConnectionFromDbConnectionInfo.sdf", "System.Data.SqlServerCe.4.0")))
            {
                Assert.Equal("System.Data.SqlServerCe.4.0", connection.ProviderName);
            }
        }

        #endregion
    }
}
