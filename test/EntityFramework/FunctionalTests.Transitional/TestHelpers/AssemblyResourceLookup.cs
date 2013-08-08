// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    /// <summary>
    /// Locates localized resources for an assembly
    /// </summary>
    public class AssemblyResourceLookup
    {
        private readonly Assembly _assembly;
        private readonly ResourceManager _resourceManager;

        /// <summary>
        /// Initializes a new instance of the AssemblyResourceLookup class.
        /// </summary>
        /// <param name="assembly"> Assembly that resources belong to </param>
        public AssemblyResourceLookup(Assembly assembly)
            : this(assembly, ResourceUtilities.BuildResourceManager(assembly))
        {
        }

        /// <summary>
        /// Initializes a new instance of the AssemblyResourceLookup class.
        /// </summary>
        /// <param name="assembly"> Assembly that resources belong to </param>
        /// <param name="resourceTable"> Resource table to lookup strings in </param>
        public AssemblyResourceLookup(Assembly assembly, string resourceTable)
            : this(assembly, new ResourceManager(resourceTable, assembly))
        {
        }

        private AssemblyResourceLookup(Assembly assembly, ResourceManager manager)
        {
            ExceptionHelpers.CheckArgumentNotNull(assembly, "assembly");
            _assembly = assembly;
            _resourceManager = manager;
        }

        /// <summary>
        /// Finds a specific string resource
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
