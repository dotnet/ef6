// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class DefaultExecutionStrategyResolverTests : TestBase
    {
        public class GetService : TestBase
        {
            [Fact]
            public void GetService_returns_null_when_contract_interface_does_not_match()
            {
                Assert.Null(new DefaultExecutionStrategyResolver().GetService<IQueryable>());
            }

            [Fact]
            public void GetService_returns_execution_strategy()
            {
                Assert.IsType<DefaultExecutionStrategy>(
                    new DefaultExecutionStrategyResolver().GetService<Func<IDbExecutionStrategy>>(
                        new ExecutionStrategyKey("FooClient", "foo"))());
            }

            [Fact]
            public void GetService_throws_for_null_key()
            {
                Assert.Equal(
                    "key",
                    Assert.Throws<ArgumentNullException>(
                        () => new DefaultExecutionStrategyResolver().GetService<Func<IDbExecutionStrategy>>(null)).ParamName);
            }

            [Fact]
            public void GetService_throws_for_wrong_key_type()
            {
                Assert.Equal(
                    Strings.DbDependencyResolver_InvalidKey(typeof(ExecutionStrategyKey).Name, "Func<IExecutionStrategy>"),
                    Assert.Throws<ArgumentException>(
                        () => new DefaultExecutionStrategyResolver().GetService<Func<IDbExecutionStrategy>>("a")).Message);
            }
        }

        public class GetServices : TestBase
        {
            [Fact]
            public void GetServices_returns_empty_list_when_contract_interface_does_not_match()
            {
                Assert.Empty(new DefaultExecutionStrategyResolver().GetServices<IQueryable>());
            }

            [Fact]
            public void GetServices_returns_execution_strategy()
            {
                Assert.IsType<DefaultExecutionStrategy>(
                    new DefaultExecutionStrategyResolver().GetServices<Func<IDbExecutionStrategy>>(
                        new ExecutionStrategyKey("FooClient", "foo")).Single()());
            }

            [Fact]
            public void GetServices_throws_for_null_key()
            {
                Assert.Equal(
                    "key",
                    Assert.Throws<ArgumentNullException>(
                        () => new DefaultExecutionStrategyResolver().GetServices<Func<IDbExecutionStrategy>>(null)).ParamName);
            }

            [Fact]
            public void GetServices_throws_for_wrong_key_type()
            {
                Assert.Equal(
                    Strings.DbDependencyResolver_InvalidKey(typeof(ExecutionStrategyKey).Name, "Func<IExecutionStrategy>"),
                    Assert.Throws<ArgumentException>(
                        () => new DefaultExecutionStrategyResolver().GetServices<Func<IDbExecutionStrategy>>("a")).Message);
            }
        }
    }
}
