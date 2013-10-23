// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner
{
    using Microsoft.VisualStudio.Modeling.Diagrams;

    internal partial class InheritanceToolConnectAction
    {
        /// <summary>
        ///     Prevent Inheritance from connecting to self
        /// </summary>
        partial class InheritanceToolConnectionType
        {
            public override bool CanCreateConnection(
                ShapeElement sourceShapeElement, ShapeElement targetShapeElement, ref string connectionWarning)
            {
                if ((sourceShapeElement != null)
                    && (targetShapeElement != null))
                {
                    if (RemovePassThroughShapes(sourceShapeElement) == RemovePassThroughShapes(targetShapeElement))
                    {
                        return false;
                    }
                }
                return base.CanCreateConnection(sourceShapeElement, targetShapeElement, ref connectionWarning);
            }

            private static ShapeElement RemovePassThroughShapes(ShapeElement shape)
            {
                if (shape is Compartment)
                {
                    return shape.ParentShape;
                }
                var swimlane = shape as SwimlaneShape;
                if (swimlane != null
                    && swimlane.ForwardDragDropToParent)
                {
                    return shape.ParentShape;
                }
                return shape;
            }
        }
    }
}
