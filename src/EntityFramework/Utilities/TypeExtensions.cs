// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    internal static class TypeExtensions
    {
        private static readonly Dictionary<Type, PrimitiveType> _primitiveTypesMap
            = new Dictionary<Type, PrimitiveType>();

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static TypeExtensions()
        {
            foreach (var primitiveType in PrimitiveType.GetEdmPrimitiveTypes())
            {
                if (!_primitiveTypesMap.ContainsKey(primitiveType.ClrEquivalentType))
                {
                    _primitiveTypesMap.Add(primitiveType.ClrEquivalentType, primitiveType);
                }
            }
        }

        public static bool IsCollection(this Type type)
        {
            DebugCheck.NotNull(type);

            return type.IsCollection(out type);
        }

        public static bool IsCollection(this Type type, out Type elementType)
        {
            DebugCheck.NotNull(type);
            Debug.Assert(!type.IsGenericTypeDefinition);

            elementType = TryGetElementType(type, typeof(ICollection<>));

            if (elementType == null
                || type.IsArray)
            {
                elementType = type;
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Determine if the given type type implements the given generic interface or derives from the given generic type,
        ///     and if so return the element type of the collection. If the type implements the generic interface several times
        ///     <c>null</c> will be returned.
        /// </summary>
        /// <param name="type"> The type to examine. </param>
        /// <param name="interfaceOrBaseType"> The generic type to be queried for. </param>
        /// <returns> 
        ///     <c>null</c> if <paramref name="interfaceOrBaseType"/> isn't implemented or implemented multiple times,
        ///     otherwise the generic argument.
        /// </returns>
        public static Type TryGetElementType(this Type type, Type interfaceOrBaseType)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(interfaceOrBaseType);
            Debug.Assert(interfaceOrBaseType.GetGenericArguments().Count() == 1);

            if (!type.IsGenericTypeDefinition)
            {
                var interfaceImpl = GetGenericTypeImplementations(type, interfaceOrBaseType).SingleOrDefault();

                if (interfaceImpl != null)
                {
                    return interfaceImpl.GetGenericArguments().Single();
                }
            }

            return null;
        }

        /// <summary>
        ///     Determine if the given type type implements the given generic interface or derives from the given generic type,
        ///     and if so return the concrete types implemented.
        /// </summary>
        /// <param name="type"> The type to examine. </param>
        /// <param name="interfaceOrBaseType"> The generic type to be queried for. </param>
        /// <returns> 
        ///     The generic types constructed from <paramref name="interfaceOrBaseType"/> and implemented by <paramref name="type"/>.
        /// </returns>
        public static IEnumerable<Type> GetGenericTypeImplementations(this Type type, Type interfaceOrBaseType)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(interfaceOrBaseType);

            if (!type.IsGenericTypeDefinition)
            {
                return (interfaceOrBaseType.IsInterface ? type.GetInterfaces() : type.GetBaseTypes())
                    .Union(new[] { type })
                    .Where(
                        t => t.IsGenericType
                             && t.GetGenericTypeDefinition() == interfaceOrBaseType);
            }

            return Enumerable.Empty<Type>();
        }

        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            DebugCheck.NotNull(type);

            type = type.BaseType;

            while (type != null)
            {
                yield return type;

                type = type.BaseType;
            }
        }

        public static Type GetTargetType(this Type type)
        {
            DebugCheck.NotNull(type);

            Type elementType;
            if (!type.IsCollection(out elementType))
            {
                elementType = type;
            }

            return elementType;
        }

        public static bool TryUnwrapNullableType(this Type type, out Type underlyingType)
        {
            DebugCheck.NotNull(type);
            Debug.Assert(!type.IsGenericTypeDefinition);

            underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType != type;
        }

        /// <summary>
        ///     Returns true if a variable of this type can be assigned a null value
        /// </summary>
        /// <returns> True if a reference type or a nullable value type, false otherwise </returns>
        public static bool IsNullable(this Type type)
        {
            DebugCheck.NotNull(type);

            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        public static bool IsValidStructuralType(this Type type)
        {
            DebugCheck.NotNull(type);

            return !(type.IsGenericType
                     || type.IsValueType
                     || type.IsPrimitive
                     || type.IsInterface
                     || type.IsArray
                     || type == typeof(string)
                     || type == typeof(DbGeography)
                     || type == typeof(DbGeometry))
                   && type.IsValidStructuralPropertyType();
        }

        public static bool IsValidStructuralPropertyType(this Type type)
        {
            DebugCheck.NotNull(type);

            return !(type.IsGenericTypeDefinition
                     || type.IsPointer
                     || type == typeof(object)
                     || typeof(ComplexObject).IsAssignableFrom(type)
                     || typeof(EntityObject).IsAssignableFrom(type)
                     || typeof(StructuralObject).IsAssignableFrom(type)
                     || typeof(EntityKey).IsAssignableFrom(type)
                     || typeof(EntityReference).IsAssignableFrom(type));
        }

        public static bool IsPrimitiveType(this Type type, out PrimitiveType primitiveType)
        {
            return _primitiveTypesMap.TryGetValue(type, out primitiveType);
        }

        public static T CreateInstance<T>(
            this Type type,
            Func<string, string, string> typeMessageFactory,
            Func<string, Exception> exceptionFactory = null)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(typeMessageFactory);

            exceptionFactory = exceptionFactory ?? (s => new InvalidOperationException(s));

            if (!typeof(T).IsAssignableFrom(type))
            {
                throw exceptionFactory(typeMessageFactory(type.ToString(), typeof(T).ToString()));
            }

            return CreateInstance<T>(type, exceptionFactory);
        }

        public static T CreateInstance<T>(this Type type, Func<string, Exception> exceptionFactory = null)
        {
            DebugCheck.NotNull(type);
            Debug.Assert(typeof(T).IsAssignableFrom(type));

            exceptionFactory = exceptionFactory ?? (s => new InvalidOperationException(s));

            if (type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, Type.EmptyTypes, null) == null)
            {
                throw exceptionFactory(Strings.CreateInstance_NoParameterlessConstructor(type));
            }

            if (type.IsAbstract)
            {
                throw exceptionFactory(Strings.CreateInstance_AbstractType(type));
            }

            if (type.IsGenericType)
            {
                throw exceptionFactory(Strings.CreateInstance_GenericType(type));
            }

            return (T)Activator.CreateInstance(type, nonPublic: true);
        }

        public static bool IsValidEdmScalarType(this Type type)
        {
            DebugCheck.NotNull(type);

            type.TryUnwrapNullableType(out type);

            PrimitiveType _;
            return type.IsPrimitiveType(out _) || type.IsEnum;
        }

        public static string NestingNamespace(this Type type)
        {
            DebugCheck.NotNull(type);

            if (!type.IsNested)
            {
                return type.Namespace;
            }

            var fullName = type.FullName;

            return fullName.Substring(0, fullName.Length - type.Name.Length - 1).Replace('+', '.');
        }

        public static string FullNameWithNesting(this Type type)
        {
            DebugCheck.NotNull(type);

            if (!type.IsNested)
            {
                return type.FullName;
            }

            return type.FullName.Replace('+', '.');
        }


        public static bool OverridesEqualsOrGetHashCode(this Type type)
        {
            DebugCheck.NotNull(type);

            while (type != typeof(object))
            {
                if (type.GetMethods()
                    .Any(m => (m.Name == "Equals" || m.Name == "GetHashCode")
                        && m.DeclaringType != typeof(object)
                        && m.GetBaseDefinition().DeclaringType == typeof(object)))
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}
