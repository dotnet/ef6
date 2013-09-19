// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    internal class EntityMappingService
    {
        private readonly DbDatabaseMapping _databaseMapping;
        private Dictionary<EntityType, TableMapping> _tableMappings;
        private SortedEntityTypeIndex _entityTypes;

        public EntityMappingService(DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(databaseMapping);
            _databaseMapping = databaseMapping;
        }

        public void Configure()
        {
            Analyze();
            Transform();
        }

        /// <summary>
        /// Populate the table mapping structure
        /// </summary>
        private void Analyze()
        {
            _tableMappings = new Dictionary<EntityType, TableMapping>();
            _entityTypes = new SortedEntityTypeIndex();

            foreach (var esm in _databaseMapping.EntityContainerMappings
                                                .SelectMany(ecm => ecm.EntitySetMappings))
            {
                foreach (var etm in esm.EntityTypeMappings)
                {
                    _entityTypes.Add(esm.EntitySet, etm.EntityType);

                    foreach (var fragment in etm.MappingFragments)
                    {
                        var tableMapping = FindOrCreateTableMapping(fragment.Table);
                        tableMapping.AddEntityTypeMappingFragment(esm.EntitySet, etm.EntityType, fragment);
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void Transform()
        {
            foreach (var entitySet in _entityTypes.GetEntitySets())
            {
                var setRootMappings = new Dictionary<TableMapping, Dictionary<EntityType, StorageEntityTypeMapping>>();

                foreach (var entityType in _entityTypes.GetEntityTypes(entitySet))
                {
                    foreach (
                        var tableMapping in
                            _tableMappings.Values.Where(tm => tm.EntityTypes.Contains(entitySet, entityType)))
                    {
                        Dictionary<EntityType, StorageEntityTypeMapping> rootMappings;
                        if (!setRootMappings.TryGetValue(tableMapping, out rootMappings))
                        {
                            rootMappings = new Dictionary<EntityType, StorageEntityTypeMapping>();
                            setRootMappings.Add(tableMapping, rootMappings);
                        }

                        RemoveRedundantDefaultDiscriminators(tableMapping);

                        var requiresIsTypeOf = DetermineRequiresIsTypeOf(tableMapping, entitySet, entityType);
                        var requiresSplit = false;

                        // Find the entity type mapping and fragment for this table / entity type mapping where properties will be mapped
                        StorageEntityTypeMapping propertiesTypeMapping;
                        StorageMappingFragment propertiesTypeMappingFragment;
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
                            tableMapping, entityType, requiresIsTypeOf);

                        // Find the entity type mapping and fragment for this table / entity type mapping where conditions will be mapped
                        var conditionTypeMapping
                            = FindConditionTypeMapping(entityType, requiresSplit, propertiesTypeMapping);

                        var conditionTypeMappingFragment
                            = FindConditionTypeMappingFragment(
                                _databaseMapping.Database.GetEntitySet(tableMapping.Table),
                                propertiesTypeMappingFragment,
                                conditionTypeMapping);

                        // Set the IsTypeOf appropriately
                        if (requiresIsTypeOf)
                        {
                            if (propertiesTypeMapping.IsHierarchyMapping == false)
                            {
                                var isTypeOfEntityTypeMapping =
                                    _databaseMapping.GetEntityTypeMappings(entityType).SingleOrDefault(
                                        etm => etm.IsHierarchyMapping);

                                if (isTypeOfEntityTypeMapping == null)
                                {
                                    if (propertiesTypeMapping.MappingFragments.Count > 1)
                                    {
                                        // Need to create a new entity type mapping with the non-IsTypeOf contents
                                        var nonIsTypeOfEntityTypeMapping = propertiesTypeMapping.Clone();
                                        var parentEntitySetMapping =
                                            _databaseMapping.GetEntitySetMappings().Single(
                                                esm => esm.EntityTypeMappings.Contains(propertiesTypeMapping));
                                        parentEntitySetMapping.AddTypeMapping(nonIsTypeOfEntityTypeMapping);
                                        foreach (
                                            var fragment in
                                                propertiesTypeMapping.MappingFragments.Where(
                                                    tmf => tmf != propertiesTypeMappingFragment).ToArray())
                                        {
                                            propertiesTypeMapping.RemoveFragment(fragment);
                                            nonIsTypeOfEntityTypeMapping.AddFragment(fragment);
                                        }
                                    }
                                    // else we just use the existing property mapping

                                    propertiesTypeMapping.AddIsOfType(propertiesTypeMapping.EntityType);
                                }
                                else
                                {
                                    // found an existing IsTypeOf mapping, so re-use that one
                                    propertiesTypeMapping.RemoveFragment(propertiesTypeMappingFragment);

                                    if (propertiesTypeMapping.MappingFragments.Count == 0)
                                    {
                                        _databaseMapping
                                            .GetEntitySetMapping(entitySet)
                                            .RemoveTypeMapping(propertiesTypeMapping);
                                    }

                                    propertiesTypeMapping = isTypeOfEntityTypeMapping;
                                    propertiesTypeMapping.AddFragment(propertiesTypeMappingFragment);
                                }
                            }
                            rootMappings.Add(entityType, propertiesTypeMapping);
                        }

                        ConfigureTypeMappings(
                            tableMapping, rootMappings, entityType, propertiesTypeMappingFragment,
                            conditionTypeMappingFragment);

                        if (propertiesTypeMappingFragment.IsUnmappedPropertiesFragment()
                            &&
                            propertiesTypeMappingFragment.ColumnMappings.All(
                                pm => entityType.GetKeyProperties().Contains(pm.PropertyPath.First())))
                        {
                            RemoveFragment(entitySet, propertiesTypeMapping, propertiesTypeMappingFragment);

                            if (requiresSplit
                                &&
                                conditionTypeMappingFragment.ColumnMappings.All(
                                    pm => entityType.GetKeyProperties().Contains(pm.PropertyPath.First())))
                            {
                                RemoveFragment(entitySet, conditionTypeMapping, conditionTypeMappingFragment);
                            }
                        }

                        EntityMappingConfiguration.CleanupUnmappedArtifacts(_databaseMapping, tableMapping.Table);

                        foreach (var fkConstraint in tableMapping.Table.ForeignKeyBuilders)
                        {
                            var associationType = fkConstraint.GetAssociationType();
                            if (associationType != null
                                && associationType.IsRequiredToNonRequired())
                            {
                                AssociationEndMember _, dependentEnd;
                                fkConstraint.GetAssociationType().TryGuessPrincipalAndDependentEnds(
                                    out _, out dependentEnd);

                                if (dependentEnd.GetEntityType() == entityType)
                                {
                                    MarkColumnsAsNonNullableIfNoTableSharing(
                                        entitySet, tableMapping.Table, entityType, fkConstraint.DependentColumns);
                                }
                            }
                        }
                    }
                }

                ConfigureAssociationSetMappingForeignKeys(entitySet);
            }
        }

        /// <summary>
        /// Sets nullability for association set mappings' foreign keys for 1:* and 1:0..1 associations
        /// when no base types share the the association set mapping's table
        /// </summary>
        private void ConfigureAssociationSetMappingForeignKeys(EntitySet entitySet)
        {
            foreach (var asm in _databaseMapping.EntityContainerMappings
                                                .SelectMany(ecm => ecm.AssociationSetMappings)
                                                .Where(
                                                    asm =>
                                                    (asm.AssociationSet.SourceSet == entitySet || asm.AssociationSet.TargetSet == entitySet)
                                                    && asm.AssociationSet.ElementType.IsRequiredToNonRequired()))
            {
                AssociationEndMember _, dependentEnd;
                asm.AssociationSet.ElementType.TryGuessPrincipalAndDependentEnds(out _, out dependentEnd);

                if ((dependentEnd == asm.AssociationSet.ElementType.SourceEnd &&
                     asm.AssociationSet.SourceSet == entitySet)
                    || (dependentEnd == asm.AssociationSet.ElementType.TargetEnd &&
                        asm.AssociationSet.TargetSet == entitySet))
                {
                    var dependentMapping
                        = asm.SourceEndMapping.EndMember == dependentEnd
                              ? asm.TargetEndMapping
                              : asm.SourceEndMapping;

                    MarkColumnsAsNonNullableIfNoTableSharing(
                        entitySet, asm.Table, dependentEnd.GetEntityType(),
                        dependentMapping.PropertyMappings.Select(pm => pm.ColumnProperty));
                }
            }
        }

        private void MarkColumnsAsNonNullableIfNoTableSharing(
            EntitySet entitySet, EntityType table, EntityType dependentEndEntityType,
            IEnumerable<EdmProperty> columns)
        {
            // determine if base entities share this table, if not, the foreign keys can be non-nullable
            var mappedBaseTypes =
                _tableMappings[table].EntityTypes.GetEntityTypes(entitySet).Where(
                    et =>
                    et != dependentEndEntityType &&
                    (et.IsAncestorOf(dependentEndEntityType) || !dependentEndEntityType.IsAncestorOf(et)));
            if (mappedBaseTypes.Count() == 0
                || mappedBaseTypes.All(et => et.Abstract))
            {
                columns.Each(c => c.Nullable = false);
            }
        }

        /// <summary>
        /// Makes sure only the required property mappings are present
        /// </summary>
        private static void ConfigureTypeMappings(
            TableMapping tableMapping,
            Dictionary<EntityType, StorageEntityTypeMapping> rootMappings,
            EntityType entityType,
            StorageMappingFragment propertiesTypeMappingFragment,
            StorageMappingFragment conditionTypeMappingFragment)
        {
            var existingPropertyMappings =
                new List<ColumnMappingBuilder>(
                    propertiesTypeMappingFragment.ColumnMappings.Where(pm => !pm.ColumnProperty.IsPrimaryKeyColumn));
            var existingConditions = new List<StorageConditionPropertyMapping>(propertiesTypeMappingFragment.ColumnConditions);

            foreach (var columnMapping in from cm in tableMapping.ColumnMappings
                                          from pm in cm.PropertyMappings
                                          where pm.EntityType == entityType
                                          select new
                                              {
                                                  cm.Column,
                                                  Property = pm
                                              })
            {
                if (columnMapping.Property.PropertyPath != null
                    &&
                    !IsRootTypeMapping(
                        rootMappings, columnMapping.Property.EntityType, columnMapping.Property.PropertyPath))
                {
                    var existingPropertyMapping =
                        propertiesTypeMappingFragment.ColumnMappings.SingleOrDefault(
                            x => x.PropertyPath == columnMapping.Property.PropertyPath);
                    if (existingPropertyMapping != null)
                    {
                        existingPropertyMappings.Remove(existingPropertyMapping);
                    }
                    else
                    {
                        existingPropertyMapping
                            = new ColumnMappingBuilder(columnMapping.Column, columnMapping.Property.PropertyPath);

                        propertiesTypeMappingFragment.AddColumnMapping(existingPropertyMapping);
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
                        else if (!entityType.Abstract)
                        {
                            conditionTypeMappingFragment.AddConditionProperty(condition);
                        }
                    }
                }
            }

            // Any leftover mappings are removed
            foreach (var leftoverPropertyMapping in existingPropertyMappings)
            {
                propertiesTypeMappingFragment.RemoveColumnMapping(leftoverPropertyMapping);
            }

            foreach (var leftoverCondition in existingConditions)
            {
                conditionTypeMappingFragment.RemoveConditionProperty(leftoverCondition);
            }

            if (entityType.Abstract)
            {
                propertiesTypeMappingFragment.ClearConditions();
            }
        }

        private static StorageMappingFragment FindConditionTypeMappingFragment(
            EntitySet tableSet, StorageMappingFragment propertiesTypeMappingFragment, StorageEntityTypeMapping conditionTypeMapping)
        {
            var table = tableSet.ElementType;

            var conditionTypeMappingFragment
                = conditionTypeMapping.MappingFragments
                                      .SingleOrDefault(x => x.Table == table);

            if (conditionTypeMappingFragment == null)
            {
                conditionTypeMappingFragment
                    = EntityMappingOperations
                        .CreateTypeMappingFragment(conditionTypeMapping, propertiesTypeMappingFragment, tableSet);

                conditionTypeMappingFragment.SetIsConditionOnlyFragment(true);

                if (propertiesTypeMappingFragment.GetDefaultDiscriminator() != null)
                {
                    conditionTypeMappingFragment.SetDefaultDiscriminator(
                        propertiesTypeMappingFragment.GetDefaultDiscriminator());
                    propertiesTypeMappingFragment.RemoveDefaultDiscriminatorAnnotation();
                }
            }
            return conditionTypeMappingFragment;
        }

        private StorageEntityTypeMapping FindConditionTypeMapping(
            EntityType entityType, bool requiresSplit, StorageEntityTypeMapping propertiesTypeMapping)
        {
            var conditionTypeMapping = propertiesTypeMapping;

            if (requiresSplit)
            {
                if (!entityType.Abstract)
                {
                    conditionTypeMapping = propertiesTypeMapping.Clone();
                    conditionTypeMapping.RemoveIsOfType(conditionTypeMapping.EntityType);

                    var parentEntitySetMapping =
                        _databaseMapping.GetEntitySetMappings().Single(
                            esm => esm.EntityTypeMappings.Contains(propertiesTypeMapping));

                    parentEntitySetMapping.AddTypeMapping(conditionTypeMapping);
                }

                propertiesTypeMapping.MappingFragments.Each(tmf => tmf.ClearConditions());
            }
            return conditionTypeMapping;
        }

        private bool DetermineRequiresIsTypeOf(
            TableMapping tableMapping, EntitySet entitySet, EntityType entityType)
        {
            // IsTypeOf if this is the root for this table and any derived type shares a property mapping
            return entityType.IsRootOfSet(tableMapping.EntityTypes.GetEntityTypes(entitySet)) &&
                   ((tableMapping.EntityTypes.GetEntityTypes(entitySet).Count() > 1
                     && tableMapping.EntityTypes.GetEntityTypes(entitySet).Any(et => et != entityType && !et.Abstract))
                    ||
                    _tableMappings.Values.Any(
                        tm =>
                        tm != tableMapping
                        &&
                        tm.Table.ForeignKeyBuilders.Any(
                            fk => fk.GetIsTypeConstraint() && fk.PrincipalTable == tableMapping.Table)));
        }

        private static bool DetermineRequiresSplitEntityTypeMapping(
            TableMapping tableMapping,
            EntityType entityType,
            bool requiresIsTypeOf)
        {
            return requiresIsTypeOf && HasConditions(tableMapping, entityType);
        }

        /// <summary>
        /// Determines if the table and entity type need mapping, and if not, removes the existing entity type mapping
        /// </summary>
        private bool FindPropertyEntityTypeMapping(
            TableMapping tableMapping,
            EntitySet entitySet,
            EntityType entityType,
            bool requiresIsTypeOf,
            out StorageEntityTypeMapping entityTypeMapping,
            out StorageMappingFragment fragment)
        {
            entityTypeMapping = null;
            fragment = null;
            var mapping = (from etm in _databaseMapping.GetEntityTypeMappings(entityType)
                           from tmf in etm.MappingFragments
                           where tmf.Table == tableMapping.Table
                           select new
                               {
                                   TypeMapping = etm,
                                   Fragment = tmf
                               }).SingleOrDefault();

            if (mapping != null)
            {
                entityTypeMapping = mapping.TypeMapping;
                fragment = mapping.Fragment;
                if (!requiresIsTypeOf
                    && entityType.Abstract)
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

        private void RemoveFragment(
            EntitySet entitySet, StorageEntityTypeMapping entityTypeMapping, StorageMappingFragment fragment)
        {
            // Make the default discriminator nullable if this type isn't using it but there is a base type
            var defaultDiscriminator = fragment.GetDefaultDiscriminator();
            if (defaultDiscriminator != null
                && entityTypeMapping.EntityType.BaseType != null)
            {
                var columnMapping =
                    _tableMappings[fragment.Table].ColumnMappings.SingleOrDefault(
                        cm => cm.Column == defaultDiscriminator);
                if (columnMapping != null)
                {
                    var propertyMapping = columnMapping.PropertyMappings.SingleOrDefault(
                        pm => pm.EntityType == entityTypeMapping.EntityType);
                    if (propertyMapping != null)
                    {
                        columnMapping.PropertyMappings.Remove(propertyMapping);
                    }
                }
                defaultDiscriminator.Nullable = true;
            }

            entityTypeMapping.RemoveFragment(fragment);

            if (!entityTypeMapping.MappingFragments.Any())
            {
                _databaseMapping.GetEntitySetMapping(entitySet).RemoveTypeMapping(entityTypeMapping);
            }
        }

        private static void RemoveRedundantDefaultDiscriminators(TableMapping tableMapping)
        {
            foreach (var entitySet in tableMapping.EntityTypes.GetEntitySets())
            {
                (from cm in tableMapping.ColumnMappings
                 from pm in cm.PropertyMappings
                 where cm.PropertyMappings
                         .Where(pm1 => tableMapping.EntityTypes.GetEntityTypes(entitySet).Contains(pm1.EntityType))
                         .Count(pms => pms.IsDefaultDiscriminatorCondition) == 1
                 select new
                     {
                         ColumnMapping = cm,
                         PropertyMapping = pm
                     }).ToArray().Each(
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

        private static bool HasConditions(TableMapping tableMapping, EntityType entityType)
        {
            return tableMapping.ColumnMappings.SelectMany(cm => cm.PropertyMappings)
                               .Any(pm => pm.EntityType == entityType && pm.Conditions.Count > 0);
        }

        private static bool IsRootTypeMapping(
            Dictionary<EntityType, StorageEntityTypeMapping> rootMappings, EntityType entityType,
            IList<EdmProperty> propertyPath)
        {
            var baseType = (EntityType)entityType.BaseType;
            while (baseType != null)
            {
                StorageEntityTypeMapping rootMapping;
                if (rootMappings.TryGetValue(baseType, out rootMapping))
                {
                    return
                        rootMapping.MappingFragments.SelectMany(etmf => etmf.ColumnMappings).Any(
                            pm => pm.PropertyPath.SequenceEqual(propertyPath));
                }
                baseType = (EntityType)baseType.BaseType;
            }
            return false;
        }

        private TableMapping FindOrCreateTableMapping(EntityType table)
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
