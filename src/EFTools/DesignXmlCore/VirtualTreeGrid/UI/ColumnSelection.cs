// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.VisualStudio;

    #region ColumnSelectionTransfer enum

    /// <summary>
    ///     Determine the action a MultiSelect VirtualTreeControl should take
    ///     when the selection column changes.
    /// </summary>
    internal enum ColumnSelectionTransferAction
    {
        /// <summary>
        ///     Select all non-blank cells in the new column that were
        ///     in rows that were selected in the old column.
        /// </summary>
        PreserveNonBlankCells,

        /// <summary>
        ///     Select all cells in the new column that were selected
        ///     in the old column and are either non-blank or have
        ///     a blank expansion anchor on the same row.
        /// </summary>
        PreserveAnchoredCells,

        /// <summary>
        ///     Select all cells in the new column that were selected
        ///     in the old column and are either non-blank or have
        ///     a blank expansion anchor on the same row.
        /// </summary>
        PreserveSharedAnchors,

        /// <summary>
        ///     Select all cells in the new column that share a blank
        ///     expansion anchor with cells in the old column.
        /// </summary>
        PreserveSharedAnchorsOnly,

        /// <summary>
        ///     Do not move selection state from the old column to the new
        /// </summary>
        ClearSelectedRows,
    }

    #endregion // ColumnSelectionTransfer enum

    #region SelectionColumn transfer routines

    internal partial class VirtualTreeControl
    {
        private ColumnSelectionTransferAction myColumnSelectionAction = ColumnSelectionTransferAction.PreserveAnchoredCells;

        /// <summary>
        ///     The VirtualTreeControl does not support cross-column selection. When the
        ///     control has a MultiSelect SelectionMode it is not clear what should happen
        ///     to the selection information with a new column is activated. This property
        ///     specifies how the control moves currently selected rows to a new column.
        /// </summary>
        [DefaultValue(ColumnSelectionTransferAction.PreserveAnchoredCells)]
        public ColumnSelectionTransferAction ColumnSelectionTransferAction
        {
            get { return myColumnSelectionAction; }
            set
            {
                if (myColumnSelectionAction != value)
                {
                    var oldSelectionExtended = ExtendSelectionToAnchors;
                    myColumnSelectionAction = value;
                    if (IsHandleCreated && oldSelectionExtended != ExtendSelectionToAnchors)
                    {
                        Refresh();
                    }
                }
            }
        }

        /// <summary>
        ///     Returns true if blank expansion anchors are drawn as selected even
        ///     when the anchor column is not current. Based on the current value of
        ///     the ColumnSelectionTransferAction property.
        /// </summary>
        protected bool ExtendSelectionToAnchors
        {
            get
            {
                switch (myColumnSelectionAction)
                {
                    case ColumnSelectionTransferAction.PreserveAnchoredCells:
                    case ColumnSelectionTransferAction.PreserveSharedAnchors:
                    case ColumnSelectionTransferAction.PreserveSharedAnchorsOnly:
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        ///     Determine the display column and native column for the current selection
        ///     column and the specified caret. If extending anchor columns is enabled,
        ///     the the returned displayColumn may not be the current selection column.
        /// </summary>
        /// <param name="caret">The caret to test. Should not be -1.</param>
        /// <param name="displayColumn">Returns the display column</param>
        /// <param name="nativeColumn">Returns the native column</param>
        private void ResolveSelectionColumn(int caret, out int displayColumn, out int nativeColumn)
        {
            var resolvedDisplayColumn = mySelectionColumn;
            displayColumn = resolvedDisplayColumn;
            if (GetStyleFlag(VTCStyleFlags.MultiSelect) && ExtendSelectionToAnchors)
            {
                // We can be on a blank in this case, make sure we aren't
                if (caret >= 0
                    && caret < myTree.VisibleItemCount)
                {
                    var expansion = myTree.GetBlankExpansion(caret, resolvedDisplayColumn, myColumnPermutation);
                    if (expansion.AnchorColumn != resolvedDisplayColumn
                        && expansion.AnchorColumn != VirtualTreeConstant.NullIndex)
                    {
                        displayColumn = resolvedDisplayColumn = expansion.AnchorColumn;
                    }
                }
            }
            nativeColumn = (myColumnPermutation != null)
                               ? myColumnPermutation.GetNativeColumn(resolvedDisplayColumn)
                               : resolvedDisplayColumn;
        }

        private void SetSelectionColumn(int column, bool fireEvents)
        {
            SetSelectionColumn(column, null, fireEvents);
        }

        /// <summary>
        ///     Switch the selection column. For multiselect, the behavior is highly dependent on the
        ///     the ColumnSelectionTransferAction property and the prior state of the permutation, if
        ///     the permutation is being reordered
        /// </summary>
        /// <param name="column">The new selection column</param>
        /// <param name="oldPermutation">The old column permuation</param>
        /// <param name="fireEvents">True if this routine should fire selection events</param>
        private void SetSelectionColumn(int column, ColumnPermutation oldPermutation, bool fireEvents)
        {
            Debug.Assert(oldPermutation != null || column != mySelectionColumn);
            var iCaret = CurrentIndex;
            DismissLabelEdit(false, false);
            var oldColumn = mySelectionColumn;
            mySelectionColumn = column;

            if (GetStyleFlag(VTCStyleFlags.MultiSelect))
            {
                switch (myColumnSelectionAction)
                {
                    case ColumnSelectionTransferAction.PreserveNonBlankCells:
                        SetSelectionColumn_PreserveBasic(oldColumn, oldPermutation, column, false, fireEvents);
                        break;
                    case ColumnSelectionTransferAction.PreserveAnchoredCells:
                        SetSelectionColumn_PreserveBasic(oldColumn, oldPermutation, column, true, fireEvents);
                        break;
                    case ColumnSelectionTransferAction.PreserveSharedAnchors:
                        SetSelectionColumn_PreserveAnchors(oldColumn, oldPermutation, column, false, fireEvents);
                        break;
                    case ColumnSelectionTransferAction.PreserveSharedAnchorsOnly:
                        SetSelectionColumn_PreserveAnchors(oldColumn, oldPermutation, column, true, fireEvents);
                        break;
                    case ColumnSelectionTransferAction.ClearSelectedRows:
                        SetSelectionColumn_Clear(fireEvents);
                        break;
                }
            }
            else if (iCaret != -1 && fireEvents)
            {
                CurrentIndex = iCaret;
            }

            if (oldColumn != mySelectionColumn)
            {
                // This may cause a focus change, because the caret moves to a different column.
                if (!GetStateFlag(VTCStateFlags.RestoringSelection)
                    && VirtualTreeAccEvents.ShouldNotify(VirtualTreeAccEvents.eventObjectFocus, this)
                    && Focused)
                {
                    VirtualTreeAccEvents.Notify(
                        VirtualTreeAccEvents.eventObjectFocus, CurrentIndex,
                        CurrentColumn, this);
                }

                // ensure newly selected column is scrolled into view.
                if (HasHorizontalScrollBar)
                {
                    ScrollColumnIntoView(CurrentColumn);
                }

                // make sure we invalidate the caret item
                if (CurrentIndex != VirtualTreeConstant.NullIndex)
                {
                    InvalidateItem(CurrentIndex, -1, NativeMethods.RedrawWindowFlags.Invalidate);
                }
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "hWnd")]
        private void SetSelectionColumn_Clear(bool fireEvents)
        {
            var hWnd = Handle; // forces evaluation of Handle
            var iCaret = CaretIndex;
            var reselect = IsSelected(iCaret);
            ClearSelection(false);
            if (reselect
                && iCaret >= 0
                && fireEvents)
            {
                SetSelected(iCaret, true);
                OnSelectionChanged(EventArgs.Empty);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "hWnd")]
        private void SetSelectionColumn_PreserveBasic(
            int oldColumn, ColumnPermutation oldPermutation, int newColumn, bool allowAnchoredBlanks, bool fireEvents)
        {
            var indices = SelectedIndicesArray;
            int index;
            if (indices != null)
            {
                var hWnd = Handle; // forces evaluation of Handle
                var firstVisible = TopIndex;
                var lastVisible = firstVisible + myPartlyVisibleCountIgnoreHScroll - 1;

                // Grab information from the previous column to give us a baseline. Not
                // all of the items in indices will correspond to items in the old column.
                var oldIndices = (int[])indices.Clone();
                var oldIter = myTree.EnumerateColumnItems(
                    oldColumn, (oldPermutation != null) ? oldPermutation : myColumnPermutation, allowAnchoredBlanks, oldIndices, true);
                while (oldIter.MoveNext())
                {
                    ; // Spin the enumerator to mark the rows
                }

                var iter = myTree.EnumerateColumnItems(newColumn, myColumnPermutation, allowAnchoredBlanks, indices, true);
                while (iter.MoveNext())
                {
                    ; // Spin the enumerator to mark the rows
                }
                var count = indices.Length;
                var lastStart = -1;
                var lastEnd = -1;
                var newEnd = -1;
                for (var i = 0; i < count; ++i)
                {
                    index = indices[i];
                    if (index < 0
                        || oldIndices[i] < 0)
                    {
                        newEnd = (index < 0) ? ~index : index;
                        if (lastEnd == -1)
                        {
                            lastStart = lastEnd = newEnd;
                        }
                        else if (1 == (newEnd - lastEnd))
                        {
                            ++lastEnd;
                        }
                        else
                        {
                            SetSelectionRange(lastStart, lastEnd, false);
                            lastStart = lastEnd = newEnd;
                        }
                    }
                    else if (index >= firstVisible
                             && index <= lastVisible)
                    {
                        InvalidateItem(index, -1, NativeMethods.RedrawWindowFlags.Invalidate);
                    }
                }
                if (lastStart != -1)
                {
                    SetSelectionRange(lastStart, lastEnd, false);
                }
            }
            if (fireEvents)
            {
                OnSelectionChanged(EventArgs.Empty);
            }
        }

        // Similar to the PreserveBasic routine, but we do column validation as well as validating the blanks
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "hWnd")]
        private void SetSelectionColumn_PreserveAnchors(
            int oldColumn, ColumnPermutation oldPermutation, int newColumn, bool sharedAnchorsOnly, bool fireEvents)
        {
            var indices = SelectedIndicesArray;
            if (indices != null)
            {
                var hWnd = Handle; // forces evaluation of Handle
                var firstVisible = TopIndex;
                var lastVisible = firstVisible + myPartlyVisibleCountIgnoreHScroll - 1;

                // Grab information from the previous column to give us a baseline. Not
                // all of the items in indices will correspond to items in the old column.
                // We walk the old array simultaneously with the iterator to track the column
                // numbers for each item.
                var oldIndices = (int[])indices.Clone();
                var oldIter = myTree.EnumerateColumnItems(
                    oldColumn, (oldPermutation != null) ? oldPermutation : myColumnPermutation, true, oldIndices, true);
                var nextFilterIndex = 0;
                while (oldIter.MoveNext())
                {
                    while (oldIndices[nextFilterIndex] < 0)
                    {
                        ++nextFilterIndex;
                    }
                    Debug.Assert(oldIndices[nextFilterIndex] == oldIter.RowInTree);

                    // Record the column in the array, the iterator is no longer using this filter slot
                    oldIndices[nextFilterIndex] = oldIter.ColumnInTree;
                    ++nextFilterIndex;
                }

                var iter = myTree.EnumerateColumnItems(newColumn, myColumnPermutation, true, indices, true);
                var expectColumnInTree = (myColumnPermutation != null) ? myColumnPermutation.GetNativeColumn(newColumn) : newColumn;
                nextFilterIndex = 0;
                int oldIndex;
                bool caughtUpToIter;
                bool tossItem;
                var lastStart = -1;
                var lastEnd = -1;
                var newEnd = -1;
                var count = indices.Length;
                var iteratorIsLive = true;
                while (iteratorIsLive && (iteratorIsLive = iter.MoveNext())
                       || nextFilterIndex < count)
                {
                    do
                    {
                        var index = indices[nextFilterIndex];
                        caughtUpToIter = index >= 0;
                        tossItem = !caughtUpToIter;
                        if (caughtUpToIter)
                        {
                            oldIndex = oldIndices[nextFilterIndex];
                            if (oldIndex < 0)
                            {
                                tossItem = true;
                            }
                            else if (sharedAnchorsOnly)
                            {
                                // The old column is stored in the oldIndex value for this case, allowing easy comparison
                                tossItem = oldIndex != iter.ColumnInTree;
                            }
                            else
                            {
                                tossItem = !(iter.ColumnInTree == expectColumnInTree || iter.ColumnInTree == oldIndex);
                            }
                        }
                        else
                        {
                            tossItem = true;
                        }
                        if (tossItem)
                        {
                            newEnd = (index < 0) ? ~index : index;
                            if (lastEnd == -1)
                            {
                                lastStart = lastEnd = newEnd;
                            }
                            else if (1 == (newEnd - lastEnd))
                            {
                                ++lastEnd;
                            }
                            else
                            {
                                SetSelectionRange(lastStart, lastEnd, false);
                                lastStart = lastEnd = newEnd;
                            }
                        }
                        else if (index >= firstVisible
                                 && index <= lastVisible)
                        {
                            InvalidateItem(index, -1, NativeMethods.RedrawWindowFlags.Invalidate);
                        }

                        // Increment and check bounds
                        ++nextFilterIndex;
                        if (!iteratorIsLive
                            && nextFilterIndex >= count)
                        {
                            break;
                        }
                    }
                    while (!caughtUpToIter);
                }
                // Finish off last section
                if (lastStart != -1)
                {
                    SetSelectionRange(lastStart, lastEnd, false);
                }
            }
            if (fireEvents)
            {
                OnSelectionChanged(EventArgs.Empty);
            }
        }
    }

    #endregion // SelectionColumn transfer routines
}
