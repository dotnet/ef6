// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    ///     Represents the Seached Case Expression - CASE WHEN THEN [ELSE] END.
    /// </summary>
    internal sealed class CaseExpr : Node
    {
        private readonly NodeList<WhenThenExpr> _whenThenExpr;
        private readonly Node _elseExpr;

        /// <summary>
        ///     Initializes case expression without else sub-expression.
        /// </summary>
        /// <param name="whenThenExpr"> whenThen expression list </param>
        internal CaseExpr(NodeList<WhenThenExpr> whenThenExpr)
            : this(whenThenExpr, null)
        {
        }

        /// <summary>
        ///     Initializes case expression with else sub-expression.
        /// </summary>
        /// <param name="whenThenExpr"> whenThen expression list </param>
        /// <param name="elseExpr"> else expression </param>
        internal CaseExpr(NodeList<WhenThenExpr> whenThenExpr, Node elseExpr)
        {
            _whenThenExpr = whenThenExpr;
            _elseExpr = elseExpr;
        }

        /// <summary>
        ///     Returns the list of WhenThen expressions.
        /// </summary>
        internal NodeList<WhenThenExpr> WhenThenExprList
        {
            get { return _whenThenExpr; }
        }

        /// <summary>
        ///     Returns the optional Else expression.
        /// </summary>
        internal Node ElseExpr
        {
            get { return _elseExpr; }
        }
    }
}
