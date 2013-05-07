// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Types
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Core;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Mappers;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Allows configuration to be performed for an entity type in a model.
    ///     This configuration functionality is available via lightweight conventions.
    /// </summary>
    public class LightweightTypeConfiguration
    {
        private readonly Type _type;
        private readonly Func<EntityTypeConfiguration> _entityTypeConfiguration;
        private readonly ModelConfiguration _modelConfiguration;
        private readonly Func<ComplexTypeConfiguration> _complexTypeConfiguration;
        private ConfigurationAspect _currentConfigurationAspect;

        internal LightweightTypeConfiguration(
            Type type,
            ModelConfiguration modelConfiguration)
            : this(type, null, null, modelConfiguration)
        {
        }

        internal LightweightTypeConfiguration(
            Type type,
            Func<EntityTypeConfiguration> entityTypeConfiguration,
            ModelConfiguration modelConfiguration)
            : this(type, entityTypeConfiguration, null, modelConfiguration)
        {
            DebugCheck.NotNull(entityTypeConfiguration);
        }

        internal LightweightTypeConfiguration(
            Type type,
            Func<ComplexTypeConfiguration> complexTypeConfiguration,
            ModelConfiguration modelConfiguration)
            : this(type, null, complexTypeConfiguration, modelConfiguration)
        {
            DebugCheck.NotNull(complexTypeConfiguration);
        }

        private LightweightTypeConfiguration(
            Type type,
            Func<EntityTypeConfiguration> entityTypeConfiguration,
            Func<ComplexTypeConfiguration> complexTypeConfiguration,
            ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(modelConfiguration);

            _type = type;
            _entityTypeConfiguration = entityTypeConfiguration;
            _complexTypeConfiguration = complexTypeConfiguration;
            _modelConfiguration = modelConfiguration;
        }

        /// <summary>
        ///     Gets the <see cref="Type" /> of this entity type.
        /// </summary>
        public Type ClrType
        {
            get { return _type; }
        }

        /// <summary>
        ///     Configures the entity set name to be used for this entity type.
        ///     The entity set name can only be configured for the base type in each set.
        /// </summary>
        /// <param name="entitySetName"> The name of the entity set. </param>
        /// <returns>
        ///     The same <see cref="LightweightTypeConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured.
        /// </remarks>
        public LightweightTypeConfiguration HasEntitySetName(string entitySetName)
        {
            Check.NotEmpty(entitySetName, "entitySetName");
            ValidateConfiguration(ConfigurationAspect.EntitySetName);

            if (_entityTypeConfiguration != null
                && _entityTypeConfiguration().EntitySetName == null)
            {
                _entityTypeConfiguration().EntitySetName = entitySetName;
            }

            return this;
        }

        /// <summary>
        ///     Excludes this entity type from the model so that it will not be mapped to the database.
        /// </summary>
        public LightweightTypeConfiguration Ignore()
        {
            ValidateConfiguration(ConfigurationAspect.IgnoreType);

            if (_entityTypeConfiguration == null
                && _complexTypeConfiguration == null)
            {
                _modelConfiguration.Ignore(_type);
            }

            return this;
        }

        /// <summary>
        ///     Changes this entity type to a complex type.
        /// </summary>
        public LightweightTypeConfiguration IsComplexType()
        {
            ValidateConfiguration(ConfigurationAspect.ComplexType);

            if (_entityTypeConfiguration == null
                && _complexTypeConfiguration == null)
            {
                _modelConfiguration.ComplexType(_type);
            }

            return this;
        }

        /// <summary>
        ///     Excludes a property from the model so that it will not be mapped to the database.
        /// </summary>
        /// <param name="propertyName"> The name of the property to be configured. </param>
        /// <remarks>
        ///     Calling this will have no effect if the property does not exist.
        /// </remarks>
        public LightweightTypeConfiguration Ignore(string propertyName)
        {
            Check.NotEmpty(propertyName, "propertyName");

            var propertyInfo = _type.GetProperty(propertyName, PropertyFilter.DefaultBindingFlags);
            if (propertyInfo == null)
            {
                throw new InvalidOperationException(Strings.NoSuchProperty(propertyName, _type.FullName));
            }

            Ignore(propertyInfo);

            return this;
        }

        /// <summary>
        ///     Excludes a property from the model so that it will not be mapped to the database.
        /// </summary>
        /// <param name="propertyInfo"> The property to be configured. </param>
        /// <remarks>
        ///     Calling this will have no effect if the property does not exist.
        /// </remarks>
        public LightweightTypeConfiguration Ignore(PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, "propertyInfo");
            ValidateConfiguration(ConfigurationAspect.IgnoreProperty);

            if (propertyInfo != null)
            {
                if (_entityTypeConfiguration != null)
                {
                    _entityTypeConfiguration().Ignore(propertyInfo);
                }
                if (_complexTypeConfiguration != null)
                {
                    _complexTypeConfiguration().Ignore(propertyInfo);
                }
            }

            return this;
        }

        /// <summary>
        ///     Configures a property that is defined on this type.
        /// </summary>
        /// <param name="propertyName"> The name of the property being configured. </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        public LightweightPrimitivePropertyConfiguration Property(string propertyName)
        {
            Check.NotEmpty(propertyName, "propertyName");

            var propertyInfo = _type.GetProperty(propertyName, PropertyFilter.DefaultBindingFlags);

            if (propertyInfo == null)
            {
                throw new InvalidOperationException(Strings.NoSuchProperty(propertyName, _type.FullName));
            }

            return Property(propertyInfo);
        }

        /// <summary>
        ///     Configures a property that is defined on this type.
        /// </summary>
        /// <param name="propertyInfo"> The property being configured. </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        public LightweightPrimitivePropertyConfiguration Property(PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, "propertyInfo");

            return Property(new PropertyPath(propertyInfo));
        }

        internal LightweightPrimitivePropertyConfiguration Property(PropertyPath propertyPath)
        {
            DebugCheck.NotNull(propertyPath);

            ValidateConfiguration(ConfigurationAspect.Property);

            var propertyInfo = propertyPath.Last();

            if (!propertyInfo.IsValidEdmScalarProperty())
            {
                throw Error.LightweightEntityConfiguration_NonScalarProperty(propertyPath);
            }

            var propertyConfiguration = _entityTypeConfiguration != null
                                            ? _entityTypeConfiguration().Property(propertyPath, OverridableConfigurationParts.None)
                                            : _complexTypeConfiguration != null
                                                  ? _complexTypeConfiguration().Property(propertyPath, OverridableConfigurationParts.None)
                                                  : null;

            return new LightweightPrimitivePropertyConfiguration(propertyInfo, () => propertyConfiguration);
        }

        /// <summary>
        ///     Configures a property that is defined on this type as a navigation property.
        /// </summary>
        /// <param name="propertyName"> The name of the property being configured. </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        public LightweightNavigationPropertyConfiguration NavigationProperty(string propertyName)
        {
            Check.NotEmpty(propertyName, "propertyName");

            var propertyInfo = _type.GetProperty(propertyName, PropertyFilter.DefaultBindingFlags);
            if (propertyInfo == null)
            {
                throw new InvalidOperationException(Strings.NoSuchProperty(propertyName, _type.FullName));
            }

            return NavigationProperty(propertyInfo);
        }

        /// <summary>
        ///     Configures a property that is defined on this type as a navigation property.
        /// </summary>
        /// <param name="propertyInfo"> The property being configured. </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        public LightweightNavigationPropertyConfiguration NavigationProperty(PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, "propertyInfo");

            return NavigationProperty(new PropertyPath(propertyInfo));
        }

        internal LightweightNavigationPropertyConfiguration NavigationProperty(PropertyPath propertyPath)
        {
            DebugCheck.NotNull(propertyPath);

            ValidateConfiguration(ConfigurationAspect.NavigationProperty);

            var propertyInfo = propertyPath.Last();

            if (!propertyInfo.IsValidEdmNavigationProperty())
            {
                throw new InvalidOperationException(Strings.LightweightEntityConfiguration_InvalidNavigationProperty(propertyPath));
            }

            var propertyConfiguration = _entityTypeConfiguration != null
                                            ? _entityTypeConfiguration().Navigation(propertyInfo)
                                            : null;

            return new LightweightNavigationPropertyConfiguration(propertyConfiguration, _modelConfiguration);
        }

        /// <summary>
        ///     Configures the primary key property for this entity type.
        /// </summary>
        /// <param name="propertyName"> The name of the property to be used as the primary key. </param>
        /// <returns>
        ///     The same <see cref="LightweightTypeConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        public LightweightTypeConfiguration HasKey(string propertyName)
        {
            Check.NotEmpty(propertyName, "propertyName");

            var propertyInfo = _type.GetProperty(propertyName, PropertyFilter.DefaultBindingFlags);
            if (propertyInfo == null)
            {
                throw new InvalidOperationException(Strings.NoSuchProperty(propertyName, _type.FullName));
            }

            return HasKey(_type.GetProperty(propertyName));
        }

        /// <summary>
        ///     Configures the primary key property for this entity type.
        /// </summary>
        /// <param name="propertyInfo"> The property to be used as the primary key. </param>
        /// <returns>
        ///     The same <see cref="LightweightTypeConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        public LightweightTypeConfiguration HasKey(PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, "propertyInfo");

            ValidateConfiguration(ConfigurationAspect.Key);

            if (_entityTypeConfiguration != null
                && !_entityTypeConfiguration().IsKeyConfigured)
            {
                _entityTypeConfiguration().Key(propertyInfo);
            }

            return this;
        }

        /// <summary>
        ///     Configures the primary key property(s) for this entity type.
        /// </summary>
        /// <param name="propertyNames"> The names of the properties to be used as the primary key. </param>
        /// <returns>
        ///     The same <see cref="LightweightTypeConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        public LightweightTypeConfiguration HasKey(IEnumerable<string> propertyNames)
        {
            Check.NotNull(propertyNames, "propertyNames");

            var propertyInfos = propertyNames
                .Select(
                    n =>
                        {
                            var propertyInfo = _type.GetProperty(n, PropertyFilter.DefaultBindingFlags);
                            if (propertyInfo == null)
                            {
                                throw new InvalidOperationException(Strings.NoSuchProperty(n, _type.FullName));
                            }
                            return propertyInfo;
                        })
                .ToArray();

            return HasKey(propertyInfos);
        }

        /// <summary>
        ///     Configures the primary key property(s) for this entity type.
        /// </summary>
        /// <param name="keyProperties"> The properties to be used as the primary key. </param>
        /// <returns>
        ///     The same <see cref="LightweightTypeConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured or if any
        ///     property does not exist.
        /// </remarks>
        public LightweightTypeConfiguration HasKey(IEnumerable<PropertyInfo> keyProperties)
        {
            Check.NotNull(keyProperties, "keyProperties");
            EntityUtil.CheckArgumentContainsNull(ref keyProperties, "keyProperties");
            EntityUtil.CheckArgumentEmpty(
                ref keyProperties,
                p => Strings.CollectionEmpty(p, "HasKey"), "keyProperties");

            ValidateConfiguration(ConfigurationAspect.Key);

            if (_entityTypeConfiguration != null
                && !_entityTypeConfiguration().IsKeyConfigured)
            {
                _entityTypeConfiguration().Key(keyProperties);
            }

            return this;
        }

        /// <summary>
        ///     Configures the table name that this entity type is mapped to.
        /// </summary>
        /// <param name="tableName"> The name of the table. </param>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured.
        /// </remarks>
        public LightweightTypeConfiguration ToTable(string tableName)
        {
            Check.NotEmpty(tableName, "tableName");
            ValidateConfiguration(ConfigurationAspect.ToTable);

            if (_entityTypeConfiguration != null
                && !_entityTypeConfiguration().IsTableNameConfigured)
            {
                var databaseName = DatabaseName.Parse(tableName);

                _entityTypeConfiguration().ToTable(databaseName.Name, databaseName.Schema);
            }

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
        public LightweightTypeConfiguration ToTable(string tableName, string schemaName)
        {
            Check.NotEmpty(tableName, "tableName");
            ValidateConfiguration(ConfigurationAspect.ToTable);

            if (_entityTypeConfiguration != null
                && !_entityTypeConfiguration().IsTableNameConfigured)
            {
                _entityTypeConfiguration().ToTable(tableName, schemaName);
            }

            return this;
        }

        public LightweightTypeConfiguration MapToStoredProcedures()
        {
            ValidateConfiguration(ConfigurationAspect.MapToStoredProcedures);

            if (_entityTypeConfiguration != null)
            {
                _entityTypeConfiguration().MapToStoredProcedures();
            }

            return this;
        }

        public LightweightTypeConfiguration MapToStoredProcedures(
            Action<LightweightModificationFunctionsConfiguration> modificationFunctionsConfigurationAction)
        {
            Check.NotNull(modificationFunctionsConfigurationAction, "modificationFunctionsConfigurationAction");
            ValidateConfiguration(ConfigurationAspect.MapToStoredProcedures);

            var modificationFunctionMappingConfiguration = new LightweightModificationFunctionsConfiguration(_type);

            modificationFunctionsConfigurationAction(modificationFunctionMappingConfiguration);

            MapToStoredProcedures(modificationFunctionMappingConfiguration.Configuration);

            return this;
        }

        internal void MapToStoredProcedures(ModificationFunctionsConfiguration modificationFunctionsConfiguration)
        {
            DebugCheck.NotNull(modificationFunctionsConfiguration);

            if (_entityTypeConfiguration != null)
            {
                _entityTypeConfiguration().MapToStoredProcedures(modificationFunctionsConfiguration, allowOverride: false);
            }
        }

        private void ValidateConfiguration(ConfigurationAspect aspect)
        {
            _currentConfigurationAspect |= aspect;

            if (_currentConfigurationAspect.HasFlag(ConfigurationAspect.IgnoreType)
                && (_currentConfigurationAspect.HasFlag(ConfigurationAspect.ComplexType)
                    || _currentConfigurationAspect.HasFlag(ConfigurationAspect.EntitySetName)
                    || _currentConfigurationAspect.HasFlag(ConfigurationAspect.IgnoreProperty)
                    || _currentConfigurationAspect.HasFlag(ConfigurationAspect.Key)
                    || _currentConfigurationAspect.HasFlag(ConfigurationAspect.MapToStoredProcedures)
                    || _currentConfigurationAspect.HasFlag(ConfigurationAspect.NavigationProperty)
                    || _currentConfigurationAspect.HasFlag(ConfigurationAspect.Property)
                    || _currentConfigurationAspect.HasFlag(ConfigurationAspect.ToTable)))
            {
                throw new InvalidOperationException(Strings.LightweightEntityConfiguration_ConfigurationConflict_IgnoreType);
            }

            if (_currentConfigurationAspect.HasFlag(ConfigurationAspect.ComplexType)
                && (_currentConfigurationAspect.HasFlag(ConfigurationAspect.EntitySetName)
                    || _currentConfigurationAspect.HasFlag(ConfigurationAspect.Key)
                    || _currentConfigurationAspect.HasFlag(ConfigurationAspect.MapToStoredProcedures)
                    || _currentConfigurationAspect.HasFlag(ConfigurationAspect.NavigationProperty)
                    || _currentConfigurationAspect.HasFlag(ConfigurationAspect.ToTable)))
            {
                throw new InvalidOperationException(Strings.LightweightEntityConfiguration_ConfigurationConflict_ComplexType);
            }
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

        [Flags]
        private enum ConfigurationAspect : uint
        {
            None = 0,
            EntitySetName = 1 << 0,
            Key = 1 << 1,
            IgnoreType = 1 << 2,
            IgnoreProperty = 1 << 3,
            ComplexType = 1 << 4,
            MapToStoredProcedures = 1 << 5,
            Property = 1 << 6,
            NavigationProperty = 1 << 7,
            ToTable = 1 << 8
        }
    }
}
