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

        // <summary>
        // List of property mappings that make up the End.
        // </summary>
        private readonly List<ScalarPropertyMapping> _properties = new List<ScalarPropertyMapping>();

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
        /// Gets a ReadOnlyCollection of ScalarPropertyMapping that specifies the children 
        /// of this association end property mapping.
        /// </summary>
        public ReadOnlyCollection<ScalarPropertyMapping> Properties
        {
            get { return new ReadOnlyCollection<ScalarPropertyMapping>(_properties); }
        }

        // <summary>
        // Returns all store properties that are mapped under this mapping fragment
        // </summary>
        internal IEnumerable<EdmMember> StoreProperties
        {
            get { return Properties.Select(propertyMap => propertyMap.Column); }
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

            _properties.Add(propertyMapping);
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

            _properties.Remove(propertyMapping);
        }

        internal override void SetReadOnly()
        {
            _properties.TrimExcess();

            SetReadOnly(_properties);

            base.SetReadOnly();
        }
    }
}
