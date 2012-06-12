namespace Microsoft.DbContextPackage.Extensions
{
    using Xunit;

    public class StringExtensionsTests
    {
        [Fact]
        public void EqualsIgnoreCase_tests_equality()
        {
            Assert.True(StringExtensions.EqualsIgnoreCase(null, null));
            Assert.True("one".EqualsIgnoreCase("one"));
            Assert.True("two".EqualsIgnoreCase("TWO"));
            Assert.False("three".EqualsIgnoreCase("four"));
            Assert.False("five".EqualsIgnoreCase(null));
        }
    }
}
