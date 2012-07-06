namespace System.Data.Entity.ModelConfiguration.Edm.Services
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Edm;
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

        public void Generate(EdmAssociationType associationType, DbDatabaseMapping databaseMapping)
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
            EdmAssociationType associationType, DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(associationType != null);
            Contract.Requires(databaseMapping != null);
            Contract.Assert(associationType.Constraint != null);

            var dependentEnd = associationType.Constraint.DependentEnd;
            var principalEnd = associationType.GetOtherEnd(dependentEnd);
            var principalEntityTypeMapping = GetEntityTypeMappingInHierarchy(databaseMapping, principalEnd.EntityType);
            var dependentEntityTypeMapping = GetEntityTypeMappingInHierarchy(databaseMapping, dependentEnd.EntityType);

            var foreignKeyConstraint = new DbForeignKeyConstraintMetadata
                {
                    Name = associationType.Name,
                    PrincipalTable =
                        principalEntityTypeMapping.TypeMappingFragments.Single().Table,
                    DeleteAction = principalEnd.DeleteAction.HasValue
                                       ? (DbOperationAction)principalEnd.DeleteAction.Value
                                       : DbOperationAction.None
                };

            foreach (var dependentProperty in associationType.Constraint.DependentProperties)
            {
                foreignKeyConstraint.DependentColumns.Add(
                    dependentEntityTypeMapping.GetPropertyMapping(dependentProperty).Column);
            }

            foreignKeyConstraint.SetAssociationType(associationType);

            dependentEntityTypeMapping
                .TypeMappingFragments
                .Single()
                .Table
                .ForeignKeyConstraints.Add(foreignKeyConstraint);
        }

        private void GenerateManyToManyAssociation(
            EdmAssociationType associationType, DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(associationType != null);
            Contract.Requires(databaseMapping != null);

            var sourceEntityType = associationType.SourceEnd.EntityType;
            var targetEntityType = associationType.TargetEnd.EntityType;

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
            EdmAssociationType associationType, DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(associationType != null);
            Contract.Requires(databaseMapping != null);

            EdmAssociationEnd principalEnd, dependentEnd;
            if (!associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd))
            {
                if (!associationType.IsPrincipalConfigured())
                {
                    throw Error.UnableToDeterminePrincipal(
                        associationType.SourceEnd.EntityType.GetClrType(),
                        associationType.TargetEnd.EntityType.GetClrType());
                }

                principalEnd = associationType.SourceEnd;
                dependentEnd = associationType.TargetEnd;
            }

            var dependentEntityTypeMapping = GetEntityTypeMappingInHierarchy(databaseMapping, dependentEnd.EntityType);

            var dependentTable = dependentEntityTypeMapping
                .TypeMappingFragments
                .First()
                .Table;

            var associationSetMapping
                = GenerateAssociationSetMapping(
                    associationType, databaseMapping, principalEnd, dependentEnd, dependentTable);

            GenerateIndependentForeignKeyConstraint(
                databaseMapping,
                principalEnd.EntityType,
                dependentEnd.EntityType,
                dependentTable,
                associationSetMapping,
                associationSetMapping.SourceEndMapping,
                associationType.Name,
                principalEnd);

            foreach (var property in dependentEnd.EntityType.KeyProperties())
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
            EdmAssociationType associationType,
            DbDatabaseMapping databaseMapping,
            EdmAssociationEnd principalEnd,
            EdmAssociationEnd dependentEnd,
            DbTableMetadata dependentTable)
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
            EdmEntityType principalEntityType,
            EdmEntityType dependentEntityType,
            DbTableMetadata dependentTable,
            DbAssociationSetMapping associationSetMapping,
            DbAssociationEndMapping associationEndMapping,
            string name,
            EdmAssociationEnd principalEnd,
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

            var foreignKeyConstraint = new DbForeignKeyConstraintMetadata
                {
                    Name = name,
                    PrincipalTable = principalTable,
                    DeleteAction = associationEndMapping.AssociationEnd.DeleteAction.HasValue
                                       ? (DbOperationAction)
                                         associationEndMapping.AssociationEnd.DeleteAction.
                                             Value
                                       : DbOperationAction.None
                };

            var principalNavigationProperty
                = databaseMapping.Model.GetEntityTypes()
                    .SelectMany(e => e.DeclaredNavigationProperties)
                    .SingleOrDefault(n => n.ResultEnd == principalEnd);

            GenerateIndependentForeignKeyColumns(
                principalEntityType,
                dependentEntityType,
                associationSetMapping,
                associationEndMapping,
                dependentTable,
                foreignKeyConstraint,
                isPrimaryKeyColumn,
                principalNavigationProperty);

            dependentTable.ForeignKeyConstraints.Add(foreignKeyConstraint);
        }

        private void GenerateIndependentForeignKeyColumns(
            EdmEntityType principalEntityType,
            EdmEntityType dependentEntityType,
            DbAssociationSetMapping associationSetMapping,
            DbAssociationEndMapping associationEndMapping,
            DbTableMetadata dependentTable,
            DbForeignKeyConstraintMetadata foreignKeyConstraint,
            bool isPrimaryKeyColumn,
            EdmNavigationProperty principalNavigationProperty)
        {
            Contract.Requires(principalEntityType != null);
            Contract.Requires(associationEndMapping != null);
            Contract.Requires(dependentTable != null);
            Contract.Requires(foreignKeyConstraint != null);

            foreach (var property in principalEntityType.KeyProperties())
            {
                var foreignKeyColumn
                    = dependentTable.AddColumn(
                        ((principalNavigationProperty != null)
                             ? principalNavigationProperty.Name
                             : principalEntityType.Name)
                        + "_" + property.Name);

                MapTableColumn(property, foreignKeyColumn, false, isPrimaryKeyColumn);

                foreignKeyColumn.IsNullable = (associationEndMapping.AssociationEnd.IsOptional()
                                               ||
                                               (associationEndMapping.AssociationEnd.IsRequired()
                                                && dependentEntityType.BaseType != null));
                foreignKeyColumn.StoreGeneratedPattern = DbStoreGeneratedPattern.None;

                foreignKeyConstraint.DependentColumns.Add(foreignKeyColumn);

                associationEndMapping.PropertyMappings.Add(
                    new DbEdmPropertyMapping
                        {
                            Column = foreignKeyColumn,
                            PropertyPath = new[] { property }
                        });

                if (foreignKeyColumn.IsNullable)
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
