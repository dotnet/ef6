// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Represents an arbitrary function call
    /// </summary>
    internal sealed class FunctionOp : ScalarOp
    {
        #region private state

        private readonly EdmFunction m_function;

        #endregion

        #region constructors

        internal FunctionOp(EdmFunction function)
            : base(OpType.Function, function.ReturnParameter.TypeUsage)
        {
            m_function = function;
        }

        private FunctionOp()
            : base(OpType.Function)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        /// Singleton instance used for patterns in transformation rules
        /// </summary>
        internal static readonly FunctionOp Pattern = new FunctionOp();

        /// <summary>
        /// The function that's being invoked
        /// </summary>
        internal EdmFunction Function
        {
            get { return m_function; }
        }

        /// <summary>
        /// Two FunctionOps are equivalent if they reference the same EdmFunction
        /// </summary>
        /// <param name="other">the other Op</param>
        /// <returns>true, if these are equivalent</returns>
        internal override bool IsEquivalent(Op other)
        {
            var otherFunctionOp = other as FunctionOp;
            return (otherFunctionOp != null && otherFunctionOp.Function.EdmEquals(Function));
        }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n)
        {
            v.Visit(this, n);
        }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n)
        {
            return v.Visit(this, n);
        }

        #endregion
    }
}
