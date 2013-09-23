// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Mapping.ViewGeneration;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;

    // <summary>
    // Describes user-defined discriminator metadata (e.g. for a basic TPH mapping). Encapsulates
    // relevant data from System.Data.Entity.Core.Mapping.ViewGenerabetion.DiscriminatorMap (that is to say,
    // data relevant to the PlanCompiler). This separate class accomplishes two things:
    // 1. Maintain separation of ViewGen and PlanCompiler
    // 2. Avoid holding references to CQT expressions in ITree ops (which the ViewGen.DiscriminatorMap
    // holds a few CQT references)
    // </summary>
    internal class ExplicitDiscriminatorMap
    {
        private readonly ReadOnlyCollection<KeyValuePair<object, EntityType>> m_typeMap;
        private readonly EdmMember m_discriminatorProperty;
        private readonly ReadOnlyCollection<EdmProperty> m_properties;

        internal ExplicitDiscriminatorMap(DiscriminatorMap template)
        {
            m_typeMap = template.TypeMap;
            m_discriminatorProperty = template.Discriminator.Property;
            m_properties = new ReadOnlyCollection<EdmProperty>(template.PropertyMap.Select(propertyValuePair => propertyValuePair.Key)
                                   .ToList());
        }

        // <summary>
        // Maps from discriminator value to type.
        // </summary>
        internal ReadOnlyCollection<KeyValuePair<object, EntityType>> TypeMap
        {
            get { return m_typeMap; }
        }

        // <summary>
        // Gets property containing discriminator value.
        // </summary>
        internal EdmMember DiscriminatorProperty
        {
            get { return m_discriminatorProperty; }
        }

        // <summary>
        // All properties for the type hierarchy.
        // </summary>
        internal ReadOnlyCollection<EdmProperty> Properties
        {
            get { return m_properties; }
        }

        // <summary>
        // Returns the type id for the given entity type, or null if non exists.
        // </summary>
        internal object GetTypeId(EntityType entityType)
        {
            object result = null;
            foreach (var discriminatorTypePair in TypeMap)
            {
                if (discriminatorTypePair.Value.EdmEquals(entityType))
                {
                    result = discriminatorTypePair.Key;
                    break;
                }
            }
            return result;
        }
    }
}
