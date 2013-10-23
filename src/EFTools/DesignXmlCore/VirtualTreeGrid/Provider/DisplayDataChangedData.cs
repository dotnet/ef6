// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    /// <summary>
    ///     The display information for an item has changed.
    /// </summary>
    internal struct DisplayDataChangedData
    {
        private readonly VirtualTreeDisplayDataChanges myChanges;
        private readonly IBranch myBranch;
        private readonly int myStartRow;
        private readonly int myColumn;
        private readonly int myCount;

        /// <summary>
        ///     One or more items in the branch have changed
        /// </summary>
        /// <param name="changes">The fields that have changed</param>
        /// <param name="branch">The branch that has changed</param>
        /// <param name="startRow">The first index in the branch that changed</param>
        /// <param name="column">The column to update, or -1 for all columns</param>
        /// <param name="count">The number of items changed</param>
        public DisplayDataChangedData(VirtualTreeDisplayDataChanges changes, IBranch branch, int startRow, int column, int count)
        {
            myChanges = changes;
            myBranch = branch;
            myStartRow = startRow;
            myColumn = column;
            myCount = count;
        }

        /// <summary>
        ///     All items in the branch have changed
        /// </summary>
        /// <param name="branch">The branch to modify</param>
        public DisplayDataChangedData(IBranch branch)
        {
            myChanges = VirtualTreeDisplayDataChanges.VisibleElements;
            myBranch = branch;
            myStartRow = -1;
            myColumn = -1;
            myCount = 0;
        }

        /// <summary>
        ///     The display fields that have changed
        /// </summary>
        public VirtualTreeDisplayDataChanges Changes
        {
            get { return myChanges; }
        }

        /// <summary>
        ///     The branch that has changed
        /// </summary>
        public IBranch Branch
        {
            get { return myBranch; }
        }

        /// <summary>
        ///     The first index in the branch that changed
        /// </summary>
        public int StartRow
        {
            get { return myStartRow; }
        }

        /// <summary>
        ///     The column to update, or -1 for all columns
        /// </summary>
        /// <value></value>
        public int Column
        {
            get { return myColumn; }
        }

        /// <summary>
        ///     The number of items changed
        /// </summary>
        public int Count
        {
            get { return myCount; }
        }

        #region Equals override and related functions

        /// <summary>
        ///     Equals override. Defers to Compare function.
        /// </summary>
        /// <param name="obj">An item to compare to this object</param>
        /// <returns>True if the items are equal</returns>
        public override bool Equals(object obj)
        {
            if (obj is DisplayDataChangedData)
            {
                return Compare(this, (DisplayDataChangedData)obj);
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
        public static bool operator ==(DisplayDataChangedData operand1, DisplayDataChangedData operand2)
        {
            return Compare(operand1, operand2);
        }

        /// <summary>
        ///     Compare two DisplayDataChangedData structures
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>true if operands are equal</returns>
        public static bool Compare(DisplayDataChangedData operand1, DisplayDataChangedData operand2)
        {
            return operand1.myChanges == operand2.myChanges && operand1.myBranch == operand2.myBranch
                   && operand1.myColumn == operand2.myColumn && operand1.myCount == operand2.myCount
                   && operand1.myStartRow == operand2.myStartRow;
        }

        /// <summary>
        ///     Not equal operator. Defers to Compare.
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns></returns>
        public static bool operator !=(DisplayDataChangedData operand1, DisplayDataChangedData operand2)
        {
            return !Compare(operand1, operand2);
        }

        #endregion // Equals override and related functions
    }
}
