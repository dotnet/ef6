// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if SQLSERVER
namespace System.Data.Entity.SqlServer.Utilities
#elif EF_FUNCTIONALS
namespace System.Data.Entity.Functionals.Utilities
#else
namespace System.Data.Entity.Utilities
#endif
{
    using System.Collections;
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
            Debug.Assert(!type.IsGenericTypeDefinition());

            elementType = TryGetElementType(type, typeof(ICollection<>));

            if (elementType == null
                || type.IsArray)
            {
                elementType = type;
                return false;
            }

            return true;
        }

        public static IEnumerable<PropertyInfo> GetNonIndexerProperties(this Type type)
        {
            DebugCheck.NotNull(type);

            return type.GetRuntimeProperties().Where(
                p => p.IsPublic()
                     && !p.GetIndexParameters().Any());
        }

        // <summary>
        // Determine if the given type type implements the given generic interface or derives from the given generic type,
        // and if so return the element type of the collection. If the type implements the generic interface several times
        // <c>null</c> will be returned.
        // </summary>
        // <param name="type"> The type to examine. </param>
        // <param name="interfaceOrBaseType"> The generic type to be queried for. </param>
        // <returns> 
        // <c>null</c> if <paramref name="interfaceOrBaseType"/> isn't implemented or implemented multiple times,
        // otherwise the generic argument.
        // </returns>
        public static Type TryGetElementType(this Type type, Type interfaceOrBaseType)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(interfaceOrBaseType);
            Debug.Assert(interfaceOrBaseType.GetGenericArguments().Count() == 1);

            if (!type.IsGenericTypeDefinition())
            {
                var interfaceImpl = GetGenericTypeImplementations(type, interfaceOrBaseType).SingleOrDefault();

                if (interfaceImpl != null)
                {
                    return interfaceImpl.GetGenericArguments().Single();
                }
            }

            return null;
        }

        // <summary>
        // Determine if the given type type implements the given generic interface or derives from the given generic type,
        // and if so return the concrete types implemented.
        // </summary>
        // <param name="type"> The type to examine. </param>
        // <param name="interfaceOrBaseType"> The generic type to be queried for. </param>
        // <returns> 
        // The generic types constructed from <paramref name="interfaceOrBaseType"/> and implemented by <paramref name="type"/>.
        // </returns>
        public static IEnumerable<Type> GetGenericTypeImplementations(this Type type, Type interfaceOrBaseType)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(interfaceOrBaseType);

            if (!type.IsGenericTypeDefinition())
            {
                return (interfaceOrBaseType.IsInterface() ? type.GetInterfaces() : type.GetBaseTypes())
                    .Union(new[] { type })
                    .Where(
                        t => t.IsGenericType()
                             && t.GetGenericTypeDefinition() == interfaceOrBaseType);
            }

            return Enumerable.Empty<Type>();
        }

        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            DebugCheck.NotNull(type);

            type = type.BaseType();

            while (type != null)
            {
                yield return type;

                type = type.BaseType();
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
            Debug.Assert(!type.IsGenericTypeDefinition());

            underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType != type;
        }

        // <summary>
        // Returns true if a variable of this type can be assigned a null value
        // </summary>
        // <returns> True if a reference type or a nullable value type, false otherwise </returns>
        public static bool IsNullable(this Type type)
        {
            DebugCheck.NotNull(type);

            return !type.IsValueType() || Nullable.GetUnderlyingType(type) != null;
        }

        public static bool IsValidStructuralType(this Type type)
        {
            DebugCheck.NotNull(type);

            return !(type.IsGenericType()
                     || type.IsValueType()
                     || type.IsPrimitive()
                     || type.IsInterface()
                     || type.IsArray
                     || type == typeof(string)
                     || type == typeof(DbGeography)
                     || type == typeof(DbGeometry))
                   && type.IsValidStructuralPropertyType();
        }

        public static bool IsValidStructuralPropertyType(this Type type)
        {
            DebugCheck.NotNull(type);

            return !(type.IsGenericTypeDefinition()
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

#if !SQLSERVER && !EF_FUNCTIONALS
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

            if (type.GetDeclaredConstructor() == null)
            {
                throw exceptionFactory(Strings.CreateInstance_NoParameterlessConstructor(type));
            }

            if (type.IsAbstract())
            {
                throw exceptionFactory(Strings.CreateInstance_AbstractType(type));
            }

            if (type.IsGenericType())
            {
                throw exceptionFactory(Strings.CreateInstance_GenericType(type));
            }

            return (T)Activator.CreateInstance(type, nonPublic: true);
        }
#endif

        public static bool IsValidEdmScalarType(this Type type)
        {
            DebugCheck.NotNull(type);

            type.TryUnwrapNullableType(out type);

            PrimitiveType _;
            return type.IsPrimitiveType(out _) || type.IsEnum();
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
                if (type.GetDeclaredMethods()
                    .Any(m => (m.Name == "Equals" || m.Name == "GetHashCode")
                        && m.DeclaringType != typeof(object)
                        && m.GetBaseDefinition().DeclaringType == typeof(object)))
                {
                    return true;
                }

                type = type.BaseType();
            }

            return false;
        }

        public static bool IsPublic(this Type type)
        {
#if NET40
            return type.IsPublic || (type.IsNestedPublic && type.DeclaringType.IsPublic());
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsPublic || (typeInfo.IsNestedPublic && type.DeclaringType.IsPublic());
#endif
        }

        public static bool IsNotPublic(this Type type)
        {
            return !type.IsPublic();
        }

        public static MethodInfo GetOnlyDeclaredMethod(this Type type, string name)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotEmpty(name);

            return type.GetDeclaredMethods(name).SingleOrDefault();
        }

        public static MethodInfo GetDeclaredMethod(this Type type, string name, params Type[] parameterTypes)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotEmpty(name);
            DebugCheck.NotNull(parameterTypes);

            return type.GetDeclaredMethods(name)
                .SingleOrDefault(m => m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes));
        }

        public static MethodInfo GetPublicInstanceMethod(this Type type, string name, params Type[] parameterTypes)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotEmpty(name);
            DebugCheck.NotNull(parameterTypes);

            return type.GetRuntimeMethod(name, m => m.IsPublic && !m.IsStatic, parameterTypes);
        }

        public static MethodInfo GetRuntimeMethod(
            this Type type, string name, Func<MethodInfo, bool> predicate, params Type[][] parameterTypes)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotEmpty(name);
            DebugCheck.NotNull(predicate);
            DebugCheck.NotNull(parameterTypes);

            return parameterTypes
                .Select(t => type.GetRuntimeMethod(name, predicate, t))
                .FirstOrDefault(m => m != null);
        }

        private static MethodInfo GetRuntimeMethod(
            this Type type, string name, Func<MethodInfo, bool> predicate, Type[] parameterTypes)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotEmpty(name);
            DebugCheck.NotNull(predicate);
            DebugCheck.NotNull(parameterTypes);

            var methods = type.GetRuntimeMethods().Where(
                m => name == m.Name
                     && predicate(m)
                     && m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes)).ToArray();

            if (methods.Length == 1)
            {
                return methods[0];
            }

            return methods.SingleOrDefault(
                m => !methods.Any(m2 => m2.DeclaringType.IsSubclassOf(m.DeclaringType)));
        }

#if NET40
        public static IEnumerable<MethodInfo> GetRuntimeMethods(this Type type)
        {
            DebugCheck.NotNull(type);

            const BindingFlags bindingFlags
                = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return type.GetMethods(bindingFlags);
        }
#endif

        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type)
        {
            DebugCheck.NotNull(type);
#if NET40
            const BindingFlags bindingFlags
                = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            return type.GetMethods(bindingFlags);
#else
            return type.GetTypeInfo().DeclaredMethods;
#endif
        }

        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type, string name)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotEmpty(name);
#if NET40
            const BindingFlags bindingFlags
                = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            return type.GetMember(name, MemberTypes.Method, bindingFlags).OfType<MethodInfo>();
#else
            return type.GetTypeInfo().GetDeclaredMethods(name);
#endif
        }

        public static PropertyInfo GetDeclaredProperty(this Type type, string name)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotEmpty(name);
#if NET40
            const BindingFlags bindingFlags
                = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            return type.GetProperty(name, bindingFlags);
#else
            return type.GetTypeInfo().GetDeclaredProperty(name);
#endif
        }

        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this Type type)
        {
            DebugCheck.NotNull(type);
#if NET40
            const BindingFlags bindingFlags
                = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            return type.GetProperties(bindingFlags);
#else
            return type.GetTypeInfo().DeclaredProperties;
#endif
        }

        public static IEnumerable<PropertyInfo> GetInstanceProperties(this Type type)
        {
            DebugCheck.NotNull(type);

            return type.GetRuntimeProperties().Where(p => !p.IsStatic());
        }

#if NET40
        public static IEnumerable<PropertyInfo> GetRuntimeProperties(this Type type)
        {
            DebugCheck.NotNull(type);

            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return type.GetProperties(bindingFlags);
        }
#endif

#if NET40
        public static PropertyInfo GetRuntimeProperty(this Type type, string name)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotEmpty(name);

            return type.GetProperty(name);
        }
#endif

        public static PropertyInfo GetAnyProperty(this Type type, string name)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotEmpty(name);

            var props = type.GetRuntimeProperties().Where(p => p.Name == name).ToList();
            if (props.Count() > 1)
            {
                throw new AmbiguousMatchException();
            }

            return props.SingleOrDefault();
        }

        public static PropertyInfo GetInstanceProperty(this Type type, string name)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotEmpty(name);

            var props = type.GetRuntimeProperties().Where(p => p.Name == name && !p.IsStatic()).ToList();
            if (props.Count() > 1)
            {
                throw new AmbiguousMatchException();
            }

            return props.SingleOrDefault();
        }

        public static PropertyInfo GetStaticProperty(this Type type, string name)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotEmpty(name);

            var properties = type.GetRuntimeProperties().Where(p => p.Name == name && p.IsStatic()).ToList();
            if (properties.Count() > 1)
            {
                throw new AmbiguousMatchException();
            }

            return properties.SingleOrDefault();
        }

        public static PropertyInfo GetTopProperty(this Type type, string name)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotEmpty(name);

            do
            {
#if NET40
                const BindingFlags bindingFlags
                    = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
                var propertyInfo = type.GetProperty(name, bindingFlags);
                if (propertyInfo != null)
                {
                    return propertyInfo;
                }
                type = type.BaseType;
#else
                var typeInfo = type.GetTypeInfo();
                var propertyInfo = typeInfo.GetDeclaredProperty(name);
                if (propertyInfo != null && !(propertyInfo.GetMethod ?? propertyInfo.SetMethod).IsStatic)
                {
                    return propertyInfo;
                }
                type = typeInfo.BaseType;
#endif
            } while (type != null);

            return null;
        }

        public static Assembly Assembly(this Type type)
        {
            DebugCheck.NotNull(type);
#if NET40
            return type.Assembly;
#else
            return type.GetTypeInfo().Assembly;
#endif
        }

        public static Type BaseType(this Type type)
        {
            DebugCheck.NotNull(type);
#if NET40
            return type.BaseType;
#else
            return type.GetTypeInfo().BaseType;
#endif
        }

#if NET40
        public static IEnumerable<FieldInfo> GetRuntimeFields(this Type type)
        {
            DebugCheck.NotNull(type);

            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return type.GetFields(bindingFlags);
        }
#endif

        public static bool IsGenericType(this Type type)
        {
            DebugCheck.NotNull(type);
#if NET40
            return type.IsGenericType;
#else
            return type.GetTypeInfo().IsGenericType;
#endif
        }

        public static bool IsGenericTypeDefinition(this Type type)
        {
            DebugCheck.NotNull(type);
#if NET40
            return type.IsGenericTypeDefinition;
#else
            return type.GetTypeInfo().IsGenericTypeDefinition;
#endif
        }

        public static TypeAttributes Attributes(this Type type)
        {
            DebugCheck.NotNull(type);
#if NET40
            return type.Attributes;
#else
            return type.GetTypeInfo().Attributes;
#endif
        }

        public static bool IsClass(this Type type)
        {
            DebugCheck.NotNull(type);
#if NET40
            return type.IsClass;
#else
            return type.GetTypeInfo().IsClass;
#endif
        }

        public static bool IsInterface(this Type type)
        {
            DebugCheck.NotNull(type);

#if NET40
            return type.IsInterface;
#else
            return type.GetTypeInfo().IsInterface;
#endif
        }

        public static bool IsValueType(this Type type)
        {
            DebugCheck.NotNull(type);
#if NET40
            return type.IsValueType;
#else
            return type.GetTypeInfo().IsValueType;
#endif
        }

        public static bool IsAbstract(this Type type)
        {
            DebugCheck.NotNull(type);
#if NET40
            return type.IsAbstract;
#else
            return type.GetTypeInfo().IsAbstract;
#endif
        }

        public static bool IsSealed(this Type type)
        {
            DebugCheck.NotNull(type);
#if NET40
            return type.IsSealed;
#else
            return type.GetTypeInfo().IsSealed;
#endif
        }

        public static bool IsEnum(this Type type)
        {
            DebugCheck.NotNull(type);
#if NET40
            return type.IsEnum;
#else
            return type.GetTypeInfo().IsEnum;
#endif
        }

        public static bool IsSerializable(this Type type)
        {
            DebugCheck.NotNull(type);
#if NET40
            return type.IsSerializable;
#else
            return type.GetTypeInfo().IsSerializable;
#endif
        }

        public static bool IsGenericParameter(this Type type)
        {
            DebugCheck.NotNull(type);
#if NET40
            return type.IsGenericParameter;
#else
            return type.GetTypeInfo().IsGenericParameter;
#endif
        }

        public static bool ContainsGenericParameters(this Type type)
        {
            DebugCheck.NotNull(type);
#if NET40
            return type.ContainsGenericParameters;
#else
            return type.GetTypeInfo().ContainsGenericParameters;
#endif
        }

        public static bool IsPrimitive(this Type type)
        {
            DebugCheck.NotNull(type);
#if NET40
            return type.IsPrimitive;
#else
            return type.GetTypeInfo().IsPrimitive;
#endif
        }

        public static IEnumerable<ConstructorInfo> GetDeclaredConstructors(this Type type)
        {
            DebugCheck.NotNull(type);
#if NET40
            const BindingFlags bindingFlags
                = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            return type.GetConstructors(bindingFlags);
#else
            return type.GetTypeInfo().DeclaredConstructors;
#endif
        }

        public static ConstructorInfo GetDeclaredConstructor(this Type type, params Type[] parameterTypes)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(parameterTypes);

            return type.GetDeclaredConstructors().SingleOrDefault(
                c => !c.IsStatic && c.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes));
        }

        public static ConstructorInfo GetPublicConstructor(this Type type, params Type[] parameterTypes)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(parameterTypes);

            var constructor = type.GetDeclaredConstructor(parameterTypes);

            return constructor != null && constructor.IsPublic ? constructor : null;
        }

        public static ConstructorInfo GetDeclaredConstructor(
            this Type type, Func<ConstructorInfo, bool> predicate, params Type[][] parameterTypes)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(parameterTypes);

            return ((IEnumerable<Type[]>)parameterTypes)
                .Select(type.GetDeclaredConstructor)
                .FirstOrDefault(c => c != null && predicate(c));
        }

#if !NET40
        // This extension method will only be used when compiling for a platform on which Type
        // does not expose this method directly.
        public static bool IsSubclassOf(this Type type, Type otherType)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(otherType);

            return type.GetTypeInfo().IsSubclassOf(otherType);
        }
#endif
    }
}
