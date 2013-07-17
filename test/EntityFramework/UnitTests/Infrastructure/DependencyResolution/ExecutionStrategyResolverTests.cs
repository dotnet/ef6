// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Data.Entity.Resources;
    using System.Linq;
    using Moq;
    using Xunit;

    public class ExecutionStrategyResolverTests
    {
        public class Constructors : TestBase
        {
            [Fact]
            public void Constructor_throws_on_invalid_arguments()
            {
                Assert.Equal(
                    "getExecutionStrategy",
                    Assert.Throws<ArgumentNullException>(() => new ExecutionStrategyResolver<IDbExecutionStrategy>("Foo", "a", null))
                        .ParamName);

                Assert.Throws<ArgumentException>(
                    () => new ExecutionStrategyResolver<IDbExecutionStrategy>(null, "a", () => new Mock<IDbExecutionStrategy>().Object));
                Assert.Throws<ArgumentException>(
                    () => new ExecutionStrategyResolver<IDbExecutionStrategy>("", "a", () => new Mock<IDbExecutionStrategy>().Object));
            }
        }

        public class GetService : TestBase
        {
            [Fact]
            public void GetService_returns_null_when_contract_interface_does_not_match()
            {
                Assert.Null(
                    new ExecutionStrategyResolver<IDbExecutionStrategy>("Foo", null, () => new Mock<IDbExecutionStrategy>().Object)
                        .GetService<IQueryable>());
            }

            [Fact]
            public void GetService_throws_for_null_or_incorrect_key_type()
            {
                Assert.Equal(
                    Strings.DbDependencyResolver_InvalidKey(typeof(ExecutionStrategyKey).Name, "Func<IExecutionStrategy>"),
                    Assert.Throws<ArgumentException>(
                        () => new ExecutionStrategyResolver<IDbExecutionStrategy>("Foo", null, () => new Mock<IDbExecutionStrategy>().Object)
                                  .GetService<Func<IDbExecutionStrategy>>(null)).Message);

                Assert.Equal(
                    Strings.DbDependencyResolver_InvalidKey(typeof(ExecutionStrategyKey).Name, "Func<IExecutionStrategy>"),
                    Assert.Throws<ArgumentException>(
                        () => new ExecutionStrategyResolver<IDbExecutionStrategy>("Foo", null, () => new Mock<IDbExecutionStrategy>().Object)
                                  .GetService<Func<IDbExecutionStrategy>>("a")).Message);
            }

            [Fact]
            public void GetService_returns_null_when_the_provider_name_doesnt_match()
            {
                Assert.Null(
                    new ExecutionStrategyResolver<IDbExecutionStrategy>("Foo", null, () => new Mock<IDbExecutionStrategy>().Object)
                        .GetService<Func<IDbExecutionStrategy>>(
                            new ExecutionStrategyKey("FooClient", "a")));
            }

            [Fact]
            public void GetService_returns_null_when_the_serverName_doesnt_match()
            {
                Assert.Null(
                    new ExecutionStrategyResolver<IDbExecutionStrategy>("Foo", "b", () => new Mock<IDbExecutionStrategy>().Object)
                        .GetService<Func<IDbExecutionStrategy>>(
                            new ExecutionStrategyKey("Foo", "a")));
            }

            [Fact]
            public void GetService_returns_result_from_factory_method_on_match_null_serverName()
            {
                var mockExecutionStrategy = new Mock<IDbExecutionStrategy>().Object;
                var resolver = new ExecutionStrategyResolver<IDbExecutionStrategy>("Foo", null, () => mockExecutionStrategy);

                var resolvedExecutionStrategy = resolver.GetService<Func<IDbExecutionStrategy>>(
                    new ExecutionStrategyKey("Foo", "bar"))();

                Assert.Same(mockExecutionStrategy, resolvedExecutionStrategy);
            }

            [Fact]
            public void GetService_returns_result_from_factory_method_on_match_notnull_serverName()
            {
                var mockExecutionStrategy = new Mock<IDbExecutionStrategy>().Object;
                var resolver = new ExecutionStrategyResolver<IDbExecutionStrategy>("Foo", "bar", () => mockExecutionStrategy);

                var resolvedExecutionStrategy = resolver.GetService<Func<IDbExecutionStrategy>>(
                    new ExecutionStrategyKey("Foo", "bar"))();

                Assert.Same(mockExecutionStrategy, resolvedExecutionStrategy);
            }
        }

        public class GetServices : TestBase
        {
            [Fact]
            public void GetServices_returns_empty_list_when_contract_interface_does_not_match()
            {
                Assert.Empty(
                    new ExecutionStrategyResolver<IDbExecutionStrategy>("Foo", null, () => new Mock<IDbExecutionStrategy>().Object)
                        .GetServices<IQueryable>());
            }

            [Fact]
            public void GetServices_throws_for_null_or_incorrect_key_type()
            {
                Assert.Equal(
                    Strings.DbDependencyResolver_InvalidKey(typeof(ExecutionStrategyKey).Name, "Func<IExecutionStrategy>"),
                    Assert.Throws<ArgumentException>(
                        () => new ExecutionStrategyResolver<IDbExecutionStrategy>("Foo", null, () => new Mock<IDbExecutionStrategy>().Object)
                                  .GetServices<Func<IDbExecutionStrategy>>(null)).Message);

                Assert.Equal(
                    Strings.DbDependencyResolver_InvalidKey(typeof(ExecutionStrategyKey).Name, "Func<IExecutionStrategy>"),
                    Assert.Throws<ArgumentException>(
                        () => new ExecutionStrategyResolver<IDbExecutionStrategy>("Foo", null, () => new Mock<IDbExecutionStrategy>().Object)
                                  .GetServices<Func<IDbExecutionStrategy>>("a")).Message);
            }

            [Fact]
            public void GetServices_returns_empty_list_when_the_provider_name_doesnt_match()
            {
                Assert.Empty(
                    new ExecutionStrategyResolver<IDbExecutionStrategy>("Foo", null, () => new Mock<IDbExecutionStrategy>().Object)
                        .GetServices<Func<IDbExecutionStrategy>>(
                            new ExecutionStrategyKey("FooClient", "a")));
            }

            [Fact]
            public void GetServices_returns_empty_list_when_the_serverName_doesnt_match()
            {
                Assert.Empty(
                    new ExecutionStrategyResolver<IDbExecutionStrategy>("Foo", "b", () => new Mock<IDbExecutionStrategy>().Object)
                        .GetServices<Func<IDbExecutionStrategy>>(
                            new ExecutionStrategyKey("Foo", "a")));
            }

            [Fact]
            public void GetServices_returns_result_from_factory_method_on_match_null_serverName()
            {
                var mockExecutionStrategy = new Mock<IDbExecutionStrategy>().Object;
                var resolver = new ExecutionStrategyResolver<IDbExecutionStrategy>("Foo", null, () => mockExecutionStrategy);

                var resolvedExecutionStrategy = resolver.GetServices<Func<IDbExecutionStrategy>>(
                    new ExecutionStrategyKey("Foo", "bar")).Single()();

                Assert.Same(mockExecutionStrategy, resolvedExecutionStrategy);
            }

            [Fact]
            public void GetServices_returns_result_from_factory_method_on_match_notnull_serverName()
            {
                var mockExecutionStrategy = new Mock<IDbExecutionStrategy>().Object;
                var resolver = new ExecutionStrategyResolver<IDbExecutionStrategy>("Foo", "bar", () => mockExecutionStrategy);

                var resolvedExecutionStrategy = resolver.GetServices<Func<IDbExecutionStrategy>>(
                    new ExecutionStrategyKey("Foo", "bar")).Single()();

                Assert.Same(mockExecutionStrategy, resolvedExecutionStrategy);
            }
        }
    }
}
