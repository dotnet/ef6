// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.ComponentModel.DataAnnotations.Schema
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class InversePropertyAttributeTests
    {
        [Fact]
        public void Property_can_be_got_and_set()
        {
            Assert.Equal("Gammer Brevis", new InversePropertyAttribute("Gammer Brevis").Property);
        }

        [Fact]
        public void Property_cannot_be_set_to_null_or_whitespace()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("property"),
                Assert.Throws<ArgumentException>(() => new InversePropertyAttribute(null)).Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("property"),
                Assert.Throws<ArgumentException>(() => new InversePropertyAttribute("")).Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("property"),
                Assert.Throws<ArgumentException>(() => new InversePropertyAttribute(" ")).Message);
        }
    }
}
