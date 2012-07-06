namespace System.Data.Entity.Utilities
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    internal static class PropertyInfoExtensions
    {
        public static bool IsSameAs(this PropertyInfo propertyInfo, PropertyInfo otherPropertyInfo)
        {
            Contract.Requires(propertyInfo != null);
            Contract.Requires(otherPropertyInfo != null);

            return (propertyInfo == otherPropertyInfo) ||
                   (propertyInfo.Name == otherPropertyInfo.Name
                    && (propertyInfo.DeclaringType == otherPropertyInfo.DeclaringType
                        || propertyInfo.DeclaringType.IsSubclassOf(otherPropertyInfo.DeclaringType)
                        || otherPropertyInfo.DeclaringType.IsSubclassOf(propertyInfo.DeclaringType)));
        }

        public static bool ContainsSame(this IEnumerable<PropertyInfo> enumerable, PropertyInfo propertyInfo)
        {
            Contract.Requires(enumerable != null);
            Contract.Requires(propertyInfo != null);

            foreach (var member in enumerable)
            {
                if (propertyInfo.IsSameAs(member))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsValidStructuralProperty(this PropertyInfo propertyInfo)
        {
            Contract.Requires(propertyInfo != null);

            return propertyInfo.CanRead
                   && (propertyInfo.CanWrite || propertyInfo.PropertyType.IsCollection())
                   && !propertyInfo.GetGetMethod(true).IsAbstract
                   && propertyInfo.PropertyType.IsValidStructuralPropertyType();
        }

        public static bool IsValidEdmScalarProperty(this PropertyInfo propertyInfo)
        {
            Contract.Requires(propertyInfo != null);

            var propertyType = propertyInfo.PropertyType;

            propertyType.TryUnwrapNullableType(out propertyType);

            EdmPrimitiveType _;
            return propertyType.IsPrimitiveType(out _) || propertyType.IsEnum;
        }

        public static EdmProperty AsEdmPrimitiveProperty(this PropertyInfo propertyInfo)
        {
            Contract.Requires(propertyInfo != null);

            var propertyType = propertyInfo.PropertyType;
            var isNullable = propertyType.TryUnwrapNullableType(out propertyType) || !propertyType.IsValueType;

            EdmPrimitiveType primitiveType;
            if (propertyType.IsPrimitiveType(out primitiveType))
            {
                var property = new EdmProperty
                    {
                        Name = propertyInfo.Name
                    }.AsPrimitive();

                property.PropertyType.EdmType = primitiveType;
                property.PropertyType.IsNullable = isNullable;

                return property;
            }

            return null;
        }
    }
}
