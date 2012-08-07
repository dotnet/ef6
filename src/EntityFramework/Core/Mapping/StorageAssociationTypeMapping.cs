// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Text;

    /// <summary>
    ///     Represents the Mapping metadata for an association type map in CS space.
    /// </summary>
    /// <example>
    ///     For Example if conceptually you could represent the CS MSL file as following
    ///     --Mapping 
    ///     --EntityContainerMapping ( CNorthwind-->SNorthwind )
    ///     --EntitySetMapping
    ///     --EntityTypeMapping
    ///     --MappingFragment
    ///     --EntityKey
    ///     --ScalarPropertyMap
    ///     --ScalarPropertyMap
    ///     --EntityTypeMapping
    ///     --MappingFragment
    ///     --EntityKey
    ///     --ScalarPropertyMap
    ///     --ComplexPropertyMap
    ///     --ComplexTypeMap
    ///     --ScalarPropertyMap
    ///     --ScalarProperyMap
    ///     --ScalarPropertyMap
    ///     --AssociationSetMapping 
    ///     --AssociationTypeMapping
    ///     --MappingFragment
    ///     --EndPropertyMap
    ///     --ScalarPropertyMap
    ///     --ScalarProperyMap
    ///     --EndPropertyMap
    ///     --ScalarPropertyMap
    ///     This class represents the metadata for all association Type map elements in the 
    ///     above example. Users can access the table mapping fragments under the 
    ///     association type mapping through this class.
    /// </example>
    internal class StorageAssociationTypeMapping : StorageTypeMapping
    {
        #region Constructors

        /// <summary>
        ///     Construct the new AssociationTypeMapping object.
        /// </summary>
        /// <param name="relation"> Represents the Association Type metadata object </param>
        /// <param name="setMapping"> Set Mapping that contains this Type mapping </param>
        internal StorageAssociationTypeMapping(AssociationType relation, StorageSetMapping setMapping)
            : base(setMapping)
        {
            m_relation = relation;
        }

        #endregion

        #region Fields

        /// <summary>
        ///     Type for which the mapping is represented.
        /// </summary>
        private readonly AssociationType m_relation;

        #endregion

        #region Properties

        /// <summary>
        ///     The AssociationTypeType Metadata object for which the mapping is represented.
        /// </summary>
        internal AssociationType AssociationType
        {
            get { return m_relation; }
        }

        /// <summary>
        ///     a list of TypeMetadata that this mapping holds true for.
        ///     Since Association types dont participate in Inheritance, This can only
        ///     be one type.
        /// </summary>
        internal override ReadOnlyCollection<EdmType> Types
        {
            get { return new ReadOnlyCollection<EdmType>(new[] { m_relation }); }
        }

        /// <summary>
        ///     a list of TypeMetadatas for which the mapping holds true for
        ///     not only the type specified but the sub-types of that type as well.
        ///     Since Association types dont participate in Inheritance, an Empty list 
        ///     is returned here.
        /// </summary>
        internal override ReadOnlyCollection<EdmType> IsOfTypes
        {
            get { return new List<EdmType>().AsReadOnly(); }
        }

        #endregion

        #region Methods

#if DEBUG
        /// <summary>
        ///     This method is primarily for debugging purposes.
        ///     Will be removed shortly.
        /// </summary>
        /// <param name="index"> </param>
        internal override void Print(int index)
        {
            StorageEntityContainerMapping.GetPrettyPrintString(ref index);
            var sb = new StringBuilder();
            sb.Append("AssociationTypeMapping");
            sb.Append("   ");
            sb.Append("Type Name:");
            sb.Append(m_relation.Name);
            sb.Append("   ");
            Console.WriteLine(sb.ToString());
            foreach (var fragment in MappingFragments)
            {
                fragment.Print(index + 5);
            }
        }
#endif

        #endregion
    }
}
