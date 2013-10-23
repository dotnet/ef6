// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class CopyPropertiesCommand : Command
    {
        private readonly PropertiesClipboardFormat _clipboardProperties;
        private readonly EntityType _entity;
        private readonly ComplexType _complexType;
        private readonly InsertPropertyPosition _insertPosition;
        private List<PropertyBase> _properties;

        /// <summary>
        ///     Creates a copy of a collection of Properties from clipboard format in the EntityType
        /// </summary>
        internal CopyPropertiesCommand(PropertiesClipboardFormat clipboardProperties, EntityType entity)
            : this(clipboardProperties, entity, null)
        {
        }

        /// <summary>
        ///     Creates a copy of a collection of Properties from clipboard format in the EntityType
        /// </summary>
        internal CopyPropertiesCommand(
            PropertiesClipboardFormat clipboardProperties, EntityType entity, InsertPropertyPosition insertPosition)
        {
            _clipboardProperties = clipboardProperties;
            _entity = entity;
            _insertPosition = insertPosition;
            _properties = null;
        }

        /// <summary>
        ///     Creates a copy of a collection of Properties from clipboard format in the ComplexType
        /// </summary>
        internal CopyPropertiesCommand(PropertiesClipboardFormat clipboardProperties, ComplexType complexType)
        {
            _clipboardProperties = clipboardProperties;
            _complexType = complexType;
        }

        /// <summary>
        ///     The properties created by this command.
        /// </summary>
        internal List<PropertyBase> Properties
        {
            get { return _properties; }
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(_entity != null || _complexType != null, "Undefined parent type");
            _properties = new List<PropertyBase>();

            // create copies of Properties
            foreach (var clipboardProperty in _clipboardProperties.ClipboardProperties)
            {
                // If there are multiple properties that are copied, we need to advance the insertPosition's PropertyRef.
                // This is to ensure that properties are created in the correct order.
                if (_insertPosition != null
                    && _properties.Count > 0)
                {
                    _insertPosition.InsertBefore = false;
                    _insertPosition.InsertAtProperty = _properties[_properties.Count - 1];
                }

                var cmd = _entity != null
                              ? new CopyPropertyCommand(clipboardProperty, _entity, _insertPosition)
                              : new CopyPropertyCommand(clipboardProperty, _complexType);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
                _properties.Add(cmd.Property);
            }
        }
    }
}
