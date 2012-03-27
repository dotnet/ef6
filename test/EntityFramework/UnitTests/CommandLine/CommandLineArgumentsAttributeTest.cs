namespace CmdLine.Tests
{
    using System;
    using Xunit;

    public class CommandLineArgumentsAttributeTest
    {
        [Fact]
        public void GetShouldReturnCommandLineArgumentsAttribute()
        {
            var attribute = CommandLineArgumentsAttribute.Get(typeof(XCopyCommandArgs));
            Assert.NotNull(attribute);
            Assert.Equal(XCopyCommandArgs.Title, attribute.Title);
            Assert.Equal(XCopyCommandArgs.Description, attribute.Description);
        }

        [Fact]
        public void GetReturnsNullWhenNoAttribute()
        {
            var attribute = CommandLineArgumentsAttribute.Get(typeof(string));
            Assert.Null(attribute);
        }

        [Fact]
        public void GetThrowsArgumentNullWhenNull()
        {
            Assert.Equal("element", Assert.Throws<ArgumentNullException>(() => CommandLineArgumentsAttribute.Get(null)).ParamName);
        }
    }
}