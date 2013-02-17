// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Reflection;

    internal class ViewAssemblyCache : IViewAssemblyCache
    {
        private readonly object _lock = new object();
        private volatile List<Assembly> _assemblies = new List<Assembly>();
        private volatile Dictionary<Assembly, bool> _visitedAssemblies = new Dictionary<Assembly, bool>();
        private readonly ViewAssemblyChecker _checker;

        public ViewAssemblyCache(ViewAssemblyChecker checker = null)
        {
            _checker = checker ?? new ViewAssemblyChecker();
        }

        public IEnumerable<Assembly> Assemblies
        {
            get { return _assemblies; }
        }

        public void CheckAssembly(Assembly assembly, bool followReferences)
        {
            DebugCheck.NotNull(assembly);

            if (IsAssemblyVisited(assembly, followReferences))
            {
                return;
            }

            lock (_lock)
            {
                if (IsAssemblyVisited(assembly, followReferences))
                {
                    return;
                }

                var scanner = new ViewAssemblyScanner(_assemblies, _visitedAssemblies, _checker);
                scanner.ScanAssembly(assembly, followReferences);

                _assemblies = scanner.ViewAssemblies;
                _visitedAssemblies = scanner.VisitedAssemblies;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _assemblies = new List<Assembly>();
                _visitedAssemblies = new Dictionary<Assembly, bool>();
            }
        }

        private bool IsAssemblyVisited(Assembly assembly, bool followReferences)
        {
            bool referencesChecked;
            return _visitedAssemblies.TryGetValue(assembly, out referencesChecked)
                   && (!followReferences || referencesChecked);
        }
    }
}
