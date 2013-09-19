// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;

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
    internal class StorageAssociationTypeMapping : StorageTypeMapping
    {
        /// <summary>
        /// Construct the new AssociationTypeMapping object.
        /// </summary>
        /// <param name="relation"> Represents the Association Type metadata object </param>
        /// <param name="setMapping"> Set Mapping that contains this Type mapping </param>
        public StorageAssociationTypeMapping(AssociationType relation, StorageSetMapping setMapping)
            : base(setMapping)
        {
            m_relation = relation;
        }

        /// <summary>
        /// Type for which the mapping is represented.
        /// </summary>
        private readonly AssociationType m_relation;

        /// <summary>
        /// The AssociationTypeType Metadata object for which the mapping is represented.
        /// </summary>
        public AssociationType AssociationType
        {
            get { return m_relation; }
        }

        /// <summary>
        /// a list of TypeMetadata that this mapping holds true for.
        /// Since Association types dont participate in Inheritance, This can only
        /// be one type.
        /// </summary>
        public override ReadOnlyCollection<EdmType> Types
        {
            get { return new ReadOnlyCollection<EdmType>(new[] { m_relation }); }
        }

        /// <summary>
        /// a list of TypeMetadatas for which the mapping holds true for
        /// not only the type specified but the sub-types of that type as well.
        /// Since Association types dont participate in Inheritance, an Empty list
        /// is returned here.
        /// </summary>
        public override ReadOnlyCollection<EdmType> IsOfTypes
        {
            get { return new List<EdmType>().AsReadOnly(); }
        }
    }
}
