// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    /// Mapping metadata for Conditional property mapping on a type.
    /// Condition Property Mapping specifies a Condition either on the C side property or S side property.
    /// </summary>
    /// <example>
    /// For Example if conceptually you could represent the CS MSL file as following
    /// --Mapping
    /// --EntityContainerMapping ( CNorthwind-->SNorthwind )
    /// --EntitySetMapping
    /// --EntityTypeMapping
    /// --MappingFragment
    /// --EntityKey
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ConditionProperyMap ( constant value-->SMemberMetadata )
    /// --EntityTypeMapping
    /// --MappingFragment
    /// --EntityKey
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ComplexPropertyMap
    /// --ComplexTypeMap
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ConditionProperyMap ( constant value-->SMemberMetadata )
    /// --AssociationSetMapping
    /// --AssociationTypeMapping
    /// --MappingFragment
    /// --EndPropertyMap
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    /// --EndPropertyMap
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// This class represents the metadata for all the condition property map elements in the
    /// above example.
    /// </example>
    public class ConditionPropertyMapping : PropertyMapping
    {
        // <summary>
        // Column EdmMember for which the condition is specified.
        // </summary>
        private EdmProperty _column;

        // <summary>
        // Value for the condition thats being mapped.
        // </summary>
        private readonly object _value;

        private readonly bool? _isNull;

        internal ConditionPropertyMapping(EdmProperty propertyOrColumn, object value, bool? isNull)
        {
            DebugCheck.NotNull(propertyOrColumn);
            Debug.Assert((isNull.HasValue) || (value != null), "Either Value or IsNull has to be specified on Condition Mapping");
            Debug.Assert(!(isNull.HasValue) || (value == null), "Both Value and IsNull can not be specified on Condition Mapping");

            var dataSpace = propertyOrColumn.TypeUsage.EdmType.DataSpace;

            switch (dataSpace)
            {
                case DataSpace.CSpace:
                    base.Property = propertyOrColumn;
                    break;

                case DataSpace.SSpace:
                    _column = propertyOrColumn;
                    break;

                default:
                    throw new ArgumentException(
                        Strings.MetadataItem_InvalidDataSpace(dataSpace, typeof(EdmProperty).Name),
                        "propertyOrColumn");
            }

            _value = value;
            _isNull = isNull;
        }

        // <summary>
        // Construct a new condition Property mapping object
        // </summary>
        internal ConditionPropertyMapping(
            EdmProperty property, EdmProperty column
            , object value, bool? isNull)
            : base(property)
        {
            Debug.Assert(column == null || column.TypeUsage.EdmType.DataSpace == DataSpace.SSpace);
            Debug.Assert(
                (property != null) || (column != null), "Either CDM or Column Members has to be specified for Condition Mapping");
            Debug.Assert(
                (property == null) || (column == null), "Both CDM and Column Members can not be specified for Condition Mapping");
            Debug.Assert((isNull.HasValue) || (value != null), "Either Value or IsNull has to be specified on Condition Mapping");
            Debug.Assert(!(isNull.HasValue) || (value == null), "Both Value and IsNull can not be specified on Condition Mapping");

            _column = column;
            
            _value = value;
            _isNull = isNull;
        }

        // <summary>
        // Value for the condition
        // </summary>
        internal object Value
        {
            get { return _value; }
        }

        // <summary>
        // Whether the property is being mapped to Null or NotNull
        // </summary>
        internal bool? IsNull
        {
            get { return _isNull; }
        }

        /// <summary>
        /// Gets an EdmProperty that specifies the mapped property.
        /// </summary>
        public override EdmProperty Property
        {
            get { return base.Property; }

            internal set
            {
                Debug.Assert(Column == null);

                base.Property = value;        
            }
        }

        /// <summary>
        /// Gets an EdmProperty that specifies the mapped column.
        /// </summary>
        public EdmProperty Column
        {
            get { return _column; }

            internal set
            {
                Debug.Assert(Property == null);

                DebugCheck.NotNull(value);
                Debug.Assert(value.TypeUsage.EdmType.DataSpace == DataSpace.SSpace);                
                Debug.Assert(!IsReadOnly);

                _column = value;
            }
        }
    }
}
