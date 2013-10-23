// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System;
    using System.Diagnostics;

    #region ColumnPermutation class

    /// <summary>
    ///     A class used to store column permutations. This is used with ITree.GetBlankExpansion
    ///     and ITree.GetNavigationTarget to enable filtered and rearranged columns in a view on
    ///     the tree.
    /// </summary>
    internal class ColumnPermutation : ICloneable
    {
        // An array indicating the index in myVisibleColumns
        // where the column is displayed, or -1 if the column
        // is hidden.
        private int[] myColumns;
        // The array of columns
        private int[] myVisibleColumns;
        private bool myPreferLeftBlanks;
        // Block default construction
        private ColumnPermutation()
        {
        }

        /// <summary>
        ///     Create a ColumnPermutation object with a given set of
        ///     visible columns, in the order they should be displayed.
        /// </summary>
        /// <param name="totalColumns">
        ///     The total number of columns that can be visible. This
        ///     should correspond to the ColumnCount on the multicolumn tree.
        /// </param>
        /// <param name="visibleColumns">The columns that are visible.</param>
        /// <param name="preferLeftBlanks">
        ///     If there is a choice, attach blank cells to the
        ///     cell on the right instead of the cell on the left. Generally corresponds to the
        ///     RightToLeft property on the tree control.
        /// </param>
        public ColumnPermutation(int totalColumns, int[] visibleColumns, bool preferLeftBlanks)
        {
            var columns = new int[totalColumns];
            int i;

            // Initialize to -1
            for (i = 0; i < totalColumns; ++i)
            {
                columns[i] = -1;
            }

            // Walk the visibleColumns array and fill in the values. When we're done,
            // if (myColumns[i] != -1) then myVisibleColumns[myColumns[i]] == i
            int targetColumn;
            var visibleColumnCount = visibleColumns.Length;
            for (i = 0; i < visibleColumnCount; ++i)
            {
                targetColumn = visibleColumns[i];
                if (columns[targetColumn] != -1) // Let this throw naturally if the argument is out of range
                {
                    throw new ArgumentException(VirtualTreeStrings.GetString(VirtualTreeStrings.DuplicateColumnException), "visibleColumns");
                }
                columns[targetColumn] = i;
            }

            myColumns = columns;
            if (visibleColumns.IsReadOnly)
            {
                myVisibleColumns = visibleColumns;
            }
            else
            {
                myVisibleColumns = (int[])visibleColumns.Clone();
            }

            myPreferLeftBlanks = preferLeftBlanks;
        }

        /// <summary>
        ///     Given a permuted column number, return the native column
        ///     corresponding to this value.
        /// </summary>
        /// <param name="permutedColumn">A column in the range 0 to visibleItems.Length - 1</param>
        /// <returns>The native column, can be used to communicate with ITree and IMultiColumnTree methods</returns>
        public int GetNativeColumn(int permutedColumn)
        {
            return myVisibleColumns[permutedColumn]; // Let this throw naturally if the argument is out of range
        }

        /// <summary>
        ///     Given a column number that is native to the underlying tree, return
        ///     the permuted column number where it currently resides, or -1.
        /// </summary>
        /// <param name="nativeColumn">A column in the range 0 to totalColumns - 1</param>
        /// <returns>The permuted column where this column number is showing, or -1 if it is not visible.</returns>
        public int GetPermutedColumn(int nativeColumn)
        {
            return myColumns[nativeColumn]; // Let this throw naturally if the argument is out of range
        }

        /// <summary>
        ///     The number of visible items visible in this permutation
        /// </summary>
        public int VisibleColumnCount
        {
            get { return myVisibleColumns.Length; }
        }

        /// <summary>
        ///     The number of items in this permutation, including visible and hidden items
        /// </summary>
        public int FullColumnCount
        {
            get { return myColumns.Length; }
        }

        /// <summary>
        ///     In a tree with non-permuted columns blank cells always
        ///     appear on the right end of the row, never in the middle.
        ///     However, with column permutations, blanks can appear to
        ///     the right and/or left of an anchor cell. If there is a
        ///     non-blank cell to both the left and the right of a blank
        ///     cell, then the choice needs to be made which anchor to
        ///     attach it to. This choice is controlled by the PreferLeftBlanks
        ///     property. Normally, this value should be set to correspond to
        ///     the RightToLeft property on the VirtualTreeControl.
        /// </summary>
        public bool PreferLeftBlanks
        {
            get { return myPreferLeftBlanks; }
            set { myPreferLeftBlanks = value; }
        }

        /// <summary>
        ///     Method to change the order of the columns in the permutation
        ///     without fully recreating a new permutation. The indices are given
        ///     in visible columns.
        /// </summary>
        /// <param name="column">The column to move</param>
        /// <param name="newLocation">The new location</param>
        public void MoveVisibleColumn(int column, int newLocation)
        {
            int moveNativeColumn;
            int newNativeColumn;
            if (column < newLocation)
            {
                moveNativeColumn = myVisibleColumns[column];
                for (var i = column; i < newLocation; ++i)
                {
                    newNativeColumn = myVisibleColumns[i + 1];
                    myVisibleColumns[i] = newNativeColumn;
                    myColumns[newNativeColumn] = i;
                }
                myColumns[moveNativeColumn] = newLocation;
                myVisibleColumns[newLocation] = moveNativeColumn;
            }
            else if (column > newLocation)
            {
                moveNativeColumn = myVisibleColumns[column];
                for (var i = column; i > newLocation; --i)
                {
                    newNativeColumn = myVisibleColumns[i - 1];
                    myVisibleColumns[i] = newNativeColumn;
                    myColumns[newNativeColumn] = i;
                }
                myColumns[moveNativeColumn] = newLocation;
                myVisibleColumns[newLocation] = moveNativeColumn;
            }
        }

        /// <summary>
        ///     Change the visible order to match the new order. This allows
        ///     a secondary ordering permutation, such as that used by the header control,
        ///     to work well with our permutation. This routine is optimized to expect a
        ///     single column moving, but will work correctly with any order modification.
        /// </summary>
        /// <param name="oldOrder">The old order, should correspond to the current display order.</param>
        /// <param name="newOrder">The new order. Compare columns to old order to deduce current display order.</param>
        public void ChangeVisibleColumnOrder(int[] oldOrder, int[] newOrder)
        {
            var columns = myVisibleColumns.Length;
            if (oldOrder.Length != columns
                && newOrder.Length != columns)
            {
                throw new ArgumentException(VirtualTreeStrings.GetString(VirtualTreeStrings.InvalidColumnOrderArrayException));
            }

            // The algorithm finds the old values in the new array and moves a column
            // as soon as it is found. To make an accurate move, this means that the
            // positions that have already moved need to be marked as already processed,
            // so we duplicate the old array to enable writing to it.
            var startOrder = oldOrder.Clone() as int[];
            var startIndex = 0;
            var newIndex = 0;
            int startCurrent;
            int newCurrent;
            var totalMarked = 0;
            var permanentlyPassedMarked = 0;
            while (startIndex < columns
                   && newIndex < columns)
            {
                if (-1 == (startCurrent = startOrder[startIndex]))
                {
                    // Slot already handle, go look for one that hasn't been
                    ++startIndex;
                    ++permanentlyPassedMarked;
                }
                else if (startCurrent == (newCurrent = newOrder[newIndex]))
                {
                    ++startIndex;
                    ++newIndex;
                }
                else
                {
                    // The values are different. The first step is to find the
                    // new value in the old array. We care about the position of
                    // the new value, as well as the number of already handled values
                    // after the located, which can be calculated by seeing how
                    // many marked values we pass on the value.
                    var passedMarked = 0;
                    var foundStartIndex = -1;
                    int testValue;
                    for (var i = startIndex + 1; i < columns; ++i)
                    {
                        testValue = startOrder[i];
                        if (testValue == newCurrent)
                        {
                            foundStartIndex = i;
                            break;
                        }
                        else if (testValue == -1)
                        {
                            ++passedMarked;
                        }
                    }
                    if (foundStartIndex == -1)
                    {
                        // We're not going to find any more
                        break;
                    }

                    // Update the arrays accordingly
                    MoveVisibleColumn(foundStartIndex + (totalMarked - permanentlyPassedMarked - passedMarked), newIndex);

                    // Mark this node as processed
                    ++totalMarked;
                    startOrder[foundStartIndex] = -1;

                    // Move to next new item
                    ++newIndex;
                }
            }
        }

        /// <summary>
        ///     Get the blank expansion column information for the given permuted column.
        /// </summary>
        /// <param name="permutedColumn">The permuted column</param>
        /// <param name="lastNativeNonBlankColumn">The last column in the native data structure that contains data</param>
        /// <returns>
        ///     Expansion data with the column fields set. The returned data will always register as Invalid
        ///     because the rows are not set, but the columns are guaranteed to be value.
        /// </returns>
        public BlankExpansionData GetColumnExpansion(int permutedColumn, int lastNativeNonBlankColumn)
        {
            int firstPermutedColumn;
            int lastPermutedColumn;
            int anchorColumn;
            var nativeColumn = myVisibleColumns[permutedColumn];
            Debug.Assert(permutedColumn != -1);

            firstPermutedColumn = lastPermutedColumn = anchorColumn = permutedColumn;
            var haveAnchor = nativeColumn <= lastNativeNonBlankColumn;

            // Lots of bad things can happen with column permutations and blank expansions. We
            // can end up with blanks to the left and the right of the anchor column, as well
            // as an entire row of blanks. We also need to consider whether the user prefers blanks
            // to the right (a standard tree), or the left (a reverse drawn or right-to-left tree).
            // If we're sitting on a blank cell, then this preference influences whether we
            // should look left first or right first for an anchor cell.

            var searchLeft = !myPreferLeftBlanks;
            int i;
            for (var pass = 0; pass < 2; ++pass)
            {
                if (searchLeft)
                {
                    // Go right on the next pass
                    searchLeft = false;
                    // Pick up blank columns to the left
                    if (permutedColumn > 0)
                    {
                        for (i = permutedColumn - 1; i >= 0; --i)
                        {
                            if (myVisibleColumns[i] <= lastNativeNonBlankColumn)
                            {
                                if (haveAnchor)
                                {
                                    // We're looking left to pick up blanks only, not to find an anchor.
                                    // If we find another non-blank column to the left, then any blanks
                                    // between it and our pivot column belong to this node if we prefer
                                    // right blanks. Otherwise, attach them to the current pivot. In any
                                    // case, we do not need to look further.
                                    if (myPreferLeftBlanks)
                                    {
                                        firstPermutedColumn = i + 1;
                                    }
                                    break;
                                }
                                else
                                {
                                    haveAnchor = true;
                                    firstPermutedColumn = anchorColumn = i;
                                    // If we did not have an anchor before, then
                                    // we need to handle the case of a central anchor
                                    // node, which can have blanks both to the left
                                    // and to the right, so we can't break here unless
                                    // there is nothing more to the left.
                                    if (i == 0)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        if (i == -1)
                        {
                            // We fell off the end, so there must be blank columns
                            // all the way to the left of the anchor
                            firstPermutedColumn = 0;
                        }
                    }
                }
                else // !searchLeft
                {
                    // Go left on the next pass
                    searchLeft = true;
                    // Pick up blank columns to the right
                    var visibleColumnCount = myVisibleColumns.Length;
                    var lastVisibleColumn = visibleColumnCount - 1;
                    if (permutedColumn < lastVisibleColumn)
                    {
                        for (i = permutedColumn + 1; i < visibleColumnCount; ++i)
                        {
                            if (myVisibleColumns[i] <= lastNativeNonBlankColumn)
                            {
                                if (haveAnchor)
                                {
                                    // We're looking right to pick up blanks only, not to find an anchor.
                                    // If we find another non-blank column to the right, then any blanks
                                    // between it and our pivot column belong to this node if we prefer
                                    // left blanks. Otherwise, attach them to the current pivot. In any
                                    // case, we do not need to look further.
                                    if (!myPreferLeftBlanks)
                                    {
                                        lastPermutedColumn = i - 1;
                                    }
                                    break;
                                }
                                else
                                {
                                    haveAnchor = true;
                                    lastPermutedColumn = anchorColumn = i;
                                    // If we did not have an anchor before, then
                                    // we need to handle the case of a central anchor
                                    // node, which can have blanks both to the left
                                    // and to the right, so we can't break here unless
                                    // there is nothing more to the left.
                                    if (i == lastVisibleColumn)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        if (i == visibleColumnCount)
                        {
                            // We fell off the end, so there must be blank columns
                            // all the way to the right of the anchor
                            lastPermutedColumn = lastVisibleColumn;
                        }
                    }
                }
            }

            if (!haveAnchor)
            {
                // This implies a totally blank row, which means that the UI code
                // allowed a root column with jagged child branches or complex column
                // to be hidden. Getting this situation to not have blank cells
                // implies pushing column visibility information down into the core
                // TREENODE structures in the core provider engine, which is a huge
                // huge amount of work. There are no plans to do this given the minimal
                // benefit to the end user. The best we'll do is to recognize the situation
                // and not crash at the control level.
                anchorColumn = VirtualTreeConstant.NullIndex;
            }
            return new BlankExpansionData(
                VirtualTreeConstant.NullIndex, firstPermutedColumn, VirtualTreeConstant.NullIndex, lastPermutedColumn, anchorColumn);
        }

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        ///     Clone the permutation
        /// </summary>
        /// <returns>A copy of the current object</returns>
        public ColumnPermutation Clone()
        {
            var copy = MemberwiseClone() as ColumnPermutation;
            copy.myColumns = copy.myColumns.Clone() as int[];
            copy.myVisibleColumns = copy.myVisibleColumns.Clone() as int[];
            return copy;
        }

        #endregion
    }

    #endregion // ColumnPermutation class
}
