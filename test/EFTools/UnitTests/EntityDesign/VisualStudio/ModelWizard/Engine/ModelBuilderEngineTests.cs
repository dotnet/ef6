// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Moq;
    using Moq.Protected;
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

                var mockHostContext = new Mock<ModelBuilderEngineHostContext>();
                var mockModelBuilderEngine =
                    new Mock<ModelBuilderEngine>(mockHostContext.Object, mockModelBuilderSettings.Object)
                        {
                            CallBase = true
                        };
                mockModelBuilderEngine.Setup(m => m.XDocument).Returns(new XDocument());
                mockModelBuilderEngine.Setup(m => m.GenerateModel(It.IsAny<EdmxHelper>()));

                mockModelBuilderEngine.Object.GenerateModel();

                mockModelBuilderEngine.Verify(m => m.GenerateModel(It.IsAny<EdmxHelper>()), Times.Once());
            }

            [Fact]
            public void GenerateModel_generates_the_model()
            {
                var mockModelGenCache = new Mock<ModelGenErrorCache>();
                var mockPackage = new Mock<IEdmPackage>();
                mockPackage.Setup(p => p.ModelGenErrorCache).Returns(mockModelGenCache.Object);
                PackageManager.Package = mockPackage.Object;

                var mockHostContext = new Mock<ModelBuilderEngineHostContext>();
                var mockModelBuilderSettings = CreateMockModelBuilderSettings();
                mockModelBuilderSettings.Object.ModelNamespace = "myModel";

                var mockModelBuilderEngine =
                    new Mock<ModelBuilderEngine>(mockHostContext.Object, mockModelBuilderSettings.Object)
                        {
                            CallBase = true
                        };

                mockModelBuilderEngine
                    .Setup(m => m.GenerateModels(It.IsAny<string>(), It.IsAny<List<EdmSchemaError>>()))
                    .Returns(new DbModel(new DbProviderInfo("System.Data.SqlClient", "2008"), Mock.Of<DbProviderManifest>()));

                mockModelBuilderEngine
                    .Protected()
                    .SetupGet<Uri>("Uri")
                    .Returns(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "temp.edmx")));

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

                mockModelBuilderEngine.Object.GenerateModel(mockEdmxHelper.Object);

                mockModelBuilderEngine
                    .Verify(m => m.GenerateModels(It.IsAny<string>(), It.IsAny<List<EdmSchemaError>>()), Times.Once());
                mockHostContext.Verify(h => h.DispatchToModelGenerationExtensions(), Times.Once());
                mockModelBuilderEngine
                    .Protected()
                    .Verify("UpdateDesignerInfo", Times.Once(), ItExpr.IsAny<EdmxHelper>());
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
                var mockModelBuilderEngine =
                    new Mock<ModelBuilderEngine>(mockHostContext.Object, mockModelBuilderSettings.Object)
                        {
                            CallBase = true
                        };

                var error = new EdmSchemaError("testError", 42, EdmSchemaErrorSeverity.Warning);

                mockModelBuilderEngine
                    .Setup(m => m.GenerateModels(It.IsAny<string>(), It.IsAny<List<EdmSchemaError>>()))
                    .Callback(
                        (string storeModelNamespace, List<EdmSchemaError> errors) => errors.Add(error))
                    .Returns(new DbModel(new DbProviderInfo("System.Data.SqlClient", "2008"), Mock.Of<DbProviderManifest>()));

                mockModelBuilderEngine
                    .Protected()
                    .SetupGet<Uri>("Uri")
                    .Returns(new Uri(@"C:\temp.edmx"));

                var mockEdmxHelper = new Mock<EdmxHelper>(new XDocument());

                mockModelBuilderEngine.Object.GenerateModel(mockEdmxHelper.Object);

                mockModelBuilderEngine
                    .Verify(m => m.GenerateModels(It.IsAny<string>(), It.IsAny<List<EdmSchemaError>>()), Times.Once());
                mockHostContext.Verify(h => h.DispatchToModelGenerationExtensions(), Times.Once());
                mockModelBuilderEngine
                    .Protected()
                    .Verify("UpdateDesignerInfo", Times.Once(), ItExpr.IsAny<EdmxHelper>());
                mockHostContext.Verify(h => h.LogMessage(It.IsAny<string>()), Times.Exactly(3));
                mockHostContext
                    .Verify(h => h.LogMessage(It.IsRegex(Resources.Engine_ModelGenErrors.Substring(1, 20))), Times.Once());
                mockHostContext
                    .Verify(h => h.LogMessage(It.IsRegex(Resources.Engine_ModelGenException)), Times.Never());

                Assert.Same(error, modelGenCache.GetErrors(@"C:\temp.edmx").Single());
            }

            [Fact]
            public void GenerateModel_logs_exception()
            {
                var mockPackage = new Mock<IEdmPackage>();
                mockPackage.Setup(p => p.ModelGenErrorCache).Returns(new ModelGenErrorCache());
                PackageManager.Package = mockPackage.Object;

                var mockHostContext = new Mock<ModelBuilderEngineHostContext>();
                var mockModelBuilderSettings = CreateMockModelBuilderSettings();
                var mockModelBuilderEngine =
                    new Mock<ModelBuilderEngine>(mockHostContext.Object, mockModelBuilderSettings.Object)
                        {
                            CallBase = true
                        };

                mockModelBuilderEngine
                    .Setup(m => m.GenerateModels(It.IsAny<string>(), It.IsAny<List<EdmSchemaError>>()))
                    .Callback(
                        (string storeModelNamespace, List<EdmSchemaError> errors) => { throw new Exception("Test exception"); });

                mockModelBuilderEngine
                    .Protected()
                    .SetupGet<Uri>("Uri")
                    .Returns(new Uri(@"C:\temp.edmx"));

                var mockEdmxHelper = new Mock<EdmxHelper>(new XDocument());

                mockModelBuilderEngine.Object.GenerateModel(mockEdmxHelper.Object);

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
                var mockModelBuilderEngine =
                    new Mock<ModelBuilderEngine>(mockHostContext.Object, mockModelBuilderSettings.Object)
                        {
                            CallBase = true
                        };

                mockModelBuilderEngine
                    .Protected()
                    .SetupGet<Uri>("Uri")
                    .Returns(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "temp.edmx")));

                mockModelBuilderEngine.Object.GenerateModel(new EdmxHelper(new XDocument()));

                mockModelGenCache.Verify(c => c.RemoveErrors(It.IsAny<string>()), Times.Once());
            }

            private static Mock<ModelBuilderSettings> CreateMockModelBuilderSettings()
            {
                var mockModelBuilderSettings = new Mock<ModelBuilderSettings>();
                mockModelBuilderSettings.Object.GenerationOption = ModelGenerationOption.GenerateFromDatabase;
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
                    : base(new Mock<ModelBuilderEngineHostContext>().Object, new ModelBuilderSettings())
                {
                }

                protected override void AddErrors(IEnumerable<EdmSchemaError> errors)
                {
                    throw new NotImplementedException();
                }

                internal override IEnumerable<EdmSchemaError> Errors
                {
                    get { throw new NotImplementedException(); }
                }

                protected override Uri Uri
                {
                    get { throw new NotImplementedException(); }
                }

                internal override XDocument XDocument
                {
                    get { throw new NotImplementedException(); }
                }

                #endregion

                internal void UpdateDesignerInfoInvoker(EdmxHelper edmxHelper)
                {
                    UpdateDesignerInfo(edmxHelper);
                }
            }

            [Fact]
            public void UpdateDesignerInfo_updates_properties_in_designer_section()
            {
                var mockEdmxHelper = new Mock<EdmxHelper>(new XDocument());
                new ModelBuilderEngineFake().UpdateDesignerInfoInvoker(mockEdmxHelper.Object);

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

        [Fact]
        public void CanCreateAndOpenConnection_returns_true_for_valid_connection()
        {
            var mockEntityConnection = new Mock<EntityConnection>();
            var mockConnectionFactory = new Mock<StoreSchemaConnectionFactory>();

            Version version;
            mockConnectionFactory
                .Setup(
                    f => f.Create(
                        It.IsAny<IDbDependencyResolver>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<Version>(), out version))
                .Returns(mockEntityConnection.Object);

            Assert.True(
                ModelBuilderEngine.CanCreateAndOpenConnection(
                    mockConnectionFactory.Object, "fakeInvariantName", "fakeInvariantName", "fakeConnectionString"));
        }

        [Fact]
        public void CanCreateAndOpenConnection_returns_true_for_invalid_connection()
        {
            var mockEntityConnection = new Mock<EntityConnection>();
            mockEntityConnection.Setup(c => c.Open()).Throws<InvalidOperationException>();

            var mockConnectionFactory = new Mock<StoreSchemaConnectionFactory>();

            Version version;
            mockConnectionFactory
                .Setup(
                    f => f.Create(
                        It.IsAny<IDbDependencyResolver>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<Version>(), out version))
                .Returns(mockEntityConnection.Object);

            Assert.False(
                ModelBuilderEngine.CanCreateAndOpenConnection(
                    mockConnectionFactory.Object, "fakeInvariantName", "fakeInvariantName", "fakeConnectionString"));
        }

        [Fact]
        public void CanCreateAndOpenConnection_passes_the_latest_EF_version_as_the_max_version()
        {
            var mockEntityConnection = new Mock<EntityConnection>();
            var mockConnectionFactory = new Mock<StoreSchemaConnectionFactory>();

            Version version;
            mockConnectionFactory
                .Setup(
                    f => f.Create(
                        It.IsAny<IDbDependencyResolver>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<Version>(), out version))
                .Returns(mockEntityConnection.Object);

            ModelBuilderEngine.CanCreateAndOpenConnection(
                mockConnectionFactory.Object, "fakeInvariantName", "fakeInvariantName", "fakeConnectionString");

            mockConnectionFactory.Verify(
                f => f.Create(
                    It.IsAny<IDbDependencyResolver>(),
                    It.Is<string>(s => s == "fakeInvariantName"),
                    It.Is<string>(s => s == "fakeConnectionString"),
                    It.Is<Version>(v => v == EntityFrameworkVersion.Latest),
                    out version),
                Times.Once());
        }
    }
}
