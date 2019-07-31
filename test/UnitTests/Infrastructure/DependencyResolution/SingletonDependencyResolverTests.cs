// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Collections.Concurrent;
    using System.Linq;
    using Moq;
    using Xunit;

    public class SingletonDependencyResolverTests : TestBase
    {
        public class Constructors : TestBase
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
        }

        public interface IPilkington
        {
        }

        public interface IGervais
        {
        }

        public class GetService : TestBase
        {
            [Fact]
            public void GetService_returns_unkeyed_instance_of_contract_interface()
            {
                var instance = new Mock<IPilkington>().Object;

                Assert.Same(instance, new SingletonDependencyResolver<IPilkington>(instance).GetService<IPilkington>());
                Assert.Same(instance, new SingletonDependencyResolver<IPilkington>(instance, (object)null).GetService<IPilkington>());
            }

            [Fact]
            public void GetService_returns_keyed_instance_of_contract_interface()
            {
                var instance = new Mock<IPilkington>().Object;

                Assert.Same(instance, new SingletonDependencyResolver<IPilkington>(instance, "Karl").GetService<IPilkington>("Karl"));
            }


            [Fact]
            public void GetService_returns_instance_of_contract_interface_only_when_key_predicate_matches()
            {
                var instance = new Mock<IPilkington>().Object;
                var resolver = new SingletonDependencyResolver<IPilkington>(instance, k => k != null && ((string)k).StartsWith("K"));

                Assert.Same(instance, resolver.GetService<IPilkington>("Karl"));
                Assert.Null(resolver.GetService<IPilkington>("Ricky"));
                Assert.Null(resolver.GetService<IPilkington>());
                Assert.Null(resolver.GetService<IGervais>("Ricky"));
            }

            [Fact]
            public void GetService_returns_null_when_contract_interface_does_not_match()
            {
                var instance = new Mock<IPilkington>().Object;

                Assert.Null(new SingletonDependencyResolver<IPilkington>(instance).GetService<IGervais>());
                Assert.Null(new SingletonDependencyResolver<IPilkington>(instance, (object)null).GetService<IGervais>());
                Assert.Null(new SingletonDependencyResolver<IPilkington>(instance, "Karl").GetService<IGervais>("Karl"));
            }

            [Fact]
            public void GetService_returns_null_when_key_does_not_match()
            {
                var instance = new Mock<IPilkington>().Object;

                Assert.Null(new SingletonDependencyResolver<IPilkington>(instance, "Karl").GetService<IPilkington>("Ricky"));
                Assert.Null(new SingletonDependencyResolver<IPilkington>(instance, "Karl").GetService<IPilkington>());
                Assert.Null(new SingletonDependencyResolver<IPilkington>(instance, "Karl").GetService<IGervais>("Ricky"));
            }

            /// <summary>
            /// This test makes calls from multiple threads such that we have at least some chance of finding threading
            /// issues. As with any test of this type just because the test passes does not mean that the code is
            /// correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
            /// be investigated. DON'T just re-run and think things are okay if the test then passes.
            /// </summary>
            [Fact]
            public void GetService_can_be_accessed_from_multiple_threads_concurrently()
            {
                for (var i = 0; i < 30; i++)
                {
                    var bag = new ConcurrentBag<IPilkington>();
                    var karl = new Mock<IPilkington>().Object;
                    var resolver = new SingletonDependencyResolver<IPilkington>(karl, "Karl");

                    ExecuteInParallel(() => bag.Add(resolver.GetService<IPilkington>("Karl")));

                    Assert.Equal(20, bag.Count);
                    Assert.True(bag.All(c => karl == c));
                }
            }
        }

        public class GetServices : TestBase
        {
            [Fact]
            public void GetServices_returns_unkeyed_instance_of_contract_interface()
            {
                var instance = new Mock<IPilkington>().Object;

                Assert.Same(instance, new SingletonDependencyResolver<IPilkington>(instance).GetServices<IPilkington>().Single());
                Assert.Same(instance, new SingletonDependencyResolver<IPilkington>(instance, (object)null).GetServices<IPilkington>().Single());
            }

            [Fact]
            public void GetServices_returns_keyed_instance_of_contract_interface()
            {
                var instance = new Mock<IPilkington>().Object;

                Assert.Same(instance, new SingletonDependencyResolver<IPilkington>(instance, "Karl").GetServices<IPilkington>("Karl").Single());
            }

            [Fact]
            public void GetServices_returns_instance_of_contract_interface_only_when_key_predicate_matches()
            {
                var instance = new Mock<IPilkington>().Object;
                var resolver = new SingletonDependencyResolver<IPilkington>(instance, k => k != null && ((string)k).StartsWith("K"));

                Assert.Same(instance, resolver.GetServices<IPilkington>("Karl").Single());
                Assert.Empty(resolver.GetServices<IPilkington>("Ricky"));
                Assert.Empty(resolver.GetServices<IPilkington>());
                Assert.Empty(resolver.GetServices<IGervais>("Ricky"));
            }

            [Fact]
            public void GetServices_returns_empty_list_when_contract_interface_does_not_match()
            {
                var instance = new Mock<IPilkington>().Object;

                Assert.Empty(new SingletonDependencyResolver<IPilkington>(instance).GetServices<IGervais>());
                Assert.Empty(new SingletonDependencyResolver<IPilkington>(instance, (object)null).GetServices<IGervais>());
                Assert.Empty(new SingletonDependencyResolver<IPilkington>(instance, "Karl").GetServices<IGervais>("Karl"));
            }

            [Fact]
            public void GetServices_returns_empty_list_when_key_does_not_match()
            {
                var instance = new Mock<IPilkington>().Object;

                Assert.Empty(new SingletonDependencyResolver<IPilkington>(instance, "Karl").GetServices<IPilkington>("Ricky"));
                Assert.Empty(new SingletonDependencyResolver<IPilkington>(instance, "Karl").GetServices<IPilkington>());
                Assert.Empty(new SingletonDependencyResolver<IPilkington>(instance, "Karl").GetServices<IGervais>("Ricky"));
            }

            /// <summary>
            /// This test makes calls from multiple threads such that we have at least some chance of finding threading
            /// issues. As with any test of this type just because the test passes does not mean that the code is
            /// correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
            /// be investigated. DON'T just re-run and think things are okay if the test then passes.
            /// </summary>
            [Fact]
            public void GetServices_can_be_accessed_from_multiple_threads_concurrently()
            {
                for (var i = 0; i < 30; i++)
                {
                    var bag = new ConcurrentBag<IPilkington>();
                    var karl = new Mock<IPilkington>().Object;
                    var resolver = new SingletonDependencyResolver<IPilkington>(karl, "Karl");

                    ExecuteInParallel(() => bag.Add(resolver.GetServices<IPilkington>("Karl").Single()));

                    Assert.Equal(20, bag.Count);
                    Assert.True(bag.All(c => karl == c));
                }
            }
        }
    }
}
