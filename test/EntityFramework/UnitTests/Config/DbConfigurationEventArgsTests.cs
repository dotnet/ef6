// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Resources;
    using Moq;
    using Xunit;

    public class DbConfigurationEventArgsTests
    {
        public class AddDependencyResolver
        {
            [Fact]
            public void AddDependencyResolver_throws_if_given_a_null_resolver()
            {
                Assert.Equal(
                    "resolver",
                    Assert.Throws<ArgumentNullException>(
                        () => (new DbConfigurationEventArgs(new Mock<InternalConfiguration>().Object))
                                  .AddDependencyResolver(null, false)).ParamName);
            }

            [Fact]
            public void AddDependencyResolver_throws_if_the_configuation_is_locked()
            {
                var internalConfiguration = new InternalConfiguration();
                internalConfiguration.Lock();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddDependencyResolver"),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        new DbConfigurationEventArgs(internalConfiguration)
                            .AddDependencyResolver(new Mock<IDbDependencyResolver>().Object, false)).Message);
            }

            [Fact]
            public void AddDependencyResolver_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var resolver = new Mock<IDbDependencyResolver>().Object;

                new DbConfigurationEventArgs(mockInternalConfiguration.Object).AddDependencyResolver(resolver, true);

                mockInternalConfiguration.Verify(m => m.AddDependencyResolver(resolver, true));
            }
        }

        public class ResolverSnapshot
        {
            [Fact]
            public void ResolverSnapshot_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var resolver = new Mock<IDbDependencyResolver>().Object;
                mockInternalConfiguration.Setup(m => m.ResolverSnapshot).Returns(resolver);

                Assert.Same(resolver, new DbConfigurationEventArgs(mockInternalConfiguration.Object).ResolverSnapshot);
            }
        }

        public class ReplaceService
        {
            public interface IPilkington
            {
            }

            [Fact]
            public void ReplaceService_throws_if_given_a_null_delegate()
            {
                Assert.Equal(
                    "serviceInterceptor",
                    Assert.Throws<ArgumentNullException>(
                        () => (new DbConfigurationEventArgs(new Mock<InternalConfiguration>().Object))
                                  .ReplaceService<IPilkington>(null)).ParamName);
            }

            [Fact]
            public void ReplaceService_wraps_service_and_returns_wrapped_service()
            {
                var originalService = new Mock<IPilkington>().Object;
                var wrappedService = new Mock<IPilkington>().Object;
                
                var resolver = new Mock<IDbDependencyResolver>();
                resolver.Setup(m => m.GetService(typeof(IPilkington), "Foo")).Returns(originalService);

                var internalConfiguration = new DbConfiguration().InternalConfiguration;
                internalConfiguration.AddDependencyResolver(resolver.Object);

                new DbConfigurationEventArgs(internalConfiguration)
                    .ReplaceService<IPilkington>(
                    (s, k) =>
                    {
                        Assert.Same(originalService, s);
                        Assert.Equal("Foo", k);
                        return wrappedService;
                    });

                Assert.Same(wrappedService, internalConfiguration.DependencyResolver.GetService<IPilkington>("Foo"));
                resolver.Verify(m => m.GetService(typeof(IPilkington), "Foo"));
            }
        }
    }
}
