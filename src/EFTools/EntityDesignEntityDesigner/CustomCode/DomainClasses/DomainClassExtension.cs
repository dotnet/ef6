// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.ViewModel
{
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling.Diagrams;

    internal static class DomainClassExtension
    {
        /// <summary>
        ///     Return DSL root view model for the given model element.
        ///     In multiple diagram world, each diagram will have a root view model.
        /// </summary>
        /// <param name="modelElement"></param>
        /// <returns></returns>
        internal static EntityDesignerViewModel GetRootViewModel(this ModelElement modelElement)
        {
            // if model element is a shape element, we should be able to find root view model from the shape element's diagram.
            var shapeElement = modelElement as ShapeElement;
            if (shapeElement != null)
            {
                Debug.Assert(
                    shapeElement.Diagram != null,
                    "ShapeElement's Diagram should never be null. Element name: " + shapeElement.AccessibleName + ", type: "
                    + shapeElement.GetType().Name);

                if (shapeElement.Diagram != null)
                {
                    return shapeElement.Diagram.ModelElement as EntityDesignerViewModel;
                }
            }
            else
            {
                Debug.Assert(modelElement.Partition != null, "ModelElement's Partition should never be null.");
                if (modelElement.Partition != null)
                {
                    Debug.Assert(modelElement.Partition.ElementDirectory != null, "Why ModelElement Partition's ElementDirectory is null?");
                    if (modelElement.Partition.ElementDirectory != null)
                    {
                        return modelElement.Partition.ElementDirectory.FindElements<EntityDesignerViewModel>().FirstOrDefault();
                    }
                }
            }
            return null;
        }
    }
}
