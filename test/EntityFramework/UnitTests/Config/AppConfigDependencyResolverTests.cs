namespace System.Data.Entity.Config
{
    using System.Configuration;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.ConfigFile;
    using Moq;
    using Xunit;

    public class AppConfigDependencyResolverTests : TestBase
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
        public void Get_returns_null_for_unknown_contract_type()
        {
            Assert.Null(new AppConfigDependencyResolver(CreateAppConfig()).Get<IPilkington>("Karl"));
        }

        [Fact]
        public void Get_returns_registered_provider()
        {
            Assert.Same(
                ProviderServicesFactoryTests.FakeProviderWithPublicProperty.Instance,
                new AppConfigDependencyResolver(CreateAppConfigWithProvider()).Get<DbProviderServices>("Is.Ee.Avin.A.Larf"));
        }

        [Fact]
        public void Get_returns_null_for_unregistered_provider_name()
        {
            Assert.Null(
                new AppConfigDependencyResolver(CreateAppConfigWithProvider()).Get<DbProviderServices>("Are.You.Avin.A.Larf"));
        }

        [Fact]
        public void Get_returns_null_for_null_empty_or_whitespace_provider_name()
        {
            Assert.Null(new AppConfigDependencyResolver(CreateAppConfig()).Get<DbProviderServices>(null));
            Assert.Null(new AppConfigDependencyResolver(CreateAppConfig()).Get<DbProviderServices>(""));
            Assert.Null(new AppConfigDependencyResolver(CreateAppConfig()).Get<DbProviderServices>(" "));
        }

        [Fact]
        public void Get_returns_connection_factory_set_in_config()
        {
            Assert.IsType<FakeConnectionFactory>(
                new AppConfigDependencyResolver(
                    new AppConfig(
                        CreateEmptyConfig().AddDefaultConnectionFactory(typeof(FakeConnectionFactory).AssemblyQualifiedName)))
                    .Get<IDbConnectionFactory>());
        }

        [Fact]
        public void Get_returns_null_if_no_connection_factory_is_set_in_config()
        {
            Assert.Null(new AppConfigDependencyResolver(CreateAppConfig()).Get<IDbConnectionFactory>());
        }

        [Fact]
        public void Release_does_not_throw()
        {
            new AppConfigDependencyResolver(CreateAppConfig()).Release(new object());
        }

        private static AppConfig CreateAppConfigWithProvider()
        {
            return CreateAppConfig(
                "Is.Ee.Avin.A.Larf",
                typeof(ProviderServicesFactoryTests.FakeProviderWithPublicProperty).AssemblyQualifiedName);
        }

        private static AppConfig CreateAppConfig(string invariantName = null, string typeName = null)
        {
            var mockEFSection = new Mock<EntityFrameworkSection>();
            mockEFSection.Setup(m => m.DefaultConnectionFactory).Returns(new DefaultConnectionFactoryElement());

            var providers = new ProviderCollection();
            if (!string.IsNullOrEmpty(invariantName))
            {
                providers.AddProvider(invariantName, typeName);
            }
            mockEFSection.Setup(m => m.Providers).Returns(providers);

            return new AppConfig(new ConnectionStringSettingsCollection(), null, mockEFSection.Object);
        }
    }
}