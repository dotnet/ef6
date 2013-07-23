// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Resources;
    using System.Data.Entity.SqlServer;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using Moq;
    using Xunit;

    public class StoreItemCollectionTests
    {
        private const string Ssdl =
            "<Schema Namespace='AdventureWorksModel.Store' ProviderManifestToken='2008' Provider='System.Data.SqlClient' xmlns='http://schemas.microsoft.com/ado/2009/11/edm/ssdl'>" +
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


        [Fact]
        public void StoreItemCollection_Create_factory_method_throws_for_null_readers()
        {
            IList<EdmSchemaError> errors;

            Assert.Equal("xmlReaders",
                Assert.Throws<ArgumentNullException>(
                    () => StoreItemCollection.Create(null, null, null, out errors)).ParamName);
        }

        [Fact]
        public void StoreItemCollection_Create_factory_method_throws_for_empty_reader_collection()
        {
            IList<EdmSchemaError> errors;

            Assert.Equal(Strings.StoreItemCollectionMustHaveOneArtifact("xmlReaders"),
                Assert.Throws<ArgumentException>(
                    () => StoreItemCollection.Create(new XmlReader[0], null, null, out errors)).Message);
            
        }

        [Fact]
        public void StoreItemCollection_Create_factory_method_throws_for_null_reader_in_the_collection()
        {
            IList<EdmSchemaError> errors;

            Assert.Equal(Strings.CheckArgumentContainsNullFailed("xmlReaders"),
                Assert.Throws<ArgumentException>(
                    () => StoreItemCollection.Create(new XmlReader[1], null, null, out errors)).Message);
        }

        [Fact]
        public void StoreItemCollection_Create_factory_method_returns_null_and_errors_for_invalid_ssdl()
        {
            var invalidSsdl = XDocument.Parse(Ssdl);
            invalidSsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm/ssdl}EntityType")
                .Remove();

            IList<EdmSchemaError> errors;
            var storeItemCollection = StoreItemCollection.Create(new[] { invalidSsdl.CreateReader() }, null, null, out errors);

            Assert.Null(storeItemCollection);
            Assert.Equal(1, errors.Count);
            Assert.Contains("Entities", errors[0].Message);
        }

        [Fact]
        public void StoreItemCollection_Create_factory_method_returns_StoreItemCollection_instance_for_valid_ssdl()
        {
            IList<EdmSchemaError> errors;
            var storeItemCollection = StoreItemCollection.Create(new[] { XDocument.Parse(Ssdl).CreateReader() }, null, null, out errors);

            Assert.NotNull(storeItemCollection);
            Assert.NotNull(storeItemCollection.GetItem<EntityType>("AdventureWorksModel.Store.Entities"));
            Assert.Equal(0, errors.Count);
        }

        [Fact]
        public void Custom_resolver_is_used_to_resolve_DbProviderServices_when_loading_StoreItemCollection()
        {
            var mockResolver = new Mock<IDbDependencyResolver>();
            mockResolver
                .Setup(
                    m => m.GetService(
                        It.Is<Type>(t => t == typeof(DbProviderServices)),
                        It.Is<object>(key => (string)key == "System.Data.SqlClient")))
                .Returns(SqlProviderServices.Instance);

            IList<EdmSchemaError> errors;
            var storeItemCollection = 
                StoreItemCollection.Create(
                    new[] { XDocument.Parse(Ssdl).CreateReader() }, 
                    null, 
                    mockResolver.Object, 
                    out errors);

            Assert.NotNull(storeItemCollection);
            Assert.NotNull(storeItemCollection.GetItem<EntityType>("AdventureWorksModel.Store.Entities"));
            Assert.Equal(0, errors.Count);

            mockResolver.Verify(                   
                m => m.GetService(
                        It.Is<Type>(t => t == typeof(DbProviderServices)),
                        It.Is<object>(key => (string)key == "System.Data.SqlClient")), Times.Once());
        }
        
        [Fact]
        public void Can_initialize_from_ssdl_model_and_items_set_read_only_and_added_to_collection()
        {
            var context = new ShopContext_v1();
            var compiledModel = context.InternalContext.CodeFirstModel;

            var builder = compiledModel.CachedModelBuilder.Clone();

            var databaseMapping
                = builder.Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping;

            var storeItemCollection = new StoreItemCollection(databaseMapping.Database);

            Assert.Equal(3.0, storeItemCollection.StoreSchemaVersion);

            foreach (var globalItem in databaseMapping.Database.GlobalItems)
            {
                Assert.True(storeItemCollection.Contains(globalItem));
                Assert.True(globalItem.IsReadOnly);
            }
        }

        [Fact]
        public void Can_initialize_and_provider_fields_set()
        {
            var model = EdmModel.CreateStoreModel(
                ProviderRegistry.Sql2008_ProviderInfo,
                new SqlProviderManifest("2008"));

            var itemCollection = new StoreItemCollection(model);

            Assert.Same(model.ProviderInfo.ProviderManifestToken, itemCollection.ProviderManifestToken);
            Assert.NotNull(itemCollection.ProviderFactory);
            Assert.Same(model.ProviderManifest, itemCollection.ProviderManifest);
            Assert.Same(model.ProviderInfo.ProviderInvariantName, itemCollection.ProviderInvariantName);
        }

        [Fact]
        public void Can_get_ProviderInvariantName_from_StoreItemCollection_loaded_from_SSDL()
        {
            IList<EdmSchemaError> errors;
            var storeItemCollection = 
                StoreItemCollection.Create(new[] { XDocument.Parse(Ssdl).CreateReader() }, null, null, out errors);

            Assert.Equal("System.Data.SqlClient", storeItemCollection.ProviderInvariantName);
        }

        [Fact]
        public void ProviderInvariantName_passed_in_StoreItemCollection_ctor_set_correctly()
        {
            var fakeSqlProviderManifest = FakeSqlProviderServices.Instance.GetProviderManifest("2008");

            var storeItemCollection = 
                new StoreItemCollection(
                    FakeSqlProviderFactory.Instance,
                    fakeSqlProviderManifest, 
                    "providerInvariantName", 
                    "token");

            Assert.Equal("providerInvariantName", storeItemCollection.ProviderInvariantName);
            Assert.Equal("token", storeItemCollection.ProviderManifestToken);
        }
    }
}
