// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.ComponentModel.DataAnnotations.Schema
{
    using Xunit;

    public class DatabaseGeneratedAttributeTests
    {
        [Fact]
        public void DatabaseGeneratedOption_can_be_got_and_set()
        {
            Assert.Equal(
                DatabaseGeneratedOption.Computed, new DatabaseGeneratedAttribute(DatabaseGeneratedOption.Computed).DatabaseGeneratedOption);
        }

        [Fact]
        public void DatabaseGeneratedOption_cannot_be_set_to_a_value_not_in_the_enum()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DatabaseGeneratedAttribute((DatabaseGeneratedOption)10));
        }
    }
}
