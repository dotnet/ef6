namespace System.Data.Entity.Utilities
{
    using Xunit;

    public class DynamicEqualityComparerTests
    {
        [Fact]
        public void Equals_should_apply_provided_comparer_function()
        {
            var dynamicComparer = new DynamicEqualityComparer<string>((a, b) => a.Length > b.Length);

            Assert.True(dynamicComparer.Equals("a", ""));
            Assert.False(dynamicComparer.Equals("foo", "bar"));
        }

        [Fact]
        public void GetHashCode_is_no_op()
        {
            var dynamicComparer = new DynamicEqualityComparer<string>((_, __) => false);

            Assert.Equal(0, dynamicComparer.GetHashCode("a"));
            Assert.Equal(0, dynamicComparer.GetHashCode("b"));
        }
    }
}
