// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Moq;
    using Xunit;

    public class EntityDesignArtifactTests
    {
        [Fact]
        public void GetModelGenErrors_on_EntityDesignArtifact_always_returns_null()
        {
            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            using (var artifact = new EntityDesignArtifact(modelManager, new Uri("urn:dummy"), modelProvider))
            {
                Assert.Null(artifact.GetModelGenErrors());
            }
        }

        [Fact]
        public void DetermineIfArtifactIsVersionSafe_sets_IsVersionSafe_to_true_if_namespaces_in_sync()
        {
            Assert.True(
                DetermineIfArtifactIsVersionSafe(
                    edmxVersion: EntityFrameworkVersion.Version1,
                    storeModelVersion: EntityFrameworkVersion.Version1,
                    conceptualModelVersion: EntityFrameworkVersion.Version1,
                    mappingModelVersion: EntityFrameworkVersion.Version1));

            Assert.True(
                DetermineIfArtifactIsVersionSafe(
                    edmxVersion: EntityFrameworkVersion.Version2,
                    storeModelVersion: EntityFrameworkVersion.Version2,
                    conceptualModelVersion: EntityFrameworkVersion.Version2,
                    mappingModelVersion: EntityFrameworkVersion.Version2));

            Assert.True(
                DetermineIfArtifactIsVersionSafe(
                    edmxVersion: EntityFrameworkVersion.Version3,
                    storeModelVersion: EntityFrameworkVersion.Version3,
                    conceptualModelVersion: EntityFrameworkVersion.Version3,
                    mappingModelVersion: EntityFrameworkVersion.Version3));
        }

        [Fact]
        public void DetermineIfArtifactIsVersionSafe_sets_IsVersionSafe_to_false_if_namespaces_not_in_sync()
        {
            Assert.False(
                DetermineIfArtifactIsVersionSafe(
                    edmxVersion: EntityFrameworkVersion.Version3,
                    storeModelVersion: EntityFrameworkVersion.Version1,
                    conceptualModelVersion: EntityFrameworkVersion.Version1,
                    mappingModelVersion: EntityFrameworkVersion.Version1));

            Assert.False(
                DetermineIfArtifactIsVersionSafe(
                    edmxVersion: EntityFrameworkVersion.Version2,
                    storeModelVersion: EntityFrameworkVersion.Version3,
                    conceptualModelVersion: EntityFrameworkVersion.Version2,
                    mappingModelVersion: EntityFrameworkVersion.Version2));

            Assert.False(
                DetermineIfArtifactIsVersionSafe(
                    edmxVersion: EntityFrameworkVersion.Version3,
                    storeModelVersion: EntityFrameworkVersion.Version3,
                    conceptualModelVersion: EntityFrameworkVersion.Version2,
                    mappingModelVersion: EntityFrameworkVersion.Version3));

            Assert.False(
                DetermineIfArtifactIsVersionSafe(
                    edmxVersion: EntityFrameworkVersion.Version3,
                    storeModelVersion: EntityFrameworkVersion.Version3,
                    conceptualModelVersion: EntityFrameworkVersion.Version2,
                    mappingModelVersion: EntityFrameworkVersion.Version2));
        }

        [Fact]
        public void DetermineIfArtifactIsVersionSafe_sets_IsVersionSafe_to_true_if_models_not_set()
        {
            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var mockEntityDesignArtifact =
                new Mock<EntityDesignArtifact>(modelManager, new Uri("urn:dummy"), modelProvider) { CallBase = true };

            mockEntityDesignArtifact
                .Setup(a => a.XDocument)
                .Returns(
                    new XDocument(
                        new XElement(
                            XName.Get(
                                "Edmx",
                                SchemaManager.GetEDMXNamespaceName(EntityFrameworkVersion.Version3)))));

            mockEntityDesignArtifact.Setup(m => m.ConceptualModel).Returns((ConceptualEntityModel)null);
            mockEntityDesignArtifact.Setup(m => m.StorageModel).Returns((StorageEntityModel)null);
            mockEntityDesignArtifact.Setup(m => m.MappingModel).Returns((MappingModel)null);

            var artifact = mockEntityDesignArtifact.Object;
            artifact.DetermineIfArtifactIsVersionSafe();

            Assert.True(artifact.IsVersionSafe);
        }

        private static bool DetermineIfArtifactIsVersionSafe(
            Version edmxVersion, Version storeModelVersion, Version conceptualModelVersion, Version mappingModelVersion)
        {
            var artifact = SetupArtifact(edmxVersion, storeModelVersion, conceptualModelVersion, mappingModelVersion);
            artifact.DetermineIfArtifactIsVersionSafe();
            return artifact.IsVersionSafe;
        }

        private static EntityDesignArtifact SetupArtifact(
            Version edmxVersion, Version storeModelVersion, Version conceptualModelVersion, Version mappingModelVersion)
        {
            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var mockEntityDesignArtifact =
                new Mock<EntityDesignArtifact>(modelManager, new Uri("urn:dummy"), modelProvider) { CallBase = true };

            var mockConceptualModel =
                new Mock<ConceptualEntityModel>(
                    mockEntityDesignArtifact.Object,
                    new XElement(XName.Get("Schema", SchemaManager.GetCSDLNamespaceName(conceptualModelVersion))));

            mockConceptualModel
                .Setup(m => m.XNamespace)
                .Returns(SchemaManager.GetCSDLNamespaceName(conceptualModelVersion));

            mockEntityDesignArtifact
                .Setup(m => m.ConceptualModel)
                .Returns(mockConceptualModel.Object);

            var mockStoreModel =
                new Mock<StorageEntityModel>(
                    mockEntityDesignArtifact.Object,
                    new XElement(XName.Get("Schema", SchemaManager.GetSSDLNamespaceName(storeModelVersion))));

            mockStoreModel
                .Setup(m => m.XNamespace)
                .Returns(SchemaManager.GetSSDLNamespaceName(storeModelVersion));

            mockEntityDesignArtifact
                .Setup(m => m.StorageModel)
                .Returns(mockStoreModel.Object);

            var mockMappingModel =
                new Mock<MappingModel>(
                    mockEntityDesignArtifact.Object,
                    new XElement(XName.Get("Mapping", SchemaManager.GetMSLNamespaceName(mappingModelVersion))));

            mockMappingModel
                .Setup(m => m.XNamespace)
                .Returns(SchemaManager.GetMSLNamespaceName(mappingModelVersion));

            mockEntityDesignArtifact
                .Setup(m => m.MappingModel)
                .Returns(mockMappingModel.Object);

            mockEntityDesignArtifact
                .Setup(a => a.XDocument)
                .Returns(
                    new XDocument(
                        new XElement(XName.Get("Edmx", SchemaManager.GetEDMXNamespaceName(edmxVersion)))));

            return mockEntityDesignArtifact.Object;
        }
    }
}
