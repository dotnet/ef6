// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using System.Reflection;

    internal static class PropertyInfoExtensions
    {
        public static bool IsSameAs(this PropertyInfo propertyInfo, PropertyInfo otherPropertyInfo)
        {
            DebugCheck.NotNull(propertyInfo);
            DebugCheck.NotNull(otherPropertyInfo);

            return (propertyInfo == otherPropertyInfo) ||
                   (propertyInfo.Name == otherPropertyInfo.Name
                    && (propertyInfo.DeclaringType == otherPropertyInfo.DeclaringType
                        || propertyInfo.DeclaringType.IsSubclassOf(otherPropertyInfo.DeclaringType)
                        || otherPropertyInfo.DeclaringType.IsSubclassOf(propertyInfo.DeclaringType)
                        || propertyInfo.DeclaringType.GetInterfaces().Contains(otherPropertyInfo.DeclaringType)
                        || otherPropertyInfo.DeclaringType.GetInterfaces().Contains(propertyInfo.DeclaringType)));
        }

        public static bool ContainsSame(this IEnumerable<PropertyInfo> enumerable, PropertyInfo propertyInfo)
        {
            DebugCheck.NotNull(enumerable);
            DebugCheck.NotNull(propertyInfo);

            return enumerable.Any(propertyInfo.IsSameAs);
        }

        public static bool IsValidStructuralProperty(this PropertyInfo propertyInfo)
        {
            DebugCheck.NotNull(propertyInfo);

            return propertyInfo.CanRead
                   && (propertyInfo.CanWrite || propertyInfo.PropertyType.IsCollection())
                   && !propertyInfo.GetGetMethod(true).IsAbstract
                   && propertyInfo.GetIndexParameters().Length == 0
                   && propertyInfo.PropertyType.IsValidStructuralPropertyType();
        }

        public static bool IsValidEdmScalarProperty(this PropertyInfo propertyInfo)
        {
            DebugCheck.NotNull(propertyInfo);

            return propertyInfo.PropertyType.IsValidEdmScalarType();
        }

        public static EdmProperty AsEdmPrimitiveProperty(this PropertyInfo propertyInfo)
        {
            DebugCheck.NotNull(propertyInfo);

            var propertyType = propertyInfo.PropertyType;
            var isNullable = propertyType.TryUnwrapNullableType(out propertyType) || !propertyType.IsValueType;

            PrimitiveType primitiveType;
            if (propertyType.IsPrimitiveType(out primitiveType))
            {
                var property = EdmProperty.Primitive(propertyInfo.Name, primitiveType);

                property.Nullable = isNullable;

                return property;
            }

            return null;
        }
    }
}
