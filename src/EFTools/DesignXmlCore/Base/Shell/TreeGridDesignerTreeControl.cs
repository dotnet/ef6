// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Shell
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing.Design;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Common;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;
    using Microsoft.VisualStudio.Modeling.Diagrams;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    ///     Class derived from VirtualTreeControl containing TreeGrid designer-specific code
    ///     such as special key bindings
    /// </summary>
    internal class TreeGridDesignerTreeControl : VirtualTreeControl
    {
        private TreeGridDesignerColumnDescriptor[] _currentColumns;
        private TreeGridDesignerColumnDescriptor[] _defaultColumns;

        // used to keep track of the current branch/index
        // during an insert operation
        private ITreeGridDesignerBranch _insertBranch;
        private int _insertIndex;

        // used to batch up OnDrawItem calls
        private bool _batchDrawItem;
        private readonly ArrayList _invalidItems = new ArrayList();

        // used to ignore OnDrawItem calls completely

        // maps TreeGridDesignerColumnDescriptor types to instances
        private HybridDictionary _columnTable;

        // provides services to column objects
        private ITreeGridDesignerColumnHost _columnHost;

        /// <summary>
        /// </summary>
        /// <param name="columns"></param>
        internal TreeGridDesignerTreeControl(ITreeGridDesignerColumnHost columnHost)
        {
            _columnTable = new HybridDictionary(5);
            _columnHost = columnHost;
        }

        /// <summary>
        ///     Call Dispose on all columns.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    // dispose of columns
                    if (_columnTable != null)
                    {
                        foreach (TreeGridDesignerColumnDescriptor column in _columnTable.Values)
                        {
                            column.Dispose();
                        }
                    }
                }

                if (_columnTable != null)
                {
                    _columnTable.Clear();
                    _columnTable = null;
                }

                _columnHost = null;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        ///     Override the header control creation so we can style it
        /// </summary>
        /// <returns>A new header control</returns>
        protected override VirtualTreeHeaderControl CreateHeaderControl()
        {
            var headerControl = base.CreateHeaderControl();

            // Theme the header control with shell colors
            var uiShell5 = Site.GetService(typeof(SVsUIShell)) as IVsUIShell5;

            if (uiShell5 != null)
            {
                uiShell5.ThemeWindow(headerControl.Handle);
            }

            return headerControl;
        }

        /// <summary>
        ///     Overridden to use IUIService for displaying error messages to the user, and to
        ///     prevent CheckoutException.Canceled from being redisplayed to user
        /// </summary>
        /// <param name="ex">an exception thrown to the underlying VirtualTreeControl</param>
        /// <returns>Return true to swallow an exception, false to rethrow it.</returns>
        protected override bool DisplayException(Exception ex)
        {
            // set "abort" flag
            TreeGridDesignerBranch.AbortVirtualTreeEdit = true;

            // don't display "Checkout Canceled". User doesn't need to be told again that he/she canceled Checkout
            if (ex == CheckoutException.Canceled)
            {
                return true;
            }

            if (Site != null)
            {
                var uiService = Site.GetService(typeof(IUIService)) as IUIService;

                if (uiService != null)
                {
                    uiService.ShowError(ex.Message);
                    return true;
                }
            }

            Debug.Fail("unable to retrieve IUIService interface to show error message: " + ex.Message);
            return base.DisplayException(ex);
        }

        /// <summary>
        ///     Allows a set of default columns.  If no columns are specified via attributes on the object
        ///     passed to PopulateTree, these default columns will be used.
        /// </summary>
        /// <param name="defaultColumns"></param>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        internal ICollection DefaultColumns
        {
            get { return ArrayList.ReadOnly(_defaultColumns); }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _defaultColumns = new TreeGridDesignerColumnDescriptor[value.Count];
                value.CopyTo(_defaultColumns, 0);
                for (var i = 0; i < _defaultColumns.Length; i++)
                {
                    var columnType = _defaultColumns[i].GetType();
                    if (_columnTable[columnType] == null)
                    {
                        _columnTable[columnType] = _defaultColumns[i];
                        _defaultColumns[i].Host = _columnHost;
                    }
                }
            }
        }

        /// <summary>
        ///     Set the columns to be displayed by the tree control
        /// </summary>
        /// <param name="columns"></param>
        internal void SetColumns(TreeGridDesignerColumnDescriptor[] columns)
        {
            if (columns == null)
            {
                throw new ArgumentNullException("columns");
            }

            // the current column may now be out of range.  
            // reset to 0 if that is the case
            if (CurrentColumn >= columns.Length)
            {
                CurrentColumn = 0;
            }

            _currentColumns = columns;

            // do initial percentage calculations.
            var percentages = new float[columns.Length];
            var calculatedPercentage = 1.0F;
            var calculatedColumns = columns.Length;
            for (var i = 0; i < columns.Length; i++)
            {
                var columnPercentage = columns[i].InitialPercentage;
                if (columnPercentage != TreeGridDesignerColumnDescriptor.CalculatePercentage)
                {
                    if (columnPercentage > calculatedPercentage)
                    {
                        percentages[i] = -1;
                        continue; // using the initial percentage specified by this column would cause all
                        // percentages to sum to more than 1, so we discard it.
                    }

                    calculatedPercentage -= columnPercentage;
                    calculatedColumns--;
                    percentages[i] = columnPercentage;
                }
                else
                {
                    percentages[i] = -1;
                }
            }

            // calculate percentage increment used for columns in which the percentage
            // is not specified explicitly
            var defaultPercentageIncr = calculatedColumns == 0 ? 0 : calculatedPercentage / calculatedColumns;

            // create headers
            var headers = new VirtualTreeColumnHeader[columns.Length];

            float percentage = 0;
            for (var i = 0; i < headers.Length - 1; ++i)
            {
                if (percentages[i] == -1)
                {
                    percentage += defaultPercentageIncr;
                }
                else
                {
                    percentage += percentages[i];
                }

                if (columns[i].ColumnIsCheckBox)
                {
                    headers[i] = new VirtualTreeColumnHeader(columns[i].Name, percentage, StateImageList.ImageSize.Width);
                }
                else
                {
                    headers[i] = new VirtualTreeColumnHeader(columns[i].Name, percentage);
                }
            }
            headers[headers.Length - 1] = new VirtualTreeColumnHeader(columns[headers.Length - 1].Name, 1f);

            SetColumnHeaders(headers, false);
        }

        /// <summary>
        ///     Enables event handlers for all columns in the tree
        /// </summary>
        internal void AddColumnEventHandlers()
        {
            // add column event handlers
            foreach (TreeGridDesignerColumnDescriptor column in _columnTable.Values)
            {
                column.AddEventHandlers();
            }
        }

        /// <summary>
        ///     Disables event handlers for all columns in the tree
        /// </summary>
        internal void RemoveColumnEventHandlers()
        {
            // remove column event handlers
            foreach (TreeGridDesignerColumnDescriptor column in _columnTable.Values)
            {
                column.RemoveEventHandlers();
            }
        }

        /// <summary>
        ///     Populates the tree via attributes specifed on th root object.  This object usually corresponds to
        ///     the selection made in a designer.
        /// </summary>
        /// <returns>True if the tree was populated.  False if the object passed in doesn't specify any branches.</returns>
        internal bool PopulateTree(object root)
        {
            object attributeOwner;
            ArrayList rootBranches = null;

            var tree = Tree;
            if (tree != null
                && root != null)
            {
                // determine which columns should be shown
                var attributes = FindAttributes(root, typeof(TreeGridDesignerColumnAttribute), out attributeOwner);
                TreeGridDesignerColumnDescriptor[] newColumns = null;

                if (attributes != null
                    && attributes.Length > 0)
                {
                    var currentColumn = 0;

                    newColumns = new TreeGridDesignerColumnDescriptor[attributes.Length];
                    Array.Sort(attributes);
                    foreach (TreeGridDesignerColumnAttribute attribute in attributes)
                    {
                        // find the column descriptor in the cache or create it
                        newColumns[currentColumn++] = FindOrCreateColumn(attribute.ColumnType);
                    }
                }

                if (newColumns == null
                    && _defaultColumns != null)
                {
                    newColumns = _defaultColumns;
                }

                if (newColumns != null)
                {
                    // determine which root nodes should be shown
                    attributes = FindAttributes(root, typeof(TreeGridDesignerRootBranchAttribute), out attributeOwner);
                    if (attributes.Length > 0)
                    {
                        Array.Sort(attributes);
                        rootBranches = new ArrayList(attributes.Length);
                        foreach (TreeGridDesignerRootBranchAttribute attribute in attributes)
                        {
                            var branchType = attribute.BranchType;

                            // support types that implement IBranch with a default constructor
                            var rootBranch = Activator.CreateInstance(branchType) 
                                as ITreeGridDesignerInitializeBranch;
                            if (rootBranch != null)
                            {
                                if (rootBranch.Initialize(attributeOwner, newColumns))
                                {
                                    // Only add in the branch if it can be successfully initialized. (Orphaned PELs can't)
                                    rootBranches.Add(rootBranch);
                                }
                            }
                        }

                        if (rootBranches.Count > 0)
                        {
                            // we have at least one root branch, now adjust columns accordingly
                            var refreshColumns = (_currentColumns == null) || (newColumns.Length != _currentColumns.Length);

                            if (!refreshColumns)
                            {
                                for (var i = 0; i < _currentColumns.Length && !refreshColumns; i++)
                                {
                                    refreshColumns = _currentColumns[i] != newColumns[i];
                                }
                            }

                            // refresh columns if necessary
                            if (refreshColumns)
                            {
                                _currentColumns = newColumns;

                                var variableColumnTree = tree as VariableColumnTree;

                                Debug.Assert(variableColumnTree != null, "unable to change column count.");
                                if (variableColumnTree != null)
                                {
                                    variableColumnTree.ChangeColumnCount(_currentColumns.Length);
                                    SetColumns(_currentColumns);
                                }
                            }

                            // root tree at branch
                            tree.Root = rootBranches.Count == 1 ? (IBranch)rootBranches[0] : new AggregateBranch(rootBranches, 0);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private object[] FindAttributes(object root, Type attributeType, out object attributeOwner)
        {
            attributeOwner = root;
            object[] attributes = null;

            // first try the selected object itself
            attributes = root.GetType().GetCustomAttributes(attributeType, false);

            if (attributes == null || attributes.Length == 0)
            {
                var pe = root as PresentationElement;
                if (pe != null)
                {
                    // try the corresponding MEL if we have a PEL
                    if (pe.ModelElement != null)
                    {
                        attributes = pe.ModelElement.GetType().GetCustomAttributes(attributeType, false);
                        attributeOwner = pe.ModelElement;
                    }
                }
            }

            if (_testAggregateBranch && attributeType == typeof(TreeGridDesignerRootBranchAttribute))
            {
                // clone branch attributes to allow AggregateBranch testing
                var testAttributes = new object[attributes.Length * 2];
                attributes.CopyTo(testAttributes, 0);
                attributes.CopyTo(testAttributes, attributes.Length);
                return testAttributes;
            }

            return attributes;
        }

        private TreeGridDesignerColumnDescriptor FindOrCreateColumn(Type columnType)
        {
            // find the column descriptor in the cache or create it
            var column = (TreeGridDesignerColumnDescriptor)_columnTable[columnType];

            if (column == null)
            {
                var constructor = columnType.GetConstructor(new Type[0]);
                column = constructor.Invoke(null) as TreeGridDesignerColumnDescriptor;

                Debug.Assert(column != null, "column type could not be created:" + columnType);

                if (column != null)
                {
                    if (_columnHost != null)
                    {
                        column.AddEventHandlers();
                    }

                    _columnTable[columnType] = column;
                    column.Host = _columnHost;
                }
            }

            return column;
        }

        /// <summary>
        ///     Overridden to preprocess key messages.
        /// </summary>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            try
            {
                // This requires some explanation.  WinForms uses this method to pre-process key messages, performing actions
                // such as moving focus around a dialog when Tab is pressed.  Returning true here causes message processing/translation to halt.
                // There are three possible results of our keyboard processing:
                // 1.  Branch doesn't handle the key
                // 2.  Branch indicates it handles the key, but due to tree structure, no handling occurs.
                //     A common example is pressing Tab on the last column of the last row.
                // 3.  Branch indicates it handles the key, and handling occurs.
                // For (1), we want base WinForms processing to continue as usual.  For (2), base preprocessing should
                // continue, but we don't want any further translation/processing of the message after that.  This allows
                // things like dialog focus movement to work correctly, but prevents the keystroke from showing up in in-place edit controls.
                // For (3), we just return true, as we've fully handled the key.
                var retVal = ProcessKeyDown(new KeyEventArgs(keyData));
                if (retVal != ProcessKeyReturn.KeyHandledActionOccurred)
                {
                    var baseRetVal = base.ProcessDialogKey(keyData);
                    return baseRetVal || retVal == ProcessKeyReturn.KeyHandledNoAction;
                }

                return true;
            }
            catch (Exception e)
            {
                if (CriticalException.IsCriticalException(e))
                {
                    throw;
                }

                DisplayException(e);
                return false;
            }
        }

        /// <summary>
        ///     Overridden to preprocess key messages.
        /// </summary>
        protected override bool ProcessDialogChar(char c)
        {
            try
            {
                // See comment in ProcessDialogKey.
                var retVal = ProcessKeyPress(c, ModifierKeys);
                if (retVal != ProcessKeyReturn.KeyHandledActionOccurred)
                {
                    var baseRetVal = base.ProcessDialogChar(c);
                    return baseRetVal || retVal == ProcessKeyReturn.KeyHandledNoAction;
                }

                return true;
            }
            catch (Exception e)
            {
                if (CriticalException.IsCriticalException(e))
                {
                    throw;
                }

                DisplayException(e);
                return false;
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (ProcessKeyReturn.NotHandled == ProcessKeyPress(e.KeyChar, ModifierKeys))
            {
                //base.OnKeyPress(e);
            }
            else
            {
                e.Handled = true;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (ProcessKeyReturn.NotHandled == ProcessKeyDown(e))
            {
                base.OnKeyDown(e);
            }
            else
            {
                e.Handled = true;
            }
        }

        /// <summary>
        ///     Overridden to run the default action specified by the currently selected branch.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDoubleClick(DoubleClickEventArgs e)
        {
            if (e.HitInfo.HitTarget == VirtualTreeHitTargets.OnItemIcon)
            {
                var branch = e.ItemInfo.Branch as ITreeGridDesignerBranch;

                if (branch != null
                    && Site != null
                    && e.ItemInfo.Row != -1)
                {
                    if (DoDefaultAction(branch, e.ItemInfo.Row))
                    {
                        return;
                    }
                }
            }

            base.OnDoubleClick(e);
        }

        protected override void OnSelectionChanged(EventArgs e)
        {
            base.OnSelectionChanged(e);

            // check for creator node selection.  In this case, we want
            // to enter edit mode.  Insert mode is a special case, edit 
            // mode will be handled by the tree control itself.
            if (CurrentColumn == 0
                && !InsertMode
                && ContainsFocus)
            {
                var info = Tree.GetItemInfo(CurrentIndex, 0, false);
                var tridBranch = info.Branch as TreeGridDesignerBranch;

                if (tridBranch != null)
                {
                    var eltCount = tridBranch.ElementCount;
                    if (info.Row >= eltCount
                        && info.Row - eltCount < tridBranch.CreatorNodeCount)
                    {
                        InLabelEdit = true;
                    }
                }
            }
        }

        private bool DoDefaultAction(ITreeGridDesignerBranch branch, int relIndex)
        {
            var command = branch.GetDefaultAction(relIndex);

            if (command != null)
            {
                var menuCommandService = Site.GetService(typeof(IMenuCommandService)) as IMenuCommandService;

                if (menuCommandService != null)
                {
                    menuCommandService.GlobalInvoke(command);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     retrieves the column index given a type.  Assumes there
        ///     will only be a single instance of each type in the columns array.
        ///     returns -1 if the column is not found
        /// </summary>
        private int FindCurrentColumnOfType(ProcessKeyResult result)
        {
            var index = -1;

            int i;
            for (i = 0; i < _currentColumns.Length; i++)
            {
                var columnType = _currentColumns[i].GetType();
                if (result.ColumnType == null
                    || columnType == result.ColumnType
                    || columnType.IsSubclassOf(result.ColumnType))
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        private static TreeNavigation TranslateNavigationDirection(NavigationDirection treeNav)
        {
            switch (treeNav)
            {
                case NavigationDirection.Left:
                    return TreeNavigation.LeftColumn;
                case NavigationDirection.Right:
                    return TreeNavigation.RightColumn;
                case NavigationDirection.Up:
                    return TreeNavigation.Up;
                case NavigationDirection.Down:
                    return TreeNavigation.Down;
            }

            return default(TreeNavigation);
        }

        /// <summary>
        ///     Gets the last column index for the given branch/row.  Accounts for
        ///     jagged columns
        /// </summary>
        private static int GetLastColumnIndex(IBranch branch, int relIndex)
        {
            var multiBranch = branch as IMultiColumnBranch;
            if (multiBranch != null)
            {
                if ((branch.Features & BranchFeatures.JaggedColumns) != 0)
                {
                    return multiBranch.GetJaggedColumnCount(relIndex) - 1;
                }
                else
                {
                    return multiBranch.ColumnCount - 1;
                }
            }

            return 0;
        }

        internal ProcessKeyReturn ProcessKeyPress(char keyPressed, Keys modifiers)
        {
            if (CurrentIndex >= 0) // can't do any processing if there's no current selection
            {
                var absIndex = CurrentIndex;
                var info = Tree.GetItemInfo(absIndex, 0, false);
                var branch = info.Branch as ITreeGridDesignerBranch;
                var column = CurrentColumn;
                var relIndex = info.Row;

                if (branch != null)
                {
                    var result = branch.ProcessKeyPress(relIndex, column, keyPressed, modifiers);
                    if (ProcessKey(result, branch, absIndex, relIndex, column))
                    {
                        return ProcessKeyReturn.KeyHandledActionOccurred;
                    }
                    else if (result.Action == KeyAction.Handle)
                    {
                        return ProcessKeyReturn.KeyHandledNoAction;
                    }
                }
            }

            return ProcessKeyReturn.NotHandled;
        }

        internal ProcessKeyReturn ProcessKeyDown(KeyEventArgs e)
        {
            if (CurrentIndex >= 0) // can't do any processing if there's no current selection
            {
                var absIndex = CurrentIndex;
                var info = Tree.GetItemInfo(absIndex, 0, false);
                var branch = info.Branch as ITreeGridDesignerBranch;
                var column = CurrentColumn;
                var relIndex = info.Row;

                if (branch != null)
                {
                    // handle default action at the control level, do not route
                    // through the branch.
                    if (e.KeyCode == Keys.Enter
                        && !InLabelEdit
                        && DoDefaultAction(branch, relIndex))
                    {
                        return ProcessKeyReturn.KeyHandledActionOccurred;
                    }
                    var result = branch.ProcessKeyDown(relIndex, column, e);
                    if (ProcessKey(result, branch, absIndex, relIndex, column))
                    {
                        return ProcessKeyReturn.KeyHandledActionOccurred;
                    }
                    else if (result.Action == KeyAction.Handle)
                    {
                        return ProcessKeyReturn.KeyHandledNoAction;
                    }
                }
            }

            return ProcessKeyReturn.NotHandled;
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private bool ProcessKey(ProcessKeyResult result, ITreeGridDesignerBranch branch, int absIndex, int relIndex, int column)
        {
            var actionOccurred = false;
            var foundEditableColumn = false;
            var inLabelEdit = InLabelEdit;

            if (result.Action == KeyAction.Discard)
            {
                return true;
            }

            if (result.Action == KeyAction.Handle)
            {
                var treeDirection = TranslateNavigationDirection(result.Direction);
                if (result.StartLabelEdit)
                {
                    // key should just put us in edit mode
                    InLabelEdit = true;
                    // Alt + Down case - open drop down
                    if (result.Direction == NavigationDirection.Down)
                    {
                        var dropDown = LabelEditControl as TypeEditorHost;
                        if (dropDown != null)
                        {
                            dropDown.OpenDropDown();
                        }
                    }
                    return true; // currently, we don't allow combining this with other options
                }

                if (result.Delete)
                {
                    branch.Delete(relIndex, column);
                    // don't restore edit mode if we deleted something
                    inLabelEdit = false;
                }

                BatchDrawItem = true;
                if (inLabelEdit)
                {
                    var tridDesignerBranch = branch as TreeGridDesignerBranch;

                    if (tridDesignerBranch != null
                        && relIndex >= tridDesignerBranch.ElementCount)
                    {
                        // creator node edit.  unless the user has actually typed some text here,
                        // we treat this as non-edit mode (i.e, don't restore edit mode after navigation)
                        inLabelEdit = LabelEditControl.Text.Length > 0;
                    }
                }

                try
                {
                    InLabelEdit = false;

                    // the branch may block deactivation of the edit control,
                    // because of an incorrect entry, for example.  In that case,
                    // we should just get out.
                    if (InLabelEdit)
                    {
                        return false;
                    }

                    // expand branch first, if necesary
                    if (result.ExpandBranch)
                    {
                        // using column = 0 because TreeGrid designer
                        // doesn't support sub-item expansions.
                        if (!Tree.IsExpanded(absIndex, 0))
                        {
                            // the branch is requesting an expansion, but we can't do it.
                            // just get out and leave things the way they are.
                            if (!Tree.IsExpandable(absIndex, 0))
                            {
                                return false;
                            }

                            Tree.ToggleExpansion(absIndex, 0);
                            actionOccurred = true;
                        }
                    }

                    var branchType = branch.GetType();

                    if (result.ColumnType != null)
                    {
                        // limit search to a particular column
                        var newColumn = FindCurrentColumnOfType(result);

                        Debug.Assert(newColumn != -1, "Couldn't find column of type: " + result.ColumnType);
                        if ((treeDirection == TreeNavigation.RightColumn && column < newColumn)
                            || (treeDirection == TreeNavigation.LeftColumn && column > newColumn))
                        {
                            // in this case, we're done as long as the branch is of the appropriate type and supports navigation or is expandable, because we have
                            // the correct row/column indices.
                            column = newColumn;
                            foundEditableColumn = (result.BranchType == null || branchType == result.BranchType
                                                   || branchType.IsSubclassOf(result.BranchType)) &&
                                                  ((branch.GetValueSupported(relIndex, column)
                                                    & TreeGridDesignerValueSupportedStates.SupportsKeyboardNavigation) != 0
                                                   || (branch as IBranch).IsExpandable(relIndex, column));
                        }

                        if (!foundEditableColumn)
                        {
                            // need to do additional search, translate to an up or down search in this particular column
                            column = newColumn;
                            treeDirection = treeDirection == TreeNavigation.RightColumn ? TreeNavigation.Down : TreeNavigation.Up;
                        }
                    }

                    if (result.Delete)
                    {
                        // we are already focused on an editable column
                        InvalidateItem(CurrentIndex);
                        actionOccurred = true;
                    }
                    else
                    {
                        // search for next matching row/column
                        int oldAbsIndex;
                        while (!foundEditableColumn)
                        {
                            oldAbsIndex = absIndex;
                            if (treeDirection == TreeNavigation.LeftColumn
                                && column == 0)
                            {
                                absIndex--;
                                if (absIndex >= 0)
                                {
                                    var info = Tree.GetItemInfo(absIndex, 0, false);
                                    column = GetLastColumnIndex(info.Branch, info.Row);
                                }
                            }
                            else if (treeDirection == TreeNavigation.RightColumn
                                     && column == GetLastColumnIndex((IBranch)branch, relIndex))
                            {
                                absIndex++;
                                column = 0;
                            }
                            else if (result.ColumnType == null)
                            {
                                // search is not restricted to a particular column, so we translate up/down to 
                                // left/right search to give a better experience.
                                if (treeDirection == TreeNavigation.Up)
                                {
                                    absIndex--;
                                    treeDirection = TreeNavigation.LeftColumn;

                                    if (absIndex >= 0)
                                    {
                                        // handle jagged column cases
                                        var info = Tree.GetItemInfo(absIndex, 0, false);
                                        if (info.Branch != null)
                                        {
                                            column = GetLastColumnIndex(info.Branch, info.Row);
                                        }
                                    }
                                }
                                else if (treeDirection == TreeNavigation.Down)
                                {
                                    absIndex++;
                                    treeDirection = TreeNavigation.RightColumn;
                                    column = 0;
                                }
                            }

                            if (absIndex < 0
                                || absIndex >= Tree.VisibleItemCount)
                            {
                                break;
                            }

                            var coordinate = new VirtualTreeCoordinate(absIndex, column);
                            if (absIndex == oldAbsIndex)
                            {
                                // if the above didn't result in any navigation, ask the tree to do it itself.
                                coordinate = Tree.GetNavigationTarget(treeDirection, absIndex, column, ColumnPermutation);
                            }

                            if (coordinate.IsValid)
                            {
                                absIndex = coordinate.Row;
                                column = coordinate.Column;

                                if (oldAbsIndex != absIndex)
                                {
                                    // we've transitioned to a new row, retrieve new row data from the tree.
                                    var info = Tree.GetItemInfo(absIndex, 0, false);

                                    if (result.Local
                                        && branch != null
                                        && branch != info.Branch)
                                    {
                                        // stop search if we shouldn't go past current branch
                                        break;
                                    }

                                    branch = info.Branch as ITreeGridDesignerBranch;
                                    branchType = branch.GetType();
                                    relIndex = info.Row;
                                }

                                // allow focus on expandable cells or cells that support navigation that are of the appropriate branch type.  
                                if (branch != null
                                    &&
                                    (result.BranchType == null || branchType == result.BranchType
                                     || branchType.IsSubclassOf(result.BranchType))
                                    &&
                                    ((branch.GetValueSupported(relIndex, column)
                                      & TreeGridDesignerValueSupportedStates.SupportsKeyboardNavigation) != 0
                                     || (branch as IBranch).IsExpandable(relIndex, column)))
                                {
                                    foundEditableColumn = true;
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (foundEditableColumn)
                    {
                        var currentIndex = CurrentIndex;

                        if (absIndex != currentIndex)
                        {
                            if (currentIndex != -1)
                            {
                                // currentIndex may be -1 if we toggled expansion,
                                // but in that case the base control will take care
                                // of the redraw
                                // TODO : is this a bug in the control?  shouldn't selection
                                // be restored after toggling expansion?
                                InvalidateItem(currentIndex);
                            }

                            CurrentIndex = absIndex;
                            actionOccurred = true;
                        }

                        if (column != CurrentColumn)
                        {
                            CurrentColumn = column;
                            actionOccurred = true;
                        }
                    }

                    InLabelEdit = inLabelEdit;
                }
                finally
                {
                    BatchDrawItem = false;
                }
            }
            return actionOccurred;
        }

        /// <summary>
        ///     If BatchDrawItem == true, store the index of the item to be drawn,
        ///     but dont actually do the drawing until BatchDrawItem is set to false.
        /// </summary>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (IgnoreDrawItem)
            {
                return; // return immediately if IgnoreDrawItem is set.
            }

            if (BatchDrawItem)
            {
                if (!_invalidItems.Contains(e.Index))
                {
                    _invalidItems.Add(e.Index);
                }
                return;
            }

            base.OnDrawItem(e);
        }

        /// <summary>
        ///     If set to true, draw item messages will be batched up by the control,
        ///     then sent when BatchDrawItem is set to false
        /// </summary>
        internal bool BatchDrawItem
        {
            get { return _batchDrawItem; }
            set
            {
                _batchDrawItem = value;
                if (!_batchDrawItem
                    && _invalidItems.Count > 0)
                {
                    if (Tree != null)
                    {
                        var visibleItemCount = Tree.VisibleItemCount;
                        // invalidate items that need to be redrawn all at once
                        foreach (int i in _invalidItems)
                        {
                            if (i < visibleItemCount)
                            {
                                InvalidateItem(i);
                            }
                        }
                    }

                    _invalidItems.Clear();
                }
            }
        }

        /// <summary>
        ///     If set to true, draw item messages will be discarded by the control
        /// </summary>
        internal bool IgnoreDrawItem { get; set; }

        #region helper methods

        /// <summary>
        /// </summary>
        internal VirtualTreeItemInfo SelectedItemInfo
        {
            get
            {
                var selectedIndex = CurrentIndex;
                if (selectedIndex != -1
                    && selectedIndex < Tree.VisibleItemCount)
                {
                    return Tree.GetItemInfo(selectedIndex, 0, false);
                }

                // selectedIndex == -1 means there is no selection in the control.
                // There is also one known case where selectedIndex >= VisibleItemCount, which can happen 
                // if you query this property during a refresh of the control.  See bug 422687.
                // just return empty item info structure for these cases.
                return new VirtualTreeItemInfo();
            }
        }

        /// <summary>
        ///     Invalidate a specific row in the tree
        /// </summary>
        /// <param name="absIndex">index of the row to invalidate</param>
        internal void InvalidateItem(int absIndex)
        {
            if (absIndex < 0
                || (Tree != null && absIndex >= Tree.VisibleItemCount))
            {
                throw new ArgumentOutOfRangeException("absIndex");
            }

            NativeMethods.Rectangle rect;
            NativeMethods.SendMessage(Handle, NativeMethods.LB_GETITEMRECT, absIndex, out rect);
            NativeMethods.RedrawWindow(
                Handle, ref rect, IntPtr.Zero, NativeMethods.RedrawWindowFlags.Invalidate | NativeMethods.RedrawWindowFlags.Erase);
        }

        internal bool InsertMode
        {
            get { return _insertBranch != null; }
        }

        internal void InsertCreatorNode(int absIndex, int creatorNodeIndex)
        {
            if (absIndex < 0
                || (Tree != null && absIndex >= Tree.VisibleItemCount))
            {
                throw new ArgumentOutOfRangeException("absIndex");
            }

            if (Tree == null)
            {
                Debug.Fail("this.Tree == null");
                return;
            }

            var info = Tree.GetItemInfo(absIndex, 0, false);
            var branch = info.Branch as ITreeGridDesignerBranch;

            Debug.Assert(branch != null, "can only insert into branches that implement ITreeGridDesignerBranch");

            if (branch != null)
            {
                // keep track of the branch so we can call it to end insertion mode later
                _insertBranch = branch;
                _insertIndex = info.Row;

                // inform the branch of the insertion
                branch.InsertCreatorNode(info.Row, creatorNodeIndex);

                // redraw appropriate portions of the tree
                Tree.DisplayDataChanged(
                    new DisplayDataChangedData(
                        VirtualTreeDisplayDataChanges.VisibleElements, info.Branch, _insertIndex, -1,
                        info.Branch.VisibleItemCount - _insertIndex));

                // enter edit mode
                CurrentIndex = absIndex;
                InLabelEdit = true;
            }
        }

        internal void EndInsert()
        {
            if (_insertBranch != null)
            {
                try
                {
                    // tell branch to cancel the insert
                    _insertBranch.EndInsert(_insertIndex);

                    // redraw appropriate portions of the tree
                    var branch = (IBranch)_insertBranch;
                    Tree.DisplayDataChanged(
                        new DisplayDataChangedData(
                            VirtualTreeDisplayDataChanges.VisibleElements, branch, _insertIndex, -1, branch.VisibleItemCount - _insertIndex));
                }
                finally
                {
                    _insertBranch = null;
                    _insertIndex = -1;
                }
            }
        }

        /// <summary>
        ///     Factory method for creating the appropriate drop-down control based on the given property descriptor.  Always
        ///     creates a TypeEditorHost with the TypeEditorHostEditControlStyle.Editable style.
        /// </summary>
        /// <param name="propertyDescriptor">
        ///     Property descriptor used to create the drop-down.  If the property descriptor supports a UITypeEditor,
        ///     that will be used first.  Otherwise, the type converter will be used.
        /// </param>
        /// <param name="instance">Instance of the object being edited.</param>
        /// <returns>A DropDownControl instance if the given property descriptor supports it, null otherwise.</returns>
        internal static TypeEditorHost CreateTypeEditorHost(PropertyDescriptor propertyDescriptor, object instance)
        {
            TypeEditorHost dropDown = null;

            if (propertyDescriptor != null)
            {
                var uiTypeEditor = propertyDescriptor.GetEditor(typeof(UITypeEditor)) as UITypeEditor;
                if (uiTypeEditor != null) // UITypeEditor case
                {
                    dropDown = new TreeGridDesignerInPlaceEditDropDown(uiTypeEditor, propertyDescriptor, instance);
                }
                else
                {
                    var converter = propertyDescriptor.Converter;
                    if (converter != null
                        && converter.GetStandardValuesSupported(null)) // converter case
                    {
                        dropDown = new TreeGridDesignerInPlaceEditCombo(converter, propertyDescriptor, instance);
                    }
                }
            }

            return dropDown;
        }

        /// <summary>
        ///     Factory method for creating the appropriate drop-down control based on the given property descriptor
        /// </summary>
        /// <param name="propertyDescriptor">
        ///     Property descriptor used to create the drop-down.  If the property descriptor supports a UITypeEditor,
        ///     that will be used first.  Otherwise, the type converter will be used.
        /// </param>
        /// <param name="instance">Instance of the object being edited.</param>
        /// <param name="editStyle">
        ///     In the case that a UITypeEditor is used, controls the style of drop-down created.  This
        ///     parameter is not used in the TypeConverter case.
        /// </param>
        /// <returns>A DropDownControl instance if the given property descriptor supports it, null otherwise.</returns>
        internal static TypeEditorHost CreateTypeEditorHost(
            PropertyDescriptor propertyDescriptor, object instance, TypeEditorHostEditControlStyle editControlStyle)
        {
            TypeEditorHost dropDown = null;

            if (propertyDescriptor != null)
            {
                var uiTypeEditor = propertyDescriptor.GetEditor(typeof(UITypeEditor)) as UITypeEditor;
                if (uiTypeEditor != null) // UITypeEditor case
                {
                    dropDown = new TreeGridDesignerInPlaceEditDropDown(uiTypeEditor, propertyDescriptor, instance, editControlStyle);
                }
                else
                {
                    var converter = propertyDescriptor.Converter;
                    if (converter != null
                        && converter.GetStandardValuesSupported(null)) // converter case
                    {
                        dropDown = new TreeGridDesignerInPlaceEditCombo(converter, propertyDescriptor, instance, editControlStyle);
                    }
                }
            }

            return dropDown;
        }

        #endregion

        #region Unit Testing

        internal TreeGridDesignerColumnDescriptor[] Columns
        {
            get { return _currentColumns; }
        }

        private bool _testAggregateBranch;

        /// <summary>
        ///     Allows testing of the AggregateBranch class.
        /// </summary>
        internal void EnableAggregateTesting()
        {
            _testAggregateBranch = true;
        }

        #endregion
    }

    internal enum KeyAction
    {
        Handle,
        Discard,
        Process
    }

    internal enum NavigationDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    /// <summary>
    ///     Enum returned from TreeGridDesignerTreeControl.ProcessKeyDown and TreeGridDesignerTreeControl.ProcessKeyPress.
    /// </summary>
    internal enum ProcessKeyReturn
    {
        /// <summary>
        ///     Branch indicated it did not want to handle the key.
        /// </summary>
        NotHandled = 0,

        /// <summary>
        ///     Branch indicated it wanted the key, but no action occurred as a result of handling.
        /// </summary>
        KeyHandledNoAction = 1,

        /// <summary>
        ///     Branch indicated it wanted the key, and an action occurred as a result of handling.
        /// </summary>
        KeyHandledActionOccurred = 2
    }
}
