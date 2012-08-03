// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections.Concurrent;
    using System.Linq;
    using Moq;
    using Xunit;

    public class DatabaseInitializerResolverTests : TestBase
    {
        [Fact]
        public void GetService_returns_null_for_non_database_initializer_type()
        {
            Assert.Null(CreateResolver().GetService<DbContext>());
        }

        [Fact]
        public void GetService_returns_null_for_database_initializer_generic_type_definition()
        {
            Assert.Null(CreateResolver().GetService(typeof(IDatabaseInitializer<>)));
        }

        [Fact]
        public void GetService_returns_null_for_context_type_that_has_not_been_registered()
        {
            Assert.Null(CreateResolver().GetService(typeof(IDatabaseInitializer<DbContext>)));
        }

        [Fact]
        public void GetService_returns_registered_initializer()
        {
            var initializer = new Mock<IDatabaseInitializer<FakeContext1>>().Object;
            Assert.Same(initializer, CreateResolver(initializer).GetService(typeof(IDatabaseInitializer<FakeContext1>)));
        }

        [Fact]
        public void SetInitializer_overrides_initializer_previously_set()
        {
            var resolver = CreateResolver();
            var initializer = new Mock<IDatabaseInitializer<FakeContext1>>().Object;
            resolver.SetInitializer(typeof(FakeContext1), initializer);

            Assert.Same(initializer, resolver.GetService(typeof(IDatabaseInitializer<FakeContext1>)));
        }

        private static DatabaseInitializerResolver CreateResolver(IDatabaseInitializer<FakeContext1> initializer = null)
        {
            var resolver = new DatabaseInitializerResolver();
            resolver.SetInitializer(typeof(FakeContext1), initializer ?? new Mock<IDatabaseInitializer<FakeContext1>>().Object);
            resolver.SetInitializer(typeof(FakeContext2), new Mock<IDatabaseInitializer<FakeContext2>>().Object);
            return resolver;
        }

        [Fact]
        public void Release_does_not_throw()
        {
            new DatabaseInitializerResolver().Release(new object());
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
                var bag = new ConcurrentBag<IDatabaseInitializer<FakeContext1>>();
                var resolver = new DatabaseInitializerResolver();
                var initializer = new Mock<IDatabaseInitializer<FakeContext1>>().Object;

                ExecuteInParallel(
                    () =>
                        {
                            resolver.SetInitializer(typeof(FakeContext1), initializer);
                            bag.Add(resolver.GetService<IDatabaseInitializer<FakeContext1>>());
                        });

                Assert.Equal(20, bag.Count);
                Assert.True(bag.All(c => initializer == c));
            }
        }

        public class FakeContext1 : DbContext
        {
        }

        public class FakeContext2 : DbContext
        {
        }
    }
}
