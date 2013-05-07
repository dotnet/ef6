// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    /// <summary>
    ///     Used to configure a primitive property of an entity type or complex type.
    ///     This configuration functionality is available via lightweight conventions.
    /// </summary>
    public class LightweightPrimitivePropertyConfiguration
    {
        private readonly PropertyInfo _propertyInfo;
        private readonly Func<PrimitivePropertyConfiguration> _configuration;
        private readonly Lazy<BinaryPropertyConfiguration> _binaryConfiguration;
        private readonly Lazy<DateTimePropertyConfiguration> _dateTimeConfiguration;
        private readonly Lazy<DecimalPropertyConfiguration> _decimalConfiguration;
        private readonly Lazy<LengthPropertyConfiguration> _lengthConfiguration;
        private readonly Lazy<StringPropertyConfiguration> _stringConfiguration;
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="LightweightPrimitivePropertyConfiguration" /> class.
        /// </summary>
        /// <param name="propertyInfo">
        ///     The <see cref="PropertyInfo" /> for this property
        /// </param>
        /// <param name="configuration"> The configuration object that this instance wraps. </param>
        internal LightweightPrimitivePropertyConfiguration(PropertyInfo propertyInfo, Func<PrimitivePropertyConfiguration> configuration)
        {
            DebugCheck.NotNull(propertyInfo);
            DebugCheck.NotNull(configuration);

            _propertyInfo = propertyInfo;
            _configuration = configuration;
            _binaryConfiguration = new Lazy<BinaryPropertyConfiguration>(
                () => _configuration() as BinaryPropertyConfiguration);
            _dateTimeConfiguration = new Lazy<DateTimePropertyConfiguration>(
                () => _configuration() as DateTimePropertyConfiguration);
            _decimalConfiguration = new Lazy<DecimalPropertyConfiguration>(
                () => _configuration() as DecimalPropertyConfiguration);
            _lengthConfiguration = new Lazy<LengthPropertyConfiguration>(
                () => _configuration() as LengthPropertyConfiguration);
            _stringConfiguration = new Lazy<StringPropertyConfiguration>(
                () => _configuration() as StringPropertyConfiguration);
        }

        /// <summary>
        ///     Gets the <see cref="PropertyInfo" /> for this property.
        /// </summary>
        public virtual PropertyInfo ClrPropertyInfo
        {
            get { return _propertyInfo; }
        }

        internal Func<PrimitivePropertyConfiguration> Configuration
        {
            get { return _configuration; }
        }

        /// <summary>
        ///     Configures the name of the database column used to store the property.
        /// </summary>
        /// <param name="columnName"> The name of the column. </param>
        /// <returns>
        ///     The same <see cref="LightweightPrimitivePropertyConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured.
        /// </remarks>
        public virtual LightweightPrimitivePropertyConfiguration HasColumnName(string columnName)
        {
            Check.NotEmpty(columnName, "columnName");

            if (_configuration() != null
                && _configuration().ColumnName == null)
            {
                _configuration().ColumnName = columnName;
            }

            return this;
        }

        public virtual LightweightPrimitivePropertyConfiguration HasParameterName(string parameterName)
        {
            Check.NotEmpty(parameterName, "parameterName");

            if (_configuration() != null
                && _configuration().ParameterName == null)
            {
                _configuration().ParameterName = parameterName;
            }

            return this;
        }

        /// <summary>
        ///     Configures the order of the database column used to store the property.
        ///     This method is also used to specify key ordering when an entity type has a composite key.
        /// </summary>
        /// <param name="columnOrder"> The order that this column should appear in the database table. </param>
        /// <returns>
        ///     The same <see cref="LightweightPrimitivePropertyConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured.
        /// </remarks>
        public virtual LightweightPrimitivePropertyConfiguration HasColumnOrder(int columnOrder)
        {
            if (columnOrder < 0)
            {
                throw new ArgumentOutOfRangeException("columnOrder");
            }

            if (_configuration() != null
                && _configuration().ColumnOrder == null)
            {
                _configuration().ColumnOrder = columnOrder;
            }

            return this;
        }

        /// <summary>
        ///     Configures the data type of the database column used to store the property.
        /// </summary>
        /// <param name="columnType"> Name of the database provider specific data type. </param>
        /// <returns>
        ///     The same <see cref="LightweightPrimitivePropertyConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured.
        /// </remarks>
        public virtual LightweightPrimitivePropertyConfiguration HasColumnType(string columnType)
        {
            Check.NotEmpty(columnType, "columnType");

            if (_configuration() != null
                && _configuration().ColumnType == null)
            {
                _configuration().ColumnType = columnType;
            }

            return this;
        }

        /// <summary>
        ///     Configures the property to be used as an optimistic concurrency token.
        /// </summary>
        /// <returns>
        ///     The same <see cref="LightweightPrimitivePropertyConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured.
        /// </remarks>
        public virtual LightweightPrimitivePropertyConfiguration IsConcurrencyToken()
        {
            return IsConcurrencyToken(true);
        }

        /// <summary>
        ///     Configures whether or not the property is to be used as an optimistic concurrency token.
        /// </summary>
        /// <param name="concurrencyToken"> Value indicating if the property is a concurrency token or not. </param>
        /// <returns>
        ///     The same <see cref="LightweightPrimitivePropertyConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured.
        /// </remarks>
        public virtual LightweightPrimitivePropertyConfiguration IsConcurrencyToken(bool concurrencyToken)
        {
            if (_configuration() != null
                && _configuration().ConcurrencyMode == null)
            {
                _configuration().ConcurrencyMode = concurrencyToken
                                                       ? ConcurrencyMode.Fixed
                                                       : ConcurrencyMode.None;
            }

            return this;
        }

        /// <summary>
        ///     Configures how values for the property are generated by the database.
        /// </summary>
        /// <param name="databaseGeneratedOption"> The pattern used to generate values for the property in the database. </param>
        /// <returns>
        ///     The same <see cref="LightweightPrimitivePropertyConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured.
        /// </remarks>
        public virtual LightweightPrimitivePropertyConfiguration HasDatabaseGeneratedOption(
            DatabaseGeneratedOption databaseGeneratedOption)
        {
            if (!Enum.IsDefined(typeof(DatabaseGeneratedOption), databaseGeneratedOption))
            {
                throw new ArgumentOutOfRangeException("databaseGeneratedOption");
            }

            if (_configuration() != null
                && _configuration().DatabaseGeneratedOption == null)
            {
                _configuration().DatabaseGeneratedOption = databaseGeneratedOption;
            }

            return this;
        }

        /// <summary>
        ///     Configures the property to be optional.
        ///     The database column used to store this property will be nullable.
        /// </summary>
        /// <returns>
        ///     The same <see cref="LightweightPrimitivePropertyConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured.
        /// </remarks>
        public virtual LightweightPrimitivePropertyConfiguration IsOptional()
        {
            if (_configuration() != null
                && _configuration().IsNullable == null)
            {
                if (!_propertyInfo.PropertyType.IsNullable())
                {
                    throw new InvalidOperationException(Strings.LightweightPrimitivePropertyConfiguration_NonNullableProperty(
                        _propertyInfo.DeclaringType + "." + _propertyInfo.Name,
                        _propertyInfo.PropertyType));
                }

                _configuration().IsNullable = true;
            }

            return this;
        }

        /// <summary>
        ///     Configures the property to be required.
        ///     The database column used to store this property will be non-nullable.
        /// </summary>
        /// <returns>
        ///     The same <see cref="LightweightPrimitivePropertyConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured.
        /// </remarks>
        public virtual LightweightPrimitivePropertyConfiguration IsRequired()
        {
            if (_configuration() != null
                && _configuration().IsNullable == null)
            {
                _configuration().IsNullable = false;
            }

            return this;
        }

        /// <summary>
        ///     Configures the property to support Unicode string content.
        /// </summary>
        /// <returns>
        ///     The same <see cref="LightweightPrimitivePropertyConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured or if the
        ///     property is not a <see cref="String" />.
        /// </remarks>
        public virtual LightweightPrimitivePropertyConfiguration IsUnicode()
        {
            return IsUnicode(true);
        }

        /// <summary>
        ///     Configures whether or not the property supports Unicode string content.
        /// </summary>
        /// <param name="unicode"> Value indicating if the property supports Unicode string content or not. </param>
        /// <returns>
        ///     The same <see cref="LightweightPrimitivePropertyConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured or if the
        ///     property is not a <see cref="String" />.
        /// </remarks>
        public virtual LightweightPrimitivePropertyConfiguration IsUnicode(bool unicode)
        {
            if (_stringConfiguration.Value != null
                && _stringConfiguration.Value.IsUnicode == null)
            {
                _stringConfiguration.Value.IsUnicode = unicode;
            }

            return this;
        }

        /// <summary>
        ///     Configures the property to be fixed length.
        ///     Use HasMaxLength to set the length that the property is fixed to.
        /// </summary>
        /// <returns>
        ///     The same <see cref="LightweightPrimitivePropertyConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured or if the
        ///     property does not have length facets.
        /// </remarks>
        public virtual LightweightPrimitivePropertyConfiguration IsFixedLength()
        {
            if (_lengthConfiguration.Value != null
                && _lengthConfiguration.Value.IsFixedLength == null)
            {
                _lengthConfiguration.Value.IsFixedLength = true;
            }

            return this;
        }

        /// <summary>
        ///     Configures the property to be variable length.
        ///     Properties are variable length by default.
        /// </summary>
        /// <returns>
        ///     The same <see cref="LightweightPrimitivePropertyConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured or if the
        ///     property does not have length facets.
        /// </remarks>
        public virtual LightweightPrimitivePropertyConfiguration IsVariableLength()
        {
            if (_lengthConfiguration.Value != null
                && _lengthConfiguration.Value.IsFixedLength == null)
            {
                _lengthConfiguration.Value.IsFixedLength = false;
            }

            return this;
        }

        /// <summary>
        ///     Configures the property to have the specified maximum length.
        /// </summary>
        /// <param name="maxLength"> The maximum length for the property. </param>
        /// <returns>
        ///     The same <see cref="LightweightPrimitivePropertyConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured or if the
        ///     property does not have length facets.
        /// </remarks>
        public virtual LightweightPrimitivePropertyConfiguration HasMaxLength(int maxLength)
        {
            if (maxLength < 1)
            {
                throw new ArgumentOutOfRangeException("maxLength");
            }

            if (_lengthConfiguration.Value != null
                && _lengthConfiguration.Value.MaxLength == null
                && _lengthConfiguration.Value.IsMaxLength == null)
            {
                _lengthConfiguration.Value.MaxLength = maxLength;

                if (_lengthConfiguration.Value.IsFixedLength == null)
                {
                    _lengthConfiguration.Value.IsFixedLength = false;
                }

                if (_stringConfiguration.Value != null
                    && _stringConfiguration.Value.IsUnicode == null)
                {
                    _stringConfiguration.Value.IsUnicode = true;
                }
            }

            return this;
        }

        /// <summary>
        ///     Configures the property to allow the maximum length supported by the database provider.
        /// </summary>
        /// <returns>
        ///     The same <see cref="LightweightPrimitivePropertyConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured or if the
        ///     property does not have length facets.
        /// </remarks>
        public virtual LightweightPrimitivePropertyConfiguration IsMaxLength()
        {
            if (_lengthConfiguration.Value != null
                && _lengthConfiguration.Value.IsMaxLength == null
                && _lengthConfiguration.Value.MaxLength == null)
            {
                _lengthConfiguration.Value.IsMaxLength = true;
            }

            return this;
        }

        /// <summary>
        ///     Configures the precision of the <see cref="DateTime" /> property.
        ///     If the database provider does not support precision for the data type of the column then the value is ignored.
        /// </summary>
        /// <param name="value"> Precision of the property. </param>
        /// <returns>
        ///     The same <see cref="LightweightPrimitivePropertyConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured or if the
        ///     property is not a <see cref="DateTime" />.
        /// </remarks>
        public virtual LightweightPrimitivePropertyConfiguration HasPrecision(byte value)
        {
            if (_dateTimeConfiguration.Value != null
                && _dateTimeConfiguration.Value.Precision == null)
            {
                _dateTimeConfiguration.Value.Precision = value;
            }

            return this;
        }

        /// <summary>
        ///     Configures the precision and scale of the <see cref="Decimal" /> property.
        /// </summary>
        /// <param name="precision"> The precision of the property. </param>
        /// <param name="scale"> The scale of the property. </param>
        /// <returns>
        ///     The same <see cref="LightweightPrimitivePropertyConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured or if the
        ///     property is not a <see cref="Decimal" />.
        /// </remarks>
        public virtual LightweightPrimitivePropertyConfiguration HasPrecision(byte precision, byte scale)
        {
            if (_decimalConfiguration.Value != null
                && _decimalConfiguration.Value.Precision == null
                && _decimalConfiguration.Value.Scale == null)
            {
                _decimalConfiguration.Value.Precision = precision;
                _decimalConfiguration.Value.Scale = scale;
            }

            return this;
        }

        /// <summary>
        ///     Configures the property to be a row version in the database.
        ///     The actual data type will vary depending on the database provider being used.
        ///     Setting the property to be a row version will automatically configure it to be an
        ///     optimistic concurrency token.
        /// </summary>
        /// <returns>
        ///     The same <see cref="LightweightPrimitivePropertyConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured or if the
        ///     property is not a <see cref="T:Byte[]" />.
        /// </remarks>
        public virtual LightweightPrimitivePropertyConfiguration IsRowVersion()
        {
            if (_binaryConfiguration.Value != null
                && _binaryConfiguration.Value.IsRowVersion == null)
            {
                _binaryConfiguration.Value.IsRowVersion = true;
            }

            return this;
        }

        /// <summary>
        ///     Configures this property to be part of the entity type's primary key.
        /// </summary>
        /// <returns>
        ///     The same <see cref="LightweightPrimitivePropertyConfiguration" /> instance so that
        ///     multiple calls can be chained.
        /// </returns>
        public virtual LightweightPrimitivePropertyConfiguration IsKey()
        {
            if (_configuration() != null)
            {
                var entityTypeConfig = _configuration().TypeConfiguration as EntityTypeConfiguration;

                if (entityTypeConfig != null
                    && !entityTypeConfig.IsKeyConfigured)
                {
                    entityTypeConfig.Key(ClrPropertyInfo);
                }
            }

            return this;
        }

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
