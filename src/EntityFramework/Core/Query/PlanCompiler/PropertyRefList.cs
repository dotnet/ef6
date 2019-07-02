// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;

    // <summary>
    // Represents a collection of property references
    // </summary>
    internal class PropertyRefList
    {
        private readonly Dictionary<PropertyRef, PropertyRef> m_propertyReferences;
        private bool m_allProperties;

        // <summary>
        // Get something that represents "all" property references
        // </summary>
        internal static PropertyRefList All = new PropertyRefList(true);

        // <summary>
        // Trivial constructor
        // </summary>
        internal PropertyRefList()
            : this(false)
        {
        }

        private PropertyRefList(bool allProps)
        {
            m_propertyReferences = new Dictionary<PropertyRef, PropertyRef>();

            if (allProps)
            {
                MakeAllProperties();
            }
        }

        private void MakeAllProperties()
        {
            m_allProperties = true;
            m_propertyReferences.Clear();
            m_propertyReferences.Add(AllPropertyRef.Instance, AllPropertyRef.Instance);
        }

        // <summary>
        // Add a new property reference to this list
        // </summary>
        // <param name="property"> new property reference </param>
        internal void Add(PropertyRef property)
        {
            if (m_allProperties)
            {
                return;
            }
            else if (property is AllPropertyRef)
            {
                MakeAllProperties();
            }
            else
            {
                m_propertyReferences[property] = property;
            }
        }

        // <summary>
        // Append an existing list of property references to myself
        // </summary>
        // <param name="propertyRefs"> list of property references </param>
        internal void Append(PropertyRefList propertyRefs)
        {
            if (m_allProperties)
            {
                return;
            }
            foreach (var p in propertyRefs.m_propertyReferences.Keys)
            {
                Add(p);
            }
        }

        // <summary>
        // Do I contain "all" properties?
        // </summary>
        internal bool AllProperties
        {
            get { return m_allProperties; }
        }

        // <summary>
        // Create a clone of myself
        // </summary>
        // <returns> a clone of myself </returns>
        internal PropertyRefList Clone()
        {
            var newProps = new PropertyRefList(m_allProperties);
            foreach (var p in m_propertyReferences.Keys)
            {
                newProps.Add(p);
            }
            return newProps;
        }

        // <summary>
        // Do I contain the specified property?
        // </summary>
        // <param name="p"> The property </param>
        // <returns> true, if I do </returns>
        internal bool Contains(PropertyRef p)
        {
            return m_allProperties || m_propertyReferences.ContainsKey(p);
        }

        // <summary>
        // Get the list of all properties
        // </summary>
        internal IEnumerable<PropertyRef> Properties
        {
            get { return m_propertyReferences.Keys; }
        }

        public override string ToString()
        {
            var x = "{";
            foreach (var p in m_propertyReferences.Keys)
            {
                x += p + ",";
            }
            x += "}";
            return x;
        }
    }
}
