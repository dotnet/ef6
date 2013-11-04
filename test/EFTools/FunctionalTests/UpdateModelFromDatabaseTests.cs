// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace EFDesigner.FunctionalTests
{
    using System;
    using System.Xml.Linq;
    using EFDesignerTestInfrastructure;
    using EFDesignerTestInfrastructure.EFDesigner;
    using Microsoft.Data.Entity.Design.Extensibility;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Moq;
    using Xunit;
    using global::FunctionalTests.TestHelpers;

    public class UpdateModelFromDatabaseTests : IUseFixture<EdmPackageFixture>
    {
        /// <summary>
        ///     Fixture that creates a mock package and sets PackageManager.Package
        /// </summary>
        /// <param name="packageFixture">Pacakge fixture</param>
        /// <remarks>
        ///     The tests need PackageManager.Package to be set but don't need the
        ///     package itself. Since it needs to be diposed IUseFixture seems fine.
        ///     We don't need to set it - xUnit is keeping a reference to it and it
        ///     will dispose it after test execution is completed.
        /// </remarks>
        public void SetFixture(EdmPackageFixture packageFixture)
        {
        }

        [Fact]
        public void SSpace_facets_not_propagated_to_CSpace_when_creating_new_model_form_non_SqlServer_database()
        {
            using (var artifactHelper = new MockEFArtifactHelper())
            {
                // set up original artifact
                var uri = TestUtils.FileName2Uri(@"TestArtifacts\PropagateSSidePropertyFacetsToCSideOracle.edmx");
                var artifact = (EntityDesignArtifact)artifactHelper.GetNewOrExistingArtifact(uri);
                var settings = SetupBaseModelBuilderSettings(artifact);

                var cp = ModelObjectItemWizard.PrepareCommandsAndIntegrityChecks(settings, artifact.GetEditingContext(), artifact);

                Assert.NotNull(cp);
                // One command for stored procedure expected
                Assert.Equal(1, cp.CommandCount);
                // No integrity checks expected - facet propagation should be disabled for non SqlServer databases
                Assert.Equal(0, cp.CommandProcessorContext.IntegrityChecks.Count);
            }
        }

        [Fact]
        public void SSpace_facets_propagated_to_CSpace_when_creating_new_model_form_SqlServer_database()
        {
            using (var artifactHelper = new MockEFArtifactHelper())
            {
                // set up original artifact
                var uri = TestUtils.FileName2Uri(@"TestArtifacts\EmptyEdmx.edmx");
                var artifact = (EntityDesignArtifact)artifactHelper.GetNewOrExistingArtifact(uri);
                var settings = SetupBaseModelBuilderSettings(artifact);

                CommandProcessor cp = null;

                try
                {
                    cp = ModelObjectItemWizard.PrepareCommandsAndIntegrityChecks(settings, artifact.GetEditingContext(), artifact);
                    Assert.NotNull(cp);
                    // Facet propagation should be enabled SqlServer databases
                    Assert.Equal(1, cp.CommandProcessorContext.IntegrityChecks.Count);
                }
                finally
                {
                    if (cp != null)
                    {
                        cp.CommandProcessorContext.ClearIntegrityChecks();
                    }
                }
            }
        }

        private static ModelBuilderSettings SetupBaseModelBuilderSettings(EFArtifact artifact)
        {
            var settings = new ModelBuilderSettings
                {
                    Artifact = artifact,
                    GenerationOption = ModelGenerationOption.GenerateFromDatabase,
                    SaveConnectionStringInAppConfig = false,
                    VSApplicationType = VisualStudioProjectSystem.WindowsApplication,
                    WizardKind = WizardKind.UpdateModel
                };

            var hostContext = new Mock<ModelBuilderEngineHostContext>();
            hostContext.Setup(hc => hc.LogMessage(It.IsAny<string>())).Callback(Console.WriteLine);
            hostContext.Setup(hc => hc.DispatchToModelGenerationExtensions()).Callback(() => { });

            var modelBuilderEngine = new Mock<UpdateModelFromDatabaseModelBuilderEngine>();
            modelBuilderEngine.SetupGet(mbe => mbe.Model).Returns(XDocument.Load(artifact.Uri.ToString()));

            settings.ModelBuilderEngine = modelBuilderEngine.Object;

            return settings;
        }
    }
}
