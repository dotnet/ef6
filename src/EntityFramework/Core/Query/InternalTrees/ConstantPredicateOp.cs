// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Represents a constant predicate (with a value of either true or false)
    /// </summary>
    internal sealed class ConstantPredicateOp : ConstantBaseOp
    {
        #region constructors

        internal ConstantPredicateOp(TypeUsage type, bool value)
            : base(OpType.ConstantPredicate, type, value)
        {
        }

        private ConstantPredicateOp()
            : base(OpType.ConstantPredicate)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly ConstantPredicateOp Pattern = new ConstantPredicateOp();

        /// <summary>
        /// Value of the constant predicate
        /// </summary>
        internal new bool Value
        {
            get { return (bool)base.Value; }
        }

        /// <summary>
        /// Is this the true predicate
        /// </summary>
        internal bool IsTrue
        {
            get { return Value; }
        }

        /// <summary>
        /// Is this the 'false' predicate
        /// </summary>
        internal bool IsFalse
        {
            get { return Value == false; }
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
