// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Edm.Validation.Internal.EdmModel
{
    using System.Data.Entity.Edm.Internal;
    using System.Data.Entity.Edm.Parsing.Xml.Internal;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Csdl;
    using System.Data.Entity.Resources;
    using System.Linq;

    internal static class EdmModelSyntacticValidationRules
    {
        #region EdmNamedDataModelItem

        internal static readonly EdmModelValidationRule<EdmNamedMetadataItem> EdmModel_NameMustNotBeEmptyOrWhiteSpace =
            new EdmModelValidationRule<EdmNamedMetadataItem>(
                (context, item) =>
                    {
                        if (!item.Name.HasContent())
                        {
                            context.AddError(
                                item,
                                CsdlConstants.Attribute_Name,
                                Strings.EdmModel_Validator_Syntactic_MissingName,
                                XmlErrorCode.InvalidName);
                        }
                    }
                );

        internal static readonly EdmModelValidationRule<EdmNamedMetadataItem> EdmModel_NameIsTooLong =
            new EdmModelValidationRule<EdmNamedMetadataItem>(
                (context, item) =>
                    {
                        if (item.Name.HasContent())
                        {
                            // max length is hard coded in the xsd
                            if (item.Name.Length > 480)
                            {
                                context.AddError(
                                    item,
                                    CsdlConstants.Attribute_Name,
                                    Strings.EdmModel_Validator_Syntactic_EdmModel_NameIsTooLong(item.Name),
                                    XmlErrorCode.InvalidName);
                            }
                        }
                    }
                );

        internal static readonly EdmModelValidationRule<EdmNamedMetadataItem> EdmModel_NameIsNotAllowed =
            new EdmModelValidationRule<EdmNamedMetadataItem>(
                (context, item) =>
                    {
                        if (item.Name.HasContent())
                        {
                            // max length is hard coded in the xsd
                            if (item.Name.Length < 480)
                            {
                                if (!(item is EdmQualifiedNameMetadataItem
                                          ? EdmUtil.IsValidQualifiedItemName(item.Name)
                                          : EdmUtil.IsValidDataModelItemName(item.Name)))
                                {
                                    context.AddError(
                                        item,
                                        CsdlConstants.Attribute_Name,
                                        Strings.EdmModel_Validator_Syntactic_EdmModel_NameIsNotAllowed(item.Name),
                                        XmlErrorCode.InvalidName);
                                }
                            }
                        }
                    }
                );

        #endregion

        #region EdmProperty

        #endregion

        #region EdmAssociationType

        internal static readonly EdmModelValidationRule<EdmAssociationType>
            EdmAssociationType_AssocationEndMustNotBeNull =
                new EdmModelValidationRule<EdmAssociationType>(
                    (context, edmAssociationType) =>
                        {
                            if (edmAssociationType.SourceEnd == null
                                ||
                                edmAssociationType.TargetEnd == null)
                            {
                                context.AddError(
                                    edmAssociationType,
                                    CsdlConstants.Element_End,
                                    Strings.EdmModel_Validator_Syntactic_EdmAssociationType_AssocationEndMustNotBeNull,
                                    XmlErrorCode.EdmAssociationType_AssocationEndMustNotBeNull);
                            }
                        }
                    );

        #endregion

        #region EdmAssociationConstraint

        internal static readonly EdmModelValidationRule<EdmAssociationConstraint>
            EdmAssociationConstraint_DependentEndMustNotBeNull =
                new EdmModelValidationRule<EdmAssociationConstraint>(
                    (context, edmAssociationConstraint) =>
                        {
                            if (edmAssociationConstraint.DependentEnd == null)
                            {
                                context.AddError(
                                    edmAssociationConstraint,
                                    CsdlConstants.Element_Dependent,
                                    Strings.
                                        EdmModel_Validator_Syntactic_EdmAssociationConstraint_DependentEndMustNotBeNull,
                                    XmlErrorCode.EdmAssociationConstraint_DependentEndMustNotBeNull);
                            }
                        }
                    );

        internal static readonly EdmModelValidationRule<EdmAssociationConstraint>
            EdmAssociationConstraint_DependentPropertiesMustNotBeEmpty
                =
                new EdmModelValidationRule<EdmAssociationConstraint>(
                    (context, edmAssociationConstraint) =>
                        {
                            if (edmAssociationConstraint.DependentProperties == null
                                ||
                                edmAssociationConstraint.DependentProperties.Count() == 0)
                            {
                                context.AddError(
                                    edmAssociationConstraint,
                                    CsdlConstants.Element_Dependent,
                                    Strings.
                                        EdmModel_Validator_Syntactic_EdmAssociationConstraint_DependentPropertiesMustNotBeEmpty,
                                    XmlErrorCode.EdmAssociationConstraint_DependentPropertiesMustNotBeEmpty);
                            }
                        }
                    );

        #endregion

        #region EdmNavigationProperty

        internal static readonly EdmModelValidationRule<EdmNavigationProperty>
            EdmNavigationProperty_AssocationMustNotBeNull =
                new EdmModelValidationRule<EdmNavigationProperty>(
                    (context, edmNavigationProperty) =>
                        {
                            if (edmNavigationProperty.Association == null)
                            {
                                context.AddError(
                                    edmNavigationProperty,
                                    CsdlConstants.Attribute_Relationship,
                                    Strings.EdmModel_Validator_Syntactic_EdmNavigationProperty_AssocationMustNotBeNull,
                                    XmlErrorCode.EdmNavigationProperty_AssocationMustNotBeNull);
                            }
                        }
                    );

        internal static readonly EdmModelValidationRule<EdmNavigationProperty>
            EdmNavigationProperty_ResultEndMustNotBeNull =
                new EdmModelValidationRule<EdmNavigationProperty>(
                    (context, edmNavigationProperty) =>
                        {
                            if (edmNavigationProperty.ResultEnd == null)
                            {
                                context.AddError(
                                    edmNavigationProperty,
                                    CsdlConstants.Attribute_ResultEnd,
                                    Strings.EdmModel_Validator_Syntactic_EdmNavigationProperty_ResultEndMustNotBeNull,
                                    XmlErrorCode.EdmNavigationProperty_ResultEndMustNotBeNull);
                            }
                        }
                    );

        #endregion

        #region EdmAssociationEnd

        internal static readonly EdmModelValidationRule<EdmAssociationEnd> EdmAssociationEnd_EntityTypeMustNotBeNull =
            new EdmModelValidationRule<EdmAssociationEnd>(
                (context, edmAssociationEnd) =>
                    {
                        if (edmAssociationEnd.EntityType == null)
                        {
                            context.AddError(
                                edmAssociationEnd,
                                CsdlConstants.Attribute_Type,
                                Strings.EdmModel_Validator_Syntactic_EdmAssociationEnd_EntityTypeMustNotBeNull,
                                XmlErrorCode.EdmAssociationEnd_EntityTypeMustNotBeNull);
                        }
                    }
                );

        #endregion

        #region EdmEntitySet

        internal static readonly EdmModelValidationRule<EdmEntitySet> EdmEntitySet_ElementTypeMustNotBeNull =
            new EdmModelValidationRule<EdmEntitySet>(
                (context, edmEntitySet) =>
                    {
                        if (edmEntitySet.ElementType == null)
                        {
                            context.AddError(
                                edmEntitySet,
                                CsdlConstants.Property_ElementType,
                                Strings.EdmModel_Validator_Syntactic_EdmEntitySet_ElementTypeMustNotBeNull,
                                XmlErrorCode.EdmEntitySet_ElementTypeMustNotBeNull);
                        }
                    }
                );

        #endregion

        #region EdmAssociationSet

        internal static readonly EdmModelValidationRule<EdmAssociationSet> EdmAssociationSet_ElementTypeMustNotBeNull =
            new EdmModelValidationRule<EdmAssociationSet>(
                (context, edmAssociationSet) =>
                    {
                        if (edmAssociationSet.ElementType == null)
                        {
                            context.AddError(
                                edmAssociationSet,
                                CsdlConstants.Property_ElementType,
                                Strings.EdmModel_Validator_Syntactic_EdmAssociationSet_ElementTypeMustNotBeNull,
                                XmlErrorCode.EdmAssociationSet_ElementTypeMustNotBeNull);
                        }
                    }
                );

        internal static readonly EdmModelValidationRule<EdmAssociationSet> EdmAssociationSet_SourceSetMustNotBeNull =
            new EdmModelValidationRule<EdmAssociationSet>(
                (context, edmAssociationSet) =>
                    {
                        if (edmAssociationSet.SourceSet == null)
                        {
                            context.AddError(
                                edmAssociationSet,
                                CsdlConstants.Property_SourceSet,
                                // Need special handling in the parser location handler
                                Strings.EdmModel_Validator_Syntactic_EdmAssociationSet_SourceSetMustNotBeNull,
                                XmlErrorCode.EdmAssociationSet_SourceSetMustNotBeNull);
                        }
                    }
                );

        internal static readonly EdmModelValidationRule<EdmAssociationSet> EdmAssociationSet_TargetSetMustNotBeNull =
            new EdmModelValidationRule<EdmAssociationSet>(
                (context, edmAssociationSet) =>
                    {
                        if (edmAssociationSet.TargetSet == null)
                        {
                            context.AddError(
                                edmAssociationSet,
                                CsdlConstants.Property_TargetSet,
                                // Need special handling in the parser location handler
                                Strings.EdmModel_Validator_Syntactic_EdmAssociationSet_TargetSetMustNotBeNull,
                                XmlErrorCode.EdmAssociationSet_TargetSetMustNotBeNull);
                        }
                    }
                );

        #endregion

        #region EdmTypeReference

        internal static readonly EdmModelValidationRule<EdmTypeReference> EdmTypeReference_TypeNotValid =
            new EdmModelValidationRule<EdmTypeReference>(
                (context, edmTypeReference) =>
                    {
                        if (!DataModelValidationHelper.IsEdmTypeReferenceValid(edmTypeReference))
                        {
                            context.AddError(
                                edmTypeReference,
                                null,
                                Strings.EdmModel_Validator_Syntactic_EdmTypeReferenceNotValid,
                                XmlErrorCode.EdmTypeReferenceNotValid);
                        }
                    }
                );

        #endregion
    }
}
