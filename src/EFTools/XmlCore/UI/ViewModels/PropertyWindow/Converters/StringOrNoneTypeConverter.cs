// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Tools.XmlDesignerBase;

    internal class StringOrNoneTypeConverter : StringConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            // this method returns the text displayed in drop-down
            if (destinationType == typeof(string))
            {
                var v = value as StringOrNone;
                if (v != null)
                {
                    if (StringOrNone.NoneValue.Equals(v))
                    {
                        return Resources.NoneDisplayValueUsedForUX;
                    }
                    else
                    {
                        return v.ToString();
                    }
                }
            }
            return null;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            // this method converts the text displayed in the Property Grid textbox to a StringOrNone object
            // Note: does _not_ apply if user comes direct from NoneOptionListBoxTypeEditor which means
            // we can tell the difference between a text entry "(None)" and the TypeEditor '(None)'
            // by always returning a non-NoneValue StringOrNone here
            var stringValue = value as string;
            if (stringValue != null)
            {
                return new StringOrNone(stringValue);
            }
            return null;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            StringOrNone[] standardValues = { StringOrNone.NoneValue };
            return new StandardValuesCollection(standardValues);
        }
    }
}
