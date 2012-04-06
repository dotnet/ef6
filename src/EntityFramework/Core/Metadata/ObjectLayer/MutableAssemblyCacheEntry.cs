using System.Collections.Generic;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm
{
    internal partial class MutableAssemblyCacheEntry : AssemblyCacheEntry
    {
        // types in "this" assembly
        private readonly List<EdmType> _typesInAssembly = new List<EdmType>();
        // other assemblies referenced by types we care about in "this" assembly
        private readonly List<Assembly> _closureAssemblies = new List<Assembly>();

        internal override IList<EdmType> TypesInAssembly
        {
            get { return _typesInAssembly; }
        }
        
        internal override IList<Assembly> ClosureAssemblies
        {
            get { return _closureAssemblies; }
        }
    }
}