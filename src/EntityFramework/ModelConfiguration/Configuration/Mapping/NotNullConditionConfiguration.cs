// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    ///     Configures a condition used to discriminate between types in an inheritance hierarchy based on the values assigned to a property.
    ///     This configuration functionality is available via the Code First Fluent API, see <see cref="DbModelBuilder" />.
    /// </summary>
    public class NotNullConditionConfiguration
    {
        private readonly EntityMappingConfiguration _entityMappingConfiguration;

        internal PropertyPath PropertyPath { get; set; }

        internal NotNullConditionConfiguration(
            EntityMappingConfiguration entityMapConfiguration, PropertyPath propertyPath)
        {
            DebugCheck.NotNull(entityMapConfiguration);
            DebugCheck.NotNull(propertyPath);

            _entityMappingConfiguration = entityMapConfiguration;
            PropertyPath = propertyPath;
        }

        private NotNullConditionConfiguration(EntityMappingConfiguration owner, NotNullConditionConfiguration source)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(owner);

            _entityMappingConfiguration = owner;
            PropertyPath = source.PropertyPath;
        }

        internal virtual NotNullConditionConfiguration Clone(EntityMappingConfiguration owner)
        {
            return new NotNullConditionConfiguration(owner, this);
        }

        /// <summary>
        ///     Configures the condition to require a value in the property.
        ///     Rows that do not have a value assigned to column that this property is stored in are
        ///     assumed to be of the base type of this entity type.
        /// </summary>
        public void HasValue()
        {
            _entityMappingConfiguration.AddNullabilityCondition(this);
        }

        internal void Configure(
            DbDatabaseMapping databaseMapping, StorageMappingFragment fragment, EntityType entityType)
        {
            DebugCheck.NotNull(fragment);

            var edmPropertyPath = EntityMappingConfiguration.PropertyPathToEdmPropertyPath(PropertyPath, entityType);

            if (edmPropertyPath.Count() > 1)
            {
                throw Error.InvalidNotNullCondition(PropertyPath.ToString(), entityType.Name);
            }

            var column
                = fragment.ColumnMappings
                          .Where(pm => pm.PropertyPath.SequenceEqual(edmPropertyPath.Single()))
                          .Select(pm => pm.ColumnProperty)
                          .SingleOrDefault();

            if (column == null
                || !fragment.Table.Properties.Contains(column))
            {
                throw Error.InvalidNotNullCondition(PropertyPath.ToString(), entityType.Name);
            }

            if (ValueConditionConfiguration.AnyBaseTypeToTableWithoutColumnCondition(
                databaseMapping, entityType, fragment.Table, column))
            {
                column.Nullable = true;
            }

            // Make the property required
            var newConfiguration = new PrimitivePropertyConfiguration
                {
                    IsNullable = false,
                    OverridableConfigurationParts =
                        OverridableConfigurationParts.OverridableInSSpace
                };

            newConfiguration.Configure(edmPropertyPath.Single().Last());

            fragment.AddNullabilityCondition(column, isNull: false);
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
