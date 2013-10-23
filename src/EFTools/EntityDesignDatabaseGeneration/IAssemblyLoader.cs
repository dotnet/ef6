// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.DatabaseGeneration
{
    using System.Reflection;

    /// <summary>
    ///     Resolves workflow OutputGenerators.
    /// </summary>
    public interface IAssemblyLoader
    {
        /// <summary>
        ///     Attempts to load an assembly.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly to be loaded.</param>
        /// <returns>The resolved assembly reference.</returns>
        Assembly LoadAssembly(string assemblyName);
    }
}
