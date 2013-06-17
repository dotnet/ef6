// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

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
    internal class StorageScalarPropertyMapping : StoragePropertyMapping
    {
        /// <summary>
        ///     Construct a new Scalar EdmProperty mapping object
        /// </summary>
        public StorageScalarPropertyMapping(EdmProperty member, EdmProperty columnMember)
            : base(member)
        {
            Check.NotNull(member, "member");
            Check.NotNull(columnMember, "columnMember");

            if (!Helper.IsScalarType(member.TypeUsage.EdmType)
                || !Helper.IsPrimitiveType(columnMember.TypeUsage.EdmType))
            {
                throw new ArgumentException(Strings.StorageScalarPropertyMapping_OnlyScalarPropertiesAllowed);
            }

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
