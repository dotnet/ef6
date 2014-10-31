// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using Xunit;

    public class MetadataMappingHasherVisitorTests
    {
        #region XML
        const string ssdl1 =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Schema Namespace=""NorthwindModel.Store"" Provider=""System.Data.SqlClient"" ProviderManifestToken=""2008"" Alias=""Self"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm/ssdl"">
  <EntityType Name=""Customers"">
    <Key>
      <PropertyRef Name=""CustomerID"" />
    </Key>
    <Property Name=""CustomerID"" Type=""nchar"" MaxLength=""5"" Nullable=""false"" />
    <Property Name=""ContactName"" Type=""nvarchar"" MaxLength=""30"" Nullable=""true"" />
  </EntityType>
  <EntityType Name=""Orders"">
    <Key>
      <PropertyRef Name=""OrderID"" />
    </Key>
    <Property Name=""OrderID"" Type=""int"" StoreGeneratedPattern=""Identity"" Nullable=""false"" />
    <Property Name=""CustomerID"" Type=""nchar"" MaxLength=""5"" Nullable=""true"" />
  </EntityType>
  <Association Name=""FK_Orders_Customers"">
    <End Role=""Customers"" Type=""Self.Customers"" Multiplicity=""0..1"" />
    <End Role=""Orders"" Type=""Self.Orders"" Multiplicity=""*"" />
    <ReferentialConstraint>
      <Principal Role=""Customers"">
        <PropertyRef Name=""CustomerID"" />
      </Principal>
      <Dependent Role=""Orders"">
        <PropertyRef Name=""CustomerID"" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <EntityContainer Name=""NorthwindModelStoreContainer"">
    <EntitySet Name=""Customers"" EntityType=""Self.Customers"" Schema=""dbo"" p3:Type=""Tables"" xmlns:p3=""http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator"" />
    <EntitySet Name=""Orders"" EntityType=""Self.Orders"" Schema=""dbo"" p3:Type=""Tables"" xmlns:p3=""http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator"" />
    <AssociationSet Name=""FK_Orders_Customers"" Association=""Self.FK_Orders_Customers"">
      <End Role=""Customers"" EntitySet=""Customers"" />
      <End Role=""Orders"" EntitySet=""Orders"" />
    </AssociationSet>
  </EntityContainer>
</Schema>";

        const string ssdl2 =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Schema Namespace=""NorthwindModel.Store"" Provider=""System.Data.SqlClient"" ProviderManifestToken=""2008"" Alias=""Self"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm/ssdl"">
  <EntityType Name=""Orders"">
    <Key>
      <PropertyRef Name=""OrderID"" />
    </Key>
    <Property Name=""CustomerID"" Type=""nchar"" MaxLength=""5"" Nullable=""true"" />
    <Property Name=""OrderID"" Type=""int"" StoreGeneratedPattern=""Identity"" Nullable=""false"" />
  </EntityType>
  <EntityType Name=""Customers"">
    <Key>
      <PropertyRef Name=""CustomerID"" />
    </Key>
    <Property Name=""ContactName"" Type=""nvarchar"" MaxLength=""30"" Nullable=""true"" />
    <Property Name=""CustomerID"" Type=""nchar"" MaxLength=""5"" Nullable=""false"" />
  </EntityType>
  <Association Name=""FK_Orders_Customers"">
    <End Role=""Orders"" Type=""Self.Orders"" Multiplicity=""*"" />
    <End Role=""Customers"" Type=""Self.Customers"" Multiplicity=""0..1"" />
    <ReferentialConstraint>
      <Principal Role=""Customers"">
        <PropertyRef Name=""CustomerID"" />
      </Principal>
      <Dependent Role=""Orders"">
        <PropertyRef Name=""CustomerID"" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <EntityContainer Name=""NorthwindModelStoreContainer"">
    <EntitySet Name=""Orders"" EntityType=""Self.Orders"" Schema=""dbo"" p3:Type=""Tables"" xmlns:p3=""http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator"" />
    <EntitySet Name=""Customers"" EntityType=""Self.Customers"" Schema=""dbo"" p3:Type=""Tables"" xmlns:p3=""http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator"" />
    <AssociationSet Name=""FK_Orders_Customers"" Association=""Self.FK_Orders_Customers"">
      <End Role=""Orders"" EntitySet=""Orders"" />
      <End Role=""Customers"" EntitySet=""Customers"" />
    </AssociationSet>
  </EntityContainer>
</Schema>";

        const string csdl1 =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Schema Namespace=""NorthwindModel"" Alias=""Self"" annotation:UseStrongSpatialTypes=""false"" xmlns:annotation=""http://schemas.microsoft.com/ado/2009/02/edm/annotation"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
  <EntityType Name=""Customer"">
    <Key>
      <PropertyRef Name=""CustomerID"" />
    </Key>
    <Property Name=""CustomerID"" Type=""String"" MaxLength=""5"" FixedLength=""true"" Unicode=""true"" Nullable=""false"" />
    <Property Name=""ContactName"" Type=""String"" MaxLength=""30"" FixedLength=""false"" Unicode=""true"" />
    <NavigationProperty Name=""Orders"" Relationship=""Self.FK_Orders_Customers"" FromRole=""Customers"" ToRole=""Orders"" />
  </EntityType>
  <EntityType Name=""Order"">
    <Key>
      <PropertyRef Name=""OrderID"" />
    </Key>
    <Property Name=""OrderID"" Type=""Int32"" Nullable=""false"" annotation:StoreGeneratedPattern=""Identity"" />
    <Property Name=""CustomerID"" Type=""String"" MaxLength=""5"" FixedLength=""true"" Unicode=""true"" />
    <NavigationProperty Name=""Customer"" Relationship=""Self.FK_Orders_Customers"" FromRole=""Orders"" ToRole=""Customers"" />
  </EntityType>
  <Association Name=""FK_Orders_Customers"">
    <End Role=""Customers"" Type=""Self.Customer"" Multiplicity=""0..1"" />
    <End Role=""Orders"" Type=""Self.Order"" Multiplicity=""*"" />
    <ReferentialConstraint>
      <Principal Role=""Customers"">
        <PropertyRef Name=""CustomerID"" />
      </Principal>
      <Dependent Role=""Orders"">
        <PropertyRef Name=""CustomerID"" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <EntityContainer Name=""NorthwindEntities"" annotation:LazyLoadingEnabled=""true"">
    <EntitySet Name=""Customers"" EntityType=""Self.Customer"" />
    <EntitySet Name=""Orders"" EntityType=""Self.Order"" />
    <AssociationSet Name=""FK_Orders_Customers"" Association=""Self.FK_Orders_Customers"">
      <End Role=""Customers"" EntitySet=""Customers"" />
      <End Role=""Orders"" EntitySet=""Orders"" />
    </AssociationSet>
  </EntityContainer>
</Schema>";

        const string csdl2 =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Schema Namespace=""NorthwindModel"" Alias=""Self"" annotation:UseStrongSpatialTypes=""false"" xmlns:annotation=""http://schemas.microsoft.com/ado/2009/02/edm/annotation"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
  <EntityType Name=""Order"">
    <Key>
      <PropertyRef Name=""OrderID"" />
    </Key>
    <Property Name=""CustomerID"" Type=""String"" MaxLength=""5"" FixedLength=""true"" Unicode=""true"" />
    <Property Name=""OrderID"" Type=""Int32"" Nullable=""false"" annotation:StoreGeneratedPattern=""Identity"" />
    <NavigationProperty Name=""Customer"" Relationship=""Self.FK_Orders_Customers"" FromRole=""Orders"" ToRole=""Customers"" />
  </EntityType>
  <EntityType Name=""Customer"">
    <Key>
      <PropertyRef Name=""CustomerID"" />
    </Key>
    <Property Name=""ContactName"" Type=""String"" MaxLength=""30"" FixedLength=""false"" Unicode=""true"" />
    <Property Name=""CustomerID"" Type=""String"" MaxLength=""5"" FixedLength=""true"" Unicode=""true"" Nullable=""false"" />
    <NavigationProperty Name=""Orders"" Relationship=""Self.FK_Orders_Customers"" FromRole=""Customers"" ToRole=""Orders"" />
  </EntityType>
  <Association Name=""FK_Orders_Customers"">
    <End Role=""Orders"" Type=""Self.Order"" Multiplicity=""*"" />
    <End Role=""Customers"" Type=""Self.Customer"" Multiplicity=""0..1"" />
    <ReferentialConstraint>
      <Principal Role=""Customers"">
        <PropertyRef Name=""CustomerID"" />
      </Principal>
      <Dependent Role=""Orders"">
        <PropertyRef Name=""CustomerID"" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <EntityContainer Name=""NorthwindEntities"" annotation:LazyLoadingEnabled=""true"">
    <EntitySet Name=""Orders"" EntityType=""Self.Order"" />
    <EntitySet Name=""Customers"" EntityType=""Self.Customer"" />
    <AssociationSet Name=""FK_Orders_Customers"" Association=""Self.FK_Orders_Customers"">
      <End Role=""Orders"" EntitySet=""Orders"" />
      <End Role=""Customers"" EntitySet=""Customers"" />
    </AssociationSet>
  </EntityContainer>
</Schema>";

        const string msl1 =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Mapping Space=""C-S"" xmlns=""http://schemas.microsoft.com/ado/2009/11/mapping/cs"">
  <EntityContainerMapping StorageEntityContainer=""NorthwindModelStoreContainer"" CdmEntityContainer=""NorthwindEntities"">
    <EntitySetMapping Name=""Customers"">
      <EntityTypeMapping TypeName=""NorthwindModel.Customer"">
        <MappingFragment StoreEntitySet=""Customers"">
          <ScalarProperty Name=""CustomerID"" ColumnName=""CustomerID"" />
          <ScalarProperty Name=""ContactName"" ColumnName=""ContactName"" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name=""Orders"">
      <EntityTypeMapping TypeName=""NorthwindModel.Order"">
        <MappingFragment StoreEntitySet=""Orders"">
          <ScalarProperty Name=""OrderID"" ColumnName=""OrderID"" />
          <ScalarProperty Name=""CustomerID"" ColumnName=""CustomerID"" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
  </EntityContainerMapping>
</Mapping>";

        const string msl2 =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Mapping Space=""C-S"" xmlns=""http://schemas.microsoft.com/ado/2009/11/mapping/cs"">
  <EntityContainerMapping StorageEntityContainer=""NorthwindModelStoreContainer"" CdmEntityContainer=""NorthwindEntities"">
    <EntitySetMapping Name=""Orders"">
      <EntityTypeMapping TypeName=""NorthwindModel.Order"">
        <MappingFragment StoreEntitySet=""Orders"">
          <ScalarProperty Name=""CustomerID"" ColumnName=""CustomerID"" />
          <ScalarProperty Name=""OrderID"" ColumnName=""OrderID"" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name=""Customers"">
      <EntityTypeMapping TypeName=""NorthwindModel.Customer"">
        <MappingFragment StoreEntitySet=""Customers"">
          <ScalarProperty Name=""ContactName"" ColumnName=""ContactName"" />
          <ScalarProperty Name=""CustomerID"" ColumnName=""CustomerID"" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
  </EntityContainerMapping>
</Mapping>";
        #endregion

        [Fact]
        public static void GetMappingClosureHash_is_impacted_by_the_order_of_elements_if_sortSequence_is_false()
        {
            var hash1 = GetMappingClosureHash(ssdl1, csdl1, msl1, sortSequence: false);
            var hash2 = GetMappingClosureHash(ssdl2, csdl1, msl1, sortSequence: false);
            Assert.NotEqual(hash1, hash2);

            hash1 = GetMappingClosureHash(ssdl1, csdl1, msl1, sortSequence: false);
            hash2 = GetMappingClosureHash(ssdl1, csdl2, msl1, sortSequence: false);
            Assert.NotEqual(hash1, hash2);

            hash1 = GetMappingClosureHash(ssdl1, csdl1, msl1, sortSequence: false);
            hash2 = GetMappingClosureHash(ssdl1, csdl1, msl2, sortSequence: false);
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public static void GetMappingClosureHash_is_not_impacted_by_the_order_of_elements_if_sortSequence_is_true()
        {
            var hash1 = GetMappingClosureHash(ssdl1, csdl1, msl1, sortSequence: true);
            var hash2 = GetMappingClosureHash(ssdl2, csdl2, msl2, sortSequence: true);
            Assert.Equal(hash1, hash2);
        }

        private static string GetMappingClosureHash(string ssdl, string csdl, string msl, bool sortSequence)
        {
            var mappingCollection = 
                StorageMappingItemCollectionTests.CreateStorageMappingItemCollection(ssdl, csdl, msl);

            return MetadataMappingHasherVisitor.GetMappingClosureHash(
                3.0, mappingCollection.GetItems<EntityContainerMapping>().Single(), sortSequence);
        }

        [Fact]
        public static void GetIdentity_of_StorageSetMapping_returns_expected_value()
        {
            var entityType = new EntityType("ET", "N", DataSpace.CSpace);
            var entitySet = new EntitySet("ES", "S", "T", null, entityType);
            var associationType = new AssociationType("AT", "N", false, DataSpace.CSpace);
            var associationSet = new AssociationSet("AS", associationType);
            var entitySetMapping = new EntitySetMapping(entitySet, null);
            var associationSetMapping = new AssociationSetMapping(associationSet, entitySet);

            Assert.Equal(entitySet.Identity, 
                BaseMetadataMappingVisitor.IdentityHelper.GetIdentity(entitySetMapping));
            Assert.Equal(associationSet.Identity, 
                BaseMetadataMappingVisitor.IdentityHelper.GetIdentity(associationSetMapping));
        }

        [Fact]
        public static void GetIdentity_of_StorageEntityTypeMapping_returns_expected_value()
        {
            var entityType1 = new EntityType("ET1", "N", DataSpace.CSpace);
            var entityType2 = new EntityType("ET2", "N", DataSpace.CSpace);
            var entityType3 = new EntityType("ET3", "N", DataSpace.CSpace);
            var entityType4 = new EntityType("ET4", "N", DataSpace.CSpace);
            var mapping = new EntityTypeMapping(null);
            mapping.AddType(entityType2);
            mapping.AddType(entityType1);
            mapping.AddIsOfType(entityType4);
            mapping.AddIsOfType(entityType3);

            Assert.Equal("N.ET1,N.ET2,N.ET3,N.ET4",
                BaseMetadataMappingVisitor.IdentityHelper.GetIdentity((TypeMapping)mapping));
        }

        [Fact]
        public static void GetIdentity_of_StorageAssociationTypeMapping_returns_expected_value()
        {
            var associationType = new AssociationType("AT", "N", false, DataSpace.CSpace);
            TypeMapping mapping = new AssociationTypeMapping(associationType, null);

            Assert.Equal(associationType.Identity, BaseMetadataMappingVisitor.IdentityHelper.GetIdentity(mapping));
        }

        [Fact]
        public static void GetIdentity_of_StorageComplexTypeMapping_returns_expected_value()
        {
            var complexType1 = new ComplexType("CT1", "N", DataSpace.CSpace);
            var complexType2 = new ComplexType("CT2", "N", DataSpace.CSpace);
            var complexType3 = new ComplexType("CT3", "N", DataSpace.CSpace);
            var complexType4 = new ComplexType("CT4", "N", DataSpace.CSpace);
            var property1 = new EdmProperty("A", TypeUsage.Create(complexType1));
            var property2 = new EdmProperty("B", TypeUsage.Create(complexType2));
            var propertyMapping1 = new ComplexPropertyMapping(property1);
            var propertyMapping2 = new ComplexPropertyMapping(property2);

            var mapping = new ComplexTypeMapping(false);
            mapping.AddType(complexType2);
            mapping.AddType(complexType1);
            mapping.AddIsOfType(complexType4);
            mapping.AddIsOfType(complexType3);
            mapping.AddPropertyMapping(propertyMapping2);
            mapping.AddPropertyMapping(propertyMapping1);

            Assert.Equal("ComplexProperty(Identity=A),ComplexProperty(Identity=B),N.CT1,N.CT2,N.CT3,N.CT4",
                BaseMetadataMappingVisitor.IdentityHelper.GetIdentity(mapping));
        }

        [Fact]
        public static void GetIdentity_of_StorageMappingFragment_returns_expected_value()
        {
            var entityType = new EntityType("ET", "N", DataSpace.CSpace);
            var entitySet = new EntitySet("ES", "S", "T", null, entityType);
            var entityTypeMapping = new EntityTypeMapping(null);
            entityTypeMapping.AddType(entityType);
            var mappingFragment = new MappingFragment(entitySet, entityTypeMapping, false);

            Assert.Equal(entitySet.Identity,
                BaseMetadataMappingVisitor.IdentityHelper.GetIdentity(mappingFragment));
        }

        [Fact]
        public static void GetIdentity_of_StorageScalarPropertyMapping_returns_expected_value()
        {
            var typeUsage = TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var property1 = new EdmProperty("A", typeUsage);
            var property2 = new EdmProperty("B", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace }));
            PropertyMapping mapping = new ScalarPropertyMapping(property1, property2);

            Assert.Equal("ScalarProperty(Identity=A,ColumnIdentity=B)", 
                BaseMetadataMappingVisitor.IdentityHelper.GetIdentity(mapping));
        }

        [Fact]
        public static void GetIdentity_of_StorageComplexPropertyMapping_returns_expected_value()
        {
            var complexType = new ComplexType("CT", "N", DataSpace.CSpace);
            var property = new EdmProperty("A", TypeUsage.Create(complexType));
            PropertyMapping mapping = new ComplexPropertyMapping(property);

            Assert.Equal("ComplexProperty(Identity=A)", 
                BaseMetadataMappingVisitor.IdentityHelper.GetIdentity(mapping));
        }

        [Fact]
        public static void GetIdentity_of_StorageEndPropertyMapping_returns_expected_value()
        {
            var entityType = new EntityType("ET", "N", DataSpace.CSpace);
            PropertyMapping mapping = new EndPropertyMapping()
            {
                AssociationEnd = new AssociationEndMember("AEM", entityType)
            };

            Assert.Equal("EndProperty(Identity=AEM)",
                BaseMetadataMappingVisitor.IdentityHelper.GetIdentity(mapping));
        }

        [Fact]
        public static void GetIdentity_of_StorageConditionPropertyMapping_returns_expected_value()
        {
            var typeUsage = TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var property1 = new EdmProperty("A", typeUsage);
            var property2 = new EdmProperty("B", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace }));

            PropertyMapping mapping = new ValueConditionMapping(property1, "V");

            Assert.Equal("ConditionProperty(Identity=A)",
                BaseMetadataMappingVisitor.IdentityHelper.GetIdentity(mapping));

            mapping = new ValueConditionMapping(property2, "V");

            Assert.Equal("ConditionProperty(ColumnIdentity=B)",
                BaseMetadataMappingVisitor.IdentityHelper.GetIdentity(mapping));
        }
    }
}
