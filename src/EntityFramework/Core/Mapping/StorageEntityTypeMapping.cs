// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Text;

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
    ///   --EntityContainerMapping ( CNorthwind-->SNorthwind )
    ///     --EntitySetMapping
    ///       --EntityTypeMapping
    ///         --MappingFragment
    ///           --EntityKey
    ///             --ScalarPropertyMap
    ///           --ScalarPropertyMap
    ///       --EntityTypeMapping
    ///         --MappingFragment
    ///           --EntityKey
    ///             --ScalarPropertyMap
    ///           --ComplexPropertyMap
    ///             --ScalarPropertyMap
    ///             --ScalarProperyMap
    ///           --ScalarPropertyMap
    ///     --AssociationSetMapping 
    ///       --AssociationTypeMapping
    ///         --MappingFragment
    ///           --EndPropertyMap
    ///             --ScalarPropertyMap
    ///             --ScalarProperyMap
    ///           --EndPropertyMap
    ///             --ScalarPropertyMap
    /// This class represents the metadata for all entity Type map elements in the 
    /// above example. Users can access the table mapping fragments under the 
    /// entity type mapping through this class.
    /// </example>
    internal class StorageEntityTypeMapping : StorageTypeMapping
    {
        #region Constructors

        /// <summary>
        /// Construct the new EntityTypeMapping object.
        /// </summary>
        /// <param name="setMapping">Set Mapping that contains this Type mapping </param>
        internal StorageEntityTypeMapping(StorageSetMapping setMapping)
            : base(setMapping)
        {
        }

        #endregion

        #region Fields

        /// <summary>
        /// Types for which the mapping holds true for.
        /// </summary>
        private readonly Dictionary<string, EdmType> m_entityTypes = new Dictionary<string, EdmType>(StringComparer.Ordinal);

        /// <summary>
        /// Types for which the mapping holds true for not only the type specified but the sub-types of that type as well.
        /// </summary>
        private readonly Dictionary<string, EdmType> m_isOfEntityTypes = new Dictionary<string, EdmType>(StringComparer.Ordinal);

        #endregion

        #region Properties

        /// <summary>
        /// a list of TypeMetadata that this mapping holds true for.
        /// </summary>
        internal override ReadOnlyCollection<EdmType> Types
        {
            get { return new List<EdmType>(m_entityTypes.Values).AsReadOnly(); }
        }

        /// <summary>
        /// a list of TypeMetadatas for which the mapping holds true for
        /// not only the type specified but the sub-types of that type as well.        
        /// </summary>
        internal override ReadOnlyCollection<EdmType> IsOfTypes
        {
            get { return new List<EdmType>(m_isOfEntityTypes.Values).AsReadOnly(); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add a Type to the list of types that this mapping is valid for
        /// </summary>
        internal void AddType(EdmType type)
        {
            m_entityTypes.Add(type.FullName, type);
        }

        /// <summary>
        /// Add a Type to the list of Is-Of types that this mapping is valid for
        /// </summary>
        internal void AddIsOfType(EdmType type)
        {
            m_isOfEntityTypes.Add(type.FullName, type);
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

#if DEBUG
    /// <summary>
    /// This method is primarily for debugging purposes.
    /// Will be removed shortly.
    /// </summary>
    /// <param name="index"></param>
        internal override void Print(int index)
        {
            StorageEntityContainerMapping.GetPrettyPrintString(ref index);
            var sb = new StringBuilder();
            sb.Append("EntityTypeMapping");
            sb.Append("   ");
            foreach (var type in m_entityTypes.Values)
            {
                sb.Append("Types:");
                sb.Append(type.FullName);
                sb.Append("   ");
            }
            foreach (var type in m_isOfEntityTypes.Values)
            {
                sb.Append("Is-Of Types:");
                sb.Append(type.FullName);
                sb.Append("   ");
            }

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
