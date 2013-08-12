// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    internal class ImmutableAssemblyCacheEntry : AssemblyCacheEntry
    {
        // types in "this" assembly
        private readonly ReadOnlyCollection<EdmType> _typesInAssembly;
        // other assemblies referenced by types we care about in "this" assembly
        private readonly ReadOnlyCollection<Assembly> _closureAssemblies;

        internal ImmutableAssemblyCacheEntry(MutableAssemblyCacheEntry mutableEntry)
        {
            _typesInAssembly = new ReadOnlyCollection<EdmType>(new List<EdmType>(mutableEntry.TypesInAssembly));
            _closureAssemblies = new ReadOnlyCollection<Assembly>(new List<Assembly>(mutableEntry.ClosureAssemblies));
        }

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
