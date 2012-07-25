// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    using System.Diagnostics;

    internal class TermCounter<T_Identifier> : Visitor<T_Identifier, int>
    {
        private static readonly TermCounter<T_Identifier> _instance = new TermCounter<T_Identifier>();

        internal static int CountTerms(BoolExpr<T_Identifier> expression)
        {
            Debug.Assert(null != expression);
            return expression.Accept(_instance);
        }

        internal override int VisitTrue(TrueExpr<T_Identifier> expression)
        {
            return 0;
        }

        internal override int VisitFalse(FalseExpr<T_Identifier> expression)
        {
            return 0;
        }

        internal override int VisitTerm(TermExpr<T_Identifier> expression)
        {
            return 1;
        }

        internal override int VisitNot(NotExpr<T_Identifier> expression)
        {
            return expression.Child.Accept(this);
        }

        internal override int VisitAnd(AndExpr<T_Identifier> expression)
        {
            return VisitTree(expression);
        }

        internal override int VisitOr(OrExpr<T_Identifier> expression)
        {
            return VisitTree(expression);
        }

        private int VisitTree(TreeExpr<T_Identifier> expression)
        {
            var sum = 0;
            foreach (var child in expression.Children)
            {
                sum += child.Accept(this);
            }
            return sum;
        }
    }
}
