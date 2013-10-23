// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Visitor
{
    internal class StateChangingVisitor : Visitor
    {
        private readonly EFElementState _state;

        internal StateChangingVisitor(EFElementState state)
        {
            _state = state;
        }

        internal override void Visit(IVisitable visitable)
        {
            var item = visitable as EFElement;

            // if this is an EFElement and it is a higher state than
            // what we want to set it to, set it
            if (item != null
                &&
                item.State > _state)
            {
                item.State = _state;
            }
        }
    }
}
