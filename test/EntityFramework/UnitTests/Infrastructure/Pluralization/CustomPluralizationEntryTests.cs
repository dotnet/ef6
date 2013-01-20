// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Pluralization
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class CustomPluralizationEntryTests
    {
        [Fact]
        public void CustomPluralizationEntry_singular_term_cannot_be_set_to_null()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("singular"),
                Assert.Throws<ArgumentException>(() => new CustomPluralizationEntry(null, "plural")).Message);
        }

        [Fact]
        public void CustomPluralizationEntry_plural_term_cannot_be_set_to_null()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("plural"),
                Assert.Throws<ArgumentException>(() => new CustomPluralizationEntry("singular", null)).Message);
        }

        [Fact]
        public void CustomPluralizationEntry_singular_term_cannot_be_set_to_empty()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("singular"),
                Assert.Throws<ArgumentException>(() => new CustomPluralizationEntry(String.Empty, "plural")).Message);
        }

        [Fact]
        public void CustomPluralizationEntry_plural_term_cannot_be_set_to_empty()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("plural"),
                Assert.Throws<ArgumentException>(() => new CustomPluralizationEntry("singular", String.Empty)).Message);
        }

        [Fact]
        public void Singular_can_be_got()
        {
            Assert.Equal("SingularTerm", new CustomPluralizationEntry("SingularTerm", "Plural").Singular);
        }

        [Fact]
        public void Plural_can_be_got()
        {
            Assert.Equal("PluralTerm", new CustomPluralizationEntry("Singular", "PluralTerm").Plural);
        }
    }
}
