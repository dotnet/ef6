// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    /// A PhysicalProjectOp is a physical Op capping the entire command tree (and the
    /// subtrees of CollectOps).
    /// </summary>
    internal class PhysicalProjectOp : PhysicalOp
    {
        #region public methods

        /// <summary>
        /// Instance for pattern matching in rules
        /// </summary>
        internal static readonly PhysicalProjectOp Pattern = new PhysicalProjectOp();

        /// <summary>
        /// Get the column map that describes how the result should be reshaped
        /// </summary>
        internal SimpleCollectionColumnMap ColumnMap
        {
            get { return m_columnMap; }
        }

        /// <summary>
        /// Get the (ordered) list of output vars that this node produces
        /// </summary>
        internal VarList Outputs
        {
            get { return m_outputVars; }
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

        #region private constructors

        /// <summary>
        /// basic constructor
        /// </summary>
        /// <param name="outputVars"> List of outputs from this Op </param>
        /// <param name="columnMap"> column map that describes the result to be shaped </param>
        internal PhysicalProjectOp(VarList outputVars, SimpleCollectionColumnMap columnMap)
            : this()
        {
            DebugCheck.NotNull(columnMap);
            m_outputVars = outputVars;
            m_columnMap = columnMap;
        }

        private PhysicalProjectOp()
            : base(OpType.PhysicalProject)
        {
        }

        #endregion

        #region private state

        private readonly SimpleCollectionColumnMap m_columnMap;
        private readonly VarList m_outputVars;

        #endregion
    }
}
