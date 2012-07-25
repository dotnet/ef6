// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Text;

    /// <summary>
    /// Mapping metadata for Complex properties.
    /// </summary>
    /// <example>
    /// For Example if conceptually you could represent the CS MSL file as following
    /// --Mapping 
    ///   --EntityContainerMapping ( CNorthwind-->SNorthwind )
    ///     --EntitySetMapping
    ///       --EntityTypeMapping
    ///         --MappingFragment
    ///           --EntityKey
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///           --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///       --EntityTypeMapping
    ///         --MappingFragment
    ///           --EntityKey
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///           --ComplexPropertyMap
    ///             --ComplexTypeMapping
    ///               --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///               --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    ///               --DiscriminatorProperyMap ( constant value-->SMemberMetadata )
    ///             --ComplexTypeMapping
    ///               --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///               --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    ///               --DiscriminatorProperyMap ( constant value-->SMemberMetadata )
    ///           --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --AssociationSetMapping 
    ///       --AssociationTypeMapping
    ///         --MappingFragment
    ///           --EndPropertyMap
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///             --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    ///           --EndPropertyMap
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// This class represents the metadata for all the complex property map elements in the 
    /// above example. ComplexPropertyMaps contain ComplexTypeMaps which define mapping based 
    /// on the type of the ComplexProperty in case of inheritance.
    /// </example>
    internal class StorageComplexPropertyMapping : StoragePropertyMapping
    {
        #region Constructors

        /// <summary>
        /// Construct a new Complex Property mapping object
        /// </summary>
        /// <param name="cdmMember">The MemberMetadata object that represents this Complex member</param>
        internal StorageComplexPropertyMapping(EdmProperty cdmMember)
            : base(cdmMember)
        {
            m_typeMappings = new List<StorageComplexTypeMapping>();
        }

        #endregion

        #region Fields

        /// <summary>
        /// Set of type mappings that make up the EdmProperty mapping.
        /// </summary>
        private readonly List<StorageComplexTypeMapping> m_typeMappings;

        #endregion

        #region Properties

        ///// <summary>
        ///// The property Metadata object for which the mapping is represented.
        ///// </summary>
        //internal EdmProperty ComplexProperty {
        //    get {
        //        return this.EdmProperty;
        //    }
        //}

        /// <summary>
        /// TypeMappings that make up this property.
        /// </summary>
        internal ReadOnlyCollection<StorageComplexTypeMapping> TypeMappings
        {
            get { return m_typeMappings.AsReadOnly(); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add type mapping as a child under this Property Mapping
        /// </summary>
        /// <param name="typeMapping"></param>
        internal void AddTypeMapping(StorageComplexTypeMapping typeMapping)
        {
            m_typeMappings.Add(typeMapping);
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
            sb.Append("ComplexPropertyMapping");
            sb.Append("   ");
            if (EdmProperty != null)
            {
                sb.Append("Name:");
                sb.Append(EdmProperty.Name);
                sb.Append("   ");
            }
            Console.WriteLine(sb.ToString());
            foreach (var typeMapping in TypeMappings)
            {
                typeMapping.Print(index + 5);
            }
        }
#endif

        #endregion
    }
}
