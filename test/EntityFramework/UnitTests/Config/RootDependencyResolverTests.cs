namespace System.Data.Entity.Config
{
    using System.Collections.Concurrent;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.SqlServer;
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

            var resolver = new RootDependencyResolver(new MigrationsConfigurationResolver(), mockProviderResolver.Object);

            Assert.Same(providerServices, resolver.GetService<DbProviderServices>("FooClient"));
            mockProviderResolver.Verify(m => m.GetService(typeof(DbProviderServices), "FooClient"), Times.Once());
            Assert.Same(providerServices, resolver.GetService<DbProviderServices>("FooClient"));
            mockProviderResolver.Verify(m => m.GetService(typeof(DbProviderServices), "FooClient"), Times.Once());
        }

        [Fact]
        public void The_root_resolver_uses_the_migrations_configuration_resolver()
        {
            var sqlGenerator = new Mock<MigrationSqlGenerator>().Object;
            var mockMigrationsResolver = new Mock<MigrationsConfigurationResolver>();
            mockMigrationsResolver
                .Setup(m => m.GetService(typeof(MigrationSqlGenerator), "FooClient"))
                .Returns(sqlGenerator);

            Assert.Same(
                sqlGenerator,
                new RootDependencyResolver(mockMigrationsResolver.Object, new DefaultProviderServicesResolver())
                    .GetService<MigrationSqlGenerator>("FooClient"));
        }

        [Fact]
        public void The_root_resolver_can_return_a_default_model_cache_key_factory()
        {
            Assert.IsType<DefaultModelCacheKeyFactory>(
                new RootDependencyResolver(new MigrationsConfigurationResolver(), new DefaultProviderServicesResolver()).GetService
                    <IDbModelCacheKeyFactory>());
        }

        [Fact]
        public void The_root_resolver_returns_the_SQL_Server_connection_factory()
        {
            Assert.IsType<SqlConnectionFactory>(
                new RootDependencyResolver(new MigrationsConfigurationResolver(), new DefaultProviderServicesResolver()).GetService
                    <IDbConnectionFactory>());
        }

        [Fact]
        public void Release_does_not_throw()
        {
            new RootDependencyResolver(new MigrationsConfigurationResolver(), new DefaultProviderServicesResolver()).Release(new object());
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
                var bag = new ConcurrentBag<DbProviderServices>();

                var resolver = new RootDependencyResolver(
                    new MigrationsConfigurationResolver(),
                    new DefaultProviderServicesResolver());

                ExecuteInParallel(() => bag.Add(resolver.GetService<DbProviderServices>("System.Data.SqlClient")));

                Assert.Equal(20, bag.Count);
                Assert.True(bag.All(c => SqlProviderServices.Instance == c));
            }
        }
    }
}
