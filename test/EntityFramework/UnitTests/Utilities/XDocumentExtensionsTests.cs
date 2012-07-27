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
        public void GetStoreItemCollection_should_return_collection()
        {
            DbProviderInfo providerInfo;
            var storeItemCollection = new ShopContext_v1().GetModel().GetStoreItemCollection(out providerInfo);

            Assert.NotNull(storeItemCollection);
            Assert.NotNull(providerInfo);
            Assert.Equal("System.Data.SqlClient", providerInfo.ProviderInvariantName);
            // May return 2008 or 2012 depending on what version is installed on your dev box
            Assert.True(providerInfo.ProviderManifestToken == "2008" || providerInfo.ProviderManifestToken == "2012");
        }

        [Fact]
        public void HasSystemOperations_should_return_true_when_any_element_has_is_system_attribute()
        {
            var xdocument = new XDocument(new XElement("foo", new XAttribute(EdmXNames.IsSystem, "true")));

            Assert.True(xdocument.HasSystemOperations());
        }
    }
}
