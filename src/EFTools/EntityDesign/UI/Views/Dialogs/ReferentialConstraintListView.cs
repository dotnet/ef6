// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Dialogs
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.VisualStudio;

    internal partial class ReferentialConstraintListView : ListView
    {
        public ReferentialConstraintListView()
        {
            InitializeComponent();
        }

        internal bool IsHorizontalScrollbarVisible
        {
            get
            {
                var wndStyle = NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_STYLE);
                return (wndStyle.ToInt32() & NativeMethods.WS_HSCROLL) != 0;
            }
        }

        internal bool IsVerticalScrollbarVisible
        {
            get
            {
                var wndStyle = NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_STYLE);
                return (wndStyle.ToInt32() & NativeMethods.WS_VSCROLL) != 0;
            }
        }

        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message msg)
        {
            // Look for the WM_VSCROLL or the WM_HSCROLL messages.
            if ((msg.Msg == NativeMethods.WM_VSCROLL)
                || (msg.Msg == NativeMethods.WM_HSCROLL))
            {
                // Move focus to the ListView to cause ComboBox to lose focus.
                Focus();
            }

            // Pass message to default handler.
            base.WndProc(ref msg);
        }
    }
}
