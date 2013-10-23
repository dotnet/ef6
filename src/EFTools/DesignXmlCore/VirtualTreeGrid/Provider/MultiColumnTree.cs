// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{

    #region IMultiColumnTree definition

    /// <summary>
    ///     An interface to implement in addition to ITree to enable multi column support
    /// </summary>
    internal interface IMultiColumnTree
    {
        /// <summary>
        ///     The number of columns supported by this tree
        /// </summary>
        /// <value>A positive column count</value>
        int ColumnCount { get; }

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
        void UpdateCellStyle(IBranch branch, int row, int column, bool makeComplex);

        /// <summary>
        ///     A single column view on the first column of the multi-column tree.
        ///     Allows a multi-column tree to exist simultaneously in both multi and
        ///     single column states.
        /// </summary>
        /// <value>The single-column view on the tree</value>
        ITree SingleColumnTree { get; }
    }

    #endregion

    #region MultiColumnTree Implementation

    /// <summary>
    ///     The data object for a multi-column tree.
    /// </summary>
    internal class MultiColumnTree : VirtualTree, IMultiColumnTree
    {
        private readonly int myColumns;
        private ITree mySingleColumnTree;

        /// <summary>
        ///     Create a new multi column tree
        /// </summary>
        /// <param name="columns">The maximum number of columns supported by the tree</param>
        public MultiColumnTree(int columns)
        {
            EnableMultiColumn();
            myColumns = columns;
        }

        /// <summary>
        ///     Override to supply the number of columns
        /// </summary>
        /// <value>The total number of columns support by the multi column tree</value>
        protected override sealed int ColumnCount
        {
            get { return myColumns; }
        }

        /// <summary>
        ///     Return the single column view on the multi column tree.
        /// </summary>
        /// <value>An ITree instance representing the single-column view</value>
        protected override sealed ITree SingleColumnTree
        {
            get
            {
                if (mySingleColumnTree == null)
                {
                    mySingleColumnTree = CreateSingleColumnTree();
                }
                return mySingleColumnTree;
            }
        }

        int IMultiColumnTree.ColumnCount
        {
            get { return myColumns; }
        }

        void IMultiColumnTree.UpdateCellStyle(IBranch branch, int row, int column, bool makeComplex)
        {
            // It is easier to code this in the base class, defer
            UpdateCellStyle(branch, row, column, makeComplex);
        }

        ITree IMultiColumnTree.SingleColumnTree
        {
            get { return SingleColumnTree; }
        }
    }

    #endregion // MultiColumnTree Implementation
}
