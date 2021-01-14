// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class DbProviderInfoTests
    {
        [Fact]
        public void DbProviderInfo_constructor_validates_arguments()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                Assert.Throws<ArgumentException>(() => new DbProviderInfo(null, "")).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                Assert.Throws<ArgumentException>(() => new DbProviderInfo("", "")).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                Assert.Throws<ArgumentException>(() => new DbProviderInfo(" ", "")).Message);

            Assert.Equal(
                "providerManifestToken",
                Assert.Throws<ArgumentNullException>(
                    () => new DbProviderInfo("Bill.And.Ben", null))
                      .ParamName);
        }

        [Fact]
        public void Invariant_name_and_manifest_token_properties_return_set_values()
        {
            var key = new DbProviderInfo("Bill", "Ben");

            Assert.Equal("Bill", key.ProviderInvariantName);
            Assert.Equal("Ben", key.ProviderManifestToken);
        }

        [Fact]
        public void Equals_correctly_determines_equality()
        {
            var key = new DbProviderInfo("Bill", "Ben");

            Assert.True(key.Equals(key));
            Assert.True(key.Equals(new DbProviderInfo("Bill", "Ben")));
            Assert.False(key.Equals(new DbProviderInfo("Bill", "Men")));
            Assert.False(key.Equals(new DbProviderInfo("FlowerPot", "Ben")));
            Assert.False(key.Equals(null));
            Assert.False(key.Equals("Bill.And.Ben"));
        }

        [Fact]
        public void GetHashCode_returns_same_hashcode_for_equal_keys()
        {
            Assert.Equal(new DbProviderInfo("Bill", "Ben").GetHashCode(), new DbProviderInfo("Bill", "Ben").GetHashCode());
        }
    }
}
