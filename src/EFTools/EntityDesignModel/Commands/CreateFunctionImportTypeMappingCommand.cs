// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    internal class CreateFunctionImportTypeMappingCommand : Command
    {
        internal static readonly string PrereqId = "CreateFunctionImportTypeMappingCommand";

        private FunctionImportMapping _functionImportMapping;
        private readonly EntityType _entityType;
        private ComplexType _complexType;

        private FunctionImportTypeMapping _createdTypeMapping;
        private bool _createDefaultScalarProperties;
        private IDictionary<string, string> _mapPropertyNameToColumnName;

        /// <summary>
        ///     Creates new EntityTypeMapping inside specified FunctionImportMapping
        /// </summary>
        /// <param name="functionImportMapping"></param>
        /// <param name="entityType"></param>
        internal CreateFunctionImportTypeMappingCommand(FunctionImportMapping functionImportMapping, EntityType entityType)
            : base(PrereqId)
        {
            CommandValidation.ValidateFunctionImportMapping(functionImportMapping);
            CommandValidation.ValidateEntityType(entityType);

            _functionImportMapping = functionImportMapping;
            _entityType = entityType;
        }

        /// <summary>
        ///     Creates new ComplexTypeMapping inside specified FunctionImportMapping
        /// </summary>
        /// <param name="functionImportMapping"></param>
        /// <param name="complexType"></param>
        internal CreateFunctionImportTypeMappingCommand(FunctionImportMapping functionImportMapping, ComplexType complexType)
            : base(PrereqId)
        {
            CommandValidation.ValidateFunctionImportMapping(functionImportMapping);
            CommandValidation.ValidateComplexType(complexType);

            _functionImportMapping = functionImportMapping;
            _complexType = complexType;
        }

        /// <summary>
        ///     Creates new ComplexTypeMapping inside specified FunctionImportMapping
        /// </summary>
        /// <param name="createFunctionImportMapping"></param>
        /// <param name="complexType"></param>
        internal CreateFunctionImportTypeMappingCommand(
            CreateFunctionImportMappingCommand createFunctionImportMappingCmd, ComplexType complexType)
            : base(PrereqId)
        {
            CommandValidation.ValidateComplexType(complexType);
            _complexType = complexType;
            AddPreReqCommand(createFunctionImportMappingCmd);
        }

        /// <summary>
        ///     Creates new ComplexTypeMapping inside specified FunctionImportMapping
        /// </summary>
        /// <param name="createFunctionImportMapping"></param>
        /// <param name="complexType"></param>
        internal CreateFunctionImportTypeMappingCommand(
            FunctionImportMapping functionImportMapping, CreateComplexTypeCommand createComplexTypeCmd)
            : base(PrereqId)
        {
            CommandValidation.ValidateFunctionImportMapping(functionImportMapping);
            _functionImportMapping = functionImportMapping;
            AddPreReqCommand(createComplexTypeCmd);
        }

        /// <summary>
        ///     Creates new ComplexTypeMapping inside specified FunctionImportMapping
        /// </summary>
        /// <param name="functionImportMapping"></param>
        /// <param name="complexType"></param>
        internal CreateFunctionImportTypeMappingCommand(
            CreateFunctionImportMappingCommand createFunctionImportMappingCmd, CreateComplexTypeCommand createComplexTypeCmd)
            : base(PrereqId)
        {
            AddPreReqCommand(createFunctionImportMappingCmd);
            AddPreReqCommand(createComplexTypeCmd);
        }

        /// <summary>
        ///     If set to true, the command will create return-type-mapping properties.
        ///     Default is set to false.
        /// </summary>
        internal bool CreateDefaultScalarProperties
        {
            get { return _createDefaultScalarProperties; }
            set { _createDefaultScalarProperties = value; }
        }

        /// <summary>
        ///     Contains the map between property name and column name. This map is used when creating Function-Import-Mapping scalar property.
        /// </summary>
        internal IDictionary<string, string> PropertyNameToColumnNameMap
        {
            get { return _mapPropertyNameToColumnName; }
            set { _mapPropertyNameToColumnName = value; }
        }

        protected override void ProcessPreReqCommands()
        {
            var preregCommand1 = GetPreReqCommand(CreateFunctionImportMappingCommand.PrereqId) as CreateFunctionImportMappingCommand;
            if (preregCommand1 != null)
            {
                _functionImportMapping = preregCommand1.FunctionImportMapping;
                Debug.Assert(
                    _functionImportMapping != null, "CreateFunctionImportMappingCommand command return null value of _functionImportMapping");
            }

            var preregCommand2 = GetPreReqCommand(CreateComplexTypeCommand.PrereqId) as CreateComplexTypeCommand;
            if (preregCommand2 != null)
            {
                _complexType = preregCommand2.ComplexType;
                Debug.Assert(_complexType != null, "CreateComplexTypeCommand command return null value of ComplexType");
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "complexType")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "functionImportMapping")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "entityType")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // safety check, this should never be hit
            if ((_entityType == null && _complexType == null)
                || _functionImportMapping == null)
            {
                throw new InvalidOperationException(
                    "InvokeInternal is called when _entityType or _complexType or _functionImportMapping is null.");
            }

            if (_functionImportMapping.ResultMapping == null)
            {
                _functionImportMapping.ResultMapping = new ResultMapping(_functionImportMapping, null);
                XmlModelHelper.NormalizeAndResolve(_functionImportMapping.ResultMapping);
            }

            // first check if we already have one (if found we'll simply return it)
            _createdTypeMapping = _entityType != null
                                      ? _functionImportMapping.ResultMapping.FindTypeMapping(_entityType)
                                      : _functionImportMapping.ResultMapping.FindTypeMapping(_complexType);

            if (_createdTypeMapping == null)
            {
                if (_entityType != null)
                {
                    _createdTypeMapping = new FunctionImportEntityTypeMapping(_functionImportMapping.ResultMapping, null);
                    _createdTypeMapping.TypeName.SetRefName(_entityType);
                }
                else
                {
                    _createdTypeMapping = new FunctionImportComplexTypeMapping(_functionImportMapping.ResultMapping, null);
                    _createdTypeMapping.TypeName.SetRefName(_complexType);
                }

                XmlModelHelper.NormalizeAndResolve(_createdTypeMapping);
                _functionImportMapping.ResultMapping.AddTypeMapping(_createdTypeMapping);
            }

            if (_createDefaultScalarProperties && _createdTypeMapping != null)
            {
                IEnumerable<Property> properties = null;

                if (_entityType != null)
                {
                    properties = _entityType.Properties();
                }
                else if (_complexType != null)
                {
                    properties = _complexType.Properties();
                }

                if (properties != null)
                {
                    foreach (var prop in properties)
                    {
                        // Skip if the property is a Complex Property or if we already have the Scalar Property in the type mapping.
                        if ((prop is ComplexConceptualProperty) == false
                            && _createdTypeMapping.FindScalarProperty(prop) == null)
                        {
                            var columnName = (_mapPropertyNameToColumnName != null
                                              && _mapPropertyNameToColumnName.ContainsKey(prop.DisplayName)
                                                  ? _mapPropertyNameToColumnName[prop.DisplayName]
                                                  : prop.DisplayName);
                            CommandProcessor.InvokeSingleCommand(
                                cpc, new CreateFunctionImportScalarPropertyCommand(_createdTypeMapping, prop, columnName));
                        }
                    }
                }
            }
        }

        internal FunctionImportTypeMapping TypeMapping
        {
            get { return _createdTypeMapping; }
        }
    }
}
