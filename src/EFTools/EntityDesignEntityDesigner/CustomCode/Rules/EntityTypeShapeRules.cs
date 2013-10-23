// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.Rules
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges;
    using Microsoft.Data.Entity.Design.EntityDesigner.Utils;
    using Microsoft.Data.Entity.Design.EntityDesigner.View;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling.Diagrams;

    [RuleOn(typeof(EntityTypeShape), FireTime = TimeToFire.TopLevelCommit)]
    internal sealed class EntityTypeShape_AddRule : AddRule
    {
        public override void ElementAdded(ElementAddedEventArgs e)
        {
            var entityShape = e.ModelElement as EntityTypeShape;
            Debug.Assert(entityShape != null);

            var tx = ModelUtils.GetCurrentTx(entityShape.Store);
            Debug.Assert(tx != null);
            if (tx != null
                && !tx.IsSerializing)
            {
                ViewModelChangeContext.GetNewOrExistingContext(tx).ViewModelChanges.Add(new EntityTypeShapeAdd(entityShape));
            }
        }
    }

    // Set to "local-commit" to ensure that all changes shapes position will be persisted back to our model.
    [RuleOn(typeof(EntityTypeShape), FireTime = TimeToFire.LocalCommit)]
    internal sealed class EntityTypeShape_ChangeRule : ChangeRule
    {
        public override void ElementPropertyChanged(ElementPropertyChangedEventArgs e)
        {
            var entityShape = e.ModelElement as EntityTypeShape;
            Debug.Assert(entityShape != null);

            var tx = ModelUtils.GetCurrentTx(entityShape.Store);
            Debug.Assert(tx != null);
            if (tx != null
                && !tx.IsSerializing)
            {
                if (e.DomainProperty.Id == NodeShape.AbsoluteBoundsDomainPropertyId)
                {
                    var oldAbsoluteBounds = (RectangleD)e.OldValue;
                    var newAbsoluteBounds = (RectangleD)e.NewValue;

                    // If only the height changed then we don't need to change anything in the model.
                    // Also check an edge case where the first entity shape added to the diagram is added at a location that is within the NestedShapeMargin
                    // in the top left corner. In this scenario, the ViewModel EntityShape is by default added to the top left of diagram at the NestedShapeMargin
                    // value (i.e. (0.5,0.5)). This means that if the user tries add a shape at a point less than (0.5,0.5), i.e. if they drop/right-click in the
                    // top left corner, it will not change the location of the shape because any point smaller than (0.5,0.5) will be rounded off to (0.5,0.5)
                    // because of the NestedShapeMargin. This means that the old and new value of the absolute bounds will not change after the drop point
                    // is applied to the new ViewModel shape's default location. Normally this would not be a problem, but when we create the EntityTypeShape in
                    // the model we initally assign it a random (x,y) value (see CreateEntityTypeShapeCommand.cs) and we're relying on the EntityTypeShapeChange
                    // command issued by this rule to update the model x,y values so that the correct co-ords are persisted in the *.diagram file.
                    // Therefore there is an extra check in the IF statement below to catch the case where the user adds their first shape in the diagram at the
                    // top left corner inside the NestedShapeMargin as this is the easiest edge case to hit. Note that this will not protect against edge cases
                    // from subsequent updates where the user drops a new shape exactly on the same point as the initial location of the new ViewModel shape
                    // (the initial co-ords cascade from top-left down to the bottom-right as more shapes are added). If our users are dextrous enough to hit the
                    // edge cases for new entities and it's causing issues it would probably be best to remove this check entirely and always return an
                    // EntityTypeShapeChange (and forgo the optimization).
                    if (oldAbsoluteBounds.X == newAbsoluteBounds.X
                        && oldAbsoluteBounds.Y == newAbsoluteBounds.Y
                        && oldAbsoluteBounds.Width == newAbsoluteBounds.Width
                        && (newAbsoluteBounds.X > entityShape.Diagram.NestedShapesMargin.Width
                            || newAbsoluteBounds.Y > entityShape.Diagram.NestedShapesMargin.Height))
                    {
                        return;
                    }
                }

                if (e.DomainProperty.Id == NodeShape.AbsoluteBoundsDomainPropertyId
                    || e.DomainProperty.Id == NodeShape.IsExpandedDomainPropertyId)
                {
                    foreach (var link in entityShape.Link)
                    {
                        link.ManuallyRouted = false;
                    }
                    ViewModelChangeContext.GetNewOrExistingContext(tx)
                        .ViewModelChanges.Add(new EntityTypeShapeChange(entityShape, e.DomainProperty.Id));
                }
            }
        }
    }

    [RuleOn(typeof(EntityTypeShape), FireTime = TimeToFire.TopLevelCommit)]
    internal sealed class EntityTypeShape_DeleteRule : DeleteRule
    {
        public override void ElementDeleted(ElementDeletedEventArgs e)
        {
            base.ElementDeleted(e);

            var entityShape = e.ModelElement as EntityTypeShape;
            Debug.Assert(entityShape != null);

            var tx = ModelUtils.GetCurrentTx(entityShape.Store);
            Debug.Assert(tx != null);
            if (tx != null
                && !tx.IsSerializing)
            {
                ViewModelChangeContext.GetNewOrExistingContext(tx).ViewModelChanges.Add(new EntityTypeShapeDelete(entityShape));
            }
        }
    }
}
