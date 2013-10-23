// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors;

    internal class ConceptualPropertyTypeConverter : StringConverter
    {
        private HashSet<string> _typeList;

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            PopulateTypeList(context);
            return new StandardValuesCollection(_typeList.ToList());
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            PopulateTypeList(context);
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            PopulateTypeList(context);
            return base.ConvertTo(context, culture, value, destinationType);
        }

        private void PopulateTypeList(ITypeDescriptorContext context)
        {
            _typeList = new HashSet<string>();

            if (context != null)
            {
                var propertyDescriptor = context.Instance as EFPropertyDescriptor;
                if (propertyDescriptor != null
                    && propertyDescriptor.TypedEFElement != null)
                {
                    var artifact = propertyDescriptor.TypedEFElement.Artifact;
                    Debug.Assert(artifact != null, "Unable to find artifact.");
                    if (artifact != null)
                    {
                        foreach (var primType in ModelHelper.AllPrimitiveTypesSorted(artifact.SchemaVersion))
                        {
                            _typeList.Add(primType);
                        }
                    }

                    var conceptualModel =
                        (ConceptualEntityModel)propertyDescriptor.TypedEFElement.GetParentOfType(typeof(ConceptualEntityModel));
                    Debug.Assert(conceptualModel != null, "Unable to find conceptual model.");
                    if (conceptualModel != null)
                    {
                        foreach (var enumType in conceptualModel.EnumTypes())
                        {
                            _typeList.Add(enumType.NormalizedNameExternal);
                        }
                    }
                }
            }
        }
    }
}
