// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Dialogs
{
    using System;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.VisualStudio;

    // <summary>
    //     Displays a dialog that allows us to display debug information in modal textbox in debug builds
    // </summary>
    internal partial class DebugViewerDialog : Form
    {
        internal enum ButtonMode
        {
            OkCancel,
            YesNo
        }

        private readonly Action<DebugViewerDialog> _onOkClick;

        internal string Message
        {
            get { return txtMessage.Text; }
        }

        internal DebugViewerDialog(string formattedTitle, string formattedMessage, Action<DebugViewerDialog> onOkClick = null)
        {
            InitializeComponent();

            // Set the default font to VS shell font.
            var vsFont = VSHelpers.GetVSFont(Services.ServiceProvider);
            if (vsFont != null)
            {
                Font = vsFont;
            }

            Text = formattedTitle;
            txtMessage.Text = formattedMessage;
            _onOkClick = onOkClick;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (_onOkClick == null)
            {
                Close();
            }
            else
            {
                _onOkClick(this);
            }
        }
    }
}
