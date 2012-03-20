namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.Edm.Internal;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal class EntityMappingService
    {
        private readonly DbDatabaseMapping _databaseMapping;
        private Dictionary<DbTableMetadata, TableMapping> _tableMappings;
        private SortedEntityTypeIndex _entityTypes;

        public EntityMappingService(DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(databaseMapping != null);
            _databaseMapping = databaseMapping;
        }

        public void Configure()
        {
            Analyze();
            Transform();
        }

        /// <summary>
        ///     Populate the table mapping structure
        /// </summary>
        private void Analyze()
        {
            _tableMappings = new Dictionary<DbTableMetadata, TableMapping>();
            _entityTypes = new SortedEntityTypeIndex();

            foreach (var esm in _databaseMapping.EntityContainerMappings
                .SelectMany(ecm => ecm.EntitySetMappings))
            {
                foreach (var etm in esm.EntityTypeMappings)
                {
                    _entityTypes.Add(esm.EntitySet, etm.EntityType);

                    foreach (var fragment in etm.TypeMappingFragments)
                    {
                        var tableMapping = FindOrCreateTableMapping(fragment.Table);
                        tableMapping.AddEntityTypeMappingFragment(esm.EntitySet, etm.EntityType, fragment);
                    }
                }
            }
        }

        private void Transform()
        {
            foreach (var entitySet in _entityTypes.GetEntitySets())
            {
                var setRootMappings = new Dictionary<TableMapping, Dictionary<EdmEntityType, DbEntityTypeMapping>>();

                foreach (var entityType in _entityTypes.GetEntityTypes(entitySet))
                {
                    foreach (var tableMapping in _tableMappings.Values.Where(tm => tm.EntityTypes.Contains(entitySet, entityType)))
                    {
                        Dictionary<EdmEntityType, DbEntityTypeMapping> rootMappings;
                        if (!setRootMappings.TryGetValue(tableMapping, out rootMappings))
                        {
                            rootMappings = new Dictionary<EdmEntityType, DbEntityTypeMapping>();
                            setRootMappings.Add(tableMapping, rootMappings);
                        }

                        RemoveRedundantDefaultDiscriminators(tableMapping);

                        var isRoot = tableMapping.EntityTypes.IsRoot(entitySet, entityType);
                        var requiresIsTypeOf = DetermineRequiresIsTypeOf(tableMapping, entitySet, entityType);
                        var requiresSplit = false;

                        // Find the entity type mapping and fragment for this table / entity type mapping where properties will be mapped
                        DbEntityTypeMapping propertiesTypeMapping;
                        DbEntityTypeMappingFragment propertiesTypeMappingFragment;
                        if (
                            !FindPropertyEntityTypeMapping(
                                tableMapping,
                                entitySet,
                                entityType,
                                requiresIsTypeOf,
                                out propertiesTypeMapping,
                                out propertiesTypeMappingFragment))
                        {
                            continue;
                        }

                        // Determine if the entity type mapping needs to be split into separate properties and condition type mappings.
                        requiresSplit = DetermineRequiresSplitEntityTypeMapping(
                            tableMapping, entityType, isRoot, requiresIsTypeOf, propertiesTypeMapping);

                        // Find the entity type mapping and fragment for this table / entity type mapping where conditions will be mapped
                        var conditionTypeMapping = FindConditionTypeMapping(entityType, requiresSplit, propertiesTypeMapping);
                        var conditionTypeMappingFragment = FindConditionTypeMappingFragment(
                            tableMapping, propertiesTypeMappingFragment, conditionTypeMapping);

                        // Set the IsTypeOf appropriately
                        if (requiresIsTypeOf)
                        {
                            if (propertiesTypeMapping.IsHierarchyMapping == false)
                            {
                                var isTypeOfEntityTypeMapping =
                                    _databaseMapping.GetEntityTypeMappings(entityType).SingleOrDefault(etm => etm.IsHierarchyMapping);

                                if (isTypeOfEntityTypeMapping == null)
                                {
                                    if (propertiesTypeMapping.TypeMappingFragments.Count > 1)
                                    {
                                        // Need to create a new entity type mapping with the non-IsTypeOf contents
                                        var nonIsTypeOfEntityTypeMapping = propertiesTypeMapping.Clone();
                                        var parentEntitySetMapping =
                                            _databaseMapping.GetEntitySetMappings().Single(
                                                esm => esm.EntityTypeMappings.Contains(propertiesTypeMapping));
                                        parentEntitySetMapping.EntityTypeMappings.Add(nonIsTypeOfEntityTypeMapping);
                                        foreach (
                                            var fragment in
                                                propertiesTypeMapping.TypeMappingFragments.Where(
                                                    tmf => tmf != propertiesTypeMappingFragment).ToArray())
                                        {
                                            propertiesTypeMapping.TypeMappingFragments.Remove(fragment);
                                            nonIsTypeOfEntityTypeMapping.TypeMappingFragments.Add(fragment);
                                        }
                                    }
                                    // else we just use the existing property mapping

                                    propertiesTypeMapping.IsHierarchyMapping = true;
                                }
                                else
                                {
                                    // found an existing IsTypeOf mapping, so re-use that one
                                    propertiesTypeMapping.TypeMappingFragments.Remove(propertiesTypeMappingFragment);
                                    if (propertiesTypeMapping.TypeMappingFragments.Count == 0)
                                    {
                                        _databaseMapping.GetEntitySetMapping(entitySet).EntityTypeMappings.Remove(propertiesTypeMapping);
                                    }
                                    propertiesTypeMapping = isTypeOfEntityTypeMapping;
                                    propertiesTypeMapping.TypeMappingFragments.Add(propertiesTypeMappingFragment);
                                }
                            }
                            rootMappings.Add(entityType, propertiesTypeMapping);
                        }

                        ConfigureTypeMappings(
                            tableMapping, rootMappings, entityType, propertiesTypeMappingFragment, conditionTypeMappingFragment);

                        if (propertiesTypeMappingFragment.IsUnmappedPropertiesFragment() &&
                            propertiesTypeMappingFragment.PropertyMappings.All(
                                pm => entityType.GetKeyProperties().Contains(pm.PropertyPath.First())))
                        {
                            RemoveFragment(entitySet, propertiesTypeMapping, propertiesTypeMappingFragment);

                            if (requiresSplit
                                &&
                                conditionTypeMappingFragment.PropertyMappings.All(
                                    pm => entityType.GetKeyProperties().Contains(pm.PropertyPath.First())))
                            {
                                RemoveFragment(entitySet, conditionTypeMapping, conditionTypeMappingFragment);
                            }
                        }

                        EntityMappingConfiguration.CleanupUnmappedArtifacts(_databaseMapping, tableMapping.Table);

                        foreach (var fkConstraint in tableMapping.Table.ForeignKeyConstraints)
                        {
                            var associationType = fkConstraint.GetAssociationType();
                            if (associationType != null && associationType.IsRequiredToNonRequired())
                            {
                                EdmAssociationEnd _, dependentEnd;
                                fkConstraint.GetAssociationType().TryGuessPrincipalAndDependentEnds(out _, out dependentEnd);

                                if (dependentEnd.EntityType == entityType)
                                {
                                    MarkColumnsAsNonNullableIfNoTableSharing(entitySet, tableMapping.Table, entityType, fkConstraint.DependentColumns);
                                }
                            }
                        }
                    }
                }

                ConfigureAssociationSetMappingForeignKeys(entitySet);
            }
        }

        /// <summary>
        ///     Sets nullability for association set mappings' foreign keys for 1:* and 1:0..1 associations
        ///     when no base types share the the association set mapping's table
        /// </summary>
        private void ConfigureAssociationSetMappingForeignKeys(EdmEntitySet entitySet)
        {
            foreach (var asm in _databaseMapping.EntityContainerMappings
                .SelectMany(ecm => ecm.AssociationSetMappings)
                .Where(
                    asm => (asm.AssociationSet.SourceSet == entitySet || asm.AssociationSet.TargetSet == entitySet)
                           && asm.AssociationSet.ElementType.IsRequiredToNonRequired()))
            {
                EdmAssociationEnd _, dependentEnd;
                asm.AssociationSet.ElementType.TryGuessPrincipalAndDependentEnds(out _, out dependentEnd);

                if ((dependentEnd == asm.AssociationSet.ElementType.SourceEnd &&
                     asm.AssociationSet.SourceSet == entitySet)
                    || (dependentEnd == asm.AssociationSet.ElementType.TargetEnd &&
                        asm.AssociationSet.TargetSet == entitySet))
                {
                    var dependentMapping = asm.SourceEndMapping.AssociationEnd == dependentEnd
                                               ? asm.TargetEndMapping
                                               : asm.SourceEndMapping;

                    MarkColumnsAsNonNullableIfNoTableSharing(entitySet, asm.Table, dependentEnd.EntityType,
                                                             dependentMapping.PropertyMappings.Select(pm => pm.Column));
                }
            }
        }

        private void MarkColumnsAsNonNullableIfNoTableSharing(EdmEntitySet entitySet, DbTableMetadata table, EdmEntityType dependentEndEntityType, IEnumerable<DbTableColumnMetadata> columns)
        {
            // determine if base entities share this table, if not, the foreign keys can be non-nullable
            var mappedBaseTypes =
                _tableMappings[table].EntityTypes.GetEntityTypes(entitySet).Where(
                    et =>
                    et != dependentEndEntityType &&
                    (et.IsAncestorOf(dependentEndEntityType) || !dependentEndEntityType.IsAncestorOf(et)));
            if (mappedBaseTypes.Count() == 0 || mappedBaseTypes.All(et => et.IsAbstract))
            {
                columns.Each(c => c.IsNullable = false);
            }
        }

        /// <summary>
        ///     Makes sure only the required property mappings are present
        /// </summary>
        private void ConfigureTypeMappings(
            TableMapping tableMapping,
            Dictionary<EdmEntityType, DbEntityTypeMapping> rootMappings,
            EdmEntityType entityType,
            DbEntityTypeMappingFragment propertiesTypeMappingFragment,
            DbEntityTypeMappingFragment conditionTypeMappingFragment)
        {
            var existingPropertyMappings =
                new List<DbEdmPropertyMapping>(propertiesTypeMappingFragment.PropertyMappings.Where(pm => !pm.Column.IsPrimaryKeyColumn));
            var existingConditions = new List<DbColumnCondition>(propertiesTypeMappingFragment.ColumnConditions);

            foreach (var columnMapping in from cm in tableMapping.ColumnMappings
                                          from pm in cm.PropertyMappings
                                          where pm.EntityType == entityType
                                          select new { cm.Column, Property = pm })
            {
                if (columnMapping.Property.PropertyPath != null &&
                    !IsRootTypeMapping(rootMappings, columnMapping.Property.EntityType, columnMapping.Property.PropertyPath))
                {
                    var existingPropertyMapping =
                        propertiesTypeMappingFragment.PropertyMappings.SingleOrDefault(
                            x => x.PropertyPath == columnMapping.Property.PropertyPath);
                    if (existingPropertyMapping != null)
                    {
                        existingPropertyMappings.Remove(existingPropertyMapping);
                    }
                    else
                    {
                        existingPropertyMapping = new DbEdmPropertyMapping
                            {
                                Column = columnMapping.Column,
                                PropertyPath = columnMapping.Property.PropertyPath
                            };
                        propertiesTypeMappingFragment.PropertyMappings.Add(existingPropertyMapping);
                    }
                }

                if (columnMapping.Property.Conditions != null)
                {
                    foreach (var condition in columnMapping.Property.Conditions)
                    {
                        if (conditionTypeMappingFragment.ColumnConditions.Contains(condition))
                        {
                            existingConditions.Remove(condition);
                        }
                        else if (!entityType.IsAbstract)
                        {
                            conditionTypeMappingFragment.ColumnConditions.Add(condition);
                        }
                    }
                }
            }

            // Any leftover mappings are removed
            foreach (var leftoverPropertyMapping in existingPropertyMappings)
            {
                propertiesTypeMappingFragment.PropertyMappings.Remove(leftoverPropertyMapping);
            }

            foreach (var leftoverCondition in existingConditions)
            {
                conditionTypeMappingFragment.ColumnConditions.Remove(leftoverCondition);
            }

            if (entityType.IsAbstract)
            {
                propertiesTypeMappingFragment.ColumnConditions.Clear();
            }
        }

        private static DbEntityTypeMappingFragment FindConditionTypeMappingFragment(
            TableMapping tableMapping, DbEntityTypeMappingFragment propertiesTypeMappingFragment, DbEntityTypeMapping conditionTypeMapping)
        {
            var conditionTypeMappingFragment = conditionTypeMapping.TypeMappingFragments.SingleOrDefault(x => x.Table == tableMapping.Table);
            if (conditionTypeMappingFragment == null)
            {
                conditionTypeMappingFragment = EntityMappingOperations.CreateTypeMappingFragment(
                    conditionTypeMapping, propertiesTypeMappingFragment, tableMapping.Table);
                conditionTypeMappingFragment.SetIsConditionOnlyFragment(true);
                if (propertiesTypeMappingFragment.GetDefaultDiscriminator() != null)
                {
                    conditionTypeMappingFragment.SetDefaultDiscriminator(propertiesTypeMappingFragment.GetDefaultDiscriminator());
                    propertiesTypeMappingFragment.RemoveDefaultDiscriminatorAnnotation();
                }
            }
            return conditionTypeMappingFragment;
        }

        private DbEntityTypeMapping FindConditionTypeMapping(
            EdmEntityType entityType, bool requiresSplit, DbEntityTypeMapping propertiesTypeMapping)
        {
            var conditionTypeMapping = propertiesTypeMapping;

            if (requiresSplit)
            {
                if (!entityType.IsAbstract)
                {
                    conditionTypeMapping = propertiesTypeMapping.Clone();
                    conditionTypeMapping.IsHierarchyMapping = false;

                    var parentEntitySetMapping =
                        _databaseMapping.GetEntitySetMappings().Single(esm => esm.EntityTypeMappings.Contains(propertiesTypeMapping));
                    parentEntitySetMapping.EntityTypeMappings.Add(conditionTypeMapping);
                }
                propertiesTypeMapping.TypeMappingFragments.Each(tmf => tmf.ColumnConditions.Clear());
            }
            return conditionTypeMapping;
        }

        private bool DetermineRequiresIsTypeOf(
            TableMapping tableMapping, EdmEntitySet entitySet, EdmEntityType entityType /*, bool isRoot*/)
        {
            // IsTypeOf if this is the root for this table and any derived type shares a property mapping
            return entityType.IsRootOfSet(tableMapping.EntityTypes.GetEntityTypes(entitySet)) &&
                   ((tableMapping.EntityTypes.GetEntityTypes(entitySet).Count() > 1
                     && tableMapping.EntityTypes.GetEntityTypes(entitySet).Any(et => et != entityType && !et.IsAbstract))
                    ||
                    _tableMappings.Values.Any(
                        tm =>
                        tm != tableMapping
                        && tm.Table.ForeignKeyConstraints.Any(fk => fk.GetIsTypeConstraint() && fk.PrincipalTable == tableMapping.Table)));
        }

        private bool DetermineRequiresSplitEntityTypeMapping(
            TableMapping tableMapping,
            EdmEntityType entityType,
            bool isRoot,
            bool requiresIsTypeOf,
            DbEntityTypeMapping propertiesTypeMapping)
        {
            return requiresIsTypeOf && HasConditions(tableMapping, entityType);
        }

        /// <summary>
        ///     Determines if the table and entity type need mapping, and if not, removes the existing entity type mapping
        /// </summary>
        private bool FindPropertyEntityTypeMapping(
            TableMapping tableMapping,
            EdmEntitySet entitySet,
            EdmEntityType entityType,
            bool requiresIsTypeOf,
            out DbEntityTypeMapping entityTypeMapping,
            out DbEntityTypeMappingFragment fragment)
        {
            entityTypeMapping = null;
            fragment = null;
            var mapping = (from etm in _databaseMapping.GetEntityTypeMappings(entityType)
                           from tmf in etm.TypeMappingFragments
                           where tmf.Table == tableMapping.Table
                           select new { TypeMapping = etm, Fragment = tmf }).SingleOrDefault();

            if (mapping != null)
            {
                entityTypeMapping = mapping.TypeMapping;
                fragment = mapping.Fragment;
                if (!requiresIsTypeOf && entityType.IsAbstract)
                {
                    RemoveFragment(entitySet, mapping.TypeMapping, mapping.Fragment);
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private void RemoveFragment(EdmEntitySet entitySet, DbEntityTypeMapping entityTypeMapping, DbEntityTypeMappingFragment fragment)
        {
            // Make the default discriminator nullable if this type isn't using it but there is a base type
            var defaultDiscriminator = fragment.GetDefaultDiscriminator();
            if (defaultDiscriminator != null && entityTypeMapping.EntityType.BaseType != null)
            {
                var columnMapping = _tableMappings[fragment.Table].ColumnMappings.SingleOrDefault(cm => cm.Column == defaultDiscriminator);
                if (columnMapping != null)
                {
                    var propertyMapping = columnMapping.PropertyMappings.SingleOrDefault(
                        pm => pm.EntityType == entityTypeMapping.EntityType);
                    if (propertyMapping != null)
                    {
                        columnMapping.PropertyMappings.Remove(propertyMapping);
                    }
                }
                defaultDiscriminator.IsNullable = true;
            }

            entityTypeMapping.TypeMappingFragments.Remove(fragment);
            if (!entityTypeMapping.TypeMappingFragments.Any())
            {
                _databaseMapping.GetEntitySetMapping(entitySet).EntityTypeMappings.Remove(entityTypeMapping);
            }
        }

        private void RemoveRedundantDefaultDiscriminators(TableMapping tableMapping)
        {
            foreach (var entitySet in tableMapping.EntityTypes.GetEntitySets())
            {
                (from cm in tableMapping.ColumnMappings
                 from pm in cm.PropertyMappings
                 where cm.PropertyMappings
                           .Where(pm1 => tableMapping.EntityTypes.GetEntityTypes(entitySet).Contains(pm1.EntityType))
                           .Count(pms => pms.IsDefaultDiscriminatorCondition) == 1
                 select new { ColumnMapping = cm, PropertyMapping = pm }).ToArray().Each(
                     x =>
                         {
                             x.PropertyMapping.Conditions.Clear();
                             if (x.PropertyMapping.PropertyPath == null)
                             {
                                 x.ColumnMapping.PropertyMappings.Remove(x.PropertyMapping);
                             }
                         });
            }
        }

        private bool HasConditions(TableMapping tableMapping, EdmEntityType entityType)
        {
            return tableMapping.ColumnMappings.SelectMany(cm => cm.PropertyMappings)
                .Any(pm => pm.EntityType == entityType && pm.Conditions.Count > 0);
        }

        private bool IsRootTypeMapping(
            Dictionary<EdmEntityType, DbEntityTypeMapping> rootMappings, EdmEntityType entityType, IList<EdmProperty> propertyPath)
        {
            var baseType = entityType.BaseType;
            while (baseType != null)
            {
                DbEntityTypeMapping rootMapping;
                if (rootMappings.TryGetValue(baseType, out rootMapping))
                {
                    return
                        rootMapping.TypeMappingFragments.SelectMany(etmf => etmf.PropertyMappings).Any(
                            pm => pm.PropertyPath.SequenceEqual(propertyPath));
                }
                baseType = baseType.BaseType;
            }
            return false;
        }

        private TableMapping FindOrCreateTableMapping(DbTableMetadata table)
        {
            TableMapping tableMapping;
            if (!_tableMappings.TryGetValue(table, out tableMapping))
            {
                tableMapping = new TableMapping(table);
                _tableMappings.Add(table, tableMapping);
            }
            return tableMapping;
        }
    }
}