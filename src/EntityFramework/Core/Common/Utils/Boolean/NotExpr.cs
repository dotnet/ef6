// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    using System.Globalization;
    using System.Linq;

    // <summary>
    // A tree expression that evaluates to true iff. its (single) child evaluates to false.
    // </summary>
    // <typeparam name="T_Identifier"> The type of leaf term identifiers in this expression. </typeparam>
    internal sealed class NotExpr<T_Identifier> : TreeExpr<T_Identifier>
    {
        // <summary>
        // Initialize a new Not expression with the given child.
        // </summary>
        internal NotExpr(BoolExpr<T_Identifier> child)
            : base(new[] { child })
        {
        }

        internal override ExprType ExprType
        {
            get { return ExprType.Not; }
        }

        internal BoolExpr<T_Identifier> Child
        {
            get { return Children.First(); }
        }

        internal override T_Return Accept<T_Return>(Visitor<T_Identifier, T_Return> visitor)
        {
            return visitor.VisitNot(this);
        }

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "!{0}", Child);
        }

        internal override BoolExpr<T_Identifier> MakeNegated()
        {
            return Child;
        }
    }
}
