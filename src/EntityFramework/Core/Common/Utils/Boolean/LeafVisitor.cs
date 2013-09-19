// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    /// A Visitor class that returns all the leaves in a boolean expression
    /// </summary>
    /// <typeparam name="T_Identifier"> Type of leaf term identifiers in expression. </typeparam>
    internal class LeafVisitor<T_Identifier> : Visitor<T_Identifier, bool>
    {
        private readonly List<TermExpr<T_Identifier>> _terms;

        private LeafVisitor()
        {
            _terms = new List<TermExpr<T_Identifier>>();
        }

        internal static List<TermExpr<T_Identifier>> GetTerms(BoolExpr<T_Identifier> expression)
        {
            DebugCheck.NotNull(expression);
            var visitor = new LeafVisitor<T_Identifier>();
            expression.Accept(visitor);
            return visitor._terms;
        }

        internal static IEnumerable<T_Identifier> GetLeaves(BoolExpr<T_Identifier> expression)
        {
            return GetTerms(expression).Select(term => term.Identifier);
        }

        internal override bool VisitTrue(TrueExpr<T_Identifier> expression)
        {
            return true;
        }

        internal override bool VisitFalse(FalseExpr<T_Identifier> expression)
        {
            return true;
        }

        internal override bool VisitTerm(TermExpr<T_Identifier> expression)
        {
            _terms.Add(expression);
            return true;
        }

        internal override bool VisitNot(NotExpr<T_Identifier> expression)
        {
            return expression.Child.Accept(this);
        }

        internal override bool VisitAnd(AndExpr<T_Identifier> expression)
        {
            return VisitTree(expression);
        }

        internal override bool VisitOr(OrExpr<T_Identifier> expression)
        {
            return VisitTree(expression);
        }

        private bool VisitTree(TreeExpr<T_Identifier> expression)
        {
            foreach (var child in expression.Children)
            {
                child.Accept(this);
            }
            return true;
        }
    }
}
