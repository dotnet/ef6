// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Shell
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Common;
    using Microsoft.VisualStudio.Modeling.Shell;

    /// <summary>
    ///     This is the base class for the main control in tool windows.
    ///     It performs such things as drawing a border, a watermark, and
    ///     any other common tool window operations.
    /// </summary>
    internal class TreeGridDesignerToolWindowContainer : ContainerControl, ITreeGridDesignerToolWindowContainer
    {
        // NA watermark...
        private readonly LinkLabel _watermark;
        private readonly Control _mainControl;
        private const int BORDER = 1;

        internal TreeGridDesignerToolWindowContainer(Control mainControl)
        {
            // Set this control to some arbitrary size.
            // Without a defined size, the anchored child controls don't anchor correctly.
            Left = 0;
            Top = 0;
            Width = 200;
            Height = 200;

            // add main control hosted in the tool window
            _mainControl = mainControl;
            mainControl.Bounds = new Rectangle(1, 1, 198, 198);
            mainControl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            Controls.Add(mainControl);

            // Label used when the tool window is irrelevant.
            _watermark = new LinkLabel();
            _watermark.Location = new Point(BORDER + 2, BORDER + 1);
            _watermark.Size = new Size(Width - 2 * BORDER - 2, Height - 2 * BORDER - 1);
            _watermark.Text = String.Empty;
            _watermark.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _watermark.TextAlign = ContentAlignment.MiddleCenter;
            _watermark.ForeColor = SystemColors.GrayText;
            _watermark.BackColor = SystemColors.Window;
            _watermark.Visible = false;
            _watermark.LinkClicked += watermarkLabel_LinkClicked;
            Controls.Add(_watermark);
        }

        protected Rectangle BorderRect
        {
            get { return new Rectangle(BORDER + 1, BORDER, Width - BORDER * 2 - 1, Height - BORDER * 2); }
        }

        /// <summary>
        ///     Event handler when window is being repainted.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Draw a bounding rect to make it look better...
            e.Graphics.DrawRectangle(SystemPens.ControlDark, BorderRect);

            base.OnPaint(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            // forward focus to the inner control
            if (!WatermarkVisible)
            {
                _mainControl.Focus();
            }
            else
            {
                // give focus to the watermark.  Allows accessibility
                // tools to read the watermark text.
                _watermark.Focus();
            }
        }

        /// <summary>
        ///     Process window messages.
        /// </summary>
        /// <param name="m"></param>
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            try
            {
                base.WndProc(ref m);
            }
            catch (Exception e)
            {
                if (CriticalException.ThrowOrShow(_mainControl != null ? _mainControl.Site : null, e))
                {
                    throw;
                }
            }
        }

        /// <summary>
        ///     Gets/sets the current display mode based on the selection in the drawing surface.
        /// </summary>
        public bool /* ITreeGridDesignerToolWindowContainer */ WatermarkVisible
        {
            get { return _watermark.Visible; }

            set
            {
                if (_watermark.Visible != value)
                {
                    _watermark.Visible = value;
                    _mainControl.Visible = !value;
                }
            }
        }

        public IWin32Window /* ITreeGridDesignerToolWindowContainer */ Window
        {
            get { return this; }
        }

        public object /* ITreeGridDesignerToolWindowContainer */ HostContext { get; set; }

        public void /* ITreeGridDesignerToolWindowContainer */ SetWatermarkInfo(TreeGridDesignerWatermarkInfo watermarkInfo)
        {
            _watermark.Text = watermarkInfo.WatermarkText;
            _watermark.Links.Clear();
            foreach (var ld in watermarkInfo.WatermarkLinkData)
            {
                if (ld.LinkStart > 0)
                {
                    _watermark.Links.Add(new LinkLabel.Link(ld.LinkStart, ld.LinkLength, ld.LinkClickedHandler));
                }
            }
        }

        private void watermarkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Link data could be an event handler
            var handler = e.Link.LinkData as LinkLabelLinkClickedEventHandler;
            Debug.Assert(handler != null, "didn't find link-clicked handler as link data!");
            if (handler != null)
            {
                handler.Invoke(sender, e);
            }
        }

        public void SetWatermarkThemedColors()
        {
        }

        public void SetToolbarThemedColors()
        {
        }
    }
}
