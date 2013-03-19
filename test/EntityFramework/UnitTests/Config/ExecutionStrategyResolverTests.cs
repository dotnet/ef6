// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Moq;
    using Xunit;

    public class ExecutionStrategyResolverTests
    {
        [Fact]
        public void Constructor_throws_on_invalid_arguments()
        {
            Assert.Equal(
                "getExecutionStrategy",
                Assert.Throws<ArgumentNullException>(() => new ExecutionStrategyResolver<IExecutionStrategy>("Foo", "a", null)).ParamName);

            Assert.Throws<ArgumentException>(
                () => new ExecutionStrategyResolver<IExecutionStrategy>(null, "a", () => new Mock<IExecutionStrategy>().Object));
            Assert.Throws<ArgumentException>(
                () => new ExecutionStrategyResolver<IExecutionStrategy>("", "a", () => new Mock<IExecutionStrategy>().Object));
        }

        [Fact]
        public void GetService_returns_null_when_contract_interface_does_not_match()
        {
            Assert.Null(
                new ExecutionStrategyResolver<IExecutionStrategy>("Foo", null, () => new Mock<IExecutionStrategy>().Object)
                    .GetService<IQueryable>());
        }

        [Fact]
        public void GetService_throws_for_null_or_incorrect_key_type()
        {
            Assert.Equal(
                Strings.DbDependencyResolver_InvalidKey(typeof(ExecutionStrategyKey).Name, "Func<IExecutionStrategy>"),
                Assert.Throws<ArgumentException>(
                    () => new ExecutionStrategyResolver<IExecutionStrategy>("Foo", null, () => new Mock<IExecutionStrategy>().Object)
                              .GetService<Func<IExecutionStrategy>>(null)).Message);

            Assert.Equal(
                Strings.DbDependencyResolver_InvalidKey(typeof(ExecutionStrategyKey).Name, "Func<IExecutionStrategy>"),
                Assert.Throws<ArgumentException>(
                    () => new ExecutionStrategyResolver<IExecutionStrategy>("Foo", null, () => new Mock<IExecutionStrategy>().Object)
                              .GetService<Func<IExecutionStrategy>>("a")).Message);
        }
        
        [Fact]
        public void GetService_returns_null_when_the_provider_name_doesnt_match()
        {
            Assert.Null(
                new ExecutionStrategyResolver<IExecutionStrategy>("Foo", null, () => new Mock<IExecutionStrategy>().Object)
                    .GetService<Func<IExecutionStrategy>>(
                        new ExecutionStrategyKey("FooClient", "a")));
        }

        [Fact]
        public void GetService_returns_null_when_the_serverName_doesnt_match()
        {
            Assert.Null(
                new ExecutionStrategyResolver<IExecutionStrategy>("Foo", "b", () => new Mock<IExecutionStrategy>().Object)
                    .GetService<Func<IExecutionStrategy>>(
                        new ExecutionStrategyKey("Foo", "a")));
        }

        [Fact]
        public void GetService_returns_result_from_factory_method_on_match_null_serverName()
        {
            var mockExecutionStrategy = new Mock<IExecutionStrategy>().Object;
            var resolver = new ExecutionStrategyResolver<IExecutionStrategy>("Foo", null, () => mockExecutionStrategy);

            var resolvedExecutionStrategy = resolver.GetService<Func<IExecutionStrategy>>(
                new ExecutionStrategyKey("Foo", "bar"))();

            Assert.Same(mockExecutionStrategy, resolvedExecutionStrategy);
        }

        [Fact]
        public void GetService_returns_result_from_factory_method_on_match_notnull_serverName()
        {
            var mockExecutionStrategy = new Mock<IExecutionStrategy>().Object;
            var resolver = new ExecutionStrategyResolver<IExecutionStrategy>("Foo", "bar", () => mockExecutionStrategy);

            var resolvedExecutionStrategy = resolver.GetService<Func<IExecutionStrategy>>(
                new ExecutionStrategyKey("Foo", "bar"))();

            Assert.Same(mockExecutionStrategy, resolvedExecutionStrategy);
        }
    }
}
