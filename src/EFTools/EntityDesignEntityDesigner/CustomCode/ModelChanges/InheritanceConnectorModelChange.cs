// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using Microsoft.Data.Entity.Design.EntityDesigner.Rules;
    using Microsoft.Data.Entity.Design.EntityDesigner.View;

    internal abstract class InheritanceConnectorModelChange : ViewModelChange
    {
        private readonly InheritanceConnector _inheritanceConnector;

        internal override bool IsDiagramChange
        {
            get { return true; }
        }

        protected InheritanceConnectorModelChange(InheritanceConnector inheritanceConnector)
        {
            _inheritanceConnector = inheritanceConnector;
        }

        public InheritanceConnector InheritanceConnector
        {
            get { return _inheritanceConnector; }
        }
    }
}
