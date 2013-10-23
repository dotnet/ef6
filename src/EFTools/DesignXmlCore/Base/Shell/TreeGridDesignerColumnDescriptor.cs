// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Shell
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Forms;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;
    using Microsoft.VisualStudio.Modeling;

    /// <summary>
    ///     Represents a tri-state checkbox
    /// </summary>
    internal enum CheckBoxState
    {
        Unsupported = -1,
        // values below correspond to indexes into the state image list
        Checked = StandardCheckBoxImage.Checked,
        Unchecked = StandardCheckBoxImage.Unchecked,
        Indeterminate = StandardCheckBoxImage.Indeterminate,
        Inactive = StandardCheckBoxImage.Inactive,
        CheckedDisabled = StandardCheckBoxImage.CheckedDisabled,
        UncheckedDisabled = StandardCheckBoxImage.UncheckedDisabled
    }

    /// <summary>
    ///     Class which represents a column in the tree.  Derived from property descriptor so it can
    ///     be used in the property grid as well.
    /// </summary>
    internal abstract class TreeGridDesignerColumnDescriptor : PropertyDescriptor, IDisposable
    {
        private float _initialPercentage; // initial percentage width of this column

        /// <summary>
        ///     Create a new column descriptor.
        /// </summary>
        protected TreeGridDesignerColumnDescriptor(string name)
            : this(name, CalculatePercentage)
        {
        }

        /// <summary>
        ///     Create a new column descriptor.
        /// </summary>
        protected TreeGridDesignerColumnDescriptor(string name, float initialPercentage)
            : base(name, null)
        {
            _initialPercentage = initialPercentage;
        }

        /// <summary>
        ///     Initial width of this column, as a percentage of the total width.  A value of
        ///     The default value of ColumnDescriptor.CalculatePercentage indicates that the percentage should be calculated by the tree control.
        ///     Otherwise, the value should be in the range (0, 1).
        /// </summary>
        internal float InitialPercentage
        {
            get { return GetWidthPercentage(); }
            set { _initialPercentage = value; }
        }

        protected virtual float GetWidthPercentage()
        {
            return _initialPercentage;
        }

        /// <summary>
        ///     Returns the supported state for the given component.  This state may be
        ///     a combination of the TreeGridDesignerValueSupportedStates values, which
        ///     determine properties such as whether a particular cell supports an in-place
        ///     editor, and whether it allows keyboard navigation.
        /// </summary>
        /// <param name="component">object instance</param>
        /// <returns></returns>
        internal virtual TreeGridDesignerValueSupportedStates GetValueSupported(object component)
        {
            return TreeGridDesignerValueSupportedStates.Supported;
        }

        /// <summary>
        ///     Returns the in-place editor for the given component.  May be either a type,
        ///     which will be instantiated, or an instance.  Must implement
        ///     IVirtualTreeInPlaceControl.  Base implementation checks to see if a TypeEditorHost (drop-down)
        ///     can be created via TreeGridDesignerTreeControl.CreateTypeEditorHost, otherwise, returns a
        ///     text box as the default edit control.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        internal virtual object GetInPlaceEdit(object component, ref string alternateText)
        {
            // perhaps we can get a custom editor via a UITypeEditor or TypeConverter
            var dropDown = TreeGridDesignerTreeControl.CreateTypeEditorHost(this, component);
            if (dropDown != null)
            {
                // set flag to dismiss the label edit when the drop down closes.
                dropDown.DismissLabelEditOnDropDownClose = true;
                return dropDown;
            }

            // default editor type (text box)
            return typeof(TreeGridDesignerInPlaceEdit);
        }

        /// <summary>
        ///     Called to delete the component for this column
        /// </summary>
        /// <param name="component"></param>
        internal abstract void Delete(object component);

        /// <summary>
        ///     Gets/sets the host used to provide services to this column
        /// </summary>
        internal ITreeGridDesignerColumnHost Host { get; set; }

        /// <summary>
        ///     Called to indicate that the column should add any necessary event handlers
        /// </summary>
        internal virtual void AddEventHandlers()
        {
        }

        /// <summary>
        ///     Called to indicate that the column should remove its event handlers
        /// </summary>
        internal virtual void RemoveEventHandlers()
        {
        }

        /// <summary>
        ///     Specify this if a check box should be displayed in the column
        /// </summary>
        internal virtual bool ColumnIsCheckBox
        {
            get { return false; }
        }

        /// <summary>
        ///     Allows columns to specify whether they want to allow special key processing
        ///     to occur for a given key down event (i.e., special keystrokes for typing signatures).  Return true to
        ///     allow this processing, false to use the default key processing provided by
        ///     the control.  Default value is false (allow special processing).
        /// </summary>
        internal virtual bool AllowKeyDownProcessing(KeyEventArgs e, object component)
        {
            return true;
        }

        /// <summary>
        ///     Allows columns to specify whether they want to allow special key processing
        ///     to occur for a given key press event (i.e., special keystrokes for typing signatures).  Return true to
        ///     allow this processing, false to use the default key processing provided by
        ///     the control.  Default value is false (allow special processing).
        /// </summary>
        internal virtual bool AllowKeyPressProcessing(char key, Keys modifiers)
        {
            return true;
        }

        /// <summary>
        ///     Derived classes that specify ColumnIsCheckBox should override this
        ///     to determine the check box state for the given component
        /// </summary>
        internal virtual CheckBoxState GetCheckBoxValue(object component)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Derived classes that specify ColumnIsCheckBox should override this
        ///     to toggle the check box state for the given component
        /// </summary>
        internal virtual StateRefreshChanges ToggleCheckBoxValue(object component)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Allows derived classes to specify a transaction name when a check box
        ///     in a header branch is toggled.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        internal virtual string GetCheckBoxTransactionName(object component)
        {
            return String.Empty;
        }

        internal const int CalculatePercentage = -1;

        /// <summary>
        ///     Allows columns to specify custom tooltip text.  Default behavior is to return null, which specifies that
        ///     the same text that is displayed in the column should be used
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        internal virtual string GetTipText(object component)
        {
            return null;
        }

        #region IDisposable

        public void /* IDisposable */ Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TreeGridDesignerColumnDescriptor()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Column descriptors may override this to do required cleanup.  Called when the tree control
        ///     owning the column is disposed.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
        }

        #endregion

        #region Abstract property descriptor methods not required for columns

        /// <summary>
        ///     Column descriptors current do not use this property, derived classes
        ///     may override if they need to use the property grid
        /// </summary>
        public override Type /* PropertyDescriptor */ ComponentType
        {
            get { return typeof(ModelElement); }
        }

        /// <summary>
        ///     Column descriptors current do not use this property, derived classes
        ///     may override if they need to use the property grid
        /// </summary>
        public override Type /* PropertyDescriptor */ PropertyType
        {
            get { return typeof(string); }
        }

        /// <summary>
        ///     Column descriptors current do not use this property, derived classes
        ///     may override if they need to use the property grid
        /// </summary>
        public override bool /* PropertyDescriptor */ IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        ///     Column descriptors current do not use this method, derived classes
        ///     may override if they need to use the property grid
        /// </summary>
        public override bool /* PropertyDescriptor */ ShouldSerializeValue(object component)
        {
            return false;
        }

        /// <summary>
        ///     Column descriptors current do not use this method, derived classes
        ///     may override if they need to use the property grid
        /// </summary>
        public override bool /* PropertyDescriptor */ CanResetValue(object component)
        {
            return false;
        }

        /// <summary>
        ///     Column descriptors current do not use this method, derived classes
        ///     may override if they need to use the property grid
        /// </summary>
        public override void /* PropertyDescriptor */ ResetValue(object component)
        {
        }

        #endregion
    }
}
