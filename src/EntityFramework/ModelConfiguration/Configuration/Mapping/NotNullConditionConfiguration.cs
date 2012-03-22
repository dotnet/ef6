namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     Configures a condition used to discriminate between types in an inheritance hierarchy based on the values assigned to a property.
    ///     This configuration functionality is available via the Code First Fluent API, see <see cref = "DbModelBuilder" />.
    /// </summary>
    public class NotNullConditionConfiguration
    {
        private readonly EntityMappingConfiguration _entityMappingConfiguration;

        internal PropertyPath PropertyPath { get; set; }

        internal NotNullConditionConfiguration(
            EntityMappingConfiguration entityMapConfiguration, PropertyPath propertyPath)
        {
            Contract.Requires(entityMapConfiguration != null);
            Contract.Requires(propertyPath != null);

            _entityMappingConfiguration = entityMapConfiguration;
            PropertyPath = propertyPath;
        }

        private NotNullConditionConfiguration(EntityMappingConfiguration owner, NotNullConditionConfiguration source)
        {
            Contract.Requires(source != null);
            Contract.Requires(owner != null);

            _entityMappingConfiguration = owner;
            PropertyPath = source.PropertyPath;
        }

        internal virtual NotNullConditionConfiguration Clone(EntityMappingConfiguration owner)
        {
            return new NotNullConditionConfiguration(owner, this);
        }

        /// <summary>
        ///     Configures the condition to require a value in the property.
        /// 
        ///     Rows that do not have a value assigned to column that this property is stored in are 
        ///     assumed to be of the base type of this entity type.
        /// </summary>
        public void HasValue()
        {
            _entityMappingConfiguration.AddNullabilityCondition(this);
        }

        internal void Configure(
            DbDatabaseMapping databaseMapping, DbEntityTypeMappingFragment fragment, EdmEntityType entityType)
        {
            Contract.Requires(fragment != null);

            var edmPropertyPath = EntityMappingConfiguration.PropertyPathToEdmPropertyPath(PropertyPath, entityType);

            if (edmPropertyPath.Count() > 1)
            {
                throw Error.InvalidNotNullCondition(PropertyPath.ToString(), entityType.Name);
            }

            var column
                = fragment.PropertyMappings
                    .Where(pm => pm.PropertyPath.SequenceEqual(edmPropertyPath.Single()))
                    .Select(pm => pm.Column)
                    .SingleOrDefault();

            if (column == null
                || !fragment.Table.Columns.Contains(column))
            {
                throw Error.InvalidNotNullCondition(PropertyPath.ToString(), entityType.Name);
            }

            if (ValueConditionConfiguration.AnyBaseTypeToTableWithoutColumnCondition(
                databaseMapping, entityType, fragment.Table, column))
            {
                column.IsNullable = true;
            }

            // Make the property required
            var newConfiguration = new Properties.Primitive.PrimitivePropertyConfiguration
                                       {
                                           IsNullable = false,
                                           OverridableConfigurationParts =
                                               OverridableConfigurationParts.OverridableInSSpace
                                       };

            newConfiguration.Configure(edmPropertyPath.Single().Last());

            fragment.AddNullabilityCondition(column, isNull: false);
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
