// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using Moq;
    using Xunit;

    public class WrappingDependencyResolverTests : TestBase
    {
        public interface IPilkington
        {
        }

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
}
