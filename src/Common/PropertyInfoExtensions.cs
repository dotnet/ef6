// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if EF_FUNCTIONALS
namespace System.Data.Entity.Functionals.Utilities
#else
namespace System.Data.Entity.Utilities
#endif
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Mappers;
    using System.Diagnostics;
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

            return propertyInfo.IsValidInterfaceStructuralProperty()
                   && !propertyInfo.GetGetMethod(true).IsAbstract;
        }

        public static bool IsValidInterfaceStructuralProperty(this PropertyInfo propertyInfo)
        {
            DebugCheck.NotNull(propertyInfo);

            return propertyInfo.CanRead
                   && (propertyInfo.CanWriteExtended() || propertyInfo.PropertyType.IsCollection())
                   && propertyInfo.GetIndexParameters().Length == 0
                   && propertyInfo.PropertyType.IsValidStructuralPropertyType();
        }

        public static bool IsValidEdmScalarProperty(this PropertyInfo propertyInfo)
        {
            DebugCheck.NotNull(propertyInfo);

            return IsValidInterfaceStructuralProperty(propertyInfo)
                   && propertyInfo.PropertyType.IsValidEdmScalarType();
        }

        public static bool IsValidEdmNavigationProperty(this PropertyInfo propertyInfo)
        {
            DebugCheck.NotNull(propertyInfo);

            Type elementType;
            return IsValidInterfaceStructuralProperty(propertyInfo)
                   && ((propertyInfo.PropertyType.IsCollection(out elementType) && elementType.IsValidStructuralType())
                       || propertyInfo.PropertyType.IsValidStructuralType());
        }

        public static EdmProperty AsEdmPrimitiveProperty(this PropertyInfo propertyInfo)
        {
            DebugCheck.NotNull(propertyInfo);

            var propertyType = propertyInfo.PropertyType;
            var isNullable = propertyType.TryUnwrapNullableType(out propertyType) || !propertyType.IsValueType;

            PrimitiveType primitiveType;
            if (propertyType.IsPrimitiveType(out primitiveType))
            {
                var property = EdmProperty.CreatePrimitive(propertyInfo.Name, primitiveType);

                property.Nullable = isNullable;

                return property;
            }

            return null;
        }

        public static bool CanWriteExtended(this PropertyInfo propertyInfo)
        {
            DebugCheck.NotNull(propertyInfo);

            if (propertyInfo.CanWrite)
            {
                return true;
            }

            var declaredProperty = GetDeclaredProperty(propertyInfo);
            return declaredProperty != null && declaredProperty.CanWrite;
        }

        public static PropertyInfo GetPropertyInfoForSet(this PropertyInfo propertyInfo)
        {
            DebugCheck.NotNull(propertyInfo);

            return propertyInfo.CanWrite ? propertyInfo : GetDeclaredProperty(propertyInfo) ?? propertyInfo;
        }

        private static PropertyInfo GetDeclaredProperty(PropertyInfo propertyInfo)
        {
            Debug.Assert(propertyInfo.DeclaringType != null);

            const BindingFlags defaultBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            return propertyInfo.DeclaringType == propertyInfo.ReflectedType
                       ? propertyInfo
                       : propertyInfo
                             .DeclaringType
                             .GetProperties(defaultBindingFlags)
                             .SingleOrDefault(
                                 p => p.Name == propertyInfo.Name
                                      && !p.GetIndexParameters().Any()
                                      && p.PropertyType == propertyInfo.PropertyType);
        }

        public static IEnumerable<PropertyInfo> GetPropertiesInHierarchy(this PropertyInfo property)
        {
            DebugCheck.NotNull(property);

            var collection = new List<PropertyInfo> { property };
            CollectProperties(property, collection);
            return collection.Distinct();
        }

        private static void CollectProperties(PropertyInfo property, IList<PropertyInfo> collection)
        {
            DebugCheck.NotNull(property);
            DebugCheck.NotNull(collection);

            FindNextProperty(property, collection, getter: true);
            FindNextProperty(property, collection, getter: false);
        }

        private static void FindNextProperty(PropertyInfo property, IList<PropertyInfo> collection, bool getter)
        {
            DebugCheck.NotNull(property);
            DebugCheck.NotNull(collection);

            var method = getter ? property.GetGetMethod(nonPublic: true) : property.GetSetMethod(nonPublic: true);

            if (method != null)
            {
                var nextType = method.DeclaringType.BaseType;
                if (nextType != null && nextType != typeof(object))
                {
                    var baseMethod = method.GetBaseDefinition();
                    const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

                    var nextProperty =
                        (from p in nextType.GetProperties(bindingFlags)
                         let candidateMethod = getter ? p.GetGetMethod(nonPublic: true) : p.GetSetMethod(nonPublic: true)
                         where candidateMethod != null && candidateMethod.GetBaseDefinition() == baseMethod
                         select p).FirstOrDefault();

                    if (nextProperty != null)
                    {
                        collection.Add(nextProperty);
                        CollectProperties(nextProperty, collection);
                    }
                }
            }
        }
    }
}
