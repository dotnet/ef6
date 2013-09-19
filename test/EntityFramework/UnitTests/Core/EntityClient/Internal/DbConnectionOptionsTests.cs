// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityClient.Internal
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class DbConnectionOptionsTests
    {
        [Fact]
        public void Name_keyword_is_parsed_correctly_in_named_connection_string()
        {
            var options = new DbConnectionOptions("Name=Suburbia", EntityConnectionStringBuilder.ValidKeywords);

            Assert.Equal("Suburbia", options.Parsetable[EntityConnectionStringBuilder.NameParameterName]);
            Assert.Equal("Suburbia", options[EntityConnectionStringBuilder.NameParameterName]);
            Assert.Null(options[EntityConnectionStringBuilder.ProviderParameterName]);
        }

        [Fact]
        public void All_other_keywords_are_parsed_correctly_in_non_named_connection_string()
        {
            var options =
                new DbConnectionOptions(
                    "Metadata=DjCulture; Provider=Its.A.Sin; Provider Connection String='Database=Domino Dancing'",
                    EntityConnectionStringBuilder.ValidKeywords);

            Assert.Equal("DjCulture", options.Parsetable[EntityConnectionStringBuilder.MetadataParameterName]);
            Assert.Equal("Its.A.Sin", options.Parsetable[EntityConnectionStringBuilder.ProviderParameterName]);
            Assert.Equal("Database=Domino Dancing", options.Parsetable[EntityConnectionStringBuilder.ProviderConnectionStringParameterName]);
        }

        [Fact]
        public void Invalid_keywords_are_detected()
        {
            Assert.Equal(
                Strings.ADP_KeywordNotSupported("love"),
                Assert.Throws<ArgumentException>(
                    () => new DbConnectionOptions("Love=ComesQuickly", EntityConnectionStringBuilder.ValidKeywords)).Message);
        }
    }
}
