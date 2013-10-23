// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors;

    internal class EFNameableItemListConverter : DynamicListConverter<EntitySet, EFEntityTypeDescriptor>
    {
        protected override void PopulateMappingForSelectedObject(EFEntityTypeDescriptor selectedObject)
        {
            Debug.Assert(selectedObject != null, "selectedObject should not be null");

            if (selectedObject != null)
            {
                var currentType = selectedObject.TypedEFElement;

                Debug.Assert(currentType != null, "currentType should not be null for selectedObject " + selectedObject);
                if (currentType != null)
                {
                    var entityContainer = currentType.EntityModel.EntityContainer;
                    Debug.Assert(entityContainer != null, "BaseEntityContainer should not be null for selectedObject " + selectedObject);
                    if (entityContainer != null)
                    {
                        foreach (var es in entityContainer.EntitySets())
                        {
                            AddMapping(es, es.NormalizedNameExternal);
                        }
                    }
                }
            }
        }
    }
}
