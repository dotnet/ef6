// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    /// <summary>
    ///     Used to configure a primitive property of an entity type or complex type. 
    ///     This configuration functionality is available via lightweight conventions.
    /// </summary>
    public class LightweightPropertyConfiguration
    {
        private readonly PropertyInfo _propertyInfo;
        private readonly Func<PrimitivePropertyConfiguration> _configuration;
        private readonly Lazy<BinaryPropertyConfiguration> _binaryConfiguration;
        private readonly Lazy<DateTimePropertyConfiguration> _dateTimeConfiguration;
        private readonly Lazy<DecimalPropertyConfiguration> _decimalConfiguration;
        private readonly Lazy<LengthPropertyConfiguration> _lengthConfiguration;
        private readonly Lazy<StringPropertyConfiguration> _stringConfiguration;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LightweightPropertyConfiguration" /> class.
        /// </summary>
        /// <param name="propertyInfo"> The <see cref="PropertyInfo" /> for this property </param>
        /// <param name="configuration"> The configuration object that this instance wraps. </param>
        public LightweightPropertyConfiguration(PropertyInfo propertyInfo, Func<PrimitivePropertyConfiguration> configuration)
        {
            Contract.Requires(propertyInfo != null);
            Contract.Requires(configuration != null);

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
        public PropertyInfo ClrPropertyInfo
        {
            get { return _propertyInfo; }
        }

        /// <summary>
        ///     Gets or sets the name of the database column used to store the property.
        /// </summary>
        /// <remarks>
        ///     Setting this will have no effect once it has been configured.
        /// </remarks>
        public string ColumnName
        {
            get { return _configuration().ColumnName; }
            set
            {
                if (_configuration().ColumnName == null)
                {
                    _configuration().ColumnName = value;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the order of the database column used to store the property.
        /// </summary>
        /// <remarks>
        ///     Setting this will have no effect once it has been configured.
        /// </remarks>
        public int? ColumnOrder
        {
            get { return _configuration().ColumnOrder; }
            set
            {
                if (_configuration().ColumnOrder == null)
                {
                    _configuration().ColumnOrder = value;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the type of the database column used to store the property.
        /// </summary>
        /// <remarks>
        ///     Setting this will have no effect once it has been configured.
        /// </remarks>
        public string ColumnType
        {
            get { return _configuration().ColumnType; }
            set
            {
                if (_configuration().ColumnType == null)
                {
                    _configuration().ColumnType = value;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the concurrency mode to use for the property.
        /// </summary>
        /// <remarks>
        ///     Setting this will have no effect once it has been configured.
        /// </remarks>
        public ConcurrencyMode? ConcurrencyMode
        {
            get { return _configuration().ConcurrencyMode; }
            set
            {
                if (_configuration().ConcurrencyMode == null)
                {
                    _configuration().ConcurrencyMode = value;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the pattern used to generate values in the database for the
        ///     property.
        /// </summary>
        /// <remarks>
        ///     Setting this will have no effect once it has been configured.
        /// </remarks>
        public DatabaseGeneratedOption? DatabaseGeneratedOption
        {
            get { return _configuration().DatabaseGeneratedOption; }
            set
            {
                if (_configuration().DatabaseGeneratedOption == null)
                {
                    _configuration().DatabaseGeneratedOption = value;
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the property is optional.
        /// </summary>
        /// <remarks>
        ///     Setting this will have no effect once it has been configured.
        /// </remarks>
        public bool? IsNullable
        {
            get { return _configuration().IsNullable; }
            set
            {
                if (_configuration().IsNullable == null)
                {
                    _configuration().IsNullable = value;
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the property supports Unicode string
        ///     content.
        /// </summary>
        /// <remarks>
        ///     Setting this will have no effect once it has been configured or if the
        ///     property is not a <see cref="String" />.
        /// </remarks>
        public bool? IsUnicode
        {
            get
            {
                return _stringConfiguration.Value != null
                           ? _stringConfiguration.Value.IsUnicode
                           : null;
            }
            set
            {
                if (_stringConfiguration.Value != null
                    && _stringConfiguration.Value.IsUnicode == null)
                {
                    _stringConfiguration.Value.IsUnicode = value;
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the property is fixed length.
        /// </summary>
        /// <remarks>
        ///     Setting this will have no effect once it has been configured or if the
        ///     property does not have length facets.
        /// </remarks>
        public bool? IsFixedLength
        {
            get
            {
                return _lengthConfiguration.Value != null
                           ? _lengthConfiguration.Value.IsFixedLength
                           : null;
            }
            set
            {
                if (_lengthConfiguration.Value != null
                    && _lengthConfiguration.Value.IsFixedLength == null)
                {
                    _lengthConfiguration.Value.IsFixedLength = value;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the maximum length of the property.
        /// </summary>
        /// <remarks>
        ///     Setting this will have no effect once it has been configured or if the
        ///     property does not have length facets.
        /// </remarks>
        public int? MaxLength
        {
            get
            {
                return _lengthConfiguration.Value != null
                           ? _lengthConfiguration.Value.MaxLength
                           : null;
            }
            set
            {
                if (_lengthConfiguration.Value != null
                    && _lengthConfiguration.Value.MaxLength == null)
                {
                    _lengthConfiguration.Value.MaxLength = value;
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the property allows the maximum
        ///     length supported by the database provider.
        /// </summary>
        /// <remarks>
        ///     Setting this will have no effect once it has been configured or if the
        ///     property does not have length facets.
        /// </remarks>
        public bool? IsMaxLength
        {
            get
            {
                return _lengthConfiguration.Value != null
                           ? _lengthConfiguration.Value.IsMaxLength
                           : null;
            }
            set
            {
                if (_lengthConfiguration.Value != null
                    && _lengthConfiguration.Value.IsMaxLength == null)
                {
                    _lengthConfiguration.Value.IsMaxLength = value;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the scale of the property.
        /// </summary>
        /// <remarks>
        ///     Setting this will have no effect once it has been configured or if the
        ///     property is not a <see cref="Decimal" />.
        /// </remarks>
        public byte? Scale
        {
            get
            {
                return _decimalConfiguration.Value != null
                           ? _decimalConfiguration.Value.Scale
                           : null;
            }
            set
            {
                if (_decimalConfiguration.Value != null
                    && _decimalConfiguration.Value.Scale == null)
                {
                    _decimalConfiguration.Value.Scale = value;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the precision of the property.
        /// </summary>
        /// <remarks>
        ///     Setting this will have no effect once it has been configured or if the
        ///     property is not a <see cref="DateTime" /> or <see cref="Decimal" />.
        /// </remarks>
        public byte? Precision
        {
            get
            {
                if (_decimalConfiguration.Value != null)
                {
                    return _decimalConfiguration.Value.Precision;
                }

                if (_dateTimeConfiguration.Value != null)
                {
                    return _dateTimeConfiguration.Value.Precision;
                }

                return null;
            }
            set
            {
                if (_decimalConfiguration.Value != null
                    && _decimalConfiguration.Value.Precision == null)
                {
                    _decimalConfiguration.Value.Precision = value;
                }
                else if (_dateTimeConfiguration.Value != null
                         && _dateTimeConfiguration.Value.Precision == null)
                {
                    _dateTimeConfiguration.Value.Precision = value;
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the property is a row version in the
        ///     database.
        /// </summary>
        /// <remarks>
        ///     Setting this will have no effect once it has been configured or if the
        ///     property is not a <see cref="T:Byte[]" />.
        /// </remarks>
        public bool? IsRowVersion
        {
            get
            {
                return _binaryConfiguration.Value != null
                           ? _binaryConfiguration.Value.IsRowVersion
                           : null;
            }
            set
            {
                if (_binaryConfiguration.Value != null
                    && _binaryConfiguration.Value.IsRowVersion == null)
                {
                    _binaryConfiguration.Value.IsRowVersion = value;
                }
            }
        }

        /// <summary>
        ///     Configures this property to be part of the entity type's primary key.
        /// </summary>
        public void IsKey()
        {
            var entityTypeConfig = _configuration().TypeConfiguration as EntityTypeConfiguration;

            if (entityTypeConfig != null)
            {
                entityTypeConfig.Key(ClrPropertyInfo);
            }
        }

        /// <summary>
        ///     Configures this property to be part of the entity type's primary key.
        /// </summary>
        /// <param name="columnOrder"> The order of the database column. This is useful when specifying a composite primary key. </param>
        public void IsKey(int columnOrder)
        {
            Contract.Requires(columnOrder >= 0);

            IsKey();
            ColumnOrder = columnOrder;
        }
    }
}
