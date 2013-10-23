// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.Metadata
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using Xunit;

    public class StoreItemCollectionExtensionsTests
    {
        private const string SsdlTemplate =
            @"<Schema Namespace=""Model.Store"" Provider=""System.Data.SqlClient"" " +
            @"    ProviderManifestToken=""2008"" Alias=""Self"" xmlns=""{0}"">" +
            @"    <EntityContainer Name=""TestContainer"" />" +
            @"</Schema>";

        private const string Ssdl =
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

        private static readonly string[] SchemaVersions =
            new[]
                {
                    "http://schemas.microsoft.com/ado/2006/04/edm/ssdl",
                    "http://schemas.microsoft.com/ado/2009/02/edm/ssdl",
                    "http://schemas.microsoft.com/ado/2009/11/edm/ssdl"
                };

        [Fact]
        public void Ssdl_versions_serialized_correctly()
        {
            foreach (var schemaVersion in SchemaVersions)
            {
                var sourceSsdl = string.Format(SsdlTemplate, schemaVersion);
                var serializdedSsdl = StoreItemCollectionToString(Utils.CreateStoreItemCollection(sourceSsdl), "Model.Store");
                Assert.True(XNode.DeepEquals(XDocument.Parse(sourceSsdl), XDocument.Parse(serializdedSsdl)));
            }
        }

        [Fact]
        public void EntityType_used_to_infere_schema_name()
        {
            var storeItemCollection = Utils.CreateStoreItemCollection(Ssdl);
            var serializedSsdl = XDocument.Parse(StoreItemCollectionToString(storeItemCollection));

            Assert.Equal("NorthwindEF5Model.Store", (string)serializedSsdl.Root.Attribute("Namespace"));
        }

        [Fact]
        public void Custom_schema_name_written_if_provided_and_no_entity_types_in_item_collection()
        {
            var storeItemCollection = Utils.CreateStoreItemCollection(string.Format(SsdlTemplate, SchemaVersions.Last()));
            var serializedSsdl = XDocument.Parse(StoreItemCollectionToString(storeItemCollection, "MyOwnSchema"));

            Assert.Equal("MyOwnSchema", (string)serializedSsdl.Root.Attribute("Namespace"));
        }

        [Fact]
        public void Schema_name_infered_from_EntityType_win_with_custom_schema_name()
        {
            var storeItemCollection = Utils.CreateStoreItemCollection(Ssdl);
            var serializedSsdl = XDocument.Parse(StoreItemCollectionToString(storeItemCollection, "MyOwnSchema"));

            Assert.Equal("NorthwindEF5Model.Store", (string)serializedSsdl.Root.Attribute("Namespace"));
        }

        [Fact]
        public void Container_name_used_to_create_schema_name_when_no_entity_types_present_and_schema_name_not_provided()
        {
            var storeItemCollection = Utils.CreateStoreItemCollection(string.Format(SsdlTemplate, SchemaVersions.Last()));
            var serializedSsdl = XDocument.Parse(StoreItemCollectionToString(storeItemCollection));

            Assert.Equal("Model.Store", (string)serializedSsdl.Root.Attribute("Namespace"));
        }

        [Fact]
        public void ToEdmModel_adds_container_and_types_to_EdmModel()
        {
            var edmModel = Utils.CreateStoreItemCollection(Ssdl).ToEdmModel();
            Assert.Equal("NorthwindEF5ModelStoreContainer", edmModel.Containers.Single().Name);
            Assert.Equal(2, edmModel.EntityTypes.Count());
            Assert.NotNull(edmModel.EntityTypes.SingleOrDefault(e => e.Name == "Customers"));
            Assert.NotNull(edmModel.EntityTypes.SingleOrDefault(e => e.Name == "Orders"));
            Assert.Equal(1, edmModel.AssociationTypes.Count());
            Assert.NotNull(edmModel.AssociationTypes.SingleOrDefault(a => a.Name == "FK_Orders_Customers"));
        }

        private static string StoreItemCollectionToString(StoreItemCollection storeItemCollection, string schemaNamespace = null)
        {
            var sb = new StringBuilder();

            using (var writer = XmlWriter.Create(sb))
            {
                storeItemCollection.WriteSsdl(writer, schemaNamespace);
            }

            return sb.ToString();
        }
    }
}
