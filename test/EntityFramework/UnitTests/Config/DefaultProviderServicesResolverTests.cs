namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Resources;
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.SqlServerCompact;
    using Xunit;

    public class DefaultProviderServicesResolverTests
    {
        [Fact]
        public void The_provider_services_resolver_returns_SqlProviderServices_type_for_SqlClient_invariant_name()
        {
            Assert.Same(
                SqlProviderServices.Instance,
                new DefaultProviderServicesResolver().GetService<DbProviderServices>("System.Data.SqlClient"));
        }

        [Fact]
        public void The_provider_services_resolver_returns_SqlCeProviderServices_type_for_Sql_Compact_invariant_name()
        {
            Assert.Same(
                SqlCeProviderServices.Instance,
                new DefaultProviderServicesResolver().GetService<DbProviderServices>("System.Data.SqlServerCe.4.0"));
        }

        [Fact]
        public void The_provider_services_resolver_throws_for_an_unknown_provider_name()
        {
            Assert.Equal(
                Strings.EF6Providers_NoProviderFound("Don't.Come.Around.Here.No.More"),
                Assert.Throws<InvalidOperationException>(
                    () => new DefaultProviderServicesResolver().GetService<DbProviderServices>("Don't.Come.Around.Here.No.More")).Message);
        }

        [Fact]
        public void The_provider_services_resolver_throws_for_an_empty_provider_name()
        {
            Assert.Equal(
                Strings.ProviderInvariantNotPassedToResolver,
                Assert.Throws<ArgumentException>(
                    () => new DefaultProviderServicesResolver().GetService<DbProviderServices>(null)).Message);

            Assert.Equal(
                Strings.ProviderInvariantNotPassedToResolver,
                Assert.Throws<ArgumentException>(
                    () => new DefaultProviderServicesResolver().GetService<DbProviderServices>("")).Message);

            Assert.Equal(
                Strings.ProviderInvariantNotPassedToResolver,
                Assert.Throws<ArgumentException>(
                    () => new DefaultProviderServicesResolver().GetService<DbProviderServices>(" ")).Message);
        }

        [Fact]
        public void Release_does_not_throw()
        {
            new DefaultProviderServicesResolver().Release(new object());
        }
    }
}
