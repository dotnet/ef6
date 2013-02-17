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
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.SqlServer;
    using System.Data.SqlClient;
    using System.Linq;
    using Moq;
    using Xunit;

    public class RootDependencyResolverTests : TestBase
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
        public void The_root_resolver_can_return_a_default_history_context_factory()
        {
            Assert.IsType<DefaultHistoryContextFactory>(
                new RootDependencyResolver(
                    new DefaultProviderServicesResolver(),
                    new DatabaseInitializerResolver()).GetService<IHistoryContextFactory>());
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
        public void The_root_resolver_returns_the_default_command_interceptor_service()
        {
            Assert.IsType<DefaultCommandInterceptor>(new RootDependencyResolver().GetService<IDbCommandInterceptor>());
        }

        [Fact]
        public void The_root_resolver_returns_the_default_sql_generators()
        {
            Assert.IsType<SqlServerMigrationSqlGenerator>(
                new RootDependencyResolver().GetService<MigrationSqlGenerator>("System.Data.SqlClient"));
            Assert.IsType<SqlCeMigrationSqlGenerator>(
                new RootDependencyResolver().GetService<MigrationSqlGenerator>("System.Data.SqlServerCe.4.0"));
        }

        [Fact]
        public void The_root_resolver_returns_default_provider_factory_service()
        {
            Assert.IsType<DefaultDbProviderFactoryService>(new RootDependencyResolver().GetService<IDbProviderFactoryService>());
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
                var bag = new ConcurrentBag<DbProviderServices>();

                var resolver = new RootDependencyResolver();

                ExecuteInParallel(() => bag.Add(resolver.GetService<DbProviderServices>("System.Data.SqlClient")));

                Assert.Equal(20, bag.Count);
                Assert.True(bag.All(c => SqlProviderServices.Instance == c));
            }
        }

        public class FakeContext : DbContext
        {
        }
    }
}
