namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.SqlServerCompact;
    using Xunit;

    public class RootDependencyResolverTests
    {
        [Fact]
        public void The_root_resolver_returns_SqlProviderServices_type_for_SqlClient_invariant_name()
        {
            Assert.Same(
                SqlProviderServices.Instance,
                new RootDependencyResolver().Get<DbProviderServices>("System.Data.SqlClient"));
        }

        [Fact]
        public void The_root_resolver_can_return_a_default_model_cache_key_factory()
        {
            Assert.IsType<DefaultModelCacheKeyFactory>(new RootDependencyResolver().Get<IDbModelCacheKeyFactory>());
        }

        [Fact]
        public void The_root_resolver_returns_SqlCeProviderServices_type_for_Sql_Compact_invariant_name()
        {
            Assert.Same(
                SqlCeProviderServices.Instance,
                new RootDependencyResolver().Get<DbProviderServices>("System.Data.SqlServerCe.4.0"));
        }

        [Fact]
        public void The_root_resolver_throws_for_an_unknown_provider_name()
        {
            Assert.Equal(
                Strings.EF6Providers_NoProviderFound("Don't.Come.Around.Here.No.More"),
                Assert.Throws<InvalidOperationException>(
                    () => new RootDependencyResolver().Get<DbProviderServices>("Don't.Come.Around.Here.No.More")).Message);
        }

        [Fact]
        public void The_root_resolver_returns_the_SQL_Server_connection_factory()
        {
            Assert.IsType<SqlConnectionFactory>(new RootDependencyResolver().Get<IDbConnectionFactory>());
        }

        [Fact]
        public void The_root_resolver_throws_for_an_empty_provider_name()
        {
            Assert.Equal(
                Strings.ProviderInvariantNotPassedToResolver,
                Assert.Throws<ArgumentException>(
                    () => new RootDependencyResolver().Get<DbProviderServices>(null)).Message);

            Assert.Equal(
                Strings.ProviderInvariantNotPassedToResolver,
                Assert.Throws<ArgumentException>(
                    () => new RootDependencyResolver().Get<DbProviderServices>("")).Message);

            Assert.Equal(
                Strings.ProviderInvariantNotPassedToResolver,
                Assert.Throws<ArgumentException>(
                    () => new RootDependencyResolver().Get<DbProviderServices>(" ")).Message);
        }

        [Fact]
        public void Release_does_not_throw()
        {
            new RootDependencyResolver().Release(new object());
        }
    }
}