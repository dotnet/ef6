// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    ///     Mapping metadata for Conditional property mapping on a type.
    ///     Condition Property Mapping specifies a Condition either on the C side property or S side property.
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
    ///     --ConditionProperyMap ( constant value-->SMemberMetadata )
    ///     --EntityTypeMapping
    ///     --MappingFragment
    ///     --EntityKey
    ///     --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --ComplexPropertyMap
    ///     --ComplexTypeMap
    ///     --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --ConditionProperyMap ( constant value-->SMemberMetadata )
    ///     --AssociationSetMapping
    ///     --AssociationTypeMapping
    ///     --MappingFragment
    ///     --EndPropertyMap
    ///     --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --EndPropertyMap
    ///     --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///     This class represents the metadata for all the condition property map elements in the
    ///     above example.
    /// </example>
    internal class StorageConditionPropertyMapping : StoragePropertyMapping
    {
        /// <summary>
        ///     Construct a new condition Property mapping object
        /// </summary>
        public StorageConditionPropertyMapping(
            EdmProperty edmProperty, EdmProperty columnMember
            , object value, bool? isNull)
            : base(edmProperty)
        {
            Debug.Assert(
                (edmProperty != null) || (columnMember != null), "Both CDM and Column Members can not be specified for Condition Mapping");
            Debug.Assert(
                (edmProperty == null) || (columnMember == null), "Either CDM or Column Members has to be specified for Condition Mapping");
            Debug.Assert((isNull.HasValue) || (value != null), "Both Value and IsNull can not be specified on Condition Mapping");
            Debug.Assert(!(isNull.HasValue) || (value == null), "Either Value or IsNull has to be specified on Condition Mapping");
            m_columnMember = columnMember;
            m_value = value;
            m_isNull = isNull;
        }

        /// <summary>
        ///     Column EdmMember for which the condition is specified.
        /// </summary>
        private EdmProperty m_columnMember;

        /// <summary>
        ///     Value for the condition thats being mapped.
        /// </summary>
        private readonly object m_value;

        private readonly bool? m_isNull;

        /// <summary>
        ///     Value for the condition
        /// </summary>
        public object Value
        {
            get { return m_value; }
        }

        /// <summary>
        ///     Whether the property is being mapped to Null or NotNull
        /// </summary>
        public bool? IsNull
        {
            get { return m_isNull; }
        }

        /// <summary>
        ///     ColumnMember for which the Condition Map is being specified
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
