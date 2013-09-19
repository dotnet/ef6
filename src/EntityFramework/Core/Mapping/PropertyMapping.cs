// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    /// Mapping metadata for all types of property mappings.
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
    /// This class represents the metadata for all property map elements in the
    /// above example. This includes the scalar property maps, complex property maps
    /// and end property maps.
    /// </example>
    public abstract class PropertyMapping : MappingItem
    {
        internal PropertyMapping()
        {
        }

        /// <summary>
        /// Construct a new EdmProperty mapping object
        /// </summary>
        /// <param name="cdmMember"> The PropertyMetadata object that represents the member for which mapping is being specified </param>
        internal PropertyMapping(EdmProperty cdmMember)
        {
            m_cdmMember = cdmMember;
        }

        /// <summary>
        /// EdmProperty metadata representing the Cdm member for which the mapping is specified.
        /// </summary>
        private EdmProperty m_cdmMember;

        /// <summary>
        /// The PropertyMetadata object that represents the member for which mapping is being specified
        /// </summary>
        internal EdmProperty EdmProperty
        {
            get { return Property; }
        }

        /// <summary>
        /// Gets an EdmProperty that specifies the mapped property.
        /// </summary>
        public EdmProperty Property
        {
            get { return m_cdmMember; }

            internal set
            {
                DebugCheck.NotNull(value);
                Debug.Assert(!IsReadOnly);

                m_cdmMember = value;
            }
        }
    }
}
