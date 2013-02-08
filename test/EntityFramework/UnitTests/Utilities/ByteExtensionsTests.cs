// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
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
