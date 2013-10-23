// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.View
{
    using System;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Visitor;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Moq;
    using Xunit;

    public class EntityDesignerDiagramTests
    {
        [Fact]
        public void ReversionModel_converts_model_namespaces_to_target_schema_version()
        {
            var model = CreateModel(EntityFrameworkVersion.Version3);

            var tempUri = new Uri("http://tempuri");

            var mockModelManager =
                new Mock<ModelManager>(new Mock<IEFArtifactFactory>().Object, new Mock<IEFArtifactSetFactory>().Object);

            using (var modelManager = mockModelManager.Object)
            {
                var mockModelProvider = new Mock<XmlModelProvider>();
                mockModelProvider
                    .Setup(p => p.BeginTransaction(It.IsAny<string>(), It.IsAny<object>()))
                    .Returns(new Mock<XmlTransaction>().Object);

                var mockFrameManager = new Mock<DocumentFrameMgr>(new Mock<IXmlDesignerPackage>().Object);
                var mockPackage = new Mock<IEdmPackage>();
                mockPackage.Setup(p => p.DocumentFrameMgr).Returns(mockFrameManager.Object);

                var mockEditingContext = new Mock<EditingContext>();
                var mockArtifact = new Mock<EntityDesignArtifact>(modelManager, tempUri, mockModelProvider.Object);
                mockArtifact.Setup(a => a.CanEditArtifact()).Returns(true);
                mockArtifact.Setup(a => a.XDocument).Returns(model);

#if DEBUG
                mockArtifact
                    .Setup(
                        a => a.GetVerifyModelIntegrityVisitor(
                            It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Returns(new Mock<VerifyModelIntegrityVisitor>().Object);
#endif

                Assert.False(mockArtifact.Object.IsDirty);

                EntityDesignerDiagram.ReversionModel(
                    mockPackage.Object, mockEditingContext.Object, mockArtifact.Object, EntityFrameworkVersion.Version2);

                Assert.True(mockArtifact.Object.IsDirty);
                Assert.True(XNode.DeepEquals(CreateModel(EntityFrameworkVersion.Version2), model));
                mockFrameManager.Verify(m => m.SetCurrentContext(mockEditingContext.Object), Times.Once());
            }
        }

        private static XDocument CreateModel(Version targetSchemaVersion)
        {
            const string edmxTemplate =
                @"<!-- edmx -->
<edmx:Edmx Version=""{0}"" xmlns:edmx=""{1}"">
  <edmx:Runtime>
    <edmx:StorageModels>
      <Schema xmlns=""{2}"" />
    </edmx:StorageModels>
    <edmx:ConceptualModels>
      <Schema xmlns=""{3}"" />
    </edmx:ConceptualModels>
    <!-- C-S mapping content -->
    <edmx:Mappings>
      <Mapping xmlns=""{4}"" />
    </edmx:Mappings>
  </edmx:Runtime>
</edmx:Edmx>";

            return
                XDocument.Parse(
                    string.Format(
                        edmxTemplate,
                        targetSchemaVersion.ToString(2),
                        SchemaManager.GetEDMXNamespaceName(targetSchemaVersion),
                        SchemaManager.GetCSDLNamespaceName(targetSchemaVersion),
                        SchemaManager.GetSSDLNamespaceName(targetSchemaVersion),
                        SchemaManager.GetMSLNamespaceName(targetSchemaVersion)));
        }
    }
}
