// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System.Collections.Generic;
    using EnvDTE;

    internal class AggregateProjectTypeGuidCache
    {
        private readonly Dictionary<Project, string> _cache;

        internal AggregateProjectTypeGuidCache()
        {
            _cache = new Dictionary<Project, string>();
        }

        internal void Add(Project project, string guids)
        {
            if (_cache.ContainsKey(project) == false)
            {
                _cache.Add(project, guids);
            }
        }

        internal void Remove(Project project)
        {
            if (_cache.ContainsKey(project))
            {
                _cache.Remove(project);
            }
        }

        internal string GetGuids(Project project)
        {
            return _cache.ContainsKey(project) ? _cache[project] : null;
        }
    }
}
