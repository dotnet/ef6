// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Types
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Services;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Allows configuration to be performed for an entity type in a model.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal class EntityTypeConfiguration : StructuralTypeConfiguration
    {
        private readonly List<PropertyInfo> _keyProperties = new List<PropertyInfo>();

        private readonly Dictionary<PropertyInfo, NavigationPropertyConfiguration> _navigationPropertyConfigurations
            = new Dictionary<PropertyInfo, NavigationPropertyConfiguration>(
                new DynamicEqualityComparer<PropertyInfo>((p1, p2) => p1.IsSameAs(p2)));

        private readonly List<EntityMappingConfiguration> _entityMappingConfigurations
            = new List<EntityMappingConfiguration>();

        private readonly Dictionary<Type, EntityMappingConfiguration> _entitySubTypesMappingConfigurations
            = new Dictionary<Type, EntityMappingConfiguration>();

        private readonly List<EntityMappingConfiguration> _nonCloneableMappings = new List<EntityMappingConfiguration>();

        private bool _isKeyConfigured;
        private string _entitySetName;

        private ModificationStoredProceduresConfiguration _modificationStoredProceduresConfiguration;

        internal EntityTypeConfiguration(Type structuralType)
            : base(structuralType)
        {
            IsReplaceable = false;
        }

        private EntityTypeConfiguration(EntityTypeConfiguration source)
            : base(source)
        {
            DebugCheck.NotNull(source);

            _keyProperties.AddRange(source._keyProperties);
            source._navigationPropertyConfigurations.Each(
                c => _navigationPropertyConfigurations.Add(c.Key, c.Value.Clone()));
            source._entitySubTypesMappingConfigurations.Each(
                c => _entitySubTypesMappingConfigurations.Add(c.Key, c.Value.Clone()));

            _entityMappingConfigurations.AddRange(
                source._entityMappingConfigurations.Except(source._nonCloneableMappings).Select(e => e.Clone()));

            _isKeyConfigured = source._isKeyConfigured;
            _entitySetName = source._entitySetName;

            if (source._modificationStoredProceduresConfiguration != null)
            {
                _modificationStoredProceduresConfiguration = source._modificationStoredProceduresConfiguration.Clone();
            }

            IsReplaceable = source.IsReplaceable;
            IsTableNameConfigured = source.IsTableNameConfigured;
            IsExplicitEntity = source.IsExplicitEntity;
        }

        internal virtual EntityTypeConfiguration Clone()
        {
            return new EntityTypeConfiguration(this);
        }

        internal IEnumerable<Type> ConfiguredComplexTypes
        {
            get
            {
                return PrimitivePropertyConfigurations
                    .Where(c => c.Key.Count > 1)
                    .Select(c => c.Key.Reverse().Skip(1))
                    .SelectMany(p => p)
                    .Select(pi => pi.PropertyType);
            }
        }

        internal bool IsStructuralConfigurationOnly
        {
            get
            {
                return !_keyProperties.Any()
                       && !_navigationPropertyConfigurations.Any()
                       && !_entityMappingConfigurations.Any()
                       && !_entitySubTypesMappingConfigurations.Any()
                       && _entitySetName == null;
            }
        }

        internal override void RemoveProperty(PropertyPath propertyPath)
        {
            base.RemoveProperty(propertyPath);

            _navigationPropertyConfigurations.Remove(propertyPath.Single());
        }

        internal bool IsKeyConfigured
        {
            get { return _isKeyConfigured; }
        }

        internal IEnumerable<PropertyInfo> KeyProperties
        {
            get { return _keyProperties; }
        }

        internal virtual void Key(IEnumerable<PropertyInfo> keyProperties)
        {
            DebugCheck.NotNull(keyProperties);

            ClearKey();

            foreach (var property in keyProperties)
            {
                Key(property, OverridableConfigurationParts.None);
            }

            _isKeyConfigured = true;
        }

        /// <summary>
        /// Configures the primary key property(s) for this entity type.
        /// </summary>
        /// <param name="propertyInfo"> The property to be used as the primary key. If the primary key is made up of multiple properties, call this method once for each of them. </param>
        public void Key(PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, "propertyInfo");

            Key(propertyInfo, null);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        internal virtual void Key(PropertyInfo propertyInfo, OverridableConfigurationParts? overridableConfigurationParts)
        {
            DebugCheck.NotNull(propertyInfo);

            if (!propertyInfo.IsValidEdmScalarProperty())
            {
                throw Error.ModelBuilder_KeyPropertiesMustBePrimitive(propertyInfo.Name, ClrType);
            }

            if (!_isKeyConfigured
                && !_keyProperties.ContainsSame(propertyInfo))
            {
                _keyProperties.Add(propertyInfo);

                Property(new PropertyPath(propertyInfo), overridableConfigurationParts);
            }
        }

        internal void ClearKey()
        {
            _keyProperties.Clear();
            _isKeyConfigured = false;
        }

        /// <summary>
        /// Gets a value indicating whether the name of the table has been configured.
        /// </summary>
        public bool IsTableNameConfigured { get; private set; }

        /// <summary>
        /// True if this configuration can be replaced in the model configuration, false otherwise
        /// This is only set to true for configurations that are registered automatically via the DbContext
        /// </summary>
        internal bool IsReplaceable { get; set; }

        internal bool IsExplicitEntity { get; set; }

        internal ModificationStoredProceduresConfiguration ModificationStoredProceduresConfiguration
        {
            get { return _modificationStoredProceduresConfiguration; }
        }

        internal virtual void MapToStoredProcedures()
        {
            if (_modificationStoredProceduresConfiguration == null)
            {
                _modificationStoredProceduresConfiguration = new ModificationStoredProceduresConfiguration();
            }
        }

        internal virtual void MapToStoredProcedures(
            ModificationStoredProceduresConfiguration modificationStoredProceduresConfiguration, bool allowOverride)
        {
            DebugCheck.NotNull(modificationStoredProceduresConfiguration);

            if (_modificationStoredProceduresConfiguration == null)
            {
                _modificationStoredProceduresConfiguration = modificationStoredProceduresConfiguration;
            }
            else
            {
                _modificationStoredProceduresConfiguration.Merge(modificationStoredProceduresConfiguration, allowOverride);
            }
        }

        internal void ReplaceFrom(EntityTypeConfiguration existing)
        {
            if (EntitySetName == null)
            {
                EntitySetName = existing.EntitySetName;
            }
        }

        /// <summary>
        /// Gets or sets the entity set name to be used for this entity type.
        /// </summary>
        public virtual string EntitySetName
        {
            get { return _entitySetName; }
            set
            {
                Check.NotEmpty(value, "value");

                _entitySetName = value;
            }
        }

        internal override IEnumerable<PropertyInfo> ConfiguredProperties
        {
            get { return base.ConfiguredProperties.Union(_navigationPropertyConfigurations.Keys); }
        }

        /// <summary>
        /// Gets the name of the table that this entity type is mapped to.
        /// </summary>
        public string TableName
        {
            get
            {
                if (!IsTableNameConfigured)
                {
                    return null;
                }

                return GetTableName().Name;
            }
        }

        /// <summary>
        /// Gets the database schema of the table that this entity type is mapped to.
        /// </summary>
        public string SchemaName
        {
            get
            {
                if (!IsTableNameConfigured)
                {
                    return null;
                }

                return GetTableName().Schema;
            }
        }

        internal DatabaseName GetTableName()
        {
            if (!IsTableNameConfigured)
            {
                return null;
            }

            return _entityMappingConfigurations.First().TableName;
        }

        /// <summary>
        /// Configures the table name that this entity type is mapped to.
        /// </summary>
        /// <param name="tableName"> The name of the table. </param>
        public void ToTable(string tableName)
        {
            Check.NotEmpty(tableName, "tableName");

            ToTable(tableName, null);
        }

        /// <summary>
        /// Configures the table name that this entity type is mapped to.
        /// </summary>
        /// <param name="tableName"> The name of the table. </param>
        /// <param name="schemaName"> The database schema of the table. </param>
        public void ToTable(string tableName, string schemaName)
        {
            Check.NotEmpty(tableName, "tableName");

            IsTableNameConfigured = true;

            if (!_entityMappingConfigurations.Any())
            {
                _entityMappingConfigurations.Add(new EntityMappingConfiguration());
            }

            _entityMappingConfigurations.First().TableName
                = string.IsNullOrWhiteSpace(schemaName)
                      ? new DatabaseName(tableName)
                      : new DatabaseName(tableName, schemaName);

            UpdateTableNameForSubTypes();
        }

        private void UpdateTableNameForSubTypes()
        {
            _entitySubTypesMappingConfigurations
                .Where(stmc => stmc.Value.TableName == null)
                .Select(tphs => tphs.Value)
                .Each(tphmc => tphmc.TableName = GetTableName());
        }

        internal void AddMappingConfiguration(EntityMappingConfiguration mappingConfiguration, bool cloneable = true)
        {
            DebugCheck.NotNull(mappingConfiguration);

            if (_entityMappingConfigurations.Contains(mappingConfiguration))
            {
                return;
            }

            var tableName = mappingConfiguration.TableName;

            if (tableName != null)
            {
                var existingMappingConfiguration
                    = _entityMappingConfigurations
                        .SingleOrDefault(mf => tableName.Equals(mf.TableName));

                if (existingMappingConfiguration != null)
                {
                    throw Error.InvalidTableMapping(ClrType.Name, tableName);
                }
            }

            _entityMappingConfigurations.Add(mappingConfiguration);

            if (_entityMappingConfigurations.Count > 1
                && _entityMappingConfigurations.Any(mc => mc.TableName == null))
            {
                throw Error.InvalidTableMapping_NoTableName(ClrType.Name);
            }

            IsTableNameConfigured |= tableName != null;

            if (!cloneable)
            {
                _nonCloneableMappings.Add(mappingConfiguration);
            }
        }

        internal void AddSubTypeMappingConfiguration(Type subType, EntityMappingConfiguration mappingConfiguration)
        {
            DebugCheck.NotNull(subType);
            DebugCheck.NotNull(mappingConfiguration);

            EntityMappingConfiguration _;
            if (_entitySubTypesMappingConfigurations.TryGetValue(subType, out _))
            {
                throw Error.InvalidChainedMappingSyntax(subType.Name);
            }

            _entitySubTypesMappingConfigurations.Add(subType, mappingConfiguration);
        }

        internal Dictionary<Type, EntityMappingConfiguration> SubTypeMappingConfigurations
        {
            get { return _entitySubTypesMappingConfigurations; }
        }

        internal NavigationPropertyConfiguration Navigation(PropertyInfo propertyInfo)
        {
            DebugCheck.NotNull(propertyInfo);

            NavigationPropertyConfiguration navigationPropertyConfiguration;
            if (!_navigationPropertyConfigurations.TryGetValue(propertyInfo, out navigationPropertyConfiguration))
            {
                _navigationPropertyConfigurations.Add(
                    propertyInfo, navigationPropertyConfiguration = new NavigationPropertyConfiguration(propertyInfo));
            }

            return navigationPropertyConfiguration;
        }

        internal virtual void Configure(EntityType entityType, EdmModel model)
        {
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(model);

            ConfigureKey(entityType);
            Configure(entityType.Name, entityType.Properties, entityType.Annotations);
            ConfigureAssociations(entityType, model);
            ConfigureEntitySetName(entityType, model);
        }

        private void ConfigureEntitySetName(EntityType entityType, EdmModel model)
        {
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(model);

            if ((EntitySetName == null)
                || (entityType.BaseType != null))
            {
                return;
            }

            var entitySet = model.GetEntitySet(entityType);

            Debug.Assert(entitySet != null);

            entitySet.Name
                = model.GetEntitySets().Except(new[] { entitySet }).UniquifyName(EntitySetName);

            entitySet.SetConfiguration(this);
        }

        private void ConfigureKey(EntityType entityType)
        {
            DebugCheck.NotNull(entityType);

            if (!_keyProperties.Any())
            {
                return;
            }

            if (entityType.BaseType != null)
            {
                throw Error.KeyRegisteredOnDerivedType(ClrType, entityType.GetRootType().GetClrType());
            }

            var keyProperties = _keyProperties.AsEnumerable();

            if (!_isKeyConfigured)
            {
                var primaryKeys
                    = from p in _keyProperties
                      select new
                          {
                              PropertyInfo = p,
                              Property(new PropertyPath(p)).ColumnOrder
                          };

                if ((_keyProperties.Count > 1)
                    && primaryKeys.Any(p => !p.ColumnOrder.HasValue))
                {
                    throw Error.ModelGeneration_UnableToDetermineKeyOrder(ClrType);
                }

                keyProperties = primaryKeys.OrderBy(p => p.ColumnOrder).Select(p => p.PropertyInfo);
            }

            foreach (var keyProperty in keyProperties)
            {
                var property = entityType.GetDeclaredPrimitiveProperty(keyProperty);

                if (property == null)
                {
                    throw Error.KeyPropertyNotFound(keyProperty.Name, entityType.Name);
                }

                property.Nullable = false;
                entityType.AddKeyMember(property);
            }
        }

        private void ConfigureAssociations(EntityType entityType, EdmModel model)
        {
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(model);

            foreach (var configuration in _navigationPropertyConfigurations)
            {
                var propertyInfo = configuration.Key;
                var navigationPropertyConfiguration = configuration.Value;
                var navigationProperty = entityType.GetNavigationProperty(propertyInfo);

                if (navigationProperty == null)
                {
                    var property = entityType.Properties.SingleOrDefault(p => p.GetClrPropertyInfo() == propertyInfo);
                    if (property != null
                        && property.ComplexType != null)
                    {
                        throw new InvalidOperationException(
                            Strings.InvalidNavigationPropertyComplexType(propertyInfo.Name, entityType.Name, property.ComplexType.Name));
                    }

                    throw Error.NavigationPropertyNotFound(propertyInfo.Name, entityType.Name);
                }

                // Don't configure inherited navigation properties
                if (entityType.DeclaredNavigationProperties.Any(np => np.GetClrPropertyInfo().IsSameAs(propertyInfo)))
                {
                    navigationPropertyConfiguration.Configure(navigationProperty, model, this);
                }
            }
        }

        internal void ConfigureTablesAndConditions(
            StorageEntityTypeMapping entityTypeMapping,
            DbDatabaseMapping databaseMapping,
            DbProviderManifest providerManifest)
        {
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(providerManifest);

            var entityType
                = (entityTypeMapping != null)
                      ? entityTypeMapping.EntityType
                      : databaseMapping.Model.GetEntityType(ClrType);

            if (_entityMappingConfigurations.Any())
            {
                for (var i = 0; i < _entityMappingConfigurations.Count; i++)
                {
                    _entityMappingConfigurations[i]
                        .Configure(
                            databaseMapping,
                            providerManifest,
                            entityType,
                            ref entityTypeMapping,
                            IsMappingAnyInheritedProperty(entityType),
                            i,
                            _entityMappingConfigurations.Count);
                }
            }
            else
            {
                ConfigureUnconfiguredType(databaseMapping, providerManifest, entityType);
            }
        }

        internal bool IsMappingAnyInheritedProperty(EntityType entityType)
        {
            return _entityMappingConfigurations.Any(emc => emc.MapsAnyInheritedProperties(entityType));
        }

        internal bool IsNavigationPropertyConfigured(PropertyInfo propertyInfo)
        {
            return _navigationPropertyConfigurations.ContainsKey(propertyInfo);
        }

        internal static void ConfigureUnconfiguredType(
            DbDatabaseMapping databaseMapping, DbProviderManifest providerManifest, EntityType entityType)
        {
            var c = new EntityMappingConfiguration();
            var entityTypeMapping
                = databaseMapping.GetEntityTypeMapping(entityType.GetClrType());
            c.Configure(databaseMapping, providerManifest, entityType, ref entityTypeMapping, false, 0, 1);
        }

        internal void Configure(
            EntityType entityType,
            DbDatabaseMapping databaseMapping,
            DbProviderManifest providerManifest)
        {
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(providerManifest);

            var entityTypeMapping
                = databaseMapping.GetEntityTypeMapping(entityType.GetClrType());

            if (entityTypeMapping != null)
            {
                VerifyAllCSpacePropertiesAreMapped(
                    databaseMapping.GetEntityTypeMappings(entityType).ToList(),
                    entityTypeMapping.EntityType.DeclaredProperties,
                    new List<EdmProperty>());
            }

            ConfigurePropertyMappings(databaseMapping, entityType, providerManifest);
            ConfigureAssociationMappings(databaseMapping, entityType, providerManifest);
            ConfigureDependentKeys(databaseMapping, providerManifest);
            ConfigureModificationStoredProcedures(databaseMapping, entityType, providerManifest);
        }

        internal void ConfigureFunctionParameters(DbDatabaseMapping databaseMapping, EntityType entityType)
        {
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(entityType);

            var parameterBindings
                = (from esm in databaseMapping.GetEntitySetMappings()
                   from mfm in esm.ModificationFunctionMappings
                   where mfm.EntityType == entityType
                   from pb in mfm.PrimaryParameterBindings
                   select pb)
                    .ToList();

            ConfigureFunctionParameters(parameterBindings);

            foreach (var derivedEntityType in databaseMapping.Model.EntityTypes.Where(et => et.BaseType == entityType))
            {
                ConfigureFunctionParameters(databaseMapping, derivedEntityType);
            }
        }

        private void ConfigureModificationStoredProcedures(
            DbDatabaseMapping databaseMapping, EntityType entityType, DbProviderManifest providerManifest)
        {
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(providerManifest);

            if (_modificationStoredProceduresConfiguration != null)
            {
                new ModificationFunctionMappingGenerator(providerManifest)
                    .Generate(entityType, databaseMapping);

                var modificationStoredProcedureMapping
                    = databaseMapping.GetEntitySetMappings()
                        .SelectMany(esm => esm.ModificationFunctionMappings)
                        .SingleOrDefault(mfm => mfm.EntityType == entityType);

                if (modificationStoredProcedureMapping != null)
                {
                    _modificationStoredProceduresConfiguration.Configure(modificationStoredProcedureMapping, providerManifest);
                }
            }
        }

        private void ConfigurePropertyMappings(
            DbDatabaseMapping databaseMapping,
            EntityType entityType,
            DbProviderManifest providerManifest,
            bool allowOverride = false)
        {
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(providerManifest);

            var entityTypeMappings
                = databaseMapping.GetEntityTypeMappings(entityType);

            var propertyMappings
                = (from etm in entityTypeMappings
                    from etmf in etm.MappingFragments
                    from pm in etmf.ColumnMappings
                    select Tuple.Create(pm, etmf.Table))
                    .ToList();

            ConfigurePropertyMappings(propertyMappings, providerManifest, allowOverride);

            _entityMappingConfigurations
                .Each(c => c.ConfigurePropertyMappings(propertyMappings, providerManifest, allowOverride));

            // Now, apply to any inherited (IsOfType) mappings
            var inheritedPropertyMappings
                = (from esm in databaseMapping.GetEntitySetMappings()
                    from etm in esm.EntityTypeMappings
                    where etm.IsHierarchyMapping
                          && etm.EntityType.IsAncestorOf(entityType)
                    from etmf in etm.MappingFragments
                    from pm1 in etmf.ColumnMappings
                    where !propertyMappings.Any(pm2 => pm2.Item1.PropertyPath.SequenceEqual(pm1.PropertyPath))
                    select Tuple.Create(pm1, etmf.Table))
                    .ToList();

            ConfigurePropertyMappings(inheritedPropertyMappings, providerManifest);

            _entityMappingConfigurations
                .Each(c => c.ConfigurePropertyMappings(inheritedPropertyMappings, providerManifest));

            foreach (var derivedEntityType 
                in databaseMapping.Model.EntityTypes.Where(et => et.BaseType == entityType))
            {
                ConfigurePropertyMappings(databaseMapping, derivedEntityType, providerManifest, true);
            }
        }

        private void ConfigureAssociationMappings(
            DbDatabaseMapping databaseMapping, EntityType entityType, DbProviderManifest providerManifest)
        {
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(providerManifest);

            foreach (var configuration in _navigationPropertyConfigurations)
            {
                var propertyInfo = configuration.Key;
                var navigationPropertyConfiguration = configuration.Value;
                var navigationProperty = entityType.GetNavigationProperty(propertyInfo);

                if (navigationProperty == null)
                {
                    throw Error.NavigationPropertyNotFound(propertyInfo.Name, entityType.Name);
                }

                var associationSetMapping
                    = databaseMapping.GetAssociationSetMappings()
                        .SingleOrDefault(asm => asm.AssociationSet.ElementType == navigationProperty.Association);

                if (associationSetMapping != null)
                {
                    navigationPropertyConfiguration.Configure(associationSetMapping, databaseMapping, providerManifest);
                }
            }
        }

        private static void ConfigureDependentKeys(DbDatabaseMapping databaseMapping, DbProviderManifest providerManifest)
        {
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(providerManifest);

            foreach (var foreignKeyConstraint in databaseMapping.Database.EntityTypes.SelectMany(t => t.ForeignKeyBuilders))
            {
                foreignKeyConstraint
                    .DependentColumns
                    .Each(
                        (c, i) =>
                            {
                                var primitivePropertyConfiguration =
                                    c.GetConfiguration() as PrimitivePropertyConfiguration;

                                if ((primitivePropertyConfiguration != null)
                                    && (primitivePropertyConfiguration.ColumnType != null))
                                {
                                    return;
                                }

                                var principalColumn = foreignKeyConstraint.PrincipalTable.KeyProperties.ElementAt(i);

                                c.PrimitiveType = providerManifest.GetStoreTypeFromName(principalColumn.TypeName);

                                c.CopyFrom(principalColumn);
                            });
            }
        }

        private static void VerifyAllCSpacePropertiesAreMapped(
            ICollection<StorageEntityTypeMapping> entityTypeMappings, IEnumerable<EdmProperty> properties,
            IList<EdmProperty> propertyPath)
        {
            DebugCheck.NotNull(entityTypeMappings);

            var entityType = entityTypeMappings.First().EntityType;

            foreach (var property in properties)
            {
                propertyPath.Add(property);

                if (property.IsComplexType)
                {
                    VerifyAllCSpacePropertiesAreMapped(
                        entityTypeMappings,
                        property.ComplexType.Properties,
                        propertyPath);
                }
                else if (!entityTypeMappings.SelectMany(etm => etm.MappingFragments)
                              .SelectMany(mf => mf.ColumnMappings)
                              .Any(pm => pm.PropertyPath.SequenceEqual(propertyPath))
                         && !entityType.Abstract)
                {
                    throw Error.InvalidEntitySplittingProperties(entityType.Name);
                }

                propertyPath.Remove(property);
            }
        }
    }
}
