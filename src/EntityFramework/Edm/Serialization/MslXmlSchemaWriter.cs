// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Serialization
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Xml;

    internal class MslXmlSchemaWriter : XmlSchemaWriter
    {
        private string _entityTypeNamespace;
        private string _dbSchemaName;

        internal MslXmlSchemaWriter(XmlWriter xmlWriter, double version)
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
            var xmlNamespace = StorageMslConstructs.GetMslNamespace(_version);
            _xmlWriter.WriteStartElement(StorageMslConstructs.MappingElement, xmlNamespace);
            _xmlWriter.WriteAttributeString(StorageMslConstructs.MappingSpaceAttribute, "C-S");
        }

        private void WriteDbModelElement(DbDatabaseMapping databaseMapping)
        {
            _entityTypeNamespace = databaseMapping.Model.NamespaceNames.SingleOrDefault();
            _dbSchemaName = databaseMapping.Database.Containers.Single().Name;
            
            WriteEntityContainerMappingElement(databaseMapping.EntityContainerMappings.FirstOrDefault());
        }

        private void WriteEntityContainerMappingElement(StorageEntityContainerMapping containerMapping)
        {
            DebugCheck.NotNull(containerMapping);

            _xmlWriter.WriteStartElement(StorageMslConstructs.EntityContainerMappingElement);
            _xmlWriter.WriteAttributeString(StorageMslConstructs.StorageEntityContainerAttribute, _dbSchemaName);
            _xmlWriter.WriteAttributeString(
                StorageMslConstructs.CdmEntityContainerAttribute, containerMapping.EdmEntityContainer.Name);

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
            _xmlWriter.WriteStartElement(StorageMslConstructs.EntitySetMappingElement);
            _xmlWriter.WriteAttributeString(StorageMslConstructs.EntitySetMappingNameAttribute, set.EntitySet.Name);

            foreach (var entityTypeMapping in set.EntityTypeMappings)
            {
                WriteEntityTypeMappingElement(entityTypeMapping);
            }
            _xmlWriter.WriteEndElement();
        }

        private void WriteAssociationSetMappingElement(StorageAssociationSetMapping set)
        {
            _xmlWriter.WriteStartElement(StorageMslConstructs.AssociationSetMappingElement);
            _xmlWriter.WriteAttributeString(StorageMslConstructs.AssociationSetMappingNameAttribute, set.AssociationSet.Name);
            _xmlWriter.WriteAttributeString(
                StorageMslConstructs.AssociationSetMappingTypeNameAttribute,
                _entityTypeNamespace + "." + set.AssociationSet.ElementType.Name);
            _xmlWriter.WriteAttributeString(StorageMslConstructs.AssociationSetMappingStoreEntitySetAttribute, set.Table.Name);
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
            _xmlWriter.WriteStartElement(StorageMslConstructs.EndPropertyMappingElement);
            _xmlWriter.WriteAttributeString(StorageMslConstructs.EndPropertyMappingNameAttribute, endMapping.EndMember.Name);

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
            _xmlWriter.WriteStartElement(StorageMslConstructs.EntityTypeMappingElement);
            _xmlWriter.WriteAttributeString(
                StorageMslConstructs.EntityTypeMappingTypeNameAttribute,
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
            _xmlWriter.WriteStartElement(StorageMslConstructs.MappingFragmentElement);
            _xmlWriter.WriteAttributeString(StorageMslConstructs.MappingFragmentStoreEntitySetAttribute, mappingFragment.Table.Name);

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
            _xmlWriter.WriteStartElement(StorageMslConstructs.ComplexPropertyElement);
            _xmlWriter.WriteAttributeString(StorageMslConstructs.ComplexPropertyNameAttribute, complexPropertyMapping.EdmProperty.Name);
            _xmlWriter.WriteAttributeString(
                StorageMslConstructs.ComplexPropertyTypeNameAttribute,
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
                return StorageMslConstructs.IsTypeOf + fullyQualifiedEntityTypeName + StorageMslConstructs.IsTypeOfTerminal;
            }
            return fullyQualifiedEntityTypeName;
        }

        private void WriteConditionElement(StorageConditionPropertyMapping condition)
        {
            _xmlWriter.WriteStartElement(StorageMslConstructs.ConditionElement);
            if (condition.IsNull.HasValue)
            {
                _xmlWriter.WriteAttributeString(
                    StorageMslConstructs.ConditionIsNullAttribute, GetLowerCaseStringFromBoolValue(condition.IsNull.Value));
            }
            else
            {
                if (condition.Value is bool)
                {
                    _xmlWriter.WriteAttributeString(StorageMslConstructs.ConditionValueAttribute, (bool)condition.Value ? "1" : "0");
                }
                else
                {
                    _xmlWriter.WriteAttributeString(StorageMslConstructs.ConditionValueAttribute, condition.Value.ToString());
                }
            }
            _xmlWriter.WriteAttributeString(StorageMslConstructs.ConditionColumnNameAttribute, condition.ColumnProperty.Name);
            _xmlWriter.WriteEndElement();
        }

        private void WriteScalarPropertyElement(EdmProperty property, EdmProperty column)
        {
            _xmlWriter.WriteStartElement(StorageMslConstructs.ScalarPropertyElement);
            _xmlWriter.WriteAttributeString(StorageMslConstructs.ScalarPropertyNameAttribute, property.Name);
            _xmlWriter.WriteAttributeString(StorageMslConstructs.ScalarPropertyColumnNameAttribute, column.Name);
            _xmlWriter.WriteEndElement();
        }
    }
}
