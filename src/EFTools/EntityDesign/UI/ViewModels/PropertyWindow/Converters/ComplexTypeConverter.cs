// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors;

    internal class ComplexTypeConverter : DynamicListConverter<ComplexType, ObjectDescriptor>
    {
        protected override void PopulateMappingForSelectedObject(ObjectDescriptor selectedObject)
        {
            Debug.Assert(selectedObject != null, "selectedObject should not be null");

            if (selectedObject != null)
            {
                var property = selectedObject.WrappedItem as ComplexConceptualProperty;
                if (property != null)
                {
                    var model = property.EntityModel as ConceptualEntityModel;
                    Debug.Assert(model != null, "Unexpected model type");
                    if (model != null)
                    {
                        // Now get all complex types to be displayed
                        foreach (var type in model.ComplexTypes())
                        {
                            AddMapping(type, type.LocalName.Value);
                        }
                    }

                    // display value for unresolved Complex Type reference
                    _displayValueForNull = property.TypeName;
                }
            }
        }
    }
}
