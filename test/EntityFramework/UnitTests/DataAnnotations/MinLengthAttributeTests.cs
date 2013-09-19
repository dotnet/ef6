// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.ComponentModel.DataAnnotations
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class MinLengthAttributeTests
    {
        [Fact]
        public void Length_returns_set_length()
        {
            Assert.Equal(10, new MinLengthAttribute(10).Length);
            Assert.Equal(0, new MinLengthAttribute(0).Length);

            // This only throws when IsValid is called
            Assert.Equal(-1, new MinLengthAttribute(-1).Length);
        }

        [Fact]
        public void IsValid_throws_for_negative_lengths()
        {
            var attribute = new MinLengthAttribute(-1);
            Assert.Equal(
                Strings.MinLengthAttribute_InvalidMinLength,
                Assert.Throws<InvalidOperationException>(() => attribute.IsValid("Rincewind")).Message);
        }

        [Fact]
        public void IsValid_throws_for_object_that_is_not_string_or_array()
        {
            Assert.Throws<InvalidCastException>(() => new MinLengthAttribute(0).IsValid(new Random()));
        }

        [Fact]
        public void IsValid_returns_true_for_null()
        {
            Assert.True(new MinLengthAttribute(10).IsValid(null));
        }

        [Fact]
        public void IsValid_validates_string_length()
        {
            Assert.True(new MinLengthAttribute(0).IsValid("The Luggage"));
            Assert.True(new MinLengthAttribute(4).IsValid("Hrun"));
            Assert.False(new MinLengthAttribute(11).IsValid("Mad Hamish"));
        }

        [Fact]
        public void IsValid_validates_array_length()
        {
            Assert.True(new MinLengthAttribute(0).IsValid(new int[500]));
            Assert.True(new MinLengthAttribute(10).IsValid(new string[10]));
            Assert.False(new MinLengthAttribute(5).IsValid(new byte[4]));
        }
    }
}
