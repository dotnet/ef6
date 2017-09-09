// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using System.Data.Entity.SqlServer;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Moq;
    using Xunit;

    public class StorageEntityModelTests
    {
        [Fact]
        public void StoreTypeNameToStoreTypeMap_returns_type_map()
        {
            var ssdl =
                XElement.Parse(
                    "<Schema Namespace=\"Model.Store\" Provider=\"System.Data.SqlClient\" ProviderManifestToken=\"2008\" Alias=\"Self\" xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm/ssdl\" />");

            using (var storageModel = new StorageEntityModel(null, ssdl))
            {
                var typeMap = storageModel.StoreTypeNameToStoreTypeMap;

                Assert.Equal(
                    SqlProviderServices.Instance.GetProviderManifest("2008").GetStoreTypes().Where(t => t.Name != "hierarchyid").Select(t => t.Name),
                    typeMap.Keys);

                Assert.False(typeMap.Any(t => t.Key != t.Value.Name));
            }
        }

        [Fact]
        public void XNamespace_returns_element_namespace_if_element_not_null()
        {
            var element = new XElement("{urn:tempuri}element");
            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var entityDesignArtifactMock = new Mock<EntityDesignArtifact>(modelManager, new Uri("urn:dummy"), modelProvider);
            entityDesignArtifactMock.Setup(a => a.SchemaVersion).Returns(EntityFrameworkVersion.Version3);

            using (var storageModel = new StorageEntityModel(entityDesignArtifactMock.Object, element))
            {
                Assert.Same(element.Name.Namespace, storageModel.XNamespace);
            }
        }

        [Fact]
        public void XNamespace_returns_root_namespace_if_element_null()
        {
            var tmpElement = new XElement("{http://schemas.microsoft.com/ado/2009/11/edm/ssdl}Schema");

            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var enityDesignArtifiact =
                new Mock<EntityDesignArtifact>(modelManager, new Uri("urn:dummy"), modelProvider)
                    {
                        CallBase = true
                    }.Object;

            enityDesignArtifiact.SetXObject(
                XDocument.Parse("<Edmx xmlns=\"http://schemas.microsoft.com/ado/2009/11/edmx\" />"));

            using (var storageModel = new StorageEntityModel(enityDesignArtifiact, tmpElement))
            {
                storageModel.SetXObject(null);
                Assert.Equal("http://schemas.microsoft.com/ado/2009/11/edm/ssdl", storageModel.XNamespace);

                // resetting the element is required for clean up
                storageModel.SetXObject(tmpElement);
            }
        }

        [Fact]
        public void GetStoragePrimitiveType_returns_type_name_for_valid_type()
        {
            var ssdl =
                XElement.Parse(
                    "<Schema Namespace=\"Model.Store\" Provider=\"System.Data.SqlClient\" ProviderManifestToken=\"2008\" Alias=\"Self\" xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm/ssdl\" />");

            using (var storageModel = new StorageEntityModel(null, ssdl))
            {
                Assert.Equal(
                    "tinyint",
                    storageModel.GetStoragePrimitiveType("tinyint").Name);
            }
        }

        [Fact]
        public void GetStoragePrimitiveType_returns_null_for_unknown_type()
        {
            var ssdl =
                XElement.Parse(
                    "<Schema Namespace=\"Model.Store\" Provider=\"System.Data.SqlClient\" ProviderManifestToken=\"2008\" Alias=\"Self\" xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm/ssdl\" />");

            using (var storageModel = new StorageEntityModel(null, ssdl))
            {
                Assert.Null(storageModel.GetStoragePrimitiveType("foo"));
            }
        }
    }
}
