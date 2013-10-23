// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    /// <summary>
    ///     Structure returned by IBranch.LocateObject
    /// </summary>
    internal struct LocateObjectData
    {
        private int myRow;
        private int myColumn;
        private int myOptions;

        /// <summary>
        ///     New object location within a branch
        /// </summary>
        /// <param name="row">Row in the branch</param>
        /// <param name="column">Column in the branch</param>
        /// <param name="options">options, data depends on requested ObjectStyle</param>
        public LocateObjectData(int row, int column, int options)
        {
            myRow = row;
            myColumn = column;
            myOptions = options;
        }

        /// <summary>
        ///     Row for the located object
        /// </summary>
        public int Row
        {
            get { return myRow; }
            set { myRow = value; }
        }

        /// <summary>
        ///     Column for the located object
        /// </summary>
        public int Column
        {
            get { return myColumn; }
            set { myColumn = value; }
        }

        /// <summary>
        ///     Options for the located object. Data depends on requested ObjectStyle.
        /// </summary>
        public int Options
        {
            get { return myOptions; }
            set { myOptions = value; }
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
                return Compare(this, (LocateObjectData)obj);
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
        public static bool operator ==(LocateObjectData operand1, LocateObjectData operand2)
        {
            return Compare(operand1, operand2);
        }

        /// <summary>
        ///     Compare two LocateObjectData structures
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>true if operands are equal</returns>
        public static bool Compare(LocateObjectData operand1, LocateObjectData operand2)
        {
            return operand1.myColumn == operand2.myColumn && operand1.myRow == operand2.myRow && operand1.myOptions == operand2.myOptions;
        }

        /// <summary>
        ///     Not equal operator. Defers to Compare.
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns></returns>
        public static bool operator !=(LocateObjectData operand1, LocateObjectData operand2)
        {
            return !Compare(operand1, operand2);
        }

        #endregion // Equals override and related functions
    }
}
