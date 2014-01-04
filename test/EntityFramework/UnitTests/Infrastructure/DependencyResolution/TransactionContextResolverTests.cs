// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Moq;
    using Xunit;

    public class TransactionContextResolverTests
    {
        public class Constructor
        {
            [Fact]
            public void Throws_on_null_TransactionHandlerFactory()
            {
                Assert.Equal(
                    "transactionContextFactory",
                    Assert.Throws<ArgumentNullException>(() => new TransactionContextResolver(null, null, null)).ParamName);
            }
        }

        public class GetService
        {
            [Fact]
            public void GetService_returns_null_for_non_IProviderInvariantName_types()
            {
                Assert.Null(new TransactionContextResolver(c => new Mock<TransactionContext>().Object, null, null).GetService<Random>());
            }

            [Fact]
            public void GetService_throws_for_null_or_incorrect_key_type()
            {
                Assert.Equal(
                    Strings.DbDependencyResolver_InvalidKey(typeof(StoreKey).Name, "Func<DbConnection, TransactionContext>"),
                    Assert.Throws<ArgumentException>(
                        () => new TransactionContextResolver(c => new Mock<TransactionContext>().Object, null, null)
                            .GetService<Func<DbConnection, TransactionContext>>(null)).Message);

                Assert.Equal(
                    Strings.DbDependencyResolver_InvalidKey(typeof(StoreKey).Name, "Func<DbConnection, TransactionContext>"),
                    Assert.Throws<ArgumentException>(
                        () => new TransactionContextResolver(c => new Mock<TransactionContext>().Object, null, null)
                            .GetService<Func<DbConnection, TransactionContext>>("Oh No!")).Message);
            }

            [Fact]
            public void GetService_returns_null_for_a_different_provider_invariant_name()
            {
                Assert.Null(
                    new TransactionContextResolver(c => new Mock<TransactionContext>().Object, "p", "s")
                        .GetService<Func<DbConnection, TransactionContext>>(new StoreKey("p1", "s")));
            }

            [Fact]
            public void GetService_returns_null_for_a_different_server()
            {
                Assert.Null(
                    new TransactionContextResolver(c => new Mock<TransactionContext>().Object, "p", "s")
                        .GetService<Func<DbConnection, TransactionContext>>(new StoreKey("p", "s1")));
            }

            [Fact]
            public void GetService_returns_the_TransactionContext_factory_registered_for_the_same_provider_and_server()
            {
                Func<DbConnection, TransactionContext> transactionContextFactory = c => new Mock<TransactionContext>().Object;
                Assert.Same(
                    transactionContextFactory,
                    new TransactionContextResolver(transactionContextFactory, "p", "s").GetService<Func<DbConnection, TransactionContext>>(
                        new StoreKey("p", "s")));
            }

            [Fact]
            public void GetService_returns_the_TransactionContext_factory_registered_for_the_same_provider_and_null_server()
            {
                Func<DbConnection, TransactionContext> transactionContextFactory = c => new Mock<TransactionContext>().Object;
                Assert.Same(
                    transactionContextFactory,
                    new TransactionContextResolver(transactionContextFactory, "p", null).GetService<Func<DbConnection, TransactionContext>>(
                        new StoreKey("p", "s")));
            }

            [Fact]
            public void GetService_returns_the_TransactionContext_factory_registered_for_the_same_server_and_null_provider_()
            {
                Func<DbConnection, TransactionContext> transactionContextFactory = c => new Mock<TransactionContext>().Object;
                Assert.Same(
                    transactionContextFactory,
                    new TransactionContextResolver(transactionContextFactory, null, "s").GetService<Func<DbConnection, TransactionContext>>(
                        new StoreKey("p", "s")));
            }
        }

        public class GetServices
        {
            [Fact]
            public void GetServices_returns_the_TransactionContext_factory_registered_for_null_server_and_provider_()
            {
                Func<DbConnection, TransactionContext> transactionContextFactory = c => new Mock<TransactionContext>().Object;
                Assert.Same(
                    transactionContextFactory,
                    new TransactionContextResolver(transactionContextFactory, null, null).GetServices<Func<DbConnection, TransactionContext>>(
                        new StoreKey("p", "s")).Single());
            }
        }

        public class EqualsTests
        {
            [Fact]
            public void Returns_true_for_equal_objects()
            {
                Func<DbConnection, TransactionContext> transactionContextFactory = c => new Mock<TransactionContext>().Object;
                var resolver1 = new TransactionContextResolver(transactionContextFactory, "p", "s");
                var resolver2 = new TransactionContextResolver(transactionContextFactory, "p", "s");

                Assert.True(resolver1.Equals(resolver2));
            }

            [Fact]
            public void Returns_false_for_different_type()
            {
                var resolver = new TransactionContextResolver(c => new Mock<TransactionContext>().Object, "p", "s");

                Assert.False(resolver.Equals(new object()));
            }

            [Fact]
            public void Returns_false_for_different_factories()
            {
                var resolver1 = new TransactionContextResolver(c => new Mock<TransactionContext>().Object, "p", "s");
                var resolver2 = new TransactionContextResolver(c => new Mock<TransactionContext>().Object, "p", "s");

                Assert.False(resolver1.Equals(resolver2));
                Assert.False(resolver2.Equals(resolver1));
            }

            [Fact]
            public void Returns_false_for_different_providers()
            {
                Func<DbConnection, TransactionContext> transactionContextFactory = c => new Mock<TransactionContext>().Object;
                var resolver1 = new TransactionContextResolver(transactionContextFactory, "p", "s");
                var resolver2 = new TransactionContextResolver(transactionContextFactory, null, "s");

                Assert.False(resolver1.Equals(resolver2));
                Assert.False(resolver2.Equals(resolver1));
            }

            [Fact]
            public void Returns_false_for_different_servers()
            {
                Func<DbConnection, TransactionContext> transactionContextFactory = c => new Mock<TransactionContext>().Object;
                var resolver1 = new TransactionContextResolver(transactionContextFactory, "p", "s");
                var resolver2 = new TransactionContextResolver(transactionContextFactory, "p", null);

                Assert.False(resolver1.Equals(resolver2));
                Assert.False(resolver2.Equals(resolver1));
            }
        }

        public class GetHashCodeTests
        {
            [Fact]
            public void Returns_same_values_for_equal_objects()
            {
                Func<DbConnection, TransactionContext> transactionContextFactory = c => new Mock<TransactionContext>().Object;
                var resolver1 = new TransactionContextResolver(transactionContextFactory, "p", "s");
                var resolver2 = new TransactionContextResolver(transactionContextFactory, "p", "s");

                Assert.Equal(resolver1.GetHashCode(), resolver2.GetHashCode());
            }

            [Fact]
            public void Returns_different_values_for_different_factories()
            {
                var resolver1 = new TransactionContextResolver(c => new TransactionContext(c), "p", "s");
                var resolver2 = new TransactionContextResolver(
                    new Func<DbConnection, FakeTransactionContext>(
                        c => new FakeTransactionContext(c)), "p", "s");

                Assert.NotEqual(resolver1.GetHashCode(), resolver2.GetHashCode());
            }

            [Fact]
            public void Returns_false_for_different_providers()
            {
                Func<DbConnection, TransactionContext> transactionContextFactory = c => new Mock<TransactionContext>().Object;
                var resolver1 = new TransactionContextResolver(transactionContextFactory, "p", "s");
                var resolver2 = new TransactionContextResolver(transactionContextFactory, null, "s");

                Assert.NotEqual(resolver1.GetHashCode(), resolver2.GetHashCode());
            }

            [Fact]
            public void Returns_false_for_different_servers()
            {
                Func<DbConnection, TransactionContext> transactionContextFactory = c => new Mock<TransactionContext>().Object;
                var resolver1 = new TransactionContextResolver(transactionContextFactory, "p", "s");
                var resolver2 = new TransactionContextResolver(transactionContextFactory, "p", null);

                Assert.NotEqual(resolver1.GetHashCode(), resolver2.GetHashCode());
            }

            private class FakeTransactionContext : TransactionContext
            {
                public FakeTransactionContext(DbConnection c)
                    :
                        base(c)
                {
                }
            }
        }
    }

}
