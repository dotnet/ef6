namespace System.Data.Entity.Utilities
{
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    internal static class MemberInfoExtensions
    {
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public static object GetValue(this MemberInfo memberInfo)
        {
            Contract.Requires(memberInfo != null);
            Contract.Assert(memberInfo is PropertyInfo || memberInfo is FieldInfo);

            var asPropertyInfo = memberInfo as PropertyInfo;
            if (asPropertyInfo != null)
            {
                return asPropertyInfo.GetValue(null, null);
            }
            return ((FieldInfo)memberInfo).GetValue(null);
        }
    }
}
