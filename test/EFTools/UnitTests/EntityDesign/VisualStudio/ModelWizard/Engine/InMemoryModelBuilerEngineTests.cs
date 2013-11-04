// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class InMemoryModelBuilerEngineTests
    {
        [Fact]
        public void InMemoryModelBuilerEngine_InitializeModelContents_initializes_model()
        {
            const string edmxString = "<edmx xmlns=\"http://schemas.microsoft.com/ado/2009/11/edmx\" />";
            var mockSettings = new Mock<ModelBuilderSettings>();
            mockSettings.Setup(s => s.DesignTimeConnectionString).Returns("fakeConnString");
            mockSettings.Object.TargetSchemaVersion = EntityFrameworkVersion.Version3;

            var mockContentsFactory = new Mock<IInitialModelContentsFactory>();
            mockContentsFactory.Setup(f => f.GetInitialModelContents(It.IsAny<Version>())).Returns(edmxString);

            var mockInMemoryBuilderEngine = new Mock<InMemoryModelBuilderEngine>(
                mockContentsFactory.Object)
            {
                CallBase = true
            };

            mockInMemoryBuilderEngine.Setup(
                e => e.GenerateModel(It.IsAny<EdmxHelper>(), It.IsAny<ModelBuilderSettings>(), It.IsAny<ModelBuilderEngineHostContext>()));

            mockInMemoryBuilderEngine.Object.GenerateModel(mockSettings.Object);

            Assert.True(XNode.DeepEquals(XDocument.Parse(edmxString), mockInMemoryBuilderEngine.Object.Model));

            mockContentsFactory.Verify(f => f.GetInitialModelContents(It.IsAny<Version>()), Times.Once());
            mockContentsFactory.Verify(f => f.GetInitialModelContents(EntityFrameworkVersion.Version3), Times.Once());

            mockInMemoryBuilderEngine.Protected().Verify("InitializeModelContents", Times.Once(), EntityFrameworkVersion.Version3);
        }
    }
}
