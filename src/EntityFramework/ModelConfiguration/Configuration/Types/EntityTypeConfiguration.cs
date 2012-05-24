namespace System.Data.Entity.ModelConfiguration.Configuration.Types
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    internal class EntityTypeConfiguration : StructuralTypeConfiguration
    {
        private readonly List<PropertyInfo> _keyProperties = new List<PropertyInfo>();

        private readonly Dictionary<PropertyInfo, NavigationPropertyConfiguration> _navigationPropertyConfigurations
            =
            new Dictionary<PropertyInfo, NavigationPropertyConfiguration>(
                new DynamicEqualityComparer<PropertyInfo>((p1, p2) => p1.IsSameAs(p2)));

        private readonly List<EntityMappingConfiguration> _entityMappingConfigurations
            = new List<EntityMappingConfiguration>();

        private readonly Dictionary<Type, EntityMappingConfiguration> _entitySubTypesMappingConfigurations
            = new Dictionary<Type, EntityMappingConfiguration>();

        private readonly List<EntityMappingConfiguration> _nonCloneableMappings = new List<EntityMappingConfiguration>();

        private bool _isKeyConfigured;
        private string _entitySetName;

        public EntityTypeConfiguration(Type structuralType)
            : base(structuralType)
        {
            IsReplaceable = false;
        }

        private EntityTypeConfiguration(EntityTypeConfiguration source)
            : base(source)
        {
            Contract.Requires(source != null);

            _keyProperties.AddRange(source._keyProperties);
            source._navigationPropertyConfigurations.Each(
                c => _navigationPropertyConfigurations.Add(c.Key, c.Value.Clone()));
            source._entitySubTypesMappingConfigurations.Each(
                c => _entitySubTypesMappingConfigurations.Add(c.Key, c.Value.Clone()));

            _entityMappingConfigurations.AddRange(
                source._entityMappingConfigurations.Except(source._nonCloneableMappings).Select(e => e.Clone()));

            _isKeyConfigured = source._isKeyConfigured;
            _entitySetName = source._entitySetName;

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

        internal virtual void Key(IEnumerable<PropertyInfo> keyProperties)
        {
            Contract.Requires(keyProperties != null);

            ClearKey();

            foreach (var property in keyProperties)
            {
                Key(property, OverridableConfigurationParts.None);
            }

            _isKeyConfigured = true;
        }

        public virtual void Key(
            PropertyInfo propertyInfo, OverridableConfigurationParts? overridableConfigurationParts = null)
        {
            Contract.Requires(propertyInfo != null);

            if (!propertyInfo.IsValidEdmScalarProperty())
            {
                throw Error.ModelBuilder_KeyPropertiesMustBePrimitive(propertyInfo.Name, ClrType);
            }

            if (!_isKeyConfigured
                &&
                // DevDiv #324763 (DbModelBuilder.Build is not idempotent):  If build is called twice when keys are configured via attributes 
                // _isKeyConfigured is not set, thus we need to check whether the key has already been included.
                !_keyProperties.ContainsSame(propertyInfo))
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

        public bool IsTableNameConfigured { get; private set; }

        /// <summary>
        ///     True if this configuration can be replaced in the model configuration, false otherwise
        ///     This is only set to true for configurations that are registered automatically via the DbContext
        /// </summary>
        internal bool IsReplaceable { get; set; }

        internal bool IsExplicitEntity { get; set; }

        internal void ReplaceFrom(EntityTypeConfiguration existing)
        {
            if (EntitySetName == null)
            {
                EntitySetName = existing.EntitySetName;
            }
        }

        public virtual string EntitySetName
        {
            get { return _entitySetName; }
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));

                _entitySetName = value;
            }
        }

        internal override IEnumerable<PropertyInfo> ConfiguredProperties
        {
            get { return base.ConfiguredProperties.Union(_navigationPropertyConfigurations.Keys); }
        }

        internal DatabaseName GetTableName()
        {
            if (!IsTableNameConfigured)
            {
                return null;
            }

            return _entityMappingConfigurations.First().TableName;
        }

        public void ToTable(string tableName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(tableName));

            ToTable(tableName, null);
        }

        public void ToTable(string tableName, string schemaName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(tableName));

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
            Contract.Requires(mappingConfiguration != null);

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
                && _entityMappingConfigurations.Where(mc => mc.TableName == null).Any())
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
            Contract.Requires(subType != null);
            Contract.Requires(mappingConfiguration != null);

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
            Contract.Requires(propertyInfo != null);

            NavigationPropertyConfiguration navigationPropertyConfiguration;
            if (!_navigationPropertyConfigurations.TryGetValue(propertyInfo, out navigationPropertyConfiguration))
            {
                _navigationPropertyConfigurations.Add(
                    propertyInfo, navigationPropertyConfiguration = new NavigationPropertyConfiguration(propertyInfo));
            }

            return navigationPropertyConfiguration;
        }

        internal virtual void Configure(EdmEntityType entityType, EdmModel model)
        {
            Contract.Requires(entityType != null);
            Contract.Requires(model != null);

            ConfigureKey(entityType);
            Configure(entityType.Name, entityType.Properties, entityType.Annotations);
            ConfigureAssociations(entityType, model);
            ConfigureEntitySetName(entityType, model);
        }

        private void ConfigureEntitySetName(EdmEntityType entityType, EdmModel model)
        {
            Contract.Requires(entityType != null);
            Contract.Requires(model != null);

            if ((EntitySetName == null)
                || (entityType.BaseType != null))
            {
                return;
            }

            var entitySet = model.GetEntitySet(entityType);

            Contract.Assert(entitySet != null);

            entitySet.Name = model.GetEntitySets().Except(new[] { entitySet }).UniquifyName(EntitySetName);

            entitySet.SetConfiguration(this);
        }

        private void ConfigureKey(EdmEntityType entityType)
        {
            Contract.Requires(entityType != null);

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

                property.PropertyType.IsNullable = false;
                entityType.DeclaredKeyProperties.Add(property);
            }
        }

        private void ConfigureAssociations(EdmEntityType entityType, EdmModel model)
        {
            Contract.Requires(entityType != null);
            Contract.Requires(model != null);

            foreach (var configuration in _navigationPropertyConfigurations)
            {
                var propertyInfo = configuration.Key;
                var navigationPropertyConfiguration = configuration.Value;
                var navigationProperty = entityType.GetNavigationProperty(propertyInfo);

                if (navigationProperty == null)
                {
                    throw Error.NavigationPropertyNotFound(propertyInfo.Name, entityType.Name);
                }

                navigationPropertyConfiguration.Configure(navigationProperty, model, this);
            }
        }

        internal void ConfigureTablesAndConditions(
            DbEntityTypeMapping entityTypeMapping,
            DbDatabaseMapping databaseMapping,
            DbProviderManifest providerManifest)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(providerManifest != null);

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

        internal bool IsMappingAnyInheritedProperty(EdmEntityType entityType)
        {
            return _entityMappingConfigurations.Any(emc => emc.MapsAnyInheritedProperties(entityType));
        }

        internal static void ConfigureUnconfiguredType(
            DbDatabaseMapping databaseMapping, DbProviderManifest providerManifest, EdmEntityType entityType)
        {
            var c = new EntityMappingConfiguration();
            var entityTypeMapping
                = databaseMapping.GetEntityTypeMapping(entityType.GetClrType());
            c.Configure(databaseMapping, providerManifest, entityType, ref entityTypeMapping, false, 0, 1);
        }

        internal void Configure(
            EdmEntityType entityType,
            DbDatabaseMapping databaseMapping,
            DbProviderManifest providerManifest)
        {
            Contract.Requires(entityType != null);
            Contract.Requires(databaseMapping != null);
            Contract.Requires(providerManifest != null);

            var entityTypeMapping
                = databaseMapping.GetEntityTypeMapping(entityType.GetClrType());

            if (entityTypeMapping != null)
            {
                VerifyAllCSpacePropertiesAreMapped(
                    databaseMapping.GetEntityTypeMappings(entityType),
                    entityTypeMapping.EntityType.DeclaredProperties,
                    new List<EdmProperty>());
            }

            ConfigurePropertyMappings(databaseMapping, entityType, providerManifest);
            ConfigureAssociationMappings(databaseMapping, entityType);
            ConfigureDependentKeys(databaseMapping);
        }

        private void ConfigurePropertyMappings(
            DbDatabaseMapping databaseMapping, EdmEntityType entityType, DbProviderManifest providerManifest,
            bool allowOverride = false)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(entityType != null);
            Contract.Requires(providerManifest != null);

            var entityTypeMappings = databaseMapping.GetEntityTypeMappings(entityType);

            var propertyMappings
                = from etm in entityTypeMappings
                  from etmf in etm.TypeMappingFragments
                  from pm in etmf.PropertyMappings
                  select Tuple.Create(pm, etmf.Table);

            Configure(propertyMappings, providerManifest, allowOverride);

            foreach (
                var derivedEntityType in databaseMapping.Model.GetEntityTypes().Where(et => et.BaseType == entityType))
            {
                ConfigurePropertyMappings(databaseMapping, derivedEntityType, providerManifest, true);
            }
        }

        private void ConfigureAssociationMappings(DbDatabaseMapping databaseMapping, EdmEntityType entityType)
        {
            Contract.Requires(databaseMapping != null);

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
                    navigationPropertyConfiguration.Configure(associationSetMapping, databaseMapping);
                }
            }
        }

        private static void ConfigureDependentKeys(DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(databaseMapping != null);

            var defaultSchema = databaseMapping.Database.Schemas.Single();

            foreach (var foreignKeyConstraint in defaultSchema.Tables.SelectMany(t => t.ForeignKeyConstraints))
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

                                var principalColumn = foreignKeyConstraint.PrincipalTable.KeyColumns.ElementAt(i);

                                c.TypeName = principalColumn.TypeName;
                                c.Facets.CopyFrom(principalColumn.Facets);
                            });
            }
        }

        private static void VerifyAllCSpacePropertiesAreMapped(
            IEnumerable<DbEntityTypeMapping> entityTypeMappings, IEnumerable<EdmProperty> properties,
            IList<EdmProperty> propertyPath)
        {
            Contract.Requires(entityTypeMappings != null);

            var entityType = entityTypeMappings.First().EntityType;

            foreach (var property in properties)
            {
                propertyPath.Add(property);

                if (property.PropertyType.IsComplexType)
                {
                    VerifyAllCSpacePropertiesAreMapped(
                        entityTypeMappings,
                        property.PropertyType.ComplexType.DeclaredProperties,
                        propertyPath);
                }
                else if (!entityTypeMappings.SelectMany(etm => etm.TypeMappingFragments)
                              .SelectMany(mf => mf.PropertyMappings)
                              .Where(pm => pm.PropertyPath.SequenceEqual(propertyPath)).Any()
                         && !entityType.IsAbstract)
                {
                    throw Error.InvalidEntitySplittingProperties(entityType.Name);
                }

                propertyPath.Remove(property);
            }
        }
    }
}
