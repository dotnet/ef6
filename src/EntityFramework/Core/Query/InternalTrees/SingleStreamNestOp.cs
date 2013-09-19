// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Single-stream nest aggregation Op.
    /// (Somewhat similar to a group-by op - should we merge these?)
    /// </summary>
    internal class SingleStreamNestOp : NestBaseOp
    {
        #region publics

        /// <summary>
        /// 1 child - the input
        /// </summary>
        internal override int Arity
        {
            get { return 1; }
        }

        /// <summary>
        /// The discriminator Var (when there are multiple collections)
        /// </summary>
        internal Var Discriminator
        {
            get { return m_discriminator; }
        }

        /// <summary>
        /// List of postfix sort keys (mostly to deal with multi-level nested collections)
        /// </summary>
        internal List<SortKey> PostfixSortKeys
        {
            get { return m_postfixSortKeys; }
        }

        /// <summary>
        /// Set of keys for this nest operation
        /// </summary>
        internal VarVec Keys
        {
            get { return m_keys; }
        }

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

        internal SingleStreamNestOp(
            VarVec keys,
            List<SortKey> prefixSortKeys, List<SortKey> postfixSortKeys,
            VarVec outputVars, List<CollectionInfo> collectionInfoList,
            Var discriminatorVar)
            : base(OpType.SingleStreamNest, prefixSortKeys, outputVars, collectionInfoList)
        {
            m_keys = keys;
            m_postfixSortKeys = postfixSortKeys;
            m_discriminator = discriminatorVar;
        }

        #endregion

        #region private state

        private readonly VarVec m_keys; // keys for this operation
        private readonly Var m_discriminator; // Var describing the discriminator
        private readonly List<SortKey> m_postfixSortKeys; // list of postfix sort keys 

        #endregion
    }
}
