// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Moq;
    using Xunit;

    public class InMemoryModelBuilerEngineTests
    {
        [Fact]
        public void XDocument_uses_latest_value_of_schema_version_when_invoked_the_first_time()
        {
            const string edmxString = "<edmx xmlns=\"http://schemas.microsoft.com/ado/2009/11/edmx\" />";
            var settings = new ModelBuilderSettings { TargetSchemaVersion = EntityFrameworkVersion.Version1 };
            var mockContentsFactory = new Mock<IInitialModelContentsFactory>();
            mockContentsFactory.Setup(f => f.GetInitialModelContents(It.IsAny<Version>())).Returns(edmxString);

            var builderEngine = new InMemoryModelBuilderEngine(
                new Mock<ModelBuilderEngineHostContext>().Object, settings,
                mockContentsFactory.Object, new Uri("http://tempUri"));

            settings.TargetSchemaVersion = EntityFrameworkVersion.Version3;

            Assert.True(XNode.DeepEquals(XDocument.Parse(edmxString), builderEngine.XDocument));
            mockContentsFactory.Verify(f => f.GetInitialModelContents(It.IsAny<Version>()), Times.Once());
            mockContentsFactory.Verify(f => f.GetInitialModelContents(EntityFrameworkVersion.Version3), Times.Once());
        }

        [Fact]
        public void InMemoryModelBuilerEngine_caches_XDocument()
        {
            const string edmxString = "<edmx xmlns=\"http://schemas.microsoft.com/ado/2009/11/edmx\" />";
            var settings = new ModelBuilderSettings { TargetSchemaVersion = EntityFrameworkVersion.Version1 };
            var mockContentsFactory = new Mock<IInitialModelContentsFactory>();
            mockContentsFactory.Setup(f => f.GetInitialModelContents(It.IsAny<Version>())).Returns(edmxString);

            var builderEngine = new InMemoryModelBuilderEngine(
                new Mock<ModelBuilderEngineHostContext>().Object, settings,
                mockContentsFactory.Object, new Uri("http://tempUri"));

            settings.TargetSchemaVersion = EntityFrameworkVersion.Version3;
            Assert.True(XNode.DeepEquals(XDocument.Parse(edmxString), builderEngine.XDocument));
            Assert.True(XNode.DeepEquals(XDocument.Parse(edmxString), builderEngine.XDocument));

            mockContentsFactory.Verify(f => f.GetInitialModelContents(It.IsAny<Version>()), Times.Once());
            mockContentsFactory.Verify(f => f.GetInitialModelContents(EntityFrameworkVersion.Version3), Times.Once());
        }
    }
}
