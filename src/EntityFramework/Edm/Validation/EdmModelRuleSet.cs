// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    internal abstract class EdmModelRuleSet : DataModelValidationRuleSet
    {
        public static EdmModelRuleSet CreateEdmModelRuleSet(double version, bool validateSyntax)
        {
            if (Equals(version, XmlConstants.EdmVersionForV1))
            {
                return new V1RuleSet(validateSyntax);
            }

            if (Equals(version, XmlConstants.EdmVersionForV1_1))
            {
                return new V1_1RuleSet(validateSyntax);
            }

            if (Equals(version, XmlConstants.EdmVersionForV2))
            {
                return new V2RuleSet(validateSyntax);
            }

            if (Equals(version, XmlConstants.EdmVersionForV3))
            {
                return new V3RuleSet(validateSyntax);
            }

            Debug.Fail("Added new version?");

            return null;
        }

        private EdmModelRuleSet(bool validateSyntax)
        {
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

            AddRule(EdmModelSemanticValidationRules.EdmType_SystemNamespaceEncountered);
            AddRule(EdmModelSemanticValidationRules.EdmEntityContainer_SimilarRelationshipEnd);
            AddRule(EdmModelSemanticValidationRules.EdmEntityContainer_InvalidEntitySetNameReference);
            AddRule(EdmModelSemanticValidationRules.EdmEntityContainer_ConcurrencyRedefinedOnSubTypeOfEntitySetType);
            AddRule(EdmModelSemanticValidationRules.EdmEntityContainer_DuplicateEntityContainerMemberName);
            AddRule(EdmModelSemanticValidationRules.EdmEntityContainer_DuplicateEntitySetTable);
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
            AddRule(EdmModelSemanticValidationRules.EdmNavigationProperty_BadNavigationPropertyBadFromRoleType);
            AddRule(EdmModelSemanticValidationRules.EdmAssociationType_InvalidOperationMultipleEndsInAssociation);
            AddRule(EdmModelSemanticValidationRules.EdmAssociationType_EndWithManyMultiplicityCannotHaveOperationsSpecified);
            AddRule(EdmModelSemanticValidationRules.EdmAssociationType_EndNameAlreadyDefinedDuplicate);
            AddRule(EdmModelSemanticValidationRules.EdmAssociationType_InvalidPropertyInRelationshipConstraint);
            AddRule(EdmModelSemanticValidationRules.EdmAssociationType_SameRoleReferredInReferentialConstraint);
            AddRule(EdmModelSemanticValidationRules.EdmAssociationType_ValidateReferentialConstraint);
            AddRule(EdmModelSemanticValidationRules.EdmComplexType_InvalidMemberNameMatchesTypeName);
            AddRule(EdmModelSemanticValidationRules.EdmNamespace_TypeNameAlreadyDefinedDuplicate);
            AddRule(EdmModelSemanticValidationRules.EdmFunction_DuplicateParameterName);
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
                AddRule(EdmModelSemanticValidationRules.EdmFunction_ComposableFunctionImportsNotAllowed_V1_V2);
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
                RemoveRule(EdmModelSemanticValidationRules.EdmFunction_ComposableFunctionImportsNotAllowed_V1_V2);
            }
        }
    }
}
