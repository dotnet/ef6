// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.ELinq
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    ///     Static utility class. Replica of query\DLinq\TypeSystem.cs
    /// </summary>
    internal static class TypeSystem
    {
        private static readonly MethodInfo _getDefaultMethod = typeof(TypeSystem).GetMethod(
            "GetDefault", BindingFlags.Static | BindingFlags.NonPublic);

        private static T GetDefault<T>()
        {
            return default(T);
        }

        internal static object GetDefaultValue(Type type)
        {
            // null is always the default for non value types and Nullable<>
            if (!type.IsValueType
                ||
                (type.IsGenericType &&
                 typeof(Nullable<>) == type.GetGenericTypeDefinition()))
            {
                return null;
            }
            var getDefaultMethod = _getDefaultMethod.MakeGenericMethod(type);
            var defaultValue = getDefaultMethod.Invoke(null, new object[] { });
            return defaultValue;
        }

        internal static bool IsSequenceType(Type seqType)
        {
            return FindIEnumerable(seqType) != null;
        }

        internal static Type GetDelegateType(IEnumerable<Type> inputTypes, Type returnType)
        {
            DebugCheck.NotNull(returnType);

            // Determine Func<> type (generic args are the input parameter types plus the return type)
            inputTypes = inputTypes ?? Enumerable.Empty<Type>();
            var argCount = inputTypes.Count();
            var typeArgs = new Type[argCount + 1];
            var i = 0;
            foreach (var typeArg in inputTypes)
            {
                typeArgs[i++] = typeArg;
            }
            typeArgs[i] = returnType;

            // Find appropriate Func<>
            Type delegateType;
            switch (argCount)
            {
                case 0:
                    delegateType = typeof(Func<>);
                    break;
                case 1:
                    delegateType = typeof(Func<,>);
                    break;
                case 2:
                    delegateType = typeof(Func<,,>);
                    break;
                case 3:
                    delegateType = typeof(Func<,,,>);
                    break;
                case 4:
                    delegateType = typeof(Func<,,,,>);
                    break;
                case 5:
                    delegateType = typeof(Func<,,,,,>);
                    break;
                case 6:
                    delegateType = typeof(Func<,,,,,,>);
                    break;
                case 7:
                    delegateType = typeof(Func<,,,,,,,>);
                    break;
                case 8:
                    delegateType = typeof(Func<,,,,,,,,>);
                    break;
                case 9:
                    delegateType = typeof(Func<,,,,,,,,,>);
                    break;
                case 10:
                    delegateType = typeof(Func<,,,,,,,,,,>);
                    break;
                case 11:
                    delegateType = typeof(Func<,,,,,,,,,,,>);
                    break;
                case 12:
                    delegateType = typeof(Func<,,,,,,,,,,,,>);
                    break;
                case 13:
                    delegateType = typeof(Func<,,,,,,,,,,,,,>);
                    break;
                case 14:
                    delegateType = typeof(Func<,,,,,,,,,,,,,,>);
                    break;
                case 15:
                    delegateType = typeof(Func<,,,,,,,,,,,,,,,>);
                    break;
                default:
                    Debug.Fail("unexpected argument count");
                    delegateType = null;
                    break;
            }
            delegateType = delegateType.MakeGenericType(typeArgs);

            return delegateType;
        }

        internal static Expression EnsureType(Expression expression, Type requiredType)
        {
            DebugCheck.NotNull(expression);
            DebugCheck.NotNull(requiredType);
            if (expression.Type != requiredType)
            {
                expression = Expression.Convert(expression, requiredType);
            }
            return expression;
        }

        /// <summary>
        ///     Resolves MemberInfo to a property or field.
        /// </summary>
        /// <param name="member"> Member to test. </param>
        /// <param name="name"> Name of member. </param>
        /// <param name="type"> Type of member. </param>
        /// <returns> Given member normalized as a property or field. </returns>
        internal static MemberInfo PropertyOrField(MemberInfo member, out string name, out Type type)
        {
            name = null;
            type = null;

            if (member.MemberType
                == MemberTypes.Field)
            {
                var field = (FieldInfo)member;
                name = field.Name;
                type = field.FieldType;
                return field;
            }
            else if (member.MemberType
                     == MemberTypes.Property)
            {
                var property = (PropertyInfo)member;
                if (0 != property.GetIndexParameters().Length)
                {
                    // don't support indexed properties
                    throw new NotSupportedException(Strings.ELinq_PropertyIndexNotSupported);
                }
                name = property.Name;
                type = property.PropertyType;
                return property;
            }
            else if (member.MemberType
                     == MemberTypes.Method)
            {
                // this may be a property accessor in disguise (if it's a RuntimeMethodHandle)
                var method = (MethodInfo)member;
                if (method.IsSpecialName) // property accessor methods must set IsSpecialName
                {
                    // try to find a property with the given getter
                    foreach (var property in method.DeclaringType.GetProperties(
                        BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (property.CanRead
                            && (property.GetGetMethod(true) == method))
                        {
                            return PropertyOrField(property, out name, out type);
                        }
                    }
                }
            }
            throw new NotSupportedException(Strings.ELinq_NotPropertyOrField(member.Name));
        }

        private static Type FindIEnumerable(Type seqType)
        {
            // Ignores "terminal" primitive types in the EDM although they may implement IEnumerable<>
            if (seqType == null
                || seqType == typeof(string)
                || seqType == typeof(byte[]))
            {
                return null;
            }
            if (seqType.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            }
            if (seqType.IsGenericType)
            {
                foreach (var arg in seqType.GetGenericArguments())
                {
                    var ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType))
                    {
                        return ienum;
                    }
                }
            }
            var ifaces = seqType.GetInterfaces();
            if (ifaces != null
                && ifaces.Length > 0)
            {
                foreach (var iface in ifaces)
                {
                    var ienum = FindIEnumerable(iface);
                    if (ienum != null)
                    {
                        return ienum;
                    }
                }
            }
            if (seqType.BaseType != null
                && seqType.BaseType != typeof(object))
            {
                return FindIEnumerable(seqType.BaseType);
            }
            return null;
        }

        internal static Type GetElementType(Type seqType)
        {
            var ienum = FindIEnumerable(seqType);
            if (ienum == null)
            {
                return seqType;
            }
            return ienum.GetGenericArguments()[0];
        }

        internal static bool IsNullableType(Type type)
        {
            var nonNullableType = GetNonNullableType(type);

            return nonNullableType != null && nonNullableType != type;
        }

        internal static Type GetNonNullableType(Type type)
        {
            if (type != null)
            {
                return Nullable.GetUnderlyingType(type) ?? type;
            }

            return null;
        }

        internal static bool IsImplementationOfGenericInterfaceMethod(this MethodInfo test, Type match, out Type[] genericTypeArguments)
        {
            genericTypeArguments = null;

            // check requirements for a match
            if (null == test
                || null == match
                || !match.IsInterface
                || !match.IsGenericTypeDefinition
                || null == test.DeclaringType)
            {
                return false;
            }

            // we might be looking at the interface implementation directly
            if (test.DeclaringType.IsInterface
                && test.DeclaringType.IsGenericType
                && test.DeclaringType.GetGenericTypeDefinition() == match)
            {
                return true;
            }

            // figure out if we implement the interface
            foreach (var testInterface in test.DeclaringType.GetInterfaces())
            {
                if (testInterface.IsGenericType
                    && testInterface.GetGenericTypeDefinition() == match)
                {
                    // check if the method aligns
                    var map = test.DeclaringType.GetInterfaceMap(testInterface);
                    if (map.TargetMethods.Contains(test))
                    {
                        genericTypeArguments = testInterface.GetGenericArguments();
                        return true;
                    }
                }
            }

            return false;
        }

        internal static bool IsImplementationOf(this PropertyInfo propertyInfo, Type interfaceType)
        {
            Debug.Assert(interfaceType.IsInterface, "Ensure interfaceType is an interface before calling IsImplementationOf");

            // Find the property with the corresponding name on the interface, if present
            var interfaceProp = interfaceType.GetProperty(propertyInfo.Name, BindingFlags.Public | BindingFlags.Instance);
            if (null == interfaceProp)
            {
                return false;
            }

            // If the declaring type is an interface, compare directly.
            if (propertyInfo.DeclaringType.IsInterface)
            {
                return interfaceProp.Equals(propertyInfo);
            }

            Debug.Assert(
                propertyInfo.DeclaringType.GetInterfaces().Contains(interfaceType),
                "Ensure propertyInfo.DeclaringType implements interfaceType before calling IsImplementationOf");

            var result = false;

            // Get the get_<Property> method from the interface property.
            var getInterfaceProp = interfaceProp.GetGetMethod();

            // Retrieve the interface mapping for the interface on the candidate property's declaring type.
            var interfaceMap = propertyInfo.DeclaringType.GetInterfaceMap(interfaceType);

            // Find the index of the interface's get_<Property> method in the interface methods of the interface map
            var propIndex = Array.IndexOf(interfaceMap.InterfaceMethods, getInterfaceProp);

            // Find the method on the property's declaring type that is the target of the interface's get_<Property> method.
            // This method will be at the same index in the interface mapping's target methods as the get_<Property> interface method index.
            var targetMethods = interfaceMap.TargetMethods;
            if (propIndex > -1
                && propIndex < targetMethods.Length)
            {
                // If the get method of the referenced property is the target of the get_<Property> method in this interface mapping,
                // then the property is the implementation of the interface's corresponding property.
                var getPropertyMethod = propertyInfo.GetGetMethod();
                if (getPropertyMethod != null)
                {
                    result = getPropertyMethod.Equals(targetMethods[propIndex]);
                }
            }

            return result;
        }
    }
}
