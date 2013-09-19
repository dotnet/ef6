// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Utilities;

    internal sealed class KnownAssemblyEntry
    {
        private readonly AssemblyCacheEntry _cacheEntry;

        internal KnownAssemblyEntry(AssemblyCacheEntry cacheEntry, bool seenWithEdmItemCollection)
        {
            DebugCheck.NotNull(cacheEntry);
            _cacheEntry = cacheEntry;
            ReferencedAssembliesAreLoaded = false;
            SeenWithEdmItemCollection = seenWithEdmItemCollection;
        }

        internal AssemblyCacheEntry CacheEntry
        {
            get { return _cacheEntry; }
        }

        public bool ReferencedAssembliesAreLoaded { get; set; }

        public bool SeenWithEdmItemCollection { get; set; }

        public bool HaveSeenInCompatibleContext(object loaderCookie, EdmItemCollection itemCollection)
        {
            // a new "context" is only when we have not seen this assembly with an itemCollection that is non-null
            // and we now have a non-null itemCollection, and we are not already in AttributeLoader mode.
            return SeenWithEdmItemCollection ||
                   itemCollection == null ||
                   ObjectItemAssemblyLoader.IsAttributeLoader(loaderCookie);
        }
    }
}
