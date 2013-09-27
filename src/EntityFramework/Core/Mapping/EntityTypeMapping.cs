// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Mapping metadata for Entity type.
    /// If an EntitySet represents entities of more than one type, than we will have
    /// more than one EntityTypeMapping for an EntitySet( For ex : if
    /// PersonSet Entity extent represents entities of types Person and Customer,
    /// than we will have two EntityType Mappings under mapping for PersonSet).
    /// </summary>
    /// <example>
    /// For Example if conceptually you could represent the CS MSL file as following
    /// --Mapping
    /// --EntityContainerMapping ( CNorthwind-->SNorthwind )
    /// --EntitySetMapping
    /// --EntityTypeMapping
    /// --MappingFragment
    /// --EntityKey
    /// --ScalarPropertyMap
    /// --ScalarPropertyMap
    /// --EntityTypeMapping
    /// --MappingFragment
    /// --EntityKey
    /// --ScalarPropertyMap
    /// --ComplexPropertyMap
    /// --ScalarPropertyMap
    /// --ScalarProperyMap
    /// --ScalarPropertyMap
    /// --AssociationSetMapping
    /// --AssociationTypeMapping
    /// --MappingFragment
    /// --EndPropertyMap
    /// --ScalarPropertyMap
    /// --ScalarProperyMap
    /// --EndPropertyMap
    /// --ScalarPropertyMap
    /// This class represents the metadata for all entity Type map elements in the
    /// above example. Users can access the table mapping fragments under the
    /// entity type mapping through this class.
    /// </example>
    public class EntityTypeMapping : TypeMapping
    {
        private readonly EntitySetMapping _entitySetMapping;
        private readonly List<MappingFragment> _fragments;

        /// <summary>
        /// Creates an EntityTypeMapping instance.
        /// </summary>
        /// <param name="entitySetMapping">The EntitySetMapping that contains this EntityTypeMapping.</param>
        public EntityTypeMapping(EntitySetMapping entitySetMapping)
        {
            _entitySetMapping = entitySetMapping;
            _fragments = new List<MappingFragment>();
        }

        // <summary>
        // Types for which the mapping holds true for.
        // </summary>
        private readonly Dictionary<string, EntityType> m_entityTypes = new Dictionary<string, EntityType>(StringComparer.Ordinal);

        // <summary>
        // Types for which the mapping holds true for not only the type specified but the sub-types of that type as well.
        // </summary>
        private readonly Dictionary<string, EntityType> m_isOfEntityTypes = new Dictionary<string, EntityType>(StringComparer.Ordinal);

        /// <summary>
        /// Gets the EntitySetMapping that contains this EntityTypeMapping.
        /// </summary>
        public EntitySetMapping EntitySetMapping
        {
            get { return _entitySetMapping; }
        }

        internal override EntitySetBaseMapping SetMapping
        {
            get { return EntitySetMapping; }
        }

        /// <summary>
        /// Gets the single EntityType being mapped. Throws exception in case of hierarchy type mapping.
        /// </summary>
        public EntityType EntityType
        {
            get { return m_entityTypes.Values.SingleOrDefault(); }
        }

        /// <summary>
        /// Gets a flag that indicates whether this is a type hierarchy mapping.
        /// </summary>
        public bool IsHierarchyMapping
        {
            get { return m_isOfEntityTypes.Count > 0 || m_entityTypes.Count > 1; }
        }

        /// <summary>
        /// Gets a read-only collection of mapping fragments.
        /// </summary>
        public ReadOnlyCollection<MappingFragment> Fragments
        {
            get { return new ReadOnlyCollection<MappingFragment>(_fragments); }
        }

        internal override ReadOnlyCollection<MappingFragment> MappingFragments
        {
            get { return Fragments; }
        }

        /// <summary>
        /// Gets the mapped entity types.
        /// </summary>
        public ReadOnlyCollection<EntityTypeBase> EntityTypes
        {
            get { return new ReadOnlyCollection<EntityTypeBase>(new List<EntityTypeBase>(m_entityTypes.Values)); }
        }

        // <summary>
        // a list of TypeMetadata that this mapping holds true for.
        // </summary>
        internal override ReadOnlyCollection<EntityTypeBase> Types
        {
            get { return EntityTypes; }
        }

        /// <summary>
        /// Gets the mapped base types for a hierarchy mapping.
        /// </summary>
        public ReadOnlyCollection<EntityTypeBase> IsOfEntityTypes
        {
            get { return new ReadOnlyCollection<EntityTypeBase>(new List<EntityTypeBase>(m_isOfEntityTypes.Values)); }
        }

        // <summary>
        // a list of TypeMetadatas for which the mapping holds true for
        // not only the type specified but the sub-types of that type as well.
        // </summary>
        internal override ReadOnlyCollection<EntityTypeBase> IsOfTypes
        {
            get { return IsOfEntityTypes; }
        }

        /// <summary>
        /// Adds an entity type to the mapping.
        /// </summary>
        /// <param name="type">The EntityType to be added.</param>
        public void AddType(EntityType type)
        {
            Check.NotNull(type, "type");
            ThrowIfReadOnly();

            m_entityTypes.Add(type.FullName, type);
        }

        /// <summary>
        /// Removes an entity type from the mapping.
        /// </summary>
        /// <param name="type">The EntityType to be removed.</param>
        public void RemoveType(EntityType type)
        {
            Check.NotNull(type, "type");
            ThrowIfReadOnly();

            m_entityTypes.Remove(type.FullName);
        }

        /// <summary>
        /// Adds an entity type hierarchy to the mapping.
        /// The hierarchy is represented by the specified root entity type.
        /// </summary>
        /// <param name="type">The root EntityType of the hierarchy to be added.</param>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "AddIs")]
        public void AddIsOfType(EntityType type)
        {
            Check.NotNull(type, "type");
            ThrowIfReadOnly();

            m_isOfEntityTypes.Add(type.FullName, type);
        }

        /// <summary>
        /// Removes an entity type hierarchy from the mapping.
        /// The hierarchy is represented by the specified root entity type.
        /// </summary>
        /// <param name="type">The root EntityType of the hierarchy to be removed.</param>
        public void RemoveIsOfType(EntityType type)
        {
            Check.NotNull(type, "type");
            ThrowIfReadOnly();

            m_isOfEntityTypes.Remove(type.FullName);
        }

        /// <summary>
        /// Adds a mapping fragment.
        /// </summary>
        /// <param name="fragment">The mapping fragment to be added.</param>
        public void AddFragment(MappingFragment fragment)
        {
            Check.NotNull(fragment, "fragment");
            ThrowIfReadOnly();

            _fragments.Add(fragment);
        }

        /// <summary>
        /// Removes a mapping fragment.
        /// </summary>
        /// <param name="fragment">The mapping fragment to be removed.</param>
        public void RemoveFragment(MappingFragment fragment)
        {
            Check.NotNull(fragment, "fragment");
            ThrowIfReadOnly();

            _fragments.Remove(fragment);
        }

        internal EntityType GetContainerType(string memberName)
        {
            foreach (var type in m_entityTypes.Values)
            {
                if (type.Properties.Contains(memberName))
                {
                    return type;
                }
            }

            foreach (var type in m_isOfEntityTypes.Values)
            {
                if (type.Properties.Contains(memberName))
                {
                    return type;
                }
            }
            return null;
        }
    }
}
