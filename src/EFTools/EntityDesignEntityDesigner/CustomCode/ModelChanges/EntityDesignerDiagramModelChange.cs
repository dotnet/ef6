// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using DslModeling = Microsoft.VisualStudio.Modeling;

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using Microsoft.Data.Entity.Design.EntityDesigner.Rules;
    using Microsoft.Data.Entity.Design.EntityDesigner.View;

    internal abstract class EntityDesignerDiagramModelChange : ViewModelChange
    {
        private readonly EntityDesignerDiagram _diagram;

        internal override bool IsDiagramChange
        {
            get { return true; }
        }

        protected EntityDesignerDiagramModelChange(EntityDesignerDiagram diagram)
        {
            _diagram = diagram;
        }

        public EntityDesignerDiagram Diagram
        {
            get { return _diagram; }
        }
    }
}
