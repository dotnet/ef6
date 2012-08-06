// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.ModelConfiguration.Configuration.Mapping;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    ///     Configures the table and column mapping for an entity type or a sub-set of properties from an entity type.
    ///     This configuration functionality is available via the Code First Fluent API, see <see cref = "DbModelBuilder" />.
    /// </summary>
    /// <typeparam name = "TEntityType">The entity type to be mapped.</typeparam>
    public class EntityMappingConfiguration<TEntityType>
        where TEntityType : class
    {
        private readonly EntityMappingConfiguration _entityMappingConfiguration;

        public EntityMappingConfiguration()
            : this(new EntityMappingConfiguration())
        {
        }

        internal EntityMappingConfiguration(EntityMappingConfiguration entityMappingConfiguration)
        {
            Contract.Requires(entityMappingConfiguration != null);

            _entityMappingConfiguration = entityMappingConfiguration;
        }

        internal EntityMappingConfiguration EntityMappingConfigurationInstance
        {
            get { return _entityMappingConfiguration; }
        }

        /// <summary>
        ///     Configures the properties that will be included in this mapping fragment.
        ///     If this method is not called then all properties that have not yet been 
        ///     included in a mapping fragment will be configured.
        /// </summary>
        /// <typeparam name = "TObject">An anonymous type including the properties to be mapped.</typeparam>
        /// <param name = "propertiesExpression">
        ///     A lambda expression to an anonymous type that contains the properties to be mapped.
        ///     C#: t => new { t.Id, t.Property1, t.Property2 }
        ///     VB.Net: Function(t) New With { p.Id, t.Property1, t.Property2 }
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void Properties<TObject>(Expression<Func<TEntityType, TObject>> propertiesExpression)
        {
            Contract.Requires(propertiesExpression != null);

            _entityMappingConfiguration.Properties
                = propertiesExpression.GetComplexPropertyAccessList().ToList();
        }

        /// <summary>
        ///     Re-maps all properties inherited from base types.
        /// 
        ///     When configuring a derived type to be mapped to a separate table this will cause all properties to 
        ///     be included in the table rather than just the non-inherited properties. This is known as
        ///     Table per Concrete Type (TPC) mapping.
        /// </summary>
        public void MapInheritedProperties()
        {
            _entityMappingConfiguration.MapInheritedProperties = true;
        }

        /// <summary>
        ///     Configures the table name to be mapped to.
        /// </summary>
        /// <param name = "tableName">Name of the table.</param>
        public void ToTable(string tableName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(tableName));

            var databaseName = DatabaseName.Parse(tableName);

            ToTable(databaseName.Name, databaseName.Schema);
        }

        /// <summary>
        ///     Configures the table name and schema to be mapped to.
        /// </summary>
        /// <param name = "tableName">Name of the table.</param>
        /// <param name = "schemaName">Schema of the table.</param>
        public void ToTable(string tableName, string schemaName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(tableName));

            _entityMappingConfiguration.TableName = new DatabaseName(tableName, schemaName);
        }

        /// <summary>
        ///     Configures the discriminator column used to differentiate between types in an inheritance hierarchy.
        /// </summary>
        /// <param name = "discriminator">The name of the discriminator column.</param>
        /// <returns>A configuration object to further configure the discriminator column and values.</returns>
        public ValueConditionConfiguration Requires(string discriminator)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(discriminator));

            return new ValueConditionConfiguration(_entityMappingConfiguration, discriminator);
        }

        /// <summary>
        ///     Configures the discriminator condition used to differentiate between types in an inheritance hierarchy.
        /// </summary>
        /// <typeparam name = "TProperty">The type of the property being used to discriminate between types.</typeparam>
        /// <param name = "property">
        ///     A lambda expression representing the property being used to discriminate between types.
        ///     C#: t => t.MyProperty   
        ///     VB.Net: Function(t) t.MyProperty
        /// </param>
        /// <returns>A configuration object to further configure the discriminator condition.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public NotNullConditionConfiguration Requires<TProperty>(Expression<Func<TEntityType, TProperty>> property)
        {
            Contract.Requires(property != null);

            return new NotNullConditionConfiguration(_entityMappingConfiguration, property.GetComplexPropertyAccess());
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
