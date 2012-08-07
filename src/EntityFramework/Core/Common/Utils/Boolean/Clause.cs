// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///     Base class for clauses, which are (constrained) combinations of literals.
    /// </summary>
    /// <typeparam name="T_Identifier"> Type of normal form literal. </typeparam>
    internal abstract class Clause<T_Identifier> : NormalFormNode<T_Identifier>
    {
        private readonly Set<Literal<T_Identifier>> _literals;
        private readonly int _hashCode;

        /// <summary>
        ///     Initialize a new clause.
        /// </summary>
        /// <param name="literals"> Literals contained in the clause. </param>
        /// <param name="treeType"> Type of expression tree to produce from literals. </param>
        protected Clause(Set<Literal<T_Identifier>> literals, ExprType treeType)
            : base(ConvertLiteralsToExpr(literals, treeType))
        {
            _literals = literals.AsReadOnly();
            _hashCode = _literals.GetElementsHashCode();
        }

        /// <summary>
        ///     Gets the literals contained in this clause.
        /// </summary>
        internal Set<Literal<T_Identifier>> Literals
        {
            get { return _literals; }
        }

        // Given a collection of literals and a tree type, returns an expression of the given type.
        private static BoolExpr<T_Identifier> ConvertLiteralsToExpr(Set<Literal<T_Identifier>> literals, ExprType treeType)
        {
            var isAnd = ExprType.And == treeType;
            Debug.Assert(isAnd || ExprType.Or == treeType);

            var literalExpressions = literals.Select(
                ConvertLiteralToExpression);

            if (isAnd)
            {
                return new AndExpr<T_Identifier>(literalExpressions);
            }
            else
            {
                return new OrExpr<T_Identifier>(literalExpressions);
            }
        }

        // Given a literal, returns its logical equivalent expression.
        private static BoolExpr<T_Identifier> ConvertLiteralToExpression(Literal<T_Identifier> literal)
        {
            return literal.Expr;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("Clause{");
            builder.Append(_literals);
            return builder.Append("}").ToString();
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            Debug.Fail("call typed Equals");
            return base.Equals(obj);
        }
    }
}
