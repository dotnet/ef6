// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml;

    internal class MslXmlSchemaWriter : XmlSchemaWriter
    {
        private string _entityTypeNamespace;
        private string _dbSchemaName;

        internal MslXmlSchemaWriter(XmlWriter xmlWriter, double version)
        {
            DebugCheck.NotNull(xmlWriter);

            _xmlWriter = xmlWriter;
            _version = version;
        }

        internal void WriteSchema(DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(databaseMapping);

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
            DebugCheck.NotNull(databaseMapping);

            _entityTypeNamespace = databaseMapping.Model.NamespaceNames.SingleOrDefault();
            _dbSchemaName = databaseMapping.Database.Containers.Single().Name;

            WriteEntityContainerMappingElement(databaseMapping.EntityContainerMappings.First());
        }

        // internal for testing
        internal void WriteEntityContainerMappingElement(StorageEntityContainerMapping containerMapping)
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

            foreach (var functionMapping in containerMapping.FunctionImportMappings.OfType<FunctionImportMappingComposable>())
            {
                WriteFunctionImportMappingElement(functionMapping);
            }

            _xmlWriter.WriteEndElement();
        }

        public void WriteEntitySetMappingElement(StorageEntitySetMapping entitySetMapping)
        {
            DebugCheck.NotNull(entitySetMapping);

            _xmlWriter.WriteStartElement(StorageMslConstructs.EntitySetMappingElement);
            _xmlWriter.WriteAttributeString(StorageMslConstructs.EntitySetMappingNameAttribute, entitySetMapping.EntitySet.Name);

            foreach (var entityTypeMapping in entitySetMapping.EntityTypeMappings)
            {
                WriteEntityTypeMappingElement(entityTypeMapping);
            }

            foreach (var modificationFunctionMapping in entitySetMapping.ModificationFunctionMappings)
            {
                _xmlWriter.WriteStartElement(StorageMslConstructs.EntityTypeMappingElement);
                _xmlWriter.WriteAttributeString(
                    StorageMslConstructs.EntityTypeMappingTypeNameAttribute,
                    GetEntityTypeName(_entityTypeNamespace + "." + modificationFunctionMapping.EntityType.Name, false));

                WriteModificationFunctionMapping(modificationFunctionMapping);

                _xmlWriter.WriteEndElement();
            }

            _xmlWriter.WriteEndElement();
        }

        public void WriteAssociationSetMappingElement(StorageAssociationSetMapping associationSetMapping)
        {
            DebugCheck.NotNull(associationSetMapping);

            _xmlWriter.WriteStartElement(StorageMslConstructs.AssociationSetMappingElement);
            _xmlWriter.WriteAttributeString(
                StorageMslConstructs.AssociationSetMappingNameAttribute, associationSetMapping.AssociationSet.Name);
            _xmlWriter.WriteAttributeString(
                StorageMslConstructs.AssociationSetMappingTypeNameAttribute,
                _entityTypeNamespace + "." + associationSetMapping.AssociationSet.ElementType.Name);
            _xmlWriter.WriteAttributeString(
                StorageMslConstructs.AssociationSetMappingStoreEntitySetAttribute, associationSetMapping.Table.Name);

            WriteAssociationEndMappingElement(associationSetMapping.SourceEndMapping);
            WriteAssociationEndMappingElement(associationSetMapping.TargetEndMapping);

            if (associationSetMapping.ModificationFunctionMapping != null)
            {
                WriteModificationFunctionMapping(associationSetMapping.ModificationFunctionMapping);
            }

            foreach (var conditionColumn in associationSetMapping.ColumnConditions)
            {
                WriteConditionElement(conditionColumn);
            }

            _xmlWriter.WriteEndElement();
        }

        private void WriteAssociationEndMappingElement(StorageEndPropertyMapping endMapping)
        {
            DebugCheck.NotNull(endMapping);

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
            DebugCheck.NotNull(entityTypeMapping);

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

        internal void WriteMappingFragmentElement(StorageMappingFragment mappingFragment)
        {
            DebugCheck.NotNull(mappingFragment);

            _xmlWriter.WriteStartElement(StorageMslConstructs.MappingFragmentElement);

            _xmlWriter.WriteAttributeString(
                StorageMslConstructs.MappingFragmentStoreEntitySetAttribute, 
                mappingFragment.TableSet.Name);

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

        public void WriteFunctionImportMappingElement(FunctionImportMappingComposable functionImportMapping)
        {
            DebugCheck.NotNull(functionImportMapping);

            _xmlWriter.WriteStartElement(StorageMslConstructs.FunctionImportMappingElement);
            _xmlWriter.WriteAttributeString(
                StorageMslConstructs.FunctionImportMappingFunctionNameAttribute,
                functionImportMapping.TargetFunction.FullName);
            _xmlWriter.WriteAttributeString(
                StorageMslConstructs.FunctionImportMappingFunctionImportNameAttribute,
                functionImportMapping.FunctionImport.Name);
            _xmlWriter.WriteStartElement(StorageMslConstructs.FunctionImportMappingResultMapping);

            Debug.Assert(
                functionImportMapping.StructuralTypeMappings.Count == 1,
                "multiple result sets not supported.");
            Debug.Assert(
                functionImportMapping.StructuralTypeMappings.First().Item1.BuiltInTypeKind == BuiltInTypeKind.ComplexType,
                "mapping to entity sets not supported.");

            var structuralMapping = functionImportMapping.StructuralTypeMappings.Single();
            _xmlWriter.WriteStartElement(StorageMslConstructs.ComplexTypeMappingElement);
            _xmlWriter.WriteAttributeString(StorageMslConstructs.ComplexTypeMappingTypeNameAttribute, structuralMapping.Item1.FullName);
            foreach (StorageScalarPropertyMapping propertyMapping in structuralMapping.Item3)
            {
                WritePropertyMapping(propertyMapping);
            }

            _xmlWriter.WriteEndElement();
            _xmlWriter.WriteEndElement();
            _xmlWriter.WriteEndElement();
        }

        private void WriteModificationFunctionMapping(StorageEntityTypeModificationFunctionMapping modificationFunctionMapping)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            _xmlWriter.WriteStartElement(StorageMslConstructs.ModificationFunctionMappingElement);

            WriteFunctionMapping(StorageMslConstructs.InsertFunctionElement, modificationFunctionMapping.InsertFunctionMapping);
            WriteFunctionMapping(StorageMslConstructs.UpdateFunctionElement, modificationFunctionMapping.UpdateFunctionMapping);
            WriteFunctionMapping(StorageMslConstructs.DeleteFunctionElement, modificationFunctionMapping.DeleteFunctionMapping);

            _xmlWriter.WriteEndElement();
        }

        private void WriteModificationFunctionMapping(StorageAssociationSetModificationFunctionMapping modificationFunctionMapping)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            _xmlWriter.WriteStartElement(StorageMslConstructs.ModificationFunctionMappingElement);

            WriteFunctionMapping(
                StorageMslConstructs.InsertFunctionElement,
                modificationFunctionMapping.InsertFunctionMapping,
                associationSetMapping: true);

            WriteFunctionMapping(
                StorageMslConstructs.DeleteFunctionElement,
                modificationFunctionMapping.DeleteFunctionMapping,
                associationSetMapping: true);

            _xmlWriter.WriteEndElement();
        }

        public void WriteFunctionMapping(
            string functionElement, StorageModificationFunctionMapping functionMapping, bool associationSetMapping = false)
        {
            DebugCheck.NotNull(functionMapping);

            _xmlWriter.WriteStartElement(functionElement);
            _xmlWriter.WriteAttributeString(StorageMslConstructs.FunctionNameAttribute, functionMapping.Function.FullName);

            if (functionMapping.RowsAffectedParameter != null)
            {
                _xmlWriter.WriteAttributeString(
                    StorageMslConstructs.RowsAffectedParameterAttribute,
                    functionMapping.RowsAffectedParameter.Name);
            }

            if (!associationSetMapping)
            {
                WritePropertyParameterBindings(functionMapping.ParameterBindings);
                WriteAssociationParameterBindings(functionMapping.ParameterBindings);

                if (functionMapping.ResultBindings != null)
                {
                    WriteResultBindings(functionMapping.ResultBindings);
                }
            }
            else
            {
                WriteAssociationSetMappingParameterBindings(functionMapping.ParameterBindings);
            }

            _xmlWriter.WriteEndElement();
        }

        private void WriteAssociationSetMappingParameterBindings(
            IEnumerable<StorageModificationFunctionParameterBinding> parameterBindings)
        {
            DebugCheck.NotNull(parameterBindings);

            var propertyGroups
                = from pm in parameterBindings
                  where pm.MemberPath.AssociationSetEnd != null
                  group pm by pm.MemberPath.AssociationSetEnd;

            foreach (var group in propertyGroups)
            {
                _xmlWriter.WriteStartElement(StorageMslConstructs.EndPropertyMappingElement);
                _xmlWriter.WriteAttributeString(StorageMslConstructs.EndPropertyMappingNameAttribute, group.Key.Name);

                foreach (var functionParameterBinding in group)
                {
                    WriteScalarParameterElement(functionParameterBinding.MemberPath.Members.First(), functionParameterBinding);
                }

                _xmlWriter.WriteEndElement();
            }
        }

        private void WritePropertyParameterBindings(
            IEnumerable<StorageModificationFunctionParameterBinding> parameterBindings, int level = 0)
        {
            DebugCheck.NotNull(parameterBindings);

            var propertyGroups
                = from pm in parameterBindings
                  where pm.MemberPath.AssociationSetEnd == null
                        && pm.MemberPath.Members.Count() > level
                  group pm by pm.MemberPath.Members.ElementAt(level);

            foreach (var group in propertyGroups)
            {
                var property = (EdmProperty)group.Key;

                if (property.IsComplexType)
                {
                    _xmlWriter.WriteStartElement(StorageMslConstructs.ComplexPropertyElement);
                    _xmlWriter.WriteAttributeString(StorageMslConstructs.ComplexPropertyNameAttribute, property.Name);
                    _xmlWriter.WriteAttributeString(
                        StorageMslConstructs.ComplexPropertyTypeNameAttribute,
                        _entityTypeNamespace + "." + property.ComplexType.Name);

                    WritePropertyParameterBindings(group, level + 1);

                    _xmlWriter.WriteEndElement();
                }
                else
                {
                    foreach (var parameterBinding in group)
                    {
                        WriteScalarParameterElement(property, parameterBinding);
                    }
                }
            }
        }

        private void WriteAssociationParameterBindings(
            IEnumerable<StorageModificationFunctionParameterBinding> parameterBindings)
        {
            DebugCheck.NotNull(parameterBindings);

            var propertyGroups
                = from pm in parameterBindings
                  where pm.MemberPath.AssociationSetEnd != null
                  group pm by pm.MemberPath.AssociationSetEnd;

            foreach (var group in propertyGroups)
            {
                _xmlWriter.WriteStartElement(StorageMslConstructs.AssociationEndElement);

                var assocationSet = group.Key.ParentAssociationSet;

                _xmlWriter.WriteAttributeString(StorageMslConstructs.AssociationSetAttribute, assocationSet.Name);
                _xmlWriter.WriteAttributeString(StorageMslConstructs.FromAttribute, group.Key.Name);
                _xmlWriter.WriteAttributeString(
                    StorageMslConstructs.ToAttribute,
                    assocationSet.AssociationSetEnds.Single(ae => ae != group.Key).Name);

                foreach (var functionParameterBinding in group)
                {
                    WriteScalarParameterElement(functionParameterBinding.MemberPath.Members.First(), functionParameterBinding);
                }

                _xmlWriter.WriteEndElement();
            }
        }

        private void WriteResultBindings(IEnumerable<StorageModificationFunctionResultBinding> resultBindings)
        {
            DebugCheck.NotNull(resultBindings);

            foreach (var resultBinding in resultBindings)
            {
                _xmlWriter.WriteStartElement(StorageMslConstructs.ResultBindingElement);
                _xmlWriter.WriteAttributeString(StorageMslConstructs.ScalarPropertyNameAttribute, resultBinding.Property.Name);
                _xmlWriter.WriteAttributeString(StorageMslConstructs.ScalarPropertyColumnNameAttribute, resultBinding.ColumnName);
                _xmlWriter.WriteEndElement();
            }
        }

        private void WriteScalarParameterElement(EdmMember member, StorageModificationFunctionParameterBinding parameterBinding)
        {
            DebugCheck.NotNull(member);
            DebugCheck.NotNull(parameterBinding);

            _xmlWriter.WriteStartElement(StorageMslConstructs.ScalarPropertyElement);
            _xmlWriter.WriteAttributeString(StorageMslConstructs.ScalarPropertyNameAttribute, member.Name);
            _xmlWriter.WriteAttributeString(StorageMslConstructs.ParameterNameAttribute, parameterBinding.Parameter.Name);
            _xmlWriter.WriteAttributeString(
                StorageMslConstructs.ParameterVersionAttribute,
                parameterBinding.IsCurrent
                    ? StorageMslConstructs.ParameterVersionAttributeCurrentValue
                    : StorageMslConstructs.ParameterVersionAttributeOriginalValue);
            _xmlWriter.WriteEndElement();
        }

        private void WritePropertyMapping(StoragePropertyMapping propertyMapping)
        {
            DebugCheck.NotNull(propertyMapping);

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
            DebugCheck.NotNull(scalarPropertyMapping);

            WriteScalarPropertyElement(scalarPropertyMapping.EdmProperty, scalarPropertyMapping.ColumnProperty);
        }

        private void WritePropertyMapping(StorageComplexPropertyMapping complexPropertyMapping)
        {
            DebugCheck.NotNull(complexPropertyMapping);

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
            DebugCheck.NotEmpty(fullyQualifiedEntityTypeName);

            if (isHierarchyMapping)
            {
                return StorageMslConstructs.IsTypeOf + fullyQualifiedEntityTypeName + StorageMslConstructs.IsTypeOfTerminal;
            }

            return fullyQualifiedEntityTypeName;
        }

        private void WriteConditionElement(StorageConditionPropertyMapping condition)
        {
            DebugCheck.NotNull(condition);

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
            DebugCheck.NotNull(property);
            DebugCheck.NotNull(column);

            _xmlWriter.WriteStartElement(StorageMslConstructs.ScalarPropertyElement);
            _xmlWriter.WriteAttributeString(StorageMslConstructs.ScalarPropertyNameAttribute, property.Name);
            _xmlWriter.WriteAttributeString(StorageMslConstructs.ScalarPropertyColumnNameAttribute, column.Name);
            _xmlWriter.WriteEndElement();
        }
    }
}
