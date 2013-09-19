// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.ComponentModel.DataAnnotations.Schema
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class ColumnAttributeTests
    {
        [Fact]
        public void Default_values_are_null_or_negative_one()
        {
            Assert.Null(new ColumnAttribute().Name);
            Assert.Equal(-1, new ColumnAttribute().Order);
            Assert.Null(new ColumnAttribute().TypeName);
        }

        [Fact]
        public void Name_can_be_got_and_set()
        {
            Assert.Equal("Granny Weatherwax", new ColumnAttribute("Granny Weatherwax").Name);
        }

        [Fact]
        public void Name_cannot_be_set_to_null_or_whitespace()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(() => new ColumnAttribute(null)).Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(() => new ColumnAttribute("")).Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(() => new ColumnAttribute(" ")).Message);
        }

        [Fact]
        public void Order_can_be_got_and_set()
        {
            Assert.Equal(
                0,
                new ColumnAttribute
                    {
                        Order = 0
                    }.Order);
        }

        [Fact]
        public void Order_cannot_be_set_to_negative_value()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new ColumnAttribute
                          {
                              Order = -1
                          });
        }

        [Fact]
        public void TypeName_can_be_got_and_set()
        {
            Assert.Equal(
                "Nanny Ogg",
                new ColumnAttribute
                    {
                        TypeName = "Nanny Ogg"
                    }.TypeName);
        }

        [Fact]
        public void TypeName_cannot_be_set_to_null_or_whitespace()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("value"),
                Assert.Throws<ArgumentException>(
                    () => new ColumnAttribute
                              {
                                  TypeName = null
                              }).Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("value"),
                Assert.Throws<ArgumentException>(
                    () => new ColumnAttribute
                              {
                                  TypeName = ""
                              }).Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("value"),
                Assert.Throws<ArgumentException>(
                    () => new ColumnAttribute
                              {
                                  TypeName = " "
                              }).Message);
        }
    }
}
