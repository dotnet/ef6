// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Serialization.Xml.Internal.Ssdl
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Csdl;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Ssdl;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using System.Xml;

    internal class DbModelSsdlSchemaWriter : XmlSchemaWriter
    {
        internal DbModelSsdlSchemaWriter(XmlWriter xmlWriter, double dbVersion)
        {
            _xmlWriter = xmlWriter;
            _version = dbVersion;
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
            else
            {
                Contract.Assert(dbVersion == DataModelVersions.Version3, "Added a new version?");
                return SsdlConstants.Version3Namespace;
            }
        }

        internal void WriteEntityTypeElementHeader(DbTableMetadata entityType)
        {
            _xmlWriter.WriteStartElement(SsdlConstants.Element_EntityType);
            _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_Name, entityType.Name);
        }

        internal void WriteForeignKeyConstraintElement(
            DbTableMetadata dbTableMetadata, DbForeignKeyConstraintMetadata tableFKConstraint)
        {
            _xmlWriter.WriteStartElement(SsdlConstants.Element_Association);
            _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_Name, tableFKConstraint.Name);

            var multiplicity = DetermineMultiplicity(dbTableMetadata, tableFKConstraint);

            // If the FK is a Self Ref, then we need to append a suffix to the second role name
            var roleNames = DbModelSsdlHelper.GetRoleNamePair(tableFKConstraint.PrincipalTable, dbTableMetadata);

            // End
            WriteAssociationEndElementHeader(roleNames[0], tableFKConstraint.PrincipalTable, multiplicity.Key);

            if (tableFKConstraint.DeleteAction
                != OperationAction.None)
            {
                WriteOperationActionElement(SsdlConstants.Element_OnDelete, tableFKConstraint.DeleteAction);
            }

            WriteEndElement();
            WriteAssociationEndElementHeader(roleNames[1], dbTableMetadata, multiplicity.Value);
            WriteEndElement();

            // ReferentialConstraint
            WriteReferentialConstraintElementHeader();
            WriteReferentialConstraintRoleElement(
                SsdlConstants.Element_PrincipalRole, roleNames[0], tableFKConstraint.PrincipalTable.KeyColumns);
            WriteReferentialConstraintRoleElement(
                SsdlConstants.Element_DependentRole, roleNames[1], tableFKConstraint.DependentColumns);
            WriteEndElement();

            WriteEndElement();
        }

        internal void WriteOperationActionElement(string elementName, OperationAction operationAction)
        {
            _xmlWriter.WriteStartElement(elementName);
            _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_Action, operationAction.ToString());
            _xmlWriter.WriteEndElement();
        }

        internal void WriteAssociationEndElementHeader(
            string roleName, DbTableMetadata associationEnd, string multiplicity)
        {
            _xmlWriter.WriteStartElement(SsdlConstants.Element_End);
            _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_Role, roleName);

            var typeName = associationEnd.Name;
            _xmlWriter.WriteAttributeString(
                SsdlConstants.Attribute_Type, GetQualifiedTypeName(SsdlConstants.Value_Self, typeName));
            _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_Multiplicity, multiplicity);
        }

        private static KeyValuePair<string, string> DetermineMultiplicity(
            DbTableMetadata dependentTable, DbForeignKeyConstraintMetadata constraint)
        {
            var principalMultiplicity = CsdlConstants.Value_EndOptional;
            var dependentMultiplicity = CsdlConstants.Value_EndMany;
            var isDependentPropertiesFullyCoverKey = false;
            var isDependentPropertiesHasNullableProperty = false;

            IEnumerable<DbTableColumnMetadata> dependentProperties = constraint.DependentColumns;

            if (dependentTable.KeyColumns.Count() == dependentProperties.Count()
                && dependentTable.KeyColumns.All(dependentProperties.Contains))
            {
                isDependentPropertiesFullyCoverKey = true;
            }

            if (dependentProperties.Any(p => p.IsNullable))
            {
                isDependentPropertiesHasNullableProperty = true;
            }

            if (!isDependentPropertiesHasNullableProperty)
            {
                principalMultiplicity = CsdlConstants.Value_EndRequired;
            }

            if (isDependentPropertiesFullyCoverKey)
            {
                principalMultiplicity = CsdlConstants.Value_EndRequired;
                dependentMultiplicity = CsdlConstants.Value_EndOptional;
            }

            return new KeyValuePair<string, string>(principalMultiplicity, dependentMultiplicity);
        }

        internal void WriteReferentialConstraintElementHeader()
        {
            _xmlWriter.WriteStartElement(SsdlConstants.Element_ReferentialConstraint);
        }

        internal void WriteDelaredKeyPropertiesElementHeader()
        {
            _xmlWriter.WriteStartElement(SsdlConstants.Element_Key);
        }

        internal void WriteDelaredKeyPropertyRefElement(DbTableColumnMetadata property)
        {
            _xmlWriter.WriteStartElement(SsdlConstants.Element_PropertyRef);
            _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_Name, property.Name);
            _xmlWriter.WriteEndElement();
        }

        internal void WritePropertyElementHeader(DbTableColumnMetadata property)
        {
            _xmlWriter.WriteStartElement(SsdlConstants.Element_Property);
            _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_Name, property.Name);
            _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_Type, property.TypeName);

            WritePropertyTypeFacets(property);
        }

        private IEnumerable<KeyValuePair<string, string>> GetEnumerableFacetValueFromPrimitiveTypeFacets(
            DbPrimitiveTypeFacets facets)
        {
            if (facets != null)
            {
                if (facets.IsFixedLength.HasValue)
                {
                    yield return
                        new KeyValuePair<string, string>(
                            SsdlConstants.Attribute_FixedLength,
                            GetLowerCaseStringFromBoolValue(facets.IsFixedLength.Value));
                }
                if (facets.IsUnicode.HasValue)
                {
                    yield return
                        new KeyValuePair<string, string>(
                            SsdlConstants.Attribute_Unicode, GetLowerCaseStringFromBoolValue(facets.IsUnicode.Value));
                }
                if (facets.MaxLength.HasValue)
                {
                    yield return
                        new KeyValuePair<string, string>(
                            SsdlConstants.Attribute_MaxLength,
                            facets.MaxLength.Value.ToString(CultureInfo.InvariantCulture));
                }
                else if (facets.IsMaxLength.HasValue)
                {
                    yield return
                        new KeyValuePair<string, string>(SsdlConstants.Attribute_MaxLength, SsdlConstants.Value_Max);
                }
                if (facets.Precision.HasValue)
                {
                    yield return
                        new KeyValuePair<string, string>(
                            SsdlConstants.Attribute_Precision,
                            facets.Precision.Value.ToString(CultureInfo.InvariantCulture));
                }
                if (facets.Scale.HasValue)
                {
                    yield return
                        new KeyValuePair<string, string>(
                            SsdlConstants.Attribute_Scale, facets.Scale.Value.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        private void WritePropertyTypeFacets(DbTableColumnMetadata property)
        {
            if (property.Facets != null)
            {
                foreach (var facet in GetEnumerableFacetValueFromPrimitiveTypeFacets(property.Facets))
                {
                    _xmlWriter.WriteAttributeString(facet.Key, facet.Value);
                }
            }
            if (property.StoreGeneratedPattern
                != StoreGeneratedPattern.None)
            {
                _xmlWriter.WriteAttributeString(
                    SsdlConstants.Attribute_StoreGeneratedPattern,
                    property.StoreGeneratedPattern == StoreGeneratedPattern.Computed
                        ? CsdlConstants.Value_Computed
                        : CsdlConstants.Value_Identity);
            }
            _xmlWriter.WriteAttributeString(
                SsdlConstants.Attribute_Nullable, GetLowerCaseStringFromBoolValue(property.IsNullable));
        }

        internal void WriteReferentialConstraintRoleElement(
            string roleElementName, string roleName, IEnumerable<DbColumnMetadata> properties)
        {
            _xmlWriter.WriteStartElement(roleElementName);
            _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_Role, roleName);
            foreach (var property in properties)
            {
                _xmlWriter.WriteStartElement(SsdlConstants.Element_PropertyRef);
                _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_Name, property.Name);
                _xmlWriter.WriteEndElement();
            }
            _xmlWriter.WriteEndElement();
        }
    }
}
