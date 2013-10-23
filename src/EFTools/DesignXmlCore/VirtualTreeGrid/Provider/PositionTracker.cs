// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    /// <summary>
    ///     A structure used to track a global position in a tree across a significant change
    ///     in the tree structure. Used with the BeforeListShuffle and AfterListShuffle events.
    /// </summary>
    internal struct PositionTracker
    {
        // CONSIDER: Support tracking an item across columns
        private int myColumn;
        private int myStartRow;
        private int myEndRow;
        private int myUserData;

        /// <summary>
        ///     The column for the item being tracked.
        /// </summary>
        public int Column
        {
            get { return myColumn; }
            set { myColumn = value; }
        }

        /// <summary>
        ///     Provide initial values for the StartRow and Column
        ///     properties.
        /// </summary>
        /// <param name="row">The row to track</param>
        /// <param name="column">The column to track, or -1 to track the first non-blank column.</param>
        public void Initialize(int row, int column)
        {
            myStartRow = row;
            myColumn = column;
        }

        /// <summary>
        ///     The initial row for this item. -1 indicates the item could not be tracked.
        /// </summary>
        public int StartRow
        {
            get { return myStartRow; }
            set { myStartRow = value; }
        }

        /// <summary>
        ///     The final row for this item. -1 indicates the item could not be tracked.
        /// </summary>
        public int EndRow
        {
            get { return myEndRow; }
            set { myEndRow = value; }
        }

        /// <summary>
        ///     Arbitrary user data. Not used by the ITree implementation.
        /// </summary>
        public int UserData
        {
            get { return myUserData; }
            set { myUserData = value; }
        }

        #region Equals override and related functions

        /// <summary>
        ///     Equals override. Defers to Compare function.
        /// </summary>
        /// <param name="obj">An item to compare to this object</param>
        /// <returns>True if the items are equal</returns>
        public override bool Equals(object obj)
        {
            if (obj is PositionTracker)
            {
                return Compare(this, (PositionTracker)obj);
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
        public static bool operator ==(PositionTracker operand1, PositionTracker operand2)
        {
            return Compare(operand1, operand2);
        }

        /// <summary>
        ///     Compare two PositionTracker structures
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>true if operands are equal</returns>
        public static bool Compare(PositionTracker operand1, PositionTracker operand2)
        {
            return operand1.myColumn == operand2.myColumn && operand1.myStartRow == operand2.myStartRow
                   && operand1.myEndRow == operand2.myEndRow && operand1.myUserData == operand2.myUserData;
        }

        /// <summary>
        ///     Not equal operator. Defers to Compare.
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns></returns>
        public static bool operator !=(PositionTracker operand1, PositionTracker operand2)
        {
            return !Compare(operand1, operand2);
        }

        #endregion // Equals override and related functions
    }
}
