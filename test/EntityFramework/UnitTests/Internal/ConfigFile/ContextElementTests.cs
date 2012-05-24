namespace System.Data.Entity.Internal.ConfigFile
{
    using System;
    using System.Data.Entity;
    using Xunit;

    public class ContextElementTests : UnitTestBase
    {
        [Fact]
        public void ContextElement_converts_valid_type()
        {
            var param = new ContextElement { ContextTypeName = "System.Int32" };
            Assert.Equal(typeof(int), param.GetContextType());
        }

        [Fact]
        public void ContextElement_throws_converting_invalid_type()
        {
            var param = new ContextElement { ContextTypeName = "Not.A.Type" };

            Assert.True(Assert.Throws<TypeLoadException>(() => param.GetContextType()).Message.Contains(" 'Not.A.Type' "));
        }
    }
}