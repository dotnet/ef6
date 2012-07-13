namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// Mapping metadata for scalar properties.
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
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///             --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    ///           --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --AssociationSetMapping 
    ///       --AssociationTypeMapping
    ///         --MappingFragment
    ///           --EndPropertyMap
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///             --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    ///           --EndPropertyMap
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// This class represents the metadata for all the scalar property map elements in the 
    /// above example.
    /// </example>
    internal class StorageScalarPropertyMapping : StoragePropertyMapping
    {
        #region Constructors

        /// <summary>
        /// Construct a new Scalar EdmProperty mapping object
        /// </summary>
        /// <param name="member"></param>
        /// <param name="columnMember"></param>
        internal StorageScalarPropertyMapping(EdmProperty member, EdmProperty columnMember)
            : base(member)
        {
            Debug.Assert(columnMember != null);
            Debug.Assert(
                Helper.IsScalarType(member.TypeUsage.EdmType),
                "StorageScalarPropertyMapping must only map primitive or enum types");
            Debug.Assert(
                Helper.IsPrimitiveType(columnMember.TypeUsage.EdmType), "StorageScalarPropertyMapping must only map primitive types");
            m_columnMember = columnMember;
        }

        #endregion

        #region Fields

        /// <summary>
        /// S-side member for which the scalar property is being mapped.
        /// This will be interpreted by the view generation algorithm based on the context.
        /// </summary>
        private readonly EdmProperty m_columnMember;

        #endregion

        #region Properties

        /// <summary>
        /// column name from which the sclar property is being mapped
        /// </summary>
        internal EdmProperty ColumnProperty
        {
            get { return m_columnMember; }
        }

        #endregion

        #region Methods

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
            sb.Append("ScalarPropertyMapping");
            sb.Append("   ");
            if (EdmProperty != null)
            {
                sb.Append("Name:");
                sb.Append(EdmProperty.Name);
                sb.Append("   ");
            }
            if (ColumnProperty != null)
            {
                sb.Append("Column Name:");
                sb.Append(ColumnProperty.Name);
            }
            Console.WriteLine(sb.ToString());
        }
#endif

        #endregion
    }
}
