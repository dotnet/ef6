// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.ComponentModel;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    ///     The base type for all expressions
    /// </summary>
    public abstract class DbExpression
    {
        private readonly TypeUsage _type;
        private readonly DbExpressionKind _kind;

        internal DbExpression()
        {
        }

        internal DbExpression(DbExpressionKind kind, TypeUsage type)
        {
            CheckExpressionKind(kind);
            _kind = kind;

            Debug.Assert(
                type != null, string.Format(CultureInfo.InvariantCulture, "{0}.Type is null in DbExpression constructor", GetType().Name));
            if (!TypeSemantics.IsNullable(type))
            {
                type = type.ShallowCopy(
                    new FacetValues
                        {
                            Nullable = true
                        });
            }
            Debug.Assert(type.IsReadOnly, "Editable type metadata specified for DbExpression.Type");
            _type = type;
        }

        /// <summary>
        ///     Gets the type metadata for the result type of the expression.
        /// </summary>
        public virtual TypeUsage ResultType
        {
            get { return _type; }
        }

        /// <summary>
        ///     Gets the kind of the expression, which indicates the operation of this expression.
        /// </summary>
        public virtual DbExpressionKind ExpressionKind
        {
            get { return _kind; }
        }

        /// <summary>
        ///     The visitor pattern interface method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor"> An instance of DbExpressionVisitor. </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="visitor" />
        ///     is null
        /// </exception>
        public abstract void Accept(DbExpressionVisitor visitor);

        /// <summary>
        ///     The visitor pattern interface method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor"> An instance of a typed DbExpressionVisitor that produces a result value of type TResultType. </param>
        /// <typeparam name="TResultType">
        ///     The type of the result produced by <paramref name="visitor" />
        /// </typeparam>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="visitor" />
        ///     is null
        /// </exception>
        /// <returns>
        ///     An instance of <typeparamref name="TResultType" /> .
        /// </returns>
        public abstract TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor);

        #region Equals / GetHashCode

        // Dev10#547254: Easy to confuse DbExpressionBuilder.Equal with object.Equals method
        // The object.Equals method is overriden on DbExpression and marked so that it does
        // not appear in IntelliSense to avoid confusion with the DbExpressionBuilder.Equal
        // expression construction method. Overriding Equals also requires that GetHashCode
        // is overridden, however in both cases we defer to the System.Object implementation.

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

        #region Implicit Cast Operators

        /// <summary>
        ///     Creates a <see cref="DbExpression" /> that represents the specified binary value, which may be <c>null</c>
        /// </summary>
        /// <param name="value"> The binary value on which the returned expression should be based </param>
        /// <returns>
        ///     A <see cref="DbExpression" /> that represents the specified binary value
        /// </returns>
        public static DbExpression FromBinary(byte[] value)
        {
            if (null == value)
            {
                return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Binary);
            }
            return DbExpressionBuilder.Constant(value);
        }

        public static implicit operator DbExpression(byte[] value)
        {
            return FromBinary(value);
        }

        /// <summary>
        ///     Creates a <see cref="DbExpression" /> that represents the specified (nullable) Boolean value
        /// </summary>
        /// <param name="value"> The Boolean value on which the returned expression should be based </param>
        /// <returns>
        ///     A <see cref="DbExpression" /> that represents the specified Boolean value
        /// </returns>
        public static DbExpression FromBoolean(bool? value)
        {
            if (!value.HasValue)
            {
                return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Boolean);
            }
            return (value.Value ? DbExpressionBuilder.True : DbExpressionBuilder.False);
        }

        public static implicit operator DbExpression(bool? value)
        {
            return FromBoolean(value);
        }

        /// <summary>
        ///     Creates a <see cref="DbExpression" /> that represents the specified (nullable) byte value
        /// </summary>
        /// <param name="value"> The byte value on which the returned expression should be based </param>
        /// <returns>
        ///     A <see cref="DbExpression" /> that represents the specified byte value
        /// </returns>
        public static DbExpression FromByte(byte? value)
        {
            if (!value.HasValue)
            {
                return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Byte);
            }
            return DbExpressionBuilder.Constant(value.Value);
        }

        public static implicit operator DbExpression(byte? value)
        {
            return FromByte(value);
        }

        /// <summary>
        ///     Creates a <see cref="DbExpression" /> that represents the specified (nullable) <see cref="DateTime" /> value
        /// </summary>
        /// <param name="value"> The DateTime value on which the returned expression should be based </param>
        /// <returns>
        ///     A <see cref="DbExpression" /> that represents the specified DateTime value
        /// </returns>
        public static DbExpression FromDateTime(DateTime? value)
        {
            if (!value.HasValue)
            {
                return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.DateTime);
            }
            return DbExpressionBuilder.Constant(value.Value);
        }

        public static implicit operator DbExpression(DateTime? value)
        {
            return FromDateTime(value);
        }

        /// <summary>
        ///     Creates a <see cref="DbExpression" /> that represents the specified (nullable) <see cref="DateTimeOffset" /> value
        /// </summary>
        /// <param name="value"> The DateTimeOffset value on which the returned expression should be based </param>
        /// <returns>
        ///     A <see cref="DbExpression" /> that represents the specified DateTimeOffset value
        /// </returns>
        public static DbExpression FromDateTimeOffset(DateTimeOffset? value)
        {
            if (!value.HasValue)
            {
                return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.DateTimeOffset);
            }
            return DbExpressionBuilder.Constant(value.Value);
        }

        public static implicit operator DbExpression(DateTimeOffset? value)
        {
            return FromDateTimeOffset(value);
        }

        /// <summary>
        ///     Creates a <see cref="DbExpression" /> that represents the specified (nullable) decimal value
        /// </summary>
        /// <param name="value"> The decimal value on which the returned expression should be based </param>
        /// <returns>
        ///     A <see cref="DbExpression" /> that represents the specified decimal value
        /// </returns>
        public static DbExpression FromDecimal(decimal? value)
        {
            if (!value.HasValue)
            {
                return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Decimal);
            }
            return DbExpressionBuilder.Constant(value.Value);
        }

        public static implicit operator DbExpression(decimal? value)
        {
            return FromDecimal(value);
        }

        /// <summary>
        ///     Creates a <see cref="DbExpression" /> that represents the specified (nullable) double value
        /// </summary>
        /// <param name="value"> The double value on which the returned expression should be based </param>
        /// <returns>
        ///     A <see cref="DbExpression" /> that represents the specified double value
        /// </returns>
        public static DbExpression FromDouble(double? value)
        {
            if (!value.HasValue)
            {
                return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Double);
            }
            return DbExpressionBuilder.Constant(value.Value);
        }

        public static implicit operator DbExpression(double? value)
        {
            return FromDouble(value);
        }

        /// <summary>
        ///     Creates a <see cref="DbExpression" /> that represents the specified <see cref="DbGeography" /> value, which may be null.
        /// </summary>
        /// <param name="value"> The DbGeography value on which the returned expression should be based </param>
        /// <returns>
        ///     A <see cref="DbExpression" /> that represents the specified DbGeography value
        /// </returns>
        public static DbExpression FromGeography(DbGeography value)
        {
            if (value == null)
            {
                return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Geography);
            }
            return DbExpressionBuilder.Constant(value);
        }

        public static implicit operator DbExpression(DbGeography value)
        {
            return FromGeography(value);
        }

        /// <summary>
        ///     Creates a <see cref="DbExpression" /> that represents the specified <see cref="DbGeometry" /> value, which may be null.
        /// </summary>
        /// <param name="value"> The DbGeometry value on which the returned expression should be based </param>
        /// <returns>
        ///     A <see cref="DbExpression" /> that represents the specified DbGeometry value
        /// </returns>
        public static DbExpression FromGeometry(DbGeometry value)
        {
            if (value == null)
            {
                return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Geometry);
            }
            return DbExpressionBuilder.Constant(value);
        }

        public static implicit operator DbExpression(DbGeometry value)
        {
            return FromGeometry(value);
        }

        /// <summary>
        ///     Creates a <see cref="DbExpression" /> that represents the specified (nullable) <see cref="Guid" /> value
        /// </summary>
        /// <param name="value"> The Guid value on which the returned expression should be based </param>
        /// <returns>
        ///     A <see cref="DbExpression" /> that represents the specified Guid value
        /// </returns>
        public static DbExpression FromGuid(Guid? value)
        {
            if (!value.HasValue)
            {
                return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Guid);
            }
            return DbExpressionBuilder.Constant(value.Value);
        }

        public static implicit operator DbExpression(Guid? value)
        {
            return FromGuid(value);
        }

        /// <summary>
        ///     Creates a <see cref="DbExpression" /> that represents the specified (nullable) Int16 value
        /// </summary>
        /// <param name="value"> The Int16 value on which the returned expression should be based </param>
        /// <returns>
        ///     A <see cref="DbExpression" /> that represents the specified Int16 value
        /// </returns>
        public static DbExpression FromInt16(short? value)
        {
            if (!value.HasValue)
            {
                return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Int16);
            }
            return DbExpressionBuilder.Constant(value.Value);
        }

        public static implicit operator DbExpression(short? value)
        {
            return FromInt16(value);
        }

        /// <summary>
        ///     Creates a <see cref="DbExpression" /> that represents the specified (nullable) Int32 value
        /// </summary>
        /// <param name="value"> The Int32 value on which the returned expression should be based </param>
        /// <returns>
        ///     A <see cref="DbExpression" /> that represents the specified Int32 value
        /// </returns>
        public static DbExpression FromInt32(int? value)
        {
            if (!value.HasValue)
            {
                return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Int32);
            }
            return DbExpressionBuilder.Constant(value.Value);
        }

        public static implicit operator DbExpression(int? value)
        {
            return FromInt32(value);
        }

        /// <summary>
        ///     Creates a <see cref="DbExpression" /> that represents the specified (nullable) Int64 value
        /// </summary>
        /// <param name="value"> The Int64 value on which the returned expression should be based </param>
        /// <returns>
        ///     A <see cref="DbExpression" /> that represents the specified Int64 value
        /// </returns>
        public static DbExpression FromInt64(long? value)
        {
            if (!value.HasValue)
            {
                return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Int64);
            }
            return DbExpressionBuilder.Constant(value.Value);
        }

        public static implicit operator DbExpression(long? value)
        {
            return FromInt64(value);
        }

        /// <summary>
        ///     Creates a <see cref="DbExpression" /> that represents the specified (nullable) Single value
        /// </summary>
        /// <param name="value"> The Single value on which the returned expression should be based </param>
        /// <returns>
        ///     A <see cref="DbExpression" /> that represents the specified Single value
        /// </returns>
        public static DbExpression FromSingle(float? value)
        {
            if (!value.HasValue)
            {
                return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Single);
            }
            return DbExpressionBuilder.Constant(value.Value);
        }

        public static implicit operator DbExpression(float? value)
        {
            return FromSingle(value);
        }

        /// <summary>
        ///     Creates a <see cref="DbExpression" /> that represents the specified string value
        /// </summary>
        /// <param name="value"> The string value on which the returned expression should be based </param>
        /// <returns>
        ///     A <see cref="DbExpression" /> that represents the specified string value
        /// </returns>
        public static DbExpression FromString(string value)
        {
            if (null == value)
            {
                return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.String);
            }
            return DbExpressionBuilder.Constant(value);
        }

        public static implicit operator DbExpression(string value)
        {
            return FromString(value);
        }

        #endregion

        #region Internal API

        internal static void CheckExpressionKind(DbExpressionKind kind)
        {
            // Add new valid DbExpressionKind values to this method as well as the enum itself.
            // DbExpressionKind is a contiguous enum from All = 0 through View            
            if ((kind < DbExpressionKind.All)
                || (DbExpressionKind.Lambda < kind))
            {
                var paramName = typeof(DbExpressionKind).Name;
                throw new ArgumentOutOfRangeException(
                    paramName, Strings.ADP_InvalidEnumerationValue(paramName, ((int)kind).ToString(CultureInfo.InvariantCulture)));
            }
        }

        #endregion
    }
}
