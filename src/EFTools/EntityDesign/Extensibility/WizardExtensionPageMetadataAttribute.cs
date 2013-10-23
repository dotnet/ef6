// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if WIZARD_EXTENSION_PAGE
namespace Microsoft.Data.Entity.Design.Extensibility
{
    using System;
    using System.ComponentModel.Composition;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui;

    /// <summary>
    ///     Metadata for a wizard extension
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal sealed class WizardExtensionPageMetadataAttribute : Attribute
    {
        // TODO:  make array
        public string ProjectGuid { get; set; }
        public ModelBuilderWizardForm.WizardMode WizardMode { get; set; }
        public WizardStage WizardStage { get; set; }

        public WizardExtensionPageMetadataAttribute()
        {
            ProjectGuid = Guid.Empty.ToString();
            WizardMode = ModelBuilderWizardForm.WizardMode.PerformAllFunctionality;
            WizardStage = WizardStage.PreModelGeneration;
        }
    }
}

#endif
