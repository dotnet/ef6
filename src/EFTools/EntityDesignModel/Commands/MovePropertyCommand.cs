// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal enum MoveDirection
    {
        Up = 0,
        Down
    }

    /// <summary>
    ///     Command to move property with the given the move direction and number of steps.
    /// </summary>
    internal class MovePropertyCommand : Command
    {
        private readonly PropertyBase _property;
        private readonly MoveDirection _moveDirection;
        private readonly uint _step;

        internal MovePropertyCommand(PropertyBase property, MoveDirection dir, uint step)
        {
            _property = property;
            _moveDirection = dir;
            _step = step;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(_step > 0, "Parameter step value is not valid. The value must be greater than 0.");
            if (_step > 0)
            {
                // Skip moving property up if the property is already the first property.
                if (_moveDirection == MoveDirection.Up
                    && _property.PreviousSiblingInPropertyXElementOrder != null)
                {
                    var previousSibling = _property.PreviousSiblingInPropertyXElementOrder;
                    for (var i = 1; i < _step && previousSibling.PreviousSiblingInPropertyXElementOrder != null; i++)
                    {
                        previousSibling = previousSibling.PreviousSiblingInPropertyXElementOrder;
                    }
                    _property.MoveTo(new InsertPropertyPosition(previousSibling, true));
                }
                    // Skip moving property down if the property is already the last property.
                else if (_moveDirection == MoveDirection.Down
                         && _property.NextSiblingInPropertyXElementOrder != null)
                {
                    var nextSibling = _property.NextSiblingInPropertyXElementOrder;
                    for (var i = 1; i < _step && nextSibling.NextSiblingInPropertyXElementOrder != null; i++)
                    {
                        nextSibling = nextSibling.NextSiblingInPropertyXElementOrder;
                    }
                    _property.MoveTo(new InsertPropertyPosition(nextSibling, false));
                }
            }
        }
    }
}
