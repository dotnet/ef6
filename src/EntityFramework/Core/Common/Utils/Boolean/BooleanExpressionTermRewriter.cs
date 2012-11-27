// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Rewrites the terms in a Boolean expression tree.
    /// </summary>
    /// <typeparam name="T_From"> Term type for leaf nodes of input </typeparam>
    /// <typeparam name="T_To"> Term type for leaf nodes of output </typeparam>
    internal class BooleanExpressionTermRewriter<T_From, T_To> : Visitor<T_From, BoolExpr<T_To>>
    {
        private readonly Func<TermExpr<T_From>, BoolExpr<T_To>> _translator;

        /// <summary>
        ///     Initialize a new translator
        /// </summary>
        /// <param name="translator"> Translator delegate; must not be null </param>
        internal BooleanExpressionTermRewriter(Func<TermExpr<T_From>, BoolExpr<T_To>> translator)
        {
            DebugCheck.NotNull(translator);
            _translator = translator;
        }

        internal override BoolExpr<T_To> VisitFalse(FalseExpr<T_From> expression)
        {
            return FalseExpr<T_To>.Value;
        }

        internal override BoolExpr<T_To> VisitTrue(TrueExpr<T_From> expression)
        {
            return TrueExpr<T_To>.Value;
        }

        internal override BoolExpr<T_To> VisitNot(NotExpr<T_From> expression)
        {
            return new NotExpr<T_To>(expression.Child.Accept(this));
        }

        internal override BoolExpr<T_To> VisitTerm(TermExpr<T_From> expression)
        {
            return _translator(expression);
        }

        internal override BoolExpr<T_To> VisitAnd(AndExpr<T_From> expression)
        {
            return new AndExpr<T_To>(VisitChildren(expression));
        }

        internal override BoolExpr<T_To> VisitOr(OrExpr<T_From> expression)
        {
            return new OrExpr<T_To>(VisitChildren(expression));
        }

        private IEnumerable<BoolExpr<T_To>> VisitChildren(TreeExpr<T_From> expression)
        {
            foreach (var child in expression.Children)
            {
                yield return child.Accept(this);
            }
        }
    }
}
