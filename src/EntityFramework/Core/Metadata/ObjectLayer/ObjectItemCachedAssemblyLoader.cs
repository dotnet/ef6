// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Reflection;

    internal sealed class ObjectItemCachedAssemblyLoader : ObjectItemAssemblyLoader
    {
        private new ImmutableAssemblyCacheEntry CacheEntry
        {
            get { return (ImmutableAssemblyCacheEntry)base.CacheEntry; }
        }

        internal ObjectItemCachedAssemblyLoader(
            Assembly assembly, ImmutableAssemblyCacheEntry cacheEntry, ObjectItemLoadingSessionData sessionData)
            : base(assembly, cacheEntry, sessionData)
        {
        }

        protected override void AddToAssembliesLoaded()
        {
            // wasn't loaded, was pulled from cache instead
            // so don't load it
        }

        protected override void LoadTypesFromAssembly()
        {
            foreach (var type in CacheEntry.TypesInAssembly)
            {
                if (!SessionData.TypesInLoading.ContainsKey(type.Identity))
                {
                    SessionData.TypesInLoading.Add(type.Identity, type);
                }
            }
        }
    }
}
