// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Represents the Mapping metadata for a type map in CS space.
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
    /// --ScalarProperyMap
    /// --ScalarPropertyMap
    /// --AssociationSetMapping
    /// --AssociationTypeMapping
    /// --MappingFragment
    /// --EndPropertyMap
    /// --ScalarPropertyMap
    /// --ScalarProperyMap
    /// --EndPropertyMap
    /// --ScalarPropertyMap
    /// This class represents the metadata for all the Type map elements in the
    /// above example namely EntityTypeMapping, AssociationTypeMapping and CompositionTypeMapping.
    /// The TypeMapping elements contain TableMappingFragments which in turn contain the property maps.
    /// </example>
    public abstract class TypeMapping : MappingItem
    {
        internal TypeMapping()
        {
        }

        internal abstract EntitySetBaseMapping SetMapping { get; }

        // <summary>
        // a list of TypeMetadata that this mapping holds true for.
        // </summary>
        internal abstract ReadOnlyCollection<EntityTypeBase> Types { get; }

        // <summary>
        // a list of TypeMetadatas for which the mapping holds true for
        // not only the type specified but the sub-types of that type as well.
        // </summary>
        internal abstract ReadOnlyCollection<EntityTypeBase> IsOfTypes { get; }

        internal abstract ReadOnlyCollection<MappingFragment> MappingFragments { get; }
    }
}
