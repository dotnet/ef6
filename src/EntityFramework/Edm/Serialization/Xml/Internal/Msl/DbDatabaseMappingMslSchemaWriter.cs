// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Serialization.Xml.Internal.Msl
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Msl;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Xml;

    internal class DbModelMslSchemaWriter : XmlSchemaWriter
    {
        private string _entityTypeNamespace;
        private string _dbSchemaName;

        internal DbModelMslSchemaWriter(XmlWriter xmlWriter, double version)
        {
            _xmlWriter = xmlWriter;
            _version = version;
        }

        internal void WriteSchema(DbDatabaseMapping databaseMapping)
        {
            WriteSchemaElementHeader();
            WriteDbModelElement(databaseMapping);
            WriteEndElement();
        }

        private void WriteSchemaElementHeader()
        {
            var xmlNamespace = GetMslNamespace(_version);
            _xmlWriter.WriteStartElement(MslConstants.Element_Mapping, xmlNamespace);
            _xmlWriter.WriteAttributeString(MslConstants.Attribute_Space, MslConstants.Value_Space);
        }

        private void WriteDbModelElement(DbDatabaseMapping databaseMapping)
        {
            _entityTypeNamespace = databaseMapping.Model.Namespaces.First().Name;
            _dbSchemaName = databaseMapping.Database.Name;
            WriteEntityContainerMappingElement(databaseMapping.EntityContainerMappings.FirstOrDefault());
        }

        private void WriteEntityContainerMappingElement(StorageEntityContainerMapping containerMapping)
        {
            Contract.Assert(containerMapping != null, "containerMapping cannot be null");

            _xmlWriter.WriteStartElement(MslConstants.Element_EntityContainerMapping);
            _xmlWriter.WriteAttributeString(MslConstants.Attribute_StorageEntityContainer, _dbSchemaName);
            _xmlWriter.WriteAttributeString(
                MslConstants.Attribute_CDMEntityContainer, containerMapping.EdmEntityContainer.Name);

            foreach (var set in containerMapping.EntitySetMappings)
            {
                WriteEntitySetMappingElement(set);
            }

            foreach (var set in containerMapping.AssociationSetMappings)
            {
                WriteAssociationSetMappingElement(set);
            }

            _xmlWriter.WriteEndElement();
        }

        private void WriteEntitySetMappingElement(StorageEntitySetMapping set)
        {
            _xmlWriter.WriteStartElement(MslConstants.Element_EntitySetMapping);
            _xmlWriter.WriteAttributeString(MslConstants.Attribute_Name, set.EntitySet.Name);

            foreach (var entityTypeMapping in set.EntityTypeMappings)
            {
                WriteEntityTypeMappingElement(entityTypeMapping);
            }
            _xmlWriter.WriteEndElement();
        }

        private void WriteAssociationSetMappingElement(StorageAssociationSetMapping set)
        {
            _xmlWriter.WriteStartElement(MslConstants.Element_AssociationSetMapping);
            _xmlWriter.WriteAttributeString(MslConstants.Attribute_Name, set.AssociationSet.Name);
            _xmlWriter.WriteAttributeString(
                MslConstants.Attribute_TypeName, _entityTypeNamespace + "." + set.AssociationSet.ElementType.Name);
            _xmlWriter.WriteAttributeString(MslConstants.Attribute_StoreEntitySet, set.Table.Name);
            WriteAssociationEndMappingElement(set.SourceEndMapping);
            WriteAssociationEndMappingElement(set.TargetEndMapping);

            foreach (var conditionColumn in set.ColumnConditions)
            {
                WriteConditionElement(conditionColumn);
            }

            _xmlWriter.WriteEndElement();
        }

        private void WriteAssociationEndMappingElement(StorageEndPropertyMapping endMapping)
        {
            _xmlWriter.WriteStartElement(MslConstants.Element_EndProperty);
            _xmlWriter.WriteAttributeString(MslConstants.Attribute_Name, endMapping.EndMember.Name);

            foreach (var propertyMapping in endMapping.PropertyMappings)
            {
                WriteScalarPropertyElement(
                    propertyMapping.EdmProperty,
                    propertyMapping.ColumnProperty);
            }

            _xmlWriter.WriteEndElement();
        }

        private void WriteEntityTypeMappingElement(StorageEntityTypeMapping entityTypeMapping)
        {
            _xmlWriter.WriteStartElement(MslConstants.Element_EntityTypeMapping);
            _xmlWriter.WriteAttributeString(
                MslConstants.Attribute_TypeName,
                GetEntityTypeName(
                    _entityTypeNamespace + "." + entityTypeMapping.EntityType.Name, entityTypeMapping.IsHierarchyMapping));

            foreach (var mappingFragment in entityTypeMapping.MappingFragments)
            {
                WriteMappingFragmentElement(mappingFragment);
            }

            _xmlWriter.WriteEndElement();
        }

        private void WriteMappingFragmentElement(StorageMappingFragment mappingFragment)
        {
            _xmlWriter.WriteStartElement(MslConstants.Element_MappingFragment);
            _xmlWriter.WriteAttributeString(MslConstants.Attribute_StoreEntitySet, mappingFragment.Table.Name);

            foreach (var propertyMapping in mappingFragment.Properties)
            {
                WritePropertyMapping(propertyMapping);
            }

            foreach (var conditionColumn in mappingFragment.ColumnConditions)
            {
                WriteConditionElement(conditionColumn);
            }

            _xmlWriter.WriteEndElement();
        }

        private void WritePropertyMapping(StoragePropertyMapping propertyMapping)
        {
            var scalarPropertyMapping = propertyMapping as StorageScalarPropertyMapping;

            if (scalarPropertyMapping != null)
            {
                WritePropertyMapping(scalarPropertyMapping);
            }
            else
            {
                var complexPropertyMapping = propertyMapping as StorageComplexPropertyMapping;

                if (complexPropertyMapping != null)
                {
                    WritePropertyMapping(complexPropertyMapping);
                }
            }
        }

        private void WritePropertyMapping(StorageScalarPropertyMapping scalarPropertyMapping)
        {
            WriteScalarPropertyElement(scalarPropertyMapping.EdmProperty, scalarPropertyMapping.ColumnProperty);
        }

        private void WritePropertyMapping(StorageComplexPropertyMapping complexPropertyMapping)
        {
            _xmlWriter.WriteStartElement(MslConstants.Element_ComplexProperty);
            _xmlWriter.WriteAttributeString(MslConstants.Attribute_Name, complexPropertyMapping.EdmProperty.Name);
            _xmlWriter.WriteAttributeString(
                MslConstants.Attribute_TypeName,
                _entityTypeNamespace + "." + complexPropertyMapping.EdmProperty.ComplexType.Name);

            foreach (var propertyMapping in complexPropertyMapping.TypeMappings.Single().Properties)
            {
                WritePropertyMapping(propertyMapping);
            }

            _xmlWriter.WriteEndElement();
        }

        private static string GetEntityTypeName(string fullyQualifiedEntityTypeName, bool isHierarchyMapping)
        {
            if (isHierarchyMapping)
            {
                return MslConstants.Value_IsTypeOf + fullyQualifiedEntityTypeName + MslConstants.Value_IsTypeOfTerminal;
            }
            return fullyQualifiedEntityTypeName;
        }

        private void WriteConditionElement(StorageConditionPropertyMapping condition)
        {
            _xmlWriter.WriteStartElement(MslConstants.Element_Condition);
            if (condition.IsNull.HasValue)
            {
                _xmlWriter.WriteAttributeString(
                    MslConstants.Attribute_IsNull, GetLowerCaseStringFromBoolValue(condition.IsNull.Value));
            }
            else
            {
                if (condition.Value is bool)
                {
                    _xmlWriter.WriteAttributeString(MslConstants.Attribute_Value, (bool)condition.Value ? "1" : "0");
                }
                else
                {
                    _xmlWriter.WriteAttributeString(MslConstants.Attribute_Value, condition.Value.ToString());
                }
            }
            _xmlWriter.WriteAttributeString(MslConstants.Attribute_ColumnName, condition.ColumnProperty.Name);
            _xmlWriter.WriteEndElement();
        }

        private void WriteScalarPropertyElement(EdmProperty property, EdmProperty column)
        {
            _xmlWriter.WriteStartElement(MslConstants.Element_ScalarProperty);
            _xmlWriter.WriteAttributeString(MslConstants.Attribute_Name, property.Name);
            _xmlWriter.WriteAttributeString(MslConstants.Attribute_ColumnName, column.Name);
            _xmlWriter.WriteEndElement();
        }

        private static string GetMslNamespace(double version)
        {
            if (version == DataModelVersions.Version1)
            {
                return MslConstants.Version1Namespace;
            }

            if (version == DataModelVersions.Version2)
            {
                return MslConstants.Version2Namespace;
            }

            Contract.Assert(version == DataModelVersions.Version3, "added new version?");

            return MslConstants.Version3Namespace;
        }
    }
}
