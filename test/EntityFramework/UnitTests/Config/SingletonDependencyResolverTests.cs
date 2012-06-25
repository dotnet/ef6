namespace System.Data.Entity.Config
{
    using Moq;
    using Xunit;

    public class SingletonDependencyResolverTests
    {
        [Fact]
        public void Constructors_throw_for_null_instance()
        {
            Assert.Equal(
                "singletonInstance", 
                Assert.Throws<ArgumentNullException>(() => new SingletonDependencyResolver<object>(null)).ParamName);

            Assert.Equal(
                "singletonInstance",
                Assert.Throws<ArgumentNullException>(() => new SingletonDependencyResolver<object>(null, "Karl")).ParamName);
        }

        public interface IPilkington
        {
        }

        public interface IGervais
        {
        }

        [Fact]
        public void Get_returns_unnamed_instance_of_contract_interface()
        {
            var instance = new Mock<IPilkington>().Object;

            Assert.Same(instance, new SingletonDependencyResolver<IPilkington>(instance).Get<IPilkington>());
            Assert.Same(instance, new SingletonDependencyResolver<IPilkington>(instance, null).Get<IPilkington>());
        }

        [Fact]
        public void Get_returns_named_instance_of_contract_interface()
        {
            var instance = new Mock<IPilkington>().Object;

            Assert.Same(instance, new SingletonDependencyResolver<IPilkington>(instance, "Karl").Get<IPilkington>("Karl"));
        }

        [Fact]
        public void Get_returns_null_when_contract_interface_does_not_match()
        {
            var instance = new Mock<IPilkington>().Object;

            Assert.Null(new SingletonDependencyResolver<IPilkington>(instance).Get<IGervais>());
            Assert.Null(new SingletonDependencyResolver<IPilkington>(instance, null).Get<IGervais>());
            Assert.Null(new SingletonDependencyResolver<IPilkington>(instance, "Karl").Get<IGervais>("Karl"));
        }

        [Fact]
        public void Get_returns_null_when_name_does_not_match()
        {
            var instance = new Mock<IPilkington>().Object;

            Assert.Null(new SingletonDependencyResolver<IPilkington>(instance, "Karl").Get<IPilkington>("Ricky"));
            Assert.Null(new SingletonDependencyResolver<IPilkington>(instance, "Karl").Get<IPilkington>());
            Assert.Null(new SingletonDependencyResolver<IPilkington>(instance, "Karl").Get<IGervais>("Ricky"));
        }

        [Fact]
        public void Release_does_not_throw()
        {
            var instance = new Mock<IPilkington>().Object;
            new SingletonDependencyResolver<IPilkington>(instance, "Karl").Release(instance);
        }
    }
}
