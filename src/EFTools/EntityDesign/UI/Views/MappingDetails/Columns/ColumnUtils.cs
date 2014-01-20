// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Columns
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails;

    internal class ColumnUtils
    {
        internal static string BuildPropertyDisplay(string name, string type)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            var propdisplay = string.Empty;

            propdisplay = string.Format(
                CultureInfo.CurrentCulture, "{0} : {1}",
                name,
                type);

            return propdisplay;
        }

        // <summary>
        //     Add this property to the list-of-values (lov) parameter, but if the Property is a
        //     ComplexProperty then expand down to its constituent ScalarProperties
        // </summary>
        // <param name="lov">list-of-values to be appended to</param>
        // <param name="property">property to be added</param>
        // <param name="ancestorComplexProperties">any ancestor complex properties (should initially be null or empty)</param>
        internal static void AddPropertyToListOfValues(
            Dictionary<MappingLovEFElement, string> lov, Property property, List<Property> ancestorComplexProperties)
        {
            if (ancestorComplexProperties == null)
            {
                ancestorComplexProperties = new List<Property>();
            }

            ancestorComplexProperties.Add(property); // at this point it has actually become "ancestors and self"
            var complexProperty = property as ComplexConceptualProperty;
            if (complexProperty != null)
            {
                if (complexProperty.ComplexType.Status == BindingStatus.Known)
                {
                    foreach (var prop in complexProperty.ComplexType.Target.Properties())
                    {
                        AddPropertyToListOfValues(lov, prop, new List<Property>(ancestorComplexProperties));
                    }
                }
            }
            else
            {
                var sb = new StringBuilder();
                foreach (var prop in ancestorComplexProperties)
                {
                    sb.Append(prop.LocalName.Value);
                    sb.Append(".");
                }
                // in order to know which property should be mapped we need to store list of all ancestor properties that will need to be mapped in the middle
                var displayName = BuildPropertyDisplay(sb.ToString(0, sb.Length - 1), property.TypeName);
                lov.Add(new MappingLovEFElement(ancestorComplexProperties, displayName), displayName);
            }
        }
    }
}
