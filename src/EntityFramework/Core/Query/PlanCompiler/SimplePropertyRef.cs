// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Query.InternalTrees;

    /// <summary>
    /// A "simple" property ref - represents a simple property of the type
    /// </summary>
    internal class SimplePropertyRef : PropertyRef
    {
        private readonly EdmMember m_property;

        /// <summary>
        /// Simple constructor
        /// </summary>
        /// <param name="property">the property metadata</param>
        internal SimplePropertyRef(EdmMember property)
        {
            m_property = property;
        }

        /// <summary>
        /// Gets the property metadata
        /// </summary>
        internal EdmMember Property
        {
            get { return m_property; }
        }

        /// <summary>
        /// Overrides the default equality function. Two SimplePropertyRefs are
        /// equal, if they describe the same property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as SimplePropertyRef;
            return (other != null &&
                    Command.EqualTypes(m_property.DeclaringType, other.m_property.DeclaringType) &&
                    other.m_property.Name.Equals(m_property.Name));
        }

        /// <summary>
        /// Overrides the default hashcode function.
        /// Simply returns the hashcode for the property instead
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return m_property.Name.GetHashCode();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return m_property.Name;
        }
    }
}
