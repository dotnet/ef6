// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Shell
{
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Forms;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;
    using Microsoft.VisualStudio.Data.Tools.Design.XmlCore;

    /// <summary>
    ///     Summary description for TreeGridDesignerBranch.
    /// </summary>
    internal abstract class TreeGridDesignerBranch : IServiceProvider, IBranch, IMultiColumnBranch, ITreeGridDesignerBranch,
                                                     ITreeGridDesignerInitializeBranch
    {
        private TreeGridDesignerColumnDescriptor[] _columns;
        private object _component;
        private bool _readOnly;

        // indices used when the branch is in insertion mode
        private int _insertingIndex = -1;
        private int _insertingCreatorIndex = -1;

        private string _lastText; // cached most recent text.  used in GetDisplayData.

        /// <summary>
        ///     Custom ObjectStyle for use with IBranch.GetObject.
        ///     Provide the object to be pushed to the property browser.
        /// </summary>
        internal static readonly ObjectStyle BrowsingObject = (ObjectStyle)VirtualTreeConstant.FirstUserObjectStyle;

        /// <summary>
        ///     Constant passed to OnCreatorNodeEditCommitted to indicate that
        ///     the value should be appended to the current list, rather than
        ///     inserted at a given position
        /// </summary>
        internal const int AppendIndex = -1;

        #region construction

        /// <summary>
        ///     Create a new branch.
        /// </summary>
        protected TreeGridDesignerBranch(object component, TreeGridDesignerColumnDescriptor[] columns)
        {
            if (columns == null)
            {
                throw new ArgumentNullException("columns");
            }

            _component = component;
            _columns = columns;
        }

        /// <summary>
        ///     Create a new branch.  Initialize must be called to set columns after construction.
        /// </summary>
        protected TreeGridDesignerBranch()
        {
        }

        /// <summary>
        ///     Derived classes may override this to perform initialization.
        /// </summary>
        /// <param name="component">Selected component that this branch may act on.</param>
        /// <param name="columns">columns to act on.</param>
        public virtual bool /* ITreeGridDesignerInitializeBranch */ Initialize(object component, TreeGridDesignerColumnDescriptor[] columns)
        {
            if (columns == null)
            {
                throw new ArgumentNullException("columns");
            }

            _component = component;
            _columns = columns;
            return true;
        }

        #endregion

        #region abstract methods

        /// <summary>
        ///     Returns the element at the given index
        /// </summary>
        /// <param name="index">Index into this branch</param>
        /// <returns></returns>
        internal abstract object GetElement(int index);

        /// <summary>
        ///     Returns the creator element
        /// </summary>
        /// <returns></returns>
        internal abstract object GetCreatorElement();

        /// <summary>
        ///     Returns the index for the given element
        /// </summary>
        /// <param name="element">Element that exists in this branch</param>
        /// <returns>Returns the index for the given element</returns>
        internal abstract int GetIndexForElement(object element);

        /// <summary>
        ///     Count of all elements in this branch
        /// </summary>
        internal abstract int ElementCount { get; }

        #endregion

        #region virtual methods

        /// <summary>
        ///     Returns the branch created by expanding the node at the given index
        /// </summary>
        /// <param name="index">Index into this branch</param>
        /// <returns></returns>
        protected virtual IBranch GetExpandedBranch(int index)
        {
            return null;
        }

        /// <summary>
        ///     Return true if the given child index represents an expandable node
        /// </summary>
        /// <param name="index">Index into this branch</param>
        /// <returns></returns>
        protected virtual bool IsExpandable(int index)
        {
            return false;
        }

        /// <summary>
        ///     Returns the text for the given creator node index (e.g. "&lt;new parameter&gt;")
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Returns the text for the given creator node index.</returns>
        protected virtual string GetCreatorNodeText(int index)
        {
            return String.Empty;
        }

        /// <summary>
        ///     Called when the user commits an edit in a creator node
        /// </summary>
        /// <param name="index">Creator node index (0 is the first creator node)</param>
        /// <param name="value">Resulting value entered by the user</param>
        /// <param name="insertIndex">
        ///     index where newly created node should be inserted.  A value of
        ///     TreeGridDesignerBranch.Append indicates that it should be appended at the end of the list.
        /// </param>
        /// <returns></returns>
        protected virtual LabelEditResult OnCreatorNodeEditCommitted(int index, object value, int insertIndex)
        {
            return LabelEditResult.CancelEdit;
        }

        /// <summary>
        ///     Count of the creator nodes that appear at the end of this branch
        /// </summary>
        internal virtual int CreatorNodeCount
        {
            get { return 0; }
        }

        /// <summary>
        ///     Flags specifying branch behavior
        /// </summary>
        protected virtual BranchFeatures Features
        {
            get
            {
                var features = BranchFeatures.Expansions | BranchFeatures.ImmediateMouseLabelEdits | BranchFeatures.ExplicitLabelEdits
                               | BranchFeatures.InsertsAndDeletes | BranchFeatures.BranchRelocation | BranchFeatures.StateChanges;

                if (CreatorNodeCount > 0)
                {
                    features |= BranchFeatures.JaggedColumns;
                }

                return features;
            }
        }

        /// <summary>
        ///     Returns tool tip text
        /// </summary>
        /// <param name="row">Index into this branch</param>
        /// <param name="column">Column tooltip was requested for</param>
        /// <param name="tipType">Type of tool tip requested</param>
        /// <returns></returns>
        protected virtual string GetTipText(int row, int column, ToolTipType tipType)
        {
            if (tipType == ToolTipType.Default
                && row < ElementCount)
            {
                return _columns[column].GetTipText(GetElement(row));
            }
            return null;
        }

        /// <summary>
        ///     Each node in the tree can have an associated tracking object used to indentify it.
        ///     The branch can override this method to return the row index corresponding to the given
        ///     object.
        /// </summary>
        /// <param name="locateData">
        ///     If the object is a direct child of this branch, set locateData.Options = TrackingObjectAction.ThisLevel.
        ///     If the object is deeper in the tree, the branch should set it to TrackingObjectAction.NextLevel,
        ///     locateData.Row should be set to the row where the search should continue.
        /// </param>
        /// <param name="tracked">Object to track</param>
        protected virtual LocateObjectData LocateTrackingObject(object tracked, LocateObjectData locateData)
        {
            // base implementation handles searching in this branch, but searching 
            // deeper in the tree requires knowledge not available 
            // in the base class.
            locateData.Row = -1;

            locateData.Row = GetIndexForElement(tracked);

            locateData.Options = locateData.Row >= 0 ? (int)TrackingObjectAction.ThisLevel : (int)TrackingObjectAction.NotTracked;

            return locateData;
        }

        /// <summary>
        ///     Returns the object to be pushed to the property browser for the given index.
        ///     The base class just returns the element at that index,
        ///     Derived classes may override this to push custom objects.
        /// </summary>
        protected virtual object GetBrowsingObject(int index)
        {
            // index should be checked prior to calling this method.
            return GetElement(index);
        }

        /// <summary>
        ///     Returns the object to be pushed to the property browser.
        ///     The base class just returns an empty creator element,
        ///     Derived classes may override this to push custom objects.
        /// </summary>
        protected virtual object GetBrowsingCreatorObject()
        {
            return GetCreatorElement();
        }

        /// <summary>
        ///     Gets/sets read-only state of this branch.  Text in read-only branches appears grayed-out
        ///     and cannot be edited.  Creator nodes also do not appear.
        /// </summary>
        public bool /* ITreeGridDesignerBranch */ ReadOnly
        {
            get { return _readOnly; }
            set
            {
                var oldValue = _readOnly;
                _readOnly = value;
                if (oldValue != value)
                {
                    // refresh elements
                    DoBranchModification(BranchModificationEventArgs.DisplayDataChanged(new DisplayDataChangedData(this)));
                }
            }
        }

        /// <summary>
        ///     Allows derived classes to specify supported states for a particular cell.  The base implementation
        ///     takes into account branch-level read-only state, and column value supported state.
        /// </summary>
        public virtual TreeGridDesignerValueSupportedStates /* ITreeGridDesignerBranch */ GetValueSupported(int row, int column)
        {
            var supportedState = TreeGridDesignerValueSupportedStates.Supported;

            if (_readOnly)
            {
                // no editing if we're read only
                supportedState = TreeGridDesignerValueSupportedStates.DisplayReadOnly
                                 | TreeGridDesignerValueSupportedStates.SupportsKeyboardNavigation;
            }
            else if (row < ElementCount)
            {
                var component = GetElement(row);

                // delegate to the column
                supportedState = _columns[column].GetValueSupported(component);
            }
            else if (column != 0)
            {
                supportedState = TreeGridDesignerValueSupportedStates.Unsupported;
                    // creator nodes are only editable/navigable in the first column
            }

            return supportedState;
        }

        /// <summary>
        ///     Returns structure describing how this branch should be displayed.
        /// </summary>
        /// <param name="row">Index into this branch</param>
        /// <param name="column">Column index</param>
        /// <param name="requiredData">The data that must be filled in</param>
        /// <returns>
        ///     Structure to be filled with display information.  By default,
        ///     No icon is specified, and read-only and creator nodes are grayed out.
        /// </returns>
        protected virtual VirtualTreeDisplayData GetDisplayData(int row, int column, VirtualTreeDisplayDataMasks requiredData)
        {
            var data = new VirtualTreeDisplayData(-1);

            // account for row insertion index
            if (_insertingIndex != -1
                && row > _insertingIndex)
            {
                row--;
            }

            var count = ElementCount;

            if ((requiredData.Mask & VirtualTreeDisplayMasks.StateImage) != 0
                && row < count)
            {
                var columnDescriptor = _columns[column];

                if (columnDescriptor.ColumnIsCheckBox)
                {
                    var checkState = columnDescriptor.GetCheckBoxValue(GetElement(row));

                    if (checkState != CheckBoxState.Unsupported)
                    {
                        // set state index
                        data.StateImageIndex = (short)checkState;
                    }
                }
            }

            if (row >= count)
            {
                // display creator nodes in gray
                data.GrayText = true;
            }
            else if (_lastText != null
                     && _lastText.Length > 0)
            {
                // use lastText here as an optimization.  If there's no text, there's no
                // reason to call derived classes to check the value, as GrayText setting is
                // irrelevant
                var supportedState = GetValueSupported(row, column);
                // display non-editable nodes in gray
                if ((supportedState & TreeGridDesignerValueSupportedStates.DisplayReadOnly) != 0)
                {
                    data.GrayText = true;
                }
            }
            return data;
        }

        internal static AccessibilityReplacementField[] _descriptionAccessibilityReplacementFields = new[]
            {
                AccessibilityReplacementField.ColumnHeader,
                AccessibilityReplacementField.DisplayText,
                AccessibilityReplacementField.ChildRowCountText,
                AccessibilityReplacementField.GlobalRowText1,
                AccessibilityReplacementField.GlobalRowAndColumnText1,
                AccessibilityReplacementField.StateImageText
            };

        /// <summary>
        ///     Retrieve accessibility data for the item at the given row, column
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <returns>Populated VirtualTreeAccessibilityData structure, or VirtualTreeAccessibilityData.Empty</returns>
        protected virtual VirtualTreeAccessibilityData GetAccessibilityData(int row, int column)
        {
            if (_columns[column].ColumnIsCheckBox)
            {
                // check box - {column header} {state image text}
                return new VirtualTreeAccessibilityData(
                    "{0} {5}", _descriptionAccessibilityReplacementFields,
                    Resources.MappingDetails_Up_And_Down, new AccessibilityReplacementField[0],
                    Resources.MappingDetails_Up_And_Down);
            }

            var eltCount = ElementCount;
            if (_insertingIndex != -1)
            {
                eltCount++;
            }

            if (row < eltCount)
            {
                // tree accessibility is implemented such that the first column is an outline item, others
                // are cells.  For outline items, we include the display text as part of the name field, because
                // we can't put it in the value field.  For cells, the tree control already supplies the
                // display text as the value, so we don't include this as part of the name field.
                if (column == 0)
                {
                    // standard outline item - {display text} {row} {column header}
                    return new VirtualTreeAccessibilityData(
                        "{1}, {3} {0}", _descriptionAccessibilityReplacementFields,
                        Resources.MappingDetails_Up_And_Down, new AccessibilityReplacementField[0],
                        Resources.MappingDetails_Up_And_Down);
                }
                else
                {
                    // standard cell - {row} {column header}.  Note that text in the cell is already reported in the value field of the accessible object.
                    return new VirtualTreeAccessibilityData(
                        "{3} {0}", _descriptionAccessibilityReplacementFields,
                        Resources.MappingDetails_Up_And_Down, new AccessibilityReplacementField[0],
                        Resources.MappingDetails_Up_And_Down);
                }
            }
            else
            {
                // creator row - {display text} {row}
                return new VirtualTreeAccessibilityData(
                    "{1}, {3}", _descriptionAccessibilityReplacementFields,
                    Resources.MappingDetails_Up_And_Down, new AccessibilityReplacementField[0],
                    Resources.MappingDetails_Up_And_Down);
            }
        }

        /// <summary>
        ///     Retrieve the accessible name for the item at the given row, column
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <returns>Accessible name for the given cell</returns>
        public virtual string GetAccessibleName(int row, int column)
        {
            return String.Empty;
        }

        /// <summary>
        ///     Retrieve the accessible value for the item at the given row, column
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <returns>Accessible value for the given cell</returns>
        public virtual string GetAccessibleValue(int row, int column)
        {
            return GetText(row, column);
        }

        void ITreeGridDesignerBranch.Delete(int row, int column)
        {
            if (row < ElementCount)
            {
                var mappingElement = GetElement(row);
                _columns[column].Delete(mappingElement);
            }
        }

        /// <summary>
        ///     Called to initiate an in-place edit
        /// </summary>
        /// <param name="row">Index into this branch</param>
        /// <param name="column">Column index</param>
        /// <param name="alternateText">
        ///     Text to display when editing commences.  By default, creator nodes
        ///     display the empty string when initially edited
        /// </param>
        /// <param name="maxTextLength">Maximum length of the text to be edited</param>
        /// <param name="customInPlaceEdit"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        protected virtual bool BeginLabelEdit(
            int row, int column, ref string alternateText, ref int maxTextLength, ref object customInPlaceEdit)
        {
            AbortVirtualTreeEdit = false;

            var supportedState = GetValueSupported(row, column);
            if ((supportedState & TreeGridDesignerValueSupportedStates.SupportsInPlaceEdit) != 0)
            {
                customInPlaceEdit = typeof(TreeGridDesignerInPlaceEdit);

                if (row >= ElementCount)
                {
                    var component = GetCreatorElement();
                    if (component != null)
                    {
                        customInPlaceEdit = _columns[column].GetInPlaceEdit(component, ref alternateText);
                        return customInPlaceEdit != null;
                    }

                    alternateText = String.Empty;
                }
                else if (row != _insertingIndex)
                {
                    // We currently also allow branches to return null here to indicate that
                    // they do not support editing.  Perhaps we should change this and require
                    // a value for customInPlaceEdit
                    var component = GetElement(row);
                    customInPlaceEdit = _columns[column].GetInPlaceEdit(component, ref alternateText);
                    return customInPlaceEdit != null;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Returns the text for the given cell
        /// </summary>
        /// <param name="row">Index into this branch</param>
        /// <param name="column">Column index</param>
        /// <returns></returns>
        protected virtual string GetText(int row, int column)
        {
            // account for element insertion mode
            if (_insertingIndex != -1)
            {
                if (row == _insertingIndex)
                {
                    // no need to return text for the insertion row itself
                    return String.Empty;
                }
                else if (row > _insertingIndex)
                {
                    row--;
                }
            }

            var eltCount = ElementCount;
            if (row < eltCount)
            {
                var mappingElement = GetElement(row);
                var value = _columns[column].GetValue(mappingElement);
                // Note: value is actually a MappingLovEFElement
                return value != null ? value.ToString() : String.Empty;
            }
            else if (column == 0)
            {
                return GetCreatorNodeText(row - eltCount);
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        ///     Called when the user commits an in-place edit
        /// </summary>
        /// <param name="row">Index into this branch</param>
        /// <param name="column">Column index</param>
        /// <param name="newText">New value entered by the user</param>
        /// <returns></returns>
        protected virtual LabelEditResult CommitLabelEdit(int row, int column, string newText)
        {
            var result = LabelEditResult.AcceptEdit;
            var eltCount = ElementCount;

            if (row != _insertingIndex
                && (row < eltCount || (row == eltCount && eltCount == 0)))
            {
                var columnDesc = _columns[column];
                Debug.Assert(columnDesc.Converter != null, "Column " + columnDesc.GetType().FullName + " has null Converter");
                if (columnDesc.Converter != null)
                {
                    // convertedObject can just the original string in case of a
                    // edit from the keyboard (which has no context)
                    var convertedObject = columnDesc.Converter.ConvertFrom(newText);
                    Debug.Assert(
                        convertedObject != null, "Column " + columnDesc.GetType().FullName + " could not convert newText " + newText);
                    object component = null;

                    // if we are setting a column value for one that doesn't have a component already, we have to at least 
                    // pass in an 'dummy' component so SetValue knows what type it is.
                    if (row == eltCount
                        && eltCount == 0)
                    {
                        component = GetCreatorElement();
                    }
                    else
                    {
                        component = GetElement(row);
                    }

                    Debug.Assert(component != null, "Why couldn't we find the component for this row in the mapping details view?");
                    columnDesc.SetValue(component, convertedObject);
                }
            }
            else
            {
                try
                {
                    InCreatorNodeCommit = true;
                    result = OnCreatorNodeEditCommitted(
                        _insertingCreatorIndex == -1 ? row - eltCount : _insertingCreatorIndex, newText, _insertingIndex);
                }
                finally
                {
                    InCreatorNodeCommit = false;
                }
            }

            return result;
        }

        /// <summary>
        ///     Count of all items in the branch, includes both elements and creator nodes
        /// </summary>
        internal int ItemCount
        {
            get { return ElementCount + CreatorNodeCount; }
        }

        ProcessKeyResult ITreeGridDesignerBranch.ProcessKeyPress(int row, int column, char key, Keys modifiers)
        {
            // first check that the column allows key processing
            if (_columns[column].AllowKeyPressProcessing(key, modifiers))
            {
                return ProcessKeyPress(row, column, key, modifiers);
            }
            return new ProcessKeyResult(KeyAction.Process);
        }

        /// <summary>
        ///     Handles special key bindings for key press events.
        /// </summary>
        /// <param name="row">Index into this branch</param>
        /// <param name="column">Column index</param>
        /// <param name="key">Character typed by the user</param>
        /// <param name="modifiers">Any modifiers keys currently held down</param>
        /// <returns>ProcessKeyResult structure that indicates what action should be taken (if any)</returns>
        protected virtual ProcessKeyResult ProcessKeyPress(int row, int column, char keyPressed, Keys modifiers)
        {
            return new ProcessKeyResult(KeyAction.Process);
        }

        ProcessKeyResult ITreeGridDesignerBranch.ProcessKeyDown(int row, int column, KeyEventArgs e)
        {
            // first check that the column allows key processing
            object component = null;
            if (row < ElementCount)
            {
                component = GetElement(row);
            }
            if (_columns[column].AllowKeyDownProcessing(e, component))
            {
                return ProcessKeyDown(row, column, e);
            }
            return new ProcessKeyResult(KeyAction.Process);
        }

        /// <summary>
        ///     Handles special key bindings for key down events.  Base class handles space and tab.
        /// </summary>
        /// <param name="row">Index into this branch</param>
        /// <param name="column">Column index</param>
        /// <param name="key">Character typed by the user</param>
        /// <param name="modifiers">Any modifiers keys currently held down</param>
        /// <returns>ProcessKeyResult structure that indicates what action should be taken (if any)</returns>
        protected virtual ProcessKeyResult ProcessKeyDown(int row, int column, KeyEventArgs e)
        {
            var result = new ProcessKeyResult();
            result.Action = KeyAction.Process;
            result.Direction = (e.Modifiers != Keys.Shift) ? NavigationDirection.Right : NavigationDirection.Left;
            switch (e.KeyCode)
            {
                case Keys.F2:
                    result.Action = KeyAction.Handle;
                    result.StartLabelEdit = true;
                    result.Direction = NavigationDirection.Down;
                    break;

                case Keys.Tab:
                    result.Action = KeyAction.Handle;
                    result.ColumnType = null;
                    break;

                case Keys.Space:
                    if (_columns[column].ColumnIsCheckBox || InLabelEdit)
                    {
                        // check box and text box columns should use default 
                        // key processing for the space bar (toggle value)
                        result.Action = KeyAction.Process;
                    }
                    break;

                case Keys.Up:
                    // let Ctrl-Up fall through to the base control processing.
                    if (!e.Control)
                    {
                        result.Action = KeyAction.Handle;
                        result.Local = false;
                        result.Direction = NavigationDirection.Up;
                    }
                    break;

                case Keys.Down:
                    // let Ctrl-Down fall through to the base control processing.
                    if (!e.Control)
                    {
                        result.Action = KeyAction.Handle;
                        result.Local = false;
                        result.Direction = NavigationDirection.Down;
                        // Alt + Down should open a drop down
                        if (e.Alt)
                        {
                            result.StartLabelEdit = true;
                        }
                    }
                    break;
                case Keys.Delete:
                    result.Action = KeyAction.Handle;
                    result.Delete = true;
                    break;
            }
            return result;
        }

        /// <summary>
        ///     Add any event handlers for any events the branch needs to listen to.
        /// </summary>
        protected virtual void AddEventHandlers()
        {
        }

        /// <summary>
        ///     Remove any event handlers previously added
        /// </summary>
        protected virtual void RemoveEventHandlers()
        {
        }

        /// <summary>
        ///     Specifies the default command that should be executed for the specified index.
        ///     The default command is executed when the tree control is double-clicked on.
        /// </summary>
        public virtual CommandID /* ITreeGridDesignerBranch */ GetDefaultAction(int index)
        {
            return null;
        }

        public object /* ITreeGridDesignerBranch */ GetBranchComponent()
        {
            return _component;
        }

        #endregion

        #region Branch functions that defer to protected virtual functions

        BranchFeatures IBranch.Features
        {
            get { return Features; }
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public bool /* IBranch */ IsExpandable(int row, int column)
        {
            // creator nodes, and nodes in columns other than the first, 
            // are not expandable.  For other nodes, we defer to the derived
            // class to determine expandability.
            return (column == 0 && row < ElementCount && IsExpandable(row));
        }

        string IBranch.GetTipText(int row, int column, ToolTipType tipType)
        {
            return GetTipText(row, column, tipType);
        }

        VirtualTreeDisplayData IBranch.GetDisplayData(int row, int column, VirtualTreeDisplayDataMasks requiredData)
        {
            return GetDisplayData(row, column, requiredData);
        }

        VirtualTreeAccessibilityData IBranch.GetAccessibilityData(int row, int column)
        {
            return GetAccessibilityData(row, column);
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public VirtualTreeLabelEditData /* IBranch */ BeginLabelEdit(
            int row, int column, VirtualTreeLabelEditActivationStyles activationStyle)
        {
            string alternateText = null;
            var maxTextLength = 0;
            object customInPlaceEdit = null;
            var result = BeginLabelEdit(row, column, ref alternateText, ref maxTextLength, ref customInPlaceEdit);
            return result
                       ? new VirtualTreeLabelEditData(customInPlaceEdit, null, alternateText, maxTextLength)
                       : VirtualTreeLabelEditData.Invalid;
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public int /* IBranch */ VisibleItemCount
        {
            get { return ItemCount; }
        }

        string IBranch.GetText(int row, int column)
        {
            _lastText = GetText(row, column);
            return _lastText;
        }

        LabelEditResult IBranch.CommitLabelEdit(int row, int column, string newText)
        {
            try
            {
                InVirtualTreeEdit = true;
                return CommitLabelEdit(row, column, newText);
            }
            finally
            {
                InVirtualTreeEdit = false;
                if (_insertingIndex != -1)
                {
                    // reset insertion indices
                    _insertingIndex = -1;
                    _insertingCreatorIndex = -1;
                }
            }
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public LocateObjectData /* IBranch */ LocateObject(object obj, ObjectStyle style, int locateOptions)
        {
            var locateData = new LocateObjectData(-1, 0, 0);
            switch (style)
            {
                case ObjectStyle.ExpandedBranch:
                case ObjectStyle.SubItemExpansion:
                    locateData.Options = (int)BranchLocationAction.DiscardBranch;
                    break;

                case ObjectStyle.TrackingObject:
                    locateData = LocateTrackingObject(obj, locateData);
                    break;
            }
            return locateData;
        }

        #endregion

        /// <summary>
        ///     IMultiColumnBranch interface implementation.
        /// </summary>
        public int /* IMultiColumnBranch */ ColumnCount
        {
            get { return _columns.Length; }
        }

        /// <summary>
        ///     IMultiColumnBranch interface implementation.
        /// </summary>
        public SubItemCellStyles /* IMultiColumnBranch */ ColumnStyles(int column)
        {
            return SubItemCellStyles.Simple;
        }

        /// <summary>
        ///     IMultiColumnBranch interface implementation.
        /// </summary>
        public int /* IMultiColumnBranch */ GetJaggedColumnCount(int row)
        {
            if (row < ElementCount)
            {
                return _columns.Length;
            }
            else
            {
                return 1; // creator nodes only support a single column
            }
        }

        public object /* IBranch */ GetObject(int row, int column, ObjectStyle style, ref int options)
        {
            switch ((int)style)
            {
                case (int)ObjectStyle.ExpandedBranch:
                    {
                        try
                        {
                            // HACK: demand loading can cause re-entrancy when we access this list for the first time.
                            InVirtualTreeEdit = true;

                            Debug.Assert(column == 0); // Simple-celled columns shouldn't ask for an expansion
                            // Use the override to get the appropriate branch
                            var newBranch = GetExpandedBranch(row);
                            if (newBranch != null)
                            {
                                return newBranch;
                            }
                        }
                        finally
                        {
                            InVirtualTreeEdit = false;
                        }
                        break;
                    }

                case (int)ObjectStyle.TrackingObject:
                    if (row < ElementCount)
                    {
                        return GetElement(row);
                    }
                    options = (int)TrackingObjectAction.NotTracked;
                    break;

                default:
                    if (style == BrowsingObject)
                    {
                        if (row < ElementCount)
                        {
                            return GetBrowsingObject(row);
                        }
                        else
                        {
                            return GetBrowsingCreatorObject();
                        }
                    }
                    break;
            }
            return null;
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public int /* IBranch */ UpdateCounter
        {
            get { return 0; }
        }

        /// <summary>
        ///     Called by the tree when item state is toggled (by clicking on the state icon, etc.).  Used to support checkboxes
        /// </summary>
        public StateRefreshChanges /* IBranch */ ToggleState(int row, int column)
        {
            try
            {
                InVirtualTreeEdit = true;

                var columnDescriptor = _columns[column];

                if (columnDescriptor.ColumnIsCheckBox)
                {
                    return columnDescriptor.ToggleCheckBoxValue(GetElement(row));
                }
            }
            finally
            {
                InVirtualTreeEdit = false;
            }
            return StateRefreshChanges.None;
        }

        /// <summary>
        ///     Called by the tree when the tree is in multiselect node and a state is toggled. Used to support synchronizing checkbox states.
        /// </summary>
        public StateRefreshChanges /* IBranch */ SynchronizeState(int row, int column, IBranch matchBranch, int matchRow, int matchColumn)
        {
            // UNDONE: Do something here to support synchronizing multiselect checkbox states
            return StateRefreshChanges.None;
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public virtual VirtualTreeStartDragData /* IBranch */ OnStartDrag(object sender, int row, int column, DragReason reason)
        {
            return VirtualTreeStartDragData.Empty;
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public virtual void /* IBranch */ OnGiveFeedback(GiveFeedbackEventArgs args, int row, int column)
        {
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public virtual void /* IBranch */ OnQueryContinueDrag(QueryContinueDragEventArgs args, int row, int column)
        {
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public virtual void /* IBranch */ OnDragEvent(object sender, int row, int column, DragEventType eventType, DragEventArgs args)
        {
        }

        public event BranchModificationEventHandler /* IBranch */ OnBranchModification
        {
            add
            {
                if (_onBranchModification == null)
                {
                    AddEventHandlers();
                }
                _onBranchModification += value;
            }
            remove
            {
                _onBranchModification -= value;
                if (_onBranchModification == null)
                {
                    RemoveEventHandlers();
                }
            }
        }

        private event BranchModificationEventHandler _onBranchModification;

        #region helper methods

        /// <summary>
        ///     Should be called by derived classes to indicate that a branch modification (add, remove, reorder) has occurred.
        /// </summary>
        /// <param name="modification">Indicates the type of modification</param>
        protected void DoBranchModification(BranchModificationEventArgs modification)
        {
            if (_onBranchModification != null)
            {
                _onBranchModification(this, modification);
            }
        }

        internal static bool InVirtualTreeEdit { get; set; }

        // set when a branch edit is aborted (because an incorrect value was entered, for example). 
        // used by the tool window to know whether to cancel in the case that the commit was a 
        // result of a CommitPendingEdit call. 
        internal static bool AbortVirtualTreeEdit { get; set; }

        /// <summary>
        ///     May be used by derived classes to determine whether a creator node edit is currently in progress for this branch.
        ///     This information may be required by event handlers, which may not have enough context to determine this themselves.
        /// </summary>
        protected bool InCreatorNodeCommit { get; private set; }

        /// <summary>
        ///     Returns the array of columns currently supported by this branch.
        /// </summary>
        /// <returns></returns>
        protected TreeGridDesignerColumnDescriptor[] GetColumns()
        {
            return _columns;
        }

        /// <summary>
        ///     Allows access to services
        /// </summary>
        public object /* IServiceProvider */ GetService(Type serviceType)
        {
            if (_columns.Length > 0
                && _columns[0].Host != null)
            {
                return _columns[0].Host.GetService(serviceType);
            }

            return null;
        }

        protected bool InLabelEdit
        {
            get
            {
                if (_columns.Length > 0
                    && _columns[0].Host != null)
                {
                    return _columns[0].Host.InLabelEdit;
                }

                return false;
            }
        }

        #endregion

        #region ITreeGridDesignerInitializeBranch

        bool ITreeGridDesignerInitializeBranch.Initialize(object selection, TreeGridDesignerColumnDescriptor[] columns)
        {
            return Initialize(selection, columns);
        }

        #endregion

        /// <summary>
        ///     Indicates that a new creator node should be inserted at the given index
        /// </summary>
        /// <param name="absIndex">index to insert</param>
        /// <param name="creatorNodeIndex">specifies the creator node to insert</param>
        public void /* ITreeGridDesignerBranch */ InsertCreatorNode(int row, int creatorNodeIndex)
        {
            if (creatorNodeIndex >= CreatorNodeCount)
            {
                throw new ArgumentOutOfRangeException("creatorNodeIndex");
            }

            // set insertion indices
            _insertingIndex = row;
            _insertingCreatorIndex = creatorNodeIndex;
        }

        /// <summary>
        ///     Called to indicate that the branch should end insert mode.
        /// </summary>
        /// <param name="row">row index specifying the insertion</param>
        public void /* ITreeGridDesignerBranch */ EndInsert(int row)
        {
            if (_insertingIndex != -1)
            {
                // reset insertion indices
                _insertingIndex = -1;
                _insertingCreatorIndex = -1;
            }
        }

        public virtual void OnColumnValueChanged(TreeGridDesignerBranchChangedArgs args)
        {
            if (args.InsertingItem)
            {
                DoBranchModification(BranchModificationEventArgs.InsertItems(this, args.Row, 1));
            }
            else if (args.DeletingItem)
            {
                DoBranchModification(BranchModificationEventArgs.DeleteItems(this, args.Row, 1));
            }
            else
            {
                DoBranchModification(
                    BranchModificationEventArgs.DisplayDataChanged(
                        new DisplayDataChangedData(VirtualTreeDisplayDataChanges.All, this, args.Row, args.Column, 1)));
            }
        }
    }
}
