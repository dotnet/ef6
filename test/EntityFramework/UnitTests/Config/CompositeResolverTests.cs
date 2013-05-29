// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Moq;
    using Xunit;

    public class CompositeResolverTests : TestBase
    {
        public class GetService : TestBase
        {
            [Fact]
            public void GetService_returns_result_of_first_resolver_if_result_is_non_null()
            {
                var mockFirstResolver = new Mock<IDbDependencyResolver>();
                var mockSecondResolver = new Mock<IDbDependencyResolver>();

                var karl = new Mock<IPilkington>().Object;
                mockFirstResolver.Setup(m => m.GetService(typeof(IPilkington), "Karl")).Returns(karl);

                Assert.Same(
                    karl,
                    new CompositeResolver<IDbDependencyResolver, IDbDependencyResolver>(
                        mockFirstResolver.Object, mockSecondResolver.Object).GetService<IPilkington>("Karl"));

                mockFirstResolver.Verify(m => m.GetService(typeof(IPilkington), "Karl"), Times.Once());
                mockSecondResolver.Verify(m => m.GetService(It.IsAny<Type>(), It.IsAny<string>()), Times.Never());
            }

            [Fact]
            public void GetService_returns_result_of_second_resolver_if_result_of_first_is_null()
            {
                var mockFirstResolver = new Mock<IDbDependencyResolver>();
                var mockSecondResolver = new Mock<IDbDependencyResolver>();

                var karl = new Mock<IPilkington>().Object;
                mockSecondResolver.Setup(m => m.GetService(typeof(IPilkington), "Karl")).Returns(karl);

                Assert.Same(
                    karl,
                    new CompositeResolver<IDbDependencyResolver, IDbDependencyResolver>(
                        mockFirstResolver.Object, mockSecondResolver.Object).GetService<IPilkington>("Karl"));

                mockFirstResolver.Verify(m => m.GetService(typeof(IPilkington), "Karl"), Times.Once());
                mockSecondResolver.Verify(m => m.GetService(typeof(IPilkington), "Karl"), Times.Once());
            }

            [Fact]
            public void GetService_returns_null_if_both_resolvers_return_null()
            {
                var mockFirstResolver = new Mock<IDbDependencyResolver>();
                var mockSecondResolver = new Mock<IDbDependencyResolver>();

                Assert.Null(
                    new CompositeResolver<IDbDependencyResolver, IDbDependencyResolver>(
                        mockFirstResolver.Object, mockSecondResolver.Object).GetService<IPilkington>("Karl"));

                mockFirstResolver.Verify(m => m.GetService(typeof(IPilkington), "Karl"), Times.Once());
                mockSecondResolver.Verify(m => m.GetService(typeof(IPilkington), "Karl"), Times.Once());
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
                    var karl1 = new Mock<IPilkington>().Object;
                    var resolver = new CompositeResolver<IDbDependencyResolver, IDbDependencyResolver>(
                        new SingletonDependencyResolver<IPilkington>(karl1),
                        new SingletonDependencyResolver<IPilkington>(new Mock<IPilkington>().Object));

                    ExecuteInParallel(() => bag.Add(resolver.GetService<IPilkington>()));

                    Assert.Equal(20, bag.Count);
                    Assert.True(bag.All(c => karl1 == c));
                }
            }
        }

        public class GetServices : TestBase
        {
            [Fact]
            public void GetServices_returns_results_of_both_resolvers_concatinated_together()
            {
                var mockFirstResolver = new Mock<IDbDependencyResolver>();
                var mockSecondResolver = new Mock<IDbDependencyResolver>();

                var karls1 = new object[] { new Mock<IPilkington>().Object, new Mock<IPilkington>().Object };
                mockFirstResolver.Setup(m => m.GetServices(typeof(IPilkington), "Karl")).Returns(karls1);

                var karls2 = new object[] { new Mock<IPilkington>().Object, new Mock<IPilkington>().Object };
                mockSecondResolver.Setup(m => m.GetServices(typeof(IPilkington), "Karl")).Returns(karls2);

                var allKarls = karls1.ToList();
                allKarls.AddRange(karls2);

                Assert.Equal(
                    allKarls,
                    new CompositeResolver<IDbDependencyResolver, IDbDependencyResolver>(
                        mockFirstResolver.Object, mockSecondResolver.Object).GetServices<IPilkington>("Karl"));

                mockFirstResolver.Verify(m => m.GetServices(typeof(IPilkington), "Karl"), Times.Once());
                mockSecondResolver.Verify(m => m.GetServices(typeof(IPilkington), "Karl"), Times.Once());
            }

            [Fact]
            public void GetServices_returns_empty_list_if_both_resolvers_return_null()
            {
                var mockFirstResolver = new Mock<IDbDependencyResolver>();
                var mockSecondResolver = new Mock<IDbDependencyResolver>();

                Assert.Empty(
                    new CompositeResolver<IDbDependencyResolver, IDbDependencyResolver>(
                        mockFirstResolver.Object, mockSecondResolver.Object).GetServices<IPilkington>("Karl"));
            }

            /// <summary>
            ///     This test makes calls from multiple threads such that we have at least some chance of finding threading
            ///     issues. As with any test of this type just because the test passes does not mean that the code is
            ///     correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
            ///     be investigated. DON'T just re-run and think things are okay if the test then passes.
            /// </summary>
            [Fact]
            public void GetServices_can_be_accessed_from_multiple_threads_concurrently()
            {
                for (var i = 0; i < 30; i++)
                {
                    var bag = new ConcurrentBag<IEnumerable<IPilkington>>();
                    var karl1 = new Mock<IPilkington>().Object;
                    var karl2 = new Mock<IPilkington>().Object;
                    var resolver = new CompositeResolver<IDbDependencyResolver, IDbDependencyResolver>(
                        new SingletonDependencyResolver<IPilkington>(karl1),
                        new SingletonDependencyResolver<IPilkington>(karl2));

                    ExecuteInParallel(() => bag.Add(resolver.GetServices<IPilkington>()));

                    Assert.Equal(20, bag.Count);
                    Assert.True(bag.All(c => c.Count() == 2 && c.Contains(karl1) && c.Contains(karl2)));
                }
            }
        }

        public interface IPilkington
        {
        }
    }
}
