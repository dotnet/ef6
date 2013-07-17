// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Services;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
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

        private Properties.Primitive.PrimitivePropertyConfiguration _configuration;

        internal ValueConditionConfiguration(EntityMappingConfiguration entityMapConfiguration, string discriminator)
        {
            DebugCheck.NotNull(entityMapConfiguration);
            DebugCheck.NotEmpty(discriminator);

            _entityMappingConfiguration = entityMapConfiguration;

            Discriminator = discriminator;
        }

        private ValueConditionConfiguration(EntityMappingConfiguration owner, ValueConditionConfiguration source)
        {
            DebugCheck.NotNull(source);

            _entityMappingConfiguration = owner;

            Discriminator = source.Discriminator;
            Value = source.Value;

            _configuration
                = (source._configuration == null)
                      ? null
                      : source._configuration.Clone();
        }

        internal virtual ValueConditionConfiguration Clone(EntityMappingConfiguration owner)
        {
            return new ValueConditionConfiguration(owner, this);
        }

        private T GetOrCreateConfiguration<T>() where T : Properties.Primitive.PrimitivePropertyConfiguration, new()
        {
            if (_configuration == null)
            {
                _configuration = new T();
            }
            else if (!(_configuration is T))
            {
                var newConfig = new T();

                newConfig.CopyFrom(_configuration);

                _configuration = newConfig;
            }

            _configuration.OverridableConfigurationParts = OverridableConfigurationParts.None;

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
                    GetOrCreateConfiguration<Properties.Primitive.PrimitivePropertyConfiguration>());
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
                    GetOrCreateConfiguration<Properties.Primitive.PrimitivePropertyConfiguration>());
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
            PrimitiveType edmType;

            if (value != null
                && !value.GetType().IsPrimitiveType(out edmType))
            {
                throw Error.InvalidDiscriminatorType(value.GetType().Name);
            }
        }

        internal static bool AnyBaseTypeToTableWithoutColumnCondition(
            DbDatabaseMapping databaseMapping, EntityType entityType, EntityType table,
            EdmProperty column)
        {
            var baseType = entityType.BaseType;

            while (baseType != null)
            {
                if (!baseType.Abstract)
                {
                    var baseTypeTableFragments
                        = databaseMapping.GetEntityTypeMappings((EntityType)baseType)
                                         .SelectMany(etm => etm.MappingFragments)
                                         .Where(tmf => tmf.Table == table)
                                         .ToList();

                    if (baseTypeTableFragments.Any()
                        && baseTypeTableFragments
                               .SelectMany(etmf => etmf.ColumnConditions)
                               .All(cc => cc.ColumnProperty != column))
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
            StorageMappingFragment fragment,
            EntityType entityType,
            DbProviderManifest providerManifest)
        {
            DebugCheck.NotNull(fragment);
            DebugCheck.NotNull(providerManifest);

            var discriminatorColumn
                = fragment.Table.Properties
                          .SingleOrDefault(c => string.Equals(c.Name, Discriminator, StringComparison.Ordinal));

            if (discriminatorColumn == null)
            {
                var typeUsage
                    = providerManifest.GetStoreType(DatabaseMappingGenerator.DiscriminatorTypeUsage);

                discriminatorColumn
                    = new EdmProperty(Discriminator, typeUsage)
                        {
                            Nullable = false
                        };

                TablePrimitiveOperations.AddColumn(fragment.Table, discriminatorColumn);
            }

            if (AnyBaseTypeToTableWithoutColumnCondition(
                databaseMapping, entityType, fragment.Table, discriminatorColumn))
            {
                discriminatorColumn.Nullable = true;
            }

            var existingConfiguration
                = discriminatorColumn.GetConfiguration() as Properties.Primitive.PrimitivePropertyConfiguration;

            if (Value != null)
            {
                ConfigureColumnType(providerManifest, existingConfiguration, discriminatorColumn);

                fragment.AddDiscriminatorCondition(discriminatorColumn, Value);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(discriminatorColumn.TypeName))
                {
                    var typeUsage
                        = providerManifest.GetStoreType(DatabaseMappingGenerator.DiscriminatorTypeUsage);

                    discriminatorColumn.PrimitiveType = (PrimitiveType)typeUsage.EdmType;
                    discriminatorColumn.MaxLength = DatabaseMappingGenerator.DiscriminatorMaxLength;
                    discriminatorColumn.Nullable = false;
                }

                GetOrCreateConfiguration<Properties.Primitive.PrimitivePropertyConfiguration>().IsNullable = true;

                fragment.AddNullabilityCondition(discriminatorColumn, true);
            }

            if (_configuration == null)
            {
                return;
            }

            if (existingConfiguration != null)
            {
                string errorMessage;
                if ((existingConfiguration.OverridableConfigurationParts &
                     OverridableConfigurationParts.OverridableInCSpace) !=
                    OverridableConfigurationParts.OverridableInCSpace
                    && !existingConfiguration.IsCompatible(
                        _configuration, inCSpace: true, errorMessage: out errorMessage))
                {
                    throw Error.ConflictingColumnConfiguration(discriminatorColumn, fragment.Table, errorMessage);
                }
            }

            if (_configuration.IsNullable != null)
            {
                discriminatorColumn.Nullable = _configuration.IsNullable.Value;
            }

            _configuration.Configure(discriminatorColumn, fragment.Table, providerManifest);
        }

        private void ConfigureColumnType(
            DbProviderManifest providerManifest,
            Properties.Primitive.PrimitivePropertyConfiguration existingConfiguration,
            EdmProperty discriminatorColumn)
        {
            if (((existingConfiguration != null)
                 && existingConfiguration.ColumnType != null)
                || ((_configuration != null)
                    && (_configuration.ColumnType != null)))
            {
                return;
            }

            PrimitiveType primitiveType;

            Value.GetType().IsPrimitiveType(out primitiveType);

            var edmType
                = (PrimitiveType)providerManifest.GetStoreType(
                    (primitiveType == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))
                        ? DatabaseMappingGenerator.DiscriminatorTypeUsage
                        : TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(primitiveType.PrimitiveTypeKind))).EdmType;

            if ((existingConfiguration != null)
                && !discriminatorColumn.TypeName.Equals(edmType.Name, StringComparison.OrdinalIgnoreCase))
            {
                throw Error.ConflictingInferredColumnType(
                    discriminatorColumn.Name, discriminatorColumn.TypeName, edmType.Name);
            }

            discriminatorColumn.PrimitiveType = edmType;
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
