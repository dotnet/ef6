// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Edm;
    using System.Xml.Linq;
    using Xunit;

    public class XDocumentExtensionsTests
    {
        [Fact]
        public void GetStorageMappingItemCollection_should_return_collection()
        {
            DbProviderInfo providerInfo;
            var storageMappingItemCollection
                = new ShopContext_v1().GetModel().GetStorageMappingItemCollection(out providerInfo);

            Assert.NotNull(storageMappingItemCollection);
            Assert.NotNull(providerInfo);
            Assert.Equal("System.Data.SqlClient", providerInfo.ProviderInvariantName);
            Assert.True(providerInfo.ProviderManifestToken == "2008");
        }
    }
}
