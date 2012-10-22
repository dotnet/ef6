// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Services
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal class AssociationTypeMappingGenerator : StructuralTypeMappingGenerator
    {
        public AssociationTypeMappingGenerator(DbProviderManifest providerManifest)
            : base(providerManifest)
        {
        }

        public void Generate(AssociationType associationType, DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(associationType != null);
            Contract.Requires(databaseMapping != null);

            if (associationType.Constraint != null)
            {
                GenerateForeignKeyAssociationType(associationType, databaseMapping);
            }
            else if (associationType.IsManyToMany())
            {
                GenerateManyToManyAssociation(associationType, databaseMapping);
            }
            else
            {
                GenerateIndependentAssociationType(associationType, databaseMapping);
            }
        }

        private static void GenerateForeignKeyAssociationType(
            AssociationType associationType, DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(associationType != null);
            Contract.Requires(databaseMapping != null);
            Contract.Assert(associationType.Constraint != null);

            var dependentEnd = associationType.Constraint.DependentEnd;
            var principalEnd = associationType.GetOtherEnd(dependentEnd);
            var principalEntityTypeMapping = GetEntityTypeMappingInHierarchy(databaseMapping, principalEnd.GetEntityType());
            var dependentEntityTypeMapping = GetEntityTypeMappingInHierarchy(databaseMapping, dependentEnd.GetEntityType());

            var foreignKeyConstraint
                = new ForeignKeyBuilder(databaseMapping.Database, associationType.Name)
                      {
                          PrincipalTable =
                              principalEntityTypeMapping.TypeMappingFragments.Single().Table,
                          DeleteAction = principalEnd.DeleteBehavior != OperationAction.None
                                             ? principalEnd.DeleteBehavior
                                             : OperationAction.None
                      };

            dependentEntityTypeMapping
                .TypeMappingFragments
                .Single()
                .Table
                .AddForeignKey(foreignKeyConstraint);

            foreignKeyConstraint.DependentColumns = associationType.Constraint.ToProperties.Select(
                dependentProperty => dependentEntityTypeMapping.GetPropertyMapping(dependentProperty).Column);

            foreignKeyConstraint.SetAssociationType(associationType);
        }

        private void GenerateManyToManyAssociation(
            AssociationType associationType, DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(associationType != null);
            Contract.Requires(databaseMapping != null);

            var sourceEntityType = associationType.SourceEnd.GetEntityType();
            var targetEntityType = associationType.TargetEnd.GetEntityType();

            var joinTable
                = databaseMapping.Database.AddTable(sourceEntityType.Name + targetEntityType.Name);

            var associationSetMapping
                = GenerateAssociationSetMapping(
                    associationType, databaseMapping, associationType.SourceEnd, associationType.TargetEnd, joinTable);

            GenerateIndependentForeignKeyConstraint(
                databaseMapping,
                sourceEntityType,
                targetEntityType,
                joinTable,
                associationSetMapping,
                associationSetMapping.SourceEndMapping,
                associationType.SourceEnd.Name,
                null,
                isPrimaryKeyColumn: true);

            GenerateIndependentForeignKeyConstraint(
                databaseMapping,
                targetEntityType,
                sourceEntityType,
                joinTable,
                associationSetMapping,
                associationSetMapping.TargetEndMapping,
                associationType.TargetEnd.Name,
                null,
                isPrimaryKeyColumn: true);
        }

        private void GenerateIndependentAssociationType(
            AssociationType associationType, DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(associationType != null);
            Contract.Requires(databaseMapping != null);

            AssociationEndMember principalEnd, dependentEnd;
            if (!associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd))
            {
                if (!associationType.IsPrincipalConfigured())
                {
                    throw Error.UnableToDeterminePrincipal(
                        associationType.SourceEnd.GetEntityType().GetClrType(),
                        associationType.TargetEnd.GetEntityType().GetClrType());
                }

                principalEnd = associationType.SourceEnd;
                dependentEnd = associationType.TargetEnd;
            }

            var dependentEntityTypeMapping = GetEntityTypeMappingInHierarchy(databaseMapping, dependentEnd.GetEntityType());

            var dependentTable = dependentEntityTypeMapping
                .TypeMappingFragments
                .First()
                .Table;

            var associationSetMapping
                = GenerateAssociationSetMapping(
                    associationType, databaseMapping, principalEnd, dependentEnd, dependentTable);

            GenerateIndependentForeignKeyConstraint(
                databaseMapping,
                principalEnd.GetEntityType(),
                dependentEnd.GetEntityType(),
                dependentTable,
                associationSetMapping,
                associationSetMapping.SourceEndMapping,
                associationType.Name,
                principalEnd);

            foreach (var property in dependentEnd.GetEntityType().KeyProperties())
            {
                associationSetMapping.TargetEndMapping.PropertyMappings.Add(
                    new DbEdmPropertyMapping
                        {
                            Column = dependentEntityTypeMapping.GetPropertyMapping(property).Column,
                            PropertyPath = new[] { property }
                        });
            }
        }

        private static DbAssociationSetMapping GenerateAssociationSetMapping(
            AssociationType associationType,
            DbDatabaseMapping databaseMapping,
            AssociationEndMember principalEnd,
            AssociationEndMember dependentEnd,
            EntityType dependentTable)
        {
            Contract.Requires(associationType != null);
            Contract.Requires(databaseMapping != null);
            Contract.Requires(principalEnd != null);
            Contract.Requires(dependentEnd != null);
            Contract.Requires(dependentTable != null);

            var associationSetMapping
                = databaseMapping.AddAssociationSetMapping(
                    databaseMapping.Model.GetAssociationSet(associationType));

            associationSetMapping.Table = dependentTable;
            associationSetMapping.SourceEndMapping.AssociationEnd = principalEnd;
            associationSetMapping.TargetEndMapping.AssociationEnd = dependentEnd;

            return associationSetMapping;
        }

        private void GenerateIndependentForeignKeyConstraint(
            DbDatabaseMapping databaseMapping,
            EntityType principalEntityType,
            EntityType dependentEntityType,
            EntityType dependentTable,
            DbAssociationSetMapping associationSetMapping,
            DbAssociationEndMapping associationEndMapping,
            string name,
            AssociationEndMember principalEnd,
            bool isPrimaryKeyColumn = false)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(principalEntityType != null);
            Contract.Requires(dependentTable != null);
            Contract.Requires(associationEndMapping != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            var principalTable
                = GetEntityTypeMappingInHierarchy(databaseMapping, principalEntityType)
                    .TypeMappingFragments
                    .Single()
                    .Table;

            var foreignKeyConstraint
                = new ForeignKeyBuilder(databaseMapping.Database, name)
                      {
                          PrincipalTable = principalTable,
                          DeleteAction = associationEndMapping.AssociationEnd.DeleteBehavior != OperationAction.None
                                             ? associationEndMapping.AssociationEnd.DeleteBehavior
                                             : OperationAction.None
                      };

            var principalNavigationProperty
                = databaseMapping.Model.GetEntityTypes()
                    .SelectMany(e => e.DeclaredNavigationProperties)
                    .SingleOrDefault(n => n.ResultEnd == principalEnd);

            dependentTable.AddForeignKey(foreignKeyConstraint);

            foreignKeyConstraint.DependentColumns = GenerateIndependentForeignKeyColumns(
                principalEntityType,
                dependentEntityType,
                associationSetMapping,
                associationEndMapping,
                dependentTable,
                foreignKeyConstraint,
                isPrimaryKeyColumn,
                principalNavigationProperty);
        }

        private IEnumerable<EdmProperty> GenerateIndependentForeignKeyColumns(
            EntityType principalEntityType,
            EntityType dependentEntityType,
            DbAssociationSetMapping associationSetMapping,
            DbAssociationEndMapping associationEndMapping,
            EntityType dependentTable,
            ForeignKeyBuilder foreignKeyConstraint,
            bool isPrimaryKeyColumn,
            NavigationProperty principalNavigationProperty)
        {
            Contract.Requires(principalEntityType != null);
            Contract.Requires(associationEndMapping != null);
            Contract.Requires(dependentTable != null);
            Contract.Requires(foreignKeyConstraint != null);

            foreach (var property in principalEntityType.KeyProperties())
            {
                var columnName
                    = ((principalNavigationProperty != null)
                           ? principalNavigationProperty.Name
                           : principalEntityType.Name) + "_" + property.Name;

                var foreignKeyColumn
                    = MapTableColumn(property, columnName, false);

                dependentTable.AddColumn(foreignKeyColumn);

                if (isPrimaryKeyColumn)
                {
                    dependentTable.AddKeyMember(foreignKeyColumn);
                }

                foreignKeyColumn.Nullable
                    = associationEndMapping.AssociationEnd.IsOptional()
                      || (associationEndMapping.AssociationEnd.IsRequired()
                          && dependentEntityType.BaseType != null);

                foreignKeyColumn.StoreGeneratedPattern = StoreGeneratedPattern.None;

                yield return foreignKeyColumn;

                associationEndMapping.PropertyMappings.Add(
                    new DbEdmPropertyMapping
                        {
                            Column = foreignKeyColumn,
                            PropertyPath = new[] { property }
                        });

                if (foreignKeyColumn.Nullable)
                {
                    associationSetMapping.ColumnConditions.Add(
                        new DbColumnCondition
                            {
                                Column = foreignKeyColumn,
                                IsNull = false
                            });
                }
            }
        }
    }
}
