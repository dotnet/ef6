// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Metadata
{
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using Xunit;

    public class MetadataSpatialTests : FunctionalTestBase
    {
        private static readonly string SpatialEntityPropertyCSDLTemplate = @"<?xml version=""1.0""?>
<Schema Namespace=""SpatialEntityPropertyTest"" {3} xmlns=""{0}"" xmlns:annotation=""http://schemas.microsoft.com/ado/2009/02/edm/annotation"">
    <EntityType Name=""EntityWithSpatialProperty"">
    <Key>
      <PropertyRef Name=""{1}"" />
    </Key>
    <Property Name=""ID"" Type=""Int32"" Nullable=""false"" />
    <Property Name=""Location"" Type=""{2}"" Nullable=""false"" />
    </EntityType>
</Schema>";

        private const string EdmNamespaceV1 = "http://schemas.microsoft.com/ado/2007/05/edm";
        private const string EdmNamespaceV2 = "http://schemas.microsoft.com/ado/2008/09/edm";
        private const string EdmNamespaceV3 = "http://schemas.microsoft.com/ado/2009/11/edm";
        private const string UseStrongSpatialAnnotation = @"annotation:UseStrongSpatialTypes=""false"" ";

        [Fact]
        public void Error_on_Geography_entity_property_in_csdl_version1()
        {
            Assert.True(
                Assert.Throws<MetadataException>(
                () => SpatialEntityPropertyTest(EdmNamespaceV1, PrimitiveTypeKind.Geography)).Message.Contains(Strings.NotNamespaceQualified("Geography")));
        }

        [Fact]
        public void Error_on_Geography_entity_property_in_csdl_version2()
        {
            Assert.True(
                Assert.Throws<MetadataException>(
                () => SpatialEntityPropertyTest(EdmNamespaceV2, PrimitiveTypeKind.Geography)).Message.Contains(Strings.NotNamespaceQualified("Geography")));
        }


        [Fact]
        public void Geography_entity_property_in_csdl_versio3n_works()
        {
            VerifySpatialEntityProperty(PrimitiveTypeKind.Geography);
        }

        [Fact]
        public void Error_on_Geography_EntityKey_property_in_csdl_version3()
        {
            Assert.True(
                Assert.Throws<MetadataException>(
                () => SpatialEntityPropertyTest(EdmNamespaceV3, PrimitiveTypeKind.Geography, "Location", UseStrongSpatialAnnotation)).Message.
                Contains(Strings.EntityKeyTypeCurrentlyNotSupported("Location", "SpatialEntityPropertyTest.EntityWithSpatialProperty", "Geography")));
        }

        [Fact]
        public void Error_on_Geography_without_StrongTypes_in_csdl_version3()
        {
            Assert.True(
                Assert.Throws<MetadataException>(
                () => SpatialEntityPropertyTest(EdmNamespaceV3, PrimitiveTypeKind.Geography, "ID", "")).Message.Contains(Strings.SpatialWithUseStrongSpatialTypesFalse));
        }

        [Fact]
        public void Error_on_Geography_with_TrueStrongTypes_in_csdl_version3()
        {
            Assert.True(
                Assert.Throws<MetadataException>(
                () => SpatialEntityPropertyTest(EdmNamespaceV3, PrimitiveTypeKind.Geography, "ID", @"annotation:UseStrongSpatialTypes=""true""")).Message.
                Contains(Strings.SpatialWithUseStrongSpatialTypesFalse));
        }

        [Fact]
        public void Error_on_Geography_with_invalid_StrongTypes_in_csdl_version3()
        {
            var exceptionMessage = Assert.Throws<MetadataException>(
                () => SpatialEntityPropertyTest(EdmNamespaceV3, PrimitiveTypeKind.Geography, "ID", @"annotation:UseStrongSpatialTypes=""Invalid""")).Message;
            
            Assert.True(exceptionMessage.Contains("http://schemas.microsoft.com/ado/2009/02/edm/annotation:UseStrongSpatialTypes"));
            Assert.True(exceptionMessage.Contains("Invalid"));
            Assert.True(exceptionMessage.Contains("http://www.w3.org/2001/XMLSchema:boolean"));
        }

        [Fact]
        public void Error_on_Geometry_entity_property_in_csdl_version1()
        {
            Assert.True(
                Assert.Throws<MetadataException>(
                () => SpatialEntityPropertyTest(EdmNamespaceV1, PrimitiveTypeKind.Geometry)).Message.Contains(Strings.NotNamespaceQualified("Geometry")));
        }

        [Fact]
        public void Error_on_Geometry_entity_property_in_csdl_version2()
        {
            Assert.True(
                Assert.Throws<MetadataException>(
                () => SpatialEntityPropertyTest(EdmNamespaceV2, PrimitiveTypeKind.Geometry)).Message.Contains(Strings.NotNamespaceQualified("Geometry")));
        }

        [Fact]
        public void Verify_Geometry_entity_property_in_csdl_version3_works()
        {
            VerifySpatialEntityProperty(PrimitiveTypeKind.Geometry);
        }

        [Fact]
        public void Error_on_Geometry_without_StrongTypes_in_csdl_version3()
        {
            Assert.True(
                Assert.Throws<MetadataException>(
                () => SpatialEntityPropertyTest(EdmNamespaceV3, PrimitiveTypeKind.Geometry, "ID", "")).Message.Contains(Strings.SpatialWithUseStrongSpatialTypesFalse));
        }

        [Fact]
        public void Error_on_Geometry_with_TrueStrongTypes_in_csdl_version3()
        {
            Assert.True(
                Assert.Throws<MetadataException>(
                () => SpatialEntityPropertyTest(EdmNamespaceV3, PrimitiveTypeKind.Geometry, "ID", @"annotation:UseStrongSpatialTypes=""true""")).Message.
                Contains(Strings.SpatialWithUseStrongSpatialTypesFalse));
        }

        [Fact]
        public void Error_on_Geometry_with_invalid_StrongTypes_in_csdl_version3()
        {
            Assert.True(
                Assert.Throws<MetadataException>(
                () => SpatialEntityPropertyTest(EdmNamespaceV3, PrimitiveTypeKind.Geometry, "ID", @"annotation:UseStrongSpatialTypes=""Invalid""")).Message.
                Contains("http://www.w3.org/2001/XMLSchema:boolean"));
        }

        [Fact]
        public void Error_on_Geometry_EntityKey_property_in_csdl__version3()
        {
            Assert.True(
                Assert.Throws<MetadataException>(
                () => SpatialEntityPropertyTest(EdmNamespaceV3, PrimitiveTypeKind.Geometry, "Location", UseStrongSpatialAnnotation)).Message.
                Contains(Strings.EntityKeyTypeCurrentlyNotSupported("Location","SpatialEntityPropertyTest.EntityWithSpatialProperty","Geometry")));
        }

        [Fact]
        public void Error_on_invalid_facet_for_Spatial()
        {
            var invalidCsdl =
@"<?xml version=""1.0"" ?>
        <Schema Namespace=""bug254266"" {1} xmlns=""{0}"" xmlns:annotation=""http://schemas.microsoft.com/ado/2009/02/edm/annotation"">
          <EntityType Name=""TestGeog"">
            <Key>
              <PropertyRef Name=""Id"" />
            </Key>
            <Property Name=""Id"" Type=""Int32"" Nullable=""false"" />
            <Property Name=""GeogData"" Type=""Geography"" ConcurrencyMode=""Fixed""/>
            <Property Name=""GeomData"" Type=""Geometry"" ConcurrencyMode=""Fixed""/>
          </EntityType>
        </Schema>";

            string csdlContent = string.Format(CultureInfo.InvariantCulture, invalidCsdl, EdmNamespaceV3, UseStrongSpatialAnnotation);
            XmlReader csdlReader = XmlReader.Create(new StringReader(csdlContent));

            var exceptionMessage = Assert.Throws<MetadataException>(() => new EdmItemCollection(new[] { csdlReader })).Message;
            
            Assert.True(exceptionMessage.Contains(Strings.FacetNotAllowed("ConcurrencyMode", "Edm.Geography")));
            Assert.True(exceptionMessage.Contains(Strings.FacetNotAllowed("ConcurrencyMode", "Edm.Geometry")));
        }

        private EdmItemCollection SpatialEntityPropertyTest(string namespaceVersion, PrimitiveTypeKind spatialType)
        {
            return SpatialEntityPropertyTest(namespaceVersion, spatialType, keyPropertyName: "ID", strongSpatialAnnotation: "");
        }

        private EdmItemCollection SpatialEntityPropertyTest(string namespaceVersion, PrimitiveTypeKind spatialType, string keyPropertyName, string strongSpatialAnnotation)
        {
            string typeName = (spatialType == PrimitiveTypeKind.Geography ? "Geography" : "Geometry");
            string csdlContent = string.Format(
                CultureInfo.InvariantCulture,
                SpatialEntityPropertyCSDLTemplate,
                namespaceVersion,
                keyPropertyName,
                typeName,
                strongSpatialAnnotation);

            var csdlReader = XmlReader.Create(new StringReader(csdlContent));
            var testCollection = new EdmItemCollection(new[] { csdlReader });

            return testCollection;
        }

        private void VerifySpatialEntityProperty(PrimitiveTypeKind spatialType)
        {
            EdmItemCollection items = SpatialEntityPropertyTest(EdmNamespaceV3, spatialType, "ID", UseStrongSpatialAnnotation);
            EntityType spatialEntityType = items.GetItem<EntityType>("SpatialEntityPropertyTest.EntityWithSpatialProperty", ignoreCase: false);
            EdmProperty spatialProperty = spatialEntityType.Properties["Location"];
            PrimitiveType primitivePropertyType = (PrimitiveType)spatialProperty.TypeUsage.EdmType;

            Assert.True(primitivePropertyType.PrimitiveTypeKind == spatialType, "Spatial Entity property was not correctly loaded for " + spatialType.ToString());
        }
    }
}
