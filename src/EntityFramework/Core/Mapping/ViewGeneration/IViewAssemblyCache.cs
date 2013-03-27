// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity.Config;
    using System.Reflection;

    /// <summary>
    ///     Represents a cache of the assemblies that contain pre-generated views. A default
    ///     implementation of this interface is used by default, but this can be replaced using the
    ///     <see cref="DbConfiguration" /> class. A replacement is typically used to let EF know
    ///     the assemblies that contain pre-generated views without doing any discovery.
    ///     Implementations of this interface must be thread-safe.
    /// </summary>
    public interface IViewAssemblyCache
    {
        /// <summary>
        ///     The list of assemblies known to contain pre-generated views.
        /// </summary>
        IEnumerable<Assembly> Assemblies { get; }

        /// <summary>
        ///     Called by EF when an assembly (and possibly any referenced assemblies) should be checked
        ///     to see if it/they contains pre-generated views. This method may have nothing to do if a custom
        ///     implementation of this interface is being used and the assemblies that contain pre-generated
        ///     views are known in advance.
        /// </summary>
        /// <param name="assembly">The assembly to start the check from.</param>
        /// <param name="followReferences">
        ///     True if all referenced assemblies should also be checked;
        ///     false if only the given assembly should be checked.
        /// </param>
        void CheckAssembly(Assembly assembly, bool followReferences);

        /// <summary>
        ///     Clears any information about which assemblies have been checked such that checking will begin
        ///     again next time that CheckAssembly is called. This method may have nothing to do if a custom
        ///     implementation of this interface is being used and the assemblies that contain pre-generated
        ///     views are known in advance.
        /// </summary>
        void Clear();
    }
}
