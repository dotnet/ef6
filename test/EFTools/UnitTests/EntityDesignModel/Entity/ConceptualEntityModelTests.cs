// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Moq;
    using Xunit;

    public class ConceptualEntityModelTests
    {
        [Fact]
        public void XNamespace_returns_element_namespace_if_element_not_null()
        {
            var element = new XElement("{urn:tempuri}element");
            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var entityDesignArtifactMock = new Mock<EntityDesignArtifact>(modelManager, new Uri("urn:dummy"), modelProvider);
            entityDesignArtifactMock.Setup(a => a.SchemaVersion).Returns(EntityFrameworkVersion.Version3);

            using (var conceptualModel = new ConceptualEntityModel(entityDesignArtifactMock.Object, element))
            {
                Assert.Same(element.Name.Namespace, conceptualModel.XNamespace);
            }
        }

        [Fact]
        public void XNamespace_returns_root_namespace_if_element_null()
        {
            var tmpElement = new XElement("{http://schemas.microsoft.com/ado/2009/11/edm}Schema");

            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var entityDesignArtifiact =
                new Mock<EntityDesignArtifact>(modelManager, new Uri("urn:dummy"), modelProvider)
                    {
                        CallBase = true
                    }.Object;

            entityDesignArtifiact.SetXObject(
                XDocument.Parse("<Edmx xmlns=\"http://schemas.microsoft.com/ado/2009/11/edmx\" />"));

            using (var conceptualModel = new ConceptualEntityModel(entityDesignArtifiact, tmpElement))
            {
                conceptualModel.SetXObject(null);
                Assert.Equal("http://schemas.microsoft.com/ado/2009/11/edm", conceptualModel.XNamespace);

                // resetting the element is required for clean up
                conceptualModel.SetXObject(tmpElement);
            }
        }
    }
}
