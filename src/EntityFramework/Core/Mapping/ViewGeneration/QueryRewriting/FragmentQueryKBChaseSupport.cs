// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Common.Utils.Boolean;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
    using System.Diagnostics;
    using System.Linq;
    using DomainBoolExpr =
            System.Data.Entity.Core.Common.Utils.Boolean.BoolExpr
                <Common.Utils.Boolean.DomainConstraint<Structures.BoolLiteral, Structures.Constant>>;
    using DomainConstraint =
            System.Data.Entity.Core.Common.Utils.Boolean.DomainConstraint<Structures.BoolLiteral, Structures.Constant>;
    using DomainTermExpr =
            System.Data.Entity.Core.Common.Utils.Boolean.TermExpr
                <Common.Utils.Boolean.DomainConstraint<Structures.BoolLiteral, Structures.Constant>>;

    /// <summary>
    /// Satisfiability test optimization.
    /// This class extends FragmentQueryKB by adding the so-called chase functionality:
    /// given an expression, the chase incorporates in this expression all the consequences derivable
    /// from the knowledge base. The knowledge base is not needed for the satisfiability test after such a procedure.
    /// This leads to better performance in many cases.
    /// </summary>
    internal class FragmentQueryKBChaseSupport : FragmentQueryKB
    {
        // Index of facts derivable from conditions, maintained via the CacheImplications method
        private readonly Dictionary<DomainTermExpr, DomainBoolExpr> _implications =
            new Dictionary<DomainTermExpr, DomainBoolExpr>();

        private readonly AtomicConditionRuleChase _chase;
        private Set<DomainBoolExpr> _residualFacts = new Set<DomainBoolExpr>();
        private int _kbSize;

        // Residue is not valid, it must be chased first this happens in PrepareResidue()
        private int _residueSize = -1;

        internal FragmentQueryKBChaseSupport()
        {
            _chase = new AtomicConditionRuleChase(this);
        }

        internal override void AddFact(DomainBoolExpr fact)
        {
            base.AddFact(fact);

            _kbSize += fact.CountTerms();

            var implication = fact as Implication;
            var equivalence = fact as Equivalence;
            if (implication != null)
            {
                CacheImplication(implication.Condition, implication.Implies);
            }
            else if (equivalence != null)
            {
                CacheImplication(equivalence.Left, equivalence.Right);
                CacheImplication(equivalence.Right, equivalence.Left);
            }
            else
            {
                CacheResidualFact(fact);
            }
        }

        /// <summary>
        /// Returns KB rules which cannot be used for chasing.
        /// </summary>
        internal DomainBoolExpr Residue
        {
            get { return new AndExpr<DomainConstraint>(ResidueInternal); }
        }

        private IEnumerable<DomainBoolExpr> ResidueInternal
        {
            get
            {
                if (_residueSize < 0
                    && _residualFacts.Count > 0)
                {
                    PrepareResidue();
                }
                return _residualFacts;
            }
        }

        private int ResidueSize
        {
            get
            {
                if (_residueSize < 0)
                {
                    PrepareResidue();
                }
                return _residueSize;
            }
        }

        /// <summary>
        /// Retrieves all implications directly derivable from the atomic expression.
        /// </summary>
        /// <param name="expression">
        /// Atomic expression to be extended with facts derivable from the knowledge base.
        /// </param>
        internal DomainBoolExpr Chase(DomainTermExpr expression)
        {
            DomainBoolExpr implication;
            _implications.TryGetValue(expression, out implication);

            return new AndExpr<DomainConstraint>(expression, implication ?? TrueExpr<DomainConstraint>.Value);
        }

        /// <summary>
        /// Checks if the given expression is satisfiable in conjunction with this knowledge base.
        /// </summary>
        /// <param name="expression">Expression to be tested for satisfiability.</param>
        internal bool IsSatisfiable(DomainBoolExpr expression)
        {
            var context = IdentifierService<DomainConstraint>.Instance.CreateConversionContext();
            var converter = new Converter<DomainConstraint>(expression, context);

            if (converter.Vertex.IsZero())
            {
                return false;
            }

            if (KbExpression.ExprType == ExprType.True)
            {
                return true;
            }

            var noChaseSize = expression.CountTerms() + _kbSize;
            var exprDnf = converter.Dnf.Expr;

            var optimalSplitForm = Normalizer.EstimateNnfAndSplitTermCount(exprDnf) > Normalizer.EstimateNnfAndSplitTermCount(expression)
                                       ? expression
                                       : exprDnf;

            var chaseExpr = _chase.Chase(Normalizer.ToNnfAndSplitRange(optimalSplitForm));

            var fullExpression = chaseExpr.CountTerms() + ResidueSize > noChaseSize
                                     ? new AndExpr<DomainConstraint>(KbExpression, expression)
                                     : new AndExpr<DomainConstraint>(
                                           new List<DomainBoolExpr>(ResidueInternal)
                                               {
                                                   chaseExpr
                                               });

            return !new Converter<DomainConstraint>(fullExpression, context).Vertex.IsZero();
        }

        /// <summary>
        /// Retrieves all implications directly derivable from the expression.
        /// </summary>
        /// <param name="expression">
        /// Expression to be extended with facts derivable from the knowledge base.
        /// </param>
        internal DomainBoolExpr Chase(DomainBoolExpr expression)
        {
            return _implications.Count == 0 ? expression : _chase.Chase(Normalizer.ToNnfAndSplitRange(expression));
        }

        /// <summary>
        /// Maintains a list of all implications derivable from the condition.
        /// Implications are stored in the _implications dictionary
        /// </summary>
        /// <param name="condition"> Condition </param>
        /// <param name="implies"> Entailed expression </param>
        private void CacheImplication(DomainBoolExpr condition, DomainBoolExpr implies)
        {
            var conditionDnf = Normalizer.ToDnf(condition, false);
            var impliesNnf = Normalizer.ToNnfAndSplitRange(implies);

            switch (conditionDnf.ExprType)
            {
                case ExprType.Or:
                    foreach (var child in ((OrExpr<DomainConstraint>)conditionDnf).Children)
                    {
                        if (child.ExprType != ExprType.Term)
                        {
                            CacheResidualFact(
                                new OrExpr<DomainConstraint>(new NotExpr<DomainConstraint>(child), implies));
                        }
                        else
                        {
                            CacheNormalizedImplication((TermExpr<DomainConstraint>)child, impliesNnf);
                        }
                    }
                    break;
                case ExprType.Term:
                    CacheNormalizedImplication((TermExpr<DomainConstraint>)conditionDnf, impliesNnf);
                    break;
                default:
                    CacheResidualFact(
                        new OrExpr<DomainConstraint>(new NotExpr<DomainConstraint>(condition), implies));
                    break;
            }
        }

        // Requires condition to be atomic
        private void CacheNormalizedImplication(
            DomainTermExpr condition, DomainBoolExpr implies)
        {
            // Check that we do not have a rule with an inconsistent condition yet
            // such rules cannot be accommodated: we require rule premises to be pair wise 
            // variable disjoint (note that the rules with coinciding conditions are merged)
            // rules with inconsistent conditions may make the chase incomplete:
            // For instance, consider the KB {c->a, b->c, !b->a} and the condition "!a".
            // chase(!a, KB) = !a, but !a ^ KB is unsatisfiable.

            foreach (var premise in _implications.Keys)
            {
                if (premise.Identifier.Variable.Equals(condition.Identifier.Variable)
                    &&
                    !premise.Identifier.Range.SetEquals(condition.Identifier.Range))
                {
                    CacheResidualFact(new OrExpr<DomainConstraint>(new NotExpr<DomainConstraint>(condition), implies));
                    return;
                }
            }

            // We first chase the implication with all the existing facts, and then 
            // chase implications of all existing rules, and all residual facts with the 
            // resulting enhanced rule

            var dnfImpl = new Converter<DomainConstraint>(
                Chase(implies),
                IdentifierService<DomainConstraint>.Instance.CreateConversionContext()).Dnf.Expr;

            // Now chase all our knowledge with the rule "condition => dnfImpl"

            // Construct a fake knowledge base for this sake
            var kb = new FragmentQueryKBChaseSupport();
            kb._implications[condition] = dnfImpl;

            var newKey = true;

            foreach (var key in new Set<TermExpr<DomainConstraint>>(_implications.Keys))
            {
                var chasedRuleImpl = kb.Chase(_implications[key]);

                if (key.Equals(condition))
                {
                    newKey = false;
                    chasedRuleImpl = new AndExpr<DomainConstraint>(chasedRuleImpl, dnfImpl);
                }

                // Simplify using the solver
                _implications[key] = new Converter<DomainConstraint>(
                    chasedRuleImpl,
                    IdentifierService<DomainConstraint>.Instance.CreateConversionContext()).Dnf.Expr;
            }

            if (newKey)
            {
                _implications[condition] = dnfImpl;
            }

            // Invalidate residue
            _residueSize = -1;
        }

        // Add un-useful for chasing fact to the residue
        private void CacheResidualFact(DomainBoolExpr fact)
        {
            _residualFacts.Add(fact);
            _residueSize = -1;
        }

        // Chase each residual fact with the atomic-condition rules
        private void PrepareResidue()
        {
            var residueSize = 0;
            if (_implications.Count > 0
                && _residualFacts.Count > 0)
            {
                var newResidualFacts = new Set<DomainBoolExpr>();
                foreach (var fact in _residualFacts)
                {
                    // Simplify using the solver
                    var dnfFact = new Converter<DomainConstraint>(
                        Chase(fact),
                        IdentifierService<DomainConstraint>.Instance.CreateConversionContext()).Dnf.Expr;

                    newResidualFacts.Add(dnfFact);
                    residueSize += dnfFact.CountTerms();
                    _residueSize = residueSize;
                }
                _residualFacts = newResidualFacts;
            }
            _residueSize = residueSize;
        }

        private static class Normalizer
        {
            internal static DomainBoolExpr ToNnfAndSplitRange(DomainBoolExpr expr)
            {
                return expr.Accept(NonNegatedTreeVisitor.Instance);
            }

            internal static int EstimateNnfAndSplitTermCount(DomainBoolExpr expr)
            {
                return expr.Accept(NonNegatedNnfSplitCounter.Instance);
            }

            internal static DomainBoolExpr ToDnf(DomainBoolExpr expr, bool isNnf)
            {
                if (!isNnf)
                {
                    expr = ToNnfAndSplitRange(expr);
                }

                return expr.Accept(DnfTreeVisitor.Instance);
            }

            private class NonNegatedTreeVisitor : BasicVisitor<DomainConstraint>
            {
                internal static readonly NonNegatedTreeVisitor Instance = new NonNegatedTreeVisitor();

                private NonNegatedTreeVisitor()
                {
                }

                internal override DomainBoolExpr VisitNot(NotExpr<DomainConstraint> expr)
                {
                    return expr.Child.Accept(NegatedTreeVisitor.Instance);
                }

                internal override DomainBoolExpr VisitTerm(TermExpr<DomainConstraint> expression)
                {
                    switch (expression.Identifier.Range.Count)
                    {
                        case 0:
                            return FalseExpr<DomainConstraint>.Value;
                        case 1:
                            return expression;
                    }

                    var split = new List<DomainBoolExpr>();
                    var variable = expression.Identifier.Variable;

                    foreach (var element in expression.Identifier.Range)
                    {
                        split.Add(new DomainConstraint(variable, new Set<Constant>(new[] { element }, Constant.EqualityComparer)));
                    }

                    return new OrExpr<DomainConstraint>(split);
                }
            }

            private class NegatedTreeVisitor : Visitor<DomainConstraint, BoolExpr<DomainConstraint>>
            {
                internal static readonly NegatedTreeVisitor Instance = new NegatedTreeVisitor();

                private NegatedTreeVisitor()
                {
                }

                internal override DomainBoolExpr VisitTrue(TrueExpr<DomainConstraint> expression)
                {
                    return FalseExpr<DomainConstraint>.Value;
                }

                internal override DomainBoolExpr VisitFalse(FalseExpr<DomainConstraint> expression)
                {
                    return TrueExpr<DomainConstraint>.Value;
                }

                internal override DomainBoolExpr VisitNot(NotExpr<DomainConstraint> expression)
                {
                    return expression.Child.Accept(NonNegatedTreeVisitor.Instance);
                }

                internal override DomainBoolExpr VisitAnd(AndExpr<DomainConstraint> expression)
                {
                    return new OrExpr<DomainConstraint>(expression.Children.Select(child => child.Accept(this)));
                }

                internal override DomainBoolExpr VisitOr(OrExpr<DomainConstraint> expression)
                {
                    return new AndExpr<DomainConstraint>(expression.Children.Select(child => child.Accept(this)));
                }

                internal override DomainBoolExpr VisitTerm(TermExpr<DomainConstraint> expression)
                {
                    var invertedConstraint = expression.Identifier.InvertDomainConstraint();
                    if (invertedConstraint.Range.Count == 0)
                    {
                        return FalseExpr<DomainConstraint>.Value;
                    }

                    var split = new List<DomainBoolExpr>();
                    var variable = invertedConstraint.Variable;

                    foreach (var element in invertedConstraint.Range)
                    {
                        split.Add(new DomainConstraint(variable, new Set<Constant>(new[] { element }, Constant.EqualityComparer)));
                    }

                    return new OrExpr<DomainConstraint>(split);
                }
            }

            private class NonNegatedNnfSplitCounter : TermCounter<DomainConstraint>
            {
                internal static readonly NonNegatedNnfSplitCounter Instance = new NonNegatedNnfSplitCounter();

                private NonNegatedNnfSplitCounter()
                {
                }

                internal override int VisitNot(NotExpr<DomainConstraint> expr)
                {
                    return expr.Child.Accept(NegatedNnfSplitCountEstimator.Instance);
                }

                internal override int VisitTerm(TermExpr<DomainConstraint> expression)
                {
                    return expression.Identifier.Range.Count;
                }
            }

            private class NegatedNnfSplitCountEstimator : TermCounter<DomainConstraint>
            {
                internal static readonly NegatedNnfSplitCountEstimator Instance = new NegatedNnfSplitCountEstimator();

                private NegatedNnfSplitCountEstimator()
                {
                }

                internal override int VisitNot(NotExpr<DomainConstraint> expression)
                {
                    return expression.Child.Accept(NonNegatedNnfSplitCounter.Instance);
                }

                internal override int VisitTerm(TermExpr<DomainConstraint> expression)
                {
                    //this might be imprecise (precise would be count the elements in the set difference),
                    //but this class is only needed for estimating the count 
                    return expression.Identifier.Variable.Domain.Count - expression.Identifier.Range.Count;
                }
            }

            private class DnfTreeVisitor : BasicVisitor<DomainConstraint>
            {
                internal static readonly DnfTreeVisitor Instance = new DnfTreeVisitor();

                private DnfTreeVisitor()
                {
                }

                internal override DomainBoolExpr VisitNot(NotExpr<DomainConstraint> expression)
                {
                    return expression;
                }

                internal override DomainBoolExpr VisitAnd(AndExpr<DomainConstraint> expression)
                {
                    var recurse = base.VisitAnd(expression);
                    var recurseTree = recurse as TreeExpr<DomainConstraint>;

                    if (recurseTree == null)
                    {
                        return recurse;
                    }

                    var conjunction = new Set<DomainBoolExpr>();
                    var buckets = new Set<Set<DomainBoolExpr>>();

                    foreach (var child in recurseTree.Children)
                    {
                        var childOr = child as OrExpr<DomainConstraint>;
                        if (childOr != null)
                        {
                            buckets.Add(new Set<DomainBoolExpr>(childOr.Children));
                        }
                        else
                        {
                            conjunction.Add(child);
                        }
                    }

                    buckets.Add(new Set<DomainBoolExpr>(new DomainBoolExpr[] { new AndExpr<DomainConstraint>(conjunction) }));

                    // Get a cartesian product of buckets using LINQ, thanks Eric Lippert 
                    // http://blogs.msdn.com/b/ericlippert/archive/2010/06/28/computing-a-cartesian-product-with-linq.aspx

                    IEnumerable<IEnumerable<DomainBoolExpr>> emptyProduct = new[] { Enumerable.Empty<DomainBoolExpr>() };
                    var product =
                        buckets.Aggregate(
                            emptyProduct,
                            (accumulator, bucket) =>
                            from accseq in accumulator
                            from item in bucket
                            select accseq.Concat(new[] { item }));

                    var clauses = new List<DomainBoolExpr>();

                    foreach (var tuple in product)
                    {
                        clauses.Add(new AndExpr<DomainConstraint>(tuple));
                    }

                    return new OrExpr<DomainConstraint>(clauses);
                }
            }
        }

        private class AtomicConditionRuleChase
        {
            private readonly NonNegatedDomainConstraintTreeVisitor _visitor;

            internal AtomicConditionRuleChase(FragmentQueryKBChaseSupport kb)
            {
                _visitor = new NonNegatedDomainConstraintTreeVisitor(kb);
            }

            internal DomainBoolExpr Chase(DomainBoolExpr expression)
            {
                return expression.Accept(_visitor);
            }

            private class NonNegatedDomainConstraintTreeVisitor : BasicVisitor<DomainConstraint>
            {
                private readonly FragmentQueryKBChaseSupport _kb;

                internal NonNegatedDomainConstraintTreeVisitor(FragmentQueryKBChaseSupport kb)
                {
                    _kb = kb;
                }

                internal override DomainBoolExpr VisitTerm(DomainTermExpr expression)
                {
                    return _kb.Chase(expression);
                }

                internal override DomainBoolExpr VisitNot(NotExpr<DomainConstraint> expression)
                {
                    Debug.Assert(false, "Negations should not happen at this point");

                    return base.VisitNot(expression);
                }
            }
        }
    }
}
