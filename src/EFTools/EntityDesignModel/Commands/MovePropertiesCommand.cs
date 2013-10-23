// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class MovePropertiesCommand : Command
    {
        private readonly IList<PropertyBase> _properties;
        private readonly MoveDirection _moveDirection;
        private readonly uint _step;

        internal MovePropertiesCommand(IList<PropertyBase> properties, MoveDirection dir, uint step)
        {
            _properties = properties;
            _moveDirection = dir;
            _step = step;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(_properties != null && _properties.Count > 0, "There is no property to be moved.");
            if (_properties != null
                && _properties.Count > 0)
            {
                Debug.Assert(_step > 0, "Parameter step value is not valid. The value must be greater than 0.");
                if (_step > 0)
                {
                    // Properties are moved in the following order:
                    // - If the properties are moved forward (MoveDirection == Down), we need to move the property that are closer to the last-property first.
                    // - If the properties are moved backward (MoveDirection == Up), we need to move the property that are closer to the first-property first.
                    var sortedProperties = ModelHelper.GetListOfPropertiesInTheirXElementsOrder(_properties);
                    Debug.Assert(
                        sortedProperties.Count == _properties.Count, "The sorted properties should have the same number of properties.");

                    PropertyBase previouslyMovedProperty = null;
                    foreach (var property in (_moveDirection == MoveDirection.Up ? sortedProperties : sortedProperties.Reverse()))
                    {
                        // Ensure that properties are moved don't change order.
                        // For example: if property A, B and C are moved forward, the move should not cause property A to be placed after Property B.
                        var numberOfSteps = GetNumberOfMoveStep(property, previouslyMovedProperty);

                        if (numberOfSteps > 0)
                        {
                            CommandProcessor.InvokeSingleCommand(cpc, new MovePropertyCommand(property, _moveDirection, numberOfSteps));
                        }
                        previouslyMovedProperty = property;
                    }
                }
            }
        }

        /// <summary>
        ///     Return the number of steps in which the property will be moved.
        ///     The logic is to return the Math.Min of the # of steps passed in to the command and the # of steps from the property to the previously moved property.
        ///     If previously moved property is null, just return the # of steps passed in to the command.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="previouslyMovedProperty"></param>
        /// <returns></returns>
        private uint GetNumberOfMoveStep(PropertyBase property, PropertyBase previouslyMovedProperty)
        {
            if (previouslyMovedProperty == null)
            {
                return _step;
            }

            uint numberOfSteps = 0;
            var insertLocation = _moveDirection == MoveDirection.Up
                                     ? property.PreviousSiblingInPropertyXElementOrder
                                     : property.NextSiblingInPropertyXElementOrder;
            for (; numberOfSteps < _step && insertLocation != null && insertLocation != previouslyMovedProperty; numberOfSteps++)
            {
                insertLocation = _moveDirection == MoveDirection.Up
                                     ? insertLocation.PreviousSiblingInPropertyXElementOrder
                                     : insertLocation.NextSiblingInPropertyXElementOrder;
            }
            return numberOfSteps;
        }
    }
}
