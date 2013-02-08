// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections.Concurrent;
    using System.Linq;
    using Moq;
    using Xunit;

    public class ThreadLocalDependencyResolverTests : TestBase
    {
        [Fact]
        public void Constructors_throw_for_null_value_factory()
        {
            Assert.Equal(
                "valueFactory",
                Assert.Throws<ArgumentNullException>(() => new ThreadLocalDependencyResolver<object>(null)).ParamName);

            Assert.Equal(
                "valueFactory",
                Assert.Throws<ArgumentNullException>(() => new ThreadLocalDependencyResolver<object>(null, "Karl")).ParamName);
        }

        public interface IPilkington
        {
        }

        public interface IGervais
        {
        }

        [Fact]
        public void GetService_returns_unnamed_instance_of_contract_interface()
        {
            var instance = new Mock<IPilkington>().Object;

            Assert.Same(instance, new ThreadLocalDependencyResolver<IPilkington>(() => instance).GetService<IPilkington>());
            Assert.Same(instance, new ThreadLocalDependencyResolver<IPilkington>(() => instance, null).GetService<IPilkington>());
        }

        [Fact]
        public void GetService_returns_named_instance_of_contract_interface()
        {
            var instance = new Mock<IPilkington>().Object;

            Assert.Same(instance, new ThreadLocalDependencyResolver<IPilkington>(() => instance, "Karl").GetService<IPilkington>("Karl"));
        }

        [Fact]
        public void GetService_returns_null_when_contract_interface_does_not_match()
        {
            var instance = new Mock<IPilkington>().Object;

            Assert.Null(new ThreadLocalDependencyResolver<IPilkington>(() => instance).GetService<IGervais>());
            Assert.Null(new ThreadLocalDependencyResolver<IPilkington>(() => instance, null).GetService<IGervais>());
            Assert.Null(new ThreadLocalDependencyResolver<IPilkington>(() => instance, "Karl").GetService<IGervais>("Karl"));
        }

        [Fact]
        public void GetService_returns_null_when_name_does_not_match()
        {
            var instance = new Mock<IPilkington>().Object;

            Assert.Null(new ThreadLocalDependencyResolver<IPilkington>(() => instance, "Karl").GetService<IPilkington>("Ricky"));
            Assert.Null(new ThreadLocalDependencyResolver<IPilkington>(() => instance, "Karl").GetService<IPilkington>());
            Assert.Null(new ThreadLocalDependencyResolver<IPilkington>(() => instance, "Karl").GetService<IGervais>("Ricky"));
        }

        /// <summary>
        ///     This test makes calls from multiple threads such that we have at least some chance of finding threading
        ///     issues. As with any test of this type just because the test passes does not mean that the code is
        ///     correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
        ///     be investigated. DON'T just re-run and think things are okay if the test then passes.
        /// </summary>
        [Fact]
        public void GetService_can_be_accessed_from_multiple_threads_concurrently()
        {
            for (var i = 0; i < 30; i++)
            {
                var bag = new ConcurrentBag<IPilkington>();
                var karl = new Mock<IPilkington>().Object;
                var resolver = new ThreadLocalDependencyResolver<IPilkington>(() => karl, "Karl");

                ExecuteInParallel(() => bag.Add(resolver.GetService<IPilkington>("Karl")));

                Assert.Equal(20, bag.Count);
                Assert.True(bag.All(c => karl == c));
            }
        }
    }
}
