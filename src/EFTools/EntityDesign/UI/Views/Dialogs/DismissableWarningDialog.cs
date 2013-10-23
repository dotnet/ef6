// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Dialogs
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using Microsoft.Data.Entity.Design.VisualStudio;

    /// <summary>
    ///     Displays a dialog that allows the user to choose to never see it again via a checkbox.
    /// </summary>
    internal partial class DismissableWarningDialog : Form
    {
        internal enum ButtonMode
        {
            OkCancel,
            YesNo
        }

        internal DismissableWarningDialog(string formattedTitle, string formattedMessage, ButtonMode buttonMode)
        {
            InitializeComponent();

            // Set the default font to VS shell font.
            var vsFont = VSHelpers.GetVSFont(Services.ServiceProvider);
            if (vsFont != null)
            {
                Font = vsFont;
            }

            Text = formattedTitle;
            lblWarning.Text = formattedMessage;

            // Default button mode is OkCancel
            if (buttonMode == ButtonMode.YesNo)
            {
                okButton.Text = DialogsResource.YesButton_Text;
                cancelButton.Text = DialogsResource.NoButton_Text;
            }
        }

        /// <summary>
        ///     Static method to instantiate a DismissableWarningDialog and persist the user setting to dismiss the dialog.
        ///     Returns a boolean indicating whether the dialog was cancelled or not.
        /// </summary>
        /// <param name="formattedTitle"></param>
        /// <param name="formattedMessage"></param>
        /// <param name="regKeyName"></param>
        /// <param name="buttonMode">Either 'OKCancel' or 'YesNo'. If 'YesNo', 'Yes' will be associated with DialogResult.OK</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText")]
        internal static bool ShowWarningDialogAndSaveDismissOption(
            string formattedTitle, string formattedMessage, string regKeyName, ButtonMode buttonMode)
        {
            var cancelled = true;

            var service = Services.ServiceProvider.GetService(typeof(IUIService)) as IUIService;
            Debug.Assert(service != null, "service should not be null");
            if (service != null)
            {
                using (var dialog = new DismissableWarningDialog(formattedTitle, formattedMessage, buttonMode))
                {
                    if (service.ShowDialog(dialog) == DialogResult.OK)
                    {
                        cancelled = false;
                        var showAgain = dialog.chkWarning.Checked == false;
                        EdmUtils.SaveUserSetting(regKeyName, showAgain.ToString());
                    }
                }
            }

            return cancelled;
        }
    }
}
