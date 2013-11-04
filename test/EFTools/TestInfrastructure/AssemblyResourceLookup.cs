// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace EFDesignerTestInfrastructure
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Resources;

    /// <summary>
    ///     Locates localized resources for an assembly
    /// </summary>
    public class AssemblyResourceLookup
    {
        private readonly Assembly _assembly;
        private readonly ResourceManager _resourceManager;

        /// <summary>
        ///     Initializes a new instance of the AssemblyResourceLookup class.
        /// </summary>
        /// <param name="assembly"> Assembly that resources belong to </param>
        public AssemblyResourceLookup(Assembly assembly)
            : this(assembly, BuildResourceManager(assembly))
        {
        }

        public static ResourceManager BuildResourceManager(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            return new ResourceManager(FindSingleResourceTable(assembly), assembly);
        }

        private static string FindSingleResourceTable(Assembly assembly)
        {
            var resources = assembly.GetManifestResourceNames().Where(r => r.EndsWith(".resources", StringComparison.Ordinal));
            if (resources.Count() != 1)
            {
                throw new NotSupportedException(
                    "The supplied assembly does not contain exactly one resource table, if the assembly contains multiple tables call the overload that specifies which table to use.");
            }

            var resource = resources.Single();

            // Need to trim the ".resources" off the end
            return resource.Substring(0, resource.Length - 10);
        }

        /// <summary>
        ///     Initializes a new instance of the AssemblyResourceLookup class.
        /// </summary>
        /// <param name="assembly"> Assembly that resources belong to </param>
        /// <param name="resourceTable"> Resource table to lookup strings in </param>
        public AssemblyResourceLookup(Assembly assembly, string resourceTable)
            : this(assembly, new ResourceManager(resourceTable, assembly))
        {
        }

        private AssemblyResourceLookup(Assembly assembly, ResourceManager manager)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            _assembly = assembly;
            _resourceManager = manager;
        }

        /// <summary>
        ///     Finds a specific string resource
        /// </summary>
        /// <param name="resourceKey"> Key of the resource to be located </param>
        /// <returns> The localized resource value </returns>
        public string LookupString(string resourceKey)
        {
            var messageFromResources = _resourceManager.GetString(resourceKey);
            if (messageFromResources == null)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "No string with key {0} was found in resource table {1} in assembly {2}.",
                        resourceKey,
                        _resourceManager.BaseName,
                        _assembly.FullName));
            }

            return messageFromResources;
        }
    }
}
