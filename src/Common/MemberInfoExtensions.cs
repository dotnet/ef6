// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if EF_FUNCTIONALS
namespace System.Data.Entity.Functionals.Utilities
#else
namespace System.Data.Entity.Utilities
#endif
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    internal static class MemberInfoExtensions
    {
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public static object GetValue(this MemberInfo memberInfo)
        {
            DebugCheck.NotNull(memberInfo);
            Debug.Assert(memberInfo is PropertyInfo || memberInfo is FieldInfo);

            var asPropertyInfo = memberInfo as PropertyInfo;
            return asPropertyInfo != null ? asPropertyInfo.GetValue(null, null) : ((FieldInfo)memberInfo).GetValue(null);
        }

#if NET40
        public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo memberInfo, bool inherit) where T : Attribute
        {
            DebugCheck.NotNull(memberInfo);

            if (inherit && memberInfo.MemberType == MemberTypes.Property)
            {
                // Handle issue that .NET code doesn't honor inherit flag, but new APIs do, so we want
                // to honor it also.
                return ((PropertyInfo)memberInfo)
                    .GetPropertiesInHierarchy()
                    .SelectMany(p => p.GetCustomAttributes(typeof(T), inherit: false).OfType<T>());
            }

            return memberInfo.GetCustomAttributes(typeof(T), inherit).OfType<T>();
        }
#endif
    }
}
