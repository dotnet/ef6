// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation.Internal.EdmModel
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Internal;
    using System.Data.Entity.Edm.Parsing.Xml.Internal;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Csdl;
    using System.Data.Entity.Resources;
    using System.Linq;

    internal static class EdmModelSyntacticValidationRules
    {
        internal static readonly EdmModelValidationRule<INamedDataModelItem> EdmModel_NameMustNotBeEmptyOrWhiteSpace =
            new EdmModelValidationRule<INamedDataModelItem>(
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

        internal static readonly EdmModelValidationRule<INamedDataModelItem> EdmModel_NameIsTooLong =
            new EdmModelValidationRule<INamedDataModelItem>(
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

        internal static readonly EdmModelValidationRule<INamedDataModelItem> EdmModel_NameIsNotAllowed =
            new EdmModelValidationRule<INamedDataModelItem>(
                (context, item) =>
                    {
                        if (item.Name.HasContent())
                        {
                            // max length is hard coded in the xsd
                            if (item.Name.Length < 480)
                            {
                                if (!(item is IQualifiedNameMetadataItem
                                          ? EdmUtil.IsValidQualifiedItemName(item.Name)
                                          : EdmUtil.IsValidDataModelItemName(item.Name)))
                                {
                                    context.AddError(
                                        (MetadataItem)item,
                                        CsdlConstants.Attribute_Name,
                                        Strings.EdmModel_Validator_Syntactic_EdmModel_NameIsNotAllowed(item.Name),
                                        XmlErrorCode.InvalidName);
                                }
                            }
                        }
                    }
                );

        internal static readonly EdmModelValidationRule<AssociationType>
            EdmAssociationType_AssocationEndMustNotBeNull =
                new EdmModelValidationRule<AssociationType>(
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

        internal static readonly EdmModelValidationRule<ReferentialConstraint>
            EdmAssociationConstraint_DependentEndMustNotBeNull =
                new EdmModelValidationRule<ReferentialConstraint>(
                    (context, edmAssociationConstraint) =>
                        {
                            if (edmAssociationConstraint.DependentEnd == null)
                            {
                                context.AddError(
                                    edmAssociationConstraint,
                                    CsdlConstants.Element_Dependent,
                                    Strings.EdmModel_Validator_Syntactic_EdmAssociationConstraint_DependentEndMustNotBeNull,
                                    XmlErrorCode.EdmAssociationConstraint_DependentEndMustNotBeNull);
                            }
                        }
                    );

        internal static readonly EdmModelValidationRule<ReferentialConstraint>
            EdmAssociationConstraint_DependentPropertiesMustNotBeEmpty
                =
                new EdmModelValidationRule<ReferentialConstraint>(
                    (context, edmAssociationConstraint) =>
                        {
                            if (edmAssociationConstraint.ToProperties == null
                                || !edmAssociationConstraint.ToProperties.Any())
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

        internal static readonly EdmModelValidationRule<NavigationProperty>
            EdmNavigationProperty_AssocationMustNotBeNull =
                new EdmModelValidationRule<NavigationProperty>(
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

        internal static readonly EdmModelValidationRule<NavigationProperty>
            EdmNavigationProperty_ResultEndMustNotBeNull =
                new EdmModelValidationRule<NavigationProperty>(
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

        internal static readonly EdmModelValidationRule<AssociationEndMember> EdmAssociationEnd_EntityTypeMustNotBeNull =
            new EdmModelValidationRule<AssociationEndMember>(
                (context, edmAssociationEnd) =>
                    {
                        if (edmAssociationEnd.GetEntityType() == null)
                        {
                            context.AddError(
                                edmAssociationEnd,
                                CsdlConstants.Attribute_Type,
                                Strings.EdmModel_Validator_Syntactic_EdmAssociationEnd_EntityTypeMustNotBeNull,
                                XmlErrorCode.EdmAssociationEnd_EntityTypeMustNotBeNull);
                        }
                    }
                );

        internal static readonly EdmModelValidationRule<EntitySet> EdmEntitySet_ElementTypeMustNotBeNull =
            new EdmModelValidationRule<EntitySet>(
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

        internal static readonly EdmModelValidationRule<AssociationSet> EdmAssociationSet_ElementTypeMustNotBeNull =
            new EdmModelValidationRule<AssociationSet>(
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

        internal static readonly EdmModelValidationRule<AssociationSet> EdmAssociationSet_SourceSetMustNotBeNull =
            new EdmModelValidationRule<AssociationSet>(
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

        internal static readonly EdmModelValidationRule<AssociationSet> EdmAssociationSet_TargetSetMustNotBeNull =
            new EdmModelValidationRule<AssociationSet>(
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

        internal static readonly EdmModelValidationRule<TypeUsage> EdmTypeReference_TypeNotValid =
            new EdmModelValidationRule<TypeUsage>(
                (context, edmTypeReference) =>
                    {
                        if (!DataModelValidationHelper.IsEdmTypeUsageValid(edmTypeReference))
                        {
                            context.AddError(
                                edmTypeReference,
                                null,
                                Strings.EdmModel_Validator_Syntactic_EdmTypeReferenceNotValid,
                                XmlErrorCode.EdmTypeReferenceNotValid);
                        }
                    }
                );
    }
}
