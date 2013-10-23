// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors;
    using Microsoft.Data.Tools.XmlDesignerBase;

    internal class AssociationListConverter : DynamicListConverter<Association, ObjectDescriptor>
    {
        protected override void PopulateMappingForSelectedObject(ObjectDescriptor selectedObject)
        {
            Debug.Assert(selectedObject != null, "selectedObject should not be null");

            if (selectedObject != null)
            {
                // Add an entry for (None) with null value
                AddMapping(null, Resources.NoneDisplayValueUsedForUX);

                var property = selectedObject.WrappedItem as NavigationProperty;
                if (property != null
                    && property.Parent != null)
                {
                    foreach (var associationEnd in property.Parent.GetAntiDependenciesOfType<AssociationEnd>())
                    {
                        var association = associationEnd.Parent as Association;
                        if (association != null
                            && !ContainsMapping(association.DisplayName))
                        {
                            AddMapping(association, association.DisplayName);
                        }
                    }
                }
            }
        }
    }
}
