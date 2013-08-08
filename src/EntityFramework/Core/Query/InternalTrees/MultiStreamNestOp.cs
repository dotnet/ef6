// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using md = System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Represents a multi-stream nest operation. The first input represents the
    /// container row, while all the other inputs represent collections
    /// </summary>
    internal class MultiStreamNestOp : NestBaseOp
    {
        #region publics

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v"> The BasicOpVisitor that is visiting this Op </param>
        /// <param name="n"> The Node that references this Op </param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n)
        {
            v.Visit(this, n);
        }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v"> The visitor </param>
        /// <param name="n"> The node in question </param>
        /// <returns> An instance of TResultType </returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n)
        {
            return v.Visit(this, n);
        }

        #endregion

        #region constructors

        internal MultiStreamNestOp(
            List<SortKey> prefixSortKeys, VarVec outputVars,
            List<CollectionInfo> collectionInfoList)
            : base(OpType.MultiStreamNest, prefixSortKeys, outputVars, collectionInfoList)
        {
        }

        #endregion

        #region private state

        #endregion
    }
}
