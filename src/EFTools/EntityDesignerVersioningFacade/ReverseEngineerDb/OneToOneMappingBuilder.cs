// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.Pluralization;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    internal class OneToOneMappingBuilder
    {
        private readonly string _namespaceName;
        private readonly string _containerName;
        private readonly bool _generateForeignKeyProperties;

        private readonly IPluralizationService _pluralizationService;

        public OneToOneMappingBuilder(
            string namespaceName,
            string containerName,
            IPluralizationService pluralizationService,
            bool generateForeignKeyProperties)
        {
            Debug.Assert(!string.IsNullOrEmpty(namespaceName), "namespaceName must not be null or empty string");

            _namespaceName = namespaceName;
            _containerName = containerName;
            _pluralizationService = pluralizationService;
            _generateForeignKeyProperties = generateForeignKeyProperties;
        }

        public SimpleMappingContext Build(EdmModel storeModel)
        {
            Debug.Assert(storeModel != null, "storeModel != null");

            var mappingContext = new SimpleMappingContext(storeModel, _generateForeignKeyProperties);

            var uniqueEntityContainerNames = new UniqueIdentifierService();
            var globallyUniqueTypeNames = new UniqueIdentifierService();
            CollectForeignKeyProperties(mappingContext, storeModel);

            foreach (var storeEntitySet in storeModel.Containers.Single().EntitySets)
            {
                GenerateEntitySet(mappingContext, storeEntitySet, uniqueEntityContainerNames, globallyUniqueTypeNames);
            }

            GenerateAssociationSets(
                mappingContext,
                uniqueEntityContainerNames,
                globallyUniqueTypeNames);

            var functionImports =
                GenerateFunctions(mappingContext, storeModel, uniqueEntityContainerNames, globallyUniqueTypeNames)
                    .ToArray();

            var conceptualModelContainer = EntityContainer.Create(
                _containerName,
                DataSpace.CSpace,
                mappingContext.ConceptualEntitySets()
                    .Concat(mappingContext.ConceptualAssociationSets().Cast<EntitySetBase>()),
                functionImports,
                EntityFrameworkVersion.DoubleToVersion(storeModel.SchemaVersion) >= EntityFrameworkVersion.Version2
                    ? new[] { CreateAnnotationMetadataProperty("LazyLoadingEnabled", "true") }
                    : null);

            mappingContext.AddMapping(storeModel.Containers.Single(), conceptualModelContainer);

            return mappingContext;
        }

        // internal for testing
        internal void GenerateEntitySet(
            SimpleMappingContext mappingContext,
            EntitySet storeEntitySet,
            UniqueIdentifierService uniqueEntityContainerNames,
            UniqueIdentifierService globallyUniqueTypeNames)
        {
            Debug.Assert(mappingContext != null, "mappingContext != null");
            Debug.Assert(storeEntitySet != null, "storeEntitySet != null");
            Debug.Assert(uniqueEntityContainerNames != null, "uniqueEntityContainerNames != null");
            Debug.Assert(globallyUniqueTypeNames != null, "globallyUniqueTypeNames != null");

            var conceptualEntityType = GenerateEntityType(mappingContext, storeEntitySet.ElementType, globallyUniqueTypeNames);

            var conceptualEntitySetName = CreateModelName(
                (_pluralizationService != null) ? _pluralizationService.Pluralize(storeEntitySet.Name) : storeEntitySet.Name,
                uniqueEntityContainerNames);

            var conceptualEntitySet = EntitySet.Create(conceptualEntitySetName, null, null, null, conceptualEntityType, null);

            mappingContext.AddMapping(storeEntitySet, conceptualEntitySet);
        }

        // internal for testing
        internal EntityType GenerateEntityType(
            SimpleMappingContext mappingContext, EntityType storeEntityType, UniqueIdentifierService globallyUniqueTypeNames)
        {
            Debug.Assert(mappingContext != null, "mappingContext != null");
            Debug.Assert(storeEntityType != null, "storeEntityType != null");
            Debug.Assert(globallyUniqueTypeNames != null, "globallyUniqueTypeNames != null");

            var conceptualEntityTypeName = CreateModelName(
                (_pluralizationService != null) ? _pluralizationService.Singularize(storeEntityType.Name) : storeEntityType.Name,
                globallyUniqueTypeNames);

            var uniquePropertyNameService = new UniqueIdentifierService();
            uniquePropertyNameService.AdjustIdentifier(conceptualEntityTypeName);

            var edmMembers = new List<EdmMember>();
            var keyMemberNames = new List<string>();

            foreach (var storeProperty in storeEntityType.Properties)
            {
                // cannot skip this even if the store property is foreign key and generating foreign keys is disabled 
                // since it creates property mappings the will be used when mapping association types.
                var conceptualProperty = GenerateScalarProperty(mappingContext, storeProperty, uniquePropertyNameService);

                if (_generateForeignKeyProperties
                    || !mappingContext.StoreForeignKeyProperties.Contains(storeProperty)
                    || storeEntityType.KeyMembers.Contains(storeProperty))
                {
                    edmMembers.Add(conceptualProperty);
                    if (storeEntityType.KeyMembers.Contains(storeProperty))
                    {
                        keyMemberNames.Add(conceptualProperty.Name);
                    }
                }
            }

            var conceptualEntity = EntityType.Create(
                conceptualEntityTypeName, _namespaceName, DataSpace.CSpace, keyMemberNames, edmMembers, null);

            mappingContext.AddMapping(storeEntityType, conceptualEntity);

            return conceptualEntity;
        }

        // internal for testing
        internal static EdmProperty GenerateScalarProperty(
            SimpleMappingContext mappingContext, EdmProperty storeProperty, UniqueIdentifierService uniquePropertyNameService)
        {
            Debug.Assert(storeProperty != null, "storeProperty != null");
            Debug.Assert(uniquePropertyNameService != null, "uniquePropertyNameService != null");

            var conceptualPropertyName = CreateModelName(storeProperty.Name, uniquePropertyNameService);

            var conceptualProperty =
                EdmProperty.Create(conceptualPropertyName, storeProperty.TypeUsage.ModelTypeUsage);

            if (storeProperty.StoreGeneratedPattern != StoreGeneratedPattern.None)
            {
                conceptualProperty.SetMetadataProperties(
                    new List<MetadataProperty>
                        {
                            CreateAnnotationMetadataProperty(
                                "StoreGeneratedPattern",
                                Enum.GetName(
                                    typeof(StoreGeneratedPattern),
                                    storeProperty.StoreGeneratedPattern))
                        });
            }

            mappingContext.AddMapping(storeProperty, conceptualProperty);

            return conceptualProperty;
        }

        #region Association sets

        internal void GenerateAssociationSets(
            SimpleMappingContext mappingContext,
            UniqueIdentifierService uniqueEntityContainerNames,
            UniqueIdentifierService globallyUniqueTypeNames)
        {
            IEnumerable<AssociationSet> associationSetsFromNonCollapsibleItems;

            var collapsibleItems = CollapsibleEntityAssociationSets.CreateCollapsibleItems(
                mappingContext.StoreModel.Containers.Single().BaseEntitySets,
                out associationSetsFromNonCollapsibleItems);

            foreach (var set in associationSetsFromNonCollapsibleItems)
            {
                GenerateAssociationSet(
                    mappingContext,
                    set,
                    uniqueEntityContainerNames,
                    globallyUniqueTypeNames);
            }

            foreach (var item in collapsibleItems)
            {
                GenerateAssociationSet(
                    mappingContext,
                    item,
                    uniqueEntityContainerNames,
                    globallyUniqueTypeNames);
            }
        }

        private void GenerateAssociationSet(
            SimpleMappingContext mappingContext,
            AssociationSet storeAssociationSet,
            UniqueIdentifierService uniqueEntityContainerNames,
            UniqueIdentifierService globallyUniqueTypeNames)
        {
            // We will get a value when the same association type is used for multiple association sets.
            AssociationType conceptualAssociationType;
            if (!mappingContext.TryGetValue(storeAssociationSet.ElementType, out conceptualAssociationType))
            {
                conceptualAssociationType = GenerateAssociationType(
                    mappingContext,
                    storeAssociationSet.ElementType,
                    globallyUniqueTypeNames);
            }

            Debug.Assert(storeAssociationSet.AssociationSetEnds.Count == 2);
            var storeSetEnd0 = storeAssociationSet.AssociationSetEnds[0];
            var storeSetEnd1 = storeAssociationSet.AssociationSetEnds[1];

            EntitySet conceptualEntitySet0, conceptualEntitySet1;
            mappingContext.TryGetValue(storeSetEnd0.EntitySet, out conceptualEntitySet0);
            mappingContext.TryGetValue(storeSetEnd1.EntitySet, out conceptualEntitySet1);

            var conceptualAssociationSet = AssociationSet.Create(
                CreateModelName(storeAssociationSet.Name, uniqueEntityContainerNames),
                conceptualAssociationType,
                conceptualEntitySet0,
                conceptualEntitySet1,
                null);

            Debug.Assert(conceptualAssociationSet.AssociationSetEnds.Count == 2);
            var conceptualSetEnd0 = conceptualAssociationSet.AssociationSetEnds[0];
            var conceptualSetEnd1 = conceptualAssociationSet.AssociationSetEnds[1];

            mappingContext.AddMapping(storeAssociationSet, conceptualAssociationSet);
            mappingContext.AddMapping(storeSetEnd0, conceptualSetEnd0);
            mappingContext.AddMapping(storeSetEnd1, conceptualSetEnd1);
        }

        private void GenerateAssociationSet(
            SimpleMappingContext mappingContext,
            CollapsibleEntityAssociationSets collapsibleItem,
            UniqueIdentifierService uniqueEntityContainerNames,
            UniqueIdentifierService globallyUniqueTypeNames)
        {
            var uniqueEndMemberNames = new UniqueIdentifierService(StringComparer.OrdinalIgnoreCase);

            var associationSetEndDetails0 = collapsibleItem.GetStoreAssociationSetEnd(0);

            var associationEndMember0 = GenerateAssociationEndMember(
                mappingContext,
                associationSetEndDetails0.AssociationSetEnd.CorrespondingAssociationEndMember,
                uniqueEndMemberNames,
                associationSetEndDetails0.Multiplicity,
                associationSetEndDetails0.DeleteBehavior);
            var conceptualEntitySet0 = mappingContext[associationSetEndDetails0.AssociationSetEnd.EntitySet];

            var associationSetEndDetails1 =
                collapsibleItem.GetStoreAssociationSetEnd(1);

            var associationEndMember1 = GenerateAssociationEndMember(
                mappingContext,
                associationSetEndDetails1.AssociationSetEnd.CorrespondingAssociationEndMember,
                uniqueEndMemberNames,
                associationSetEndDetails1.Multiplicity,
                associationSetEndDetails1.DeleteBehavior);
            var conceptualEntitySet1 = mappingContext[associationSetEndDetails1.AssociationSetEnd.EntitySet];

            globallyUniqueTypeNames.UnregisterIdentifier(mappingContext[collapsibleItem.EntitySet.ElementType].Name);
            uniqueEntityContainerNames.UnregisterIdentifier(mappingContext[collapsibleItem.EntitySet].Name);

            var associationTypeName = CreateModelName(collapsibleItem.EntitySet.Name, globallyUniqueTypeNames);
            var associationSetName = CreateModelName(collapsibleItem.EntitySet.Name, uniqueEntityContainerNames);

            var conceptualAssociationType = AssociationType.Create(
                associationTypeName,
                _namespaceName,
                false,
                DataSpace.CSpace,
                associationEndMember0,
                associationEndMember1,
                null, // Don't need a referential constraint.
                null);

            CreateModelNavigationProperties(conceptualAssociationType);

            var conceptualAssociationSet = AssociationSet.Create(
                associationSetName,
                conceptualAssociationType,
                conceptualEntitySet0,
                conceptualEntitySet1,
                null);

            Debug.Assert(conceptualAssociationSet.AssociationSetEnds.Count == 2);
            var conceptualSetEnd0 = conceptualAssociationSet.AssociationSetEnds[0];
            var conceptualSetEnd1 = conceptualAssociationSet.AssociationSetEnds[1];

            mappingContext.AddMapping(associationSetEndDetails0.AssociationSetEnd, conceptualSetEnd0);
            mappingContext.AddMapping(associationSetEndDetails1.AssociationSetEnd, conceptualSetEnd1);
            mappingContext.AddMapping(collapsibleItem, conceptualAssociationSet);
            mappingContext.RemoveMapping(collapsibleItem.EntitySet);
        }

        private AssociationType GenerateAssociationType(
            SimpleMappingContext mappingContext,
            AssociationType storeAssociationType,
            UniqueIdentifierService globallyUniqueTypeNames)
        {
            Debug.Assert(storeAssociationType.RelationshipEndMembers.Count == 2);

            var storeEndMember0 = (AssociationEndMember)storeAssociationType.RelationshipEndMembers[0];
            var storeEndMember1 = (AssociationEndMember)storeAssociationType.RelationshipEndMembers[1];

            var storeSchemaVersion = EntityFrameworkVersion.DoubleToVersion(mappingContext.StoreModel.SchemaVersion);
            var isFkAssociation = storeSchemaVersion > EntityFrameworkVersion.Version1
                                  && (_generateForeignKeyProperties || RequiresReferentialConstraint(storeAssociationType));

            var uniqueEndMemberNames = new UniqueIdentifierService(StringComparer.OrdinalIgnoreCase);
            var multiplicityOverride = GetMultiplicityOverride(storeAssociationType);

            var conceptualEndMember0 = GenerateAssociationEndMember(
                mappingContext,
                storeEndMember0,
                uniqueEndMemberNames,
                multiplicityOverride);

            var conceptualEndMember1 = GenerateAssociationEndMember(
                mappingContext,
                storeEndMember1,
                uniqueEndMemberNames,
                multiplicityOverride);

            var conceptualAssociationType = AssociationType.Create(
                CreateModelName(storeAssociationType.Name, globallyUniqueTypeNames),
                _namespaceName,
                isFkAssociation,
                DataSpace.CSpace,
                conceptualEndMember0,
                conceptualEndMember1,
                CreateReferentialConstraint(mappingContext, storeAssociationType),
                null);

            CreateModelNavigationProperties(conceptualAssociationType);

            mappingContext.AddMapping(storeAssociationType, conceptualAssociationType);

            return conceptualAssociationType;
        }

        private static AssociationEndMember GenerateAssociationEndMember(
            SimpleMappingContext mappingContext,
            AssociationEndMember storeEndMember,
            UniqueIdentifierService uniqueEndMemberNames,
            KeyValuePair<string, RelationshipMultiplicity> multiplicityOverride)
        {
            var multiplicity = (multiplicityOverride.Key != null && multiplicityOverride.Key == storeEndMember.Name)
                                   ? multiplicityOverride.Value
                                   : storeEndMember.RelationshipMultiplicity;

            return GenerateAssociationEndMember(
                mappingContext,
                storeEndMember,
                uniqueEndMemberNames,
                multiplicity,
                storeEndMember.DeleteBehavior);
        }

        private static AssociationEndMember GenerateAssociationEndMember(
            SimpleMappingContext mappingContext,
            AssociationEndMember storeEndMember,
            UniqueIdentifierService uniqueEndMemberNames,
            RelationshipMultiplicity multiplicity,
            OperationAction deleteBehavior)
        {
            var storeEntityType = ((EntityType)((RefType)storeEndMember.TypeUsage.EdmType).ElementType);
            var conceptualEntityType = mappingContext[storeEntityType];

            var conceptualEndMember = AssociationEndMember.Create(
                CreateModelName(storeEndMember.Name, uniqueEndMemberNames),
                conceptualEntityType.GetReferenceType(),
                multiplicity,
                deleteBehavior,
                null);

            mappingContext.AddMapping(storeEndMember, conceptualEndMember);

            return conceptualEndMember;
        }

        private KeyValuePair<string, RelationshipMultiplicity> GetMultiplicityOverride(AssociationType storeAssociationType)
        {
            var storeConstraint = storeAssociationType.ReferentialConstraints.FirstOrDefault();
            if (storeConstraint == null
                || storeConstraint.FromRole.RelationshipMultiplicity != RelationshipMultiplicity.ZeroOrOne
                || storeConstraint.ToRole.RelationshipMultiplicity != RelationshipMultiplicity.Many)
            {
                return new KeyValuePair<string, RelationshipMultiplicity>();
            }

            // For foreign key associations, having any nullable columns will imply 0..1 multiplicity, 
            // while for independent associations, all columns must be non-nullable.
            var nullableColumnsImplyingOneToOneMultiplicity = _generateForeignKeyProperties
                                                                  ? storeConstraint.ToProperties.All(p => p.Nullable == false)
                                                                  : storeConstraint.ToProperties.Any(p => p.Nullable == false);

            return nullableColumnsImplyingOneToOneMultiplicity
                       ? new KeyValuePair<string, RelationshipMultiplicity>(storeConstraint.FromRole.Name, RelationshipMultiplicity.One)
                       : new KeyValuePair<string, RelationshipMultiplicity>();
        }

        private ReferentialConstraint CreateReferentialConstraint(
            SimpleMappingContext mappingContext,
            AssociationType storeAssociationType)
        {
            Debug.Assert(storeAssociationType != null);
            Debug.Assert(storeAssociationType.ReferentialConstraints.Count <= 1);

            if (storeAssociationType.ReferentialConstraints.Count == 0)
            {
                return null;
            }

            var storeConstraint = storeAssociationType.ReferentialConstraints[0];
            Debug.Assert(
                storeConstraint.FromProperties.Count == storeConstraint.ToProperties.Count,
                "FromProperties and ToProperties have different counts.");
            Debug.Assert(storeConstraint.FromProperties.Count != 0, "No properties in the constraint, why does the constraint exist?");
            Debug.Assert(
                storeConstraint.ToProperties[0].DeclaringType.BuiltInTypeKind == BuiltInTypeKind.EntityType,
                "The property is not from an EntityType.");

            var toType = (EntityType)storeConstraint.ToProperties[0].DeclaringType;
            // If we are generating foreign keys, there is always a referential constraint. Otherwise, check
            // if the dependent end includes key properties. If so, this implies that there is a referential
            // constraint. Otherwise, it is assumed that the foreign key properties are not defined in the 
            // entity (verified ealier).
            if (!_generateForeignKeyProperties
                && !RequiresReferentialConstraint(storeConstraint, toType))
            {
                return null;
            }

            // Create the constraint.
            var count = storeConstraint.FromProperties.Count;
            var fromProperties = new EdmProperty[count];
            var toProperties = new EdmProperty[count];
            var fromRole = mappingContext[(AssociationEndMember)storeConstraint.FromRole];
            var toRole = mappingContext[(AssociationEndMember)storeConstraint.ToRole];
            for (var index = 0; index < count; index++)
            {
                fromProperties[index] = mappingContext[storeConstraint.FromProperties[index]];
                toProperties[index] = mappingContext[storeConstraint.ToProperties[index]];
            }

            return new ReferentialConstraint(
                fromRole,
                toRole,
                fromProperties,
                toProperties);
        }

        private static bool RequiresReferentialConstraint(AssociationType storeAssociationType)
        {
            if (storeAssociationType.ReferentialConstraints.Count == 0)
            {
                return false;
            }

            var storeConstraint = storeAssociationType.ReferentialConstraints[0];
            Debug.Assert(
                storeConstraint.FromProperties.Count == storeConstraint.ToProperties.Count,
                "FromProperties and ToProperties have different counts.");
            Debug.Assert(storeConstraint.FromProperties.Count != 0, "No properties in the constraint, why does the constraint exist?");
            Debug.Assert(
                storeConstraint.ToProperties[0].DeclaringType.BuiltInTypeKind == BuiltInTypeKind.EntityType,
                "The property is not from an EntityType.");

            var toType = (EntityType)storeConstraint.ToProperties[0].DeclaringType;
            return RequiresReferentialConstraint(storeConstraint, toType);
        }

        private static bool RequiresReferentialConstraint(ReferentialConstraint storeConstraint, EntityType toType)
        {
            return toType.KeyMembers.Contains(storeConstraint.ToProperties[0]);
        }

        private void CreateModelNavigationProperties(AssociationType associationType)
        {
            Debug.Assert(associationType.Members.Count == 2, "association.Members.Count == 2");

            var endMember0 = (AssociationEndMember)associationType.Members[0];
            var endMember1 = (AssociationEndMember)associationType.Members[1];

            CreateModelNavigationProperty(endMember0, endMember1);
            CreateModelNavigationProperty(endMember1, endMember0);
        }

        private void CreateModelNavigationProperty(AssociationEndMember from, AssociationEndMember to)
        {
            var entityType = (EntityType)((RefType)from.TypeUsage.EdmType).ElementType;
            var uniqueMemberNames = new UniqueIdentifierService(StringComparer.OrdinalIgnoreCase);

            LoadNameLookupWithUsedMemberNames(entityType, uniqueMemberNames);

            var name = CreateModelName(GetNavigationPropertyName(_pluralizationService, to, to.Name), uniqueMemberNames);
            var navigationProperty = NavigationProperty.Create(
                name,
                to.TypeUsage,
                (AssociationType)to.DeclaringType,
                from,
                to,
                null);

            entityType.AddNavigationProperty(navigationProperty);
        }

        private static void LoadNameLookupWithUsedMemberNames(EntityType entityType, UniqueIdentifierService uniqueMemberNames)
        {
            // A property should not have the same name as its entity
            uniqueMemberNames.RegisterUsedIdentifier(entityType.Name);
            foreach (var member in entityType.Members)
            {
                uniqueMemberNames.RegisterUsedIdentifier(member.Name);
            }
        }

        private static string GetNavigationPropertyName(IPluralizationService service, AssociationEndMember toEnd, string storeTableName)
        {
            return service == null
                       ? storeTableName
                       : toEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many
                             ? service.Pluralize(storeTableName)
                             : service.Singularize(storeTableName);
        }

        #endregion

        // internal for testing
        internal IEnumerable<EdmFunction> GenerateFunctions(
            SimpleMappingContext mappingContext,
            EdmModel storeModel,
            UniqueIdentifierService uniqueEntityContainerNames,
            UniqueIdentifierService globallyUniqueTypeNames)
        {
            Debug.Assert(mappingContext != null, "mappingContext != null");
            Debug.Assert(storeModel != null, "storeModel != null");
            Debug.Assert(uniqueEntityContainerNames != null, "uniqueEntityContainerNames != null");
            Debug.Assert(globallyUniqueTypeNames != null, "globallyUniqueTypeNames != null");

            // TODO: Note we import only TVFs here and other store functions are 
            // imported elsewhere - ideally we import all functions in one place
            // http://entityframework.codeplex.com/workitem/925
            if (EntityFrameworkVersion.DoubleToVersion(storeModel.SchemaVersion) < EntityFrameworkVersion.Version3)
            {
                yield break;
            }

            foreach (var storeFunction in storeModel.Functions)
            {
                if (storeFunction.IsComposableAttribute
                    && !storeFunction.AggregateAttribute
                    &&
                    storeFunction.Parameters.All(p => p.Mode == ParameterMode.In))
                {
                    var functionImport = GenerateFunction(
                        mappingContext,
                        storeFunction,
                        uniqueEntityContainerNames,
                        globallyUniqueTypeNames);

                    if (functionImport != null)
                    {
                        mappingContext.AddMapping(storeFunction, functionImport);

                        yield return functionImport;
                    }
                }
            }
        }

        // internal for testing
        internal EdmFunction GenerateFunction(
            SimpleMappingContext mappingContext,
            EdmFunction storeFunction,
            UniqueIdentifierService uniqueEntityContainerNames,
            UniqueIdentifierService globallyUniqueTypeNames)
        {
            Debug.Assert(mappingContext != null, "mappingContext != null");
            Debug.Assert(storeFunction != null, "storeFunction != null");
            Debug.Assert(uniqueEntityContainerNames != null, "uniqueEntityContainerNames != null");
            Debug.Assert(globallyUniqueTypeNames != null, "globallyUniqueTypeNames != null");

            var tvfReturnType = GetStoreTvfReturnType(mappingContext, storeFunction);
            if (tvfReturnType == null)
            {
                return null;
            }

            var parameters = CreateFunctionImportParameters(mappingContext, storeFunction);
            if (parameters == null)
            {
                return null;
            }

            var functionImportName = CreateModelName(storeFunction.Name, uniqueEntityContainerNames);
            var returnTypeName = CreateModelName(functionImportName + "_Result", globallyUniqueTypeNames);
            var returnParameter =
                FunctionParameter.Create(
                    "ReturnType",
                    CreateComplexTypeFromRowType(mappingContext, tvfReturnType, returnTypeName).GetCollectionType(),
                    ParameterMode.ReturnValue);

            return
                EdmFunction.Create(
                    functionImportName,
                    _namespaceName,
                    DataSpace.CSpace,
                    new EdmFunctionPayload
                        {
                            Parameters = parameters,
                            ReturnParameters = new[] { returnParameter },
                            IsComposable = true,
                            IsFunctionImport = true
                        },
                    null);
        }

        // internal for testing
        internal static FunctionParameter[] CreateFunctionImportParameters(SimpleMappingContext mappingContext, EdmFunction storeFunction)
        {
            Debug.Assert(mappingContext != null, "mappingContext != null");
            Debug.Assert(storeFunction != null, "storeFunctionParameters != null");

            var functionImportParameters = new FunctionParameter[storeFunction.Parameters.Count];

            var uniqueParameterNames = new UniqueIdentifierService();

            for (var idx = 0; idx < storeFunction.Parameters.Count; idx++)
            {
                Debug.Assert(storeFunction.Parameters[idx].Mode == ParameterMode.In, "Only In parameters are supported.");

                var parameterName = CreateModelName(storeFunction.Parameters[idx].Name, uniqueParameterNames);

                if (parameterName != storeFunction.Parameters[idx].Name)
                {
                    mappingContext.Errors.Add(
                        new EdmSchemaError(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                Resources_VersioningFacade.UnableToGenerateFunctionImportParameterName,
                                storeFunction.Parameters[idx].Name,
                                storeFunction.Name),
                            (int)ModelBuilderErrorCode.UnableToGenerateFunctionImportParameterName,
                            EdmSchemaErrorSeverity.Warning));
                    return null;
                }

                functionImportParameters[idx] =
                    FunctionParameter.Create(
                        parameterName,
                        storeFunction.Parameters[idx].TypeUsage.ModelTypeUsage.EdmType,
                        storeFunction.Parameters[idx].Mode);
            }

            return functionImportParameters;
        }

        // internal for testing
        internal static RowType GetStoreTvfReturnType(SimpleMappingContext mappingContext, EdmFunction storeFunction)
        {
            Debug.Assert(mappingContext != null, "mappingContext != null");
            Debug.Assert(storeFunction != null, "storeFunction != null");

            if (storeFunction.ReturnParameter != null)
            {
                var collectionType = storeFunction.ReturnParameter.TypeUsage.EdmType as CollectionType;

                var returnType = collectionType != null ? collectionType.TypeUsage.EdmType as RowType : null;

                if (returnType != null)
                {
                    return returnType;
                }
            }

            mappingContext.Errors.Add(
                new EdmSchemaError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.UnableToGenerateFunctionImportReturnType,
                        storeFunction.Name),
                    (int)ModelBuilderErrorCode.UnableToGenerateFunctionImportReturnType,
                    EdmSchemaErrorSeverity.Warning));

            return null;
        }

        // internal for testing
        internal ComplexType CreateComplexTypeFromRowType(SimpleMappingContext mappingContext, RowType rowType, string typeName)
        {
            Debug.Assert(mappingContext != null, "mappingContext != null");
            Debug.Assert(!string.IsNullOrEmpty(typeName), "typeName cannot be null or empty string.");
            Debug.Assert(rowType != null, "rowType != null");

            var uniquePropertyNameService = new UniqueIdentifierService();
            uniquePropertyNameService.AdjustIdentifier(typeName);

            return
                ComplexType.Create(
                    typeName,
                    _namespaceName,
                    DataSpace.CSpace,
                    rowType.Properties.Select(p => GenerateScalarProperty(mappingContext, p, uniquePropertyNameService)),
                    null);
        }

        private static void CollectForeignKeyProperties(SimpleMappingContext mappingContext, EdmModel storeModel)
        {
            var foreignKeyProperties =
                storeModel
                    .Containers.Single()
                    .BaseEntitySets.Where(storeSet => storeSet.BuiltInTypeKind == BuiltInTypeKind.AssociationSet)
                    .Select(storeSet => ((AssociationSet)storeSet).ElementType.ReferentialConstraints[0])
                    .SelectMany(constraint => constraint.ToProperties);

            foreach (var foreignKeyProperty in foreignKeyProperties)
            {
                mappingContext.StoreForeignKeyProperties.Add(foreignKeyProperty);
            }
        }

        private static MetadataProperty CreateAnnotationMetadataProperty(string name, string value)
        {
            return
                MetadataProperty.Create(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}:{1}",
                        SchemaManager.AnnotationNamespace,
                        name),
                    TypeUsage.CreateStringTypeUsage(
                        PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String),
                        isUnicode: false,
                        isFixedLength: false),
                    value);
        }

        private static string CreateModelName(string storeName, UniqueIdentifierService uniqueNameService)
        {
            Debug.Assert(!string.IsNullOrEmpty(storeName), "storeName cannot be null or empty string");
            Debug.Assert(uniqueNameService != null, "uniqueNameService != null");

            return uniqueNameService.AdjustIdentifier(CreateModelName(storeName));
        }

        private static string CreateModelName(string storeName)
        {
            Debug.Assert(!string.IsNullOrEmpty(storeName), "storeName cannot be null or empty string");

            return ModelGeneratorUtils.CreateValidEcmaName(storeName, 'C');
        }
    }
}
