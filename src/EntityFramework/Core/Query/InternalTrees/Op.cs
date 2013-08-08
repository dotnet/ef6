// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics;

    /// <summary>
    /// Represents an operator
    /// </summary>
    internal abstract class Op
    {
        #region private state

        private readonly OpType m_opType;

        #endregion

        #region constructors

        /// <summary>
        /// Basic constructor
        /// </summary>
        internal Op(OpType opType)
        {
            m_opType = opType;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Represents an unknown arity. Usually for Ops that can have a varying number of Args
        /// </summary>
        internal const int ArityVarying = -1;

        /// <summary>
        /// Kind of Op
        /// </summary>
        internal OpType OpType
        {
            get { return m_opType; }
        }

        /// <summary>
        /// The Arity of this Op (ie) how many arguments can it have.
        /// Returns -1 if the arity is not known a priori
        /// </summary>
        internal virtual int Arity
        {
            get { return ArityVarying; }
        }

        /// <summary>
        /// Is this a ScalarOp
        /// </summary>
        internal virtual bool IsScalarOp
        {
            get { return false; }
        }

        /// <summary>
        /// Is this a RulePatternOp
        /// </summary>
        internal virtual bool IsRulePatternOp
        {
            get { return false; }
        }

        /// <summary>
        /// Is this a RelOp
        /// </summary>
        internal virtual bool IsRelOp
        {
            get { return false; }
        }

        /// <summary>
        /// Is this an AncillaryOp
        /// </summary>
        internal virtual bool IsAncillaryOp
        {
            get { return false; }
        }

        /// <summary>
        /// Is this a PhysicalOp
        /// </summary>
        internal virtual bool IsPhysicalOp
        {
            get { return false; }
        }

        /// <summary>
        /// Is the other Op equivalent?
        /// </summary>
        /// <param name="other"> the other Op to compare </param>
        /// <returns> true, if the Ops are equivalent </returns>
        internal virtual bool IsEquivalent(Op other)
        {
            return false;
        }

        /// <summary>
        /// Simple mechanism to get the type for an Op. Applies only to scalar and ancillaryOps
        /// </summary>
        internal virtual TypeUsage Type
        {
            get { return null; }
            set { throw Error.NotSupported(); }
        }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v"> The BasicOpVisitor that is visiting this Op </param>
        /// <param name="n"> The Node that references this Op </param>
        [DebuggerNonUserCode]
        internal virtual void Accept(BasicOpVisitor v, Node n)
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
        internal virtual TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n)
        {
            return v.Visit(this, n);
        }

        #endregion
    }
}
