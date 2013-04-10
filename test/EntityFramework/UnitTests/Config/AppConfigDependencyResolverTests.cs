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
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.SqlServerCompact;
    using System.Data.Entity.Utilities;
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
            Assert.Null(
                new AppConfigDependencyResolver(CreateAppConfig(), new Mock<InternalConfiguration>(null, null, null, null).Object)
                    .GetService<IPilkington>("Karl"));
        }

        [Fact]
        public void GetService_returns_registered_provider()
        {
            Assert.Same(
                ProviderServicesFactoryTests.FakeProviderWithPublicProperty.Instance,
                new AppConfigDependencyResolver(
                    CreateAppConfigWithProvider(), new Mock<InternalConfiguration>(null, null, null, null).Object)
                    .GetService<DbProviderServices>("Is.Ee.Avin.A.Larf"));
        }

        [Fact]
        public void GetService_returns_null_for_unregistered_provider_name()
        {
            Assert.Null(
                new AppConfigDependencyResolver(
                    CreateAppConfigWithProvider(), new Mock<InternalConfiguration>(null, null, null, null).Object)
                    .GetService<DbProviderServices>("Are.You.Avin.A.Larf"));
        }

        [Fact]
        public void GetService_returns_null_for_null_empty_or_whitespace_provider_name()
        {
            Assert.Null(
                new AppConfigDependencyResolver(CreateAppConfig(), new Mock<InternalConfiguration>(null, null, null, null).Object)
                    .GetService<DbProviderServices>(null));
            Assert.Null(
                new AppConfigDependencyResolver(CreateAppConfig(), new Mock<InternalConfiguration>(null, null, null, null).Object)
                    .GetService<DbProviderServices>(""));
            Assert.Null(
                new AppConfigDependencyResolver(CreateAppConfig(), new Mock<InternalConfiguration>(null, null, null, null).Object)
                    .GetService<DbProviderServices>(" "));
        }

        [Fact]
        public void GetService_caches_provider()
        {
            var mockFactory = new Mock<ProviderServicesFactory>();
            mockFactory.Setup(m => m.GetInstance("Rhods.Provider", "Ask.Rhod.Gilbert")).Returns(new Mock<DbProviderServices>().Object);

            var mockProviders = new Mock<ProviderConfig>();
            mockProviders.Setup(m => m.GetAllDbProviderServices())
                         .Returns(
                             new[]
                                 {
                                     new ProviderElement
                                         {
                                             InvariantName = "Ask.Rhod.Gilbert",
                                             ProviderTypeName = "Rhods.Provider"
                                         }
                                 });

            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.Providers).Returns(mockProviders.Object);

            var resolver = new AppConfigDependencyResolver(
                mockConfig.Object,
                new Mock<InternalConfiguration>(null, null, null, null).Object,
                mockFactory.Object);

            var factoryInstance = resolver.GetService<DbProviderServices>("Ask.Rhod.Gilbert");

            Assert.NotNull(factoryInstance);
            mockProviders.Verify(m => m.GetAllDbProviderServices(), Times.Once());
            Assert.Same(factoryInstance, resolver.GetService<DbProviderServices>("Ask.Rhod.Gilbert"));
            mockProviders.Verify(m => m.GetAllDbProviderServices(), Times.Once());
        }

        [Fact]
        public void GetService_caches_the_fact_that_no_provider_is_registered()
        {
            var mockProviders = new Mock<ProviderConfig>();
            mockProviders.Setup(m => m.GetAllDbProviderServices())
                         .Returns(Enumerable.Empty<ProviderElement>());

            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.Providers).Returns(mockProviders.Object);

            var resolver = new AppConfigDependencyResolver(
                mockConfig.Object, new Mock<InternalConfiguration>(null, null, null, null).Object);

            Assert.Null(resolver.GetService<DbProviderServices>("Ask.Rhod.Gilbert"));
            mockProviders.Verify(m => m.GetAllDbProviderServices(), Times.Once());
            Assert.Null(resolver.GetService<DbProviderServices>("Ask.Rhod.Gilbert"));
            mockProviders.Verify(m => m.GetAllDbProviderServices(), Times.Once());
        }

        [Fact]
        public void GetService_registers_all_providers_as_secondary_resolvers_in_order_the_first_time_any_service_is_requested()
        {
            var mockConfig = CreateMockConfigWithProviders();
            var mockFactory = CreateMockFactory(mockConfig.Object);
            var mockConfiguration = new Mock<InternalConfiguration>(null, null, null, null);

            var resolvers = new List<IDbDependencyResolver>();

            mockConfiguration.Setup(
                m => m.AddSecondaryResolver(It.IsAny<IDbDependencyResolver>())).Callback<IDbDependencyResolver>(resolvers.Add);

            new AppConfigDependencyResolver(mockConfig.Object, mockConfiguration.Object, mockFactory.Object).GetService<IPilkington>();

            Assert.Equal(3, resolvers.Count);

            Assert.Equal("Around.The.World", resolvers[0].GetService<string>());
            Assert.Equal("One.More.Time", resolvers[1].GetService<string>());
            Assert.Equal("Robot.Rock", resolvers[2].GetService<string>());
        }

        [Fact]
        public void GetService_registers_all_providers_from_real_app_config_as_secondary_resolvers_in_correct_order()
        {
            var mockConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
            var resolvers = new List<IDbDependencyResolver>();

            mockConfiguration.Setup(
                m => m.AddSecondaryResolver(It.IsAny<IDbDependencyResolver>())).Callback<IDbDependencyResolver>(resolvers.Add);

            new AppConfigDependencyResolver(AppConfig.DefaultInstance, mockConfiguration.Object).GetService<IPilkington>();

            Assert.Equal(3, resolvers.Count);

            Assert.IsType<FakeSqlProviderServices>(resolvers[0]);
            Assert.IsType<SqlCeProviderServices>(resolvers[1]);
            Assert.IsType<SqlProviderServices>(resolvers[2]);
        }

        [Fact]
        public void GetService_registers_all_providers_as_secondary_resolvers_only_once()
        {
            var mockConfig = CreateMockConfigWithProviders();
            var mockFactory = CreateMockFactory(mockConfig.Object);
            var mockConfiguration = new Mock<InternalConfiguration>(null, null, null, null);

            var resolver = new AppConfigDependencyResolver(mockConfig.Object, mockConfiguration.Object, mockFactory.Object);

            resolver.GetService<IPilkington>();

            mockConfiguration.Verify(m => m.AddSecondaryResolver(It.IsAny<DbProviderServices>()), Times.Exactly(3));

            resolver.GetService<IPilkington>();

            mockConfiguration.Verify(m => m.AddSecondaryResolver(It.IsAny<DbProviderServices>()), Times.Exactly(3));
        }

        [Fact]
        public void GetService_registers_SQL_Server_as_a_fallback_if_it_is_not_already_registered()
        {
            var mockConfig = CreateMockConfigWithProviders();

            var mockSqlProvider = new Mock<DbProviderServices>();
            mockSqlProvider.Setup(m => m.GetService(typeof(string), null)).Returns("System.Data.SqlClient");
            var someRandomThing = new Random();
            mockSqlProvider.Setup(m => m.GetService(typeof(Random), null)).Returns(someRandomThing);

            var mockFactory = CreateMockFactory(mockConfig.Object);
            mockFactory.Setup(m => m.TryGetInstance("System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer"))
                       .Returns(mockSqlProvider.Object);

            var mockConfiguration = new Mock<InternalConfiguration>(null, null, null, null);
            var resolvers = new ResolverChain();
            mockConfiguration.Setup(m => m.AddSecondaryResolver(It.IsAny<IDbDependencyResolver>()))
                             .Callback<IDbDependencyResolver>(resolvers.Add);

            new AppConfigDependencyResolver(mockConfig.Object, mockConfiguration.Object, mockFactory.Object).GetService<IPilkington>();

            mockConfiguration.Verify(m => m.AddSecondaryResolver(It.IsAny<DbProviderServices>()), Times.Exactly(4));
            mockConfiguration.Verify(m => m.AddSecondaryResolver(It.IsAny<SingletonDependencyResolver<DbProviderServices>>()), Times.Once());

            Assert.Equal("Robot.Rock", resolvers.GetService<string>());
            Assert.Same(someRandomThing, resolvers.GetService<Random>());
            Assert.Same(mockSqlProvider.Object, resolvers.GetService<DbProviderServices>("System.Data.SqlClient"));
        }

        [Fact]
        public void GetService_returns_connection_factory_set_in_config()
        {
            try
            {
                Assert.IsType<FakeConnectionFactory>(
                    new AppConfigDependencyResolver(
                        new AppConfig(
                            CreateEmptyConfig().AddDefaultConnectionFactory(typeof(FakeConnectionFactory).AssemblyQualifiedName)),
                        new Mock<InternalConfiguration>(null, null, null, null).Object)
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
            Assert.Null(
                new AppConfigDependencyResolver(CreateAppConfig(), new Mock<InternalConfiguration>(null, null, null, null).Object)
                    .GetService<IDbConnectionFactory>());
        }

        [Fact]
        public void GetService_caches_connection_factory()
        {
            try
            {
                var mockProviders = new Mock<ProviderConfig>();
                mockProviders.Setup(m => m.GetAllDbProviderServices())
                             .Returns(Enumerable.Empty<ProviderElement>());

                var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
                mockConfig.Setup(m => m.Providers).Returns(mockProviders.Object);
                mockConfig.Setup(m => m.TryGetDefaultConnectionFactory()).Returns(new FakeConnectionFactory());
                var resolver = new AppConfigDependencyResolver(
                    mockConfig.Object, new Mock<InternalConfiguration>(null, null, null, null).Object);

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
            var mockProviders = new Mock<ProviderConfig>();
            mockProviders.Setup(m => m.GetAllDbProviderServices())
                         .Returns(Enumerable.Empty<ProviderElement>());

            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.Providers).Returns(mockProviders.Object);
            mockConfig.Setup(m => m.TryGetDefaultConnectionFactory()).Returns((IDbConnectionFactory)null);
            var resolver = new AppConfigDependencyResolver(
                mockConfig.Object, new Mock<InternalConfiguration>(null, null, null, null).Object);

            Assert.Null(resolver.GetService<IDbConnectionFactory>());
            mockConfig.Verify(m => m.TryGetDefaultConnectionFactory(), Times.Once());
            Assert.Null(resolver.GetService<IDbConnectionFactory>());
            mockConfig.Verify(m => m.TryGetDefaultConnectionFactory(), Times.Once());
        }

        [Fact]
        public void GetService_returns_registered_database_initializer()
        {
            var mockProviders = new Mock<ProviderConfig>();
            mockProviders.Setup(m => m.GetAllDbProviderServices())
                         .Returns(Enumerable.Empty<ProviderElement>());

            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.Providers).Returns(mockProviders.Object);
            mockConfig.Setup(m => m.Initializers).Returns(
                new InitializerConfig(
                    CreateEfSection(initializerDisabled: false),
                    new KeyValueConfigurationCollection()));

            Assert.IsType<FakeInitializer<FakeContext>>(
                new AppConfigDependencyResolver(mockConfig.Object, new Mock<InternalConfiguration>(null, null, null, null).Object)
                    .GetService<IDatabaseInitializer<FakeContext>>());
        }

        [Fact]
        public void GetService_returns_null_for_unregistered_database_initializer()
        {
            var mockProviders = new Mock<ProviderConfig>();
            mockProviders.Setup(m => m.GetAllDbProviderServices())
                         .Returns(Enumerable.Empty<ProviderElement>());

            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.Providers).Returns(mockProviders.Object);
            mockConfig.Setup(m => m.Initializers).Returns(
                new InitializerConfig(
                    CreateEfSection(initializerDisabled: false),
                    new KeyValueConfigurationCollection()));

            Assert.Null(
                new AppConfigDependencyResolver(mockConfig.Object, new Mock<InternalConfiguration>(null, null, null, null).Object)
                    .GetService<IDatabaseInitializer<DbContext>>());
        }

        [Fact]
        public void GetService_caches_database_initializer()
        {
            var mockProviders = new Mock<ProviderConfig>();
            mockProviders.Setup(m => m.GetAllDbProviderServices())
                         .Returns(Enumerable.Empty<ProviderElement>());

            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.Providers).Returns(mockProviders.Object);
            mockConfig.Setup(m => m.Initializers).Returns(
                new InitializerConfig(
                    CreateEfSection(initializerDisabled: false),
                    new KeyValueConfigurationCollection()));

            var resolver = new AppConfigDependencyResolver(
                mockConfig.Object, new Mock<InternalConfiguration>(null, null, null, null).Object);
            var initializer = resolver.GetService<IDatabaseInitializer<FakeContext>>();

            Assert.NotNull(initializer);
            mockConfig.Verify(m => m.Initializers, Times.Once());
            Assert.Same(initializer, resolver.GetService<IDatabaseInitializer<FakeContext>>());
            mockConfig.Verify(m => m.Initializers, Times.Once());
        }

        [Fact]
        public void GetService_caches_the_fact_that_no_database_initializer_is_registered()
        {
            var mockProviders = new Mock<ProviderConfig>();
            mockProviders.Setup(m => m.GetAllDbProviderServices())
                         .Returns(Enumerable.Empty<ProviderElement>());

            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.Providers).Returns(mockProviders.Object);
            mockConfig.Setup(m => m.Initializers).Returns(
                new InitializerConfig(
                    CreateEfSection(initializerDisabled: false),
                    new KeyValueConfigurationCollection()));

            var resolver = new AppConfigDependencyResolver(
                mockConfig.Object, new Mock<InternalConfiguration>(null, null, null, null).Object);

            Assert.Null(resolver.GetService<IDatabaseInitializer<DbContext>>());
            mockConfig.Verify(m => m.Initializers, Times.Once());
            Assert.Null(resolver.GetService<IDatabaseInitializer<DbContext>>());
            mockConfig.Verify(m => m.Initializers, Times.Once());
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
                new AppConfigDependencyResolver(AppConfig.DefaultInstance, new Mock<InternalConfiguration>(null, null, null, null).Object)
                    .GetService<DbProviderServices>("System.Data.FakeSqlClient"));
        }

        private static AppConfig CreateAppConfigWithProvider(string sqlGeneratorName = null)
        {
            return CreateAppConfig(
                "Is.Ee.Avin.A.Larf",
                typeof(ProviderServicesFactoryTests.FakeProviderWithPublicProperty).AssemblyQualifiedName);
        }

        private static Mock<AppConfig> CreateMockConfigWithProviders()
        {
            var providerElements = new[]
                {
                    new ProviderElement
                        {
                            InvariantName = "Around.The.World",
                            ProviderTypeName = "Around.The.World.Type"
                        },
                    new ProviderElement
                        {
                            InvariantName = "One.More.Time",
                            ProviderTypeName = "One.More.Time.Type"
                        },
                    new ProviderElement
                        {
                            InvariantName = "Robot.Rock",
                            ProviderTypeName = "Robot.Rock.Type"
                        }
                };

            var mockProviders = new Mock<ProviderConfig>();
            mockProviders.Setup(m => m.GetAllDbProviderServices()).Returns(providerElements);

            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.Providers).Returns(mockProviders.Object);

            return mockConfig;
        }

        private static Mock<ProviderServicesFactory> CreateMockFactory(AppConfig config)
        {
            var mockFactory = new Mock<ProviderServicesFactory>();
            config.Providers.GetAllDbProviderServices().Each(
                e =>
                    {
                        var mockServices = new Mock<DbProviderServices>();
                        mockServices.Setup(m => m.GetService(typeof(string), null)).Returns(e.InvariantName);
                        mockFactory.Setup(m => m.GetInstance(e.ProviderTypeName, e.InvariantName)).Returns(mockServices.Object);
                    });
            return mockFactory;
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
                    var resolver = new AppConfigDependencyResolver(
                        appConfig, new Mock<InternalConfiguration>(null, null, null, null).Object);

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
