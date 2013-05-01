// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Types
{
    using System.ComponentModel;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    ///     Allows configuration to be performed for an entity type in a model.
    ///     This configuration functionality is available via lightweight conventions.
    /// </summary>
    /// <typeparam name="T"> A type inherited by the entity type. </typeparam>
    public class LightweightTypeConfiguration<T>
        where T : class
    {
        private readonly LightweightTypeConfiguration _configuration;

        internal LightweightTypeConfiguration(
            Type type,
            Func<EntityTypeConfiguration> entityTypeConfiguration,
            ModelConfiguration modelConfiguration)
        {
            VerifyType(type);

            _configuration = new LightweightTypeConfiguration(type, entityTypeConfiguration, modelConfiguration);
        }

        internal LightweightTypeConfiguration(
            Type type,
            Func<ComplexTypeConfiguration> complexTypeConfiguration,
            ModelConfiguration modelConfiguration)
        {
            VerifyType(type);

            _configuration = new LightweightTypeConfiguration(type, complexTypeConfiguration, modelConfiguration);
        }

        internal LightweightTypeConfiguration(
            Type type,
            ModelConfiguration modelConfiguration)
        {
            VerifyType(type);

            _configuration = new LightweightTypeConfiguration(type, modelConfiguration);
        }

        [Conditional("DEBUG")]
        private static void VerifyType(Type type)
        {
            DebugCheck.NotNull(type);
            Debug.Assert(
                typeof(T).IsAssignableFrom(type),
                string.Format("The type '{0}' is invalid. The specified type must derive from '{1}'.", type, typeof(T)));
        }

        /// <summary>
        ///     Gets the <see cref="Type" /> of this entity type.
        /// </summary>
        public Type ClrType
        {
            get { return _configuration.ClrType; }
        }

        /// <summary>
        ///     Configures the entity set name to be used for this entity type.
        ///     The entity set name can only be configured for the base type in each set.
        /// </summary>
        /// <param name="entitySetName"> The name of the entity set. </param>
        /// <returns>
        ///     The same <see cref="LightweightTypeConfiguration{T}" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured.
        /// </remarks>
        public LightweightTypeConfiguration<T> HasEntitySetName(string entitySetName)
        {
            _configuration.HasEntitySetName(entitySetName);

            return this;
        }

        /// <summary>
        ///     Excludes this entity type from the model so that it will not be mapped to the database.
        /// </summary>
        public LightweightTypeConfiguration<T> Ignore()
        {
            _configuration.Ignore();

            return this;
        }

        /// <summary>
        ///     Changes this entity type to a complex type.
        /// </summary>
        public LightweightTypeConfiguration<T> IsComplexType()
        {
            _configuration.IsComplexType();

            return this;
        }

        /// <summary>
        ///     Excludes a property from the model so that it will not be mapped to the database.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property to be ignored. </typeparam>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public LightweightTypeConfiguration<T> Ignore<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            _configuration.Ignore(propertyExpression.GetSimplePropertyAccess().Single());

            return this;
        }

        /// <summary>
        ///     Configures a property that is defined on this type.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public LightweightPrimitivePropertyConfiguration Property<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return _configuration.Property(propertyExpression.GetComplexPropertyAccess());
        }

        /// <summary>
        ///     Configures a property that is defined on this type as a navigation property.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public LightweightNavigationPropertyConfiguration NavigationProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return _configuration.NavigationProperty(propertyExpression.GetComplexPropertyAccess());
        }

        /// <summary>
        ///     Configures the primary key property(s) for this entity type.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the key. </typeparam>
        /// <param name="keyExpression"> A lambda expression representing the property to be used as the primary key. C#: t => t.Id VB.Net: Function(t) t.Id If the primary key is made up of multiple properties then specify an anonymous type including the properties. C#: t => new { t.Id1, t.Id2 } VB.Net: Function(t) New With { t.Id1, t.Id2 } </param>
        /// <returns>
        ///     The same <see cref="LightweightTypeConfiguration{T}" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public LightweightTypeConfiguration<T> HasKey<TProperty>(Expression<Func<T, TProperty>> keyExpression)
        {
            Check.NotNull(keyExpression, "keyExpression");

            _configuration.HasKey(keyExpression.GetSimplePropertyAccessList().Select(p => p.Single()));

            return this;
        }

        /// <summary>
        ///     Configures the table name that this entity type is mapped to.
        /// </summary>
        /// <param name="tableName"> The name of the table. </param>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured.
        /// </remarks>
        public LightweightTypeConfiguration<T> ToTable(string tableName)
        {
            Check.NotEmpty(tableName, "tableName");

            _configuration.ToTable(tableName);

            return this;
        }

        /// <summary>
        ///     Configures the table name that this entity type is mapped to.
        /// </summary>
        /// <param name="tableName"> The name of the table. </param>
        /// <param name="schemaName"> The database schema of the table. </param>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured.
        /// </remarks>
        public LightweightTypeConfiguration<T> ToTable(string tableName, string schemaName)
        {
            Check.NotEmpty(tableName, "tableName");

            _configuration.ToTable(tableName, schemaName);

            return this;
        }

        public LightweightTypeConfiguration<T> MapToStoredProcedures()
        {
            _configuration.MapToStoredProcedures();

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public LightweightTypeConfiguration<T> MapToStoredProcedures(
            Action<ModificationFunctionsConfiguration<T>> modificationFunctionsConfigurationAction)
        {
            Check.NotNull(modificationFunctionsConfigurationAction, "modificationFunctionsConfigurationAction");

            var modificationFunctionMappingConfiguration = new ModificationFunctionsConfiguration<T>();

            modificationFunctionsConfigurationAction(modificationFunctionMappingConfiguration);

            _configuration.MapToStoredProcedures(modificationFunctionMappingConfiguration.Configuration);

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
