namespace System.Data.Entity.Core.Objects
{
    using System;
    using Xunit;

    public class FieldDescriptorTests
    {
        [Fact]
        public void GetValue_throws_for_null_argument()
        {
            Assert.Equal("item",
                Assert.Throws<ArgumentNullException>(
                    () => new FieldDescriptor("foo").GetValue(null)).ParamName);
        }

        [Fact]
        public void SetValue_throws_for_null_argument()
        {
            Assert.Equal("item",
                Assert.Throws<ArgumentNullException>(
                    () => new FieldDescriptor("foo").SetValue(null, new object())).ParamName);
        }
    }
}
