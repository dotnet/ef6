// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Types
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
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
    public class LightweightEntityConfiguration
    {
        private readonly Type _type;

        internal LightweightEntityConfiguration(
            Type type,
            ModelConfiguration modelConfiguration)
            : this(type, null, null, modelConfiguration)
        {
            DebugCheck.NotNull(modelConfiguration);
        }

        internal LightweightEntityConfiguration(
            Type type,
            Func<EntityTypeConfiguration> entityTypeConfiguration,
            ModelConfiguration modelConfiguration)
            : this(type, entityTypeConfiguration, null, modelConfiguration)
        {
            DebugCheck.NotNull(entityTypeConfiguration);
            DebugCheck.NotNull(modelConfiguration);
        }

        internal LightweightEntityConfiguration(
            Type type,
            Func<ComplexTypeConfiguration> complexTypeConfiguration,
            ModelConfiguration modelConfiguration)
            : this(type, null, complexTypeConfiguration, modelConfiguration)
        {
            DebugCheck.NotNull(complexTypeConfiguration);
            DebugCheck.NotNull(modelConfiguration);
        }

        private LightweightEntityConfiguration(Type type,
            Func<EntityTypeConfiguration> entityTypeConfiguration,
            Func<ComplexTypeConfiguration> complexTypeConfiguration,
            ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(type);

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
        ///     The same <see cref="LightweightEntityConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured.
        /// </remarks>
        public LightweightEntityConfiguration HasEntitySetName(string entitySetName)
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
        public LightweightEntityConfiguration Ignore()
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
        public LightweightEntityConfiguration IsComplexType()
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
        public LightweightEntityConfiguration Ignore(string propertyName)
        {
            Check.NotEmpty(propertyName, "propertyName");

            Ignore(_type.GetProperty(propertyName));

            return this;
        }

        /// <summary>
        ///     Excludes a property from the model so that it will not be mapped to the database.
        /// </summary>
        /// <param name="propertyInfo"> The property to be configured. </param>
        /// <remarks>
        ///     Calling this will have no effect if the property does not exist.
        /// </remarks>
        public LightweightEntityConfiguration Ignore(PropertyInfo propertyInfo)
        {
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
        /// <param name="name"> The name of the property being configured. </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        /// <remarks>
        ///     If the property doesn't exist, any configuration will be silently ignored.
        /// </remarks>
        public LightweightPropertyConfiguration Property(string name)
        {
            Check.NotEmpty(name, "name");

            return Property(_type.GetProperty(name));
        }

        /// <summary>
        ///     Configures a property that is defined on this type.
        /// </summary>
        /// <param name="propertyInfo"> The property being configured. </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        /// <remarks>
        ///     If the property doesn't exist, any configuration will be silently ignored.
        /// </remarks>
        public LightweightPropertyConfiguration Property(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                return new MissingPropertyConfiguration();
            }

            return Property(new PropertyPath(propertyInfo));
        }

        internal LightweightPropertyConfiguration Property(PropertyPath propertyPath)
        {
            DebugCheck.NotNull(propertyPath);

            ValidateConfiguration(ConfigurationAspect.Property);

            var propertyInfo = propertyPath.Last();

            if (!propertyInfo.IsValidEdmScalarProperty()
                || propertyInfo.GetIndexParameters().Any())
            {
                throw Error.LightweightEntityConfiguration_NonScalarProperty(propertyPath);
            }

            var propertyConfiguration = new Lazy<PrimitivePropertyConfiguration>(
                () => _entityTypeConfiguration != null
                          ? _entityTypeConfiguration().Property(propertyPath, OverridableConfigurationParts.None)
                          : _complexTypeConfiguration != null
                                ? _complexTypeConfiguration().Property(propertyPath, OverridableConfigurationParts.None)
                                : null);

            return new LightweightPropertyConfiguration(propertyPath.Single(), () => propertyConfiguration.Value);
        }

        /// <summary>
        ///     Configures the primary key property for this entity type.
        /// </summary>
        /// <param name="propertyName"> The name of the property to be used as the primary key. </param>
        /// <returns>
        ///     The same <see cref="LightweightEntityConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured of if the
        ///     property does not exist.
        /// </remarks>
        public LightweightEntityConfiguration HasKey(string propertyName)
        {
            Check.NotEmpty(propertyName, "propertyName");

            return HasKey(new[] { propertyName });
        }

        /// <summary>
        ///     Configures the primary key property for this entity type.
        /// </summary>
        /// <param name="propertyInfo"> The property to be used as the primary key. </param>
        /// <returns>
        ///     The same <see cref="LightweightEntityConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured of if the
        ///     property does not exist.
        /// </remarks>
        public LightweightEntityConfiguration HasKey(PropertyInfo propertyInfo)
        {
            return HasKey(new[] { propertyInfo });
        }

        /// <summary>
        ///     Configures the primary key property(s) for this entity type.
        /// </summary>
        /// <param name="propertyNames"> The names of the properties to be used as the primary key. </param>
        /// <returns>
        ///     The same <see cref="LightweightEntityConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured or if any
        ///     property does not exist.
        /// </remarks>
        public LightweightEntityConfiguration HasKey(IEnumerable<string> propertyNames)
        {
            Check.NotNull(propertyNames, "propertyNames");

            var propertyInfos = propertyNames
                .Select(n => _type.GetProperty(n))
                .ToArray();

            return HasKey(propertyInfos);
        }

        /// <summary>
        ///     Configures the primary key property(s) for this entity type.
        /// </summary>
        /// <param name="keyProperties"> The properties to be used as the primary key. </param>
        /// <returns>
        ///     The same <see cref="LightweightEntityConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured or if any
        ///     property does not exist.
        /// </remarks>
        public LightweightEntityConfiguration HasKey(IEnumerable<PropertyInfo> keyProperties)
        {
            Check.NotNull(keyProperties, "keyProperties");
            ValidateConfiguration(ConfigurationAspect.Key);

            if (_entityTypeConfiguration != null
                && !_entityTypeConfiguration().IsKeyConfigured
                && keyProperties.Any()
                && keyProperties.All(p => p != null))
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
        public LightweightEntityConfiguration ToTable(string tableName)
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
        public LightweightEntityConfiguration ToTable(string tableName, string schemaName)
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

        public LightweightEntityConfiguration MapToStoredProcedures()
        {
            ValidateConfiguration(ConfigurationAspect.MapToStoredProcedures);

            if (_entityTypeConfiguration != null)
            {
                _entityTypeConfiguration().MapToStoredProcedures();
            }

            return this;
        }

        public LightweightEntityConfiguration MapToStoredProcedures(
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
                    || _currentConfigurationAspect.HasFlag(ConfigurationAspect.Property)
                    || _currentConfigurationAspect.HasFlag(ConfigurationAspect.ToTable)))
            {
                throw new InvalidOperationException(Strings.LightweightEntityConfiguration_ConfigurationConflict_IgnoreType);
            }

            if (_currentConfigurationAspect.HasFlag(ConfigurationAspect.ComplexType)
                && (_currentConfigurationAspect.HasFlag(ConfigurationAspect.EntitySetName)
                    || _currentConfigurationAspect.HasFlag(ConfigurationAspect.Key)
                    || _currentConfigurationAspect.HasFlag(ConfigurationAspect.MapToStoredProcedures)
                    || _currentConfigurationAspect.HasFlag(ConfigurationAspect.ToTable)))
            {
                throw new InvalidOperationException(Strings.LightweightEntityConfiguration_ConfigurationConflict_ComplexType);
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
            ToTable = 1 << 7
        }
    }
}
