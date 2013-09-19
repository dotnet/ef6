// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;

    internal class FragmentQuery : ITileQuery
    {
        private readonly BoolExpression m_fromVariable; // optional
        private readonly string m_label; // optional

        private readonly HashSet<MemberPath> m_attributes;
        private readonly BoolExpression m_condition;

        public HashSet<MemberPath> Attributes
        {
            get { return m_attributes; }
        }

        public BoolExpression Condition
        {
            get { return m_condition; }
        }

        public static FragmentQuery Create(BoolExpression fromVariable, CellQuery cellQuery)
        {
            var whereClause = cellQuery.WhereClause;
            whereClause = whereClause.MakeCopy();
            whereClause.ExpensiveSimplify();
            return new FragmentQuery(null /*label*/, fromVariable, new HashSet<MemberPath>(cellQuery.GetProjectedMembers()), whereClause);
        }

        public static FragmentQuery Create(string label, RoleBoolean roleBoolean, CellQuery cellQuery)
        {
            var whereClause = cellQuery.WhereClause.Create(roleBoolean);
            whereClause = BoolExpression.CreateAnd(whereClause, cellQuery.WhereClause);
            //return new FragmentQuery(label, null /* fromVariable */, new HashSet<MemberPath>(cellQuery.GetProjectedMembers()), whereClause);
            // don't need any attributes 
            whereClause = whereClause.MakeCopy();
            whereClause.ExpensiveSimplify();
            return new FragmentQuery(label, null /* fromVariable */, new HashSet<MemberPath>(), whereClause);
        }

        public static FragmentQuery Create(IEnumerable<MemberPath> attrs, BoolExpression whereClause)
        {
            return new FragmentQuery(null /* no name */, null /* no fromVariable*/, attrs, whereClause);
        }

        public static FragmentQuery Create(BoolExpression whereClause)
        {
            return new FragmentQuery(null /* no name */, null /* no fromVariable*/, new MemberPath[] { }, whereClause);
        }

        internal FragmentQuery(string label, BoolExpression fromVariable, IEnumerable<MemberPath> attrs, BoolExpression condition)
        {
            m_label = label;
            m_fromVariable = fromVariable;
            m_condition = condition;
            m_attributes = new HashSet<MemberPath>(attrs);
        }

        public BoolExpression FromVariable
        {
            get { return m_fromVariable; }
        }

        public string Description
        {
            get
            {
                var label = m_label;
                if (label == null
                    && m_fromVariable != null)
                {
                    label = m_fromVariable.ToString();
                }
                return label;
            }
        }

        public override string ToString()
        {
            // attributes
            var b = new StringBuilder();
            foreach (var value in Attributes)
            {
                if (b.Length > 0)
                {
                    b.Append(',');
                }
                b.Append(value);
            }

            if (Description != null
                && Description != b.ToString())
            {
                return String.Format(CultureInfo.InvariantCulture, "{0}: [{1} where {2}]", Description, b, Condition);
            }
            else
            {
                return String.Format(CultureInfo.InvariantCulture, "[{0} where {1}]", b, Condition);
            }
        }

        // creates a condition member=value
        internal static BoolExpression CreateMemberCondition(MemberPath path, Constant domainValue, MemberDomainMap domainMap)
        {
            if (domainValue is TypeConstant)
            {
                return BoolExpression.CreateLiteral(
                    new TypeRestriction(
                        new MemberProjectedSlot(path),
                        new Domain(domainValue, domainMap.GetDomain(path))), domainMap);
            }
            else
            {
                return BoolExpression.CreateLiteral(
                    new ScalarRestriction(
                        new MemberProjectedSlot(path),
                        new Domain(domainValue, domainMap.GetDomain(path))), domainMap);
            }
        }

        internal static IEqualityComparer<FragmentQuery> GetEqualityComparer(FragmentQueryProcessor qp)
        {
            return new FragmentQueryEqualityComparer(qp);
        }

        // Two queries are "equal" if they project the same set of attributes
        // and their WHERE clauses are equivalent
        private class FragmentQueryEqualityComparer : IEqualityComparer<FragmentQuery>
        {
            private readonly FragmentQueryProcessor _qp;

            internal FragmentQueryEqualityComparer(FragmentQueryProcessor qp)
            {
                _qp = qp;
            }

            [SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCode",
                Justification = "Based on Bug VSTS Pioneer #433188: IsVisibleOutsideAssembly is wrong on generic instantiations.")]
            public bool Equals(FragmentQuery x, FragmentQuery y)
            {
                if (!x.Attributes.SetEquals(y.Attributes))
                {
                    return false;
                }
                return _qp.IsEquivalentTo(x, y);
            }

            // Hashing a bit naive: it exploits syntactic properties,
            // i.e., some semantically equivalent queries may produce different hash codes
            // But that's fine for usage scenarios in QueryRewriter.cs 
            public int GetHashCode(FragmentQuery q)
            {
                var attrHashCode = 0;
                foreach (var member in q.Attributes)
                {
                    attrHashCode ^= MemberPath.EqualityComparer.GetHashCode(member);
                }
                var varHashCode = 0;
                var constHashCode = 0;
                foreach (var oneOf in q.Condition.MemberRestrictions)
                {
                    varHashCode ^= MemberPath.EqualityComparer.GetHashCode(oneOf.RestrictedMemberSlot.MemberPath);
                    foreach (var constant in oneOf.Domain.Values)
                    {
                        constHashCode ^= Constant.EqualityComparer.GetHashCode(constant);
                    }
                }
                return attrHashCode * 13 + varHashCode * 7 + constHashCode;
            }
        }
    }
}
