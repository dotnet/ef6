// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Package
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Windows.Forms;
    using Microsoft.VisualStudio.Modeling.Diagrams;

    /// <summary>
    ///     A thumbnail form class to host a pan/zoom control.
    /// </summary>
    internal sealed class ThumbnailViewForm : Form
    {
        // width/height of the window.
        private const int ViewSize = 180;

        // control itself
        private readonly PanZoomPanel _panZoomPanel;

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal ThumbnailViewForm(Control baseControl, DiagramClientView diagramClientView)
        {
            if (baseControl == null)
            {
                throw new ArgumentNullException("baseControl");
            }
            if (diagramClientView == null)
            {
                throw new ArgumentNullException("diagramClientView");
            }

            // Initialize the form.
            TopMost = true;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;

            // Position form so that its center lines up with the center of thumbnail control
            // at designer's bottom-right corner.
            var location = baseControl.PointToScreen(new Point(baseControl.Width / 2, baseControl.Height / 2));
            location.Offset(-ViewSize / 2, -ViewSize / 2);
            Bounds = new Rectangle(location.X, location.Y, ViewSize, ViewSize);

            // Make sure thumbnail form fits the screen and doesn't go below or off the right
            // edge of the screen.
            var screenBounds = Screen.FromControl(diagramClientView).WorkingArea;
            if (Right > screenBounds.Right)
            {
                Left = screenBounds.Right - Width;
            }
            if (Bottom > screenBounds.Bottom)
            {
                Top = screenBounds.Bottom - Height;
            }

            // Initialize a panel to host pan/zoom control.
            var panel1 = new Panel();
            panel1.Dock = DockStyle.Fill;
            panel1.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(panel1);

            // Initialize and dock pan/zoom control on the panel.
            _panZoomPanel = new PanZoomPanel();
            _panZoomPanel.Dock = DockStyle.Fill;
            panel1.Controls.Add(_panZoomPanel);
            _panZoomPanel.InvalidateImage(diagramClientView);

            Cursor.Hide();
        }

        protected override CreateParams CreateParams
        {
            [DebuggerStepThrough]
            get
            {
                // Give this form a nice shadow.
                var createParams = base.CreateParams;
                Debug.Assert(createParams != null);
                createParams.ClassStyle |= 0x00020000;
                return createParams;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            Debug.Assert(e != null);
            Debug.Assert(_panZoomPanel != null);

            var initialMousePos = Cursor.Position;
            _panZoomPanel.MouseUp += delegate
                {
                    // When mouse is released, the form should go away.
                    Cursor.Position = initialMousePos;
                    Close();
                    Cursor.Show();
                };

            // We automatically start moving diagram view on load.
            _panZoomPanel.StartMove();
        }
    }
}
