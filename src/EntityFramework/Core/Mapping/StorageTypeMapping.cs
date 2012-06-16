namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    /// Represents the Mapping metadata for a type map in CS space.
    /// </summary>
    /// <example>
    /// For Example if conceptually you could represent the CS MSL file as following
    /// --Mapping 
    ///   --EntityContainerMapping ( CNorthwind-->SNorthwind )
    ///     --EntitySetMapping
    ///       --EntityTypeMapping
    ///         --MappingFragment
    ///           --EntityKey
    ///             --ScalarPropertyMap
    ///           --ScalarPropertyMap
    ///       --EntityTypeMapping
    ///         --MappingFragment
    ///           --EntityKey
    ///             --ScalarPropertyMap
    ///           --ComplexPropertyMap
    ///             --ScalarPropertyMap
    ///             --ScalarProperyMap
    ///           --ScalarPropertyMap
    ///     --AssociationSetMapping 
    ///       --AssociationTypeMapping
    ///         --MappingFragment
    ///           --EndPropertyMap
    ///             --ScalarPropertyMap
    ///             --ScalarProperyMap
    ///           --EndPropertyMap
    ///             --ScalarPropertyMap
    /// This class represents the metadata for all the Type map elements in the 
    /// above example namely EntityTypeMapping, AssociationTypeMapping and CompositionTypeMapping.
    /// The TypeMapping elements contain TableMappingFragments which in turn contain the property maps.
    /// </example>
    internal abstract class StorageTypeMapping
    {
        #region Constructors

        /// <summary>
        /// Construct the new StorageTypeMapping object.
        /// </summary>
        /// <param name="setMapping">SetMapping that contains this type mapping </param>
        internal StorageTypeMapping(StorageSetMapping setMapping)
        {
            m_fragments = new List<StorageMappingFragment>();
            m_setMapping = setMapping;
        }

        #endregion

        #region Fields

        /// <summary>
        /// ExtentMap that contains this type mapping.
        /// </summary>
        private readonly StorageSetMapping m_setMapping;

        /// <summary>
        /// Set of fragments that make up the type Mapping.
        /// </summary>
        private readonly List<StorageMappingFragment> m_fragments;

        #endregion

        #region Properties

        /// <summary>
        /// Mapping fragments that make up this set type
        /// </summary>
        internal ReadOnlyCollection<StorageMappingFragment> MappingFragments
        {
            get { return m_fragments.AsReadOnly(); }
        }

        internal StorageSetMapping SetMapping
        {
            get { return m_setMapping; }
        }

        /// <summary>
        /// a list of TypeMetadata that this mapping holds true for.
        /// </summary>
        internal abstract ReadOnlyCollection<EdmType> Types { get; }

        /// <summary>
        /// a list of TypeMetadatas for which the mapping holds true for
        /// not only the type specified but the sub-types of that type as well.        
        /// </summary>
        internal abstract ReadOnlyCollection<EdmType> IsOfTypes { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Add a fragment mapping as child of this type mapping
        /// </summary>
        /// <param name="fragment"></param>
        internal void AddFragment(StorageMappingFragment fragment)
        {
            m_fragments.Add(fragment);
        }

#if DEBUG
    /// <summary>
    /// This method is primarily for debugging purposes.
    /// Will be removed shortly.
    /// </summary>
    /// <param name="index"></param>
        internal abstract void Print(int index);
#endif

        #endregion
    }
}
