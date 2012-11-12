// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;

    internal class TablePrimitiveOperations
    {
        public static void AddColumn(EntityType table, EdmProperty column)
        {
            Contract.Requires(table != null);
            Contract.Requires(column != null);

            if (!table.Properties.Contains(column))
            {
                var configuration = column.GetConfiguration() as PrimitivePropertyConfiguration;

                if ((configuration == null)
                    || string.IsNullOrWhiteSpace(configuration.ColumnName))
                {
                    var preferredName = column.GetPreferredName() ?? column.Name;
                    column.SetUnpreferredUniqueName(column.Name);
                    column.Name = table.Properties.UniquifyName(preferredName);
                }

                table.AddMember(column);
            }
        }

        public static EdmProperty RemoveColumn(EntityType table, EdmProperty column)
        {
            Contract.Requires(table != null);
            Contract.Requires(column != null);

            if (!column.IsPrimaryKeyColumn)
            {
                table.RemoveMember(column);
            }

            return column;
        }

        public static EdmProperty IncludeColumn(
            EntityType table, EdmProperty templateColumn, bool useExisting)
        {
            Contract.Requires(table != null);
            Contract.Requires(templateColumn != null);

            var existingColumn =
                table.Properties.SingleOrDefault(c => string.Equals(c.Name, templateColumn.Name, StringComparison.Ordinal));

            if (existingColumn == null)
            {
                templateColumn = templateColumn.Clone();
            }
            else if (!useExisting
                     && !existingColumn.IsPrimaryKeyColumn)
            {
                templateColumn = templateColumn.Clone();
            }
            else
            {
                templateColumn = existingColumn;
            }

            AddColumn(table, templateColumn);

            return templateColumn;
        }
    }

    internal class ForeignKeyPrimitiveOperations
    {
        public static void UpdatePrincipalTables(
            DbDatabaseMapping databaseMapping,
            EntityType entityType,
            EntityType fromTable,
            EntityType toTable,
            bool isMappingAnyInheritedProperty)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(fromTable != null);
            Contract.Requires(toTable != null);

            if (fromTable != toTable)
            {
                // Update the principal tables for associations/fks defined on the exact given entity type
                // In this case they need to be moved to the appropriate table, but not removed
                UpdatePrincipalTables(databaseMapping, toTable, entityType, removeFks: false);

                if (isMappingAnyInheritedProperty)
                {
                    // if mapping inherited properties, remove FKs that have the base type as the principal
                    UpdatePrincipalTables(databaseMapping, toTable, (EntityType)entityType.BaseType, removeFks: true);
                }
            }
        }

        private static void UpdatePrincipalTables(
            DbDatabaseMapping databaseMapping, EntityType toTable, EntityType entityType, bool removeFks)
        {
            foreach (var associationType in databaseMapping.Model.Namespaces.Single().AssociationTypes
                .Where(at => at.SourceEnd.GetEntityType().Equals(entityType) || at.TargetEnd.GetEntityType().Equals(entityType)))
            {
                UpdatePrincipalTables(databaseMapping, toTable, removeFks, associationType, entityType);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static void UpdatePrincipalTables(
            DbDatabaseMapping databaseMapping, EntityType toTable, bool removeFks,
            AssociationType associationType, EntityType et)
        {
            AssociationEndMember principalEnd, dependentEnd;
            var endsToCheck = new List<AssociationEndMember>();
            if (associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd))
            {
                endsToCheck.Add(principalEnd);
            }
            else if (associationType.SourceEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many
                     && associationType.TargetEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many)
            {
                // many to many consider both ends
                endsToCheck.Add(associationType.SourceEnd);
                endsToCheck.Add(associationType.TargetEnd);
            }
            else
            {
                // 1:1 and 0..1:0..1
                endsToCheck.Add(associationType.SourceEnd);
            }

            foreach (var end in endsToCheck)
            {
                if (end.GetEntityType() == et)
                {
                    IEnumerable<KeyValuePair<EntityType, IEnumerable<EdmProperty>>> dependentTableInfos;
                    if (associationType.Constraint != null)
                    {
                        var originalDependentType = associationType.GetOtherEnd(end).GetEntityType();
                        var allDependentTypes = databaseMapping.Model.GetSelfAndAllDerivedTypes(originalDependentType);

                        dependentTableInfos =
                            allDependentTypes.Select(t => databaseMapping.GetEntityTypeMapping(t)).Where(
                                dm => dm != null)
                                .SelectMany(
                                    dm => dm.TypeMappingFragments
                                              .Where(
                                                  tmf => associationType.Constraint.ToProperties
                                                             .All(
                                                                 p =>
                                                                 tmf.PropertyMappings.Any(
                                                                     pm => pm.PropertyPath.First() == p))))
                                .Distinct((f1, f2) => f1.Table == f2.Table)
                                .Select(
                                    df =>
                                    new KeyValuePair<EntityType, IEnumerable<EdmProperty>>(
                                        df.Table,
                                        df.PropertyMappings.Where(
                                            pm =>
                                            associationType.Constraint.ToProperties.Contains(
                                                pm.PropertyPath.First())).Select(
                                                    pm => pm.Column)));
                    }
                    else
                    {
                        // IA
                        var associationSetMapping =
                            databaseMapping.EntityContainerMappings.Single().AssociationSetMappings.Where(
                                asm => asm.AssociationSet.ElementType == associationType).Single();
                        var dependentTable = associationSetMapping.Table;
                        var propertyMappings = associationSetMapping.SourceEndMapping.AssociationEnd == end
                                                   ? associationSetMapping.SourceEndMapping.PropertyMappings
                                                   : associationSetMapping.TargetEndMapping.PropertyMappings;
                        var dependentColumns = propertyMappings.Select(pm => pm.Column);

                        dependentTableInfos = new[]
                                                  {
                                                      new KeyValuePair
                                                          <EntityType, IEnumerable<EdmProperty>>(
                                                          dependentTable, dependentColumns)
                                                  };
                    }

                    foreach (var tableInfo in dependentTableInfos)
                    {
                        foreach (
                            var fk in
                                tableInfo.Key.ForeignKeyBuilders.Where(
                                    fk => fk.DependentColumns.SequenceEqual(tableInfo.Value)).ToArray(
                                        
                                    ))
                        {
                            if (removeFks)
                            {
                                tableInfo.Key.RemoveForeignKey(fk);
                            }
                            else
                            {
                                fk.PrincipalTable = toTable;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Moves a foreign key constraint from oldTable to newTable and updates column references
        /// </summary>
        private static void MoveForeignKeyConstraint(
            EntityType fromTable, EntityType toTable, ForeignKeyBuilder fk)
        {
            Contract.Requires(fromTable != null);
            Contract.Requires(toTable != null);
            Contract.Requires(fk != null);

            fromTable.RemoveForeignKey(fk);

            // Only move it to the new table if the destination is not the principal table or if all dependent columns are not FKs
            // Otherwise you end up with an FK from the PKs to the PKs of the same table
            if (fk.PrincipalTable != toTable
                || !fk.DependentColumns.All(c => c.IsPrimaryKeyColumn))
            {
                // Make sure all the dependent columns refer to columns in the newTable
                var oldColumns = fk.DependentColumns.ToArray();

                var dependentColumns
                    = GetDependentColumns(oldColumns, toTable.Properties);

                if (!ContainsEquivalentForeignKey(toTable, fk.PrincipalTable, dependentColumns))
                {
                    toTable.AddForeignKey(fk);

                    fk.DependentColumns = dependentColumns;
                }
            }
        }

        private static void CopyForeignKeyConstraint(
            EdmModel database, EntityType toTable,
            ForeignKeyBuilder fk)
        {
            Contract.Requires(toTable != null);
            Contract.Requires(fk != null);

            var newFk
                = new ForeignKeyBuilder(
                    database,
                    database.GetEntityTypes().SelectMany(t => t.ForeignKeyBuilders).UniquifyName(fk.Name))
                      {
                          PrincipalTable = fk.PrincipalTable,
                          DeleteAction = fk.DeleteAction
                      };

            var dependentColumns
                = GetDependentColumns(fk.DependentColumns, toTable.Properties);

            if (!ContainsEquivalentForeignKey(toTable, newFk.PrincipalTable, dependentColumns))
            {
                toTable.AddForeignKey(newFk);

                newFk.DependentColumns = dependentColumns;
            }
        }

        private static bool ContainsEquivalentForeignKey(
            EntityType dependentTable, EntityType principalTable, IEnumerable<EdmProperty> columns)
        {
            return dependentTable.ForeignKeyBuilders
                .Any(
                    fk => fk.PrincipalTable == principalTable
                          && fk.DependentColumns.SequenceEqual(columns));
        }

        private static IList<EdmProperty> GetDependentColumns(
            IEnumerable<EdmProperty> sourceColumns,
            IEnumerable<EdmProperty> destinationColumns)
        {
            return sourceColumns
                .Select(
                    sc =>
                    destinationColumns.Single(
                        dc =>
                        string.Equals(dc.Name, sc.Name, StringComparison.Ordinal)
                        || string.Equals(dc.GetUnpreferredUniqueName(), sc.Name, StringComparison.Ordinal))
                )
                .ToList();
        }

        private static IEnumerable<ForeignKeyBuilder> FindAllForeignKeyConstraintsForColumn(
            EntityType fromTable, EntityType toTable, EdmProperty column)
        {
            return fromTable
                .ForeignKeyBuilders
                .Where(
                    fk => fk.DependentColumns.Contains(column) &&
                          fk.DependentColumns.All(
                              c => toTable.Properties.Any(
                                  nc =>
                                  string.Equals(nc.Name, c.Name, StringComparison.Ordinal)
                                  || string.Equals(nc.GetUnpreferredUniqueName(), c.Name, StringComparison.Ordinal))));
        }

        public static void CopyAllForeignKeyConstraintsForColumn(
            EdmModel database, EntityType fromTable, EntityType toTable,
            EdmProperty column)
        {
            Contract.Requires(fromTable != null);
            Contract.Requires(toTable != null);
            Contract.Requires(column != null);

            FindAllForeignKeyConstraintsForColumn(fromTable, toTable, column)
                .ToArray()
                .Each(fk => CopyForeignKeyConstraint(database, toTable, fk));
        }

        public static void MoveAllDeclaredForeignKeyConstraintsForPrimaryKeyColumns(
            EntityType entityType, EntityType fromTable, EntityType toTable)
        {
            Contract.Requires(fromTable != null);
            Contract.Requires(toTable != null);

            foreach (var column in fromTable.DeclaredKeyProperties)
            {
                FindAllForeignKeyConstraintsForColumn(fromTable, toTable, column)
                    .ToArray()
                    .Each(
                        fk =>
                            {
                                var at = fk.GetAssociationType();
                                if (at != null && at.Constraint.DependentEnd.GetEntityType() == entityType
                                    && !fk.GetIsTypeConstraint())
                                {
                                    MoveForeignKeyConstraint(fromTable, toTable, fk);
                                }
                            });
            }
        }

        public static void CopyAllForeignKeyConstraintsForPrimaryKeyColumns(
            EdmModel database, EntityType fromTable, EntityType toTable)
        {
            Contract.Requires(fromTable != null);
            Contract.Requires(toTable != null);

            foreach (var column in fromTable.DeclaredKeyProperties)
            {
                FindAllForeignKeyConstraintsForColumn(fromTable, toTable, column)
                    .ToArray()
                    .Each(
                        fk =>
                            {
                                if (!fk.GetIsTypeConstraint())
                                {
                                    CopyForeignKeyConstraint(database, toTable, fk);
                                }
                            });
            }
        }

        /// <summary>
        ///     Move any FK constraints that are now completely in newTable and used to refer to oldColumn
        /// </summary>
        public static void MoveAllForeignKeyConstraintsForColumn(
            EntityType fromTable, EntityType toTable, EdmProperty column)
        {
            Contract.Requires(fromTable != null);
            Contract.Requires(toTable != null);
            Contract.Requires(column != null);

            FindAllForeignKeyConstraintsForColumn(fromTable, toTable, column)
                .ToArray()
                .Each(fk => { MoveForeignKeyConstraint(fromTable, toTable, fk); });
        }

        public static void RemoveAllForeignKeyConstraintsForColumn(EntityType table, EdmProperty column)
        {
            Contract.Requires(table != null);
            Contract.Requires(column != null);

            table.ForeignKeyBuilders
                .Where(fk => fk.DependentColumns.Contains(column))
                .ToArray()
                .Each(table.RemoveForeignKey);
        }
    }

    internal static class TableOperations
    {
        public static EdmProperty CopyColumnAndAnyConstraints(
            EdmModel database,
            EntityType fromTable,
            EntityType toTable,
            EdmProperty column,
            bool useExisting,
            bool allowPkConstraintCopy)
        {
            Contract.Requires(fromTable != null);
            Contract.Requires(toTable != null);
            Contract.Requires(column != null);

            var movedColumn = column;

            if (fromTable != toTable)
            {
                movedColumn = TablePrimitiveOperations.IncludeColumn(toTable, column, useExisting);
                if (allowPkConstraintCopy || !movedColumn.IsPrimaryKeyColumn)
                {
                    ForeignKeyPrimitiveOperations.CopyAllForeignKeyConstraintsForColumn(
                        database, fromTable, toTable, column);
                }
            }

            return movedColumn;
        }

        public static EdmProperty MoveColumnAndAnyConstraints(
            EntityType fromTable, EntityType toTable, EdmProperty column, bool useExisting)
        {
            Contract.Requires(fromTable != null);
            Contract.Requires(toTable != null);
            Contract.Requires(column != null);

            var movedColumn = column;

            if (fromTable != toTable)
            {
                movedColumn = TablePrimitiveOperations.IncludeColumn(toTable, column, useExisting);
                TablePrimitiveOperations.RemoveColumn(fromTable, column);
                ForeignKeyPrimitiveOperations.MoveAllForeignKeyConstraintsForColumn(fromTable, toTable, column);
            }

            return movedColumn;
        }
    }

    internal class EntityMappingOperations
    {
        public static DbEntityTypeMappingFragment CreateTypeMappingFragment(
            DbEntityTypeMapping entityTypeMapping, DbEntityTypeMappingFragment templateFragment, EntityType table)
        {
            var fragment = new DbEntityTypeMappingFragment
                               {
                                   Table = table
                               };
            entityTypeMapping.TypeMappingFragments.Add(fragment);

            // Move all PK mappings to the extra fragment
            foreach (
                var pkPropertyMapping in templateFragment.PropertyMappings.Where(pm => pm.Column.IsPrimaryKeyColumn))
            {
                CopyPropertyMappingToFragment(pkPropertyMapping, fragment, true);
            }
            return fragment;
        }

        private static void UpdatePropertyMapping(
            EdmModel database,
            DbEdmPropertyMapping propertyMapping,
            EntityType fromTable,
            EntityType toTable,
            bool useExisting)
        {
            propertyMapping.Column
                = TableOperations.CopyColumnAndAnyConstraints(
                    database, fromTable, toTable, propertyMapping.Column, useExisting, false);
            propertyMapping.SyncNullabilityCSSpace();
        }

        public static void UpdatePropertyMappings(
            EdmModel database,
            EntityType fromTable,
            DbEntityTypeMappingFragment fragment,
            bool useExisting)
        {
            // move the column from the formTable to the table in fragment
            if (fromTable != fragment.Table)
            {
                fragment.PropertyMappings.Each(
                    pm => UpdatePropertyMapping(database, pm, fromTable, fragment.Table, useExisting));
            }
        }

        public static void MovePropertyMapping(
            EdmModel database,
            DbEntityTypeMappingFragment fromFragment,
            DbEntityTypeMappingFragment toFragment,
            DbEdmPropertyMapping propertyMapping,
            bool requiresUpdate,
            bool useExisting)
        {
            // move the column from the formTable to the table in fragment
            if (requiresUpdate && fromFragment.Table != toFragment.Table)
            {
                UpdatePropertyMapping(database, propertyMapping, fromFragment.Table, toFragment.Table, useExisting);
            }

            // move the propertyMapping
            fromFragment.PropertyMappings.Remove(propertyMapping);
            toFragment.PropertyMappings.Add(propertyMapping);
        }

        public static void CopyPropertyMappingToFragment(
            DbEdmPropertyMapping propertyMapping, DbEntityTypeMappingFragment fragment, bool useExisting)
        {
            // Ensure column is in the fragment's table
            var column = TablePrimitiveOperations.IncludeColumn(fragment.Table, propertyMapping.Column, useExisting);

            // Add the property mapping
            fragment.PropertyMappings.Add(
                new DbEdmPropertyMapping
                    {
                        PropertyPath = propertyMapping.PropertyPath,
                        Column = column
                    });
        }

        public static void UpdateConditions(
            EdmModel database, EntityType fromTable, DbEntityTypeMappingFragment fragment)
        {
            // move the condition's column from the formTable to the table in fragment
            if (fromTable != fragment.Table)
            {
                fragment.ColumnConditions.Each(
                    cc =>
                        {
                            cc.Column = TableOperations.CopyColumnAndAnyConstraints(
                                database, fromTable, fragment.Table, cc.Column, true, false);
                        });
            }
        }
    }

    internal class AssociationMappingOperations
    {
        private static void MoveAssociationSetMappingDependents(
            DbAssociationSetMapping associationSetMapping,
            DbAssociationEndMapping dependentMapping,
            EntityType toTable,
            bool useExistingColumns)
        {
            Contract.Requires(associationSetMapping != null);
            Contract.Requires(dependentMapping != null);
            Contract.Requires(toTable != null);

            dependentMapping.PropertyMappings.Each(
                pm =>
                    {
                        var oldColumn = pm.Column;
                        pm.Column = TableOperations.MoveColumnAndAnyConstraints(
                            associationSetMapping.Table, toTable, oldColumn, useExistingColumns);
                        associationSetMapping.ColumnConditions.Where(cc => cc.Column == oldColumn).Each(
                            cc =>
                            cc.Column = pm.Column);
                    });

            associationSetMapping.Table = toTable;
        }

        public static void MoveAllDeclaredAssociationSetMappings(
            DbDatabaseMapping databaseMapping,
            EntityType entityType,
            EntityType fromTable,
            EntityType toTable,
            bool useExistingColumns)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(entityType != null);
            Contract.Requires(fromTable != null);
            Contract.Requires(toTable != null);

            foreach (
                var associationSetMapping in
                    databaseMapping.EntityContainerMappings.SelectMany(asm => asm.AssociationSetMappings)
                        .Where(
                            a =>
                            a.Table == fromTable &&
                            (a.AssociationSet.ElementType.SourceEnd.GetEntityType() == entityType ||
                             a.AssociationSet.ElementType.TargetEnd.GetEntityType() == entityType)).ToArray())
            {
                AssociationEndMember _, dependentEnd;
                if (
                    !associationSetMapping.AssociationSet.ElementType.TryGuessPrincipalAndDependentEnds(
                        out _, out dependentEnd))
                {
                    dependentEnd = associationSetMapping.AssociationSet.ElementType.TargetEnd;
                }

                if (dependentEnd.GetEntityType() == entityType)
                {
                    var dependentMapping = dependentEnd == associationSetMapping.TargetEndMapping.AssociationEnd
                                               ? associationSetMapping.SourceEndMapping
                                               : associationSetMapping.TargetEndMapping;

                    MoveAssociationSetMappingDependents(
                        associationSetMapping, dependentMapping, toTable, useExistingColumns);
                }
            }
        }
    }

    internal class DatabaseOperations
    {
        public static void AddTypeConstraint(
            EdmModel database,
            EntityType entityType,
            EntityType principalTable,
            EntityType dependentTable,
            bool isSplitting)
        {
            Contract.Requires(principalTable != null);
            Contract.Requires(dependentTable != null);
            Contract.Requires(entityType != null);

            var foreignKeyConstraintMetadata
                = new ForeignKeyBuilder(
                    database, String.Format(
                        CultureInfo.InvariantCulture,
                        "{0}_TypeConstraint_From_{1}_To_{2}",
                        entityType.Name,
                        principalTable.Name,
                        dependentTable.Name))
                      {
                          PrincipalTable = principalTable
                      };

            dependentTable.AddForeignKey(foreignKeyConstraintMetadata);

            if (isSplitting)
            {
                foreignKeyConstraintMetadata.SetIsSplitConstraint();
            }
            else
            {
                foreignKeyConstraintMetadata.SetIsTypeConstraint();
            }

            foreignKeyConstraintMetadata.DependentColumns = dependentTable.Properties.Where(c => c.IsPrimaryKeyColumn);

            //If "DbStoreGeneratedPattern.Identity" was copied from the parent table, it should be removed
            dependentTable.Properties.Where(c => c.IsPrimaryKeyColumn).Each(c => c.RemoveStoreGeneratedIdentityPattern());
        }
    }
}
