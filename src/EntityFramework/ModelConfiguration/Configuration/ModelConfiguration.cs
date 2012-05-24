namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

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
        }

        internal virtual ModelConfiguration Clone()
        {
            return new ModelConfiguration(this);
        }

        internal virtual IEnumerable<Type> ConfiguredTypes
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

        internal string DefaultSchema { get; set; }

        internal virtual void Add(EntityTypeConfiguration entityTypeConfiguration)
        {
            Contract.Requires(entityTypeConfiguration != null);

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
            Contract.Requires(complexTypeConfiguration != null);

            if ((_entityConfigurations.ContainsKey(complexTypeConfiguration.ClrType)
                 || _complexTypeConfigurations.ContainsKey(complexTypeConfiguration.ClrType)))
            {
                throw Error.DuplicateStructuralTypeConfiguration(complexTypeConfiguration.ClrType);
            }

            _complexTypeConfigurations.Add(complexTypeConfiguration.ClrType, complexTypeConfiguration);
        }

        internal virtual EntityTypeConfiguration Entity(Type entityType, bool explicitEntity = false)
        {
            Contract.Requires(entityType != null);

            if (_complexTypeConfigurations.ContainsKey(entityType))
            {
                throw Error.EntityTypeConfigurationMismatch(entityType.FullName);
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

        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#")]
        public virtual ComplexTypeConfiguration ComplexType(Type complexType)
        {
            Contract.Requires(complexType != null);

            if (_entityConfigurations.ContainsKey(complexType))
            {
                throw Error.ComplexTypeConfigurationMismatch(complexType.FullName);
            }

            ComplexTypeConfiguration complexTypeConfiguration;
            if (!_complexTypeConfigurations.TryGetValue(complexType, out complexTypeConfiguration))
            {
                _complexTypeConfigurations.Add(
                    complexType, complexTypeConfiguration = new ComplexTypeConfiguration(complexType));
            }

            return complexTypeConfiguration;
        }

        public virtual void Ignore(Type type)
        {
            Contract.Requires(type != null);

            _ignoredTypes.Add(type);
        }

        internal virtual StructuralTypeConfiguration GetStructuralTypeConfiguration(Type type)
        {
            Contract.Requires(type != null);

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

        internal virtual bool IsComplexType(Type type)
        {
            Contract.Requires(type != null);

            return _complexTypeConfigurations.ContainsKey(type);
        }

        internal virtual bool IsIgnoredType(Type type)
        {
            Contract.Requires(type != null);

            return _ignoredTypes.Contains(type);
        }

        internal virtual IEnumerable<PropertyInfo> GetConfiguredProperties(Type type)
        {
            Contract.Requires(type != null);

            var structuralTypeConfiguration = GetStructuralTypeConfiguration(type);

            return (structuralTypeConfiguration != null)
                       ? structuralTypeConfiguration.ConfiguredProperties
                       : Enumerable.Empty<PropertyInfo>();
        }

        internal virtual bool IsIgnoredProperty(Type type, PropertyInfo propertyInfo)
        {
            Contract.Requires(type != null);
            Contract.Requires(propertyInfo != null);

            var structuralTypeConfiguration = GetStructuralTypeConfiguration(type);

            return (structuralTypeConfiguration != null)
                       ? structuralTypeConfiguration.IgnoredProperties.Any(p => p.IsSameAs(propertyInfo))
                       : false;
        }

        internal void Configure(EdmModel model)
        {
            Contract.Requires(model != null);

            ConfigureEntities(model);
            ConfigureComplexTypes(model);
        }

        internal void ConfigureEntities(EdmModel model)
        {
            Contract.Requires(model != null);

            foreach (var entityTypeConfiguration in ActiveEntityConfigurations)
            {
                var structuralType = entityTypeConfiguration.ClrType;
                var entityType = model.GetEntityType(structuralType);

                Contract.Assert(entityType != null);

                entityTypeConfiguration.Configure(entityType, model);
            }
        }

        private void ConfigureComplexTypes(EdmModel model)
        {
            Contract.Requires(model != null);

            foreach (var complexTypeConfiguration in ActiveComplexTypeConfigurations)
            {
                var complexType = model.GetComplexType(complexTypeConfiguration.ClrType);

                Contract.Assert(complexType != null);

                complexTypeConfiguration.Configure(complexType);
            }
        }

        internal void Configure(DbDatabaseMapping databaseMapping, DbProviderManifest providerManifest)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(providerManifest != null);

            foreach (var structuralTypeConfiguration
                in databaseMapping.Model.GetComplexTypes()
                    .Select(ct => ct.GetConfiguration())
                    .Cast<StructuralTypeConfiguration>()
                    .Where(c => c != null))
            {
                structuralTypeConfiguration.Configure(
                    databaseMapping.GetComplexPropertyMappings(structuralTypeConfiguration.ClrType),
                    providerManifest);
            }

            ConfigureDefaultSchema(databaseMapping);
            ConfigureEntityTypes(databaseMapping, providerManifest);
            RemoveRedundantColumnConditions(databaseMapping);
            RemoveRedundantTables(databaseMapping);
            ConfigureTables(databaseMapping.Database);
        }

        private void ConfigureDefaultSchema(DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(databaseMapping != null);

            if (!string.IsNullOrWhiteSpace(DefaultSchema))
            {
                var defaultSchema = databaseMapping.Database.Schemas.Single();

                defaultSchema.Name = DefaultSchema;
                defaultSchema.DatabaseIdentifier = DefaultSchema;
            }
        }

        private void ConfigureEntityTypes(DbDatabaseMapping databaseMapping, DbProviderManifest providerManifest)
        {
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

            foreach (var entityType in databaseMapping.Model.GetEntityTypes().Where(e => e.GetConfiguration() != null))
            {
                var entityTypeConfiguration = (EntityTypeConfiguration)entityType.GetConfiguration();

                entityTypeConfiguration.Configure(entityType, databaseMapping, providerManifest);
            }
        }

        private static void ConfigureUnconfiguredDerivedTypes(
            DbDatabaseMapping databaseMapping,
            DbProviderManifest providerManifest,
            EdmEntityType entityType,
            IEnumerable<EntityTypeConfiguration> sortedEntityConfigurations)
        {
            var derivedTypes = databaseMapping.Model.GetDerivedTypes(entityType).ToList();
            while (derivedTypes.Count > 0)
            {
                var currentType = derivedTypes[0];
                derivedTypes.RemoveAt(0);

                // Configure the derived type if it is not abstract and is not otherwise configured
                // if the type is not configured, then also run through that type's derived types
                if (!currentType.IsAbstract
                    &&
                    !sortedEntityConfigurations.Any(etc => etc.ClrType == currentType.GetClrType()))
                {
                    // run through mapping configuration to make sure property mappings point to where the base type is now mapping them
                    EntityTypeConfiguration.ConfigureUnconfiguredType(databaseMapping, providerManifest, currentType);
                    derivedTypes.AddRange(databaseMapping.Model.GetDerivedTypes(currentType));
                }
            }
        }

        private static void ConfigureTables(DbDatabaseMetadata database)
        {
            Contract.Requires(database != null);
            Contract.Assert(database.Schemas.Count() == 1);

            var defaultSchema = database.Schemas.Single();

            foreach (var table in defaultSchema.Tables.ToList())
            {
                ConfigureTable(database, defaultSchema, table);
            }
        }

        private static void ConfigureTable(
            DbDatabaseMetadata database, DbSchemaMetadata containingSchema, DbTableMetadata table)
        {
            Contract.Requires(database != null);
            Contract.Requires(containingSchema != null);
            Contract.Requires(table != null);

            var tableName = table.GetTableName();

            if (tableName == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(tableName.Schema)
                && !string.Equals(containingSchema.Name, tableName.Schema, StringComparison.Ordinal))
            {
                containingSchema
                    = database.Schemas
                        .SingleOrDefault(s => string.Equals(s.Name, tableName.Schema, StringComparison.Ordinal));

                if (containingSchema == null)
                {
                    database.Schemas.Add(
                        containingSchema = new DbSchemaMetadata
                                               {
                                                   Name = tableName.Schema,
                                                   DatabaseIdentifier = tableName.Schema
                                               });
                }

                database.RemoveTable(table);
                containingSchema.Tables.Add(table);
            }

            if (!string.Equals(table.DatabaseIdentifier, tableName.Name, StringComparison.Ordinal))
            {
                table.DatabaseIdentifier = tableName.Name;
            }
        }

        private IEnumerable<EntityTypeConfiguration> SortEntityConfigurationsByInheritance(
            DbDatabaseMapping databaseMapping)
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
                    var derivedTypes = new Stack<EdmEntityType>();
                    while (entityType != null)
                    {
                        derivedTypes.Push(entityType);
                        entityType = entityType.BaseType;
                    }

                    while (derivedTypes.Count > 0)
                    {
                        entityType = derivedTypes.Pop();
                        var correspondingEntityConfiguration =
                            ActiveEntityConfigurations.Where(ec => ec.ClrType == entityType.GetClrType()).
                                SingleOrDefault();
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

        /// <summary>
        ///     Initializes configurations in the ModelConfiguration so that configuration data
        ///     is in a single place
        /// </summary>
        public void NormalizeConfigurations()
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

        private static void RemoveRedundantColumnConditions(DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(databaseMapping != null);

            // Remove all the default discriminators where there is only one table using it
            (from esm in databaseMapping.GetEntitySetMappings()
             select new
                        {
                            Set = esm,
                            Fragments =
                 (from etm in esm.EntityTypeMappings
                  from etmf in etm.TypeMappingFragments
                  group etmf by etmf.Table
                  into g
                  where g.Count(x => x.GetDefaultDiscriminator() != null) == 1
                  select g.Single(x => x.GetDefaultDiscriminator() != null))
                        })
                .Each(x => x.Fragments.Each(f => f.RemoveDefaultDiscriminator(x.Set)));
        }

        private static void RemoveRedundantTables(DbDatabaseMapping databaseMapping)
        {
            Contract.Assert(databaseMapping != null);

            var tables
                = (from t in databaseMapping.Database.Schemas.SelectMany(s => s.Tables)
                   where !databaseMapping.GetEntitySetMappings()
                              .SelectMany(esm => esm.EntityTypeMappings)
                              .SelectMany(etm => etm.TypeMappingFragments)
                              .Any(etmf => etmf.Table == t)
                         && !databaseMapping.GetAssociationSetMappings()
                                 .Any(asm => asm.Table == t)
                   select t).ToList();

            tables.Each(
                t =>
                    {
                        var tableName = t.GetTableName();

                        if (tableName != null)
                        {
                            throw Error.OrphanedConfiguredTableDetected(tableName);
                        }

                        databaseMapping.Database.RemoveTable(t);
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
