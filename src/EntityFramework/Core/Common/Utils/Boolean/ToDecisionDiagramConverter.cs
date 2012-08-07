// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    ///     Converts a BoolExpr to a Vertex within a solver.
    /// </summary>
    internal class ToDecisionDiagramConverter<T_Identifier> : Visitor<T_Identifier, Vertex>
    {
        private readonly ConversionContext<T_Identifier> _context;

        private ToDecisionDiagramConverter(ConversionContext<T_Identifier> context)
        {
            Debug.Assert(null != context, "must provide a context");
            _context = context;
        }

        internal static Vertex TranslateToRobdd(BoolExpr<T_Identifier> expr, ConversionContext<T_Identifier> context)
        {
            Debug.Assert(null != expr, "must provide an expression");
            var converter =
                new ToDecisionDiagramConverter<T_Identifier>(context);
            return expr.Accept(converter);
        }

        internal override Vertex VisitTrue(TrueExpr<T_Identifier> expression)
        {
            return Vertex.One;
        }

        internal override Vertex VisitFalse(FalseExpr<T_Identifier> expression)
        {
            return Vertex.Zero;
        }

        internal override Vertex VisitTerm(TermExpr<T_Identifier> expression)
        {
            return _context.TranslateTermToVertex(expression);
        }

        internal override Vertex VisitNot(NotExpr<T_Identifier> expression)
        {
            return _context.Solver.Not(expression.Child.Accept(this));
        }

        internal override Vertex VisitAnd(AndExpr<T_Identifier> expression)
        {
            return _context.Solver.And(expression.Children.Select(child => child.Accept(this)));
        }

        internal override Vertex VisitOr(OrExpr<T_Identifier> expression)
        {
            return _context.Solver.Or(expression.Children.Select(child => child.Accept(this)));
        }
    }
}
