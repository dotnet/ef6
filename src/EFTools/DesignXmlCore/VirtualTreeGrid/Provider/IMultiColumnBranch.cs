// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    /// <summary>
    ///     Implement IMultiColumnBranch in addition to IBranch to get multi-column branch support
    /// </summary>
    internal interface IMultiColumnBranch
    {
        /// <summary>
        ///     The number of columns supported by the branch
        /// </summary>
        int ColumnCount { get; }

        /// <summary>
        ///     Get the style for an individual column in the tree. This should return
        ///     SubItemCellStyles.Complex only if BranchFeatures.ComplexColumns is also set.
        /// </summary>
        /// <param name="column">The column (>=1)</param>
        /// <returns>The styles of cells in this column</returns>
        SubItemCellStyles ColumnStyles(int column);

        /// <summary>
        ///     Returns the number of columns for a specific row. This
        ///     is only called if the IBranch.BranchFeatures include BranchFeatures.JaggedColumns.
        ///     The number return must be in the range {1,..., ColumnCount}
        /// </summary>
        /// <param name="row">The row to get a column count for</param>
        /// <returns>The number of columns on this row</returns>
        int GetJaggedColumnCount(int row);
    }
}
