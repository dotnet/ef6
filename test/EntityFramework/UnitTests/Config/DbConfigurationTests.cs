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

                mockInternalConfiguration.Verify(m => m.AddProvider("900.FTW", providerServices));
            }
        }

        public class GetProvider
        {
            [Fact]
            public void GetProvider_throws_if_given_bad_invariant_name()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(() => DbConfiguration.GetProvider(null)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(() => DbConfiguration.GetProvider("")).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(() => DbConfiguration.GetProvider(" ")).Message);
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
            public void SetDefaultConnectionFactory_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var connectionFactory = new Mock<IDbConnectionFactory>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetDefaultConnectionFactory(connectionFactory);

                mockInternalConfiguration.VerifySet(m => m.DefaultConnectionFactory = connectionFactory);
            }
        }

        public class DependencyResolver
        {
            [Fact]
            public void Default_IDbModelCacheKeyFactory_is_returned_by_default()
            {
                Assert.IsType<DefaultModelCacheKeyFactory>(DbConfiguration.DependencyResolver.GetService<IDbModelCacheKeyFactory>());
            }
        }
    }
}
