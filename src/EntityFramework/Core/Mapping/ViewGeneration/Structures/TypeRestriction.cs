// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using DomainBoolExpr =
        System.Data.Entity.Core.Common.Utils.Boolean.BoolExpr<Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>>;

    /// <summary>
    ///     A class that denotes the boolean expression: "varType in values".
    ///     See the comments in <see cref="MemberRestriction" /> for complete and incomplete restriction objects.
    /// </summary>
    internal class TypeRestriction : MemberRestriction
    {
        /// <summary>
        ///     Creates an incomplete type restriction of the form "<paramref name="member" /> in <paramref name="values" />".
        /// </summary>
        internal TypeRestriction(MemberPath member, IEnumerable<EdmType> values)
            : base(new MemberProjectedSlot(member), CreateTypeConstants(values))
        {
        }

        /// <summary>
        ///     Creates an incomplete type restriction of the form "<paramref name="member" /> = <paramref name="value" />".
        /// </summary>
        internal TypeRestriction(MemberPath member, Constant value)
            : base(new MemberProjectedSlot(member), value)
        {
            Debug.Assert(value is TypeConstant || value.IsNull(), "Type or NULL expected.");
        }

        /// <summary>
        ///     Creates a complete type restriction of the form "<paramref name="slot" /> in <paramref name="domain" />".
        /// </summary>
        internal TypeRestriction(MemberProjectedSlot slot, Domain domain)
            : base(slot, domain)
        {
        }

        /// <summary>
        ///     Requires: <see cref="MemberRestriction.IsComplete" /> is true.
        /// </summary>
        internal override DomainBoolExpr FixRange(Set<Constant> range, MemberDomainMap memberDomainMap)
        {
            Debug.Assert(IsComplete, "Ranges are fixed only for complete type restrictions.");
            var possibleValues = memberDomainMap.GetDomain(RestrictedMemberSlot.MemberPath);
            BoolLiteral newLiteral = new TypeRestriction(RestrictedMemberSlot, new Domain(range, possibleValues));
            return newLiteral.GetDomainBoolExpression(memberDomainMap);
        }

        internal override BoolLiteral RemapBool(Dictionary<MemberPath, MemberPath> remap)
        {
            var newVar = RestrictedMemberSlot.RemapSlot(remap);
            return new TypeRestriction(newVar, Domain);
        }

        internal override MemberRestriction CreateCompleteMemberRestriction(IEnumerable<Constant> possibleValues)
        {
            Debug.Assert(!IsComplete, "CreateCompleteMemberRestriction must be called only for incomplete restrictions.");
            return new TypeRestriction(RestrictedMemberSlot, new Domain(Domain.Values, possibleValues));
        }

        internal override StringBuilder AsEsql(StringBuilder builder, string blockAlias, bool skipIsNotNull)
        {
            // Add Cql of the form "(T.A IS OF (ONLY Person) OR .....)"

            // Important to enclose all the OR statements in parens.
            if (Domain.Count > 1)
            {
                builder.Append('(');
            }

            var isFirst = true;
            foreach (var constant in Domain.Values)
            {
                var typeConstant = constant as TypeConstant;
                Debug.Assert(typeConstant != null || constant.IsNull(), "Constants for type checks must be type constants or NULLs");

                if (isFirst == false)
                {
                    builder.Append(" OR ");
                }
                isFirst = false;
                if (Helper.IsRefType(RestrictedMemberSlot.MemberPath.EdmType))
                {
                    builder.Append("Deref(");
                    RestrictedMemberSlot.MemberPath.AsEsql(builder, blockAlias);
                    builder.Append(')');
                }
                else
                {
                    // non-reference type
                    RestrictedMemberSlot.MemberPath.AsEsql(builder, blockAlias);
                }
                if (constant.IsNull())
                {
                    builder.Append(" IS NULL");
                }
                else
                {
                    // type constant
                    builder.Append(" IS OF (ONLY ");
                    CqlWriter.AppendEscapedTypeName(builder, typeConstant.EdmType);
                    builder.Append(')');
                }
            }

            if (Domain.Count > 1)
            {
                builder.Append(')');
            }

            return builder;
        }

        internal override DbExpression AsCqt(DbExpression row, bool skipIsNotNull)
        {
            var cqt = RestrictedMemberSlot.MemberPath.AsCqt(row);

            if (Helper.IsRefType(RestrictedMemberSlot.MemberPath.EdmType))
            {
                cqt = cqt.Deref();
            }

            if (Domain.Count == 1)
            {
                // Single value
                cqt = cqt.IsOfOnly(TypeUsage.Create(((TypeConstant)Domain.Values.Single()).EdmType));
            }
            else
            {
                // Multiple values: build list of var IsOnOnly(t1), var = IsOnOnly(t1), ..., then OR them all.
                var operands = Domain.Values.Select(t => (DbExpression)cqt.IsOfOnly(TypeUsage.Create(((TypeConstant)t).EdmType))).ToList();
                cqt = Helpers.BuildBalancedTreeInPlace(operands, (prev, next) => prev.Or(next));
            }

            return cqt;
        }

        internal override StringBuilder AsUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull)
        {
            // Add user readable string of the form "T.A IS a (Person OR .....)"

            if (Helper.IsRefType(RestrictedMemberSlot.MemberPath.EdmType))
            {
                builder.Append("Deref(");
                RestrictedMemberSlot.MemberPath.AsEsql(builder, blockAlias);
                builder.Append(')');
            }
            else
            {
                // non-reference type
                RestrictedMemberSlot.MemberPath.AsEsql(builder, blockAlias);
            }

            if (Domain.Count > 1)
            {
                builder.Append(" is a (");
            }
            else
            {
                builder.Append(" is type ");
            }

            var isFirst = true;
            foreach (var constant in Domain.Values)
            {
                var typeConstant = constant as TypeConstant;
                Debug.Assert(typeConstant != null || constant.IsNull(), "Constants for type checks must be type constants or NULLs");

                if (isFirst == false)
                {
                    builder.Append(" OR ");
                }

                if (constant.IsNull())
                {
                    builder.Append(" NULL");
                }
                else
                {
                    CqlWriter.AppendEscapedTypeName(builder, typeConstant.EdmType);
                }

                isFirst = false;
            }

            if (Domain.Count > 1)
            {
                builder.Append(')');
            }
            return builder;
        }

        /// <summary>
        ///     Given a list of <paramref name="types" /> (which can contain nulls), returns a corresponding list of <see
        ///      cref="TypeConstant" />s for those types.
        /// </summary>
        private static IEnumerable<Constant> CreateTypeConstants(IEnumerable<EdmType> types)
        {
            foreach (var type in types)
            {
                if (type == null)
                {
                    yield return Constant.Null;
                }
                else
                {
                    yield return new TypeConstant(type);
                }
            }
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            builder.Append("type(");
            RestrictedMemberSlot.ToCompactString(builder);
            builder.Append(") IN (");
            StringUtil.ToCommaSeparatedStringSorted(builder, Domain.Values);
            builder.Append(")");
        }
    }
}
