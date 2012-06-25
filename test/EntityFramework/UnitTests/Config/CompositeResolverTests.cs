namespace System.Data.Entity.Config
{
    using Moq;
    using Xunit;

    public class CompositeResolverTests
    {
        [Fact]
        public void Get_returns_result_of_first_resolver_if_result_is_non_null()
        {
            var mockFirstResolver = new Mock<IDbDependencyResolver>();
            var mockSecondResolver = new Mock<IDbDependencyResolver>();
            
            var karl = new Mock<IPilkington>().Object;
            mockFirstResolver.Setup(m => m.Get(typeof(IPilkington), "Karl")).Returns(karl);

            Assert.Same(
                karl,
                new CompositeResolver<IDbDependencyResolver, IDbDependencyResolver>(
                    mockFirstResolver.Object, mockSecondResolver.Object).Get<IPilkington>("Karl"));

            mockFirstResolver.Verify(m => m.Get(typeof(IPilkington), "Karl"), Times.Once());
            mockSecondResolver.Verify(m => m.Get(It.IsAny<Type>(), It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public void Get_returns_result_of_second_resolver_if_result_of_first_is_null()
        {
            var mockFirstResolver = new Mock<IDbDependencyResolver>();
            var mockSecondResolver = new Mock<IDbDependencyResolver>();
            
            var karl = new Mock<IPilkington>().Object;
            mockSecondResolver.Setup(m => m.Get(typeof(IPilkington), "Karl")).Returns(karl);

            Assert.Same(
                karl,
                new CompositeResolver<IDbDependencyResolver, IDbDependencyResolver>(
                    mockFirstResolver.Object, mockSecondResolver.Object).Get<IPilkington>("Karl"));

            mockFirstResolver.Verify(m => m.Get(typeof(IPilkington), "Karl"), Times.Once());
            mockSecondResolver.Verify(m => m.Get(typeof(IPilkington), "Karl"), Times.Once());
        }

        [Fact]
        public void Get_returns_null_if_both_resolvers_return_null()
        {
            var mockFirstResolver = new Mock<IDbDependencyResolver>();
            var mockSecondResolver = new Mock<IDbDependencyResolver>();

            Assert.Null(
                new CompositeResolver<IDbDependencyResolver, IDbDependencyResolver>(
                    mockFirstResolver.Object, mockSecondResolver.Object).Get<IPilkington>("Karl"));

            mockFirstResolver.Verify(m => m.Get(typeof(IPilkington), "Karl"), Times.Once());
            mockSecondResolver.Verify(m => m.Get(typeof(IPilkington), "Karl"), Times.Once());
        }

        [Fact]
        public void Release_calls_release_on_both_resolvers()
        {
            var mockFirstResolver = new Mock<IDbDependencyResolver>();
            var mockSecondResolver = new Mock<IDbDependencyResolver>();

            var karl = new object();
            
            new CompositeResolver<IDbDependencyResolver, IDbDependencyResolver>(
                mockFirstResolver.Object, mockSecondResolver.Object).Release(karl);

            mockFirstResolver.Verify(m => m.Release(karl), Times.Once());
            mockSecondResolver.Verify(m => m.Release(karl), Times.Once());
        }

        public interface IPilkington
        {
        }
    }
}