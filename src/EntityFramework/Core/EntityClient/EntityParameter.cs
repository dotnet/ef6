// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityClient
{
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Metadata.Edm.Provider;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// Class representing a parameter used in EntityCommand
    /// </summary>
    public class EntityParameter : DbParameter, IDbDataParameter
    {
        private string _parameterName;
        private DbType? _dbType;
        private EdmType _edmType;
        private byte? _precision;
        private byte? _scale;
        private bool _isDirty;

        private object _value;
        private object _parent;
        private ParameterDirection _direction;
        private int? _size;
        private string _sourceColumn;
        private DataRowVersion _sourceVersion;
        private bool _sourceColumnNullMapping;
        private bool? _isNullable;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> class using the default values.
        /// </summary>
        public EntityParameter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> class using the specified parameter name and data type.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="dbType">
        /// One of the <see cref="T:System.Data.DbType" /> values.
        /// </param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EntityParameter(string parameterName, DbType dbType)
        {
            SetParameterNameWithValidation(parameterName, "parameterName");
            DbType = dbType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> class using the specified parameter name, data type and size.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="dbType">
        /// One of the <see cref="T:System.Data.DbType" /> values.
        /// </param>
        /// <param name="size">The size of the parameter.</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EntityParameter(string parameterName, DbType dbType, int size)
        {
            SetParameterNameWithValidation(parameterName, "parameterName");
            DbType = dbType;
            Size = size;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> class using the specified properties.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="dbType">
        /// One of the <see cref="T:System.Data.DbType" /> values.
        /// </param>
        /// <param name="size">The size of the parameter.</param>
        /// <param name="sourceColumn">The name of the source column.</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EntityParameter(string parameterName, DbType dbType, int size, string sourceColumn)
        {
            SetParameterNameWithValidation(parameterName, "parameterName");
            DbType = dbType;
            Size = size;
            SourceColumn = sourceColumn;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> class using the specified properties.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="dbType">
        /// One of the <see cref="T:System.Data.DbType" /> values.
        /// </param>
        /// <param name="size">The size of the parameter.</param>
        /// <param name="direction">
        /// One of the <see cref="T:System.Data.ParameterDirection" /> values.
        /// </param>
        /// <param name="isNullable">true to indicate that the parameter accepts null values; otherwise, false.</param>
        /// <param name="precision">The number of digits used to represent the value.</param>
        /// <param name="scale">The number of decimal places to which value is resolved.</param>
        /// <param name="sourceColumn">The name of the source column.</param>
        /// <param name="sourceVersion">
        /// One of the <see cref="T:System.Data.DataRowVersion" /> values.
        /// </param>
        /// <param name="value">The value of the parameter.</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EntityParameter(
            string parameterName,
            DbType dbType,
            int size,
            ParameterDirection direction,
            bool isNullable,
            byte precision,
            byte scale,
            string sourceColumn,
            DataRowVersion sourceVersion,
            object value)
        {
            SetParameterNameWithValidation(parameterName, "parameterName");
            DbType = dbType;
            Size = size;
            Direction = direction;
            IsNullable = isNullable;
            Precision = precision;
            Scale = scale;
            SourceColumn = sourceColumn;
            SourceVersion = sourceVersion;
            Value = value;
        }

        private EntityParameter(EntityParameter source)
            : this()
        {
            DebugCheck.NotNull(source);

            source.CloneHelper(this);

            var cloneable = (_value as ICloneable);
            if (null != cloneable)
            {
                _value = cloneable.Clone();
            }
        }

        /// <summary>Gets or sets the name of the entity parameter.</summary>
        /// <returns>The name of the entity parameter.</returns>
        public override string ParameterName
        {
            get { return _parameterName ?? ""; }
            set { SetParameterNameWithValidation(value, "value"); }
        }

        /// <summary>
        /// Helper method to validate the parameter name; Ideally we'd only call this once, but
        /// we have to put an argumentName on the Argument exception, and the property setter would
        /// need "value" which confuses folks when they call the constructor that takes the value
        /// of the parameter.  c'est la vie.
        /// </summary>
        private void SetParameterNameWithValidation(string parameterName, string argumentName)
        {
            if (!string.IsNullOrEmpty(parameterName)
                && !DbCommandTree.IsValidParameterName(parameterName))
            {
                throw new ArgumentException(Strings.EntityClient_InvalidParameterName(parameterName), argumentName);
            }

            PropertyChanging();
            _parameterName = parameterName;
        }

        /// <summary>
        /// Gets or sets the <see cref="T:System.Data.DbType" /> of the parameter.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Data.DbType" /> values.
        /// </returns>
        public override DbType DbType
        {
            get
            {
                // if the user has not set the dbType but has set the dbType, use the edmType to try to deduce a dbType
                if (!_dbType.HasValue)
                {
                    if (_edmType != null)
                    {
                        return GetDbTypeFromEdm(_edmType);
                    }
                    else
                    {
                        // If the user has set neither the DbType nor the EdmType, 
                        // then we attempt to deduce it from the value, but we won't set it in the
                        // member field as that's used to keep track of what the user set explicitly
                        // If we can't deduce the type because there are no values, we still have to return something, 
                        // just assume it's string type
                        if (_value == null)
                        {
                            return DbType.String;
                        }

                        try
                        {
                            return TypeHelpers.ConvertClrTypeToDbType(_value.GetType());
                        }
                        catch (ArgumentException e)
                        {
                            throw new InvalidOperationException(Strings.EntityClient_CannotDeduceDbType, e);
                        }
                    }
                }

                return (DbType)_dbType;
            }
            set
            {
                PropertyChanging();
                _dbType = value;
            }
        }

        /// <summary>Gets or sets the type of the parameter, expressed as an EdmType.</summary>
        /// <returns>The type of the parameter, expressed as an EdmType.</returns>
        public virtual EdmType EdmType
        {
            get { return _edmType; }
            set
            {
                if (value != null
                    && !Helper.IsScalarType(value))
                {
                    throw new InvalidOperationException(Strings.EntityClient_EntityParameterEdmTypeNotScalar(value.FullName));
                }

                PropertyChanging();
                _edmType = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of digits used to represent the
        /// <see
        ///     cref="P:System.Data.Entity.Core.EntityClient.EntityParameter.Value" />
        /// property.
        /// </summary>
        /// <returns>The number of digits used to represent the value.</returns>
        public virtual byte Precision
        {
            get
            {
                var result = _precision.HasValue ? _precision.Value : (byte)0;
                return result;
            }
            set
            {
                PropertyChanging();
                _precision = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of decimal places to which
        /// <see
        ///     cref="P:System.Data.Entity.Core.EntityClient.EntityParameter.Value" />
        /// is resolved.
        /// </summary>
        /// <returns>The number of decimal places to which value is resolved.</returns>
        public virtual byte Scale
        {
            get
            {
                var result = _scale.HasValue ? _scale.Value : (byte)0;
                return result;
            }
            set
            {
                PropertyChanging();
                _scale = value;
            }
        }

        /// <summary>Gets or sets the value of the parameter.</summary>
        /// <returns>The value of the parameter.</returns>
        public override object Value
        {
            get { return _value; }
            set
            {
                // If the user hasn't set the DbType, then we have to figure out if the DbType will change as a result
                // of the change in the value.  What we want to achieve is that changes to the value will not cause
                // it to be dirty, but changes to the value that causes the apparent DbType to change, then should be
                // dirty.
                if (!_dbType.HasValue
                    && _edmType == null)
                {
                    // If the value is null, then we assume it's string type
                    var oldDbType = DbType.String;
                    if (_value != null)
                    {
                        oldDbType = TypeHelpers.ConvertClrTypeToDbType(_value.GetType());
                    }

                    // If the value is null, then we assume it's string type
                    var newDbType = DbType.String;
                    if (value != null)
                    {
                        newDbType = TypeHelpers.ConvertClrTypeToDbType(value.GetType());
                    }

                    if (oldDbType != newDbType)
                    {
                        PropertyChanging();
                    }
                }

                _value = value;
            }
        }

        /// <summary>
        /// Gets whether this collection has been changes since the last reset
        /// </summary>
        internal virtual bool IsDirty
        {
            get { return _isDirty; }
        }

        /// <summary>
        /// Indicates whether the DbType property has been set by the user;
        /// </summary>
        internal virtual bool IsDbTypeSpecified
        {
            get { return _dbType.HasValue; }
        }

        /// <summary>
        /// Indicates whether the Direction property has been set by the user;
        /// </summary>
        internal virtual bool IsDirectionSpecified
        {
            get { return _direction != 0; }
        }

        /// <summary>
        /// Indicates whether the IsNullable property has been set by the user;
        /// </summary>
        internal virtual bool IsIsNullableSpecified
        {
            get { return _isNullable.HasValue; }
        }

        /// <summary>
        /// Indicates whether the Precision property has been set by the user;
        /// </summary>
        internal virtual bool IsPrecisionSpecified
        {
            get { return _precision.HasValue; }
        }

        /// <summary>
        /// Indicates whether the Scale property has been set by the user;
        /// </summary>
        internal virtual bool IsScaleSpecified
        {
            get { return _scale.HasValue; }
        }

        /// <summary>
        /// Indicates whether the Size property has been set by the user;
        /// </summary>
        internal virtual bool IsSizeSpecified
        {
            get { return _size.HasValue; }
        }

        /// <summary>Gets or sets the direction of the parameter.</summary>
        /// <returns>
        /// One of the <see cref="T:System.Data.ParameterDirection" /> values.
        /// </returns>
        [RefreshProperties(RefreshProperties.All)]
        [EntityResCategory(EntityRes.DataCategory_Data)]
        [EntityResDescription(EntityRes.DbParameter_Direction)]
        public override ParameterDirection Direction
        {
            get
            {
                var direction = _direction;
                return ((0 != direction) ? direction : ParameterDirection.Input);
            }
            set
            {
                if (_direction != value)
                {
                    switch (value)
                    {
                        case ParameterDirection.Input:
                        case ParameterDirection.Output:
                        case ParameterDirection.InputOutput:
                        case ParameterDirection.ReturnValue:
                            PropertyChanging();
                            _direction = value;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(
                                typeof(ParameterDirection).Name,
                                Strings.ADP_InvalidEnumerationValue(
                                    typeof(ParameterDirection).Name, ((int)value).ToString(CultureInfo.InvariantCulture)));
                    }
                }
            }
        }

        /// <summary>Gets or sets a value that indicates whether the parameter accepts null values.</summary>
        /// <returns>true if null values are accepted; otherwise, false.</returns>
        public override bool IsNullable
        {
            get
            {
                var result = _isNullable.HasValue ? _isNullable.Value : true;
                return result;
            }
            set { _isNullable = value; }
        }

        /// <summary>Gets or sets the maximum size of the data within the column.</summary>
        /// <returns>The maximum size of the data within the column.</returns>
        [EntityResCategory(EntityRes.DataCategory_Data)]
        [EntityResDescription(EntityRes.DbParameter_Size)]
        public override int Size
        {
            get
            {
                var size = _size.HasValue ? _size.Value : 0;
                if (0 == size)
                {
                    size = ValueSize(Value);
                }

                return size;
            }
            set
            {
                if (!_size.HasValue
                    || _size.Value != value)
                {
                    if (value < -1)
                    {
                        throw new ArgumentException(Strings.ADP_InvalidSizeValue(value.ToString(CultureInfo.InvariantCulture)));
                    }

                    PropertyChanging();
                    if (0 == value)
                    {
                        _size = null;
                    }
                    else
                    {
                        _size = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the source column mapped to the <see cref="T:System.Data.DataSet" /> and used for loading or returning the
        /// <see
        ///     cref="P:System.Data.Entity.Core.EntityClient.EntityParameter.Value" />
        /// .
        /// </summary>
        /// <returns>The name of the source column mapped to the dataset and used for loading or returning the value.</returns>
        [EntityResCategory(EntityRes.DataCategory_Update)]
        [EntityResDescription(EntityRes.DbParameter_SourceColumn)]
        public override string SourceColumn
        {
            get
            {
                var sourceColumn = _sourceColumn;
                return ((null != sourceColumn) ? sourceColumn : string.Empty);
            }
            set { _sourceColumn = value; }
        }

        /// <summary>Gets or sets a value that indicates whether source column is nullable.</summary>
        /// <returns>true if source column is nullable; otherwise, false.</returns>
        public override bool SourceColumnNullMapping
        {
            get { return _sourceColumnNullMapping; }
            set { _sourceColumnNullMapping = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="T:System.Data.DataRowVersion" /> to use when loading the value.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Data.DataRowVersion" /> values.
        /// </returns>
        [EntityResCategory(EntityRes.DataCategory_Update)]
        [EntityResDescription(EntityRes.DbParameter_SourceVersion)]
        public override DataRowVersion SourceVersion
        {
            get
            {
                var sourceVersion = _sourceVersion;
                return ((0 != sourceVersion) ? sourceVersion : DataRowVersion.Current);
            }
            set
            {
                switch (value)
                {
                    case DataRowVersion.Original:
                    case DataRowVersion.Current:
                    case DataRowVersion.Proposed:
                    case DataRowVersion.Default:
                        _sourceVersion = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            typeof(DataRowVersion).Name,
                            Strings.ADP_InvalidEnumerationValue(
                                typeof(DataRowVersion).Name, ((int)value).ToString(CultureInfo.InvariantCulture)));
                }
            }
        }

        /// <summary>
        /// Resets the type associated with the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" />.
        /// </summary>
        public override void ResetDbType()
        {
            if (_dbType != null
                || _edmType != null)
            {
                PropertyChanging();
            }

            _edmType = null;
            _dbType = null;
        }

        /// <summary>
        /// Marks that this parameter has been changed
        /// </summary>
        private void PropertyChanging()
        {
            _isDirty = true;
        }

        /// <summary>
        /// Determines the size of the given object
        /// </summary>
        private static int ValueSize(object value)
        {
            return ValueSizeCore(value);
        }

        /// <summary>
        /// Clones this parameter object
        /// </summary>
        /// <returns> The new cloned object </returns>
        internal virtual EntityParameter Clone()
        {
            return new EntityParameter(this);
        }

        /// <summary>
        /// Clones this parameter object
        /// </summary>
        private void CloneHelper(EntityParameter destination)
        {
            destination._value = _value;

            destination._direction = _direction;
            destination._size = _size;

            destination._sourceColumn = _sourceColumn;
            destination._sourceVersion = _sourceVersion;
            destination._sourceColumnNullMapping = _sourceColumnNullMapping;
            destination._isNullable = _isNullable;

            destination._parameterName = _parameterName;
            destination._dbType = _dbType;
            destination._edmType = _edmType;
            destination._precision = _precision;
            destination._scale = _scale;
        }

        /// <summary>
        /// Get the type usage for this parameter in model terms.
        /// </summary>
        /// <returns> The type usage for this parameter </returns>
        /// <remarks>
        /// Because GetTypeUsage throws CommandValidationExceptions, it should only be called from EntityCommand during command execution
        /// </remarks>
        internal virtual TypeUsage GetTypeUsage()
        {
            TypeUsage typeUsage;
            if (!IsTypeConsistent)
            {
                throw new InvalidOperationException(
                    Strings.EntityClient_EntityParameterInconsistentEdmType(
                        _edmType.FullName, _parameterName));
            }

            if (_edmType != null)
            {
                typeUsage = TypeUsage.Create(_edmType);
            }
            else if (!DbTypeMap.TryGetModelTypeUsage(DbType, out typeUsage))
            {
                // Spatial types have only DbType 'Object', and cannot be represented in the static type map.
                PrimitiveType primitiveParameterType;
                if (DbType == DbType.Object
                    && Value != null
                    && ClrProviderManifest.Instance.TryGetPrimitiveType(Value.GetType(), out primitiveParameterType)
                    && Helper.IsSpatialType(primitiveParameterType))
                {
                    typeUsage = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(primitiveParameterType.PrimitiveTypeKind);
                }
                else
                {
                    throw new InvalidOperationException(Strings.EntityClient_UnsupportedDbType(DbType.ToString(), ParameterName));
                }
            }

            Debug.Assert(typeUsage != null, "DbType.TryGetModelTypeUsage returned true for null TypeUsage?");
            return typeUsage;
        }

        /// <summary>
        /// Reset the dirty flag on the collection
        /// </summary>
        internal virtual void ResetIsDirty()
        {
            _isDirty = false;
        }

        private bool IsTypeConsistent
        {
            get
            {
                if (_edmType != null
                    && _dbType.HasValue)
                {
                    var dbType = GetDbTypeFromEdm(_edmType);
                    if (dbType == DbType.String)
                    {
                        // would need facets to distinguish the various sorts of string, 
                        // a generic string EdmType is consistent with any string DbType.
                        return _dbType == DbType.String || _dbType == DbType.AnsiString
                               || dbType == DbType.AnsiStringFixedLength || dbType == DbType.StringFixedLength;
                    }
                    else
                    {
                        return _dbType == dbType;
                    }
                }

                return true;
            }
        }

        private static DbType GetDbTypeFromEdm(EdmType edmType)
        {
            var primitiveType = Helper.AsPrimitive(edmType);
            DbType dbType;
            if (Helper.IsSpatialType(primitiveType))
            {
                return DbType.Object;
            }
            else if (DbCommandDefinition.TryGetDbTypeFromPrimitiveType(primitiveType, out dbType))
            {
                return dbType;
            }

            // we shouldn't ever get here.   Assert in a debug build, and pick a type.
            Debug.Assert(false, "The provided edmType is of an unknown primitive type.");
            return default(DbType);
        }

        private void ResetSize()
        {
            if (_size.HasValue)
            {
                PropertyChanging();
                _size = null;
            }
        }

        private bool ShouldSerializeSize()
        {
            return (_size.HasValue && _size.Value != 0);
        }

        internal virtual void CopyTo(DbParameter destination)
        {
            DebugCheck.NotNull(destination);
            CloneHelper((EntityParameter)destination);
        }

        internal virtual object CompareExchangeParent(object value, object comparand)
        {
            var parent = _parent;
            if (comparand == parent)
            {
                _parent = value;
            }

            return parent;
        }

        internal virtual void ResetParent()
        {
            _parent = null;
        }

        /// <summary>Returns a string representation of the parameter.</summary>
        /// <returns>A string representation of the parameter.</returns>
        public override string ToString()
        {
            return ParameterName;
        }

        private static int ValueSizeCore(object value)
        {
            if (!EntityUtil.IsNull(value))
            {
                var svalue = value as string;
                if (null != svalue)
                {
                    return svalue.Length;
                }

                var bvalue = value as byte[];
                if (null != bvalue)
                {
                    return bvalue.Length;
                }

                var cvalue = value as char[];
                if (null != cvalue)
                {
                    return cvalue.Length;
                }

                if (value is byte
                    || value is char)
                {
                    return 1;
                }
            }

            return 0;
        }
    }
}
