// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using Moq;
    using Xunit;

    public class InternalConfigurationTests
    {
        public class AddAppConfigResolver
        {
            [Fact]
            public void AddAppConfigResolver_adds_a_resolver_to_the_app_config_chain()
            {
                var mockAppConfigChain = new Mock<ResolverChain>();
                var resolver = new Mock<IDbDependencyResolver>().Object;

                new InternalConfiguration(
                    mockAppConfigChain.Object, new Mock<ResolverChain>().Object,
                    new RootDependencyResolver()).
                    AddAppConfigResolver(resolver);

                mockAppConfigChain.Verify(m => m.Add(resolver));
            }
        }

        public class AddDependencyResolver
        {
            [Fact]
            public void AddDependencyResolver_throws_if_the_configuation_is_locked()
            {
                var configuration = new InternalConfiguration();
                configuration.Lock();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddDependencyResolver"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddDependencyResolver(new Mock<IDbDependencyResolver>().Object)).Message);
            }

            [Fact]
            public void AddDependencyResolver_adds_a_resolver_to_the_normal_chain()
            {
                var mockNormalChain = new Mock<ResolverChain>();
                var resolver = new Mock<IDbDependencyResolver>().Object;

                new InternalConfiguration(
                    new Mock<ResolverChain>().Object, mockNormalChain.Object,
                    new RootDependencyResolver()).AddDependencyResolver(resolver);

                mockNormalChain.Verify(m => m.Add(resolver));
            }
        }

        public class AddProvider
        {
            [Fact]
            public void AddProvider_throws_if_the_configuation_is_locked()
            {
                var configuration = new InternalConfiguration();
                configuration.Lock();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddProvider"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddProvider("Karl", new Mock<DbProviderServices>().Object)).Message);
            }
        }

        public class GetProvider
        {
            [Fact]
            public void GetProvider_returns_provider_added_by_AddProvider()
            {
                var configuration = new InternalConfiguration();
                var provider = new Mock<DbProviderServices>().Object;

                configuration.AddProvider("Karl", provider);

                Assert.Same(provider, configuration.GetProvider("Karl"));
            }
        }

        public class DefaultConnectionFactory
        {
            [Fact]
            public void Setting_DefaultConnectionFactory_throws_if_the_configuation_is_locked()
            {
                var configuration = new InternalConfiguration();
                configuration.Lock();

                Assert.Equal(
                    Strings.ConfigurationLocked("DefaultConnectionFactory"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.DefaultConnectionFactory = new Mock<IDbConnectionFactory>().Object).Message);
            }

            [Fact]
            public void Getting_DefaultConnectionFactory_returns_factory_previously_set()
            {
                var configuration = new InternalConfiguration();
                var factory = new Mock<IDbConnectionFactory>().Object;

                configuration.DefaultConnectionFactory = factory;

                Assert.Same(factory, configuration.DefaultConnectionFactory);
            }

            [Fact]
            public void Getting_DefaultConnectionFactory_returns_factory_set_by_legacy_API()
            {
                var configuration = new InternalConfiguration();
                var legacyFactory = new Mock<IDbConnectionFactory>().Object;
                var factory = new Mock<IDbConnectionFactory>().Object;

                try
                {
#pragma warning disable 612,618
                    Database.DefaultConnectionFactory = legacyFactory;
#pragma warning restore 612,618

                    configuration.DefaultConnectionFactory = factory;

                    Assert.Same(legacyFactory, configuration.DefaultConnectionFactory);
                }
                finally
                {
                    Database.ResetDefaultConnectionFactory();
                }
            }

            [Fact]
            public void The_app_config_chain_is_prefered_over_the_normal_chain()
            {
                var mockAppConfigChain = new Mock<ResolverChain>();
                var configService = new Mock<IDbConnectionFactory>().Object;
                mockAppConfigChain.Setup(m => m.GetService(typeof(IDbConnectionFactory), It.IsAny<string>())).Returns(configService);

                var mockNormalChain = new Mock<ResolverChain>();
                var normalService = new Mock<IDbConnectionFactory>().Object;
                mockNormalChain.Setup(m => m.GetService(typeof(IDbConnectionFactory), It.IsAny<string>())).Returns(normalService);

                Assert.Same(
                    configService,
                    new InternalConfiguration(
                        mockAppConfigChain.Object, mockNormalChain.Object,
                        new RootDependencyResolver()).
                        DefaultConnectionFactory);

                mockAppConfigChain.Verify(m => m.GetService(typeof(IDbConnectionFactory), It.IsAny<string>()), Times.Once());
                mockNormalChain.Verify(m => m.GetService(typeof(IDbConnectionFactory), It.IsAny<string>()), Times.Never());
            }
        }

        public class DependencyResolver
        {
            [Fact]
            public void DependencyResolver_returns_the_dependency_resolver_in_use()
            {
                var mockAppConfigChain = new Mock<ResolverChain>();
                var mockNormalChain = new Mock<ResolverChain>();

                var config = new InternalConfiguration(
                    mockAppConfigChain.Object, mockNormalChain.Object,
                    new RootDependencyResolver());
                var resolver = (CompositeResolver<ResolverChain, ResolverChain>)config.DependencyResolver;

                Assert.Same(mockAppConfigChain.Object, resolver.First);
                Assert.Same(mockNormalChain.Object, resolver.Second);
            }
        }

        public class RootResolver
        {
            [Fact]
            public void RootResolver_returns_the_root_resolver()
            {
                var rootResolver = new RootDependencyResolver();

                var config = new InternalConfiguration(new Mock<ResolverChain>().Object, new Mock<ResolverChain>().Object, rootResolver);

                Assert.Same(rootResolver, config.RootResolver);
            }

            [Fact]
            public void RootResolver_is_added_to_the_non_app_config_resolver_chain()
            {
                var normalChain = new ResolverChain();
                var mockRootResolver = new Mock<RootDependencyResolver>();

                new InternalConfiguration(new Mock<ResolverChain>().Object, normalChain, mockRootResolver.Object);

                normalChain.GetService<object>("Foo");

                mockRootResolver.Verify(m => m.GetService(typeof(object), "Foo"));
            }
        }

        public class SwitchInRootResolver
        {
            [Fact]
            public void SwitchInRootResolver_swicthes_in_given_root_resolver()
            {
                var configuration = new InternalConfiguration();
                var mockRootResolver = new Mock<RootDependencyResolver>();
                configuration.SwitchInRootResolver(mockRootResolver.Object);

                Assert.Same(mockRootResolver.Object, configuration.RootResolver);

                configuration.DependencyResolver.GetService<object>("Foo");
                mockRootResolver.Verify(m => m.GetService(typeof(object), "Foo"));
            }

            [Fact]
            public void SwitchInRootResolver_leaves_other_resolvers_intact()
            {
                var configuration = new InternalConfiguration();
                var mockResolver = new Mock<IDbDependencyResolver>();
                configuration.AddDependencyResolver(mockResolver.Object);

                configuration.SwitchInRootResolver(new Mock<RootDependencyResolver>().Object);

                configuration.DependencyResolver.GetService<object>("Foo");
                mockResolver.Verify(m => m.GetService(typeof(object), "Foo"));
            }
        }
    }
}
