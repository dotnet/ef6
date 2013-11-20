// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class EditingContextManagerTests
    {
        [Fact]
        public void Verify_CloseArtifact_removes_artifact_from_model_manager()
        {
            var artifactUri = new Uri("c:\\artifact.edmx");
            var mockModelManager = new Mock<ModelManager>(null, null) { CallBase = true };
            var mockArtifact = new Mock<EFArtifact>(mockModelManager.Object, artifactUri, new Mock<XmlModelProvider>().Object);
            mockArtifact.Setup(a => a.Uri).Returns(artifactUri);

            mockModelManager
                .Setup(m => m.GetNewOrExistingArtifact(artifactUri, It.IsAny<XmlModelProvider>()))
                .Returns(mockArtifact.Object);
            mockModelManager
                .Setup(m => m.GetArtifact(artifactUri))
                .Returns(mockArtifact.Object);

            var mockPackage = new Mock<IXmlDesignerPackage>();
            mockPackage.Setup(p => p.ModelManager).Returns(mockModelManager.Object);

            var mockEditingContextMgr = new Mock<EditingContextManager>(mockPackage.Object) { CallBase = true };
            mockEditingContextMgr
                .Protected()
                .Setup<EFArtifact>("GetNewOrExistingArtifact", artifactUri)
                .Returns(mockArtifact.Object);

            var editingContext = mockEditingContextMgr.Object.GetNewOrExistingContext(artifactUri);
            
            Assert.NotNull(editingContext.GetEFArtifactService());
            mockEditingContextMgr.Object.CloseArtifact(artifactUri);
            mockModelManager.Verify(m => m.ClearArtifact(artifactUri), Times.Once());
            Assert.Null(editingContext.GetEFArtifactService());
        }
    }
}
