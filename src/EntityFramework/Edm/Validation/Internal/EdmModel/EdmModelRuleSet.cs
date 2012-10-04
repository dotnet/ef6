// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation.Internal.EdmModel
{
    using System.Data.Entity.Edm.Common;
    using System.Diagnostics;

    /// <summary>
    ///     The RuleSet for EdmModel
    /// </summary>
    internal abstract class EdmModelRuleSet : DataModelValidationRuleSet
    {
        private EdmModelRuleSet(bool validateSyntax)
        {
            #region Common Syntax Rules

            if (validateSyntax)
            {
                AddRule(EdmModelSyntacticValidationRules.EdmAssociationConstraint_DependentEndMustNotBeNull);
                AddRule(EdmModelSyntacticValidationRules.EdmAssociationConstraint_DependentPropertiesMustNotBeEmpty);
                AddRule(EdmModelSyntacticValidationRules.EdmAssociationEnd_EntityTypeMustNotBeNull);
                AddRule(EdmModelSyntacticValidationRules.EdmAssociationSet_ElementTypeMustNotBeNull);
                AddRule(EdmModelSyntacticValidationRules.EdmAssociationSet_SourceSetMustNotBeNull);
                AddRule(EdmModelSyntacticValidationRules.EdmAssociationSet_TargetSetMustNotBeNull);
                AddRule(EdmModelSyntacticValidationRules.EdmAssociationType_AssocationEndMustNotBeNull);
                AddRule(EdmModelSyntacticValidationRules.EdmEntitySet_ElementTypeMustNotBeNull);
                AddRule(EdmModelSyntacticValidationRules.EdmModel_NameMustNotBeEmptyOrWhiteSpace);
                AddRule(EdmModelSyntacticValidationRules.EdmModel_NameIsTooLong);
                AddRule(EdmModelSyntacticValidationRules.EdmModel_NameIsNotAllowed);
                AddRule(EdmModelSyntacticValidationRules.EdmNavigationProperty_AssocationMustNotBeNull);
                AddRule(EdmModelSyntacticValidationRules.EdmNavigationProperty_ResultEndMustNotBeNull);
                AddRule(EdmModelSyntacticValidationRules.EdmTypeReference_TypeNotValid);
            }

            #endregion

            #region Common Semantic Rules

            AddRule(EdmModelSemanticValidationRules.EdmModel_SystemNamespaceEncountered);

            AddRule(EdmModelSemanticValidationRules.EdmEntityContainer_SimilarRelationshipEnd);
            AddRule(EdmModelSemanticValidationRules.EdmEntityContainer_InvalidEntitySetNameReference);
            AddRule(EdmModelSemanticValidationRules.EdmEntityContainer_ConcurrencyRedefinedOnSubTypeOfEntitySetType);
            AddRule(EdmModelSemanticValidationRules.EdmEntityContainer_DuplicateEntityContainerMemberName);

            AddRule(EdmModelSemanticValidationRules.EdmEntitySet_EntitySetTypeHasNoKeys);

            AddRule(EdmModelSemanticValidationRules.EdmAssociationSet_DuplicateEndName);

            AddRule(EdmModelSemanticValidationRules.EdmEntityType_EntityKeyMustBeScalar);
            AddRule(EdmModelSemanticValidationRules.EdmEntityType_DuplicatePropertyNameSpecifiedInEntityKey);
            AddRule(EdmModelSemanticValidationRules.EdmEntityType_InvalidKeyNullablePart);
            AddRule(EdmModelSemanticValidationRules.EdmEntityType_InvalidKeyKeyDefinedInBaseClass);
            AddRule(EdmModelSemanticValidationRules.EdmEntityType_KeyMissingOnEntityType);
            AddRule(EdmModelSemanticValidationRules.EdmEntityType_InvalidMemberNameMatchesTypeName);
            AddRule(EdmModelSemanticValidationRules.EdmEntityType_PropertyNameAlreadyDefinedDuplicate);
            AddRule(EdmModelSemanticValidationRules.EdmEntityType_CycleInTypeHierarchy);

            AddRule(EdmModelSemanticValidationRules.EdmNavigationProperty_BadNavigationPropertyUndefinedRole);
            AddRule(EdmModelSemanticValidationRules.EdmNavigationProperty_BadNavigationPropertyRolesCannotBeTheSame);

            AddRule(EdmModelSemanticValidationRules.EdmAssociationType_InvalidOperationMultipleEndsInAssociation);
            AddRule(
                EdmModelSemanticValidationRules.EdmAssociationType_EndWithManyMultiplicityCannotHaveOperationsSpecified);
            AddRule(EdmModelSemanticValidationRules.EdmAssociationType_EndNameAlreadyDefinedDuplicate);
            AddRule(EdmModelSemanticValidationRules.EdmAssociationType_InvalidPropertyInRelationshipConstraint);
            AddRule(EdmModelSemanticValidationRules.EdmAssociationType_SameRoleReferredInReferentialConstraint);
            AddRule(EdmModelSemanticValidationRules.EdmAssociationType_ValidateReferentialConstraint);

            AddRule(EdmModelSemanticValidationRules.EdmComplexType_InvalidMemberNameMatchesTypeName);

            AddRule(EdmModelSemanticValidationRules.EdmNamespace_TypeNameAlreadyDefinedDuplicate);

            #endregion
        }

        private abstract class NonV1_1RuleSet : EdmModelRuleSet
        {
            protected NonV1_1RuleSet(bool validateSyntax)
                : base(validateSyntax)
            {
                AddRule(EdmModelSemanticValidationRules.EdmProperty_NullableComplexType);
                AddRule(EdmModelSemanticValidationRules.EdmProperty_InvalidCollectionKind);
                AddRule(EdmModelSemanticValidationRules.EdmComplexType_PropertyNameAlreadyDefinedDuplicate);
                AddRule(EdmModelSemanticValidationRules.EdmComplexType_InvalidIsAbstract);
                AddRule(EdmModelSemanticValidationRules.EdmComplexType_InvalidIsPolymorphic);
            }
        }

        private sealed class V1RuleSet : NonV1_1RuleSet
        {
            internal V1RuleSet(bool validateSyntax)
                : base(validateSyntax)
            {
                AddRule(EdmModelSemanticValidationRules.EdmProperty_InvalidPropertyType);
            }
        }

        private sealed class V1_1RuleSet : EdmModelRuleSet
        {
            internal V1_1RuleSet(bool validateSyntax)
                : base(validateSyntax)
            {
                AddRule(EdmModelSemanticValidationRules.EdmComplexType_PropertyNameAlreadyDefinedDuplicate_V1_1);
                AddRule(EdmModelSemanticValidationRules.EdmComplexType_CycleInTypeHierarchy_V1_1);
                AddRule(EdmModelSemanticValidationRules.EdmProperty_InvalidCollectionKind_V1_1);
                AddRule(EdmModelSemanticValidationRules.EdmProperty_InvalidPropertyType_V1_1);
            }
        }

        private class V2RuleSet : NonV1_1RuleSet
        {
            internal V2RuleSet(bool validateSyntax)
                : base(validateSyntax)
            {
                AddRule(EdmModelSemanticValidationRules.EdmProperty_InvalidPropertyType);
            }
        }

        private sealed class V3RuleSet : V2RuleSet
        {
            internal V3RuleSet(bool validateSyntax)
                : base(validateSyntax)
            {
                RemoveRule(EdmModelSemanticValidationRules.EdmProperty_InvalidPropertyType);
                AddRule(EdmModelSemanticValidationRules.EdmProperty_InvalidPropertyType_V3);
            }
        }

        /// <summary>
        ///     Get <see cref="EdmModelRuleSet" /> based on version
        /// </summary>
        /// <param name="version"> a double value of version </param>
        /// <returns> <see cref="EdmModelRuleSet" /> </returns>
        internal static EdmModelRuleSet CreateEdmModelRuleSet(double version, bool validateSyntax)
        {
            if (version == DataModelVersions.Version1)
            {
                return new V1RuleSet(validateSyntax);
            }

            if (version == DataModelVersions.Version1_1)
            {
                return new V1_1RuleSet(validateSyntax);
            }

            if (version == DataModelVersions.Version2)
            {
                return new V2RuleSet(validateSyntax);
            }

            if (version == DataModelVersions.Version3)
            {
                return new V3RuleSet(validateSyntax);
            }

            Debug.Fail("Added new version?");
            return null;
        }
    }
}
