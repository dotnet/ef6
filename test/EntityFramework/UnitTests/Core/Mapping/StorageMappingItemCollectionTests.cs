// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping.ViewGeneration;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Linq;
    using Moq;
    using Xunit;

    public class StorageMappingItemCollectionTests
    {
        internal const string Ssdl =
            "<Schema Namespace='AdventureWorksModel.Store' Provider='System.Data.SqlClient' ProviderManifestToken='2008' xmlns='http://schemas.microsoft.com/ado/2009/11/edm/ssdl'>" +
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

        internal const string Csdl =
            "<Schema Namespace='AdventureWorksModel' Alias='Self' p1:UseStrongSpatialTypes='false' xmlns:annotation='http://schemas.microsoft.com/ado/2009/02/edm/annotation' xmlns:p1='http://schemas.microsoft.com/ado/2009/02/edm/annotation' xmlns='http://schemas.microsoft.com/ado/2009/11/edm'>" +
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

        internal const string Msl =
            "<Mapping Space='C-S' xmlns='http://schemas.microsoft.com/ado/2009/11/mapping/cs'>" +
            "  <EntityContainerMapping StorageEntityContainer='AdventureWorksModelStoreContainer' CdmEntityContainer='AdventureWorksEntities3'>" +
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

        [Fact]
        public void StorageMappingItemCollection_Create_factory_method_throws_for_null_edmItemCollection()
        {
            IList<EdmSchemaError> errors;

            Assert.Equal("edmItemCollection",
                Assert.Throws<ArgumentNullException>(
                    () => StorageMappingItemCollection.Create(null, null, null, null, out errors)).ParamName);
        }

        [Fact]
        public void StorageMappingItemCollection_Create_factory_method_throws_for_null_storeItemCollection()
        {
            var edmItemCollection = new EdmItemCollection(new[] { XDocument.Parse(Csdl).CreateReader() });

            IList<EdmSchemaError> errors;

            Assert.Equal("storeItemCollection",
                Assert.Throws<ArgumentNullException>(
                    () => StorageMappingItemCollection.Create(edmItemCollection, null, null, null, out errors)).ParamName);
        }

        [Fact]
        public void StorageMappingItemCollection_Create_factory_method_throws_for_null_readers()
        {
            var edmItemCollection = new EdmItemCollection(new[] { XDocument.Parse(Csdl).CreateReader() });
            var storeItemCollection = new StoreItemCollection(new[] { XDocument.Parse(Ssdl).CreateReader() });

            IList<EdmSchemaError> errors;

            Assert.Equal("xmlReaders",
                Assert.Throws<ArgumentNullException>(
                    () => StorageMappingItemCollection.Create(
                        edmItemCollection, 
                        storeItemCollection, 
                        null, 
                        null, 
                        out errors)).ParamName);
        }

        [Fact]
        public void StorageMappingItemCollection_Create_factory_method_throws_for_null_reader_in_the_collection()
        {
            var edmItemCollection = new EdmItemCollection(new[] { XDocument.Parse(Csdl).CreateReader() });
            var storeItemCollection = new StoreItemCollection(new[] { XDocument.Parse(Ssdl).CreateReader() });

            IList<EdmSchemaError> errors;

            Assert.Equal(Strings.CheckArgumentContainsNullFailed("xmlReaders"),
                Assert.Throws<ArgumentException>(
                    () => StorageMappingItemCollection.Create(
                        edmItemCollection,
                        storeItemCollection,
                        new XmlReader[1], 
                        null,
                        out errors)).Message);
        }

        [Fact]
        public void StorageMappingItemCollection_Create_factory_method_returns_null_and_errors_for_invalid_msl()
        {
            var edmItemCollection = new EdmItemCollection(new[] { XDocument.Parse(Csdl).CreateReader() });
            var storeItemCollection = new StoreItemCollection(new[] { XDocument.Parse(Ssdl).CreateReader() });

            var invalidMsl = XDocument.Parse(Msl);
            invalidMsl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/mapping/cs}ScalarProperty")
                .First()
                .SetAttributeValue("Name", "Non-existing-property");

            IList<EdmSchemaError> errors;
            var storageMappingItemCollection = StorageMappingItemCollection.Create(
                edmItemCollection,
                storeItemCollection,
                new[] { invalidMsl.CreateReader() },
                null,
                out errors);            

            Assert.Null(storageMappingItemCollection);
            Assert.Equal(1, errors.Count);
            Assert.Contains("'Non-existing-property'", errors[0].Message);
        }

        [Fact]
        public void StorageMappingItemCollection_Create_factory_method_returns_StorageMappingItemCollection_instance_for_valid_msl()
        {
            var edmItemCollection = new EdmItemCollection(new[] { XDocument.Parse(Csdl).CreateReader() });
            var storeItemCollection = new StoreItemCollection(new[] { XDocument.Parse(Ssdl).CreateReader() });

            IList<EdmSchemaError> errors;
            var storageMappingItemCollection = StorageMappingItemCollection.Create(
                edmItemCollection,
                storeItemCollection,
                new [] { XDocument.Parse(Msl).CreateReader()},
                null,
                out errors);

            Assert.NotNull(storageMappingItemCollection);
            Assert.NotNull(storageMappingItemCollection.GetItem<GlobalItem>("AdventureWorksEntities3"));
            Assert.Equal(0, errors.Count);
        }

        [Fact]
        public void Workspace_returns_a_new_workspace_with_all_collections_registered()
        {
            var edmItemCollection = new EdmItemCollection(new[] { XDocument.Parse(Csdl).CreateReader() });
            var storeItemCollection = new StoreItemCollection(new[] { XDocument.Parse(Ssdl).CreateReader() });

            IList<EdmSchemaError> errors;
            var storageMappingItemCollection = StorageMappingItemCollection.Create(
                edmItemCollection,
                storeItemCollection,
                new[] { XDocument.Parse(Msl).CreateReader() },
                null,
                out errors);

            var workspace = storageMappingItemCollection.Workspace;

            Assert.Same(edmItemCollection, workspace.GetItemCollection(DataSpace.CSpace));
            Assert.Same(storeItemCollection, workspace.GetItemCollection(DataSpace.SSpace));
            Assert.Same(storageMappingItemCollection, workspace.GetItemCollection(DataSpace.CSSpace));

            var objectItemCollection = (ObjectItemCollection)workspace.GetItemCollection(DataSpace.OSpace);
            var ocMappingCollection = (DefaultObjectMappingItemCollection)workspace.GetItemCollection(DataSpace.OCSpace);

            Assert.Same(objectItemCollection, ocMappingCollection.ObjectItemCollection);
            Assert.Same(edmItemCollection, ocMappingCollection.EdmItemCollection);
        }

        [Fact]
        public void SerializedCollectViewsFromCache_performs_scan_from_entry_assembly_if_no_view_assemblies_known()
        {
            var mockCache = new Mock<IViewAssemblyCache>();
            mockCache.Setup(m => m.Assemblies).Returns(Enumerable.Empty<Assembly>());

            Dictionary<EntitySetBase, GeneratedView> _;
            Dictionary<Pair<EntitySetBase, Pair<EntityTypeBase, bool>>, GeneratedView> __;
            var viewDictionary = new StorageMappingItemCollection.ViewDictionary(
                new Mock<StorageMappingItemCollection>().Object, out _, out __, mockCache.Object);

            viewDictionary.SerializedCollectViewsFromCache(
                new Mock<MetadataWorkspace>().Object, 
                new Mock<Dictionary<EntitySetBase, GeneratedView>>().Object,
                () => typeof(object).Assembly);

            mockCache.Verify(m => m.Assemblies, Times.Exactly(2));
            mockCache.Verify(m => m.CheckAssembly(typeof(object).Assembly, true), Times.Once());
        }

        [Fact]
        public void SerializedCollectViewsFromCache_does_not_scan_from_entry_assembly_if_any_view_assemblies_known()
        {
            var mockCache = new Mock<IViewAssemblyCache>();
            mockCache.Setup(m => m.Assemblies).Returns(new[] { typeof(object).Assembly });

            Dictionary<EntitySetBase, GeneratedView> _;
            Dictionary<Pair<EntitySetBase, Pair<EntityTypeBase, bool>>, GeneratedView> __;
            var viewDictionary = new StorageMappingItemCollection.ViewDictionary(
                new Mock<StorageMappingItemCollection>().Object, out _, out __, mockCache.Object);

            viewDictionary.SerializedCollectViewsFromCache(
                new Mock<MetadataWorkspace>().Object,
                new Mock<Dictionary<EntitySetBase, GeneratedView>>().Object,
                () => typeof(object).Assembly);

            mockCache.Verify(m => m.Assemblies, Times.Exactly(2));
            mockCache.Verify(m => m.CheckAssembly(It.IsAny<Assembly>(), It.IsAny<bool>()), Times.Never());
        }

        internal static StorageMappingItemCollection CreateStorageMappingItemCollection(string ssdl, string csdl, string msl)
        {
            StoreItemCollection storeCollection;
            EdmItemCollection edmCollection;
            StorageMappingItemCollection mappingCollection;

            using (var stringReader = new StringReader(ssdl))
            using (var xmlReader = XmlReader.Create(stringReader))
            {
                storeCollection = new StoreItemCollection(new[] { xmlReader });
            }

            using (var stringReader = new StringReader(csdl))
            using (var xmlReader = XmlReader.Create(stringReader))
            {
                edmCollection = new EdmItemCollection(new[] { xmlReader });
            }

            using (var stringReader = new StringReader(msl))
            using (var xmlReader = XmlReader.Create(stringReader))
            {
                mappingCollection = new StorageMappingItemCollection(edmCollection, storeCollection, new[] { xmlReader });
            }

            return mappingCollection;
        }

        [Fact]
        public static void Generate_creates_expected_result()
        {
            var mappingCollection =
                StorageMappingItemCollectionTests.CreateStorageMappingItemCollection(
                    StorageMappingItemCollectionTests.Ssdl,
                    StorageMappingItemCollectionTests.Csdl,
                    StorageMappingItemCollectionTests.Msl);

            var errors = new List<EdmSchemaError>();
            var viewGroups = mappingCollection.GenerateViews(errors);

            Assert.Equal(1, viewGroups.Count);

            var group = viewGroups[0];

            Assert.Equal("AdventureWorksModelStoreContainer", group.StoreContainerName);
            Assert.Equal("AdventureWorksEntities3", group.ModelContainerName);
            Assert.Equal("e6447993a1b3926723f95dce0a1caccc96ec5774b5ee78bbd28748745ad30db2", group.MappingHash);
            Assert.Equal(2, group.Views.Count);
        }
    }
}
