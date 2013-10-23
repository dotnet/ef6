// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Shell
{
    using System;
    using System.Diagnostics;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;

    internal class TreeGridDesignerInPlaceEdit : VirtualTreeInPlaceEditControl
    {
        public TreeGridDesignerInPlaceEdit()
        {
            BorderStyle = BorderStyle.None;
            AutoSize = false; // with no border, AutoSize causes some overlap with the grid lines on the tree control, so we remove it.
            ((IVirtualTreeInPlaceControlDefer)this).Flags &= ~(VirtualTreeInPlaceControls.SizeToText);
        }

        /// <summary>
        ///     Pass KeyPress events on to the parent control for special handling
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (!e.Handled)
            {
                var parent = Parent as TreeGridDesignerTreeControl;

                Debug.Assert(parent != null, "parent of TreeGridDesignerInPlaceEdit must be TreeGridDesignerTreeControl");

                if (parent != null)
                {
                    e.Handled = (ProcessKeyReturn.NotHandled != parent.ProcessKeyPress(e.KeyChar, ModifierKeys));
                }
            }
        }

        /// <summary>
        ///     Pass KeyDown events on to the parent control for special handling
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!e.Handled)
            {
                var parent = Parent as TreeGridDesignerTreeControl;

                Debug.Assert(parent != null, "parent of TreeGridDesignerInPlaceEdit must be TreeGridDesignerTreeControl");

                if (parent != null)
                {
                    e.Handled = (ProcessKeyReturn.NotHandled != parent.ProcessKeyDown(e));
                }
            }
        }

        /// <summary>
        ///     End insertion mode when focus is lost
        /// </summary>
        protected override void OnLostFocus(EventArgs e)
        {
            // base method may destroy parent link, so get parent control first.
            var parent = Parent as TreeGridDesignerTreeControl;

            base.OnLostFocus(e);

            Debug.Assert(parent != null, "parent of TreeGridDesignerInPlaceEdit must be TreeGridDesignerTreeControl");

            if (parent != null
                && parent.InsertMode)
            {
                // tell the branch to end insert mode
                parent.EndInsert();
            }
        }

        /// <summary>
        ///     Forward certain key combinations directly to the text box (e.g. Ctrl-Z)
        /// </summary>
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Z: // ensure that the edit control handles undo.
                    if (((keyData & Keys.Control) != 0)
                        && ((keyData & Keys.Shift) == 0)
                        && ((keyData & Keys.Alt) == 0))
                    {
                        Undo();
                        return true;
                    }
                    break;

                case Keys.A: // ensure that the edit control handles select all.
                    if (((keyData & Keys.Control) != 0)
                        && ((keyData & Keys.Shift) == 0)
                        && ((keyData & Keys.Alt) == 0))
                    {
                        SelectAll();
                        return true;
                    }
                    break;

                case Keys.Delete:
                    NativeMethods.SendMessage(Handle, msg.Msg, msg.WParam.ToInt32(), msg.LParam.ToInt32());
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        ///     Set accessibility name to the name of the column header.
        /// </summary>
        protected override AccessibleObject CreateAccessibilityInstance()
        {
            AccessibleName = GetAccessibleObjectName(Parent as VirtualTreeControl);
            return base.CreateAccessibilityInstance();
        }

        internal static string GetAccessibleObjectName(VirtualTreeControl parent)
        {
            var treeControl = parent;
            if (treeControl != null)
            {
                var headers = treeControl.GetColumnHeaders();
                // Fix for 332947 - Exception thrown when trying to in-place edit a resource name with JAWS running 
                // v-matbir 7/28/04
                if (headers != null)
                {
                    var currentColumn = treeControl.CurrentColumn;
                    Debug.Assert(currentColumn >= 0 && currentColumn < headers.Length, "column index out of range");
                    if (currentColumn >= 0
                        && currentColumn < headers.Length)
                    {
                        return headers[currentColumn].Text;
                    }
                }
            }
            return string.Empty;
        }
    }
}
