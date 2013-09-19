// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// A class that represents NOT(elements), e.g., NOT(1, 2, NULL), i.e., all values other than null, 1 and 2
    /// </summary>
    internal sealed class NegatedConstant : Constant
    {
        /// <summary>
        /// Creates a negated constant with the <paramref name="values" /> in it.
        /// </summary>
        /// <param name="values">
        /// must have no <see cref=" NegatedConstant" /> items
        /// </param>
        internal NegatedConstant(IEnumerable<Constant> values)
        {
            Debug.Assert(!values.Any(v => v is NegatedConstant), "Negated constant values must not contain another negated constant.");
            m_negatedDomain = new Set<Constant>(values, EqualityComparer);
        }

        /// <summary>
        /// e.g., NOT(1, 2, Undefined)
        /// </summary>
        private readonly Set<Constant> m_negatedDomain;

        internal IEnumerable<Constant> Elements
        {
            get { return m_negatedDomain; }
        }

        /// <summary>
        /// Returns true if the negated constant contains <paramref name="constant" />.
        /// </summary>
        internal bool Contains(Constant constant)
        {
            return m_negatedDomain.Contains(constant);
        }

        internal override bool IsNull()
        {
            return false;
        }

        internal override bool IsNotNull()
        {
            if (ReferenceEquals(this, NotNull))
            {
                return true;
            }
            else
            {
                return m_negatedDomain.Count == 1 && m_negatedDomain.Contains(Null);
            }
        }

        internal override bool IsUndefined()
        {
            return false;
        }

        /// <summary>
        /// Returns true if the negated constant contains <see cref="Constant.Null" />.
        /// </summary>
        internal override bool HasNotNull()
        {
            return m_negatedDomain.Contains(Null);
        }

        public override int GetHashCode()
        {
            var result = 0;
            foreach (var constant in m_negatedDomain)
            {
                result ^= EqualityComparer.GetHashCode(constant);
            }
            return result;
        }

        protected override bool IsEqualTo(Constant right)
        {
            var rightNegatedConstant = right as NegatedConstant;
            if (rightNegatedConstant == null)
            {
                return false;
            }

            return m_negatedDomain.SetEquals(rightNegatedConstant.m_negatedDomain);
        }

        /// <summary>
        /// Not supported in this class.
        /// </summary>
        internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias)
        {
            Debug.Fail("Should not be called.");
            return null; // To keep the compiler happy
        }

        /// <summary>
        /// Not supported in this class.
        /// </summary>
        internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
        {
            Debug.Fail("Should not be called.");
            return null; // To keep the compiler happy
        }

        internal StringBuilder AsEsql(
            StringBuilder builder, string blockAlias, IEnumerable<Constant> constants, MemberPath outputMember, bool skipIsNotNull)
        {
            return ToStringHelper(builder, blockAlias, constants, outputMember, skipIsNotNull, false);
        }

        internal DbExpression AsCqt(DbExpression row, IEnumerable<Constant> constants, MemberPath outputMember, bool skipIsNotNull)
        {
            DbExpression cqt = null;

            AsCql(
                // trueLiteral action
                () => cqt = DbExpressionBuilder.True,
                // varIsNotNull action
                () => cqt = outputMember.AsCqt(row).IsNull().Not(),
                // varNotEqualsTo action
                (constant) =>
                    {
                        DbExpression notEqualsExpr = outputMember.AsCqt(row).NotEqual(constant.AsCqt(row, outputMember));
                        if (cqt != null)
                        {
                            cqt = cqt.And(notEqualsExpr);
                        }
                        else
                        {
                            cqt = notEqualsExpr;
                        }
                    },
                constants, outputMember, skipIsNotNull);

            return cqt;
        }

        internal StringBuilder AsUserString(
            StringBuilder builder, string blockAlias, IEnumerable<Constant> constants, MemberPath outputMember, bool skipIsNotNull)
        {
            return ToStringHelper(builder, blockAlias, constants, outputMember, skipIsNotNull, true);
        }

        /// <summary>
        /// Given a set of positive <paramref name="constants" /> generates a simplified negated constant Cql expression.
        /// Examples:
        /// - 7, NOT(7, NULL) means NOT(NULL)
        /// - 7, 8, NOT(7, 8, 9, 10) means NOT(9, 10)
        /// </summary>
        private void AsCql(
            Action trueLiteral, Action varIsNotNull, Action<Constant> varNotEqualsTo, IEnumerable<Constant> constants,
            MemberPath outputMember, bool skipIsNotNull)
        {
            var isNullable = outputMember.IsNullable;
            // Remove all the constants from negated and then print "x <> C1 .. AND x <> C2 .. AND x <> C3 ..."
            var negatedConstants = new Set<Constant>(Elements, EqualityComparer);
            foreach (var constant in constants)
            {
                if (constant.Equals(this))
                {
                    continue;
                }
                Debug.Assert(negatedConstants.Contains(constant), "Negated constant must contain all positive constants");
                negatedConstants.Remove(constant);
            }

            if (negatedConstants.Count == 0)
            {
                // All constants cancel out - emit True.
                trueLiteral();
            }
            else
            {
                var hasNull = negatedConstants.Contains(Null);
                negatedConstants.Remove(Null);

                // We always add IS NOT NULL if the property is nullable (and we cannot skip IS NOT NULL).
                // Also, if the domain contains NOT NULL, we must add it.

                if (hasNull || (isNullable && !skipIsNotNull))
                {
                    varIsNotNull();
                }

                foreach (var constant in negatedConstants)
                {
                    varNotEqualsTo(constant);
                }
            }
        }

        private StringBuilder ToStringHelper(
            StringBuilder builder, string blockAlias, IEnumerable<Constant> constants, MemberPath outputMember, bool skipIsNotNull,
            bool userString)
        {
            var anyAdded = false;
            AsCql(
                // trueLiteral action
                () => builder.Append("true"),
                // varIsNotNull action
                () =>
                    {
                        if (userString)
                        {
                            outputMember.ToCompactString(builder, blockAlias);
                            builder.Append(" is not NULL");
                        }
                        else
                        {
                            outputMember.AsEsql(builder, blockAlias);
                            builder.Append(" IS NOT NULL");
                        }
                        anyAdded = true;
                    },
                // varNotEqualsTo action
                (constant) =>
                    {
                        if (anyAdded)
                        {
                            builder.Append(" AND ");
                        }
                        anyAdded = true;

                        if (userString)
                        {
                            outputMember.ToCompactString(builder, blockAlias);
                            builder.Append(" <>");
                            constant.ToCompactString(builder);
                        }
                        else
                        {
                            outputMember.AsEsql(builder, blockAlias);
                            builder.Append(" <>");
                            constant.AsEsql(builder, outputMember, blockAlias);
                        }
                    },
                constants, outputMember, skipIsNotNull);
            return builder;
        }

        internal override string ToUserString()
        {
            if (IsNotNull())
            {
                return Strings.ViewGen_NotNull;
            }
            else
            {
                var builder = new StringBuilder();
                var isFirst = true;
                foreach (var constant in m_negatedDomain)
                {
                    // Skip printing out Null if m_negatedDomain has other values
                    if (m_negatedDomain.Count > 1
                        && constant.IsNull())
                    {
                        continue;
                    }
                    if (isFirst == false)
                    {
                        builder.Append(Strings.ViewGen_CommaBlank);
                    }
                    isFirst = false;
                    builder.Append(constant.ToUserString());
                }
                var result = new StringBuilder();
                result.Append(Strings.ViewGen_NegatedCellConstant(builder.ToString()));
                return result.ToString();
            }
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            if (IsNotNull())
            {
                builder.Append("NOT_NULL");
            }
            else
            {
                builder.Append("NOT(");
                StringUtil.ToCommaSeparatedStringSorted(builder, m_negatedDomain);
                builder.Append(")");
            }
        }
    }
}
