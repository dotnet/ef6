namespace System.Data.Entity.Config
{
    using Moq;
    using Xunit;

    public class IDbDependencyResolverExtensionsTests
    {
        public interface IPilkington
        {
        }

        [Fact]
        public void Generic_get_with_name_calls_resolver_with_correct_type_and_name()
        {
            var karl = new Mock<IPilkington>().Object;
            var mockResolver = CreateMockResolver(karl);

            Assert.Same(karl, mockResolver.Object.Get<IPilkington>("Karl"));

            mockResolver.Verify(m => m.Get(typeof(IPilkington), "Karl"), Times.Once());
        }

        [Fact]
        public void Generic_get_without_name_calls_resolver_with_correct_type_and_null_name()
        {
            var karl = new Mock<IPilkington>().Object;
            var mockResolver = CreateMockResolver(karl);

            Assert.Same(karl, mockResolver.Object.Get<IPilkington>());

            mockResolver.Verify(m => m.Get(typeof(IPilkington), null), Times.Once());
        }

        [Fact]
        public void Non_generic_get_without_name_calls_resolver_with_given_type_and_null_name()
        {
            var karl = new Mock<IPilkington>().Object;
            var mockResolver = CreateMockResolver(karl);

            Assert.Same(karl, mockResolver.Object.Get(typeof(IPilkington)));

            mockResolver.Verify(m => m.Get(typeof(IPilkington), null), Times.Once());
        }

        [Fact]
        public void Get_methods_verify_resolver_and_type_are_non_null()
        {
            Assert.Equal(
                "resolver",
                Assert.Throws<ArgumentNullException>(() => IDbDependencyResolverExtensions.Get<IPilkington>(null, "Karl")).ParamName);

            Assert.Equal(
                "resolver",
                Assert.Throws<ArgumentNullException>(() => IDbDependencyResolverExtensions.Get<IPilkington>(null)).ParamName);

            Assert.Equal(
                "resolver",
                Assert.Throws<ArgumentNullException>(() => IDbDependencyResolverExtensions.Get(null, typeof(IPilkington))).ParamName);

            Assert.Equal(
                "type",
                Assert.Throws<ArgumentNullException>(() => new Mock<IDbDependencyResolver>().Object.Get(null)).ParamName);
        }

        private static Mock<IDbDependencyResolver> CreateMockResolver(IPilkington karl)
        {
            var mockResolver = new Mock<IDbDependencyResolver>();
            mockResolver.Setup(m => m.Get(It.IsAny<Type>(), It.IsAny<string>())).Returns(karl);

            return mockResolver;
        }
    }
}
