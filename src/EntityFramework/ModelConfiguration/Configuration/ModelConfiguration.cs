// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    // <summary>
    // Allows configuration to be performed for a model.
    // </summary>
    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    internal class ModelConfiguration : ConfigurationBase
    {
        private readonly Dictionary<Type, EntityTypeConfiguration> _entityConfigurations
            = new Dictionary<Type, EntityTypeConfiguration>();

        private readonly Dictionary<Type, ComplexTypeConfiguration> _complexTypeConfigurations
            = new Dictionary<Type, ComplexTypeConfiguration>();

        private readonly HashSet<Type> _ignoredTypes = new HashSet<Type>();

        internal ModelConfiguration()
        {
        }

        private ModelConfiguration(ModelConfiguration source)
        {
            source._entityConfigurations.Each(c => _entityConfigurations.Add(c.Key, c.Value.Clone()));
            source._complexTypeConfigurations.Each(c => _complexTypeConfigurations.Add(c.Key, c.Value.Clone()));

            _ignoredTypes.AddRange(source._ignoredTypes);

            DefaultSchema = source.DefaultSchema;
            ModelNamespace = source.ModelNamespace;
        }

        internal virtual ModelConfiguration Clone()
        {
            return new ModelConfiguration(this);
        }

        // <summary>
        // Gets a collection of types that have been configured in this model including
        // entity types, complex types, and ignored types.
        // </summary>
        public virtual IEnumerable<Type> ConfiguredTypes
        {
            get { return _entityConfigurations.Keys.Union(_complexTypeConfigurations.Keys).Union(_ignoredTypes); }
        }

        internal virtual IEnumerable<Type> Entities
        {
            get { return _entityConfigurations.Keys.Except(_ignoredTypes).ToList(); }
        }

        internal virtual IEnumerable<Type> ComplexTypes
        {
            get { return _complexTypeConfigurations.Keys.Except(_ignoredTypes).ToList(); }
        }

        internal virtual IEnumerable<Type> StructuralTypes
        {
            get { return _entityConfigurations.Keys.Union(_complexTypeConfigurations.Keys).Except(_ignoredTypes).ToList(); }
        }

        // <summary>
        // Gets or sets the default schema name.
        // </summary>
        public string DefaultSchema { get; set; }

        // <summary>
        // Gets or sets the default model namespace.
        // </summary>
        public string ModelNamespace { get; set; }

        internal virtual void Add(EntityTypeConfiguration entityTypeConfiguration)
        {
            DebugCheck.NotNull(entityTypeConfiguration);

            EntityTypeConfiguration existingConfiguration;

            if ((_entityConfigurations.TryGetValue(entityTypeConfiguration.ClrType, out existingConfiguration)
                 && !existingConfiguration.IsReplaceable)
                || _complexTypeConfigurations.ContainsKey(entityTypeConfiguration.ClrType))
            {
                throw Error.DuplicateStructuralTypeConfiguration(entityTypeConfiguration.ClrType);
            }

            if (existingConfiguration != null
                && existingConfiguration.IsReplaceable)
            {
                _entityConfigurations.Remove(existingConfiguration.ClrType);
                entityTypeConfiguration.ReplaceFrom(existingConfiguration);
            }
            else
            {
                entityTypeConfiguration.IsReplaceable = false;
            }

            _entityConfigurations.Add(entityTypeConfiguration.ClrType, entityTypeConfiguration);
        }

        internal virtual void Add(ComplexTypeConfiguration complexTypeConfiguration)
        {
            DebugCheck.NotNull(complexTypeConfiguration);

            if ((_entityConfigurations.ContainsKey(complexTypeConfiguration.ClrType)
                 || _complexTypeConfigurations.ContainsKey(complexTypeConfiguration.ClrType)))
            {
                throw Error.DuplicateStructuralTypeConfiguration(complexTypeConfiguration.ClrType);
            }

            _complexTypeConfigurations.Add(complexTypeConfiguration.ClrType, complexTypeConfiguration);
        }

        // <summary>
        // Registers an entity type as part of the model and returns an object that can
        // be used to configure the entity. This method can be called multiple times
        // for the same entity to perform multiple configurations.
        // </summary>
        // <param name="entityType"> The type to be registered or configured. </param>
        // <returns> The configuration object for the specified entity type. </returns>
        // <remarks>
        // Types registered as an entity type may later be changed to a complex type by
        // the <see cref="ComplexTypeDiscoveryConvention" />.
        // </remarks>
        public virtual EntityTypeConfiguration Entity(Type entityType)
        {
            Check.NotNull(entityType, "entityType");

            return Entity(entityType, false);
        }

        internal virtual EntityTypeConfiguration Entity(Type entityType, bool explicitEntity)
        {
            DebugCheck.NotNull(entityType);

            if (_complexTypeConfigurations.ContainsKey(entityType))
            {
                throw Error.EntityTypeConfigurationMismatch(entityType.Name);
            }

            EntityTypeConfiguration entityTypeConfiguration;
            if (!_entityConfigurations.TryGetValue(entityType, out entityTypeConfiguration))
            {
                _entityConfigurations.Add(
                    entityType,
                    entityTypeConfiguration = new EntityTypeConfiguration(entityType)
                                                  {
                                                      IsExplicitEntity = explicitEntity
                                                  });
            }

            return entityTypeConfiguration;
        }

        // <summary>
        // Registers a type as a complex type in the model and returns an object that
        // can be used to configure the complex type. This method can be called
        // multiple times for the same type to perform multiple configurations.
        // </summary>
        // <param name="complexType"> The type to be registered or configured. </param>
        // <returns> The configuration object for the specified entity type. </returns>
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#")]
        public virtual ComplexTypeConfiguration ComplexType(Type complexType)
        {
            Check.NotNull(complexType, "complexType");

            if (_entityConfigurations.ContainsKey(complexType))
            {
                throw Error.ComplexTypeConfigurationMismatch(complexType.Name);
            }

            ComplexTypeConfiguration complexTypeConfiguration;
            if (!_complexTypeConfigurations.TryGetValue(complexType, out complexTypeConfiguration))
            {
                _complexTypeConfigurations.Add(
                    complexType, complexTypeConfiguration = new ComplexTypeConfiguration(complexType));
            }

            return complexTypeConfiguration;
        }

        // <summary>
        // Excludes a type from the model.
        // </summary>
        // <param name="type"> The type to be excluded. </param>
        public virtual void Ignore(Type type)
        {
            Check.NotNull(type, "type");

            _ignoredTypes.Add(type);
        }

        internal virtual StructuralTypeConfiguration GetStructuralTypeConfiguration(Type type)
        {
            DebugCheck.NotNull(type);

            EntityTypeConfiguration entityTypeConfiguration;
            if (_entityConfigurations.TryGetValue(type, out entityTypeConfiguration))
            {
                return entityTypeConfiguration;
            }

            ComplexTypeConfiguration complexTypeConfiguration;
            if (_complexTypeConfigurations.TryGetValue(type, out complexTypeConfiguration))
            {
                return complexTypeConfiguration;
            }

            return null;
        }

        // <summary>
        // Gets a value indicating whether the specified type has been configured as a
        // complex type in the model.
        // </summary>
        // <param name="type"> The type to test. </param>
        // <returns> True if the type is a complex type; false otherwise. </returns>
        public virtual bool IsComplexType(Type type)
        {
            Check.NotNull(type, "type");

            return _complexTypeConfigurations.ContainsKey(type);
        }

        // <summary>
        // Gets a value indicating whether the specified type has been excluded from
        // the model.
        // </summary>
        // <param name="type"> The type to test. </param>
        // <returns> True if the type is excluded; false otherwise. </returns>
        public virtual bool IsIgnoredType(Type type)
        {
            Check.NotNull(type, "type");

            return _ignoredTypes.Contains(type);
        }

        // <summary>Gets the properties that have been configured in this model for a given type.</summary>
        // <returns>The properties that have been configured in this model.</returns>
        // <param name="type">The type to get configured properties for.</param>
        public virtual IEnumerable<PropertyInfo> GetConfiguredProperties(Type type)
        {
            Check.NotNull(type, "type");

            var structuralTypeConfiguration = GetStructuralTypeConfiguration(type);

            return (structuralTypeConfiguration != null)
                       ? structuralTypeConfiguration.ConfiguredProperties
                       : Enumerable.Empty<PropertyInfo>();
        }

        // <summary>Gets a value indicating whether the specified property is excluded from the model.</summary>
        // <returns>true if the property  is excluded; otherwise, false.</returns>
        // <param name="type">The type that the property belongs to.</param>
        // <param name="propertyInfo">The property to be checked.</param>
        public virtual bool IsIgnoredProperty(Type type, PropertyInfo propertyInfo)
        {
            Check.NotNull(type, "type");
            Check.NotNull(propertyInfo, "propertyInfo");

            var structuralTypeConfiguration = GetStructuralTypeConfiguration(type);

            return structuralTypeConfiguration != null
                   && structuralTypeConfiguration.IgnoredProperties.Any(p => p.IsSameAs(propertyInfo));
        }

        internal void Configure(EdmModel model)
        {
            DebugCheck.NotNull(model);

            ConfigureEntities(model);
            ConfigureComplexTypes(model);
        }

        private void ConfigureEntities(EdmModel model)
        {
            DebugCheck.NotNull(model);

            foreach (var entityTypeConfiguration in ActiveEntityConfigurations)
            {
                ConfigureFunctionMappings(model, entityTypeConfiguration, model.GetEntityType(entityTypeConfiguration.ClrType));
            }

            foreach (var entityTypeConfiguration in ActiveEntityConfigurations)
            {
                entityTypeConfiguration.Configure(model.GetEntityType(entityTypeConfiguration.ClrType), model);
            }
        }

        private void ConfigureFunctionMappings(EdmModel model, EntityTypeConfiguration entityTypeConfiguration, EntityType entityType)
        {
            if (entityTypeConfiguration.ModificationStoredProceduresConfiguration == null)
            {
                return;
            }

            while (entityType.BaseType != null)
            {
                EntityTypeConfiguration baseTypeConfiguration;

                var baseClrType = ((EntityType)entityType.BaseType).GetClrType();

                Debug.Assert(baseClrType != null);

                if (!entityType.BaseType.Abstract
                    && (!_entityConfigurations
                             .TryGetValue(baseClrType, out baseTypeConfiguration)
                        || baseTypeConfiguration.ModificationStoredProceduresConfiguration == null))
                {
                    throw Error.BaseTypeNotMappedToFunctions(
                        baseClrType.Name,
                        entityTypeConfiguration.ClrType.Name);
                }

                entityType = (EntityType)entityType.BaseType;
            }

            // Propagate function mapping down hierarchy
            model.GetSelfAndAllDerivedTypes(entityType)
                 .Each(
                     e =>
                     {
                         var entityConfiguration = Entity(e.GetClrType());

                         if (entityConfiguration.ModificationStoredProceduresConfiguration == null)
                         {
                             entityConfiguration.MapToStoredProcedures();
                         }
                     });
        }

        private void ConfigureComplexTypes(EdmModel model)
        {
            DebugCheck.NotNull(model);

            foreach (var complexTypeConfiguration in ActiveComplexTypeConfigurations)
            {
                var complexType = model.GetComplexType(complexTypeConfiguration.ClrType);

                Debug.Assert(complexType != null);

                complexTypeConfiguration.Configure(complexType);
            }
        }

        internal void Configure(DbDatabaseMapping databaseMapping, DbProviderManifest providerManifest)
        {
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(providerManifest);

            foreach (var structuralTypeConfiguration
                in databaseMapping.Model.ComplexTypes
                                  .Select(ct => ct.GetConfiguration())
                                  .Cast<StructuralTypeConfiguration>()
                                  .Where(c => c != null))
            {
                structuralTypeConfiguration.ConfigurePropertyMappings(
                    databaseMapping.GetComplexPropertyMappings(structuralTypeConfiguration.ClrType).ToList(),
                    providerManifest);
            }

            ConfigureEntityTypes(databaseMapping, providerManifest);
            RemoveRedundantColumnConditions(databaseMapping);
            RemoveRedundantTables(databaseMapping);
            ConfigureTables(databaseMapping.Database);
            ConfigureDefaultSchema(databaseMapping);
            UniquifyFunctionNames(databaseMapping);
            ConfigureFunctionParameters(databaseMapping);
            RemoveDuplicateTphColumns(databaseMapping);
        }

        private static void ConfigureFunctionParameters(DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(databaseMapping);

            foreach (var structuralTypeConfiguration
                in databaseMapping.Model.ComplexTypes
                                  .Select(ct => ct.GetConfiguration())
                                  .Cast<StructuralTypeConfiguration>()
                                  .Where(c => c != null))
            {
                structuralTypeConfiguration.ConfigureFunctionParameters(
                    databaseMapping.GetComplexParameterBindings(structuralTypeConfiguration.ClrType).ToList());
            }

            foreach (var entityType in databaseMapping.Model.EntityTypes.Where(e => e.GetConfiguration() != null))
            {
                var entityTypeConfiguration = (EntityTypeConfiguration)entityType.GetConfiguration();

                entityTypeConfiguration.ConfigureFunctionParameters(databaseMapping, entityType);
            }
        }

        private static void UniquifyFunctionNames(DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(databaseMapping);

            foreach (var modificationStoredProcedureMapping
                in databaseMapping
                    .GetEntitySetMappings()
                    .SelectMany(esm => esm.ModificationFunctionMappings))
            {
                var entityTypeConfiguration
                    = (EntityTypeConfiguration)modificationStoredProcedureMapping.EntityType.GetConfiguration();

                if (entityTypeConfiguration.ModificationStoredProceduresConfiguration == null)
                {
                    continue;
                }

                var modificationStoredProceduresConfiguration
                    = entityTypeConfiguration.ModificationStoredProceduresConfiguration;

                UniquifyFunctionName(
                    databaseMapping,
                    modificationStoredProceduresConfiguration.InsertModificationStoredProcedureConfiguration,
                    modificationStoredProcedureMapping.InsertFunctionMapping);

                UniquifyFunctionName(
                    databaseMapping,
                    modificationStoredProceduresConfiguration.UpdateModificationStoredProcedureConfiguration,
                    modificationStoredProcedureMapping.UpdateFunctionMapping);

                UniquifyFunctionName(
                    databaseMapping,
                    modificationStoredProceduresConfiguration.DeleteModificationStoredProcedureConfiguration,
                    modificationStoredProcedureMapping.DeleteFunctionMapping);
            }

            foreach (var modificationStoredProcedureMapping
                in databaseMapping
                    .GetAssociationSetMappings()
                    .Select(asm => asm.ModificationFunctionMapping)
                    .Where(asm => asm != null))
            {
                var navigationPropertyConfiguration
                    = (NavigationPropertyConfiguration)modificationStoredProcedureMapping
                                                           .AssociationSet.ElementType.GetConfiguration();

                if (navigationPropertyConfiguration.ModificationStoredProceduresConfiguration == null)
                {
                    continue;
                }

                UniquifyFunctionName(
                    databaseMapping,
                    navigationPropertyConfiguration.ModificationStoredProceduresConfiguration.InsertModificationStoredProcedureConfiguration,
                    modificationStoredProcedureMapping.InsertFunctionMapping);

                UniquifyFunctionName(
                    databaseMapping,
                    navigationPropertyConfiguration.ModificationStoredProceduresConfiguration.DeleteModificationStoredProcedureConfiguration,
                    modificationStoredProcedureMapping.DeleteFunctionMapping);
            }
        }

        private static void UniquifyFunctionName(
            DbDatabaseMapping databaseMapping,
            ModificationStoredProcedureConfiguration modificationStoredProcedureConfiguration,
            ModificationFunctionMapping functionMapping)
        {
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(functionMapping);

            if ((modificationStoredProcedureConfiguration == null)
                || string.IsNullOrWhiteSpace(modificationStoredProcedureConfiguration.Name))
            {
                functionMapping.Function.StoreFunctionNameAttribute
                    = databaseMapping.Database.Functions.Except(new[] { functionMapping.Function })
                                     .Select(f => f.FunctionName)
                                     .Uniquify(functionMapping.Function.FunctionName);
            }
        }

        private void ConfigureDefaultSchema(DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(databaseMapping);

            databaseMapping.Database.GetEntitySets()
                           .Where(es => string.IsNullOrWhiteSpace(es.Schema))
                           .Each(es => es.Schema = DefaultSchema ?? EdmModelExtensions.DefaultSchema);

            databaseMapping.Database.Functions
                           .Where(f => string.IsNullOrWhiteSpace(f.Schema))
                           .Each(f => f.Schema = DefaultSchema ?? EdmModelExtensions.DefaultSchema);
        }

        private void ConfigureEntityTypes(DbDatabaseMapping databaseMapping, DbProviderManifest providerManifest)
        {
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(providerManifest);

            var sortedEntityConfigurations =
                SortEntityConfigurationsByInheritance(databaseMapping);

            foreach (var entityTypeConfiguration in sortedEntityConfigurations)
            {
                var entityTypeMapping
                    = databaseMapping.GetEntityTypeMapping(entityTypeConfiguration.ClrType);

                entityTypeConfiguration.ConfigureTablesAndConditions(
                    entityTypeMapping, databaseMapping, providerManifest);

                // run through all unconfigured derived types of the current entityType to make sure the property mappings now point to the right places
                ConfigureUnconfiguredDerivedTypes(
                    databaseMapping,
                    providerManifest,
                    databaseMapping.Model.GetEntityType(entityTypeConfiguration.ClrType),
                    sortedEntityConfigurations);
            }

            new EntityMappingService(databaseMapping).Configure();

            foreach (var entityType in databaseMapping.Model.EntityTypes.Where(e => e.GetConfiguration() != null))
            {
                var entityTypeConfiguration = (EntityTypeConfiguration)entityType.GetConfiguration();

                entityTypeConfiguration.Configure(entityType, databaseMapping, providerManifest);
            }
        }

        private static void ConfigureUnconfiguredDerivedTypes(
            DbDatabaseMapping databaseMapping,
            DbProviderManifest providerManifest,
            EntityType entityType,
            IList<EntityTypeConfiguration> sortedEntityConfigurations)
        {
            var derivedTypes = databaseMapping.Model.GetDerivedTypes(entityType).ToList();
            while (derivedTypes.Count > 0)
            {
                var currentType = derivedTypes[0];
                derivedTypes.RemoveAt(0);

                // Configure the derived type if it is not abstract and is not otherwise configured
                // if the type is not configured, then also run through that type's derived types
                if (!currentType.Abstract
                    && sortedEntityConfigurations.All(etc => etc.ClrType != currentType.GetClrType()))
                {
                    // run through mapping configuration to make sure property mappings point to where the base type is now mapping them
                    EntityTypeConfiguration.ConfigureUnconfiguredType(databaseMapping, providerManifest, currentType);
                    derivedTypes.AddRange(databaseMapping.Model.GetDerivedTypes(currentType));
                }
            }
        }

        private static void ConfigureTables(EdmModel database)
        {
            foreach (var table in database.EntityTypes.ToList())
            {
                ConfigureTable(database, table);
            }
        }

        private static void ConfigureTable(
            EdmModel database, EntityType table)
        {
            DebugCheck.NotNull(table);

            var tableName = table.GetTableName();

            if (tableName == null)
            {
                return;
            }

            var entitySet = database.GetEntitySet(table);

            if (!string.IsNullOrWhiteSpace(tableName.Schema))
            {
                entitySet.Schema = tableName.Schema;
            }

            entitySet.Table = tableName.Name;
        }

        private IList<EntityTypeConfiguration> SortEntityConfigurationsByInheritance(DbDatabaseMapping databaseMapping)
        {
            var entityConfigurationsSortedByInheritance = new List<EntityTypeConfiguration>();

            // Build a list such that parent type appears before its children
            foreach (var entityTypeConfiguration in ActiveEntityConfigurations)
            {
                var entityType = databaseMapping.Model.GetEntityType(entityTypeConfiguration.ClrType);

                if (entityType == null)
                {
                    // for example, when the configuration points to a complex type
                    continue;
                }

                if (entityType.BaseType == null)
                {
                    if (!entityConfigurationsSortedByInheritance.Contains(entityTypeConfiguration))
                    {
                        entityConfigurationsSortedByInheritance.Add(entityTypeConfiguration);
                    }
                }
                else
                {
                    var derivedTypes = new Stack<EntityType>();
                    while (entityType != null)
                    {
                        derivedTypes.Push(entityType);
                        entityType = (EntityType)entityType.BaseType;
                    }

                    while (derivedTypes.Count > 0)
                    {
                        entityType = derivedTypes.Pop();
                        var correspondingEntityConfiguration =
                            ActiveEntityConfigurations.SingleOrDefault(ec => ec.ClrType == entityType.GetClrType());
                        if ((correspondingEntityConfiguration != null)
                            &&
                            (!entityConfigurationsSortedByInheritance.Contains(correspondingEntityConfiguration)))
                        {
                            entityConfigurationsSortedByInheritance.Add(correspondingEntityConfiguration);
                        }
                    }
                }
            }
            return entityConfigurationsSortedByInheritance;
        }

        // <summary>
        // Initializes configurations in the ModelConfiguration so that configuration data
        // is in a single place
        // </summary>
        internal void NormalizeConfigurations()
        {
            DiscoverIndirectlyConfiguredComplexTypes();
            ReassignSubtypeMappings();
        }

        private void DiscoverIndirectlyConfiguredComplexTypes()
        {
            ActiveEntityConfigurations
                .SelectMany(ec => ec.ConfiguredComplexTypes)
                .Each(t => ComplexType(t));
        }

        private void ReassignSubtypeMappings()
        {
            // Re-assign sub-type mapping configurations to entity types
            foreach (var entityTypeConfiguration in ActiveEntityConfigurations)
            {
                foreach (var subTypeAndMappingConfigurationPair in entityTypeConfiguration.SubTypeMappingConfigurations)
                {
                    var subTypeClrType = subTypeAndMappingConfigurationPair.Key;

                    var subTypeEntityConfiguration
                        = ActiveEntityConfigurations
                            .SingleOrDefault(ec => ec.ClrType == subTypeClrType);

                    if (subTypeEntityConfiguration == null)
                    {
                        subTypeEntityConfiguration = new EntityTypeConfiguration(subTypeClrType);

                        _entityConfigurations.Add(subTypeClrType, subTypeEntityConfiguration);
                    }

                    subTypeEntityConfiguration.AddMappingConfiguration(
                        subTypeAndMappingConfigurationPair.Value, cloneable: false);
                }
            }
        }

        private static void RemoveDuplicateTphColumns(DbDatabaseMapping databaseMapping)
        {
            foreach (var table in databaseMapping.Database.EntityTypes)
            {
                var currentTable = table; // Prevent access to foreach variable in closure
                new TphColumnFixer(
                    databaseMapping
                        .GetEntitySetMappings()
                        .SelectMany(e => e.EntityTypeMappings)
                        .SelectMany(e => e.MappingFragments)
                        .Where(f => f.Table == currentTable)
                        .SelectMany(f => f.ColumnMappings)).RemoveDuplicateTphColumns();
            }
        }

        private static void RemoveRedundantColumnConditions(DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(databaseMapping);

            // Remove all the default discriminators where there is only one table using it
            (from esm in databaseMapping.GetEntitySetMappings()
             select new
                        {
                            Set = esm,
                            Fragments =
                 (from etm in esm.EntityTypeMappings
                  from etmf in etm.MappingFragments
                  group etmf by etmf.Table
                      into g
                      where g.Count(x => x.GetDefaultDiscriminator() != null) == 1
                      select g.Single(x => x.GetDefaultDiscriminator() != null))
                        })
                .Each(x => x.Fragments.Each(f => f.RemoveDefaultDiscriminator(x.Set)));
        }

        private static void RemoveRedundantTables(DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(databaseMapping);

            var tables
                = (from t in databaseMapping.Database.EntityTypes
                   where databaseMapping.GetEntitySetMappings()
                                        .SelectMany(esm => esm.EntityTypeMappings)
                                        .SelectMany(etm => etm.MappingFragments)
                                        .All(etmf => etmf.Table != t)
                         && databaseMapping.GetAssociationSetMappings().All(asm => asm.Table != t)
                   select t).ToList();

            tables.Each(
                t =>
                {
                    var tableName = t.GetTableName();

                    if (tableName != null)
                    {
                        throw Error.OrphanedConfiguredTableDetected(tableName);
                    }

                    databaseMapping.Database.RemoveEntityType(t);

                    // Remove any FKs on the removed table
                    var associationTypes
                        = databaseMapping.Database.AssociationTypes
                            .Where(at => at.SourceEnd.GetEntityType() == t
                                        || at.TargetEnd.GetEntityType() == t)
                            .ToList();

                    associationTypes.Each(at => databaseMapping.Database.RemoveAssociationType(at));
                });
        }

        private IEnumerable<EntityTypeConfiguration> ActiveEntityConfigurations
        {
            get
            {
                return (from keyValuePair in _entityConfigurations
                        where !_ignoredTypes.Contains(keyValuePair.Key)
                        select keyValuePair.Value).ToList();
            }
        }

        private IEnumerable<ComplexTypeConfiguration> ActiveComplexTypeConfigurations
        {
            get
            {
                return (from keyValuePair in _complexTypeConfigurations
                        where !_ignoredTypes.Contains(keyValuePair.Key)
                        select keyValuePair.Value).ToList();
            }
        }
    }
}
