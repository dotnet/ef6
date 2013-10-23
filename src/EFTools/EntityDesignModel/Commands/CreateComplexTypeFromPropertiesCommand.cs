// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     This class creates a ComplexType from a set of EntityType properties
    ///     then it replace those properties with a property of created ComplexType.
    ///     All mappings are preserved.
    /// </summary>
    internal class CreateComplexTypeFromPropertiesCommand : Command
    {
        private readonly EntityType _entityType;
        private readonly List<Property> _properties;
        private ComplexType _createdComplexType;
        private ComplexConceptualProperty _createdComplexProperty;

        internal CreateComplexTypeFromPropertiesCommand(EntityType entityType, List<Property> properties)
        {
            CommandValidation.ValidateConceptualEntityType(entityType);
            Debug.Assert(properties.Count > 0, "No properties to copy");

            _entityType = entityType;
            _properties = properties;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // first create new ComplexType
            _createdComplexType = CreateComplexTypeCommand.CreateComplexTypeWithDefaultName(cpc);
            // add a copy of Entity properties to the ComplexType
            var copyCmd = new CopyPropertiesCommand(new PropertiesClipboardFormat(_properties), _createdComplexType);
            var propertyName = ModelHelper.GetUniqueConceptualPropertyName(
                ComplexConceptualProperty.DefaultComplexPropertyName, _entityType);
            // add a new Property of created ComplexType to the Entity
            var createCPCmd = new CreateComplexPropertyCommand(propertyName, _entityType, _createdComplexType);
            var cp = new CommandProcessor(cpc, copyCmd, createCPCmd);
            cp.Invoke();
            _createdComplexProperty = createCPCmd.Property;

            // preserve mappings
            foreach (var property in _properties)
            {
                if (property is ComplexConceptualProperty)
                {
                    var createdComplexTypeProperty =
                        _createdComplexType.FindPropertyByLocalName(property.LocalName.Value) as ComplexConceptualProperty;
                    Debug.Assert(createdComplexTypeProperty != null, "Copied complex property not found");
                    if (createdComplexTypeProperty != null)
                    {
                        foreach (var complexPropertyMapping in property.GetAntiDependenciesOfType<ComplexProperty>())
                        {
                            PreserveComplexPropertyMapping(cpc, complexPropertyMapping, createdComplexTypeProperty);
                        }

                        foreach (var fcp in property.GetAntiDependenciesOfType<FunctionComplexProperty>())
                        {
                            PreserveFunctionComplexPropertyMapping(cpc, fcp, createdComplexTypeProperty);
                        }
                    }
                }
                else
                {
                    var createdComplexTypeProperty = _createdComplexType.FindPropertyByLocalName(property.LocalName.Value);
                    Debug.Assert(createdComplexTypeProperty != null, "Copied property not found");
                    if (createdComplexTypeProperty != null)
                    {
                        // update EntityTypeMappings
                        foreach (var scalarPropertyMapping in property.GetAntiDependenciesOfType<ScalarProperty>())
                        {
                            PreserveScalarPropertyMapping(cpc, scalarPropertyMapping, createdComplexTypeProperty);
                        }

                        // update ModificationFunctionMappings
                        foreach (var fsp in property.GetAntiDependenciesOfType<FunctionScalarProperty>())
                        {
                            PreserveFunctionScalarPropertyMapping(cpc, fsp, createdComplexTypeProperty);
                        }
                    }
                }
            }
        }

        private void PreserveComplexPropertyMapping(
            CommandProcessorContext cpc, ComplexProperty complexPropertyMapping, ComplexConceptualProperty createdComplexTypeProperty)
        {
            // walk the Properties tree
            foreach (var sp in complexPropertyMapping.ScalarProperties())
            {
                PreserveScalarPropertyMapping(cpc, sp, createdComplexTypeProperty);
            }
            foreach (var cp in complexPropertyMapping.ComplexProperties())
            {
                PreserveComplexPropertyMapping(cpc, cp, createdComplexTypeProperty);
            }
        }

        private void PreserveFunctionComplexPropertyMapping(
            CommandProcessorContext cpc, FunctionComplexProperty fcp, ComplexConceptualProperty createdComplexTypeProperty)
        {
            // walk the Properties tree
            foreach (var fsp in fcp.ScalarProperties())
            {
                PreserveFunctionScalarPropertyMapping(cpc, fsp, createdComplexTypeProperty);
            }
            foreach (var childFcp in fcp.ComplexProperties())
            {
                PreserveFunctionComplexPropertyMapping(cpc, childFcp, createdComplexTypeProperty);
            }
        }

        private void PreserveScalarPropertyMapping(
            CommandProcessorContext cpc, ScalarProperty scalarPropertyMapping, Property createdComplexTypeProperty)
        {
            // this represents a path to a scalar Property in the original EntityType properties tree
            var propertiesChain = scalarPropertyMapping.GetMappedPropertiesList();
            // we need to create a corresponding path in changed EntityType
            // in order to do that we need to replace first (root) item with a created property from ComplexType...
            propertiesChain.RemoveAt(0);
            propertiesChain.Insert(0, createdComplexTypeProperty);
            // and add the created EntityType complex property as a root of that path
            propertiesChain.Insert(0, _createdComplexProperty);
            var cmd = new CreateFragmentScalarPropertyTreeCommand(_entityType, propertiesChain, scalarPropertyMapping.ColumnName.Target);
            CommandProcessor.InvokeSingleCommand(cpc, cmd);
        }

        private void PreserveFunctionScalarPropertyMapping(
            CommandProcessorContext cpc, FunctionScalarProperty fsp, Property createdComplexTypeProperty)
        {
            // this represents a path to a scalar Property in the original EntityType properties tree
            var propertiesChain = fsp.GetMappedPropertiesList();
            // we need to create a corresponding path in changed EntityType
            // in order to do that we need to replace first (root) item with a created property from ComplexType...
            propertiesChain.RemoveAt(0);
            propertiesChain.Insert(0, createdComplexTypeProperty);
            // and add the created EntityType complex property as a root of that path
            propertiesChain.Insert(0, _createdComplexProperty);
            var mf = fsp.GetParentOfType(typeof(ModificationFunction)) as ModificationFunction;
            Debug.Assert(
                null != mf,
                "PreserveFunctionScalarPropertyMapping(): Could not find ancestor of type + " + typeof(ModificationFunction).FullName);
            if (null != mf)
            {
                var cmd = new CreateFunctionScalarPropertyTreeCommand(
                    mf, propertiesChain, null, fsp.ParameterName.Target, fsp.Version.Value);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
            }
        }

        protected override void PostInvoke(CommandProcessorContext cpc)
        {
            foreach (var property in _properties)
            {
                var deleteCommand = property.GetDeleteCommand();
                var deletePropertyCommand = deleteCommand as DeletePropertyCommand;
                Debug.Assert(
                    deletePropertyCommand != null,
                    "Property.GetDeleteCommand() failed to return a DeletePropertyCommand, command translation will not receive the correct value for the IsConceptualDeleteOnly flag");

                if (deletePropertyCommand != null)
                {
                    deletePropertyCommand.IsConceptualOnlyDelete = true;
                }

                DeleteEFElementCommand.DeleteInTransaction(cpc, deletePropertyCommand);
            }
        }

        /// <summary>
        ///     The ComplexType that this command created
        /// </summary>
        internal ComplexType ComplexType
        {
            get { return _createdComplexType; }
        }

        /// <summary>
        ///     The ComplexConceptualProperty that this command created
        /// </summary>
        internal ComplexConceptualProperty ComplexConceptualProperty
        {
            get { return _createdComplexProperty; }
        }
    }
}
