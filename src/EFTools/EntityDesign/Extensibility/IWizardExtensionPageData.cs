// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if WIZARD_EXTENSION_PAGE
namespace Microsoft.Data.Entity.Design.Extensibility
{
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui;

    internal interface IWizardExtensionPageData
    {
        // TODO:  make this an array
        string ProjectGuid { get; }
        ModelBuilderWizardForm.WizardMode WizardMode { get; }
        WizardStage WizardStage { get; }
    }
}

#endif
