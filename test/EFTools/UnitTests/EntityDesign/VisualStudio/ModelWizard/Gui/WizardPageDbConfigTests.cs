// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    using System;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Moq;
    using Xunit;

    public class WizardPageDbConfigTests
    {
        static WizardPageDbConfigTests()
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
        public void OnActivate_result_depends_on_FileAlreadyExistsError()
        {
            var wizard = new ModelBuilderWizardForm(new ModelBuilderSettings(), ModelBuilderWizardForm.WizardMode.PerformAllFunctionality);
            var wizardPageDbConfig = new WizardPageDbConfig(wizard, new Mock<IServiceProvider>().Object);

            wizard.FileAlreadyExistsError = true;
            Assert.False(wizardPageDbConfig.OnActivate());

            wizard.FileAlreadyExistsError = false;
            Assert.True(wizardPageDbConfig.OnActivate());
        }
    }
}
