// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    ///     Represents a constant value.
    /// </summary>
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
            Debug.Assert(value != null, "DbConstantExpression value cannot be null");
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
        ///     Provides direct access to the constant value, even for byte[] constants.
        /// </summary>
        /// <returns> The object value contained by this constant expression, not a copy. </returns>
        internal object GetValue()
        {
            return _value;
        }

        /// <summary>
        ///     Gets the constant value.
        /// </summary>
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

        /// <summary>
        ///     The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor"> An instance of DbExpressionVisitor. </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="visitor" />
        ///     is null</exception>
        public override void Accept(DbExpressionVisitor visitor)
        {
            if (visitor != null)
            {
                visitor.Visit(this);
            }
            else
            {
                throw new ArgumentNullException("visitor");
            }
        }

        /// <summary>
        ///     The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor"> An instance of a typed DbExpressionVisitor that produces a result value of type TResultType. </param>
        /// <typeparam name="TResultType"> The type of the result produced by <paramref name="visitor" /> </typeparam>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="visitor" />
        ///     is null</exception>
        /// <returns> An instance of <typeparamref name="TResultType" /> . </returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor)
        {
            if (visitor != null)
            {
                return visitor.Visit(this);
            }
            else
            {
                throw new ArgumentNullException("visitor");
            }
        }
    }
}
