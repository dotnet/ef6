// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Configuration;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Internal;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.TestHelpers;
    using System.Data.SqlClient;
    using System.Linq;
    using Moq;
    using Xunit;

    public class DbContextInfoTests : TestBase
    {
        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal("contextType", Assert.Throws<ArgumentNullException>(() => new DbContextInfo((Type)null)).ParamName);

            Assert.Equal(
                "contextType", Assert.Throws<ArgumentNullException>(() => new DbContextInfo(null, new DbConnectionInfo("Name"))).ParamName);
            Assert.Equal(
                "connectionInfo",
                Assert.Throws<ArgumentNullException>(() => new DbContextInfo(typeof(DbContext), (DbConnectionInfo)null)).ParamName);

            Assert.Equal("contextType", Assert.Throws<ArgumentNullException>(() => new DbContextInfo(null, CreateEmptyConfig())).ParamName);
            Assert.Equal(
                "config", Assert.Throws<ArgumentNullException>(() => new DbContextInfo(typeof(DbContext), (Configuration)null)).ParamName);

            Assert.Equal(
                "contextType",
                Assert.Throws<ArgumentNullException>(() => new DbContextInfo(null, CreateEmptyConfig(), new DbConnectionInfo("Name"))).
                    ParamName);
            Assert.Equal(
                "config",
                Assert.Throws<ArgumentNullException>(() => new DbContextInfo(typeof(DbContext), null, new DbConnectionInfo("Name"))).
                    ParamName);
            Assert.Equal(
                "connectionInfo",
                Assert.Throws<ArgumentNullException>(
                    () => new DbContextInfo(typeof(DbContext), CreateEmptyConfig(), (DbConnectionInfo)null)).ParamName);

#pragma warning disable 618 // Obsolete ctor
            Assert.Equal(
                "connectionStringSettings",
                Assert.Throws<ArgumentNullException>(
                    () => new DbContextInfo(typeof(SimpleContext), (ConnectionStringSettingsCollection)null)).ParamName);
#pragma warning restore 618

            Assert.Equal(
                Error.ArgumentOutOfRange("contextType").Message,
                Assert.Throws<ArgumentOutOfRangeException>(() => new DbContextInfo(typeof(string))).Message);

            Assert.Equal(
                "contextType",
                Assert.Throws<ArgumentNullException>(() => new DbContextInfo(null, ProviderRegistry.SqlCe4_ProviderInfo)).ParamName);
            Assert.Equal(
                "modelProviderInfo",
                Assert.Throws<ArgumentNullException>(() => new DbContextInfo(typeof(DbContext), (DbProviderInfo)null)).ParamName);

            Assert.Equal(
                "contextType",
                Assert.Throws<ArgumentNullException>(
                    () => new DbContextInfo(null, CreateEmptyConfig(), ProviderRegistry.SqlCe4_ProviderInfo)).ParamName);
            Assert.Equal(
                "config",
                Assert.Throws<ArgumentNullException>(() => new DbContextInfo(typeof(DbContext), null, ProviderRegistry.SqlCe4_ProviderInfo))
                    .ParamName);
            Assert.Equal(
                "modelProviderInfo",
                Assert.Throws<ArgumentNullException>(() => new DbContextInfo(typeof(DbContext), CreateEmptyConfig(), (DbProviderInfo)null)).
                    ParamName);
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

        [DbConfigurationType(typeof(FunctionalTestsConfiguration))]
        public class SimpleContext : DbContext
        {
        }

        [DbConfigurationType(typeof(FunctionalTestsConfiguration))]
        public class SimpleContextWithInit : DbContext
        {
            public SimpleContextWithInit()
            {
                var _ = ((IObjectContextAdapter)this).ObjectContext;
            }
        }

        [Fact]
        public void CreateInstance_should_return_valid_instance_when_context_constructible()
        {
            var contextInfo = new DbContextInfo(typeof(SimpleContext));

            Assert.True(contextInfo.IsConstructible);
            using (var context = contextInfo.CreateInstance())
            {
                Assert.Same(typeof(SimpleContext), context.GetType());
            }
        }

        [Fact]
        public void CreateInstance_should_return_null_when_context_not_constructible_and_unregister_mapping()
        {
            var contextInfo = new DbContextInfo(typeof(DbContext));

            Assert.False(contextInfo.IsConstructible);
            Assert.Null(contextInfo.CreateInstance());
            Assert.Null(DbContextInfo.TryGetInfoForContext(typeof(DbContext)));
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
        public void ConnectionString_and_ConnectionName_should_return_values_when_context_constructible_normal_constructor()
        {
            ConnectionString_and_ConnectionName_should_return_values_when_context_constructible(typeof(SimpleContext));
        }

        [Fact]
        public void ConnectionString_and_ConnectionName_should_return_values_when_context_constructible_init_constructor()
        {
            ConnectionString_and_ConnectionName_should_return_values_when_context_constructible(typeof(SimpleContextWithInit));
        }

        private void ConnectionString_and_ConnectionName_should_return_values_when_context_constructible(Type contextType)
        {
            var contextInfo = new DbContextInfo(contextType);

            Assert.True(!string.IsNullOrWhiteSpace(contextInfo.ConnectionString));
            Assert.Equal(contextType.FullName, contextInfo.ConnectionStringName);
        }

        [Fact]
        public void ConnectionProviderName_should_return_value_when_context_constructible_normal_constructor()
        {
            ConnectionProviderName_should_return_value_when_context_constructible(typeof(SimpleContextWithInit));
        }

        [Fact]
        public void ConnectionProviderName_should_return_value_when_context_constructible_init_constructor()
        {
            ConnectionProviderName_should_return_value_when_context_constructible(typeof(SimpleContext));
        }

        private void ConnectionProviderName_should_return_value_when_context_constructible(Type contextType)
        {
            var contextInfo = new DbContextInfo(contextType);

            Assert.Equal("System.Data.SqlClient", contextInfo.ConnectionProviderName);
        }

        [Fact]
        public void ConnectionOrigin_should_return_by_convention_when_default_initialization_normal_constructor()
        {
            ConnectionOrigin_should_return_by_convention_when_default_initialization(typeof(SimpleContext));
        }

        [Fact]
        public void ConnectionOrigin_should_return_by_convention_when_default_initialization_init_constructor()
        {
            ConnectionOrigin_should_return_by_convention_when_default_initialization(typeof(SimpleContextWithInit));
        }

        private void ConnectionOrigin_should_return_by_convention_when_default_initialization(Type contextType)
        {
            var contextInfo = new DbContextInfo(contextType);

            Assert.Equal(DbConnectionStringOrigin.Convention, contextInfo.ConnectionStringOrigin);
        }

        [Fact]
        public void Constructor_should_use_interception()
        {
            var dbConnectionInterceptorMock = new Mock<IDbConnectionInterceptor>();
            DbInterception.Add(dbConnectionInterceptorMock.Object);
            try
            {
                new DbContextInfo(typeof(SimpleContext));
            }
            finally
            {
                DbInterception.Remove(dbConnectionInterceptorMock.Object);
            }

            dbConnectionInterceptorMock.Verify(
                m => m.ConnectionStringGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                Times.Exactly(2));
            dbConnectionInterceptorMock.Verify(
                m => m.ConnectionStringGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                Times.Exactly(2));

            dbConnectionInterceptorMock.Verify(
                m => m.ConnectionStringSetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionPropertyInterceptionContext<string>>()),
                Times.Once());
            dbConnectionInterceptorMock.Verify(
                m => m.ConnectionStringSet(It.IsAny<DbConnection>(), It.IsAny<DbConnectionPropertyInterceptionContext<string>>()),
                Times.Once());

            dbConnectionInterceptorMock.Verify(
                m => m.DataSourceGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                Times.Once());
            dbConnectionInterceptorMock.Verify(
                m => m.DataSourceGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                Times.Once());

            dbConnectionInterceptorMock.Verify(
                m => m.DatabaseGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                Times.Once());
            dbConnectionInterceptorMock.Verify(
                m => m.DatabaseGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                Times.Once());
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

        [DbConfigurationType(typeof(FunctionalTestsConfiguration))]
        public class ContextWithoutDefaultCtor : DbContext
        {
            public string NameOrConnectionString { get; private set; }

            internal ContextWithoutDefaultCtor(string nameOrConnectionString)
                : base(nameOrConnectionString)
            {
                NameOrConnectionString = nameOrConnectionString;
            }

            public class ContextFactory : IDbContextFactory<ContextWithoutDefaultCtor>
            {
                public ContextWithoutDefaultCtor Create()
                {
                    return new ContextWithoutDefaultCtor("foo");
                }
            }
        }

        [DbConfigurationType(typeof(FunctionalTestsConfiguration))]
        public class ContextWithoutDefaultCtorWithInit : DbContext
        {
            private ContextWithoutDefaultCtorWithInit(string nameOrConnectionString)
                : base(nameOrConnectionString)
            {
                var _ = ((IObjectContextAdapter)this).ObjectContext;
            }

            public class ContextFactory : IDbContextFactory<ContextWithoutDefaultCtorWithInit>
            {
                public ContextWithoutDefaultCtorWithInit Create()
                {
                    return new ContextWithoutDefaultCtorWithInit("foo");
                }
            }
        }

        [Fact]
        public void CreateInstance_should_return_valid_instance_when_context_constructible_via_factory()
        {
            var contextInfo = new DbContextInfo(typeof(ContextWithoutDefaultCtor));

            Assert.True(contextInfo.IsConstructible);
            using (var context = contextInfo.CreateInstance())
            {
                Assert.IsType<ContextWithoutDefaultCtor>(context);
                Assert.Equal("foo", ((ContextWithoutDefaultCtor)context).NameOrConnectionString);
            }
        }

        [Fact]
        public void CreateInstance_should_use_resolver_when_both_context_factory_resolver_and_factory_class_exist()
        {
            var resolver = new SingletonDependencyResolver<Func<DbContext>>(
                () => new ContextWithoutDefaultCtor("bar"), typeof(ContextWithoutDefaultCtor));

            var contextInfo = new DbContextInfo(typeof(ContextWithoutDefaultCtor), () => resolver);

            Assert.True(contextInfo.IsConstructible);
            using (var context = contextInfo.CreateInstance())
            {
                Assert.IsType<ContextWithoutDefaultCtor>(context);
                Assert.Equal("bar", ((ContextWithoutDefaultCtor)context).NameOrConnectionString);
            }
        }

        [Fact]
        public void CreateInstance_should_use_resolver_when_no_factory_class_exists()
        {
            var resolver = new SingletonDependencyResolver<Func<DbContext>>(
                () => new ContextForResolverTest("bar"), typeof(ContextForResolverTest));

            var contextInfo = new DbContextInfo(typeof(ContextForResolverTest), () => resolver);

            Assert.True(contextInfo.IsConstructible);
            using (var context = contextInfo.CreateInstance())
            {
                Assert.IsType<ContextForResolverTest>(context);
            }
        }

        [DbConfigurationType(typeof(FunctionalTestsConfiguration))]
        public class ContextForResolverTest : DbContext
        {
            internal ContextForResolverTest(string nameOrConnectionString)
                : base(nameOrConnectionString)
            {
            }
        }

        [Fact]
        public void ConnectionOrigin_should_return_by_convention_when_named_initialization_normal_constructor()
        {
            ConnectionOrigin_should_return_by_convention_when_named_initialization(typeof(ContextWithoutDefaultCtor));
        }

        [Fact]
        public void ConnectionOrigin_should_return_by_convention_when_named_initialization_init_constructor()
        {
            ConnectionOrigin_should_return_by_convention_when_named_initialization(typeof(ContextWithoutDefaultCtorWithInit));
        }

        private void ConnectionOrigin_should_return_by_convention_when_named_initialization(Type contextType)
        {
            var contextInfo = new DbContextInfo(contextType);

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

        private class ContextWithConfiguredConnectionStringWithInit : DbContext
        {
            public ContextWithConfiguredConnectionStringWithInit()
                : base("ShortNameDbContext")
            {
                var _ = ((IObjectContextAdapter)this).ObjectContext;
            }
        }

        [Fact]
        public void ConnectionOrigin_should_return_configuration_when_connection_string_configured_normal_constructor()
        {
            ConnectionOrigin_should_return_configuration_when_connection_string_configured(typeof(ContextWithConfiguredConnectionString));
        }

        [Fact]
        public void ConnectionOrigin_should_return_configuration_when_connection_string_configured_init_constructor()
        {
            ConnectionOrigin_should_return_configuration_when_connection_string_configured(
                typeof(ContextWithConfiguredConnectionStringWithInit));
        }

        private void ConnectionOrigin_should_return_configuration_when_connection_string_configured(Type contextType)
        {
            var contextInfo = new DbContextInfo(contextType);

            Assert.Equal(DbConnectionStringOrigin.Configuration, contextInfo.ConnectionStringOrigin);
            Assert.Equal("ShortNameDbContext", contextInfo.ConnectionStringName);
        }

        [Fact]
        public void Should_select_connection_string_from_supplied_candidates_normal_constructor()
        {
            Should_select_connection_string_from_supplied_candidates(typeof(ContextWithoutDefaultCtor));
        }

        [Fact]
        public void Should_select_connection_string_from_supplied_candidates_init_constructor()
        {
            Should_select_connection_string_from_supplied_candidates(typeof(ContextWithoutDefaultCtorWithInit));
        }

        private void Should_select_connection_string_from_supplied_candidates(Type contextType)
        {
            var connectionStringSettings
                = new ConnectionStringSettingsCollection
                {
                    new ConnectionStringSettings("foo", "Initial Catalog=foo", "System.Data.SqlClient")
                };

#pragma warning disable 618 // Obsolete ctor
            var contextInfo = new DbContextInfo(contextType, connectionStringSettings);
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

        private class ContextWithConnectionStringWithInit : DbContext
        {
            public ContextWithConnectionStringWithInit()
                : base("Database=foo")
            {
                var _ = ((IObjectContextAdapter)this).ObjectContext;
            }
        }

        [Fact]
        public void ConnectionOrigin_should_return_user_code_when_connection_string_initialization_normal_constructor()
        {
            ConnectionOrigin_should_return_user_code_when_connection_string_initialization(typeof(ContextWithConnectionString));
        }

        [Fact]
        public void ConnectionOrigin_should_return_user_code_when_connection_string_initialization_init_constructor()
        {
            ConnectionOrigin_should_return_user_code_when_connection_string_initialization(typeof(ContextWithConnectionStringWithInit));
        }

        private void ConnectionOrigin_should_return_user_code_when_connection_string_initialization(Type contextType)
        {
            var contextInfo = new DbContextInfo(contextType);

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

        private class ContextWithCompiledModelWithInit : DbContext
        {
            public ContextWithCompiledModelWithInit()
                : base(new DbModelBuilder().Build(ProviderRegistry.Sql2008_ProviderInfo).Compile())
            {
                var _ = ((IObjectContextAdapter)this).ObjectContext;
            }
        }

        [Fact]
        public void ConnectionOrigin_should_return_by_convention_when_compiled_model_normal_constructor()
        {
            ConnectionOrigin_should_return_by_convention_when_compiled_model(typeof(ContextWithCompiledModel));
        }

        [Fact]
        public void ConnectionOrigin_should_return_by_convention_when_compiled_model_init_constructor()
        {
            ConnectionOrigin_should_return_by_convention_when_compiled_model(typeof(ContextWithCompiledModelWithInit));
        }

        private void ConnectionOrigin_should_return_by_convention_when_compiled_model(Type contextType)
        {
            var contextInfo = new DbContextInfo(contextType);

            Assert.Equal(DbConnectionStringOrigin.Convention, contextInfo.ConnectionStringOrigin);
            Assert.Equal(contextType.FullName, contextInfo.ConnectionStringName);
        }

        private class ContextWithExistingConnection : DbContext
        {
            public ContextWithExistingConnection()
                : base(new SqlConnection("Database=Foo"), true)
            {
            }
        }

        private class ContextWithExistingConnectionWithInit : DbContext
        {
            public ContextWithExistingConnectionWithInit()
                : base(new SqlConnection("Database=Foo"), true)
            {
                var _ = ((IObjectContextAdapter)this).ObjectContext;
            }
        }

        [Fact]
        public void ConnectionOrigin_should_return_user_code_when_existing_connection_normal_constructor()
        {
            ConnectionOrigin_should_return_user_code_when_existing_connection(typeof(ContextWithExistingConnection));
        }

        [Fact]
        public void ConnectionOrigin_should_return_user_code_when_existing_connection_init_constructor()
        {
            ConnectionOrigin_should_return_user_code_when_existing_connection(typeof(ContextWithExistingConnectionWithInit));
        }

        private void ConnectionOrigin_should_return_user_code_when_existing_connection(Type contextType)
        {
            var contextInfo = new DbContextInfo(contextType);

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

        private class ContextWithExistingConnectionAndCompiledModelWithInit : DbContext
        {
            public ContextWithExistingConnectionAndCompiledModelWithInit()
                : base(new SqlConnection(), new DbModelBuilder().Build(ProviderRegistry.Sql2008_ProviderInfo).Compile(), true)
            {
                var _ = ((IObjectContextAdapter)this).ObjectContext;
            }
        }

        [Fact]
        public void ConnectionOrigin_should_return_user_code_when_existing_connection_and_compiled_model_normal_constructor()
        {
            ConnectionOrigin_should_return_user_code_when_existing_connection_and_compiled_model(
                typeof(ContextWithExistingConnectionAndCompiledModel));
        }

        [Fact]
        public void ConnectionOrigin_should_return_user_code_when_existing_connection_and_compiled_model_init_constructor()
        {
            ConnectionOrigin_should_return_user_code_when_existing_connection_and_compiled_model(
                typeof(ContextWithExistingConnectionAndCompiledModelWithInit));
        }

        private void ConnectionOrigin_should_return_user_code_when_existing_connection_and_compiled_model(Type contextType)
        {
            var contextInfo = new DbContextInfo(contextType);

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

        private class ContextWithExistingObjectContextWithInit : DbContext
        {
            public ContextWithExistingObjectContextWithInit()
                : base(new ObjectContext(
                    new EntityConnection(
                        new DbModelBuilder().Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping.ToMetadataWorkspace(),
                        new SqlConnection())), true)
            {
                var _ = ((IObjectContextAdapter)this).ObjectContext;
            }
        }

        [Fact]
        public void ConnectionOrigin_should_return_user_code_when_existing_object_context_normal_constructor()
        {
            ConnectionOrigin_should_return_user_code_when_existing_object_context(typeof(ContextWithExistingObjectContext));
        }

        [Fact]
        public void ConnectionOrigin_should_return_user_code_when_existing_object_context_init_constructor()
        {
            ConnectionOrigin_should_return_user_code_when_existing_object_context(typeof(ContextWithExistingObjectContextWithInit));
        }

        private void ConnectionOrigin_should_return_user_code_when_existing_object_context(Type contextType)
        {
            var contextInfo = new DbContextInfo(contextType);

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
            Assert.Equal(
                Strings.DbContextServices_MissingDefaultCtor(typeof(ContextWithoutDefaultCtorBadFactory.ContextFactory)),
                Assert.Throws<InvalidOperationException>(() => new DbContextInfo(typeof(ContextWithoutDefaultCtorBadFactory))).Message);
        }

        [Fact]
        public void CreateInstance_should_use_passed_provider_info_when_building_model_normal_constructor()
        {
            CreateInstance_should_use_passed_provider_info_when_building_model(typeof(SimpleContext));
        }

        [Fact]
        public void CreateInstance_should_use_passed_provider_info_when_building_model_init_constructor()
        {
            CreateInstance_should_use_passed_provider_info_when_building_model(typeof(SimpleContextWithInit));
        }

        private void CreateInstance_should_use_passed_provider_info_when_building_model(Type contextType)
        {
            var contextInfo = new DbContextInfo(contextType, ProviderRegistry.SqlCe4_ProviderInfo);

            Assert.Equal(ProviderRegistry.SqlCe4_ProviderInfo.ProviderInvariantName, contextInfo.ConnectionProviderName);
            Assert.Equal(string.Empty, contextInfo.ConnectionString);

            using (var context = contextInfo.CreateInstance())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                Assert.NotNull(objectContext);
                Assert.Equal("SqlCeConnection", ((EntityConnection)objectContext.Connection).StoreConnection.GetType().Name);
            }
        }

        [Fact]
        public void
            CreateInstance_should_use_passed_provider_info_when_building_model_even_when_connection_is_in_app_config_normal_constructor()
        {
            CreateInstance_should_use_passed_provider_info_when_building_model_even_when_connection_is_in_app_config(
                typeof(ContextWithConnectionInConfig));
        }

        [Fact]
        public void
            CreateInstance_should_use_passed_provider_info_when_building_model_even_when_connection_is_in_app_config_init_constructor()
        {
            CreateInstance_should_use_passed_provider_info_when_building_model_even_when_connection_is_in_app_config(
                typeof(ContextWithConnectionInConfigWithInit));
        }

        // CodePlex 1524
        private void CreateInstance_should_use_passed_provider_info_when_building_model_even_when_connection_is_in_app_config(
            Type contextType)
        {
            var contextInfo = new DbContextInfo(contextType, ProviderRegistry.SqlCe4_ProviderInfo);

            using (var context = contextInfo.CreateInstance())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                Assert.NotNull(objectContext);
                Assert.Equal("SqlCeConnection", ((EntityConnection)objectContext.Connection).StoreConnection.GetType().Name);
            }
        }

        public class ContextWithConnectionInConfig : DbContext
        {
            public ContextWithConnectionInConfig()
                : base("name=DbContextInfoTest")
            {
            }
        }

        public class ContextWithConnectionInConfigWithInit : DbContext
        {
            public ContextWithConnectionInConfigWithInit()
                : base("name=DbContextInfoTest")
            {
                var _ = ((IObjectContextAdapter)this).ObjectContext;
            }
        }

        private class ContextWithExternalOnModelCreating1 : DbContext
        {
        }

        private class ContextWithExternalOnModelCreating1WithInit : DbContext
        {
        }

        [Fact]
        public void CreateInstance_should_attach_on_model_creating_custom_action_and_invoke_once_normal_constructor()
        {
            CreateInstance_should_attach_on_model_creating_custom_action_and_invoke_once(typeof(ContextWithExternalOnModelCreating1));
        }

        [Fact]
        public void CreateInstance_should_attach_on_model_creating_custom_action_and_invoke_once_init_constructor()
        {
            CreateInstance_should_attach_on_model_creating_custom_action_and_invoke_once(
                typeof(ContextWithExternalOnModelCreating1WithInit));
        }

        private void CreateInstance_should_attach_on_model_creating_custom_action_and_invoke_once(Type contextType)
        {
            var calledCount = 0;

            var contextInfo = new DbContextInfo(contextType);
            contextInfo.OnModelCreating = _ => calledCount++;

            contextInfo.CreateInstance().Dispose();

            Assert.Equal(0, calledCount);

            ObjectContext objectContext;
            using (var context = contextInfo.CreateInstance())
            {
                objectContext = ((IObjectContextAdapter)context).ObjectContext;
            }

            Assert.NotNull(objectContext);
            Assert.Equal(1, calledCount);

            using (var contexct = contextInfo.CreateInstance())
            {
                objectContext = ((IObjectContextAdapter)contexct).ObjectContext;
            }

            Assert.NotNull(objectContext);
            Assert.Equal(1, calledCount);
        }

        private class ContextWithExternalOnModelCreating2 : DbContext
        {
        }

        private class ContextWithExternalOnModelCreating2WithInit : DbContext
        {
        }

        [Fact]
        public void Can_use_custom_on_model_creating_action_to_configure_model_builder_normal_constructor()
        {
            Can_use_custom_on_model_creating_action_to_configure_model_builder(typeof(ContextWithExternalOnModelCreating2));
        }

        [Fact]
        public void Can_use_custom_on_model_creating_action_to_configure_model_builder_init_constructor()
        {
            Can_use_custom_on_model_creating_action_to_configure_model_builder(typeof(ContextWithExternalOnModelCreating2WithInit));
        }

        private void Can_use_custom_on_model_creating_action_to_configure_model_builder(Type contextType)
        {
            var contextInfo = new DbContextInfo(contextType);

            using (var context = contextInfo.CreateInstance())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                Assert.NotNull(objectContext);
                Assert.False(objectContext.CreateDatabaseScript().Contains("EdmMetadata"));
            }
        }

        private class ContextWithExternalOnModelCreating3 : DbContext
        {
            public DbSet<FakeEntity> Fakes { get; set; }
        }

        private class ContextWithExternalOnModelCreating3WithInit : DbContext
        {
            public DbSet<FakeEntity> Fakes { get; set; }
        }

        [Fact]
        public void Can_unset_custom_on_model_creating_action_normal_constructor()
        {
            Can_unset_custom_on_model_creating_action(typeof(ContextWithExternalOnModelCreating3));
        }

        [Fact]
        public void Can_unset_custom_on_model_creating_action_init_constructor()
        {
            Can_unset_custom_on_model_creating_action(typeof(ContextWithExternalOnModelCreating3WithInit));
        }

        private void Can_unset_custom_on_model_creating_action(Type contextType)
        {
            var contextInfo
                = new DbContextInfo(contextType)
                {
                    OnModelCreating = mb => mb.Ignore<FakeEntity>()
                };

            contextInfo.OnModelCreating = null;

            using (var context = contextInfo.CreateInstance())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                Assert.NotNull(objectContext);
                Assert.True(objectContext.CreateDatabaseScript().Contains("FakeEntities"));
            }
        }

        [Fact]
        public void Should_obtain_DefaultConnectionFactory_from_supplied_config_but_this_can_be_overriden_normal_constructor()
        {
            Should_obtain_DefaultConnectionFactory_from_supplied_config_but_this_can_be_overriden(typeof(ContextWithoutDefaultCtor));
        }

        [Fact]
        public void Should_obtain_DefaultConnectionFactory_from_supplied_config_but_this_can_be_overriden_init_constructor()
        {
            Should_obtain_DefaultConnectionFactory_from_supplied_config_but_this_can_be_overriden(typeof(ContextWithoutDefaultCtorWithInit));
        }

        private void Should_obtain_DefaultConnectionFactory_from_supplied_config_but_this_can_be_overriden(Type contextType)
        {
            RunTestWithConnectionFactory(
                Database.ResetDefaultConnectionFactory,
                () =>
                {
                    var config = CreateEmptyConfig().AddDefaultConnectionFactory(
                        typeof(FakeDbContextInfoConnectionFactory).FullName + ", EntityFramework.UnitTests",
                        new string[0]);

                    var contextInfo = new DbContextInfo(contextType, config);

                    Assert.IsType<FakeDbContextInfoConnectionFactory>(FunctionalTestsConfiguration.OriginalConnectionFactories.Last());
                    Assert.Equal(DbConnectionStringOrigin.Convention, contextInfo.ConnectionStringOrigin);
                    Assert.True(contextInfo.ConnectionString.Contains(@"Initial Catalog=foo"));
                    Assert.Equal("System.Data.SqlClient", contextInfo.ConnectionProviderName);
                    if (!DatabaseTestHelpers.IsSqlAzure(contextInfo.ConnectionString))
                    {
                        Assert.True(contextInfo.ConnectionString.Contains(@"Integrated Security=True"));
                    }

                    if (!DatabaseTestHelpers.IsSqlAzure(contextInfo.ConnectionString)
                        && !DatabaseTestHelpers.IsLocalDb(contextInfo.ConnectionString))
                    {
                        Assert.True(contextInfo.ConnectionString.Contains(@"Data Source=.\SQLEXPRESS"));
                    }
                });
        }

        [Fact]
        public void
            Should_use_use_default_DefaultConnectionFactory_if_supplied_config_contains_no_DefaultConnectionFactory_normal_constructor()
        {
            Should_use_use_default_DefaultConnectionFactory_if_supplied_config_contains_no_DefaultConnectionFactory(
                typeof(ContextWithoutDefaultCtor));
        }

        [Fact]
        public void Should_use_use_default_DefaultConnectionFactory_if_supplied_config_contains_no_DefaultConnectionFactory_init_constructor
            ()
        {
            Should_use_use_default_DefaultConnectionFactory_if_supplied_config_contains_no_DefaultConnectionFactory(
                typeof(ContextWithoutDefaultCtorWithInit));
        }

        private void Should_use_use_default_DefaultConnectionFactory_if_supplied_config_contains_no_DefaultConnectionFactory(
            Type contextType)
        {
            RunTestWithConnectionFactory(
                Database.ResetDefaultConnectionFactory,
                () =>
                {
                    var config = CreateEmptyConfig();

                    var contextInfo = new DbContextInfo(contextType, config);

                    Assert.Equal(DbConnectionStringOrigin.Convention, contextInfo.ConnectionStringOrigin);
                    Assert.True(contextInfo.ConnectionString.Contains(@"Initial Catalog=foo"));
                    Assert.Equal("System.Data.SqlClient", contextInfo.ConnectionProviderName);
                    if (!DatabaseTestHelpers.IsSqlAzure(contextInfo.ConnectionString)
                        && !DatabaseTestHelpers.IsLocalDb(contextInfo.ConnectionString))
                    {
                        Assert.True(contextInfo.ConnectionString.Contains(@"Data Source=.\SQLEXPRESS"));
                    }
                });
        }

        [Fact]
        public void Should_use_connectioin_string_from_supplied_config_even_if_DefaultConnectionFactory_is_also_present_normal_constructor()
        {
            Should_use_connectioin_string_from_supplied_config_even_if_DefaultConnectionFactory_is_also_present(
                typeof(ContextWithoutDefaultCtor));
        }

        [Fact]
        public void Should_use_connectioin_string_from_supplied_config_even_if_DefaultConnectionFactory_is_also_present_init_constructor()
        {
            Should_use_connectioin_string_from_supplied_config_even_if_DefaultConnectionFactory_is_also_present(
                typeof(ContextWithoutDefaultCtorWithInit));
        }

        private void Should_use_connectioin_string_from_supplied_config_even_if_DefaultConnectionFactory_is_also_present(Type contextType)
        {
            RunTestWithConnectionFactory(
                Database.ResetDefaultConnectionFactory,
                () =>
                {
                    var config =
                        AddConnectionStrings(
                            CreateEmptyConfig().AddDefaultConnectionFactory(
                                typeof(FakeDbContextInfoConnectionFactory).FullName + ", EntityFramework.UnitTests",
                                new string[0]));

                    var contextInfo = new DbContextInfo(contextType, config);

                    Assert.Equal(DbConnectionStringOrigin.Configuration, contextInfo.ConnectionStringOrigin);
                    Assert.Equal("Initial Catalog=foo", contextInfo.ConnectionString);
                    Assert.Equal("foo", contextInfo.ConnectionStringName);
                    Assert.Equal("System.Data.SqlClient", contextInfo.ConnectionProviderName);
                });
        }

        [Fact]
        public void Should_use_DefaultConnectionFactory_set_in_code_even_if_one_was_supplied_in_config_normal_constructor()
        {
            Should_use_DefaultConnectionFactory_set_in_code_even_if_one_was_supplied_in_config(typeof(ContextWithoutDefaultCtor));
        }

        [Fact]
        public void Should_use_DefaultConnectionFactory_set_in_code_even_if_one_was_supplied_in_config_init_constructor()
        {
            Should_use_DefaultConnectionFactory_set_in_code_even_if_one_was_supplied_in_config(typeof(ContextWithoutDefaultCtorWithInit));
        }

        public void Should_use_DefaultConnectionFactory_set_in_code_even_if_one_was_supplied_in_config(Type contextType)
        {
#pragma warning disable 612,618
            RunTestWithConnectionFactory(
                () => Database.DefaultConnectionFactory = new SqlConnectionFactory(),
                () =>
#pragma warning restore 612,618
                {
                    var config = CreateEmptyConfig().
                        AddDefaultConnectionFactory(
                            typeof(FakeDbContextInfoConnectionFactory).FullName + ", EntityFramework.UnitTests",
                            new string[0]);

                    var contextInfo = new DbContextInfo(contextType, config);

                    Assert.Equal(DbConnectionStringOrigin.Convention, contextInfo.ConnectionStringOrigin);
                    Assert.True(contextInfo.ConnectionString.Contains(@"Initial Catalog=foo"));
                    Assert.Equal("System.Data.SqlClient", contextInfo.ConnectionProviderName);
                    if (!DatabaseTestHelpers.IsSqlAzure(contextInfo.ConnectionString)
                        && !DatabaseTestHelpers.IsLocalDb(contextInfo.ConnectionString))
                    {
                        Assert.True(contextInfo.ConnectionString.Contains(@"Data Source=.\SQLEXPRESS"));
                    }
                });
        }

        [Fact]
        public void Setting_DefaultConnectionFactory_from_code_marks_DefaultConnectionFactory_as_changed_and_this_can_be_reset()
        {
            RunTestWithConnectionFactory(
                Database.ResetDefaultConnectionFactory,
                () =>
                {
                    Assert.False(Database.DefaultConnectionFactoryChanged);

#pragma warning disable 612,618
                    Database.DefaultConnectionFactory = new SqlConnectionFactory();
#pragma warning restore 612,618
                    Assert.True(Database.DefaultConnectionFactoryChanged);

                    Database.ResetDefaultConnectionFactory();
                    Assert.False(Database.DefaultConnectionFactoryChanged);
                });
        }

        private void RunTestWithConnectionFactory(Action connectionFactorySetter, Action test)
        {
            connectionFactorySetter();
            try
            {
                test();
            }
            finally
            {
                Database.ResetDefaultConnectionFactory();
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
        public void Can_set_hard_coded_connection_normal_constructor()
        {
            Can_set_hard_coded_connection(typeof(SimpleContext));
        }

        [Fact]
        public void Can_set_hard_coded_connection_init_constructor()
        {
            Can_set_hard_coded_connection(typeof(SimpleContextWithInit));
        }

        private void Can_set_hard_coded_connection(Type contextType)
        {
            var connection = new DbConnectionInfo("Database=UseThisDatabaseInstead", "System.Data.SqlClient");
            var contextInfo = new DbContextInfo(contextType, connection);

            Assert.Equal(DbConnectionStringOrigin.DbContextInfo, contextInfo.ConnectionStringOrigin);
            Assert.Equal("System.Data.SqlClient", contextInfo.ConnectionProviderName);
            Assert.Equal(null, contextInfo.ConnectionStringName);
            Assert.True(contextInfo.IsConstructible);

            using (var context = contextInfo.CreateInstance())
            {
                Assert.Equal("UseThisDatabaseInstead", context.Database.Connection.Database);
                Assert.Equal(
                    "UseThisDatabaseInstead",
                    ((EntityConnection)((IObjectContextAdapter)context).ObjectContext.Connection).StoreConnection.Database);
            }
        }

        [Fact]
        public void Can_set_hard_coded_connection_from_default_config_normal_constructor()
        {
            Can_set_hard_coded_connection_from_default_config(typeof(SimpleContext));
        }

        [Fact]
        public void Can_set_hard_coded_connection_from_default_config_init_constructor()
        {
            Can_set_hard_coded_connection_from_default_config(typeof(SimpleContextWithInit));
        }

        private void Can_set_hard_coded_connection_from_default_config(Type contextType)
        {
            var connection = new DbConnectionInfo("OverrideConnectionTest");
            var contextInfo = new DbContextInfo(contextType, connection);

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
        public void Can_set_hard_coded_connection_from_supplied_config_normal_constructor()
        {
            Can_set_hard_coded_connection_from_supplied_config(typeof(SimpleContext));
        }

        [Fact]
        public void Can_set_hard_coded_connection_from_supplied_config_init_constructor()
        {
            Can_set_hard_coded_connection_from_supplied_config(typeof(SimpleContextWithInit));
        }

        private void Can_set_hard_coded_connection_from_supplied_config(Type contextType)
        {
            var connection = new DbConnectionInfo("GetMeFromSuppliedConfig");
            var contextInfo = new DbContextInfo(
                contextType,
                CreateEmptyConfig().AddConnectionString(
                    "GetMeFromSuppliedConfig", "Database=ConnectionFromSuppliedConfig", "System.Data.SqlClient"),
                connection);

            Assert.Equal(DbConnectionStringOrigin.DbContextInfo, contextInfo.ConnectionStringOrigin);
            Assert.Equal("System.Data.SqlClient", contextInfo.ConnectionProviderName);
            Assert.Equal("GetMeFromSuppliedConfig", contextInfo.ConnectionStringName);
            Assert.True(contextInfo.IsConstructible);

            using (var context = contextInfo.CreateInstance())
            {
                Assert.Equal("ConnectionFromSuppliedConfig", context.Database.Connection.Database);
                Assert.Equal(
                    "ConnectionFromSuppliedConfig",
                    ((EntityConnection)((IObjectContextAdapter)context).ObjectContext.Connection).StoreConnection.Database);
            }
        }

        [Fact]
        public void Supplied_config_used_to_load_original_and_overriden_connection_normal_constructor()
        {
            Supplied_config_used_to_load_original_and_overriden_connection(typeof(ContextWithConnectionNameNotInAppConfigFile));
        }

        [Fact]
        public void Supplied_config_used_to_load_original_and_overriden_connection_init_constructor()
        {
            Supplied_config_used_to_load_original_and_overriden_connection(typeof(ContextWithConnectionNameNotInAppConfigFileWithInit));
        }

        private void Supplied_config_used_to_load_original_and_overriden_connection(Type contextType)
        {
            var connection = new DbConnectionInfo("GetMeFromSuppliedConfig");
            var contextInfo = new DbContextInfo(
                contextType,
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
        public void Exceptions_applying_new_connection_surfaced_and_context_type_is_unmapped_normal_constructor()
        {
            Exceptions_applying_new_connection_surfaced_and_context_type_is_unmapped(typeof(ContextWithConnectionNameNotInAppConfigFile));
        }

        [Fact]
        public void Exceptions_applying_new_connection_surfaced_and_context_type_is_unmapped_init_constructor()
        {
            Exceptions_applying_new_connection_surfaced_and_context_type_is_unmapped(
                typeof(ContextWithConnectionNameNotInAppConfigFileWithInit));
        }

        private void Exceptions_applying_new_connection_surfaced_and_context_type_is_unmapped(Type contextType)
        {
            var connection = new DbConnectionInfo("GetMeFromSuppliedConfig");

            Assert.Equal(
                Strings.DbContext_ConnectionStringNotFound("GetMeFromSuppliedConfig"),
                Assert.Throws<InvalidOperationException>(
                    () => new DbContextInfo(contextType, CreateEmptyConfig(), connection)).Message);

            Assert.Null(DbContextInfo.TryGetInfoForContext(contextType));
        }

        [DbConfigurationType(typeof(FunctionalTestsConfiguration))]
        public class ContextWithConnectionNameNotInAppConfigFile : DbContext
        {
            public ContextWithConnectionNameNotInAppConfigFile()
                : base("name=WontFindMeInDefaultConfig")
            {
            }
        }

        [DbConfigurationType(typeof(FunctionalTestsConfiguration))]
        public class ContextWithConnectionNameNotInAppConfigFileWithInit : DbContext
        {
            public ContextWithConnectionNameNotInAppConfigFileWithInit()
                : base("name=WontFindMeInDefaultConfig")
            {
                var _ = ((IObjectContextAdapter)this).ObjectContext;
            }
        }

        [Fact]
        public void CreateInstance_should_use_passed_provider_info_when_building_model_even_when_config_is_passed_as_well_normal_constructor
            ()
        {
            CreateInstance_should_use_passed_provider_info_when_building_model_even_when_config_is_passed_as_well(typeof(SimpleContext));
        }

        [Fact]
        public void CreateInstance_should_use_passed_provider_info_when_building_model_even_when_config_is_passed_as_well_init_constructor()
        {
            CreateInstance_should_use_passed_provider_info_when_building_model_even_when_config_is_passed_as_well(
                typeof(SimpleContextWithInit));
        }

        private void CreateInstance_should_use_passed_provider_info_when_building_model_even_when_config_is_passed_as_well(Type contextType)
        {
            var config = AddConnectionStrings(
                CreateEmptyConfig().AddDefaultConnectionFactory(
                    typeof(FakeDbContextInfoConnectionFactory).FullName + ", EntityFramework.UnitTests",
                    new string[0]));

            var contextInfo = new DbContextInfo(contextType, config, ProviderRegistry.SqlCe4_ProviderInfo);

            Assert.Equal(ProviderRegistry.SqlCe4_ProviderInfo.ProviderInvariantName, contextInfo.ConnectionProviderName);
            Assert.Equal(string.Empty, contextInfo.ConnectionString);

            using (var context = contextInfo.CreateInstance())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                Assert.NotNull(objectContext);
                Assert.Equal("SqlCeConnection", ((EntityConnection)objectContext.Connection).StoreConnection.GetType().Name);
            }
        }

        [Fact]
        public void CreateInstance_should_use_passed_connection_string_even_when_provider_info_is_passed_as_well_normal_constructor()
        {
            CreateInstance_should_use_passed_connection_string_even_when_provider_info_is_passed_as_well(typeof(ContextWithoutDefaultCtor));
        }

        [Fact]
        public void CreateInstance_should_use_passed_connection_string_even_when_provider_info_is_passed_as_well_init_constructor()
        {
            CreateInstance_should_use_passed_connection_string_even_when_provider_info_is_passed_as_well(
                typeof(ContextWithoutDefaultCtorWithInit));
        }

        private void CreateInstance_should_use_passed_connection_string_even_when_provider_info_is_passed_as_well(Type contextType)
        {
            var config = AddConnectionStrings(
                CreateEmptyConfig().AddDefaultConnectionFactory(
                    typeof(FakeDbContextInfoConnectionFactory).FullName + ", EntityFramework.UnitTests",
                    new string[0]));

            var contextInfo = new DbContextInfo(contextType, config, ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(DbConnectionStringOrigin.Configuration, contextInfo.ConnectionStringOrigin);
            Assert.Equal("Initial Catalog=foo", contextInfo.ConnectionString);
            Assert.Equal("foo", contextInfo.ConnectionStringName);
        }

        [Fact] // CodePlex 291, 1316
        public void CreateInstance_should_not_cause_database_initializer_to_run_even_if_context_consturctor_would_cause_it()
        {
            var contextInfo = new DbContextInfo(typeof(InitTestContext));

            using (var context = contextInfo.CreateInstance())
            {
                // Do something that would normally cause initialization
                Assert.NotNull(((IObjectContextAdapter)context).ObjectContext);
            }

            Assert.False(InitTestInitializer.HasRun);

            // Use context normally--initializer should now run.
            using (var context = new InitTestContext())
            {
                Assert.NotNull(((IObjectContextAdapter)context).ObjectContext);
            }

            Assert.True(InitTestInitializer.HasRun);
        }

        public class InitTestContext : DbContext
        {
            static InitTestContext()
            {
                Database.SetInitializer(new InitTestInitializer());
            }

            public InitTestContext()
            {
                // Do something that would normally cause initialization
                Assert.NotNull(((IObjectContextAdapter)this).ObjectContext);
            }
        }

        public class InitTestInitializer : IDatabaseInitializer<InitTestContext>
        {
            public static bool HasRun { get; set; }

            public void InitializeDatabase(InitTestContext context)
            {
                HasRun = true;
            }
        }

        [Fact]
        public void Context_type_can_be_assoictaed_with_DbContextInfo_and_then_later_unsuppressed()
        {
            Assert.Null(DbContextInfo.TryGetInfoForContext(typeof(ContextToAssociate)));
            Assert.Null(DbContextInfo.TryGetInfoForContext(typeof(ContextToNotAssociate)));

            var contextInfo = new DbContextInfo(typeof(ContextToAssociate));
            DbContextInfo.MapContextToInfo(typeof(ContextToAssociate), contextInfo);

            Assert.Same(contextInfo, DbContextInfo.TryGetInfoForContext(typeof(ContextToAssociate)));
            Assert.Null(DbContextInfo.TryGetInfoForContext(typeof(ContextToNotAssociate)));

            DbContextInfo.ClearInfoForContext(typeof(ContextToAssociate));

            Assert.Null(DbContextInfo.TryGetInfoForContext(typeof(ContextToAssociate)));
            Assert.Null(DbContextInfo.TryGetInfoForContext(typeof(ContextToNotAssociate)));
        }

        public class ContextToAssociate : DbContext
        {
        }

        public class ContextToNotAssociate : DbContext
        {
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
