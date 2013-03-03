// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class DbFunctionAttributeTests : TestBase
    {
        [Fact]
        public void Namespace_and_name_can_be_set_and_obtained()
        {
            var attribute = new DbFunctionAttribute("D.W.", "Buster");
            Assert.Equal("D.W.", attribute.NamespaceName);
            Assert.Equal("Buster", attribute.FunctionName);
        }

        [Fact]
        public void DbFunctionAttribute_throws_for_null_or_empty_args()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("namespaceName"),
                Assert.Throws<ArgumentException>(() => new DbFunctionAttribute(null, "Francine")).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("namespaceName"),
                Assert.Throws<ArgumentException>(() => new DbFunctionAttribute("", "Muffy")).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("namespaceName"),
                Assert.Throws<ArgumentException>(() => new DbFunctionAttribute(" ", "Brain")).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("functionName"),
                Assert.Throws<ArgumentException>(() => new DbFunctionAttribute("Binky", null)).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("functionName"),
                Assert.Throws<ArgumentException>(() => new DbFunctionAttribute("Tommy", "")).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("functionName"),
                Assert.Throws<ArgumentException>(() => new DbFunctionAttribute("Timmy", " ")).Message);
        }
    }
}
