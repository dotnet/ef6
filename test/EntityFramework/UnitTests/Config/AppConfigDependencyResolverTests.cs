// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.ConfigFile;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer;
    using System.Linq;
    using Moq;
    using Xunit;

    public class AppConfigDependencyResolverTests : AppConfigTestBase
    {
        public interface IPilkington
        {
        }

        public class FakeConnectionFactory : IDbConnectionFactory
        {
            public DbConnection CreateConnection(string nameOrConnectionString)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void GetService_returns_null_for_unknown_contract_type()
        {
            Assert.Null(new AppConfigDependencyResolver(CreateAppConfig()).GetService<IPilkington>("Karl"));
        }

        [Fact]
        public void GetService_returns_registered_provider()
        {
            Assert.Same(
                ProviderServicesFactoryTests.FakeProviderWithPublicProperty.Instance,
                new AppConfigDependencyResolver(CreateAppConfigWithProvider()).GetService<DbProviderServices>("Is.Ee.Avin.A.Larf"));
        }

        [Fact]
        public void GetService_returns_null_for_unregistered_provider_name()
        {
            Assert.Null(
                new AppConfigDependencyResolver(CreateAppConfigWithProvider()).GetService<DbProviderServices>("Are.You.Avin.A.Larf"));
        }

        [Fact]
        public void GetService_returns_null_for_null_empty_or_whitespace_provider_name()
        {
            Assert.Null(new AppConfigDependencyResolver(CreateAppConfig()).GetService<DbProviderServices>(null));
            Assert.Null(new AppConfigDependencyResolver(CreateAppConfig()).GetService<DbProviderServices>(""));
            Assert.Null(new AppConfigDependencyResolver(CreateAppConfig()).GetService<DbProviderServices>(" "));
        }

        [Fact]
        public void GetService_caches_provider()
        {
            var mockProviders = new Mock<ProviderConfig>();
            mockProviders.Setup(m => m.TryGetDbProviderServices("Ask.Rhod.Gilbert")).Returns(new Mock<DbProviderServices>().Object);
            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.Providers).Returns(mockProviders.Object);

            var resolver = new AppConfigDependencyResolver(mockConfig.Object);

            var factoryInstance = resolver.GetService<DbProviderServices>("Ask.Rhod.Gilbert");

            Assert.NotNull(factoryInstance);
            mockProviders.Verify(m => m.TryGetDbProviderServices("Ask.Rhod.Gilbert"), Times.Once());
            Assert.Same(factoryInstance, resolver.GetService<DbProviderServices>("Ask.Rhod.Gilbert"));
            mockProviders.Verify(m => m.TryGetDbProviderServices("Ask.Rhod.Gilbert"), Times.Once());
        }

        [Fact]
        public void GetService_caches_the_fact_that_no_provider_is_registered()
        {
            var mockProviders = new Mock<ProviderConfig>();
            mockProviders.Setup(m => m.TryGetDbProviderServices("Ask.Rhod.Gilbert")).Returns((DbProviderServices)null);
            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.Providers).Returns(mockProviders.Object);

            var resolver = new AppConfigDependencyResolver(mockConfig.Object);

            Assert.Null(resolver.GetService<DbProviderServices>("Ask.Rhod.Gilbert"));
            mockProviders.Verify(m => m.TryGetDbProviderServices("Ask.Rhod.Gilbert"), Times.Once());
            Assert.Null(resolver.GetService<DbProviderServices>("Ask.Rhod.Gilbert"));
            mockProviders.Verify(m => m.TryGetDbProviderServices("Ask.Rhod.Gilbert"), Times.Once());
        }

        [Fact]
        public void GetService_returns_registered_Migrations_SQL_generator()
        {
            Assert.IsType<ProviderConfigTests.TryGetMigrationSqlGeneratorFactory.MySqlGenerator>(
                new AppConfigDependencyResolver(
                    CreateAppConfigWithProvider(
                        typeof(ProviderConfigTests.TryGetMigrationSqlGeneratorFactory.MySqlGenerator).AssemblyQualifiedName))
                    .GetService<MigrationSqlGenerator>("Is.Ee.Avin.A.Larf"));
        }

        [Fact]
        public void GetService_returns_null_SQL_generator_for_unregistered_provider()
        {
            Assert.Null(
                new AppConfigDependencyResolver(CreateAppConfigWithProvider()).GetService<MigrationSqlGenerator>("Are.You.Avin.A.Larf"));
        }

        [Fact]
        public void GetService_returns_null_for_registered_provider_with_no_registered_Migrations_SQL_generator()
        {
            Assert.Null(
                new AppConfigDependencyResolver(CreateAppConfigWithProvider()).GetService<MigrationSqlGenerator>("Is.Ee.Avin.A.Larf"));
        }

        [Fact]
        public void GetService_returns_null_when_asked_for_SQL_generator_for_null_empty_or_whitespace_provider_name()
        {
            Assert.Null(new AppConfigDependencyResolver(CreateAppConfig()).GetService<MigrationSqlGenerator>(null));
            Assert.Null(new AppConfigDependencyResolver(CreateAppConfig()).GetService<MigrationSqlGenerator>(""));
            Assert.Null(new AppConfigDependencyResolver(CreateAppConfig()).GetService<MigrationSqlGenerator>(" "));
        }

        [Fact]
        public void GetService_caches_Migrations_SQL_generator_and_uses_it_to_return_a_new_instance_every_call()
        {
            var mockProviders = new Mock<ProviderConfig>();
            mockProviders.Setup(m => m.TryGetMigrationSqlGeneratorFactory("Ask.Rhod.Gilbert"))
                .Returns(() => new Mock<MigrationSqlGenerator>().Object);

            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.Providers).Returns(mockProviders.Object);

            var resolver = new AppConfigDependencyResolver(mockConfig.Object);

            var migrationSqlGenerator1 = resolver.GetService<MigrationSqlGenerator>("Ask.Rhod.Gilbert");
            Assert.NotNull(migrationSqlGenerator1);
            mockProviders.Verify(m => m.TryGetMigrationSqlGeneratorFactory("Ask.Rhod.Gilbert"), Times.Once());

            var migrationSqlGenerator2 = resolver.GetService<MigrationSqlGenerator>("Ask.Rhod.Gilbert");
            Assert.NotNull(migrationSqlGenerator2);
            Assert.NotSame(migrationSqlGenerator1, migrationSqlGenerator2);
            mockProviders.Verify(m => m.TryGetMigrationSqlGeneratorFactory("Ask.Rhod.Gilbert"), Times.Once());
        }

        [Fact]
        public void GetService_caches_the_fact_that_no_Migrations_SQL_generator_is_registered()
        {
            var mockProviders = new Mock<ProviderConfig>();
            mockProviders.Setup(m => m.TryGetMigrationSqlGeneratorFactory("Ask.Rhod.Gilbert"))
                .Returns((Func<MigrationSqlGenerator>)(() => null));

            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.Providers).Returns(mockProviders.Object);

            var resolver = new AppConfigDependencyResolver(mockConfig.Object);

            Assert.Null(resolver.GetService<MigrationSqlGenerator>("Ask.Rhod.Gilbert"));
            mockProviders.Verify(m => m.TryGetMigrationSqlGeneratorFactory("Ask.Rhod.Gilbert"), Times.Once());
            Assert.Null(resolver.GetService<MigrationSqlGenerator>("Ask.Rhod.Gilbert"));
            mockProviders.Verify(m => m.TryGetMigrationSqlGeneratorFactory("Ask.Rhod.Gilbert"), Times.Once());
        }

        [Fact]
        public void GetService_returns_connection_factory_set_in_config()
        {
            try
            {
                Assert.IsType<FakeConnectionFactory>(
                    new AppConfigDependencyResolver(
                        new AppConfig(
                            CreateEmptyConfig().AddDefaultConnectionFactory(typeof(FakeConnectionFactory).AssemblyQualifiedName)))
                        .GetService<IDbConnectionFactory>());
            }
            finally
            {
                Database.ResetDefaultConnectionFactory();
            }
        }

        [Fact]
        public void GetService_returns_null_if_no_connection_factory_is_set_in_config()
        {
            Assert.Null(new AppConfigDependencyResolver(CreateAppConfig()).GetService<IDbConnectionFactory>());
        }

        [Fact]
        public void GetService_caches_connection_factory()
        {
            try
            {
                var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
                mockConfig.Setup(m => m.TryGetDefaultConnectionFactory()).Returns(new FakeConnectionFactory());
                var resolver = new AppConfigDependencyResolver(mockConfig.Object);

                var factoryInstance = resolver.GetService<IDbConnectionFactory>();

                Assert.NotNull(factoryInstance);
                mockConfig.Verify(m => m.TryGetDefaultConnectionFactory(), Times.Once());
                Assert.Same(factoryInstance, resolver.GetService<IDbConnectionFactory>());
                mockConfig.Verify(m => m.TryGetDefaultConnectionFactory(), Times.Once());
            }
            finally
            {
                Database.ResetDefaultConnectionFactory();
            }
        }

        [Fact]
        public void GetService_caches_the_fact_that_no_connection_factory_is_set()
        {
            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.TryGetDefaultConnectionFactory()).Returns((IDbConnectionFactory)null);
            var resolver = new AppConfigDependencyResolver(mockConfig.Object);

            Assert.Null(resolver.GetService<IDbConnectionFactory>());
            mockConfig.Verify(m => m.TryGetDefaultConnectionFactory(), Times.Once());
            Assert.Null(resolver.GetService<IDbConnectionFactory>());
            mockConfig.Verify(m => m.TryGetDefaultConnectionFactory(), Times.Once());
        }

        [Fact]
        public void GetService_returns_registered_database_initializer()
        {
            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.Initializers).Returns(
                new InitializerConfig(
                    CreateEfSection(initializerDisabled: false),
                    new KeyValueConfigurationCollection()));

            Assert.IsType<FakeInitializer<FakeContext>>(
                new AppConfigDependencyResolver(mockConfig.Object).GetService<IDatabaseInitializer<FakeContext>>());
        }

        [Fact]
        public void GetService_returns_null_for_unregistered_database_initializer()
        {
            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.Initializers).Returns(
                new InitializerConfig(
                    CreateEfSection(initializerDisabled: false),
                    new KeyValueConfigurationCollection()));

            Assert.Null(new AppConfigDependencyResolver(mockConfig.Object).GetService<IDatabaseInitializer<DbContext>>());
        }

        [Fact]
        public void GetService_caches_database_initializer()
        {
            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.Initializers).Returns(
                new InitializerConfig(
                    CreateEfSection(initializerDisabled: false),
                    new KeyValueConfigurationCollection()));

            var resolver = new AppConfigDependencyResolver(mockConfig.Object);
            var initializer = resolver.GetService<IDatabaseInitializer<FakeContext>>();

            Assert.NotNull(initializer);
            mockConfig.Verify(m => m.Initializers, Times.Once());
            Assert.Same(initializer, resolver.GetService<IDatabaseInitializer<FakeContext>>());
            mockConfig.Verify(m => m.Initializers, Times.Once());
        }

        [Fact]
        public void GetService_caches_the_fact_that_no_database_initializer_is_registered()
        {
            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.Initializers).Returns(
                new InitializerConfig(
                    CreateEfSection(initializerDisabled: false),
                    new KeyValueConfigurationCollection()));

            var resolver = new AppConfigDependencyResolver(mockConfig.Object);

            Assert.Null(resolver.GetService<IDatabaseInitializer<DbContext>>());
            mockConfig.Verify(m => m.Initializers, Times.Once());
            Assert.Null(resolver.GetService<IDatabaseInitializer<DbContext>>());
            mockConfig.Verify(m => m.Initializers, Times.Once());
        }

        [Fact]
        public void GetService_returns_registered_spatial_provider()
        {
            Assert.IsType<SqlSpatialServices>(
                new AppConfigDependencyResolver(CreateAppConfigWithSpatial(typeof(SqlSpatialServices).AssemblyQualifiedName)).GetService
                    <DbSpatialServices>());
        }

        [Fact]
        public void GetService_returns_null_when_no_provider_registered()
        {
            Assert.Null(
                new AppConfigDependencyResolver(CreateAppConfigWithSpatial()).GetService<DbSpatialServices>());
        }

        [Fact]
        public void GetService_caches_spatial_provider()
        {
            var mockProviders = new Mock<ProviderConfig>();
            mockProviders.Setup(m => m.TryGetSpatialProvider()).Returns(new Mock<DbSpatialServices>().Object);
            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.Providers).Returns(mockProviders.Object);

            var resolver = new AppConfigDependencyResolver(mockConfig.Object);

            var factoryInstance = resolver.GetService<DbSpatialServices>();

            Assert.NotNull(factoryInstance);
            mockProviders.Verify(m => m.TryGetSpatialProvider(), Times.Once());
            Assert.Same(factoryInstance, resolver.GetService<DbSpatialServices>());
            mockProviders.Verify(m => m.TryGetSpatialProvider(), Times.Once());
        }

        [Fact]
        public void GetService_caches_the_fact_that_no_spatial_provider_is_registered()
        {
            var mockProviders = new Mock<ProviderConfig>();
            mockProviders.Setup(m => m.TryGetSpatialProvider()).Returns((DbSpatialServices)null);
            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.Providers).Returns(mockProviders.Object);

            var resolver = new AppConfigDependencyResolver(mockConfig.Object);

            Assert.Null(resolver.GetService<DbSpatialServices>());
            mockProviders.Verify(m => m.TryGetSpatialProvider(), Times.Once());
            Assert.Null(resolver.GetService<DbSpatialServices>());
            mockProviders.Verify(m => m.TryGetSpatialProvider(), Times.Once());
        }

        private static EntityFrameworkSection CreateEfSection(bool initializerDisabled)
        {
            var mockDatabaseInitializerElement = new Mock<DatabaseInitializerElement>();
            mockDatabaseInitializerElement
                .Setup(m => m.InitializerTypeName)
                .Returns(typeof(FakeInitializer<FakeContext>).AssemblyQualifiedName);
            mockDatabaseInitializerElement.Setup(m => m.Parameters).Returns(new ParameterCollection());

            var mockContextElement = new Mock<ContextElement>();
            mockContextElement.Setup(m => m.IsDatabaseInitializationDisabled).Returns(initializerDisabled);
            mockContextElement.Setup(m => m.ContextTypeName).Returns(typeof(FakeContext).AssemblyQualifiedName);
            mockContextElement.Setup(m => m.DatabaseInitializer).Returns(mockDatabaseInitializerElement.Object);

            var mockContextCollection = new Mock<ContextCollection>();
            mockContextCollection.As<IEnumerable>().Setup(m => m.GetEnumerator()).Returns(
                new List<ContextElement>
                    {
                        mockContextElement.Object
                    }.GetEnumerator());

            var mockEfSection = new Mock<EntityFrameworkSection>();
            mockEfSection.Setup(m => m.Contexts).Returns(mockContextCollection.Object);

            return mockEfSection.Object;
        }

        public class FakeContext : DbContext
        {
        }

        public class FakeInitializer<TContext> : IDatabaseInitializer<TContext>
            where TContext : DbContext
        {
            public void InitializeDatabase(TContext context)
            {
            }
        }

        [Fact]
        public void EF_provider_can_be_loaded_from_real_app_config()
        {
            Assert.Same(
                FakeSqlProviderServices.Instance,
                new AppConfigDependencyResolver(AppConfig.DefaultInstance).GetService<DbProviderServices>("System.Data.FakeSqlClient"));
        }

        [Fact]
        public void SQL_generator_can_be_loaded_from_real_app_config()
        {
            Assert.IsType<FakeSqlGenerator>(
                new AppConfigDependencyResolver(AppConfig.DefaultInstance).GetService<MigrationSqlGenerator>("System.Data.FakeSqlClient"));
        }

        [Fact]
        public void Spatial_provider_can_be_loaded_from_real_app_config()
        {
            Assert.IsType<SqlSpatialServices>(
                new AppConfigDependencyResolver(AppConfig.DefaultInstance).GetService<DbSpatialServices>());
        }

        private static AppConfig CreateAppConfigWithProvider(string sqlGeneratorName = null)
        {
            return CreateAppConfig(
                "Is.Ee.Avin.A.Larf",
                typeof(ProviderServicesFactoryTests.FakeProviderWithPublicProperty).AssemblyQualifiedName,
                sqlGeneratorName);
        }

        /// <summary>
        ///     This test makes calls from multiple threads such that we have at least some chance of finding threading
        ///     issues. As with any test of this type just because the test passes does not mean that the code is
        ///     correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
        ///     be investigated. DON'T just re-run and think things are okay if the test then passes.
        /// </summary>
        [Fact]
        public void GetService_can_be_accessed_from_multiple_threads_concurrently()
        {
            try
            {
                var appConfig = new AppConfig(
                    CreateEmptyConfig().AddDefaultConnectionFactory(typeof(FakeConnectionFactory).AssemblyQualifiedName));

                for (var i = 0; i < 30; i++)
                {
                    var bag = new ConcurrentBag<IDbConnectionFactory>();
                    var resolver = new AppConfigDependencyResolver(appConfig);

                    ExecuteInParallel(() => bag.Add(resolver.GetService<IDbConnectionFactory>()));

                    Assert.Equal(20, bag.Count);
                    Assert.True(bag.All(c => resolver.GetService<IDbConnectionFactory>() == c));
                }
            }
            finally
            {
                Database.ResetDefaultConnectionFactory();
            }
        }
    }
}
