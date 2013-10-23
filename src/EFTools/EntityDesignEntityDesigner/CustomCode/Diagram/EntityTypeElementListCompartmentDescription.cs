// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.View
{
    using Microsoft.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling.Diagrams;

    internal class EntityTypeElementListCompartmentDescription : ElementListCompartmentDescription
    {
        public EntityTypeElementListCompartmentDescription(
            ListCompartmentDescription wrappedElementListCompartmentDescription, bool isDefaultCollapsed)
            : base(wrappedElementListCompartmentDescription.Name, wrappedElementListCompartmentDescription.Title,
                wrappedElementListCompartmentDescription.TitleFillColor,
                wrappedElementListCompartmentDescription.AllowCustomTitleFillColor,
                wrappedElementListCompartmentDescription.CompartmentFillColor,
                wrappedElementListCompartmentDescription.AllowCustomCompartmentFillColor,
                wrappedElementListCompartmentDescription.TitleFontSettings, wrappedElementListCompartmentDescription.ItemFontSettings,
                isDefaultCollapsed)
        {
        }

        /// <summary>
        ///     Instantiate EntityTypeElementListCompartment so we can control FontSetting style for the list item.
        /// </summary>
        /// <returns></returns>
        public override Compartment CreateCompartment(Partition partition)
        {
            var compartment = new EntityTypeElementListCompartment(partition);
            if (IsDefaultCollapsed)
            {
                compartment.IsExpanded = false;
            }
            return compartment;
        }
    }
}
