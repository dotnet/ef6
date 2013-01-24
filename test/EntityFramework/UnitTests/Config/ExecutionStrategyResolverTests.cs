// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using FunctionalTests.TestHelpers;
    using Moq;
    using Xunit;

    public class ExecutionStrategyResolverTests : TestBase
    {
        [Fact]
        public void GetService_returns_null_when_contract_interface_does_not_match()
        {
            Assert.Null(new ExecutionStrategyResolver().GetService<IQueryable>());
        }

        [Fact]
        public void GetService_returns_execution_strategy_from_provider()
        {
            var mockExecutionStrategy = new Mock<NonRetryingExecutionStrategy>().Object;
            var providerServicesMock = new Mock<DbProviderServices>();
            providerServicesMock.Setup(m => m.GetExecutionStrategy()).Returns(mockExecutionStrategy);
            var mockProviderServices = providerServicesMock.Object;
            var resolver = new ExecutionStrategyResolver();

            MutableResolver.AddResolver<DbProviderServices>(
                key =>
                {
                    var invariantName = key as string;
                    return "FooClient" == invariantName ? mockProviderServices : null;
                });

            IExecutionStrategy resolvedExecutionStrategy;
            try
            {
                resolvedExecutionStrategy = resolver.GetService<IExecutionStrategy>(new ExecutionStrategyKey("FooClient", "foo"));
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }

            Assert.Same(mockExecutionStrategy, resolvedExecutionStrategy);
        }

        [Fact]
        public void The_root_resolver_returns_null_execution_strategy_for_null_key()
        {
            Assert.Null(new ExecutionStrategyResolver().GetService<IExecutionStrategy>(null));
        }
    }
}
