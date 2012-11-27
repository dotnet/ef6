// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    ///     Base type for Boolean expressions. Boolean expressions are immutable,
    ///     and value-comparable using Equals. Services include local simplification
    ///     and normalization to Conjunctive and Disjunctive Normal Forms.
    /// </summary>
    /// <remarks>
    ///     Comments use the following notation convention:
    ///     "A . B" means "A and B"
    ///     "A + B" means "A or B"
    ///     "!A" means "not A"
    /// </remarks>
    /// <typeparam name="T_Identifier"> The type of leaf term identifiers in this expression. </typeparam>
    internal abstract class BoolExpr<T_Identifier> : IEquatable<BoolExpr<T_Identifier>>
    {
        /// <summary>
        ///     Gets an enumeration value indicating the type of the expression node.
        /// </summary>
        internal abstract ExprType ExprType { get; }

        /// <summary>
        ///     Standard accept method invoking the appropriate method overload
        ///     in the given visitor.
        /// </summary>
        /// <typeparam name="T_Return"> T_Return is the return type for the visitor. </typeparam>
        /// <param name="visitor"> Visitor implementation. </param>
        /// <returns> Value computed for this node. </returns>
        internal abstract T_Return Accept<T_Return>(Visitor<T_Identifier, T_Return> visitor);

        /// <summary>
        ///     Invokes the Simplifier visitor on this expression tree.
        ///     Simplifications are purely local (see Simplifier class
        ///     for details).
        /// </summary>
        internal BoolExpr<T_Identifier> Simplify()
        {
            return IdentifierService<T_Identifier>.Instance.LocalSimplify(this);
        }

        /// <summary>
        ///     Expensive simplification that considers various permutations of the
        ///     expression (including Decision Diagram, DNF, and CNF translations)
        /// </summary>
        internal BoolExpr<T_Identifier> ExpensiveSimplify(out Converter<T_Identifier> converter)
        {
            var context = IdentifierService<T_Identifier>.Instance.CreateConversionContext();
            converter = new Converter<T_Identifier>(this, context);

            // Check for valid/unsat constraints
            if (converter.Vertex.IsOne())
            {
                return TrueExpr<T_Identifier>.Value;
            }
            if (converter.Vertex.IsZero())
            {
                return FalseExpr<T_Identifier>.Value;
            }

            // Pick solution from the (unmodified) expression, its CNF and its DNF
            return ChooseCandidate(this, converter.Cnf.Expr, converter.Dnf.Expr);
        }

        private static BoolExpr<T_Identifier> ChooseCandidate(params BoolExpr<T_Identifier>[] candidates)
        {
            Debug.Assert(null != candidates && 1 < candidates.Length, "must be at least one to pick");

            var resultUniqueTermCount = default(int);
            var resultTermCount = default(int);
            BoolExpr<T_Identifier> result = null;

            foreach (var candidate in candidates)
            {
                // first do basic simplification
                var simplifiedCandidate = candidate.Simplify();

                // determine "interesting" properties of the expression
                var candidateUniqueTermCount = simplifiedCandidate.GetTerms().Distinct().Count();
                var candidateTermCount = simplifiedCandidate.CountTerms();

                // see if it's better than the current result best result
                if (null == result
                    || // bootstrap
                    candidateUniqueTermCount < resultUniqueTermCount
                    || // check if the candidate improves on # of terms
                    (candidateUniqueTermCount == resultUniqueTermCount && // in case of tie, choose based on total
                     candidateTermCount < resultTermCount))
                {
                    result = simplifiedCandidate;
                    resultUniqueTermCount = candidateUniqueTermCount;
                    resultTermCount = candidateTermCount;
                }
            }

            return result;
        }

        /// <summary>
        ///     Returns all term expressions below this node.
        /// </summary>
        internal List<TermExpr<T_Identifier>> GetTerms()
        {
            return LeafVisitor<T_Identifier>.GetTerms(this);
        }

        /// <summary>
        ///     Counts terms in this expression.
        /// </summary>
        internal int CountTerms()
        {
            return TermCounter<T_Identifier>.CountTerms(this);
        }

        /// <summary>
        ///     Implicit cast from a value of type T to a TermExpr where
        ///     TermExpr.Value is set to the given value.
        /// </summary>
        /// <param name="value"> Value to wrap in term expression </param>
        /// <returns> Term expression </returns>
        public static implicit operator BoolExpr<T_Identifier>(T_Identifier value)
        {
            return new TermExpr<T_Identifier>(value);
        }

        /// <summary>
        ///     Creates the negation of the current element.
        /// </summary>
        internal virtual BoolExpr<T_Identifier> MakeNegated()
        {
            return new NotExpr<T_Identifier>(this);
        }

        public override string ToString()
        {
            return ExprType.ToString();
        }

        public bool Equals(BoolExpr<T_Identifier> other)
        {
            return null != other && ExprType == other.ExprType &&
                   EquivalentTypeEquals(other);
        }

        protected abstract bool EquivalentTypeEquals(BoolExpr<T_Identifier> other);
    }
}
