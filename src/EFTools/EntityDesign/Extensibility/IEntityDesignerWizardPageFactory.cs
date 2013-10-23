// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if WIZARD_EXTENSION_PAGE
namespace Microsoft.Data.Entity.Design.Extensibility
{
    using System.Collections.Generic;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui;

    /// <summary>
    ///     Interface of a factory method that will create new wizard pages
    /// </summary>
    internal interface IEntityDesignerWizardPageFactory
    {
        WizardPageBase CreatePage(ModelBuilderWizardForm wizardForm, Dictionary<string, object> wizardData);
    }
}

#endif
