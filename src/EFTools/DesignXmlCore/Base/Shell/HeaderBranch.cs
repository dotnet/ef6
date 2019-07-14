// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Shell
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Windows.Forms;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;
    using Microsoft.VisualStudio.Data.Tools.Design.XmlCore;

    /// <summary>
    ///     simple branch that exposes a set of child branches via headers
    /// </summary>
    internal class HeaderBranch : IServiceProvider, IBranch, IMultiColumnBranch, ITreeGridDesignerBranch, ITreeGridDesignerInitializeBranch
    {
        private ChildBranchInfo[] _childBranchArray;

        // contains branches currently in use.  headers are not displayed for child branches with no items.
        private List<ChildBranchInfo> _currentBranches;

        private TreeGridDesignerColumnDescriptor[] _columns; // current columns
        private object _component;

        protected object Component
        {
            get { return _component; }
            set { _component = value; }
        }

        /// <summary>
        ///     ITreeGridDesignerInitializeBranch
        /// </summary>
        public virtual bool Initialize(object component, TreeGridDesignerColumnDescriptor[] columns)
        {
            if (columns == null)
            {
                throw new ArgumentNullException("columns");
            }

            _component = component;
            _columns = columns;
            return true;
        }

        /// <summary>
        ///     Sets header info for this branch.
        /// </summary>
        internal void SetHeaderInfo(IList childBranchList, TreeGridDesignerColumnDescriptor[] columns)
        {
            if (childBranchList == null)
            {
                throw new ArgumentNullException("childBranchList");
            }

            _childBranchArray = new ChildBranchInfo[childBranchList.Count];
            childBranchList.CopyTo(_childBranchArray, 0);

            _columns = columns;

            _currentBranches = new List<ChildBranchInfo>(_childBranchArray.Length);
            for (var i = 0; i < _childBranchArray.Length; i++)
            {
                _currentBranches.Add(_childBranchArray[i]);
            }
        }

        /// <summary>
        ///     Allows access to services
        ///     IServiceProvider
        /// </summary>
        public object GetService(Type serviceType)
        {
            if (_columns.Length > 0
                && _columns[0].Host != null)
            {
                return _columns[0].Host.GetService(serviceType);
            }

            return null;
        }

        /// <summary>
        ///     Retrieves display data for the header row.
        /// </summary>
        protected virtual VirtualTreeDisplayData GetDisplayData(int row, int column, VirtualTreeDisplayDataMasks requiredData)
        {
            var data = new VirtualTreeDisplayData(-1);

            if (column == 0)
            {
                data.Bold = true;
            }

            if ((requiredData.Mask & VirtualTreeDisplayMasks.StateImage) != 0
                && row < _currentBranches.Count)
            {
                var columnDescriptor = _columns[column];

                if (columnDescriptor.ColumnIsCheckBox)
                {
                    var checkState = GetCheckBoxValue(row, column);

                    if (checkState != CheckBoxState.Unsupported)
                    {
                        // set state index
                        data.StateImageIndex = (short)checkState;
                    }
                }
            }

            return data;
        }

        /// <summary>
        ///     Retrieve accessibility data for the item at the given row, column
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <returns>Populated VirtualTreeAccessibilityData structure, or VirtualTreeAccessibilityData.Empty</returns>
        protected virtual VirtualTreeAccessibilityData GetAccessibilityData(int row, int column)
        {
            // initial header column - {display text} {row} {child count}
            if (column == 0)
            {
                return new VirtualTreeAccessibilityData(
                    "{1}, {3} {2}", TreeGridDesignerBranch._descriptionAccessibilityReplacementFields,
                    Resources.MappingDetails_Up_And_Down, new AccessibilityReplacementField[0],
                    Resources.MappingDetails_Up_And_Down);
            }

            // header check box columns
            if (_columns[column].ColumnIsCheckBox
                && GetCheckBoxValue(row, column) != CheckBoxState.Unsupported)
            {
                // check box - {column header} {state image text} {row}
                return new VirtualTreeAccessibilityData(
                    "{0} {5} {3}", TreeGridDesignerBranch._descriptionAccessibilityReplacementFields,
                    Resources.MappingDetails_Up_And_Down, new AccessibilityReplacementField[0],
                    Resources.MappingDetails_Up_And_Down);
            }

            // other header columns - {row} {column header)
            return new VirtualTreeAccessibilityData(
                "{3} {0}", TreeGridDesignerBranch._descriptionAccessibilityReplacementFields,
                Resources.MappingDetails_Up_And_Down, new AccessibilityReplacementField[0],
                Resources.MappingDetails_Up_And_Down);
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
            return String.Empty;
        }

        /// <summary>
        ///     Derived classes may override to provide custom object location.  The base class
        ///     provides implementation for ExpandedBranch and TrackingObject styles
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="style"></param>
        protected virtual object GetObject(int row, int column, ObjectStyle style)
        {
            switch (style)
            {
                case ObjectStyle.ExpandedBranch:
                    Debug.Assert(column == 0); // Simple-celled columns shouldn't ask for an expansion
                    return _currentBranches[row].Branch;

                case ObjectStyle.TrackingObject:
                    return _currentBranches[row].Id;

                default:
                    if (style == TreeGridDesignerBranch.BrowsingObject)
                    {
                        goto case ObjectStyle.TrackingObject;
                    }
                    break;
            }
            return null;
        }

        #region IBranch Members

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public BranchFeatures Features
        {
            get
            {
                return BranchFeatures.Expansions | BranchFeatures.StateChanges | BranchFeatures.BranchRelocation
                       | BranchFeatures.InsertsAndDeletes;
            }
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public int VisibleItemCount
        {
            get { return _currentBranches.Count; }
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public object GetObject(int row, int column, ObjectStyle style, ref int options)
        {
            return GetObject(row, column, style);
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public virtual LocateObjectData LocateObject(object obj, ObjectStyle style, int locateOptions)
        {
            var row = -1;
            var column = 0;
            var options = 0;
            switch (style)
            {
                case ObjectStyle.ExpandedBranch:
                case ObjectStyle.SubItemExpansion:
                    options = (int)BranchLocationAction.DiscardBranch;
                    for (var i = 0; i < _currentBranches.Count; i++)
                    {
                        if (_currentBranches[i].Branch == obj)
                        {
                            return new LocateObjectData(i, 0, (int)BranchLocationAction.KeepBranch);
                        }
                    }
                    break;

                case ObjectStyle.TrackingObject:
                    for (var i = 0; i < _currentBranches.Count; i++)
                    {
                        if (obj != null
                            && obj == _currentBranches[i].Id)
                        {
                            return new LocateObjectData(i, 0, (int)TrackingObjectAction.ThisLevel);
                        }

                        var data = _currentBranches[i].Branch.LocateObject(obj, ObjectStyle.TrackingObject, locateOptions);
                        if (data.Options != (int)TrackingObjectAction.NotTracked)
                        {
                            return new LocateObjectData(i, 0, (int)TrackingObjectAction.NextLevel);
                        }
                    }
                    break;
            }
            return new LocateObjectData(row, column, options);
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public string GetText(int row, int column)
        {
            if (column == 0)
            {
                Debug.Assert(row < _currentBranches.Count);
                return _currentBranches[row].Name;
            }

            return String.Empty;
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public string GetTipText(int row, int column, ToolTipType tipType)
        {
            return null;
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public bool IsExpandable(int row, int column)
        {
            return column == 0;
        }

        VirtualTreeDisplayData IBranch.GetDisplayData(int row, int column, VirtualTreeDisplayDataMasks requiredData)
        {
            return GetDisplayData(row, column, requiredData);
        }

        VirtualTreeAccessibilityData IBranch.GetAccessibilityData(int row, int column)
        {
            return GetAccessibilityData(row, column);
        }

        public VirtualTreeLabelEditData BeginLabelEdit(int row, int column, VirtualTreeLabelEditActivationStyles activationStyle)
        {
            // label editing not supported for headers
            throw new NotImplementedException();
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public LabelEditResult CommitLabelEdit(int row, int column, string newText)
        {
            // label editing not supported for headers
            throw new NotImplementedException();
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public int UpdateCounter
        {
            get { return 0; }
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public StateRefreshChanges ToggleState(int row, int column)
        {
            if (row < 0
                || row >= _currentBranches.Count)
            {
                throw new ArgumentException("Invalid value for row");
            }
            if (column < 0
                || column >= _columns.Length)
            {
                throw new ArgumentException("Invalid value for columns");
            }

            var columnDescriptor = _columns[column];

            if (columnDescriptor.ColumnIsCheckBox)
            {
                try
                {
                    TreeGridDesignerBranch.InVirtualTreeEdit = true;
                    var checkState = GetCheckBoxValue(row, column);
                    if (checkState != CheckBoxState.Unsupported)
                    {
                        // checkboxes in header branches work differently.  The value is set 
                        // by toggling the values of all child branches
                        checkState = checkState == CheckBoxState.Checked ? CheckBoxState.Unchecked : CheckBoxState.Checked;
                        var branch = _currentBranches[row].Branch as TreeGridDesignerBranch;
                        if (branch != null)
                        {
                            for (var i = 0; i < branch.ElementCount; i++)
                            {
                                var component = branch.GetElement(i);
                                var childCheckState = columnDescriptor.GetCheckBoxValue(component);
                                if (childCheckState != CheckBoxState.Unsupported
                                    && childCheckState != checkState)
                                {
                                    columnDescriptor.ToggleCheckBoxValue(component);
                                }
                            }

                            return StateRefreshChanges.Children;
                        }
                    }
                }
                finally
                {
                    TreeGridDesignerBranch.InVirtualTreeEdit = false;
                }
            }

            return StateRefreshChanges.None;
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public StateRefreshChanges SynchronizeState(int row, int column, IBranch matchBranch, int matchRow, int matchColumn)
        {
            // UNDONE: Do something here to support synchronizing multiselect checkbox states
            return StateRefreshChanges.None;
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public void OnDragEvent(object sender, int row, int column, DragEventType eventType, DragEventArgs args)
        {
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public VirtualTreeStartDragData OnStartDrag(object sender, int row, int column, DragReason reason)
        {
            return VirtualTreeStartDragData.Empty;
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public void OnGiveFeedback(GiveFeedbackEventArgs args, int row, int column)
        {
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public void OnQueryContinueDrag(QueryContinueDragEventArgs args, int row, int column)
        {
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public event BranchModificationEventHandler OnBranchModification
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

        #endregion

        #region IMultiColumnBranch Members

        /// <summary>
        ///     IMultiColumnBranch interface implementation.
        /// </summary>
        public int ColumnCount
        {
            get { return _columns.Length; }
        }

        public SubItemCellStyles ColumnStyles(int column)
        {
            return SubItemCellStyles.Simple;
        }

        /// <summary>
        ///     IMultiColumnBranch interface implementation.
        /// </summary>
        public int GetJaggedColumnCount(int row)
        {
            Debug.Assert(false); // Should not be called unless TreeFlags.JaggedColumnCount is set
            return _columns.Length;
            ;
        }

        #endregion

        #region helper methods

        private CheckBoxState GetCheckBoxValue(int row, int column)
        {
            var state = CheckBoxState.Unsupported;
            var columnDescriptor = _columns[column];
            var branch = _currentBranches[row].Branch as TreeGridDesignerBranch;

            if (branch != null)
            {
                for (var i = 0; i < branch.ElementCount; i++)
                {
                    var component = branch.GetElement(i);
                    var newState = columnDescriptor.GetCheckBoxValue(component);
                    if (state == CheckBoxState.Unsupported)
                    {
                        // initial state.  set state to whatever was found in
                        // the child column
                        state = newState;
                    }
                    else if (state == CheckBoxState.Checked
                             || state == CheckBoxState.Unchecked)
                    {
                        // states differ, but both are supported
                        // indicates an indeterminate state
                        if (state != newState
                            && newState != CheckBoxState.Unsupported)
                        {
                            return CheckBoxState.Indeterminate;
                        }
                    }
                }
            }

            return state;
        }

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

        #endregion

        #region event handlers

        protected virtual void AddEventHandlers()
        {
            // attach event handlers to child branches to support header checkboxes
            foreach (var column in _columns)
            {
                if (column.ColumnIsCheckBox)
                {
                    foreach (var branchInfo in _childBranchArray)
                    {
                        // attach event handler to child branch
                        branchInfo.ChildModificationHandler = OnChildBranchModification;
                    }
                }
            }
        }

        protected virtual void RemoveEventHandlers()
        {
            // remove any child branch event handlers we added
            foreach (var branchInfo in _childBranchArray)
            {
                branchInfo.ChildModificationHandler = null;
            }
        }

        private void OnChildBranchModification(object sender, BranchModificationEventArgs e)
        {
            if (e.Action == BranchModificationAction.DeleteItems
                ||
                e.Action == BranchModificationAction.InsertItems)
            {
                // this can also cause changes in column check box state.  Find and refresh appropriate row
                for (var i = 0; i < _childBranchArray.Length; i++)
                {
                    if (_childBranchArray[i].Branch == e.Branch)
                    {
                        if (_onBranchModification != null)
                        {
                            _onBranchModification(
                                this,
                                BranchModificationEventArgs.DisplayDataChanged(
                                    new DisplayDataChangedData(VirtualTreeDisplayDataChanges.StateImage, this, i, -1, 1)));
                        }
                    }
                }
            }
        }

        #endregion

        #region ITreeGridDesignerBranch Members

        /// <summary>
        ///     Process key press events.
        /// </summary>
        public ProcessKeyResult ProcessKeyPress(int row, int column, char keyPressed, Keys modifiers)
        {
            return new ProcessKeyResult(KeyAction.Process);
        }

        /// <summary>
        ///     Process key down events.
        /// </summary>
        public ProcessKeyResult ProcessKeyDown(int row, int column, KeyEventArgs e)
        {
            var result = new ProcessKeyResult(KeyAction.Process);
            if (e != null)
            {
                switch (e.KeyCode)
                {
                    case Keys.Tab:
                        result.Action = KeyAction.Handle;
                        result.Direction = !e.Shift ? NavigationDirection.Right : NavigationDirection.Left;
                        result.Local = false;
                        return result;
                    case Keys.Space:
                        if (_columns[column].ColumnIsCheckBox)
                        {
                            // if we're on a check box, use standard processing
                            return new ProcessKeyResult(KeyAction.Process);
                        }

                        // special case for only one checkbox in the row,
                        // it should be toggled no matter where the focus is.
                        var checkBoxIndex = -1;
                        for (var i = 0; i < _columns.Length; i++)
                        {
                            if (_columns[i].ColumnIsCheckBox
                                && (GetCheckBoxValue(row, i) != CheckBoxState.Unsupported))
                            {
                                if (checkBoxIndex == -1)
                                {
                                    checkBoxIndex = i;
                                }
                                else
                                {
                                    // more than one checkbox, use standard processing
                                    return new ProcessKeyResult(KeyAction.Process);
                                }
                            }

                            if (checkBoxIndex != -1)
                            {
                                ((IBranch)this).ToggleState(row, checkBoxIndex);

                                // need to refresh both ourselves and our children.  Do we need BranchModificationAction.ToggleState?
                                if (_onBranchModification != null)
                                {
                                    _onBranchModification(
                                        this,
                                        BranchModificationEventArgs.DisplayDataChanged(
                                            new DisplayDataChangedData(
                                                VirtualTreeDisplayDataChanges.StateImage, this, row, checkBoxIndex, 1)));
                                    _onBranchModification(
                                        this,
                                        BranchModificationEventArgs.DisplayDataChanged(
                                            new DisplayDataChangedData(
                                                VirtualTreeDisplayDataChanges.StateImage, _childBranchArray[row].Branch, -1, checkBoxIndex,
                                                -1)));
                                }

                                return new ProcessKeyResult(KeyAction.Discard); // we've handled this, no further processing necessary
                            }
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
                        }
                        break;
                }
            }

            return result;
        }

        /// <summary>
        ///     Returns true iff the given row/column are editable.
        /// </summary>
        public virtual TreeGridDesignerValueSupportedStates GetValueSupported(int row, int column)
        {
            // checkboxes in header branches support keyboard navigation
            var columnDescriptor = _columns[column];
            if (columnDescriptor.ColumnIsCheckBox
                && (GetCheckBoxValue(row, column) != CheckBoxState.Unsupported))
            {
                return TreeGridDesignerValueSupportedStates.SupportsKeyboardNavigation;
            }

            // otherwise nothing special supported.
            return TreeGridDesignerValueSupportedStates.None;
        }

        /// <summary>
        ///     Returns the default action when row is double-clicked on.
        /// </summary>
        public CommandID GetDefaultAction(int index)
        {
            return null;
        }

        /// <summary>
        ///     Header branches don't support creator nodes.  Unused.
        /// </summary>
        public void InsertCreatorNode(int index, int creatorNodeIndex)
        {
            // header branches don't support creator nodes
        }

        /// <summary>
        ///     Header branches don't support creator nodes.  Unused.
        /// </summary>
        public void EndInsert(int row)
        {
            // header branches don't support creator nodes
        }

        /// <summary>
        ///     Header branches don't support deleting. Unused.
        /// </summary>
        void ITreeGridDesignerBranch.Delete(int row, int column)
        {
            // header branches don't support deleting nodes
        }

        /// <summary>
        ///     Unused.
        /// </summary>
        public bool ReadOnly
        {
            get
            {
                // header branch doesn't support read-only
                return false;
            }
            set
            {
                // header branch doesn't support read-only
            }
        }

        public object GetBranchComponent()
        {
            return _component;
        }

        #endregion

        public void OnColumnValueChanged(TreeGridDesignerBranchChangedArgs args)
        {
            Debug.Fail("There should be no changes to the header branch");
        }
    }
}
