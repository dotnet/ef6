// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class EdmFunctionAttributeTests : TestBase
    {
        [Fact]
        public void Namespace_and_name_can_be_set_and_obtained()
        {
#pragma warning disable 612,618
            var attribute = new EdmFunctionAttribute("D.W.", "Buster");
#pragma warning restore 612,618
            Assert.Equal("D.W.", attribute.NamespaceName);
            Assert.Equal("Buster", attribute.FunctionName);
        }

        [Fact]
        public void DbFunctionAttribute_throws_for_null_or_empty_args()
        {
#pragma warning disable 612,618
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("namespaceName"),
                Assert.Throws<ArgumentException>(() => new EdmFunctionAttribute(null, "Francine")).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("namespaceName"),
                Assert.Throws<ArgumentException>(() => new EdmFunctionAttribute("", "Muffy")).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("namespaceName"),
                Assert.Throws<ArgumentException>(() => new EdmFunctionAttribute(" ", "Brain")).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("functionName"),
                Assert.Throws<ArgumentException>(() => new EdmFunctionAttribute("Binky", null)).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("functionName"),
                Assert.Throws<ArgumentException>(() => new EdmFunctionAttribute("Tommy", "")).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("functionName"),
                Assert.Throws<ArgumentException>(() => new EdmFunctionAttribute("Timmy", " ")).Message);
#pragma warning restore 612,618
        }
    }
}
