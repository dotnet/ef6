// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
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

        public class RegisterSingleton
        {
            [Fact]
            public void Adds_a_singleton_resolver()
            {
                var mockNormalChain = new Mock<ResolverChain>();

                new InternalConfiguration(
                    new Mock<ResolverChain>().Object, mockNormalChain.Object,
                    new RootDependencyResolver()).RegisterSingleton(new object(), null);

                mockNormalChain.Verify(m => m.Add(It.IsAny<SingletonDependencyResolver<object>>()));
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
                    new RootDependencyResolver()).GetService<object>(42);

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
                    new RootDependencyResolver()).AddDependencyResolver(resolver);

                mockNormalChain.Verify(m => m.Add(resolver));
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
