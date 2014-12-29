// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Resources;
    using System.Data.Entity.SqlServer;
    using Xunit;

    public class DbProviderManifestExtensionsTests
    {
        [Fact]
        public void Can_get_type_from_name()
        {
            var providerManifest = new SqlProviderManifest("2008");

            Assert.NotNull(providerManifest.GetStoreTypeFromName("nvarchar(max)"));
            Assert.NotNull(providerManifest.GetStoreTypeFromName("Datetime2"));
            Assert.NotNull(providerManifest.GetStoreTypeFromName("TINYINT"));
        }

        [Fact]
        public void Get_non_existing_type_throws()
        {
            var providerManifest = new SqlProviderManifest("2008");
            const string typeName = "nvarchat";

            Assert.ThrowsDelegate test =
                () => providerManifest.GetStoreTypeFromName(typeName);

            Assert.Equal(
                Strings.StoreTypeNotFound(typeName, providerManifest.NamespaceName),
                Assert.Throws<InvalidOperationException>(test).Message);
        }
    }
}
