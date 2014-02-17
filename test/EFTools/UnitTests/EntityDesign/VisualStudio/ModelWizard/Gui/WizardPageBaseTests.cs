// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    using EnvDTE;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Moq;
    using System;
    using System.Xml.Linq;
    using Xunit;

    public class WizardPageBaseTests
    {
        static WizardPageBaseTests()
        {
            // The code below is required to avoid test failures due to:
            // Due to limitations in CLR, DynamicProxy was unable to successfully replicate non-inheritable attribute 
            // System.Security.Permissions.UIPermissionAttribute on 
            // Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui.WizardPageStart.ProcessDialogChar. 
            // To avoid this error you can chose not to replicate this attribute type by calling
            // 'Castle.DynamicProxy.Generators.AttributesToAvoidReplicating.Add(typeof(System.Security.Permissions.UIPermissionAttribute))'.
            // 
            // Note that the same pattern need to be used when creating tests for other wizard pages to avoid 
            // issues related to the order the tests are run. Alternatively we could have code that is always being run
            // before any tests (e.g. a ctor of a class all test classes would derive from) where we would do that
            Castle.DynamicProxy.Generators
                .AttributesToAvoidReplicating
                .Add(typeof(System.Security.Permissions.UIPermissionAttribute));
        }

        [Fact]
        public void OnDeactivate_generates_model()
        {
            var mockModelBuilderEngine = 
                new Mock<EdmxModelBuilderEngine>(Mock.Of<IInitialModelContentsFactory>());
            
            var mockSettings = new Mock<ModelBuilderSettings> { CallBase = true };
            mockSettings.Setup(s => s.DesignTimeConnectionString).Returns("fakeConnString");
            mockSettings.Object.ModelBuilderEngine = mockModelBuilderEngine.Object;
            mockSettings.Object.Project = Mock.Of<Project>();

            var mockWizardPageBase = new Mock<WizardPageBase>(
                ModelBuilderWizardFormHelper.CreateWizard(mockSettings.Object)) { CallBase = true };
            mockWizardPageBase.Setup(p => p.MovingNext).Returns(true);

            mockWizardPageBase.Object.OnDeactivate();

            mockModelBuilderEngine
                .Verify(m => m.GenerateModel(It.IsAny<ModelBuilderSettings>(), It.IsAny<ModelBuilderEngineHostContext>()), Times.Once());
        }
    }
}
