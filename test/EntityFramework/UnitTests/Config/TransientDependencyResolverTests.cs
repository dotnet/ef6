// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections.Concurrent;
    using System.Linq;
    using Xunit;

    public class TransientDependencyResolverTests : TestBase
    {
        [Fact]
        public void Constructors_throw_for_null_instance()
        {
            Assert.Equal(
                "activator",
                Assert.Throws<ArgumentNullException>(() => new TransientDependencyResolver<object>(null)).ParamName);

            Assert.Equal(
                "activator",
                Assert.Throws<ArgumentNullException>(() => new TransientDependencyResolver<object>(null, "Karl")).ParamName);
        }

        public interface IPilkington
        {
        }

        public class Karl : IPilkington
        {
        }

        public interface IGervais
        {
        }

        [Fact]
        public void GetService_returns_unnamed_instance_of_contract_interface()
        {
            Assert.NotNull(new TransientDependencyResolver<IPilkington>(() => new Karl()).GetService<IPilkington>());
        }

        [Fact]
        public void GetService_returns_named_instance_of_contract_interface()
        {
            Assert.NotNull(new TransientDependencyResolver<IPilkington>(() => new Karl(), "Karl").GetService<IPilkington>("Karl"));
        }

        [Fact]
        public void GetService_returns_null_when_contract_interface_does_not_match()
        {
            Assert.Null(new TransientDependencyResolver<IPilkington>(() => new Karl()).GetService<IGervais>());
            Assert.Null(new TransientDependencyResolver<IPilkington>(() => new Karl(), null).GetService<IGervais>());
            Assert.Null(new TransientDependencyResolver<IPilkington>(() => new Karl(), "Karl").GetService<IGervais>("Karl"));
        }

        [Fact]
        public void GetService_returns_null_when_name_does_not_match()
        {
            Assert.Null(new TransientDependencyResolver<IPilkington>(() => new Karl(), "Karl").GetService<IPilkington>("Ricky"));
            Assert.Null(new TransientDependencyResolver<IPilkington>(() => new Karl(), "Karl").GetService<IPilkington>());
            Assert.Null(new TransientDependencyResolver<IPilkington>(() => new Karl(), "Karl").GetService<IGervais>("Ricky"));
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
                var karl = new Karl();
                var resolver = new TransientDependencyResolver<IPilkington>(() => karl, "Karl");

                ExecuteInParallel(() => bag.Add(resolver.GetService<IPilkington>("Karl")));

                Assert.Equal(20, bag.Count);
                Assert.True(bag.All(c => karl == c));
            }
        }
    }
}
