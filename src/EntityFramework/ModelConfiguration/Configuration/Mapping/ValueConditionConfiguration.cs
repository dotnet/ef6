// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm.Services;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     Configures a discriminator column used to differentiate between types in an inheritance hierarchy.
    ///     This configuration functionality is available via the Code First Fluent API, see <see cref="DbModelBuilder" />.
    /// </summary>
    [DebuggerDisplay("{Discriminator}")]
    public class ValueConditionConfiguration
    {
        private readonly EntityMappingConfiguration _entityMappingConfiguration;

        internal string Discriminator { get; set; }
        internal object Value { get; set; }
        private PrimitivePropertyConfiguration _configuration;

        internal ValueConditionConfiguration(EntityMappingConfiguration entityMapConfiguration, string discriminator)
        {
            Contract.Requires(entityMapConfiguration != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(discriminator));

            _entityMappingConfiguration = entityMapConfiguration;
            Discriminator = discriminator;
        }

        private ValueConditionConfiguration(EntityMappingConfiguration owner, ValueConditionConfiguration source)
        {
            Contract.Requires(source != null);

            _entityMappingConfiguration = owner;

            Discriminator = source.Discriminator;
            Value = source.Value;
            _configuration = source._configuration == null ? null : source._configuration.Clone();
        }

        internal virtual ValueConditionConfiguration Clone(EntityMappingConfiguration owner)
        {
            return new ValueConditionConfiguration(owner, this);
        }

        private T GetOrCreateConfiguration<T>() where T : PrimitivePropertyConfiguration, new()
        {
            if (_configuration == null)
            {
                _configuration = new T();
            }
            else if (!typeof(T).IsAssignableFrom(_configuration.GetType()))
            {
                var newConfig = new T();
                newConfig.CopyFrom(_configuration);
                _configuration = newConfig;
            }
            _configuration.OverridableConfigurationParts =
                OverridableConfigurationParts.None;
            return (T)_configuration;
        }

        /// <summary>
        ///     Configures the discriminator value used to identify the entity type being 
        ///     configured from other types in the inheritance hierarchy.
        /// </summary>
        /// <typeparam name="T"> Type of the discriminator value. </typeparam>
        /// <param name="value"> The value to be used to identify the entity type. </param>
        /// <returns> A configuration object to configure the column used to store discriminator values. </returns>
        public PrimitiveColumnConfiguration HasValue<T>(T value)
            where T : struct
        {
            ValidateValueType(value);
            Value = value;
            _entityMappingConfiguration.AddValueCondition(this);
            return
                new PrimitiveColumnConfiguration(
                    GetOrCreateConfiguration<PrimitivePropertyConfiguration>());
        }

        /// <summary>
        ///     Configures the discriminator value used to identify the entity type being 
        ///     configured from other types in the inheritance hierarchy.
        /// </summary>
        /// <typeparam name="T"> Type of the discriminator value. </typeparam>
        /// <param name="value"> The value to be used to identify the entity type. </param>
        /// <returns> A configuration object to configure the column used to store discriminator values. </returns>
        public PrimitiveColumnConfiguration HasValue<T>(T? value)
            where T : struct
        {
            ValidateValueType(value);
            Value = value;
            _entityMappingConfiguration.AddValueCondition(this);
            return
                new PrimitiveColumnConfiguration(
                    GetOrCreateConfiguration<PrimitivePropertyConfiguration>());
        }

        /// <summary>
        ///     Configures the discriminator value used to identify the entity type being 
        ///     configured from other types in the inheritance hierarchy.
        /// </summary>
        /// <param name="value"> The value to be used to identify the entity type. </param>
        /// <returns> A configuration object to configure the column used to store discriminator values. </returns>
        public StringColumnConfiguration HasValue(string value)
        {
            Value = value;
            _entityMappingConfiguration.AddValueCondition(this);
            return
                new StringColumnConfiguration(
                    GetOrCreateConfiguration<Properties.Primitive.StringPropertyConfiguration>());
        }

        private static void ValidateValueType(object value)
        {
            EdmPrimitiveType edmType;
            if (value != null
                && !value.GetType().IsPrimitiveType(out edmType))
            {
                throw Error.InvalidDiscriminatorType(value.GetType().Name);
            }
        }

        internal static bool AnyBaseTypeToTableWithoutColumnCondition(
            DbDatabaseMapping databaseMapping, EdmEntityType entityType, DbTableMetadata table,
            DbTableColumnMetadata column)
        {
            var baseType = entityType.BaseType;
            while (baseType != null)
            {
                if (!baseType.IsAbstract)
                {
                    var baseTypeTableFragments = databaseMapping.GetEntityTypeMappings(baseType)
                        .SelectMany(etm => etm.TypeMappingFragments)
                        .Where(tmf => tmf.Table == table);
                    if (baseTypeTableFragments.Any()
                        && !baseTypeTableFragments.SelectMany(etmf => etmf.ColumnConditions)
                                .Any(cc => cc.Column == column))
                    {
                        return true;
                    }
                }
                baseType = baseType.BaseType;
            }
            return false;
        }

        internal void Configure(
            DbDatabaseMapping databaseMapping,
            DbEntityTypeMappingFragment fragment,
            EdmEntityType entityType,
            DbProviderManifest providerManifest)
        {
            Contract.Requires(fragment != null);
            Contract.Requires(providerManifest != null);

            var discriminatorColumn = TablePrimitiveOperations.IncludeColumn(fragment.Table, Discriminator, true);

            Contract.Assert(
                discriminatorColumn.TypeName == null || !string.IsNullOrWhiteSpace(discriminatorColumn.TypeName));

            if (AnyBaseTypeToTableWithoutColumnCondition(
                databaseMapping, entityType, fragment.Table, discriminatorColumn))
            {
                discriminatorColumn.IsNullable = true;
            }

            var existingConfiguration = discriminatorColumn.GetConfiguration()
                                        as PrimitivePropertyConfiguration;

            if (Value != null)
            {
                if ((existingConfiguration == null ||
                     existingConfiguration.ColumnType == null)
                    &&
                    (_configuration == null ||
                     _configuration.ColumnType == null))
                {
                    EdmPrimitiveType primitiveType;
                    Value.GetType().IsPrimitiveType(out primitiveType);

                    string inferredTypeName;
                    if (primitiveType == EdmPrimitiveType.String)
                    {
                        inferredTypeName = providerManifest.GetStoreType(
                            TypeUsage.CreateStringTypeUsage(
                                PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String),
                                isUnicode: true,
                                isFixedLength: false,
                                maxLength: DatabaseMappingGenerator.DiscriminatorLength)).EdmType.Name;

                        discriminatorColumn.Facets.MaxLength = DatabaseMappingGenerator.DiscriminatorLength;
                    }
                    else
                    {
                        inferredTypeName =
                            providerManifest.GetStoreTypeName((PrimitiveTypeKind)primitiveType.PrimitiveTypeKind);
                    }

                    if (string.IsNullOrWhiteSpace(discriminatorColumn.TypeName)
                        ||
                        discriminatorColumn.TypeName.Equals(inferredTypeName, StringComparison.OrdinalIgnoreCase))
                    {
                        discriminatorColumn.TypeName = inferredTypeName;
                    }
                    else
                    {
                        throw Error.ConflictingInferredColumnType(
                            discriminatorColumn.Name, discriminatorColumn.TypeName, inferredTypeName);
                    }
                }

                fragment.AddDiscriminatorCondition(discriminatorColumn, Value);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(discriminatorColumn.TypeName))
                {
                    new DatabaseMappingGenerator(providerManifest).InitializeDefaultDiscriminatorColumn(
                        discriminatorColumn);
                }

                GetOrCreateConfiguration<PrimitivePropertyConfiguration>().IsNullable = true;
                fragment.AddNullabilityCondition(discriminatorColumn, true);
            }

            if (_configuration != null)
            {
                if (existingConfiguration != null)
                {
                    string errorMessage;
                    if ((existingConfiguration.OverridableConfigurationParts &
                         OverridableConfigurationParts.OverridableInCSpace) !=
                        OverridableConfigurationParts.OverridableInCSpace
                        &&
                        !existingConfiguration.IsCompatible(
                            _configuration, inCSpace: true, errorMessage: out errorMessage))
                    {
                        throw Error.ConflictingColumnConfiguration(discriminatorColumn, fragment.Table, errorMessage);
                    }
                }

                if (_configuration.IsNullable != null)
                {
                    discriminatorColumn.IsNullable = _configuration.IsNullable.Value;
                }

                _configuration.Configure(discriminatorColumn, fragment.Table, providerManifest);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
