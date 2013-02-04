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
                    ModificationOperator.Insert,
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
                    ModificationOperator.Update,
                    entitySetMapping.EntitySet,
                    entityType,
                    databaseMapping,
                    entityType.Properties,
                    iaFkProperties,
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
                                    associationSetMapping.AssociationSet), propertyMapping.ColumnProperty.Name);
                    }
                }
            }
        }

        private static IEnumerable<EntityType> GetParents(EntityType entityType)
        {
            while (entityType.BaseType != null)
            {
                yield return (EntityType)entityType.BaseType;

                entityType = (EntityType)entityType.BaseType;
            }
        }

        private StorageModificationFunctionMapping GenerateFunctionMapping(
            ModificationOperator modificationOperator,
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
                        modificationOperator,
                        parameterProperties,
                        new List<EdmProperty>(),
                        useOriginalValues)
                    .Concat(
                        parameterMappingGenerator
                            .Generate(iaFkProperties, useOriginalValues))
                    .ToList();

            FunctionParameter rowsAffectedParameter = null;

            var parameters
                = parameterBindings.Select(b => b.Parameter);

            if (parameterBindings
                .Any(
                    pb => !pb.IsCurrent
                          && pb.MemberPath.AssociationSetEnd == null
                          && ((EdmProperty)pb.MemberPath.Members.Last()).ConcurrencyMode == ConcurrencyMode.Fixed))
            {
                rowsAffectedParameter
                    = new FunctionParameter(
                        "RowsAffected",
                        _providerManifest.GetStoreType(
                            TypeUsage.CreateDefaultTypeUsage(
                                PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32))),
                        ParameterMode.Out);

                parameters = parameters.Concat(new[] { rowsAffectedParameter });
            }

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
                                     entityType.Name + "_" + modificationOperator.ToString(),
                                     functionPayload);

            var functionMapping
                = new StorageModificationFunctionMapping(
                    entitySet,
                    entityType,
                    function,
                    parameterBindings,
                    rowsAffectedParameter,
                    resultProperties != null
                        ? resultProperties.Select(p => new StorageModificationFunctionResultBinding(p.Name, p))
                        : null);

            return functionMapping;
        }
    }
}
