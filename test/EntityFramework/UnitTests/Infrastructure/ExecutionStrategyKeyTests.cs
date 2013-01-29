// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using Xunit;

    public class ExecutionStrategyKeyTests
    {
        [Fact]
        public void Constructor_throws_on_invalid_parameters()
        {
            Assert.Throws<ArgumentException>(() => new ExecutionStrategyKey(null, ""))
                  .ValidateMessage("ArgumentIsNullOrWhitespace", "invariantProviderName");
            Assert.Throws<ArgumentException>(() => new ExecutionStrategyKey("", "b"))
                  .ValidateMessage("ArgumentIsNullOrWhitespace", "invariantProviderName");
            Assert.Throws<ArgumentException>(() => new ExecutionStrategyKey("a", null))
                  .ValidateMessage("ArgumentIsNullOrWhitespace", "dataSource");
            Assert.Throws<ArgumentException>(() => new ExecutionStrategyKey("a", ""))
                  .ValidateMessage("ArgumentIsNullOrWhitespace", "dataSource");
        }

        [Fact]
        public void Equals_returns_correct_results()
        {
            TestEquals(
                (left, right) => left.Equals(right));
        }

        private void TestEquals(Func<object, object, bool> equals)
        {
            var sameInstace = new ExecutionStrategyKey("a", "b");
            Assert.True(
                equals(
                    sameInstace,
                    sameInstace));
            Assert.True(
                equals(
                    new ExecutionStrategyKey("a", "b"),
                    new ExecutionStrategyKey("a", "b")));
            Assert.False(
                equals(
                    new ExecutionStrategyKey("a", "b1"),
                    new ExecutionStrategyKey("a", "b2")));
            Assert.False(
                equals(
                    new ExecutionStrategyKey("a1", "b"),
                    new ExecutionStrategyKey("a2", "b")));
            Assert.False(
                equals(
                    new ExecutionStrategyKey("a", "b"),
                    null));
        }

        [Fact]
        public void GetHashCode_returns_correct_results()
        {
            Assert.Equal(
                new ExecutionStrategyKey("a", "b").GetHashCode(),
                new ExecutionStrategyKey("a", "b").GetHashCode());
            Assert.NotEqual(
                new ExecutionStrategyKey("a1", "b").GetHashCode(),
                new ExecutionStrategyKey("a2", "b").GetHashCode());
            Assert.NotEqual(
                new ExecutionStrategyKey("a", "b1").GetHashCode(),
                new ExecutionStrategyKey("a", "b2").GetHashCode());
        }
    }
}
