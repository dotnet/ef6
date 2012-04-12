namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.EntityModel.SchemaObjectModel;
    using System.IO;
    using System.Reflection;

    internal static class MetadataAssemblyHelper
    {
        private const string EcmaPublicKey = "b77a5c561934e089";
        private const string MicrosoftPublicKey = "b03f5f7f11d50a3a";

        private static readonly byte[] EcmaPublicKeyToken = ScalarType.ConvertToByteArray(EcmaPublicKey);
        private static readonly byte[] MsPublicKeyToken = ScalarType.ConvertToByteArray(MicrosoftPublicKey);

        private static readonly Memoizer<Assembly, bool> _filterAssemblyCacheByAssembly =
            new Memoizer<Assembly, bool>(ComputeShouldFilterAssembly, EqualityComparer<Assembly>.Default);

        internal static Assembly SafeLoadReferencedAssembly(AssemblyName assemblyName)
        {
            Assembly assembly = null;

            try
            {
                assembly = Assembly.Load(assemblyName);
            }
            catch (FileNotFoundException)
            {
                // See 552932: ObjectItemCollection: fails on referenced assemblies that are not available
            }

            return assembly;
        }

        private static bool ComputeShouldFilterAssembly(Assembly assembly)
        {
            var assemblyName = new AssemblyName(assembly.FullName);
            return ShouldFilterAssembly(assemblyName);
        }

        internal static bool ShouldFilterAssembly(Assembly assembly)
        {
            return _filterAssemblyCacheByAssembly.Evaluate(assembly);
        }

        /// <summary>Is the assembly and its referened assemblies not expected to have any metadata</summary>
        private static bool ShouldFilterAssembly(AssemblyName assemblyName)
        {
            return (ArePublicKeyTokensEqual(assemblyName.GetPublicKeyToken(), EcmaPublicKeyToken) ||
                    ArePublicKeyTokensEqual(assemblyName.GetPublicKeyToken(), MsPublicKeyToken));
        }

        private static bool ArePublicKeyTokensEqual(byte[] left, byte[] right)
        {
            // some assemblies don't have public keys
            if (left.Length
                != right.Length)
            {
                return false;
            }

            for (var i = 0; i < left.Length; i++)
            {
                if (left[i]
                    != right[i])
                {
                    return false;
                }
            }
            return true;
        }

        internal static IEnumerable<Assembly> GetNonSystemReferencedAssemblies(Assembly assembly)
        {
            foreach (var name in assembly.GetReferencedAssemblies())
            {
                if (!ShouldFilterAssembly(name))
                {
                    var referenceAssembly = SafeLoadReferencedAssembly(name);
                    if (referenceAssembly != null)
                    {
                        yield return referenceAssembly;
                    }
                }
            }
        }
    }
}
