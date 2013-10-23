// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal abstract class EndMultiplicityConverter : DynamicListConverter<string, ObjectDescriptor>
    {
        protected override void PopulateMappingForSelectedObject(ObjectDescriptor selectedObject)
        {
            Debug.Assert(selectedObject != null, "selectedObject should not be null");

            if (selectedObject != null)
            {
                var association = selectedObject.WrappedItem as Association;
                if (association != null)
                {
                    var end = GetEnd(association);
                    if (end != null)
                    {
                        var typeName = String.Empty;
                        if (end.Type.Target != null)
                        {
                            typeName = end.Type.Target.LocalName.Value;
                        }
                        AddMapping(
                            ModelConstants.Multiplicity_Many,
                            String.Format(CultureInfo.CurrentCulture, Resources.PropertyWindow_Value_MultiplicityManyOf, typeName));
                        AddMapping(
                            ModelConstants.Multiplicity_One,
                            String.Format(CultureInfo.CurrentCulture, Resources.PropertyWindow_Value_MultiplicityOneOf, typeName));
                        AddMapping(
                            ModelConstants.Multiplicity_ZeroOrOne,
                            String.Format(CultureInfo.CurrentCulture, Resources.PropertyWindow_Value_MultiplicityZeroOrOneOf, typeName));
                    }
                }
            }
        }

        protected abstract AssociationEnd GetEnd(Association association);
    }

    internal class End1MultiplicityConverter : EndMultiplicityConverter
    {
        /// <summary>
        ///     Returns the first End of the given Association
        /// </summary>
        protected override AssociationEnd GetEnd(Association association)
        {
            if (association.AssociationEnds().Count > 0)
            {
                return association.AssociationEnds()[0];
            }
            return null;
        }
    }

    internal class End2MultiplicityConverter : EndMultiplicityConverter
    {
        /// <summary>
        ///     Returns the second End of the given Association
        /// </summary>
        protected override AssociationEnd GetEnd(Association association)
        {
            if (association.AssociationEnds().Count > 1)
            {
                return association.AssociationEnds()[1];
            }
            return null;
        }
    }
}
