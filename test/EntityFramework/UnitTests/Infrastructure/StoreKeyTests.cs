// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class StoreKeyTests
    {
        [Fact]
        public void Constructor_throws_on_invalid_parameters()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                Assert.Throws<ArgumentException>(() => new StoreKey(null, null)).Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                Assert.Throws<ArgumentException>(() => new StoreKey("", null)).Message);
        }

        [Fact]
        public void ProviderInvariantName_returns_the_supplied_value()
        {
            Assert.Equal("p", new StoreKey("p", null).ProviderInvariantName);
        }

        [Fact]
        public void ServerName_returns_the_supplied_value()
        {
            Assert.Equal("s", new StoreKey("p", "s").ServerName);
        }
    }
}
