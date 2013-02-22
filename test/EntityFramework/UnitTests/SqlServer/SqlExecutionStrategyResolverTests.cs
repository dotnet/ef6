// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Config;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Moq;
    using Xunit;

    public class SqlExecutionStrategyResolverTests
    {
        [Fact]
        public void Constructor_throws_on_null()
        {
            Assert.Equal(
                "getExecutionStrategy",
                Assert.Throws<ArgumentNullException>(() => new SqlExecutionStrategyResolver(null, "a")).ParamName);
        }

        [Fact]
        public void GetService_returns_null_when_contract_interface_does_not_match()
        {
            Assert.Null(new SqlExecutionStrategyResolver(() => new Mock<IExecutionStrategy>().Object, null).GetService<IQueryable>());
        }

        [Fact]
        public void GetService_throws_for_null_key()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlExecutionStrategyResolver(() => new Mock<IExecutionStrategy>().Object, null).GetService<IExecutionStrategy>(null));
        }

        [Fact]
        public void GetService_returns_null_when_the_key_is_not_ExecutionStrategyKey()
        {
            Assert.Null(
                new SqlExecutionStrategyResolver(() => new Mock<IExecutionStrategy>().Object, null).GetService<IExecutionStrategy>("a"));
        }

        [Fact]
        public void GetService_returns_null_when_the_provider_name_doesnt_match()
        {
            Assert.Null(
                new SqlExecutionStrategyResolver(() => new Mock<IExecutionStrategy>().Object, null).GetService<IExecutionStrategy>(
                    new ExecutionStrategyKey("FooClient", "a")));
        }

        [Fact]
        public void GetService_returns_null_when_the_datasource_doesnt_match()
        {
            Assert.Null(
                new SqlExecutionStrategyResolver(() => new Mock<IExecutionStrategy>().Object, "b").GetService<IExecutionStrategy>(
                    new ExecutionStrategyKey("System.Data.SqlClient", "a")));
        }

        [Fact]
        public void GetService_returns_result_from_factory_method_on_match_null_datasource()
        {
            var mockExecutionStrategy = new Mock<IExecutionStrategy>().Object;
            var resolver = new SqlExecutionStrategyResolver(() => mockExecutionStrategy, null);

            var resolvedExecutionStrategy = resolver.GetService<IExecutionStrategy>(
                new ExecutionStrategyKey("System.Data.SqlClient", "foo"));

            Assert.Same(mockExecutionStrategy, resolvedExecutionStrategy);
        }

        [Fact]
        public void GetService_returns_result_from_factory_method_on_match_notnull_datasource()
        {
            var mockExecutionStrategy = new Mock<IExecutionStrategy>().Object;
            var resolver = new SqlExecutionStrategyResolver(() => mockExecutionStrategy, "foo");

            var resolvedExecutionStrategy = resolver.GetService<IExecutionStrategy>(
                new ExecutionStrategyKey("System.Data.SqlClient", "foo"));

            Assert.Same(mockExecutionStrategy, resolvedExecutionStrategy);
        }
    }
}
