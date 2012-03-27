namespace System.Data.Entity.ModelConfiguration.Utilities.UnitTests
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Resources;
    using Xunit;

    public sealed class RuntimeFailureMethodsTests
    {
        [Fact]
        public void Requires_non_null_object_is_detected_and_correct_exception_is_generated()
        {
            Assert.Equal(Error.ArgumentNull("keyExpression").Message, Assert.Throws<ArgumentNullException>(() => new EntityTypeConfiguration<object>().HasKey<int>(null)).Message);
        }

        [Fact]
        public void Requires_non_empty_string_is_detected_and_correct_exception_is_generated()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("name"), Assert.Throws<ArgumentException>(() => new ColumnAttribute(null)).Message);
        }

        [Fact]
        public void NotNull_regex_matches_correctly()
        {
            Assert.True(RuntimeFailureMethods.IsNotNull.IsMatch("foo != null"));
            Assert.True(RuntimeFailureMethods.IsNotNull.IsMatch(" @Fo_o!=null "));
            Assert.False(RuntimeFailureMethods.IsNotNull.IsMatch(" _Foo && bar !=null "));
        }

        [Fact]
        public void IsNullOrWhiteSpace_regex_matches_correctly()
        {
            Assert.True(RuntimeFailureMethods.IsNullOrWhiteSpace.IsMatch("!string.IsNullOrWhiteSpace(foo)"));
            Assert.True(RuntimeFailureMethods.IsNullOrWhiteSpace.IsMatch(" ! String . IsNullOrWhiteSpace ( @_foo ) "));
            Assert.False(RuntimeFailureMethods.IsNullOrWhiteSpace.IsMatch("string.IsNullOrWhiteSpace(foo)"));
        }
    }
}