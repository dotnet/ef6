// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using ThreadSafeMutex = System.Threading.Mutex;

namespace Microsoft.Data.Entity.Design.DataSourceWizardExtension
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.VSDesigner.Data.DataSourceWizard.Interface;
    using Microsoft.VSWizards;
    using Microsoft.VisualStudio.TemplateWizard;

    [Export(typeof(DataSourceWizardPageBase))]
    internal abstract class LaunchWizardPageBase : DataSourceWizardPageBase
    {
        private Label _statusLabel;
        private Timer _hideDataSourceWizardFormTimer;
        private bool _dismissDSWizard;
        private bool _isPageClosing;
        private readonly ThreadSafeMutex _mutex;

        /// <summary>
        ///     LaunchWizardPageBase is the base class for the data source wizard pages that launch Entity Data Model Wizard.
        /// </summary>
        /// <param name="wizard">The wizard that will display the page</param>
        internal LaunchWizardPageBase(DataSourceWizardFormBase wizard)
            : base(wizard)
        {
            _mutex = new ThreadSafeMutex();
            _isPageClosing = false;
            _dismissDSWizard = false;
            InitializeComponent();
        }

        protected override void OnLeavePage(LeavePageEventArgs e)
        {
            base.OnLeavePage(e);

            // On Page-Entered event, we spawn another thread that hides DS Wizard (the parent wizard) asynchronously. The problem is that 
            // if the thread runs AFTER this page is closed, the DS wizard will never be visible again. The code below
            // is added to prevent such problem to surface.
            _mutex.WaitOne();

            // Set the flag to indicate the page is closing.
            _isPageClosing = true;
            // Set DS Wizard to be visible if _dismissDSWizard is false;
            if (_dismissDSWizard == false)
            {
                WizardForm.Visible = true;
            }

            _mutex.ReleaseMutex();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                _mutex.Dispose();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected override void OnEnteredPage(EventArgs e)
        {
            // Set the AcceptButton to be a disabled Finish Button
            WizardForm.AcceptButton = WizardForm.FinishButton;
            WizardForm.FinishButton.Enabled = false;

            var wizardData = WizardForm.WizardData as EdmDataSourceWizardData;
            Debug.Assert(wizardData != null, "Instance of EdmDataSourceWizardData is not passed in to LaunchModelGenWizardPage.");

            _isPageClosing = false;
            _dismissDSWizard = false;

            if (wizardData != null)
            {
                try
                {
                    Message = Resources.LaunchingEntityDataModelWizardMessage;
                    // Hide Data-Source Wizard asynchronously. This is a workaround so our wizard can be displayed in the correct position.
                    _hideDataSourceWizardFormTimer.Start();
                    _dismissDSWizard = LaunchEntityDesignerWizard(wizardData);
                }
                catch (WizardCancelledException)
                {
                    wizardData.IsCancelled = true;
                }
                catch (Exception ex)
                {
                    try
                    {
                        var errMessageWithInnerExceptions = VsUtils.ConstructInnerExceptionErrorMessage(ex);
                        var message = string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.LaunchEntityDesignerWizardErrorMessage,
                            ex.GetType().FullName, errMessageWithInnerExceptions);
                        VsUtils.ShowErrorDialog(message);
                    }
                    catch (Exception)
                    {
                        // exception trying to show error dialog - do nothing
                    }
                }
                finally
                {
                    // Don't need to make the form visible if we are about to dismiss the wizard.
                    if (!_dismissDSWizard)
                    {
                        WizardForm.Visible = true;
                    }
                }
            }

            // Just return to the previous DS Wizard page if we should not dismiss the DS Wizard.
            if (!_dismissDSWizard)
            {
                WizardForm.BackPage();
            }
            else
            {
                // Call Finish method to dismiss the wizard.
                WizardForm.FinishButton.Enabled = true;
                WizardForm.Finish();
            }
        }

        protected abstract bool LaunchEntityDesignerWizard(EdmDataSourceWizardData wizardData);

        #region Event handlers

        private void hideDataSourceWizardFormTimer_Tick(object sender, EventArgs e)
        {
            _mutex.WaitOne();
            if (_isPageClosing == false)
            {
                WizardForm.Visible = false;
                Message = String.Empty;
            }
            _mutex.ReleaseMutex();

            _hideDataSourceWizardFormTimer.Stop();
        }

        #endregion

        #region Component Designer generated code

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._hideDataSourceWizardFormTimer = new Timer();
            this._hideDataSourceWizardFormTimer.Interval = 1000; // wait 1 second before hiding the DataSource wizard.
            this._hideDataSourceWizardFormTimer.Tick += new EventHandler(hideDataSourceWizardFormTimer_Tick);
            this.Title = Resources.WizardLaunchPageTitle;

            this._statusLabel = new Label();
            this._statusLabel.Size = this.ClientSize;
            this._statusLabel.Padding = new System.Windows.Forms.Padding(15);
            this._statusLabel.AccessibleName = Resources.StatusLabelAccessibleName;

            this.Controls.Add(this._statusLabel);
            this.PerformLayout();
        }

        #endregion

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        protected string Message
        {
            get { return _statusLabel.Text; }
            set { _statusLabel.Text = value; }
        }
    }
}
