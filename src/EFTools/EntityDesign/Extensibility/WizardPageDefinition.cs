// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    using Microsoft.WizardFramework;

    internal class WizardPageDefinition
    {
        internal WizardPage WizardPage { get; private set; }

        internal string WizardPageId { get; private set; }

        // <summary>
        //     Allows creation of a definition that is composed of a new WizardPage
        // </summary>
        internal WizardPageDefinition(WizardPage wizardPage)
        {
            WizardPage = wizardPage;
        }

        // <summary>
        //     Allows creation of a definition that is composed of a WizardPageKind enum value representing an existing core wizard page
        // </summary>
        internal WizardPageDefinition(string wizardPageId)
        {
            WizardPageId = wizardPageId;
        }
    }
}
