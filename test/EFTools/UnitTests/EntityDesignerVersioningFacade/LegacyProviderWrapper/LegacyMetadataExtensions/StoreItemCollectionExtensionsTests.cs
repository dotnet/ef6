// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using LegacyMetadata = System.Data.Metadata.Edm;

namespace Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper.LegacyMetadataExtensions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public class StoreItemCollectionExtensionsTests
    {
        [Fact]
        public void LegacyStoreItemCollection_and_source_StoreItemCollection_are_equivalent_for_all_versions()
        {
            const string ssdlTemplate =
                @"<Schema Namespace=""Model.Store"" Provider=""System.Data.SqlClient"" " +
                @"    ProviderManifestToken=""2008"" Alias=""Self"" xmlns=""{0}"">" +
                @"    <EntityContainer Name=""TestContainer"" />" +
                @"</Schema>";

            var schemaVersions = new[]
                {
                    "http://schemas.microsoft.com/ado/2006/04/edm/ssdl",
                    "http://schemas.microsoft.com/ado/2009/02/edm/ssdl",
                    "http://schemas.microsoft.com/ado/2009/11/edm/ssdl"
                };

            foreach (var schemaVersion in schemaVersions)
            {
                var storeItemCollection = Utils.CreateStoreItemCollection(string.Format(ssdlTemplate, schemaVersion));
                var legacyStoreItemCollection = storeItemCollection.ToLegacyStoreItemCollection("Model.Store");

                Assert.Equal(storeItemCollection.StoreSchemaVersion, legacyStoreItemCollection.StoreSchemaVersion);

                Assert.Equal(
                    storeItemCollection.GetItems<GlobalItem>().Count,
                    legacyStoreItemCollection.GetItems<LegacyMetadata.GlobalItem>().Count);
            }
        }

        [Fact]
        public void LegacyStoreItemCollection_and_source_StoreItemCollection_are_equivalent()
        {
            const string ssdl =
                @"<Schema Namespace=""NorthwindEF5Model.Store"" Provider=""System.Data.SqlClient"" ProviderManifestToken=""2008"" Alias=""Self"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm/ssdl"" xmlns:store=""http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator"">"
                +
                @"  <EntityType Name=""Customers"">" +
                @"    <Key>" +
                @"      <PropertyRef Name=""CustomerID"" />" +
                @"    </Key>" +
                @"    <Property Name=""CustomerID"" Type=""nchar"" MaxLength=""5"" Nullable=""false"" />" +
                @"    <Property Name=""CompanyName"" Type=""nvarchar"" MaxLength=""40"" Nullable=""false"" />" +
                @"  </EntityType>" +
                @"  <EntityType Name=""Orders"">" +
                @"    <Key>" +
                @"      <PropertyRef Name=""OrderID"" />" +
                @"    </Key>" +
                @"    <Property Name=""OrderID"" Type=""int"" StoreGeneratedPattern=""Identity"" Nullable=""false"" />" +
                @"    <Property Name=""CustomerID"" Type=""nchar"" MaxLength=""5"" />" +
                @"  </EntityType>" +
                @"  <Association Name=""FK_Orders_Customers"">" +
                @"    <End Role=""Customers"" Type=""Self.Customers"" Multiplicity=""0..1"" />" +
                @"    <End Role=""Orders"" Type=""Self.Orders"" Multiplicity=""*"" />" +
                @"    <ReferentialConstraint>" +
                @"      <Principal Role=""Customers"">" +
                @"        <PropertyRef Name=""CustomerID"" />" +
                @"      </Principal>" +
                @"      <Dependent Role=""Orders"">" +
                @"        <PropertyRef Name=""CustomerID"" />" +
                @"      </Dependent>" +
                @"    </ReferentialConstraint>" +
                @"  </Association>" +
                @"  <EntityContainer Name=""NorthwindEF5ModelStoreContainer"">" +
                @"    <EntitySet Name=""Customers"" EntityType=""Self.Customers"" Schema=""dbo"" store:Type=""Tables"" />" +
                @"    <EntitySet Name=""Orders"" EntityType=""Self.Orders"" Schema=""dbo"" store:Type=""Tables"" />" +
                @"    <AssociationSet Name=""FK_Orders_Customers"" Association=""Self.FK_Orders_Customers"">" +
                @"      <End Role=""Customers"" EntitySet=""Customers"" />" +
                @"      <End Role=""Orders"" EntitySet=""Orders"" />" +
                @"    </AssociationSet>" +
                @"  </EntityContainer>" +
                @"</Schema>";

            var storeItemCollection = Utils.CreateStoreItemCollection(ssdl);
            var legacyStoreItemCollection = storeItemCollection.ToLegacyStoreItemCollection();

            var sourceGlobalItems = storeItemCollection.GetItems<EdmType>();
            var legacyGlobalItems = legacyStoreItemCollection.GetItems<LegacyMetadata.EdmType>();

            Assert.Equal(sourceGlobalItems.Count, legacyGlobalItems.Count);
            Assert.True(sourceGlobalItems.All(i => legacyGlobalItems.Any(j => j.FullName == i.FullName)));
        }
    }
}
