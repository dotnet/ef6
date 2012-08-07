// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    using System.Collections.Generic;

    /// <summary>
    ///     A tree expression that evaluates to true iff. none of its children
    ///     evaluate to false.
    /// </summary>
    /// <remarks>
    ///     An And expression with no children is equivalent to True (this is an
    ///     operational convenience because we assume an implicit True is along
    ///     for the ride in every And expression)
    /// 
    ///     A . True iff. A
    /// </remarks>
    /// <typeparam name="T_Identifier"> The type of leaf term identifiers in this expression. </typeparam>
    internal class AndExpr<T_Identifier> : TreeExpr<T_Identifier>
    {
        /// <summary>
        ///     Initialize a new And expression with the given children.
        /// </summary>
        /// <param name="children"> Child expressions </param>
        internal AndExpr(params BoolExpr<T_Identifier>[] children)
            : this((IEnumerable<BoolExpr<T_Identifier>>)children)
        {
        }

        /// <summary>
        ///     Initialize a new And expression with the given children.
        /// </summary>
        /// <param name="children"> Child expressions </param>
        internal AndExpr(IEnumerable<BoolExpr<T_Identifier>> children)
            : base(children)
        {
        }

        internal override ExprType ExprType
        {
            get { return ExprType.And; }
        }

        internal override T_Return Accept<T_Return>(Visitor<T_Identifier, T_Return> visitor)
        {
            return visitor.VisitAnd(this);
        }
    }
}
