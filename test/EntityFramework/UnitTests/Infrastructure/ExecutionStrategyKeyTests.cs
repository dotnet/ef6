// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class ExecutionStrategyKeyTests
    {
        [Fact]
        public void Constructor_throws_on_invalid_parameters()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                Assert.Throws<ArgumentException>(() => new ExecutionStrategyKey(null, "")).Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                Assert.Throws<ArgumentException>(() => new ExecutionStrategyKey("", "b")).Message);
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
            Assert.True(
                equals(
                    new ExecutionStrategyKey("a", null),
                    new ExecutionStrategyKey("a", null)));
            Assert.False(
                equals(
                    new ExecutionStrategyKey("a", "b1"),
                    new ExecutionStrategyKey("a", "b2")));
            Assert.False(
                equals(
                    new ExecutionStrategyKey("a", null),
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
            Assert.Equal(
                new ExecutionStrategyKey("a", null).GetHashCode(),
                new ExecutionStrategyKey("a", null).GetHashCode());
            Assert.NotEqual(
                new ExecutionStrategyKey("a1", "b").GetHashCode(),
                new ExecutionStrategyKey("a2", "b").GetHashCode());
            Assert.NotEqual(
                new ExecutionStrategyKey("a", "b1").GetHashCode(),
                new ExecutionStrategyKey("a", "b2").GetHashCode());
        }
    }
}
