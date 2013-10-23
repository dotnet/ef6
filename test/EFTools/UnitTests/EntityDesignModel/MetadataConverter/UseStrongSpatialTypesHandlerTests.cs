// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System.Globalization;
    using System.Xml;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Xunit;

    public class UseStrongSpatialTypesHandlerTests
    {
        private const string EdmxTemplate = @"
<edmx:Edmx Version=""{0}"" xmlns:edmx=""{1}"">
  <edmx:Runtime>
    <edmx:StorageModels>
      <Schema Namespace=""Model1.Store"" Alias=""Self"" Provider=""System.Data.SqlClient"" ProviderManifestToken=""2005"" xmlns=""{2}"">
        <EntityContainer Name=""Model1TargetContainer"">
        </EntityContainer>
      </Schema>
    </edmx:StorageModels>
    <!-- CSDL content -->
    <edmx:ConceptualModels>
      <Schema Namespace=""Model1"" Alias=""Self"" xmlns=""{3}"" {4}>
        <EntityContainer Name=""Model1Container"">
        </EntityContainer>
      </Schema>
    </edmx:ConceptualModels>
    <!-- C-S mapping content -->
    <edmx:Mappings>
      <Mapping Space=""C-S"" xmlns=""{5}"">
        <Alias Key=""Model"" Value=""Model1"" />
        <Alias Key=""Target"" Value=""Model1.Store"" />
        <EntityContainerMapping CdmEntityContainer=""Model1Container"" StorageEntityContainer=""Model1TargetContainer"">
        </EntityContainerMapping>
      </Mapping>
    </edmx:Mappings>
  </edmx:Runtime>
</edmx:Edmx>";

        private const string UseStrongSpatialTypesFragment =
            @"annotation:UseStrongSpatialTypes=""false"" xmlns:annotation=""http://schemas.microsoft.com/ado/2009/02/edm/annotation""";

        private readonly string V1EdmxWithUseStrongSpatialTypes =
            string.Format(
                CultureInfo.InvariantCulture,
                EdmxTemplate,
                new[]
                    {
                        "1.0",
                        SchemaManager.GetEDMXNamespaceName(EntityFrameworkVersion.Version1),
                        SchemaManager.GetSSDLNamespaceName(EntityFrameworkVersion.Version1),
                        SchemaManager.GetCSDLNamespaceName(EntityFrameworkVersion.Version1),
                        UseStrongSpatialTypesFragment,
                        SchemaManager.GetMSLNamespaceName(EntityFrameworkVersion.Version1)
                    });

        private readonly string V2EdmxWithUseStrongSpatialTypes =
            string.Format(
                CultureInfo.InvariantCulture,
                EdmxTemplate,
                new[]
                    {
                        "2.0",
                        SchemaManager.GetEDMXNamespaceName(EntityFrameworkVersion.Version2),
                        SchemaManager.GetSSDLNamespaceName(EntityFrameworkVersion.Version2),
                        SchemaManager.GetCSDLNamespaceName(EntityFrameworkVersion.Version2),
                        UseStrongSpatialTypesFragment,
                        SchemaManager.GetMSLNamespaceName(EntityFrameworkVersion.Version2)
                    });

        private readonly string V3EdmxWithoutUseStrongSpatialTypes =
            string.Format(
                CultureInfo.InvariantCulture,
                EdmxTemplate,
                new[]
                    {
                        "3.0",
                        SchemaManager.GetEDMXNamespaceName(EntityFrameworkVersion.Version3),
                        SchemaManager.GetSSDLNamespaceName(EntityFrameworkVersion.Version3),
                        SchemaManager.GetCSDLNamespaceName(EntityFrameworkVersion.Version3),
                        string.Empty,
                        SchemaManager.GetMSLNamespaceName(EntityFrameworkVersion.Version3)
                    });

        [Fact]
        public void HandleConversion_Targeting_EntityFramework_V1_Removes_UseStrongSpatialTypes_Attribute()
        {
            var inputDoc = LoadEdmx(V1EdmxWithUseStrongSpatialTypes);
            var handler = new UseStrongSpatialTypesHandler(EntityFrameworkVersion.Version1);
            var resultDoc = handler.HandleConversion(inputDoc);
            var nsmgr = SchemaManager.GetEdmxNamespaceManager(resultDoc.NameTable, EntityFrameworkVersion.Version1);
            nsmgr.AddNamespace("annotation", SchemaManager.GetAnnotationNamespaceName());
            var useStrongSpatialTypeAttr =
                (XmlAttribute)
                resultDoc.SelectSingleNode(
                    "/edmx:Edmx/edmx:Runtime/edmx:ConceptualModels/csdl:Schema/@annotation:UseStrongSpatialTypes", nsmgr);
            Assert.Null(useStrongSpatialTypeAttr);
        }

        [Fact]
        public void HandleConversion_Targeting_EntityFramework_V2_Removes_UseStrongSpatialTypes_Attribute()
        {
            var inputDoc = LoadEdmx(V2EdmxWithUseStrongSpatialTypes);
            var handler = new UseStrongSpatialTypesHandler(EntityFrameworkVersion.Version2);
            var resultDoc = handler.HandleConversion(inputDoc);
            var nsmgr = SchemaManager.GetEdmxNamespaceManager(resultDoc.NameTable, EntityFrameworkVersion.Version2);
            nsmgr.AddNamespace("annotation", SchemaManager.GetAnnotationNamespaceName());
            var useStrongSpatialTypeAttr =
                (XmlAttribute)
                resultDoc.SelectSingleNode(
                    "/edmx:Edmx/edmx:Runtime/edmx:ConceptualModels/csdl:Schema/@annotation:UseStrongSpatialTypes", nsmgr);
            Assert.Null(useStrongSpatialTypeAttr);
        }

        [Fact]
        public void HandleConversion_Targeting_EntityFramework_V3_Inserts_UseStrongSpatialTypes_Attribute()
        {
            var inputDoc = LoadEdmx(V3EdmxWithoutUseStrongSpatialTypes);
            var handler = new UseStrongSpatialTypesHandler(EntityFrameworkVersion.Version3);
            var resultDoc = handler.HandleConversion(inputDoc);
            var nsmgr = SchemaManager.GetEdmxNamespaceManager(resultDoc.NameTable, EntityFrameworkVersion.Version3);
            nsmgr.AddNamespace("annotation", SchemaManager.GetAnnotationNamespaceName());

            var schemaElement = (XmlElement)resultDoc.SelectSingleNode("/edmx:Edmx/edmx:Runtime/edmx:ConceptualModels/csdl:Schema", nsmgr);
            Assert.NotNull(schemaElement.Attributes["annotation", "http://www.w3.org/2000/xmlns/"]);
            var useStrongSpatialTypeAttr =
                (XmlAttribute)
                resultDoc.SelectSingleNode(
                    "/edmx:Edmx/edmx:Runtime/edmx:ConceptualModels/csdl:Schema/@annotation:UseStrongSpatialTypes", nsmgr);
            Assert.Equal("false", useStrongSpatialTypeAttr.Value);
        }

        private static XmlDocument LoadEdmx(string edmx)
        {
            var doc = new XmlDocument();
            doc.LoadXml(edmx);
            return doc;
        }
    }
}
