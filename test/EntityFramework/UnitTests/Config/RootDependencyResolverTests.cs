namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.Sql;
    using Moq;
    using Xunit;

    public class RootDependencyResolverTests
    {
        [Fact]
        public void The_root_resolver_uses_the_default_provider_services_resolver()
        {
            var providerServices = new Mock<DbProviderServices>().Object;
            var mockProviderResolver = new Mock<DefaultProviderServicesResolver>();
            mockProviderResolver
                .Setup(m => m.GetService(typeof(DbProviderServices), "FooClient"))
                .Returns(providerServices);

            Assert.Same(
                providerServices,
                new RootDependencyResolver(new MigrationsConfigurationResolver(), mockProviderResolver.Object)
                    .GetService<DbProviderServices>("FooClient"));
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
    }
}
