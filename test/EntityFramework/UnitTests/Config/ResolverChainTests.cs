namespace System.Data.Entity.Config
{
    using Moq;
    using Xunit;

    public class ResolverChainTests
    {
        public interface IPilkington
        {
        }

        [Fact]
        public void Get_returns_null_for_empty_chain()
        {
            Assert.Null(new ResolverChain().Get<IPilkington>());
        }

        [Fact]
        public void Get_returns_null_if_no_resolver_in_the_chain_resolves_the_dependency()
        {
            var mockResolver1 = CreateMockResolver("Steve", new Mock<IPilkington>().Object);
            var mockResolver2 = CreateMockResolver("Ricky", new Mock<IPilkington>().Object);

            var chain = new ResolverChain();
            chain.Add(mockResolver1.Object);
            chain.Add(mockResolver2.Object);

            Assert.Null(chain.Get<IPilkington>("Karl"));

            mockResolver1.Verify(m => m.Get(typeof(IPilkington), "Karl"), Times.Once());
            mockResolver2.Verify(m => m.Get(typeof(IPilkington), "Karl"), Times.Once());
        }

        [Fact]
        public void Get_returns_the_service_returned_by_the_most_recently_added_resolver_that_resolves_the_dependency()
        {
            var karl = new Mock<IPilkington>().Object;

            var mockResolver1 = CreateMockResolver("Karl", new Mock<IPilkington>().Object);
            var mockResolver2 = CreateMockResolver("Karl", karl);
            var mockResolver3 = CreateMockResolver("Ricky", new Mock<IPilkington>().Object);

            var chain = new ResolverChain();
            chain.Add(mockResolver1.Object);
            chain.Add(mockResolver2.Object);
            chain.Add(mockResolver3.Object);

            Assert.Same(karl, chain.Get<IPilkington>("Karl"));

            mockResolver1.Verify(m => m.Get(typeof(IPilkington), "Karl"), Times.Never());
            mockResolver2.Verify(m => m.Get(typeof(IPilkington), "Karl"), Times.Once());
            mockResolver3.Verify(m => m.Get(typeof(IPilkington), "Karl"), Times.Once());
        }

        [Fact]
        public void Release_is_called_for_every_provider_in_the_chain()
        {
            var karl = new Mock<IPilkington>().Object;

            var mockResolver1 = CreateMockResolver("Steve", new Mock<IPilkington>().Object);
            var mockResolver2 = CreateMockResolver("Ricky", new Mock<IPilkington>().Object);

            var chain = new ResolverChain();
            chain.Add(mockResolver1.Object);
            chain.Add(mockResolver2.Object);

            chain.Release(karl);

            mockResolver1.Verify(m => m.Release(karl), Times.Once());
            mockResolver2.Verify(m => m.Release(karl), Times.Once());
        }

        private static Mock<IDbDependencyResolver> CreateMockResolver<T>(string name, T service)
        {
            var mockResolver = new Mock<IDbDependencyResolver>();
            mockResolver.Setup(m => m.Get(typeof(T), name)).Returns(service);

            return mockResolver;
        }
    }
}