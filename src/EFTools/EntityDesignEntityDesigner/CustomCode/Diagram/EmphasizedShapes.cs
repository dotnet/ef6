// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.View
{
    using Microsoft.VisualStudio.Modeling.Diagrams;

    /// <summary>
    ///     Collection of diagram items that will be emphasized.
    /// </summary>
    internal sealed class EmphasizedShapes : DiagramItemCollection
    {
        /// <summary>
        ///     Adds the specified DiagramItems to the current emphasis list.
        /// </summary>
        /// <remarks>
        ///     If a DiagramItem in the collection is already in the emphasis list, the DiagramItem is ignored.
        /// </remarks>
        /// <param name="diagramItems">The collection of DiagramItems to add.</param>
        private void Add(DiagramItemCollection diagramItems)
        {
            // only add shapes that are not currently in the emphasis list.
            foreach (var diagramItem in diagramItems)
            {
                if (!Contains(diagramItem))
                {
                    base.Add(diagramItem);
                }
            }
        }

        /// <summary>
        ///     Replaces the current emphasis list with a new emphasis list.
        /// </summary>
        /// <param name="diagramItems">The collection of DiagramItems that is to replace the current emphasis list.</param>
        /// <remarks>
        ///     If the DiagramItemCollection is null, then the emphasis list is cleared.
        /// </remarks>
        internal void Set(DiagramItemCollection diagramItems)
        {
            Invalidate(); // Invalidate to ensure that the old shapes will be repainted.
            List.Clear();
            if (diagramItems != null)
            {
                Add(diagramItems);
                Invalidate();
            }
        }

        /// <summary>
        ///     Invalidates the current emphasis list of ShapeElements
        /// </summary>
        private void Invalidate()
        {
            foreach (DiagramItem diagramItem in List)
            {
                if (!(diagramItem.Shape is Diagram))
                {
                    diagramItem.Shape.Invalidate();
                }
            }
        }
    }
}
