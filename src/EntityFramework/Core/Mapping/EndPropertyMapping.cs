// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
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
    public class EndPropertyMapping : PropertyMapping
    {
        private AssociationEndMember _associationEnd;

        /// <summary>
        /// Creates an association end property mapping.
        /// </summary>
        /// <param name="associationEnd">An AssociationEndMember that specifies 
        /// the association end to be mapped.</param>
        public EndPropertyMapping(AssociationEndMember associationEnd)
        {
            Check.NotNull(associationEnd, "associationEnd");

            _associationEnd = associationEnd;
        }

        internal EndPropertyMapping()
        {
        }

        /// <summary>
        /// List of property mappings that make up the End.
        /// </summary>
        private readonly List<ScalarPropertyMapping> m_properties = new List<ScalarPropertyMapping>();

        /// <summary>
        /// Gets a ReadOnlyCollection of ScalarPropertyMapping that specifies the children 
        /// of this association end property mapping.
        /// </summary>
        public ReadOnlyCollection<ScalarPropertyMapping> Properties
        {
            get { return new ReadOnlyCollection<ScalarPropertyMapping>(m_properties); }
        }

        internal IEnumerable<ScalarPropertyMapping> PropertyMappings
        {
            get { return m_properties; }
        }

        /// <summary>
        /// Gets an AssociationEndMember that specifies the mapped association end.
        /// </summary>
        public AssociationEndMember AssociationEnd
        {
            get { return _associationEnd; }

            internal set
            {
                DebugCheck.NotNull(value);
                Debug.Assert(!IsReadOnly);

                _associationEnd = value;
            }
        }

        /// <summary>
        /// The relation end property Metadata object for which the mapping is represented.
        /// </summary>
        internal RelationshipEndMember EndMember
        {
            get { return AssociationEnd; }
            set { AssociationEnd = (AssociationEndMember)value; }
        }

        /// <summary>
        /// Returns all store properties that are mapped under this mapping fragment
        /// </summary>
        internal IEnumerable<EdmMember> StoreProperties
        {
            get { return PropertyMappings.Select(propertyMap => propertyMap.ColumnProperty); }
        }

        /// <summary>
        /// Adds a child property-column mapping.
        /// </summary>
        /// <param name="propertyMapping">A ScalarPropertyMapping that specifies
        /// the property-column mapping to be added.</param>
        public void AddProperty(ScalarPropertyMapping propertyMapping)
        {
            Check.NotNull(propertyMapping, "propertyMapping");
            ThrowIfReadOnly();

            m_properties.Add(propertyMapping);
        }

        /// <summary>
        /// Removes a child property-column mapping.
        /// </summary>
        /// <param name="propertyMapping">A ScalarPropertyMapping that specifies
        /// the property-column mapping to be removed.</param>
        public void RemoveProperty(ScalarPropertyMapping propertyMapping)
        {
            Check.NotNull(propertyMapping, "propertyMapping");
            ThrowIfReadOnly();

            m_properties.Remove(propertyMapping);
        }
    }
}
