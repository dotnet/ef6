// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using EntityDesignerView = Microsoft.Data.Entity.Design.EntityDesigner.View;
using EntityDesignerViewModel = Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;

namespace EFDesigner.InProcTests.Extensions
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.Modeling.Diagrams;

    internal static class DiagramExtensions
    {
        internal static EntityDesignerView.EntityTypeShape GetShape(
            this EntityDesignerView.EntityDesignerDiagram entityDesignerDiagram, string name)
        {
            return entityDesignerDiagram
                .NestedChildShapes
                .OfType<EntityDesignerView.EntityTypeShape>()
                .SingleOrDefault(ets => ((EntityDesignerViewModel.EntityType)ets.ModelElement).Name == name);
        }

        internal static void SelectDiagramItems(
            this EntityDesignerView.EntityDesignerDiagram entityDesignerDiagram, ShapeElement[] shapeElements)
        {
            var diagramItemCollection = new DiagramItemCollection();
            foreach (var shapeElement in shapeElements)
            {
                diagramItemCollection.Add(new DiagramItem(shapeElement));
            }

            if (entityDesignerDiagram.ActiveDiagramView != null)
            {
                entityDesignerDiagram.ActiveDiagramView.Focus();
                entityDesignerDiagram.ActiveDiagramView.Selection.Set(diagramItemCollection);
                entityDesignerDiagram.EnsureSelectionVisible();
            }
            else
            {
                // if no active diagram view is available, set the selection in any client view.
                if (entityDesignerDiagram.ClientViews != null
                    && entityDesignerDiagram.ClientViews.Count > 0)
                {
                    foreach (DiagramClientView clientView in entityDesignerDiagram.ClientViews)
                    {
                        clientView.Selection.Set(diagramItemCollection);
                        clientView.Selection.EnsureVisible(DiagramClientView.EnsureVisiblePreferences.ScrollIntoViewCenter);
                        break;
                    }
                }
                else
                {
                    throw new InvalidOperationException("There is no active client views in the diagram.");
                }
            }
        }
    }
}
