// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Pluralization;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.TestHelpers;
    using System.Reflection;
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

        public class LoadConfiguration
        {
            [Fact]
            public void LoadConfiguration_throws_for_invalid_arguments()
            {
                Assert.Equal(
                    "assemblyHint",
                    Assert.Throws<ArgumentNullException>(() => DbConfiguration.LoadConfiguration((Assembly)null)).ParamName);

                Assert.Equal(
                    "contextType",
                    Assert.Throws<ArgumentNullException>(() => DbConfiguration.LoadConfiguration((Type)null)).ParamName);

                Assert.Equal(
                    Strings.BadContextTypeForDiscovery("Random"),
                    Assert.Throws<ArgumentException>(() => DbConfiguration.LoadConfiguration(typeof(Random))).Message);
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
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
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
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var resolver = new Mock<IDbDependencyResolver>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).AddSecondaryResolver(resolver);

                mockInternalConfiguration.Verify(m => m.AddSecondaryResolver(resolver));
            }
        }

        public class Loaded
        {
            [Fact]
            public void Loaded_throws_when_attempting_to_add_or_remove_a_null_handler()
            {
                Assert.Equal(
                    "value",
                    Assert.Throws<ArgumentNullException>(() => DbConfiguration.Loaded += null).ParamName);

                Assert.Equal(
                    "value",
                    Assert.Throws<ArgumentNullException>(() => DbConfiguration.Loaded -= null).ParamName);
            }
        }

        public class ProviderServices
        {
            [Fact]
            public void ProviderServices_throws_if_given_a_null_provider_or_bad_invariant_name()
            {
                Assert.Equal(
                    "provider",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().ProviderServices("Karl", null)).ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().ProviderServices(null, new Mock<DbProviderServices>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().ProviderServices("", new Mock<DbProviderServices>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().ProviderServices(" ", new Mock<DbProviderServices>().Object)).Message);
            }

            [Fact]
            public void ProviderServices_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var providerServices = new Mock<DbProviderServices>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).ProviderServices("900.FTW", providerServices);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(providerServices, "900.FTW"));
            }

            [Fact]
            public void ProviderServices_also_adds_the_provider_as_a_secondary_resolver()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var providerServices = new Mock<DbProviderServices>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).ProviderServices("900.FTW", providerServices);

                mockInternalConfiguration.Verify(m => m.AddSecondaryResolver(providerServices));
            }

            [Fact]
            public void ProviderServices_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("ProviderServices"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.ProviderServices("Karl", new Mock<DbProviderServices>().Object)).Message);
            }
        }

        public class ProviderFactory
        {
            [Fact]
            public void ProviderFactory_throws_if_given_a_null_provider_or_bad_invariant_name()
            {
                Assert.Equal(
                    "providerFactory",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().ProviderFactory("Karl", null)).ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().ProviderFactory(null, new Mock<DbProviderFactory>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().ProviderFactory("", new Mock<DbProviderFactory>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().ProviderFactory(" ", new Mock<DbProviderFactory>().Object)).Message);
            }

            [Fact]
            public void ProviderFactory_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var providerFactory = new Mock<DbProviderFactory>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).ProviderFactory("920.FTW", providerFactory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(providerFactory, "920.FTW"));
                mockInternalConfiguration.Verify(m => m.AddDependencyResolver(new InvariantNameResolver(providerFactory, "920.FTW"), false));
            }

            [Fact]
            public void ProviderFactory_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("ProviderFactory"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.ProviderFactory("Karl", new Mock<DbProviderFactory>().Object)).Message);
            }
        }

        public class ExecutionStrategy
        {
            [Fact]
            public void Throws_if_given_a_null_server_name_or_bad_invariant_name()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().ExecutionStrategy(null, () => new Mock<IDbExecutionStrategy>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().ExecutionStrategy("", () => new Mock<IDbExecutionStrategy>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().ExecutionStrategy(" ", () => new Mock<IDbExecutionStrategy>().Object)).Message);
                Assert.Equal(
                    "getExecutionStrategy",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().ExecutionStrategy("a", null)).ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().ExecutionStrategy(null, () => new Mock<IDbExecutionStrategy>().Object, "a")).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().ExecutionStrategy("", () => new Mock<IDbExecutionStrategy>().Object, "a")).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().ExecutionStrategy(" ", () => new Mock<IDbExecutionStrategy>().Object, "a")).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("serverName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().ExecutionStrategy("a", () => new Mock<IDbExecutionStrategy>().Object, null)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("serverName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().ExecutionStrategy("a", () => new Mock<IDbExecutionStrategy>().Object, "")).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("serverName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().ExecutionStrategy("a", () => new Mock<IDbExecutionStrategy>().Object, " ")).Message);
                Assert.Equal(
                    "getExecutionStrategy",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().ExecutionStrategy("a", null, "b")).ParamName);
            }

            [Fact]
            public void Throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("ExecutionStrategy"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.ExecutionStrategy("a", () => new Mock<IDbExecutionStrategy>().Object, "b")).Message);
            }

            [Fact]
            public void Delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var executionStrategy = new Func<IDbExecutionStrategy>(() => new Mock<IDbExecutionStrategy>().Object);

                new DbConfiguration(mockInternalConfiguration.Object).ExecutionStrategy("a", executionStrategy, "b");

                mockInternalConfiguration.Verify(
                    m => m.AddDependencyResolver(It.IsAny<ExecutionStrategyResolver<IDbExecutionStrategy>>(), false));
            }
        }

        public class ConnectionFactory : TestBase
        {
            [Fact]
            public void Setting_ConnectionFactory_throws_if_given_a_null_factory()
            {
                Assert.Equal(
                    "connectionFactory",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().ConnectionFactory(null)).ParamName);
            }

            [Fact]
            public void Setting_ConnectionFactory_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("ConnectionFactory"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.ConnectionFactory(new Mock<IDbConnectionFactory>().Object)).Message);
            }

            [Fact]
            public void ConnectionFactory_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var connectionFactory = new Mock<IDbConnectionFactory>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).ConnectionFactory(connectionFactory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(connectionFactory));
            }

            [Fact]
            public void ConnectionFactory_set_in_code_can_be_overriden_before_config_is_locked()
            {
                Assert.IsType<SqlConnectionFactory>(DbConfiguration.DependencyResolver.GetService<IDbConnectionFactory>());
                Assert.IsType<DefaultUnitTestsConnectionFactory>(FunctionalTestsConfiguration.OriginalConnectionFactories[0]);
            }
        }

        public class PluralizationService
        {
            [Fact]
            public void Setting_PluralizationService_throws_if_given_a_null_service()
            {
                Assert.Equal(
                    "pluralizationService",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().PluralizationService(null)).ParamName);
            }

            [Fact]
            public void Setting_PluralizationService_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("PluralizationService"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.PluralizationService(new Mock<IPluralizationService>().Object)).Message);
            }

            [Fact]
            public void PluralizationService_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var pluralizationService = new Mock<IPluralizationService>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).PluralizationService(pluralizationService);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(pluralizationService));
            }
        }

        public class DependencyResolver
        {
            [Fact]
            public void DefaultModelCacheKey_is_returned_by_default_cache_key_factory()
            {
                var factory = DbConfiguration.DependencyResolver.GetService<Func<DbContext, IDbModelCacheKey>>();

                using (var context = new CacheKeyContext())
                {
                    Assert.IsType<DefaultModelCacheKey>(factory(context));
                }
            }

            public class CacheKeyContext : DbContext
            {
                static CacheKeyContext()
                {
                    Database.SetInitializer<CacheKeyContext>(null);
                }
            }
        }

        public class DatabaseInitializer
        {
            [Fact]
            public void DatabaseInitializer_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("DatabaseInitializer"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.DatabaseInitializer(new Mock<IDatabaseInitializer<DbContext>>().Object)).Message);
            }

            [Fact]
            public void DatabaseInitializer_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var initializer = new Mock<IDatabaseInitializer<DbContext>>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).DatabaseInitializer(initializer);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(initializer));
            }

            [Fact]
            public void DatabaseInitializer_creates_null_initializer_when_given_null_argument()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);

                new DbConfiguration(mockInternalConfiguration.Object).DatabaseInitializer<DbContext>(null);

                mockInternalConfiguration.Verify(
                    m => m.RegisterSingleton<IDatabaseInitializer<DbContext>>(It.IsAny<NullDatabaseInitializer<DbContext>>()));
            }
        }

        public class MigrationSqlGeneratorTests
        {
            [Fact]
            public void MigrationSqlGenerator_throws_if_given_a_null_generator_or_bad_invariant_name()
            {
                Assert.Equal(
                    "sqlGenerator",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().MigrationSqlGenerator("Karl", null)).ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().MigrationSqlGenerator(null, () => new Mock<MigrationSqlGenerator>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().MigrationSqlGenerator("", () => new Mock<MigrationSqlGenerator>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().MigrationSqlGenerator(" ", () => new Mock<MigrationSqlGenerator>().Object)).Message);
            }

            [Fact]
            public void MigrationSqlGenerator_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("MigrationSqlGenerator"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.MigrationSqlGenerator("Karl", () => new Mock<MigrationSqlGenerator>().Object)).Message);
            }

            [Fact]
            public void MigrationSqlGenerator_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var generator = new Func<MigrationSqlGenerator>(() => new Mock<MigrationSqlGenerator>().Object);

                new DbConfiguration(mockInternalConfiguration.Object).MigrationSqlGenerator("Karl", generator);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(generator, "Karl"));
            }
        }

        public class ManifestTokenResolver
        {
            [Fact]
            public void ManifestTokenResolver_throws_if_given_a_null_service()
            {
                Assert.Equal(
                    "resolver",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().ManifestTokenResolver(null)).ParamName);
            }

            [Fact]
            public void ManifestTokenResolver_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("ManifestTokenResolver"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.ManifestTokenResolver(new Mock<IManifestTokenResolver>().Object)).Message);
            }

            [Fact]
            public void ManifestTokenResolver_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var service = new Mock<IManifestTokenResolver>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).ManifestTokenResolver(service);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(service));
            }
        }

        public class ProviderFactoryResolver
        {
            [Fact]
            public void ProviderFactoryResolver_throws_if_given_a_null_service()
            {
                Assert.Equal(
                    "providerFactoryResolver",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().ProviderFactoryResolver(null)).ParamName);
            }

            [Fact]
            public void ProviderFactoryResolver_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("ProviderFactoryResolver"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.ProviderFactoryResolver(new Mock<IDbProviderFactoryResolver>().Object)).Message);
            }

            [Fact]
            public void ProviderFactoryResolver_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var service = new Mock<IDbProviderFactoryResolver>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).ProviderFactoryResolver(service);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(service));
            }
        }

        public class ModelCacheKey
        {
            [Fact]
            public void ModelCacheKey_throws_if_given_a_null_factory()
            {
                Assert.Equal(
                    "keyFactory",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().ModelCacheKey(null)).ParamName);
            }

            [Fact]
            public void ModelCacheKey_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("ModelCacheKey"),
                    Assert.Throws<InvalidOperationException>(() => configuration.ModelCacheKey(c => null)).Message);
            }

            [Fact]
            public void ModelCacheKey_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var factory = (Func<DbContext, IDbModelCacheKey>)(c => null);

                new DbConfiguration(mockInternalConfiguration.Object).ModelCacheKey(factory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(factory));
            }
        }

        public class HistoryContextFactory
        {
            [Fact]
            public void Throws_if_given_a_null_provider()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().HistoryContext(null, null)).Message);
            }

            [Fact]
            public void Throws_if_given_a_null_factory()
            {
                Assert.Equal(
                    "factory",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbConfiguration().HistoryContext("Foo", null)).ParamName);
            }

            [Fact]
            public void Throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("HistoryContext"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.HistoryContext("Foo", (e, d) => null)).Message);
            }

            [Fact]
            public void Delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                Func<DbConnection, string, HistoryContext> factory = (e, d) => null;

                new DbConfiguration(mockInternalConfiguration.Object).HistoryContext("Foo", factory);

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
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SpatialServices(null)).ParamName);
            }

            [Fact]
            public void SetDefaultSpatialProvider_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SpatialServices"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SpatialServices(new Mock<DbSpatialServices>().Object)).Message);
            }

            [Fact]
            public void SetDefaultSpatialProvider_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var provider = new Mock<DbSpatialServices>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SpatialServices(provider);

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
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SpatialServices("Good.As.Gold", null)).ParamName);

                Assert.Equal(
                    "spatialProvider",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbConfiguration().SpatialServices(new DbProviderInfo("Especially.For.You", ""), null)).ParamName);

                Assert.Equal(
                    "key",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbConfiguration().SpatialServices((DbProviderInfo)null, new Mock<DbSpatialServices>().Object))
                        .ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SpatialServices((string)null, new Mock<DbSpatialServices>().Object)).Message);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SpatialServices("", new Mock<DbSpatialServices>().Object)).Message);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SpatialServices(" ", new Mock<DbSpatialServices>().Object)).Message);
            }

            [Fact]
            public void SetSpatialProvider_with_invariant_name_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SpatialServices"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SpatialServices("Song.For.Whoever", new Mock<DbSpatialServices>().Object)).Message);
            }

            [Fact]
            public void SetSpatialProvider_with_key_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SpatialServices"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SpatialServices(
                            new DbProviderInfo("Song.For.Whoever", ""),
                            new Mock<DbSpatialServices>().Object)).Message);
            }

            [Fact]
            public void SetSpatialProvider_with_invariant_name_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var provider = SqlSpatialServices.Instance;
                Func<object, bool> keyPredicate = null;
                mockInternalConfiguration.Setup(
                    m => m.RegisterSingleton<DbSpatialServices>(
                        provider,
                        It.IsAny<Func<object, bool>>())).Callback<DbSpatialServices, Func<object, bool>>((s, k) => { keyPredicate = k; });

                new DbConfiguration(mockInternalConfiguration.Object).SpatialServices("Mini.Tattoo", provider);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton<DbSpatialServices>(provider, It.IsAny<Func<object, bool>>()));

                Assert.True(keyPredicate(new DbProviderInfo("Mini.Tattoo", "Foo")));
                Assert.False(keyPredicate(new DbProviderInfo("Maxi.Tattoo", "Foo")));
                Assert.False(keyPredicate("Mini.Tattoo"));
                Assert.False(keyPredicate(null));
            }

            [Fact]
            public void SetSpatialProvider_with_key_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var provider = SqlSpatialServices.Instance;

                var key = new DbProviderInfo("A.Little.Time", "Paul");
                new DbConfiguration(mockInternalConfiguration.Object).SpatialServices(key, provider);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton<DbSpatialServices>(provider, key));
            }
        }

        public class CommandLogger
        {
            [Fact]
            public void Throws_if_given_a_null_factory()
            {
                Assert.Equal(
                    "commandLoggerFactory",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbConfiguration().CommandLogger(null)).ParamName);
            }

            [Fact]
            public void Throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("CommandLogger"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.CommandLogger((_, __) => null)).Message);
            }

            [Fact]
            public void Delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                Func<DbContext, Action<string>, DbCommandLogger> factory = (_, __) => null;

                new DbConfiguration(mockInternalConfiguration.Object).CommandLogger(factory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(factory));
            }
        }

        public class Interceptor
        {
            [Fact]
            public void Throws_if_given_a_null_interceptor()
            {
                Assert.Equal(
                    "interceptor",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().Interceptor(null)).ParamName);
            }

            [Fact]
            public void Throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("Interceptor"),
                    Assert.Throws<InvalidOperationException>(() => configuration.Interceptor(new Mock<IDbInterceptor>().Object)).Message);
            }

            [Fact]
            public void Delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var interceptor = new Mock<IDbInterceptor>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).Interceptor(interceptor);

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
