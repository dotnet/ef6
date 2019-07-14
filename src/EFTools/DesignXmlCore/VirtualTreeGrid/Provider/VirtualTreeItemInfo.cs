// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System;
    using System.Globalization;

    /// <summary>
    ///     Data returned by the ITree.GetItemInfo method.
    /// </summary>
    internal struct VirtualTreeItemInfo
    {
        #region ItemInfoFlags

        [Flags]
        private enum ItemInfoFlags
        {
            Expanded = 1,
            Expandable = 2,
            FirstBranchItem = 4,
            LastBranchItem = 8,
            LeadingSubItem = 0x10,
            TrailingSubItem = 0x20,
            Blank = 0x40,
            SimpleCell = 0x80,
        }

        #endregion

        #region Debugging helpers

        public override string ToString()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                "Level={0}, Row={1}, Column={2}, Flags={3}, Branch={4}",
                Level, Row, Column, myFlags, Branch);
        }

        #endregion // Debugging helpers

        private ItemInfoFlags myFlags;
        private readonly IBranch myBranch;
        private int myLevel;
        private readonly int myRow;
        private readonly int myColumn;

        internal VirtualTreeItemInfo(IBranch branch, int row, int column, int level)
        {
            myBranch = branch;
            myColumn = column;
            myRow = row;
            myLevel = level;
            myFlags = 0;
        }

        internal void ClearLevel()
        {
            myLevel = -1;
        }

        /// <summary>
        ///     The branch associated with the requested item. Can be null if Blank is true.
        /// </summary>
        public IBranch Branch
        {
            get { return myBranch; }
        }

        /// <summary>
        ///     The indentation level for this branch
        /// </summary>
        public int Level
        {
            get { return myLevel; }
        }

        /// <summary>
        ///     The row in the branch for this item
        /// </summary>
        public int Row
        {
            get { return myRow; }
        }

        /// <summary>
        ///     The column in the branch for this item
        /// </summary>
        public int Column
        {
            get { return myColumn; }
        }

        private bool GetFlag(ItemInfoFlags bit)
        {
            return (myFlags & bit) == bit;
        }

        private void SetFlag(ItemInfoFlags bit, bool value)
        {
            if (value)
            {
                myFlags |= bit;
            }
            else
            {
                myFlags &= ~bit;
            }
        }

        /// <summary>
        ///     Is this item currently expanded?
        ///     Calculated only if setFlags is true.
        /// </summary>
        public bool Expanded
        {
            get { return GetFlag(ItemInfoFlags.Expanded); }
            set { SetFlag(ItemInfoFlags.Expanded, value); }
        }

        /// <summary>
        ///     Is this item expandable?
        ///     Calculated only if setFlags is true.
        /// </summary>
        public bool Expandable
        {
            get { return GetFlag(ItemInfoFlags.Expandable); }
            set { SetFlag(ItemInfoFlags.Expandable, value); }
        }

        /// <summary>
        ///     Can this item be displayed as a simple cell? This flag
        ///     is true for subitems (items in columns > 0) that are in
        ///     a Simple column, are non expandable in an Expandable column,
        ///     or are in a non-expandable branch in a complex cell.
        ///     Calculated only if setFlags is true.
        /// </summary>
        public bool SimpleCell
        {
            get { return GetFlag(ItemInfoFlags.SimpleCell); }
            set { SetFlag(ItemInfoFlags.SimpleCell, value); }
        }

        /// <summary>
        ///     Is this the first item in the branch?
        ///     Calculated only if setFlags is true.
        /// </summary>
        public bool FirstBranchItem
        {
            get { return GetFlag(ItemInfoFlags.FirstBranchItem); }
            set { SetFlag(ItemInfoFlags.FirstBranchItem, value); }
        }

        /// <summary>
        ///     Is this the last item in a branch?
        ///     Calculated only if setFlags is true.
        /// </summary>
        public bool LastBranchItem
        {
            get { return GetFlag(ItemInfoFlags.LastBranchItem); }
            set { SetFlag(ItemInfoFlags.LastBranchItem, value); }
        }

        /// <summary>
        ///     Is this the first item in a set of subitems?
        ///     Calculated only if setFlags is true.
        /// </summary>
        public bool LeadingItem
        {
            get { return GetFlag(ItemInfoFlags.LeadingSubItem); }
            set { SetFlag(ItemInfoFlags.LeadingSubItem, value); }
        }

        /// <summary>
        ///     Is this is the last item in a set of subitems?
        ///     Calculated only if setFlags is true.
        /// </summary>
        public bool TrailingItem
        {
            get { return GetFlag(ItemInfoFlags.TrailingSubItem); }
            set { SetFlag(ItemInfoFlags.TrailingSubItem, value); }
        }

        /// <summary>
        ///     Is the requested coordinate on a blank cell? Blank cells occur
        ///     in multi column lists when a subitem cell contains more than one
        ///     item, or when a branch supports a total number of columns less than
        ///     number of columns supported by the tree. This value is always set, even
        ///     if the setFlags parameter is false.
        /// </summary>
        public bool Blank
        {
            get { return GetFlag(ItemInfoFlags.Blank); }
            set { SetFlag(ItemInfoFlags.Blank, value); }
        }

        #region Equals override and related functions

        /// <summary>
        ///     Equals override. Defers to Compare function.
        /// </summary>
        /// <param name="obj">An item to compare to this object</param>
        /// <returns>True if the items are equal</returns>
        public override bool Equals(object obj)
        {
            if (obj is VirtualTreeItemInfo)
            {
                return Compare(this, (VirtualTreeItemInfo)obj);
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
        public static bool operator ==(VirtualTreeItemInfo operand1, VirtualTreeItemInfo operand2)
        {
            return Compare(operand1, operand2);
        }

        /// <summary>
        ///     Compare two VirtualTreeItemInfo structures
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>true if operands are equal</returns>
        public static bool Compare(VirtualTreeItemInfo operand1, VirtualTreeItemInfo operand2)
        {
            return operand1.myBranch == operand2.myBranch && operand1.myColumn == operand2.myColumn && operand1.myRow == operand2.myRow
                   && operand1.myLevel == operand2.myLevel && operand1.myFlags == operand2.myFlags;
        }

        /// <summary>
        ///     Not equal operator. Defers to Compare.
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns></returns>
        public static bool operator !=(VirtualTreeItemInfo operand1, VirtualTreeItemInfo operand2)
        {
            return !Compare(operand1, operand2);
        }

        #endregion // Equals override and related functions
    }
}
