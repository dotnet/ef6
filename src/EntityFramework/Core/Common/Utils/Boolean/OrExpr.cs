// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    using System.Collections.Generic;

    /// <summary>
    ///     A tree expression that evaluates to true iff. any of its children
    ///     evaluates to true.
    /// </summary>
    /// <remarks>
    ///     An Or expression with no children is equivalent to False (this is an
    ///     operational convenience because we assume an implicit False is along
    ///     for the ride in every Or expression)
    /// 
    ///     A + False iff. A
    /// </remarks>
    /// <typeparam name="T_Identifier"> The type of leaf term identifiers in this expression. </typeparam>
    internal class OrExpr<T_Identifier> : TreeExpr<T_Identifier>
    {
        /// <summary>
        ///     Initialize a new Or expression with the given children.
        /// </summary>
        /// <param name="children"> Child expressions </param>
        internal OrExpr(params BoolExpr<T_Identifier>[] children)
            : this((IEnumerable<BoolExpr<T_Identifier>>)children)
        {
        }

        /// <summary>
        ///     Initialize a new Or expression with the given children.
        /// </summary>
        /// <param name="children"> Child expressions </param>
        internal OrExpr(IEnumerable<BoolExpr<T_Identifier>> children)
            : base(children)
        {
        }

        internal override ExprType ExprType
        {
            get { return ExprType.Or; }
        }

        internal override T_Return Accept<T_Return>(Visitor<T_Identifier, T_Return> visitor)
        {
            return visitor.VisitOr(this);
        }
    }
}
