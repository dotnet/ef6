// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Configuration.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Configures the table and column mapping for an entity type or a sub-set of properties from an entity type.
    /// This configuration functionality is available via the Code First Fluent API, see <see cref="DbModelBuilder" />.
    /// </summary>
    /// <typeparam name="TEntityType"> The entity type to be mapped. </typeparam>
    public class EntityMappingConfiguration<TEntityType>
        where TEntityType : class
    {
        private readonly EntityMappingConfiguration _entityMappingConfiguration;

        /// <summary>Initializes a new instance of the <see cref="T:System.Data.Entity.ModelConfiguration.Configuration.EntityMappingConfiguration`1" /> class.</summary>
        public EntityMappingConfiguration()
            : this(new EntityMappingConfiguration())
        {
        }

        internal EntityMappingConfiguration(EntityMappingConfiguration entityMappingConfiguration)
        {
            DebugCheck.NotNull(entityMappingConfiguration);

            _entityMappingConfiguration = entityMappingConfiguration;
        }

        internal EntityMappingConfiguration EntityMappingConfigurationInstance
        {
            get { return _entityMappingConfiguration; }
        }

        /// <summary>
        /// Configures the properties that will be included in this mapping fragment.
        /// If this method is not called then all properties that have not yet been
        /// included in a mapping fragment will be configured.
        /// </summary>
        /// <typeparam name="TObject"> An anonymous type including the properties to be mapped. </typeparam>
        /// <param name="propertiesExpression"> A lambda expression to an anonymous type that contains the properties to be mapped. C#: t => new { t.Id, t.Property1, t.Property2 } VB.Net: Function(t) New With { p.Id, t.Property1, t.Property2 } </param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void Properties<TObject>(Expression<Func<TEntityType, TObject>> propertiesExpression)
        {
            Check.NotNull(propertiesExpression, "propertiesExpression");

            _entityMappingConfiguration.Properties
                = propertiesExpression.GetComplexPropertyAccessList().ToList();
        }

        /// <summary>
        /// Configures a <see cref="T:System.struct" /> property that is included in this mapping fragment.
        /// </summary>
        /// <typeparam name="T"> The type of the property being configured. </typeparam>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public PropertyMappingConfiguration Property<T>(
            Expression<Func<TEntityType, T>> propertyExpression)
            where T : struct
        {
            return new PropertyMappingConfiguration(
                Property<Properties.Primitive.PrimitivePropertyConfiguration>(propertyExpression));
        }

        /// <summary>
        /// Configures a <see cref="T:System.struct?" /> property that is included in this mapping fragment.
        /// </summary>
        /// <typeparam name="T"> The type of the property being configured. </typeparam>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public PropertyMappingConfiguration Property<T>(
            Expression<Func<TEntityType, T?>> propertyExpression)
            where T : struct
        {
            return new PropertyMappingConfiguration(
                Property<Properties.Primitive.PrimitivePropertyConfiguration>(propertyExpression));
        }

        /// <summary>
        /// Configures a <see cref="T:DbGeometry" /> property that is included in this mapping fragment.
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public PropertyMappingConfiguration Property(
            Expression<Func<TEntityType, DbGeometry>> propertyExpression)
        {
            return new PropertyMappingConfiguration(
                Property<Properties.Primitive.PrimitivePropertyConfiguration>(propertyExpression));
        }

        /// <summary>
        /// Configures a <see cref="T:DbGeography" /> property that is included in this mapping fragment.
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public PropertyMappingConfiguration Property(
            Expression<Func<TEntityType, DbGeography>> propertyExpression)
        {
            return new PropertyMappingConfiguration(
                Property<Properties.Primitive.PrimitivePropertyConfiguration>(propertyExpression));
        }

        /// <summary>
        /// Configures a <see cref="T:System.string" /> property that is included in this mapping fragment.
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public PropertyMappingConfiguration Property(Expression<Func<TEntityType, string>> propertyExpression)
        {
            return new PropertyMappingConfiguration(
                Property<Properties.Primitive.StringPropertyConfiguration>(propertyExpression));
        }

        /// <summary>
        /// Configures a <see cref="T:System.byte[]" /> property that is included in this mapping fragment.
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public PropertyMappingConfiguration Property(Expression<Func<TEntityType, byte[]>> propertyExpression)
        {
            return new PropertyMappingConfiguration(
                Property<Properties.Primitive.BinaryPropertyConfiguration>(propertyExpression));
        }

        /// <summary>
        /// Configures a <see cref="T:System.decimal" /> property that is included in this mapping fragment.
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public PropertyMappingConfiguration Property(Expression<Func<TEntityType, decimal>> propertyExpression)
        {
            return new PropertyMappingConfiguration(
                Property<Properties.Primitive.DecimalPropertyConfiguration>(propertyExpression));
        }

        /// <summary>
        /// Configures a <see cref="T:System.decimal?" /> property that is included in this mapping fragment.
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public PropertyMappingConfiguration Property(Expression<Func<TEntityType, decimal?>> propertyExpression)
        {
            return new PropertyMappingConfiguration(
                Property<Properties.Primitive.DecimalPropertyConfiguration>(propertyExpression));
        }

        /// <summary>
        /// Configures a <see cref="T:System.DateTime" /> property that is included in this mapping fragment.
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public PropertyMappingConfiguration Property(Expression<Func<TEntityType, DateTime>> propertyExpression)
        {
            return new PropertyMappingConfiguration(
                Property<Properties.Primitive.DateTimePropertyConfiguration>(propertyExpression));
        }

        /// <summary>
        /// Configures a <see cref="T:System.DateTime?" /> property that is included in this mapping fragment.
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public PropertyMappingConfiguration Property(Expression<Func<TEntityType, DateTime?>> propertyExpression)
        {
            return new PropertyMappingConfiguration(
                Property<Properties.Primitive.DateTimePropertyConfiguration>(propertyExpression));
        }

        /// <summary>
        /// Configures a <see cref="T:System.DateTimeOffset" /> property that is included in this mapping fragment.
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public PropertyMappingConfiguration Property(
            Expression<Func<TEntityType, DateTimeOffset>> propertyExpression)
        {
            return new PropertyMappingConfiguration(
                Property<Properties.Primitive.DateTimePropertyConfiguration>(propertyExpression));
        }

        /// <summary>
        /// Configures a <see cref="T:System.DateTimeOffset?" /> property that is included in this mapping fragment.
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public PropertyMappingConfiguration Property(
            Expression<Func<TEntityType, DateTimeOffset?>> propertyExpression)
        {
            return new PropertyMappingConfiguration(
                Property<Properties.Primitive.DateTimePropertyConfiguration>(propertyExpression));
        }

        /// <summary>
        /// Configures a <see cref="T:System.TimeSpan" /> property that is included in this mapping fragment.
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public PropertyMappingConfiguration Property(Expression<Func<TEntityType, TimeSpan>> propertyExpression)
        {
            return new PropertyMappingConfiguration(
                Property<Properties.Primitive.DateTimePropertyConfiguration>(propertyExpression));
        }

        /// <summary>
        /// Configures a <see cref="T:System.TimeSpan?" /> property that is included in this mapping fragment.
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public PropertyMappingConfiguration Property(Expression<Func<TEntityType, TimeSpan?>> propertyExpression)
        {
            return new PropertyMappingConfiguration(
                Property<Properties.Primitive.DateTimePropertyConfiguration>(propertyExpression));
        }

        internal TPrimitivePropertyConfiguration Property<TPrimitivePropertyConfiguration>(
            LambdaExpression lambdaExpression)
            where TPrimitivePropertyConfiguration : Properties.Primitive.PrimitivePropertyConfiguration, new()
        {
            return _entityMappingConfiguration.Property(
                lambdaExpression.GetComplexPropertyAccess(),
                () => new TPrimitivePropertyConfiguration
                {
                    OverridableConfigurationParts = OverridableConfigurationParts.None
                });
        }

        /// <summary>
        /// Re-maps all properties inherited from base types.
        /// When configuring a derived type to be mapped to a separate table this will cause all properties to
        /// be included in the table rather than just the non-inherited properties. This is known as
        /// Table per Concrete Type (TPC) mapping.
        /// </summary>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        public EntityMappingConfiguration<TEntityType> MapInheritedProperties()
        {
            _entityMappingConfiguration.MapInheritedProperties = true;

            return this;
        }

        /// <summary>
        /// Configures the table name to be mapped to.
        /// </summary>
        /// <param name="tableName"> Name of the table. </param>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        public EntityMappingConfiguration<TEntityType> ToTable(string tableName)
        {
            Check.NotEmpty(tableName, "tableName");

            var databaseName = DatabaseName.Parse(tableName);

            ToTable(databaseName.Name, databaseName.Schema);

            return this;
        }

        /// <summary>
        /// Configures the table name and schema to be mapped to.
        /// </summary>
        /// <param name="tableName"> Name of the table. </param>
        /// <param name="schemaName"> Schema of the table. </param>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        public EntityMappingConfiguration<TEntityType> ToTable(string tableName, string schemaName)
        {
            Check.NotEmpty(tableName, "tableName");

            _entityMappingConfiguration.TableName = new DatabaseName(tableName, schemaName);

            return this;
        }

        /// <summary>
        /// Sets an annotation in the model for the table to which this entity is mapped. The annotation
        /// value can later be used when processing the table such as when creating migrations.
        /// </summary>
        /// <remarks>
        /// It will likely be necessary to register a <see cref="IMetadataAnnotationSerializer"/> if the type of
        /// the annotation value is anything other than a string. Passing a null value clears any annotation with
        /// the given name on the column that had been previously set.
        /// </remarks>
        /// <param name="name">The annotation name, which must be a valid C#/EDM identifier.</param>
        /// <param name="value">The annotation value, which may be a string or some other type that
        /// can be serialized with an <see cref="IMetadataAnnotationSerializer"/></param>.
        /// <returns>The same configuration instance so that multiple calls can be chained.</returns>
        public EntityMappingConfiguration<TEntityType> HasTableAnnotation(string name, object value)
        {
            Check.NotEmpty(name, "name");

            _entityMappingConfiguration.SetAnnotation(name, value);

            return this;
        }

        /// <summary>
        /// Configures the discriminator column used to differentiate between types in an inheritance hierarchy.
        /// </summary>
        /// <param name="discriminator"> The name of the discriminator column. </param>
        /// <returns> A configuration object to further configure the discriminator column and values. </returns>
        public ValueConditionConfiguration Requires(string discriminator)
        {
            Check.NotEmpty(discriminator, "discriminator");

            return new ValueConditionConfiguration(_entityMappingConfiguration, discriminator);
        }

        /// <summary>
        /// Configures the discriminator condition used to differentiate between types in an inheritance hierarchy.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being used to discriminate between types. </typeparam>
        /// <param name="property"> A lambda expression representing the property being used to discriminate between types. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object to further configure the discriminator condition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public NotNullConditionConfiguration Requires<TProperty>(Expression<Func<TEntityType, TProperty>> property)
        {
            Check.NotNull(property, "property");

            return new NotNullConditionConfiguration(_entityMappingConfiguration, property.GetComplexPropertyAccess());
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

        /// <summary>
        /// Gets the <see cref="Type" /> of the current instance.
        /// </summary>
        /// <returns>The exact runtime type of the current instance.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
