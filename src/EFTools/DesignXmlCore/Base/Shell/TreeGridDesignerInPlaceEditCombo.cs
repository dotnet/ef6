// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Shell
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Forms;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;
    using Microsoft.VisualStudio.Data.Tools.Design.XmlCore;

    internal class TreeGridDesignerInPlaceEditCombo : TypeEditorHostListBox
    {
        /// <summary>
        ///     Default constructor.  Users are responsible for filling the item list, no type converter is used.  However,
        ///     the PropertyDescriptor is required in order to get/set the current value when users make a selection from
        ///     the drop-down.
        /// </summary>
        internal TreeGridDesignerInPlaceEditCombo(PropertyDescriptor descriptor, object instance)
            : base(null, descriptor, instance)
        {
        }

        /// <summary>
        ///     Creates a new drop-down list to display the given type converter.
        ///     The type converter must support a standard values collection.
        /// </summary>
        internal TreeGridDesignerInPlaceEditCombo(
            TypeConverter converter, PropertyDescriptor propertyDescriptor, object instance, TypeEditorHostEditControlStyle editStyle)
            : base(converter, propertyDescriptor, instance, editStyle)
        {
        }

        /// <summary>
        ///     Creates a new drop-down list to display the given type converter.
        ///     The type converter must support a standard values collection.
        /// </summary>
        protected internal TreeGridDesignerInPlaceEditCombo(TypeConverter converter, PropertyDescriptor propertyDescriptor, object instance)
            : base(converter, propertyDescriptor, instance)
        {
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "handle")]
        protected override int DropDownHeight
        {
            get
            {
                // Ensure that control handle is created before calling DropDownHeight; otherwise DropDownHeight will just return default height value which might not
                // big enough for VS that uses non standard font.
                if (DropDown.IsHandleCreated == false)
                {
                    var handle = DropDown.Handle; // enforces creation of the handle
                }
                return base.DropDownHeight;
            }
        }

        /// <summary>
        ///     Pass KeyDown events on to the parent control for special handling
        /// </summary>
        /// <param name="e"></param>
        protected override void OnEditKeyDown(KeyEventArgs e)
        {
            var parent = Parent as TreeGridDesignerTreeControl;

            Debug.Assert(parent != null, "parent of TreeGridDesignerInPlaceEdit must be TreeGridDesignerTreeControl");

            // allow Up/Down to go straight to the edit control.  These will change selection
            // in the list box.
            if (parent != null
                && e.KeyCode != Keys.Up
                && e.KeyCode != Keys.Down)
            {
                e.Handled = (ProcessKeyReturn.NotHandled != parent.ProcessKeyDown(e));
            }

            if (!e.Handled)
            {
                base.OnEditKeyDown(e);
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
                base.OnEditKeyPress(e);
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

            // allow Up/Down to go straight to the edit control.  These will change selection
            // in the list box.
            if (parent != null
                && e.KeyCode != Keys.Up
                && e.KeyCode != Keys.Down)
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
            EditAccessibleDescription = Resources.TridDes_ComboAccDesc;
            return base.CreateAccessibilityInstance();
        }
    }
}
