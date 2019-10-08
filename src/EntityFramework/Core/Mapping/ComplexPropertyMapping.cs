// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Mapping metadata for Complex properties.
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
    /// --ComplexTypeMapping
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --DiscriminatorPropertyMap ( constant value-->SMemberMetadata )
    /// --ComplexTypeMapping
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --DiscriminatorPropertyMap ( constant value-->SMemberMetadata )
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --AssociationSetMapping
    /// --AssociationTypeMapping
    /// --MappingFragment
    /// --EndPropertyMap
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --EndPropertyMap
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// This class represents the metadata for all the complex property map elements in the
    /// above example. ComplexPropertyMaps contain ComplexTypeMaps which define mapping based
    /// on the type of the ComplexProperty in case of inheritance.
    /// </example>
    public class ComplexPropertyMapping : PropertyMapping
    {
        // <summary>
        // Set of type mappings that make up the EdmProperty mapping.
        // </summary>
        private readonly List<ComplexTypeMapping> _typeMappings;

        /// <summary>
        /// Construct a new Complex Property mapping object
        /// </summary>
        /// <param name="property"> The MemberMetadata object that represents this Complex member </param>
        public ComplexPropertyMapping(EdmProperty property)
            : base(property)
        {
            Check.NotNull(property, "property");

            if (!TypeSemantics.IsComplexType(property.TypeUsage))
            {
                throw new ArgumentException(Strings.StorageComplexPropertyMapping_OnlyComplexPropertyAllowed, "property");
            }

            _typeMappings = new List<ComplexTypeMapping>();
        }

        /// <summary>
        /// Gets a read only collections of type mappings corresponding to the 
        /// nested complex types.
        /// </summary>
        public ReadOnlyCollection<ComplexTypeMapping> TypeMappings
        {
            get { return new ReadOnlyCollection<ComplexTypeMapping>(_typeMappings); }
        }

        /// <summary>
        /// Adds a type mapping corresponding to a nested complex type.
        /// </summary>
        /// <param name="typeMapping">The complex type mapping to be added.</param>
        public void AddTypeMapping(ComplexTypeMapping typeMapping)
        {
            Check.NotNull(typeMapping, "typeMapping");
            ThrowIfReadOnly();

            _typeMappings.Add(typeMapping);
        }

        /// <summary>
        /// Removes a type mapping corresponding to a nested complex type.
        /// </summary>
        /// <param name="typeMapping">The complex type mapping to be removed.</param>
        public void RemoveTypeMapping(ComplexTypeMapping typeMapping)
        {
            Check.NotNull(typeMapping, "typeMapping");
            ThrowIfReadOnly();

            _typeMappings.Remove(typeMapping);
        }

        internal override void SetReadOnly()
        {
            _typeMappings.TrimExcess();

            SetReadOnly(_typeMappings);

            base.SetReadOnly();
        }
    }
}
