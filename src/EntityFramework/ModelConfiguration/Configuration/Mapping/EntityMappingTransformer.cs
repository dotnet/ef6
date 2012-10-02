// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
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
        private static DbTableColumnMetadata AddColumn(DbTableMetadata table, DbTableColumnMetadata column)
        {
            Contract.Requires(table != null);
            Contract.Requires(column != null);

            if (!table.Columns.Contains(column))
            {
                var configuration = column.GetConfiguration() as PrimitivePropertyConfiguration;

                if ((configuration == null)
                    || string.IsNullOrWhiteSpace(configuration.ColumnName))
                {
                    var preferredName = column.GetPreferredName() ?? column.Name;
                    column.SetUnpreferredUniqueName(column.Name);
                    column.Name = table.Columns.UniquifyName(preferredName);
                }

                table.Columns.Add(column);
            }

            return column;
        }

        public static DbTableColumnMetadata RemoveColumn(DbTableMetadata table, DbTableColumnMetadata column)
        {
            Contract.Requires(table != null);
            Contract.Requires(column != null);

            if (!column.IsPrimaryKeyColumn)
            {
                table.Columns.Remove(column);
            }

            return column;
        }

        public static DbTableColumnMetadata IncludeColumn(
            DbTableMetadata table, DbTableColumnMetadata templateColumn, bool useExisting)
        {
            Contract.Requires(table != null);
            Contract.Requires(templateColumn != null);

            var existingColumn =
                table.Columns.SingleOrDefault(c => string.Equals(c.Name, templateColumn.Name, StringComparison.Ordinal));

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

            return AddColumn(table, templateColumn);
        }

        public static DbTableColumnMetadata IncludeColumn(DbTableMetadata table, string columnName, bool useExisting)
        {
            Contract.Requires(table != null);
            Contract.Requires(columnName != null);

            var existingColumn =
                table.Columns.SingleOrDefault(c => string.Equals(c.Name, columnName, StringComparison.Ordinal));
            DbTableColumnMetadata column = null;
            if (existingColumn == null)
            {
                column = table.AddColumn(columnName);
            }
            else if (!useExisting
                     && !existingColumn.IsPrimaryKeyColumn)
            {
                column = table.AddColumn(columnName);
            }
            else
            {
                column = existingColumn;
            }

            return AddColumn(table, column);
        }
    }

    internal class ForeignKeyPrimitiveOperations
    {
        public static void UpdatePrincipalTables(
            DbDatabaseMapping databaseMapping,
            EdmEntityType entityType,
            DbTableMetadata fromTable,
            DbTableMetadata toTable,
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
                    UpdatePrincipalTables(databaseMapping, toTable, entityType.BaseType, removeFks: true);
                }
            }
        }

        private static void UpdatePrincipalTables(
            DbDatabaseMapping databaseMapping, DbTableMetadata toTable, EdmEntityType entityType, bool removeFks)
        {
            foreach (var associationType in databaseMapping.Model.Namespaces.Single().AssociationTypes
                .Where(at => at.SourceEnd.EntityType.Equals(entityType) || at.TargetEnd.EntityType.Equals(entityType)))
            {
                UpdatePrincipalTables(databaseMapping, toTable, removeFks, associationType, entityType);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static void UpdatePrincipalTables(
            DbDatabaseMapping databaseMapping, DbTableMetadata toTable, bool removeFks,
            EdmAssociationType associationType, EdmEntityType et)
        {
            EdmAssociationEnd principalEnd, dependentEnd;
            var endsToCheck = new List<EdmAssociationEnd>();
            if (associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd))
            {
                endsToCheck.Add(principalEnd);
            }
            else if (associationType.SourceEnd.EndKind == EdmAssociationEndKind.Many
                     && associationType.TargetEnd.EndKind == EdmAssociationEndKind.Many)
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
                if (end.EntityType == et)
                {
                    IEnumerable<KeyValuePair<DbTableMetadata, IEnumerable<DbTableColumnMetadata>>> dependentTableInfos;
                    if (associationType.Constraint != null)
                    {
                        var originalDependentType = associationType.GetOtherEnd(end).EntityType;
                        var allDependentTypes = databaseMapping.Model.GetSelfAndAllDerivedTypes(originalDependentType);

                        dependentTableInfos =
                            allDependentTypes.Select(t => databaseMapping.GetEntityTypeMapping(t)).Where(
                                dm => dm != null)
                                .SelectMany(
                                    dm => dm.TypeMappingFragments
                                              .Where(
                                                  tmf => associationType.Constraint.DependentProperties
                                                             .All(
                                                                 p =>
                                                                 tmf.PropertyMappings.Any(
                                                                     pm => pm.PropertyPath.First() == p))))
                                .Distinct((f1, f2) => f1.Table == f2.Table)
                                .Select(
                                    df =>
                                    new KeyValuePair<DbTableMetadata, IEnumerable<DbTableColumnMetadata>>(
                                        df.Table,
                                        df.PropertyMappings.Where(
                                            pm =>
                                            associationType.Constraint.DependentProperties.Contains(
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
                                                          <DbTableMetadata, IEnumerable<DbTableColumnMetadata>>(
                                                          dependentTable, dependentColumns)
                                                  };
                    }

                    foreach (var tableInfo in dependentTableInfos)
                    {
                        foreach (
                            var fk in
                                tableInfo.Key.ForeignKeyConstraints.Where(
                                    fk => fk.DependentColumns.SequenceEqual(tableInfo.Value)).ToArray(
                                        
                                    ))
                        {
                            if (removeFks)
                            {
                                tableInfo.Key.ForeignKeyConstraints.Remove(fk);
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
            DbTableMetadata fromTable, DbTableMetadata toTable, DbForeignKeyConstraintMetadata fk)
        {
            Contract.Requires(fromTable != null);
            Contract.Requires(toTable != null);
            Contract.Requires(fk != null);

            fromTable.ForeignKeyConstraints.Remove(fk);

            // Only move it to the new table if the destination is not the principal table or if all dependent columns are not FKs
            // Otherwise you end up with an FK from the PKs to the PKs of the same table
            if (fk.PrincipalTable != toTable
                ||
                !fk.DependentColumns.All(c => c.IsPrimaryKeyColumn))
            {
                // Make sure all the dependent columns refer to columns in the newTable
                var oldColumns = fk.DependentColumns.ToArray();
                fk.DependentColumns.Clear();
                SetAllDependentColumns(fk, oldColumns, toTable.Columns);

                if (!toTable.ContainsEquivalentForeignKey(fk))
                {
                    toTable.ForeignKeyConstraints.Add(fk);
                }
            }
        }

        private static void CopyForeignKeyConstraint(
            DbDatabaseMetadata database, DbTableMetadata toTable,
            DbForeignKeyConstraintMetadata fk)
        {
            Contract.Requires(toTable != null);
            Contract.Requires(fk != null);

            var newFk = new DbForeignKeyConstraintMetadata
                            {
                                DeleteAction = fk.DeleteAction,
                                Name =
                                    database.Schemas.Single().Tables.SelectMany(t => t.ForeignKeyConstraints).
                                    UniquifyName(fk.Name),
                                PrincipalTable = fk.PrincipalTable
                            };

            // Make sure all the dependent columns refer to columns in the newTable
            SetAllDependentColumns(newFk, fk.DependentColumns, toTable.Columns);

            if (!toTable.ContainsEquivalentForeignKey(newFk))
            {
                toTable.ForeignKeyConstraints.Add(newFk);
            }
        }

        private static void SetAllDependentColumns(
            DbForeignKeyConstraintMetadata fk,
            IEnumerable<DbTableColumnMetadata> sourceColumns,
            IEnumerable<DbTableColumnMetadata> destinationColumns)
        {
            foreach (var dc in sourceColumns)
            {
                fk.DependentColumns.Add(
                    destinationColumns.Single(
                        c =>
                        string.Equals(c.Name, dc.Name, StringComparison.Ordinal)
                        || string.Equals(c.GetUnpreferredUniqueName(), dc.Name, StringComparison.Ordinal)));
            }
        }

        private static IEnumerable<DbForeignKeyConstraintMetadata> FindAllForeignKeyConstraintsForColumn(
            DbTableMetadata fromTable, DbTableMetadata toTable, DbTableColumnMetadata column)
        {
            return fromTable
                .ForeignKeyConstraints
                .Where(
                    fk => fk.DependentColumns.Contains(column) &&
                          fk.DependentColumns.All(
                              c => toTable.Columns.Any(
                                  nc =>
                                  string.Equals(nc.Name, c.Name, StringComparison.Ordinal)
                                  || string.Equals(nc.GetUnpreferredUniqueName(), c.Name, StringComparison.Ordinal))));
        }

        public static void CopyAllForeignKeyConstraintsForColumn(
            DbDatabaseMetadata database, DbTableMetadata fromTable, DbTableMetadata toTable,
            DbTableColumnMetadata column)
        {
            Contract.Requires(fromTable != null);
            Contract.Requires(toTable != null);
            Contract.Requires(column != null);

            FindAllForeignKeyConstraintsForColumn(fromTable, toTable, column)
                .ToArray()
                .Each(fk => CopyForeignKeyConstraint(database, toTable, fk));
        }

        public static void MoveAllDeclaredForeignKeyConstraintsForPrimaryKeyColumns(
            EdmEntityType entityType, DbTableMetadata fromTable, DbTableMetadata toTable)
        {
            Contract.Requires(fromTable != null);
            Contract.Requires(toTable != null);

            foreach (var column in fromTable.KeyColumns)
            {
                FindAllForeignKeyConstraintsForColumn(fromTable, toTable, column)
                    .ToArray()
                    .Each(
                        fk =>
                            {
                                var at = fk.GetAssociationType();
                                if (at != null && at.Constraint.DependentEnd.EntityType == entityType
                                    && !fk.GetIsTypeConstraint())
                                {
                                    MoveForeignKeyConstraint(fromTable, toTable, fk);
                                }
                            });
            }
        }

        public static void CopyAllForeignKeyConstraintsForPrimaryKeyColumns(
            DbDatabaseMetadata database, DbTableMetadata fromTable, DbTableMetadata toTable)
        {
            Contract.Requires(fromTable != null);
            Contract.Requires(toTable != null);

            foreach (var column in fromTable.KeyColumns)
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
            DbTableMetadata fromTable, DbTableMetadata toTable, DbTableColumnMetadata column)
        {
            Contract.Requires(fromTable != null);
            Contract.Requires(toTable != null);
            Contract.Requires(column != null);

            FindAllForeignKeyConstraintsForColumn(fromTable, toTable, column)
                .ToArray()
                .Each(fk => { MoveForeignKeyConstraint(fromTable, toTable, fk); });
        }

        public static void RemoveAllForeignKeyConstraintsForColumn(DbTableMetadata table, DbTableColumnMetadata column)
        {
            Contract.Requires(table != null);
            Contract.Requires(column != null);

            table.ForeignKeyConstraints
                .Where(fk => fk.DependentColumns.Contains(column))
                .ToArray()
                .Each(fk => table.ForeignKeyConstraints.Remove(fk));
        }
    }

    internal static class TableOperations
    {
        public static DbTableColumnMetadata CopyColumnAndAnyConstraints(
            DbDatabaseMetadata database,
            DbTableMetadata fromTable,
            DbTableMetadata toTable,
            DbTableColumnMetadata column,
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

        public static DbTableColumnMetadata MoveColumnAndAnyConstraints(
            DbTableMetadata fromTable, DbTableMetadata toTable, DbTableColumnMetadata column, bool useExisting)
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
            DbEntityTypeMapping entityTypeMapping, DbEntityTypeMappingFragment templateFragment, DbTableMetadata table)
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
            DbDatabaseMetadata database,
            DbEdmPropertyMapping propertyMapping,
            DbTableMetadata fromTable,
            DbTableMetadata toTable,
            bool useExisting)
        {
            propertyMapping.Column
                = TableOperations.CopyColumnAndAnyConstraints(
                    database, fromTable, toTable, propertyMapping.Column, useExisting, false);
            propertyMapping.SyncNullabilityCSSpace();
        }

        public static void UpdatePropertyMappings(
            DbDatabaseMetadata database,
            DbTableMetadata fromTable,
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
            DbDatabaseMetadata database,
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
            DbDatabaseMetadata database, DbTableMetadata fromTable, DbEntityTypeMappingFragment fragment)
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
            DbTableMetadata toTable,
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
            EdmEntityType entityType,
            DbTableMetadata fromTable,
            DbTableMetadata toTable,
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
                            (a.AssociationSet.ElementType.SourceEnd.EntityType == entityType ||
                             a.AssociationSet.ElementType.TargetEnd.EntityType == entityType)).ToArray())
            {
                EdmAssociationEnd _, dependentEnd;
                if (
                    !associationSetMapping.AssociationSet.ElementType.TryGuessPrincipalAndDependentEnds(
                        out _, out dependentEnd))
                {
                    dependentEnd = associationSetMapping.AssociationSet.ElementType.TargetEnd;
                }

                if (dependentEnd.EntityType == entityType)
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
            EdmEntityType entityType, DbTableMetadata principalTable, DbTableMetadata dependentTable, bool isSplitting)
        {
            Contract.Requires(principalTable != null);
            Contract.Requires(dependentTable != null);
            Contract.Requires(entityType != null);

            var foreignKeyConstraintMetadata = new DbForeignKeyConstraintMetadata
                                                   {
                                                       Name =
                                                           String.Format(
                                                               CultureInfo.InvariantCulture,
                                                               "{0}_TypeConstraint_From_{1}_To_{2}",
                                                               entityType.Name,
                                                               principalTable.Name,
                                                               dependentTable.Name),
                                                       PrincipalTable = principalTable
                                                   };

            if (isSplitting)
            {
                foreignKeyConstraintMetadata.SetIsSplitConstraint();
            }
            else
            {
                foreignKeyConstraintMetadata.SetIsTypeConstraint();
            }
            dependentTable.Columns
                .Where(c => c.IsPrimaryKeyColumn)
                .Each(c => foreignKeyConstraintMetadata.DependentColumns.Add(c));

            dependentTable.ForeignKeyConstraints.Add(foreignKeyConstraintMetadata);

            //If "DbStoreGeneratedPattern.Identity" was copied from the parent table, it should be removed
            dependentTable.Columns.Where(c => c.IsPrimaryKeyColumn).Each(c => c.RemoveStoreGeneratedIdentityPattern());
        }
    }
}
