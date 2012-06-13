namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    internal class SortedEntityTypeIndex
    {
        private static readonly EdmEntityType[] _emptyTypes = new EdmEntityType[0];

        private readonly Dictionary<EdmEntitySet, List<EdmEntityType>> _entityTypes;
        // these are sorted where base types come before derived types

        public SortedEntityTypeIndex()
        {
            _entityTypes = new Dictionary<EdmEntitySet, List<EdmEntityType>>();
        }

        public void Add(EdmEntitySet entitySet, EdmEntityType entityType)
        {
            Contract.Requires(entitySet != null);
            Contract.Requires(entityType != null);

            var i = 0;

            List<EdmEntityType> entityTypes;
            if (!_entityTypes.TryGetValue(entitySet, out entityTypes))
            {
                entityTypes = new List<EdmEntityType>();
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

        public bool Contains(EdmEntitySet entitySet, EdmEntityType entityType)
        {
            Contract.Requires(entitySet != null);
            Contract.Requires(entityType != null);

            List<EdmEntityType> setTypes;
            return _entityTypes.TryGetValue(entitySet, out setTypes) && setTypes.Contains(entityType);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool IsRoot(EdmEntitySet entitySet, EdmEntityType entityType)
        {
            Contract.Requires(entitySet != null);
            Contract.Requires(entityType != null);

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

        public IEnumerable<EdmEntitySet> GetEntitySets()
        {
            return _entityTypes.Keys;
        }

        public IEnumerable<EdmEntityType> GetEntityTypes(EdmEntitySet entitySet)
        {
            List<EdmEntityType> entityTypes;
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
