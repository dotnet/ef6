// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Reflection;

    internal class ObjectItemNoOpAssemblyLoader : ObjectItemAssemblyLoader
    {
        internal ObjectItemNoOpAssemblyLoader(Assembly assembly, ObjectItemLoadingSessionData sessionData)
            : base(assembly, new MutableAssemblyCacheEntry(), sessionData)
        {
        }

        internal override void Load()
        {
            // don't do anything but make sure we know we have seen this assembly
            if (
                !SessionData.KnownAssemblies.Contains(
                    SourceAssembly, SessionData.ObjectItemAssemblyLoaderFactory, SessionData.EdmItemCollection))
            {
                AddToKnownAssemblies();
            }
        }

        protected override void AddToAssembliesLoaded()
        {
            throw new NotImplementedException();
        }

        protected override void LoadTypesFromAssembly()
        {
            throw new NotImplementedException();
        }
    }
}
