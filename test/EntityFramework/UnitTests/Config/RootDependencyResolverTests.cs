// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections.Concurrent;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping.ViewGeneration;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Pluralization;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Utilities;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using Moq;
    using Xunit;

    public class RootDependencyResolverTests : TestBase
    {
        public class GetService : TestBase
        {
            [Fact]
            public void The_root_resolver_uses_the_default_provider_services_resolver_and_caches_provider_instances()
            {
                var providerServices = new Mock<DbProviderServices>().Object;
                var mockProviderResolver = new Mock<DefaultProviderServicesResolver>();
                mockProviderResolver
                    .Setup(m => m.GetService(typeof(DbProviderServices), "FooClient"))
                    .Returns(providerServices);

                var resolver = new RootDependencyResolver(
                    mockProviderResolver.Object,
                    new DatabaseInitializerResolver());

                Assert.Same(providerServices, resolver.GetService<DbProviderServices>("FooClient"));
                mockProviderResolver.Verify(m => m.GetService(typeof(DbProviderServices), "FooClient"), Times.Once());
                Assert.Same(providerServices, resolver.GetService<DbProviderServices>("FooClient"));
                mockProviderResolver.Verify(m => m.GetService(typeof(DbProviderServices), "FooClient"), Times.Once());
            }

            [Fact]
            public void The_root_resolver_can_return_a_default_history_context_factory_that_creates_HistoryContext_instances()
            {
                var factory =
                    new RootDependencyResolver(new DefaultProviderServicesResolver(), new DatabaseInitializerResolver())
                        .GetService<HistoryContextFactory>();

                Assert.IsType<HistoryContextFactory>(factory);

                using (var connection = new SqlConnection())
                {
                    using (var context = factory(connection, null))
                    {
                        Assert.IsType<HistoryContext>(context);
                    }
                }
            }

            public delegate HistoryContext NotHistoryContextFactory(
                DbConnection existingConnection, bool contextOwnsConnection, string defaultSchema);

            [Fact]
            public void The_root_resolver_does_not_return_a_history_context_factory_for_other_matching_delegate_types()
            {
                Assert.Null(
                    new RootDependencyResolver(
                        new DefaultProviderServicesResolver(),
                        new DatabaseInitializerResolver()).GetService<NotHistoryContextFactory>());
            }

            [Fact]
            public void The_root_resolver_does_not_return_a_history_context_factory_for_matching_generic_Func()
            {
                Assert.Null(
                    new RootDependencyResolver(
                        new DefaultProviderServicesResolver(),
                        new DatabaseInitializerResolver()).GetService<Func<DbConnection, bool, string, HistoryContext>>());
            }

            [Fact]
            public void The_root_resolver_can_return_a_default_model_cache_key_factory()
            {
                Assert.IsType<DefaultModelCacheKeyFactory>(
                    new RootDependencyResolver(
                        new DefaultProviderServicesResolver(),
                        new DatabaseInitializerResolver()).GetService<IDbModelCacheKeyFactory>());
            }

            [Fact]
            public void The_root_resolver_returns_the_SQL_Server_connection_factory()
            {
                Assert.IsType<SqlConnectionFactory>(
                    new RootDependencyResolver().GetService<IDbConnectionFactory>());
            }

            [Fact]
            public void The_root_resolver_returns_default_pluralization_service()
            {
                Assert.IsType<EnglishPluralizationService>(new RootDependencyResolver().GetService<IPluralizationService>());
            }

            [Fact]
            public void The_root_resolver_uses_the_database_initializer_resolver()
            {
                var initializer = new Mock<IDatabaseInitializer<FakeContext>>().Object;
                var mockInitializerResolver = new Mock<DatabaseInitializerResolver>();
                mockInitializerResolver.Setup(m => m.GetService(typeof(IDatabaseInitializer<FakeContext>), null)).Returns(initializer);

                Assert.Same(
                    initializer,
                    new RootDependencyResolver(
                        new DefaultProviderServicesResolver(), mockInitializerResolver.Object)
                        .GetService<IDatabaseInitializer<FakeContext>>());
            }

            [Fact]
            public void The_database_initializer_resolver_can_be_obtained_from_the_root_resolver()
            {
                var initializerResolver = new DatabaseInitializerResolver();
                Assert.Same(
                    initializerResolver,
                    new RootDependencyResolver(
                        new DefaultProviderServicesResolver(), initializerResolver)
                        .DatabaseInitializerResolver);
            }

            [Fact]
            public void The_root_resolver_returns_the_default_manifest_token_service()
            {
                Assert.IsType<DefaultManifestTokenService>(new RootDependencyResolver().GetService<IManifestTokenService>());
            }

            [Fact]
            public void The_root_resolver_returns_default_provider_factory_service()
            {
                var expectedType =
#if NET40
                typeof(Net40DefaultDbProviderFactoryService);
#else
                    typeof(DefaultDbProviderFactoryService);
#endif
                Assert.IsType(expectedType, new RootDependencyResolver().GetService<IDbProviderFactoryService>());
            }

            [Fact]
            public void The_root_resolver_uses_the_default_invariant_name_resolver_to_return_an_invariant_name()
            {
                Assert.Equal(
                    "System.Data.SqlClient",
                    new RootDependencyResolver().GetService<IProviderInvariantName>(SqlClientFactory.Instance).Name);
            }

            [Fact]
            public void The_root_resolver_uses_the_default_DbProviderFactory_resolver_to_return_a_provider_factory()
            {
                Assert.Same(
                    SqlClientFactory.Instance,
                    new RootDependencyResolver().GetService<DbProviderFactory>("System.Data.SqlClient"));
            }

            [Fact]
            public void The_root_resolver_returns_default_view_assembly_cache()
            {
                Assert.IsType<ViewAssemblyCache>(new RootDependencyResolver().GetService<IViewAssemblyCache>());
            }

            [Fact]
            public void The_root_resolver_returns_default_attribute_provider()
            {
                Assert.IsType<AttributeProvider>(new RootDependencyResolver().GetService<AttributeProvider>());
            }

            [Fact]
            public void The_root_resolver_resolves_from_secondary_resolvers_before_roots()
            {
                var attributeProvider1 = new Mock<AttributeProvider>().Object;
                var attributeProvider2 = new Mock<AttributeProvider>().Object;

                var mockSecondaryResolver1 = new Mock<IDbDependencyResolver>();
                mockSecondaryResolver1.Setup(m => m.GetService(typeof(AttributeProvider), null)).Returns(attributeProvider1);
                var mockSecondaryResolver2 = new Mock<IDbDependencyResolver>();
                mockSecondaryResolver2.Setup(m => m.GetService(typeof(AttributeProvider), null)).Returns(attributeProvider2);

                var rootResolver = new RootDependencyResolver();

                Assert.IsType<AttributeProvider>(rootResolver.GetService<AttributeProvider>());

                rootResolver.AddSecondaryResolver(mockSecondaryResolver1.Object);
                Assert.Same(attributeProvider1, rootResolver.GetService<AttributeProvider>());

                rootResolver.AddSecondaryResolver(mockSecondaryResolver2.Object);
                Assert.Same(attributeProvider2, rootResolver.GetService<AttributeProvider>());

                Assert.IsType<ViewAssemblyCache>(new RootDependencyResolver().GetService<IViewAssemblyCache>());
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
                for (var i = 0; i < 30; i++)
                {
                    var bag = new ConcurrentBag<AttributeProvider>();

                    var resolver = new RootDependencyResolver();
                    resolver.AddSecondaryResolver(new SingletonDependencyResolver<AttributeProvider>(new AttributeProvider()));
                    resolver.AddSecondaryResolver(new SingletonDependencyResolver<AttributeProvider>(new AttributeProvider()));

                    ExecuteInParallel(() => bag.Add(resolver.GetService<AttributeProvider>()));

                    Assert.Equal(20, bag.Count);
                    Assert.True(bag.All(c => c.GetType() == typeof(AttributeProvider)));
                }
            }

            public class FakeContext : DbContext
            {
            }

            [Fact]
            public void The_root_resolver_returns_the_default_DbCommandLoggerFactory()
            {
                var factory =
                    new RootDependencyResolver(new DefaultProviderServicesResolver(), new DatabaseInitializerResolver())
                        .GetService<DbCommandLoggerFactory>();

                Assert.IsType<DbCommandLoggerFactory>(factory);

                var context = new Mock<DbContext>().Object;
                Action<string> sink = new StringWriter().Write;

                var logger = factory(context, sink);

                Assert.IsType<DbCommandLogger>(logger);
                Assert.Same(context, logger.Context);
                Assert.Same(sink, logger.Sink);
            }
        }

        public class GetServices : TestBase
        {
            [Fact]
            public void The_root_resolver_resolves_from_secondary_resolvers_and_roots()
            {
                var attributeProvider1 = new Mock<AttributeProvider>().Object;
                var attributeProvider2 = new Mock<AttributeProvider>().Object;

                var mockSecondaryResolver1 = new Mock<IDbDependencyResolver>();
                mockSecondaryResolver1.Setup(m => m.GetServices(typeof(AttributeProvider), null)).Returns(new object[] { attributeProvider1 });
                var mockSecondaryResolver2 = new Mock<IDbDependencyResolver>();
                mockSecondaryResolver2.Setup(m => m.GetServices(typeof(AttributeProvider), null)).Returns(new object[] { attributeProvider2 });

                var rootResolver = new RootDependencyResolver();

                var defaultProvider = rootResolver.GetServices<AttributeProvider>().Single();
                Assert.IsType<AttributeProvider>(defaultProvider);

                rootResolver.AddSecondaryResolver(mockSecondaryResolver1.Object);
                rootResolver.AddSecondaryResolver(mockSecondaryResolver2.Object);

                var attributeProviders = rootResolver.GetServices<AttributeProvider>().ToList();
                
                Assert.Equal(3, attributeProviders.Count);
                Assert.Same(attributeProvider2, attributeProviders[0]);
                Assert.Same(attributeProvider1, attributeProviders[1]);
                Assert.Same(defaultProvider, attributeProviders[2]);

                Assert.IsType<ViewAssemblyCache>(new RootDependencyResolver().GetServices<IViewAssemblyCache>().Single());
            }

            /// <summary>
            ///     This test makes calls from multiple threads such that we have at least some chance of finding threading
            ///     issues. As with any test of this type just because the test passes does not mean that the code is
            ///     correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
            ///     be investigated. DON'T just re-run and think things are okay if the test then passes.
            /// </summary>
            [Fact]
            public void GetServices_can_be_accessed_from_multiple_threads_concurrently()
            {
                for (var i = 0; i < 30; i++)
                {
                    var bag = new ConcurrentBag<AttributeProvider>();

                    var resolver = new RootDependencyResolver();
                    resolver.AddSecondaryResolver(new SingletonDependencyResolver<AttributeProvider>(new AttributeProvider()));
                    resolver.AddSecondaryResolver(new SingletonDependencyResolver<AttributeProvider>(new AttributeProvider()));

                    ExecuteInParallel(() => resolver.GetServices<AttributeProvider>().Each(bag.Add));

                    Assert.Equal(60, bag.Count);
                    Assert.True(bag.All(c => c.GetType() == typeof(AttributeProvider)));
                }
            }
        }
    }
}
