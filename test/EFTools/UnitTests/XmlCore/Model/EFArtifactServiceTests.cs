// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Moq;
    using Xunit;

    public class EFArtifactServiceTests
    {
        [Fact]
        public void EFArtifactService_returns_artifact_passed_in_ctor()
        {
            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var artifact = new Mock<EFArtifact>(modelManager, new Uri("urn:dummy"), modelProvider).Object;

            Assert.Same(artifact, new EFArtifactService(artifact).Artifact);
        }
    }
}
