using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Common.Utils;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Entity.Core.Mapping
{
    using Triple = Pair<EntitySetBase, Pair<EntityTypeBase, bool>>;

    /// <summary>
    /// Represents the Mapping metadata for an Extent in CS space.
    /// </summary>
    /// <example>
    /// For Example if conceptually you could represent the CS MSL file as following
    /// --Mapping 
    ///   --EntityContainerMapping ( CNorthwind-->SNorthwind )
    ///     --EntitySetMapping
    ///       --EntityTypeMapping
    ///         --MappingFragment
    ///       --EntityTypeMapping
    ///         --MappingFragment
    ///     --AssociationSetMapping 
    ///       --AssociationTypeMapping
    ///         --MappingFragment
    /// This class represents the metadata for all the extent map elements in the 
    /// above example namely EntitySetMapping, AssociationSetMapping and CompositionSetMapping.
    /// The SetMapping elements that are children of the EntityContainerMapping element
    /// can be accessed through the properties on this type.
    /// </example>
    internal abstract class StorageSetMapping
    {
        #region Constructors

        /// <summary>
        /// Construct the new StorageSetMapping object.
        /// </summary>
        /// <param name="extent">Extent metadata object</param>
        /// <param name="entityContainerMapping">The EntityContainer mapping that contains this extent mapping</param>
        internal StorageSetMapping(EntitySetBase extent, StorageEntityContainerMapping entityContainerMapping)
        {
            m_entityContainerMapping = entityContainerMapping;
            m_extent = extent;
            m_typeMappings = new List<StorageTypeMapping>();
        }

        #endregion

        #region Fields

        /// <summary>
        /// The EntityContainer mapping that contains this extent mapping.
        /// </summary>
        private readonly StorageEntityContainerMapping m_entityContainerMapping;

        /// <summary>
        /// The extent for which this mapping represents.
        /// </summary>
        private readonly EntitySetBase m_extent;

        /// <summary>
        /// Set of type mappings that make up the Set Mapping.
        /// Unless this is a EntitySetMapping with inheritance,
        /// you would have a single type mapping per set.
        /// </summary>
        private readonly List<StorageTypeMapping> m_typeMappings;

        /// <summary>
        /// Stores type-Specific user-defined QueryViews.
        /// </summary>
        private readonly Dictionary<Triple, string> m_typeSpecificQueryViews = new Dictionary<Triple, string>(Triple.PairComparer.Instance);

        #endregion

        #region Properties

        /// <summary>
        /// The set for which this mapping is for
        /// </summary>
        internal EntitySetBase Set
        {
            get { return m_extent; }
        }

        ///// <summary>
        ///// TypeMappings that make up this set type.
        ///// For AssociationSet and CompositionSet there will be one type (at least that's what
        ///// we expect as of now). EntitySet could have mappings for multiple Entity types.
        ///// </summary>
        internal ReadOnlyCollection<StorageTypeMapping> TypeMappings
        {
            get { return m_typeMappings.AsReadOnly(); }
        }

        internal StorageEntityContainerMapping EntityContainerMapping
        {
            get { return m_entityContainerMapping; }
        }

        /// <summary>
        /// Whether the SetMapping has empty content
        /// Returns true if there no table Mapping fragments
        /// </summary>
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
                        foreach (var propertyMap in mapFragment.AllProperties)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        internal string QueryView { get; set; }

        /// <summary>
        /// Line Number in MSL file where the Set Mapping Element's Start Tag is present.
        /// </summary>
        internal int StartLineNumber { get; set; }

        /// <summary>
        /// Line Position in MSL file where the Set Mapping Element's Start Tag is present.
        /// </summary>
        internal int StartLinePosition { get; set; }

        internal bool HasModificationFunctionMapping { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Add type mapping as a child under this SetMapping
        /// </summary>
        /// <param name="typeMapping"></param>
        internal void AddTypeMapping(StorageTypeMapping typeMapping)
        {
            m_typeMappings.Add(typeMapping);
        }

#if DEBUG
        /// <summary>
        /// This method is primarily for debugging purposes.
        /// Will be removed shortly.
        /// </summary>
        internal abstract void Print(int index);
#endif

        internal bool ContainsTypeSpecificQueryView(Triple key)
        {
            return m_typeSpecificQueryViews.ContainsKey(key);
        }

        /// <summary>
        /// Stores a type-specific user-defiend QueryView so that it can be loaded
        /// into StorageMappingItemCollection's view cache.
        /// </summary>
        internal void AddTypeSpecificQueryView(Triple key, string viewString)
        {
            Debug.Assert(!m_typeSpecificQueryViews.ContainsKey(key), "Query View already present for the given Key");
            m_typeSpecificQueryViews.Add(key, viewString);
        }

        internal ReadOnlyCollection<Triple> GetTypeSpecificQVKeys()
        {
            return new ReadOnlyCollection<Triple>(m_typeSpecificQueryViews.Keys.ToList());
        }

        internal string GetTypeSpecificQueryView(Triple key)
        {
            Debug.Assert(m_typeSpecificQueryViews.ContainsKey(key));
            return m_typeSpecificQueryViews[key];
        }

        #endregion
    }
}
