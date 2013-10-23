// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    internal class CreateFunctionImportScalarPropertyCommand : Command
    {
        private FunctionImportTypeMapping _typeMapping;
        private readonly Property _property;
        private readonly string _columnName;

        private FunctionImportScalarProperty _createdScalarProperty;

        /// <summary>
        ///     Creates new ScalarProperty inside specified FunctionImportTypeMapping
        /// </summary>
        /// <param name="typeMapping"></param>
        /// <param name="property">Valid c-side Property (either from EntityType or ComplexType)</param>
        /// <param name="columnName"></param>
        internal CreateFunctionImportScalarPropertyCommand(FunctionImportTypeMapping typeMapping, Property property, string columnName)
        {
            CommandValidation.ValidateFunctionImportTypeMapping(typeMapping);
            CommandValidation.ValidateConceptualProperty(property);
            ValidateString(columnName);

            _typeMapping = typeMapping;
            _property = property;
            _columnName = columnName;
        }

        /// <summary>
        ///     Creates new ScalarProperty inside FunctionImportTypeMapping created by the prereq command
        /// </summary>
        /// <param name="prereq"></param>
        /// <param name="property">Valid c-side Property (either from EntityType or ComplexType)</param>
        /// <param name="columnName"></param>
        internal CreateFunctionImportScalarPropertyCommand(
            CreateFunctionImportTypeMappingCommand prereq, Property property, string columnName)
        {
            ValidatePrereqCommand(prereq);
            CommandValidation.ValidateConceptualProperty(property);
            ValidateString(columnName);

            _property = property;
            _columnName = columnName;
            AddPreReqCommand(prereq);
        }

        protected override void ProcessPreReqCommands()
        {
            if (_typeMapping == null)
            {
                var prereq = GetPreReqCommand(CreateFunctionImportTypeMappingCommand.PrereqId) as CreateFunctionImportTypeMappingCommand;
                if (prereq != null)
                {
                    _typeMapping = prereq.TypeMapping;
                }

                Debug.Assert(_typeMapping != null, "We didn't get good FunctionImportTypeMapping out of the prereq command");
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "typeMapping")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "columnName")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // safety check
            Debug.Assert(
                _typeMapping != null && _property != null && _columnName != null,
                "InvokeInternal is called when _typeMapping or _property or _columnName is null.");

            if (_typeMapping == null
                || _property == null
                || String.IsNullOrEmpty(_columnName))
            {
                throw new InvalidOperationException("InvokeInternal is called when _typeMapping or _property or _columnName is null.");
            }

            _createdScalarProperty = new FunctionImportScalarProperty(_typeMapping, null);
            _createdScalarProperty.Name.SetRefName(_property);
            _createdScalarProperty.ColumnName.Value = _columnName;
            XmlModelHelper.NormalizeAndResolve(_createdScalarProperty);
            _typeMapping.AddScalarProperty(_createdScalarProperty);
        }

        internal FunctionImportScalarProperty ScalarProperty
        {
            get { return _createdScalarProperty; }
        }
    }
}
