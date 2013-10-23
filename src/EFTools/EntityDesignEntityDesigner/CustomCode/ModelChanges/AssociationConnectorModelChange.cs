// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using Microsoft.Data.Entity.Design.EntityDesigner.Rules;
    using Microsoft.Data.Entity.Design.EntityDesigner.View;

    internal abstract class AssociationConnectorModelChange : ViewModelChange
    {
        private readonly AssociationConnector _associationConnector;

        internal override bool IsDiagramChange
        {
            get { return true; }
        }

        protected AssociationConnectorModelChange(AssociationConnector associationConnector)
        {
            _associationConnector = associationConnector;
        }

        public AssociationConnector AssociationConnector
        {
            get { return _associationConnector; }
        }
    }
}
