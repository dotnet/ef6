namespace ProductivityApiUnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using Moq;
    using Xunit;

    public class AppConfigTests : UnitTestBase
    {
        #region Connection string tests

        [Fact]
        public void GetConnectionString_from_running_application_config()
        {
            var conn = AppConfig.DefaultInstance.GetConnectionString("AppConfigTest");

            Assert.Equal("FromTheDefaultConfigFile", conn.ConnectionString);
        }

        [Fact]
        public void GetConnectionString_from_ConnectionStringSettingsCollection()
        {
            var strings = new ConnectionStringSettingsCollection();
            strings.Add(new ConnectionStringSettings("AppConfigTest", "FromConnectionStringSettingsCollection", ""));
            var config = new AppConfig(strings);

            var conn = config.GetConnectionString("AppConfigTest");

            Assert.Equal("FromConnectionStringSettingsCollection", conn.ConnectionString);
        }

        [Fact]
        public void GetConnectionString_from_Configuration()
        {
            var config = new AppConfig(
                CreateEmptyConfig().AddConnectionString("AppConfigTest", "FromConfiguration"));

            var conn = config.GetConnectionString("AppConfigTest");

            Assert.Equal("FromConfiguration", conn.ConnectionString);
        }

        #endregion

        #region Connection factory tests

        private const string SqlExpressBaseConnectionString = @"Data Source=.\SQLEXPRESS; Integrated Security=True; MultipleActiveResultSets=True";
        private const string SqlConnectionFactoryName = "System.Data.Entity.Infrastructure.SqlConnectionFactory, EntityFramework";

        [Fact]
        public void GetDefaultConnectionFactory_creates_normal_SqlConnectionFactory_using_default_ctor_if_no_factory_is_in_app_config()
        {
            var factory = AppConfig.DefaultInstance.DefaultConnectionFactory;

            Assert.IsType<SqlConnectionFactory>(factory);
            Assert.Equal(SqlExpressBaseConnectionString, ((SqlConnectionFactory)factory).BaseConnectionString);
        }

        [Fact]
        public void GetDefaultConnectionFactory_creates_normal_SqlConnectionFactory_if_no_factory_is_in_Configuration_ctor()
        {
            var factory = new AppConfig(CreateEmptyConfig()).DefaultConnectionFactory;

            Assert.IsType<SqlConnectionFactory>(factory);
            Assert.Equal(SqlExpressBaseConnectionString, ((SqlConnectionFactory)factory).BaseConnectionString);
        }

        [Fact]
        public void GetDefaultConnectionFactory_creates_normal_SqlConnectionFactory_from_ConnectionStringSettingsCollection_ctor()
        {
            var factory = new AppConfig(new ConnectionStringSettingsCollection()).DefaultConnectionFactory;

            Assert.IsType<SqlConnectionFactory>(factory);
            Assert.Equal(SqlExpressBaseConnectionString, ((SqlConnectionFactory)factory).BaseConnectionString);
        }

        [Fact]
        public void GetDefaultConnectionFactory_throws_if_factory_name_in_config_is_empty()
        {
            var config = new AppConfig(CreateEmptyConfig().AddDefaultConnectionFactory(""));
            Assert.Equal(Strings.SetConnectionFactoryFromConfigFailed(""), Assert.Throws<InvalidOperationException>(() => { var temp = config.DefaultConnectionFactory; }).Message);
        }

        [Fact]
        public void GetDefaultConnectionFactory_throws_if_factory_name_in_config_is_whitespace()
        {
            var config = new AppConfig(CreateEmptyConfig().AddDefaultConnectionFactory(" "));
            Assert.Equal(Strings.SetConnectionFactoryFromConfigFailed(" "), Assert.Throws<InvalidOperationException>(() => { var temp = config.DefaultConnectionFactory; }).Message);
        }

        [Fact]
        public void GetDefaultConnectionFactory_can_create_instance_of_specified_connection_factory_with_no_arguments()
        {
            var factory = new AppConfig(CreateEmptyConfig().AddDefaultConnectionFactory(typeof(FakeConnectionFactoryNoParams).AssemblyQualifiedName))
                .DefaultConnectionFactory;

            Assert.IsType<FakeConnectionFactoryNoParams>(factory);
        }

        [Fact]
        public void GetDefaultConnectionFactory_can_create_instance_of_specified_connection_factory_with_one_argument()
        {
            var factory = new AppConfig(CreateEmptyConfig().AddDefaultConnectionFactory(
                    typeof(FakeConnectionFactoryOneParam).AssemblyQualifiedName,
                    "argument 0"))
                .DefaultConnectionFactory;

            Assert.IsType<FakeConnectionFactoryOneParam>(factory);
            Assert.Equal("argument 0", ((FakeConnectionFactoryOneParam)factory).Arg);
        }

        [Fact]
        public void GetDefaultConnectionFactory_can_create_instance_of_specified_connection_factory_with_multiple_arguments()
        {
            var arguments = new[] { "arg0", "arg1", "arg2", "arg3", "arg4", "arg5", "arg6", "arg7", "arg8", "arg9", "arg10" };

            var factory = new AppConfig(CreateEmptyConfig().AddDefaultConnectionFactory(
                    typeof(FakeConnectionFactoryManyParams).AssemblyQualifiedName,
                    arguments))
                .DefaultConnectionFactory;

            Assert.IsType<FakeConnectionFactoryManyParams>(factory);
            for (int i = 0; i < arguments.Length; i++)
            {
                Assert.Equal(arguments[i], ((FakeConnectionFactoryManyParams)factory).Args[i]);
            }
        }

        [Fact]
        public void GetDefaultConnectionFactory_can_handle_empty_string_arguments()
        {
            var factory = new AppConfig(CreateEmptyConfig().AddDefaultConnectionFactory(typeof(FakeConnectionFactoryOneParam).AssemblyQualifiedName, ""))
               .DefaultConnectionFactory;

            Assert.IsType<FakeConnectionFactoryOneParam>(factory);
            Assert.Equal("", ((FakeConnectionFactoryOneParam)factory).Arg);
        }

        [Fact]
        public void GetDefaultConnectionFactory_throws_if_factory_type_cannot_be_found()
        {
            var config = new AppConfig(CreateEmptyConfig().AddDefaultConnectionFactory("BogusFactory"));

            Assert.Equal(Strings.SetConnectionFactoryFromConfigFailed("BogusFactory"), Assert.Throws<InvalidOperationException>(() => { var temp = config.DefaultConnectionFactory; }).Message);
        }

        [Fact]
        public void GetDefaultConnectionFactory_throws_if_empty_constructor_is_required_but_not_available()
        {
            var config = new AppConfig(CreateEmptyConfig().AddDefaultConnectionFactory(typeof(FakeConnectionFactoryOneParam).AssemblyQualifiedName));

            Assert.Equal(Strings.SetConnectionFactoryFromConfigFailed(typeof(FakeConnectionFactoryOneParam).AssemblyQualifiedName), Assert.Throws<InvalidOperationException>(() => { var temp = config.DefaultConnectionFactory; }).Message);
        }

        [Fact]
        public void GetDefaultConnectionFactory_throws_if_constructor_with_parameters_is_required_but_not_available()
        {
            var config = new AppConfig(CreateEmptyConfig().AddDefaultConnectionFactory(typeof(FakeConnectionFactoryNoParams).AssemblyQualifiedName, ""));

            Assert.Equal(Strings.SetConnectionFactoryFromConfigFailed(typeof(FakeConnectionFactoryNoParams).AssemblyQualifiedName), Assert.Throws<InvalidOperationException>(() => { var temp = config.DefaultConnectionFactory; }).Message);
        }

        [Fact]
        public void GetDefaultConnectionFactory_throws_if_factory_type_does_not_implement_IDbConnectionFactory()
        {
            var config = new AppConfig(CreateEmptyConfig().AddDefaultConnectionFactory(typeof(FakeNonConnectionFactory).AssemblyQualifiedName));

            Assert.Equal(Strings.SetConnectionFactoryFromConfigFailed(typeof(FakeNonConnectionFactory).AssemblyQualifiedName), Assert.Throws<InvalidOperationException>(() => { var temp = config.DefaultConnectionFactory; }).Message);
        }

        [Fact]
        public void GetDefaultConnectionFactory_throws_if_factory_type_is_Abstract()
        {
            var config = new AppConfig(CreateEmptyConfig().AddDefaultConnectionFactory(typeof(FakeBaseConnectionFactory).AssemblyQualifiedName));

            Assert.Equal(Strings.SetConnectionFactoryFromConfigFailed(typeof(FakeBaseConnectionFactory).AssemblyQualifiedName), Assert.Throws<InvalidOperationException>(() => { var temp = config.DefaultConnectionFactory; }).Message);
        }

        [Fact]
        public void GetDefaultConnectionFactory_can_be_used_to_create_instance_of_SqlConnectionFactory()
        {
            var factory = new AppConfig(CreateEmptyConfig().AddDefaultConnectionFactory(SqlConnectionFactoryName, "Some Connection String"))
                .DefaultConnectionFactory;

            Assert.IsType<SqlConnectionFactory>(factory);
            Assert.Equal("Some Connection String", ((SqlConnectionFactory)factory).BaseConnectionString);
        }

        [Fact]
        public void GetDefaultConnectionFactory_can_be_used_to_create_instance_of_SqlCEConnectionFactory_with_just_provider_invariant_name()
        {
            var factory = new AppConfig(CreateEmptyConfig().AddDefaultConnectionFactory(
                    "System.Data.Entity.Infrastructure.SqlCeConnectionFactory, EntityFramework",
                    "Provider Invariant Name"))
                .DefaultConnectionFactory;

            Assert.IsType<SqlCeConnectionFactory>(factory);
            Assert.Equal("Provider Invariant Name", ((SqlCeConnectionFactory)factory).ProviderInvariantName);
            Assert.Equal(new SqlCeConnectionFactory("PIN").DatabaseDirectory, ((SqlCeConnectionFactory)factory).DatabaseDirectory);
            Assert.Equal(new SqlCeConnectionFactory("PIN").BaseConnectionString, ((SqlCeConnectionFactory)factory).BaseConnectionString);
        }

        [Fact]
        public void GetDefaultConnectionFactory_can_be_used_to_create_instance_of_SqlCEConnectionFactory_with_provider_invariant_name_and_other_arguments()
        {
            var factory = new AppConfig(CreateEmptyConfig().AddDefaultConnectionFactory(
                    "System.Data.Entity.Infrastructure.SqlCeConnectionFactory, EntityFramework",
                    "Provider Invariant Name",
                    "Database Directory",
                    "Base Connection String"))
                .DefaultConnectionFactory;

            Assert.IsType<SqlCeConnectionFactory>(factory);
            Assert.Equal("Provider Invariant Name", ((SqlCeConnectionFactory)factory).ProviderInvariantName);
            Assert.Equal("Database Directory", ((SqlCeConnectionFactory)factory).DatabaseDirectory);
            Assert.Equal("Base Connection String", ((SqlCeConnectionFactory)factory).BaseConnectionString);
        }

        #endregion

        #region Database initializer tests
        public class FakeForAppConfigWithoutInitializer : DbContextUsingMockInternalContext
        {
        }

        [Fact]
        public void Context_config_without_initializer_doesnt_affect_initializer()
        {
            Database.SetInitializer<FakeForAppConfigWithoutInitializer>(new GenericFakeInitializerForAppConfig<FakeForAppConfigWithoutInitializer>());
            GenericFakeInitializerForAppConfig<FakeForAppConfigWithoutInitializer>.Reset();

            var config = new AppConfig(
                CreateEmptyConfig()
                    .AddContextConfig(typeof(FakeForAppConfigWithoutInitializer).AssemblyQualifiedName));

            config.InternalApplyInitializers(force: true);

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForAppConfigWithoutInitializer>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            Assert.True(GenericFakeInitializerForAppConfig<FakeForAppConfigWithoutInitializer>.InitializerCalled);
        }

        public class FakeForAppConfigApplied : DbContextUsingMockInternalContext
        {
        }

        [Fact]
        public void Initializer_can_be_applied_from_configuration()
        {
            Database.SetInitializer<FakeForAppConfigApplied>(null);
            GenericFakeInitializerForAppConfig<FakeForAppConfigApplied>.Reset();

            var config = new AppConfig(CreateEmptyConfig().AddContextConfig(
                typeof(FakeForAppConfigApplied).AssemblyQualifiedName,
                initializerType: typeof(GenericFakeInitializerForAppConfig<FakeForAppConfigApplied>).AssemblyQualifiedName));

            config.InternalApplyInitializers(force: true);

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForAppConfigApplied>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            Assert.True(GenericFakeInitializerForAppConfig<FakeForAppConfigApplied>.InitializerCalled);
        }

        [Fact]
        public void AppConfig_processes_legacy_configurations()
        {
            Database.SetInitializer<FakeForLegacyAppConfig>(null);
            FakeInitializerForLegacyAppConfig.Reset();

            var config = new AppConfig(CreateEmptyConfig()
                .AddLegacyDatabaseInitializer(
                    typeof(FakeForLegacyAppConfig).AssemblyQualifiedName,
                    typeof(FakeInitializerForLegacyAppConfig).AssemblyQualifiedName));

            config.InternalApplyInitializers(force: true);

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForLegacyAppConfig>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            Assert.True(FakeInitializerForLegacyAppConfig.InitializerCalled);
            Assert.Equal(FakeInitializerCtor.NoArgs, FakeInitializerForLegacyAppConfig.CtorCalled);
        }

        [Fact]
        public void Initializer_can_be_applied_from_legacy_configuration()
        {
            Database.SetInitializer<FakeForLegacyAppConfig>(null);
            FakeInitializerForLegacyAppConfig.Reset();

            var initializerConfig = new LegacyDatabaseInitializerConfig(
                "DatabaseInitializerForType " + typeof(FakeForLegacyAppConfig).AssemblyQualifiedName,
                typeof(FakeInitializerForLegacyAppConfig).AssemblyQualifiedName);

            initializerConfig.ApplyInitializer();

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForLegacyAppConfig>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            Assert.True(FakeInitializerForLegacyAppConfig.InitializerCalled);
            Assert.Equal(FakeInitializerCtor.NoArgs, FakeInitializerForLegacyAppConfig.CtorCalled);
        }

        [Fact]
        public void Initializer_set_in_configuration_overrides_initializer_set_using_legacy_configuration()
        {
            Database.SetInitializer<FakeForLegacyAppConfig>(null);
            FakeInitializerForLegacyAppConfig.Reset();
            GenericFakeInitializerForAppConfig<FakeForLegacyAppConfig>.Reset();

            var config = new AppConfig(CreateEmptyConfig()
                .AddContextConfig(typeof(FakeForLegacyAppConfig).AssemblyQualifiedName, initializerType: typeof(GenericFakeInitializerForAppConfig<FakeForLegacyAppConfig>).AssemblyQualifiedName)
                .AddLegacyDatabaseInitializer(typeof(FakeForLegacyAppConfig).AssemblyQualifiedName, typeof(FakeInitializerForLegacyAppConfig).AssemblyQualifiedName));

            config.InternalApplyInitializers(force: true);

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForLegacyAppConfig>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            Assert.False(FakeInitializerForLegacyAppConfig.InitializerCalled);
            Assert.True(GenericFakeInitializerForAppConfig<FakeForLegacyAppConfig>.InitializerCalled);
        }

        [Fact]
        public void Initializer_set_in_configuration_overrides_initializer_disabled_using_legacy_configuration()
        {
            Database.SetInitializer<FakeForLegacyAppConfig>(null);
            FakeInitializerForLegacyAppConfig.Reset();

            var config = new AppConfig(CreateEmptyConfig()
                .AddContextConfig(typeof(FakeForLegacyAppConfig).AssemblyQualifiedName, initializerType: typeof(FakeInitializerForLegacyAppConfig).AssemblyQualifiedName)
                .AddLegacyDatabaseInitializer(typeof(FakeForLegacyAppConfig).AssemblyQualifiedName, "Disabled"));

            config.InternalApplyInitializers(force: true);

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForLegacyAppConfig>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            Assert.True(FakeInitializerForLegacyAppConfig.InitializerCalled);
        }

        [Fact]
        public void Initializer_disabled_in_configuration_overrides_initializer_set_using_legacy_configuration()
        {
            Database.SetInitializer<FakeForLegacyAppConfig>(null);
            FakeInitializerForLegacyAppConfig.Reset();

            var config = new AppConfig(CreateEmptyConfig()
                .AddContextConfig(typeof(FakeForLegacyAppConfig).AssemblyQualifiedName, isDatabaseInitializationDisabled: true)
                .AddLegacyDatabaseInitializer(typeof(FakeForLegacyAppConfig).AssemblyQualifiedName, typeof(FakeInitializerForLegacyAppConfig).AssemblyQualifiedName));

            config.InternalApplyInitializers(force: true);

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForLegacyAppConfig>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            Assert.False(FakeInitializerForLegacyAppConfig.InitializerCalled);
        }

        public class FakeForAppConfigCtorParam : DbContextUsingMockInternalContext
        {
        }

        [Fact]
        public void Initializer_can_be_applied_from_configuration_with_ctor_param()
        {
            Database.SetInitializer<FakeForAppConfigCtorParam>(null);
            GenericFakeInitializerForAppConfig<FakeForAppConfigCtorParam>.Reset();

            var config = new AppConfig(CreateEmptyConfig().AddContextConfig(
                typeof(FakeForAppConfigCtorParam).AssemblyQualifiedName,
                initializerType: typeof(GenericFakeInitializerForAppConfig<FakeForAppConfigCtorParam>).AssemblyQualifiedName,
                initializerParameters: new string[] { "TestValue" },
                initializerParameterTypes: new string[] { "System.String" }));

            config.InternalApplyInitializers(force: true);

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForAppConfigCtorParam>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            Assert.True(GenericFakeInitializerForAppConfig<FakeForAppConfigCtorParam>.InitializerCalled);
            Assert.Equal(FakeInitializerCtor.OneArg, GenericFakeInitializerForAppConfig<FakeForAppConfigCtorParam>.CtorCalled);
            Assert.Equal("TestValue", GenericFakeInitializerForAppConfig<FakeForAppConfigCtorParam>.Arg1);
        }

        public class FakeForAppConfigAssumeStringCtorParam : DbContextUsingMockInternalContext
        {
        }

        [Fact]
        public void Initializer_from_configuration_will_assume_string_ctor_param()
        {
            Database.SetInitializer<FakeForAppConfigAssumeStringCtorParam>(null);
            GenericFakeInitializerForAppConfig<FakeForAppConfigAssumeStringCtorParam>.Reset();

            var config = new AppConfig(CreateEmptyConfig().AddContextConfig(
                typeof(FakeForAppConfigAssumeStringCtorParam).AssemblyQualifiedName,
                initializerType: typeof(GenericFakeInitializerForAppConfig<FakeForAppConfigAssumeStringCtorParam>).AssemblyQualifiedName,
                initializerParameters: new string[] { "StringValue" }));

            config.InternalApplyInitializers(force: true);

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForAppConfigAssumeStringCtorParam>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            Assert.True(GenericFakeInitializerForAppConfig<FakeForAppConfigAssumeStringCtorParam>.InitializerCalled);
            Assert.Equal(FakeInitializerCtor.OneArg, GenericFakeInitializerForAppConfig<FakeForAppConfigAssumeStringCtorParam>.CtorCalled);
            Assert.Equal("StringValue", GenericFakeInitializerForAppConfig<FakeForAppConfigAssumeStringCtorParam>.Arg1);
        }

        public class FakeForAppConfigMultipleCtorParams : DbContextUsingMockInternalContext
        {
        }

        [Fact]
        public void Initializer_can_be_applied_from_configuration_with_multiple_ctor_params()
        {
            Database.SetInitializer<FakeForAppConfigMultipleCtorParams>(null);
            GenericFakeInitializerForAppConfig<FakeForAppConfigMultipleCtorParams>.Reset();

            var config = new AppConfig(CreateEmptyConfig().AddContextConfig(
                typeof(FakeForAppConfigMultipleCtorParams).AssemblyQualifiedName,
                initializerType: typeof(GenericFakeInitializerForAppConfig<FakeForAppConfigMultipleCtorParams>).AssemblyQualifiedName,
                initializerParameters: new string[] { "TestValueOne", "TestValueTwo" }));

            config.InternalApplyInitializers(force: true);

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForAppConfigMultipleCtorParams>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            Assert.True(GenericFakeInitializerForAppConfig<FakeForAppConfigMultipleCtorParams>.InitializerCalled);
            Assert.Equal(FakeInitializerCtor.TwoArgs, GenericFakeInitializerForAppConfig<FakeForAppConfigMultipleCtorParams>.CtorCalled);
            Assert.Equal("TestValueOne", GenericFakeInitializerForAppConfig<FakeForAppConfigMultipleCtorParams>.Arg1);
            Assert.Equal("TestValueTwo", GenericFakeInitializerForAppConfig<FakeForAppConfigMultipleCtorParams>.Arg2);
        }

        [Fact]
        public void Initializer_can_be_applied_from_configuration_with_non_string_ctor_param()
        {
            Database.SetInitializer<FakeForAppConfigNonStringParams>(null);
            FakeInitializerForAppConfigNonStringParams.Reset();

            var config = new AppConfig(CreateEmptyConfig().AddContextConfig(
                typeof(FakeForAppConfigNonStringParams).AssemblyQualifiedName,
                initializerType: typeof(FakeInitializerForAppConfigNonStringParams).AssemblyQualifiedName,
                initializerParameters: new string[] { "3" },
                initializerParameterTypes: new string[] { "System.Int32" }));

            config.InternalApplyInitializers(force: true);

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForAppConfigNonStringParams>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            Assert.True(FakeInitializerForAppConfigNonStringParams.InitializerCalled);
            Assert.Equal(3, FakeInitializerForAppConfigNonStringParams.Arg1);
        }

        [Fact]
        public void Initializer_can_be_applied_from_configuration_with_ctor_params_of_mixed_types()
        {
            Database.SetInitializer<FakeForAppConfigNonStringParams>(null);
            FakeInitializerForAppConfigNonStringParams.Reset();

            var config = new AppConfig(CreateEmptyConfig().AddContextConfig(
                typeof(FakeForAppConfigNonStringParams).AssemblyQualifiedName,
                initializerType: typeof(FakeInitializerForAppConfigNonStringParams).AssemblyQualifiedName,
                initializerParameters: new string[] { "99", "MyString" },
                initializerParameterTypes: new string[] { "System.Int32", "System.String" }));

            config.InternalApplyInitializers(force: true);

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForAppConfigNonStringParams>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            Assert.True(FakeInitializerForAppConfigNonStringParams.InitializerCalled);
            Assert.Equal(99, FakeInitializerForAppConfigNonStringParams.Arg1);
            Assert.Equal("MyString", FakeInitializerForAppConfigNonStringParams.Arg2);
        }

        [Fact]
        public void Initializer_throws_with_ctor_params_of_mixed_types_in_wrong_order()
        {
            Database.SetInitializer<FakeForAppConfigNonStringParams>(null);
            FakeInitializerForAppConfigNonStringParams.Reset();

            var config = new AppConfig(CreateEmptyConfig().AddContextConfig(
                typeof(FakeForAppConfigNonStringParams).AssemblyQualifiedName,
                initializerType: typeof(FakeInitializerForAppConfigNonStringParams).AssemblyQualifiedName,
                initializerParameters: new string[] { "MyString", "99" },
                initializerParameterTypes: new string[] { "System.String", "System.Int32" }));

            Assert.Equal(new InvalidOperationException(Strings.Database_InitializeFromConfigFailed(
                typeof(FakeInitializerForAppConfigNonStringParams).AssemblyQualifiedName,
                typeof(FakeForAppConfigNonStringParams).AssemblyQualifiedName)).Message, Assert.Throws<InvalidOperationException>(() => config.InternalApplyInitializers(force: true)).Message);
        }

        public class FakeForAppConfigOverrideNull : DbContextUsingMockInternalContext
        {
        }

        [Fact]
        public void Initializer_applied_from_configuration_will_override_disabled_initializer()
        {
            GenericFakeInitializerForAppConfig<FakeForAppConfigOverrideNull>.Reset();
            Database.SetInitializer<FakeForAppConfigOverrideNull>(null);

            var config = new AppConfig(CreateEmptyConfig().AddContextConfig(
                typeof(FakeForAppConfigOverrideNull).AssemblyQualifiedName,
                typeof(GenericFakeInitializerForAppConfig<FakeForAppConfigOverrideNull>).AssemblyQualifiedName));

            config.InternalApplyInitializers(force: true);

            Database.SetInitializer<FakeForAppConfigOverrideNull>(null);

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForAppConfigOverrideNull>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            Assert.True(GenericFakeInitializerForAppConfig<FakeForAppConfigOverrideNull>.InitializerCalled);
        }

        [Fact]
        public void Initializer_applied_from_legacy_configuration_will_override_disabled_initializer()
        {
            FakeInitializerForLegacyAppConfig.Reset();
            Database.SetInitializer<FakeForLegacyAppConfig>(null);

            var initializerConfig = new LegacyDatabaseInitializerConfig("DatabaseInitializerForType " + typeof(FakeForLegacyAppConfig).AssemblyQualifiedName,
                                                                  typeof(FakeInitializerForLegacyAppConfig).AssemblyQualifiedName);
            initializerConfig.ApplyInitializer();

            Database.SetInitializer<FakeForLegacyAppConfig>(null);

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForLegacyAppConfig>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            Assert.True(FakeInitializerForLegacyAppConfig.InitializerCalled);
        }

        public class FakeForAppConfigOverride : DbContextUsingMockInternalContext
        {
        }

        [Fact]
        public void Initializer_applied_from_configuration_will_override_existing_initializer()
        {
            GenericFakeInitializerForAppConfig<FakeForAppConfigOverride>.Reset();
            Database.SetInitializer(new DropCreateDatabaseAlways<FakeForAppConfigOverride>());

            var config = new AppConfig(CreateEmptyConfig().AddContextConfig(
               typeof(FakeForAppConfigOverride).AssemblyQualifiedName,
               typeof(GenericFakeInitializerForAppConfig<FakeForAppConfigOverride>).AssemblyQualifiedName));

            config.InternalApplyInitializers(force: true);

            Database.SetInitializer(new DropCreateDatabaseAlways<FakeForAppConfigOverride>());

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForAppConfigOverride>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            Assert.True(GenericFakeInitializerForAppConfig<FakeForAppConfigOverride>.InitializerCalled);
        }

        [Fact]
        public void Initializer_applied_from_legacy_configuration_will_override_existing_initializer()
        {
            FakeInitializerForLegacyAppConfig.Reset();
            Database.SetInitializer(new DropCreateDatabaseAlways<FakeForLegacyAppConfig>());

            var initializerConfig = new LegacyDatabaseInitializerConfig("DatabaseInitializerForType " + typeof(FakeForLegacyAppConfig).AssemblyQualifiedName,
                                                                  typeof(FakeInitializerForLegacyAppConfig).AssemblyQualifiedName);
            initializerConfig.ApplyInitializer();

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForLegacyAppConfig>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            Assert.True(FakeInitializerForLegacyAppConfig.InitializerCalled);
        }

        public class FakeForAppConfigDisable : DbContextUsingMockInternalContext
        {
        }

        public void Existing_initializer_can_be_disabled_from_configuration()
        {
            GenericFakeInitializerForAppConfig<FakeForAppConfigDisable>.Reset();

            Database.SetInitializer(new GenericFakeInitializerForAppConfig<FakeForAppConfigDisable>());

            var config = new AppConfig(CreateEmptyConfig().AddContextConfig(
              typeof(FakeForAppConfigDisable).AssemblyQualifiedName,
              isDatabaseInitializationDisabled: true));

            config.InternalApplyInitializers(force: true);

            Database.SetInitializer(new GenericFakeInitializerForAppConfig<FakeForAppConfigDisable>());

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForAppConfigDisable>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            Assert.False(GenericFakeInitializerForAppConfig<FakeForAppConfigDisable>.InitializerCalled);
        }

        public class FakeForAppConfigDisabledConfigured : DbContextUsingMockInternalContext
        {
        }

        [Fact]
        public void Initializer_marked_as_disabled_in_configuration_is_disabled_if_type_is_also_specified_in_configuration()
        {
            GenericFakeInitializerForAppConfig<FakeForAppConfigDisabledConfigured>.Reset();

            var config = new AppConfig(CreateEmptyConfig().AddContextConfig(
              typeof(FakeForAppConfigDisabledConfigured).AssemblyQualifiedName,
              initializerType: typeof(GenericFakeInitializerForAppConfig<FakeForAppConfigDisabledConfigured>).AssemblyQualifiedName,
              isDatabaseInitializationDisabled: true));

            config.InternalApplyInitializers(force: true);

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForAppConfigDisabledConfigured>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            Assert.False(GenericFakeInitializerForAppConfig<FakeForAppConfigDisabledConfigured>.InitializerCalled);
        }

        [Fact]
        public void Existing_initializer_can_be_disabled_from_legacy_configuration_with_Disabled()
        {
            Existing_initializer_can_be_disabled_from_legacy_configuration("Disabled");
        }

        [Fact]
        public void Existing_initializer_can_be_disabled_from_legacy_configuration_with_empty_string()
        {
            Existing_initializer_can_be_disabled_from_legacy_configuration("");
        }

        [Fact]
        public void Existing_initializer_can_be_disabled_from_legacy_configuration_with_whitespace()
        {
            Existing_initializer_can_be_disabled_from_legacy_configuration(" ");
        }

        [Fact]
        public void Existing_initializer_can_be_disabled_from_legacy_configuration_with_null()
        {
            Existing_initializer_can_be_disabled_from_legacy_configuration(null);
        }

        private void Existing_initializer_can_be_disabled_from_legacy_configuration(string configValue)
        {
            FakeInitializerForLegacyAppConfig.Reset();

            Database.SetInitializer(new FakeInitializerForLegacyAppConfig());

            var initializerConfig = new LegacyDatabaseInitializerConfig("DatabaseInitializerForType " + typeof(FakeForLegacyAppConfig).AssemblyQualifiedName,
                                                                    configValue);
            initializerConfig.ApplyInitializer();

            Database.SetInitializer(new FakeInitializerForLegacyAppConfig());

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForLegacyAppConfig>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            Assert.False(FakeInitializerForLegacyAppConfig.InitializerCalled);
        }

        [Fact]
        public void Generic_initializer_for_generic_context_can_be_applied_from_configuration()
        {
            FakeInitializerForAppConfigGeneric<string>.Reset();

            var config = new AppConfig(CreateEmptyConfig().AddContextConfig(
              typeof(FakeForAppConfigGeneric<string>).AssemblyQualifiedName,
              initializerType: typeof(FakeInitializerForAppConfigGeneric<string>).AssemblyQualifiedName));

            config.InternalApplyInitializers(force: true);

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForAppConfigGeneric<string>>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            Assert.True(FakeInitializerForAppConfigGeneric<string>.InitializerCalled);
        }

        [Fact]
        public void Generic_initializer_for_generic_context_can_be_applied_from_legacy_configuration()
        {
            FakeInitializerForAppConfigGeneric<int>.Reset();
            var initializerConfig = new LegacyDatabaseInitializerConfig("DatabaseInitializerForType " + typeof(FakeForAppConfigGeneric<int>).AssemblyQualifiedName,
                                                                  typeof(FakeInitializerForAppConfigGeneric<int>).AssemblyQualifiedName);
            initializerConfig.ApplyInitializer();

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForAppConfigGeneric<int>>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            Assert.True(FakeInitializerForAppConfigGeneric<int>.InitializerCalled);
        }

        [Fact]
        public void Initializer_in_config_throws_when_given_empty_context_type()
        {
            const string contextName = "";
            var initializerName = typeof(FakeInitializerForAppConfig).AssemblyQualifiedName;
            var config = new AppConfig(CreateEmptyConfig().AddContextConfig(contextName, initializerName));

            Assert.Equal(Strings.Database_InitializeFromConfigFailed(initializerName, contextName), Assert.Throws<InvalidOperationException>(() =>
                                                                                                                                                   config.InternalApplyInitializers(force: true)).Message);
        }

        [Fact]
        public void Initializer_in_legacy_config_throws_when_given_empty_context_type()
        {
            const string configKey = "DatabaseInitializerForType ";
            const string configValue = "InitializerType";
            Assert.Equal(Strings.Database_BadLegacyInitializerEntry(configKey, configValue), Assert.Throws<InvalidOperationException>(() =>
                                                                                                                                            new LegacyDatabaseInitializerConfig(configKey, configValue)).Message);
        }

        [Fact]
        public void Initializer_in_config_throws_when_given_context_class_that_cannot_be_found()
        {
            const string contextName = "ANightAtTheOpera";
            var initializerName = typeof(FakeInitializerForAppConfig).AssemblyQualifiedName;
            var config = new AppConfig(CreateEmptyConfig().AddContextConfig(contextName, initializerName));

            Assert.Equal(Strings.Database_InitializeFromConfigFailed(initializerName, contextName), Assert.Throws<InvalidOperationException>(() =>
                                                                                                                                                   config.InternalApplyInitializers(force: true)).Message);
        }

        [Fact]
        public void Initializer_in_legacy_config_throws_when_given_context_class_that_cannot_be_found()
        {
            const string contextName = "ANightAtTheOpera";
            var initializerName = typeof(FakeInitializerForLegacyAppConfig).AssemblyQualifiedName;
            var config = new LegacyDatabaseInitializerConfig("DatabaseInitializerForType " + contextName, initializerName);

            var exception = Assert.Throws<InvalidOperationException>(() => config.ApplyInitializer());
            Assert.Equal(Strings.Database_InitializeFromLegacyConfigFailed(initializerName, contextName), exception.Message);
            Assert.IsType<InvalidOperationException>(exception.InnerException);
            Assert.Equal(Strings.Database_FailedToResolveType(contextName), exception.InnerException.Message);
        }

        [Fact]
        public void Initializer_in_config_throws_when_given_initializer_class_that_cannot_be_found()
        {
            var contextName = typeof(FakeForAppConfig).AssemblyQualifiedName;
            const string initializerName = "ADayAtTheRaces";
            var config = new AppConfig(CreateEmptyConfig().AddContextConfig(contextName, initializerName));

            Assert.Equal(Strings.Database_InitializeFromConfigFailed(initializerName, contextName), Assert.Throws<InvalidOperationException>(() =>
                                                                                                                                                  config.InternalApplyInitializers(force: true)).Message);
        }

        [Fact]
        public void Initializer_in_legacy_config_throws_when_given_initializer_class_that_cannot_be_found()
        {
            var contextName = typeof(FakeForLegacyAppConfig).AssemblyQualifiedName;
            const string initializerName = "ADayAtTheRaces";
            var config = new LegacyDatabaseInitializerConfig("DatabaseInitializerForType " + contextName, initializerName);

            var exception = Assert.Throws<InvalidOperationException>(() => config.ApplyInitializer());
            Assert.Equal(Strings.Database_InitializeFromLegacyConfigFailed(initializerName, contextName), exception.Message);
            Assert.IsType<InvalidOperationException>(exception.InnerException);
            Assert.Equal(Strings.Database_FailedToResolveType(initializerName), exception.InnerException.Message);
        }

        [Fact]
        public void Initializer_in_config_throws_when_given_non_DbContext_type()
        {
            var contextName = typeof(String).AssemblyQualifiedName;
            var initializerName = typeof(FakeInitializerForAppConfig).AssemblyQualifiedName;
            var config = new AppConfig(CreateEmptyConfig().AddContextConfig(contextName, initializerName));

            Assert.Equal(Strings.Database_InitializeFromConfigFailed(initializerName, contextName), Assert.Throws<InvalidOperationException>(() =>
                                                                                                                                                   config.InternalApplyInitializers(force: true)).Message);
        }

        [Fact]
        public void Initializer_in_legacy_config_throws_when_given_non_DbContext_type()
        {
            var contextName = typeof(String).AssemblyQualifiedName;
            var initializerName = typeof(FakeInitializerForLegacyAppConfig).AssemblyQualifiedName;
            var config = new LegacyDatabaseInitializerConfig("DatabaseInitializerForType " + contextName, initializerName);

            Assert.Equal(Strings.Database_InitializeFromLegacyConfigFailed(initializerName, contextName), Assert.Throws<InvalidOperationException>(() => config.ApplyInitializer()).Message);
        }

        [Fact]
        public void Initializer_in_config_throws_when_given_non_DatabaseInitializer_type()
        {
            var contextName = typeof(FakeForAppConfig).AssemblyQualifiedName;
            var initializerName = typeof(String).AssemblyQualifiedName;
            var config = new AppConfig(CreateEmptyConfig().AddContextConfig(contextName, initializerName));

            Assert.Equal(Strings.Database_InitializeFromConfigFailed(initializerName, contextName), Assert.Throws<InvalidOperationException>(() =>
                                                                                                                                                   config.InternalApplyInitializers(force: true)).Message);
        }

        [Fact]
        public void Initializer_in_legacy_config_throws_when_given_non_DatabaseInitializer_type()
        {
            var contextName = typeof(FakeForLegacyAppConfig).AssemblyQualifiedName;
            var initializerName = typeof(String).AssemblyQualifiedName;
            var config = new LegacyDatabaseInitializerConfig("DatabaseInitializerForType " + contextName, initializerName);

            Assert.Equal(Strings.Database_InitializeFromLegacyConfigFailed(initializerName, contextName), Assert.Throws<InvalidOperationException>(() => config.ApplyInitializer()).Message);
        }

        [Fact]
        public void Initializer_in_config_throws_when_given_DatabaseInitializer_type_for_wrong_context_type()
        {
            var contextName = typeof(FakeForAppConfig).AssemblyQualifiedName;
            var initializerName = typeof(FakeInitializerForLegacyAppConfig).AssemblyQualifiedName;
            var config = new AppConfig(CreateEmptyConfig().AddContextConfig(contextName, initializerName));

            Assert.Equal(Strings.Database_InitializeFromConfigFailed(initializerName, contextName), Assert.Throws<InvalidOperationException>(() =>
                                                                                                                                                   config.InternalApplyInitializers(force: true)).Message);
        }

        [Fact]
        public void Initializer_in_legacy_config_throws_when_given_DatabaseInitializer_type_for_wrong_context_type()
        {
            var contextName = typeof(FakeForLegacyAppConfig).AssemblyQualifiedName;
            var initializerName = typeof(FakeInitializerForAppConfig).AssemblyQualifiedName;
            var config = new LegacyDatabaseInitializerConfig("DatabaseInitializerForType " + contextName, initializerName);

            Assert.Equal(Strings.Database_InitializeFromLegacyConfigFailed(initializerName, contextName), Assert.Throws<InvalidOperationException>(() => config.ApplyInitializer()).Message);
        }

        [Fact]
        public void Initializer_in_legacy_config_throws_when_given_DatabaseInitializer_type_without_parameterless_constructor()
        {
            var contextName = typeof(FakeForLegacyAppConfig).AssemblyQualifiedName;
            var initializerName = typeof(FakeInitializerWithMissingConstructor).AssemblyQualifiedName;
            var config = new LegacyDatabaseInitializerConfig("DatabaseInitializerForType " + contextName, initializerName);

            Assert.Equal(Strings.Database_InitializeFromLegacyConfigFailed(initializerName, contextName), Assert.Throws<InvalidOperationException>(() => config.ApplyInitializer()).Message);
        }

        [Fact]
        public void Initializer_in_config_throws_when_given_context_class_without_assembly_name()
        {
            var contextName = typeof(FakeForAppConfig).FullName;
            var initializerName = typeof(FakeInitializerForAppConfig).AssemblyQualifiedName;
            var config = new AppConfig(CreateEmptyConfig().AddContextConfig(contextName, initializerName));

            Assert.Equal(Strings.Database_InitializeFromConfigFailed(initializerName, contextName), Assert.Throws<InvalidOperationException>(() =>
                                                                                                                                                   config.InternalApplyInitializers(force: true)).Message);
        }

        // See Dev11 bug 109236
        [Fact]
        public void Initializer_in_legacy_config_throws_when_given_context_class_without_assembly_name()
        {
            var contextName = typeof(FakeForLegacyAppConfig).FullName;
            var initializerName = typeof(FakeInitializerForLegacyAppConfig).AssemblyQualifiedName;
            var config = new LegacyDatabaseInitializerConfig("DatabaseInitializerForType " + contextName, initializerName);

            var exception = Assert.Throws<InvalidOperationException>(() => config.ApplyInitializer());
            Assert.Equal(Strings.Database_InitializeFromLegacyConfigFailed(initializerName, contextName), exception.Message);
            Assert.IsType<InvalidOperationException>(exception.InnerException);
            Assert.Equal(Strings.Database_FailedToResolveType(contextName), exception.InnerException.Message);
        }

        [Fact]
        public void Initializer_in_config_throws_when_given_initializer_class_without_assembly_name()
        {
            var contextName = typeof(FakeForAppConfig).AssemblyQualifiedName;
            var initializerName = typeof(FakeInitializerForAppConfig).FullName;
            var config = new AppConfig(CreateEmptyConfig().AddContextConfig(contextName, initializerName));

            Assert.Equal(Strings.Database_InitializeFromConfigFailed(initializerName, contextName), Assert.Throws<InvalidOperationException>(() =>
                                                                                                                                                   config.InternalApplyInitializers(force: true)).Message);
        }

        // See Dev11 bug 109236
        [Fact]
        public void Initializer_in_legacy_config_throws_when_given_initializer_class_without_assembly_name()
        {
            var contextName = typeof(FakeForLegacyAppConfig).AssemblyQualifiedName;
            var initializerName = typeof(FakeInitializerForLegacyAppConfig).FullName;
            var config = new LegacyDatabaseInitializerConfig("DatabaseInitializerForType " + contextName, initializerName);

            var exception = Assert.Throws<InvalidOperationException>(() => config.ApplyInitializer());
            Assert.Equal(Strings.Database_InitializeFromLegacyConfigFailed(initializerName, contextName), exception.Message);
            Assert.IsType<InvalidOperationException>(exception.InnerException);
            Assert.Equal(Strings.Database_FailedToResolveType(initializerName), exception.InnerException.Message);
        }

        #endregion
    }

    #region Fake connection factories

    public abstract class FakeBaseConnectionFactory : IDbConnectionFactory
    {
        public DbConnection CreateConnection(string nameOrConnectionString)
        {
            throw new NotImplementedException();
        }
    }

    public class FakeConnectionFactoryNoParams : FakeBaseConnectionFactory
    {
    }

    public class FakeConnectionFactoryOneParam : FakeBaseConnectionFactory
    {
        public string Arg { get; set; }

        public FakeConnectionFactoryOneParam(string arg)
        {
            Arg = arg;
        }
    }

    public class FakeConnectionFactoryManyParams : FakeBaseConnectionFactory
    {
        public List<string> Args { get; set; }

        public FakeConnectionFactoryManyParams(string arg0, string arg1, string arg2, string arg3, string arg4,
                                               string arg5, string arg6, string arg7, string arg8, string arg9,
                                               string arg10)
        {
            Args = new List<string> { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10 };
        }
    }

    public class FakeNonConnectionFactory
    {
    }

    #endregion

    #region Fake Initializers
    public enum FakeInitializerCtor
    {
        NoArgs,
        OneArg,
        TwoArgs
    }

    public class GenericFakeInitializerForAppConfig<T> : IDatabaseInitializer<T> where T : DbContext
    {
        public GenericFakeInitializerForAppConfig()
        {
            Reset();
            CtorCalled = FakeInitializerCtor.NoArgs;
        }

        public GenericFakeInitializerForAppConfig(string arg1)
        {
            Reset();
            CtorCalled = FakeInitializerCtor.OneArg;
            Arg1 = arg1;
        }

        public GenericFakeInitializerForAppConfig(string arg1, string arg2)
        {
            Reset();
            CtorCalled = FakeInitializerCtor.TwoArgs;
            Arg1 = arg1;
            Arg2 = arg2;
        }

        public static void Reset()
        {
            InitializerCalled = false;
            CtorCalled = null;
            Arg1 = null;
            Arg2 = null;
        }

        public void InitializeDatabase(T context)
        {
            InitializerCalled = true;
        }

        public static bool InitializerCalled { get; set; }

        public static FakeInitializerCtor? CtorCalled { get; set; }

        public static string Arg1 { get; set; }

        public static string Arg2 { get; set; }
    }

    /// <summary>
    /// Don't use this for LegacyDatabaseInitializerConfig or InternalApplyInitializers(force: true) tests.
    /// </summary>
    public class FakeForAppConfig : DbContextUsingMockInternalContext
    {
    }

    public class FakeInitializerForAppConfig : IDatabaseInitializer<FakeForAppConfig>
    {
        public static bool InitializerCalled { get; set; }

        public static void Reset()
        {
            InitializerCalled = false;
        }

        public void InitializeDatabase(FakeForAppConfig context)
        {
            InitializerCalled = true;
        }
    }

    /// <summary>
    /// Only use this for LegacyDatabaseInitializerConfig tests.
    /// </summary>
    public class FakeForLegacyAppConfig : DbContextUsingMockInternalContext
    {
    }

    public class FakeInitializerForLegacyAppConfig : IDatabaseInitializer<FakeForLegacyAppConfig>
    {
        public FakeInitializerForLegacyAppConfig()
        {
            Reset();
            CtorCalled = FakeInitializerCtor.NoArgs;
        }

        public FakeInitializerForLegacyAppConfig(string arg1)
        {
            Reset();
            CtorCalled = FakeInitializerCtor.OneArg;
            Arg1 = arg1;
        }

        public FakeInitializerForLegacyAppConfig(string arg1, string arg2)
        {
            Reset();
            CtorCalled = FakeInitializerCtor.TwoArgs;
            Arg1 = arg1;
            Arg2 = arg2;
        }

        public static void Reset()
        {
            InitializerCalled = false;
            CtorCalled = null;
            Arg1 = null;
            Arg2 = null;
        }

        public void InitializeDatabase(FakeForLegacyAppConfig context)
        {
            InitializerCalled = true;
        }

        public static bool InitializerCalled { get; set; }

        public static FakeInitializerCtor? CtorCalled { get; set; }

        public static string Arg1 { get; set; }

        public static string Arg2 { get; set; }
    }

    public class FakeForAppConfigNonStringParams : DbContextUsingMockInternalContext
    {
    }

    public class FakeInitializerForAppConfigNonStringParams : IDatabaseInitializer<FakeForAppConfigNonStringParams>
    {
        public FakeInitializerForAppConfigNonStringParams(int arg1)
        {
            Arg1 = arg1;
        }

        public FakeInitializerForAppConfigNonStringParams(int arg1, string arg2)
        {
            Arg1 = arg1;
            Arg2 = arg2;
        }

        public void InitializeDatabase(FakeForAppConfigNonStringParams context)
        {
            InitializerCalled = true;
        }

        public static bool InitializerCalled { get; set; }

        public static int? Arg1 { get; set; }

        public static string Arg2 { get; set; }

        public static void Reset()
        {
            InitializerCalled = false;
            Arg1 = null;
            Arg2 = null;
        }
    }

    public class FakeForAppConfigGeneric<T> : DbContextUsingMockInternalContext
    {
    }

    public class FakeInitializerForAppConfigGeneric<T> : IDatabaseInitializer<FakeForAppConfigGeneric<T>>
    {
        public void InitializeDatabase(FakeForAppConfigGeneric<T> context)
        {
            InitializerCalled = true;
        }

        public static bool InitializerCalled { get; set; }

        public static void Reset()
        {
            InitializerCalled = false;
        }
    }

    public class FakeInitializerWithMissingConstructor : IDatabaseInitializer<FakeForLegacyAppConfig>
    {
        public FakeInitializerWithMissingConstructor(int _)
        {
        }

        public void InitializeDatabase(FakeForLegacyAppConfig context)
        {
        }
    }

    #endregion
}
