// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    /// ProjectOp
    /// </summary>
    internal sealed class ProjectOp : RelOp
    {
        #region private state

        private readonly VarVec m_vars;

        #endregion

        #region constructors

        private ProjectOp()
            : base(OpType.Project)
        {
        }

        internal ProjectOp(VarVec vars)
            : this()
        {
            DebugCheck.NotNull(vars);
            Debug.Assert(!vars.IsEmpty, "empty varlist?");
            m_vars = vars;
        }

        #endregion

        #region public methods

        internal static readonly ProjectOp Pattern = new ProjectOp();

        /// <summary>
        /// 2 children - input, projections (VarDefList)
        /// </summary>
        internal override int Arity
        {
            get { return 2; }
        }

        /// <summary>
        /// The Vars projected by this Op
        /// </summary>
        internal VarVec Outputs
        {
            get { return m_vars; }
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
    }
}
