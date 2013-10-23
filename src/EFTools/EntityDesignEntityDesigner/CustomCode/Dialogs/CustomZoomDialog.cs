// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.Dialogs
{
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.VisualStudio;

    internal partial class CustomZoomDialog : Form
    {
        public int ZoomPercent
        {
            get { return (int)numericUpDownZoom.Value; }

            set { numericUpDownZoom.Value = value; }
        }

        public CustomZoomDialog()
        {
            InitializeComponent();

            // Set the default font to VS shell font.
            var vsFont = VSHelpers.GetVSFont(Services.ServiceProvider);
            if (vsFont != null)
            {
                Font = vsFont;
            }
        }
    }
}
