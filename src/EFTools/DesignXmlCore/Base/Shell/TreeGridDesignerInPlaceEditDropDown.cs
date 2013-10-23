// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Shell
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing.Design;
    using System.Windows.Forms;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;
    using Microsoft.VisualStudio.Data.Tools.Design.XmlCore;

    internal class TreeGridDesignerInPlaceEditDropDown : TypeEditorHost
    {
        /// <summary>
        ///     Creates a new drop-down control to display the given UITypeEditor
        /// </summary>
        protected internal TreeGridDesignerInPlaceEditDropDown(
            UITypeEditor editor, PropertyDescriptor propertyDescriptor, object instance, TypeEditorHostEditControlStyle editStyle)
            : base(editor, propertyDescriptor, instance, editStyle)
        {
        }

        /// <summary>
        ///     Creates a new drop-down control to display the given UITypeEditor
        /// </summary>
        protected internal TreeGridDesignerInPlaceEditDropDown(UITypeEditor editor, PropertyDescriptor propertyDescriptor, object instance)
            : base(editor, propertyDescriptor, instance)
        {
        }

        /// <summary>
        ///     Pass KeyDown events on to the parent control for special handling
        /// </summary>
        /// <param name="e"></param>
        protected override void OnEditKeyDown(KeyEventArgs e)
        {
            var parent = Parent as TreeGridDesignerTreeControl;

            Debug.Assert(parent != null, "parent of TreeGridDesignerInPlaceEdit must be TreeGridDesignerTreeControl");

            if (parent != null)
            {
                e.Handled = (ProcessKeyReturn.NotHandled != parent.ProcessKeyDown(e));
            }

            if (!e.Handled)
            {
                base.OnKeyDown(e);
            }
        }

        /// <summary>
        ///     Pass KeyPress events on to the parent control for special handling
        /// </summary>
        /// <param name="e"></param>
        protected override void OnEditKeyPress(KeyPressEventArgs e)
        {
            var parentControl = Parent;
            var parent = parentControl as TreeGridDesignerTreeControl;

            Debug.Assert(parent != null, "parent of TreeGridDesignerInPlaceEdit must be TreeGridDesignerTreeControl");

            if (parent != null)
            {
                e.Handled = (ProcessKeyReturn.NotHandled != parent.ProcessKeyPress(e.KeyChar, ModifierKeys));
            }

            if (!e.Handled)
            {
                base.OnKeyPress(e);
            }
        }

        /// <summary>
        ///     Pass KeyDown events on to the parent control for special handling
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            // this override gets called when there's no edit control, i.e., when the TransparentEditRegion flag is set.
            var parent = Parent as TreeGridDesignerTreeControl;

            Debug.Assert(parent != null, "parent of TreeGridDesignerInPlaceEdit must be TreeGridDesignerTreeControl");

            // allow Alt-Down to go straight to the edit control.  This opens the drop-down
            if (parent != null
                && !(e.KeyCode == Keys.Down && e.Alt))
            {
                e.Handled = (ProcessKeyReturn.NotHandled != parent.ProcessKeyDown(e));
            }

            if (!e.Handled)
            {
                base.OnKeyDown(e);
            }
        }

        /// <summary>
        ///     Pass KeyPress events on to the parent control for special handling
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            // this override gets called when there's no edit control, i.e., when the TransparentEditRegion flag is set.
            var parentControl = Parent;
            var parent = parentControl as TreeGridDesignerTreeControl;

            Debug.Assert(parent != null, "parent of TreeGridDesignerInPlaceEdit must be TreeGridDesignerTreeControl");

            if (parent != null)
            {
                e.Handled = (ProcessKeyReturn.NotHandled != parent.ProcessKeyPress(e.KeyChar, ModifierKeys));
            }

            if (!e.Handled)
            {
                base.OnKeyPress(e);
            }
        }

        /// <summary>
        ///     Set accessibility name to the name of the column header.
        /// </summary>
        protected override AccessibleObject CreateAccessibilityInstance()
        {
            // set edit control accessible name because this is the focused item.
            EditAccessibleName = TreeGridDesignerInPlaceEdit.GetAccessibleObjectName(Parent as VirtualTreeControl);
            EditAccessibleDescription = Resources.TridDes_LauncherAccDesc;
            return base.CreateAccessibilityInstance();
        }
    }
}
