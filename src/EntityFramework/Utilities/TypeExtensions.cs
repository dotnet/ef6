namespace System.Data.Entity.Utilities
{
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Edm;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static class TypeExtensions
    {
        private static readonly Dictionary<Type, EdmPrimitiveType> PrimitiveTypesMap
            = new Dictionary<Type, EdmPrimitiveType>();

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static TypeExtensions()
        {
            foreach (var efPrimitiveType in PrimitiveType.GetEdmPrimitiveTypes())
            {
                EdmPrimitiveType primitiveType;
                if (EdmPrimitiveType.TryGetByName(efPrimitiveType.Name, out primitiveType))
                {
                    Contract.Assert(primitiveType != null);

                    PrimitiveTypesMap.Add(efPrimitiveType.ClrEquivalentType, primitiveType);
                }
            }
        }

        public static bool IsCollection(this Type type)
        {
            Contract.Requires(type != null);

            return type.IsCollection(out type);
        }

        public static bool IsCollection(this Type type, out Type elementType)
        {
            Contract.Requires(type != null);
            Contract.Assert(!type.IsGenericTypeDefinition);

            elementType = type;

            var collectionInterface
                = type.GetInterfaces()
                    .Union(new[] { type })
                    .FirstOrDefault(
                        t => t.IsGenericType
                             && t.GetGenericTypeDefinition() == typeof(ICollection<>));

            if (!type.IsArray
                && collectionInterface != null)
            {
                elementType = collectionInterface.GetGenericArguments().Single();

                return true;
            }

            return false;
        }

        public static Type GetTargetType(this Type type)
        {
            Contract.Requires(type != null);

            Type elementType;
            if (!type.IsCollection(out elementType))
            {
                elementType = type;
            }

            return elementType;
        }

        public static bool TryUnwrapNullableType(this Type type, out Type underlyingType)
        {
            Contract.Requires(type != null);
            Contract.Assert(!type.IsGenericTypeDefinition);

            underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType != type;
        }

        /// <summary>
        ///     Returns true if a variable of this type can be assigned a null value
        /// </summary>
        /// <param name = "type"></param>
        /// <returns>
        ///     True if a reference type or a nullable value type,
        ///     false otherwise
        /// </returns>
        public static bool IsNullable(this Type type)
        {
            Contract.Requires(type != null);

            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        public static bool IsValidStructuralType(this Type type)
        {
            Contract.Requires(type != null);

            return !(type.IsGenericType
                     || type.IsValueType
                     || type.IsPrimitive
                     || type.IsInterface
                     || type.IsArray
                     || type == typeof(string))
                   && type.IsValidStructuralPropertyType();
        }

        public static bool IsValidStructuralPropertyType(this Type type)
        {
            Contract.Requires(type != null);

            return !(type.IsGenericTypeDefinition
                     || type.IsNested
                     || type.IsPointer
                     || type == typeof(object)
                     || typeof(ComplexObject).IsAssignableFrom(type)
                     || typeof(EntityObject).IsAssignableFrom(type)
                     || typeof(StructuralObject).IsAssignableFrom(type)
                     || typeof(EntityKey).IsAssignableFrom(type)
                     || typeof(EntityReference).IsAssignableFrom(type));
        }

        public static bool IsPrimitiveType(this Type type, out EdmPrimitiveType primitiveType)
        {
            return PrimitiveTypesMap.TryGetValue(type, out primitiveType);
        }
    }
}
