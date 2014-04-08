// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
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
    using Xunit;

    public class ModelBuilderEngineTests
    {
        [Fact]
        public void GenerateModel_generates_the_model_and_calls_ProcessModel()
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
                .Setup(m => m.GenerateModels("myModel.Store", It.IsAny<ModelBuilderSettings>(), It.IsAny<List<EdmSchemaError>>()))
                .Returns(new DbModel(new DbProviderInfo("System.Data.SqlClient", "2008"), Mock.Of<DbProviderManifest>()));

            mockModelBuilderEngine.Object
                .GenerateModel(mockModelBuilderSettings.Object, Mock.Of<IVsUtils>(), mockHostContext.Object);

            mockModelBuilderEngine
                .Verify(m => m.GenerateModels(
                    It.IsAny<string>(), It.IsAny<ModelBuilderSettings>(), It.IsAny<List<EdmSchemaError>>()), Times.Once());

            mockModelBuilderEngine
                .Protected()
                .Verify(
                    "ProcessModel", 
                    Times.Once(), 
                    ItExpr.IsAny<DbModel>(), ItExpr.IsAny<string>(), ItExpr.IsAny<ModelBuilderSettings>(), 
                    ItExpr.IsAny<ModelBuilderEngineHostContext>(), ItExpr.IsAny<List<EdmSchemaError>>());

            mockHostContext
                .Verify(h => h.LogMessage(It.IsRegex(Resources.Engine_ModelGenSuccess)), Times.Once());
            mockHostContext
                .Verify(h => h.LogMessage(It.IsRegex(Resources.Engine_ModelGenException)), Times.Never());

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

            mockModelBuilderEngine.Object.GenerateModel(mockModelBuilderSettings.Object, Mock.Of<IVsUtils>(), mockHostContext.Object);

            mockModelBuilderEngine
                .Verify(m => m.GenerateModels(
                    It.IsAny<string>(), It.IsAny<ModelBuilderSettings>(), It.IsAny<List<EdmSchemaError>>()), Times.Once());

            mockHostContext.Verify(h => h.LogMessage(It.IsAny<string>()), Times.Exactly(3));
            mockHostContext
                .Verify(h => h.LogMessage(It.IsRegex(Resources.Engine_ModelGenErrors.Substring(1, 20))), Times.Once());
            mockHostContext
                .Verify(h => h.LogMessage(It.IsRegex(Resources.Engine_ModelGenException)), Times.Never());

            Assert.Same(error, modelGenCache.GetErrors(mockModelBuilderSettings.Object.ModelPath).Single());
        }

        [Fact]
        public void GenerateModel_writes_errors_returned_from_ProcessModel()
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
                .Returns(new DbModel(new DbProviderInfo("System.Data.SqlClient", "2008"), Mock.Of<DbProviderManifest>()));

            mockModelBuilderEngine
                .Protected()
                .Setup(
                    "ProcessModel",
                    ItExpr.IsAny<DbModel>(), ItExpr.IsAny<string>(), ItExpr.IsAny<ModelBuilderSettings>(),
                    ItExpr.IsAny<ModelBuilderEngineHostContext>(), ItExpr.IsAny<List<EdmSchemaError>>())
                .Callback(
                    (DbModel model, string storeModelNamespace, ModelBuilderSettings settings,
                        ModelBuilderEngineHostContext hostContext, List<EdmSchemaError> errors) => errors.Add(error));

            mockModelBuilderEngine.Object.GenerateModel(mockModelBuilderSettings.Object, Mock.Of<IVsUtils>(), mockHostContext.Object);
           
            mockModelBuilderEngine
                .Verify(m => m.GenerateModels(
                    It.IsAny<string>(), It.IsAny<ModelBuilderSettings>(), It.IsAny<List<EdmSchemaError>>()), Times.Once());

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
            var mockVsUtils = new Mock<IVsUtils>();
            var mockModelBuilderSettings = CreateMockModelBuilderSettings();
            var mockModelBuilderEngine = new Mock<ModelBuilderEngine> { CallBase = true };
            var exception = new Exception("Test exception");

            mockModelBuilderEngine
                .Setup(m => m.GenerateModels(It.IsAny<string>(), It.IsAny<ModelBuilderSettings>(), It.IsAny<List<EdmSchemaError>>()))
                .Callback(
                    (string storeModelNamespace, ModelBuilderSettings settings, List<EdmSchemaError> errors) =>
                    {
                        throw exception;
                    });

            Assert.Same(
                exception,
                Assert.Throws<Exception>(
                    () =>
                    mockModelBuilderEngine.Object
                        .GenerateModel(mockModelBuilderSettings.Object, mockVsUtils.Object, mockHostContext.Object)));

            mockHostContext.Verify(h => h.LogMessage(It.IsAny<string>()), Times.Exactly(3));
            mockHostContext
                .Verify(h => h.LogMessage(It.IsRegex(Resources.Engine_ModelGenException + ".+Test exception")), Times.Once());
            mockHostContext
                .Verify(h => h.LogMessage(It.IsRegex(Resources.Engine_ModelGenSuccess)), Times.Never());

            mockVsUtils.Verify(u => u.ShowErrorDialog(string.Format(
                Resources.Engine_ModelGenExceptionMessageBox, exception.GetType().Name, exception.Message)));
        }

        [Fact]
        public void GenerateModel_clears_ModelGenErrorCache()
        {
            var mockModelGenCache = new Mock<ModelGenErrorCache>();
            var mockPackage = new Mock<IEdmPackage>();
            mockPackage.Setup(p => p.ModelGenErrorCache).Returns(mockModelGenCache.Object);
            PackageManager.Package = mockPackage.Object;

            var mockVsUtils = new Mock<IVsUtils>();
            var mockModelBuilderSettings = CreateMockModelBuilderSettings();
            var mockModelBuilderEngine = new Mock<ModelBuilderEngine> { CallBase = true };
            mockModelBuilderEngine
                .Setup(e => e.GenerateModels(It.IsAny<string>(), It.IsAny<ModelBuilderSettings>(), It.IsAny<List<EdmSchemaError>>()))
                .Returns(new DbModel(new DbProviderInfo("System.Data.SqlClient", "2008"), Mock.Of<DbProviderManifest>()));

            mockModelBuilderEngine.Object
                .GenerateModel(mockModelBuilderSettings.Object, Mock.Of<IVsUtils>(), Mock.Of<ModelBuilderEngineHostContext>());

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
}
