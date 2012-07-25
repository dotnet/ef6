// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations;
    using Xunit;

    public class XDocumentExtensionsTests
    {
        [Fact]
        public void GetStoreItemCollection_should_return_collection()
        {
            DbProviderInfo providerInfo;
            var storeItemCollection = new ShopContext_v1().GetModel().GetStoreItemCollection(out providerInfo);

            Assert.NotNull(storeItemCollection);
            Assert.NotNull(providerInfo);
            Assert.Equal("System.Data.SqlClient", providerInfo.ProviderInvariantName);
            Assert.Equal("2008", providerInfo.ProviderManifestToken);
        }
    }
}