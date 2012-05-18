namespace System.Data.Entity.Utilities
{
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    internal static class AssemblyExtensions
    {
        public static string GetInformationalVersion(this Assembly assembly)
        {
            Contract.Requires(assembly != null);

            return assembly
                .GetCustomAttributes(false)
                .OfType<AssemblyInformationalVersionAttribute>()
                .Single()
                .InformationalVersion;
        }
    }
}
