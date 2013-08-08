// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
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
    internal class StorageEntityTypeMapping : StorageTypeMapping
    {
        /// <summary>
        /// Construct the new EntityTypeMapping object.
        /// </summary>
        /// <param name="setMapping"> Set Mapping that contains this Type mapping </param>
        public StorageEntityTypeMapping(StorageSetMapping setMapping)
            : base(setMapping)
        {
        }

        private readonly List<MetadataProperty> _annotationsList = new List<MetadataProperty>();

        internal IList<MetadataProperty> Annotations
        {
            get { return _annotationsList; }
        }

        /// <summary>
        /// Types for which the mapping holds true for.
        /// </summary>
        private readonly Dictionary<string, EdmType> m_entityTypes = new Dictionary<string, EdmType>(StringComparer.Ordinal);

        /// <summary>
        /// Types for which the mapping holds true for not only the type specified but the sub-types of that type as well.
        /// </summary>
        private readonly Dictionary<string, EdmType> m_isOfEntityTypes = new Dictionary<string, EdmType>(StringComparer.Ordinal);

        /// <summary>
        /// a list of TypeMetadata that this mapping holds true for.
        /// </summary>
        public override ReadOnlyCollection<EdmType> Types
        {
            get { return new List<EdmType>(m_entityTypes.Values).AsReadOnly(); }
        }

        public EntityType EntityType
        {
            get { return m_entityTypes.Values.OfType<EntityType>().SingleOrDefault(); }
        }

        public bool IsHierarchyMapping
        {
            get
            {
                return (EntityType != null)
                       && m_isOfEntityTypes.ContainsKey(EntityType.FullName);
            }
        }

        /// <summary>
        /// a list of TypeMetadatas for which the mapping holds true for
        /// not only the type specified but the sub-types of that type as well.
        /// </summary>
        public override ReadOnlyCollection<EdmType> IsOfTypes
        {
            get { return new List<EdmType>(m_isOfEntityTypes.Values).AsReadOnly(); }
        }

        /// <summary>
        /// Add a Type to the list of types that this mapping is valid for
        /// </summary>
        public void AddType(EdmType type)
        {
            Check.NotNull(type, "type");

            m_entityTypes.Add(type.FullName, type);
        }

        /// <summary>
        /// Add a Type to the list of Is-Of types that this mapping is valid for
        /// </summary>
        internal void AddIsOfType(EdmType type)
        {
            DebugCheck.NotNull(type);

            m_isOfEntityTypes.Add(type.FullName, type);
        }

        internal void RemoveIsOfType(EdmType type)
        {
            DebugCheck.NotNull(type);

            m_isOfEntityTypes.Remove(type.FullName);
        }

        internal EntityType GetContainerType(string memberName)
        {
            foreach (EntityType type in m_entityTypes.Values)
            {
                if (type.Properties.Contains(memberName))
                {
                    return type;
                }
            }

            foreach (EntityType type in m_isOfEntityTypes.Values)
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
