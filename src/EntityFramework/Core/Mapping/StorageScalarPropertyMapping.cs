// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    ///     Mapping metadata for scalar properties.
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
    ///     --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --AssociationSetMapping
    ///     --AssociationTypeMapping
    ///     --MappingFragment
    ///     --EndPropertyMap
    ///     --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --EndPropertyMap
    ///     --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///     This class represents the metadata for all the scalar property map elements in the
    ///     above example.
    /// </example>
    public class StorageScalarPropertyMapping : StoragePropertyMapping
    {
        /// <summary>
        ///     Construct a new Scalar EdmProperty mapping object
        /// </summary>
        /// <param name="member"> </param>
        /// <param name="columnMember"> </param>
        internal StorageScalarPropertyMapping(EdmProperty member, EdmProperty columnMember)
            : base(member)
        {
            DebugCheck.NotNull(columnMember);
            Debug.Assert(
                Helper.IsScalarType(member.TypeUsage.EdmType),
                "StorageScalarPropertyMapping must only map primitive or enum types");
            Debug.Assert(
                Helper.IsPrimitiveType(columnMember.TypeUsage.EdmType), "StorageScalarPropertyMapping must only map primitive types");

            m_columnMember = columnMember;
        }

        /// <summary>
        ///     S-side member for which the scalar property is being mapped.
        ///     This will be interpreted by the view generation algorithm based on the context.
        /// </summary>
        private EdmProperty m_columnMember;

        /// <summary>
        ///     column name from which the sclar property is being mapped
        /// </summary>
        public EdmProperty ColumnProperty
        {
            get { return m_columnMember; }
            internal set
            {
                DebugCheck.NotNull(value);

                m_columnMember = value;
            }
        }
    }
}
