// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Serialization.Xml.Internal.Csdl
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.Edm.Internal;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Csdl;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Ssdl;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using System.Xml;

    internal class EdmXmlSchemaWriter : XmlSchemaWriter
    {
        private readonly bool _serializeDefaultNullability;
        private const string DataServicesPrefix = "m";
        private const string DataServicesNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
        private const string DataServicesMimeTypeAttribute = "System.Data.Services.MimeTypeAttribute";
        private const string DataServicesHasStreamAttribute = "System.Data.Services.Common.HasStreamAttribute";

        private const string DataServicesEntityPropertyMappingAttribute =
            "System.Data.Services.Common.EntityPropertyMappingAttribute";

        internal static class XmlConstants
        {
            /// <summary>
            ///     author/email
            /// </summary>
            internal const string SyndAuthorEmail = "SyndicationAuthorEmail";

            /// <summary>
            ///     author/name
            /// </summary>
            internal const string SyndAuthorName = "SyndicationAuthorName";

            /// <summary>
            ///     author/uri
            /// </summary>
            internal const string SyndAuthorUri = "SyndicationAuthorUri";

            /// <summary>
            ///     published
            /// </summary>
            internal const string SyndPublished = "SyndicationPublished";

            /// <summary>
            ///     rights
            /// </summary>
            internal const string SyndRights = "SyndicationRights";

            /// <summary>
            ///     summary
            /// </summary>
            internal const string SyndSummary = "SyndicationSummary";

            /// <summary>
            ///     title
            /// </summary>
            internal const string SyndTitle = "SyndicationTitle";

            /// <summary>
            ///     contributor/email
            /// </summary>
            internal const string SyndContributorEmail = "SyndicationContributorEmail";

            /// <summary>
            ///     contributor/name
            /// </summary>
            internal const string SyndContributorName = "SyndicationContributorName";

            /// <summary>
            ///     contributor/uri
            /// </summary>
            internal const string SyndContributorUri = "SyndicationContributorUri";

            /// <summary>
            ///     category/@label
            /// </summary>
            internal const string SyndCategoryLabel = "SyndicationCategoryLabel";

            /// <summary>
            ///     Plaintext
            /// </summary>
            internal const string SyndContentKindPlaintext = "text";

            /// <summary>
            ///     HTML
            /// </summary>
            internal const string SyndContentKindHtml = "html";

            /// <summary>
            ///     XHTML
            /// </summary>
            internal const string SyndContentKindXHtml = "xhtml";

            /// <summary>
            ///     updated
            /// </summary>
            internal const string SyndUpdated = "SyndicationUpdated";

            /// <summary>
            ///     link/@href
            /// </summary>
            internal const string SyndLinkHref = "SyndicationLinkHref";

            /// <summary>
            ///     link/@rel
            /// </summary>
            internal const string SyndLinkRel = "SyndicationLinkRel";

            /// <summary>
            ///     link/@type
            /// </summary>
            internal const string SyndLinkType = "SyndicationLinkType";

            /// <summary>
            ///     link/@hreflang
            /// </summary>
            internal const string SyndLinkHrefLang = "SyndicationLinkHrefLang";

            /// <summary>
            ///     link/@title
            /// </summary>
            internal const string SyndLinkTitle = "SyndicationLinkTitle";

            /// <summary>
            ///     link/@length
            /// </summary>
            internal const string SyndLinkLength = "SyndicationLinkLength";

            /// <summary>
            ///     category/@term
            /// </summary>
            internal const string SyndCategoryTerm = "SyndicationCategoryTerm";

            /// <summary>
            ///     category/@scheme
            /// </summary>
            internal const string SyndCategoryScheme = "SyndicationCategoryScheme";
        }

        private static string SyndicationItemPropertyToString(object value)
        {
            return _syndicationItemToTargetPath[(int)value];
        }

        private static readonly string[] _syndicationItemToTargetPath 
            = new[]
                                                                            {
                                                                                String.Empty,
                                                                                // SyndicationItemProperty.Custom
                                                                                XmlConstants.SyndAuthorEmail,
                                                                                XmlConstants.SyndAuthorName,
                                                                                XmlConstants.SyndAuthorUri,
                                                                                XmlConstants.SyndContributorEmail,
                                                                                XmlConstants.SyndContributorName,
                                                                                XmlConstants.SyndContributorUri,
                                                                                XmlConstants.SyndUpdated,
                                                                                XmlConstants.SyndPublished,
                                                                                XmlConstants.SyndRights,
                                                                                XmlConstants.SyndSummary,
                                                                                XmlConstants.SyndTitle,
                                                                                XmlConstants.SyndCategoryLabel,
                                                                                XmlConstants.SyndCategoryScheme,
                                                                                XmlConstants.SyndCategoryTerm,
                                                                                XmlConstants.SyndLinkHref,
                                                                                XmlConstants.SyndLinkHrefLang,
                                                                                XmlConstants.SyndLinkLength,
                                                                                XmlConstants.SyndLinkRel,
                                                                                XmlConstants.SyndLinkTitle,
                                                                                XmlConstants.SyndLinkType
                                                                            };

        private static string SyndicationTextContentKindToString(object value)
        {
            return _syndicationTextContentKindToString[(int)value];
        }

        private static readonly string[] _syndicationTextContentKindToString = new[]
                                                                                   {
                                                                                       XmlConstants.
                                                                                           SyndContentKindPlaintext,
                                                                                       XmlConstants.SyndContentKindHtml,
                                                                                       XmlConstants.SyndContentKindXHtml
                                                                                   };

        internal EdmXmlSchemaWriter(XmlWriter xmlWriter, double edmVersion, bool serializeDefaultNullability)
        {
            _serializeDefaultNullability = serializeDefaultNullability;
            _xmlWriter = xmlWriter;
            _version = edmVersion;
        }

        internal void WriteSchemaElementHeader(string schemaNamespace)
        {
            var xmlNamespace = DataModelVersions.GetCsdlNamespace(_version);
            _xmlWriter.WriteStartElement(CsdlConstants.Element_Schema, xmlNamespace);
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_Namespace, schemaNamespace);
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_Alias, CsdlConstants.Value_Self);
            if (_version == DataModelVersions.Version3)
            {
                _xmlWriter.WriteAttributeString(
                    CsdlConstants.Attribute_UseStrongSpatialTypes, CsdlConstants.AnnotationNamespace,
                    CsdlConstants.Value_False);
            }
        }

        internal void WriteSchemaElementHeader(string schemaNamespace, string provider, string providerManifestToken)
        {
            var xmlNamespace = GetSsdlNamespace(_version);
            _xmlWriter.WriteStartElement(SsdlConstants.Element_Schema, xmlNamespace);
            _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_Namespace, schemaNamespace + "Schema");
            _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_Provider, provider);
            _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_ProviderManifestToken, providerManifestToken);
            _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_Alias, SsdlConstants.Value_Self);
        }

        private static string GetSsdlNamespace(double dbVersion)
        {
            if (dbVersion == DataModelVersions.Version1)
            {
                return SsdlConstants.Version1Namespace;
            }

            if (dbVersion == DataModelVersions.Version2)
            {
                return SsdlConstants.Version2Namespace;
            }

            Contract.Assert(dbVersion == DataModelVersions.Version3, "Added a new version?");

            return SsdlConstants.Version3Namespace;
        }

        private void WritePolymorphicTypeAttributes(EdmType edmType)
        {
            if (edmType.BaseType != null)
            {
                _xmlWriter.WriteAttributeString(
                    CsdlConstants.Attribute_BaseType,
                    GetQualifiedTypeName(CsdlConstants.Value_Self, edmType.BaseType.Name));
            }

            if (edmType.Abstract)
            {
                _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_Abstract, CsdlConstants.Value_True);
            }
        }

        internal void WriteEntityTypeElementHeader(EntityType entityType)
        {
            _xmlWriter.WriteStartElement(CsdlConstants.Element_EntityType);
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_Name, entityType.Name);

            if (entityType.Annotations.GetClrAttributes() != null)
            {
                foreach (var a in entityType.Annotations.GetClrAttributes())
                {
                    if (a.GetType().FullName.Equals(DataServicesHasStreamAttribute, StringComparison.Ordinal))
                    {
                        _xmlWriter.WriteAttributeString(DataServicesPrefix, "HasStream", DataServicesNamespace, "true");
                    }
                    else if (a.GetType().FullName.Equals(DataServicesMimeTypeAttribute, StringComparison.Ordinal))
                    {
                        // Move down to the appropriate property
                        var propertyName = a.GetType().GetProperty("MemberName").GetValue(a, null) as string;
                        var property =
                            entityType.Properties.SingleOrDefault(
                                p => p.Name.Equals(propertyName, StringComparison.Ordinal));
                        AddAttributeAnnotation(property, a);
                    }
                    else if (a.GetType().FullName.Equals(
                        DataServicesEntityPropertyMappingAttribute, StringComparison.Ordinal))
                    {
                        // Move down to the appropriate property
                        var sourcePath = a.GetType().GetProperty("SourcePath").GetValue(a, null) as string;
                        var slashIndex = sourcePath.IndexOf("/", StringComparison.Ordinal);
                        string propertyName;
                        if (slashIndex == -1)
                        {
                            propertyName = sourcePath;
                        }
                        else
                        {
                            propertyName = sourcePath.Substring(0, slashIndex);
                        }
                        var property =
                            entityType.Properties.SingleOrDefault(
                                p => p.Name.Equals(propertyName, StringComparison.Ordinal));
                        AddAttributeAnnotation(property, a);
                    }
                }
            }

            WritePolymorphicTypeAttributes(entityType);
        }

        internal void WriteEnumTypeElementHeader(EnumType enumType)
        {
            _xmlWriter.WriteStartElement(CsdlConstants.Element_EnumType);
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_Name, enumType.Name);
            _xmlWriter.WriteAttributeString(
                CsdlConstants.Attribute_IsFlags, GetLowerCaseStringFromBoolValue(enumType.IsFlags));

            if (enumType.UnderlyingType != null)
            {
                _xmlWriter.WriteAttributeString(
                    CsdlConstants.Attribute_UnderlyingType,
                    enumType.UnderlyingType.PrimitiveTypeKind.ToString());
            }
        }

        internal void WriteEnumTypeMemberElementHeader(EnumMember enumTypeMember)
        {
            _xmlWriter.WriteStartElement(CsdlConstants.Element_EnumTypeMember);
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_Name, enumTypeMember.Name);
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_Value, enumTypeMember.Value.ToString());
        }

        private static void AddAttributeAnnotation(EdmProperty property, Attribute a)
        {
            if (property != null)
            {
                var clrAttributes = property.Annotations.GetClrAttributes();
                if (clrAttributes != null)
                {
                    if (!clrAttributes.Contains(a))
                    {
                        clrAttributes.Add(a);
                    }
                }
                else
                {
                    property.Annotations.SetClrAttributes(
                        new List<Attribute>
                            {
                                a
                            });
                }
            }
        }

        internal void WriteComplexTypeElementHeader(ComplexType complexType)
        {
            _xmlWriter.WriteStartElement(CsdlConstants.Element_ComplexType);
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_Name, complexType.Name);
            WritePolymorphicTypeAttributes(complexType);
        }

        internal void WriteAssociationTypeElementHeader(AssociationType associationType)
        {
            _xmlWriter.WriteStartElement(CsdlConstants.Element_Association);
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_Name, associationType.Name);
        }

        internal void WriteAssociationEndElementHeader(AssociationEndMember associationEnd)
        {
            _xmlWriter.WriteStartElement(CsdlConstants.Element_End);
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_Role, associationEnd.Name);

            var typeName = associationEnd.GetEntityType().Name;
            _xmlWriter.WriteAttributeString(
                CsdlConstants.Attribute_Type, GetQualifiedTypeName(CsdlConstants.Value_Self, typeName));
            _xmlWriter.WriteAttributeString(
                CsdlConstants.Attribute_Multiplicity, GetXmlMultiplicity(associationEnd.RelationshipMultiplicity));
        }

        internal void WriteOperationActionElement(string elementName, OperationAction operationAction)
        {
            _xmlWriter.WriteStartElement(elementName);
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_Action, operationAction.ToString());
            _xmlWriter.WriteEndElement();
        }

        internal void WriteReferentialConstraintElementHeader()
        {
            _xmlWriter.WriteStartElement(CsdlConstants.Element_ReferentialConstraint);
        }

        internal void WriteDelaredKeyPropertiesElementHeader()
        {
            _xmlWriter.WriteStartElement(CsdlConstants.Element_Key);
        }

        internal void WriteDelaredKeyPropertyRefElement(EdmProperty property)
        {
            _xmlWriter.WriteStartElement(CsdlConstants.Element_PropertyRef);
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_Name, property.Name);
            _xmlWriter.WriteEndElement();
        }

        internal void WritePropertyElementHeader(EdmProperty property)
        {
            _xmlWriter.WriteStartElement(CsdlConstants.Element_Property);
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_Name, property.Name);
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_Type, GetTypeReferenceName(property));

            if (property.CollectionKind
                != CollectionKind.None)
            {
                _xmlWriter.WriteAttributeString(
                    CsdlConstants.Attribute_CollectionKind, property.CollectionKind.ToString());
            }

            if (property.ConcurrencyMode
                == ConcurrencyMode.Fixed)
            {
                _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_ConcurrencyMode, CsdlConstants.Value_Fixed);
            }

            if (property.Annotations.GetClrAttributes() != null)
            {
                var epmCount = 0;
                foreach (var a in property.Annotations.GetClrAttributes())
                {
                    if (a.GetType().FullName.Equals(DataServicesMimeTypeAttribute, StringComparison.Ordinal))
                    {
                        var mimeType = a.GetType().GetProperty("MimeType").GetValue(a, null) as string;
                        _xmlWriter.WriteAttributeString(DataServicesPrefix, "MimeType", DataServicesNamespace, mimeType);
                    }
                    else if (a.GetType().FullName.Equals(
                        DataServicesEntityPropertyMappingAttribute, StringComparison.Ordinal))
                    {
                        var suffix = epmCount == 0
                                         ? String.Empty
                                         : string.Format(CultureInfo.InvariantCulture, "_{0}", epmCount);

                        var sourcePath = a.GetType().GetProperty("SourcePath").GetValue(a, null) as string;
                        var slashIndex = sourcePath.IndexOf("/", StringComparison.Ordinal);
                        if (slashIndex != -1
                            && slashIndex + 1 < sourcePath.Length)
                        {
                            _xmlWriter.WriteAttributeString(
                                DataServicesPrefix, "FC_SourcePath" + suffix, DataServicesNamespace,
                                sourcePath.Substring(slashIndex + 1));
                        }

                        // There are three ways to write out this attribute
                        var syndicationItem = a.GetType().GetProperty("TargetSyndicationItem").GetValue(a, null);
                        var keepInContext = a.GetType().GetProperty("KeepInContent").GetValue(a, null).ToString();
                        var criteriaValueProperty = a.GetType().GetProperty("CriteriaValue");
                        string criteriaValue = null;
                        if (criteriaValueProperty != null)
                        {
                            criteriaValue = criteriaValueProperty.GetValue(a, null) as string;
                        }

                        if (criteriaValue != null)
                        {
                            _xmlWriter.WriteAttributeString(
                                DataServicesPrefix,
                                "FC_TargetPath" + suffix,
                                DataServicesNamespace,
                                SyndicationItemPropertyToString(syndicationItem));
                            _xmlWriter.WriteAttributeString(
                                DataServicesPrefix, "FC_KeepInContent" + suffix, DataServicesNamespace,
                                keepInContext);
                            _xmlWriter.WriteAttributeString(
                                DataServicesPrefix, "FC_CriteriaValue" + suffix, DataServicesNamespace,
                                criteriaValue);
                        }
                        else if (string.Equals(
                            syndicationItem.ToString(), "CustomProperty", StringComparison.Ordinal))
                        {
                            var targetPath = a.GetType().GetProperty("TargetPath").GetValue(a, null).ToString();
                            var targetNamespacePrefix =
                                a.GetType().GetProperty("TargetNamespacePrefix").GetValue(a, null).ToString();
                            var targetNamespaceUri =
                                a.GetType().GetProperty("TargetNamespaceUri").GetValue(a, null).ToString();

                            _xmlWriter.WriteAttributeString(
                                DataServicesPrefix, "FC_TargetPath" + suffix, DataServicesNamespace, targetPath);
                            _xmlWriter.WriteAttributeString(
                                DataServicesPrefix, "FC_NsUri" + suffix, DataServicesNamespace,
                                targetNamespaceUri);
                            _xmlWriter.WriteAttributeString(
                                DataServicesPrefix, "FC_NsPrefix" + suffix, DataServicesNamespace,
                                targetNamespacePrefix);
                            _xmlWriter.WriteAttributeString(
                                DataServicesPrefix, "FC_KeepInContent" + suffix, DataServicesNamespace,
                                keepInContext);
                        }
                        else
                        {
                            var contextKind = a.GetType().GetProperty("TargetTextContentKind").GetValue(a, null);

                            _xmlWriter.WriteAttributeString(
                                DataServicesPrefix,
                                "FC_TargetPath" + suffix,
                                DataServicesNamespace,
                                SyndicationItemPropertyToString(syndicationItem));
                            _xmlWriter.WriteAttributeString(
                                DataServicesPrefix,
                                "FC_ContentKind" + suffix,
                                DataServicesNamespace,
                                SyndicationTextContentKindToString(contextKind));
                            _xmlWriter.WriteAttributeString(
                                DataServicesPrefix, "FC_KeepInContent" + suffix, DataServicesNamespace,
                                keepInContext);
                        }

                        epmCount++;
                    }
                }
            }

            if (property.IsMaxLength)
            {
                _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_MaxLength, CsdlConstants.Value_Max);
            }
            else if (property.MaxLength.HasValue)
            {
                _xmlWriter.WriteAttributeString(
                    CsdlConstants.Attribute_MaxLength,
                    property.MaxLength.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (property.IsFixedLength.HasValue)
            {
                _xmlWriter.WriteAttributeString(
                    CsdlConstants.Attribute_FixedLength,
                    GetLowerCaseStringFromBoolValue(property.IsFixedLength.Value));
            }

            if (property.IsUnicode.HasValue)
            {
                _xmlWriter.WriteAttributeString(
                    CsdlConstants.Attribute_Unicode, GetLowerCaseStringFromBoolValue(property.IsUnicode.Value));
            }

            if (property.Precision.HasValue)
            {
                _xmlWriter.WriteAttributeString(
                    CsdlConstants.Attribute_Precision,
                    property.Precision.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (property.Scale.HasValue)
            {
                _xmlWriter.WriteAttributeString(
                    CsdlConstants.Attribute_Scale, property.Scale.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (property.StoreGeneratedPattern != StoreGeneratedPattern.None)
            {
                _xmlWriter.WriteAttributeString(
                    SsdlConstants.Attribute_StoreGeneratedPattern,
                    property.StoreGeneratedPattern == StoreGeneratedPattern.Computed
                        ? CsdlConstants.Value_Computed
                        : CsdlConstants.Value_Identity);
            }
            
            if (_serializeDefaultNullability || !property.Nullable)
            {
                _xmlWriter.WriteAttributeString(
                    CsdlConstants.Attribute_Nullable, GetLowerCaseStringFromBoolValue(property.Nullable));
            }

            DataModelAnnotation annotation;

            if (property.Annotations.TryGetByName(SsdlConstants.Attribute_StoreGeneratedPattern, out annotation))
            {
                _xmlWriter.WriteAttributeString(
                    SsdlConstants.Attribute_StoreGeneratedPattern, CsdlConstants.AnnotationNamespace,
                    annotation.Value.ToString());
            }
        }

        private static string GetTypeReferenceName(EdmProperty property)
        {
            if (property.IsPrimitiveType)
            {
                return property.TypeName;
            }

            if (property.IsComplexType)
            {
                return GetQualifiedTypeName(CsdlConstants.Value_Self, property.ComplexType.Name);
            }

            Contract.Assert(property.IsEnumType);

            return GetQualifiedTypeName(CsdlConstants.Value_Self, property.EnumType.Name);
        }

        internal void WriteNavigationPropertyElementHeader(NavigationProperty member)
        {
            _xmlWriter.WriteStartElement(CsdlConstants.Element_NavigationProperty);
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_Name, member.Name);
            _xmlWriter.WriteAttributeString(
                CsdlConstants.Attribute_Relationship,
                GetQualifiedTypeName(CsdlConstants.Value_Self, member.Association.Name));
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_FromRole, member.GetFromEnd().Name);
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_ToRole, member.ResultEnd.Name);
        }

        private static string GetXmlMultiplicity(RelationshipMultiplicity endKind)
        {
            switch (endKind)
            {
                case RelationshipMultiplicity.Many:
                    return CsdlConstants.Value_EndMany;
                case RelationshipMultiplicity.One:
                    return CsdlConstants.Value_EndRequired;
                case RelationshipMultiplicity.ZeroOrOne:
                    return CsdlConstants.Value_EndOptional;
                default:
                    Debug.Fail("Did you add a new EdmAssociationEndKind?");
                    return string.Empty;
            }
        }

        internal void WriteReferentialConstraintRoleElement(
            string roleName, AssociationEndMember edmAssociationEnd, IEnumerable<EdmProperty> properties)
        {
            _xmlWriter.WriteStartElement(roleName);
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_Role, edmAssociationEnd.Name);
            foreach (var property in properties)
            {
                _xmlWriter.WriteStartElement(CsdlConstants.Element_PropertyRef);
                _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_Name, property.Name);
                _xmlWriter.WriteEndElement();
            }
            _xmlWriter.WriteEndElement();
        }

        internal void WriteEntityContainerElementHeader(EntityContainer container)
        {
            _xmlWriter.WriteStartElement(CsdlConstants.Element_EntityContainer);
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_Name, container.Name);
        }

        internal void WriteAssociationSetElementHeader(AssociationSet associationSet)
        {
            _xmlWriter.WriteStartElement(CsdlConstants.Element_AssociationSet);
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_Name, associationSet.Name);
            _xmlWriter.WriteAttributeString(
                CsdlConstants.Attribute_Association,
                GetQualifiedTypeName(CsdlConstants.Value_Self, associationSet.ElementType.Name));
        }

        internal void WriteAssociationSetEndElement(EntitySet end, string roleName)
        {
            _xmlWriter.WriteStartElement(CsdlConstants.Element_End);
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_Role, roleName);
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_EntitySet, end.Name);
            _xmlWriter.WriteEndElement();
        }

        internal void WriteEntitySetElementHeader(EntitySet entitySet)
        {
            _xmlWriter.WriteStartElement(CsdlConstants.Element_EntitySet);
            _xmlWriter.WriteAttributeString(CsdlConstants.Attribute_Name, entitySet.Name);
            _xmlWriter.WriteAttributeString(
                CsdlConstants.Attribute_EntityType,
                GetQualifiedTypeName(CsdlConstants.Value_Self, entitySet.ElementType.Name));

            if (!string.IsNullOrWhiteSpace(entitySet.Schema))
            {
                _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_Schema, entitySet.Schema);
            }

            if (!string.IsNullOrWhiteSpace(entitySet.Table))
            {
                _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_Table, entitySet.Table);
            }
        }
    }
}
