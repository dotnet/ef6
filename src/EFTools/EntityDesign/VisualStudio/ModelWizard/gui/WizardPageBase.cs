// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.WizardFramework;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class WizardPageBase : WizardPage
    {
        private readonly ModelBuilderWizardForm _wizard;

        // this constructor is needed for designer to load. Do not delete it
        protected WizardPageBase()
        {
        }

        public WizardPageBase(ModelBuilderWizardForm wizard)
            : base(wizard)
        {
            _wizard = wizard;

            // Set the default font to VS shell font.
            var vsFont = VSHelpers.GetVSFont(wizard.ServiceProvider);
            if (vsFont != null)
            {
                Font = vsFont;
            }
        }

        protected Font LabelFont
        {
            get { return new Font(Font.FontFamily, Font.Size, FontStyle.Bold, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont); }
        }

        protected new ModelBuilderWizardForm Wizard
        {
            get { return _wizard; }
        }

        internal virtual void OnWizardCancel()
        {
        }

        // <summary>
        //     this will be called by the ModelWizard for all remaining pages (starting from active one), when the Finish button is clicked
        // </summary>
        // <returns>true if wizard can finish, false otherwise</returns>
        internal virtual bool OnWizardFinish()
        {
            return true;
        }

        protected IServiceProvider ServiceProvider
        {
            get { return _wizard.ServiceProvider; }
        }

        internal virtual bool MovingNext 
        {
            get { return Wizard.MovingNext; }
        }

        public override bool OnDeactivate()
        {
            if (MovingNext
                && !Wizard.MovingPrevious
                && !Wizard.WizardFinishing)
            {
                // we check here if the current page being deactivated is the last of the pre-model-generation pages
                // if so we generate the model.  All subsequent pages are post-model-generation pages, and expect the
                // model to be generated.
                //
                //  OnFinish() is called is called when the wizard as a whole completes.
                //
                if (Wizard.Mode == ModelBuilderWizardForm.WizardMode.PerformAllFunctionality)
                {
                    if (Wizard.IsLastPreModelGenerationPageActive())
                    {
                        // generate the model
                        if (Wizard.ModelBuilderSettings.GenerationOption == ModelGenerationOption.GenerateFromDatabase)
                        {
                            GenerateModel(Wizard.ModelBuilderSettings);
                        }
                    }
                }
            }

            return base.OnDeactivate();
        }

        protected static void GenerateModel(ModelBuilderSettings settings)
        {
            using (new VsUtils.HourglassHelper())
            {
                var mbe = settings.ModelBuilderEngine;
                mbe.GenerateModel(settings);
            }
        }

        protected override void OnHelpRequested(HelpEventArgs hevent)
        {
            // ignore request
            hevent.Handled = true;
        }
    }
}
