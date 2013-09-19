// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Linq;
    using Xunit;

    public class DynamicEqualityComparerLinqIntegrationTests
    {
        [Fact]
        public void Distinct_removes_duplicates_using_provided_equality_function()
        {
            var values = new[] { "a", "b", "A" };

            Assert.True(
                new[] { "a", "b" }.SequenceEqual(
                    values.Distinct((a, b) => a.Equals(b, StringComparison.OrdinalIgnoreCase))));
        }

        [Fact]
        public void Contains_uses_provided_equality_function()
        {
            var values = new[] { "a", "b", "A" };

            Assert.True(values.Contains("B", (a, b) => a.Equals(b, StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public void Intersect_returns_intersection_using_provided_equality_function()
        {
            var setA = new[] { "a", "b" };
            var setB = new[] { "A", "C" };

            Assert.True(
                new[] { "a" }.SequenceEqual(
                    setA.Intersect(setB, (a, b) => a.Equals(b, StringComparison.OrdinalIgnoreCase))));
        }

        [Fact]
        public void GroupBy_groups_items_using_provided_equality_function()
        {
            var values = new[] { "a", "A" };

            Assert.Equal(1, values.GroupBy((a, b) => a.Equals(b, StringComparison.OrdinalIgnoreCase)).Count());
        }

        [Fact]
        public void SequenceEqual_compares_items_using_provided_equality_function()
        {
            var first = new[] { "a", "A" };
            var second = new[] { "A", "a" };

            Assert.True(first.SequenceEqual(second, (a, b) => a.Equals(b, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
