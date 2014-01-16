// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Xml.Linq;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal class EdmModelDiffer
    {
        private static readonly PrimitiveTypeKind[] _validIdentityTypes =
        {
            PrimitiveTypeKind.Byte,
            PrimitiveTypeKind.Decimal,
            PrimitiveTypeKind.Guid,
            PrimitiveTypeKind.Int16,
            PrimitiveTypeKind.Int32,
            PrimitiveTypeKind.Int64
        };

        private class ModelMetadata
        {
            public EdmItemCollection EdmItemCollection { get; set; }
            public StoreItemCollection StoreItemCollection { get; set; }
            public EntityContainerMapping EntityContainerMapping { get; set; }
            public EntityContainer StoreEntityContainer { get; set; }
            public DbProviderManifest ProviderManifest { get; set; }
            public DbProviderInfo ProviderInfo { get; set; }
        }

        private static readonly DynamicEqualityComparer<ForeignKeyOperation> _foreignKeyEqualityComparer
            = new DynamicEqualityComparer<ForeignKeyOperation>((fk1, fk2) => fk1.Name.EqualsOrdinal(fk2.Name));

        private static readonly DynamicEqualityComparer<IndexOperation> _indexEqualityComparer
            = new DynamicEqualityComparer<IndexOperation>(
                (i1, i2) => i1.Name.EqualsOrdinal(i2.Name)
                            && i1.Table.EqualsOrdinal(i2.Table));

        private ModelMetadata _source;
        private ModelMetadata _target;

        public ICollection<MigrationOperation> Diff(
            XDocument sourceModel,
            XDocument targetModel,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator = null,
            MigrationSqlGenerator migrationSqlGenerator = null)
        {
            DebugCheck.NotNull(sourceModel);
            DebugCheck.NotNull(targetModel);

            if (sourceModel == targetModel
                || XNode.DeepEquals(sourceModel, targetModel))
            {
                // Trivial checks before we do the hard stuff...
                return new MigrationOperation[0];
            }

            DbProviderInfo providerInfo;

            var storageMappingItemCollection
                = sourceModel.GetStorageMappingItemCollection(out providerInfo);

            var source
                = new ModelMetadata
                {
                    EdmItemCollection = storageMappingItemCollection.EdmItemCollection,
                    StoreItemCollection = storageMappingItemCollection.StoreItemCollection,
                    StoreEntityContainer 
                        = storageMappingItemCollection.StoreItemCollection.GetItems<EntityContainer>().Single(),
                    EntityContainerMapping
                        = storageMappingItemCollection.GetItems<EntityContainerMapping>().Single(),
                    ProviderManifest = GetProviderManifest(providerInfo),
                    ProviderInfo = providerInfo
                };

            storageMappingItemCollection
                = targetModel.GetStorageMappingItemCollection(out providerInfo);

            var target
                = new ModelMetadata
                {
                    EdmItemCollection = storageMappingItemCollection.EdmItemCollection,
                    StoreItemCollection = storageMappingItemCollection.StoreItemCollection,
                    StoreEntityContainer
                        = storageMappingItemCollection.StoreItemCollection.GetItems<EntityContainer>().Single(),
                    EntityContainerMapping
                        = storageMappingItemCollection.GetItems<EntityContainerMapping>().Single(),
                    ProviderManifest = GetProviderManifest(providerInfo),
                    ProviderInfo = providerInfo
                };

            return Diff(source, target, modificationCommandTreeGenerator, migrationSqlGenerator);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private ICollection<MigrationOperation> Diff(
            ModelMetadata source,
            ModelMetadata target,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(target);

            _source = source;
            _target = target;

            var entityTypePairs = FindEntityTypePairs().ToList();
            var mappingFragmentPairs = FindMappingFragmentPairs(entityTypePairs).ToList();
            var associationTypePairs = FindAssociationTypePairs(entityTypePairs).ToList();
            var tablePairs = FindTablePairs(mappingFragmentPairs, associationTypePairs).ToList();

            associationTypePairs.AddRange(FindStoreOnlyAssociationTypePairs(associationTypePairs, tablePairs));

            var renamedTablePairs = FindRenamedTablePairs(tablePairs).ToList();
            var renamedTables = FindRenamedTables(renamedTablePairs).ToList();
            var renamedColumns = FindRenamedColumns(mappingFragmentPairs, associationTypePairs).ToList();

            var addedColumns = FindAddedColumns(tablePairs, renamedColumns).ToList();
            var droppedColumns = FindDroppedColumns(tablePairs, renamedColumns).ToList();
            var alteredColumns = FindAlteredColumns(tablePairs, renamedColumns).ToList();
            var orphanedColumns = FindOrphanedColumns(tablePairs, renamedColumns).ToList();

            var movedTables = FindMovedTables(tablePairs).ToList();
            var addedTables = FindAddedTables(tablePairs).ToList();
            var droppedTables = FindDroppedTables(tablePairs).ToList();
            var alteredTables = FindAlteredTables(tablePairs).ToList();

            var alteredPrimaryKeys
                = FindAlteredPrimaryKeys(tablePairs, renamedColumns, alteredColumns)
                    .ToList();

            var addedForeignKeys
                = FindAddedForeignKeys(associationTypePairs, renamedColumns)
                    .Concat(alteredPrimaryKeys.OfType<AddForeignKeyOperation>())
                    .ToList();

            var droppedForeignKeys
                = FindDroppedForeignKeys(associationTypePairs, renamedColumns)
                    .Concat(alteredPrimaryKeys.OfType<DropForeignKeyOperation>())
                    .ToList();

            var addedModificationFunctions
                = FindAddedModificationFunctions(modificationCommandTreeGenerator, migrationSqlGenerator)
                    .ToList();

            var alteredModificationFunctions
                = FindAlteredModificationFunctions(modificationCommandTreeGenerator, migrationSqlGenerator)
                    .ToList();

            var removedModificationFunctions = FindDroppedModificationFunctions().ToList();
            var renamedModificationFunctions = FindRenamedModificationFunctions().ToList();
            var movedModificationFunctions = FindMovedModificationFunctions().ToList();

            var sourceIndexes = FindSourceIndexes(renamedTablePairs, renamedColumns).ToList();
            var targetIndexes = FindTargetIndexes().ToList();

            var droppedIndexes = FindDroppedIndexes(sourceIndexes, targetIndexes).ToList();
            var addedIndexes = FindAddedIndexes(sourceIndexes, targetIndexes).ToList();

            return HandleTransitiveRenameDependencies(renamedTables)
                .Concat<MigrationOperation>(movedTables)
                .Concat(droppedForeignKeys.Distinct(_foreignKeyEqualityComparer))
                .Concat(
                    droppedForeignKeys
                        .Select(fko => fko.CreateDropIndexOperation())
                        .Concat(droppedIndexes)
                        .Distinct(_indexEqualityComparer))
                .Concat(orphanedColumns)
                .Concat(HandleTransitiveRenameDependencies(renamedColumns))
                .Concat(alteredPrimaryKeys.OfType<DropPrimaryKeyOperation>())
                .Concat(addedTables)
                .Concat(alteredTables)
                .Concat(addedColumns)
                .Concat(alteredColumns)
                .Concat(alteredPrimaryKeys.OfType<AddPrimaryKeyOperation>())
                .Concat(
                    addedForeignKeys
                        .Select(fko => fko.CreateCreateIndexOperation())
                        .Concat(addedIndexes)
                        .Distinct(_indexEqualityComparer))
                .Concat(addedForeignKeys.Distinct(_foreignKeyEqualityComparer))
                .Concat(droppedColumns)
                .Concat(droppedTables)
                .Concat(addedModificationFunctions)
                .Concat(movedModificationFunctions)
                .Concat(renamedModificationFunctions)
                .Concat(alteredModificationFunctions)
                .Concat(removedModificationFunctions)
                .ToList();
        }

        private IEnumerable<Tuple<EntityType, EntityType>> FindEntityTypePairs()
        {
            var entityPairs
                = (from et1 in _source.EdmItemCollection.GetItems<EntityType>()
                   from et2 in _target.EdmItemCollection.GetItems<EntityType>()
                   where et1.Name.EqualsOrdinal(et2.Name)
                   // easy case, names match
                   select Tuple.Create(et1, et2)).ToList();

            var sourceEntityTypes
                = entityPairs.Select(t => t.Item1)
                    .ToList();

            var sourceRemainingEntities
                = _source.EdmItemCollection
                    .GetItems<EntityType>()
                    .Except(sourceEntityTypes)
                    .ToList();

            var targetEntityTypes
                = entityPairs.Select(t => t.Item2)
                    .ToList();

            var targetRemainingEntities
                = _target.EdmItemCollection
                    .GetItems<EntityType>()
                    .Except(targetEntityTypes)
                    .ToList();

            return entityPairs.Concat(
                from et1 in sourceRemainingEntities
                from et2 in targetRemainingEntities
                where FuzzyMatchEntities(et1, et2)
                select Tuple.Create(et1, et2));
        }

        private static bool FuzzyMatchEntities(EntityType entityType1, EntityType entityType2)
        {
            DebugCheck.NotNull(entityType1);
            DebugCheck.NotNull(entityType2);

            if (!entityType1.KeyMembers
                .SequenceEqual(
                    entityType2.KeyMembers,
                    new DynamicEqualityComparer<EdmMember>((m1, m2) => m1.EdmEquals(m2))))
            {
                // Keys don't match
                return false;
            }

            if ((entityType1.BaseType != null && entityType2.BaseType == null)
                || (entityType1.BaseType == null && entityType2.BaseType != null))
            {
                // Inheritance mismatch
                return false;
            }

            // Find declared members that are the same across both entities
            var matchingMemberCount
                = (from m1 in entityType1.DeclaredMembers
                   from m2 in entityType2.DeclaredMembers
                   where m1.EdmEquals(m2)
                   select 1)
                    .Count();

            // Entities match if at least 80% of members matched across both tables
            return ((matchingMemberCount * 2.0f)
                    / (entityType1.DeclaredMembers.Count + entityType2.DeclaredMembers.Count)) > 0.80;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private IEnumerable<Tuple<MappingFragment, MappingFragment>> FindMappingFragmentPairs(
            ICollection<Tuple<EntityType, EntityType>> entityTypePairs)
        {
            DebugCheck.NotNull(entityTypePairs);

            // Zip the two models together. Our goal here is to match mapping fragments across
            // the source and target input models.

            var targetEntityTypeMappings 
                = _target.EntityContainerMapping.EntitySetMappings
                    .SelectMany(esm => esm.EntityTypeMappings)
                    .ToList();

            foreach (var etm1
                in _source.EntityContainerMapping.EntitySetMappings.SelectMany(esm => esm.EntityTypeMappings))
            {
                foreach (var etm2 in targetEntityTypeMappings)
                {
                    if (entityTypePairs
                        .Any(
                            t => (etm1.EntityType != null
                                  && etm2.EntityType != null
                                  && t.Item1 == etm1.EntityType
                                  && t.Item2 == etm2.EntityType)
                                 || (etm1.EntityType == null
                                     && etm2.EntityType == null
                                     && etm1.IsOfTypes.Contains(t.Item1)
                                     && etm2.IsOfTypes.Contains(t.Item2)
                                     && etm1.IsOfTypes.Except(new[] { t.Item1 }).Select(et => et.Name)
                                         .SequenceEqual(etm2.IsOfTypes.Except(new[] { t.Item2 }).Select(et => et.Name)))))
                    {
                        foreach (var t in etm1.MappingFragments.Zip(etm2.MappingFragments, Tuple.Create))
                        {
                            yield return t;
                        }

                        break;
                    }
                }
            }
        }

        private IEnumerable<Tuple<AssociationType, AssociationType>> FindAssociationTypePairs(
            ICollection<Tuple<EntityType, EntityType>> entityTypePairs)
        {
            DebugCheck.NotNull(entityTypePairs);

            // Zip the two models together. Our goal here is to match store association types across
            // the source and target input models.

            var storeAssociationTypePairs
                = (from ets in entityTypePairs
                   from np1 in ets.Item1.NavigationProperties
                   from np2 in ets.Item2.NavigationProperties
                   where np1.Name.EqualsIgnoreCase(np2.Name)
                   from t in GetStoreAssociationTypePairs(np1.Association, np2.Association, entityTypePairs)
                   select t)
                    .Distinct()
                    .ToList();

            var sourceRemainingAssociationTypes
                = _source.StoreItemCollection
                    .GetItems<AssociationType>()
                    .Except(storeAssociationTypePairs.Select(t => t.Item1))
                    .ToList();

            var targetRemainingAssociationTypes
                = _target.StoreItemCollection
                    .GetItems<AssociationType>()
                    .Except(storeAssociationTypePairs.Select(t => t.Item2))
                    .ToList();

            return storeAssociationTypePairs
                .Concat(
                    from at1 in sourceRemainingAssociationTypes
                    from at2 in targetRemainingAssociationTypes
                    where at1.Name.EqualsIgnoreCase(at2.Name)
                    select Tuple.Create(at1, at2));
        }

        private IEnumerable<Tuple<AssociationType, AssociationType>> GetStoreAssociationTypePairs(
            AssociationType conceptualAssociationType1,
            AssociationType conceptualAssociationType2,
            ICollection<Tuple<EntityType, EntityType>> entityTypePairs)
        {
            DebugCheck.NotNull(conceptualAssociationType1);
            DebugCheck.NotNull(conceptualAssociationType2);
            DebugCheck.NotNull(entityTypePairs);

            AssociationType associationType1;
            AssociationType associationType2;

            if (_source.StoreItemCollection
                .TryGetItem(
                    GetStoreAssociationIdentity(conceptualAssociationType1.Name),
                    out associationType1)
                && _target.StoreItemCollection
                    .TryGetItem(
                        GetStoreAssociationIdentity(conceptualAssociationType2.Name),
                        out associationType2))
            {
                // Non many-to-many case; one FK per conceptual association
                yield return Tuple.Create(associationType1, associationType2);
            }
            else
            {
                // Many-to-many case, two FKs per conceptual association. We
                // need to pair up the ends

                var sourceEnd1 = conceptualAssociationType1.SourceEnd;

                var sourceEndEntityTypePair
                    = entityTypePairs
                        .Single(t => t.Item1 == sourceEnd1.GetEntityType());

                var sourceEnd2
                    = conceptualAssociationType2.SourceEnd.GetEntityType() == sourceEndEntityTypePair.Item2
                        ? conceptualAssociationType2.SourceEnd
                        : conceptualAssociationType2.TargetEnd;

                if (_source.StoreItemCollection
                    .TryGetItem(GetStoreAssociationIdentity(sourceEnd1.Name), out associationType1)
                    && _target.StoreItemCollection
                        .TryGetItem(GetStoreAssociationIdentity(sourceEnd2.Name), out associationType2))
                {
                    yield return Tuple.Create(associationType1, associationType2);
                }

                var targetEnd1 = conceptualAssociationType1.GetOtherEnd(sourceEnd1);
                var targetEnd2 = conceptualAssociationType2.GetOtherEnd(sourceEnd2);

                if (_source.StoreItemCollection
                    .TryGetItem(GetStoreAssociationIdentity(targetEnd1.Name), out associationType1)
                    && _target.StoreItemCollection
                        .TryGetItem(GetStoreAssociationIdentity(targetEnd2.Name), out associationType2))
                {
                    yield return Tuple.Create(associationType1, associationType2);
                }
            }
        }

        private IEnumerable<Tuple<AssociationType, AssociationType>> FindStoreOnlyAssociationTypePairs(
            ICollection<Tuple<AssociationType, AssociationType>> associationTypePairs,
            ICollection<Tuple<EntitySet, EntitySet>> tablePairs)
        {
            DebugCheck.NotNull(associationTypePairs);
            DebugCheck.NotNull(tablePairs);

            var sourceRemainingAssociationTypes
                = _source.StoreItemCollection
                    .GetItems<AssociationType>()
                    .Except(associationTypePairs.Select(t => t.Item1))
                    .ToList();

            var targetRemainingAssociationTypes
                = _target.StoreItemCollection
                    .GetItems<AssociationType>()
                    .Except(associationTypePairs.Select(t => t.Item2))
                    .ToList();

            return
                from at1 in sourceRemainingAssociationTypes
                from at2 in targetRemainingAssociationTypes
                where tablePairs.Any(
                    t => t.Item1.ElementType == at1.Constraint.PrincipalEnd.GetEntityType()
                         && t.Item2.ElementType == at2.Constraint.PrincipalEnd.GetEntityType())
                      && tablePairs.Any(
                          t => t.Item1.ElementType == at1.Constraint.DependentEnd.GetEntityType()
                               && t.Item2.ElementType == at2.Constraint.DependentEnd.GetEntityType())
                select Tuple.Create(at1, at2);
        }

        private static string GetStoreAssociationIdentity(string associationName)
        {
            DebugCheck.NotEmpty(associationName);

            return EdmModelExtensions.DefaultStoreNamespace + "." + associationName;
        }

        private IEnumerable<Tuple<EntitySet, EntitySet>> FindTablePairs(
            ICollection<Tuple<MappingFragment, MappingFragment>> mappingFragmentPairs,
            ICollection<Tuple<AssociationType, AssociationType>> associationTypePairs)
        {
            DebugCheck.NotNull(mappingFragmentPairs);
            DebugCheck.NotNull(associationTypePairs);

            // Zip the two models together. Our goal here is to match tables across
            // the source and target input models.

            var sourceTables = new HashSet<EntitySet>();
            var targetTables = new HashSet<EntitySet>();

            foreach (var mappingFragmentPair in mappingFragmentPairs)
            {
                var sourceTable = mappingFragmentPair.Item1.TableSet;
                var targetTable = mappingFragmentPair.Item2.TableSet;

                if (!sourceTables.Contains(sourceTable)
                    && !targetTables.Contains(targetTable))
                {
                    sourceTables.Add(sourceTable);
                    targetTables.Add(targetTable);

                    yield return Tuple.Create(sourceTable, targetTable);
                }
            }

            foreach (var associationTypePair in associationTypePairs)
            {
                var sourceTable
                    = _source.StoreEntityContainer.EntitySets
                        .Single(es => es.ElementType == associationTypePair.Item1.Constraint.DependentEnd.GetEntityType());

                var targetTable
                    = _target.StoreEntityContainer.EntitySets
                        .Single(es => es.ElementType == associationTypePair.Item2.Constraint.DependentEnd.GetEntityType());

                if (!sourceTables.Contains(sourceTable)
                    && !targetTables.Contains(targetTable))
                {
                    sourceTables.Add(sourceTable);
                    targetTables.Add(targetTable);

                    yield return Tuple.Create(sourceTable, targetTable);
                }
            }
        }

        private static IEnumerable<RenameTableOperation> HandleTransitiveRenameDependencies(
            IList<RenameTableOperation> renameTableOperations)
        {
            DebugCheck.NotNull(renameTableOperations);

            return HandleTransitiveRenameDependencies(
                renameTableOperations,
                (rt1, rt2) =>
                {
                    var databaseName1 = DatabaseName.Parse(rt1.Name);
                    var databaseName2 = DatabaseName.Parse(rt2.Name);

                    return databaseName1.Name.EqualsIgnoreCase(rt2.NewName)
                           && databaseName1.Schema.EqualsIgnoreCase(databaseName2.Schema);
                },
                (t, rt) => new RenameTableOperation(t, rt.NewName),
                (rt, t) => rt.NewName = t);
        }

        private static IEnumerable<RenameColumnOperation> HandleTransitiveRenameDependencies(
            IList<RenameColumnOperation> renameColumnOperations)
        {
            DebugCheck.NotNull(renameColumnOperations);

            return HandleTransitiveRenameDependencies(
                renameColumnOperations,
                (rc1, rc2) => rc1.Table.EqualsIgnoreCase(rc2.Table)
                              && rc1.Name.EqualsIgnoreCase(rc2.NewName),
                (t, rc) => new RenameColumnOperation(rc.Table, t, rc.NewName),
                (rc, t) => rc.NewName = t);
        }

        private static IEnumerable<T> HandleTransitiveRenameDependencies<T>(
            IList<T> renameOperations,
            Func<T, T, bool> dependencyFinder,
            Func<string, T, T> renameCreator,
            Action<T, string> setNewName)
            where T : class
        {
            DebugCheck.NotNull(renameOperations);
            DebugCheck.NotNull(dependencyFinder);
            DebugCheck.NotNull(renameCreator);
            DebugCheck.NotNull(setNewName);

            var tempCounter = 0;
            var tempRenames = new List<T>();

            for (var i = 0; i < renameOperations.Count; i++)
            {
                var renameOperation = renameOperations[i];

                var dependentRename
                    = renameOperations
                        .Skip(i + 1)
                        .SingleOrDefault(rt => dependencyFinder(renameOperation, rt));

                if (dependentRename != null)
                {
                    var tempNewName = "__mig_tmp__" + tempCounter++;

                    tempRenames.Add(renameCreator(tempNewName, renameOperation));

                    setNewName(renameOperation, tempNewName);
                }

                yield return renameOperation;
            }

            foreach (var renameOperation in tempRenames)
            {
                yield return renameOperation;
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private IEnumerable<MoveProcedureOperation> FindMovedModificationFunctions()
        {
            return
                (from esm1 in _source.EntityContainerMapping.EntitySetMappings
                 from mfm1 in esm1.ModificationFunctionMappings
                 from esm2 in _target.EntityContainerMapping.EntitySetMappings
                 from mfm2 in esm2.ModificationFunctionMappings
                 where mfm1.EntityType.Identity == mfm2.EntityType.Identity
                 from o in DiffModificationFunctionSchemas(mfm1, mfm2)
                 select o)
                    .Concat(
                        from asm1 in _source.EntityContainerMapping.AssociationSetMappings
                        where asm1.ModificationFunctionMapping != null
                        from asm2 in _target.EntityContainerMapping.AssociationSetMappings
                        where asm2.ModificationFunctionMapping != null
                              && asm1.ModificationFunctionMapping.AssociationSet.Identity
                              == asm2.ModificationFunctionMapping.AssociationSet.Identity
                        from o in DiffModificationFunctionSchemas(asm1.ModificationFunctionMapping, asm2.ModificationFunctionMapping)
                        select o);
        }

        private static IEnumerable<MoveProcedureOperation> DiffModificationFunctionSchemas(
            EntityTypeModificationFunctionMapping sourceModificationFunctionMapping,
            EntityTypeModificationFunctionMapping targetModificationFunctionMapping)
        {
            DebugCheck.NotNull(sourceModificationFunctionMapping);
            DebugCheck.NotNull(targetModificationFunctionMapping);

            if (!sourceModificationFunctionMapping.InsertFunctionMapping.Function.Schema
                .EqualsOrdinal(targetModificationFunctionMapping.InsertFunctionMapping.Function.Schema))
            {
                yield return new MoveProcedureOperation(
                    GetSchemaQualifiedName(sourceModificationFunctionMapping.InsertFunctionMapping.Function),
                    targetModificationFunctionMapping.InsertFunctionMapping.Function.Schema);
            }

            if (!sourceModificationFunctionMapping.UpdateFunctionMapping.Function.Schema
                .EqualsOrdinal(targetModificationFunctionMapping.UpdateFunctionMapping.Function.Schema))
            {
                yield return new MoveProcedureOperation(
                    GetSchemaQualifiedName(sourceModificationFunctionMapping.UpdateFunctionMapping.Function),
                    targetModificationFunctionMapping.UpdateFunctionMapping.Function.Schema);
            }

            if (!sourceModificationFunctionMapping.DeleteFunctionMapping.Function.Schema
                .EqualsOrdinal(targetModificationFunctionMapping.DeleteFunctionMapping.Function.Schema))
            {
                yield return new MoveProcedureOperation(
                    GetSchemaQualifiedName(sourceModificationFunctionMapping.DeleteFunctionMapping.Function),
                    targetModificationFunctionMapping.DeleteFunctionMapping.Function.Schema);
            }
        }

        private static IEnumerable<MoveProcedureOperation> DiffModificationFunctionSchemas(
            AssociationSetModificationFunctionMapping sourceModificationFunctionMapping,
            AssociationSetModificationFunctionMapping targetModificationFunctionMapping)
        {
            DebugCheck.NotNull(sourceModificationFunctionMapping);
            DebugCheck.NotNull(targetModificationFunctionMapping);

            if (!sourceModificationFunctionMapping.InsertFunctionMapping.Function.Schema
                .EqualsOrdinal(targetModificationFunctionMapping.InsertFunctionMapping.Function.Schema))
            {
                yield return new MoveProcedureOperation(
                    GetSchemaQualifiedName(sourceModificationFunctionMapping.InsertFunctionMapping.Function),
                    targetModificationFunctionMapping.InsertFunctionMapping.Function.Schema);
            }

            if (!sourceModificationFunctionMapping.DeleteFunctionMapping.Function.Schema
                .EqualsOrdinal(targetModificationFunctionMapping.DeleteFunctionMapping.Function.Schema))
            {
                yield return new MoveProcedureOperation(
                    GetSchemaQualifiedName(sourceModificationFunctionMapping.DeleteFunctionMapping.Function),
                    targetModificationFunctionMapping.DeleteFunctionMapping.Function.Schema);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private IEnumerable<CreateProcedureOperation> FindAddedModificationFunctions(
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator, MigrationSqlGenerator migrationSqlGenerator)
        {
            return
                (from esm1 in _target.EntityContainerMapping.EntitySetMappings
                 from mfm1 in esm1.ModificationFunctionMappings
                 where !(from esm2 in _source.EntityContainerMapping.EntitySetMappings
                         from mfm2 in esm2.ModificationFunctionMappings
                         where mfm1.EntityType.Identity == mfm2.EntityType.Identity
                         select mfm2
                     ).Any()
                 from o in BuildCreateProcedureOperations(mfm1, modificationCommandTreeGenerator, migrationSqlGenerator)
                 select o)
                    .Concat(
                        from asm1 in _target.EntityContainerMapping.AssociationSetMappings
                        where asm1.ModificationFunctionMapping != null
                        where !(from asm2 in _source.EntityContainerMapping.AssociationSetMappings
                                where asm2.ModificationFunctionMapping != null
                                      && asm1.ModificationFunctionMapping.AssociationSet.Identity
                                      == asm2.ModificationFunctionMapping.AssociationSet.Identity
                                select asm2.ModificationFunctionMapping
                            ).Any()
                        from o in BuildCreateProcedureOperations(
                            asm1.ModificationFunctionMapping,
                            modificationCommandTreeGenerator,
                            migrationSqlGenerator)
                        select o);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private IEnumerable<RenameProcedureOperation> FindRenamedModificationFunctions()
        {
            return
                (from esm1 in _source.EntityContainerMapping.EntitySetMappings
                 from mfm1 in esm1.ModificationFunctionMappings
                 from esm2 in _target.EntityContainerMapping.EntitySetMappings
                 from mfm2 in esm2.ModificationFunctionMappings
                 where mfm1.EntityType.Identity == mfm2.EntityType.Identity
                 from o in DiffModificationFunctionNames(mfm1, mfm2)
                 select o)
                    .Concat(
                        from asm1 in _source.EntityContainerMapping.AssociationSetMappings
                        where asm1.ModificationFunctionMapping != null
                        from asm2 in _target.EntityContainerMapping.AssociationSetMappings
                        where asm2.ModificationFunctionMapping != null
                              && asm1.ModificationFunctionMapping.AssociationSet.Identity
                              == asm2.ModificationFunctionMapping.AssociationSet.Identity
                        from o in DiffModificationFunctionNames(asm1.ModificationFunctionMapping, asm2.ModificationFunctionMapping)
                        select o);
        }

        private static IEnumerable<RenameProcedureOperation> DiffModificationFunctionNames(
            AssociationSetModificationFunctionMapping sourceModificationFunctionMapping,
            AssociationSetModificationFunctionMapping targetModificationFunctionMapping)
        {
            DebugCheck.NotNull(sourceModificationFunctionMapping);
            DebugCheck.NotNull(targetModificationFunctionMapping);

            if (!sourceModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName
                .EqualsOrdinal(targetModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName))
            {
                yield return new RenameProcedureOperation(
                    GetSchemaQualifiedName(
                        sourceModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName,
                        targetModificationFunctionMapping.InsertFunctionMapping.Function.Schema),
                    targetModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName);
            }

            if (!sourceModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName
                .EqualsOrdinal(targetModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName))
            {
                yield return new RenameProcedureOperation(
                    GetSchemaQualifiedName(
                        sourceModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName,
                        targetModificationFunctionMapping.DeleteFunctionMapping.Function.Schema),
                    targetModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName);
            }
        }

        private static IEnumerable<RenameProcedureOperation> DiffModificationFunctionNames(
            EntityTypeModificationFunctionMapping sourceModificationFunctionMapping,
            EntityTypeModificationFunctionMapping targetModificationFunctionMapping)
        {
            DebugCheck.NotNull(sourceModificationFunctionMapping);
            DebugCheck.NotNull(targetModificationFunctionMapping);

            if (!sourceModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName
                .EqualsOrdinal(targetModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName))
            {
                yield return new RenameProcedureOperation(
                    GetSchemaQualifiedName(
                        sourceModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName,
                        targetModificationFunctionMapping.InsertFunctionMapping.Function.Schema),
                    targetModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName);
            }

            if (!sourceModificationFunctionMapping.UpdateFunctionMapping.Function.FunctionName
                .EqualsOrdinal(targetModificationFunctionMapping.UpdateFunctionMapping.Function.FunctionName))
            {
                yield return new RenameProcedureOperation(
                    GetSchemaQualifiedName(
                        sourceModificationFunctionMapping.UpdateFunctionMapping.Function.FunctionName,
                        targetModificationFunctionMapping.UpdateFunctionMapping.Function.Schema),
                    targetModificationFunctionMapping.UpdateFunctionMapping.Function.FunctionName);
            }

            if (!sourceModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName
                .EqualsOrdinal(targetModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName))
            {
                yield return new RenameProcedureOperation(
                    GetSchemaQualifiedName(
                        sourceModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName,
                        targetModificationFunctionMapping.DeleteFunctionMapping.Function.Schema),
                    targetModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName);
            }
        }

        private static string GetSchemaQualifiedName(string table, string schema)
        {
            DebugCheck.NotEmpty(table);
            DebugCheck.NotEmpty(schema);

            return new DatabaseName(table, schema).ToString();
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private IEnumerable<AlterProcedureOperation> FindAlteredModificationFunctions(
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator, MigrationSqlGenerator migrationSqlGenerator)
        {
            return
                (from esm1 in _source.EntityContainerMapping.EntitySetMappings
                 from mfm1 in esm1.ModificationFunctionMappings
                 from esm2 in _target.EntityContainerMapping.EntitySetMappings
                 from mfm2 in esm2.ModificationFunctionMappings
                 where mfm1.EntityType.Identity == mfm2.EntityType.Identity
                 from o in DiffModificationFunctions(mfm1, mfm2, modificationCommandTreeGenerator, migrationSqlGenerator)
                 select o)
                    .Concat(
                        from asm1 in _source.EntityContainerMapping.AssociationSetMappings
                        where asm1.ModificationFunctionMapping != null
                        from asm2 in _target.EntityContainerMapping.AssociationSetMappings
                        where asm2.ModificationFunctionMapping != null
                              && asm1.ModificationFunctionMapping.AssociationSet.Identity
                              == asm2.ModificationFunctionMapping.AssociationSet.Identity
                        from o in DiffModificationFunctions(
                            asm1.ModificationFunctionMapping,
                            asm2.ModificationFunctionMapping,
                            modificationCommandTreeGenerator,
                            migrationSqlGenerator)
                        select o);
        }

        private IEnumerable<AlterProcedureOperation> DiffModificationFunctions(
            AssociationSetModificationFunctionMapping sourceModificationFunctionMapping,
            AssociationSetModificationFunctionMapping targetModificationFunctionMapping,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator)
        {
            DebugCheck.NotNull(sourceModificationFunctionMapping);
            DebugCheck.NotNull(targetModificationFunctionMapping);

            if (!DiffModificationFunction(
                sourceModificationFunctionMapping.InsertFunctionMapping,
                targetModificationFunctionMapping.InsertFunctionMapping))
            {
                yield return BuildAlterProcedureOperation(
                    targetModificationFunctionMapping.InsertFunctionMapping.Function,
                    GenerateInsertFunctionBody(
                        targetModificationFunctionMapping,
                        modificationCommandTreeGenerator,
                        migrationSqlGenerator));
            }

            if (!DiffModificationFunction(
                sourceModificationFunctionMapping.DeleteFunctionMapping,
                targetModificationFunctionMapping.DeleteFunctionMapping))
            {
                yield return BuildAlterProcedureOperation(
                    targetModificationFunctionMapping.DeleteFunctionMapping.Function,
                    GenerateDeleteFunctionBody(
                        targetModificationFunctionMapping,
                        modificationCommandTreeGenerator,
                        migrationSqlGenerator));
            }
        }

        private IEnumerable<AlterProcedureOperation> DiffModificationFunctions(
            EntityTypeModificationFunctionMapping sourceModificationFunctionMapping,
            EntityTypeModificationFunctionMapping targetModificationFunctionMapping,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator)
        {
            DebugCheck.NotNull(sourceModificationFunctionMapping);
            DebugCheck.NotNull(targetModificationFunctionMapping);

            if (!DiffModificationFunction(
                sourceModificationFunctionMapping.InsertFunctionMapping,
                targetModificationFunctionMapping.InsertFunctionMapping))
            {
                yield return BuildAlterProcedureOperation(
                    targetModificationFunctionMapping.InsertFunctionMapping.Function,
                    GenerateInsertFunctionBody(
                        targetModificationFunctionMapping,
                        modificationCommandTreeGenerator,
                        migrationSqlGenerator));
            }

            if (!DiffModificationFunction(
                sourceModificationFunctionMapping.UpdateFunctionMapping,
                targetModificationFunctionMapping.UpdateFunctionMapping))
            {
                yield return BuildAlterProcedureOperation(
                    targetModificationFunctionMapping.UpdateFunctionMapping.Function,
                    GenerateUpdateFunctionBody(
                        targetModificationFunctionMapping,
                        modificationCommandTreeGenerator,
                        migrationSqlGenerator));
            }

            if (!DiffModificationFunction(
                sourceModificationFunctionMapping.DeleteFunctionMapping,
                targetModificationFunctionMapping.DeleteFunctionMapping))
            {
                yield return BuildAlterProcedureOperation(
                    targetModificationFunctionMapping.DeleteFunctionMapping.Function,
                    GenerateDeleteFunctionBody(
                        targetModificationFunctionMapping,
                        modificationCommandTreeGenerator,
                        migrationSqlGenerator));
            }
        }

        private string GenerateInsertFunctionBody(
            EntityTypeModificationFunctionMapping modificationFunctionMapping,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            return GenerateFunctionBody(
                modificationFunctionMapping,
                (m, s) => m.GenerateInsert(s),
                modificationCommandTreeGenerator,
                migrationSqlGenerator,
                modificationFunctionMapping.InsertFunctionMapping.Function.FunctionName,
                rowsAffectedParameterName: null);
        }

        private string GenerateInsertFunctionBody(
            AssociationSetModificationFunctionMapping modificationFunctionMapping,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            return GenerateFunctionBody(
                modificationFunctionMapping,
                (m, s) => m.GenerateAssociationInsert(s),
                modificationCommandTreeGenerator,
                migrationSqlGenerator,
                rowsAffectedParameterName: null);
        }

        private string GenerateUpdateFunctionBody(
            EntityTypeModificationFunctionMapping modificationFunctionMapping,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            return GenerateFunctionBody(
                modificationFunctionMapping,
                (m, s) => m.GenerateUpdate(s),
                modificationCommandTreeGenerator,
                migrationSqlGenerator,
                modificationFunctionMapping.UpdateFunctionMapping.Function.FunctionName,
                rowsAffectedParameterName: modificationFunctionMapping.UpdateFunctionMapping.RowsAffectedParameterName);
        }

        private string GenerateDeleteFunctionBody(
            EntityTypeModificationFunctionMapping modificationFunctionMapping,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            return GenerateFunctionBody(
                modificationFunctionMapping,
                (m, s) => m.GenerateDelete(s),
                modificationCommandTreeGenerator,
                migrationSqlGenerator,
                modificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName,
                rowsAffectedParameterName: modificationFunctionMapping.DeleteFunctionMapping.RowsAffectedParameterName);
        }

        private string GenerateDeleteFunctionBody(
            AssociationSetModificationFunctionMapping modificationFunctionMapping,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            return GenerateFunctionBody(
                modificationFunctionMapping,
                (m, s) => m.GenerateAssociationDelete(s),
                modificationCommandTreeGenerator,
                migrationSqlGenerator,
                rowsAffectedParameterName: modificationFunctionMapping.DeleteFunctionMapping.RowsAffectedParameterName);
        }

        private string GenerateFunctionBody<TCommandTree>(
            EntityTypeModificationFunctionMapping modificationFunctionMapping,
            Func<ModificationCommandTreeGenerator, string, IEnumerable<TCommandTree>> treeGenerator,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator,
            string functionName,
            string rowsAffectedParameterName)
            where TCommandTree : DbModificationCommandTree
        {
            DebugCheck.NotNull(modificationFunctionMapping);
            DebugCheck.NotNull(treeGenerator);

            var commandTrees = new TCommandTree[0];

            if (modificationCommandTreeGenerator != null)
            {
                var dynamicToFunctionModificationCommandConverter
                    = new DynamicToFunctionModificationCommandConverter(
                        modificationFunctionMapping,
                        _target.EntityContainerMapping);

                try
                {
                    commandTrees
                        = dynamicToFunctionModificationCommandConverter
                            .Convert(treeGenerator(modificationCommandTreeGenerator.Value, modificationFunctionMapping.EntityType.Identity))
                            .ToArray();
                }
                catch (UpdateException e)
                {
                    throw new InvalidOperationException(
                        Strings.ErrorGeneratingCommandTree(
                            functionName,
                            modificationFunctionMapping.EntityType.Name), e);
                }
            }

            return GenerateFunctionBody(migrationSqlGenerator, rowsAffectedParameterName, commandTrees);
        }

        private string GenerateFunctionBody<TCommandTree>(
            AssociationSetModificationFunctionMapping modificationFunctionMapping,
            Func<ModificationCommandTreeGenerator, string, IEnumerable<TCommandTree>> treeGenerator,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator,
            string rowsAffectedParameterName)
            where TCommandTree : DbModificationCommandTree
        {
            DebugCheck.NotNull(modificationFunctionMapping);
            DebugCheck.NotNull(treeGenerator);

            var commandTrees = new TCommandTree[0];

            if (modificationCommandTreeGenerator != null)
            {
                var dynamicToFunctionModificationCommandConverter
                    = new DynamicToFunctionModificationCommandConverter(
                        modificationFunctionMapping,
                        _target.EntityContainerMapping);

                commandTrees
                    = dynamicToFunctionModificationCommandConverter
                        .Convert(
                            treeGenerator(
                                modificationCommandTreeGenerator.Value,
                                modificationFunctionMapping.AssociationSet.ElementType.Identity))
                        .ToArray();
            }

            return GenerateFunctionBody(migrationSqlGenerator, rowsAffectedParameterName, commandTrees);
        }

        private string GenerateFunctionBody<TCommandTree>(
            MigrationSqlGenerator migrationSqlGenerator,
            string rowsAffectedParameterName,
            TCommandTree[] commandTrees)
            where TCommandTree : DbModificationCommandTree
        {
            if (migrationSqlGenerator == null)
            {
                return null;
            }

            var providerManifestToken = _target.ProviderInfo.ProviderManifestToken;

            return migrationSqlGenerator
                .GenerateProcedureBody(commandTrees, rowsAffectedParameterName, providerManifestToken);
        }

        private bool DiffModificationFunction(
            ModificationFunctionMapping functionMapping1,
            ModificationFunctionMapping functionMapping2)
        {
            DebugCheck.NotNull(functionMapping1);
            DebugCheck.NotNull(functionMapping2);

            if (!functionMapping1.RowsAffectedParameterName.EqualsOrdinal(functionMapping2.RowsAffectedParameterName))
            {
                return false;
            }

            if (!functionMapping1.ParameterBindings
                .SequenceEqual(
                    functionMapping2.ParameterBindings,
                    DiffParameterBinding))
            {
                return false;
            }

            var nullResultBindings
                = Enumerable.Empty<ModificationFunctionResultBinding>();

            if (!(functionMapping1.ResultBindings ?? nullResultBindings)
                .SequenceEqual(
                    (functionMapping2.ResultBindings ?? nullResultBindings),
                    DiffResultBinding))
            {
                return false;
            }

            return true;
        }

        private bool DiffParameterBinding(
            ModificationFunctionParameterBinding parameterBinding1,
            ModificationFunctionParameterBinding parameterBinding2)
        {
            DebugCheck.NotNull(parameterBinding1);
            DebugCheck.NotNull(parameterBinding2);

            var parameter1 = parameterBinding1.Parameter;
            var parameter2 = parameterBinding2.Parameter;
            
            if (!parameter1.Name.EqualsOrdinal(parameter2.Name))
            {
                return false;
            }

            if (parameter1.Mode != parameter2.Mode)
            {
                return false;
            }

            if (parameterBinding1.IsCurrent != parameterBinding2.IsCurrent)
            {
                return false;
            }

            if (!parameterBinding1.MemberPath.Members
                .SequenceEqual(
                    parameterBinding2.MemberPath.Members,
                    (m1, m2) => m1.Identity.EqualsOrdinal(m2.Identity)))
            {
                return false;
            }
            
            if (_source.ProviderInfo.Equals(_target.ProviderInfo))
            {
                return parameter1.TypeName.EqualsIgnoreCase(parameter2.TypeName)
                       && parameter1.TypeUsage.EdmEquals(parameter2.TypeUsage);
            }

            // Different providers, do what we can
            return parameter1.Precision == parameter2.Precision
                   && parameter1.Scale == parameter2.Scale;
        }

        private static bool DiffResultBinding(
            ModificationFunctionResultBinding resultBinding1,
            ModificationFunctionResultBinding resultBinding2)
        {
            DebugCheck.NotNull(resultBinding1);
            DebugCheck.NotNull(resultBinding2);

            if (!resultBinding1.ColumnName.EqualsOrdinal(resultBinding2.ColumnName))
            {
                return false;
            }

            if (!resultBinding1.Property.Identity.EqualsOrdinal(resultBinding2.Property.Identity))
            {
                return false;
            }

            return true;
        }

        private IEnumerable<CreateProcedureOperation> BuildCreateProcedureOperations(
            EntityTypeModificationFunctionMapping modificationFunctionMapping,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            yield return BuildCreateProcedureOperation(
                modificationFunctionMapping.InsertFunctionMapping.Function,
                GenerateInsertFunctionBody(modificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator));

            yield return BuildCreateProcedureOperation(
                modificationFunctionMapping.UpdateFunctionMapping.Function,
                GenerateUpdateFunctionBody(modificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator));

            yield return BuildCreateProcedureOperation(
                modificationFunctionMapping.DeleteFunctionMapping.Function,
                GenerateDeleteFunctionBody(modificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator));
        }

        private IEnumerable<CreateProcedureOperation> BuildCreateProcedureOperations(
            AssociationSetModificationFunctionMapping modificationFunctionMapping,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            yield return BuildCreateProcedureOperation(
                modificationFunctionMapping.InsertFunctionMapping.Function,
                GenerateInsertFunctionBody(modificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator));

            yield return BuildCreateProcedureOperation(
                modificationFunctionMapping.DeleteFunctionMapping.Function,
                GenerateDeleteFunctionBody(modificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator));
        }

        private CreateProcedureOperation BuildCreateProcedureOperation(EdmFunction function, string bodySql)
        {
            DebugCheck.NotNull(function);

            var createProcedureOperation
                = new CreateProcedureOperation(GetSchemaQualifiedName(function), bodySql);

            function
                .Parameters
                .Each(p => createProcedureOperation.Parameters.Add(BuildParameterModel(p, _target)));

            return createProcedureOperation;
        }

        private AlterProcedureOperation BuildAlterProcedureOperation(EdmFunction function, string bodySql)
        {
            DebugCheck.NotNull(function);

            var alterProcedureOperation
                = new AlterProcedureOperation(GetSchemaQualifiedName(function), bodySql);

            function
                .Parameters
                .Each(p => alterProcedureOperation.Parameters.Add(BuildParameterModel(p, _target)));

            return alterProcedureOperation;
        }

        private static ParameterModel BuildParameterModel(
            FunctionParameter functionParameter,
            ModelMetadata modelMetadata)
        {
            DebugCheck.NotNull(functionParameter);
            DebugCheck.NotNull(modelMetadata);

            var edmTypeUsage
                = functionParameter.TypeUsage.ModelTypeUsage;

            var defaultStoreTypeName
                = modelMetadata.ProviderManifest.GetStoreType(edmTypeUsage).EdmType.Name;

            var parameterModel
                = new ParameterModel(((PrimitiveType)edmTypeUsage.EdmType).PrimitiveTypeKind, edmTypeUsage)
                {
                    Name = functionParameter.Name,
                    IsOutParameter = functionParameter.Mode == ParameterMode.Out,
                    StoreType
                        = !functionParameter.TypeName.EqualsIgnoreCase(defaultStoreTypeName)
                            ? functionParameter.TypeName
                            : null
                };

            Facet facet;

            if (edmTypeUsage.Facets.TryGetValue(DbProviderManifest.MaxLengthFacetName, true, out facet)
                && facet.Value != null)
            {
                parameterModel.MaxLength = facet.Value as int?; // could be MAX sentinel
            }

            if (edmTypeUsage.Facets.TryGetValue(DbProviderManifest.PrecisionFacetName, true, out facet)
                && facet.Value != null)
            {
                parameterModel.Precision = (byte?)facet.Value;
            }

            if (edmTypeUsage.Facets.TryGetValue(DbProviderManifest.ScaleFacetName, true, out facet)
                && facet.Value != null)
            {
                parameterModel.Scale = (byte?)facet.Value;
            }

            if (edmTypeUsage.Facets.TryGetValue(DbProviderManifest.FixedLengthFacetName, true, out facet)
                && facet.Value != null
                && (bool)facet.Value)
            {
                parameterModel.IsFixedLength = true;
            }

            if (edmTypeUsage.Facets.TryGetValue(DbProviderManifest.UnicodeFacetName, true, out facet)
                && facet.Value != null
                && !(bool)facet.Value)
            {
                parameterModel.IsUnicode = false;
            }

            return parameterModel;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private IEnumerable<DropProcedureOperation> FindDroppedModificationFunctions()
        {
            return
                (from esm1 in _source.EntityContainerMapping.EntitySetMappings
                 from mfm1 in esm1.ModificationFunctionMappings
                 where !(from esm2 in _target.EntityContainerMapping.EntitySetMappings
                         from mfm2 in esm2.ModificationFunctionMappings
                         where mfm1.EntityType.Identity == mfm2.EntityType.Identity
                         select mfm2
                     ).Any()
                 from o in new[]
                 {
                     new DropProcedureOperation(
                         GetSchemaQualifiedName(mfm1.InsertFunctionMapping.Function)),
                     new DropProcedureOperation(
                         GetSchemaQualifiedName(mfm1.UpdateFunctionMapping.Function)),
                     new DropProcedureOperation(
                         GetSchemaQualifiedName(mfm1.DeleteFunctionMapping.Function))
                 }
                 select o)
                    .Concat(
                        from asm1 in _source.EntityContainerMapping.AssociationSetMappings
                        where asm1.ModificationFunctionMapping != null
                        where !(from asm2 in _target.EntityContainerMapping.AssociationSetMappings
                                where asm2.ModificationFunctionMapping != null
                                      && asm1.ModificationFunctionMapping.AssociationSet.Identity
                                      == asm2.ModificationFunctionMapping.AssociationSet.Identity
                                select asm2.ModificationFunctionMapping
                            ).Any()
                        from o in new[]
                        {
                            new DropProcedureOperation(
                                GetSchemaQualifiedName(asm1.ModificationFunctionMapping.InsertFunctionMapping.Function)),
                            new DropProcedureOperation(
                                GetSchemaQualifiedName(asm1.ModificationFunctionMapping.DeleteFunctionMapping.Function))
                        }
                        select o);
        }

        private static IEnumerable<Tuple<string, EntitySet>> FindRenamedTablePairs(ICollection<Tuple<EntitySet, EntitySet>> tablePairs)
        {
            DebugCheck.NotNull(tablePairs);

            return tablePairs
                .Where(p => !p.Item1.Table.EqualsIgnoreCase(p.Item2.Table))
                .Select(p => Tuple.Create(GetSchemaQualifiedName(p.Item1), p.Item2));
        }

        private static IEnumerable<RenameTableOperation> FindRenamedTables(ICollection<Tuple<string, EntitySet>> renamedTablePairs)
        {
            DebugCheck.NotNull(renamedTablePairs);

            return renamedTablePairs.Select(p => new RenameTableOperation(p.Item1, p.Item2.Table));
        }

        private IEnumerable<CreateTableOperation> FindAddedTables(ICollection<Tuple<EntitySet, EntitySet>> tablePairs)
        {
            DebugCheck.NotNull(tablePairs);

            return (_target.StoreEntityContainer.EntitySets
                .Except(tablePairs.Select(p => p.Item2))
                .Select(es => BuildCreateTableOperation(es, _target)));
        }

        private IEnumerable<MoveTableOperation> FindMovedTables(ICollection<Tuple<EntitySet, EntitySet>> tablePairs)
        {
            DebugCheck.NotNull(tablePairs);

            return (from p in tablePairs
                    where !p.Item1.Schema.EqualsIgnoreCase(p.Item2.Schema)
                    select
                        new MoveTableOperation(
                            new DatabaseName(p.Item2.Table, p.Item1.Schema).ToString(),
                            p.Item2.Schema)
                        {
                            CreateTableOperation = BuildCreateTableOperation(p.Item2, _target)
                        });
        }

        private IEnumerable<DropTableOperation> FindDroppedTables(ICollection<Tuple<EntitySet, EntitySet>> tablePairs)
        {
            DebugCheck.NotNull(tablePairs);

            return
                (_source.StoreEntityContainer.EntitySets
                    .Except(tablePairs.Select(p => p.Item1))
                    .Select(
                        es => new DropTableOperation(
                            GetSchemaQualifiedName(es),
                            GetAnnotations(es.ElementType),
                            es.ElementType.Properties.Where(p => GetAnnotations(p).Count > 0)
                                .ToDictionary(p => p.Name, p => (IDictionary<string, object>)GetAnnotations(p)),
                            BuildCreateTableOperation(es, _source))));
        }

        private IEnumerable<AlterTableOperation> FindAlteredTables(ICollection<Tuple<EntitySet, EntitySet>> tablePairs)
        {
            DebugCheck.NotNull(tablePairs);

            return tablePairs
                .Where(p => !GetAnnotations(p.Item1.ElementType).SequenceEqual(GetAnnotations(p.Item2.ElementType)))
                .Select(p => BuildAlterTableAnnotationsOperation(p.Item1, p.Item2));
        }

        private AlterTableOperation BuildAlterTableAnnotationsOperation(EntitySet sourceTable, EntitySet destinationTable)
        {
            var operation = new AlterTableOperation(
                GetSchemaQualifiedName(destinationTable),
                BuildAnnotationPairs(
                    GetAnnotations(sourceTable.ElementType),
                    GetAnnotations(destinationTable.ElementType)));

            destinationTable.ElementType.Properties
                .Each(
                    p =>
                        operation.Columns.Add(
                            BuildColumnModel(
                                p, _target,
                                GetAnnotations(p).ToDictionary(a => a.Key, a => new AnnotationValues(a.Value, a.Value)))));
            return operation;
        }

        internal static Dictionary<string, object> GetAnnotations(MetadataItem item)
        {
            // The intention is to return annotations that will be serialized to SSDL and which are
            // not handled natively by the differ.
            return item.Annotations.Where(
                a => a.Name.StartsWith(XmlConstants.CustomAnnotationPrefix, StringComparison.Ordinal)
                     && !a.Name.EndsWith(IndexAnnotation.AnnotationName, StringComparison.Ordinal))
                .ToDictionary(a => a.Name.Substring(XmlConstants.CustomAnnotationPrefix.Length), a => a.Value);
        }

        private IEnumerable<MigrationOperation> FindAlteredPrimaryKeys(
            ICollection<Tuple<EntitySet, EntitySet>> tablePairs,
            ICollection<RenameColumnOperation> renamedColumns, 
            ICollection<AlterColumnOperation> alteredColumns)
        {
            DebugCheck.NotNull(tablePairs);
            DebugCheck.NotNull(renamedColumns);
            DebugCheck.NotNull(alteredColumns);

            return
                from ts in tablePairs
                let t2 = GetSchemaQualifiedName(ts.Item2)
                where !ts.Item1.ElementType.KeyProperties.SequenceEqual(
                    ts.Item2.ElementType.KeyProperties,
                    (p1, p2) => p1.Name.EqualsIgnoreCase(p2.Name)
                                || renamedColumns.Any(
                                    rc => rc.Table.EqualsIgnoreCase(t2)
                                          && rc.Name.EqualsIgnoreCase(p1.Name)
                                          && rc.NewName.EqualsIgnoreCase(p2.Name)))
                      || ts.Item2.ElementType.KeyProperties
                          .Any(
                              p => alteredColumns.Any(
                                  ac => ac.Table.EqualsIgnoreCase(t2)
                                        && ac.Column.Name.EqualsIgnoreCase(p.Name)))
                from o in BuildChangePrimaryKeyOperations(ts)
                select o;
        }

        private IEnumerable<MigrationOperation> BuildChangePrimaryKeyOperations(Tuple<EntitySet, EntitySet> tablePair)
        {
            DebugCheck.NotNull(tablePair);

            var referencedForeignKeys
                = _target.StoreItemCollection.GetItems<AssociationType>()
                    .Select(at => at.Constraint)
                    .Where(c => c.FromProperties.SequenceEqual(tablePair.Item2.ElementType.KeyProperties))
                    .ToList();

            foreach (var constraint in referencedForeignKeys)
            {
                yield return BuildDropForeignKeyOperation(constraint, _target);
            }

            var dropPrimaryKeyOperation
                = new DropPrimaryKeyOperation
                {
                    Table = GetSchemaQualifiedName(tablePair.Item1)
                };

            tablePair.Item1.ElementType.KeyProperties
                .Each(pr => dropPrimaryKeyOperation.Columns.Add(pr.Name));

            yield return dropPrimaryKeyOperation;

            var addPrimaryKeyOperation
                = new AddPrimaryKeyOperation
                {
                    Table = GetSchemaQualifiedName(tablePair.Item2)
                };

            tablePair.Item2.ElementType.KeyProperties
                .Each(pr => addPrimaryKeyOperation.Columns.Add(pr.Name));

            yield return addPrimaryKeyOperation;

            foreach (var constraint in referencedForeignKeys)
            {
                yield return BuildAddForeignKeyOperation(constraint, _target);
            }
        }

        private IEnumerable<AddForeignKeyOperation> FindAddedForeignKeys(
            ICollection<Tuple<AssociationType, AssociationType>> assocationTypePairs,
            ICollection<RenameColumnOperation> renamedColumns)
        {
            DebugCheck.NotNull(assocationTypePairs);
            DebugCheck.NotNull(renamedColumns);

            return _target.StoreItemCollection.GetItems<AssociationType>()
                .Except(assocationTypePairs.Select(p => p.Item2))
                .Concat(
                    assocationTypePairs
                        .Where(at => !DiffAssociations(at.Item1.Constraint, at.Item2.Constraint, renamedColumns))
                        .Select(at => at.Item2))
                .Select(at => BuildAddForeignKeyOperation(at.Constraint, _target));
        }

        private IEnumerable<DropForeignKeyOperation> FindDroppedForeignKeys(
            ICollection<Tuple<AssociationType, AssociationType>> assocationTypePairs,
            ICollection<RenameColumnOperation> renamedColumns)
        {
            DebugCheck.NotNull(assocationTypePairs);
            DebugCheck.NotNull(renamedColumns);

            return _source.StoreItemCollection.GetItems<AssociationType>()
                .Except(assocationTypePairs.Select(p => p.Item1))
                .Concat(
                    assocationTypePairs
                        .Where(at => !DiffAssociations(at.Item1.Constraint, at.Item2.Constraint, renamedColumns))
                        .Select(at => at.Item1))
                .Select(at => BuildDropForeignKeyOperation(at.Constraint, _source));
        }

        private bool DiffAssociations(
            ReferentialConstraint referentialConstraint1,
            ReferentialConstraint referentialConstraint2,
            ICollection<RenameColumnOperation> renamedColumns)
        {
            DebugCheck.NotNull(referentialConstraint1);
            DebugCheck.NotNull(referentialConstraint2);
            DebugCheck.NotNull(renamedColumns);

            var targetTable
                = GetSchemaQualifiedName(
                    _target.StoreEntityContainer.EntitySets
                        .Single(es => es.ElementType == referentialConstraint2.DependentEnd.GetEntityType()));

            return
                referentialConstraint1.ToProperties
                    .SequenceEqual(
                        referentialConstraint2.ToProperties,
                        (p1, p2) => p1.Name.EqualsIgnoreCase(p2.Name)
                                    || renamedColumns.Any(
                                        rc => rc.Table.EqualsIgnoreCase(targetTable)
                                              && rc.Name.EqualsIgnoreCase(p1.Name)
                                              && rc.NewName.EqualsIgnoreCase(p2.Name)))
                && referentialConstraint1.PrincipalEnd.DeleteBehavior == referentialConstraint2.PrincipalEnd.DeleteBehavior;
        }

        private static AddForeignKeyOperation BuildAddForeignKeyOperation(
            ReferentialConstraint referentialConstraint,
            ModelMetadata modelMetadata)
        {
            DebugCheck.NotNull(referentialConstraint);
            DebugCheck.NotNull(modelMetadata);

            var addForeignKeyOperation = new AddForeignKeyOperation();

            BuildForeignKeyOperation(referentialConstraint, addForeignKeyOperation, modelMetadata);

            referentialConstraint.FromProperties
                .Each(pr => addForeignKeyOperation.PrincipalColumns.Add(pr.Name));

            addForeignKeyOperation.CascadeDelete
                = referentialConstraint.PrincipalEnd.DeleteBehavior == OperationAction.Cascade;

            return addForeignKeyOperation;
        }

        private static DropForeignKeyOperation BuildDropForeignKeyOperation(
            ReferentialConstraint referentialConstraint,
            ModelMetadata modelMetadata)
        {
            DebugCheck.NotNull(referentialConstraint);
            DebugCheck.NotNull(modelMetadata);

            var dropForeignKeyOperation
                = new DropForeignKeyOperation(BuildAddForeignKeyOperation(referentialConstraint, modelMetadata));

            BuildForeignKeyOperation(referentialConstraint, dropForeignKeyOperation, modelMetadata);

            return dropForeignKeyOperation;
        }
        
        private static void BuildForeignKeyOperation(
            ReferentialConstraint referentialConstraint, 
            ForeignKeyOperation foreignKeyOperation, 
            ModelMetadata modelMetadata)
        {
            DebugCheck.NotNull(referentialConstraint);
            DebugCheck.NotNull(foreignKeyOperation);
            DebugCheck.NotNull(modelMetadata);

            foreignKeyOperation.PrincipalTable 
                = GetSchemaQualifiedName( 
                    modelMetadata.StoreEntityContainer.EntitySets
                    .Single(es => es.ElementType == referentialConstraint.PrincipalEnd.GetEntityType()));

            foreignKeyOperation.DependentTable
                = GetSchemaQualifiedName(
                    modelMetadata.StoreEntityContainer.EntitySets
                    .Single(es => es.ElementType == referentialConstraint.DependentEnd.GetEntityType()));

            referentialConstraint.ToProperties
                .Each(pr => foreignKeyOperation.DependentColumns.Add(pr.Name));
        }

        private IEnumerable<AddColumnOperation> FindAddedColumns(
            ICollection<Tuple<EntitySet, EntitySet>> tablePairs,
            ICollection<RenameColumnOperation> renamedColumns)
        {
            DebugCheck.NotNull(tablePairs);
            DebugCheck.NotNull(renamedColumns);

            return
                from p in tablePairs
                let t = GetSchemaQualifiedName(p.Item2)
                from c in p.Item2.ElementType.Properties
                    .Except(
                        p.Item1.ElementType.Properties,
                        (c1, c2) => c1.Name.EqualsIgnoreCase(c2.Name))
                where !renamedColumns
                    .Any(
                        cr => cr.Table.EqualsIgnoreCase(t)
                              && cr.NewName.EqualsIgnoreCase(c.Name))
                select new AddColumnOperation(
                    t,
                    BuildColumnModel(
                        c, _target, GetAnnotations(c).ToDictionary(a => a.Key, a => new AnnotationValues(null, a.Value))));
        }

        private IEnumerable<DropColumnOperation> FindDroppedColumns(
            ICollection<Tuple<EntitySet, EntitySet>> tablePairs,
            ICollection<RenameColumnOperation> renamedColumns)
        {
            DebugCheck.NotNull(tablePairs);
            DebugCheck.NotNull(renamedColumns);

            return
                from p in tablePairs
                let t = GetSchemaQualifiedName(p.Item2)
                from c in p.Item1.ElementType.Properties
                    .Except(
                        p.Item2.ElementType.Properties,
                        (c1, c2) => c1.Name.EqualsIgnoreCase(c2.Name))
                where !renamedColumns
                    .Any(
                        rc => rc.Table.EqualsIgnoreCase(t)
                              && rc.Name.EqualsIgnoreCase(c.Name))
                select new DropColumnOperation(
                    t,
                    c.Name,
                    GetAnnotations(c),
                    new AddColumnOperation(
                        t,
                        BuildColumnModel(
                            c, _source, GetAnnotations(c).ToDictionary(a => a.Key, a => new AnnotationValues(null, a.Value)))));
        }

        private IEnumerable<DropColumnOperation> FindOrphanedColumns(
            ICollection<Tuple<EntitySet, EntitySet>> tablePairs,
            ICollection<RenameColumnOperation> renamedColumns)
        {
            DebugCheck.NotNull(tablePairs);
            DebugCheck.NotNull(renamedColumns);

            return
                from p in tablePairs
                let t = GetSchemaQualifiedName(p.Item2)
                from rc1 in renamedColumns
                where rc1.Table.EqualsIgnoreCase(t)
                from c in p.Item1.ElementType.Properties
                where c.Name.EqualsIgnoreCase(rc1.NewName)
                      && !renamedColumns.Any(
                          // Ensure the candidate column is not also being renamed
                          rc2 =>
                              rc2 != rc1
                              && rc2.Table.EqualsIgnoreCase(rc1.Table)
                              && rc2.Name.EqualsIgnoreCase(rc1.NewName))
                select new DropColumnOperation(
                    t,
                    c.Name,
                    GetAnnotations(c),
                    new AddColumnOperation(
                        t,
                        BuildColumnModel(
                            c, _source, GetAnnotations(c).ToDictionary(a => a.Key, a => new AnnotationValues(null, a.Value)))));
        }

        private IEnumerable<AlterColumnOperation> FindAlteredColumns(
            ICollection<Tuple<EntitySet, EntitySet>> tablePairs,
            ICollection<RenameColumnOperation> renamedColumns)
        {
            DebugCheck.NotNull(tablePairs);
            DebugCheck.NotNull(renamedColumns);

            return
                from p in tablePairs
                let t = GetSchemaQualifiedName(p.Item2)
                from p1 in p.Item1.ElementType.Properties
                let p2 = p.Item2.ElementType.Properties
                    .SingleOrDefault(
                        c => (p1.Name.EqualsIgnoreCase(c.Name)
                              || renamedColumns.Any(
                                  rc => rc.Table.EqualsIgnoreCase(t)
                                        && rc.Name.EqualsIgnoreCase(p1.Name)
                                        && rc.NewName.EqualsIgnoreCase(c.Name)))
                             && !DiffColumns(p1, c))
                where p2 != null
                select BuildAlterColumnOperation(t, p2, _target, p1, _source);
        }

        private IEnumerable<ConsolidatedIndex> FindSourceIndexes(
            ICollection<Tuple<string, EntitySet>> renameTablePairs,
            ICollection<RenameColumnOperation> renamedColumns)
        {
            DebugCheck.NotNull(renameTablePairs);
            DebugCheck.NotNull(renamedColumns);

            return
                from es in _source.StoreEntityContainer.EntitySets
                let ot = GetSchemaQualifiedName(es)
                let t = renameTablePairs.Where(rt => rt.Item1.EqualsIgnoreCase(ot))
                    .Select(rt => GetSchemaQualifiedName(rt.Item2))
                    .SingleOrDefault() ?? ot
                from i in ConsolidatedIndex.BuildIndexes(
                    t, es.ElementType.Properties.Select(
                        c => Tuple.Create(
                            renamedColumns.Where(
                                rc => rc.Table.EqualsIgnoreCase(t)
                                      && rc.Name.EqualsIgnoreCase(c.Name))
                                .Select(rc => rc.NewName)
                                .SingleOrDefault() ?? c.Name, c)))
                select i;
        }

        private IEnumerable<ConsolidatedIndex> FindTargetIndexes()
        {
            return
                from es in _target.StoreEntityContainer.EntitySets
                from i in ConsolidatedIndex.BuildIndexes(
                    GetSchemaQualifiedName(es), es.ElementType.Properties.Select(p => Tuple.Create(p.Name, p)))
                select i;
        }

        private static IEnumerable<CreateIndexOperation> FindAddedIndexes(
            ICollection<ConsolidatedIndex> sourceIndexes,
            ICollection<ConsolidatedIndex> targetIndexes)
        {
            DebugCheck.NotNull(sourceIndexes);
            DebugCheck.NotNull(targetIndexes);

            return targetIndexes.Except(sourceIndexes).Select(i => i.CreateCreateIndexOperation());
        }

        private static IEnumerable<DropIndexOperation> FindDroppedIndexes(
            ICollection<ConsolidatedIndex> sourceIndexes,
            ICollection<ConsolidatedIndex> targetIndexes)
        {
            DebugCheck.NotNull(sourceIndexes);
            DebugCheck.NotNull(targetIndexes);

            return sourceIndexes.Except(targetIndexes).Select(i => i.CreateDropIndexOperation());
        }

        private bool DiffColumns(EdmProperty column1, EdmProperty column2)
        {
            DebugCheck.NotNull(column1);
            DebugCheck.NotNull(column2);

            if (column1.Nullable != column2.Nullable)
            {
                return false;
            }

            if (column1.PrimitiveType.PrimitiveTypeKind != column2.PrimitiveType.PrimitiveTypeKind)
            {
                return false;
            }

            if (column1.StoreGeneratedPattern != column2.StoreGeneratedPattern)
            {
                return false;
            }

            if (!GetAnnotations(column1).OrderBy(a => a.Key)
                .SequenceEqual(GetAnnotations(column2).OrderBy(a => a.Key)))
            {
                return false;
            }

            if (_source.ProviderInfo.Equals(_target.ProviderInfo))
            {
                return column1.TypeName.EqualsIgnoreCase(column2.TypeName)
                       && column1.TypeUsage.EdmEquals(column2.TypeUsage);
            }

            // Different providers, do what we can
            return column1.Precision == column2.Precision
                   && column1.Scale == column2.Scale
                   && column1.IsUnicode == column2.IsUnicode
                   && column1.IsFixedLength == column2.IsFixedLength;
        }

        private AlterColumnOperation BuildAlterColumnOperation(
            string table,
            EdmProperty targetProperty,
            ModelMetadata targetModelMetadata,
            EdmProperty sourceProperty,
            ModelMetadata sourceModelMetadata)
        {
            DebugCheck.NotEmpty(table);
            DebugCheck.NotNull(targetProperty);
            DebugCheck.NotNull(targetModelMetadata);
            DebugCheck.NotNull(sourceProperty);
            DebugCheck.NotNull(sourceModelMetadata);

            var targetAnnotations = BuildAnnotationPairs(
                GetAnnotations(sourceProperty), GetAnnotations(targetProperty));
            
            var sourceAnnotations = targetAnnotations
                .ToDictionary(a => a.Key, a => new AnnotationValues(a.Value.NewValue, a.Value.OldValue));

            var targetModel
                = BuildColumnModel(targetProperty, targetModelMetadata, targetAnnotations);

            var sourceModel
                = BuildColumnModel(sourceProperty, sourceModelMetadata, sourceAnnotations);

            // In-case the column is also being renamed.
            sourceModel.Name = targetModel.Name;

            return new AlterColumnOperation(
                table,
                targetModel,
                isDestructiveChange: targetModel.IsNarrowerThan(sourceModel, _target.ProviderManifest),
                inverse: new AlterColumnOperation(
                    table,
                    sourceModel,
                    isDestructiveChange: sourceModel.IsNarrowerThan(targetModel, _target.ProviderManifest)));
        }

        private static IDictionary<string, AnnotationValues> BuildAnnotationPairs(
            IDictionary<string, object> rawSourceAnnotations,
            IDictionary<string, object> rawTargetAnnotations)
        {
            var pairs = new Dictionary<string, AnnotationValues>();

            var allKeys = rawTargetAnnotations.Keys.Concat(rawSourceAnnotations.Keys).Distinct();
            foreach (var key in allKeys)
            {
                if (!rawSourceAnnotations.ContainsKey(key))
                {
                    pairs[key] = new AnnotationValues(null, rawTargetAnnotations[key]);
                }
                else if (!rawTargetAnnotations.ContainsKey(key))
                {
                    pairs[key] = new AnnotationValues(rawSourceAnnotations[key], null);
                }
                else if (!Equals(rawSourceAnnotations[key], rawTargetAnnotations[key]))
                {
                    pairs[key] = new AnnotationValues(rawSourceAnnotations[key], rawTargetAnnotations[key]);
                }
            }

            return pairs;
        }

        private IEnumerable<RenameColumnOperation> FindRenamedColumns(
            ICollection<Tuple<MappingFragment, MappingFragment>> mappingFragmentPairs,
            ICollection<Tuple<AssociationType, AssociationType>> associationTypePairs)
        {
            DebugCheck.NotNull(mappingFragmentPairs);
            DebugCheck.NotNull(associationTypePairs);

            return
                FindRenamedMappedColumns(mappingFragmentPairs)
                    .Concat(FindRenamedForeignKeyColumns(associationTypePairs))
                    .Concat(FindRenamedDiscriminatorColumns(mappingFragmentPairs))
                    .Distinct(
                        new DynamicEqualityComparer<RenameColumnOperation>(
                            (c1, c2) => c1.Table.EqualsIgnoreCase(c2.Table)
                                        && c1.Name.EqualsIgnoreCase(c2.Name)
                                        && c1.NewName.EqualsIgnoreCase(c2.NewName)));
        }

        private static IEnumerable<RenameColumnOperation> FindRenamedMappedColumns(
            ICollection<Tuple<MappingFragment, MappingFragment>> mappingFragmentPairs)
        {
            DebugCheck.NotNull(mappingFragmentPairs);

            return from mfs in mappingFragmentPairs
                   let t = GetSchemaQualifiedName(mfs.Item2.StoreEntitySet)
                   from cr in FindRenamedMappedColumns(mfs.Item1, mfs.Item2, t)
                   select cr;
        }

        private static IEnumerable<RenameColumnOperation> FindRenamedMappedColumns(
            MappingFragment mappingFragment1, MappingFragment mappingFragment2, string table)
        {
            DebugCheck.NotNull(mappingFragment1);
            DebugCheck.NotNull(mappingFragment2);
            DebugCheck.NotEmpty(table);

            return (from cmb1 in mappingFragment1.FlattenedProperties
                    from cmb2 in mappingFragment2.FlattenedProperties
                    where cmb1.PropertyPath.SequenceEqual(
                        cmb2.PropertyPath,
                        new DynamicEqualityComparer<EdmProperty>((p1, p2) => p1.EdmEquals(p2)))
                          && !cmb1.ColumnProperty.Name.EqualsIgnoreCase(cmb2.ColumnProperty.Name)
                    select new RenameColumnOperation(table, cmb1.ColumnProperty.Name, cmb2.ColumnProperty.Name));
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private IEnumerable<RenameColumnOperation> FindRenamedForeignKeyColumns(
            ICollection<Tuple<AssociationType, AssociationType>> associationTypePairs)
        {
            DebugCheck.NotNull(associationTypePairs);

            return
                from ats in associationTypePairs
                let rc1 = ats.Item1.Constraint
                let rc2 = ats.Item2.Constraint
                from ps in rc1.ToProperties.Zip(rc2.ToProperties)
                where !ps.Key.Name.EqualsIgnoreCase(ps.Value.Name)
                        && (!rc2.DependentEnd.GetEntityType().Properties
                                .Any(p => p.Name.EqualsIgnoreCase(ps.Key.Name))
                            || rc1.DependentEnd.GetEntityType().Properties
                                    .Any(p => p.Name.EqualsIgnoreCase(ps.Value.Name)))
                select new RenameColumnOperation(
                    GetSchemaQualifiedName(
                        _target.StoreEntityContainer.EntitySets
                            .Single(es => es.ElementType == rc2.DependentEnd.GetEntityType())),
                    ps.Key.Name,
                    ps.Value.Name);
        }

        private static IEnumerable<RenameColumnOperation> FindRenamedDiscriminatorColumns(
            ICollection<Tuple<MappingFragment, MappingFragment>> mappingFragmentPairs)
        {
            DebugCheck.NotNull(mappingFragmentPairs);

            return from mfs in mappingFragmentPairs
                   let t = GetSchemaQualifiedName(mfs.Item2.StoreEntitySet)
                   from cr in FindRenamedDiscriminatorColumns(mfs.Item1, mfs.Item2, t)
                   select cr;
        }

        private static IEnumerable<RenameColumnOperation> FindRenamedDiscriminatorColumns(
            MappingFragment mappingFragment1, MappingFragment mappingFragment2, string table)
        {
            DebugCheck.NotNull(mappingFragment1);
            DebugCheck.NotNull(mappingFragment2);
            DebugCheck.NotEmpty(table);

            return from c1 in mappingFragment1.Conditions
                   from c2 in mappingFragment2.Conditions
                   where c1.Value.Equals(c2.Value)
                   where !c1.Column.Name.EqualsIgnoreCase(c2.Column.Name)
                   select new RenameColumnOperation(table, c1.Column.Name, c2.Column.Name);
        }

        private static CreateTableOperation BuildCreateTableOperation(EntitySet entitySet, ModelMetadata modelMetadata)
        {
            DebugCheck.NotNull(entitySet);
            DebugCheck.NotNull(modelMetadata);

            var createTableOperation
                = new CreateTableOperation(GetSchemaQualifiedName(entitySet), GetAnnotations(entitySet.ElementType));

            entitySet.ElementType.Properties
                .Each(
                    p =>
                        createTableOperation.Columns.Add(
                            BuildColumnModel(
                                p, modelMetadata,
                                GetAnnotations(p).ToDictionary(a => a.Key, a => new AnnotationValues(null, a.Value)))));

            var addPrimaryKeyOperation = new AddPrimaryKeyOperation();

            entitySet.ElementType.KeyProperties
                .Each(p => addPrimaryKeyOperation.Columns.Add(p.Name));

            createTableOperation.PrimaryKey = addPrimaryKeyOperation;

            return createTableOperation;
        }

        private static ColumnModel BuildColumnModel(
            EdmProperty property, ModelMetadata modelMetadata, IDictionary<string, AnnotationValues> annotations)
        {
            DebugCheck.NotNull(property);
            DebugCheck.NotNull(modelMetadata);

            var conceptualTypeUsage = modelMetadata.ProviderManifest.GetEdmType(property.TypeUsage);
            var defaultStoreTypeUsage = modelMetadata.ProviderManifest.GetStoreType(conceptualTypeUsage);

            return BuildColumnModel(property, conceptualTypeUsage, defaultStoreTypeUsage, annotations);
        }

        public static ColumnModel BuildColumnModel(
            EdmProperty property,
            TypeUsage conceptualTypeUsage,
            TypeUsage defaultStoreTypeUsage,
            IDictionary<string, AnnotationValues> annotations)
        {
            DebugCheck.NotNull(property);
            DebugCheck.NotNull(conceptualTypeUsage);
            DebugCheck.NotNull(defaultStoreTypeUsage);

            var column = new ColumnModel(property.PrimitiveType.PrimitiveTypeKind, conceptualTypeUsage)
            {
                Name
                    = property.Name,
                IsNullable
                    = !property.Nullable ? false : (bool?)null,
                StoreType
                    = !property.TypeName.EqualsIgnoreCase(defaultStoreTypeUsage.EdmType.Name)
                        ? property.TypeName
                        : null,
                IsIdentity
                    = property.IsStoreGeneratedIdentity
                      && _validIdentityTypes.Contains(property.PrimitiveType.PrimitiveTypeKind),
                IsTimestamp
                    = property.PrimitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Binary
                      && property.MaxLength == 8
                      && property.IsStoreGeneratedComputed,
                IsUnicode
                    = property.IsUnicode == false ? false : (bool?)null,
                IsFixedLength
                    = property.IsFixedLength == true ? true : (bool?)null,
                Annotations
                    = annotations
            };

            Facet facet;

            if (property.TypeUsage.Facets.TryGetValue(DbProviderManifest.MaxLengthFacetName, true, out facet)
                && !facet.IsUnbounded
                && !facet.Description.IsConstant)
            {
                column.MaxLength = (int?)facet.Value;
            }

            if (property.TypeUsage.Facets.TryGetValue(DbProviderManifest.PrecisionFacetName, true, out facet)
                && !facet.IsUnbounded
                && !facet.Description.IsConstant)
            {
                column.Precision = (byte?)facet.Value;
            }

            if (property.TypeUsage.Facets.TryGetValue(DbProviderManifest.ScaleFacetName, true, out facet)
                && !facet.IsUnbounded
                && !facet.Description.IsConstant)
            {
                column.Scale = (byte?)facet.Value;
            }

            return column;
        }

        private static DbProviderManifest GetProviderManifest(DbProviderInfo providerInfo)
        {
            DebugCheck.NotNull(providerInfo);

            var providerFactory = DbConfiguration.DependencyResolver.GetService<DbProviderFactory>(providerInfo.ProviderInvariantName);

            return providerFactory.GetProviderServices().GetProviderManifest(providerInfo.ProviderManifestToken);
        }

        private static string GetSchemaQualifiedName(EntitySet entitySet)
        {
            DebugCheck.NotNull(entitySet);

            return new DatabaseName(entitySet.Table, entitySet.Schema).ToString();
        }

        private static string GetSchemaQualifiedName(EdmFunction function)
        {
            DebugCheck.NotNull(function);

            return new DatabaseName(function.FunctionName, function.Schema).ToString();
        }
    }
}
