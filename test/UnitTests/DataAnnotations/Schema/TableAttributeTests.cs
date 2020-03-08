// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.ComponentModel.DataAnnotations.Schema
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class TableAttributeTests
    {
        [Fact]
        public void Default_value_for_schema_is_null()
        {
            Assert.Null(new TableAttribute("Perspicacia Tick").Schema);
        }

        [Fact]
        public void Name_can_be_got_and_set()
        {
            Assert.Equal("Black Aliss", new TableAttribute("Black Aliss").Name);
        }

#if NET40
        [Fact]
        public void Name_cannot_be_set_to_null_or_whitespace()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(() => new TableAttribute(null)).Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(() => new TableAttribute("")).Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(() => new TableAttribute(" ")).Message);
        }
#endif

        [Fact]
        public void Schema_can_be_got_and_set()
        {
            Assert.Equal(
                "Mrs Letice Earwig", new TableAttribute("Perspicacia Tick")
                {
                    Schema = "Mrs Letice Earwig"
                }.Schema);
        }

#if NET40
        [Fact]
        public void Schema_cannot_be_set_to_null_or_whitespace()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("value"),
                Assert.Throws<ArgumentException>(
                    () => new TableAttribute("Perspicacia Tick")
                              {
                                  Schema = null
                              }).Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("value"),
                Assert.Throws<ArgumentException>(
                    () => new TableAttribute("Perspicacia Tick")
                              {
                                  Schema = ""
                              }).Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("value"),
                Assert.Throws<ArgumentException>(
                    () => new TableAttribute("Perspicacia Tick")
                              {
                                  Schema = " "
                              }).Message);
        }
#endif
    }
}
