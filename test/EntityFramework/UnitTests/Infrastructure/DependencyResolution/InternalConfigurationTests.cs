// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Data.Entity.Infrastructure.Interception;
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
                    new RootDependencyResolver(), new Mock<AppConfigDependencyResolver>().Object)
                    .AddAppConfigResolver(resolver);

                mockAppConfigChain.Verify(m => m.Add(resolver));
            }
        }

        public class RegisterSingleton
        {
            [Fact]
            public void Adds_a_singleton_resolver()
            {
                var mockNormalChain = new Mock<ResolverChain>();

                new InternalConfiguration(
                    new Mock<ResolverChain>().Object, mockNormalChain.Object,
                    new RootDependencyResolver(),
                    new Mock<AppConfigDependencyResolver>().Object).RegisterSingleton(new object());

                mockNormalChain.Verify(m => m.Add(It.IsAny<SingletonDependencyResolver<object>>()));
            }

            [Fact]
            public void Adds_a_singleton_resolver_with_a_key()
            {
                var normalChain = new ResolverChain();

                new InternalConfiguration(
                    new Mock<ResolverChain>().Object, normalChain,
                    new RootDependencyResolver(),
                    new Mock<AppConfigDependencyResolver>().Object).RegisterSingleton("Bilbo", "Baggins");

                Assert.Equal("Bilbo", normalChain.GetService<string>("Baggins"));
                Assert.Null(normalChain.GetService<string>("Biggins"));
            }

            [Fact]
            public void Adds_a_singleton_resolver_with_a_key_predicate()
            {
                var normalChain = new ResolverChain();

                new InternalConfiguration(
                    new Mock<ResolverChain>().Object, normalChain,
                    new RootDependencyResolver(),
                    new Mock<AppConfigDependencyResolver>().Object).RegisterSingleton("Bilbo", k => ((string)k).StartsWith("B"));

                Assert.Equal("Bilbo", normalChain.GetService<string>("Baggins"));
                Assert.Equal("Bilbo", normalChain.GetService<string>("Biggins"));
                Assert.Null(normalChain.GetService<string>("More than half a Brandybuck"));
            }
        }

        public class GetService
        {
            [Fact]
            public void Queries_resolvers_for_service()
            {
                var mockNormalChain = new Mock<ResolverChain>();

                new InternalConfiguration(
                    new Mock<ResolverChain>().Object, mockNormalChain.Object,
                    new RootDependencyResolver(),
                    new Mock<AppConfigDependencyResolver>().Object).GetService<object>(42);

                mockNormalChain.Verify(m => m.GetService(typeof(object), 42));
            }
        }

        public class AddDependencyResolver
        {
            [Fact]
            public void AddDependencyResolver_adds_a_resolver_to_the_normal_chain()
            {
                var mockNormalChain = new Mock<ResolverChain>();
                var resolver = new Mock<IDbDependencyResolver>().Object;

                new InternalConfiguration(
                    new Mock<ResolverChain>().Object, mockNormalChain.Object,
                    new RootDependencyResolver(),
                    new Mock<AppConfigDependencyResolver>().Object).AddDependencyResolver(resolver);

                mockNormalChain.Verify(m => m.Add(resolver));
            }

            [Fact]
            public void AddDependencyResolver_adds_a_resolver_to_the_app_config_chain_when_override_flag_is_used()
            {
                var mockAppConfigChain = new Mock<ResolverChain>();
                var resolver = new Mock<IDbDependencyResolver>().Object;

                new InternalConfiguration(
                    mockAppConfigChain.Object, new Mock<ResolverChain>().Object,
                    new RootDependencyResolver(),
                    new Mock<AppConfigDependencyResolver>().Object).AddDependencyResolver(resolver, overrideConfigFile: true);

                mockAppConfigChain.Verify(m => m.Add(resolver));
            }
        }

        public class AddSecondaryResolver
        {
            [Fact]
            public void AddSecondaryResolver_adds_a_secondary_resolver_to_the_root()
            {
                var mockRootResolver = new Mock<RootDependencyResolver>();
                var resolver = new Mock<IDbDependencyResolver>().Object;

                new InternalConfiguration(
                    new Mock<ResolverChain>().Object,
                    new Mock<ResolverChain>().Object,
                    mockRootResolver.Object,
                    new Mock<AppConfigDependencyResolver>().Object).AddSecondaryResolver(resolver);

                mockRootResolver.Verify(m => m.AddSecondaryResolver(resolver));
            }

            [Fact]
            public void AddDependencyResolver_adds_a_resolver_to_the_app_config_chain_when_override_flag_is_used()
            {
                var mockAppConfigChain = new Mock<ResolverChain>();
                var resolver = new Mock<IDbDependencyResolver>().Object;

                new InternalConfiguration(
                    mockAppConfigChain.Object, new Mock<ResolverChain>().Object,
                    new RootDependencyResolver(),
                    new Mock<AppConfigDependencyResolver>().Object).AddDependencyResolver(resolver, overrideConfigFile: true);

                mockAppConfigChain.Verify(m => m.Add(resolver));
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
                    new RootDependencyResolver(),
                    new Mock<AppConfigDependencyResolver>().Object);
                var resolver = (CompositeResolver<ResolverChain, ResolverChain>)config.DependencyResolver;

                Assert.Same(mockAppConfigChain.Object, resolver.First);
                Assert.Same(mockNormalChain.Object, resolver.Second);
            }
        }

        public class Lock
        {
            [Fact]
            public void All_interceptors_registered_in_DbConfiguration_are_added_when_the_config_is_locked()
            {
                var mockAppConfigChain = new Mock<ResolverChain>();
                var mockNormalChain = new Mock<ResolverChain>();
                var interceptor1 = new Mock<IDbInterceptor>().Object;
                var interceptor2 = new Mock<IDbInterceptor>().Object;
                mockNormalChain
                    .Setup(m => m.GetServices(typeof(IDbInterceptor), null))
                    .Returns(new[] { interceptor1, interceptor2 });

                var mockDispatchers = new Mock<DbDispatchers>();

                var config = new InternalConfiguration(
                    mockAppConfigChain.Object, mockNormalChain.Object,
                    new RootDependencyResolver(),
                    new Mock<AppConfigDependencyResolver>().Object,
                    () => mockDispatchers.Object);

                config.Lock();

                mockNormalChain.Verify(m => m.GetServices(typeof(IDbInterceptor), null));
                mockDispatchers.Verify(m => m.AddInterceptor(interceptor1));
                mockDispatchers.Verify(m => m.AddInterceptor(interceptor2));
            }
        }

        public class RootResolver
        {
            [Fact]
            public void RootResolver_returns_the_root_resolver()
            {
                var rootResolver = new RootDependencyResolver();

                var config = new InternalConfiguration(
                    new Mock<ResolverChain>().Object,
                    new Mock<ResolverChain>().Object,
                    rootResolver,
                    new Mock<AppConfigDependencyResolver>().Object);

                Assert.Same(rootResolver, config.RootResolver);
            }

            [Fact]
            public void RootResolver_is_added_to_the_non_app_config_resolver_chain()
            {
                var normalChain = new ResolverChain();
                var mockRootResolver = new Mock<RootDependencyResolver>();

                new InternalConfiguration(
                    new Mock<ResolverChain>().Object,
                    normalChain, mockRootResolver.Object,
                    new Mock<AppConfigDependencyResolver>().Object);

                normalChain.GetService<object>("Foo");

                mockRootResolver.Verify(m => m.GetService(typeof(object), "Foo"));
            }
        }

        public class SwitchInRootResolver
        {
            [Fact]
            public void SwitchInRootResolver_swicthes_in_given_root_resolver()
            {
                var configuration = new DbConfiguration().InternalConfiguration;
                var mockRootResolver = new Mock<RootDependencyResolver>();
                configuration.SwitchInRootResolver(mockRootResolver.Object);

                Assert.Same(mockRootResolver.Object, configuration.RootResolver);

                configuration.DependencyResolver.GetService<object>("Foo");
                mockRootResolver.Verify(m => m.GetService(typeof(object), "Foo"));
            }

            [Fact]
            public void SwitchInRootResolver_leaves_other_resolvers_intact()
            {
                var configuration = new DbConfiguration().InternalConfiguration;
                var mockResolver = new Mock<IDbDependencyResolver>();
                configuration.AddDependencyResolver(mockResolver.Object);

                configuration.SwitchInRootResolver(new Mock<RootDependencyResolver>().Object);

                configuration.DependencyResolver.GetService<object>("Foo");
                mockResolver.Verify(m => m.GetService(typeof(object), "Foo"));
            }
        }

        public class ResolverSnapshot
        {
            [Fact]
            public void ResolverSnapshot_returns_copy_of_resolver_chain()
            {
                var configuration = new DbConfiguration().InternalConfiguration;
                var resolver1 = new Mock<IDbDependencyResolver>();
                configuration.AddDependencyResolver(resolver1.Object);

                var snapshot = configuration.ResolverSnapshot;

                var resolver2 = new Mock<IDbDependencyResolver>();
                configuration.AddDependencyResolver(resolver2.Object);

                snapshot.GetService<object>("Foo");
                resolver1.Verify(m => m.GetService(typeof(object), "Foo"), Times.Once());
                resolver2.Verify(m => m.GetService(typeof(object), "Foo"), Times.Never());
            }

            [Fact]
            public void ResolverSnapshot_returns_resolvers_in_correct_order()
            {
                var callOrder = "";

                var configResolver = new Mock<IDbDependencyResolver>();
                configResolver.Setup(m => m.GetService(typeof(object), "Foo")).Callback(() => callOrder += " Config");
                var configChain = new ResolverChain();
                configChain.Add(configResolver.Object);

                var rootResolver = new Mock<RootDependencyResolver>();
                rootResolver.Setup(m => m.GetService(typeof(object), "Foo")).Callback(() => callOrder += " Root");

                var configuration = new DbConfiguration(
                    new InternalConfiguration(
                        configChain,
                        new ResolverChain(),
                        rootResolver.Object,
                        new Mock<AppConfigDependencyResolver>().Object)).InternalConfiguration;

                var normalResolver = new Mock<IDbDependencyResolver>();
                normalResolver.Setup(m => m.GetService(typeof(object), "Foo")).Callback(() => callOrder += " Normal");
                configuration.AddDependencyResolver(normalResolver.Object);

                var overrideResolver = new Mock<IDbDependencyResolver>();
                overrideResolver.Setup(m => m.GetService(typeof(object), "Foo")).Callback(() => callOrder += " Override");
                configuration.AddDependencyResolver(overrideResolver.Object, overrideConfigFile: true);

                configuration.ResolverSnapshot.GetService<object>("Foo");

                Assert.Equal(" Override Config Normal Root", callOrder);
            }
        }
    }
}
