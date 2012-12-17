// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
    internal static class EdmModelSemanticValidationRules
    {
        internal static readonly EdmModelValidationRule<EdmType> EdmType_SystemNamespaceEncountered =
            new EdmModelValidationRule<EdmType>(
                (context, edmType) =>
                    {
                        if (IsEdmSystemNamespace(edmType.NamespaceName))
                        {
                            context.AddError(
                                edmType,
                                null,
                                Strings.EdmModel_Validator_Semantic_SystemNamespaceEncountered(edmType.Name));
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
                                    e => AreRelationshipEndsEqual(e, sourceEnd));
                            var existTargetEnd =
                                targetEndList.FirstOrDefault(
                                    e => AreRelationshipEndsEqual(e, targetEnd));

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
                                        edmEntityContainer.Name));
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
                                        edmEntityContainer.Name));
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
                                    if (associationSet.SourceSet != null
                                        && associationSet.ElementType != null
                                        && associationSet.ElementType.SourceEnd != null)
                                    {
                                        if (!edmEntityContainer.EntitySets.Contains(associationSet.SourceSet))
                                        {
                                            context.AddError(
                                                associationSet.SourceSet,
                                                null,
                                                Strings.EdmModel_Validator_Semantic_InvalidEntitySetNameReference(
                                                    associationSet.SourceSet.Name,
                                                    associationSet.ElementType.SourceEnd.Name));
                                        }
                                    }

                                    if (associationSet.TargetSet != null
                                        && associationSet.ElementType != null
                                        && associationSet.ElementType.TargetEnd != null)
                                    {
                                        if (!edmEntityContainer.EntitySets.Contains(associationSet.TargetSet))
                                        {
                                            context.AddError(
                                                associationSet.TargetSet,
                                                null,
                                                Strings.EdmModel_Validator_Semantic_InvalidEntitySetNameReference(
                                                    associationSet.TargetSet.Name,
                                                    associationSet.ElementType.TargetEnd.Name));
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
                                if (entitySet != null
                                    && entitySet.ElementType != null
                                    && !baseEntitySetTypes.ContainsKey(entitySet.ElementType))
                                {
                                    baseEntitySetTypes.Add(entitySet.ElementType, entitySet);
                                }
                            }

                            // look through each type in this schema and see if it is derived from a base
                            // type if it is then see if it has some "new" Concurrency fields
                            foreach (var entityType in context.Model.EntityTypes)
                            {
                                EntitySet set;
                                if (TypeIsSubTypeOf(entityType, baseEntitySetTypes, out set)
                                    && IsTypeDefinesNewConcurrencyProperties(entityType))
                                {
                                    context.AddError(
                                        entityType,
                                        null,
                                        Strings.EdmModel_Validator_Semantic_ConcurrencyRedefinedOnSubTypeOfEntitySetType
                                            (
                                                entityType.GetQualifiedName(entityType.NamespaceName),
                                                set.ElementType.GetQualifiedName(
                                                    set.ElementType.NamespaceName),
                                                set.GetQualifiedName(set.EntityContainer.Name)));
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
                                AddMemberNameToHashSet(
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
                                    XmlConstants.EntityType,
                                    Strings.EdmModel_Validator_Semantic_EntitySetTypeHasNoKeys(
                                        edmEntitySet.Name, edmEntitySet.ElementType.Name));
                            }
                        }
                    });

        internal static readonly EdmModelValidationRule<AssociationSet> EdmAssociationSet_DuplicateEndName =
            new EdmModelValidationRule<AssociationSet>(
                (context, edmAssociationSet) =>
                    {
                        if (edmAssociationSet.ElementType != null
                            && edmAssociationSet.ElementType.SourceEnd != null
                            && edmAssociationSet.ElementType.TargetEnd != null)
                        {
                            if (edmAssociationSet.ElementType.SourceEnd.Name
                                == edmAssociationSet.ElementType.TargetEnd.Name)
                            {
                                context.AddError(
                                    edmAssociationSet.SourceSet,
                                    XmlConstants.Name,
                                    Strings.EdmModel_Validator_Semantic_DuplicateEndName(
                                        edmAssociationSet.ElementType.SourceEnd.Name));
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
                                                            edmEntityType.Name, key.Name));
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
                                        EdmConstants.Nullable,
                                        Strings.EdmModel_Validator_Semantic_InvalidKeyNullablePart(
                                            key.Name, edmEntityType.Name));
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
                                        edmEntityType.Name, key.Name));
                            }
                        }
                    });

        internal static readonly EdmModelValidationRule<EntityType> EdmEntityType_InvalidKeyKeyDefinedInBaseClass =
            new EdmModelValidationRule<EntityType>(
                (context, edmEntityType) =>
                    {
                        if (edmEntityType.BaseType != null
                            &&
                            edmEntityType.DeclaredKeyProperties != null
                            && edmEntityType.DeclaredKeyProperties.Any())
                        {
                            context.AddError(
                                edmEntityType.BaseType,
                                null,
                                Strings.EdmModel_Validator_Semantic_InvalidKeyKeyDefinedInBaseClass(
                                    edmEntityType.Name, edmEntityType.BaseType.Name));
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
                                Strings.EdmModel_Validator_Semantic_KeyMissingOnEntityType(edmEntityType.Name));
                        }
                    });

        internal static readonly EdmModelValidationRule<EntityType> EdmEntityType_InvalidMemberNameMatchesTypeName =
            new EdmModelValidationRule<EntityType>(
                (context, edmEntityType) =>
                    {
                        var properties = edmEntityType.Properties.ToList();
                        if (!string.IsNullOrWhiteSpace(edmEntityType.Name)
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
                                            XmlConstants.Name,
                                            Strings.EdmModel_Validator_Semantic_InvalidMemberNameMatchesTypeName(
                                                property.Name,
                                                edmEntityType.GetQualifiedName(
                                                    edmEntityType.NamespaceName)));
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
                                                XmlConstants.Name,
                                                Strings.EdmModel_Validator_Semantic_InvalidMemberNameMatchesTypeName(
                                                    property.Name,
                                                    edmEntityType.GetQualifiedName(
                                                        edmEntityType.NamespaceName)));
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
                                if (!string.IsNullOrWhiteSpace(property.Name))
                                {
                                    AddMemberNameToHashSet(
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
                                    if (!string.IsNullOrWhiteSpace(property.Name))
                                    {
                                        AddMemberNameToHashSet(
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
                        if (CheckForInheritanceCycle(edmEntityType, et => (EntityType)et.BaseType))
                        {
                            context.AddError(
                                edmEntityType,
                                XmlConstants.BaseType,
                                Strings.EdmModel_Validator_Semantic_CycleInTypeHierarchy(
                                    edmEntityType.GetQualifiedName(edmEntityType.NamespaceName)));
                        }
                    });

        internal static readonly EdmModelValidationRule<NavigationProperty>
            EdmNavigationProperty_BadNavigationPropertyUndefinedRole =
                new EdmModelValidationRule<NavigationProperty>(
                    (context, edmNavigationProperty) =>
                        {
                            if (edmNavigationProperty.Association != null
                                && edmNavigationProperty.Association.SourceEnd != null
                                && edmNavigationProperty.Association.TargetEnd != null
                                && edmNavigationProperty.Association.SourceEnd.Name != null
                                && edmNavigationProperty.Association.TargetEnd.Name != null)
                            {
                                if (edmNavigationProperty.ToEndMember != edmNavigationProperty.Association.SourceEnd
                                    && edmNavigationProperty.ToEndMember != edmNavigationProperty.Association.TargetEnd)
                                {
                                    context.AddError(
                                        edmNavigationProperty,
                                        null,
                                        Strings.EdmModel_Validator_Semantic_BadNavigationPropertyUndefinedRole(
                                            edmNavigationProperty.Association.SourceEnd.Name,
                                            edmNavigationProperty.Association.TargetEnd.Name,
                                            edmNavigationProperty.Association.Name));
                                }
                            }
                        });

        internal static readonly EdmModelValidationRule<NavigationProperty>
            EdmNavigationProperty_BadNavigationPropertyRolesCannotBeTheSame =
                new EdmModelValidationRule<NavigationProperty>(
                    (context, edmNavigationProperty) =>
                        {
                            if (edmNavigationProperty.Association != null
                                && edmNavigationProperty.Association.SourceEnd != null
                                && edmNavigationProperty.Association.TargetEnd != null)
                            {
                                if (edmNavigationProperty.ToEndMember == edmNavigationProperty.GetFromEnd())
                                {
                                    context.AddError(
                                        edmNavigationProperty,
                                        XmlConstants.ToRole,
                                        Strings.EdmModel_Validator_Semantic_BadNavigationPropertyRolesCannotBeTheSame);
                                }
                            }
                        });

        internal static readonly EdmModelValidationRule<NavigationProperty>
            EdmNavigationProperty_BadNavigationPropertyBadFromRoleType =
                new EdmModelValidationRule<NavigationProperty>(
                    (context, edmNavigationProperty) =>
                        {
                            AssociationEndMember fromEnd;

                            if (edmNavigationProperty.Association != null
                                && (fromEnd = edmNavigationProperty.GetFromEnd()) != null)
                            {
                                var parent
                                    = context.Model.EntityTypes
                                             .Single(e => e.DeclaredNavigationProperties.Contains(edmNavigationProperty));

                                var fromEndEntityType = fromEnd.GetEntityType();

                                if (parent != fromEndEntityType)
                                {
                                    context.AddError(
                                        edmNavigationProperty,
                                        XmlConstants.FromRole,
                                        Strings.BadNavigationPropertyBadFromRoleType(
                                            edmNavigationProperty.Name,
                                            fromEndEntityType.Name,
                                            fromEnd.Name,
                                            edmNavigationProperty.Association.Name,
                                            parent.Name));
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
                                    Strings.EdmModel_Validator_Semantic_InvalidOperationMultipleEndsInAssociation);
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
                                        XmlConstants.OnDelete,
                                        Strings.
                                            EdmModel_Validator_Semantic_EndWithManyMultiplicityCannotHaveOperationsSpecified
                                            (
                                                edmAssociationType.SourceEnd.Name,
                                                edmAssociationType.Name));
                                }
                            }

                            if (edmAssociationType.TargetEnd != null)
                            {
                                if (edmAssociationType.TargetEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many
                                    && edmAssociationType.TargetEnd.DeleteBehavior != OperationAction.None)
                                {
                                    context.AddError(
                                        edmAssociationType.TargetEnd,
                                        XmlConstants.OnDelete,
                                        Strings.
                                            EdmModel_Validator_Semantic_EndWithManyMultiplicityCannotHaveOperationsSpecified
                                            (
                                                edmAssociationType.TargetEnd.Name,
                                                edmAssociationType.Name));
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
                                        XmlConstants.Name,
                                        Strings.EdmModel_Validator_Semantic_EndNameAlreadyDefinedDuplicate(
                                            edmAssociationType.SourceEnd.Name));
                                }
                            }
                        });

        internal static readonly EdmModelValidationRule<AssociationType>
            EdmAssociationType_SameRoleReferredInReferentialConstraint =
                new EdmModelValidationRule<AssociationType>(
                    (context, edmAssociationType) =>
                        {
                            if (IsReferentialConstraintReadyForValidation(edmAssociationType))
                            {
                                // this also includes the scenario if the Principal and Dependent are pointing to the same AssociationEndMember
                                if (edmAssociationType.Constraint.FromRole.Name
                                    ==
                                    edmAssociationType.Constraint.ToRole.Name)
                                {
                                    context.AddError(
                                        edmAssociationType.Constraint.ToRole,
                                        null,
                                        Strings.EdmModel_Validator_Semantic_SameRoleReferredInReferentialConstraint(
                                            edmAssociationType.Name));
                                }
                            }
                        });

        internal static readonly EdmModelValidationRule<AssociationType>
            EdmAssociationType_ValidateReferentialConstraint =
                new EdmModelValidationRule<AssociationType>(
                    (context, edmAssociationType) =>
                        {
                            if (IsReferentialConstraintReadyForValidation(edmAssociationType))
                            {
                                var constraint = edmAssociationType.Constraint;

                                // Validate the to end and from end of the referential constraint
                                var principalRoleEnd = constraint.FromRole;
                                var dependentRoleEnd = constraint.ToRole;

                                bool isPrincipalRoleKeyProperty, isDependentRoleKeyProperty;
                                bool areAllPrinicipalRolePropertiesNullable, areAllDependentRolePropertiesNullable;
                                bool isDependentRolePropertiesSubsetofKeyProperties,
                                     isPrinicipalRolePropertiesSubsetofKeyProperties;
                                bool isAnyPrinicipalRolePropertyNullable, isAnyDependentRolePropertyNullable;

                                // Resolve all the property in the dependent end attribute. Also checks whether this is nullable or not and 
                                // whether the properties are the keys for the type in the dependent end
                                IsKeyProperty(
                                    constraint.ToProperties.ToList(),
                                    dependentRoleEnd,
                                    out isPrincipalRoleKeyProperty,
                                    out areAllDependentRolePropertiesNullable,
                                    out isAnyDependentRolePropertyNullable,
                                    out isDependentRolePropertiesSubsetofKeyProperties);

                                // Resolve all the property in the principal end attribute. Also checks whether this is nullable or not and 
                                // whether the properties are the keys for the type in the principal role
                                IsKeyProperty(
                                    constraint.FromRole.GetEntityType().GetValidKey().ToList(),
                                    principalRoleEnd,
                                    out isDependentRoleKeyProperty,
                                    out areAllPrinicipalRolePropertiesNullable,
                                    out isAnyPrinicipalRolePropertyNullable,
                                    out isPrinicipalRolePropertiesSubsetofKeyProperties);

                                Debug.Assert(
                                    constraint.FromRole.GetEntityType().GetValidKey().Any(),
                                    "There should be some ref properties in Principal Role");
                                Debug.Assert(constraint.ToProperties.Count() != 0, "There should be some ref properties in Dependent Role");
                                Debug.Assert(
                                    isDependentRoleKeyProperty,
                                    "The properties in the PrincipalRole must be the key of the Entity type referred to by the principal role");

                                var v1Behavior = context.ValidationContextVersion <= XmlConstants.EdmVersionForV1_1;

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
                                                principalRoleEnd.Name, edmAssociationType.Name));
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
                                        message);
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
                                        message);
                                }

                                // Need to constrain the dependent role in CSDL to Key properties if this is not a IsForeignKey
                                // relationship.
                                if ((!isDependentRolePropertiesSubsetofKeyProperties)
                                    && !edmAssociationType.IsForeignKey(context.ValidationContextVersion))
                                {
                                    context.AddError(
                                        dependentRoleEnd,
                                        null,
                                        Strings.EdmModel_Validator_Semantic_InvalidToPropertyInRelationshipConstraint(
                                            dependentRoleEnd.Name,
                                            dependentRoleEnd.GetEntityType().GetQualifiedName(
                                                dependentRoleEnd.GetEntityType().NamespaceName),
                                            edmAssociationType.GetQualifiedName(
                                                edmAssociationType.NamespaceName)));
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
                                                    dependentRoleEnd.Name, edmAssociationType.Name));
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
                                                dependentRoleEnd.Name, edmAssociationType.Name));
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
                                            EdmModel_Validator_Semantic_MismatchNumberOfPropertiesinRelationshipConstraint);
                                }
                                else
                                {
                                    var count = dependentProperties.Count;
                                    for (var i = 0; i < count; i++)
                                    {
                                        // The principal Role End must be a primitive type
                                        var principalProperty = keyProperties_PrincipalRoleEnd[i];
                                        var dependentProperty = dependentProperties[i];
                                        if (principalProperty != null
                                            && dependentProperty != null
                                            &&
                                            principalProperty.TypeUsage != null
                                            && dependentProperty.TypeUsage != null
                                            &&
                                            principalProperty.IsPrimitiveType
                                            && dependentProperty.IsPrimitiveType)
                                        {
                                            if (!IsPrimitiveTypesEqual(
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
                                                        ));
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
                            if (edmAssociationType.Constraint != null
                                &&
                                edmAssociationType.Constraint.ToRole != null
                                &&
                                edmAssociationType.Constraint.ToRole.GetEntityType() != null)
                            {
                                var dependentEndProperties =
                                    edmAssociationType.Constraint.ToRole.GetEntityType().Properties.ToList();
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
                                                        edmAssociationType.Constraint.ToRole.Name));
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
                                EdmConstants.Abstract,
                                Strings.EdmModel_Validator_Semantic_InvalidComplexTypeAbstract(
                                    edmComplexType.GetQualifiedName(edmComplexType.NamespaceName)));
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
                                EdmConstants.BaseType,
                                Strings.EdmModel_Validator_Semantic_InvalidComplexTypePolymorphic(
                                    edmComplexType.GetQualifiedName(edmComplexType.NamespaceName)));
                        }
                    });

        internal static readonly EdmModelValidationRule<ComplexType> EdmComplexType_InvalidMemberNameMatchesTypeName
            =
            new EdmModelValidationRule<ComplexType>(
                (context, edmComplexType) =>
                    {
                        if (!string.IsNullOrWhiteSpace(edmComplexType.Name)
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
                                            XmlConstants.Name,
                                            Strings.EdmModel_Validator_Semantic_InvalidMemberNameMatchesTypeName(
                                                property.Name,
                                                edmComplexType.GetQualifiedName(
                                                    edmComplexType.NamespaceName)));
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
                                    if (!string.IsNullOrWhiteSpace(property.Name))
                                    {
                                        AddMemberNameToHashSet(
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
                                        if (!string.IsNullOrWhiteSpace(property.Name))
                                        {
                                            AddMemberNameToHashSet(
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
                        if (CheckForInheritanceCycle(edmComplexType, ct => (ComplexType)ct.BaseType))
                        {
                            context.AddError(
                                edmComplexType,
                                XmlConstants.BaseType,
                                Strings.EdmModel_Validator_Semantic_CycleInTypeHierarchy(
                                    edmComplexType.GetQualifiedName(edmComplexType.NamespaceName)));
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
                                EdmConstants.CollectionKind,
                                Strings.EdmModel_Validator_Semantic_InvalidCollectionKindNotV1_1(edmProperty.Name));
                        }
                    });

        internal static readonly EdmModelValidationRule<EdmProperty> EdmProperty_InvalidCollectionKind_V1_1 =
            new EdmModelValidationRule<EdmProperty>(
                (context, edmProperty) =>
                    {
                        if (edmProperty.CollectionKind != CollectionKind.None
                            &&
                            edmProperty.TypeUsage != null
                            && !edmProperty.IsCollectionType)
                        {
                            context.AddError(
                                edmProperty,
                                EdmConstants.CollectionKind,
                                Strings.EdmModel_Validator_Semantic_InvalidCollectionKindNotCollection(edmProperty.Name));
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
                                        EdmConstants.Nullable,
                                        Strings.EdmModel_Validator_Semantic_NullableComplexType(edmProperty.Name));
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
                                    XmlConstants.TypeAttribute,
                                    Strings.EdmModel_Validator_Semantic_InvalidPropertyType(
                                        (edmProperty.IsCollectionType
                                             ? EdmConstants.CollectionType
                                             : edmProperty.TypeUsage.EdmType.BuiltInTypeKind.ToString())));
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
                            if (!edmProperty.IsPrimitiveType
                                &&
                                !edmProperty.IsComplexType
                                && !edmProperty.IsCollectionType)
                            {
                                context.AddError(
                                    edmProperty,
                                    XmlConstants.TypeAttribute,
                                    Strings.EdmModel_Validator_Semantic_InvalidPropertyType_V1_1(
                                        edmProperty.TypeUsage.EdmType.BuiltInTypeKind.ToString()));
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
                            if (!edmProperty.IsPrimitiveType
                                &&
                                !edmProperty.IsComplexType
                                && !edmProperty.IsEnumType)
                            {
                                context.AddError(
                                    edmProperty,
                                    XmlConstants.TypeAttribute,
                                    Strings.EdmModel_Validator_Semantic_InvalidPropertyType_V3(
                                        edmProperty.TypeUsage.EdmType.BuiltInTypeKind.ToString()));
                            }
                        }
                    });

        internal static readonly EdmModelValidationRule<EdmModel> EdmNamespace_TypeNameAlreadyDefinedDuplicate =
            new EdmModelValidationRule<EdmModel>(
                (context, model) =>
                    {
                        var memberNameList = new HashSet<string>();

                        foreach (var item in model.NamespaceItems)
                        {
                            AddMemberNameToHashSet(
                                item,
                                memberNameList,
                                context,
                                Strings.EdmModel_Validator_Semantic_TypeNameAlreadyDefinedDuplicate);
                        }
                    }
                );

        private static bool AreRelationshipEndsEqual(
            KeyValuePair<AssociationSet, EntitySet> left, KeyValuePair<AssociationSet, EntitySet> right)
        {
            if (ReferenceEquals(left.Value, right.Value)
                && ReferenceEquals(left.Key.ElementType, right.Key.ElementType))
            {
                return true;
            }

            return false;
        }

        private static bool IsReferentialConstraintReadyForValidation(AssociationType association)
        {
            var constraint = association.Constraint;
            if (constraint == null)
            {
                return false;
            }

            if (constraint.FromRole == null
                || constraint.ToRole == null)
            {
                return false;
            }

            if (constraint.FromRole.GetEntityType() == null
                || constraint.ToRole.GetEntityType() == null)
            {
                return false;
            }

            if (constraint.ToProperties.Any())
            {
                foreach (var propRef in constraint.ToProperties)
                {
                    if (propRef == null)
                    {
                        return false;
                    }

                    if (propRef.TypeUsage == null
                        || propRef.TypeUsage.EdmType == null)
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            var keyList = constraint.FromRole.GetEntityType().GetValidKey();

            if (keyList.Any())
            {
                return keyList.All(
                    propRef => propRef != null
                               && propRef.TypeUsage != null
                               && propRef.TypeUsage.EdmType != null);
            }

            return false;
        }

        private static void IsKeyProperty(
            List<EdmProperty> roleProperties,
            RelationshipEndMember roleElement,
            out bool isKeyProperty,
            out bool areAllPropertiesNullable,
            out bool isAnyPropertyNullable,
            out bool isSubsetOfKeyProperties)
        {
            isKeyProperty = true;
            areAllPropertiesNullable = true;
            isAnyPropertyNullable = false;
            isSubsetOfKeyProperties = true;

            if (roleElement.GetEntityType().GetValidKey().Count()
                != roleProperties.Count())
            {
                isKeyProperty = false;
            }

            // Checking that ToProperties must be the key properties in the entity type referred by the ToRole
            for (var i = 0; i < roleProperties.Count(); i++)
            {
                // Once we find that the properties in the constraint are not a subset of the
                // Key, one need not search for it every time
                if (isSubsetOfKeyProperties)
                {
                    var keyProperties = roleElement.GetEntityType().GetValidKey().ToList();

                    // All properties that are defined in ToProperties must be the key property on the entity type
                    var foundKeyProperty = keyProperties.Contains(roleProperties[i]);

                    if (!foundKeyProperty)
                    {
                        isKeyProperty = false;
                        isSubsetOfKeyProperties = false;
                    }
                }

                // by default if IsNullable doesn't have a value, the IsNullable is true
                var isNullable = roleProperties[i].Nullable;

                areAllPropertiesNullable &= isNullable;
                isAnyPropertyNullable |= isNullable;
            }
        }

        private static void AddMemberNameToHashSet(
            INamedDataModelItem item,
            HashSet<string> memberNameList,
            EdmModelValidationContext context,
            Func<string, string> getErrorString)
        {
            if (!string.IsNullOrWhiteSpace(item.Name))
            {
                if (!memberNameList.Add(item.Name))
                {
                    context.AddError(
                        (MetadataItem)item,
                        XmlConstants.Name,
                        getErrorString(item.Name));
                }
            }
        }

        private static bool CheckForInheritanceCycle<T>(T type, Func<T, T> getBaseType)
            where T : class
        {
            var baseType = getBaseType(type);
            if (baseType != null)
            {
                var ref1 = baseType;
                var ref2 = baseType;

                do
                {
                    ref2 = getBaseType(ref2);

                    if (ReferenceEquals(ref1, ref2))
                    {
                        return true;
                    }

                    if (ref1 == null)
                    {
                        return false;
                    }

                    ref1 = getBaseType(ref1);

                    if (ref2 != null)
                    {
                        ref2 = getBaseType(ref2);
                    }
                }
                while (ref2 != null);
            }
            return false;
        }

        private static bool IsPrimitiveTypesEqual(EdmProperty primitiveType1, EdmProperty primitiveType2)
        {
            Debug.Assert(primitiveType1.IsPrimitiveType, "primitiveType1 must be a PrimitiveType");
            Debug.Assert(primitiveType2.IsPrimitiveType, "primitiveType2 must be a PrimitiveType");

            if (primitiveType1.PrimitiveType.PrimitiveTypeKind
                == primitiveType2.PrimitiveType.PrimitiveTypeKind)
            {
                return true;
            }
            return false;
        }

        private static bool IsEdmSystemNamespace(string namespaceName)
        {
            return (namespaceName == EdmConstants.TransientNamespace ||
                    namespaceName == EdmConstants.EdmNamespace ||
                    namespaceName == EdmConstants.ClrPrimitiveTypeNamespace);
        }

        private static bool IsTypeDefinesNewConcurrencyProperties(EntityType entityType)
        {
            return entityType.DeclaredProperties.Where(property => property.TypeUsage != null)
                             .Any(
                                 property => property.PrimitiveType != null
                                             && property.ConcurrencyMode != ConcurrencyMode.None);
        }

        private static bool TypeIsSubTypeOf(
            EntityType entityType, Dictionary<EntityType, EntitySet> baseEntitySetTypes, out EntitySet set)
        {
            if (entityType.IsTypeHierarchyRoot())
            {
                // can't be a sub type if we are a base type
                set = null;
                return false;
            }

            // walk up the hierarchy looking for a base that is the base type of an entityset
            foreach (var baseType in entityType.ToHierarchy())
            {
                if (baseEntitySetTypes.ContainsKey(baseType))
                {
                    set = baseEntitySetTypes[baseType];
                    return true;
                }
            }

            set = null;
            return false;
        }

        private static bool IsTypeHierarchyRoot(this EntityType entityType)
        {
            return entityType.BaseType == null;
        }

        private static bool IsForeignKey(this AssociationType association, double version)
        {
            if (version >= XmlConstants.EdmVersionForV2
                && association.Constraint != null)
            {
                // in V2, referential constraint implies foreign key
                return true;
            }
            return false;
        }
    }
}
