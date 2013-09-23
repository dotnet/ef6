// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    // <summary>
    // Abstract base class for normal form sentences (CNF and DNF)
    // </summary>
    // <typeparam name="T_Identifier"> Type of expression leaf term identifiers. </typeparam>
    // <typeparam name="T_Clause"> Type of clauses in the sentence. </typeparam>
    internal abstract class Sentence<T_Identifier, T_Clause> : NormalFormNode<T_Identifier>
        where T_Clause : Clause<T_Identifier>, IEquatable<T_Clause>
    {
        private readonly Set<T_Clause> _clauses;

        // <summary>
        // Initialize a sentence given the appropriate sentence clauses. Produces
        // an equivalent expression by composing the clause expressions using
        // the given tree type.
        // </summary>
        // <param name="clauses"> Sentence clauses </param>
        // <param name="treeType"> Tree type for sentence (and generated expression) </param>
        protected Sentence(Set<T_Clause> clauses, ExprType treeType)
            : base(ConvertClausesToExpr(clauses, treeType))
        {
            _clauses = clauses.AsReadOnly();
        }

        // Produces an expression equivalent to the given clauses by composing the clause
        // expressions using the given tree type.
        private static BoolExpr<T_Identifier> ConvertClausesToExpr(Set<T_Clause> clauses, ExprType treeType)
        {
            var isAnd = ExprType.And == treeType;
            Debug.Assert(isAnd || ExprType.Or == treeType);

            var clauseExpressions =
                clauses.Select(ExprSelector);

            if (isAnd)
            {
                return new AndExpr<T_Identifier>(clauseExpressions);
            }
            else
            {
                return new OrExpr<T_Identifier>(clauseExpressions);
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("Sentence{");
            builder.Append(_clauses);
            return builder.Append("}").ToString();
        }
    }
}
