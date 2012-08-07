// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Reflection;

    internal class CustomAssemblyResolver : MetadataArtifactAssemblyResolver
    {
        private readonly Func<AssemblyName, Assembly> _referenceResolver;
        private readonly Func<IEnumerable<Assembly>> _wildcardAssemblyEnumerator;

        internal CustomAssemblyResolver(
            Func<IEnumerable<Assembly>> wildcardAssemblyEnumerator, Func<AssemblyName, Assembly> referenceResolver)
        {
            Debug.Assert(wildcardAssemblyEnumerator != null);
            Debug.Assert(referenceResolver != null);
            _wildcardAssemblyEnumerator = wildcardAssemblyEnumerator;
            _referenceResolver = referenceResolver;
        }

        internal override bool TryResolveAssemblyReference(AssemblyName refernceName, out Assembly assembly)
        {
            assembly = _referenceResolver(refernceName);
            return assembly != null;
        }

        internal override IEnumerable<Assembly> GetWildcardAssemblies()
        {
            var wildcardAssemblies = _wildcardAssemblyEnumerator();
            if (wildcardAssemblies == null)
            {
                throw new InvalidOperationException(Strings.WildcardEnumeratorReturnedNull);
            }
            return wildcardAssemblies;
        }
    }
}
