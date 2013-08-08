// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Collections.Concurrent;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping.ViewGeneration;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Interception;
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
                        .GetService<Func<DbConnection, string, HistoryContext>>();

                using (var connection = new SqlConnection())
                {
                    using (var context = factory(connection, null))
                    {
                        Assert.IsType<HistoryContext>(context);
                    }
                }
            }

            [Fact]
            public void The_root_resolver_can_return_a_default_model_cache_key_factory()
            {
                Assert.NotNull(
                    new RootDependencyResolver(
                        new DefaultProviderServicesResolver(),
                        new DatabaseInitializerResolver()).GetService<Func<DbContext, IDbModelCacheKey>>());
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
                Assert.IsType<DefaultManifestTokenResolver>(new RootDependencyResolver().GetService<IManifestTokenResolver>());
            }

            [Fact]
            public void The_root_resolver_returns_default_provider_factory_service()
            {
                var expectedType =
#if NET40
                typeof(Net40DefaultDbProviderFactoryResolver);
#else
                    typeof(DefaultDbProviderFactoryResolver);
#endif
                Assert.IsType(expectedType, new RootDependencyResolver().GetService<IDbProviderFactoryResolver>());
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
            public void The_root_resolver_returns_default_attribute_provider()
            {
                Assert.IsType<AttributeProvider>(new RootDependencyResolver().GetService<AttributeProvider>());
            }

            [Fact]
            public void The_root_resolver_resolves_from_default_resolvers_before_roots()
            {
                var attributeProvider1 = new Mock<AttributeProvider>().Object;
                var attributeProvider2 = new Mock<AttributeProvider>().Object;

                var mockDefaultResolver1 = new Mock<IDbDependencyResolver>();
                mockDefaultResolver1.Setup(m => m.GetService(typeof(AttributeProvider), null)).Returns(attributeProvider1);
                var mockDefaultResolver2 = new Mock<IDbDependencyResolver>();
                mockDefaultResolver2.Setup(m => m.GetService(typeof(AttributeProvider), null)).Returns(attributeProvider2);

                var rootResolver = new RootDependencyResolver();

                Assert.IsType<AttributeProvider>(rootResolver.GetService<AttributeProvider>());

                rootResolver.AddDefaultResolver(mockDefaultResolver1.Object);
                Assert.Same(attributeProvider1, rootResolver.GetService<AttributeProvider>());

                rootResolver.AddDefaultResolver(mockDefaultResolver2.Object);
                Assert.Same(attributeProvider2, rootResolver.GetService<AttributeProvider>());
            }

            /// <summary>
            /// This test makes calls from multiple threads such that we have at least some chance of finding threading
            /// issues. As with any test of this type just because the test passes does not mean that the code is
            /// correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
            /// be investigated. DON'T just re-run and think things are okay if the test then passes.
            /// </summary>
            [Fact]
            public void GetService_can_be_accessed_from_multiple_threads_concurrently()
            {
                for (var i = 0; i < 30; i++)
                {
                    var bag = new ConcurrentBag<AttributeProvider>();

                    var resolver = new RootDependencyResolver();
                    resolver.AddDefaultResolver(new SingletonDependencyResolver<AttributeProvider>(new AttributeProvider()));
                    resolver.AddDefaultResolver(new SingletonDependencyResolver<AttributeProvider>(new AttributeProvider()));

                    ExecuteInParallel(() => bag.Add(resolver.GetService<AttributeProvider>()));

                    Assert.Equal(20, bag.Count);
                    Assert.True(bag.All(c => c.GetType() == typeof(AttributeProvider)));
                }
            }

            public class FakeContext : DbContext
            {
            }

            [Fact]
            public void The_root_resolver_returns_the_default_command_formatter_factory()
            {
                var factory =
                    new RootDependencyResolver(new DefaultProviderServicesResolver(), new DatabaseInitializerResolver())
                        .GetService<Func<DbContext, Action<string>, DatabaseLogFormatter>>();

                var context = new Mock<DbContext>().Object;
                Action<string> sink = new StringWriter().Write;

                var formatter = factory(context, sink);

                Assert.IsType<DatabaseLogFormatter>(formatter);
                Assert.Same(context, formatter.Context);
                Assert.Same(sink, formatter.WriteAction);
            }
        }

        public class GetServices : TestBase
        {
            [Fact]
            public void The_root_resolver_resolves_from_default_resolvers_and_roots()
            {
                var attributeProvider1 = new Mock<AttributeProvider>().Object;
                var attributeProvider2 = new Mock<AttributeProvider>().Object;

                var mockDefaultResolver1 = new Mock<IDbDependencyResolver>();
                mockDefaultResolver1.Setup(m => m.GetServices(typeof(AttributeProvider), null)).Returns(new object[] { attributeProvider1 });
                var mockDefaultResolver2 = new Mock<IDbDependencyResolver>();
                mockDefaultResolver2.Setup(m => m.GetServices(typeof(AttributeProvider), null)).Returns(new object[] { attributeProvider2 });

                var rootResolver = new RootDependencyResolver();

                var defaultProvider = rootResolver.GetServices<AttributeProvider>().Single();
                Assert.IsType<AttributeProvider>(defaultProvider);

                rootResolver.AddDefaultResolver(mockDefaultResolver1.Object);
                rootResolver.AddDefaultResolver(mockDefaultResolver2.Object);

                var attributeProviders = rootResolver.GetServices<AttributeProvider>().ToList();
                
                Assert.Equal(3, attributeProviders.Count);
                Assert.Same(attributeProvider2, attributeProviders[0]);
                Assert.Same(attributeProvider1, attributeProviders[1]);
                Assert.Same(defaultProvider, attributeProviders[2]);
            }

            /// <summary>
            /// This test makes calls from multiple threads such that we have at least some chance of finding threading
            /// issues. As with any test of this type just because the test passes does not mean that the code is
            /// correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
            /// be investigated. DON'T just re-run and think things are okay if the test then passes.
            /// </summary>
            [Fact]
            public void GetServices_can_be_accessed_from_multiple_threads_concurrently()
            {
                for (var i = 0; i < 30; i++)
                {
                    var bag = new ConcurrentBag<AttributeProvider>();

                    var resolver = new RootDependencyResolver();
                    resolver.AddDefaultResolver(new SingletonDependencyResolver<AttributeProvider>(new AttributeProvider()));
                    resolver.AddDefaultResolver(new SingletonDependencyResolver<AttributeProvider>(new AttributeProvider()));

                    ExecuteInParallel(() => resolver.GetServices<AttributeProvider>().Each(bag.Add));

                    Assert.Equal(60, bag.Count);
                    Assert.True(bag.All(c => c.GetType() == typeof(AttributeProvider)));
                }
            }
        }
    }
}
