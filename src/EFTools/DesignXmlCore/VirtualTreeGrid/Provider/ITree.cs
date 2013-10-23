// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    /// <summary>
    ///     An abstraction defining the data structure for a tree structure. An ITree
    ///     implementation tracks relationships between IBranch implementations.
    /// </summary>
    internal interface ITree
    {
        /// <summary>
        ///     Set the root object for the tree.
        /// </summary>
        IBranch Root { get; set; }

        /// <summary>
        ///     Query the branch to reorder child branches. This will call IBranch.VisibleItemCount
        ///     and IBranch.LocateObject as needed. If a list doesn't support relocation, then the
        ///     expanded list will be collapsed. If a branch is located after the visible item count,
        ///     then it's expansion state will be maintained so it can be realigned later back into
        ///     the visible range intact.
        /// </summary>
        /// <param name="branch">The branch to Realign, or null for all branches.</param>
        void Realign(IBranch branch);

        /// <summary>
        ///     Use to insert items without forcing a realign. InsertItems should be used
        ///     to adjust an existing branch: do not add a node with no children then immediately
        ///     call InsertItems repeatedly to add children. The branches VisibleItemCount is
        ///     assumed to be adjusted for the new items before a call to InsertItems. If the
        ///     items are inserted at the end of the visible portion of the branch, then
        ///     VisibleItemCount will be used to determine if the new items are hidden or visible.
        /// </summary>
        /// <param name="branch">The branch where items have been inserted.</param>
        /// <param name="after">The index to insert after. Use -1 to insert at the beginning</param>
        /// <param name="count">The number of items that have been inserted.</param>
        void InsertItems(IBranch branch, int after, int count);

        /// <summary>
        ///     Delete specific items without calling Realign
        /// </summary>
        /// <param name="branch">The branch where items have been removed</param>
        /// <param name="start">The first item to deleted</param>
        /// <param name="count">The number of items to delete</param>
        void DeleteItems(IBranch branch, int start, int count);

        /// <summary>
        ///     Change the position of a single item in a branch
        /// </summary>
        /// <param name="branch">The branch where the item moved</param>
        /// <param name="fromRow">The row the item used to be on</param>
        /// <param name="toRow">The row the item is on now</param>
        void MoveItem(IBranch branch, int fromRow, int toRow);

        /// <summary>
        ///     Expand or collapse the item at the given position
        /// </summary>
        /// <param name="row">The row coordinate</param>
        /// <param name="column">The column coordinate</param>
        /// <returns>Data showing the incremental change in the number of items in the tree</returns>
        ToggleExpansionData ToggleExpansion(int row, int column);

        /// <summary>
        ///     Is the item at this absolute index expanded?
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <returns>true if the item is currently expanded</returns>
        bool IsExpanded(int row, int column);

        /// <summary>
        ///     Is the item at this absolute index expandable?
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <returns>true if the item can be expanded</returns>
        bool IsExpandable(int row, int column);

        /// <summary>
        ///     Retrieve information about the item at the target position
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <param name="setFlags">Calculate information beyond Branch, Row, Column, Level, and Blank</param>
        /// <returns>An VirtualTreeItemInfo describing the item at the given location</returns>
        VirtualTreeItemInfo GetItemInfo(int row, int column, bool setFlags);

        /// <summary>
        ///     The total number of items currently displayed by the tree
        /// </summary>
        /// <value></value>
        int VisibleItemCount { get; }

        /// <summary>
        ///     Update children as needed, depending on their supported BranchFeatures
        /// </summary>
        void Refresh();

        /// <summary>
        ///     Get the number of descendants of a given node
        /// </summary>
        /// <param name="row">The row coordinate</param>
        /// <param name="column">The column coordinate</param>
        /// <param name="includeSubItems">Whether to include any subitems in the item count.</param>
        /// <param name="complexColumnRoot">row and column are the first node in a complex subitem column. Return the count of all items in the root list, not the expanded count for the first item.</param>
        /// <returns>0 if the node is not expanded and there are no subitems</returns>
        int GetDescendantItemCount(int row, int column, bool includeSubItems, bool complexColumnRoot);

        /// <summary>
        ///     Get the number of sub items for this item. The next non-blank item
        ///     in a given column is at row + GetSubItemCount(absRow, column) + 1
        /// </summary>
        /// <param name="row">The row coordinate</param>
        /// <param name="column">The column coordinate</param>
        /// <returns>The number of sub items immediately below this node.</returns>
        int GetSubItemCount(int row, int column);

        /// <summary>
        ///     For the given row and column, retrieve the range of cells that form the
        ///     blank region for the given items. The anchor cell, which is the only
        ///     cell in the range that is not a blank item, will always be the top left
        ///     cell. In most cases, the returned range will simply be the input cell.
        /// </summary>
        /// <param name="row">The row coordinate</param>
        /// <param name="column">The column coordinate</param>
        /// <param name="columnPermutation">
        ///     The column permutation to apply. If this
        ///     is provided, then column is relative to the column permutation, not
        ///     the unpermuted position.
        /// </param>
        /// <returns>The expansion for this row and column</returns>
        BlankExpansionData GetBlankExpansion(int row, int column, ColumnPermutation columnPermutation);

        /// <summary>
        ///     Get the parent index of the given row and column
        /// </summary>
        /// <param name="row">The row coordinate</param>
        /// <param name="column">The column coordinate</param>
        /// <returns>Returns the parent index, or -1 if the parent is the root list</returns>
        int GetParentIndex(int row, int column);

        /// <summary>
        ///     Returns the expanded list at the given index if one already exists. This method
        ///     will throw an exception if IsExpanded for this cell is false. Use ToggleExpansion
        ///     to create a new expansion.
        /// </summary>
        /// <param name="row">The row coordinate</param>
        /// <param name="column">The column coordinate</param>
        /// <returns>The branch and level of the expansion at the given location</returns>
        ExpandedBranchData GetExpandedBranch(int row, int column);

        /// <summary>
        ///     Toggles the state of the given item (may be more than two states)
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <returns>The related nodes that need to be refreshed</returns>
        StateRefreshChanges ToggleState(int row, int column);

        /// <summary>
        ///     Synchronize the state of the items in the given enumerator to the state of another branch.
        /// </summary>
        /// <param name="itemsToSynchronize">Enumerator representing a list of items whose state should be synchronized</param>
        /// <param name="matchBranch">Branch to synchronize with</param>
        /// <param name="matchRow">Row in branch to synchronize with</param>
        /// <param name="matchColumn">Column in branch to synchronize with</param>
        void SynchronizeState(ColumnItemEnumerator itemsToSynchronize, IBranch matchBranch, int matchRow, int matchColumn);

        //Given a treelist and an index, walk all of the items in the list
        //UNDONE: Get this in the interface
        //void EnumAbsoluteIndices(IBranch *pList, int Index, [in,out] void** ppvNext, out int *pAbsIndex);
        /// <summary>
        ///     Determine how far a given index is from its parent node. Due to expansions
        ///     and subitems, this value can be much greater than the index in the branch itself.
        /// </summary>
        /// <param name="parentRow">The row of the parent object</param>
        /// <param name="column">The column coordinate</param>
        /// <param name="relativeIndex">The index in the child list to get the offset for</param>
        /// <param name="complexColumnRoot">The row and column are the first node of a complex subitem column. Return the offset from this node to other items in the root branch.</param>
        /// <returns>The offset from the parent to the given index in an expanded child list</returns>
        int GetOffsetFromParent(int parentRow, int column, int relativeIndex, bool complexColumnRoot);

        /// <summary>
        ///     Get the navigation target coordinate for a given cell.
        /// </summary>
        /// <param name="direction">The direction to navigate</param>
        /// <param name="sourceRow">The starting row coordinate</param>
        /// <param name="sourceColumn">The starting column coordinate</param>
        /// <param name="columnPermutation">
        ///     The column permutation to apply. If this
        ///     is provided, then sourceColumn and the returned column are relative to the
        ///     column permutation, not the unpermuted position.
        /// </param>
        /// <returns>The target coordinates, or VirtualTreeCoordinate.Invalid</returns>
        VirtualTreeCoordinate GetNavigationTarget(
            TreeNavigation direction, int sourceRow, int sourceColumn, ColumnPermutation columnPermutation);

        /// <summary>
        ///     Walk the items in a single tree column. EnumerateColumnItems is much more efficient
        ///     than calling GetItemInfo for each row in the column. The behavior of the returned
        ///     iterator is undefined if the tree structure changes during iteration.
        /// </summary>
        /// <param name="column">The column to iterate</param>
        /// <param name="columnPermutation">
        ///     The column permutation to apply. If this
        ///     is provided, then column is relative to the permutation.
        /// </param>
        /// <param name="returnBlankAnchors">
        ///     If an item is on a horizontal blank expansion, then
        ///     return the anchor for that item. All blanks are skipped if this is false.
        /// </param>
        /// <param name="startRow">The position of the first item to return</param>
        /// <param name="endRow">
        ///     The position of the last item to return. If endRow is less than startRow, the
        ///     the iterator will cycle back to the top of the tree. To search to the end of the list, pass an endRow
        ///     of -1 (VirtualTreeConstant.NullIndex)
        /// </param>
        ColumnItemEnumerator EnumerateColumnItems(
            int column, ColumnPermutation columnPermutation, bool returnBlankAnchors, int startRow, int endRow);

        /// <summary>
        ///     Walk the items in a single tree column. EnumerateColumnItems is much more efficient
        ///     than calling GetItemInfo for each row in the column. The behavior of the returned
        ///     iterator is undefined if the tree structure changes during iteration.
        /// </summary>
        /// <param name="column">The column to iterate</param>
        /// <param name="columnPermutation">
        ///     The column permutation to apply. If this
        ///     is provided, then column is relative to the permutation.
        /// </param>
        /// <param name="returnBlankAnchors">
        ///     If an item is on a horizontal blank expansion, then
        ///     return the anchor for that item. All blanks are skipped if this is false.
        /// </param>
        /// <param name="rowFilter">
        ///     An array of items to return. The array must be sorted in ascending order
        ///     and all values must be valid indices in the tree.
        /// </param>
        /// <param name="markExcludedFilterItems">If true, items in the filter that are not returned will be marked by bitwise inverting them (~initialIndex)</param>
        ColumnItemEnumerator EnumerateColumnItems(
            int column, ColumnPermutation columnPermutation, bool returnBlankAnchors, int[] rowFilter, bool markExcludedFilterItems);

        /// <summary>
        ///     Locate the given object in the tree, expanding branches as needed.
        /// </summary>
        /// <param name="startingBranch">The branch to search, or null for the root branch</param>
        /// <param name="target">The target object to locate</param>
        /// <param name="locateStyle">
        ///     The style to send to IBranch.LocateObject.
        ///     Generally ObjectStyle.TrackableObject, but values at or above ObjectStyle.FirstUserStyle may also be used.
        ///     Regardless of the style used, IBranch.LocateObject should return values from the TrackingObjectAction enum.
        /// </param>
        /// <param name="locateOptions">User-definable options passed to each call to IBranch.LocateObject</param>
        /// <returns>
        ///     A VirtualTreeCoordinate structure with the correct coordinates. The IsValid property of
        ///     the returned structure will be false if the object could not be located.
        /// </returns>
        VirtualTreeCoordinate LocateObject(IBranch startingBranch, object target, int locateStyle, int locateOptions);

        /// <summary>
        ///     Add or remove entire levels from existing branch structures in the tree
        /// </summary>
        /// <param name="shiftData">The data for the shift operation</param>
        void ShiftBranchLevels(ShiftBranchLevelsData shiftData);

        /// <summary>
        ///     Views on the tree should not attempt to redraw any items when Redraw is off.
        ///     Calls to Redraw can be nested, and must be balanced.
        /// </summary>
        /// <value>false to turn off redraw, true to restore it.</value>
        bool Redraw { get; set; }

        /// <summary>
        ///     Used to batch calls to Redraw without triggering unnecessary redraw operations.
        ///     Set this property to true if an operation may cause one or more redraw calls, then
        ///     to false on completion. The cost is negligible if Redraw is never triggered, whereas
        ///     an unneeded Redraw true/false can be very expensive. Calls to DelayRedraw can
        ///     be nested, and must be balanced.
        /// </summary>
        /// <value>true to delay, false to finish operation</value>
        bool DelayRedraw { get; set; }

        /// <summary>
        ///     A significant change is being made to the layout of the list, so give
        ///     any views on this object the chance to cache selection information. Calls to
        ///     ListShuffle can be nested, and must be balanced.
        /// </summary>
        /// <value>true to start a shuffle (cache state), false to end one</value>
        bool ListShuffle { get; set; }

        /// <summary>
        ///     Used to batch calls to ListShuffle without triggering unnecessary shuffle operations.
        ///     Set this property to true if an operation may cause one or more list shuffles, then
        ///     to false on completion. The cost is negligible if a shuffle is never triggered, whereas
        ///     an unneeded ListShuffle true/false can be very expensive. Calls to DelayListShuffle can
        ///     be nested, and must be balanced.
        /// </summary>
        /// <value>true to delay, false to finish operation</value>
        bool DelayListShuffle { get; set; }

        /// <summary>
        ///     The displayed data for the branch has changed, and
        ///     any views on this tree need to be notified.
        /// </summary>
        /// <param name="changeData">The DisplayDataChangedData structure</param>
        void DisplayDataChanged(DisplayDataChangedData changeData);

        /// <summary>
        ///     Fire OnQueryItemVisible events to listeners to see if an item is visible in any view
        /// </summary>
        /// <param name="absIndex">The absolute index of the item to test</param>
        /// <returns>true if the item is visible. If an OnQueryItemVisible listener is not attached, then the item is assumed to be visible.</returns>
        bool IsItemVisible(int absIndex);

        /// <summary>
        ///     Remove all occurrences of branch from the tree. Note that removing all
        ///     items from a branch is not the same as removing the branch itself.
        /// </summary>
        /// <param name="branch">The branch to remove</param>
        void RemoveBranch(IBranch branch);

        #region Events, see additional comments on corresponding delegate types

        /// <summary>
        ///     The number of items in the tree has changed
        /// </summary>
        event ItemCountChangedEventHandler ItemCountChanged;

        /// <summary>
        ///     An item has moved in the tree
        /// </summary>
        event ItemMovedEventHandler ItemMoved;

        /// <summary>
        ///     The state of a checkbox has been toggled
        /// </summary>
        event ToggleStateEventHandler StateToggled;

        /// <summary>
        ///     The tree must be refreshed
        /// </summary>
        event RefreshEventHandler OnRefresh;

        /// <summary>
        ///     Redraw is being turned on/off.
        /// </summary>
        event SetRedrawEventHandler OnSetRedraw;

        /// <summary>
        ///     Determine whether an item is visible
        /// </summary>
        event QueryItemVisibleEventHandler OnQueryItemVisible;

        /// <summary>
        ///     A list shuffle is beginning, cache position information
        /// </summary>
        event ListShuffleEventHandler ListShuffleBeginning;

        /// <summary>
        ///     A list shuffle is ending, apply changes to position information
        /// </summary>
        event ListShuffleEventHandler ListShuffleEnding;

        /// <summary>
        ///     Display information must be refreshed
        /// </summary>
        event DisplayDataChangedEventHandler OnDisplayDataChanged;

        /// <summary>
        ///     Fired when a synchronization operation is beginning.  Allows providers to batch process calls to ITree.SynchronizeState.
        ///     In some cases providers can handle synchronization more efficiently as a batch.
        /// </summary>
        event SynchronizeStateEventHandler SynchronizationBeginning;

        /// <summary>
        ///     Fired when a synchronization operation is ending.  Allows providers to batch process calls to ITree.SynchronizeState.
        ///     In some cases providers can handle synchronization more efficiently as a batch.
        /// </summary>
        event SynchronizeStateEventHandler SynchronizationEnding;

        #endregion
    }
}
