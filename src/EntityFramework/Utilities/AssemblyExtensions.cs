// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class AssemblyExtensions
    {
        public static string GetInformationalVersion(this Assembly assembly)
        {
            DebugCheck.NotNull(assembly);

            return assembly
                .GetCustomAttributes(false)
                .OfType<AssemblyInformationalVersionAttribute>()
                .Single()
                .InformationalVersion;
        }

        public static IEnumerable<Type> GetAccessibleTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // The exception is thrown if some types cannot be loaded in partial trust.
                // For our purposes we just want to get the types that are loaded, which are
                // provided in the Types property of the exception.
                return ex.Types.Where(t => t != null);
            }
        }
    }
}
