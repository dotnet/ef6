// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     Command for changing complex property type
    /// </summary>
    internal class ChangeComplexPropertyTypeCommand : Command
    {
        private readonly ComplexConceptualProperty _property;
        private readonly ComplexType _newType;

        internal ChangeComplexPropertyTypeCommand(ComplexConceptualProperty property, ComplexType newType)
        {
            CommandValidation.ValidateProperty(property);
            CommandValidation.ValidateComplexType(newType);

            _property = property;
            _newType = newType;
        }

        protected override void PreInvoke(CommandProcessorContext cpc)
        {
            base.PreInvoke(cpc);

            // first make sure that we are changing the type
            if (_property != null
                && _property.ComplexType.Target != _newType)
            {
                foreach (var cp in _property.GetAntiDependenciesOfType<ComplexProperty>())
                {
                    // delete all related ComplexProperty mappings when the property type changes
                    DeleteEFElementCommand.DeleteInTransaction(cpc, cp);
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(_property != null, "Property is null");

            if (_property == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when _property is null.");
            }

            if (_property.ComplexType.Target == _newType)
            {
                // no change needed
                return;
            }

            // check for ComplexType circular definition
            var parent = _property.Parent as ComplexType;
            if (parent != null)
            {
                if (ModelHelper.ContainsCircularComplexTypeDefinition(parent, _newType))
                {
                    throw new CommandValidationFailedException(
                        String.Format(
                            CultureInfo.CurrentCulture, Resources.Error_CircularComplexTypeDefinitionOnChange, _newType.LocalName.Value));
                }
            }

            // Update to new type
            _property.ComplexType.SetRefName(_newType);
            _property.ComplexType.Rebind();
            Debug.Assert(_property.ComplexType.Status == BindingStatus.Known, "Rebind for the ComplexType failed");
        }
    }
}
