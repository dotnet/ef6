// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using Microsoft.Data.Entity.Design.Core.Controls;

    /// <summary>
    ///     type converter for enums, with support for a Description attribute on the enum values
    ///     for localization support.
    /// </summary>
    internal class EnumConverter<TEnum> : FixedListConverter<TEnum>
    {
        protected override void PopulateMapping()
        {
            var type = typeof(TEnum);
            Debug.Assert(type.IsEnum);
            if (type.IsEnum)
            {
                var enumValues = type.GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (var enumValue in enumValues)
                {
                    DescriptionAttribute descriptionAttr = null;
                    foreach (Attribute attr in enumValue.GetCustomAttributes(typeof(DescriptionAttribute), false))
                    {
                        descriptionAttr = attr as DescriptionAttribute;
                        break;
                    }
                    var value = (TEnum)Enum.Parse(type, enumValue.Name);
                    var displayValue = (descriptionAttr != null) ? descriptionAttr.Description : value.ToString();
                    AddMapping(value, displayValue);
                }
            }
        }
    }
}
