namespace System.Data.Entity.Config
{
    using System.Configuration;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.ConfigFile;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using Moq;
    using ProductivityApiUnitTests;
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
            Assert.Null(new AppConfigDependencyResolver(CreateAppConfig()).GetService<IPilkington>("Karl"));
        }

        [Fact]
        public void Get_returns_registered_provider()
        {
            Assert.Same(
                ProviderServicesFactoryTests.FakeProviderWithPublicProperty.Instance,
                new AppConfigDependencyResolver(CreateAppConfigWithProvider()).GetService<DbProviderServices>("Is.Ee.Avin.A.Larf"));
        }

        [Fact]
        public void Get_returns_null_for_unregistered_provider_name()
        {
            Assert.Null(
                new AppConfigDependencyResolver(CreateAppConfigWithProvider()).GetService<DbProviderServices>("Are.You.Avin.A.Larf"));
        }

        [Fact]
        public void Get_returns_null_for_null_empty_or_whitespace_provider_name()
        {
            Assert.Null(new AppConfigDependencyResolver(CreateAppConfig()).GetService<DbProviderServices>(null));
            Assert.Null(new AppConfigDependencyResolver(CreateAppConfig()).GetService<DbProviderServices>(""));
            Assert.Null(new AppConfigDependencyResolver(CreateAppConfig()).GetService<DbProviderServices>(" "));
        }

        [Fact]
        public void Get_returns_registered_Migrations_SQL_generator()
        {
            Assert.IsType<AppConfigTests.TryGetMigrationSqlGenerator.MySqlGenerator>(
                new AppConfigDependencyResolver(CreateAppConfigWithProvider(
                    typeof(AppConfigTests.TryGetMigrationSqlGenerator.MySqlGenerator).AssemblyQualifiedName))
                    .GetService<MigrationSqlGenerator>("Is.Ee.Avin.A.Larf"));
        }

        [Fact]
        public void Get_returns_null_SQL_generator_for_unregistered_provider()
        {
            Assert.Null(
                new AppConfigDependencyResolver(CreateAppConfigWithProvider()).GetService<MigrationSqlGenerator>("Are.You.Avin.A.Larf"));
        }

        [Fact]
        public void Get_returns_null_for_registered_provider_with_no_registered_Migrations_SQL_generator()
        {
            Assert.Null(
                new AppConfigDependencyResolver(CreateAppConfigWithProvider()).GetService<MigrationSqlGenerator>("Is.Ee.Avin.A.Larf"));
        }

        [Fact]
        public void Get_returns_null_when_asked_for_SQL_generator_for_null_empty_or_whitespace_provider_name()
        {
            Assert.Null(new AppConfigDependencyResolver(CreateAppConfig()).GetService<MigrationSqlGenerator>(null));
            Assert.Null(new AppConfigDependencyResolver(CreateAppConfig()).GetService<MigrationSqlGenerator>(""));
            Assert.Null(new AppConfigDependencyResolver(CreateAppConfig()).GetService<MigrationSqlGenerator>(" "));
        }

        [Fact]
        public void Get_returns_connection_factory_set_in_config()
        {
            Assert.IsType<FakeConnectionFactory>(
                new AppConfigDependencyResolver(
                    new AppConfig(
                        CreateEmptyConfig().AddDefaultConnectionFactory(typeof(FakeConnectionFactory).AssemblyQualifiedName)))
                    .GetService<IDbConnectionFactory>());
        }

        [Fact]
        public void Get_returns_null_if_no_connection_factory_is_set_in_config()
        {
            Assert.Null(new AppConfigDependencyResolver(CreateAppConfig()).GetService<IDbConnectionFactory>());
        }

        [Fact]
        public void Release_does_not_throw()
        {
            new AppConfigDependencyResolver(CreateAppConfig()).Release(new object());
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

        private static AppConfig CreateAppConfigWithProvider(string sqlGeneratorName = null)
        {
            return CreateAppConfig(
                "Is.Ee.Avin.A.Larf",
                typeof(ProviderServicesFactoryTests.FakeProviderWithPublicProperty).AssemblyQualifiedName,
                sqlGeneratorName);
        }

        private static AppConfig CreateAppConfig(string invariantName = null, string typeName = null, string sqlGeneratorName = null)
        {
            var mockEFSection = new Mock<EntityFrameworkSection>();
            mockEFSection.Setup(m => m.DefaultConnectionFactory).Returns(new DefaultConnectionFactoryElement());

            var providers = new ProviderCollection();
            if (!string.IsNullOrEmpty(invariantName))
            {
                var providerElement = providers.AddProvider(invariantName, typeName);
                if (sqlGeneratorName != null)
                {
                    providerElement.SqlGeneratorElement = new MigrationSqlGeneratorElement
                        {
                            SqlGeneratorTypeName = sqlGeneratorName
                        };
                }
            }
            mockEFSection.Setup(m => m.Providers).Returns(providers);

            return new AppConfig(new ConnectionStringSettingsCollection(), null, mockEFSection.Object);
        }
    }
}
