// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using Xunit;

    public class DefaultManifestTokenServiceTests
    {
        [Fact]
        public void GetProviderManifestToken_gets_the_token_from_the_provider_of_the_given_connection()
        {
            Assert.Equal("1908", new DefaultManifestTokenResolver().ResolveManifestToken(new FakeSqlConnection("1908")));
        }

        [Fact]
        public void GetProviderManifestToken_throws_if_given_null_connection()
        {
            Assert.Equal(
                "connection",
                Assert.Throws<ArgumentNullException>(() => new DefaultManifestTokenResolver().ResolveManifestToken(null)).ParamName);
        }
    }
}
