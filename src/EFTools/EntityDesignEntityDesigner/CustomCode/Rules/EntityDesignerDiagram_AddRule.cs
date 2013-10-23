// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.Rules
{
    using Microsoft.Data.Entity.Design.EntityDesigner.View;
    using Microsoft.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling.Diagrams;

    /// <summary>
    ///     After a shape is added to the diagram, keep track of it
    ///     so that the auto arrange can be done for all the objects before the
    ///     transaction is complete.
    ///     (This used to include relations, but that doesn't seem to be needed)
    /// </summary>
    [RuleOn(typeof(ShapeElement), FireTime = TimeToFire.TopLevelCommit, Priority = DiagramFixupConstants.AddShapeRulePriority)]
    internal sealed class EntityDesignerDiagram_AddRule : AddRule
    {
        public override void ElementAdded(ElementAddedEventArgs e)
        {
            // if aren't adding a shape, just return
            var addedShape = e.ModelElement as ShapeElement;
            if (addedShape == null)
            {
                return;
            }

            // only layout classes and links
            if (!(addedShape is EntityTypeShape ||
                  addedShape is AssociationConnector ||
                  addedShape is InheritanceConnector))
            {
                return;
            }

            // layout this new shape
            var diagram = addedShape.Diagram as EntityDesignerDiagram;
            if (diagram != null
                && diagram.Arranger != null)
            {
                diagram.Arranger.Add(addedShape, false);
            }
        }
    }
}
