// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    /// Mapping metadata for scalar properties.
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
    /// --EntityTypeMapping
    /// --MappingFragment
    /// --EntityKey
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ComplexPropertyMap
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --AssociationSetMapping
    /// --AssociationTypeMapping
    /// --MappingFragment
    /// --EndPropertyMap
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --EndPropertyMap
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// This class represents the metadata for all the scalar property map elements in the
    /// above example.
    /// </example>
    public class ScalarPropertyMapping : PropertyMapping
    {
        // <summary>
        // S-side member for which the scalar property is being mapped.
        // This will be interpreted by the view generation algorithm based on the context.
        // </summary>
        private EdmProperty _column;

        /// <summary>
        /// Creates a mapping between a simple property and a column.
        /// </summary>
        /// <param name="property">The property to be mapped.</param>
        /// <param name="column">The column to be mapped.</param>
        public ScalarPropertyMapping(EdmProperty property, EdmProperty column)
            : base(property)
        {
            Check.NotNull(property, "property");
            Check.NotNull(column, "column");

            Debug.Assert(column.TypeUsage.EdmType.DataSpace == DataSpace.SSpace);

            if (!Helper.IsScalarType(property.TypeUsage.EdmType)
                || !Helper.IsPrimitiveType(column.TypeUsage.EdmType))
            {
                throw new ArgumentException(Strings.StorageScalarPropertyMapping_OnlyScalarPropertiesAllowed);
            }

            _column = column;
        }

        /// <summary>
        /// Gets an EdmProperty that specifies the mapped column.
        /// </summary>
        public EdmProperty Column
        {
            get { return _column; }

            internal set
            {
                DebugCheck.NotNull(value);
                Debug.Assert(value.TypeUsage.EdmType.DataSpace == DataSpace.SSpace);
                Debug.Assert(!IsReadOnly);

                _column = value;
            }
        }
    }
}
