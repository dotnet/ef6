// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Cast operation. Convert a type instance into an instance of another type
    /// </summary>
    internal sealed class CastOp : ScalarOp
    {
        #region constructors

        internal CastOp(TypeUsage type)
            : base(OpType.Cast, type)
        {
        }

        private CastOp()
            : base(OpType.Cast)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly CastOp Pattern = new CastOp();

        /// <summary>
        /// 1 child - instance
        /// </summary>
        internal override int Arity
        {
            get { return 1; }
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
