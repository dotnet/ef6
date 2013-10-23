// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Shell
{
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;

    /// <summary>
    ///     An implementation of IMultiColumnTree in which the number of columns may vary.  Ideally, perhaps, this
    ///     functionality would be part of IMultiColumnTree itself (i.e, make ColumnCount a read/write property), but
    ///     for now, since there is some coordination required between the tree and branches (such as is provided
    ///     by the OperationDesignerTreeControl/OperationDesignerBranch classes) that is not currently part of IBranch,
    ///     this doesn't make sense at this time.
    /// </summary>
    internal sealed class VariableColumnTree : VirtualTree, IMultiColumnTree
    {
        private int _vctColumns;
        private ITree _singleColumnTree;

        /// <summary>
        ///     Construct a VariableColumnTree
        /// </summary>
        /// <param name="columns"></param>
        internal VariableColumnTree(int columns)
        {
            EnableMultiColumn();
            _vctColumns = columns;
        }

        /// <summary>
        ///     Return our column count
        /// </summary>
        protected override sealed int ColumnCount
        {
            get { return _vctColumns; }
        }

        /// <summary>
        ///     Return the single-column version of this tree
        /// </summary>
        protected override sealed ITree SingleColumnTree
        {
            get
            {
                if (_singleColumnTree == null)
                {
                    _singleColumnTree = CreateSingleColumnTree();
                }
                return _singleColumnTree;
            }
        }

        /// <summary>
        ///     Allows the ability to change the column count
        /// </summary>
        /// <param name="newColumnCount"></param>
        internal void ChangeColumnCount(int newColumnCount)
        {
            _vctColumns = newColumnCount;
        }

        /// <summary>
        ///     IMultiColumnTree implementation
        /// </summary>
        int IMultiColumnTree.ColumnCount
        {
            get { return _vctColumns; }
        }

        /// <summary>
        ///     IMultiColumnTree implementation
        /// </summary>
        void IMultiColumnTree.UpdateCellStyle(IBranch branch, int row, int column, bool makeComplex)
        {
            // It is easier to code this in the base class, defer
            UpdateCellStyle(branch, row, column, makeComplex);
        }

        /// <summary>
        ///     IMultiColumnTree implementation
        /// </summary>
        ITree IMultiColumnTree.SingleColumnTree
        {
            get { return SingleColumnTree; }
        }
    }
}
