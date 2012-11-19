// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

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

        public static Type TryGetElementType(this Type type, Type interfaceType)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(interfaceType);

            if (!type.IsGenericTypeDefinition)
            {
                var interfaceImpl = type.GetInterfaces()
                                        .Union(new[] { type })
                                        .FirstOrDefault(
                                            t => t.IsGenericType
                                                 && t.GetGenericTypeDefinition() == interfaceType);

                if (interfaceImpl != null)
                {
                    return interfaceImpl.GetGenericArguments().Single();
                }
            }

            return null;
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
        /// <param name="type"> </param>
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
                     || type == typeof(string))
                   && type.IsValidStructuralPropertyType();
        }

        public static bool IsValidStructuralPropertyType(this Type type)
        {
            DebugCheck.NotNull(type);

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

            if (type.GetConstructor(Type.EmptyTypes) == null)
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

            return (T)Activator.CreateInstance(type);
        }
    }
}
