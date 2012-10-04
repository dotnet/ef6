// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    internal static class EdmStructuralMemberExtensions
    {
        public static PropertyInfo GetClrPropertyInfo(this EdmMember property)
        {
            Contract.Requires(property != null);

            return property.Annotations.GetClrPropertyInfo();
        }

        public static void SetClrPropertyInfo(this EdmMember property, PropertyInfo propertyInfo)
        {
            Contract.Requires(property != null);

            property.Annotations.SetClrPropertyInfo(propertyInfo);
        }

        public static IEnumerable<T> GetClrAttributes<T>(this EdmMember property) where T : Attribute
        {
            Contract.Requires(property != null);

            var clrAttributes = property.Annotations.GetClrAttributes();
            return clrAttributes != null ? clrAttributes.OfType<T>() : Enumerable.Empty<T>();
        }
    }
}
