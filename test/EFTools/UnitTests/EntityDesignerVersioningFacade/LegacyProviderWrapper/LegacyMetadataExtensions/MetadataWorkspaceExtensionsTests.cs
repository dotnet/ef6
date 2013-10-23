// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using LegacyMetadata = System.Data.Metadata.Edm;

namespace Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper.LegacyMetadataExtensions
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.IO;
    using System.Xml;
    using Xunit;

    public class MetadataWorkspaceExtensionsTests
    {
        [Fact]
        public void ToLegacyMetadataWorkspace_creates_equivalent_legacy_MetadataWorkspace_for_all_versions()
        {
            const string ssdlTemplate =
                @"<Schema Namespace=""NorthwindEF5Model.Store"" Provider=""System.Data.SqlClient"" ProviderManifestToken=""2008"" Alias=""Self"" xmlns=""{0}"" xmlns:store=""http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator"">"
                +
                @"  <EntityType Name=""Customers"">" +
                @"    <Key>" +
                @"      <PropertyRef Name=""CustomerID"" />" +
                @"    </Key>" +
                @"    <Property Name=""CustomerID"" Type=""nchar"" MaxLength=""5"" Nullable=""false"" />" +
                @"    <Property Name=""CompanyName"" Type=""nvarchar"" MaxLength=""40"" Nullable=""false"" />" +
                @"  </EntityType>" +
                @"  <EntityContainer Name=""Container"" />" +
                @"</Schema>";

            const string csdlTemplate =
                @"<Schema xmlns=""{0}"" Namespace=""dummy"">" +
                @"    <EntityContainer Name=""DummyContainer""/>" +
                @"</Schema>";

            const string mslTemplate =
                @"<Mapping Space=""C-S"" xmlns=""{0}"">" +
                @"  <EntityContainerMapping StorageEntityContainer=""Container"" CdmEntityContainer=""DummyContainer"" />" +
                @"</Mapping>";

            foreach (var version in EntityFrameworkVersion.GetAllVersions())
            {
                var storeItemCollection =
                    Utils.CreateStoreItemCollection(
                        string.Format(
                            ssdlTemplate,
                            SchemaManager.GetSSDLNamespaceName(version)));
                var edmItemCollection =
                    new EdmItemCollection(
                        new[]
                            {
                                XmlReader.Create(
                                    new StringReader(
                                        string.Format(
                                            csdlTemplate,
                                            SchemaManager.GetCSDLNamespaceName(version))))
                            });
                var mappingItemCollection =
                    new StorageMappingItemCollection(
                        edmItemCollection,
                        storeItemCollection,
                        new[]
                            {
                                XmlReader.Create(
                                    new StringReader(
                                        string.Format(
                                            mslTemplate,
                                            SchemaManager.GetMSLNamespaceName(version))))
                            });
                var workspace = new MetadataWorkspace(
                    () => edmItemCollection,
                    () => storeItemCollection,
                    () => mappingItemCollection);

                var legacyWorkspace = workspace.ToLegacyMetadataWorkspace();

                Assert.NotNull(legacyWorkspace);

                var legacyStoreItemCollection = legacyWorkspace.GetItemCollection(LegacyMetadata.DataSpace.SSpace);

                Assert.Equal(
                    storeItemCollection.GetItems<GlobalItem>().Count,
                    legacyStoreItemCollection.GetItems<LegacyMetadata.GlobalItem>().Count);

                Assert.NotNull(
                    legacyStoreItemCollection.GetItem<LegacyMetadata.EntityType>("NorthwindEF5Model.Store.Customers"));

                var legacyEdmItemCollection =
                    (LegacyMetadata.EdmItemCollection)legacyWorkspace.GetItemCollection(LegacyMetadata.DataSpace.CSpace);
                Assert.NotNull(legacyEdmItemCollection);
                Assert.Equal(version, EntityFrameworkVersion.DoubleToVersion(legacyEdmItemCollection.EdmVersion));

                Assert.NotNull(legacyWorkspace.GetItemCollection(LegacyMetadata.DataSpace.CSSpace));
            }
        }
    }
}
