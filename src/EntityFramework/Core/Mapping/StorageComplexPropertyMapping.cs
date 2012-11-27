// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    ///     Mapping metadata for Complex properties.
    /// </summary>
    /// <example>
    ///     For Example if conceptually you could represent the CS MSL file as following
    ///     --Mapping
    ///     --EntityContainerMapping ( CNorthwind-->SNorthwind )
    ///     --EntitySetMapping
    ///     --EntityTypeMapping
    ///     --MappingFragment
    ///     --EntityKey
    ///     --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --EntityTypeMapping
    ///     --MappingFragment
    ///     --EntityKey
    ///     --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --ComplexPropertyMap
    ///     --ComplexTypeMapping
    ///     --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --DiscriminatorProperyMap ( constant value-->SMemberMetadata )
    ///     --ComplexTypeMapping
    ///     --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --DiscriminatorProperyMap ( constant value-->SMemberMetadata )
    ///     --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --AssociationSetMapping
    ///     --AssociationTypeMapping
    ///     --MappingFragment
    ///     --EndPropertyMap
    ///     --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --EndPropertyMap
    ///     --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///     This class represents the metadata for all the complex property map elements in the
    ///     above example. ComplexPropertyMaps contain ComplexTypeMaps which define mapping based
    ///     on the type of the ComplexProperty in case of inheritance.
    /// </example>
    internal class StorageComplexPropertyMapping : StoragePropertyMapping
    {
        /// <summary>
        ///     Construct a new Complex Property mapping object
        /// </summary>
        /// <param name="cdmMember"> The MemberMetadata object that represents this Complex member </param>
        internal StorageComplexPropertyMapping(EdmProperty cdmMember)
            : base(cdmMember)
        {
            m_typeMappings = new List<StorageComplexTypeMapping>();
        }

        /// <summary>
        ///     Set of type mappings that make up the EdmProperty mapping.
        /// </summary>
        private readonly List<StorageComplexTypeMapping> m_typeMappings;

        /// <summary>
        ///     TypeMappings that make up this property.
        /// </summary>
        internal ReadOnlyCollection<StorageComplexTypeMapping> TypeMappings
        {
            get { return m_typeMappings.AsReadOnly(); }
        }

        /// <summary>
        ///     Add type mapping as a child under this Property Mapping
        /// </summary>
        /// <param name="typeMapping"> </param>
        internal void AddTypeMapping(StorageComplexTypeMapping typeMapping)
        {
            m_typeMappings.Add(typeMapping);
        }
    }
}
