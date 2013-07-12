// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Config;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Mapping.ViewGeneration;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.ViewGeneration;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Linq;
    using Moq;
    using Xunit;

    public class MetadataWorkspaceTests
    {
        public class Constructors : TestBase
        {
            [Fact]
            public void Loader_constructors_validate_for_null_delegates()
            {
                Assert.Equal(
                    "cSpaceLoader",
                    Assert.Throws<ArgumentNullException>(() => new MetadataWorkspace(null, () => null, () => null)).ParamName);
                Assert.Equal(
                    "sSpaceLoader",
                    Assert.Throws<ArgumentNullException>(() => new MetadataWorkspace(() => null, null, () => null)).ParamName);
                Assert.Equal(
                    "csMappingLoader",
                    Assert.Throws<ArgumentNullException>(() => new MetadataWorkspace(() => null, () => null, null)).ParamName);

                Assert.Equal(
                    "cSpaceLoader",
                    Assert.Throws<ArgumentNullException>(() => new MetadataWorkspace(null, () => null, () => null, () => null))
                          .ParamName);
                Assert.Equal(
                    "sSpaceLoader",
                    Assert.Throws<ArgumentNullException>(() => new MetadataWorkspace(() => null, null, () => null, () => null))
                          .ParamName);
                Assert.Equal(
                    "csMappingLoader",
                    Assert.Throws<ArgumentNullException>(() => new MetadataWorkspace(() => null, () => null, null, () => null))
                          .ParamName);
                Assert.Equal(
                    "oSpaceLoader",
                    Assert.Throws<ArgumentNullException>(() => new MetadataWorkspace(() => null, () => null, () => null, null))
                          .ParamName);
            }

            [Fact]
            public void Parameterless_constructor_sets_up_default_o_space_collection()
            {
                Assert.IsType<ObjectItemCollection>(new MetadataWorkspace().GetItemCollection(DataSpace.OSpace));
            }

            [Fact]
            public void Three_delegates_constructor_uses_given_delegates_and_sets_up_default_o_space_and_oc_mapping()
            {
                var edmItemCollection = new EdmItemCollection(new[] { XDocument.Parse(_csdlV3).CreateReader() });
                var storeItemCollection = new StoreItemCollection(new[] { XDocument.Parse(_ssdlV3).CreateReader() });
                var storageMappingItemCollection = LoadMsl(edmItemCollection, storeItemCollection);

                var workspace = new MetadataWorkspace(
                    () => edmItemCollection,
                    () => storeItemCollection,
                    () => storageMappingItemCollection);

                Assert.Same(edmItemCollection, workspace.GetItemCollection(DataSpace.CSpace));
                Assert.Same(storeItemCollection, workspace.GetItemCollection(DataSpace.SSpace));
                Assert.Same(storageMappingItemCollection, workspace.GetItemCollection(DataSpace.CSSpace));

                var objectItemCollection = (ObjectItemCollection)workspace.GetItemCollection(DataSpace.OSpace);
                var ocMappingCollection = (DefaultObjectMappingItemCollection)workspace.GetItemCollection(DataSpace.OCSpace);

                Assert.Same(objectItemCollection, ocMappingCollection.ObjectItemCollection);
                Assert.Same(edmItemCollection, ocMappingCollection.EdmItemCollection);
            }

            [Fact]
            public void GetItemCollection_on_both_C_and_S_space_using_item_collections_with_inconsistent_versions_throws_MetadataException()
            {
                var edmItemCollection = new EdmItemCollection(new[] { XDocument.Parse(_csdlV3).CreateReader() });
                var storeItemCollection =
                    new StoreItemCollection(
                        new[]
                            {
                                XDocument.Parse(string.Format(CultureInfo.InvariantCulture, SsdlTemplate, XmlConstants.TargetNamespace_2))
                                    .CreateReader()
                            });

                var workspace = new MetadataWorkspace(
                    () => edmItemCollection,
                    () => storeItemCollection,
                    () => null);

                workspace.GetItemCollection(DataSpace.CSpace); // this sets up the MetadataWorkspace's expected schema version as the csdl version i.e. 3.0
                Assert.Equal(
                    Resources.Strings.DifferentSchemaVersionInCollection("StoreItemCollection", 2.0, 3.0),
                    Assert.Throws<MetadataException>(() => workspace.GetItemCollection(DataSpace.SSpace)).Message);
            }

            [Fact]
            public void GetItemCollection_on_both_C_and_CS_space_using_item_collections_with_inconsistent_versions_throws_MetadataException()
            {
                var edmItemCollection = new EdmItemCollection(new[] { XDocument.Parse(_csdlV3).CreateReader() });
                var storeItemCollection = new StoreItemCollection(new[] { XDocument.Parse(_ssdlV3).CreateReader() });
                var storageMappingItemCollection = LoadMsl(edmItemCollection, storeItemCollection);

                // edmItemCollection and storeItemCollection must have the same version to generate storageMappingItemCollection
                // here we override the edmItemCollection so that it's version will be different for the MetadataWorkspace
                edmItemCollection =
                    new EdmItemCollection(
                        new[]
                            {
                                XDocument.Parse(string.Format(CultureInfo.InvariantCulture, CsdlTemplate, XmlConstants.ModelNamespace_1))
                                    .CreateReader()
                            });

                var workspace = new MetadataWorkspace(
                    () => edmItemCollection,
                    () => null,
                    () => storageMappingItemCollection);

                workspace.GetItemCollection(DataSpace.CSSpace); // this sets up the MetadataWorkspace's expected schema version as the msl version i.e. 3.0
                Assert.Equal(
                    Resources.Strings.DifferentSchemaVersionInCollection("EdmItemCollection", 1.0, 3.0),
                    Assert.Throws<MetadataException>(() => workspace.GetItemCollection(DataSpace.CSpace)).Message);
            }

            [Fact]
            public void GetItemCollection_on_both_CS_and_S_space_using_item_collections_with_inconsistent_versions_throws_MetadataException()
            {
                var edmItemCollection = new EdmItemCollection(new[] { XDocument.Parse(_csdlV3).CreateReader() });
                var storeItemCollection = new StoreItemCollection(new[] { XDocument.Parse(_ssdlV3).CreateReader() });
                var storageMappingItemCollection = LoadMsl(edmItemCollection, storeItemCollection);

                // edmItemCollection and storeItemCollection must have the same version to generate storageMappingItemCollection
                // here we override the storeItemCollection so that it's version will be different for the MetadataWorkspace
                storeItemCollection =
                    new StoreItemCollection(
                        new[]
                            {
                                XDocument.Parse(string.Format(CultureInfo.InvariantCulture, SsdlTemplate, XmlConstants.TargetNamespace_2))
                                    .CreateReader()
                            });

                var workspace = new MetadataWorkspace(
                    () => null,
                    () => storeItemCollection,
                    () => storageMappingItemCollection);

                workspace.GetItemCollection(DataSpace.SSpace); // this sets up the MetadataWorkspace's expected schema version as the ssdl version i.e. 2.0
                Assert.Equal(
                    Resources.Strings.DifferentSchemaVersionInCollection("StorageMappingItemCollection", 3.0, 2.0),
                    Assert.Throws<MetadataException>(() => workspace.GetItemCollection(DataSpace.CSSpace)).Message);
            }

            [Fact]
            public void Four_delegates_constructor_uses_given_delegates_and_sets_up_default_oc_mapping()
            {
                var edmItemCollection = new EdmItemCollection(new[] { XDocument.Parse(_csdlV3).CreateReader() });
                var storeItemCollection = new StoreItemCollection(new[] { XDocument.Parse(_ssdlV3).CreateReader() });
                var objectItemCollection = new ObjectItemCollection();
                var storageMappingItemCollection = LoadMsl(edmItemCollection, storeItemCollection);

                var workspace = new MetadataWorkspace(
                    () => edmItemCollection,
                    () => storeItemCollection,
                    () => storageMappingItemCollection,
                    () => objectItemCollection);

                Assert.Same(edmItemCollection, workspace.GetItemCollection(DataSpace.CSpace));
                Assert.Same(storeItemCollection, workspace.GetItemCollection(DataSpace.SSpace));
                Assert.Same(storageMappingItemCollection, workspace.GetItemCollection(DataSpace.CSSpace));
                Assert.Same(objectItemCollection, workspace.GetItemCollection(DataSpace.OSpace));

                var ocMappingCollection = (DefaultObjectMappingItemCollection)workspace.GetItemCollection(DataSpace.OCSpace);
                Assert.Same(objectItemCollection, ocMappingCollection.ObjectItemCollection);
                Assert.Same(edmItemCollection, ocMappingCollection.EdmItemCollection);
            }

            [Fact]
            public void Paths_constructor_loads_collections_from_given_paths_and_sets_up_o_space_and_oc_mapping()
            {
                RunTestWithTempMetadata(
                    _csdlV3, _ssdlV3, _mslV3,
                    paths =>
                        {
                            var workspace = new MetadataWorkspace(paths, new Assembly[0]);

                            var cSpace = (EdmItemCollection)workspace.GetItemCollection(DataSpace.CSpace);
                            Assert.NotNull(cSpace.GetType("Entity", "AdventureWorksModel"));

                            var sSpace = (StoreItemCollection)workspace.GetItemCollection(DataSpace.SSpace);
                            Assert.NotNull(sSpace.GetType("Entities", "AdventureWorksModel.Store"));

                            var csMapping = (StorageMappingItemCollection)workspace.GetItemCollection(DataSpace.CSSpace);
                            Assert.Same(cSpace, csMapping.EdmItemCollection);
                            Assert.Same(sSpace, csMapping.StoreItemCollection);

                            var oSpace = (ObjectItemCollection)workspace.GetItemCollection(DataSpace.OSpace);
                            var ocMapping = (DefaultObjectMappingItemCollection)workspace.GetItemCollection(DataSpace.OCSpace);
                            Assert.Same(oSpace, ocMapping.ObjectItemCollection);
                            Assert.Same(cSpace, ocMapping.EdmItemCollection);
                        });
            }
        }

        public class RegisterItemCollection : TestBase
        {
            [Fact]
            public void Item_collections_can_be_registered_into_an_empty_workspace()
            {
                Item_collections_can_be_registered(new MetadataWorkspace());
            }

            [Fact]
            public void Registering_a_new_item_collection_replaces_any_existing_registration()
            {
                var storageMappingItemCollection = LoadMsl(
                    new EdmItemCollection(new[] { XDocument.Parse(_csdlV3).CreateReader() }),
                    new StoreItemCollection(new[] { XDocument.Parse(_ssdlV3).CreateReader() }));

                Item_collections_can_be_registered(
                    new MetadataWorkspace(
                        () => storageMappingItemCollection.EdmItemCollection,
                        () => storageMappingItemCollection.StoreItemCollection,
                        () => storageMappingItemCollection));
            }

            private static void Item_collections_can_be_registered(MetadataWorkspace workspace)
            {
                var edmItemCollection = new EdmItemCollection(new[] { XDocument.Parse(_csdlV3).CreateReader() });
                var storeItemCollection = new StoreItemCollection(new[] { XDocument.Parse(_ssdlV3).CreateReader() });
                var objectItemCollection = new ObjectItemCollection();
                var storageMappingItemCollection = LoadMsl(edmItemCollection, storeItemCollection);
                var ocMappingItemCollection = new DefaultObjectMappingItemCollection(edmItemCollection, objectItemCollection);

#pragma warning disable 612,618
                workspace.RegisterItemCollection(edmItemCollection);
                workspace.RegisterItemCollection(storeItemCollection);
                workspace.RegisterItemCollection(objectItemCollection);
                workspace.RegisterItemCollection(storageMappingItemCollection);
                workspace.RegisterItemCollection(ocMappingItemCollection);
#pragma warning restore 612,618

                Assert.Same(edmItemCollection, workspace.GetItemCollection(DataSpace.CSpace));
                Assert.Same(storeItemCollection, workspace.GetItemCollection(DataSpace.SSpace));
                Assert.Same(storageMappingItemCollection, workspace.GetItemCollection(DataSpace.CSSpace));
                Assert.Same(objectItemCollection, workspace.GetItemCollection(DataSpace.OSpace));
                Assert.Same(ocMappingItemCollection, workspace.GetItemCollection(DataSpace.OCSpace));
            }

            [Fact]
            public void Registering_c_space_causes_oc_mapping_to_also_be_registered_if_it_is_not_already_registered()
            {
                var edmItemCollection = new EdmItemCollection(new[] { XDocument.Parse(_csdlV3).CreateReader() });

                var workspace = new MetadataWorkspace();
#pragma warning disable 612,618
                workspace.RegisterItemCollection(edmItemCollection);
#pragma warning restore 612,618

                Assert.Same(edmItemCollection, workspace.GetItemCollection(DataSpace.CSpace));

                var ocMappingCollection = (DefaultObjectMappingItemCollection)workspace.GetItemCollection(DataSpace.OCSpace);
                Assert.Same(workspace.GetItemCollection(DataSpace.OSpace), ocMappingCollection.ObjectItemCollection);
                Assert.Same(edmItemCollection, ocMappingCollection.EdmItemCollection);
            }

            [Fact]
            public void Registering_c_space_or_o_space_does_not_cause_oc_mapping_to_be_registered_if_it_is_already_registered()
            {
                var edmItemCollection = new EdmItemCollection(new[] { XDocument.Parse(_csdlV3).CreateReader() });
                var objectItemCollection = new ObjectItemCollection();
                var ocMappingItemCollection = new DefaultObjectMappingItemCollection(edmItemCollection, objectItemCollection);

                var workspace = new MetadataWorkspace();
#pragma warning disable 612,618
                workspace.RegisterItemCollection(ocMappingItemCollection);
                workspace.RegisterItemCollection(edmItemCollection);
                workspace.RegisterItemCollection(objectItemCollection);
#pragma warning restore 612,618

                Assert.Same(ocMappingItemCollection, workspace.GetItemCollection(DataSpace.OCSpace));
                Assert.Same(edmItemCollection, workspace.GetItemCollection(DataSpace.CSpace));
                Assert.Same(objectItemCollection, workspace.GetItemCollection(DataSpace.OSpace));
            }

            [Fact]
            public void
                Registering_o_space_causes_oc_mapping_to_also_be_registered_if_it_is_not_already_registered_and_c_space_is_registered()
            {
                var edmItemCollection = new EdmItemCollection(new[] { XDocument.Parse(_csdlV3).CreateReader() });
                var objectItemCollection = new ObjectItemCollection();

                var workspace = new MetadataWorkspace();
#pragma warning disable 612,618
                workspace.RegisterItemCollection(edmItemCollection);
                workspace.RegisterItemCollection(objectItemCollection);
#pragma warning restore 612,618

                Assert.Same(edmItemCollection, workspace.GetItemCollection(DataSpace.CSpace));
                Assert.Same(objectItemCollection, workspace.GetItemCollection(DataSpace.OSpace));

                var ocMappingCollection = (DefaultObjectMappingItemCollection)workspace.GetItemCollection(DataSpace.OCSpace);
                Assert.Same(objectItemCollection, ocMappingCollection.ObjectItemCollection);
                Assert.Same(edmItemCollection, ocMappingCollection.EdmItemCollection);
            }

            [Fact]
            public void Registering_o_space_does_not_cause_oc_mapping_to_be_registered_if_c_space_is_not_registered()
            {
                var objectItemCollection = new ObjectItemCollection();

                var workspace = new MetadataWorkspace();
#pragma warning disable 612,618
                workspace.RegisterItemCollection(objectItemCollection);
#pragma warning restore 612,618

                Assert.Same(objectItemCollection, workspace.GetItemCollection(DataSpace.OSpace));
                ItemCollection _;
                Assert.False(workspace.TryGetItemCollection(DataSpace.OCSpace, out _));
            }
        }

        private static StorageMappingItemCollection LoadMsl(EdmItemCollection edmItemCollection, StoreItemCollection storeItemCollection)
        {
            IList<EdmSchemaError> errors;
            return StorageMappingItemCollection.Create(
                edmItemCollection,
                storeItemCollection,
                new[] { XDocument.Parse(_mslV3).CreateReader() },
                null,
                out errors);
        }

        private const string SsdlTemplate =
            "<Schema Namespace='AdventureWorksModel.Store' Provider='System.Data.SqlClient' ProviderManifestToken='2008' xmlns='{0}'>"
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

        private static readonly string _ssdlV3 = string.Format(CultureInfo.InvariantCulture, SsdlTemplate, XmlConstants.TargetNamespace_3);

        private const string CsdlTemplate =
            "<Schema Namespace='AdventureWorksModel' Alias='Self' p1:UseStrongSpatialTypes='false' xmlns:annotation='http://schemas.microsoft.com/ado/2009/02/edm/annotation' xmlns:p1='http://schemas.microsoft.com/ado/2009/02/edm/annotation' xmlns='{0}'>"
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

        private static readonly string _csdlV3 = string.Format(CultureInfo.InvariantCulture, CsdlTemplate, XmlConstants.ModelNamespace_3);

        private const string MslTemplate =
            "<Mapping Space='C-S' xmlns='{0}'>" +
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

        private static readonly string _mslV3 = string.Format(CultureInfo.InvariantCulture, MslTemplate, StorageMslConstructs.NamespaceUriV3);

    }
}
    