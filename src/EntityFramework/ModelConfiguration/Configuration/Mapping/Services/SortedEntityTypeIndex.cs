// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    internal class SortedEntityTypeIndex
    {
        private static readonly EntityType[] _emptyTypes = new EntityType[0];

        private readonly Dictionary<EntitySet, List<EntityType>> _entityTypes;
        // these are sorted where base types come before derived types

        public SortedEntityTypeIndex()
        {
            _entityTypes = new Dictionary<EntitySet, List<EntityType>>();
        }

        public void Add(EntitySet entitySet, EntityType entityType)
        {
            DebugCheck.NotNull(entitySet);
            DebugCheck.NotNull(entityType);

            var i = 0;

            List<EntityType> entityTypes;
            if (!_entityTypes.TryGetValue(entitySet, out entityTypes))
            {
                entityTypes = new List<EntityType>();
                _entityTypes.Add(entitySet, entityTypes);
            }

            for (; i < entityTypes.Count; i++)
            {
                if (entityTypes[i] == entityType)
                {
                    return;
                }
                else if (entityType.IsAncestorOf(entityTypes[i]))
                {
                    break;
                }
            }
            entityTypes.Insert(i, entityType);
        }

        public bool Contains(EntitySet entitySet, EntityType entityType)
        {
            DebugCheck.NotNull(entitySet);
            DebugCheck.NotNull(entityType);

            List<EntityType> setTypes;
            return _entityTypes.TryGetValue(entitySet, out setTypes) && setTypes.Contains(entityType);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool IsRoot(EntitySet entitySet, EntityType entityType)
        {
            DebugCheck.NotNull(entitySet);
            DebugCheck.NotNull(entityType);

            var isRoot = true;
            var entityTypes = _entityTypes[entitySet];

            foreach (var et in entityTypes)
            {
                if (et != entityType
                    &&
                    et.IsAncestorOf(entityType))
                {
                    isRoot = false;
                }
            }

            return isRoot;
        }

        public IEnumerable<EntitySet> GetEntitySets()
        {
            return _entityTypes.Keys;
        }

        public IEnumerable<EntityType> GetEntityTypes(EntitySet entitySet)
        {
            List<EntityType> entityTypes;
            if (_entityTypes.TryGetValue(entitySet, out entityTypes))
            {
                return entityTypes;
            }
            else
            {
                return _emptyTypes;
            }
        }
    }
}
