namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Reflection;

    internal abstract class MetadataArtifactAssemblyResolver
    {
        internal abstract bool TryResolveAssemblyReference(AssemblyName refernceName, out Assembly assembly);
        internal abstract IEnumerable<Assembly> GetWildcardAssemblies();
    }
}
