// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
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
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var resolver = new Mock<IDbDependencyResolver>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).AddDependencyResolver(resolver);

                mockInternalConfiguration.Verify(m => m.AddDependencyResolver(resolver));
            }
        }

        public class AddProvider
        {
            [Fact]
            public void AddProvider_throws_if_given_a_null_provider_or_bad_invariant_name()
            {
                Assert.Equal(
                    "provider",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddProvider("Karl", null)).ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddProvider(null, new Mock<DbProviderServices>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddProvider("", new Mock<DbProviderServices>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddProvider(" ", new Mock<DbProviderServices>().Object)).Message);
            }

            [Fact]
            public void AddProvider_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var providerServices = new Mock<DbProviderServices>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).AddProvider("900.FTW", providerServices);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(providerServices, "900.FTW"));
            }

            [Fact]
            public void AddProvider_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddProvider"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddProvider("Karl", new Mock<DbProviderServices>().Object)).Message);
            }
        }

        public class SetDefaultConnectionFactory
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
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var connectionFactory = new Mock<IDbConnectionFactory>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetDefaultConnectionFactory(connectionFactory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(connectionFactory, null));
            }
        }

        public class GetService
        {
            [Fact]
            public void Delegates_to_internal_configuration()
            {
                Assert.NotNull(DbConfiguration.GetService<IDbCommandInterceptor>(null));
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
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var initializer = new Mock<IDatabaseInitializer<DbContext>>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetDatabaseInitializer(initializer);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(initializer, null));
            }

            [Fact]
            public void SetDatabaseInitializer_creates_null_initializer_when_given_null_argument()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();

                new DbConfiguration(mockInternalConfiguration.Object).SetDatabaseInitializer<DbContext>(null);

                mockInternalConfiguration.Verify(
                    m => m.RegisterSingleton<IDatabaseInitializer<DbContext>>(It.IsAny<NullDatabaseInitializer<DbContext>>(), null));
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
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var generator = new Func<MigrationSqlGenerator>(() => new Mock<MigrationSqlGenerator>().Object);

                new DbConfiguration(mockInternalConfiguration.Object).AddMigrationSqlGenerator("Karl", generator);

                mockInternalConfiguration.Verify(m => m.AddDependencyResolver(It.IsAny<TransientDependencyResolver<MigrationSqlGenerator>>()));
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
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var service = new Mock<IManifestTokenService>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetManifestTokenService(service);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(service, null));
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
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var service = new Mock<IDbProviderFactoryService>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetProviderFactoryService(service);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(service, null));
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
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var factory = new Mock<IDbModelCacheKeyFactory>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetModelCacheKeyFactory(factory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(factory, null));
            }
        }

        public class SetSpatialProvider
        {
            [Fact]
            public void SetSpatialProvider_throws_if_given_a_null_factory()
            {
                Assert.Equal(
                    "spatialProvider",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetSpatialProvider(null)).ParamName);
            }

            [Fact]
            public void SetSpatialProvider_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetSpatialProvider"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetSpatialProvider(new Mock<DbSpatialServices>().Object)).Message);
            }

            [Fact]
            public void SetSpatialProvider_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var provider = new Mock<DbSpatialServices>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetSpatialProvider(provider);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(provider, null));
            }
        }

        private static DbConfiguration CreatedLockedConfiguration()
        {
            var internalConfiguration = new InternalConfiguration();
            internalConfiguration.Lock();
            return new DbConfiguration(internalConfiguration);
        }
    }
}
