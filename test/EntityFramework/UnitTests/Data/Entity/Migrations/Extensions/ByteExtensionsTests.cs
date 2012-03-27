namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Extensions;
    using Xunit;

    public class ByteExtensionsTests
    {
        [Fact]
        public void Should_be_able_to_get_hex_string_from_bytes()
        {
            var bytes = new byte[] { 0x0, 0x1, 0x2 };

            Assert.Equal("000102", bytes.ToHexString());
        }
    }
}