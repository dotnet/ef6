// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Services
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Mapping.Update.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
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

            if (entityType.Abstract)
            {
                return;
            }

            var entitySet = databaseMapping.Model.GetEntitySet(entityType);

            Debug.Assert(entitySet != null);

            var entitySetMapping = databaseMapping.GetEntitySetMapping(entitySet);

            Debug.Assert(entitySetMapping != null);

            var columnMappings = GetColumnMappings(entityType, entitySetMapping).ToList();
            var iaFkProperties = GetIndependentFkColumns(entityType, databaseMapping).ToList();

            var insertFunctionMapping
                = GenerateFunctionMapping(
                    ModificationOperator.Insert,
                    entitySetMapping.EntitySet,
                    entityType,
                    databaseMapping,
                    entityType.Properties,
                    iaFkProperties,
                    columnMappings,
                    entityType
                        .Properties
                        .Where(p => p.HasStoreGeneratedPattern()));

            var updateFunctionMapping
                = GenerateFunctionMapping(
                    ModificationOperator.Update,
                    entitySetMapping.EntitySet,
                    entityType,
                    databaseMapping,
                    entityType.Properties,
                    iaFkProperties,
                    columnMappings,
                    entityType
                        .Properties
                        .Where(p => p.GetStoreGeneratedPattern() == StoreGeneratedPattern.Computed));

            var deleteFunctionMapping
                = GenerateFunctionMapping(
                    ModificationOperator.Delete,
                    entitySetMapping.EntitySet,
                    entityType,
                    databaseMapping,
                    entityType.Properties,
                    iaFkProperties,
                    columnMappings);

            var modificationStoredProcedureMapping
                = new StorageEntityTypeModificationFunctionMapping(
                    entityType,
                    deleteFunctionMapping,
                    insertFunctionMapping,
                    updateFunctionMapping);

            entitySetMapping.AddModificationFunctionMapping(modificationStoredProcedureMapping);
        }

        private static IEnumerable<ColumnMappingBuilder> GetColumnMappings(
            EntityType entityType, StorageEntitySetMapping entitySetMapping)
        {
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(entitySetMapping);

            return new[] { entityType }
                .Concat(GetParents(entityType))
                .SelectMany(
                    et => entitySetMapping
                              .TypeMappings
                              .Where(stm => stm.Types.Contains(et))
                              .SelectMany(stm => stm.MappingFragments)
                              .SelectMany(mf => mf.ColumnMappings));
        }

        public void Generate(StorageAssociationSetMapping associationSetMapping, DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(associationSetMapping);
            DebugCheck.NotNull(databaseMapping);

            var iaFkProperties = GetIndependentFkColumns(associationSetMapping).ToList();
            var sourceEntityType = associationSetMapping.AssociationSet.ElementType.SourceEnd.GetEntityType();
            var targetEntityType = associationSetMapping.AssociationSet.ElementType.TargetEnd.GetEntityType();
            var functionNamePrefix = sourceEntityType.Name + targetEntityType.Name;

            var insertFunctionMapping
                = GenerateFunctionMapping(
                    ModificationOperator.Insert,
                    associationSetMapping.AssociationSet,
                    associationSetMapping.AssociationSet.ElementType,
                    databaseMapping,
                    Enumerable.Empty<EdmProperty>(),
                    iaFkProperties,
                    new ColumnMappingBuilder[0],
                    functionNamePrefix: functionNamePrefix);

            var deleteFunctionMapping
                = GenerateFunctionMapping(
                    ModificationOperator.Delete,
                    associationSetMapping.AssociationSet,
                    associationSetMapping.AssociationSet.ElementType,
                    databaseMapping,
                    Enumerable.Empty<EdmProperty>(),
                    iaFkProperties,
                    new ColumnMappingBuilder[0],
                    functionNamePrefix: functionNamePrefix);

            associationSetMapping.ModificationFunctionMapping
                = new StorageAssociationSetModificationFunctionMapping(
                    associationSetMapping.AssociationSet,
                    deleteFunctionMapping,
                    insertFunctionMapping);
        }

        private static IEnumerable<Tuple<StorageModificationFunctionMemberPath, EdmProperty>> GetIndependentFkColumns(
            StorageAssociationSetMapping associationSetMapping)
        {
            DebugCheck.NotNull(associationSetMapping);

            foreach (var propertyMapping in associationSetMapping.SourceEndMapping.PropertyMappings)
            {
                yield return
                    Tuple.Create(
                        new StorageModificationFunctionMemberPath(
                            new EdmMember[] { propertyMapping.EdmProperty, associationSetMapping.SourceEndMapping.EndMember },
                            associationSetMapping.AssociationSet), propertyMapping.ColumnProperty);
            }

            foreach (var propertyMapping in associationSetMapping.TargetEndMapping.PropertyMappings)
            {
                yield return
                    Tuple.Create(
                        new StorageModificationFunctionMemberPath(
                            new EdmMember[] { propertyMapping.EdmProperty, associationSetMapping.TargetEndMapping.EndMember },
                            associationSetMapping.AssociationSet), propertyMapping.ColumnProperty);
            }
        }

        private static IEnumerable<Tuple<StorageModificationFunctionMemberPath, EdmProperty>> GetIndependentFkColumns(
            EntityType entityType, DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(databaseMapping);

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
                    || GetParents(entityType).Contains(dependentEntityType))
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
                                    associationSetMapping.AssociationSet), propertyMapping.ColumnProperty);
                    }
                }
            }
        }

        private static IEnumerable<EntityType> GetParents(EntityType entityType)
        {
            DebugCheck.NotNull(entityType);

            while (entityType.BaseType != null)
            {
                yield return (EntityType)entityType.BaseType;

                entityType = (EntityType)entityType.BaseType;
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private StorageModificationFunctionMapping GenerateFunctionMapping(
            ModificationOperator modificationOperator,
            EntitySetBase entitySetBase,
            EntityTypeBase entityTypeBase,
            DbDatabaseMapping databaseMapping,
            IEnumerable<EdmProperty> parameterProperties,
            IEnumerable<Tuple<StorageModificationFunctionMemberPath, EdmProperty>> iaFkProperties,
            IList<ColumnMappingBuilder> columnMappings,
            IEnumerable<EdmProperty> resultProperties = null,
            string functionNamePrefix = null)
        {
            DebugCheck.NotNull(entitySetBase);
            DebugCheck.NotNull(entityTypeBase);
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(parameterProperties);
            DebugCheck.NotNull(iaFkProperties);
            DebugCheck.NotNull(columnMappings);

            var useOriginalValues = modificationOperator == ModificationOperator.Delete;

            var parameterMappingGenerator
                = new FunctionParameterMappingGenerator(_providerManifest);

            var parameterBindings
                = parameterMappingGenerator
                    .Generate(
                        modificationOperator,
                        parameterProperties,
                        columnMappings,
                        new List<EdmProperty>(),
                        useOriginalValues)
                    .Concat(
                        parameterMappingGenerator
                            .Generate(iaFkProperties, useOriginalValues))
                    .ToList();

            var parameters
                = parameterBindings.Select(b => b.Parameter).ToList();

            UniquifyParameterNames(parameters);

            var functionPayload
                = new EdmFunctionPayload
                      {
                          ReturnParameters = new FunctionParameter[0],
                          Parameters = parameters.ToArray(),
                          IsComposable = false
                      };

            var function
                = databaseMapping.Database
                    .AddFunction(
                        (functionNamePrefix ?? entityTypeBase.Name) + "_" + modificationOperator.ToString(),
                        functionPayload);

            var functionMapping
                = new StorageModificationFunctionMapping(
                    entitySetBase,
                    entityTypeBase,
                    function,
                    parameterBindings,
                    null,
                    resultProperties != null
                        ? resultProperties.Select(
                            p => new StorageModificationFunctionResultBinding(
                                     columnMappings.First(cm => cm.PropertyPath.SequenceEqual(new[] { p })).ColumnProperty.Name,
                                     p))
                        : null);

            return functionMapping;
        }

        private static void UniquifyParameterNames(IList<FunctionParameter> parameters)
        {
            DebugCheck.NotNull(parameters);

            foreach (var parameter in parameters)
            {
                parameter.Name = parameters.Except(new[] { parameter }).UniquifyName(parameter.Name);
            }
        }
    }
}
