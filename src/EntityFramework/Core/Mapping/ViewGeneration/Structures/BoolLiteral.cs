// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.Utils;
    using System.Text;
    using DomainConstraint = System.Data.Entity.Core.Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>;
    using DomainVariable = System.Data.Entity.Core.Common.Utils.Boolean.DomainVariable<BoolLiteral, Constant>;
    using DomainBoolExpr =
        System.Data.Entity.Core.Common.Utils.Boolean.BoolExpr<Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>>;
    using DomainTermExpr =
        System.Data.Entity.Core.Common.Utils.Boolean.TermExpr<Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>>;

    /// <summary>
    ///     A class that ties up all the literals in boolean expressions.
    ///     Conditions represented by <see cref="BoolLiteral" />s need to be synchronized with <see cref="DomainConstraint" />s,
    ///     which may be modified upon calling <see cref="BoolExpression.ExpensiveSimplify" />. This is what the method
    ///     <see
    ///         cref="BoolLiteral.FixRange" />
    ///     is used for.
    /// </summary>
    internal abstract class BoolLiteral : InternalBase
    {
        internal static readonly IEqualityComparer<BoolLiteral> EqualityComparer = new BoolLiteralComparer();
        internal static readonly IEqualityComparer<BoolLiteral> EqualityIdentifierComparer = new IdentifierComparer();

        /// <summary>
        ///     Creates a term expression of the form: "<paramref name="literal" /> in <paramref name="range" /> with all possible values being
        ///     <paramref
        ///         name="domain" />
        ///     ".
        /// </summary>
        internal static DomainTermExpr MakeTermExpression(BoolLiteral literal, IEnumerable<Constant> domain, IEnumerable<Constant> range)
        {
            var domainSet = new Set<Constant>(domain, Constant.EqualityComparer);
            var rangeSet = new Set<Constant>(range, Constant.EqualityComparer);
            return MakeTermExpression(literal, domainSet, rangeSet);
        }

        /// <summary>
        ///     Creates a term expression of the form: "<paramref name="literal" /> in <paramref name="range" /> with all possible values being
        ///     <paramref
        ///         name="domain" />
        ///     ".
        /// </summary>
        internal static DomainTermExpr MakeTermExpression(BoolLiteral literal, Set<Constant> domain, Set<Constant> range)
        {
            domain.MakeReadOnly();
            range.MakeReadOnly();

            var variable = new DomainVariable(literal, domain, EqualityIdentifierComparer);
            var constraint = new DomainConstraint(variable, range);
            var result = new DomainTermExpr(EqualityComparer<DomainConstraint>.Default, constraint);
            return result;
        }

        /// <summary>
        ///     Fixes the range of the literal using the new values provided in <paramref name="range" /> and returns a boolean expression corresponding to the new value.
        /// </summary>
        internal abstract DomainBoolExpr FixRange(Set<Constant> range, MemberDomainMap memberDomainMap);

        internal abstract DomainBoolExpr GetDomainBoolExpression(MemberDomainMap domainMap);

        /// <summary>
        ///     See <see cref="BoolExpression.RemapBool" />.
        /// </summary>
        internal abstract BoolLiteral RemapBool(Dictionary<MemberPath, MemberPath> remap);

        /// <summary>
        ///     See <see cref="BoolExpression.GetRequiredSlots" />.
        /// </summary>
        /// <param name="projectedSlotMap"> </param>
        /// <param name="requiredSlots"> </param>
        internal abstract void GetRequiredSlots(MemberProjectionIndex projectedSlotMap, bool[] requiredSlots);

        /// <summary>
        ///     See <see cref="BoolExpression.AsEsql" />.
        /// </summary>
        internal abstract StringBuilder AsEsql(StringBuilder builder, string blockAlias, bool skipIsNotNull);

        /// <summary>
        ///     See <see cref="BoolExpression.AsCqt" />.
        /// </summary>
        internal abstract DbExpression AsCqt(DbExpression row, bool skipIsNotNull);

        internal abstract StringBuilder AsUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull);

        internal abstract StringBuilder AsNegatedUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull);

        /// <summary>
        ///     Checks if the identifier in this is the same as the one in <paramref name="right" />.
        /// </summary>
        protected virtual bool IsIdentifierEqualTo(BoolLiteral right)
        {
            return IsEqualTo(right);
        }

        protected abstract bool IsEqualTo(BoolLiteral right);

        /// <summary>
        ///     Get the hash code based on the identifier.
        /// </summary>
        protected virtual int GetIdentifierHash()
        {
            return GetHashCode();
        }

        /// <summary>
        ///     This class compares boolean expressions.
        /// </summary>
        private sealed class BoolLiteralComparer : IEqualityComparer<BoolLiteral>
        {
            public bool Equals(BoolLiteral left, BoolLiteral right)
            {
                // Quick check with references
                if (ReferenceEquals(left, right))
                {
                    // Gets the Null and Undefined case as well
                    return true;
                }
                // One of them is non-null at least
                if (left == null
                    || right == null)
                {
                    return false;
                }
                // Both are non-null at this point
                return left.IsEqualTo(right);
            }

            public int GetHashCode(BoolLiteral literal)
            {
                return literal.GetHashCode();
            }
        }

        /// <summary>
        ///     This class compares just the identifier in boolean expressions.
        /// </summary>
        private sealed class IdentifierComparer : IEqualityComparer<BoolLiteral>
        {
            public bool Equals(BoolLiteral left, BoolLiteral right)
            {
                // Quick check with references
                if (ReferenceEquals(left, right))
                {
                    // Gets the Null and Undefined case as well
                    return true;
                }
                // One of them is non-null at least
                if (left == null
                    || right == null)
                {
                    return false;
                }
                // Both are non-null at this point
                return left.IsIdentifierEqualTo(right);
            }

            public int GetHashCode(BoolLiteral literal)
            {
                return literal.GetIdentifierHash();
            }
        }
    }
}
