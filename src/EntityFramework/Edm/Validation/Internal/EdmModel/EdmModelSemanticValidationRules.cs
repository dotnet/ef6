// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation.Internal.EdmModel
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.Edm.Internal;
    using System.Data.Entity.Edm.Parsing.Xml.Internal;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Csdl;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using EdmConstants = System.Data.Entity.Edm.Internal.EdmConstants;

    [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
    internal static class EdmModelSemanticValidationRules
    {
        internal static readonly EdmModelValidationRule<EdmModel> EdmModel_SystemNamespaceEncountered =
            new EdmModelValidationRule<EdmModel>(
                (context, edmModel) =>
                    {
                        foreach (var namespaceItem in edmModel.Namespaces)
                        {
                            if (DataModelValidationHelper.IsEdmSystemNamespace(namespaceItem.Name))
                            {
                                context.AddError(
                                    namespaceItem,
                                    null,
                                    Strings.EdmModel_Validator_Semantic_SystemNamespaceEncountered(namespaceItem.Name),
                                    XmlErrorCode.NeedNotUseSystemNamespaceInUsing);
                            }
                        }
                    });

        internal static readonly EdmModelValidationRule<EntityContainer> EdmEntityContainer_SimilarRelationshipEnd =
            new EdmModelValidationRule<EntityContainer>(
                (context, edmEntityContainer) =>
                    {
                        var sourceEndList =
                            new List<KeyValuePair<AssociationSet, EntitySet>>();
                        var targetEndList =
                            new List<KeyValuePair<AssociationSet, EntitySet>>();
                        foreach (var set in edmEntityContainer.AssociationSets)
                        {
                            var sourceEnd =
                                new KeyValuePair<AssociationSet, EntitySet>(set, set.SourceSet);
                            var targetEnd =
                                new KeyValuePair<AssociationSet, EntitySet>(set, set.TargetSet);

                            var existSourceEnd =
                                sourceEndList.FirstOrDefault(
                                    e => DataModelValidationHelper.AreRelationshipEndsEqual(e, sourceEnd));
                            var existTargetEnd =
                                targetEndList.FirstOrDefault(
                                    e => DataModelValidationHelper.AreRelationshipEndsEqual(e, targetEnd));

                            if (!existSourceEnd.Equals(default(KeyValuePair<AssociationSet, EntitySet>)))
                            {
                                context.AddError(
                                    edmEntityContainer,
                                    null,
                                    Strings.EdmModel_Validator_Semantic_SimilarRelationshipEnd(
                                        existSourceEnd.Key.ElementType.SourceEnd.Name,
                                        existSourceEnd.Key.Name,
                                        set.Name,
                                        existSourceEnd.Value.Name,
                                        edmEntityContainer.Name),
                                    XmlErrorCode.SimilarRelationshipEnd);
                            }
                            else
                            {
                                sourceEndList.Add(sourceEnd);
                            }

                            if (!existTargetEnd.Equals(default(KeyValuePair<AssociationSet, EntitySet>)))
                            {
                                context.AddError(
                                    edmEntityContainer,
                                    null,
                                    Strings.EdmModel_Validator_Semantic_SimilarRelationshipEnd(
                                        existTargetEnd.Key.ElementType.TargetEnd.Name,
                                        existTargetEnd.Key.Name,
                                        set.Name,
                                        existTargetEnd.Value.Name,
                                        edmEntityContainer.Name),
                                    XmlErrorCode.SimilarRelationshipEnd);
                            }
                            else
                            {
                                targetEndList.Add(targetEnd);
                            }
                        }
                    });

        internal static readonly EdmModelValidationRule<EntityContainer>
            EdmEntityContainer_InvalidEntitySetNameReference =
                new EdmModelValidationRule<EntityContainer>(
                    (context, edmEntityContainer) =>
                        {
                            if (edmEntityContainer.AssociationSets != null)
                            {
                                foreach (var associationSet in edmEntityContainer.AssociationSets)
                                {
                                    if (associationSet.SourceSet != null && associationSet.ElementType != null
                                        && associationSet.ElementType.SourceEnd != null)
                                    {
                                        if (!edmEntityContainer.EntitySets.Contains(associationSet.SourceSet))
                                        {
                                            context.AddError(
                                                associationSet.SourceSet,
                                                null,
                                                Strings.EdmModel_Validator_Semantic_InvalidEntitySetNameReference(
                                                    associationSet.SourceSet.Name,
                                                    associationSet.ElementType.SourceEnd.Name),
                                                XmlErrorCode.InvalidEndEntitySet);
                                        }
                                    }

                                    if (associationSet.TargetSet != null && associationSet.ElementType != null
                                        && associationSet.ElementType.TargetEnd != null)
                                    {
                                        if (!edmEntityContainer.EntitySets.Contains(associationSet.TargetSet))
                                        {
                                            context.AddError(
                                                associationSet.TargetSet,
                                                null,
                                                Strings.EdmModel_Validator_Semantic_InvalidEntitySetNameReference(
                                                    associationSet.TargetSet.Name,
                                                    associationSet.ElementType.TargetEnd.Name),
                                                XmlErrorCode.InvalidEndEntitySet);
                                        }
                                    }
                                }
                            }
                        });

        internal static readonly EdmModelValidationRule<EntityContainer>
            EdmEntityContainer_ConcurrencyRedefinedOnSubTypeOfEntitySetType
                =
                new EdmModelValidationRule<EntityContainer>(
                    (context, edmEntityContainer) =>
                        {
                            var baseEntitySetTypes = new Dictionary<EntityType, EntitySet>();
                            foreach (var entitySet in edmEntityContainer.EntitySets)
                            {
                                if (entitySet != null && entitySet.ElementType != null
                                    && !baseEntitySetTypes.ContainsKey(entitySet.ElementType))
                                {
                                    baseEntitySetTypes.Add(entitySet.ElementType, entitySet);
                                }
                            }

                            // look through each type in this schema and see if it is derived from a base
                            // type if it is then see if it has some "new" Concurrency fields
                            foreach (var entityType in context.ModelParentMap.NamespaceItems.OfType<EntityType>())
                            {
                                EntitySet set;
                                if (DataModelValidationHelper.TypeIsSubTypeOf(entityType, baseEntitySetTypes, out set)
                                    &&
                                    DataModelValidationHelper.IsTypeDefinesNewConcurrencyProperties(entityType))
                                {
                                    context.AddError(
                                        entityType,
                                        null,
                                        Strings.EdmModel_Validator_Semantic_ConcurrencyRedefinedOnSubTypeOfEntitySetType
                                            (
                                                entityType.GetQualifiedName(context.GetQualifiedPrefix(entityType)),
                                                set.ElementType.GetQualifiedName(
                                                    context.GetQualifiedPrefix(set.ElementType)),
                                                set.GetQualifiedName(context.GetQualifiedPrefix(set))),
                                        XmlErrorCode.ConcurrencyRedefinedOnSubTypeOfEntitySetType);
                                }
                            }
                        });

        internal static readonly EdmModelValidationRule<EntityContainer>
            EdmEntityContainer_DuplicateEntityContainerMemberName =
                new EdmModelValidationRule<EntityContainer>(
                    (context, edmEntityContainer) =>
                        {
                            var memberNameList = new HashSet<string>();
                            foreach (var item in edmEntityContainer.BaseEntitySets)
                            {
                                DataModelValidationHelper.AddMemberNameToHashSet(
                                    item,
                                    memberNameList,
                                    context,
                                    Strings.EdmModel_Validator_Semantic_DuplicateEntityContainerMemberName);
                            }
                        }
                    );

        internal static readonly EdmModelValidationRule<EntitySet> EdmEntitySet_EntitySetTypeHasNoKeys =
            new EdmModelValidationRule<EntitySet>(
                (context, edmEntitySet) =>
                    {
                        if (edmEntitySet.ElementType != null)
                        {
                            if (!edmEntitySet.ElementType.GetValidKey().Any())
                            {
                                context.AddError(
                                    edmEntitySet,
                                    CsdlConstants.Attribute_EntityType,
                                    Strings.EdmModel_Validator_Semantic_EntitySetTypeHasNoKeys(
                                        edmEntitySet.Name, edmEntitySet.ElementType.Name),
                                    XmlErrorCode.EntitySetTypeHasNoKeys);
                            }
                        }
                    });

        internal static readonly EdmModelValidationRule<AssociationSet> EdmAssociationSet_DuplicateEndName =
            new EdmModelValidationRule<AssociationSet>(
                (context, edmAssociationSet) =>
                    {
                        if (edmAssociationSet.ElementType != null && edmAssociationSet.ElementType.SourceEnd != null
                            && edmAssociationSet.ElementType.TargetEnd != null)
                        {
                            if (edmAssociationSet.ElementType.SourceEnd.Name
                                == edmAssociationSet.ElementType.TargetEnd.Name)
                            {
                                context.AddError(
                                    edmAssociationSet.SourceSet,
                                    CsdlConstants.Attribute_Name,
                                    Strings.EdmModel_Validator_Semantic_DuplicateEndName(
                                        edmAssociationSet.ElementType.SourceEnd.Name),
                                    XmlErrorCode.InvalidName);
                            }
                        }
                    });

        internal static readonly EdmModelValidationRule<EntityType>
            EdmEntityType_DuplicatePropertyNameSpecifiedInEntityKey =
                new EdmModelValidationRule<EntityType>(
                    (context, edmEntityType) =>
                        {
                            var keyProperties = edmEntityType.GetKeyProperties().ToList();
                            if (keyProperties.Count > 0)
                            {
                                var visitedKeyProperties = new List<EdmProperty>();
                                foreach (var key in keyProperties)
                                {
                                    if (key != null)
                                    {
                                        if (!visitedKeyProperties.Contains(key))
                                        {
                                            if (keyProperties.Count(p => key.Equals(p)) > 1)
                                            {
                                                context.AddError(
                                                    key,
                                                    null,
                                                    Strings.
                                                        EdmModel_Validator_Semantic_DuplicatePropertyNameSpecifiedInEntityKey
                                                        (
                                                            edmEntityType.Name, key.Name),
                                                    XmlErrorCode.DuplicatePropertySpecifiedInEntityKey);
                                            }
                                            visitedKeyProperties.Add(key);
                                        }
                                    }
                                }
                            }
                        });

        internal static readonly EdmModelValidationRule<EntityType> EdmEntityType_InvalidKeyNullablePart =
            new EdmModelValidationRule<EntityType>(
                (context, edmEntityType) =>
                    {
                        foreach (var key in edmEntityType.GetValidKey())
                        {
                            if (key.IsPrimitiveType)
                            {
                                if (key.Nullable)
                                {
                                    context.AddError(
                                        key,
                                        CsdlConstants.Attribute_Nullable,
                                        Strings.EdmModel_Validator_Semantic_InvalidKeyNullablePart(
                                            key.Name, edmEntityType.Name),
                                        XmlErrorCode.InvalidKey);
                                }
                            }
                        }
                    });

        internal static readonly EdmModelValidationRule<EntityType> EdmEntityType_EntityKeyMustBeScalar =
            new EdmModelValidationRule<EntityType>(
                (context, edmEntityType) =>
                    {
                        foreach (var key in edmEntityType.GetValidKey())
                        {
                            if (!key.IsUnderlyingPrimitiveType)
                            {
                                context.AddError(
                                    key,
                                    null,
                                    Strings.EdmModel_Validator_Semantic_EntityKeyMustBeScalar(
                                        edmEntityType.Name, key.Name),
                                    XmlErrorCode.EntityKeyMustBeScalar);
                            }
                        }
                    });

        internal static readonly EdmModelValidationRule<EntityType> EdmEntityType_InvalidKeyKeyDefinedInBaseClass =
            new EdmModelValidationRule<EntityType>(
                (context, edmEntityType) =>
                    {
                        if (edmEntityType.BaseType != null &&
                            edmEntityType.DeclaredKeyProperties != null
                            && edmEntityType.DeclaredKeyProperties.Any())
                        {
                            context.AddError(
                                edmEntityType.BaseType,
                                null,
                                Strings.EdmModel_Validator_Semantic_InvalidKeyKeyDefinedInBaseClass(
                                    edmEntityType.Name, edmEntityType.BaseType.Name),
                                XmlErrorCode.InvalidKey);
                        }
                    });

        internal static readonly EdmModelValidationRule<EntityType> EdmEntityType_KeyMissingOnEntityType =
            new EdmModelValidationRule<EntityType>(
                (context, edmEntityType) =>
                    {
                        if (edmEntityType.BaseType == null
                            && !edmEntityType.DeclaredKeyProperties.Any())
                        {
                            context.AddError(
                                edmEntityType,
                                null,
                                Strings.EdmModel_Validator_Semantic_KeyMissingOnEntityType(edmEntityType.Name),
                                XmlErrorCode.KeyMissingOnEntityType);
                        }
                    });

        internal static readonly EdmModelValidationRule<EntityType> EdmEntityType_InvalidMemberNameMatchesTypeName =
            new EdmModelValidationRule<EntityType>(
                (context, edmEntityType) =>
                    {
                        var properties = edmEntityType.Properties.ToList();
                        if (edmEntityType.Name.HasContent()
                            && properties.Count > 0)
                        {
                            foreach (var property in properties)
                            {
                                if (property != null)
                                {
                                    if (property.Name.EqualsOrdinal(edmEntityType.Name))
                                    {
                                        context.AddError(
                                            property,
                                            CsdlConstants.Attribute_Name,
                                            Strings.EdmModel_Validator_Semantic_InvalidMemberNameMatchesTypeName(
                                                property.Name,
                                                edmEntityType.GetQualifiedName(
                                                    context.GetQualifiedPrefix(edmEntityType))),
                                            XmlErrorCode.BadProperty);
                                    }
                                }
                            }

                            if (edmEntityType.DeclaredNavigationProperties.Any())
                            {
                                foreach (var property in edmEntityType.DeclaredNavigationProperties)
                                {
                                    if (property != null)
                                    {
                                        if (property.Name.EqualsOrdinal(edmEntityType.Name))
                                        {
                                            context.AddError(
                                                property,
                                                CsdlConstants.Attribute_Name,
                                                Strings.EdmModel_Validator_Semantic_InvalidMemberNameMatchesTypeName(
                                                    property.Name,
                                                    edmEntityType.GetQualifiedName(
                                                        context.GetQualifiedPrefix(edmEntityType))),
                                                XmlErrorCode.BadProperty);
                                        }
                                    }
                                }
                            }
                        }
                    });

        internal static readonly EdmModelValidationRule<EntityType> EdmEntityType_PropertyNameAlreadyDefinedDuplicate
            =
            new EdmModelValidationRule<EntityType>(
                (context, edmEntityType) =>
                    {
                        var propertyNames = new HashSet<string>();
                        foreach (var property in edmEntityType.Properties)
                        {
                            if (property != null)
                            {
                                if (property.Name.HasContent())
                                {
                                    DataModelValidationHelper.AddMemberNameToHashSet(
                                        property,
                                        propertyNames,
                                        context,
                                        Strings.EdmModel_Validator_Semantic_PropertyNameAlreadyDefinedDuplicate);
                                }
                            }
                        }

                        if (edmEntityType.DeclaredNavigationProperties.Any())
                        {
                            foreach (var property in edmEntityType.DeclaredNavigationProperties)
                            {
                                if (property != null)
                                {
                                    if (property.Name.HasContent())
                                    {
                                        DataModelValidationHelper.AddMemberNameToHashSet(
                                            property,
                                            propertyNames,
                                            context,
                                            Strings.EdmModel_Validator_Semantic_PropertyNameAlreadyDefinedDuplicate);
                                    }
                                }
                            }
                        }
                    });

        internal static readonly EdmModelValidationRule<EntityType> EdmEntityType_CycleInTypeHierarchy =
            new EdmModelValidationRule<EntityType>(
                (context, edmEntityType) =>
                    {
                        if (DataModelValidationHelper.CheckForInheritanceCycle(edmEntityType, et => (EntityType)et.BaseType))
                        {
                            context.AddError(
                                edmEntityType,
                                CsdlConstants.Attribute_BaseType,
                                Strings.EdmModel_Validator_Semantic_CycleInTypeHierarchy(
                                    edmEntityType.GetQualifiedName(context.GetQualifiedPrefix(edmEntityType))),
                                XmlErrorCode.CycleInTypeHierarchy);
                        }
                    });

        internal static readonly EdmModelValidationRule<NavigationProperty>
            EdmNavigationProperty_BadNavigationPropertyUndefinedRole =
                new EdmModelValidationRule<NavigationProperty>(
                    (context, edmNavigationProperty) =>
                        {
                            if (edmNavigationProperty.Association != null &&
                                edmNavigationProperty.Association.SourceEnd != null &&
                                edmNavigationProperty.Association.TargetEnd != null &&
                                edmNavigationProperty.Association.SourceEnd.Name != null
                                &&
                                edmNavigationProperty.Association.TargetEnd.Name != null)
                            {
                                if (edmNavigationProperty.ResultEnd != edmNavigationProperty.Association.SourceEnd
                                    &&
                                    edmNavigationProperty.ResultEnd != edmNavigationProperty.Association.TargetEnd)
                                {
                                    context.AddError(
                                        edmNavigationProperty,
                                        null,
                                        Strings.EdmModel_Validator_Semantic_BadNavigationPropertyUndefinedRole(
                                            edmNavigationProperty.Association.SourceEnd.Name,
                                            edmNavigationProperty.Association.TargetEnd.Name,
                                            edmNavigationProperty.Association.Name),
                                        XmlErrorCode.BadNavigationProperty);
                                }
                            }
                        });

        internal static readonly EdmModelValidationRule<NavigationProperty>
            EdmNavigationProperty_BadNavigationPropertyRolesCannotBeTheSame =
                new EdmModelValidationRule<NavigationProperty>(
                    (context, edmNavigationProperty) =>
                        {
                            if (edmNavigationProperty.Association != null &&
                                edmNavigationProperty.Association.SourceEnd != null
                                &&
                                edmNavigationProperty.Association.TargetEnd != null)
                            {
                                if (edmNavigationProperty.ResultEnd
                                    == edmNavigationProperty.GetFromEnd())
                                {
                                    context.AddError(
                                        edmNavigationProperty,
                                        CsdlConstants.Attribute_ToRole,
                                        Strings.EdmModel_Validator_Semantic_BadNavigationPropertyRolesCannotBeTheSame,
                                        XmlErrorCode.BadNavigationProperty);
                                }
                            }
                        });

        internal static readonly EdmModelValidationRule<AssociationType>
            EdmAssociationType_InvalidOperationMultipleEndsInAssociation =
                new EdmModelValidationRule<AssociationType>(
                    (context, edmAssociationType) =>
                        {
                            if ((edmAssociationType.SourceEnd != null
                                 && edmAssociationType.SourceEnd.DeleteBehavior != OperationAction.None)
                                &&
                                (edmAssociationType.TargetEnd != null
                                 && edmAssociationType.TargetEnd.DeleteBehavior != OperationAction.None))
                            {
                                context.AddError(
                                    edmAssociationType,
                                    null,
                                    Strings.EdmModel_Validator_Semantic_InvalidOperationMultipleEndsInAssociation,
                                    XmlErrorCode.InvalidOperation);
                            }
                        });

        internal static readonly EdmModelValidationRule<AssociationType>
            EdmAssociationType_EndWithManyMultiplicityCannotHaveOperationsSpecified =
                new EdmModelValidationRule<AssociationType>(
                    (context, edmAssociationType) =>
                        {
                            if (edmAssociationType.SourceEnd != null)
                            {
                                // Check if the end has multiplicity as many, it cannot have any operation behaviour
                                if (edmAssociationType.SourceEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many
                                    && edmAssociationType.SourceEnd.DeleteBehavior != OperationAction.None)
                                {
                                    context.AddError(
                                        edmAssociationType.SourceEnd,
                                        CsdlConstants.Element_OnDelete,
                                        Strings.
                                            EdmModel_Validator_Semantic_EndWithManyMultiplicityCannotHaveOperationsSpecified
                                            (
                                                edmAssociationType.SourceEnd.Name,
                                                edmAssociationType.Name),
                                        XmlErrorCode.EndWithManyMultiplicityCannotHaveOperationsSpecified);
                                }
                            }

                            if (edmAssociationType.TargetEnd != null)
                            {
                                if (edmAssociationType.TargetEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many
                                    && edmAssociationType.TargetEnd.DeleteBehavior != OperationAction.None)
                                {
                                    context.AddError(
                                        edmAssociationType.TargetEnd,
                                        CsdlConstants.Element_OnDelete,
                                        Strings.
                                            EdmModel_Validator_Semantic_EndWithManyMultiplicityCannotHaveOperationsSpecified
                                            (
                                                edmAssociationType.TargetEnd.Name,
                                                edmAssociationType.Name),
                                        XmlErrorCode.EndWithManyMultiplicityCannotHaveOperationsSpecified);
                                }
                            }
                        });

        internal static readonly EdmModelValidationRule<AssociationType>
            EdmAssociationType_EndNameAlreadyDefinedDuplicate =
                new EdmModelValidationRule<AssociationType>(
                    (context, edmAssociationType) =>
                        {
                            if (edmAssociationType.SourceEnd != null
                                && edmAssociationType.TargetEnd != null)
                            {
                                if (edmAssociationType.SourceEnd.Name
                                    == edmAssociationType.TargetEnd.Name)
                                {
                                    context.AddError(
                                        edmAssociationType.SourceEnd,
                                        CsdlConstants.Attribute_Name,
                                        Strings.EdmModel_Validator_Semantic_EndNameAlreadyDefinedDuplicate(
                                            edmAssociationType.SourceEnd.Name),
                                        XmlErrorCode.AlreadyDefined);
                                }
                            }
                        });

        internal static readonly EdmModelValidationRule<AssociationType>
            EdmAssociationType_SameRoleReferredInReferentialConstraint =
                new EdmModelValidationRule<AssociationType>(
                    (context, edmAssociationType) =>
                        {
                            if (DataModelValidationHelper.IsReferentialConstraintReadyForValidation(edmAssociationType))
                            {
                                // this also includes the scenario if the Principal and Dependent are pointing to the same AssociationEndMember
                                if (edmAssociationType.Constraint.PrincipalEnd(edmAssociationType).Name
                                    ==
                                    edmAssociationType.Constraint.DependentEnd.Name)
                                {
                                    context.AddError(
                                        edmAssociationType.Constraint.DependentEnd,
                                        null,
                                        Strings.EdmModel_Validator_Semantic_SameRoleReferredInReferentialConstraint(
                                            edmAssociationType.Name),
                                        XmlErrorCode.SameRoleReferredInReferentialConstraint);
                                }
                            }
                        });

        internal static readonly EdmModelValidationRule<AssociationType>
            EdmAssociationType_ValidateReferentialConstraint =
                new EdmModelValidationRule<AssociationType>(
                    (context, edmAssociationType) =>
                        {
                            if (DataModelValidationHelper.IsReferentialConstraintReadyForValidation(edmAssociationType))
                            {
                                var constraint = edmAssociationType.Constraint;

                                // Validate the to end and from end of the referential constraint
                                var principalRoleEnd = constraint.PrincipalEnd(edmAssociationType);
                                var dependentRoleEnd = constraint.DependentEnd;

                                bool isPrincipalRoleKeyProperty, isDependentRoleKeyProperty;
                                bool areAllPrinicipalRolePropertiesNullable, areAllDependentRolePropertiesNullable;
                                bool isDependentRolePropertiesSubsetofKeyProperties,
                                     isPrinicipalRolePropertiesSubsetofKeyProperties;
                                bool isAnyPrinicipalRolePropertyNullable, isAnyDependentRolePropertyNullable;

                                // Resolve all the property in the dependent end attribute. Also checks whether this is nullable or not and 
                                // whether the properties are the keys for the type in the dependent end
                                DataModelValidationHelper.IsKeyProperty(
                                    constraint.ToProperties.ToList(),
                                    dependentRoleEnd,
                                    out isPrincipalRoleKeyProperty,
                                    out areAllDependentRolePropertiesNullable,
                                    out isAnyDependentRolePropertyNullable,
                                    out isDependentRolePropertiesSubsetofKeyProperties);

                                // Resolve all the property in the principal end attribute. Also checks whether this is nullable or not and 
                                // whether the properties are the keys for the type in the principal role
                                DataModelValidationHelper.IsKeyProperty(
                                    constraint.PrincipalEnd(edmAssociationType).GetEntityType().GetValidKey().ToList(),
                                    principalRoleEnd,
                                    out isDependentRoleKeyProperty,
                                    out areAllPrinicipalRolePropertiesNullable,
                                    out isAnyPrinicipalRolePropertyNullable,
                                    out isPrinicipalRolePropertiesSubsetofKeyProperties);

                                Contract.Assert(
                                    constraint.PrincipalEnd(edmAssociationType).GetEntityType().GetValidKey().Any(),
                                    "There should be some ref properties in Principal Role");
                                Contract.Assert(
                                    constraint.ToProperties.Count() != 0,
                                    "There should be some ref properties in Dependent Role");
                                Contract.Assert(
                                    isDependentRoleKeyProperty,
                                    "The properties in the PrincipalRole must be the key of the Entity type referred to by the principal role");

                                var v1Behavior = context.ValidationContextVersion <= DataModelVersions.Version1_1;

                                // Since the FromProperty must be the key of the FromRole, the FromRole cannot be '*' as multiplicity
                                // Also the lower bound of multiplicity of FromRole can be zero if and only if all the properties in 
                                // ToProperties are nullable
                                // for v2+
                                if (principalRoleEnd.RelationshipMultiplicity
                                    == RelationshipMultiplicity.Many)
                                {
                                    context.AddError(
                                        principalRoleEnd,
                                        null,
                                        Strings.
                                            EdmModel_Validator_Semantic_InvalidMultiplicityFromRoleUpperBoundMustBeOne(
                                                principalRoleEnd.Name, edmAssociationType.Name),
                                        XmlErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint);
                                }
                                else if (areAllDependentRolePropertiesNullable
                                         && principalRoleEnd.RelationshipMultiplicity == RelationshipMultiplicity.One)
                                {
                                    var message =
                                        Strings.
                                            EdmModel_Validator_Semantic_InvalidMultiplicityFromRoleToPropertyNullableV1(
                                                principalRoleEnd.Name, edmAssociationType.Name);
                                    context.AddError(
                                        edmAssociationType,
                                        null,
                                        message,
                                        XmlErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint);
                                }
                                else if ((
                                             (v1Behavior && !areAllDependentRolePropertiesNullable) ||
                                             (!v1Behavior && !isAnyDependentRolePropertyNullable)
                                         )
                                         && principalRoleEnd.RelationshipMultiplicity != RelationshipMultiplicity.One)
                                {
                                    string message;
                                    if (v1Behavior)
                                    {
                                        message =
                                            Strings.
                                                EdmModel_Validator_Semantic_InvalidMultiplicityFromRoleToPropertyNonNullableV1
                                                (
                                                    principalRoleEnd.Name, edmAssociationType.Name);
                                    }
                                    else
                                    {
                                        message =
                                            Strings.
                                                EdmModel_Validator_Semantic_InvalidMultiplicityFromRoleToPropertyNonNullableV2
                                                (
                                                    principalRoleEnd.Name, edmAssociationType.Name);
                                    }
                                    context.AddError(
                                        edmAssociationType,
                                        null,
                                        message,
                                        XmlErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint);
                                }

                                // Need to constrain the dependent role in CSDL to Key properties if this is not a IsForeignKey
                                // relationship.
                                if ((!isDependentRolePropertiesSubsetofKeyProperties)
                                    &&
                                    (!edmAssociationType.IsForeignKey(context.ValidationContextVersion)))
                                {
                                    context.AddError(
                                        dependentRoleEnd,
                                        null,
                                        Strings.EdmModel_Validator_Semantic_InvalidToPropertyInRelationshipConstraint(
                                            dependentRoleEnd.Name,
                                            dependentRoleEnd.GetEntityType().GetQualifiedName(
                                                context.GetQualifiedPrefix(dependentRoleEnd.GetEntityType())),
                                            edmAssociationType.GetQualifiedName(
                                                context.GetQualifiedPrefix(edmAssociationType))),
                                        XmlErrorCode.InvalidPropertyInRelationshipConstraint);
                                }

                                // If the principal role property is a key property, then the upper bound must be 1 i.e. every parent (from property) can 
                                // have exactly one child
                                if (isPrincipalRoleKeyProperty)
                                {
                                    if (dependentRoleEnd.RelationshipMultiplicity
                                        == RelationshipMultiplicity.Many)
                                    {
                                        context.AddError(
                                            dependentRoleEnd,
                                            null,
                                            Strings.
                                                EdmModel_Validator_Semantic_InvalidMultiplicityToRoleUpperBoundMustBeOne
                                                (
                                                    dependentRoleEnd.Name, edmAssociationType.Name),
                                            XmlErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint);
                                    }
                                }
                                    // if the principal role property is not the key, then the upper bound must be many i.e every parent (from property) can
                                    // be related to many childs
                                else if (dependentRoleEnd.RelationshipMultiplicity
                                         != RelationshipMultiplicity.Many)
                                {
                                    context.AddError(
                                        dependentRoleEnd,
                                        null,
                                        Strings.
                                            EdmModel_Validator_Semantic_InvalidMultiplicityToRoleUpperBoundMustBeMany(
                                                dependentRoleEnd.Name, edmAssociationType.Name),
                                        XmlErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint);
                                }
                                var keyProperties_PrincipalRoleEnd = principalRoleEnd.GetEntityType().GetValidKey().ToList();
                                var dependentProperties = constraint.ToProperties.ToList();

                                if (dependentProperties.Count
                                    != keyProperties_PrincipalRoleEnd.Count)
                                {
                                    context.AddError(
                                        constraint,
                                        null,
                                        Strings.
                                            EdmModel_Validator_Semantic_MismatchNumberOfPropertiesinRelationshipConstraint,
                                        XmlErrorCode.MismatchNumberOfPropertiesInRelationshipConstraint);
                                }
                                else
                                {
                                    var count = dependentProperties.Count;
                                    for (var i = 0; i < count; i++)
                                    {
                                        // The principal Role End must be a primitive type
                                        var principalProperty = keyProperties_PrincipalRoleEnd[i];
                                        var dependentProperty = dependentProperties[i];
                                        if (principalProperty != null && dependentProperty != null &&
                                            principalProperty.TypeUsage != null
                                            && dependentProperty.TypeUsage != null &&
                                            principalProperty.IsPrimitiveType
                                            && dependentProperty.IsPrimitiveType)
                                        {
                                            if (!DataModelValidationHelper.IsPrimitiveTypesEqual(
                                                dependentProperty,
                                                principalProperty))
                                            {
                                                context.AddError(
                                                    constraint,
                                                    null,
                                                    Strings.
                                                        EdmModel_Validator_Semantic_TypeMismatchRelationshipConstraint(
                                                            constraint.ToProperties.ToList()[i].Name,
                                                            dependentRoleEnd.GetEntityType().Name,
                                                            keyProperties_PrincipalRoleEnd[i].Name,
                                                            principalRoleEnd.GetEntityType().Name,
                                                            edmAssociationType.Name
                                                        ),
                                                    XmlErrorCode.TypeMismatchRelationshipConstraint);
                                            }
                                        }
                                    }
                                }
                            }
                        });

        internal static readonly EdmModelValidationRule<AssociationType>
            EdmAssociationType_InvalidPropertyInRelationshipConstraint =
                new EdmModelValidationRule<AssociationType>(
                    (context, edmAssociationType) =>
                        {
                            if (edmAssociationType.Constraint != null &&
                                edmAssociationType.Constraint.DependentEnd != null
                                &&
                                edmAssociationType.Constraint.DependentEnd.GetEntityType() != null)
                            {
                                var dependentEndProperties =
                                    edmAssociationType.Constraint.DependentEnd.GetEntityType().Properties.ToList();
                                foreach (var property in edmAssociationType.Constraint.ToProperties)
                                {
                                    if (property != null)
                                    {
                                        if (!dependentEndProperties.Contains(property))
                                        {
                                            context.AddError(
                                                property,
                                                null,
                                                Strings.
                                                    EdmModel_Validator_Semantic_InvalidPropertyInRelationshipConstraint(
                                                        property.Name,
                                                        edmAssociationType.Constraint.DependentEnd.Name),
                                                XmlErrorCode.InvalidPropertyInRelationshipConstraint);
                                        }
                                    }
                                }
                            }
                        });

        internal static readonly EdmModelValidationRule<ComplexType> EdmComplexType_InvalidIsAbstract =
            new EdmModelValidationRule<ComplexType>(
                (context, edmComplexType) =>
                    {
                        if (edmComplexType.Abstract)
                        {
                            context.AddError(
                                edmComplexType,
                                EdmConstants.Property_IsAbstract,
                                Strings.EdmModel_Validator_Semantic_InvalidComplexTypeAbstract(
                                    edmComplexType.GetQualifiedName(context.GetQualifiedPrefix(edmComplexType))),
                                XmlErrorCode.EdmModel_Validator_InvalidAbstractComplexType);
                        }
                    });

        internal static readonly EdmModelValidationRule<ComplexType> EdmComplexType_InvalidIsPolymorphic =
            new EdmModelValidationRule<ComplexType>(
                (context, edmComplexType) =>
                    {
                        if (edmComplexType.BaseType != null)
                        {
                            context.AddError(
                                edmComplexType,
                                EdmConstants.Property_BaseType,
                                Strings.EdmModel_Validator_Semantic_InvalidComplexTypePolymorphic(
                                    edmComplexType.GetQualifiedName(context.GetQualifiedPrefix(edmComplexType))),
                                XmlErrorCode.EdmModel_Validator_InvalidPolymorphicComplexType);
                        }
                    });

        internal static readonly EdmModelValidationRule<ComplexType> EdmComplexType_InvalidMemberNameMatchesTypeName
            =
            new EdmModelValidationRule<ComplexType>(
                (context, edmComplexType) =>
                    {
                        if (edmComplexType.Name.HasContent()
                            && edmComplexType.Properties.Any())
                        {
                            foreach (var property in edmComplexType.Properties)
                            {
                                if (property != null)
                                {
                                    if (property.Name.EqualsOrdinal(edmComplexType.Name))
                                    {
                                        context.AddError(
                                            property,
                                            CsdlConstants.Attribute_Name,
                                            Strings.EdmModel_Validator_Semantic_InvalidMemberNameMatchesTypeName(
                                                property.Name,
                                                edmComplexType.GetQualifiedName(
                                                    context.GetQualifiedPrefix(edmComplexType))),
                                            XmlErrorCode.BadProperty);
                                    }
                                }
                            }
                        }
                    });

        internal static readonly EdmModelValidationRule<ComplexType>
            EdmComplexType_PropertyNameAlreadyDefinedDuplicate =
                new EdmModelValidationRule<ComplexType>(
                    (context, edmComplexType) =>
                        {
                            if (edmComplexType.Properties.Any())
                            {
                                var propertyNames = new HashSet<string>();
                                foreach (var property in edmComplexType.Properties)
                                {
                                    if (property.Name.HasContent())
                                    {
                                        DataModelValidationHelper.AddMemberNameToHashSet(
                                            property,
                                            propertyNames,
                                            context,
                                            Strings.EdmModel_Validator_Semantic_PropertyNameAlreadyDefinedDuplicate);
                                    }
                                }
                            }
                        });

        internal static readonly EdmModelValidationRule<ComplexType>
            EdmComplexType_PropertyNameAlreadyDefinedDuplicate_V1_1 =
                new EdmModelValidationRule<ComplexType>(
                    (context, edmComplexType) =>
                        {
                            if (edmComplexType.Properties.Any())
                            {
                                var propertyNames = new HashSet<string>();
                                foreach (var property in edmComplexType.Properties)
                                {
                                    if (property != null)
                                    {
                                        if (property.Name.HasContent())
                                        {
                                            DataModelValidationHelper.AddMemberNameToHashSet(
                                                property,
                                                propertyNames,
                                                context,
                                                Strings.EdmModel_Validator_Semantic_PropertyNameAlreadyDefinedDuplicate);
                                        }
                                    }
                                }
                            }
                        });

        internal static readonly EdmModelValidationRule<ComplexType> EdmComplexType_CycleInTypeHierarchy_V1_1 =
            new EdmModelValidationRule<ComplexType>(
                (context, edmComplexType) =>
                    {
                        if (DataModelValidationHelper.CheckForInheritanceCycle(edmComplexType, ct => (ComplexType)ct.BaseType))
                        {
                            context.AddError(
                                edmComplexType,
                                CsdlConstants.Attribute_BaseType,
                                Strings.EdmModel_Validator_Semantic_CycleInTypeHierarchy(
                                    edmComplexType.GetQualifiedName(context.GetQualifiedPrefix(edmComplexType))),
                                XmlErrorCode.CycleInTypeHierarchy);
                        }
                    });

        internal static readonly EdmModelValidationRule<EdmProperty> EdmProperty_InvalidCollectionKind =
            new EdmModelValidationRule<EdmProperty>(
                (context, edmProperty) =>
                    {
                        if (edmProperty.CollectionKind
                            != CollectionKind.None)
                        {
                            context.AddError(
                                edmProperty,
                                EdmConstants.Property_CollectionKind,
                                Strings.EdmModel_Validator_Semantic_InvalidCollectionKindNotV1_1(edmProperty.Name),
                                XmlErrorCode.InvalidCollectionKind);
                        }
                    });

        internal static readonly EdmModelValidationRule<EdmProperty> EdmProperty_InvalidCollectionKind_V1_1 =
            new EdmModelValidationRule<EdmProperty>(
                (context, edmProperty) =>
                    {
                        if (edmProperty.CollectionKind != CollectionKind.None &&
                            edmProperty.TypeUsage != null
                            && !edmProperty.IsCollectionType)
                        {
                            context.AddError(
                                edmProperty,
                                EdmConstants.Property_CollectionKind,
                                Strings.EdmModel_Validator_Semantic_InvalidCollectionKindNotCollection(edmProperty.Name),
                                XmlErrorCode.InvalidCollectionKind);
                        }
                    });

        internal static readonly EdmModelValidationRule<EdmProperty> EdmProperty_NullableComplexType =
            new EdmModelValidationRule<EdmProperty>(
                (context, edmProperty) =>
                    {
                        if (edmProperty.TypeUsage != null)
                        {
                            if (edmProperty.ComplexType != null)
                            {
                                if (edmProperty.Nullable)
                                {
                                    context.AddError(
                                        edmProperty,
                                        CsdlConstants.Attribute_Nullable,
                                        Strings.EdmModel_Validator_Semantic_NullableComplexType(edmProperty.Name),
                                        XmlErrorCode.NullableComplexType);
                                }
                            }
                        }
                    });

        internal static readonly EdmModelValidationRule<EdmProperty> EdmProperty_InvalidPropertyType =
            new EdmModelValidationRule<EdmProperty>(
                (context, edmProperty) =>
                    {
                        if (edmProperty.TypeUsage.EdmType != null)
                        {
                            if (!edmProperty.IsPrimitiveType
                                && !edmProperty.IsComplexType)
                            {
                                context.AddError(
                                    edmProperty,
                                    CsdlConstants.Attribute_Type,
                                    Strings.EdmModel_Validator_Semantic_InvalidPropertyType(
                                        (edmProperty.IsCollectionType
                                             ? EdmConstants.Value_CollectionType
                                             : edmProperty.TypeUsage.EdmType.BuiltInTypeKind.ToString())),
                                    XmlErrorCode.InvalidPropertyType);
                            }
                        }
                    });

        internal static readonly EdmModelValidationRule<EdmProperty> EdmProperty_InvalidPropertyType_V1_1 =
            new EdmModelValidationRule<EdmProperty>(
                (context, edmProperty) =>
                    {
                        if (edmProperty.TypeUsage != null
                            &&
                            edmProperty.TypeUsage.EdmType != null)
                        {
                            if (!edmProperty.IsPrimitiveType &&
                                !edmProperty.IsComplexType
                                && !edmProperty.IsCollectionType)
                            {
                                context.AddError(
                                    edmProperty,
                                    CsdlConstants.Attribute_Type,
                                    Strings.EdmModel_Validator_Semantic_InvalidPropertyType_V1_1(
                                        edmProperty.TypeUsage.EdmType.BuiltInTypeKind.ToString()),
                                    XmlErrorCode.InvalidPropertyType);
                            }
                        }
                    });

        internal static readonly EdmModelValidationRule<EdmProperty> EdmProperty_InvalidPropertyType_V3 =
            new EdmModelValidationRule<EdmProperty>(
                (context, edmProperty) =>
                    {
                        if (edmProperty.TypeUsage != null
                            &&
                            edmProperty.TypeUsage.EdmType != null)
                        {
                            if (!edmProperty.IsPrimitiveType &&
                                !edmProperty.IsComplexType
                                && !edmProperty.IsEnumType)
                            {
                                context.AddError(
                                    edmProperty,
                                    CsdlConstants.Attribute_Type,
                                    Strings.EdmModel_Validator_Semantic_InvalidPropertyType_V3(
                                        edmProperty.TypeUsage.EdmType.BuiltInTypeKind.ToString()),
                                    XmlErrorCode.InvalidPropertyType);
                            }
                        }
                    });

        internal static readonly EdmModelValidationRule<EdmNamespace> EdmNamespace_TypeNameAlreadyDefinedDuplicate =
            new EdmModelValidationRule<EdmNamespace>(
                (context, edmNamespace) =>
                    {
                        var memberNameList = new HashSet<string>();
                        foreach (var item in edmNamespace.NamespaceItems)
                        {
                            DataModelValidationHelper.AddMemberNameToHashSet(
                                item,
                                memberNameList,
                                context,
                                Strings.EdmModel_Validator_Semantic_TypeNameAlreadyDefinedDuplicate);
                        }
                    }
                );
    }
}
