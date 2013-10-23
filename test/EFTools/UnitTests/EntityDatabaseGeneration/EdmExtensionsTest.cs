// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Legacy = System.Data.Common;

namespace Microsoft.Data.Entity.Design.DatabaseGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.DatabaseGeneration.OutputGenerators;
    using Microsoft.Data.Entity.Design.DatabaseGeneration.Properties;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper;
    using Moq;
    using Xunit;

    public class EdmExtensionTests
    {
        private const string Ssdl =
            "<Schema Namespace='AdventureWorksModel.Store' Provider='System.Data.SqlClient' ProviderManifestToken='2008' xmlns='http://schemas.microsoft.com/ado/2009/11/edm/ssdl'>"
            +
            "  <EntityContainer Name='AdventureWorksModelStoreContainer'>" +
            "    <EntitySet Name='Entities' EntityType='AdventureWorksModel.Store.Entities' Schema='dbo' />" +
            "  </EntityContainer>" +
            "  <EntityType Name='Entities'>" +
            "    <Key>" +
            "      <PropertyRef Name='Id' />" +
            "    </Key>" +
            "    <Property Name='Id' Type='int' StoreGeneratedPattern='Identity' Nullable='false' />" +
            "    <Property Name='Name' Type='nvarchar(max)' Nullable='false' />" +
            "  </EntityType>" +
            "</Schema>";

        private const string Csdl =
            "<Schema Namespace='AdventureWorksModel' Alias='Self' p1:UseStrongSpatialTypes='false' xmlns:annotation='http://schemas.microsoft.com/ado/2009/02/edm/annotation' xmlns:p1='http://schemas.microsoft.com/ado/2009/02/edm/annotation' xmlns='http://schemas.microsoft.com/ado/2009/11/edm'>"
            +
            "   <EntityContainer Name='AdventureWorksEntities3' p1:LazyLoadingEnabled='true' >" +
            "       <EntitySet Name='Entities' EntityType='AdventureWorksModel.Entity' />" +
            "   </EntityContainer>" +
            "   <EntityType Name='Entity'>" +
            "       <Key>" +
            "           <PropertyRef Name='Id' />" +
            "       </Key>" +
            "       <Property Type='Int32' Name='Id' Nullable='false' annotation:StoreGeneratedPattern='Identity' />" +
            "       <Property Type='String' Name='Name' Nullable='false' />" +
            "   </EntityType>" +
            "</Schema>";

        private const string Msl =
            "<Mapping Space='C-S' xmlns='http://schemas.microsoft.com/ado/2009/11/mapping/cs'>" +
            "  <EntityContainerMapping StorageEntityContainer='AdventureWorksModelStoreContainer' CdmEntityContainer='AdventureWorksEntities3'>"
            +
            "    <EntitySetMapping Name='Entities'>" +
            "      <EntityTypeMapping TypeName='IsTypeOf(AdventureWorksModel.Entity)'>" +
            "        <MappingFragment StoreEntitySet='Entities'>" +
            "          <ScalarProperty Name='Id' ColumnName='Id' />" +
            "          <ScalarProperty Name='Name' ColumnName='Name' />" +
            "        </MappingFragment>" +
            "      </EntityTypeMapping>" +
            "    </EntitySetMapping>" +
            "  </EntityContainerMapping>" +
            "</Mapping>";

        private readonly IDbDependencyResolver resolver;

        public EdmExtensionTests()
        {
            var providerServices =
                new LegacyDbProviderServicesWrapper(
                    ((Legacy.DbProviderServices)
                     ((IServiceProvider)Legacy.DbProviderFactories.GetFactory("System.Data.SqlClient"))
                         .GetService(typeof(Legacy.DbProviderServices))));

            var mockResolver = new Mock<IDbDependencyResolver>();
            mockResolver.Setup(
                r => r.GetService(
                    It.Is<Type>(t => t == typeof(DbProviderServices)),
                    It.IsAny<string>())).Returns(providerServices);

            resolver = mockResolver.Object;
        }

        [Fact]
        public void CreateAndValidateEdmItemCollection_throws_ArgumentNullException_for_null_csdl()
        {
            Assert.Equal(
                "csdl",
                Assert.Throws<ArgumentNullException>(
                    () => EdmExtension.CreateAndValidateEdmItemCollection(null, new Version(1, 0, 0, 0))).ParamName);
        }

        [Fact]
        public void CreateAndValidateEdmItemCollection_throws_ArgumentNullException_for_null_targetFrameworkVersion()
        {
            Assert.Equal(
                "targetFrameworkVersion",
                Assert.Throws<ArgumentNullException>(
                    () => EdmExtension.CreateAndValidateEdmItemCollection(string.Empty, null)).ParamName);
        }

        [Fact]
        public void CreateAndValidateEdmItemCollection_throws_ArgumentException_for_incorrect_targetFrameworkVersion()
        {
            var exception = Assert.Throws<ArgumentException>(
                () => EdmExtension.CreateAndValidateEdmItemCollection(string.Empty, new Version(0, 0)));

            Assert.Equal("targetFrameworkVersion", exception.ParamName);
            Assert.True(
                exception.Message.StartsWith(
                    string.Format(CultureInfo.CurrentCulture, Resources.ErrorNonValidTargetVersion, "0.0")));
        }

        [Fact]
        public void CreateAndValidateEdmItemCollection_throws_for_invalid_csdl()
        {
            var invalidCsdl = XDocument.Parse(Csdl);
            invalidCsdl.Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}PropertyRef").Remove();
            invalidCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}EntityType")
                .Single()
                .Add(new XElement("{http://schemas.microsoft.com/ado/2009/11/edm}InvalidElement"));

            var exception = Assert.Throws<InvalidOperationException>(
                () => EdmExtension.CreateAndValidateEdmItemCollection(invalidCsdl.ToString(), new Version(3, 0, 0, 0)));

            Assert.True(exception.Message.StartsWith(Resources.ErrorCsdlNotValid.Replace("{0}", string.Empty)));
            var errorMessages = exception.Message.Split('\n');
            Assert.Equal(3, errorMessages.Length);
            Assert.Contains("'PropertyRef'", errorMessages[0]);
            Assert.Contains("'InvalidElement'", errorMessages[1]);
            Assert.Contains("InvalidElement", errorMessages[2]);
        }

        [Fact]
        public void CreateAndValidateEdmItemCollection_throws_for_valid_csdl_whose_version_is_newer_than_targetFrameworkVersion()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => EdmExtension.CreateAndValidateEdmItemCollection(Csdl, new Version(2, 0, 0, 0)));

            Assert.Contains(
                exception.Message,
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.TargetVersionSchemaVersionMismatch,
                    new Version(3, 0, 0, 0),
                    new Version(2, 0, 0, 0)));
        }

        [Fact]
        public void CreateAndValidateEdmItemCollection_throws_original_erros_even_if_targetFrameworkVersion_is_older_than_csdl_version()
        {
            var invalidCsdl = XDocument.Parse(Csdl);
            invalidCsdl.Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}PropertyRef").Remove();
            invalidCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}EntityType")
                .Single()
                .Add(new XElement("{http://schemas.microsoft.com/ado/2009/11/edm}InvalidElement"));

            var exception = Assert.Throws<InvalidOperationException>(
                () => EdmExtension.CreateAndValidateEdmItemCollection(invalidCsdl.ToString(), new Version(2, 0, 0, 0)));

            Assert.True(exception.Message.StartsWith(Resources.ErrorCsdlNotValid.Replace("{0}", string.Empty)));
            var errorMessages = exception.Message.Split('\n');
            Assert.Equal(3, errorMessages.Length);
            Assert.Contains("'PropertyRef'", errorMessages[0]);
            Assert.Contains("'InvalidElement'", errorMessages[1]);
            Assert.Contains("InvalidElement", errorMessages[2]);
        }

        [Fact]
        public void CreateAndValidateEdmItemCollection_creates_EdmItemCollection_for_valid_csdl_and_targetFrameworkVersion()
        {
            var edmItemCollection = EdmExtension.CreateAndValidateEdmItemCollection(Csdl, new Version(3, 0, 0, 0));

            Assert.NotNull(edmItemCollection);
            Assert.NotNull(edmItemCollection.GetItem<EntityType>("AdventureWorksModel.Entity"));
        }

        [Fact]
        public void CreateStoreItemCollection_throws_ArgumentNullException_for_null_ssdl()
        {
            IList<EdmSchemaError> schemaErrors;
            Assert.Equal(
                "ssdl",
                Assert.Throws<ArgumentNullException>(
                    () => EdmExtension.CreateStoreItemCollection(
                        null,
                        new Version(1, 0, 0, 0),
                        null,
                        out schemaErrors)).ParamName);
        }

        [Fact]
        public void CreateStoreItemCollection_throws_ArgumentNullException_for_null_targetFrameworkVersion()
        {
            IList<EdmSchemaError> schemaErrors;

            Assert.Equal(
                "targetFrameworkVersion",
                Assert.Throws<ArgumentNullException>(
                    () => EdmExtension.CreateStoreItemCollection(
                        string.Empty,
                        null,
                        null,
                        out schemaErrors)).ParamName);
        }

        [Fact]
        public void CreateStoreItemCollection_throws_ArgumentException_for_incorrect_targetFrameworkVersion()
        {
            IList<EdmSchemaError> schemaErrors;

            var exception = Assert.Throws<ArgumentException>(
                () => EdmExtension.CreateStoreItemCollection(
                    string.Empty,
                    new Version(0, 0),
                    null,
                    out schemaErrors));

            Assert.Equal("targetFrameworkVersion", exception.ParamName);
            Assert.True(
                exception.Message.StartsWith(
                    string.Format(CultureInfo.CurrentCulture, Resources.ErrorNonValidTargetVersion, "0.0")));
        }

        [Fact]
        public void CreateStoreItemCollection_returns_errors_StoreItemCollections_for_invalid_ssdl()
        {
            var invalidSsdl = XDocument.Parse(Ssdl);
            invalidSsdl.Descendants("{http://schemas.microsoft.com/ado/2009/11/edm/ssdl}" + "PropertyRef").Remove();

            IList<EdmSchemaError> schemaErrors;
            var storeItemCollection = EdmExtension.CreateStoreItemCollection(
                invalidSsdl.ToString(),
                new Version(3, 0, 0, 0),
                resolver,
                out schemaErrors);

            Assert.Null(storeItemCollection);
            Assert.Equal(1, schemaErrors.Count);
            Assert.Contains("'PropertyRef'", schemaErrors[0].Message);
        }

        [Fact]
        public void CreateStoreItemCollection_creates_StoreItemCollection_for_valid_ssdl_and_targetFrameworkVersion()
        {
            IList<EdmSchemaError> schemaErrors;
            var storeItemCollection =
                EdmExtension.CreateStoreItemCollection(
                    Ssdl,
                    new Version(3, 0, 0, 0),
                    resolver,
                    out schemaErrors);

            Assert.NotNull(storeItemCollection);
            Assert.Equal(0, schemaErrors.Count);
            Assert.NotNull(storeItemCollection.GetItem<EntityType>("AdventureWorksModel.Store.Entities"));
        }

        [Fact]
        public void CreateAndValidateStoreItemCollection_throws_ArgumentNullException_for_null_ssdl()
        {
            Assert.Equal(
                "ssdl",
                Assert.Throws<ArgumentNullException>(
                    () => EdmExtension.CreateAndValidateStoreItemCollection(null, new Version(1, 0, 0, 0), null, true)).ParamName);
        }

        [Fact]
        public void CreateAndValidateStoreItemCollection_throws_ArgumentNullException_for_null_targetFrameworkVersion()
        {
            Assert.Equal(
                "targetFrameworkVersion",
                Assert.Throws<ArgumentNullException>(
                    () => EdmExtension.CreateAndValidateStoreItemCollection(string.Empty, null, null, true)).ParamName);
        }

        [Fact]
        public void CreateAndValidateStoreItemCollection_throws_ArgumentException_for_incorrect_targetFrameworkVersion()
        {
            var exception = Assert.Throws<ArgumentException>(
                () => EdmExtension.CreateAndValidateStoreItemCollection(string.Empty, new Version(0, 0), null, true));

            Assert.Equal("targetFrameworkVersion", exception.ParamName);
            Assert.True(
                exception.Message.StartsWith(
                    string.Format(CultureInfo.CurrentCulture, Resources.ErrorNonValidTargetVersion, "0.0")));
        }

        [Fact]
        public void CreateAndValidateStoreItemCollection_throws_for_invalid_ssdl_catchThrowNamingConflicts_false()
        {
            var invalidSsdl = XDocument.Parse(Ssdl);
            var entityTypeElement =
                invalidSsdl.Descendants("{http://schemas.microsoft.com/ado/2009/11/edm/ssdl}EntityType").Single();
            entityTypeElement.AddAfterSelf(new XElement("{http://schemas.microsoft.com/ado/2009/11/edm/ssdl}InvalidElement"));
            entityTypeElement.AddAfterSelf(entityTypeElement);

            var exception = Assert.Throws<InvalidOperationException>(
                () => EdmExtension.CreateAndValidateStoreItemCollection(
                    invalidSsdl.ToString(),
                    new Version(3, 0, 0, 0),
                    resolver,
                    catchThrowNamingConflicts: false));

            Assert.True(exception.Message.StartsWith(Resources.ErrorNonValidSsdl.Replace("{0}", string.Empty)));
            var exceptionData = (IList<EdmSchemaError>)exception.Data["ssdlErrors"];
            Assert.Equal(3, exceptionData.Count);
            Assert.True(exceptionData.All(e => exception.Message.Contains(e.Message)));
        }

        [Fact]
        public void CreateAndValidateStoreItemCollection_rewrites_exception_for_naming_conflicts_when_catchThrowNamingConflicts_true()
        {
            var invalidSsdl = XDocument.Parse(Ssdl);
            var entityTypeElement =
                invalidSsdl.Descendants("{http://schemas.microsoft.com/ado/2009/11/edm/ssdl}EntityType").Single();
            entityTypeElement.AddAfterSelf(new XElement("{http://schemas.microsoft.com/ado/2009/11/edm/ssdl}InvalidElement"));
            entityTypeElement.AddAfterSelf(entityTypeElement);

            var exception = Assert.Throws<InvalidOperationException>(
                () => EdmExtension.CreateAndValidateStoreItemCollection(
                    invalidSsdl.ToString(),
                    new Version(3, 0, 0, 0),
                    resolver,
                    catchThrowNamingConflicts: true));

            var exceptionData = (IList<EdmSchemaError>)exception.Data["ssdlErrors"];
            Assert.Equal(string.Format(Resources.ErrorNameCollision, exceptionData[0].Message), exception.Message);
            Assert.Equal(3, exceptionData.Count);
            Assert.Contains("'AdventureWorksModel.Store.Entities'", exceptionData[0].Message);
            Assert.Contains("'InvalidElement'", exceptionData[1].Message);
            Assert.Contains("InvalidElement", exceptionData[2].Message);
        }

        [Fact]
        public void CreateAndValidateStoreItemCollection_creates_StoreItemCollection_for_valid_ssdl_and_targetFrameworkVersion()
        {
            var storeItemCollection =
                EdmExtension.CreateAndValidateStoreItemCollection(
                    Ssdl,
                    new Version(3, 0, 0, 0),
                    resolver,
                    catchThrowNamingConflicts: true);

            Assert.NotNull(storeItemCollection);
            Assert.NotNull(storeItemCollection.GetItem<EntityType>("AdventureWorksModel.Store.Entities"));
        }

        [Fact]
        public void CreateStorageMappingItemCollection_returns_errors_for_invalid_ssdl()
        {
            var v3 = new Version(3, 0, 0, 0);
            var edmItemCollection = EdmExtension.CreateAndValidateEdmItemCollection(Csdl, v3);
            var storeItemCollection =
                EdmExtension.CreateAndValidateStoreItemCollection(
                    Ssdl,
                    v3,
                    resolver,
                    false);

            var invalidMsl = XDocument.Parse(Msl);
            invalidMsl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/mapping/cs}ScalarProperty")
                .First()
                .SetAttributeValue("Name", "Non-existing-property");

            IList<EdmSchemaError> edmErrors;
            var storageMappingItemCollection =
                EdmExtension.CreateStorageMappingItemCollection(
                    edmItemCollection,
                    storeItemCollection,
                    invalidMsl.ToString(),
                    out edmErrors);

            Assert.Null(storageMappingItemCollection);
            Assert.Equal(1, edmErrors.Count);
            Assert.Contains("'Non-existing-property'", edmErrors[0].Message);
        }

        [Fact]
        public void CreateStorageMappingItemCollection_creates_storage_mapping_item_collection_for_valid_artifacts()
        {
            var v3 = new Version(3, 0, 0, 0);
            var edmItemCollection = EdmExtension.CreateAndValidateEdmItemCollection(Csdl, v3);
            var storeItemCollection =
                EdmExtension.CreateAndValidateStoreItemCollection(
                    Ssdl,
                    v3,
                    resolver,
                    false);

            IList<EdmSchemaError> edmErrors;
            var storageMappingItemCollection = EdmExtension.CreateStorageMappingItemCollection(
                edmItemCollection, storeItemCollection, Msl, out edmErrors);

            Assert.NotNull(storageMappingItemCollection);
            Assert.Equal(0, edmErrors.Count);
            Assert.NotNull(storageMappingItemCollection.GetItem<GlobalItem>("AdventureWorksEntities3"));
        }

        public class CopyToSSDLTests
        {
            [Fact]
            public void CopyToSSDL_true_causes_CopyExtendedPropertiesToSsdlElement_to_copy_property()
            {
                foreach (var version in EntityFrameworkVersion.GetAllVersions())
                {
                    var csdlEntityTypeWithVersionedEdmxNamespaceCopyToSSDL = CreateEntityTypeWithExtendedProperty(
                        SchemaManager.GetEDMXNamespaceName(version), "true");
                    var ssdlEntityTypeElement = new XElement(
                        (XNamespace)(SchemaManager.GetSSDLNamespaceName(version)) + "EntityType",
                        new XAttribute("Name", "TestEntityType"));
                    OutputGeneratorHelpers.CopyExtendedPropertiesToSsdlElement(
                        csdlEntityTypeWithVersionedEdmxNamespaceCopyToSSDL, ssdlEntityTypeElement);
                    Assert.Equal(
                        "<MyProp p1:MyAttribute=\"MyValue\" xmlns:p1=\"http://myExtendedProperties\" xmlns=\"http://myExtendedProperties\" />",
                        ssdlEntityTypeElement.Elements().First().ToString());
                }
            }

            [Fact]
            public void CopyToSSDL_false_causes_CopyExtendedPropertiesToSsdlElement_to_not_copy_property()
            {
                foreach (var version in EntityFrameworkVersion.GetAllVersions())
                {
                    var csdlEntityTypeWithVersionedEdmxNamespaceCopyToSSDL = CreateEntityTypeWithExtendedProperty(
                        SchemaManager.GetEDMXNamespaceName(EntityFrameworkVersion.Version1), "false");
                    var ssdlEntityTypeElement = new XElement(
                        (XNamespace)(SchemaManager.GetSSDLNamespaceName(EntityFrameworkVersion.Version1)) + "EntityType",
                        new XAttribute("Name", "TestEntityType"));
                    OutputGeneratorHelpers.CopyExtendedPropertiesToSsdlElement(
                        csdlEntityTypeWithVersionedEdmxNamespaceCopyToSSDL, ssdlEntityTypeElement);
                    Assert.Empty(ssdlEntityTypeElement.Elements());
                }
            }

            [Fact]
            public void CopyToSSDL_in_non_edmx_namespace_causes_CopyExtendedPropertiesToSsdlElement_to_not_copy_property()
            {
                var csdlEntityTypeWithNonEdmxNamespaceCopyToSSDL = CreateEntityTypeWithExtendedProperty(
                    "http://SomeOtherNamespace", "true");

                foreach (var version in EntityFrameworkVersion.GetAllVersions())
                {
                    var ssdlEntityTypeElement = new XElement(
                        (XNamespace)(SchemaManager.GetSSDLNamespaceName(EntityFrameworkVersion.Version1)) + "EntityType",
                        new XAttribute("Name", "TestEntityType"));
                    OutputGeneratorHelpers.CopyExtendedPropertiesToSsdlElement(
                        csdlEntityTypeWithNonEdmxNamespaceCopyToSSDL, ssdlEntityTypeElement);
                    Assert.Empty(ssdlEntityTypeElement.Elements());
                }
            }

            private static EntityType CreateEntityTypeWithExtendedProperty(XNamespace copyToSSDLNamespace, string copyToSSDLValue)
            {
                var extendedPropertyContents =
                    new XElement(
                        (XNamespace)"http://myExtendedProperties" + "MyProp",
                        new XAttribute(
                            (XNamespace)"http://myExtendedProperties" + "MyAttribute", "MyValue"),
                        new XAttribute(
                            copyToSSDLNamespace + "CopyToSSDL", copyToSSDLValue));
                var extendedPropertyMetadataProperty =
                    MetadataProperty.Create(
                        "http://myExtendedProperties:MyProp",
                        TypeUsage.CreateStringTypeUsage(
                            PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String),
                            true,
                            false),
                        extendedPropertyContents
                        );
                return EntityType.Create(
                    "TestEntityType",
                    "Model1",
                    DataSpace.CSpace,
                    new[] { "Id" },
                    new[] { EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)) },
                    new[] { extendedPropertyMetadataProperty });
            }
        }
    }
}
