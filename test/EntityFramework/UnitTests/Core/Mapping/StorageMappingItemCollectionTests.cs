// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping.ViewGeneration;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.MappingViews;
    using System.Data.Entity.Resources;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Linq;
    using Moq;
    using Xunit;
using System.Data.Entity.Infrastructure;

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
            "<Schema Namespace='AdventureWorksModel' Alias='Self' xmlns:annotation='http://schemas.microsoft.com/ado/2009/02/edm/annotation' xmlns='http://schemas.microsoft.com/ado/2009/11/edm'>" +
            "   <EntityContainer Name='AdventureWorksEntities3'>" +
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

        private const string SchoolSsdl =
            "<Schema Namespace='SchoolModel.Store' Provider='System.Data.SqlClient' ProviderManifestToken='2008' xmlns='http://schemas.microsoft.com/ado/2009/11/edm/ssdl'>" +
            "  <EntityContainer Name='SchoolStoreContainer'>" +
            "    <EntitySet Name='Students' EntityType='SchoolModel.Store.Students' Schema='dbo' />" +
            "  </EntityContainer>" +
            "  <EntityType Name='Students'>" +
            "    <Key>" +
            "      <PropertyRef Name='Id' />" +
            "    </Key>" +
            "    <Property Name='Id' Type='int' StoreGeneratedPattern='Identity' Nullable='false' />" +
            "  </EntityType>" +
            "</Schema>";

        private const string SchoolCsdl =
            "<Schema Namespace='SchoolModel' Alias='Self' xmlns:annotation='http://schemas.microsoft.com/ado/2009/02/edm/annotation' xmlns='http://schemas.microsoft.com/ado/2009/11/edm'>" +
            "   <EntityContainer Name='SchoolContainer'>" +
            "       <EntitySet Name='Students' EntityType='SchoolModel.Student' />" +
            "   </EntityContainer>" +
            "   <EntityType Name='Student'>" +
            "       <Key>" +
            "           <PropertyRef Name='Id' />" +
            "       </Key>" +
            "       <Property Type='Int32' Name='Id' Nullable='false' annotation:StoreGeneratedPattern='Identity' />" +
            "   </EntityType>" +
            "</Schema>";

        private const string SchoolMsl =
            "<Mapping Space='C-S' xmlns='http://schemas.microsoft.com/ado/2009/11/mapping/cs'>" +
            "  <EntityContainerMapping StorageEntityContainer='SchoolStoreContainer' CdmEntityContainer='SchoolContainer'>" +
            "    <EntitySetMapping Name='Students'>" +
            "      <EntityTypeMapping TypeName='SchoolModel.Student'>" +
            "        <MappingFragment StoreEntitySet='Students'>" +
            "          <ScalarProperty Name='Id' ColumnName='Id' />" +
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

        internal static StorageMappingItemCollection CreateStorageMappingItemCollection(string ssdl, string csdl, string msl)
        {
            return CreateStorageMappingItemCollection(new[] {ssdl}, new [] {csdl}, new [] { msl });
        }

        private static StorageMappingItemCollection CreateStorageMappingItemCollection(string[] ssdlArtifacts, string[] csdlArtifacts, string[] mslArtifacts)
        {
            return new StorageMappingItemCollection(
                new EdmItemCollection(csdlArtifacts.Select(csdl => XmlReader.Create(new StringReader(csdl)))),
                new StoreItemCollection(ssdlArtifacts.Select(ssdl => XmlReader.Create(new StringReader(ssdl)))), 
                mslArtifacts.Select(msl => XmlReader.Create(new StringReader(msl))));
        }

        [Fact]
        public void ComputeMappingHashValue_computes_hashvalue_for_single_container_mapping()
        {
            var mappingCollection =
                CreateStorageMappingItemCollection(new[] { SchoolSsdl }, new[] { SchoolCsdl }, new[] { SchoolMsl });

            var hashValue = mappingCollection.ComputeMappingHashValue();

            Assert.Equal("e117e3d423b1c43cf4e9bc77462fe1434fc14872ec7873ad283748398706bc67", hashValue);
        }

        [Fact]
        public void ComputeMappingHashValue_computes_hashvalue_for_multiple_container_mappings()
        {
            var mappingCollection =
                CreateStorageMappingItemCollection(new[] { Ssdl, SchoolSsdl }, new[] { Csdl, SchoolCsdl }, new[] { Msl, SchoolMsl });

            var hashValue = mappingCollection.ComputeMappingHashValue("AdventureWorksEntities3", "AdventureWorksModelStoreContainer");

            Assert.Equal("e6447993a1b3926723f95dce0a1caccc96ec5774b5ee78bbd28748745ad30db2", hashValue);

            hashValue = mappingCollection.ComputeMappingHashValue("SchoolContainer", "SchoolStoreContainer");

            Assert.Equal("e117e3d423b1c43cf4e9bc77462fe1434fc14872ec7873ad283748398706bc67", hashValue);
        }

        [Fact]
        public void ComputeMappingHashValue_for_single_container_mapping_throws_if_multiple_mappings()
        {
            var mappingCollection =
                CreateStorageMappingItemCollection(new[] { Ssdl, SchoolSsdl }, new[] { Csdl, SchoolCsdl }, new[] { Msl, SchoolMsl });

            Assert.Throws<InvalidOperationException>(() => mappingCollection.ComputeMappingHashValue());
        }

        [Fact]
        public void ComputeMappingHashValue_throws_if_cannot_match_container_names()
        {
            var mappingCollection =
                CreateStorageMappingItemCollection(new[] { Ssdl, SchoolSsdl }, new[] { Csdl, SchoolCsdl }, new[] { Msl, SchoolMsl });

            Assert.Throws<InvalidOperationException>(() => mappingCollection.ComputeMappingHashValue("C", "S"));
        }

        [Fact]
        public static void GenerateViews_creates_views_for_single_container_mapping()
        {
            var mappingCollection =
                CreateStorageMappingItemCollection(new[] { SchoolSsdl }, new[] { SchoolCsdl }, new[] { SchoolMsl });

            var errors = new List<EdmSchemaError>();
            var views = mappingCollection.GenerateViews(errors);

            Assert.Empty(errors);
            Assert.Equal(2, views.Count);
        }

        [Fact]
        public void GenerateViews_creates_views_for_multiple_container_mappings()
        {
            var mappingCollection =
                CreateStorageMappingItemCollection(new[] { Ssdl, SchoolSsdl }, new[] { Csdl, SchoolCsdl }, new[] { Msl, SchoolMsl });

            var errors = new List<EdmSchemaError>();
            var views = mappingCollection.GenerateViews("AdventureWorksEntities3", "AdventureWorksModelStoreContainer", errors);
            
            Assert.Empty(errors);
            Assert.Equal(2, views.Count);

            errors = new List<EdmSchemaError>();
            views = mappingCollection.GenerateViews("SchoolContainer", "SchoolStoreContainer", errors);

            Assert.Empty(errors);
            Assert.Equal(2, views.Count);
        }

        [Fact]
        public void GenerateViews_for_single_container_mapping_throws_if_multiple_mappings()
        {
            var mappingCollection =
                CreateStorageMappingItemCollection(new[] { Ssdl, SchoolSsdl }, new[] { Csdl, SchoolCsdl }, new[] { Msl, SchoolMsl });

            var errors = new List<EdmSchemaError>();

            Assert.Throws<InvalidOperationException>(() => mappingCollection.GenerateViews(errors));
        }

        [Fact]
        public void GenerateViews_throws_if_cannot_match_container_names()
        {
            var mappingCollection =
                CreateStorageMappingItemCollection(new[] { Ssdl, SchoolSsdl }, new[] { Csdl, SchoolCsdl }, new[] { Msl, SchoolMsl });

            var errors = new List<EdmSchemaError>();

            Assert.Throws<InvalidOperationException>(() => mappingCollection.GenerateViews("C", "S", errors));
        }

        [Fact]
        public void GenerateViews_returns_errors_for_invalid_mapping()
        {
            var msl = XDocument.Parse(Msl);
            msl.Descendants("{http://schemas.microsoft.com/ado/2009/11/mapping/cs}ScalarProperty")
                .Where(e => (string)e.Attribute("Name") == "Name")
                .Remove();

            var errors = new List<EdmSchemaError>();
            var storageMappingItemCollection = CreateStorageMappingItemCollection(Ssdl, Csdl, msl.ToString());
            var views = storageMappingItemCollection.GenerateViews(errors);

            Assert.Equal(1, errors.Count);
            Assert.Contains(Strings.ViewGen_No_Default_Value("Entities", "Entities.Name"), errors[0].Message);
            Assert.Equal(0, views.Count);
        }

        [Fact]
        public void GenerateViews_generates_views_for_all_containers_that_contain_mappings()
        {
            var storageMappingItemCollection = 
                CreateStorageMappingItemCollection(new[] { Ssdl, SchoolSsdl }, new[] { Csdl, SchoolCsdl }, new[] { SchoolMsl, Msl});

            var containerMapping = storageMappingItemCollection.GetItems<StorageEntityContainerMapping>().First();
            foreach (var entityTypeMapping in containerMapping.EntitySetMappings.SelectMany(m => m.EntityTypeMappings).ToList())
            {
                entityTypeMapping.SetMapping.RemoveTypeMapping(entityTypeMapping);
            }

            var errors = new List<EdmSchemaError>();
            var views = storageMappingItemCollection.GenerateViews("AdventureWorksEntities3", "AdventureWorksModelStoreContainer", errors);

            Assert.Empty(errors);
            Assert.Equal(2, views.Count);

            errors = new List<EdmSchemaError>();
            views = storageMappingItemCollection.GenerateViews("SchoolContainer", "SchoolStoreContainer", errors);

            Assert.Empty(errors);
            Assert.Equal(0, views.Count);
        }

        [Fact]
        public void MappingViewCacheFactory_can_be_set_and_retrieved()
        {
            var itemCollection = new StorageMappingItemCollection();
            var factory = new SampleMappingViewCacheFactory("value");

            itemCollection.MappingViewCacheFactory = factory;

            Assert.Same(factory, itemCollection.MappingViewCacheFactory);
        }

        [Fact]
        public void MappingViewCacheFactory_cannot_be_changed()
        {
            var itemCollection = new StorageMappingItemCollection();

            var factory1 = new SampleMappingViewCacheFactory("value1");
            var factory2 = new SampleMappingViewCacheFactory("value1");
            var factory3 = new SampleMappingViewCacheFactory("value2");

            itemCollection.MappingViewCacheFactory = factory1;

            // Set with same instance does not throw.
            itemCollection.MappingViewCacheFactory = factory1;

            // Set with new instance and equal value does not throw.
            itemCollection.MappingViewCacheFactory = factory2;

            // Set with new instance and different value throws.
            var exception = new ArgumentException(Strings.MappingViewCacheFactory_MustNotChange, "value");
            Assert.Equal(exception.Message,
                Assert.Throws<ArgumentException>(
                    () => itemCollection.MappingViewCacheFactory = factory3).Message);
        }

        private class SampleMappingViewCacheFactory : DbMappingViewCacheFactory
        {
            private readonly string _value;

            public SampleMappingViewCacheFactory(string value)
            {
                _value = value;
            }

            public override DbMappingViewCache Create(string conceptualModelContainerName, string storeModelContainerName)
            {
	            throw new NotImplementedException();
            }

            public override bool Equals(object obj)
            {
                return _value.Equals(((SampleMappingViewCacheFactory)obj)._value);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}
