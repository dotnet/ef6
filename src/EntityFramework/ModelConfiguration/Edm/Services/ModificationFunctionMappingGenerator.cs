// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Services
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    internal class ModificationFunctionMappingGenerator : StructuralTypeMappingGenerator
    {
        public ModificationFunctionMappingGenerator(DbProviderManifest providerManifest)
            : base(providerManifest)
        {
        }

        public void Generate(EntityType entityType, DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(databaseMapping);

            var entitySet = databaseMapping.Model.GetEntitySet(entityType);

            Debug.Assert(entitySet != null);

            var entitySetMapping = databaseMapping.GetEntitySetMapping(entitySet);

            Debug.Assert(entitySetMapping != null);

            var iaFkProperties = GetIndependentFkColumns(entityType, databaseMapping).ToList();

            var insertFunctionMapping
                = GenerateFunctionMapping(
                    "Insert" + entityType.Name,
                    entitySetMapping.EntitySet,
                    entityType,
                    databaseMapping,
                    entityType
                        .Properties
                        .Where(p => !p.HasStoreGeneratedPattern()),
                    iaFkProperties,
                    entityType
                        .Properties
                        .Where(p => p.HasStoreGeneratedPattern()));

            var updateFunctionMapping
                = GenerateFunctionMapping(
                    "Update" + entityType.Name,
                    entitySetMapping.EntitySet,
                    entityType,
                    databaseMapping,
                    entityType
                        .Properties
                        .Where(p => p.GetStoreGeneratedPattern() != StoreGeneratedPattern.Computed),
                    iaFkProperties,
                    entityType
                        .Properties
                        .Where(p => p.GetStoreGeneratedPattern() == StoreGeneratedPattern.Computed));

            var deleteFunctionMapping
                = GenerateFunctionMapping(
                    "Delete" + entityType.Name,
                    entitySetMapping.EntitySet,
                    entityType,
                    databaseMapping,
                    entityType.KeyProperties(),
                    iaFkProperties,
                    useOriginalValues: true);

            var modificationFunctionMapping
                = new StorageEntityTypeModificationFunctionMapping(
                    entityType,
                    deleteFunctionMapping,
                    insertFunctionMapping,
                    updateFunctionMapping);

            entitySetMapping.AddModificationFunctionMapping(modificationFunctionMapping);
        }

        private static IEnumerable<Tuple<StorageModificationFunctionMemberPath, string>> GetIndependentFkColumns(
            EntityType entityType, DbDatabaseMapping databaseMapping)
        {
            foreach (var associationSetMapping in databaseMapping.GetAssociationSetMappings())
            {
                var associationType = associationSetMapping.AssociationSet.ElementType;

                if (associationType.IsManyToMany())
                {
                    continue;
                }

                AssociationEndMember _, dependentEnd;
                if (!associationType.TryGuessPrincipalAndDependentEnds(out _, out dependentEnd))
                {
                    dependentEnd = associationType.TargetEnd;
                }

                var dependentEntityType = dependentEnd.GetEntityType();

                if (dependentEntityType == entityType
                    || GetAbstractParents(entityType).Contains(dependentEntityType))
                {
                    var endPropertyMapping
                        = associationSetMapping.TargetEndMapping.EndMember != dependentEnd
                              ? associationSetMapping.TargetEndMapping
                              : associationSetMapping.SourceEndMapping;

                    foreach (var propertyMapping in endPropertyMapping.PropertyMappings)
                    {
                        yield return
                            Tuple.Create(
                                new StorageModificationFunctionMemberPath(
                                    new EdmMember[] { propertyMapping.EdmProperty, dependentEnd },
                                    associationSetMapping.AssociationSet), propertyMapping.ColumnProperty.Name);
                    }
                }
            }
        }

        private static IEnumerable<EntityType> GetAbstractParents(EntityType entityType)
        {
            while (entityType.BaseType != null
                   /*&& entityType.BaseType.Abstract*/)
            {
                yield return (EntityType)entityType.BaseType;

                entityType = (EntityType)entityType.BaseType;
            }
        }

        private StorageModificationFunctionMapping GenerateFunctionMapping(
            string functionName,
            EntitySet entitySet,
            EntityType entityType,
            DbDatabaseMapping databaseMapping,
            IEnumerable<EdmProperty> parameterProperties,
            IEnumerable<Tuple<StorageModificationFunctionMemberPath, string>> iaFkProperties,
            IEnumerable<EdmProperty> resultProperties = null,
            bool useOriginalValues = false)
        {
            var parameterMappingGenerator
                = new FunctionParameterMappingGenerator(_providerManifest);

            var parameterBindings
                = parameterMappingGenerator
                    .Generate(
                        parameterProperties,
                        new List<EdmProperty>(),
                        useOriginalValues)
                    .Concat(
                        parameterMappingGenerator
                            .Generate(iaFkProperties, useOriginalValues))
                    .ToList();

            var functionPayload
                = new EdmFunctionPayload
                      {
                          ReturnParameters = new FunctionParameter[0],
                          Parameters = parameterBindings.Select(b => b.Parameter).ToArray(),
                          IsComposable = false
                      };

            var function = databaseMapping.Database.AddFunction(functionName, functionPayload);

            var functionMapping
                = new StorageModificationFunctionMapping(
                    entitySet,
                    entityType,
                    function,
                    parameterBindings,
                    null,
                    resultProperties != null
                        ? resultProperties.Select(p => new StorageModificationFunctionResultBinding(p.Name, p))
                        : null);

            return functionMapping;
        }
    }
}
