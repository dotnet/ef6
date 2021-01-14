// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.ComponentModel.DataAnnotations
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class MaxLengthAttributeTests
    {
        [Fact]
        public void Length_returns_set_length()
        {
            Assert.Equal(-1, new MaxLengthAttribute().Length);
            Assert.Equal(-1, new MaxLengthAttribute(-1).Length);
            Assert.Equal(10, new MaxLengthAttribute(10).Length);

            // These only throw when IsValid is called
            Assert.Equal(0, new MaxLengthAttribute(0).Length);
            Assert.Equal(-10, new MaxLengthAttribute(-10).Length);
        }

#if NET40
        [Fact]
        public void IsValid_throws_for_negative_or_zero_lengths_other_than_negative_one()
        {
            var attribute1 = new MaxLengthAttribute(-10);
            Assert.Equal(
                Strings.MaxLengthAttribute_InvalidMaxLength,
                Assert.Throws<InvalidOperationException>(() => attribute1.IsValid("Rincewind")).Message);

            var attribute2 = new MaxLengthAttribute(0);
            Assert.Equal(
                Strings.MaxLengthAttribute_InvalidMaxLength,
                Assert.Throws<InvalidOperationException>(() => attribute2.IsValid("Twoflower")).Message);
        }
#endif

        [Fact]
        public void IsValid_throws_for_object_that_is_not_string_or_array()
        {
            Assert.Throws<InvalidCastException>(() => new MaxLengthAttribute().IsValid(new Random()));
        }

        [Fact]
        public void IsValid_returns_true_for_null()
        {
            Assert.True(new MaxLengthAttribute(10).IsValid(null));
        }

        [Fact]
        public void IsValid_validates_string_length()
        {
            Assert.True(new MaxLengthAttribute().IsValid("The Luggage"));
            Assert.True(new MaxLengthAttribute(10).IsValid("Hrun"));
            Assert.True(new MaxLengthAttribute(18).IsValid("Mr. Ronald Saveloy"));
            Assert.True(new MaxLengthAttribute(-1).IsValid("Cohen"));
            Assert.False(new MaxLengthAttribute(5).IsValid("Mad Hamish"));
        }

        [Fact]
        public void IsValid_validates_array_length()
        {
            Assert.True(new MaxLengthAttribute().IsValid(new int[500]));
            Assert.True(new MaxLengthAttribute(10).IsValid(new string[7]));
            Assert.True(new MaxLengthAttribute(10).IsValid(new string[10]));
            Assert.True(new MaxLengthAttribute(-1).IsValid(new object[500]));
            Assert.False(new MaxLengthAttribute(5).IsValid(new byte[6]));
        }
    }
}
