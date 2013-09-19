// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.ComponentModel.DataAnnotations.Schema
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class ForeignKeyAttributeTests
    {
        [Fact]
        public void Name_can_be_got_and_set()
        {
            Assert.Equal("Old Mother Dismass", new ForeignKeyAttribute("Old Mother Dismass").Name);
        }

        [Fact]
        public void Name_cannot_be_set_to_null_or_whitespace()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(() => new ForeignKeyAttribute(null)).Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(() => new ForeignKeyAttribute("")).Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(() => new ForeignKeyAttribute(" ")).Message);
        }
    }
}
