// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;

    /// <summary>
    /// Mapping metadata for End property of an association.
    /// </summary>
    /// <example>
    /// For Example if conceptually you could represent the CS MSL file as following
    /// --Mapping
    /// --EntityContainerMapping ( CNorthwind-->SNorthwind )
    /// --EntitySetMapping
    /// --EntityTypeMapping
    /// --MappingFragment
    /// --EntityKey
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --EntityTypeMapping
    /// --MappingFragment
    /// --EntityKey
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ComplexPropertyMap
    /// --ComplexTypeMapping
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    /// --DiscriminatorProperyMap ( constant value-->SMemberMetadata )
    /// --ComplexTypeMapping
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    /// --DiscriminatorProperyMap ( constant value-->SMemberMetadata )
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --AssociationSetMapping
    /// --AssociationTypeMapping
    /// --MappingFragment
    /// --EndPropertyMap
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    /// --EndPropertyMap
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// This class represents the metadata for all the end property map elements in the
    /// above example. EndPropertyMaps provide mapping for each end of the association.
    /// </example>
    internal class StorageEndPropertyMapping : StoragePropertyMapping
    {
        /// <summary>
        /// Construct a new End Property mapping object
        /// </summary>
        public StorageEndPropertyMapping()
            : base(null)
        {
        }

        /// <summary>
        /// List of property mappings that make up the End.
        /// </summary>
        private readonly List<StoragePropertyMapping> m_properties = new List<StoragePropertyMapping>();

        /// <summary>
        /// return ReadOnlyCollection of property mappings that are children of this End mapping
        /// </summary>
        public ReadOnlyCollection<StoragePropertyMapping> Properties
        {
            get { return m_properties.AsReadOnly(); }
        }

        public IEnumerable<StorageScalarPropertyMapping> PropertyMappings
        {
            get { return m_properties.OfType<StorageScalarPropertyMapping>(); }
        }

        /// <summary>
        /// The relation end property Metadata object for which the mapping is represented.
        /// </summary>
        public RelationshipEndMember EndMember { get; set; }

        /// <summary>
        /// Returns all store properties that are mapped under this mapping fragment
        /// </summary>
        internal IEnumerable<EdmMember> StoreProperties
        {
            get { return PropertyMappings.Select(propertyMap => propertyMap.ColumnProperty); }
        }

        /// <summary>
        /// Add a property mapping as a child of End property mapping
        /// </summary>
        public void AddProperty(StoragePropertyMapping prop)
        {
            m_properties.Add(prop);
        }
    }
}
