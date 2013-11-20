// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using Microsoft.Data.Entity.Design.Extensibility;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.VisualStudio.Model;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Moq;
    using System;
    using Xunit;

    public class EntityDesignDocumentFrameMgrTests
    {
        [Fact]
        public void OnBeforeLastDesignerDocumentUnlock_does_not_try_to_unload_artifact_it_does_not_own()
        {
            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var mockVsArtifact =
                new Mock<VSArtifact>(modelManager, new Uri("c:\\artifact.edmx"), modelProvider) { CallBase = true };

            var mockLayerManager = new Mock<LayerManager>(mockVsArtifact.Object);

            mockVsArtifact.Setup(m => m.LayerManager).Returns(mockLayerManager.Object);

            using (var frameMgrMock = new EntityDesignDocumentFrameMgrTestDouble { Artifact = mockVsArtifact.Object })
            {
                frameMgrMock.OnBeforeLastDocumentUnlockInvoker(new Uri("urn:dummy"));
            }

            mockLayerManager.Verify(m => m.Unload(), Times.Never());
        }

        [Fact]
        public void OnBeforeLastDesignerDocumentUnlock_unloads_artifact_it_not_owns()
        {
            var artifactUri = new Uri("c:\\artifact.edmx");

            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var mockVsArtifact =
                new Mock<VSArtifact>(modelManager, artifactUri, modelProvider) { CallBase = true };

            var mockLayerManager = new Mock<LayerManager>(mockVsArtifact.Object);

            mockVsArtifact.Setup(m => m.LayerManager).Returns(mockLayerManager.Object);

            using (var frameMgrMock = new EntityDesignDocumentFrameMgrTestDouble { Artifact = mockVsArtifact.Object })
            {
                frameMgrMock.OnBeforeLastDocumentUnlockInvoker(artifactUri);
            }

            mockLayerManager.Verify(m => m.Unload(), Times.Once());
        }

        private class EntityDesignDocumentFrameMgrTestDouble : EntityDesignDocumentFrameMgr
        {
            public EFArtifact Artifact;

            public EntityDesignDocumentFrameMgrTestDouble()
                : base(new Mock<IXmlDesignerPackage>().Object)
            {
                
            }

            internal override EFArtifact CurrentArtifact { get { return Artifact; } }

            public void OnBeforeLastDocumentUnlockInvoker(Uri uri)
            {
                OnBeforeLastDesignerDocumentUnlock(uri);
            }
        }
    }
}
