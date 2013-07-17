// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Linq;
    using Moq;
    using Xunit;

    public class WrappingDependencyResolverTests : TestBase
    {
        public interface IPilkington
        {
        }

        public class GetService : TestBase
        {
            [Fact]
            public void GetService_returns_null_when_contract_type_does_not_match()
            {
                var resolver = new WrappingDependencyResolver<IPilkington>(
                    new Mock<IDbDependencyResolver>().Object,
                    (s, k) =>
                    {
                        Assert.True(false);
                        return s;
                    });

                Assert.Null(resolver.GetService<Random>());
            }

            [Fact]
            public void GetService_wraps_service_and_returns_wrapped_service()
            {
                var originalService = new Mock<IPilkington>().Object;
                var wrappedService = new Mock<IPilkington>().Object;

                var snapshot = new Mock<IDbDependencyResolver>();
                snapshot.Setup(m => m.GetService(typeof(IPilkington), "Foo")).Returns(originalService);

                var resolver = new WrappingDependencyResolver<IPilkington>(
                    snapshot.Object,
                    (s, k) =>
                    {
                        Assert.Same(originalService, s);
                        Assert.Equal("Foo", k);
                        return wrappedService;
                    });

                Assert.Same(wrappedService, resolver.GetService<IPilkington>("Foo"));
                snapshot.Verify(m => m.GetService(typeof(IPilkington), "Foo"));
            }
        }

        public class GetServices : TestBase
        {
            [Fact]
            public void GetServices_returns_empty_list_when_contract_type_does_not_match()
            {
                var resolver = new WrappingDependencyResolver<IPilkington>(
                    new Mock<IDbDependencyResolver>().Object,
                    (s, k) =>
                        {
                            Assert.True(false);
                            return s;
                        });

                Assert.Empty(resolver.GetServices<Random>());
            }

            [Fact]
            public void GetServices_wraps_service_and_returns_wrapped_services()
            {
                var originalService1 = new Mock<IPilkington>().Object;
                var wrappedService1 = new Mock<IPilkington>().Object;
                var originalService2 = new Mock<IPilkington>().Object;
                var wrappedService2 = new Mock<IPilkington>().Object;

                var snapshot = new Mock<IDbDependencyResolver>();
                snapshot.Setup(m => m.GetServices(typeof(IPilkington), "Foo"))
                    .Returns(new object[] { originalService1, originalService2 });

                var resolver = new WrappingDependencyResolver<IPilkington>(
                    snapshot.Object,
                    (s, k) =>
                        {
                            Assert.Equal("Foo", k);
                            return s == originalService1 ? wrappedService1 : s == originalService2 ? wrappedService2 : null;
                        });

                var pilkingtons = resolver.GetServices<IPilkington>("Foo").ToList();

                Assert.Equal(2, pilkingtons.Count);
                Assert.Same(wrappedService1, pilkingtons[0]);
                Assert.Same(wrappedService2, pilkingtons[1]);
                snapshot.Verify(m => m.GetServices(typeof(IPilkington), "Foo"));
            }
        }
    }
}
