// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Windows.Forms;

    #region PositionManagerEventArgs class

    /// <summary>
    ///     Event arguments for ListShuffle events. Positions are bound to items, which
    ///     are then tracked internally as the tree structure is being modified.
    /// </summary>
    internal sealed class PositionManagerEventArgs : EventArgs, IEnumerable
    {
        private readonly Hashtable myTable;
        private readonly VirtualTree myMultiColumnTree;

        internal PositionManagerEventArgs(VirtualTree owningTree)
        {
            myMultiColumnTree = (null != (owningTree as IMultiColumnTree)) ? owningTree : null;
            myTable = new Hashtable();
        }

        /// <summary>
        ///     Store positions to track. Generally called from an BeforeListShuffle event
        /// </summary>
        /// <param name="positions">An array of PositionTracker structures</param>
        /// <param name="key">The key for the tracked positions. Usually the instance of the object tracking positions.</param>
        /// <param name="multiColumnPositions">Specify if the positions use multi or single column indices. Note that this value can be different in the RetrievePositions call.</param>
        public void StorePositions(PositionTracker[] positions, object key, bool multiColumnPositions)
        {
            if (positions != null)
            {
                // Translate from single column positions into multicolumn indices when
                // we store the data, then translate back if needed on the way out. Feeding
                // the position tracking algorithms in the core engine consistent data is well
                // worth the up front translation cost.
                if (myMultiColumnTree != null)
                {
                    int i;
                    int startRow;
                    var rowBound = (myMultiColumnTree as ITree).VisibleItemCount;
                    var positionsCount = positions.Length;
                    if (multiColumnPositions)
                    {
                        // Find any items with a column of 'I don't care' and
                        // bind it to the first possible column.
                        for (i = 0; i < positionsCount; ++i)
                        {
                            if (positions[i].Column == -1)
                            {
                                startRow = positions[i].StartRow;
                                if (startRow != -1
                                    && startRow < rowBound)
                                {
                                    positions[i].Column = myMultiColumnTree.FindFirstNonBlankColumn(startRow);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (i = 0; i < positionsCount; ++i)
                        {
                            startRow = positions[i].StartRow;
                            if (startRow != -1
                                && startRow < rowBound)
                            {
                                positions[i].StartRow = myMultiColumnTree.TranslateSingleColumnRow(startRow);
                                positions[i].Column = 0; // Ignore NoColumnAffinity setting, this will always bind correctly
                            }
                        }
                    }
                }
                myTable[key] = positions;
            }
        }

        /// <summary>
        ///     Retrieve tracked positions. Generally called from an AfterListShuffle event
        /// </summary>
        /// <param name="key">The key for the tracked positions. Usually the instance of the object tracking positions.</param>
        /// <param name="multiColumnPositions">Specify if the positions use multi or single column indices</param>
        /// <returns>An array of PositionTracker structures</returns>
        public PositionTracker[] RetrievePositions(object key, bool multiColumnPositions)
        {
            var positions = myTable[key] as PositionTracker[];
            if (!multiColumnPositions
                && positions != null
                && myMultiColumnTree != null)
            {
                var positionsCount = positions.Length;
                int endRow;
                for (var i = 0; i < positionsCount; ++i)
                {
                    endRow = positions[i].EndRow;
                    if (endRow != -1)
                    {
                        positions[i].EndRow = myMultiColumnTree.TranslateMultiColumnRow(endRow);
                        positions[i].Column = 0;
                    }
                }
            }
            return positions;
        }

        /// <summary>
        ///     Enumerator all PositionTracker arrays in the PositionManager.
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            return myTable.Values.GetEnumerator();
        }
    }

    #endregion

    #region ItemCountChanged event

    /// <summary>
    ///     ItemCountChanged event signature. The expanded state of an item has been toggled, or items have been otherwise inserted or deleted.
    /// </summary>
    internal delegate void ItemCountChangedEventHandler(object sender, ItemCountChangedEventArgs e);

    /// <summary>
    ///     Event arguments for an item count change. Provides details about the location and nature of the change.
    /// </summary>
    internal sealed class ItemCountChangedEventArgs : EventArgs
    {
        private readonly ITree myTree;
        private readonly int myRow;
        private readonly int myColumn;
        private readonly int myChange;
        private readonly int myParentRow;
        private readonly int myBlanksAfterAnchor;
        private readonly SubItemColumnAdjustment[] myColumnAdjustments;
        private SubItemColumnAdjustmentCollection myColumnAdjustmentsCollection;

        internal ItemCountChangedEventArgs(
            ITree tree, int anchorRow, int column, int change, int parentRow, int blanksAfterAnchor,
            SubItemColumnAdjustment[] subItemChanges, bool isToggle)
        {
            myTree = tree;
            myRow = anchorRow;
            myColumn = column;
            myChange = change;
            myParentRow = parentRow;
            myBlanksAfterAnchor = isToggle ? blanksAfterAnchor : -1;
            myColumnAdjustments = subItemChanges;
            myColumnAdjustmentsCollection = null;
        }

        /// <summary>
        ///     The tree where the change is happening
        /// </summary>
        public ITree Tree
        {
            get { return myTree; }
        }

        /// <summary>
        ///     The row immediately before the items being changed. If IsExpansionToggle
        ///     is true, then this is the row being expanded or collapsed. For an insertion (Change &lt; 0),
        ///     this is the row immediately before the items being added. For a deletion (Change &gt; 0), this
        ///     is the row before the first item being deleted.
        /// </summary>
        public int AnchorRow
        {
            get { return myRow; }
        }

        /// <summary>
        ///     The column where the count change is happening.
        /// </summary>
        public int Column
        {
            get { return myColumn; }
        }

        /// <summary>
        ///     The number of items added. Change will be negative for a deletion.
        /// </summary>
        public int Change
        {
            get { return myChange; }
        }

        /// <summary>
        ///     The parent row of the branch the change is occuring in. This can
        ///     be used to stop selection restoration for subitem columns from restoring
        ///     selection in a different subitem block.
        /// </summary>
        public int ParentRow
        {
            get { return myParentRow; }
        }

        /// <summary>
        ///     The number of blank items after the anchor row and before the changes.
        ///     This will only be set if IsExpansionToggle is true.
        /// </summary>
        public int BlanksAfterAnchor
        {
            get { return (myBlanksAfterAnchor == -1) ? 0 : myBlanksAfterAnchor; }
        }

        /// <summary>
        ///     Is this event firing as the result of an expansion being expanded or collapsed in the tree
        /// </summary>
        public bool IsExpansionToggle
        {
            get { return myBlanksAfterAnchor != -1; }
        }

        /// <summary>
        ///     Returns true if SubItemChanges does not return a null collection
        /// </summary>
        public bool HasSubItemChanges
        {
            get { return myColumnAdjustments != null; }
        }

        /// <summary>
        ///     The subitem adjustments that need to be made for this change. Sub item
        ///     adjustments are required if an expansion is made in a column that does
        ///     not affect the total number of items in the list (only applicable if there
        ///     are more than two columns).
        /// </summary>
        public SubItemColumnAdjustmentCollection SubItemChanges
        {
            get
            {
                if (myColumnAdjustmentsCollection == null
                    && myColumnAdjustments != null)
                {
                    myColumnAdjustmentsCollection = new SubItemColumnAdjustmentCollection(myColumnAdjustments);
                }
                return myColumnAdjustmentsCollection;
            }
        }
    }

    #endregion

    #region ItemMoved event

    /// <summary>
    ///     ItemMoved event signature. An item has moved within a branch.
    /// </summary>
    internal delegate void ItemMovedEventHandler(object sender, ItemMovedEventArgs e);

    /// <summary>
    ///     Event arguments indicating that an item has moved within branch
    /// </summary>
    internal sealed class ItemMovedEventArgs : EventArgs
    {
        private readonly ITree myTree;
        private readonly int myColumn;
        private readonly int myFromRow;
        private readonly int myToRow;
        private readonly int myItemCount;
        private readonly bool myUpdateTrailingColumns;

        internal ItemMovedEventArgs(ITree tree, int column, int fromRow, int toRow, int itemCount, bool updateTrailingColumns)
        {
            myTree = tree;
            myColumn = column;
            myFromRow = fromRow;
            myToRow = toRow;
            myItemCount = itemCount;
            myUpdateTrailingColumns = updateTrailingColumns;
        }

        /// <summary>
        ///     The tree sending the event
        /// </summary>
        public ITree Tree
        {
            get { return myTree; }
        }

        /// <summary>
        ///     The column the change happened in. All columns to the
        ///     right of this column must also be changed.
        /// </summary>
        public int Column
        {
            get { return myColumn; }
        }

        /// <summary>
        ///     The row the item moved from.
        /// </summary>
        public int FromRow
        {
            get { return myFromRow; }
        }

        /// <summary>
        ///     The row the item moved to. If ToRow is greater than FromRow, the
        ///     the ToRow value is the final row and assumes that ItemCount items
        ///     are no longer at FromRow.
        /// </summary>
        public int ToRow
        {
            get { return myToRow; }
        }

        /// <summary>
        ///     The number of items to move. If StartRow is not expanded and
        ///     does not have any complex subitems, then this number will be 1.
        /// </summary>
        public int ItemCount
        {
            get { return myItemCount; }
        }

        /// <summary>
        ///     Set to true if items in all columns to the right of the specified column are updated
        /// </summary>
        /// <value>True to move trailing column values with the column, false if only the specified comment is affected</value>
        public bool UpdateTrailingColumns
        {
            get { return myUpdateTrailingColumns; }
        }
    }

    #endregion

    #region ToggleState event

    /// <summary>
    ///     ToggleState event signature. The state icon was clicked.
    /// </summary>
    internal delegate void ToggleStateEventHandler(object sender, ToggleStateEventArgs e);

    /// <summary>
    ///     Event arguments for the result of a state icon being clicked.
    /// </summary>
    internal sealed class ToggleStateEventArgs : EventArgs
    {
        private readonly int myRow;
        private readonly int myColumn;
        private readonly StateRefreshChanges myStateRefreshOptions;

        internal ToggleStateEventArgs(int row, int column, StateRefreshChanges stateRefreshOptions)
        {
            myRow = row;
            myColumn = column;
            myStateRefreshOptions = stateRefreshOptions;
        }

        /// <summary>
        ///     Row coordinate
        /// </summary>
        public int Row
        {
            get { return myRow; }
        }

        /// <summary>
        ///     Column coordinate
        /// </summary>
        public int Column
        {
            get { return myColumn; }
        }

        /// <summary>
        ///     The set of items (relative to the modified item) that need to be refreshed
        /// </summary>
        public StateRefreshChanges StateRefreshOptions
        {
            get { return myStateRefreshOptions; }
        }
    }

    #endregion

    #region Refresh event

    /// <summary>
    ///     Refresh event signature. The list has been refreshed (update count, window, current selection)
    /// </summary>
    internal delegate void RefreshEventHandler(object sender, EventArgs e);

    /// <summary>
    ///     SetRedraw event signature. Turn redraw on/off.
    /// </summary>
    internal delegate void SetRedrawEventHandler(object sender, SetRedrawEventArgs e);

    /// <summary>
    ///     Event arguments for turning off redraw on a view. No requests should
    ///     be made to the ITree implementation when redraw is off.
    /// </summary>
    internal sealed class SetRedrawEventArgs : EventArgs
    {
        private readonly bool myRedrawOn;

        internal SetRedrawEventArgs(bool redrawOn)
        {
            myRedrawOn = redrawOn;
        }

        /// <summary>
        ///     Test whether redraw is on or off
        /// </summary>
        /// <value>true if redraw is turned on, false otherwise</value>
        public bool RedrawOn
        {
            get { return myRedrawOn; }
        }
    }

    #endregion

    #region QueryItemVisible event

    /// <summary>
    ///     QueryItemVisible event signature. Test if an item is currently visible.
    /// </summary>
    internal delegate void QueryItemVisibleEventHandler(object sender, QueryItemVisibleEventArgs e);

    /// <summary>
    ///     Event arguments for requesting information on item visibility.
    /// </summary>
    internal sealed class QueryItemVisibleEventArgs : EventArgs
    {
        private bool myIsVisible;
        private readonly int myRow;

        internal QueryItemVisibleEventArgs(int row)
        {
            myRow = row;
        }

        /// <summary>
        ///     The row to test
        /// </summary>
        /// <value></value>
        public int Row
        {
            get { return myRow; }
        }

        /// <summary>
        ///     Tests whether the item is visible
        /// </summary>
        /// <value>Event handler should set to true if visible. Cannot explicitly be set to false.</value>
        public bool IsVisible
        {
            get { return myIsVisible; }
            set
            {
                if (value)
                {
                    myIsVisible = value;
                }
            }
        }
    }

    #endregion

    #region DisplayDataChanged event

    /// <summary>
    ///     ListShuffle event signature. Track selection and other indices during an extensive list shuffle.
    /// </summary>
    internal delegate void ListShuffleEventHandler(object sender, PositionManagerEventArgs e);

    /// <summary>
    ///     DisplayDataChanged event signature. Refresh the display of the items in the given range
    /// </summary>
    internal delegate void DisplayDataChangedEventHandler(object sender, DisplayDataChangedEventArgs e);

    /// <summary>
    ///     Event arguments describing a display change. DisplayDataChanged events are fired when
    ///     items need updated, but there is no structural change to the tree.
    /// </summary>
    internal sealed class DisplayDataChangedEventArgs : EventArgs
    {
        private readonly ITree myTree;
        private readonly VirtualTreeDisplayDataChanges myChanges;
        private readonly int myStartRow;
        private readonly int myColumn;
        private readonly int myCount;

        internal DisplayDataChangedEventArgs(ITree tree, VirtualTreeDisplayDataChanges changes, int startRow, int column, int count)
        {
            myTree = tree;
            myChanges = changes;
            myStartRow = startRow;
            myColumn = column;
            myCount = count;
        }

        /// <summary>
        ///     The tree that needs refreshing
        /// </summary>
        /// <value>ITree</value>
        public ITree Tree
        {
            get { return myTree; }
        }

        /// <summary>
        ///     The type of update to make. Views can reduce flicker by only updating portions of an item.
        /// </summary>
        /// <value>VirtualTreeDisplayDataChanges</value>
        public VirtualTreeDisplayDataChanges Changes
        {
            get { return myChanges; }
        }

        /// <summary>
        ///     The first row to update.
        /// </summary>
        /// <value></value>
        public int StartRow
        {
            get { return myStartRow; }
        }

        /// <summary>
        ///     The column to update. -1 means all columns need to be refreshed.
        /// </summary>
        /// <value></value>
        public int Column
        {
            get { return myColumn; }
        }

        /// <summary>
        ///     The number of rows that need to be refreshed.
        /// </summary>
        /// <value></value>
        public int Count
        {
            get { return myCount; }
        }
    }

    #region VirtualTreeCoordinate class

    /// <summary>
    ///     Structure representing the global position in a tree.
    /// </summary>
    internal struct VirtualTreeCoordinate
    {
        private int myRow;
        private int myColumn;

        /// <summary>
        ///     Create a new coordinate
        /// </summary>
        /// <param name="row">Coordinate row</param>
        /// <param name="column">Coordinate column</param>
        public VirtualTreeCoordinate(int row, int column)
        {
            myRow = row;
            myColumn = column;
        }

        /// <summary>
        ///     A value representing an invalid coordinate
        /// </summary>
        public static readonly VirtualTreeCoordinate Invalid = new VirtualTreeCoordinate(VirtualTreeConstant.NullIndex, 0);

        /// <summary>
        ///     Test if this is a valid coordinate
        /// </summary>
        /// <value>true if structure does not represent a valid coordinate</value>
        public bool IsValid
        {
            get { return myRow != VirtualTreeConstant.NullIndex; }
        }

        /// <summary>
        ///     The coordinate row
        /// </summary>
        /// <value>Nonnegative value for a valid coordinate</value>
        public int Row
        {
            get { return myRow; }
            set { myRow = value; }
        }

        /// <summary>
        ///     The coordinate column
        /// </summary>
        /// <value>Nonnegative value</value>
        public int Column
        {
            get { return myColumn; }
            set { myColumn = value; }
        }

        #region Equals override and related functions

        /// <summary>
        ///     Equals override. Defers to Compare function.
        /// </summary>
        /// <param name="obj">An item to compare to this object</param>
        /// <returns>True if the items are equal</returns>
        public override bool Equals(object obj)
        {
            if (obj is LocateObjectData)
            {
                return Compare(this, (VirtualTreeCoordinate)obj);
            }
            return false;
        }

        /// <summary>
        ///     GetHashCode override
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            // We're forced to override this with the Equals override.
            return base.GetHashCode();
        }

        /// <summary>
        ///     Equals operator. Defers to Compare.
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns></returns>
        public static bool operator ==(VirtualTreeCoordinate operand1, VirtualTreeCoordinate operand2)
        {
            return Compare(operand1, operand2);
        }

        /// <summary>
        ///     Compare two VirtualTreeCoordinate structures
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>true if operands are equal</returns>
        public static bool Compare(VirtualTreeCoordinate operand1, VirtualTreeCoordinate operand2)
        {
            return operand1.myColumn == operand2.myColumn && operand1.myRow == operand2.myRow;
        }

        /// <summary>
        ///     Not equal operator. Defers to Compare.
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns></returns>
        public static bool operator !=(VirtualTreeCoordinate operand1, VirtualTreeCoordinate operand2)
        {
            return !Compare(operand1, operand2);
        }

        #endregion // Equals override and related functions
    }

    #endregion

    #endregion

    #region BranchModification event

    /// <summary>
    ///     Event arguments for the IBranch.OnBranchModification event. Static
    ///     methods on this class are used to generate arguments for firing this
    ///     event, which modifies all parties using the branch that a modification
    ///     has occurred.
    /// </summary>
    internal abstract class BranchModificationEventArgs : EventArgs
    {
        private BranchModificationEventArgs()
        {
        }

        //These are marked internal so that the appropriate constructors are used
        //to simulate the methods on the tree branch instead of setting the fields directly

        private class BranchModificationMost : BranchModificationEventArgs
        {
            public BranchModificationMost(
                BranchModificationAction action,
                IBranch branch,
                int index,
                int count)
            {
                Action = action;
                Branch = branch;
                Index = index;
                Count = count;
                Flag = false;
            }

            public BranchModificationMost(
                BranchModificationAction action,
                bool flag)
            {
                Action = action;
                Flag = flag;
                Branch = null;
            }

            public BranchModificationMost(
                BranchModificationAction action,
                IBranch branch,
                int index,
                int count,
                bool flag)
            {
                Action = action;
                Branch = branch;
                Index = index;
                Count = count;
                Flag = flag;
            }

            public BranchModificationMost(
                BranchModificationAction action,
                IBranch branch)
            {
                Action = action;
                Branch = branch;
            }
        }

        internal class BranchModificationDisplayData : BranchModificationEventArgs
        {
            // The rest are used for DisplayDataChanged only
            internal int Column;
            internal VirtualTreeDisplayDataChanges Changes;

            public BranchModificationDisplayData(BranchModificationAction action, ref DisplayDataChangedData changeData)
            {
                Action = action;
                Changes = changeData.Changes;
                Branch = changeData.Branch;
                Index = changeData.StartRow;
                Column = changeData.Column;
                Count = changeData.Count;
                Flag = false;
            }
        }

        internal class BranchModificationLevelShift : BranchModificationEventArgs
        {
            // The rest are used for ShiftBranchLevels only
            internal int Depth;
            internal int NewCount;
            internal int RemoveLevels;
            internal int InsertLevels;
            internal IBranch ReplacementBranch;
            internal ILevelShiftAdjuster BranchTester;

            public BranchModificationLevelShift(BranchModificationAction action, ref ShiftBranchLevelsData shiftData)
            {
                Action = action;
                Branch = shiftData.Branch;
                Flag = false;
                Index = shiftData.StartIndex;
                Count = shiftData.Count;
                Depth = shiftData.Depth;
                NewCount = shiftData.NewCount;
                RemoveLevels = shiftData.RemoveLevels;
                InsertLevels = shiftData.InsertLevels;
                ReplacementBranch = shiftData.ReplacementBranch;
                BranchTester = shiftData.BranchTester;
            }
        }

        /// <summary>
        ///     The displayed data for the branch has changed, and
        ///     any views on this tree need to be notified.
        /// </summary>
        /// <param name="changeData">The DisplayDataChangedData structure</param>
        /// <returns>An events args object for IBranch.OnBranchModification</returns>
        public static BranchModificationEventArgs DisplayDataChanged(DisplayDataChangedData changeData)
        {
            return new BranchModificationDisplayData(BranchModificationAction.DisplayDataChanged, ref changeData);
        }

        /// <summary>
        ///     The order and count of the items in this branch has changed.
        /// </summary>
        /// <param name="branch">The branch to modify, or null for all branches</param>
        /// <returns>An events args object for IBranch.OnBranchModification</returns>
        public static BranchModificationEventArgs Realign(IBranch branch)
        {
            return new BranchModificationMost(BranchModificationAction.Realign, branch);
        }

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
        /// <returns>An events args object for IBranch.OnBranchModification</returns>
        public static BranchModificationEventArgs InsertItems(IBranch branch, int after, int count)
        {
            return new BranchModificationMost(BranchModificationAction.InsertItems, branch, after, count);
        }

        /// <summary>
        ///     Delete specific items without calling Realign
        /// </summary>
        /// <param name="branch">The branch where items have been removed</param>
        /// <param name="start">The first item to deleted</param>
        /// <param name="count">The number of items to delete</param>
        /// <returns>An events args object for IBranch.OnBranchModification</returns>
        public static BranchModificationEventArgs DeleteItems(IBranch branch, int start, int count)
        {
            return new BranchModificationMost(BranchModificationAction.DeleteItems, branch, start, count);
        }

        /// <summary>
        ///     Change the position of a single item in a branch
        /// </summary>
        /// <param name="branch">The branch where the item moved</param>
        /// <param name="fromRow">The row the item used to be on</param>
        /// <param name="toRow">The row the item is on now</param>
        /// <returns>An events args object for IBranch.OnBranchModification</returns>
        public static BranchModificationEventArgs MoveItem(IBranch branch, int fromRow, int toRow)
        {
            return new BranchModificationMost(BranchModificationAction.MoveItem, branch, fromRow, toRow);
        }

        /// <summary>
        ///     Views on the tree should not attempt to redraw any items when Redraw is off.
        ///     Calls to Redraw can be nested, and must be balanced.
        /// </summary>
        /// <param name="value">false to turn off redraw, true to restore it.</param>
        /// <returns>An events args object for IBranch.OnBranchModification</returns>
        public static BranchModificationEventArgs Redraw(bool value)
        {
            return new BranchModificationMost(BranchModificationAction.Redraw, value);
        }

        /// <summary>
        ///     Used to batch calls to Redraw without triggering unnecessary redraw operations.
        ///     Set this property to true if an operation may cause one or more redraw calls, then
        ///     to false on completion. The cost is negligible if Redraw is never triggered, whereas
        ///     an unneeded Redraw true/false can be very expensive. Calls to DelayRedraw can
        ///     be nested, and must be balanced.
        /// </summary>
        /// <param name="value">true to delay, false to finish operation</param>
        /// <returns>An events args object for IBranch.OnBranchModification</returns>
        public static BranchModificationEventArgs DelayRedraw(bool value)
        {
            return new BranchModificationMost(BranchModificationAction.DelayRedraw, value);
        }

        /// <summary>
        ///     A significant change is being made to the layout of the list, so give
        ///     any views on this object the chance to cache selection information. Calls to
        ///     ListShuffle can be nested, and must be balanced.
        /// </summary>
        /// <param name="value">true to start a shuffle (cache state), false to end one</param>
        /// <returns>An events args object for IBranch.OnBranchModification</returns>
        public static BranchModificationEventArgs ListShuffle(bool value)
        {
            return new BranchModificationMost(BranchModificationAction.ListShuffle, value);
        }

        /// <summary>
        ///     Used to batch calls to ListShuffle without triggering unnecessary shuffle operations.
        ///     Set this property to true if an operation may cause one or more list shuffles, then
        ///     to false on completion. The cost is negligible if a shuffle is never triggered, whereas
        ///     an unneeded ListShuffle true/false can be very expensive. Calls to DelayListShuffle can
        ///     be nested, and must be balanced.
        /// </summary>
        /// <param name="value">true to delay, false to finish operation</param>
        /// <returns>An events args object for IBranch.OnBranchModification</returns>
        public static BranchModificationEventArgs DelayListShuffle(bool value)
        {
            return new BranchModificationMost(BranchModificationAction.DelayListShuffle, value);
        }

        /// <summary>
        ///     Add or remove entire levels from existing branch structures in the tree
        /// </summary>
        /// <param name="shiftData">The data for the shift operation</param>
        /// <returns>An events args object for IBranch.OnBranchModification</returns>
        public static BranchModificationEventArgs ShiftBranchLevels(ShiftBranchLevelsData shiftData)
        {
            return new BranchModificationLevelShift(BranchModificationAction.ShiftBranchLevels, ref shiftData);
        }

        /// <summary>
        ///     A mechanism for changing a cell from simple or expandable
        ///     to complex, or vice versa. This enables a potentially
        ///     complex cell to begin life as a simple cell, then switch later.
        ///     The makeComplex variable is interpreted according to the
        ///     cell style settings for the given branch.
        /// </summary>
        /// <param name="branch">The branch to modify</param>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <param name="makeComplex">True to switch to a complex cell, false to switch to a simple cell</param>
        /// <returns>An events args object for IBranch.OnBranchModification</returns>
        public static BranchModificationEventArgs UpdateCellStyle(IBranch branch, int row, int column, bool makeComplex)
        {
            return new BranchModificationMost(BranchModificationAction.UpdateCellStyle, branch, row, column, makeComplex);
        }

        /// <summary>
        ///     Remove all occurrences of branch from the tree. Note that removing all
        ///     items from a branch is not the same as removing the branch itself.
        /// </summary>
        /// <param name="branch">The branch to remove</param>
        /// <returns>An events args object for IBranch.OnBranchModification</returns>
        public static BranchModificationEventArgs RemoveBranch(IBranch branch)
        {
            return new BranchModificationMost(BranchModificationAction.RemoveBranch, branch);
        }

        /// <summary>
        ///     The action represented by this branch modification
        /// </summary>
        public BranchModificationAction Action { get; set; }

        /// <summary>
        ///     The Branch property corresponds to the branch parameter, if present, for all events
        /// </summary>
        public IBranch Branch { get; set; }

        /// <summary>
        ///     The Flag property corresponds to different input parameters for different events.
        ///     Redraw:value,
        ///     DelayRedraw:value,
        ///     ListShuffle:value,
        ///     DelayListShuffle:value,
        ///     UpdateCellStyle:makeComplex,
        /// </summary>
        public bool Flag { get; set; }

        /// <summary>
        ///     The Index property corresponds to different input parameters for different events.
        ///     DisplayDataChanged:startIndex,
        ///     InsertItems:after,
        ///     DeleteItems:start,
        ///     ShiftBranchLevels:row,
        ///     DisplayDataChanged:count,
        ///     UpdateCellStyle:column,
        ///     MoveItem:from
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        ///     The Count property corresponds to different input parameters for different events.
        ///     InsertItems:count,
        ///     DeleteItems:count,
        ///     ShiftBranchLevels:count,
        ///     DisplayDataChanged:count,
        ///     UpdateCellStyle:column,
        ///     MoveItem:to
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    ///     Event signature for all branch modification actions.
    ///     The change being made is described by the BranchModificationEventArgs.
    /// </summary>
    internal delegate void BranchModificationEventHandler(object sender, BranchModificationEventArgs e);

    #endregion

    #region Synchronization event

    /// <summary>
    ///     Synchronization event signature.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    internal delegate void SynchronizeStateEventHandler(object sender, SynchronizeStateEventArgs e);

    /// <summary>
    ///     Event arguments describing a range of items in the tree to be synchronized.
    /// </summary>
    internal class SynchronizeStateEventArgs : EventArgs
    {
        private readonly ColumnItemEnumerator myItemsToSynchronize;
        private readonly IBranch myMatchBranch;
        private readonly int myMatchRow;
        private readonly int myMatchColumn;
        private readonly VirtualTree myTree;

        /// <summary>
        ///     Create a new SynchronizeStateEventArgs.
        /// </summary>
        /// <param name="tree">Tree raising this event.  Necessary to allow clients that handle synchronization themeselves to raise events back to the tree.</param>
        /// <param name="itemsToSynchronize">Enumerator of items to be synchronized.</param>
        /// <param name="matchBranch">Branch whose state should be matched.</param>
        /// <param name="matchRow">Row whose state should be matched.</param>
        /// <param name="matchColumn">Column whose state should be matched.</param>
        public SynchronizeStateEventArgs(
            ITree tree, ColumnItemEnumerator itemsToSynchronize, IBranch matchBranch, int matchRow, int matchColumn)
        {
            Handled = false;
            myItemsToSynchronize = itemsToSynchronize;
            myMatchBranch = matchBranch;
            myMatchRow = matchRow;
            myMatchColumn = matchColumn;
            myTree = tree as VirtualTree;
        }

        /// <summary>
        ///     Flag that indicates whether this Synchronization event has been processed.  If this is set to true by a handler
        ///     of the ITree.SynchronizationBeginning event, subsequent calls to IBranch.SynchronizeState will not be made.  The
        ///     ITree.SynchronizationEnding event will still be raised, however.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        ///     Enumerator that allows listeners to iterate over the items to be synchronized.  Handlers should not change the structure
        ///     of the tree during this event, so this enumerator does not get out of sync.  Also, before use, handlers should call
        ///     Reset() to ensure enumeration starts from the beginning.
        /// </summary>
        public ColumnItemEnumerator ItemsToSynchronize
        {
            get { return myItemsToSynchronize; }
        }

        /// <summary>
        ///     The branch whose state the items in the ItemsToSynchronize enumerator should be synchronized to.
        /// </summary>
        public IBranch MatchBranch
        {
            get { return myMatchBranch; }
        }

        /// <summary>
        ///     The row index (relative to MatchBranch) that indicates the item in MatchBranch that the items in the ItemsToSynchronize enumerator should be synchronized to.
        /// </summary>
        public int MatchRow
        {
            get { return myMatchRow; }
        }

        /// <summary>
        ///     The column index that indicates the item in MatchBranch that the items in the ItemsToSynchronize enumerator should be synchronized to.
        /// </summary>
        public int MatchColumn
        {
            get { return myMatchColumn; }
        }

        /// <summary>
        ///     Allows providers listening to synchronization events to communicate state changes back to the tree.  This
        ///     is required so that the tree sends out the appropriate notifications, which, when the tree is attached to a
        ///     control, will result in things like correct repaint and firing accessibility state change events.
        /// </summary>
        /// <param name="stateChanges">Description of changes.</param>
        /// <param name="row">Row that changed.</param>
        /// <param name="column">Column that changed.</param>
        public void NotifyStateChange(StateRefreshChanges stateChanges, int row, int column)
        {
            Debug.Assert(myTree != null, "unable to notify state change.");
            if (myTree != null)
            {
                myTree.NotifyStateChange(row, column, stateChanges);
            }
        }
    }

    #endregion

    /// <summary>
    ///     A callback delegate to enable a richer label edit commit experience than IBranch.CommitLabelEdit.
    ///     The callback receives information about the item that was commit, and the instance of the control
    ///     used to commit the edit.
    /// </summary>
    internal delegate LabelEditResult CommitLabelEditCallback(VirtualTreeItemInfo itemInfo, Control editControl);
}
