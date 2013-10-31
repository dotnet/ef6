// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Services;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    // Equivalent to a mapping fragment in the MSL
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal class EntityMappingConfiguration
    {
        #region Fields and constructors

        private DatabaseName _tableName;
        private List<PropertyPath> _properties;
        private readonly List<ValueConditionConfiguration> _valueConditions = new List<ValueConditionConfiguration>();

        private readonly List<NotNullConditionConfiguration> _notNullConditions =
            new List<NotNullConditionConfiguration>();

        private readonly Dictionary<PropertyPath, PrimitivePropertyConfiguration> _primitivePropertyConfigurations
            = new Dictionary<PropertyPath, PrimitivePropertyConfiguration>();

        internal EntityMappingConfiguration()
        {
        }

        private EntityMappingConfiguration(EntityMappingConfiguration source)
        {
            DebugCheck.NotNull(source);

            _tableName = source._tableName;

            MapInheritedProperties = source.MapInheritedProperties;

            if (source._properties != null)
            {
                _properties = new List<PropertyPath>(source._properties);
            }

            _valueConditions.AddRange(source._valueConditions.Select(c => c.Clone(this)));
            _notNullConditions.AddRange(source._notNullConditions.Select(c => c.Clone(this)));

            source._primitivePropertyConfigurations.Each(
                c => _primitivePropertyConfigurations.Add(c.Key, c.Value.Clone()));
        }

        internal virtual EntityMappingConfiguration Clone()
        {
            return new EntityMappingConfiguration(this);
        }

        #endregion

        #region Properties

        public bool MapInheritedProperties { get; set; }

        public DatabaseName TableName
        {
            get { return _tableName; }
            set
            {
                DebugCheck.NotNull(value);

                _tableName = value;
            }
        }

        internal List<PropertyPath> Properties
        {
            get { return _properties; }
            set
            {
                DebugCheck.NotNull(value);
                if (_properties == null)
                {
                    _properties = new List<PropertyPath>();
                }
                value.Each(Property);
            }
        }

        internal IDictionary<PropertyPath, PrimitivePropertyConfiguration> PrimitivePropertyConfigurations
        {
            get { return _primitivePropertyConfigurations; }
        }

        internal TPrimitivePropertyConfiguration Property<TPrimitivePropertyConfiguration>(
            PropertyPath propertyPath, Func<TPrimitivePropertyConfiguration> primitivePropertyConfigurationCreator)
            where TPrimitivePropertyConfiguration : PrimitivePropertyConfiguration
        {
            DebugCheck.NotNull(propertyPath);

            if (_properties == null)
            {
                _properties = new List<PropertyPath>();
            }
            Property(propertyPath);

            PrimitivePropertyConfiguration primitivePropertyConfiguration;
            if (!_primitivePropertyConfigurations.TryGetValue(propertyPath, out primitivePropertyConfiguration))
            {
                _primitivePropertyConfigurations.Add(propertyPath,
                    primitivePropertyConfiguration = primitivePropertyConfigurationCreator());
            }

            return (TPrimitivePropertyConfiguration)primitivePropertyConfiguration;
        }

        private void Property(PropertyPath property)
        {
            DebugCheck.NotNull(property);

            if (!_properties.Where(pp => pp.SequenceEqual(property)).Any())
            {
                _properties.Add(property);
            }
        }
        #endregion

        #region Condition Properties

        public List<ValueConditionConfiguration> ValueConditions
        {
            get { return _valueConditions; }
        }

        public void AddValueCondition(ValueConditionConfiguration valueCondition)
        {
            DebugCheck.NotNull(valueCondition);

            var existingValueCondition =
                ValueConditions
                    .Where(vc => vc.Discriminator.Equals(valueCondition.Discriminator, StringComparison.Ordinal))
                    .SingleOrDefault();

            if (existingValueCondition == null)
            {
                ValueConditions.Add(valueCondition);
            }
            else
            {
                existingValueCondition.Value = valueCondition.Value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public List<NotNullConditionConfiguration> NullabilityConditions
        {
            get { return _notNullConditions; }
            set
            {
                DebugCheck.NotNull(value);

                value.Each(AddNullabilityCondition);
            }
        }

        public void AddNullabilityCondition(NotNullConditionConfiguration notNullConditionConfiguration)
        {
            DebugCheck.NotNull(notNullConditionConfiguration);

            if (!NullabilityConditions.Contains(notNullConditionConfiguration))
            {
                NullabilityConditions.Add(notNullConditionConfiguration);
            }
        }

        #endregion

        public bool MapsAnyInheritedProperties(EntityType entityType)
        {
            var properties = new HashSet<EdmPropertyPath>();
            if (Properties != null)
            {
                Properties.Each(
                    p =>
                    properties.AddRange(PropertyPathToEdmPropertyPath(p, entityType)));
            }
            return MapInheritedProperties ||
                   properties.Any(
                       x =>
                       !entityType.KeyProperties().Contains(x.First())
                       && !entityType.DeclaredProperties.Contains(x.First()));
        }

        public void Configure(
            DbDatabaseMapping databaseMapping,
            DbProviderManifest providerManifest,
            EntityType entityType,
            ref StorageEntityTypeMapping entityTypeMapping,
            bool isMappingAnyInheritedProperty,
            int configurationIndex,
            int configurationCount)
        {
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(providerManifest);

            var isIdentityTable = entityType.BaseType == null && configurationIndex == 0;

            var fragment = FindOrCreateTypeMappingFragment(
                databaseMapping, ref entityTypeMapping, configurationIndex, entityType, providerManifest);
            var fromTable = fragment.Table;
            bool isTableSharing;
            var toTable = FindOrCreateTargetTable(
                databaseMapping, fragment, entityType, fromTable, out isTableSharing);

            var isSharingTableWithBase = DiscoverIsSharingWithBase(databaseMapping, entityType, toTable);

            // Ensure all specified properties are the only ones present in this fragment and table
            var mappingsToContain = DiscoverAllMappingsToContain(
                databaseMapping, entityType, toTable, isSharingTableWithBase);

            // Validate that specified properties can be mapped
            var mappingsToMove = fragment.ColumnMappings.ToList();

            foreach (var propertyPath in mappingsToContain)
            {
                var propertyMapping = fragment.ColumnMappings.SingleOrDefault(
                    pm =>
                    pm.PropertyPath.SequenceEqual(propertyPath));

                if (propertyMapping == null)
                {
                    throw Error.EntityMappingConfiguration_DuplicateMappedProperty(
                        entityType.Name, propertyPath.ToString());
                }
                mappingsToMove.Remove(propertyMapping);
            }

            // Add table constraint if there are no inherited properties
            if (!isIdentityTable)
            {
                bool isSplitting;
                var parentTable = FindParentTable(
                    databaseMapping,
                    fromTable,
                    entityTypeMapping,
                    toTable,
                    isMappingAnyInheritedProperty,
                    configurationIndex,
                    configurationCount,
                    out isSplitting);
                if (parentTable != null)
                {
                    DatabaseOperations.AddTypeConstraint(databaseMapping.Database, entityType, parentTable, toTable, isSplitting);
                }
            }

            // Update AssociationSetMappings (IAs) and FKs
            if (fromTable != toTable)
            {
                if (Properties == null)
                {
                    AssociationMappingOperations.MoveAllDeclaredAssociationSetMappings(
                        databaseMapping, entityType, fromTable, toTable, !isTableSharing);
                    ForeignKeyPrimitiveOperations.MoveAllDeclaredForeignKeyConstraintsForPrimaryKeyColumns(
                        entityType, fromTable, toTable);
                }
                if (isMappingAnyInheritedProperty)
                {
                    // With TPC, we need to move down FK constraints, even on PKs (except type mapping constraints that are not about associations)
                    ForeignKeyPrimitiveOperations.CopyAllForeignKeyConstraintsForPrimaryKeyColumns(
                        databaseMapping.Database, fromTable, toTable);
                }
            }

            if (mappingsToMove.Any())
            {
                EntityType extraTable = null;
                if (configurationIndex < configurationCount - 1)
                {
                    // Move all extra properties to a single new fragment
                    var anyPropertyMapping = mappingsToMove.First();

                    extraTable
                        = FindTableForTemporaryExtraPropertyMapping(
                            databaseMapping, entityType, fromTable, toTable, anyPropertyMapping);

                    var extraFragment
                        = EntityMappingOperations
                            .CreateTypeMappingFragment(entityTypeMapping, fragment, databaseMapping.Database.GetEntitySet(extraTable));

                    var requiresUpdate = extraTable != fromTable;

                    foreach (var pm in mappingsToMove)
                    {
                        // move the property mapping from toFragment to extraFragment
                        EntityMappingOperations.MovePropertyMapping(
                            databaseMapping, fragment, extraFragment, pm, requiresUpdate, true);
                    }
                }
                else
                {
                    // Move each extra property mapping to a fragment refering to the table with the base mapping
                    EntityType unmappedTable = null;
                    foreach (var pm in mappingsToMove)
                    {
                        extraTable = FindTableForExtraPropertyMapping(
                            databaseMapping, entityType, fromTable, toTable, ref unmappedTable, pm);

                        var extraFragment =
                            entityTypeMapping.MappingFragments.SingleOrDefault(tmf => tmf.Table == extraTable);

                        if (extraFragment == null)
                        {
                            extraFragment
                                = EntityMappingOperations
                                    .CreateTypeMappingFragment(
                                        entityTypeMapping, fragment, databaseMapping.Database.GetEntitySet(extraTable));

                            extraFragment.SetIsUnmappedPropertiesFragment(true);
                        }

                        if (extraTable == fromTable)
                        {
                            // move the default discriminator along with the properties
                            MoveDefaultDiscriminator(fragment, extraFragment);
                        }

                        var requiresUpdate = extraTable != fromTable;
                        EntityMappingOperations.MovePropertyMapping(
                            databaseMapping, fragment, extraFragment, pm, requiresUpdate, true);
                    }
                }
            }

            // Ensure all property mappings refer to the table in the fragment
            // Uniquify: true if table sharing, false otherwise
            //           FK names should be uniquified
            //           declared properties are moved, inherited ones are copied (duplicated)
            EntityMappingOperations.UpdatePropertyMappings(
                databaseMapping, fromTable, fragment, !isTableSharing);

            // Configure Conditions for the fragment
            ConfigureDefaultDiscriminator(entityType, fragment);
            ConfigureConditions(databaseMapping, entityType, fragment, providerManifest);

            // Ensure all conditions refer to columns on the table in the fragment
            EntityMappingOperations.UpdateConditions(databaseMapping.Database, fromTable, fragment);

            ForeignKeyPrimitiveOperations.UpdatePrincipalTables(
                databaseMapping, entityType, fromTable, toTable, isMappingAnyInheritedProperty);

            CleanupUnmappedArtifacts(databaseMapping, fromTable);
            CleanupUnmappedArtifacts(databaseMapping, toTable);

            toTable.SetConfiguration(this);
        }

        internal void ConfigurePropertyMappings(
            IList<Tuple<ColumnMappingBuilder, EntityType>> propertyMappings,
            DbProviderManifest providerManifest,
            bool allowOverride = false)
        {
            DebugCheck.NotNull(propertyMappings);
            DebugCheck.NotNull(providerManifest);

            foreach (var configuration in _primitivePropertyConfigurations)
            {
                var propertyPath = configuration.Key;
                var propertyConfiguration = configuration.Value;

                propertyConfiguration.Configure(
                    propertyMappings.Where(
                        pm =>
                        propertyPath.Equals(
                            new PropertyPath(
                            pm.Item1.PropertyPath
                                .Skip(pm.Item1.PropertyPath.Count - propertyPath.Count)
                                .Select(p => p.GetClrPropertyInfo()))
                            )
                        && Object.Equals(TableName, pm.Item2.GetTableName())),
                    providerManifest,
                    allowOverride,
                    fillFromExistingConfiguration: true);
            }
        }

        private void ConfigureDefaultDiscriminator(
            EntityType entityType, StorageMappingFragment fragment)
        {
            if (ValueConditions.Any() || NullabilityConditions.Any())
            {
                var discriminator = fragment.RemoveDefaultDiscriminatorCondition();
                if (discriminator != null
                    && entityType.BaseType != null)
                {
                    discriminator.Nullable = true;
                }
            }
        }

        private static void MoveDefaultDiscriminator(
            StorageMappingFragment fromFragment, StorageMappingFragment toFragment)
        {
            var discriminatorColumn = fromFragment.GetDefaultDiscriminator();
            if (discriminatorColumn != null)
            {
                var discriminator = fromFragment.ColumnConditions.SingleOrDefault(
                    cc => cc.ColumnProperty == discriminatorColumn);
                if (discriminator != null)
                {
                    fromFragment.RemoveDefaultDiscriminatorAnnotation();
                    fromFragment.RemoveConditionProperty(discriminator);
                    toFragment.AddDiscriminatorCondition(discriminator.ColumnProperty, discriminator.Value);
                    toFragment.SetDefaultDiscriminator(discriminator.ColumnProperty);
                }
            }
        }

        private static EntityType FindTableForTemporaryExtraPropertyMapping(
            DbDatabaseMapping databaseMapping,
            EntityType entityType,
            EntityType fromTable,
            EntityType toTable,
            ColumnMappingBuilder pm)
        {
            var extraTable = fromTable;
            if (fromTable == toTable)
            {
                extraTable = databaseMapping.Database.AddTable(entityType.Name, fromTable);
            }
            else if (entityType.BaseType == null)
            {
                extraTable = fromTable;
            }
            else
            {
                // find where the base mappings are and put them in that table
                extraTable = FindBaseTableForExtraPropertyMapping(databaseMapping, entityType, pm);
                if (extraTable == null)
                {
                    extraTable = fromTable;
                }
            }
            return extraTable;
        }

        private static EntityType FindTableForExtraPropertyMapping(
            DbDatabaseMapping databaseMapping,
            EntityType entityType,
            EntityType fromTable,
            EntityType toTable,
            ref EntityType unmappedTable,
            ColumnMappingBuilder pm)
        {
            var extraTable = FindBaseTableForExtraPropertyMapping(databaseMapping, entityType, pm);

            if (extraTable == null)
            {
                if (fromTable != toTable
                    && entityType.BaseType == null)
                {
                    return fromTable;
                }

                if (unmappedTable == null)
                {
                    unmappedTable = databaseMapping.Database.AddTable(fromTable.Name, fromTable);
                }
                extraTable = unmappedTable;
            }

            return extraTable;
        }

        private static EntityType FindBaseTableForExtraPropertyMapping(
            DbDatabaseMapping databaseMapping, EntityType entityType, ColumnMappingBuilder pm)
        {
            var baseType = (EntityType)entityType.BaseType;

            StorageMappingFragment baseFragment = null;

            while (baseType != null
                   && baseFragment == null)
            {
                var baseMapping = databaseMapping.GetEntityTypeMapping(baseType);
                if (baseMapping != null)
                {
                    baseFragment =
                        baseMapping.MappingFragments.SingleOrDefault(
                            f => f.ColumnMappings.Any(bpm => bpm.PropertyPath.SequenceEqual(pm.PropertyPath)));

                    if (baseFragment != null)
                    {
                        return baseFragment.Table;
                    }
                }
                baseType = (EntityType)baseType.BaseType;
            }
            return null;
        }

        private bool DiscoverIsSharingWithBase(
            DbDatabaseMapping databaseMapping, EntityType entityType, EntityType toTable)
        {
            var isSharingTableWithBase = false;

            if (entityType.BaseType != null)
            {
                var baseType = entityType.BaseType;
                var anyBaseMappings = false;

                while (baseType != null
                       && !isSharingTableWithBase)
                {
                    var baseMappings = databaseMapping.GetEntityTypeMappings((EntityType)baseType);

                    if (baseMappings.Any())
                    {
                        isSharingTableWithBase =
                            baseMappings.SelectMany(m => m.MappingFragments).Any(tmf => tmf.Table == toTable);
                        anyBaseMappings = true;
                    }

                    baseType = baseType.BaseType;
                }

                if (!anyBaseMappings)
                {
                    isSharingTableWithBase = TableName == null || string.IsNullOrWhiteSpace(TableName.Name);
                }
            }
            return isSharingTableWithBase;
        }

        private static EntityType FindParentTable(
            DbDatabaseMapping databaseMapping,
            EntityType fromTable,
            StorageEntityTypeMapping entityTypeMapping,
            EntityType toTable,
            bool isMappingInheritedProperties,
            int configurationIndex,
            int configurationCount,
            out bool isSplitting)
        {
            EntityType parentTable = null;
            isSplitting = false;
            // Check for entity splitting first, since splitting on a derived type in TPT/TPC will always have fromTable != toTable
            if (entityTypeMapping.UsesOtherTables(toTable)
                || configurationCount > 1)
            {
                if (configurationIndex != 0)
                {
                    // Entity Splitting case
                    parentTable = entityTypeMapping.GetPrimaryTable();
                    isSplitting = true;
                }
            }

            if (parentTable == null
                && fromTable != toTable
                && !isMappingInheritedProperties)
            {
                // TPT case
                var baseType = entityTypeMapping.EntityType.BaseType;
                while (baseType != null
                       && parentTable == null)
                {
                    // Traverse to first anscestor with a mapping
                    var baseMapping = databaseMapping.GetEntityTypeMappings((EntityType)baseType).FirstOrDefault();
                    if (baseMapping != null)
                    {
                        parentTable = baseMapping.GetPrimaryTable();
                    }
                    baseType = baseType.BaseType;
                }
            }

            return parentTable;
        }

        private StorageMappingFragment FindOrCreateTypeMappingFragment(
            DbDatabaseMapping databaseMapping,
            ref StorageEntityTypeMapping entityTypeMapping,
            int configurationIndex,
            EntityType entityType,
            DbProviderManifest providerManifest)
        {
            StorageMappingFragment fragment = null;

            if (entityTypeMapping == null)
            {
                Debug.Assert(entityType.Abstract);
                new TableMappingGenerator(providerManifest).
                    Generate(entityType, databaseMapping);
                entityTypeMapping = databaseMapping.GetEntityTypeMapping(entityType);
                configurationIndex = 0;
            }

            if (configurationIndex < entityTypeMapping.MappingFragments.Count)
            {
                fragment = entityTypeMapping.MappingFragments[configurationIndex];
            }
            else
            {
                if (MapInheritedProperties)
                {
                    throw Error.EntityMappingConfiguration_DuplicateMapInheritedProperties(entityType.Name);
                }
                else if (Properties == null)
                {
                    throw Error.EntityMappingConfiguration_DuplicateMappedProperties(entityType.Name);
                }
                else
                {
                    Properties.Each(
                        p =>
                            {
                                if (
                                    PropertyPathToEdmPropertyPath(p, entityType).Any(
                                        pp => !entityType.KeyProperties().Contains(pp.First())))
                                {
                                    throw Error.EntityMappingConfiguration_DuplicateMappedProperty(
                                        entityType.Name, p.ToString());
                                }
                            });
                }

                // Special case where they've asked for an extra table related to this type that only will include the PK columns
                // Uniquify: can be false, always move to a new table
                var templateTable = entityTypeMapping.MappingFragments[0].Table;

                var table = databaseMapping.Database.AddTable(templateTable.Name, templateTable);

                fragment
                    = EntityMappingOperations.CreateTypeMappingFragment(
                        entityTypeMapping,
                        entityTypeMapping.MappingFragments[0],
                        databaseMapping.Database.GetEntitySet(table));
            }
            return fragment;
        }

        private EntityType FindOrCreateTargetTable(
            DbDatabaseMapping databaseMapping,
            StorageMappingFragment fragment,
            EntityType entityType,
            EntityType fromTable,
            out bool isTableSharing)
        {
            EntityType toTable;
            isTableSharing = false;

            if (TableName == null)
            {
                toTable = fragment.Table;
            }
            else
            {
                toTable = databaseMapping.Database.FindTableByName(TableName);

                if (toTable == null)
                {
                    if (entityType.BaseType == null)
                    {
                        // Rule: base type's always own the fragment's initial table
                        toTable = fragment.Table;
                    }
                    else
                    {
                        toTable = databaseMapping.Database.AddTable(TableName.Name, fromTable);
                    }
                }

                // Validate this table can be used and update as needed if it is
                isTableSharing = UpdateColumnNamesForTableSharing(databaseMapping, entityType, toTable, fragment);

                fragment.TableSet = databaseMapping.Database.GetEntitySet(toTable);

                // Make sure that the fragment points to any existing key columns as these shouldn't be duplicated
                foreach (var columnMapping in fragment.ColumnMappings.Where(cm => cm.ColumnProperty.IsPrimaryKeyColumn))
                {
                    var column = toTable.Properties.SingleOrDefault(
                            c => string.Equals(c.Name, columnMapping.ColumnProperty.Name, StringComparison.Ordinal));
                    columnMapping.ColumnProperty = column ?? columnMapping.ColumnProperty;
                }

                toTable.SetTableName(TableName);
            }

            return toTable;
        }

        private HashSet<EdmPropertyPath> DiscoverAllMappingsToContain(
            DbDatabaseMapping databaseMapping, EntityType entityType, EntityType toTable,
            bool isSharingTableWithBase)
        {
            // Ensure all specified properties are the only ones present in this fragment and table
            var mappingsToContain = new HashSet<EdmPropertyPath>();

            // Include Key Properties always
            entityType.KeyProperties().Each(
                p =>
                mappingsToContain.AddRange(p.ToPropertyPathList()));

            // Include All Inherited Properties
            if (MapInheritedProperties)
            {
                entityType.Properties.Except(entityType.DeclaredProperties).Each(
                    p =>
                    mappingsToContain.AddRange(p.ToPropertyPathList()));
            }

            // If sharing table with base type, include all the mappings that the base has
            if (isSharingTableWithBase)
            {
                var baseMappingsToContain = new HashSet<EdmPropertyPath>();
                var baseType = (EntityType)entityType.BaseType;
                StorageEntityTypeMapping baseMapping = null;
                StorageMappingFragment baseFragment = null;
                // if the base is abstract it may have no mapping so look upwards until you find either:
                //   1. a type with mappings and 
                //   2. if none can be found (abstract until the root or hit another table), then include all declared properties on that base type
                while (baseType != null
                       && baseMapping == null)
                {
                    baseMapping = databaseMapping.GetEntityTypeMapping((EntityType)entityType.BaseType);
                    if (baseMapping != null)
                    {
                        baseFragment = baseMapping.MappingFragments.SingleOrDefault(tmf => tmf.Table == toTable);
                    }

                    if (baseFragment == null)
                    {
                        baseType.DeclaredProperties.Each(
                            p =>
                            baseMappingsToContain.AddRange(p.ToPropertyPathList()));
                    }

                    baseType = (EntityType)baseType.BaseType;
                }

                if (baseFragment != null)
                {
                    foreach (var pm in baseFragment.ColumnMappings)
                    {
                        mappingsToContain.Add(new EdmPropertyPath(pm.PropertyPath));
                    }
                }

                mappingsToContain.AddRange(baseMappingsToContain);
            }

            if (Properties == null)
            {
                // Include All Declared Properties
                entityType.DeclaredProperties.Each(
                    p =>
                    mappingsToContain.AddRange(p.ToPropertyPathList()));
            }
            else
            {
                // Include Specific Properties
                Properties.Each(
                    p =>
                    mappingsToContain.AddRange(PropertyPathToEdmPropertyPath(p, entityType)));
            }

            return mappingsToContain;
        }

        private void ConfigureConditions(
            DbDatabaseMapping databaseMapping,
            EntityType entityType,
            StorageMappingFragment fragment,
            DbProviderManifest providerManifest)
        {
            if (ValueConditions.Any()
                || NullabilityConditions.Any())
            {
                fragment.ClearConditions();

                foreach (var condition in ValueConditions)
                {
                    condition.Configure(databaseMapping, fragment, entityType, providerManifest);
                }

                foreach (var condition in NullabilityConditions)
                {
                    condition.Configure(databaseMapping, fragment, entityType);
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal static void CleanupUnmappedArtifacts(DbDatabaseMapping databaseMapping, EntityType table)
        {
            var associationMappings = databaseMapping.EntityContainerMappings
                                                     .SelectMany(ecm => ecm.AssociationSetMappings)
                                                     .Where(asm => asm.Table == table)
                                                     .ToArray();

            var entityFragments = databaseMapping.EntityContainerMappings
                                                 .SelectMany(ecm => ecm.EntitySetMappings)
                                                 .SelectMany(esm => esm.EntityTypeMappings)
                                                 .SelectMany(etm => etm.MappingFragments).Where(f => f.Table == table).ToArray();

            if (!associationMappings.Any()
                && !entityFragments.Any())
            {
                databaseMapping.Database.RemoveEntityType(table);

                databaseMapping.Database.AssociationTypes
                    .Where(t => t.SourceEnd.GetEntityType() == table
                        || t.TargetEnd.GetEntityType() == table)
                    .ToArray()
                    .Each(t => databaseMapping.Database.RemoveAssociationType(t));
            }
            else
            {
                // check the columns of table to see if they are actually used in any fragment
                foreach (var column in table.Properties.ToArray())
                {
                    if (entityFragments.SelectMany(f => f.ColumnMappings).All(pm => pm.ColumnProperty != column)
                        && entityFragments.SelectMany(f => f.ColumnConditions).All(cc => cc.ColumnProperty != column)
                        && associationMappings.SelectMany(am => am.SourceEndMapping.PropertyMappings).All(pm => pm.ColumnProperty != column)
                        && associationMappings.SelectMany(am => am.SourceEndMapping.PropertyMappings).All(pm => pm.ColumnProperty != column))
                    {
                        // Remove table FKs that refer to this column, and then remove the column
                        ForeignKeyPrimitiveOperations.RemoveAllForeignKeyConstraintsForColumn(table, column);
                        TablePrimitiveOperations.RemoveColumn(table, column);
                    }
                }

                // Remove FKs where Principal Table == Dependent Table and the PK == FK (redundant)
                table.ForeignKeyBuilders
                     .Where(fk => fk.PrincipalTable == table && fk.DependentColumns.SequenceEqual(table.KeyProperties))
                     .ToArray()
                     .Each(table.RemoveForeignKey);
            }
        }

        internal static IEnumerable<EdmPropertyPath> PropertyPathToEdmPropertyPath(
            PropertyPath path, EntityType entityType)
        {
            var propertyPath = new List<EdmProperty>();
            StructuralType propertyOwner = entityType;
            for (var i = 0; i < path.Count; i++)
            {
                var edmProperty =
                    propertyOwner.Members.OfType<EdmProperty>().SingleOrDefault(
                        p => p.GetClrPropertyInfo().IsSameAs(path[i]));
                if (edmProperty == null)
                {
                    throw Error.EntityMappingConfiguration_CannotMapIgnoredProperty(entityType.Name, path.ToString());
                }
                propertyPath.Add(edmProperty);
                if (edmProperty.IsComplexType)
                {
                    propertyOwner = edmProperty.ComplexType;
                }
            }

            var lastProperty = propertyPath.Last();
            if (lastProperty.IsUnderlyingPrimitiveType)
            {
                return new[] { new EdmPropertyPath(propertyPath) };
            }
            else if (lastProperty.IsComplexType)
            {
                propertyPath.Remove(lastProperty);
                return lastProperty.ToPropertyPathList(propertyPath);
            }

            return new[] { EdmPropertyPath.Empty };
        }

        private static IEnumerable<EntityType> FindAllTypesUsingTable(
            DbDatabaseMapping databaseMapping, EntityType toTable)
        {
            return databaseMapping.EntityContainerMappings
                                  .SelectMany(ecm => ecm.EntitySetMappings)
                                  .SelectMany(esm => esm.EntityTypeMappings)
                                  .Where(etm => etm.MappingFragments.Any(tmf => tmf.Table == toTable))
                                  .Select(etm => etm.EntityType);
        }

        private static IEnumerable<AssociationType> FindAllOneToOneFKAssociationTypes(
            EdmModel model, EntityType entityType, EntityType candidateType)
        {
            return model.Containers.SelectMany(ec => ec.AssociationSets)
                        .Where(
                            aset => (aset.ElementType.Constraint != null &&
                                     aset.ElementType.SourceEnd.RelationshipMultiplicity == RelationshipMultiplicity.One &&
                                     aset.ElementType.TargetEnd.RelationshipMultiplicity == RelationshipMultiplicity.One) &&
                                    ((aset.ElementType.SourceEnd.GetEntityType() == entityType
                                      && aset.ElementType.TargetEnd.GetEntityType() == candidateType) ||
                                     (aset.ElementType.TargetEnd.GetEntityType() == entityType
                                      && aset.ElementType.SourceEnd.GetEntityType() == candidateType)))
                        .Select(aset => aset.ElementType);
        }

        private static bool UpdateColumnNamesForTableSharing(
            DbDatabaseMapping databaseMapping, EntityType entityType, EntityType toTable,
            StorageMappingFragment fragment)
        {
            // Validate: this table can be used only if:
            //  1. The table is not used by any other type
            //  2. The table is used only by types in the same type hierarchy (TPH)
            //  3. There is a 1:1 relationship and the PK count and types match (Table Splitting)
            var typesSharingTable = FindAllTypesUsingTable(databaseMapping, toTable);
            var associationsToSharedTable = new Dictionary<EntityType, List<AssociationType>>();

            foreach (var candidateType in typesSharingTable)
            {
                var oneToOneAssocations = FindAllOneToOneFKAssociationTypes(
                    databaseMapping.Model, entityType, candidateType);

                var rootType = candidateType.GetRootType();
                if (!associationsToSharedTable.ContainsKey(rootType))
                {
                    associationsToSharedTable.Add(rootType, oneToOneAssocations.ToList());
                }
                else
                {
                    associationsToSharedTable[rootType].AddRange(oneToOneAssocations);
                }
            }
            foreach (var candidateTypePair in associationsToSharedTable)
            {
                // Check if these types are in a TPH hierarchy
                if (candidateTypePair.Key != entityType.GetRootType()
                    && candidateTypePair.Value.Count == 0)
                {
                    var tableName = toTable.GetTableName();

                    throw Error.EntityMappingConfiguration_InvalidTableSharing(
                        entityType.Name, candidateTypePair.Key.Name,
                        tableName != null ? tableName.Name : databaseMapping.Database.GetEntitySet(toTable).Table);
                }
            }

            var allAssociations = associationsToSharedTable.Values.SelectMany(l => l);
            if (allAssociations.Any())
            {
                var principalKeyNamesType = toTable.GetKeyNamesType();
                if (principalKeyNamesType == null)
                {
                    // grab a candidate
                    var association = allAssociations.First();
                    principalKeyNamesType = association.Constraint.FromRole.GetEntityType();

                    if (allAssociations.All(x => x.Constraint.FromRole.GetEntityType() == principalKeyNamesType))
                    {
                        toTable.SetKeyNamesType(principalKeyNamesType);
                    }
                }

                // rename the columns in the fragment to match the principal keys
                var principalKeys = principalKeyNamesType.KeyProperties().ToArray();
                var i = 0;
                foreach (var k in entityType.KeyProperties())
                {
                    var dependentColumn = fragment.ColumnMappings.Single(pm => pm.PropertyPath.First() == k).ColumnProperty;
                    dependentColumn.Name = principalKeys[i].Name;
                    i++;
                }
                return true;
            }
            return false;
        }
    }
}
