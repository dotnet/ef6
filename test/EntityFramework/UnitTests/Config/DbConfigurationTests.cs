// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using Moq;
    using Xunit;

    public class DbConfigurationTests
    {
        public class SetConfiguration
        {
            [Fact]
            public void DbConfiguration_cannot_be_set_to_null()
            {
                Assert.Equal(
                    "configuration",
                    Assert.Throws<ArgumentNullException>(() => DbConfiguration.SetConfiguration(null)).ParamName);
            }
        }

        public class AddDependencyResolver
        {
            [Fact]
            public void AddDependencyResolver_throws_if_given_a_null_resolver()
            {
                Assert.Equal(
                    "resolver",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddDependencyResolver(null)).ParamName);
            }

            [Fact]
            public void AddDependencyResolver_throws_if_the_configuation_is_locked()
            {
                var internalConfiguration = new InternalConfiguration();
                internalConfiguration.Lock();
                var configuration = new DbConfiguration(internalConfiguration);

                Assert.Equal(
                    Strings.ConfigurationLocked("AddDependencyResolver"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddDependencyResolver(new Mock<IDbDependencyResolver>().Object)).Message);
            }

            [Fact]
            public void AddDependencyResolver_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var resolver = new Mock<IDbDependencyResolver>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).AddDependencyResolver(resolver);

                mockInternalConfiguration.Verify(m => m.AddDependencyResolver(resolver));
            }
        }

        public class AddProvider
        {
            [Fact]
            public void AddProvider_throws_if_given_a_null_provider_or_bad_invariant_name()
            {
                Assert.Equal(
                    "provider",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddProvider("Karl", null)).ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddProvider(null, new Mock<DbProviderServices>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddProvider("", new Mock<DbProviderServices>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddProvider(" ", new Mock<DbProviderServices>().Object)).Message);
            }

            [Fact]
            public void AddProvider_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var providerServices = new Mock<DbProviderServices>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).AddProvider("900.FTW", providerServices);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(providerServices, "900.FTW"));
            }

            [Fact]
            public void AddProvider_throws_if_the_configuation_is_locked()
            {
                var internalConfiguration = new InternalConfiguration();
                internalConfiguration.Lock();
                var configuration = new DbConfiguration(internalConfiguration);

                Assert.Equal(
                    Strings.ConfigurationLocked("AddProvider"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddProvider("Karl", new Mock<DbProviderServices>().Object)).Message);
            }
        }

        public class SetDefaultConnectionFactory
        {
            [Fact]
            public void Setting_DefaultConnectionFactory_throws_if_given_a_null_factory()
            {
                Assert.Equal(
                    "connectionFactory",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetDefaultConnectionFactory(null)).ParamName);
            }

            [Fact]
            public void Setting_DefaultConnectionFactory_throws_if_the_configuation_is_locked()
            {
                var internalConfiguration = new InternalConfiguration();
                internalConfiguration.Lock();
                var configuration = new DbConfiguration(internalConfiguration);

                Assert.Equal(
                    Strings.ConfigurationLocked("SetDefaultConnectionFactory"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetDefaultConnectionFactory(new Mock<IDbConnectionFactory>().Object)).Message);
            }

            [Fact]
            public void SetDefaultConnectionFactory_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var connectionFactory = new Mock<IDbConnectionFactory>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetDefaultConnectionFactory(connectionFactory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(connectionFactory, null));
            }
        }

        public class RegisterSingleton
        {
            [Fact]
            public void Throws_if_the_configuation_is_locked()
            {
                var internalConfiguration = new InternalConfiguration();
                internalConfiguration.Lock();
                var configuration = new DbConfiguration(internalConfiguration);

                Assert.Equal(
                    Strings.ConfigurationLocked("RegisterSingleton"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.RegisterSingleton(new object(), null)).Message);
            }

            [Fact]
            public void Throws_if_given_a_null_instance()
            {
                Assert.Equal(
                    "instance",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().RegisterSingleton<object>(null)).ParamName);
            }

            [Fact]
            public void Delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();

                var instance = new object();
                new DbConfiguration(mockInternalConfiguration.Object).RegisterSingleton(instance, 42);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(instance, 42));
            }
        }

        public class GetService
        {
            [Fact]
            public void Delegates_to_internal_configuration()
            {
                Assert.NotNull(DbConfiguration.GetService<IDbCommandInterceptor>(null));
            }
        }

        public class DependencyResolver
        {
            [Fact]
            public void Default_IDbModelCacheKeyFactory_is_returned_by_default()
            {
                Assert.IsType<DefaultModelCacheKeyFactory>(DbConfiguration.GetService<IDbModelCacheKeyFactory>());
            }
        }
    }
}
