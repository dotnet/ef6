// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Visitor
{
    internal class UnbindingVisitor : StateChangingVisitor
    {
        internal UnbindingVisitor()
            : base(EFElementState.Normalized)
        {
        }

        internal override void Visit(IVisitable visitable)
        {
            base.Visit(visitable);

            var ib = visitable as ItemBinding;
            if (ib != null)
            {
                ib.Unbind();
            }
        }
    }
}
