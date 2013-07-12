// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping.ViewGeneration;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Pluralization;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.TestHelpers;
    using System.Linq;
    using Moq;
    using Xunit;

    public class DbConfigurationTests
    {
        public class SetConfiguration
        {
            [Fact]
            public void DbConfiguration_cannot_be_set_to_null()
            {
                Assert.Equal(
                    "configuration",
                    Assert.Throws<ArgumentNullException>(() => DbConfiguration.SetConfiguration(null)).ParamName);
            }
        }

        public class AddDependencyResolver
        {
            [Fact]
            public void AddDependencyResolver_throws_if_given_a_null_resolver()
            {
                Assert.Equal(
                    "resolver",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddDependencyResolver(null)).ParamName);
            }

            [Fact]
            public void AddDependencyResolver_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddDependencyResolver"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddDependencyResolver(new Mock<IDbDependencyResolver>().Object)).Message);
            }

            [Fact]
            public void AddDependencyResolver_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var resolver = new Mock<IDbDependencyResolver>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).AddDependencyResolver(resolver);

                mockInternalConfiguration.Verify(m => m.AddDependencyResolver(resolver, false));
            }
        }

        public class AddSecondaryResolver
        {
            [Fact]
            public void AddSecondaryResolver_throws_if_given_a_null_resolver()
            {
                Assert.Equal(
                    "resolver",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddSecondaryResolver(null)).ParamName);
            }

            [Fact]
            public void AddSecondaryResolver_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddSecondaryResolver"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddSecondaryResolver(new Mock<IDbDependencyResolver>().Object)).Message);
            }

            [Fact]
            public void AddSecondaryResolver_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var resolver = new Mock<IDbDependencyResolver>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).AddSecondaryResolver(resolver);

                mockInternalConfiguration.Verify(m => m.AddSecondaryResolver(resolver));
            }
        }

        public class OnLockingConfiguration
        {
            [Fact]
            public void OnLockingConfiguration_throws_when_attempting_to_add_or_remove_a_null_handler()
            {
                Assert.Equal(
                    "value",
                    Assert.Throws<ArgumentNullException>(() => DbConfiguration.OnLockingConfiguration += null).ParamName);

                Assert.Equal(
                    "value",
                    Assert.Throws<ArgumentNullException>(() => DbConfiguration.OnLockingConfiguration -= null).ParamName);
            }
        }

        public class AddDbProviderServices
        {
            [Fact]
            public void AddDbProviderServices_throws_if_given_a_null_provider_or_bad_invariant_name()
            {
                Assert.Equal(
                    "provider",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddDbProviderServices("Karl", null)).ParamName);

                Assert.Equal(
                    "provider",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddDbProviderServices(null)).ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddDbProviderServices(null, new Mock<DbProviderServices>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddDbProviderServices("", new Mock<DbProviderServices>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddDbProviderServices(" ", new Mock<DbProviderServices>().Object)).Message);
            }

            [Fact]
            public void AddDbProviderServices_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var providerServices = new Mock<DbProviderServices>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).AddDbProviderServices("900.FTW", providerServices);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(providerServices, "900.FTW"));
            }

            [Fact]
            public void AddDbProviderServices_also_adds_the_provider_as_a_secondary_resolver()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var providerServices = new Mock<DbProviderServices>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).AddDbProviderServices("900.FTW", providerServices);

                mockInternalConfiguration.Verify(m => m.AddSecondaryResolver(providerServices));
            }

            [Fact]
            public void AddDbProviderServices_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddDbProviderServices"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddDbProviderServices("Karl", new Mock<DbProviderServices>().Object)).Message);
            }

            [Fact]
            public void AddDbProviderServices_throws_if_no_ProviderInvariantNameAttribute()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var providerServices = new Mock<DbProviderServices>().Object;

                var configuration = new DbConfiguration(mockInternalConfiguration.Object);

                Assert.Equal(
                    Strings.DbProviderNameAttributeNotFound(providerServices.GetType()),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddDbProviderServices(providerServices)).Message);
            }
        }

        public class AddDbProviderFactory
        {
            [Fact]
            public void AddDbProviderFactory_throws_if_given_a_null_provider_or_bad_invariant_name()
            {
                Assert.Equal(
                    "providerFactory",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddDbProviderFactory("Karl", null)).ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddDbProviderFactory(null, new Mock<DbProviderFactory>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddDbProviderFactory("", new Mock<DbProviderFactory>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddDbProviderFactory(" ", new Mock<DbProviderFactory>().Object)).Message);
            }

            [Fact]
            public void AddDbProviderFactory_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var providerFactory = new Mock<DbProviderFactory>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).AddDbProviderFactory("920.FTW", providerFactory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(providerFactory, "920.FTW"));
                mockInternalConfiguration.Verify(m => m.AddDependencyResolver(new InvariantNameResolver(providerFactory, "920.FTW"), false));
            }

            [Fact]
            public void AddDbProviderFactory_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddDbProviderFactory"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddDbProviderFactory("Karl", new Mock<DbProviderFactory>().Object)).Message);
            }
        }

        public class AddExecutionStrategy
        {
            [Fact]
            public void Throws_if_given_a_null_server_name_or_bad_invariant_name()
            {
                Assert.Equal(
                    "getExecutionStrategy",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddExecutionStrategy<IDbExecutionStrategy>(null))
                        .ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("serverName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy(() => new Mock<IDbExecutionStrategy>().Object, null)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("serverName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy(() => new Mock<IDbExecutionStrategy>().Object, "")).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("serverName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy(() => new Mock<IDbExecutionStrategy>().Object, " ")).Message);
                Assert.Equal(
                    "getExecutionStrategy",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddExecutionStrategy<IDbExecutionStrategy>(null, "a"))
                        .ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy(null, () => new Mock<IDbExecutionStrategy>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy("", () => new Mock<IDbExecutionStrategy>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy(" ", () => new Mock<IDbExecutionStrategy>().Object)).Message);
                Assert.Equal(
                    "getExecutionStrategy",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddExecutionStrategy("a", null)).ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy(null, () => new Mock<IDbExecutionStrategy>().Object, "a")).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy("", () => new Mock<IDbExecutionStrategy>().Object, "a")).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy(" ", () => new Mock<IDbExecutionStrategy>().Object, "a")).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("serverName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy("a", () => new Mock<IDbExecutionStrategy>().Object, null)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("serverName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy("a", () => new Mock<IDbExecutionStrategy>().Object, "")).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("serverName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy("a", () => new Mock<IDbExecutionStrategy>().Object, " ")).Message);
                Assert.Equal(
                    "getExecutionStrategy",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddExecutionStrategy("a", null, "b")).ParamName);
            }

            [Fact]
            public void Throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddExecutionStrategy"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddExecutionStrategy("a", () => new Mock<IDbExecutionStrategy>().Object, "b")).Message);
            }

            [Fact]
            public void Delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var executionStrategy = new Func<IDbExecutionStrategy>(() => new Mock<IDbExecutionStrategy>().Object);

                new DbConfiguration(mockInternalConfiguration.Object).AddExecutionStrategy("a", executionStrategy, "b");

                mockInternalConfiguration.Verify(
                    m => m.AddDependencyResolver(It.IsAny<ExecutionStrategyResolver<IDbExecutionStrategy>>(), false));
            }

            [Fact]
            public void Throws_if_no_ProviderInvariantNameAttribute()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var executionStrategy = new Func<IDbExecutionStrategy>(() => new Mock<IDbExecutionStrategy>().Object);

                var configuration = new DbConfiguration(mockInternalConfiguration.Object);

                Assert.Equal(
                    Strings.DbProviderNameAttributeNotFound(typeof(IDbExecutionStrategy).FullName),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddExecutionStrategy(executionStrategy, "a")).Message);
            }
        }

        public class SetDefaultConnectionFactory : TestBase
        {
            [Fact]
            public void Setting_DefaultConnectionFactory_throws_if_given_a_null_factory()
            {
                Assert.Equal(
                    "connectionFactory",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetDefaultConnectionFactory(null)).ParamName);
            }

            [Fact]
            public void Setting_DefaultConnectionFactory_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetDefaultConnectionFactory"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetDefaultConnectionFactory(new Mock<IDbConnectionFactory>().Object)).Message);
            }

            [Fact]
            public void SetDefaultConnectionFactory_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var connectionFactory = new Mock<IDbConnectionFactory>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetDefaultConnectionFactory(connectionFactory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(connectionFactory));
            }

            [Fact]
            public void DefaultConnectionFactory_set_in_code_can_be_overriden_before_config_is_locked()
            {
                Assert.IsType<SqlConnectionFactory>(DbConfiguration.GetService<IDbConnectionFactory>());
                Assert.IsType<DefaultUnitTestsConnectionFactory>(FunctionalTestsConfiguration.OriginalConnectionFactories[0]);
            }
        }

        public class SetPluralizationService
        {
            [Fact]
            public void Setting_PluralizationService_throws_if_given_a_null_service()
            {
                Assert.Equal(
                    "pluralizationService",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetPluralizationService(null)).ParamName);
            }

            [Fact]
            public void Setting_PluralizationService_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetPluralizationService"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetPluralizationService(new Mock<IPluralizationService>().Object)).Message);
            }

            [Fact]
            public void SetPluralizationService_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var pluralizationService = new Mock<IPluralizationService>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetPluralizationService(pluralizationService);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(pluralizationService));
            }
        }

        public class DependencyResolver
        {
            [Fact]
            public void Default_IDbModelCacheKeyFactory_is_returned_by_default()
            {
                Assert.IsType<DefaultModelCacheKeyFactory>(DbConfiguration.GetService<IDbModelCacheKeyFactory>());
            }
        }

        public class SetDatabaseInitializer
        {
            [Fact]
            public void SetDatabaseInitializer_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetDatabaseInitializer"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetDatabaseInitializer(new Mock<IDatabaseInitializer<DbContext>>().Object)).Message);
            }

            [Fact]
            public void SetDatabaseInitializer_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var initializer = new Mock<IDatabaseInitializer<DbContext>>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetDatabaseInitializer(initializer);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(initializer));
            }

            [Fact]
            public void SetDatabaseInitializer_creates_null_initializer_when_given_null_argument()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);

                new DbConfiguration(mockInternalConfiguration.Object).SetDatabaseInitializer<DbContext>(null);

                mockInternalConfiguration.Verify(
                    m => m.RegisterSingleton<IDatabaseInitializer<DbContext>>(It.IsAny<NullDatabaseInitializer<DbContext>>()));
            }
        }

        public class AddMigrationSqlGenerator
        {
            [Fact]
            public void AddMigrationSqlGenerator_throws_if_given_a_null_generator_or_bad_invariant_name()
            {
                Assert.Equal(
                    "sqlGenerator",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddMigrationSqlGenerator("Karl", null)).ParamName);

                Assert.Equal(
                    "sqlGenerator",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddMigrationSqlGenerator<MigrationSqlGenerator>(null))
                        .ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddMigrationSqlGenerator(null, () => new Mock<MigrationSqlGenerator>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddMigrationSqlGenerator("", () => new Mock<MigrationSqlGenerator>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddMigrationSqlGenerator(" ", () => new Mock<MigrationSqlGenerator>().Object)).Message);
            }

            [Fact]
            public void AddMigrationSqlGenerator_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddMigrationSqlGenerator"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddMigrationSqlGenerator("Karl", () => new Mock<MigrationSqlGenerator>().Object)).Message);
            }

            [Fact]
            public void AddMigrationSqlGenerator_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var generator = new Func<MigrationSqlGenerator>(() => new Mock<MigrationSqlGenerator>().Object);

                new DbConfiguration(mockInternalConfiguration.Object).AddMigrationSqlGenerator("Karl", generator);

                mockInternalConfiguration.Verify(
                    m => m.AddDependencyResolver(It.IsAny<TransientDependencyResolver<MigrationSqlGenerator>>(), false));
            }

            [Fact]
            public void AddMigrationSqlGenerator_throws_if_no_ProviderInvariantNameAttribute()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var generator = new Func<MigrationSqlGenerator>(() => new Mock<MigrationSqlGenerator>().Object);

                var configuration = new DbConfiguration(mockInternalConfiguration.Object);

                Assert.Equal(
                    Strings.DbProviderNameAttributeNotFound(typeof(MigrationSqlGenerator)),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddMigrationSqlGenerator(generator)).Message);
            }
        }

        public class SetManifestTokenService
        {
            [Fact]
            public void SetManifestTokenService_throws_if_given_a_null_service()
            {
                Assert.Equal(
                    "service",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetManifestTokenService(null)).ParamName);
            }

            [Fact]
            public void SetManifestTokenService_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetManifestTokenService"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetManifestTokenService(new Mock<IManifestTokenService>().Object)).Message);
            }

            [Fact]
            public void SetManifestTokenService_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var service = new Mock<IManifestTokenService>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetManifestTokenService(service);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(service));
            }
        }

        public class SetProviderFactoryService
        {
            [Fact]
            public void SetProviderFactoryService_throws_if_given_a_null_service()
            {
                Assert.Equal(
                    "providerFactoryService",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetProviderFactoryService(null)).ParamName);
            }

            [Fact]
            public void SetProviderFactoryService_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetProviderFactoryService"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetProviderFactoryService(new Mock<IDbProviderFactoryService>().Object)).Message);
            }

            [Fact]
            public void SetProviderFactoryService_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var service = new Mock<IDbProviderFactoryService>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetProviderFactoryService(service);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(service));
            }
        }

        public class SetModelCacheKeyFactory
        {
            [Fact]
            public void SetModelCacheKeyFactory_throws_if_given_a_null_factory()
            {
                Assert.Equal(
                    "keyFactory",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetModelCacheKeyFactory(null)).ParamName);
            }

            [Fact]
            public void SetModelCacheKeyFactory_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetModelCacheKeyFactory"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetModelCacheKeyFactory(new Mock<IDbModelCacheKeyFactory>().Object)).Message);
            }

            [Fact]
            public void SetModelCacheKeyFactory_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var factory = new Mock<IDbModelCacheKeyFactory>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetModelCacheKeyFactory(factory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(factory));
            }
        }

        public class AddHistoryContextFactory
        {
            [Fact]
            public void Throws_if_given_a_null_provider()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddHistoryContextFactory(null, null)).Message);
            }

            [Fact]
            public void Throws_if_given_a_null_factory()
            {
                Assert.Equal(
                    "historyContextFactory",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbConfiguration().AddHistoryContextFactory("Foo", null)).ParamName);
            }

            [Fact]
            public void Throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddHistoryContextFactory"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddHistoryContextFactory("Foo", (e, d) => null)).Message);
            }

            [Fact]
            public void Delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                HistoryContextFactory factory = (e, d) => null;

                new DbConfiguration(mockInternalConfiguration.Object)
                    .AddHistoryContextFactory("Foo", factory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(factory, "Foo"));
            }
        }

        public class SetDefaultSpatialProvider
        {
            [Fact]
            public void SetDefaultSpatialProvider_throws_if_given_a_null_factory()
            {
                Assert.Equal(
                    "spatialProvider",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetDefaultDbSpatialServices(null)).ParamName);
            }

            [Fact]
            public void SetDefaultSpatialProvider_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddDbSpatialServices"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetDefaultDbSpatialServices(new Mock<DbSpatialServices>().Object)).Message);
            }

            [Fact]
            public void SetDefaultSpatialProvider_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var provider = new Mock<DbSpatialServices>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetDefaultDbSpatialServices(provider);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(provider));
            }
        }

        public class SetSpatialProvider
        {
            [Fact]
            public void SetSpatialProvider_throws_if_given_a_null_factory_or_key_or_an_empty_invariant_name()
            {
                Assert.Equal(
                    "spatialProvider",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddDbSpatialServices(null)).ParamName);

                Assert.Equal(
                    "spatialProvider",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddDbSpatialServices("Good.As.Gold", null)).ParamName);

                Assert.Equal(
                    "spatialProvider",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbConfiguration().AddDbSpatialServices(new DbProviderInfo("Especially.For.You", ""), null)).ParamName);

                Assert.Equal(
                    "key",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbConfiguration().AddDbSpatialServices((DbProviderInfo)null, new Mock<DbSpatialServices>().Object))
                        .ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddDbSpatialServices((string)null, new Mock<DbSpatialServices>().Object)).Message);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddDbSpatialServices("", new Mock<DbSpatialServices>().Object)).Message);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddDbSpatialServices(" ", new Mock<DbSpatialServices>().Object)).Message);
            }

            [Fact]
            public void SetSpatialProvider_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddDbSpatialServices"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddDbSpatialServices(SqlSpatialServices.Instance)).Message);
            }

            [Fact]
            public void SetSpatialProvider_with_invariant_name_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddDbSpatialServices"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddDbSpatialServices("Song.For.Whoever", new Mock<DbSpatialServices>().Object)).Message);
            }

            [Fact]
            public void SetSpatialProvider_with_key_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddDbSpatialServices"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddDbSpatialServices(
                            new DbProviderInfo("Song.For.Whoever", ""),
                            new Mock<DbSpatialServices>().Object)).Message);
            }

            [DbProviderName("My.Book")]
            [DbProviderName("My.Nook")]
            private class FakeSqlSpatialServices : SqlSpatialServices
            {
            }

            [Fact]
            public void SetSpatialProvider_delegates_to_internal_configuration_for_all_invariant_name_attributes()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var provider = new FakeSqlSpatialServices();
                var keyPredicates = new List<Func<object, bool>>();
                mockInternalConfiguration.Setup(
                    m => m.RegisterSingleton<DbSpatialServices>(
                        provider,
                        It.IsAny<Func<object, bool>>())).Callback<DbSpatialServices, Func<object, bool>>((s, k) => keyPredicates.Add(k));

                new DbConfiguration(mockInternalConfiguration.Object).AddDbSpatialServices(provider);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton<DbSpatialServices>(provider, It.IsAny<Func<object, bool>>()));

                Assert.Equal(2, keyPredicates.Count);
                Assert.Equal(1, keyPredicates.Count(k => k(new DbProviderInfo("My.Book", "Foo"))));
                Assert.Equal(1, keyPredicates.Count(k => k(new DbProviderInfo("My.Nook", "Foo"))));

                Assert.False(keyPredicates.Any(k => k(new DbProviderInfo("System.Data.SqlClient", "Foo"))));
                Assert.False(keyPredicates.Any(k => k("System.Data.SqlClient")));
                Assert.False(keyPredicates.Any(k => k(null)));
            }

            [Fact]
            public void SetSpatialProvider_with_invariant_name_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var provider = SqlSpatialServices.Instance;
                Func<object, bool> keyPredicate = null;
                mockInternalConfiguration.Setup(
                    m => m.RegisterSingleton<DbSpatialServices>(
                        provider,
                        It.IsAny<Func<object, bool>>())).Callback<DbSpatialServices, Func<object, bool>>((s, k) => { keyPredicate = k; });

                new DbConfiguration(mockInternalConfiguration.Object).AddDbSpatialServices("Mini.Tattoo", provider);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton<DbSpatialServices>(provider, It.IsAny<Func<object, bool>>()));

                Assert.True(keyPredicate(new DbProviderInfo("Mini.Tattoo", "Foo")));
                Assert.False(keyPredicate(new DbProviderInfo("Maxi.Tattoo", "Foo")));
                Assert.False(keyPredicate("Mini.Tattoo"));
                Assert.False(keyPredicate(null));
            }

            [Fact]
            public void SetSpatialProvider_with_key_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var provider = SqlSpatialServices.Instance;

                var key = new DbProviderInfo("A.Little.Time", "Paul");
                new DbConfiguration(mockInternalConfiguration.Object).AddDbSpatialServices(key, provider);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton<DbSpatialServices>(provider, key));
            }

            [Fact]
            public void SetSpatialProvider_throws_if_no_ProviderInvariantNameAttribute()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var spatialServices = new Mock<DbSpatialServices>().Object;

                var configuration = new DbConfiguration(mockInternalConfiguration.Object);

                Assert.Equal(
                    Strings.DbProviderNameAttributeNotFound(spatialServices.GetType()),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddDbSpatialServices(spatialServices)).Message);
            }
        }

        public class SetCommandLogger
        {
            [Fact]
            public void Throws_if_given_a_null_factory()
            {
                Assert.Equal(
                    "commandLoggerFactory",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbConfiguration().SetCommandLogger(null)).ParamName);
            }

            [Fact]
            public void Throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetCommandLogger"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetCommandLogger((_, __) => null)).Message);
            }

            [Fact]
            public void Delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                DbCommandLoggerFactory factory = (_, __) => null;

                new DbConfiguration(mockInternalConfiguration.Object).SetCommandLogger(factory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(factory));
            }
        }

        public class AddInterceptor
        {
            [Fact]
            public void Throws_if_given_a_null_interceptor()
            {
                Assert.Equal(
                    "interceptor",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddInterceptor(null)).ParamName);
            }

            [Fact]
            public void Throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddInterceptor"),
                    Assert.Throws<InvalidOperationException>(() => configuration.AddInterceptor(new Mock<IDbInterceptor>().Object)).Message);
            }

            [Fact]
            public void Delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
                var interceptor = new Mock<IDbInterceptor>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).AddInterceptor(interceptor);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(interceptor));
            }
        }

        private static DbConfiguration CreatedLockedConfiguration()
        {
            var configuration = new DbConfiguration();
            configuration.InternalConfiguration.Lock();
            return configuration;
        }
    }
}
