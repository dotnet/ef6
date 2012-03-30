namespace System.Data.Entity.Edm.Validation.Internal.EdmModel
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.Edm.Internal;
    using System.Data.Entity.Edm.Parsing.Xml.Internal;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Csdl;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using EdmModel = System.Data.Entity.Edm.EdmModel;

    [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
    internal static class EdmModelSemanticValidationRules
    {
        #region EdmModel

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

        #endregion

        #region EdmEntityContainer

        internal static readonly EdmModelValidationRule<EdmEntityContainer> EdmEntityContainer_SimilarRelationshipEnd =
            new EdmModelValidationRule<EdmEntityContainer>(
                (context, edmEntityContainer) =>
                    {
                        var sourceEndList =
                            new List<KeyValuePair<EdmAssociationSet, EdmEntitySet>>();
                        var targetEndList =
                            new List<KeyValuePair<EdmAssociationSet, EdmEntitySet>>();
                        foreach (var set in edmEntityContainer.AssociationSets)
                        {
                            var sourceEnd =
                                new KeyValuePair<EdmAssociationSet, EdmEntitySet>(set, set.SourceSet);
                            var targetEnd =
                                new KeyValuePair<EdmAssociationSet, EdmEntitySet>(set, set.TargetSet);

                            var existSourceEnd =
                                sourceEndList.FirstOrDefault(
                                    e => DataModelValidationHelper.AreRelationshipEndsEqual(e, sourceEnd));
                            var existTargetEnd =
                                targetEndList.FirstOrDefault(
                                    e => DataModelValidationHelper.AreRelationshipEndsEqual(e, targetEnd));

                            if (!existSourceEnd.Equals(default(KeyValuePair<EdmAssociationSet, EdmEntitySet>)))
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

                            if (!existTargetEnd.Equals(default(KeyValuePair<EdmAssociationSet, EdmEntitySet>)))
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

        internal static readonly EdmModelValidationRule<EdmEntityContainer>
            EdmEntityContainer_InvalidEntitySetNameReference =
                new EdmModelValidationRule<EdmEntityContainer>(
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

        internal static readonly EdmModelValidationRule<EdmEntityContainer>
            EdmEntityContainer_ConcurrencyRedefinedOnSubTypeOfEntitySetType
                =
                new EdmModelValidationRule<EdmEntityContainer>(
                    (context, edmEntityContainer) =>
                        {
                            var baseEntitySetTypes = new Dictionary<EdmEntityType, EdmEntitySet>();
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
                            foreach (var entityType in context.ModelParentMap.NamespaceItems.OfType<EdmEntityType>())
                            {
                                EdmEntitySet set;
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

        internal static readonly EdmModelValidationRule<EdmEntityContainer>
            EdmEntityContainer_DuplicateEntityContainerMemberName =
                new EdmModelValidationRule<EdmEntityContainer>(
                    (context, edmEntityContainer) =>
                        {
                            var memberNameList = new HashSet<string>();
                            foreach (var item in edmEntityContainer.ContainerItems)
                            {
                                DataModelValidationHelper.AddMemberNameToHashSet(
                                    item,
                                    memberNameList,
                                    context,
                                    Strings.EdmModel_Validator_Semantic_DuplicateEntityContainerMemberName);
                            }
                        }
                    );

        #endregion

        #region EdmEntitySet

        internal static readonly EdmModelValidationRule<EdmEntitySet> EdmEntitySet_EntitySetTypeHasNoKeys =
            new EdmModelValidationRule<EdmEntitySet>(
                (context, edmEntitySet) =>
                    {
                        if (edmEntitySet.ElementType != null)
                        {
                            if (edmEntitySet.ElementType.GetValidKey().Count() == 0)
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

        #endregion

        #region EdmAssociationSet

        internal static readonly EdmModelValidationRule<EdmAssociationSet> EdmAssociationSet_DuplicateEndName =
            new EdmModelValidationRule<EdmAssociationSet>(
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

        #endregion

        #region EdmEntityType

        internal static readonly EdmModelValidationRule<EdmEntityType>
            EdmEntityType_DuplicatePropertyNameSpecifiedInEntityKey =
                new EdmModelValidationRule<EdmEntityType>(
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

        internal static readonly EdmModelValidationRule<EdmEntityType> EdmEntityType_InvalidKeyNullablePart =
            new EdmModelValidationRule<EdmEntityType>(
                (context, edmEntityType) =>
                    {
                        foreach (var key in edmEntityType.GetValidKey())
                        {
                            if (key.PropertyType.IsPrimitiveType)
                            {
                                if (key.PropertyType.IsNullable.HasValue
                                    &&
                                    key.PropertyType.IsNullable.Value)
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

        internal static readonly EdmModelValidationRule<EdmEntityType> EdmEntityType_EntityKeyMustBeScalar =
            new EdmModelValidationRule<EdmEntityType>(
                (context, edmEntityType) =>
                    {
                        foreach (var key in edmEntityType.GetValidKey())
                        {
                            if (!key.PropertyType.IsUnderlyingPrimitiveType)
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

        internal static readonly EdmModelValidationRule<EdmEntityType> EdmEntityType_InvalidKeyKeyDefinedInBaseClass =
            new EdmModelValidationRule<EdmEntityType>(
                (context, edmEntityType) =>
                    {
                        if (edmEntityType.BaseType != null &&
                            edmEntityType.DeclaredKeyProperties != null
                            &&
                            edmEntityType.DeclaredKeyProperties.Count() > 0)
                        {
                            context.AddError(
                                edmEntityType.BaseType,
                                null,
                                Strings.EdmModel_Validator_Semantic_InvalidKeyKeyDefinedInBaseClass(
                                    edmEntityType.Name, edmEntityType.BaseType.Name),
                                XmlErrorCode.InvalidKey);
                        }
                    });

        internal static readonly EdmModelValidationRule<EdmEntityType> EdmEntityType_KeyMissingOnEntityType =
            new EdmModelValidationRule<EdmEntityType>(
                (context, edmEntityType) =>
                    {
                        if (edmEntityType.BaseType == null
                            && edmEntityType.DeclaredKeyProperties.Count() == 0)
                        {
                            context.AddError(
                                edmEntityType,
                                null,
                                Strings.EdmModel_Validator_Semantic_KeyMissingOnEntityType(edmEntityType.Name),
                                XmlErrorCode.KeyMissingOnEntityType);
                        }
                    });

        internal static readonly EdmModelValidationRule<EdmEntityType> EdmEntityType_InvalidMemberNameMatchesTypeName =
            new EdmModelValidationRule<EdmEntityType>(
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

                            if (edmEntityType.HasDeclaredNavigationProperties)
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

        internal static readonly EdmModelValidationRule<EdmEntityType> EdmEntityType_PropertyNameAlreadyDefinedDuplicate
            =
            new EdmModelValidationRule<EdmEntityType>(
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
                                        (name) =>
                                        Strings.EdmModel_Validator_Semantic_PropertyNameAlreadyDefinedDuplicate(name));
                                }
                            }
                        }

                        if (edmEntityType.HasDeclaredNavigationProperties)
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
                                            (name) =>
                                            Strings.EdmModel_Validator_Semantic_PropertyNameAlreadyDefinedDuplicate(
                                                name));
                                    }
                                }
                            }
                        }
                    });

        internal static readonly EdmModelValidationRule<EdmEntityType> EdmEntityType_CycleInTypeHierarchy =
            new EdmModelValidationRule<EdmEntityType>(
                (context, edmEntityType) =>
                    {
                        if (DataModelValidationHelper.CheckForInheritanceCycle(edmEntityType, et => et.BaseType))
                        {
                            context.AddError(
                                edmEntityType,
                                CsdlConstants.Attribute_BaseType,
                                Strings.EdmModel_Validator_Semantic_CycleInTypeHierarchy(
                                    edmEntityType.GetQualifiedName(context.GetQualifiedPrefix(edmEntityType))),
                                XmlErrorCode.CycleInTypeHierarchy);
                        }
                    });

        #endregion

        #region EdmNavigationProperty

        internal static readonly EdmModelValidationRule<EdmNavigationProperty>
            EdmNavigationProperty_BadNavigationPropertyUndefinedRole =
                new EdmModelValidationRule<EdmNavigationProperty>(
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

        internal static readonly EdmModelValidationRule<EdmNavigationProperty>
            EdmNavigationProperty_BadNavigationPropertyRolesCannotBeTheSame =
                new EdmModelValidationRule<EdmNavigationProperty>(
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

        #endregion

        #region EdmAssociationType

        internal static readonly EdmModelValidationRule<EdmAssociationType>
            EdmAssociationType_InvalidOperationMultipleEndsInAssociation =
                new EdmModelValidationRule<EdmAssociationType>(
                    (context, edmAssociationType) =>
                        {
                            if ((edmAssociationType.SourceEnd != null
                                 && edmAssociationType.SourceEnd.DeleteAction.HasValue
                                 && edmAssociationType.SourceEnd.DeleteAction.Value != EdmOperationAction.None)
                                &&
                                (edmAssociationType.TargetEnd != null
                                 && edmAssociationType.TargetEnd.DeleteAction.HasValue
                                 && edmAssociationType.TargetEnd.DeleteAction.Value != EdmOperationAction.None))
                            {
                                context.AddError(
                                    edmAssociationType,
                                    null,
                                    Strings.EdmModel_Validator_Semantic_InvalidOperationMultipleEndsInAssociation,
                                    XmlErrorCode.InvalidOperation);
                            }
                        });

        internal static readonly EdmModelValidationRule<EdmAssociationType>
            EdmAssociationType_EndWithManyMultiplicityCannotHaveOperationsSpecified =
                new EdmModelValidationRule<EdmAssociationType>(
                    (context, edmAssociationType) =>
                        {
                            if (edmAssociationType.SourceEnd != null)
                            {
                                // Check if the end has multiplicity as many, it cannot have any operation behaviour
                                if (edmAssociationType.SourceEnd.EndKind == EdmAssociationEndKind.Many &&
                                    edmAssociationType.SourceEnd.DeleteAction.HasValue
                                    &&
                                    edmAssociationType.SourceEnd.DeleteAction.Value != EdmOperationAction.None)
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
                                if (edmAssociationType.TargetEnd.EndKind == EdmAssociationEndKind.Many &&
                                    edmAssociationType.TargetEnd.DeleteAction.HasValue
                                    &&
                                    edmAssociationType.TargetEnd.DeleteAction.Value != EdmOperationAction.None)
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

        internal static readonly EdmModelValidationRule<EdmAssociationType>
            EdmAssociationType_EndNameAlreadyDefinedDuplicate =
                new EdmModelValidationRule<EdmAssociationType>(
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

        internal static readonly EdmModelValidationRule<EdmAssociationType>
            EdmAssociationType_SameRoleReferredInReferentialConstraint =
                new EdmModelValidationRule<EdmAssociationType>(
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

        internal static readonly EdmModelValidationRule<EdmAssociationType>
            EdmAssociationType_ValidateReferentialConstraint =
                new EdmModelValidationRule<EdmAssociationType>(
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
                                    constraint.DependentProperties.ToList(),
                                    dependentRoleEnd,
                                    out isPrincipalRoleKeyProperty,
                                    out areAllDependentRolePropertiesNullable,
                                    out isAnyDependentRolePropertyNullable,
                                    out isDependentRolePropertiesSubsetofKeyProperties);

                                // Resolve all the property in the principal end attribute. Also checks whether this is nullable or not and 
                                // whether the properties are the keys for the type in the principal role
                                DataModelValidationHelper.IsKeyProperty(
                                    constraint.PrincipalEnd(edmAssociationType).EntityType.GetValidKey().ToList(),
                                    principalRoleEnd,
                                    out isDependentRoleKeyProperty,
                                    out areAllPrinicipalRolePropertiesNullable,
                                    out isAnyPrinicipalRolePropertyNullable,
                                    out isPrinicipalRolePropertiesSubsetofKeyProperties);

                                Contract.Assert(
                                    constraint.PrincipalEnd(edmAssociationType).EntityType.GetValidKey().Count() > 0,
                                    "There should be some ref properties in Principal Role");
                                Contract.Assert(
                                    constraint.DependentProperties.Count() != 0,
                                    "There should be some ref properties in Dependent Role");
                                Contract.Assert(
                                    isDependentRoleKeyProperty,
                                    "The properties in the PrincipalRole must be the key of the Entity type referred to by the principal role");

                                // TODO fix the versioning after the real version class from the builder class checkin
                                var v1Behavior = context.ValidationContextVersion <= DataModelVersions.Version1_1;

                                // Since the FromProperty must be the key of the FromRole, the FromRole cannot be '*' as multiplicity
                                // Also the lower bound of multiplicity of FromRole can be zero if and only if all the properties in 
                                // ToProperties are nullable
                                // for v2+
                                if (principalRoleEnd.EndKind
                                    == EdmAssociationEndKind.Many)
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
                                         && principalRoleEnd.EndKind == EdmAssociationEndKind.Required)
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
                                         && principalRoleEnd.EndKind != EdmAssociationEndKind.Required)
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
                                            dependentRoleEnd.EntityType.GetQualifiedName(
                                                context.GetQualifiedPrefix(dependentRoleEnd.EntityType)),
                                            edmAssociationType.GetQualifiedName(
                                                context.GetQualifiedPrefix(edmAssociationType))),
                                        XmlErrorCode.InvalidPropertyInRelationshipConstraint);
                                }

                                // If the principal role property is a key property, then the upper bound must be 1 i.e. every parent (from property) can 
                                // have exactly one child
                                if (isPrincipalRoleKeyProperty)
                                {
                                    if (dependentRoleEnd.EndKind
                                        == EdmAssociationEndKind.Many)
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
                                else if (dependentRoleEnd.EndKind
                                         != EdmAssociationEndKind.Many)
                                {
                                    context.AddError(
                                        dependentRoleEnd,
                                        null,
                                        Strings.
                                            EdmModel_Validator_Semantic_InvalidMultiplicityToRoleUpperBoundMustBeMany(
                                                dependentRoleEnd.Name, edmAssociationType.Name),
                                        XmlErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint);
                                }
                                var keyProperties_PrincipalRoleEnd = principalRoleEnd.EntityType.GetValidKey().ToList();
                                var dependentProperties = constraint.DependentProperties.ToList();

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
                                            principalProperty.PropertyType != null
                                            && dependentProperty.PropertyType != null &&
                                            principalProperty.PropertyType.IsPrimitiveType
                                            && dependentProperty.PropertyType.IsPrimitiveType)
                                        {
                                            if (!DataModelValidationHelper.IsPrimitiveTypesEqual(
                                                dependentProperty.PropertyType,
                                                principalProperty.PropertyType))
                                            {
                                                context.AddError(
                                                    constraint,
                                                    null,
                                                    Strings.
                                                        EdmModel_Validator_Semantic_TypeMismatchRelationshipConstraint(
                                                            constraint.DependentProperties.ToList()[i].Name,
                                                            dependentRoleEnd.EntityType.Name,
                                                            keyProperties_PrincipalRoleEnd[i].Name,
                                                            principalRoleEnd.EntityType.Name,
                                                            edmAssociationType.Name
                                                        ),
                                                    XmlErrorCode.TypeMismatchRelationshipConstraint);
                                            }
                                        }
                                    }
                                }
                            }
                        });

        internal static readonly EdmModelValidationRule<EdmAssociationType>
            EdmAssociationType_InvalidPropertyInRelationshipConstraint =
                new EdmModelValidationRule<EdmAssociationType>(
                    (context, edmAssociationType) =>
                        {
                            if (edmAssociationType.Constraint != null &&
                                edmAssociationType.Constraint.DependentEnd != null
                                &&
                                edmAssociationType.Constraint.DependentEnd.EntityType != null)
                            {
                                var dependentEndProperties =
                                    edmAssociationType.Constraint.DependentEnd.EntityType.Properties.ToList();
                                foreach (var property in edmAssociationType.Constraint.DependentProperties)
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

        #endregion

        #region EdmComplexType

        internal static readonly EdmModelValidationRule<EdmComplexType> EdmComplexType_InvalidIsAbstract =
            new EdmModelValidationRule<EdmComplexType>(
                (context, edmComplexType) =>
                    {
                        if (edmComplexType.IsAbstract)
                        {
                            context.AddError(
                                edmComplexType,
                                EdmConstants.Property_IsAbstract,
                                Strings.EdmModel_Validator_Semantic_InvalidComplexTypeAbstract(
                                    edmComplexType.GetQualifiedName(context.GetQualifiedPrefix(edmComplexType))),
                                XmlErrorCode.EdmModel_Validator_InvalidAbstractComplexType);
                        }
                    });

        internal static readonly EdmModelValidationRule<EdmComplexType> EdmComplexType_InvalidIsPolymorphic =
            new EdmModelValidationRule<EdmComplexType>(
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

        internal static readonly EdmModelValidationRule<EdmComplexType> EdmComplexType_InvalidMemberNameMatchesTypeName
            =
            new EdmModelValidationRule<EdmComplexType>(
                (context, edmComplexType) =>
                    {
                        if (edmComplexType.Name.HasContent()
                            && edmComplexType.HasDeclaredProperties)
                        {
                            foreach (var property in edmComplexType.DeclaredProperties)
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

        internal static readonly EdmModelValidationRule<EdmComplexType>
            EdmComplexType_PropertyNameAlreadyDefinedDuplicate =
                new EdmModelValidationRule<EdmComplexType>(
                    (context, edmComplexType) =>
                        {
                            if (edmComplexType.HasDeclaredProperties)
                            {
                                var propertyNames = new HashSet<string>();
                                foreach (var property in edmComplexType.DeclaredProperties)
                                {
                                    if (property.Name.HasContent())
                                    {
                                        DataModelValidationHelper.AddMemberNameToHashSet(
                                            property,
                                            propertyNames,
                                            context,
                                            (name) =>
                                            Strings.EdmModel_Validator_Semantic_PropertyNameAlreadyDefinedDuplicate(
                                                name));
                                    }
                                }
                            }
                        });

        internal static readonly EdmModelValidationRule<EdmComplexType>
            EdmComplexType_PropertyNameAlreadyDefinedDuplicate_V1_1 =
                new EdmModelValidationRule<EdmComplexType>(
                    (context, edmComplexType) =>
                        {
                            if (edmComplexType.HasDeclaredProperties)
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
                                                (name) =>
                                                Strings.EdmModel_Validator_Semantic_PropertyNameAlreadyDefinedDuplicate(
                                                    name));
                                        }
                                    }
                                }
                            }
                        });

        internal static readonly EdmModelValidationRule<EdmComplexType> EdmComplexType_CycleInTypeHierarchy_V1_1 =
            new EdmModelValidationRule<EdmComplexType>(
                (context, edmComplexType) =>
                    {
                        if (DataModelValidationHelper.CheckForInheritanceCycle(edmComplexType, ct => ct.BaseType))
                        {
                            context.AddError(
                                edmComplexType,
                                CsdlConstants.Attribute_BaseType,
                                Strings.EdmModel_Validator_Semantic_CycleInTypeHierarchy(
                                    edmComplexType.GetQualifiedName(context.GetQualifiedPrefix(edmComplexType))),
                                XmlErrorCode.CycleInTypeHierarchy);
                        }
                    });

        #endregion

        #region EdmProperty

        internal static readonly EdmModelValidationRule<EdmProperty> EdmProperty_InvalidCollectionKind =
            new EdmModelValidationRule<EdmProperty>(
                (context, edmProperty) =>
                    {
                        if (edmProperty.CollectionKind
                            != EdmCollectionKind.Default)
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
                        if (edmProperty.CollectionKind != EdmCollectionKind.Default &&
                            edmProperty.PropertyType != null
                            &&
                            !edmProperty.PropertyType.IsCollectionType)
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
                        if (edmProperty.PropertyType != null)
                        {
                            if (edmProperty.PropertyType.ComplexType != null)
                            {
                                if (edmProperty.PropertyType.IsNullable.HasValue
                                    &&
                                    edmProperty.PropertyType.IsNullable.Value)
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
                        if (edmProperty.PropertyType != null
                            &&
                            edmProperty.PropertyType.EdmType != null)
                        {
                            if (!edmProperty.PropertyType.IsPrimitiveType
                                &&
                                !edmProperty.PropertyType.IsComplexType)
                            {
                                context.AddError(
                                    edmProperty,
                                    CsdlConstants.Attribute_Type,
                                    Strings.EdmModel_Validator_Semantic_InvalidPropertyType(
                                        (edmProperty.PropertyType.IsCollectionType
                                             ? EdmConstants.Value_CollectionType
                                             : edmProperty.PropertyType.EdmType.ItemKind.ToString())),
                                    XmlErrorCode.InvalidPropertyType);
                            }
                        }
                    });

        internal static readonly EdmModelValidationRule<EdmProperty> EdmProperty_InvalidPropertyType_V1_1 =
            new EdmModelValidationRule<EdmProperty>(
                (context, edmProperty) =>
                    {
                        if (edmProperty.PropertyType != null
                            &&
                            edmProperty.PropertyType.EdmType != null)
                        {
                            if (!edmProperty.PropertyType.IsPrimitiveType &&
                                !edmProperty.PropertyType.IsComplexType
                                &&
                                !edmProperty.PropertyType.IsCollectionType)
                            {
                                context.AddError(
                                    edmProperty,
                                    CsdlConstants.Attribute_Type,
                                    Strings.EdmModel_Validator_Semantic_InvalidPropertyType_V1_1(
                                        edmProperty.PropertyType.EdmType.ItemKind.ToString()),
                                    XmlErrorCode.InvalidPropertyType);
                            }
                        }
                    });

        internal static readonly EdmModelValidationRule<EdmProperty> EdmProperty_InvalidPropertyType_V3 =
            new EdmModelValidationRule<EdmProperty>(
                (context, edmProperty) =>
                    {
                        if (edmProperty.PropertyType != null
                            &&
                            edmProperty.PropertyType.EdmType != null)
                        {
                            if (!edmProperty.PropertyType.IsPrimitiveType &&
                                !edmProperty.PropertyType.IsComplexType
                                &&
                                !edmProperty.PropertyType.IsEnumType)
                            {
                                context.AddError(
                                    edmProperty,
                                    CsdlConstants.Attribute_Type,
                                    Strings.EdmModel_Validator_Semantic_InvalidPropertyType_V3(
                                        edmProperty.PropertyType.EdmType.ItemKind.ToString()),
                                    XmlErrorCode.InvalidPropertyType);
                            }
                        }
                    });

        #endregion

        #region EdmTypeReference

        // TODO: Need the EdmProviderManifest to have the facet description to validate on the PrimitiveTypeFacets

        #endregion

        #region EdmNamespace

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

        #endregion
    }
}
