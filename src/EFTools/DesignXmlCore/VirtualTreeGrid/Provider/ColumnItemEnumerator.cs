// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System.Diagnostics;

    /// <summary>
    ///     Efficiently walk the items in a given column. Class returned
    ///     by ITree.EnumerateColumnItems.
    /// </summary>
    internal abstract class ColumnItemEnumerator
    {
        private int myNextStartRow;
        private int myColumn;
        private int myColumnInTree;
        private IBranch myBranch;
        private int myFirstRelativeRow;
        private int myLastRelativeRow;
        private int myRelativeColumn;
        private int myLevel;
        private int myTrailingBlanks;
        private int myCurrentRelativeRow;
        private int myCurrentRow;
        private int myLimitRow;
        private int myStartRow;
        private int myEndRow;
        private readonly int[] myFilter;
        private int myFilterRow;
        private bool myIsSimpleCell;
        private bool myCycle;
        private bool myReturnBlankAnchors;
        private readonly bool myMarkFilterExclusions;
        private ColumnPermutation myColumnPermutation;

        /// <summary>
        ///     Create a new enumerator
        /// </summary>
        /// <param name="column">The column to enumerate</param>
        /// <param name="columnPermutation">
        ///     The column permutation to apply. If this
        ///     is provided, then column is relative to the permutation.
        /// </param>
        /// <param name="returnBlankAnchors">
        ///     If an item is on a horizontal blank expansion, then
        ///     return the anchor for that item. All blanks are skipped if this is false.
        /// </param>
        /// <param name="startRow">The first row to return, 0 to ITree.VisibleItemCount - 1</param>
        /// <param name="endRow">The first row to return, 0 to ITree.VisibleItemCount - 1, can be less than startRow</param>
        /// <param name="itemCount">The number of rows in the desired enumeration range</param>
        protected ColumnItemEnumerator(
            int column, ColumnPermutation columnPermutation, bool returnBlankAnchors, int startRow, int endRow, int itemCount)
        {
            Initialize(column, columnPermutation, returnBlankAnchors, startRow, endRow, itemCount);
        }

        /// <summary>
        ///     Create a new enumerator
        /// </summary>
        /// <param name="column">The column to enumerate</param>
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
        protected ColumnItemEnumerator(
            int column, ColumnPermutation columnPermutation, bool returnBlankAnchors, int[] rowFilter, bool markExcludedFilterItems)
        {
            var startRow = rowFilter[0];
            var endRow = rowFilter[rowFilter.Length - 1];
            Initialize(column, columnPermutation, returnBlankAnchors, startRow, endRow, endRow - startRow + 1);
            myMarkFilterExclusions = markExcludedFilterItems;
            myFilter = rowFilter;
            myFilterRow = 0;
        }

        private void Initialize(
            int column, ColumnPermutation columnPermutation, bool returnBlankAnchors, int startRow, int endRow, int itemCount)
        {
            if (columnPermutation != null)
            {
                column = columnPermutation.GetNativeColumn(column);
            }
            myColumn = column;
            myColumnInTree = column;
            myStartRow = startRow;
            myEndRow = endRow;
            if (endRow < startRow
                && endRow >= 0)
            {
                myCycle = true;
                myLimitRow = itemCount - 1;
            }
            else
            {
                myCycle = false;
                myLimitRow = (endRow < 0) ? itemCount - 1 : endRow;
            }
            myNextStartRow = startRow;
            myCurrentRow = startRow - 1;
            myCurrentRelativeRow = myFirstRelativeRow = myLastRelativeRow = myRelativeColumn = myTrailingBlanks = myLevel = 0;
            myIsSimpleCell = false;
            myReturnBlankAnchors = returnBlankAnchors;
            myColumnPermutation = columnPermutation;
            myBranch = null;
        }

        /// <summary>
        ///     Resets the enumeration.
        /// </summary>
        public void Reset()
        {
            myCurrentRow = myStartRow - 1;
            myNextStartRow = myStartRow;
            myCurrentRelativeRow = myFirstRelativeRow = myLastRelativeRow = myRelativeColumn = myTrailingBlanks = myLevel = 0;
            myIsSimpleCell = false;
            myBranch = null;
            myFilterRow = 0;
            if (!myCycle
                && myEndRow != myLimitRow
                && myEndRow > 0)
            {
                // This means we cycled around.  Swap limit/end rows and reset myCycle.
                var limitRow = myLimitRow;
                myLimitRow = myEndRow;
                myEndRow = limitRow;
                myCycle = true;
            }
        }

        /// <summary>
        ///     Move to the next item in the column
        /// </summary>
        /// <returns>true if there is a next item</returns>
        public bool MoveNext()
        {
            return (myFilter == null) ? MoveNextUnfiltered() : MoveNextFiltered();
        }

        private bool MoveNextUnfiltered()
        {
            if (myBranch != null
                && ++myCurrentRelativeRow <= myLastRelativeRow)
            {
                ++myCurrentRow;
                if (myCurrentRow > myLimitRow)
                {
                    if (myCycle)
                    {
                        return Cycle();
                    }
                    return false;
                }
                return true;
            }
            else
            {
                myBranch = null;
                while (myNextStartRow != VirtualTreeConstant.NullIndex)
                {
                    if (myTrailingBlanks != 0)
                    {
                        myCurrentRow += myTrailingBlanks;
                        if (myCurrentRow > myLimitRow)
                        {
                            break;
                        }
                    }
                    // Defer to something more private to get the next section of data
                    GetNextSection();
                    if (myBranch != null)
                    {
                        ++myCurrentRow;
                        if (myCurrentRow > myLimitRow)
                        {
                            break;
                        }
                        myCurrentRelativeRow = myFirstRelativeRow;
                        return true;
                    }
                }
                if (myCycle)
                {
                    return Cycle();
                }
            }
            return false;
        }

        /// <summary>
        ///     Move to the next item in the column
        /// </summary>
        /// <returns>true if there is a next item</returns>
        private bool MoveNextFiltered()
        {
            var maxFilterRow = myFilter.Length - 1;
            if (myFilterRow > maxFilterRow)
            {
                return false;
            }
            var testIndex = myFilter[myFilterRow];

            // Fastforward MoveNextUnfiltered
            if (myBranch == null
                || myCurrentRelativeRow >= myLastRelativeRow)
            {
                if (myTrailingBlanks > 0)
                {
                    while ((myCurrentRow + myTrailingBlanks) >= testIndex)
                    {
                        if (myMarkFilterExclusions)
                        {
                            myFilter[myFilterRow] = ~testIndex;
                        }
                        ++myFilterRow;
                        if (myFilterRow > maxFilterRow)
                        {
                            return false;
                        }
                        testIndex = myFilter[myFilterRow];
                    }
                    myTrailingBlanks = 0;
                }
                myCurrentRow = testIndex - 1;
                Debug.Assert(myCurrentRow < myLimitRow);
                    // The limit row is based on the last filter row. If this assert fails, mark trailing filter rows and get out
                myNextStartRow = testIndex;
            }

            while (MoveNextUnfiltered())
            {
                // See if we have a match
                if (testIndex == myCurrentRow)
                {
                    ++myFilterRow;
                    return true;
                }
                else if (testIndex < myCurrentRow)
                {
                    // Iterating columns has already skipped the value
                    while (testIndex < myCurrentRow)
                    {
                        if (myMarkFilterExclusions)
                        {
                            myFilter[myFilterRow] = ~testIndex;
                        }
                        ++myFilterRow;
                        if (myFilterRow > maxFilterRow)
                        {
                            return false;
                        }
                        testIndex = myFilter[myFilterRow];
                    }
                    if (testIndex == myCurrentRow)
                    {
                        ++myFilterRow;
                        return true;
                    }
                }

                // The row we're looking for is now higher than the current row in the enumerator.
                // At this point we could just let this loop spin, but that is very inefficient
                // if we're moving a long way, so we make a couple of extra checks.
                if (myCurrentRelativeRow >= myLastRelativeRow)
                {
                    myBranch = null;
                    if (myTrailingBlanks > 0)
                    {
                        while ((myCurrentRow + myTrailingBlanks) >= testIndex)
                        {
                            if (myMarkFilterExclusions)
                            {
                                myFilter[myFilterRow] = ~testIndex;
                            }
                            ++myFilterRow;
                            if (myFilterRow > maxFilterRow)
                            {
                                return false;
                            }
                            testIndex = myFilter[myFilterRow];
                        }
                        myTrailingBlanks = 0;
                    }
                    myCurrentRow = testIndex - 1;

                    // The limit row is based on the last filter row. If this assert fails, mark trailing filter rows and get out
                    Debug.Assert(myCurrentRow < myLimitRow);
                    myNextStartRow = testIndex;
                }
                else
                {
                    var remainingSectionItems = myLastRelativeRow - myCurrentRelativeRow;
                    var distanceToRemaining = myCurrentRow + remainingSectionItems - testIndex;
                    if (distanceToRemaining >= 0)
                    {
                        ++myFilterRow;
                        myCurrentRow = testIndex;
                        myCurrentRelativeRow = myLastRelativeRow - distanceToRemaining;
                        return true;
                    }
                }
            }

            // Clean up any exclusions we haven't hit yet
            if (myMarkFilterExclusions && myFilterRow <= maxFilterRow)
            {
                for (var i = myFilterRow; i <= maxFilterRow; ++i)
                {
                    myFilter[i] = ~myFilter[i];
                }
            }
            return false;
        }

        private bool Cycle()
        {
            Debug.Assert(myCycle);
            myIsSimpleCell = false;
            myTrailingBlanks = 0;
            myBranch = null;
            myCycle = false;
            myNextStartRow = 0;

            // swap myEndRow and myLimitRow.  Need to store the old value of myEndRow somewhere so that we can put it back upon Reset().
            var limitRow = myLimitRow;
            myLimitRow = myEndRow;
            myEndRow = limitRow;
            myCurrentRow = -1;
            return MoveNext();
        }

        // Protected accessors to enable an internal call to EnumOrderedListItems.
        // This allows the tree structure to remain abstract instead of tied to the
        // class implementation.

        /// <summary>
        ///     Protected NextStartRow accessor for internal use.
        /// </summary>
        protected int NextStartRow
        {
            get { return myNextStartRow; }
            set { myNextStartRow = value; }
        }

        /// <summary>
        ///     Protected CurrentBranch accessor for internal use.
        /// </summary>
        protected IBranch CurrentBranch
        {
            get { return myBranch; }
            set { myBranch = value; }
        }

        /// <summary>
        ///     Protected FirstRelativeRow accessor for internal use.
        /// </summary>
        protected int FirstRelativeRow
        {
            get { return myFirstRelativeRow; }
            set { myFirstRelativeRow = value; }
        }

        /// <summary>
        ///     Protected LastRelativeRow accessor for internal use.
        /// </summary>
        protected int LastRelativeRow
        {
            get { return myLastRelativeRow; }
            set { myLastRelativeRow = value; }
        }

        /// <summary>
        ///     Protected RelativeColumn accessor for internal use.
        /// </summary>
        protected int RelativeColumn
        {
            get { return myRelativeColumn; }
            set { myRelativeColumn = value; }
        }

        /// <summary>
        ///     Protected CurrentLevel accessor for internal use.
        /// </summary>
        protected int CurrentLevel
        {
            get { return myLevel; }
            set { myLevel = value; }
        }

        /// <summary>
        ///     Protected CurrentTrailingBlanks accessor for internal use.
        /// </summary>
        protected int CurrentTrailingBlanks
        {
            get { return myTrailingBlanks; }
            set { myTrailingBlanks = value; }
        }

        /// <summary>
        ///     Protected CurrentCellIsSimple accessor for internal use.
        /// </summary>
        protected bool CurrentCellIsSimple
        {
            get { return myIsSimpleCell; }
            set { myIsSimpleCell = value; }
        }

        /// <summary>
        ///     Protected CurrentTreeColumn for internal use.
        /// </summary>
        protected int CurrentTreeColumn
        {
            get { return myColumnInTree; }
            set { myColumnInTree = value; }
        }

        /// <summary>
        ///     Protected GetNextSection accessor for internal use.
        /// </summary>
        protected abstract void GetNextSection();

        /// <summary>
        ///     The branch for the current position in the iterator
        /// </summary>
        public IBranch Branch
        {
            get { return myBranch; }
        }

        /// <summary>
        ///     The row relative to the current branch
        /// </summary>
        public int RowInBranch
        {
            get { return myCurrentRelativeRow; }
        }

        /// <summary>
        ///     The column relative to the current branch
        /// </summary>
        public int ColumnInBranch
        {
            get { return myRelativeColumn; }
        }

        /// <summary>
        ///     The row in the tree for the current item
        /// </summary>
        public int RowInTree
        {
            get { return myCurrentRow; }
        }

        /// <summary>
        ///     The indent level for the current item
        /// </summary>
        public int Level
        {
            get { return myLevel; }
        }

        /// <summary>
        ///     The native column being enumerated.
        /// </summary>
        public int EnumerationColumn
        {
            get { return myColumn; }
        }

        /// <summary>
        ///     The column permutation specified when the enumerator was created
        /// </summary>
        public ColumnPermutation ColumnPermutation
        {
            get { return myColumnPermutation; }
        }

        /// <summary>
        ///     Determines whether the enumerator should return the blank expansion
        ///     anchor for a blank item. If this is true, then ColumnInTree and EnumerationColumn
        ///     can be different.
        /// </summary>
        public bool ReturnBlankAnchors
        {
            get { return myReturnBlankAnchors; }
        }

        /// <summary>
        ///     The column for the current item in the tree. May be
        ///     different than the EnumerationColumn if the iterator
        ///     is allowed to include blank anchors on the same row.
        /// </summary>
        public int ColumnInTree
        {
            get { return myColumnInTree; }
        }

        /// <summary>
        ///     The position where the column is currently displayed in the tree. This
        ///     value is adjusted by the column permutation used to create the enumerator.
        /// </summary>
        public int DisplayColumn
        {
            get { return (myColumnPermutation != null) ? myColumnPermutation.GetPermutedColumn(myColumnInTree) : myColumnInTree; }
        }

        /// <summary>
        ///     Indicates whether the cell is simple
        /// </summary>
        public bool SimpleCell
        {
            get { return myIsSimpleCell; }
        }
    }
}
