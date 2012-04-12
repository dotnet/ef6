namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using DomainBoolExpr =
        System.Data.Entity.Core.Common.Utils.Boolean.BoolExpr<Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>>;
    using DomainTermExpr =
        System.Data.Entity.Core.Common.Utils.Boolean.TermExpr<Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>>;

    /// <summary>
    /// An abstract class that denotes the boolean expression: "var in values".
    /// An object of this type can be complete or incomplete. 
    /// An incomplete object is one whose domain was not created with all possible values.
    /// Incomplete objects have a limited set of methods that can be called.
    /// </summary>
    internal abstract class MemberRestriction : BoolLiteral
    {
        #region Constructors

        /// <summary>
        /// Creates an incomplete member restriction with the meaning "<paramref name="slot"/> = <paramref name="value"/>".
        /// "Partial" means that the <see cref="Domain"/> in this restriction is partial - hence the operations on the restriction are limited.
        /// </summary>
        protected MemberRestriction(MemberProjectedSlot slot, Constant value)
            : this(slot, new[] { value })
        {
        }

        /// <summary>
        /// Creates an incomplete member restriction with the meaning "<paramref name="slot"/> in <paramref name="values"/>".
        /// </summary>
        protected MemberRestriction(MemberProjectedSlot slot, IEnumerable<Constant> values)
        {
            m_restrictedMemberSlot = slot;
            m_domain = new Domain(values, values);
        }

        /// <summary>
        /// Creates a complete member restriction with the meaning "<paramref name="slot"/> in <paramref name="domain"/>".
        /// </summary>
        protected MemberRestriction(MemberProjectedSlot slot, Domain domain)
        {
            m_restrictedMemberSlot = slot;
            m_domain = domain;
            m_isComplete = true;
            Debug.Assert(
                m_domain.Count != 0, "If you want a boolean that evaluates to false, " +
                                     "use the ConstantBool abstraction");
        }

        /// <summary>
        /// Creates a complete member restriction with the meaning "<paramref name="slot"/> in <paramref name="values"/>".
        /// </summary>
        /// <param name="possibleValues">all the values that the <paramref name="slot"/> can take</param>
        protected MemberRestriction(MemberProjectedSlot slot, IEnumerable<Constant> values, IEnumerable<Constant> possibleValues)
            : this(slot, new Domain(values, possibleValues))
        {
            Debug.Assert(possibleValues != null);
        }

        #endregion

        #region Fields

        private readonly MemberProjectedSlot m_restrictedMemberSlot;
        private readonly Domain m_domain;
        private readonly bool m_isComplete;

        #endregion

        #region Properties

        internal bool IsComplete
        {
            get { return m_isComplete; }
        }

        /// <summary>
        /// Returns the variable in the member restriction.
        /// </summary>
        internal MemberProjectedSlot RestrictedMemberSlot
        {
            get { return m_restrictedMemberSlot; }
        }

        /// <summary>
        /// Returns the values that <see cref="RestrictedMemberSlot"/> is being checked for.
        /// </summary>
        internal Domain Domain
        {
            get { return m_domain; }
        }

        #endregion

        #region BoolLiteral Members

        /// <summary>
        /// Returns a boolean expression that is domain-aware and ready for optimizations etc.
        /// </summary>
        /// <param name="domainMap">Maps members to the values that each member can take;
        /// it can be null in which case the possible and actual values are the same.</param>
        internal override DomainBoolExpr GetDomainBoolExpression(MemberDomainMap domainMap)
        {
            // Get the variable name from the slot's memberpath and the possible domain values from the slot
            DomainTermExpr result;
            if (domainMap != null)
            {
                // Look up the domain from the domainMap
                var domain = domainMap.GetDomain(m_restrictedMemberSlot.MemberPath);
                result = MakeTermExpression(this, domain, m_domain.Values);
            }
            else
            {
                result = MakeTermExpression(this, m_domain.AllPossibleValues, m_domain.Values);
            }
            return result;
        }

        /// <summary>
        /// Creates a complete member restriction based on the existing restriction with possible values for the domain being given by <paramref name="possibleValues"/>.
        /// </summary>
        internal abstract MemberRestriction CreateCompleteMemberRestriction(IEnumerable<Constant> possibleValues);

        /// <summary>
        /// See <see cref="BoolLiteral.GetRequiredSlots"/>.
        /// </summary>
        internal override void GetRequiredSlots(MemberProjectionIndex projectedSlotMap, bool[] requiredSlots)
        {
            // Simply get the slot for the variable var in "var in values"
            var member = RestrictedMemberSlot.MemberPath;
            var slotNum = projectedSlotMap.IndexOf(member);
            requiredSlots[slotNum] = true;
        }

        /// <summary>
        /// See <see cref="BoolLiteral.IsEqualTo"/>. Member restriction can be incomplete for this operation. 
        /// </summary>
        protected override bool IsEqualTo(BoolLiteral right)
        {
            var rightRestriction = right as MemberRestriction;
            if (rightRestriction == null)
            {
                return false;
            }
            if (ReferenceEquals(this, rightRestriction))
            {
                return true;
            }
            if (false == ProjectedSlot.EqualityComparer.Equals(m_restrictedMemberSlot, rightRestriction.m_restrictedMemberSlot))
            {
                return false;
            }

            return m_domain.IsEqualTo(rightRestriction.m_domain);
        }

        /// <summary>
        /// Member restriction can be incomplete for this operation. 
        /// </summary>
        public override int GetHashCode()
        {
            var result = ProjectedSlot.EqualityComparer.GetHashCode(m_restrictedMemberSlot);
            result ^= m_domain.GetHash();
            return result;
        }

        /// <summary>
        /// See <see cref="BoolLiteral.IsIdentifierEqualTo"/>. Member restriction can be incomplete for this operation. 
        /// </summary>
        protected override bool IsIdentifierEqualTo(BoolLiteral right)
        {
            var rightOneOfConst = right as MemberRestriction;
            if (rightOneOfConst == null)
            {
                return false;
            }
            if (ReferenceEquals(this, rightOneOfConst))
            {
                return true;
            }
            return ProjectedSlot.EqualityComparer.Equals(m_restrictedMemberSlot, rightOneOfConst.m_restrictedMemberSlot);
        }

        /// <summary>
        /// See <see cref="BoolLiteral.GetIdentifierHash"/>. Member restriction can be incomplete for this operation. 
        /// </summary>
        protected override int GetIdentifierHash()
        {
            var result = ProjectedSlot.EqualityComparer.GetHashCode(m_restrictedMemberSlot);
            return result;
        }

        #endregion

        #region Other Methods

        internal override StringBuilder AsUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull)
        {
            return AsEsql(builder, blockAlias, skipIsNotNull);
        }

        internal override StringBuilder AsNegatedUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull)
        {
            builder.Append("NOT(");
            builder = AsUserString(builder, blockAlias, skipIsNotNull);
            builder.Append(")");
            return builder;
        }

        #endregion
    }
}
