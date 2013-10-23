// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors;

    internal class FromRoleListConverter : DynamicListConverter<AssociationEnd, ObjectDescriptor>
    {
        protected override void PopulateMappingForSelectedObject(ObjectDescriptor selectedObject)
        {
            Debug.Assert(selectedObject != null, "selectedObject should not be null");

            if (selectedObject != null)
            {
                var navigationProperty = selectedObject.WrappedItem as NavigationProperty;
                if (navigationProperty != null
                    && navigationProperty.FromRole.Status == BindingStatus.Known)
                {
                    AddMapping(navigationProperty.FromRole.Target, navigationProperty.FromRole.RefName);
                }
                else if (navigationProperty.FromRole.Target == null
                         && !string.IsNullOrEmpty(navigationProperty.FromRole.RefName))
                {
                    AddMapping(null, navigationProperty.FromRole.RefName);
                }
            }
        }
    }
}
