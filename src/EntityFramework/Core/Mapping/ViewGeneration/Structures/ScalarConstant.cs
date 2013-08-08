// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Text;

    /// <summary>
    /// A class that denotes a constant value that can be stored in a multiconstant or in a projected slot of a
    /// <see
    ///     cref="CellQuery" />
    /// .
    /// </summary>
    internal sealed class ScalarConstant : Constant
    {
        /// <summary>
        /// Creates a scalar constant corresponding to the <paramref name="value" />.
        /// </summary>
        internal ScalarConstant(object value)
        {
            DebugCheck.NotNull(value);
            m_scalar = value;
        }

        /// <summary>
        /// The actual value of the scalar.
        /// </summary>
        private readonly object m_scalar;

        internal object Value
        {
            get { return m_scalar; }
        }

        internal override bool IsNull()
        {
            return false;
        }

        internal override bool IsNotNull()
        {
            return false;
        }

        internal override bool IsUndefined()
        {
            return false;
        }

        internal override bool HasNotNull()
        {
            return false;
        }

        internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias)
        {
            DebugCheck.NotNull(outputMember.LeafEdmMember);
            var modelTypeUsage = Helper.GetModelTypeUsage(outputMember.LeafEdmMember);
            var modelType = modelTypeUsage.EdmType;

            // Some built-in constants
            if (BuiltInTypeKind.PrimitiveType
                == modelType.BuiltInTypeKind)
            {
                var primitiveTypeKind = ((PrimitiveType)modelType).PrimitiveTypeKind;
                if (primitiveTypeKind == PrimitiveTypeKind.Boolean)
                {
                    // This better be a boolean. Else we crash!
                    var val = (bool)m_scalar;
                    var value = StringUtil.FormatInvariant("{0}", val);
                    builder.Append(value);
                    return builder;
                }
                else if (primitiveTypeKind == PrimitiveTypeKind.String)
                {
                    bool isUnicode;
                    if (!TypeHelpers.TryGetIsUnicode(modelTypeUsage, out isUnicode))
                    {
                        // If can't determine - use the safest option, assume unicode.
                        isUnicode = true;
                    }

                    if (isUnicode)
                    {
                        builder.Append('N');
                    }

                    AppendEscapedScalar(builder);
                    return builder;
                }
            }
            else if (BuiltInTypeKind.EnumType
                     == modelType.BuiltInTypeKind)
            {
                // Enumerated type - we should be able to cast it
                var enumMember = (EnumMember)m_scalar;

                builder.Append(enumMember.Name);
                return builder;
            }

            // Need to cast
            builder.Append("CAST(");
            AppendEscapedScalar(builder);
            builder.Append(" AS ");
            CqlWriter.AppendEscapedTypeName(builder, modelType);
            builder.Append(')');
            return builder;
        }

        private StringBuilder AppendEscapedScalar(StringBuilder builder)
        {
            var value = StringUtil.FormatInvariant("{0}", m_scalar);
            if (value.Contains("'"))
            {
                // Deal with strings with ' by doubling it
                value = value.Replace("'", "''");
            }
            StringUtil.FormatStringBuilder(builder, "'{0}'", value);
            return builder;
        }

        internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
        {
            DebugCheck.NotNull(outputMember.LeafEdmMember);
            var modelTypeUsage = Helper.GetModelTypeUsage(outputMember.LeafEdmMember);
            return modelTypeUsage.Constant(m_scalar);
        }

        protected override bool IsEqualTo(Constant right)
        {
            var rightScalarConstant = right as ScalarConstant;
            if (rightScalarConstant == null)
            {
                return false;
            }

            return ByValueEqualityComparer.Default.Equals(m_scalar, rightScalarConstant.m_scalar);
        }

        public override int GetHashCode()
        {
            return m_scalar.GetHashCode();
        }

        internal override string ToUserString()
        {
            var builder = new StringBuilder();
            ToCompactString(builder);
            return builder.ToString();
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            var enumMember = m_scalar as EnumMember;
            if (enumMember != null)
            {
                builder.Append(enumMember.Name);
            }
            else
            {
                builder.Append(StringUtil.FormatInvariant("'{0}'", m_scalar));
            }
        }
    }
}
