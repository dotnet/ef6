// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    #region SubItemColumnAdjustment class

    /// <summary>
    ///     Used with the OnToggleExpansion event to give change item change counts
    ///     for individual columns. Because multiple and complex subitems can introduce
    ///     blanks into a multi column tree grid, it is often possible to expand and
    ///     collapse items without adding or removing rows. Per-column adjustment counts
    ///     are needed to support subitem expansion in these cases without redrawing all
    ///     columns.
    /// </summary>
    internal struct SubItemColumnAdjustment
    {
        private readonly int myColumn;
        private readonly int myLastColumnOnRow;
        private readonly int myChange;
        private readonly int myContainedTrailingItems;
        private readonly int myItemsBelowAnchor;

        internal SubItemColumnAdjustment(int column, int lastColumnOnRow, int change, int containedTrailingItems, int itemsBelowAnchor)
        {
            myColumn = column;
            myChange = change;
            myContainedTrailingItems = containedTrailingItems;
            myLastColumnOnRow = lastColumnOnRow;
            myItemsBelowAnchor = itemsBelowAnchor;
        }

        #region Equals override and related functions

        /// <summary>
        ///     Equals override. Defers to Compare function.
        /// </summary>
        /// <param name="obj">An item to compare to this object</param>
        /// <returns>True if the items are equal</returns>
        public override bool Equals(object obj)
        {
            Debug.Assert(false); // There is no need to compare these
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
        /// <returns>Always returns false, there is no need to compare two SubItemColumnAdjustment structures</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool operator ==(SubItemColumnAdjustment operand1, SubItemColumnAdjustment operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     Compare two SubItemColumnAdjustment structures
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>Always returns false, there is no need to compare two SubItemColumnAdjustment structures</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool Compare(SubItemColumnAdjustment operand1, SubItemColumnAdjustment operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     Not equal operator. Defers to Compare.
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>Always returns true, there is no need to compare two SubItemColumnAdjustment structures</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool operator !=(SubItemColumnAdjustment operand1, SubItemColumnAdjustment operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return true;
        }

        #endregion // Equals override and related functions

        /// <summary>
        ///     The column number in the the grid
        /// </summary>
        public int Column
        {
            get { return myColumn; }
        }

        /// <summary>
        ///     The last column for the owning row of this column
        /// </summary>
        public int LastColumnOnRow
        {
            get { return myLastColumnOnRow; }
        }

        /// <summary>
        ///     The number of local items in the given column that need to
        ///     be added or removed.
        /// </summary>
        public int Change
        {
            get { return myChange; }
        }

        /// <summary>
        ///     The number of unchanged items below the change region that are still wholly contained
        ///     in the subitem column. This is used to optimize drawing of items that have not changed.
        /// </summary>
        public int ContainedTrailingItems
        {
            get { return myContainedTrailingItems; }
        }

        /// <summary>
        ///     The number of (pre-change) items after the anchor position that are contained in the
        ///     full subitem cell.
        /// </summary>
        public int ItemsBelowAnchor
        {
            get { return myItemsBelowAnchor; }
        }
    }

    #endregion

    #region SubItemColumnAdjustmentCollection class

    /// <summary>
    ///     A read-only collection of SubItemColumnAdjustment structures. Used by ItemCountChangedEventArgs
    ///     to disseminate information about changes in sub item columns.
    /// </summary>
    internal sealed class SubItemColumnAdjustmentCollection : IList
    {
        private readonly SubItemColumnAdjustment[] myInner;

        internal SubItemColumnAdjustmentCollection(SubItemColumnAdjustment[] adjustments)
        {
            myInner = adjustments;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            myInner.CopyTo(array, index);
        }

        int ICollection.Count
        {
            get { return myInner.Length; }
        }

        bool ICollection.IsSynchronized
        {
            get { return myInner.IsSynchronized; }
        }

        object ICollection.SyncRoot
        {
            get { return myInner.SyncRoot; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return myInner.GetEnumerator();
        }

        int IList.Add(object value)
        {
            throw new NotSupportedException();
        }

        void IList.Clear()
        {
            throw new NotSupportedException();
        }

        bool IList.Contains(object value)
        {
            return (myInner as IList).Contains(value);
        }

        int IList.IndexOf(object value)
        {
            return (myInner as IList).IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        void IList.Remove(object value)
        {
            throw new NotSupportedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        bool IList.IsFixedSize
        {
            get { return true; }
        }

        bool IList.IsReadOnly
        {
            get { return true; }
        }

        object IList.this[int index]
        {
            get { return myInner[index]; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        ///     Retrieve a SubItemColumnAdjust from the collection
        /// </summary>
        public SubItemColumnAdjustment this[int index]
        {
            get { return myInner[index]; }
        }

        /// <summary>
        ///     Add not supported
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "adjustment")]
        public void Add(SubItemColumnAdjustment adjustment)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     The number of column adjustments in the collection
        /// </summary>
        public int Count
        {
            get { return myInner.Length; }
        }

        /// <summary>
        ///     Copy items into a separate array
        /// </summary>
        /// <param name="array">A pre-allocated array</param>
        /// <param name="index">The starting index to copy from</param>
        public void CopyTo(SubItemColumnAdjustment[] array, int index)
        {
            myInner.CopyTo(array, index);
        }

        /// <summary>
        ///     Find the index of the given value in this collection
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int IndexOf(SubItemColumnAdjustment value)
        {
            return (myInner as IList).IndexOf(value);
        }

        /// <summary>
        ///     Test whether the collection contains this item.
        /// </summary>
        public bool Contains(SubItemColumnAdjustment value)
        {
            return (myInner as IList).Contains(value);
        }

        /// <summary>
        ///     Insert not supported
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "index")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public void Insert(int index, SubItemColumnAdjustment value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Remove not supported
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public void Remove(SubItemColumnAdjustment value)
        {
            throw new NotSupportedException();
        }
    }

    #endregion
}
