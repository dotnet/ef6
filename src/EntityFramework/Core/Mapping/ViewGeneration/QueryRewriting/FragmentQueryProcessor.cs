// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Common.Utils.Boolean;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using BoolDomainConstraint = System.Data.Entity.Core.Common.Utils.Boolean.DomainConstraint<Structures.BoolLiteral, Structures.Constant>;

    internal class FragmentQueryProcessor : TileQueryProcessor<FragmentQuery>
    {
        private readonly FragmentQueryKBChaseSupport _kb;

        public FragmentQueryProcessor(FragmentQueryKBChaseSupport kb)
        {
            _kb = kb;
        }

        internal static FragmentQueryProcessor Merge(FragmentQueryProcessor qp1, FragmentQueryProcessor qp2)
        {
            var mergedKB = new FragmentQueryKBChaseSupport();
            mergedKB.AddKnowledgeBase(qp1.KnowledgeBase);
            mergedKB.AddKnowledgeBase(qp2.KnowledgeBase);
            return new FragmentQueryProcessor(mergedKB);
        }

        internal FragmentQueryKB KnowledgeBase
        {
            get { return _kb; }
        }

        // resulting query contains an intersection of attributes
        [SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCode",
            Justification = "Based on Bug VSTS Pioneer #433188: IsVisibleOutsideAssembly is wrong on generic instantiations.")]
        internal override FragmentQuery Union(FragmentQuery q1, FragmentQuery q2)
        {
            var attributes = new HashSet<MemberPath>(q1.Attributes);
            attributes.IntersectWith(q2.Attributes);

            var condition = BoolExpression.CreateOr(q1.Condition, q2.Condition);

            return FragmentQuery.Create(attributes, condition);
        }

        internal bool IsDisjointFrom(FragmentQuery q1, FragmentQuery q2)
        {
            return !IsSatisfiable(Intersect(q1, q2));
        }

        internal bool IsContainedIn(FragmentQuery q1, FragmentQuery q2)
        {
            return !IsSatisfiable(Difference(q1, q2));
        }

        internal bool IsEquivalentTo(FragmentQuery q1, FragmentQuery q2)
        {
            return IsContainedIn(q1, q2) && IsContainedIn(q2, q1);
        }

        [SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCode",
            Justification = "Based on Bug VSTS Pioneer #433188: IsVisibleOutsideAssembly is wrong on generic instantiations.")]
        internal override FragmentQuery Intersect(FragmentQuery q1, FragmentQuery q2)
        {
            var attributes = new HashSet<MemberPath>(q1.Attributes);
            attributes.IntersectWith(q2.Attributes);

            var condition = BoolExpression.CreateAnd(q1.Condition, q2.Condition);

            return FragmentQuery.Create(attributes, condition);
        }

        internal override FragmentQuery Difference(FragmentQuery qA, FragmentQuery qB)
        {
            return FragmentQuery.Create(qA.Attributes, BoolExpression.CreateAndNot(qA.Condition, qB.Condition));
        }

        internal override bool IsSatisfiable(FragmentQuery query)
        {
            return IsSatisfiable(query.Condition);
        }

        private bool IsSatisfiable(BoolExpression condition)
        {
            return _kb.IsSatisfiable(condition.Tree);
        }

        // creates "derived" views that may be helpful for answering the query
        // for example, view = SELECT ID WHERE B=2, query = SELECT ID,B WHERE B=2
        // Created derived view: SELECT ID,B WHERE B=2 by adding the attribute whose value is determined by the where clause to projected list
        internal override FragmentQuery CreateDerivedViewBySelectingConstantAttributes(FragmentQuery view)
        {
            var newProjectedAttributes = new HashSet<MemberPath>();
            // collect all variables from the view
            var variables = view.Condition.Variables;
            foreach (var var in variables)
            {
                var variableCondition = var.Identifier as MemberRestriction;
                if (variableCondition != null)
                {
                    // Is this attribute not already projected?
                    var conditionMember = variableCondition.RestrictedMemberSlot.MemberPath;
                    // Iterating through the variable domain var.Domain could be wasteful
                    // Instead, consider the actual condition values on the variable. Usually, they don't get repeated (if not, we could cache and check)
                    var conditionValues = variableCondition.Domain;

                    if ((false == view.Attributes.Contains(conditionMember))
                        && !(conditionValues.AllPossibleValues.Any(it => it.HasNotNull())))
                        //Don't add member to the projected list if the condition involves a 
                    {
                        foreach (var value in conditionValues.Values)
                        {
                            // construct constraint: X = value
                            var constraint = new DomainConstraint<BoolLiteral, Constant>(
                                var,
                                new Set<Constant>(new[] { value }, Constant.EqualityComparer));
                            // is this constraint implied by the where clause?
                            var exclusion = view.Condition.Create(
                                new AndExpr<DomainConstraint<BoolLiteral, Constant>>(
                                    view.Condition.Tree,
                                    new NotExpr<DomainConstraint<BoolLiteral, Constant>>(
                                        new TermExpr<DomainConstraint<BoolLiteral, Constant>>(constraint))));
                            var isImplied = false == IsSatisfiable(exclusion);
                            if (isImplied)
                            {
                                // add this variable to the projection, if it is used in the query
                                newProjectedAttributes.Add(conditionMember);
                            }
                        }
                    }
                }
            }
            if (newProjectedAttributes.Count > 0)
            {
                newProjectedAttributes.UnionWith(view.Attributes);
                var derivedView = new FragmentQuery(
                    String.Format(CultureInfo.InvariantCulture, "project({0})", view.Description), view.FromVariable,
                    newProjectedAttributes, view.Condition);
                return derivedView;
            }
            return null;
        }

        public override string ToString()
        {
            return _kb.ToString();
        }

        private class AttributeSetComparator : IEqualityComparer<HashSet<MemberPath>>
        {
            [SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCode",
                Justification = "Based on Bug VSTS Pioneer #433188: IsVisibleOutsideAssembly is wrong on generic instantiations.")]
            public bool Equals(HashSet<MemberPath> x, HashSet<MemberPath> y)
            {
                return x.SetEquals(y);
            }

            public int GetHashCode(HashSet<MemberPath> attrs)
            {
                var hashCode = 123;
                foreach (var attr in attrs)
                {
                    hashCode += MemberPath.EqualityComparer.GetHashCode(attr) * 7;
                }
                return hashCode;
            }
        }
    }
}
