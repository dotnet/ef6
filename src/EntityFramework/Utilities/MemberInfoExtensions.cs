// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    internal static class MemberInfoExtensions
    {
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public static object GetValue(this MemberInfo memberInfo)
        {
            DebugCheck.NotNull(memberInfo);
            Debug.Assert(memberInfo is PropertyInfo || memberInfo is FieldInfo);

            var asPropertyInfo = memberInfo as PropertyInfo;
            if (asPropertyInfo != null)
            {
                return asPropertyInfo.GetValue(null, null);
            }
            return ((FieldInfo)memberInfo).GetValue(null);
        }
    }
}
