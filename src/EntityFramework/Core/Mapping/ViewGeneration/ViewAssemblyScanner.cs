// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Reflection;

    internal class ViewAssemblyScanner
    {
        private readonly List<Assembly> _viewAssemblies;
        private readonly Dictionary<Assembly, bool> _visitedAssemblies;
        private readonly ViewAssemblyChecker _checker;

        public ViewAssemblyScanner(
            IEnumerable<Assembly> viewAssemblies,
            IDictionary<Assembly, bool> visitedAssemblies,
            ViewAssemblyChecker checker)
        {
            DebugCheck.NotNull(viewAssemblies);
            DebugCheck.NotNull(visitedAssemblies);
            DebugCheck.NotNull(checker);

            _viewAssemblies = new List<Assembly>(viewAssemblies);
            _visitedAssemblies = new Dictionary<Assembly, bool>(visitedAssemblies);
            _checker = checker;
        }

        public virtual Dictionary<Assembly, bool> VisitedAssemblies
        {
            get { return _visitedAssemblies; }
        }

        public virtual List<Assembly> ViewAssemblies
        {
            get { return _viewAssemblies; }
        }

        public virtual void ScanAssembly(Assembly assembly, bool followReferences)
        {
            DebugCheck.NotNull(assembly);

            bool referencesChecked;
            if (_visitedAssemblies.TryGetValue(assembly, out referencesChecked)
                && (!followReferences || referencesChecked))
            {
                return;
            }

            if (!_viewAssemblies.Contains(assembly)
                && _checker.IsViewAssembly(assembly))
            {
                _viewAssemblies.Add(assembly);
            }

            _visitedAssemblies[assembly] = followReferences;

            if (followReferences)
            {
                foreach (var referenceAssembly in MetadataAssemblyHelper.GetNonSystemReferencedAssemblies(assembly))
                {
                    ScanAssembly(referenceAssembly, true);
                }
            }
        }
    }
}
