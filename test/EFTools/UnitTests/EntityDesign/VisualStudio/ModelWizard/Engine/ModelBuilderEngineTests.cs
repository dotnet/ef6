// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Moq;
    using Moq.Protected;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Xunit;

    public class ModelBuilderEngineTests
    {
        public class GenerateModelTests
        {
            [Fact]
            public void Public_GenerateModel_calls_into_GenerateModel_that_does_work()
            {
                var mockModelBuilderSettings = new Mock<ModelBuilderSettings>();
                mockModelBuilderSettings.Object.GenerationOption = ModelGenerationOption.GenerateFromDatabase;
                mockModelBuilderSettings
                    .Setup(p => p.DesignTimeConnectionString)
                    .Returns("fakeConnectionString");

                var mockModelBuilderEngine = new Mock<ModelBuilderEngine> { CallBase = true };
                mockModelBuilderEngine.Setup(m => m.Model).Returns(new XDocument());
                mockModelBuilderEngine.Setup(
                    m => m.GenerateModel(
                        It.IsAny<EdmxHelper>(), It.IsAny<ModelBuilderSettings>(), It.IsAny<ModelBuilderEngineHostContext>()));

                mockModelBuilderEngine.Object.GenerateModel(mockModelBuilderSettings.Object);

                mockModelBuilderEngine.Verify(
                    m => m.GenerateModel(
                        It.IsAny<EdmxHelper>(), It.IsAny<ModelBuilderSettings>(), It.IsAny<ModelBuilderEngineHostContext>()),
                        Times.Once());
            }

            [Fact]
            public void GenerateModel_generates_the_model()
            {
                var mockModelGenCache = new Mock<ModelGenErrorCache>();
                var mockPackage = new Mock<IEdmPackage>();
                mockPackage.Setup(p => p.ModelGenErrorCache).Returns(mockModelGenCache.Object);
                PackageManager.Package = mockPackage.Object;

                var mockModelBuilderSettings = CreateMockModelBuilderSettings();
                mockModelBuilderSettings.Object.ModelNamespace = "myModel";

                var mockHostContext = new Mock<VSModelBuilderEngineHostContext>(mockModelBuilderSettings.Object);

                var mockModelBuilderEngine = new Mock<ModelBuilderEngine> { CallBase = true };

                mockModelBuilderEngine
                    .Setup(m => m.GenerateModels(It.IsAny<string>(), It.IsAny<ModelBuilderSettings>(), It.IsAny<List<EdmSchemaError>>()))
                    .Returns(new DbModel(new DbProviderInfo("System.Data.SqlClient", "2008"), Mock.Of<DbProviderManifest>()));

                var mockEdmxHelper = new Mock<EdmxHelper>(new XDocument());
                mockEdmxHelper.Setup(
                    h => h.UpdateStorageModels(
                        It.IsAny<EdmModel>(),
                        It.IsAny<string>(),
                        It.IsAny<DbProviderInfo>(),
                        It.IsAny<List<EdmSchemaError>>()))
                    .Returns(true);
                mockEdmxHelper.Setup(
                    h => h.UpdateConceptualModels(
                        It.IsAny<EdmModel>(),
                        It.Is<string>(n => n == "myModel")))
                    .Returns(true);
                mockEdmxHelper.Setup(
                    h => h.UpdateMapping(
                        It.IsAny<DbModel>()))
                    .Returns(true);

                mockModelBuilderEngine.Object
                    .GenerateModel(mockEdmxHelper.Object, mockModelBuilderSettings.Object, mockHostContext.Object);

                mockModelBuilderEngine
                    .Verify(m => m.GenerateModels(
                        It.IsAny<string>(), It.IsAny<ModelBuilderSettings>(), It.IsAny<List<EdmSchemaError>>()), Times.Once());

                mockHostContext.Verify(h => h.DispatchToModelGenerationExtensions(), Times.Once());
                mockModelBuilderEngine
                    .Protected()
                    .Verify("UpdateDesignerInfo", Times.Once(), ItExpr.IsAny<EdmxHelper>(), ItExpr.IsAny<ModelBuilderSettings>());
                mockHostContext.Verify(h => h.LogMessage(It.IsAny<string>()), Times.Exactly(3));
                mockHostContext
                    .Verify(h => h.LogMessage(It.IsRegex(Resources.Engine_ModelGenSuccess)), Times.Once());
                mockHostContext
                    .Verify(h => h.LogMessage(It.IsRegex(Resources.Engine_ModelGenException)), Times.Never());

                mockEdmxHelper.Verify(
                    e => e.UpdateConceptualModels(
                        It.IsAny<EdmModel>(), It.Is<string>(n => n == "myModel")), Times.Once());

                mockModelGenCache.Verify(
                    c => c.AddErrors(It.IsAny<string>(), It.IsAny<List<EdmSchemaError>>()), Times.Never());
            }

            [Fact]
            public void GenerateModel_writes_errors_if_any_returned_from_model_generation()
            {
                var modelGenCache = new ModelGenErrorCache();
                var mockPackage = new Mock<IEdmPackage>();
                mockPackage.Setup(p => p.ModelGenErrorCache).Returns(modelGenCache);

                PackageManager.Package = mockPackage.Object;
                var mockHostContext = new Mock<ModelBuilderEngineHostContext>();
                var mockModelBuilderSettings = CreateMockModelBuilderSettings();
                var mockModelBuilderEngine = new Mock<ModelBuilderEngine> { CallBase = true };

                var error = new EdmSchemaError("testError", 42, EdmSchemaErrorSeverity.Warning);

                mockModelBuilderEngine
                    .Setup(m => m.GenerateModels(It.IsAny<string>(), It.IsAny<ModelBuilderSettings>(), It.IsAny<List<EdmSchemaError>>()))
                    .Callback(
                        (string storeModelNamespace, ModelBuilderSettings settings, List<EdmSchemaError> errors) => errors.Add(error))
                    .Returns(new DbModel(new DbProviderInfo("System.Data.SqlClient", "2008"), Mock.Of<DbProviderManifest>()));

                var mockEdmxHelper = new Mock<EdmxHelper>(new XDocument());

                mockModelBuilderEngine.Object.GenerateModel(mockEdmxHelper.Object, mockModelBuilderSettings.Object, mockHostContext.Object);

                mockModelBuilderEngine
                    .Verify(m => m.GenerateModels(
                        It.IsAny<string>(), It.IsAny<ModelBuilderSettings>(), It.IsAny<List<EdmSchemaError>>()), Times.Once());

                mockHostContext.Verify(h => h.DispatchToModelGenerationExtensions(), Times.Once());
                mockModelBuilderEngine
                    .Protected()
                    .Verify("UpdateDesignerInfo", Times.Once(), ItExpr.IsAny<EdmxHelper>(), ItExpr.IsAny<ModelBuilderSettings>());
                mockHostContext.Verify(h => h.LogMessage(It.IsAny<string>()), Times.Exactly(3));
                mockHostContext
                    .Verify(h => h.LogMessage(It.IsRegex(Resources.Engine_ModelGenErrors.Substring(1, 20))), Times.Once());
                mockHostContext
                    .Verify(h => h.LogMessage(It.IsRegex(Resources.Engine_ModelGenException)), Times.Never());

                Assert.Same(error, modelGenCache.GetErrors(mockModelBuilderSettings.Object.ModelPath).Single());
            }

            [Fact]
            public void GenerateModel_logs_exception()
            {
                var mockPackage = new Mock<IEdmPackage>();
                mockPackage.Setup(p => p.ModelGenErrorCache).Returns(new ModelGenErrorCache());
                PackageManager.Package = mockPackage.Object;

                var mockHostContext = new Mock<ModelBuilderEngineHostContext>();
                var mockModelBuilderSettings = CreateMockModelBuilderSettings();
                var mockModelBuilderEngine = new Mock<ModelBuilderEngine> { CallBase = true };

                mockModelBuilderEngine
                    .Setup(m => m.GenerateModels(It.IsAny<string>(), It.IsAny<ModelBuilderSettings>(), It.IsAny<List<EdmSchemaError>>()))
                    .Callback(
                        (string storeModelNamespace, ModelBuilderSettings settings, List<EdmSchemaError> errors) =>
                        {
                            throw new Exception("Test exception");
                        });

                var mockEdmxHelper = new Mock<EdmxHelper>(new XDocument());

                mockModelBuilderEngine.Object
                    .GenerateModel(mockEdmxHelper.Object, mockModelBuilderSettings.Object, mockHostContext.Object);

                mockHostContext.Verify(h => h.LogMessage(It.IsAny<string>()), Times.Exactly(3));
                mockHostContext
                    .Verify(h => h.LogMessage(It.IsRegex(Resources.Engine_ModelGenException)), Times.Once());
                mockHostContext
                    .Verify(h => h.LogMessage(It.IsRegex(Resources.Engine_ModelGenSuccess)), Times.Never());
            }

            [Fact]
            public void GenerateModel_clears_ModelGenErrorCache()
            {
                var mockModelGenCache = new Mock<ModelGenErrorCache>();
                var mockPackage = new Mock<IEdmPackage>();
                mockPackage.Setup(p => p.ModelGenErrorCache).Returns(mockModelGenCache.Object);
                PackageManager.Package = mockPackage.Object;

                var mockHostContext = new Mock<ModelBuilderEngineHostContext>();
                var mockModelBuilderSettings = CreateMockModelBuilderSettings();
                var mockModelBuilderEngine = new Mock<ModelBuilderEngine>{CallBase = true};

                mockModelBuilderEngine.Object
                    .GenerateModel(new EdmxHelper(new XDocument()), mockModelBuilderSettings.Object, mockHostContext.Object);

                mockModelGenCache.Verify(c => c.RemoveErrors(It.IsAny<string>()), Times.Once());
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
            private class ModelBuilderEngineFake : ModelBuilderEngine
            {
                #region not important

                public ModelBuilderEngineFake()
                {
                }

                internal override XDocument Model
                {
                    get { throw new NotImplementedException(); }
                }

                protected override void InitializeModelContents(Version targetSchemaVersion)
                {
                    throw new NotImplementedException();
                }

                #endregion

                internal void UpdateDesignerInfoInvoker(EdmxHelper edmxHelper, ModelBuilderSettings settings)
                {
                    UpdateDesignerInfo(edmxHelper, settings);
                }
            }

            [Fact]
            public void UpdateDesignerInfo_updates_properties_in_designer_section()
            {
                var mockEdmxHelper = new Mock<EdmxHelper>(new XDocument());
                new ModelBuilderEngineFake().UpdateDesignerInfoInvoker(mockEdmxHelper.Object, new ModelBuilderSettings());

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
