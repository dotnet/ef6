// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>Represents different kinds of constants (literals). This class cannot be inherited.</summary>
    public class DbConstantExpression : DbExpression
    {
        private readonly bool _shouldCloneValue;
        private readonly object _value;

        internal DbConstantExpression()
        {
        }

        internal DbConstantExpression(TypeUsage resultType, object value)
            : base(DbExpressionKind.Constant, resultType)
        {
            DebugCheck.NotNull(value);
            Debug.Assert(TypeSemantics.IsScalarType(resultType), "DbConstantExpression must have a primitive or enum value");
            Debug.Assert(
                !value.GetType().IsEnum || TypeSemantics.IsEnumerationType(resultType),
                "value is an enum while the result type is not of enum type.");
            Debug.Assert(
                Helper.AsPrimitive(resultType.EdmType).ClrEquivalentType
                == (value.GetType().IsEnum ? value.GetType().GetEnumUnderlyingType() : value.GetType()),
                "the type of the value has to match the result type (for enum types only underlying types are compared).");

            // binary values should be cloned before use
            PrimitiveType primitiveType;
            _shouldCloneValue = TypeHelpers.TryGetEdmType(resultType, out primitiveType)
                                && primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Binary;

            if (_shouldCloneValue)
            {
                // DevDiv#480416: DbConstantExpression with a binary value is not fully immutable
                // CONSIDER: Adding an immutable Binary type or using System.Data.Linq.Binary
                _value = ((byte[])value).Clone();
            }
            else
            {
                _value = value;
            }
        }

        /// <summary>
        /// Provides direct access to the constant value, even for byte[] constants.
        /// </summary>
        /// <returns> The object value contained by this constant expression, not a copy. </returns>
        internal object GetValue()
        {
            return _value;
        }

        /// <summary>Gets the constant value.</summary>
        /// <returns>The constant value.</returns>
        public virtual object Value
        {
            get
            {
                // DevDiv#480416: DbConstantExpression with a binary value is not fully immutable
                // CONSIDER: Adding an immutable Binary type or using System.Data.Linq.Binary
                if (_shouldCloneValue)
                {
                    return ((byte[])_value).Clone();
                }
                else
                {
                    return _value;
                }
            }
        }

        /// <summary>Implements the visitor pattern for expressions that do not produce a result value.</summary>
        /// <param name="visitor">
        /// An instance of <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionVisitor" />.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException"> visitor  is null.</exception>
        public override void Accept(DbExpressionVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }

        /// <summary>Implements the visitor pattern for expressions that produce a result value of a specific type.</summary>
        /// <returns>
        /// A result value of a specific type produced by
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionVisitor" />
        /// .
        /// </returns>
        /// <param name="visitor">
        /// An instance of a typed <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionVisitor" /> that produces a result value of a specific type.
        /// </param>
        /// <typeparam name="TResultType">The type of the result produced by  visitor. </typeparam>
        /// <exception cref="T:System.ArgumentNullException"> visitor  is null.</exception>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor)
        {
            Check.NotNull(visitor, "visitor");

            return visitor.Visit(this);
        }
    }
}
