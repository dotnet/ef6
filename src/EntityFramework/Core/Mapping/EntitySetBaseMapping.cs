// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;
    using Triple = Common.Utils.Pair<Metadata.Edm.EntitySetBase, Common.Utils.Pair<Metadata.Edm.EntityTypeBase, bool>>;

    /// <summary>
    /// Represents the Mapping metadata for an Extent in CS space.
    /// </summary>
    /// <example>
    /// For Example if conceptually you could represent the CS MSL file as following
    /// --Mapping
    /// --EntityContainerMapping ( CNorthwind-->SNorthwind )
    /// --EntitySetMapping
    /// --EntityTypeMapping
    /// --MappingFragment
    /// --EntityTypeMapping
    /// --MappingFragment
    /// --AssociationSetMapping
    /// --AssociationTypeMapping
    /// --MappingFragment
    /// This class represents the metadata for all the extent map elements in the
    /// above example namely EntitySetMapping, AssociationSetMapping and CompositionSetMapping.
    /// The EntitySetBaseMapping elements that are children of the EntityContainerMapping element
    /// can be accessed through the properties on this type.
    /// </example>
    public abstract class EntitySetBaseMapping : MappingItem
    {
        private readonly EntityContainerMapping _containerMapping;
        private string _queryView;

        // Stores type-Specific user-defined query views.
        private readonly Dictionary<Triple, string> _typeSpecificQueryViews = new Dictionary<Triple, string>(Triple.PairComparer.Instance);

        internal EntitySetBaseMapping(EntityContainerMapping containerMapping)
        {
            _containerMapping = containerMapping;
        }

        /// <summary>
        /// Gets the parent container mapping.
        /// </summary>
        public EntityContainerMapping ContainerMapping
        {
            get { return _containerMapping; }
        }

        internal EntityContainerMapping EntityContainerMapping
        {
            get { return ContainerMapping; }
        }

        /// <summary>
        /// Gets or sets the query view associated with this mapping.
        /// </summary>
        public string QueryView
        {
            get { return _queryView; }

            set
            {
                ThrowIfReadOnly();

                _queryView = value;                 
            }
        }

        internal abstract EntitySetBase Set { get; }

        internal abstract IEnumerable<TypeMapping> TypeMappings { get; }

        // Returns true if there no table mapping fragments.
        internal virtual bool HasNoContent
        {
            get
            {
                if (QueryView != null)
                {
                    return false;
                }
                foreach (var typeMap in TypeMappings)
                {
                    foreach (var mapFragment in typeMap.MappingFragments)
                    {
                        if (mapFragment.AllProperties.Any())
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }     

        // <summary>
        // Line Number in MSL file where the Set Mapping Element's Start Tag is present.
        // </summary>
        internal int StartLineNumber { get; set; }

        // <summary>
        // Line Position in MSL file where the Set Mapping Element's Start Tag is present.
        // </summary>
        internal int StartLinePosition { get; set; }

        internal bool HasModificationFunctionMapping { get; set; }

        internal bool ContainsTypeSpecificQueryView(Triple key)
        {
            return _typeSpecificQueryViews.ContainsKey(key);
        }

        // Stores a type-specific user-defiend QueryView so that it can be loaded
        // into StorageMappingItemCollection's view cache.
        internal void AddTypeSpecificQueryView(Triple key, string viewString)
        {
            Debug.Assert(!_typeSpecificQueryViews.ContainsKey(key), "Query View already present for the given Key");
            _typeSpecificQueryViews.Add(key, viewString);
        }

        internal ReadOnlyCollection<Triple> GetTypeSpecificQVKeys()
        {
            return new ReadOnlyCollection<Triple>(_typeSpecificQueryViews.Keys.ToList());
        }

        internal string GetTypeSpecificQueryView(Triple key)
        {
            Debug.Assert(_typeSpecificQueryViews.ContainsKey(key));
            return _typeSpecificQueryViews[key];
        }
    }
}
