// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    /// Represents the Mapping metadata for an association type map in CS space.
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
    /// --ComplexTypeMap
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
    /// This class represents the metadata for all association Type map elements in the
    /// above example. Users can access the table mapping fragments under the
    /// association type mapping through this class.
    /// </example>
    public class AssociationTypeMapping : TypeMapping
    {
        private readonly AssociationSetMapping _associationSetMapping;
        private MappingFragment _mappingFragment;

        /// <summary>
        /// Creates an AssociationTypeMapping instance.
        /// </summary>
        /// <param name="associationSetMapping">The AssociationSetMapping that 
        /// the contains this AssociationTypeMapping.</param>
        public AssociationTypeMapping(AssociationSetMapping associationSetMapping)
        {
            Check.NotNull(associationSetMapping, "associationSetMapping");

            _associationSetMapping = associationSetMapping;
            m_relation = associationSetMapping.AssociationSet.ElementType;
        }

        // <summary>
        // Construct the new AssociationTypeMapping object.
        // </summary>
        // <param name="relation"> Represents the Association Type metadata object </param>
        // <param name="associationSetMapping"> Set Mapping that contains this Type mapping </param>
        internal AssociationTypeMapping(AssociationType relation, AssociationSetMapping associationSetMapping)
        {
            _associationSetMapping = associationSetMapping;
            m_relation = relation;
        }

        // <summary>
        // Type for which the mapping is represented.
        // </summary>
        private readonly AssociationType m_relation;

        /// <summary>
        /// Gets the AssociationSetMapping that contains this AssociationTypeMapping.
        /// </summary>
        public AssociationSetMapping AssociationSetMapping
        {
            get { return _associationSetMapping; }
        }

        internal override EntitySetBaseMapping SetMapping
        {
            get { return AssociationSetMapping;  }
        }

        /// <summary>
        /// Gets the association type being mapped.
        /// </summary>
        public AssociationType AssociationType
        {
            get { return m_relation; }
        }

        /// <summary>
        /// Gets the single mapping fragment.
        /// </summary>
        public MappingFragment MappingFragment
        {
            get { return _mappingFragment;  }

            internal set
            {
                DebugCheck.NotNull(value);
                Debug.Assert(_mappingFragment == null);
                Debug.Assert(!IsReadOnly);

                _mappingFragment = value;                  
            }
        }

        internal override ReadOnlyCollection<MappingFragment> MappingFragments
        {
            get { return new ReadOnlyCollection<MappingFragment>(new[] { _mappingFragment }); }
        }

        // <summary>
        // a list of TypeMetadata that this mapping holds true for.
        // Since Association types dont participate in Inheritance, This can only
        // be one type.
        // </summary>
        internal override ReadOnlyCollection<EntityTypeBase> Types
        {
            get { return new ReadOnlyCollection<EntityTypeBase>(new[] { m_relation }); }
        }

        // <summary>
        // a list of TypeMetadatas for which the mapping holds true for
        // not only the type specified but the sub-types of that type as well.
        // Since Association types dont participate in Inheritance, an Empty list
        // is returned here.
        // </summary>
        internal override ReadOnlyCollection<EntityTypeBase> IsOfTypes
        {
            get { return new ReadOnlyCollection<EntityTypeBase>(new List<EntityTypeBase>()); }
        }
    }
}
