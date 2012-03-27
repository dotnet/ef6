namespace System.Data.Entity.ModelConfiguration.Utilities.UnitTests
{
    using System.Linq;
    using Xunit;

    public sealed class DynamicEqualityComparerLinqIntegrationTests
    {
        [Fact]
        public void Distinct_removes_duplicates_using_provided_equality_function()
        {
            var values = new[] { "a", "b", "A" };

            Assert.True(new[] { "a", "b" }.SequenceEqual(
                values.Distinct((a, b) => a.Equals(b, StringComparison.OrdinalIgnoreCase))));
        }

        [Fact]
        public void Intersect_returns_intersection_using_provided_equality_function()
        {
            var setA = new[] { "a", "b" };
            var setB = new[] { "A", "C" };

            Assert.True(new[] { "a" }.SequenceEqual(
                setA.Intersect(setB, (a, b) => a.Equals(b, StringComparison.OrdinalIgnoreCase))));
        }

        [Fact]
        public void GroupBy_groups_items_using_provided_equality_function()
        {
            var values = new[] { "a", "A" };

            Assert.Equal(1, values.GroupBy((a, b) => a.Equals(b, StringComparison.OrdinalIgnoreCase)).Count());
        }
    }
}