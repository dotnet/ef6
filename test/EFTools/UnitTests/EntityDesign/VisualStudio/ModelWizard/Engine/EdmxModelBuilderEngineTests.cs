// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Xunit;

    public class EdmxModelBuilderEngineTests
    {
        public class ProcessModelTests
        {
            [Fact]
            public void ProcessModel_invokes_initial_contents_from_factory()
            {
                var modelGenCache = new ModelGenErrorCache();
                var mockPackage = new Mock<IEdmPackage>();
                mockPackage.Setup(p => p.ModelGenErrorCache).Returns(modelGenCache);

                PackageManager.Package = mockPackage.Object;

                var mockInitialContentsFactory = new Mock<IInitialModelContentsFactory>();
                mockInitialContentsFactory
                    .Setup(f => f.GetInitialModelContents(It.IsAny<Version>()))
                    .Returns(new InitialModelContentsFactory().GetInitialModelContents(EntityFrameworkVersion.Version3));

                var mockModelBuilderEngine = new Mock<EdmxModelBuilderEngine>(mockInitialContentsFactory.Object) { CallBase = true };

                mockModelBuilderEngine
                    .Setup(m => m.GenerateModels(It.IsAny<string>(), It.IsAny<ModelBuilderSettings>(), It.IsAny<List<EdmSchemaError>>()))
                    .Returns(new DbModel(new DbProviderInfo("System.Data.SqlClient", "2008"), Mock.Of<DbProviderManifest>()));

                mockModelBuilderEngine.Object.GenerateModel(CreateMockModelBuilderSettings().Object, 
                    Mock.Of<IVsUtils>(), Mock.Of<ModelBuilderEngineHostContext>());

                mockInitialContentsFactory.Verify(f => f.GetInitialModelContents(It.IsAny<Version>()), Times.Once());
            }


            [Fact]
            public void ProcessModel_calls_into_model_generation_extension_dispatcher()
            {
                var modelGenCache = new ModelGenErrorCache();
                var mockPackage = new Mock<IEdmPackage>();
                mockPackage.Setup(p => p.ModelGenErrorCache).Returns(modelGenCache);

                PackageManager.Package = mockPackage.Object;

                var mockInitialContentsFactory = new Mock<IInitialModelContentsFactory>();
                mockInitialContentsFactory
                    .Setup(f => f.GetInitialModelContents(It.IsAny<Version>()))
                    .Returns(new InitialModelContentsFactory().GetInitialModelContents(EntityFrameworkVersion.Version3));

                var settings = CreateMockModelBuilderSettings().Object;
                var mockHostContext = new Mock<VSModelBuilderEngineHostContext>(settings);

                var mockModelBuilderEngine = new Mock<EdmxModelBuilderEngine>(mockInitialContentsFactory.Object) { CallBase = true };

                mockModelBuilderEngine
                    .Setup(m => m.GenerateModels(It.IsAny<string>(), It.IsAny<ModelBuilderSettings>(), It.IsAny<List<EdmSchemaError>>()))
                    .Returns(new DbModel(new DbProviderInfo("System.Data.SqlClient", "2008"), Mock.Of<DbProviderManifest>()));

                mockModelBuilderEngine.Object.GenerateModel(settings, Mock.Of<IVsUtils>(), mockHostContext.Object);

                mockHostContext.Verify(h => h.DispatchToModelGenerationExtensions(), Times.Once());
            }

            [Fact]
            public void ProcessModel_serializes_model_to_edmx_and_adds_designer_otions()
            {
                var modelGenCache = new ModelGenErrorCache();
                var mockPackage = new Mock<IEdmPackage>();
                mockPackage.Setup(p => p.ModelGenErrorCache).Returns(modelGenCache);

                PackageManager.Package = mockPackage.Object;

                var mockInitialContentsFactory = new Mock<IInitialModelContentsFactory>();
                mockInitialContentsFactory
                    .Setup(f => f.GetInitialModelContents(It.IsAny<Version>()))
                    .Returns(new InitialModelContentsFactory().GetInitialModelContents(EntityFrameworkVersion.Version3));

                var settings = CreateMockModelBuilderSettings().Object;

                var mockModelBuilderEngine = new Mock<EdmxModelBuilderEngine>(mockInitialContentsFactory.Object) { CallBase = true };

                mockModelBuilderEngine
                    .Setup(m => m.GenerateModels(It.IsAny<string>(), It.IsAny<ModelBuilderSettings>(), It.IsAny<List<EdmSchemaError>>()))
                    .Returns(new DbModel(new DbProviderInfo("System.Data.SqlClient", "2008"), Mock.Of<DbProviderManifest>()));

                mockModelBuilderEngine.Object.GenerateModel(settings, Mock.Of<IVsUtils>(), Mock.Of<ModelBuilderEngineHostContext>());

                var edmx = mockModelBuilderEngine.Object.Edmx;

                Assert.NotEmpty(
                    edmx
                        .Descendants().Where(e => e.Name.LocalName == "StorageModels")
                        .Elements().Where(e => e.Name.LocalName == "Schema"));

                Assert.NotEmpty(
                    edmx
                        .Descendants().Where(e => e.Name.LocalName == "ConceptualModels")
                        .Elements().Where(e => e.Name.LocalName == "Schema"));

                Assert.NotEmpty(
                    edmx
                        .Descendants().Where(e => e.Name.LocalName == "Mappings")
                        .Elements().Where(e => e.Name.LocalName == "Mapping"));

                Assert.Equal(
                    3,
                    edmx
                        .Descendants().Where(e => e.Name.LocalName == "DesignerInfoPropertySet")
                        .Elements().Count(e => e.Name.LocalName == "DesignerProperty"));
            }

            private static Mock<ModelBuilderSettings> CreateMockModelBuilderSettings()
            {
                var mockModelBuilderSettings = new Mock<ModelBuilderSettings>();
                mockModelBuilderSettings.Object.GenerationOption = ModelGenerationOption.GenerateFromDatabase;
                mockModelBuilderSettings.Object.ModelPath = Path.Combine(Directory.GetCurrentDirectory(), "temp.edmx");
                mockModelBuilderSettings
                    .Setup(p => p.DesignTimeConnectionString)
                    .Returns("fakeConnectionString");
                
                return mockModelBuilderSettings;
            }
        }

        public class UpdateDesignerInfoTests
        {
            private class EdmxModelBuilderEngineFake : EdmxModelBuilderEngine
            {
                public EdmxModelBuilderEngineFake()
                    :base(Mock.Of<IInitialModelContentsFactory>())
                {}

                internal void UpdateDesignerInfoInvoker(EdmxHelper edmxHelper, ModelBuilderSettings settings)
                {
                    UpdateDesignerInfo(edmxHelper, settings);
                }
            }

            [Fact]
            public void UpdateDesignerInfo_updates_properties_in_designer_section()
            {
                var mockEdmxHelper = new Mock<EdmxHelper>(new XDocument());
                new EdmxModelBuilderEngineFake().UpdateDesignerInfoInvoker(mockEdmxHelper.Object, new ModelBuilderSettings());

                mockEdmxHelper
                    .Verify(h => h.UpdateDesignerOptionProperty(It.IsAny<string>(), It.IsAny<bool>()), Times.Exactly(3));

                mockEdmxHelper
                    .Verify(
                        h => h.UpdateDesignerOptionProperty(
                            OptionsDesignerInfo.AttributeEnablePluralization, It.IsAny<bool>()), Times.Once());

                mockEdmxHelper
                    .Verify(
                        h => h.UpdateDesignerOptionProperty(
                            OptionsDesignerInfo.AttributeIncludeForeignKeysInModel, It.IsAny<bool>()), Times.Once());

                mockEdmxHelper
                    .Verify(
                        h => h.UpdateDesignerOptionProperty(
                            OptionsDesignerInfo.AttributeUseLegacyProvider, It.IsAny<bool>()), Times.Once());
            }
        }
    }
}
