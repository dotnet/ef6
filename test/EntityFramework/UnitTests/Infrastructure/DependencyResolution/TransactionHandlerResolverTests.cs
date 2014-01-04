// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Data.Entity.Resources;
    using System.Linq;
    using Moq;
    using Xunit;

    public class TransactionHandlerResolverTests
    {
        public class Constructor
        {
            [Fact]
            public void Throws_on_null_TransactionHandlerFactory()
            {
                Assert.Equal(
                    "transactionHandlerFactory",
                    Assert.Throws<ArgumentNullException>(() => new TransactionHandlerResolver(null, null, null)).ParamName);
            }
        }

        public class GetService
        {
            [Fact]
            public void GetService_returns_null_for_non_IProviderInvariantName_types()
            {
                Assert.Null(new TransactionHandlerResolver(() => new Mock<TransactionHandler>().Object, null, null).GetService<Random>());
            }

            [Fact]
            public void GetService_throws_for_null_or_incorrect_key_type()
            {
                Assert.Equal(
                    Strings.DbDependencyResolver_InvalidKey(typeof(StoreKey).Name, "Func<TransactionHandler>"),
                    Assert.Throws<ArgumentException>(
                        () => new TransactionHandlerResolver(() => new Mock<TransactionHandler>().Object, null, null)
                            .GetService<Func<TransactionHandler>>(null)).Message);

                Assert.Equal(
                    Strings.DbDependencyResolver_InvalidKey(typeof(StoreKey).Name, "Func<TransactionHandler>"),
                    Assert.Throws<ArgumentException>(
                        () => new TransactionHandlerResolver(() => new Mock<TransactionHandler>().Object, null, null)
                            .GetService<Func<TransactionHandler>>("Oh No!")).Message);
            }

            [Fact]
            public void GetService_returns_null_for_a_different_provider_invariant_name()
            {
                Assert.Null(
                    new TransactionHandlerResolver(() => new Mock<TransactionHandler>().Object, "p", "s")
                        .GetService<Func<TransactionHandler>>(new StoreKey("p1", "s")));
            }

            [Fact]
            public void GetService_returns_null_for_a_different_server()
            {
                Assert.Null(
                    new TransactionHandlerResolver(() => new Mock<TransactionHandler>().Object, "p", "s")
                        .GetService<Func<TransactionHandler>>(new StoreKey("p", "s1")));
            }

            [Fact]
            public void GetService_returns_the_TransactionHandler_factory_registered_for_the_same_provider_and_server()
            {
                Func<TransactionHandler> transactionHandlerFactory = () => new Mock<TransactionHandler>().Object;
                Assert.Same(
                    transactionHandlerFactory,
                    new TransactionHandlerResolver(transactionHandlerFactory, "p", "s").GetService<Func<TransactionHandler>>(
                        new StoreKey("p", "s")));
            }

            [Fact]
            public void GetService_returns_the_TransactionHandler_factory_registered_for_the_same_provider_and_null_server()
            {
                Func<TransactionHandler> transactionHandlerFactory = () => new Mock<TransactionHandler>().Object;
                Assert.Same(
                    transactionHandlerFactory,
                    new TransactionHandlerResolver(transactionHandlerFactory, "p", null).GetService<Func<TransactionHandler>>(
                        new StoreKey("p", "s")));
            }

            [Fact]
            public void GetService_returns_the_TransactionHandler_factory_registered_for_the_same_server_and_null_provider_()
            {
                Func<TransactionHandler> transactionHandlerFactory = () => new Mock<TransactionHandler>().Object;
                Assert.Same(
                    transactionHandlerFactory,
                    new TransactionHandlerResolver(transactionHandlerFactory, null, "s").GetService<Func<TransactionHandler>>(
                        new StoreKey("p", "s")));
            }
        }

        public class GetServices
        {
            [Fact]
            public void GetServices_returns_the_TransactionHandler_factory_registered_for_null_server_and_provider_()
            {
                Func<TransactionHandler> transactionHandlerFactory = () => new Mock<TransactionHandler>().Object;
                Assert.Same(
                    transactionHandlerFactory,
                    new TransactionHandlerResolver(transactionHandlerFactory, null, null).GetServices<Func<TransactionHandler>>(
                        new StoreKey("p", "s")).Single());
            }
        }

        public class EqualsTests
        {
            [Fact]
            public void Returns_true_for_equal_objects()
            {
                Func<TransactionHandler> transactionHandlerFactory = () => new Mock<TransactionHandler>().Object;
                var resolver1 = new TransactionHandlerResolver(transactionHandlerFactory, "p", "s");
                var resolver2 = new TransactionHandlerResolver(transactionHandlerFactory, "p", "s");

                Assert.True(resolver1.Equals(resolver2));
            }

            [Fact]
            public void Returns_false_for_different_type()
            {
                var resolver = new TransactionHandlerResolver(() => new Mock<TransactionHandler>().Object, "p", "s");

                Assert.False(resolver.Equals(new object()));
            }

            [Fact]
            public void Returns_false_for_different_factories()
            {
                var resolver1 = new TransactionHandlerResolver(() => new Mock<TransactionHandler>().Object, "p", "s");
                var resolver2 = new TransactionHandlerResolver(() => new Mock<TransactionHandler>().Object, "p", "s");

                Assert.False(resolver1.Equals(resolver2));
                Assert.False(resolver2.Equals(resolver1));
            }

            [Fact]
            public void Returns_false_for_different_providers()
            {
                Func<TransactionHandler> transactionHandlerFactory = () => new Mock<TransactionHandler>().Object;
                var resolver1 = new TransactionHandlerResolver(transactionHandlerFactory, "p", "s");
                var resolver2 = new TransactionHandlerResolver(transactionHandlerFactory, null, "s");

                Assert.False(resolver1.Equals(resolver2));
                Assert.False(resolver2.Equals(resolver1));
            }

            [Fact]
            public void Returns_false_for_different_servers()
            {
                Func<TransactionHandler> transactionHandlerFactory = () => new Mock<TransactionHandler>().Object;
                var resolver1 = new TransactionHandlerResolver(transactionHandlerFactory, "p", "s");
                var resolver2 = new TransactionHandlerResolver(transactionHandlerFactory, "p", null);

                Assert.False(resolver1.Equals(resolver2));
                Assert.False(resolver2.Equals(resolver1));
            }
        }

        public class GetHashCodeTests
        {
            [Fact]
            public void Returns_same_values_for_equal_objects()
            {
                Func<TransactionHandler> transactionHandlerFactory = () => new Mock<TransactionHandler>().Object;
                var resolver1 = new TransactionHandlerResolver(transactionHandlerFactory, "p", "s");
                var resolver2 = new TransactionHandlerResolver(transactionHandlerFactory, "p", "s");

                Assert.Equal(resolver1.GetHashCode(), resolver2.GetHashCode());
            }

            [Fact]
            public void Returns_different_values_for_different_factories()
            {
                var resolver1 = new TransactionHandlerResolver(() => new Mock<TransactionHandler>().Object, "p", "s");
                var resolver2 = new TransactionHandlerResolver(
                    new Func<FakeTransactionHandler>(
                        () => new FakeTransactionHandler()), "p", "s");

                Assert.NotEqual(resolver1.GetHashCode(), resolver2.GetHashCode());
            }

            [Fact]
            public void Returns_false_for_different_providers()
            {
                Func<TransactionHandler> transactionHandlerFactory = () => new Mock<TransactionHandler>().Object;
                var resolver1 = new TransactionHandlerResolver(transactionHandlerFactory, "p", "s");
                var resolver2 = new TransactionHandlerResolver(transactionHandlerFactory, null, "s");

                Assert.NotEqual(resolver1.GetHashCode(), resolver2.GetHashCode());
            }

            [Fact]
            public void Returns_false_for_different_servers()
            {
                Func<TransactionHandler> transactionHandlerFactory = () => new Mock<TransactionHandler>().Object;
                var resolver1 = new TransactionHandlerResolver(transactionHandlerFactory, "p", "s");
                var resolver2 = new TransactionHandlerResolver(transactionHandlerFactory, "p", null);

                Assert.NotEqual(resolver1.GetHashCode(), resolver2.GetHashCode());
            }

            private class FakeTransactionHandler : TransactionHandler
            {
                public override string BuildDatabaseInitializationScript()
                {
                    return null;
                }
            }
        }
    }
}

