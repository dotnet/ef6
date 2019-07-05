// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Mapping metadata for all types of property mappings.
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
    /// --ScalarPropertyMap
    /// --ScalarPropertyMap
    /// --ScalarPropertyMap
    /// --AssociationSetMapping
    /// --AssociationTypeMapping
    /// --MappingFragment
    /// --EndPropertyMap
    /// --ScalarPropertyMap
    /// --ScalarPropertyMap
    /// --EndPropertyMap
    /// --ScalarPropertyMap
    /// This class represents the metadata for all property map elements in the
    /// above example. This includes the scalar property maps, complex property maps
    /// and end property maps.
    /// </example>
    public abstract class PropertyMapping : MappingItem
    {
        // <summary>
        // The EdmProperty being mapped.
        // </summary>
        private EdmProperty _property;

        internal PropertyMapping(EdmProperty property)
        {
            Debug.Assert(property == null || property.TypeUsage.EdmType.DataSpace == DataSpace.CSpace);

            _property = property;
        }

        internal PropertyMapping()
        {
        }

        /// <summary>
        /// Gets an EdmProperty that specifies the mapped property.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Property")]
        public virtual EdmProperty Property
        {
            get { return _property; }

            internal set
            {
                DebugCheck.NotNull(value);
                Debug.Assert(value.TypeUsage.EdmType.DataSpace == DataSpace.CSpace);
                Debug.Assert(!IsReadOnly);

                _property = value;
            }
        }
    }
}
