// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;

    internal static class EdmMemberExtensions
    {
        public static PropertyInfo GetClrPropertyInfo(this EdmMember property)
        {
            DebugCheck.NotNull(property);

            return property.Annotations.GetClrPropertyInfo();
        }

        public static void SetClrPropertyInfo(this EdmMember property, PropertyInfo propertyInfo)
        {
            DebugCheck.NotNull(property);

            property.Annotations.SetClrPropertyInfo(propertyInfo);
        }

        public static IEnumerable<T> GetClrAttributes<T>(this EdmMember property) where T : Attribute
        {
            DebugCheck.NotNull(property);

            var clrAttributes = property.Annotations.GetClrAttributes();
            return clrAttributes != null ? clrAttributes.OfType<T>() : Enumerable.Empty<T>();
        }
    }
}
