// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.VisualStudio;

    /// <summary>
    ///     Selection support for the VirtualTreeControl.  The motivation for implementing this here, rather than allowing the
    ///     underlying listbox to handle it, is that we need to fire the correct WinEvents for selection/focus.  If we let the listbox
    ///     handle it, it will fire WinEvents with it's own child ids that assume a flat list structure.  We need to be able to account for
    ///     hierarchy in the tree.  Also, it gets us a step closer to removing the listbox dependency altogether.
    /// </summary>
    internal partial class VirtualTreeControl
    {
        /// <summary>
        ///     Struct that represents a range of selected objects.
        /// </summary>
        private struct SelectionRange
        {
            /// <summary>
            ///     Inclusive starting index of the selection range.
            /// </summary>
            public int StartIndex;

            /// <summary>
            ///     Inclusive ending index of the selection range.
            /// </summary>
            public int EndIndex;

            public SelectionRange(int startIndex, int endIndex)
            {
                StartIndex = startIndex;
                EndIndex = endIndex;
            }

            public bool Contains(int i)
            {
                return i >= StartIndex && i <= EndIndex;
            }
        }

        private sealed class SelectedIndexEnumerator : IEnumerator<int>
        {
            private readonly VirtualTreeControl myOwner;
            private int myCurrentRange;
            private int myCurrentSelection;

            public SelectedIndexEnumerator(VirtualTreeControl owner)
            {
                myOwner = owner;
                myCurrentRange = -2;
                    // -2 to distinguish initial state from ending state (where these will equal VirtualTreeConstant.NullIndex)
                myCurrentSelection = -2;
            }

            public bool MoveNext()
            {
                if (myOwner.mySelectedRanges != null
                    && myOwner.mySelectedRanges.Count > 0)
                {
                    if (myCurrentRange == -2
                        && myCurrentSelection == -2)
                    {
                        myCurrentRange = 0;
                        myCurrentSelection = myOwner.mySelectedRanges[myCurrentRange].StartIndex;
                        return true;
                    }
                    else if (myCurrentSelection == myOwner.mySelectedRanges[myCurrentRange].EndIndex)
                    {
                        myCurrentRange++;
                        if (myCurrentRange < myOwner.mySelectedRanges.Count)
                        {
                            myCurrentSelection = myOwner.mySelectedRanges[myCurrentRange].StartIndex;
                            return true;
                        }
                    }
                    else
                    {
                        myCurrentSelection++;
                        return true;
                    }
                }

                myCurrentRange = VirtualTreeConstant.NullIndex;
                myCurrentSelection = VirtualTreeConstant.NullIndex;
                return false;
            }

            object IEnumerator.Current
            {
                get { return myCurrentSelection; }
            }

            public int Current
            {
                get { return myCurrentSelection; }
            }

            public void Reset()
            {
                myCurrentRange = -2;
                    // -2 to distinguish initial state from ending state (where these will equal VirtualTreeConstant.NullIndex)
                myCurrentSelection = -2;
            }

            public void Dispose()
            {
                myCurrentRange = VirtualTreeConstant.NullIndex;
                myCurrentSelection = VirtualTreeConstant.NullIndex;
            }
        }

        private int myAnchorIndex = VirtualTreeConstant.NullIndex; // anchor starts out undefined
        private int myCaretIndex; // caret starts at 0

        /// <summary>
        ///     Stores sorted list of selected ranges.  Assumption here is that large, contiguous selection ranges will
        ///     be more common than large numbers of discontiguous ranges, so this representation is more efficient than storing
        ///     a list of individually selected indices.
        /// </summary>
        private List<SelectionRange> mySelectedRanges;

        /// <summary>
        ///     Gets/sets the caret (focused) index.  Also the ending index for range multiple selections.
        ///     Note:  this is not public because clients should use the existing CurrentIndex property instead.
        /// </summary>
        private int CaretIndex
        {
            get { return myCaretIndex; }
            set
            {
                if (IsHandleCreated)
                {
                    var itemCount = ItemCount;
                    Debug.Assert(value < itemCount, "attempting to set caret to an invalid value");

                    // redraw old caret index
                    if (myCaretIndex >= 0
                        && myCaretIndex < itemCount)
                    {
                        InvalidateItem(myCaretIndex, -1, NativeMethods.RedrawWindowFlags.Invalidate | NativeMethods.RedrawWindowFlags.Erase);
                    }

                    var oldCaretIndex = myCaretIndex;
                    myCaretIndex = value;

                    if (myCaretIndex >= 0)
                    {
                        // ensure new caret position is visible
                        ScrollVertIntoView(myCaretIndex);

                        if (myCaretIndex != oldCaretIndex)
                        {
                            // redraw new caret index
                            InvalidateItem(
                                myCaretIndex, -1, NativeMethods.RedrawWindowFlags.Invalidate | NativeMethods.RedrawWindowFlags.Erase);

                            // notify accessibility clients of focus change
                            if (!GetStateFlag(VTCStateFlags.RestoringSelection)
                                && VirtualTreeAccEvents.ShouldNotify(VirtualTreeAccEvents.eventObjectFocus, this)
                                && Focused)
                            {
                                VirtualTreeAccEvents.Notify(
                                    VirtualTreeAccEvents.eventObjectFocus, CurrentIndex,
                                    CurrentColumn, this);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Gets/sets the anchor index, specifies the starting index for range selections in extended multi-select mode.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(-1)]
        public int AnchorIndex
        {
            get { return myAnchorIndex; }
            set
            {
                if (IsHandleCreated)
                {
                    // Anchor index may be either -1 or a valid item index.
                    if (value != VirtualTreeConstant.NullIndex)
                    {
                        CheckIndex(value);
                    }
                    myAnchorIndex = value;
                }
            }
        }

        /// <summary>
        ///     Selects/deselects the given index.
        /// </summary>
        private void SetSelected(int index, bool select)
        {
            CheckIndex(index);

            SetSelectionRange(index, index, select);
        }

        /// <summary>
        ///     Selects/deselects the given range.
        /// </summary>
        private void SetSelectionRange(int startIndex, int endIndex, bool select)
        {
            if (mySelectedRanges == null)
            {
                mySelectedRanges = new List<SelectionRange>();
            }

            if (select)
            {
                // algorithm for selection case:
                // 1. expand newRange to first overlapping or contiguous range
                // 2. expand newRange to last overlapping or contiguous range
                // 3. collapse overlapping ranges

                var newRange = new SelectionRange(startIndex, endIndex);
                int startPos;
                int endPos;

                // expand newRange to first overlapping or contiguous range
                for (startPos = 0; startPos < mySelectedRanges.Count; startPos++)
                {
                    var range = mySelectedRanges[startPos];
                    if (newRange.StartIndex - 1 <= range.EndIndex)
                    {
                        newRange.StartIndex = Math.Min(range.StartIndex, newRange.StartIndex);
                        break;
                    }
                }

                // expand newRange to last overlapping or contiguous range
                for (endPos = mySelectedRanges.Count; --endPos >= startPos;)
                {
                    var range = mySelectedRanges[endPos];
                    if (newRange.EndIndex + 1 >= range.StartIndex)
                    {
                        newRange.EndIndex = Math.Max(range.EndIndex, newRange.EndIndex);
                        break;
                    }
                }

                // collapse overlapping ranges
                if (endPos < mySelectedRanges.Count)
                {
                    for (var pos = endPos; pos >= startPos; pos--)
                    {
                        var range = mySelectedRanges[pos];
                        if ((newRange.StartIndex <= range.StartIndex)
                            && (newRange.EndIndex >= range.EndIndex))
                        {
                            mySelectedRanges.RemoveAt(pos);
                        }
                    }
                }

                // store new range
                mySelectedRanges.Insert(startPos, newRange);

                // invalidate entire selected area.
                InvalidateItem(
                    startIndex, -1, NativeMethods.RedrawWindowFlags.Invalidate | NativeMethods.RedrawWindowFlags.Erase,
                    (endIndex - startIndex) + 1);
            }
            else
            {
                // algorithm for deselection case.  For each range:
                // 1.  If contains new range (exclusive), split existing range into two.
                // 2.  If contained by new range (inclusive), discard.
                // 3.  If start index or end index fall within new range, adjust accordingly.
                // Note: we're a bit more careful about invalidating only the necessary regions here, because SetCurrentExtendedMultiSelectIndex
                // deselects large item ranges.
                for (var i = 0; i < mySelectedRanges.Count; i++)
                {
                    var currentRange = mySelectedRanges[i];
                    if (currentRange.StartIndex > endIndex)
                    {
                        break;
                    }
                    if (startIndex > currentRange.StartIndex
                        && endIndex < currentRange.EndIndex)
                    {
                        mySelectedRanges[i] = new SelectionRange(currentRange.StartIndex, startIndex - 1);
                        mySelectedRanges.Insert(i + 1, new SelectionRange(endIndex + 1, currentRange.EndIndex));
                        InvalidateItem(
                            startIndex, -1, NativeMethods.RedrawWindowFlags.Invalidate | NativeMethods.RedrawWindowFlags.Erase,
                            (endIndex - startIndex) + 1);
                        break; // finished, we've accounted for entire range.
                    }
                    if (startIndex <= currentRange.StartIndex
                        && endIndex >= currentRange.EndIndex)
                    {
                        mySelectedRanges.RemoveAt(i--);
                        InvalidateItem(
                            currentRange.StartIndex, -1, NativeMethods.RedrawWindowFlags.Invalidate | NativeMethods.RedrawWindowFlags.Erase,
                            (currentRange.EndIndex - currentRange.StartIndex) + 1);
                        continue; // keep going, there may be additional ranges to modify
                    }

                    if (startIndex <= currentRange.StartIndex
                        && endIndex >= currentRange.StartIndex)
                    {
                        InvalidateItem(
                            currentRange.StartIndex, -1, NativeMethods.RedrawWindowFlags.Invalidate | NativeMethods.RedrawWindowFlags.Erase,
                            (endIndex - currentRange.StartIndex) + 1);
                        currentRange.StartIndex = endIndex + 1;
                        mySelectedRanges[i] = currentRange;
                    }
                    else if (startIndex <= currentRange.EndIndex
                             && endIndex >= currentRange.EndIndex)
                    {
                        InvalidateItem(
                            startIndex, -1, NativeMethods.RedrawWindowFlags.Invalidate | NativeMethods.RedrawWindowFlags.Erase,
                            (currentRange.EndIndex - startIndex) + 1);
                        currentRange.EndIndex = startIndex - 1;
                        mySelectedRanges[i] = currentRange;
                    }
                }
            }
        }

        /// <summary>
        ///     Returns true iff the given index is selected.
        /// </summary>
        private bool IsSelected(int index)
        {
            CheckIndex(index);

            if (mySelectedRanges == null)
            {
                return false;
            }

            for (var i = 0; i < mySelectedRanges.Count; i++)
            {
                if (mySelectedRanges[i].Contains(index))
                {
                    return true;
                }
                if (mySelectedRanges[i].StartIndex > index)
                {
                    break;
                }
            }

            return false;
        }

        /// <summary>
        ///     Returns an array of selected indices.  This array is allocated each time this is called, so using it
        ///     is not as efficient as the SelectedIndexEnumerator.  This is kept mostly because it simplifies the selection
        ///     tracking code considerably.
        /// </summary>
        private int[] SelectedIndicesArray
        {
            get
            {
                // check handle creation to ensure we work the same way we did when we used the listbox to track selection.
                if (!IsHandleCreated)
                {
                    return null;
                }

                var count = SelectionCount;
                if (count > 0)
                {
                    var selectedIndices = new int[SelectionCount];
                    var currentIndex = 0;
                    for (var i = 0; i < mySelectedRanges.Count; i++)
                    {
                        var endIndex = mySelectedRanges[i].EndIndex;
                        for (var j = mySelectedRanges[i].StartIndex; j <= endIndex; j++)
                        {
                            selectedIndices[currentIndex++] = j;
                        }
                    }

                    return selectedIndices;
                }

                return null;
            }
        }

        /// <summary>
        ///     Count of selected indices.
        /// </summary>
        private int SelectionCount
        {
            get
            {
                if (mySelectedRanges == null)
                {
                    return 0;
                }

                var count = 0;
                for (var i = 0; i < mySelectedRanges.Count; i++)
                {
                    count += (mySelectedRanges[i].EndIndex - mySelectedRanges[i].StartIndex) + 1;
                }
                return count;
            }
        }

        /// <summary>
        ///     Clears selected indices.  Pass true to invalidate the cleared region.
        /// </summary>
        private void ClearSelection(bool invalidate)
        {
            if (invalidate)
            {
                RedrawVisibleSelectedItems();
            }
            if (mySelectedRanges != null)
            {
                mySelectedRanges.Clear();
            }
        }

        /// <summary>
        ///     Handles selection and caret changes due to mouse clicks.
        /// </summary>
        private void DoSelectionChangeFromMouse(
            ref VirtualTreeHitInfo hitInfo, bool shiftPressed, bool controlPressed, MouseButtons mouseButton)
        {
            var fireEvent = false;
            var columnSwitch = RequireColumnSwitchForSelection(ref hitInfo);
            if (columnSwitch)
            {
                SetSelectionColumn(myMouseDownHitInfo.DisplayColumn, false);
                fireEvent = true;
            }

            if (hitInfo.Row != VirtualTreeConstant.NullIndex)
            {
                if (mouseButton == MouseButtons.Right
                    && IsSelected(hitInfo.Row))
                {
                    // if right button is clicked, and the index is already selected,
                    // we'll just change the caret.
                    if (CaretIndex != hitInfo.Row)
                    {
                        CaretIndex = hitInfo.Row;
                    }
                }
                else
                {
                    if (GetStyleFlag(VTCStyleFlags.ExtendedMultiSelect))
                    {
                        var action = controlPressed ? ModifySelectionAction.Toggle : ModifySelectionAction.Select;
                        SetCurrentExtendedMultiSelectIndex(hitInfo.Row, shiftPressed, controlPressed, action);
                        // SetCurrentExtendedMultiSelectIndex fires SelectionChanged event.
                        fireEvent = false;
                    }
                    else if (GetStyleFlag(VTCStyleFlags.MultiSelect))
                    {
                        SetSelected(hitInfo.Row, !IsSelected(hitInfo.Row) || columnSwitch);
                        CurrentIndex = hitInfo.Row; // CurrentIndex setter fires SelectionChanged event.
                        fireEvent = false;
                    }
                    else if (!IsSelected(hitInfo.Row))
                    {
                        CurrentIndex = hitInfo.Row; // CurrentIndex setter fires SelectionChanged event.
                        fireEvent = false;
                    }
                }
            }

            if (fireEvent)
            {
                DoSelectionChanged();
            }
        }

        /// <summary>
        ///     Fires WinEvents for given selection change.
        /// </summary>
        private void FireWinEventsForSelection(bool extendFromAnchor, bool preserveSelection, ModifySelectionAction caretAction)
        {
            if (GetStateFlag(VTCStateFlags.RestoringSelection))
            {
                return; // no events if we're restoring selection.
            }

            if (SelectionCount == 1)
            {
                // if there's currently only one thing selected, we know we should fire a regular selection event.
                if (VirtualTreeAccEvents.ShouldNotify(VirtualTreeAccEvents.eventObjectSelection, this))
                {
                    VirtualTreeAccEvents.Notify(
                        VirtualTreeAccEvents.eventObjectSelection, CurrentIndex,
                        CurrentColumn, this);
                }
            }
            else if (extendFromAnchor)
            {
                if (VirtualTreeAccEvents.ShouldNotify(VirtualTreeAccEvents.eventObjectSelectionWithin, this))
                {
                    // we extended selection from the anchor position, this constitutes a significant change in selection
                    // state, so we fire selection within from the tree control, and let clients query us for the result.
                    // UNDONE: querying currently won't work because we need a way to return an IEnumVariant from IAccessible::get_accSelection.
                    // We just fire selection for the caret instead.
                    // VirtualTreeAccEvents.Notify(VirtualTreeAccEvents.eventObjectSelectionWithin, VirtualTreeConstant.NullIndex,
                    //								 VirtualTreeConstant.NullIndex, this);
                    VirtualTreeAccEvents.Notify(
                        VirtualTreeAccEvents.eventObjectSelection, CurrentIndex,
                        CurrentColumn, this);
                }
            }
            else if (preserveSelection)
            {
                // we're preserving an original selection, which means that we fire either an add or a remove,
                // depending on what happened to the caret
                switch (caretAction)
                {
                    case ModifySelectionAction.Select:
                        if (VirtualTreeAccEvents.ShouldNotify(VirtualTreeAccEvents.eventObjectSelectionAdd, this))
                        {
                            VirtualTreeAccEvents.Notify(
                                VirtualTreeAccEvents.eventObjectSelectionAdd, CurrentIndex,
                                CurrentColumn, this);
                        }
                        break;
                    case ModifySelectionAction.Clear:
                        if (VirtualTreeAccEvents.ShouldNotify(VirtualTreeAccEvents.eventObjectSelectionRemove, this))
                        {
                            VirtualTreeAccEvents.Notify(
                                VirtualTreeAccEvents.eventObjectSelectionRemove, CurrentIndex,
                                CurrentColumn, this);
                        }
                        break;
                    case ModifySelectionAction.Toggle:
                        if (IsSelected(CaretIndex))
                        {
                            goto case ModifySelectionAction.Select;
                        }
                        else
                        {
                            goto case ModifySelectionAction.Clear;
                        }
                    case ModifySelectionAction.None:
                        // Caret position didn't change, but selection still changed overall.  This happens, for instance,
                        // during cross-column selection transfer.  We would ideally fire selection within here, for now
                        // we just fire selection at the caret position anyway.
                        if (VirtualTreeAccEvents.ShouldNotify(VirtualTreeAccEvents.eventObjectSelectionWithin, this))
                        {
                            //VirtualTreeAccEvents.Notify(VirtualTreeAccEvents.eventObjectSelectionWithin, VirtualTreeConstant.NullIndex,
                            //								VirtualTreeConstant.NullIndex, this);
                            VirtualTreeAccEvents.Notify(
                                VirtualTreeAccEvents.eventObjectSelection, CurrentIndex,
                                CurrentColumn, this);
                        }
                        break;
                }
            }
        }

        #region Public APIs

        /// <summary>
        ///     Selects all items in the tree.  The SelectionMode property must be set to a value that supports multiple selections for this to work.
        ///     Note that selection only spans a single column.
        /// </summary>
        public void SelectAll()
        {
            if (GetStyleFlag(VTCStyleFlags.MultiSelect))
            {
                var itemCount = myTree.VisibleItemCount;
                if (itemCount > 0)
                {
                    ClearSelection(false);
                    SetSelectionRange(0, myTree.VisibleItemCount - 1, true);
                    DoSelectionChanged();
                    FireWinEventsForSelection(true, false, ModifySelectionAction.None);
                }
            }
        }

        /// <summary>
        ///     Clears selected items in the tree.
        /// </summary>
        public void ClearSelection()
        {
            var selectionCount = SelectionCount;
            if (selectionCount > 0)
            {
                ClearSelection(true);
                DoSelectionChanged();
                FireWinEventsForSelection(true, false, ModifySelectionAction.None);
            }
        }

        /// <summary>
        ///     Selects a range of items in the tree.  The SelectionMode property must be set to a value that supports multiple selections for this to work.
        ///     Note that selection only spans a single column.  This method does not change anchor or caret indices.
        /// </summary>
        /// <param name="startIndex">Beginning index of the selection.</param>
        /// <param name="endIndex">Ending index of the selection.</param>
        /// <param name="performSelection">True to select items, false to deselect.</param>
        public void SelectRange(int startIndex, int endIndex, bool performSelection)
        {
            if (GetStyleFlag(VTCStyleFlags.MultiSelect))
            {
                if (startIndex > endIndex)
                {
                    throw new ArgumentOutOfRangeException("startIndex");
                }
                CheckIndex(startIndex);
                CheckIndex(endIndex);

                SetSelectionRange(startIndex, endIndex, performSelection);
                DoSelectionChanged();
                FireWinEventsForSelection(true, false, ModifySelectionAction.None);
            }
        }

        /// <summary>
        ///     Set the multi select caret index on the given item. Calling this with false, false will
        ///     make the new caret the only selected item.
        /// </summary>
        /// <param name="newCaret">The new caret position</param>
        /// <param name="extendFromAnchor">True to extend the selection from the current anchor position</param>
        /// <param name="preserveSelection">True to maintain selected items outside the anchored range</param>
        /// <param name="selectCaretAction">Specify how the selection state should be modified</param>
        public void SetCurrentExtendedMultiSelectIndex(
            int newCaret, bool extendFromAnchor, bool preserveSelection, ModifySelectionAction selectCaretAction)
        {
            SetCurrentExtendedMultiSelectIndex(newCaret, extendFromAnchor, preserveSelection, selectCaretAction, true);
        }

        /// <summary>
        ///     Set the multi select caret index on the given item. Calling this with false, false will
        ///     make the new caret the only selected item.
        /// </summary>
        /// <param name="newCaret">The new caret position</param>
        /// <param name="extendFromAnchor">True to extend the selection from the current anchor position</param>
        /// <param name="preserveSelection">True to maintain selected items outside the anchored range</param>
        /// <param name="selectCaretAction">Specify how the selection state should be modified</param>
        /// <param name="select">True to select items in the caret to anchor range, false to deselect.  Implies extendFromAnchor.</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void SetCurrentExtendedMultiSelectIndex(
            int newCaret, bool extendFromAnchor, bool preserveSelection, ModifySelectionAction selectCaretAction, bool select)
        {
            if (!GetStyleFlag(VTCStyleFlags.ExtendedMultiSelect)
                || !IsHandleCreated)
            {
                return;
            }

            // We fire off a lot of messages in this routine. Go ahead and use DefWindowProc to
            // bypass all of the extra handling done in the various subclassing routines (WinForms and us)
            var hWnd = Handle;
            var itemCount = (int)NativeMethods.SendMessage(hWnd, NativeMethods.LB_GETCOUNT, 0, 0);

            // Get the starting caret
            var startCaret = CaretIndex;

            var caretInitiallySelected = IsSelected(newCaret);

            // Get the initial anchor position
            var anchor = newCaret;
            if (extendFromAnchor)
            {
                anchor = AnchorIndex;
                if (anchor < 0)
                {
                    extendFromAnchor = false;
                    anchor = newCaret;
                }
            }

            if ((anchor == newCaret && !preserveSelection)
                || startCaret == newCaret)
            {
                SetSelected(newCaret, true);
            }
            else
            {
                // We're moving toward the anchor (the first two cases here), then the
                // remaining items are generally cleared with the !preverseSelection code
                // below, but we need to clear between the old and new carets even if we're
                // preserving selection elsewhere or things look really weird.
                if (newCaret >= anchor
                    && startCaret > newCaret
                    && startCaret > anchor)
                {
                    if (preserveSelection && extendFromAnchor)
                    {
                        SetSelectionRange(newCaret + 1, startCaret, false);
                    }
                }
                else if (newCaret <= anchor
                         && startCaret < newCaret
                         && startCaret < anchor)
                {
                    if (preserveSelection && extendFromAnchor)
                    {
                        SetSelectionRange(startCaret, newCaret - 1, false);
                    }
                }
                else
                {
                    // Select all items between the old and the new carets, inclusive. Not all of these
                    // items will be selectable items for multicolumn trees with blank expansions. However,
                    // these items will be filtered when the selected items are enumerated. We have to handle
                    // this state regardless because blocking the listbox from reaching this state when the
                    // user selects items with the mouse is extremely difficult (basically impossible). We
                    // could block here, but it is not worth creating the ColumnItemEnumerator.
                    SetSelectionRange(Math.Min(newCaret, startCaret), Math.Max(newCaret, startCaret), select);
                }
            }

            if (!preserveSelection)
            {
                var keepSelTop = Math.Min(anchor, newCaret);
                var keepSelBottom = Math.Max(anchor, newCaret);
                // Use selitemrangeex with the lowest number last to clear
                if (keepSelTop > 0)
                {
                    SetSelectionRange(0, keepSelTop - 1, false);
                }
                if (keepSelBottom < itemCount - 1)
                {
                    SetSelectionRange(keepSelBottom + 1, itemCount - 1, false);
                }
            }

            switch (selectCaretAction)
            {
                case ModifySelectionAction.Clear:
                    if (IsSelected(newCaret))
                    {
                        SetSelected(newCaret, false);
                    }
                    else if (!caretInitiallySelected)
                    {
                        selectCaretAction = ModifySelectionAction.None; // nothing happened to the caret
                    }
                    break;
                case ModifySelectionAction.Select:
                    if (!IsSelected(newCaret))
                    {
                        SetSelected(newCaret, true);
                    }
                    else if (caretInitiallySelected)
                    {
                        selectCaretAction = ModifySelectionAction.None; // nothing happened to the caret
                    }
                    break;
                case ModifySelectionAction.Toggle:
                    if (caretInitiallySelected)
                    {
                        goto case ModifySelectionAction.Clear;
                    }
                    else
                    {
                        goto case ModifySelectionAction.Select;
                    }
            }

            // Some of the calls above will move the anchor index. Back sure it's where we want it.
            if (anchor != AnchorIndex)
            {
                AnchorIndex = anchor;
            }
            CaretIndex = newCaret;
            DoSelectionChanged();
            FireWinEventsForSelection(extendFromAnchor, preserveSelection, selectCaretAction);
        }

        #endregion
    }
}
