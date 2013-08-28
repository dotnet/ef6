// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Services
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    internal class AssociationTypeMappingGenerator : StructuralTypeMappingGenerator
    {
        public AssociationTypeMappingGenerator(DbProviderManifest providerManifest)
            : base(providerManifest)
        {
        }

        public void Generate(AssociationType associationType, DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(associationType);
            DebugCheck.NotNull(databaseMapping);

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
            DebugCheck.NotNull(associationType);
            DebugCheck.NotNull(databaseMapping);
            Debug.Assert(associationType.Constraint != null);

            var dependentEnd = associationType.Constraint.DependentEnd;
            var principalEnd = associationType.GetOtherEnd(dependentEnd);
            var principalEntityTypeMapping = GetEntityTypeMappingInHierarchy(databaseMapping, principalEnd.GetEntityType());
            var dependentEntityTypeMapping = GetEntityTypeMappingInHierarchy(databaseMapping, dependentEnd.GetEntityType());

            var foreignKeyConstraint
                = new ForeignKeyBuilder(databaseMapping.Database, associationType.Name)
                    {
                        PrincipalTable =
                            principalEntityTypeMapping.MappingFragments.Single().Table,
                        DeleteAction = principalEnd.DeleteBehavior != OperationAction.None
                                           ? principalEnd.DeleteBehavior
                                           : OperationAction.None
                    };

            dependentEntityTypeMapping
                .MappingFragments
                .Single()
                .Table
                .AddForeignKey(foreignKeyConstraint);

            foreignKeyConstraint.DependentColumns = associationType.Constraint.ToProperties.Select(
                dependentProperty => dependentEntityTypeMapping.GetPropertyMapping(dependentProperty).ColumnProperty);

            foreignKeyConstraint.SetAssociationType(associationType);
        }

        private void GenerateManyToManyAssociation(
            AssociationType associationType, DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(associationType);
            DebugCheck.NotNull(databaseMapping);

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
            DebugCheck.NotNull(associationType);
            DebugCheck.NotNull(databaseMapping);

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
                .MappingFragments
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
                associationSetMapping.TargetEndMapping
                                     .AddProperty(
                                         new ScalarPropertyMapping(
                                             property,
                                             dependentEntityTypeMapping.GetPropertyMapping(property).ColumnProperty));
            }
        }

        private static AssociationSetMapping GenerateAssociationSetMapping(
            AssociationType associationType,
            DbDatabaseMapping databaseMapping,
            AssociationEndMember principalEnd,
            AssociationEndMember dependentEnd,
            EntityType dependentTable)
        {
            DebugCheck.NotNull(associationType);
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(principalEnd);
            DebugCheck.NotNull(dependentEnd);
            DebugCheck.NotNull(dependentTable);

            var associationSetMapping
                = databaseMapping.AddAssociationSetMapping(
                    databaseMapping.Model.GetAssociationSet(associationType),
                    databaseMapping.Database.GetEntitySet(dependentTable));

            associationSetMapping.StoreEntitySet = databaseMapping.Database.GetEntitySet(dependentTable);
            associationSetMapping.SourceEndMapping.EndMember = principalEnd;
            associationSetMapping.TargetEndMapping.EndMember = dependentEnd;

            return associationSetMapping;
        }

        private void GenerateIndependentForeignKeyConstraint(
            DbDatabaseMapping databaseMapping,
            EntityType principalEntityType,
            EntityType dependentEntityType,
            EntityType dependentTable,
            AssociationSetMapping associationSetMapping,
            EndPropertyMapping associationEndMapping,
            string name,
            AssociationEndMember principalEnd,
            bool isPrimaryKeyColumn = false)
        {
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(principalEntityType);
            DebugCheck.NotNull(dependentTable);
            DebugCheck.NotNull(associationEndMapping);
            DebugCheck.NotEmpty(name);

            var principalTable
                = GetEntityTypeMappingInHierarchy(databaseMapping, principalEntityType)
                    .MappingFragments
                    .Single()
                    .Table;

            var foreignKeyConstraint
                = new ForeignKeyBuilder(databaseMapping.Database, name)
                    {
                        PrincipalTable = principalTable,
                        DeleteAction = associationEndMapping.EndMember.DeleteBehavior != OperationAction.None
                                           ? associationEndMapping.EndMember.DeleteBehavior
                                           : OperationAction.None
                    };

            var principalNavigationProperty
                = databaseMapping.Model.EntityTypes
                                 .SelectMany(e => e.DeclaredNavigationProperties)
                                 .SingleOrDefault(n => n.ResultEnd == principalEnd);

            dependentTable.AddForeignKey(foreignKeyConstraint);

            foreignKeyConstraint.DependentColumns = GenerateIndependentForeignKeyColumns(
                principalEntityType,
                dependentEntityType,
                associationSetMapping,
                associationEndMapping,
                dependentTable,
                isPrimaryKeyColumn,
                principalNavigationProperty);
        }

        private IEnumerable<EdmProperty> GenerateIndependentForeignKeyColumns(
            EntityType principalEntityType,
            EntityType dependentEntityType,
            AssociationSetMapping associationSetMapping,
            EndPropertyMapping associationEndMapping,
            EntityType dependentTable,
            bool isPrimaryKeyColumn,
            NavigationProperty principalNavigationProperty)
        {
            DebugCheck.NotNull(principalEntityType);
            DebugCheck.NotNull(associationEndMapping);
            DebugCheck.NotNull(dependentTable);

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
                    = associationEndMapping.EndMember.IsOptional()
                      || (associationEndMapping.EndMember.IsRequired()
                          && dependentEntityType.BaseType != null);

                foreignKeyColumn.StoreGeneratedPattern = StoreGeneratedPattern.None;

                yield return foreignKeyColumn;

                associationEndMapping.AddProperty(new ScalarPropertyMapping(property, foreignKeyColumn));

                if (foreignKeyColumn.Nullable)
                {
                    associationSetMapping
                        .AddColumnCondition(new ConditionPropertyMapping(null, foreignKeyColumn, null, false));
                }
            }
        }
    }
}
