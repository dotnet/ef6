// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Mapping metadata for Complex Types.
    /// </summary>
    public class ComplexTypeMapping : StructuralTypeMapping
    {
        private readonly Dictionary<string, PropertyMapping> m_properties =
            new Dictionary<string, PropertyMapping>(StringComparer.Ordinal);

        //child property mappings that make up this complex property

        private readonly Dictionary<EdmProperty, PropertyMapping> m_conditionProperties =
            new Dictionary<EdmProperty, PropertyMapping>(EqualityComparer<EdmProperty>.Default);

        //Condition property mappings for this complex type

#if DEBUG
        private readonly bool m_isPartial; //Whether the property mapping representation is 
#endif

        //totally represented in this table mapping fragment or not.
        private readonly Dictionary<string, ComplexType> m_types = new Dictionary<string, ComplexType>(StringComparer.Ordinal);
        //Types for which the mapping holds true for.

        private readonly Dictionary<string, ComplexType> m_isOfTypes = new Dictionary<string, ComplexType>(StringComparer.Ordinal);
        //Types for which the mapping holds true for

        // not only the type specified but the sub-types of that type as well.        

        /// <summary>
        /// Construct a new Complex Property mapping object
        /// </summary>
        /// <param name="isPartial"> Whether the property mapping representation is totally represented in this table mapping fragment or not. </param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "isPartial")]
        internal ComplexTypeMapping(bool isPartial)
        {
#if DEBUG
            m_isPartial = isPartial;
#endif
        }

        /// <summary>
        /// a list of TypeMetadata that this mapping holds true for.
        /// </summary>
        internal ReadOnlyCollection<ComplexType> Types
        {
            get { return new ReadOnlyCollection<ComplexType>(new List<ComplexType>(m_types.Values)); }
        }

        /// <summary>
        /// a list of TypeMetadatas for which the mapping holds true for
        /// not only the type specified but the sub-types of that type as well.
        /// </summary>
        internal ReadOnlyCollection<ComplexType> IsOfTypes
        {
            get { return new ReadOnlyCollection<ComplexType>(new List<ComplexType>(m_isOfTypes.Values)); }
        }

        /// <summary>
        /// List of child properties that make up this complex property
        /// </summary>
        public override ReadOnlyCollection<PropertyMapping> Properties
        {
            get { return new ReadOnlyCollection<PropertyMapping>(new List<PropertyMapping>(m_properties.Values)); }
        }

        /// <summary>
        /// Returns all the property mappings defined in the complex type mapping
        /// including Properties and Condition Properties
        /// </summary>
        internal ReadOnlyCollection<PropertyMapping> AllProperties
        {
            get
            {
                var properties = new List<PropertyMapping>();
                properties.AddRange(m_properties.Values);
                properties.AddRange(m_conditionProperties.Values);
                return new ReadOnlyCollection<PropertyMapping>(properties);
            }
        }

        /// <summary>
        /// Add a Type to the list of types that this mapping is valid for
        /// </summary>
        internal void AddType(ComplexType type)
        {
            m_types.Add(type.FullName, type);
        }

        /// <summary>
        /// Add a Type to the list of Is-Of types that this mapping is valid for
        /// </summary>
        internal void AddIsOfType(ComplexType type)
        {
            m_isOfTypes.Add(type.FullName, type);
        }

        /// <summary>
        /// Add a property mapping as a child of this complex property mapping
        /// </summary>
        /// <param name="propertyMapping"> The mapping that needs to be added </param>
        internal override void AddProperty(PropertyMapping propertyMapping)
        {
            m_properties.Add(propertyMapping.EdmProperty.Name, propertyMapping);
        }

        internal override void RemoveProperty(PropertyMapping prop)
        {
            m_properties.Remove(prop.EdmProperty.Name);
        }

        /// <summary>
        /// Add a condition property mapping as a child of this complex property mapping
        /// Condition Property Mapping specifies a Condition either on the C side property or S side property.
        /// </summary>
        /// <param name="conditionPropertyMap"> The Condition Property mapping that needs to be added </param>
        internal void AddConditionProperty(
            ConditionPropertyMapping conditionPropertyMap, Action<EdmMember> duplicateMemberConditionError)
        {
            //Same Member can not have more than one Condition with in the 
            //same Complex Type.
            var conditionMember = (conditionPropertyMap.EdmProperty != null)
                                      ? conditionPropertyMap.EdmProperty
                                      : conditionPropertyMap.ColumnProperty;
            Debug.Assert(conditionMember != null);
            if (!m_conditionProperties.ContainsKey(conditionMember))
            {
                m_conditionProperties.Add(conditionMember, conditionPropertyMap);
            }
            else
            {
                duplicateMemberConditionError(conditionMember);
            }
        }

        /// <summary>
        /// The method finds the type in which the member with the given name exists
        /// form the list of IsOfTypes and Type.
        /// </summary>
        internal ComplexType GetOwnerType(string memberName)
        {
            foreach (var type in m_types.Values)
            {
                EdmMember tempMember;
                if ((type.Members.TryGetValue(memberName, false, out tempMember))
                    && (tempMember is EdmProperty))
                {
                    return type;
                }
            }

            foreach (var type in m_isOfTypes.Values)
            {
                EdmMember tempMember;
                if ((type.Members.TryGetValue(memberName, false, out tempMember))
                    && (tempMember is EdmProperty))
                {
                    return type;
                }
            }
            return null;
        }
    }
}
