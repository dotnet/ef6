// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    using System;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Moq;
    using Moq.Protected;
    using UnitTests.TestHelpers;

    internal class ModelBuilderWizardFormHelper
    {
        public static ModelBuilderWizardForm CreateWizard(ModelGenerationOption generationOption = (ModelGenerationOption)(-1), 
            Project project = null, string modelPath = null, IServiceProvider serviceProvider = null)
        {
            var modelBuilderSettings =
                new ModelBuilderSettings
                {
                    Project = project ?? MockDTE.CreateProject(),
                    GenerationOption = generationOption,
                    ModelPath = modelPath
                };

            return CreateWizard(modelBuilderSettings, serviceProvider);
        }

        public static ModelBuilderWizardForm CreateWizard(ModelBuilderSettings modelBuilderSettings, IServiceProvider serviceProvider = null)
        {
            var mockWizard = new Mock<ModelBuilderWizardForm>(
                serviceProvider ?? Mock.Of<IServiceProvider>(),
                modelBuilderSettings,
                ModelBuilderWizardForm.WizardMode.PerformAllFunctionality)
            {
                CallBase = true
            };

            mockWizard.Protected().Setup("InitializeWizardPages");

            return mockWizard.Object;
        }
    }
}