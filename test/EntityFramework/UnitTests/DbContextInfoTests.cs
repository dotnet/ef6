namespace ProductivityApiUnitTests
{
    using System;
    using System.Configuration;
    using System.Data.Entity.Core.Common;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Objects;
    using System.Data.SqlClient;
    using Moq;
    using Xunit;

    public class DbContextInfoTests : UnitTestBase
    {
        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal("contextType", Assert.Throws<ArgumentNullException>(() => new DbContextInfo((Type)null)).ParamName);

            Assert.Equal("contextType", Assert.Throws<ArgumentNullException>(() => new DbContextInfo((Type)null, new DbConnectionInfo("Name"))).ParamName);
            Assert.Equal("connectionInfo", Assert.Throws<ArgumentNullException>(() => new DbContextInfo(typeof(DbContext), (DbConnectionInfo)null)).ParamName);

            Assert.Equal("contextType", Assert.Throws<ArgumentNullException>(() => new DbContextInfo((Type)null, CreateEmptyConfig())).ParamName);
            Assert.Equal("config", Assert.Throws<ArgumentNullException>(() => new DbContextInfo(typeof(DbContext), (Configuration)null)).ParamName);

            Assert.Equal("contextType", Assert.Throws<ArgumentNullException>(() => new DbContextInfo((Type)null, CreateEmptyConfig(), new DbConnectionInfo("Name"))).ParamName);
            Assert.Equal("config", Assert.Throws<ArgumentNullException>(() => new DbContextInfo(typeof(DbContext), (Configuration)null, new DbConnectionInfo("Name"))).ParamName);
            Assert.Equal("connectionInfo", Assert.Throws<ArgumentNullException>(() => new DbContextInfo(typeof(DbContext), CreateEmptyConfig(), (DbConnectionInfo)null)).ParamName);

#pragma warning disable 618 // Obsolete ctor
            Assert.Equal("connectionStringSettings", Assert.Throws<ArgumentNullException>(() => new DbContextInfo(typeof(SimpleContext), (ConnectionStringSettingsCollection)null)).ParamName);
#pragma warning restore 618

            Assert.Equal(Error.ArgumentOutOfRange("contextType").Message, Assert.Throws<ArgumentOutOfRangeException>(() => new DbContextInfo(typeof(string))).Message);
        }

        [Fact]
        public void DbContextType_should_return_passed_context_type()
        {
            var contextInfo = new DbContextInfo(typeof(DbContext));

            Assert.Same(typeof(DbContext), contextInfo.ContextType);
        }
        
        [Fact]
        public void ContextType_should_return_tyoe_of_passed_context_instance()
        {
            var contextInfo = new DbContextInfo(new SimpleContext());

            Assert.Same(typeof(SimpleContext), contextInfo.ContextType);
        }
        
        public class SimpleContext : DbContext
        {
        }

        [Fact]
        public void CreateInstance_should_return_valid_instance_when_context_constructible()
        {
            var contextInfo = new DbContextInfo(typeof(SimpleContext));

            Assert.True(contextInfo.IsConstructible);
            Assert.Same(typeof(SimpleContext), contextInfo.CreateInstance().GetType());
        }

        [Fact]
        public void CreateInstance_should_return_null_when_context_not_constructible()
        {
            var contextInfo = new DbContextInfo(typeof(DbContext));

            Assert.False(contextInfo.IsConstructible);
            Assert.Null(contextInfo.CreateInstance());
        }

        [Fact]
        public void ConnectionString_and_ConnectionName_should_return_nulls_when_context_not_constructible()
        {
            var contextInfo = new DbContextInfo(typeof(DbContext));

            Assert.False(contextInfo.IsConstructible);
            Assert.Null(contextInfo.ConnectionString);
            Assert.Null(contextInfo.ConnectionStringName);
        }

        [Fact]
        public void ConnectionString_and_ConnectionName_should_return_values_when_context_constructible()
        {
            var contextInfo = new DbContextInfo(typeof(SimpleContext));

            Assert.True(!string.IsNullOrWhiteSpace(contextInfo.ConnectionString));
            Assert.Equal("ProductivityApiUnitTests.DbContextInfoTests+SimpleContext", contextInfo.ConnectionStringName);
        }

        [Fact]
        public void ConnectionProviderName_should_return_value_when_context_constructible()
        {
            var contextInfo = new DbContextInfo(typeof(SimpleContext));

            Assert.Equal("System.Data.SqlClient", contextInfo.ConnectionProviderName);
        }

        [Fact]
        public void ConnectionOrigin_should_return_by_convention_when_default_initialization()
        {
            var contextInfo = new DbContextInfo(typeof(SimpleContext));

            Assert.Equal(DbConnectionStringOrigin.Convention, contextInfo.ConnectionStringOrigin);
        }

        [Fact]
        public void DbContextInfo_should_get_connection_info_from_given_existing_context()
        {
            var mockContext = new Mock<InternalContextForMock<SimpleContext>>();
            mockContext.Setup(m => m.ConnectionStringOrigin).Returns(DbConnectionStringOrigin.UserCode);
            mockContext.Setup(m => m.ProviderName).Returns("My.Provider");
            mockContext.Setup(m => m.OriginalConnectionString).Returns("Databse=Foo");
            mockContext.Setup(m => m.ConnectionStringName).Returns("SomeName");

            var contextInfo = new DbContextInfo(mockContext.Object.Owner);

            Assert.Equal(DbConnectionStringOrigin.UserCode, contextInfo.ConnectionStringOrigin);
            Assert.Equal("Databse=Foo", contextInfo.ConnectionString);
            Assert.Equal("SomeName", contextInfo.ConnectionStringName);
            Assert.Equal("My.Provider", contextInfo.ConnectionProviderName);
        }

        public class ContextWithoutDefaultCtor : DbContext
        {
            private ContextWithoutDefaultCtor(string nameOrConnectionString)
                : base(nameOrConnectionString)
            {
            }

            public class ContextFactory : IDbContextFactory<ContextWithoutDefaultCtor>
            {
                public ContextWithoutDefaultCtor Create()
                {
                    return new ContextWithoutDefaultCtor("foo");
                }
            }
        }

        [Fact]
        public void CreateInstance_should_return_valid_instance_when_context_constructible_via_factory()
        {
            var contextInfo = new DbContextInfo(typeof(ContextWithoutDefaultCtor));

            Assert.True(contextInfo.IsConstructible);
            Assert.Same(typeof(ContextWithoutDefaultCtor), contextInfo.CreateInstance().GetType());
        }

        [Fact]
        public void ConnectionOrigin_should_return_by_convention_when_named_initialization()
        {
            var contextInfo = new DbContextInfo(typeof(ContextWithoutDefaultCtor));

            Assert.Equal(DbConnectionStringOrigin.Convention, contextInfo.ConnectionStringOrigin);
            Assert.Equal("foo", contextInfo.ConnectionStringName);
        }

        private class ContextWithConfiguredConnectionString : DbContext
        {
            public ContextWithConfiguredConnectionString()
                : base("ShortNameDbContext")
            {
            }
        }

        [Fact]
        public void ConnectionOrigin_should_return_configuration_when_connection_string_configured()
        {
            var contextInfo = new DbContextInfo(typeof(ContextWithConfiguredConnectionString));

            Assert.Equal(DbConnectionStringOrigin.Configuration, contextInfo.ConnectionStringOrigin);
            Assert.Equal("ShortNameDbContext", contextInfo.ConnectionStringName);
        }

        [Fact]
        public void Should_select_connection_string_from_supplied_candidates()
        {
            var connectionStringSettings
                = new ConnectionStringSettingsCollection
                      {
                          new ConnectionStringSettings("foo", "Initial Catalog=foo", "System.Data.SqlClient")
                      };

#pragma warning disable 618 // Obsolete ctor
            var contextInfo = new DbContextInfo(typeof(ContextWithoutDefaultCtor), connectionStringSettings);
#pragma warning restore 618

            Assert.Equal(DbConnectionStringOrigin.Configuration, contextInfo.ConnectionStringOrigin);
            Assert.Equal("Initial Catalog=foo", contextInfo.ConnectionString);
            Assert.Equal("foo", contextInfo.ConnectionStringName);
        }

        private class ContextWithConnectionString : DbContext
        {
            public ContextWithConnectionString()
                : base("Database=foo")
            {
            }
        }

        [Fact]
        public void ConnectionOrigin_should_return_user_code_when_connection_string_initialization()
        {
            var contextInfo = new DbContextInfo(typeof(ContextWithConnectionString));

            Assert.Equal(DbConnectionStringOrigin.UserCode, contextInfo.ConnectionStringOrigin);
            Assert.Null(contextInfo.ConnectionStringName);
        }

        private class ContextWithCompiledModel : DbContext
        {
            public ContextWithCompiledModel()
                : base(new DbModelBuilder().Build(ProviderRegistry.Sql2008_ProviderInfo).Compile())
            {
            }
        }

        [Fact]
        public void ConnectionOrigin_should_return_by_convention_when_compiled_model()
        {
            var contextInfo = new DbContextInfo(typeof(ContextWithCompiledModel));

            Assert.Equal(DbConnectionStringOrigin.Convention, contextInfo.ConnectionStringOrigin);
            Assert.Equal("ProductivityApiUnitTests.DbContextInfoTests+ContextWithCompiledModel", contextInfo.ConnectionStringName);
        }

        private class ContextWithExistingConnection : DbContext
        {
            public ContextWithExistingConnection()
                : base(new SqlConnection(), true)
            {
            }
        }

        [Fact]
        public void ConnectionOrigin_should_return_user_code_when_existing_connection()
        {
            var contextInfo = new DbContextInfo(typeof(ContextWithExistingConnection));

            Assert.Equal(DbConnectionStringOrigin.UserCode, contextInfo.ConnectionStringOrigin);
            Assert.Null(contextInfo.ConnectionStringName);
        }

        private class ContextWithExistingConnectionAndCompiledModel : DbContext
        {
            public ContextWithExistingConnectionAndCompiledModel()
                : base(new SqlConnection(), new DbModelBuilder().Build(ProviderRegistry.Sql2008_ProviderInfo).Compile(), true)
            {
            }
        }

        [Fact]
        public void ConnectionOrigin_should_return_user_code_when_existing_connection_and_compiled_model()
        {
            var contextInfo = new DbContextInfo(typeof(ContextWithExistingConnectionAndCompiledModel));

            Assert.Equal(DbConnectionStringOrigin.UserCode, contextInfo.ConnectionStringOrigin);
            Assert.Null(contextInfo.ConnectionStringName);
        }

        private class ContextWithExistingObjectContext : DbContext
        {
            public ContextWithExistingObjectContext()
                : base(new ObjectContext(
                           new EntityConnection(
                               new DbModelBuilder().Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping.ToMetadataWorkspace(),
                               new SqlConnection())), true)
            {
            }
        }

        [Fact]
        public void ConnectionOrigin_should_return_user_code_when_existing_object_context()
        {
            var contextInfo = new DbContextInfo(typeof(ContextWithExistingObjectContext));

            Assert.Equal(DbConnectionStringOrigin.UserCode, contextInfo.ConnectionStringOrigin);
            Assert.Null(contextInfo.ConnectionStringName);
        }

        private class ContextWithoutDefaultCtorBadFactory : DbContext
        {
            private ContextWithoutDefaultCtorBadFactory(string nameOrConnectionString)
                : base(nameOrConnectionString)
            {
            }

            public class ContextFactory : IDbContextFactory<ContextWithoutDefaultCtorBadFactory>
            {
                private ContextFactory()
                {
                }

                public ContextWithoutDefaultCtorBadFactory Create()
                {
                    return new ContextWithoutDefaultCtorBadFactory("foo");
                }
            }
        }

        [Fact]
        public void CreateActivator_should_throw_when_context_factory_not_constructible()
        {
            Assert.Equal(Strings.DbContextServices_MissingDefaultCtor(typeof(ContextWithoutDefaultCtorBadFactory.ContextFactory)), Assert.Throws<InvalidOperationException>(() => new DbContextInfo(typeof(ContextWithoutDefaultCtorBadFactory))).Message);
        }

        [Fact(Skip = "No CE Provider")]
        public void CreateInstance_should_use_passed_provider_info_when_building_model()
        {
            var contextInfo = new DbContextInfo(typeof(SimpleContext), ProviderRegistry.SqlCe4_ProviderInfo);

            Assert.Equal(ProviderRegistry.SqlCe4_ProviderInfo.ProviderInvariantName, contextInfo.ConnectionProviderName);
            Assert.Equal(string.Empty, contextInfo.ConnectionString);

            Database.SetInitializer<SimpleContext>(null);

            var objectContext = ((IObjectContextAdapter)contextInfo.CreateInstance()).ObjectContext;

            Assert.NotNull(objectContext);
            Assert.Equal("SqlCeConnection", ((EntityConnection)objectContext.Connection).StoreConnection.GetType().Name);
        }
        
        private class ContextWithExternalOnModelCreating1 : DbContext
        {
        }

        [Fact]
        public void CreateInstance_should_attach_on_model_creating_custom_action_and_invoke_once()
        {
            var calledCount = 0;

            var contextInfo = new DbContextInfo(typeof(ContextWithExternalOnModelCreating1));
            contextInfo.OnModelCreating = _ => calledCount++;

            contextInfo.CreateInstance();

            Assert.Equal(0, calledCount);

            var objectContext = ((IObjectContextAdapter)contextInfo.CreateInstance()).ObjectContext;

            Assert.NotNull(objectContext);
            Assert.Equal(1, calledCount);

            objectContext = ((IObjectContextAdapter)contextInfo.CreateInstance()).ObjectContext;

            Assert.NotNull(objectContext);
            Assert.Equal(1, calledCount);
        }

        private class ContextWithExternalOnModelCreating2 : DbContext
        {
        }

        [Fact]
        public void Can_use_custom_on_model_creating_action_to_configure_model_builder()
        {
            var contextInfo = new DbContextInfo(typeof(ContextWithExternalOnModelCreating2));

            var objectContext = ((IObjectContextAdapter)contextInfo.CreateInstance()).ObjectContext;

            Assert.NotNull(objectContext);
            Assert.False(objectContext.CreateDatabaseScript().Contains("EdmMetadata"));
        }

        private class ContextWithExternalOnModelCreating3 : DbContext
        {
            static ContextWithExternalOnModelCreating3()
            {
                Database.SetInitializer<ContextWithExternalOnModelCreating3>(null);
            }

            public DbSet<FakeEntity> Fakes { get; set; }
        }

        [Fact]
        public void Can_unset_custom_on_model_creating_action()
        {
            var contextInfo
                = new DbContextInfo(
                    typeof(ContextWithExternalOnModelCreating3))
                {
                    OnModelCreating = mb => mb.Ignore<FakeEntity>()
                };

            contextInfo.OnModelCreating = null;

            var objectContext = ((IObjectContextAdapter)contextInfo.CreateInstance()).ObjectContext;

            Assert.NotNull(objectContext);
            Assert.True(objectContext.CreateDatabaseScript().Contains("FakeEntities"));
        }

        [Fact]
        public void Should_use_DefaultConnectionFactory_from_supplied_config()
        {
            RunTestWithConnectionFactory(Database.ResetDefaultConnectionFactory, () =>
            {
                var config = CreateEmptyConfig().AddDefaultConnectionFactory(
                    "ProductivityApiUnitTests.FakeDbContextInfoConnectionFactory, EntityFramework.UnitTests",
                    new string[0]);

                var contextInfo = new DbContextInfo(typeof(ContextWithoutDefaultCtor), config);

                Assert.Equal(DbConnectionStringOrigin.Convention, contextInfo.ConnectionStringOrigin);
                Assert.Equal("Database=foo", contextInfo.ConnectionString);
                Assert.Equal("System.Data.SqlClient", contextInfo.ConnectionProviderName);
            });
        }

        [Fact]
        public void Should_use_use_default_DefaultConnectionFactory_if_supplied_config_contains_no_DefaultConnectionFactory()
        {
            RunTestWithConnectionFactory(Database.ResetDefaultConnectionFactory, () =>
            {
                var config = CreateEmptyConfig();

                var contextInfo = new DbContextInfo(typeof(ContextWithoutDefaultCtor), config);

                Assert.Equal(DbConnectionStringOrigin.Convention, contextInfo.ConnectionStringOrigin);
                Assert.True(contextInfo.ConnectionString.Contains(@"Data Source=.\SQLEXPRESS"));
                Assert.True(contextInfo.ConnectionString.Contains(@"Initial Catalog=foo"));
                Assert.Equal("System.Data.SqlClient", contextInfo.ConnectionProviderName);
            });
        }

        [Fact]
        public void Should_use_connectioin_string_from_supplied_config_even_if_DefaultConnectionFactory_is_also_present()
        {
            RunTestWithConnectionFactory(Database.ResetDefaultConnectionFactory, () =>
            {
                var config = AddConnectionStrings(CreateEmptyConfig().AddDefaultConnectionFactory(
                    "ProductivityApiUnitTests.FakeDbContextInfoConnectionFactory, EntityFramework.UnitTests",
                    new string[0]));

                var contextInfo = new DbContextInfo(typeof(ContextWithoutDefaultCtor), config);

                Assert.Equal(DbConnectionStringOrigin.Configuration, contextInfo.ConnectionStringOrigin);
                Assert.Equal("Initial Catalog=foo", contextInfo.ConnectionString);
                Assert.Equal("foo", contextInfo.ConnectionStringName);
                Assert.Equal("System.Data.SqlClient", contextInfo.ConnectionProviderName);
            });
        }

        [Fact]
        public void Should_use_DefaultConnectionFactory_set_in_code_even_if_one_was_supplied_in_config()
        {
            RunTestWithConnectionFactory(() => Database.DefaultConnectionFactory = new SqlConnectionFactory(), () =>
            {
                var config = CreateEmptyConfig().AddDefaultConnectionFactory(
                    "ProductivityApiUnitTests.FakeDbContextInfoConnectionFactory, EntityFramework.UnitTests",
                    new string[0]);

                var contextInfo = new DbContextInfo(typeof(ContextWithoutDefaultCtor), config);

                Assert.Equal(DbConnectionStringOrigin.Convention, contextInfo.ConnectionStringOrigin);
                Assert.True(contextInfo.ConnectionString.Contains(@"Data Source=.\SQLEXPRESS"));
                Assert.True(contextInfo.ConnectionString.Contains(@"Initial Catalog=foo"));
                Assert.Equal("System.Data.SqlClient", contextInfo.ConnectionProviderName);
            });
        }

        [Fact]
        public void Setting_DefaultConnectionFactory_from_code_marks_DefaultConnectionFactory_as_changed_and_this_can_be_reset()
        {
            RunTestWithConnectionFactory(Database.ResetDefaultConnectionFactory, () =>
            {
                Assert.False(Database.DefaultConnectionFactoryChanged);

                Database.DefaultConnectionFactory = new SqlConnectionFactory();
                Assert.True(Database.DefaultConnectionFactoryChanged);

                Database.ResetDefaultConnectionFactory();
                Assert.False(Database.DefaultConnectionFactoryChanged);
            });
        }

        private void RunTestWithConnectionFactory(Action connectionFactorySetter, Action test)
        {
            var currentConnectionFactory = Database.DefaultConnectionFactory;
            connectionFactorySetter();
            try
            {
                test();
            }
            finally
            {
                Database.DefaultConnectionFactory = currentConnectionFactory;
            }
        }

        private Configuration AddConnectionStrings(Configuration config)
        {
            config.ConnectionStrings.ConnectionStrings.Add(
                new ConnectionStringSettings("foo", "Initial Catalog=foo", "System.Data.SqlClient"));

            config.ConnectionStrings.ConnectionStrings.Add(
                new ConnectionStringSettings("bar", "Initial Catalog=bar", "SomeProvider"));

            return config;
        }

        [Fact]
        public void Can_set_hard_coded_connection()
        {
            var connection = new DbConnectionInfo("Database=UseThisDatabaseInstead", "System.Data.SqlClient");
            var contextInfo = new DbContextInfo(typeof(SimpleContext), connection);

            Assert.Equal(DbConnectionStringOrigin.DbContextInfo, contextInfo.ConnectionStringOrigin);
            Assert.Equal("System.Data.SqlClient", contextInfo.ConnectionProviderName);
            Assert.Equal(null, contextInfo.ConnectionStringName);
            Assert.True(contextInfo.IsConstructible);

            using (var context = contextInfo.CreateInstance())
            {
                Assert.Equal("UseThisDatabaseInstead", context.Database.Connection.Database);
            }
        }

        [Fact]
        public void Can_set_hard_coded_connection_from_default_config()
        {
            var connection = new DbConnectionInfo("OverrideConnectionTest");
            var contextInfo = new DbContextInfo(typeof(SimpleContext), connection);

            Assert.Equal(DbConnectionStringOrigin.DbContextInfo, contextInfo.ConnectionStringOrigin);
            Assert.Equal("System.Data.SqlClient", contextInfo.ConnectionProviderName);
            Assert.Equal("OverrideConnectionTest", contextInfo.ConnectionStringName);
            Assert.True(contextInfo.IsConstructible);

            using (var context = contextInfo.CreateInstance())
            {
                Assert.Equal("ConnectionFromAppConfig", context.Database.Connection.Database);
            }
        }

        [Fact]
        public void Can_set_hard_coded_connection_from_supplied_config()
        {
            var connection = new DbConnectionInfo("GetMeFromSuppliedConfig");
            var contextInfo = new DbContextInfo(
                typeof(SimpleContext),
                CreateEmptyConfig().AddConnectionString("GetMeFromSuppliedConfig", "Database=ConnectionFromSuppliedConfig", "System.Data.SqlClient"),
                connection);

            Assert.Equal(DbConnectionStringOrigin.DbContextInfo, contextInfo.ConnectionStringOrigin);
            Assert.Equal("System.Data.SqlClient", contextInfo.ConnectionProviderName);
            Assert.Equal("GetMeFromSuppliedConfig", contextInfo.ConnectionStringName);
            Assert.True(contextInfo.IsConstructible);

            using (var context = contextInfo.CreateInstance())
            {
                Assert.Equal("ConnectionFromSuppliedConfig", context.Database.Connection.Database);
            }
        }

        [Fact]
        public void Supplied_config_used_to_load_original_and_overriden_connection()
        {
            var connection = new DbConnectionInfo("GetMeFromSuppliedConfig");
            var contextInfo = new DbContextInfo(
                typeof(ContextWithConnectionNameNotInAppConfigFile),
                CreateEmptyConfig()
                    .AddConnectionString("GetMeFromSuppliedConfig", "Database=ConnectionFromSuppliedConfig", "System.Data.SqlClient")
                    .AddConnectionString("WontFindMeInDefaultConfig", "Database=WontFindMeInDefaultConfig", "System.Data.SqlClient"),
                connection);

            using (var context = contextInfo.CreateInstance())
            {
                Assert.Equal("ConnectionFromSuppliedConfig", context.Database.Connection.Database);
            }
        }

        [Fact]
        public void Exceptions_applying_new_connection_surfaced()
        {
            var connection = new DbConnectionInfo("GetMeFromSuppliedConfig");

            Assert.Equal(Strings.DbContext_ConnectionStringNotFound("GetMeFromSuppliedConfig"), Assert.Throws<InvalidOperationException>(() => new DbContextInfo(typeof(ContextWithConnectionNameNotInAppConfigFile), CreateEmptyConfig(), connection)).Message);
        }

        public class ContextWithConnectionNameNotInAppConfigFile : DbContext
        {
            public ContextWithConnectionNameNotInAppConfigFile()
                : base("name=WontFindMeInDefaultConfig")
            {
            }
        }
    }

    public class FakeDbContextInfoConnectionFactory : IDbConnectionFactory
    {
        public DbConnection CreateConnection(string nameOrConnectionString)
        {
            return new SqlConnection("Database=" + nameOrConnectionString);
        }
    }
}