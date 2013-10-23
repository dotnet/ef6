// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Linq;

    internal class DbDatabaseMappingBuilder
    {
        public static DbModel Build(SimpleMappingContext mappingContext)
        {
            Debug.Assert(mappingContext != null, "mappingContext != null");

            var databaseMapping =
                new DbDatabaseMapping
                    {
                        Database = mappingContext.StoreModel,
                        Model = BuildEntityModel(mappingContext)
                    };

            databaseMapping.AddEntityContainerMapping(BuildEntityContainerMapping(mappingContext));

            return new DbModel(databaseMapping, new DbModelBuilder());
        }

        private static EdmModel BuildEntityModel(SimpleMappingContext mappingContext)
        {
            var conceptualModelContainer = mappingContext[mappingContext.StoreModel.Containers.Single()];
            var entityModel = EdmModel.CreateConceptualModel(conceptualModelContainer, mappingContext.StoreModel.SchemaVersion);

            foreach (var entityType in mappingContext.ConceptualEntityTypes())
            {
                entityModel.AddItem(entityType);
            }

            foreach (var associationSet in mappingContext.ConceptualAssociationSets())
            {
                entityModel.AddItem(associationSet.ElementType);
            }

            foreach (var mappedStoredFunction in mappingContext.MappedStoreFunctions())
            {
                var functionImport = mappingContext[mappedStoredFunction];
                entityModel.AddItem(
                    (ComplexType)
                    ((CollectionType)functionImport.ReturnParameter.TypeUsage.EdmType).TypeUsage.EdmType);
            }

            return entityModel;
        }

        private static EntityContainerMapping BuildEntityContainerMapping(SimpleMappingContext mappingContext)
        {
            var storeEntityContainer = mappingContext.StoreModel.Containers.Single();
            var entityContainerMapping =
                new EntityContainerMapping(
                    mappingContext[storeEntityContainer],
                    storeEntityContainer,
                    null,
                    false,
                    false);

            foreach (var entitySetMapping in BuildEntitySetMappings(entityContainerMapping, mappingContext))
            {
                entityContainerMapping.AddSetMapping(entitySetMapping);
            }

            foreach (var associationSetMapping in BuildAssociationSetMappings(entityContainerMapping, mappingContext))
            {
                entityContainerMapping.AddSetMapping(associationSetMapping);
            }

            foreach (var mappedStoredFunction in mappingContext.MappedStoreFunctions())
            {
                entityContainerMapping.AddFunctionImportMapping(
                    BuildComposableFunctionMapping(mappedStoredFunction, mappingContext));
            }

            return entityContainerMapping;
        }

        // internal for testing
        internal static IEnumerable<EntitySetMapping>
            BuildEntitySetMappings(EntityContainerMapping entityContainerMapping, SimpleMappingContext mappingContext)
        {
            Debug.Assert(entityContainerMapping != null, "entityContainerMapping != null");
            Debug.Assert(mappingContext != null, "mappingContext != null");

            foreach (var storeEntitySet in mappingContext.StoreEntitySets())
            {
                var entitySetMapping = new EntitySetMapping(mappingContext[storeEntitySet], entityContainerMapping);
                entitySetMapping.AddTypeMapping(BuildEntityTypeMapping(entitySetMapping, mappingContext, storeEntitySet));
                yield return entitySetMapping;
            }
        }

        // internal for testing
        internal static EntityTypeMapping
            BuildEntityTypeMapping(EntitySetMapping storeEntitySetMapping, SimpleMappingContext mappingContext, EntitySet storeEntitySet)
        {
            Debug.Assert(storeEntitySetMapping != null, "storeEntitySetMapping != null");
            Debug.Assert(mappingContext != null, "mappingContext != null");

            var entityType = storeEntitySetMapping.EntitySet.ElementType;

            var entityTypeMapping = new EntityTypeMapping(storeEntitySetMapping);
            entityTypeMapping.AddType(entityType);

            var mappingFragment = new MappingFragment(storeEntitySet, entityTypeMapping, false);
            entityTypeMapping.AddFragment(mappingFragment);

            foreach (var propertyMapping in BuildPropertyMapping(storeEntitySet.ElementType, mappingContext))
            {
                mappingFragment.AddColumnMapping(propertyMapping);
            }

            return entityTypeMapping;
        }

        // internal for testing
        internal static IEnumerable<ColumnMappingBuilder>
            BuildPropertyMapping(EntityType storeEntityType, SimpleMappingContext mappingContext)
        {
            return storeEntityType
                .Properties
                .Where(
                    storeProperty =>
                    mappingContext.IncludeForeignKeyProperties ||
                    !mappingContext.StoreForeignKeyProperties.Contains(storeProperty) ||
                    storeEntityType.KeyMembers.Contains(storeProperty))
                .Select(
                    storeProperty => new ColumnMappingBuilder(
                                         storeProperty,
                                         new List<EdmProperty>
                                             {
                                                 mappingContext[storeProperty]
                                             }));
        }

        // internal for testing
        internal static IEnumerable<AssociationSetMapping>
            BuildAssociationSetMappings(
            EntityContainerMapping entityContainerMapping,
            SimpleMappingContext mappingContext)
        {
            Debug.Assert(entityContainerMapping != null, "entityContainerMapping != null");
            Debug.Assert(mappingContext != null, "mappingContext != null");

            var associationSetMappings = new List<AssociationSetMapping>();

            foreach (var associationSet in mappingContext.StoreAssociationSets())
            {
                var mapping = BuildAssociationSetMapping(associationSet, entityContainerMapping, mappingContext);
                if (mapping != null)
                {
                    associationSetMappings.Add(mapping);
                }
            }

            foreach (var collapsibleItem in mappingContext.CollapsedAssociationSets())
            {
                var mapping = BuildAssociationSetMapping(collapsibleItem, entityContainerMapping, mappingContext);
                if (mapping != null)
                {
                    associationSetMappings.Add(mapping);
                }
            }

            return associationSetMappings;
        }

        private static AssociationSetMapping BuildAssociationSetMapping(
            AssociationSet storeAssociationSet,
            EntityContainerMapping entityContainerMapping,
            SimpleMappingContext mappingContext)
        {
            Debug.Assert(storeAssociationSet != null, "storeAssociationSet != null");
            Debug.Assert(entityContainerMapping != null, "entityContainerMapping != null");
            Debug.Assert(mappingContext != null, "mappingContext != null");

            var modelAssociationSet = mappingContext[storeAssociationSet];
            if (modelAssociationSet.ElementType.IsForeignKey)
            {
                return null;
            }

            var foreignKeyTableEnd = GetAssociationSetEndForForeignKeyTable(storeAssociationSet);
            var associationSetMapping = new AssociationSetMapping(
                modelAssociationSet, foreignKeyTableEnd.EntitySet, entityContainerMapping);

            var count = storeAssociationSet.AssociationSetEnds.Count;
            if (count > 0)
            {
                associationSetMapping.SourceEndMapping = BuildEndPropertyMapping(
                    storeAssociationSet.AssociationSetEnds[0], mappingContext);
            }
            if (count > 1)
            {
                associationSetMapping.TargetEndMapping = BuildEndPropertyMapping(
                    storeAssociationSet.AssociationSetEnds[1], mappingContext);
            }

            var constraint = GetReferentialConstraint(storeAssociationSet);
            foreach (var foreignKeyColumn in constraint.ToProperties)
            {
                if (foreignKeyColumn.Nullable)
                {
                    associationSetMapping.AddCondition(
                        new ConditionPropertyMapping(null, foreignKeyColumn, null, false));
                }
            }

            return associationSetMapping;
        }

        private static AssociationSetMapping BuildAssociationSetMapping(
            CollapsibleEntityAssociationSets collapsibleItem,
            EntityContainerMapping entityContainerMapping,
            SimpleMappingContext mappingContext)
        {
            Debug.Assert(collapsibleItem != null, "collapsibleItem != null");
            Debug.Assert(entityContainerMapping != null, "entityContainerMapping != null");
            Debug.Assert(mappingContext != null, "mappingContext != null");

            var modelAssociationSet = mappingContext[collapsibleItem];
            if (modelAssociationSet.ElementType.IsForeignKey)
            {
                return null;
            }

            var associationSetMapping = new AssociationSetMapping(
                modelAssociationSet, collapsibleItem.EntitySet, entityContainerMapping);

            var count = collapsibleItem.AssociationSets.Count;
            if (count > 0)
            {
                associationSetMapping.SourceEndMapping = BuildEndPropertyMapping(
                    collapsibleItem.GetStoreAssociationSetEnd(0).AssociationSetEnd, mappingContext);
            }
            if (count > 1)
            {
                associationSetMapping.TargetEndMapping = BuildEndPropertyMapping(
                    collapsibleItem.GetStoreAssociationSetEnd(1).AssociationSetEnd, mappingContext);
            }

            return associationSetMapping;
        }

        private static EndPropertyMapping BuildEndPropertyMapping(
            AssociationSetEnd storeSetEnd,
            SimpleMappingContext mappingContext)
        {
            Debug.Assert(storeSetEnd != null, "storeSetEnd != null");
            Debug.Assert(mappingContext != null, "mappingContext != null");

            var endPropertyMapping =
                new EndPropertyMapping
                    {
                        AssociationEnd = mappingContext[storeSetEnd].CorrespondingAssociationEndMember
                    };

            foreach (EdmProperty storeKeyMember in storeSetEnd.EntitySet.ElementType.KeyMembers)
            {
                var modelKeyMember = mappingContext[storeKeyMember];
                var storeFkTableMember = GetAssociatedFkColumn(storeSetEnd, storeKeyMember);

                endPropertyMapping.AddProperty(
                    new ScalarPropertyMapping(modelKeyMember, storeFkTableMember));
            }

            return endPropertyMapping;
        }

        private static AssociationSetEnd GetAssociationSetEndForForeignKeyTable(AssociationSet associationSet)
        {
            var constraint = GetReferentialConstraint(associationSet);
            return associationSet.AssociationSetEnds.GetValue(constraint.ToRole.Name, false);
        }

        private static EdmProperty GetAssociatedFkColumn(AssociationSetEnd storeEnd, EdmProperty storeKeyProperty)
        {
            Debug.Assert(storeEnd != null, "storeEnd != null");
            Debug.Assert(storeKeyProperty != null, "storeKeyProperty != null");

            var constraint = GetReferentialConstraint(storeEnd.ParentAssociationSet);
            if (storeEnd.Name == constraint.FromRole.Name)
            {
                for (var i = 0; i < constraint.FromProperties.Count; i++)
                {
                    if (constraint.FromProperties[i] == storeKeyProperty)
                    {
                        return constraint.ToProperties[i];
                    }
                }
            }

            return storeKeyProperty;
        }

        private static ReferentialConstraint GetReferentialConstraint(AssociationSet associationSet)
        {
            Debug.Assert(associationSet.ElementType.ReferentialConstraints.Count == 1);

            return associationSet.ElementType.ReferentialConstraints[0];
        }

        // internal for testing
        internal static FunctionImportMappingComposable BuildComposableFunctionMapping(
            EdmFunction storeFunction, SimpleMappingContext mappingContext)
        {
            Debug.Assert(storeFunction != null, "storeFunction != null");
            Debug.Assert(mappingContext != null, "mappingContext != null");

            var functionImport = mappingContext[storeFunction];
            Debug.Assert(
                functionImport.ReturnParameter.TypeUsage.EdmType is CollectionType &&
                ((CollectionType)functionImport.ReturnParameter.TypeUsage.EdmType).TypeUsage.EdmType is ComplexType,
                "Return type should be collection of complex types");

            var returnComplexType =
                (ComplexType)((CollectionType)functionImport.ReturnParameter.TypeUsage.EdmType).TypeUsage.EdmType;

            var structuralTypeMapping =
                new Tuple<StructuralType, List<ConditionPropertyMapping>, List<PropertyMapping>>(
                    returnComplexType, new List<ConditionPropertyMapping>(), new List<PropertyMapping>());

            foreach (
                var storeProperty in
                    ((RowType)((CollectionType)storeFunction.ReturnParameter.TypeUsage.EdmType).TypeUsage.EdmType).Properties)
            {
                structuralTypeMapping.Item3.Add(new ScalarPropertyMapping(mappingContext[storeProperty], storeProperty));
            }

            return
                new FunctionImportMappingComposable(
                    functionImport,
                    storeFunction,
                    new List<Tuple<StructuralType, List<ConditionPropertyMapping>, List<PropertyMapping>>>
                        {
                            structuralTypeMapping
                        });
        }
    }
}
