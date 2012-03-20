namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.Edm.Internal;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm.Services;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    // Equivalent to a mapping fragment in the MSL
    internal class EntityMappingConfiguration
    {
        #region Fields and constructors

        private DatabaseName _tableName;
        private List<PropertyPath> _properties;
        private readonly List<ValueConditionConfiguration> _valueConditions = new List<ValueConditionConfiguration>();
        private readonly List<NotNullConditionConfiguration> _notNullConditions = new List<NotNullConditionConfiguration>();

        internal EntityMappingConfiguration()
        {
        }

        private EntityMappingConfiguration(EntityMappingConfiguration source)
        {
            Contract.Requires(source != null);

            _tableName = source._tableName;

            MapInheritedProperties = source.MapInheritedProperties;

            if (source._properties != null)
            {
                _properties = new List<PropertyPath>(source._properties);
            }

            _valueConditions.AddRange(source._valueConditions.Select(c => c.Clone(this)));
            _notNullConditions.AddRange(source._notNullConditions.Select(c => c.Clone(this)));
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
                Contract.Requires(value != null);

                _tableName = value;
            }
        }

        internal List<PropertyPath> Properties
        {
            get { return _properties; }
            set
            {
                Contract.Requires(value != null);
                if (_properties == null)
                {
                    _properties = new List<PropertyPath>();
                }
                value.Each(Property);
            }
        }

        private void Property(PropertyPath property)
        {
            Contract.Requires(property != null);

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
#if IncludeUnusedEdmCode
            set
            {
                if (value == null) throw new ArgumentNullException("value");

                value.Each(AddValueCondition);
            }
#endif
        }

        public void AddValueCondition(ValueConditionConfiguration valueCondition)
        {
            Contract.Requires(valueCondition != null);

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

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by test code.")]
        public List<NotNullConditionConfiguration> NullabilityConditions
        {
            get { return _notNullConditions; }
            set
            {
                Contract.Requires(value != null);

                value.Each(AddNullabilityCondition);
            }
        }

        public void AddNullabilityCondition(NotNullConditionConfiguration notNullConditionConfiguration)
        {
            Contract.Requires(notNullConditionConfiguration != null);

            if (!NullabilityConditions.Contains(notNullConditionConfiguration))
            {
                NullabilityConditions.Add(notNullConditionConfiguration);
            }
        }

        #endregion

        public bool MapsAnyInheritedProperties(EdmEntityType entityType)
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
                       x => !entityType.KeyProperties().Contains(x.First()) && !entityType.DeclaredProperties.Contains(x.First()));
        }

        public void Configure(
            DbDatabaseMapping databaseMapping,
            DbProviderManifest providerManifest,
            EdmEntityType entityType,
            ref DbEntityTypeMapping entityTypeMapping,
            bool isMappingAnyInheritedProperty,
            int configurationIndex,
            int configurationCount)
        {
            Contract.Requires(entityType != null);
            Contract.Requires(databaseMapping != null);
            Contract.Requires(providerManifest != null);

            var isIdentityTable = entityType.BaseType == null && configurationIndex == 0;

            var fragment = FindOrCreateTypeMappingFragment(
                databaseMapping, ref entityTypeMapping, configurationIndex, entityType, providerManifest);
            var fromTable = fragment.Table;
            bool isTableSharing;
            var toTable = FindOrCreateTargetTable(databaseMapping, fragment, entityType, fromTable, isIdentityTable, out isTableSharing);

            var isSharingTableWithBase = DiscoverIsSharingWithBase(databaseMapping, fragment, entityType, toTable);

            // Ensure all specified properties are the only ones present in this fragment and table
            var mappingsToContain = DiscoverAllMappingsToContain(databaseMapping, entityType, toTable, isSharingTableWithBase);

            // Validate that specified properties can be mapped
            var mappingsToMove = fragment.PropertyMappings.ToList();

            foreach (var propertyPath in mappingsToContain)
            {
                var propertyMapping = fragment.PropertyMappings.SingleOrDefault(
                    pm =>
                    pm.PropertyPath.SequenceEqual(propertyPath));

                if (propertyMapping == null)
                {
                    throw Error.EntityMappingConfiguration_DuplicateMappedProperty(entityType.Name, propertyPath.ToString());
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
                    DatabaseOperations.AddTypeConstraint(entityType, parentTable, toTable, isSplitting);
                }
            }

            // Update AssociationSetMappings (IAs) and FKs
            if (fromTable != toTable)
            {
                if (Properties == null)
                {
                    AssociationMappingOperations.MoveAllDeclaredAssociationSetMappings(
                        databaseMapping, entityType, fromTable, toTable, !isTableSharing);
                    ForeignKeyPrimitiveOperations.MoveAllDeclaredForeignKeyConstraintsForPrimaryKeyColumns(entityType, fromTable, toTable);
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
                DbTableMetadata extraTable = null;
                if (configurationIndex < configurationCount - 1)
                {
                    // Move all extra properties to a single new fragment
                    var anyPropertyMapping = mappingsToMove.First();
                    extraTable = FindTableForTemporaryExtraPropertyMapping(
                        databaseMapping, entityType, fromTable, toTable, anyPropertyMapping);
                    var extraFragment = EntityMappingOperations.CreateTypeMappingFragment(entityTypeMapping, fragment, extraTable);
                    var requiresUpdate = extraTable != fromTable;

                    foreach (var pm in mappingsToMove)
                    {
                        // move the property mapping from toFragment to extraFragment
                        EntityMappingOperations.MovePropertyMapping(
                            databaseMapping.Database, fragment, extraFragment, pm, requiresUpdate, true);
                    }
                }
                else
                {
                    // Move each extra property mapping to a fragment refering to the table with the base mapping
                    DbTableMetadata unmappedTable = null;
                    foreach (var pm in mappingsToMove)
                    {
                        extraTable = FindTableForExtraPropertyMapping(
                            databaseMapping, entityType, fromTable, toTable, ref unmappedTable, pm);

                        var extraFragment = entityTypeMapping.TypeMappingFragments.SingleOrDefault(tmf => tmf.Table == extraTable);

                        if (extraFragment == null)
                        {
                            extraFragment = EntityMappingOperations.CreateTypeMappingFragment(entityTypeMapping, fragment, extraTable);
                            extraFragment.SetIsUnmappedPropertiesFragment(true);
                        }

                        if (extraTable == fromTable)
                        {
                            // move the default discriminator along with the properties
                            MoveDefaultDiscriminator(fragment, extraFragment);
                        }

                        var requiresUpdate = extraTable != fromTable;
                        EntityMappingOperations.MovePropertyMapping(
                            databaseMapping.Database, fragment, extraFragment, pm, requiresUpdate, true);
                    }
                }
            }

            // Ensure all property mappings refer to the table in the fragment
            // Uniquify: true if table sharing, false otherwise
            //           FK names should be uniquified
            //           declared properties are moved, inherited ones are copied (duplicated)
            EntityMappingOperations.UpdatePropertyMappings(databaseMapping.Database, entityType, fromTable, fragment, !isTableSharing);

            // Configure Conditions for the fragment
            ConfigureDefaultDiscriminator(entityType, fragment, isSharingTableWithBase);
            ConfigureConditions(databaseMapping, entityType, fragment, providerManifest);

            // Ensure all conditions refer to columns on the table in the fragment
            EntityMappingOperations.UpdateConditions(databaseMapping.Database, fromTable, fragment);

            ForeignKeyPrimitiveOperations.UpdatePrincipalTables(
                databaseMapping, entityType, fromTable, toTable, isMappingAnyInheritedProperty);

            CleanupUnmappedArtifacts(databaseMapping, fromTable);
            CleanupUnmappedArtifacts(databaseMapping, toTable);

            toTable.SetConfiguration(this);
        }

        private void ConfigureDefaultDiscriminator(
            EdmEntityType entityType, DbEntityTypeMappingFragment fragment, bool isSharingTableWithBase)
        {
            if ((entityType.BaseType != null && !isSharingTableWithBase)
                || ValueConditions.Any()
                || NullabilityConditions.Any())
            {
                var discriminator = fragment.RemoveDefaultDiscriminatorCondition();
                if (discriminator != null && entityType.BaseType != null)
                {
                    discriminator.IsNullable = true;
                }
            }
        }

        private void MoveDefaultDiscriminator(DbEntityTypeMappingFragment fromFragment, DbEntityTypeMappingFragment toFragment)
        {
            var discriminatorColumn = fromFragment.GetDefaultDiscriminator();
            if (discriminatorColumn != null)
            {
                var discriminator = fromFragment.ColumnConditions.SingleOrDefault(cc => cc.Column == discriminatorColumn);
                if (discriminator != null)
                {
                    fromFragment.RemoveDefaultDiscriminatorAnnotation();
                    fromFragment.ColumnConditions.Remove(discriminator);
                    toFragment.AddDiscriminatorCondition(discriminator.Column, discriminator.Value);
                    toFragment.SetDefaultDiscriminator(discriminator.Column);
                }
            }
        }

        private static DbTableMetadata FindTableForTemporaryExtraPropertyMapping(
            DbDatabaseMapping databaseMapping,
            EdmEntityType entityType,
            DbTableMetadata fromTable,
            DbTableMetadata toTable,
            DbEdmPropertyMapping pm)
        {
            var extraTable = fromTable;
            if (fromTable == toTable)
            {
                extraTable = databaseMapping.Database.AddTable(entityType.Name, fromTable, false);
            }
            else if (entityType.BaseType == null)
            {
                extraTable = fromTable;
            }
            else
            {
                // find where the base mappings are and put them in that table
                extraTable = FindBaseTableForExtraPropertyMapping(databaseMapping, entityType, fromTable, pm);
                if (extraTable == null)
                {
                    extraTable = fromTable;
                }
            }
            return extraTable;
        }

        private DbTableMetadata FindTableForExtraPropertyMapping(
            DbDatabaseMapping databaseMapping,
            EdmEntityType entityType,
            DbTableMetadata fromTable,
            DbTableMetadata toTable,
            ref DbTableMetadata unmappedTable,
            DbEdmPropertyMapping pm)
        {
            var extraTable = FindBaseTableForExtraPropertyMapping(databaseMapping, entityType, fromTable, pm);

            if (extraTable == null)
            {
                if (fromTable != toTable && entityType.BaseType == null)
                {
                    return fromTable;
                }

                if (unmappedTable == null)
                {
                    unmappedTable = databaseMapping.Database.AddTable(fromTable.Name, fromTable, false);
                }
                extraTable = unmappedTable;
            }

            return extraTable;
        }

        private static DbTableMetadata FindBaseTableForExtraPropertyMapping(
            DbDatabaseMapping databaseMapping, EdmEntityType entityType, DbTableMetadata fromTable, DbEdmPropertyMapping pm)
        {
            var baseType = entityType.BaseType;
            DbEntityTypeMapping baseMapping = null;
            DbEntityTypeMappingFragment baseFragment = null;
            while (baseType != null && baseFragment == null)
            {
                baseMapping = databaseMapping.GetEntityTypeMapping(baseType);
                if (baseMapping != null)
                {
                    baseFragment =
                        baseMapping.TypeMappingFragments.SingleOrDefault(
                            f => f.PropertyMappings.Any(bpm => bpm.PropertyPath.SequenceEqual(pm.PropertyPath)));
                    if (baseFragment != null)
                    {
                        return baseFragment.Table;
                    }
                }
                baseType = baseType.BaseType;
            }
            return null;
        }

        private bool DiscoverIsSharingWithBase(
            DbDatabaseMapping databaseMapping, DbEntityTypeMappingFragment fragment, EdmEntityType entityType, DbTableMetadata toTable)
        {
            var isSharingTableWithBase = false;
            if (entityType.BaseType != null)
            {
                var baseType = entityType.BaseType;
                var anyBaseMappings = false;
                while (baseType != null && !isSharingTableWithBase)
                {
                    var baseMappings = databaseMapping.GetEntityTypeMappings(baseType);
                    if (baseMappings.Count() > 0)
                    {
                        isSharingTableWithBase = baseMappings.SelectMany(m => m.TypeMappingFragments).Any(tmf => tmf.Table == toTable);
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

        private DbTableMetadata FindParentTable(
            DbDatabaseMapping databaseMapping,
            DbTableMetadata fromTable,
            DbEntityTypeMapping entityTypeMapping,
            DbTableMetadata toTable,
            bool isMappingInheritedProperties,
            int configurationIndex,
            int configurationCount,
            out bool isSplitting)
        {
            DbTableMetadata parentTable = null;
            isSplitting = false;
            // Check for entity splitting first, since splitting on a derived type in TPT/TPC will always have fromTable != toTable
            if (entityTypeMapping.UsesOtherTables(toTable) || configurationCount > 1)
            {
                if (configurationIndex != 0)
                {
                    // Entity Splitting case
                    parentTable = entityTypeMapping.GetPrimaryTable();
                    isSplitting = true;
                }
            }

            if (parentTable == null && fromTable != toTable && !isMappingInheritedProperties)
            {
                // TPT case
                var baseType = entityTypeMapping.EntityType.BaseType;
                while (baseType != null && parentTable == null)
                {
                    // Traverse to first anscestor with a mapping
                    var baseMapping = databaseMapping.GetEntityTypeMappings(baseType).FirstOrDefault();
                    if (baseMapping != null)
                    {
                        parentTable = baseMapping.GetPrimaryTable();
                    }
                    baseType = baseType.BaseType;
                }
            }

            return parentTable;
        }

        private DbEntityTypeMappingFragment FindOrCreateTypeMappingFragment(
            DbDatabaseMapping databaseMapping,
            ref DbEntityTypeMapping entityTypeMapping,
            int configurationIndex,
            EdmEntityType entityType,
            DbProviderManifest providerManifest)
        {
            DbEntityTypeMappingFragment fragment = null;

            if (entityTypeMapping == null)
            {
                Contract.Assert(entityType.IsAbstract);
                new EntityTypeMappingGenerator(providerManifest).
                    Generate(entityType, databaseMapping);
                entityTypeMapping = databaseMapping.GetEntityTypeMapping(entityType);
                configurationIndex = 0;
            }

            if (configurationIndex < entityTypeMapping.TypeMappingFragments.Count)
            {
                fragment = entityTypeMapping.TypeMappingFragments[configurationIndex];
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
                                if (PropertyPathToEdmPropertyPath(p, entityType).Any(pp => !entityType.KeyProperties().Contains(pp.First())))
                                {
                                    throw Error.EntityMappingConfiguration_DuplicateMappedProperty(entityType.Name, p.ToString());
                                }
                            });
                }

                // Special case where they've asked for an extra table related to this type that only will include the PK columns
                // Uniquify: can be false, always move to a new table
                var templateTable = entityTypeMapping.TypeMappingFragments[0].Table;
                fragment = EntityMappingOperations.CreateTypeMappingFragment(
                    entityTypeMapping,
                    entityTypeMapping.TypeMappingFragments[0],
                    databaseMapping.Database.AddTable(templateTable.Name, templateTable, false));
            }
            return fragment;
        }

        private DbTableMetadata FindOrCreateTargetTable(
            DbDatabaseMapping databaseMapping,
            DbEntityTypeMappingFragment fragment,
            EdmEntityType entityType,
            DbTableMetadata fromTable,
            bool isIdentityTable,
            out bool isTableSharing)
        {
            DbTableMetadata toTable;
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
                        toTable = databaseMapping.Database.AddTable(TableName.Name, fromTable, isIdentityTable);
                    }
                }

                // Validate this table can be used and update as needed if it is
                isTableSharing = UpdateColumnNamesForTableSharing(databaseMapping, entityType, toTable, fragment);

                fragment.Table = toTable;
                toTable.SetTableName(TableName);
            }

            return toTable;
        }

        private HashSet<EdmPropertyPath> DiscoverAllMappingsToContain(
            DbDatabaseMapping databaseMapping, EdmEntityType entityType, DbTableMetadata toTable, bool isSharingTableWithBase)
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
                var baseType = entityType.BaseType;
                DbEntityTypeMapping baseMapping = null;
                DbEntityTypeMappingFragment baseFragment = null;
                // if the base is abstract it may have no mapping so look upwards until you find either:
                //   1. a type with mappings and 
                //   2. if none can be found (abstract until the root or hit another table), then include all declared properties on that base type
                while (baseType != null && baseMapping == null)
                {
                    baseMapping = databaseMapping.GetEntityTypeMapping(entityType.BaseType);
                    if (baseMapping != null)
                    {
                        baseFragment = baseMapping.TypeMappingFragments.SingleOrDefault(tmf => tmf.Table == toTable);
                    }

                    if (baseFragment == null)
                    {
                        baseType.DeclaredProperties.Each(
                            p =>
                            baseMappingsToContain.AddRange(p.ToPropertyPathList()));
                    }

                    baseType = baseType.BaseType;
                }

                if (baseFragment != null)
                {
                    foreach (var pm in baseFragment.PropertyMappings)
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
            EdmEntityType entityType,
            DbEntityTypeMappingFragment fragment,
            DbProviderManifest providerManifest)
        {
            if (ValueConditions.Any() || NullabilityConditions.Any())
            {
#if IncludeUnusedEdmCode
                 fragment.PropertyConditions.Clear();
#endif

                fragment.ColumnConditions.Clear();

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

        internal static void CleanupUnmappedArtifacts(DbDatabaseMapping databaseMapping, DbTableMetadata table)
        {
            var associationMappings = databaseMapping.EntityContainerMappings
                .SelectMany(ecm => ecm.AssociationSetMappings).Where(asm => asm.Table == table).ToArray();

            var entityFragments = databaseMapping.EntityContainerMappings
                .SelectMany(ecm => ecm.EntitySetMappings)
                .SelectMany(esm => esm.EntityTypeMappings)
                .SelectMany(etm => etm.TypeMappingFragments).Where(f => f.Table == table).ToArray();

            if (!associationMappings.Any() && !entityFragments.Any())
            {
                databaseMapping.Database.Schemas.Single().Tables.Remove(table);
            }
            else
            {
                // check the columns of table to see if they are actually used in any fragment
                foreach (var column in table.Columns.ToArray())
                {
                    if (!entityFragments.SelectMany(f => f.PropertyMappings).Where(pm => pm.Column == column).Any() &&
                        !entityFragments.SelectMany(f => f.ColumnConditions).Where(cc => cc.Column == column).Any() &&
                        !associationMappings.SelectMany(am => am.SourceEndMapping.PropertyMappings).Where(pm => pm.Column == column).Any() &&
                        !associationMappings.SelectMany(am => am.SourceEndMapping.PropertyMappings).Where(pm => pm.Column == column).Any())
                    {
                        // Remove table FKs that refer to this column, and then remove the column
                        ForeignKeyPrimitiveOperations.RemoveAllForeignKeyConstraintsForColumn(table, column);
                        TablePrimitiveOperations.RemoveColumn(table, column);
                    }
                }

                // Remove FKs where Principal Table == Dependent Table and the PK == FK (redundant)
                table.ForeignKeyConstraints
                    .Where(fk => fk.PrincipalTable == table && fk.DependentColumns.SequenceEqual(table.KeyColumns))
                    .ToArray()
                    .Each(fk => table.ForeignKeyConstraints.Remove(fk));
            }
        }

        internal static IEnumerable<EdmPropertyPath> PropertyPathToEdmPropertyPath(PropertyPath path, EdmEntityType entityType)
        {
            var propertyPath = new List<EdmProperty>();
            EdmStructuralType propertyOwner = entityType;
            for (var i = 0; i < path.Count; i++)
            {
                var edmProperty = propertyOwner.Members.OfType<EdmProperty>().SingleOrDefault(p => p.GetClrPropertyInfo().IsSameAs(path[i]));
                if (edmProperty == null)
                {
                    throw Error.EntityMappingConfiguration_CannotMapIgnoredProperty(entityType.Name, path.ToString());
                }
                propertyPath.Add(edmProperty);
                if (edmProperty.PropertyType.IsComplexType)
                {
                    propertyOwner = edmProperty.PropertyType.ComplexType;
                }
            }

            var lastProperty = propertyPath.Last();
            if (lastProperty.PropertyType.IsUnderlyingPrimitiveType)
            {
                return new[] { new EdmPropertyPath(propertyPath) };
            }
            else if (lastProperty.PropertyType.IsComplexType)
            {
                propertyPath.Remove(lastProperty);
                return lastProperty.ToPropertyPathList(propertyPath);
            }

            return new[] { EdmPropertyPath.Empty };
        }

        private static IEnumerable<EdmEntityType> FindAllTypesUsingTable(DbDatabaseMapping databaseMapping, DbTableMetadata toTable)
        {
            return databaseMapping.EntityContainerMappings
                .SelectMany(ecm => ecm.EntitySetMappings)
                .SelectMany(esm => esm.EntityTypeMappings)
                .Where(etm => etm.TypeMappingFragments.Any(tmf => tmf.Table == toTable))
                .Select(etm => etm.EntityType);
        }

        private static IEnumerable<EdmAssociationType> FindAllOneToOneFKAssociationTypes(
            EdmModel model, EdmEntityType entityType, EdmEntityType candidateType)
        {
            return model.Containers.SelectMany(ec => ec.AssociationSets)
                .Where(
                    aset => (aset.ElementType.Constraint != null &&
                             aset.ElementType.SourceEnd.EndKind == EdmAssociationEndKind.Required &&
                             aset.ElementType.TargetEnd.EndKind == EdmAssociationEndKind.Required) &&
                            ((aset.ElementType.SourceEnd.EntityType == entityType && aset.ElementType.TargetEnd.EntityType == candidateType) ||
                             (aset.ElementType.TargetEnd.EntityType == entityType && aset.ElementType.SourceEnd.EntityType == candidateType)))
                .Select(aset => aset.ElementType);
        }

        private static bool UpdateColumnNamesForTableSharing(
            DbDatabaseMapping databaseMapping, EdmEntityType entityType, DbTableMetadata toTable, DbEntityTypeMappingFragment fragment)
        {
            // Validate: this table can be used only if:
            //  1. The table is not used by any other type
            //  2. The table is used only by types in the same type hierarchy (TPH)
            //  3. There is a 1:1 relationship and the PK count and types match (Table Splitting)
            var typesSharingTable = FindAllTypesUsingTable(databaseMapping, toTable);
            var associationsToSharedTable = new Dictionary<EdmEntityType, List<EdmAssociationType>>();

            foreach (var candidateType in typesSharingTable)
            {
                var oneToOneAssocations = FindAllOneToOneFKAssociationTypes(databaseMapping.Model, entityType, candidateType);

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
                if (candidateTypePair.Key != entityType.GetRootType() &&
                    candidateTypePair.Value.Count == 0)
                {
                    var tableName = toTable.GetTableName();
                    throw Error.EntityMappingConfiguration_InvalidTableSharing(
                        entityType.Name, candidateTypePair.Key.Name, tableName != null ? tableName.Name : toTable.DatabaseIdentifier);
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
                    principalKeyNamesType = association.Constraint.PrincipalEnd(association).EntityType;

                    if (allAssociations.All(x => x.Constraint.PrincipalEnd(x).EntityType == principalKeyNamesType))
                    {
                        toTable.SetKeyNamesType(principalKeyNamesType);
                    }
                }

                // rename the columns in the fragment to match the principal keys
                var principalKeys = principalKeyNamesType.KeyProperties().ToArray();
                var i = 0;
                foreach (var k in entityType.KeyProperties())
                {
                    var dependentColumn = fragment.PropertyMappings.Single(pm => pm.PropertyPath.First() == k).Column;
                    dependentColumn.Name = principalKeys[i].Name;
                    i++;
                }
                return true;
            }
            return false;
        }
    }
}