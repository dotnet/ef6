// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Data.Entity.Core.Query.InternalTrees;

    // <summary>
    // A rel-property ref - represents a rel property of the type
    // </summary>
    internal class RelPropertyRef : PropertyRef
    {
        #region private state

        private readonly RelProperty m_property;

        #endregion

        #region constructor

        // <summary>
        // Simple constructor
        // </summary>
        // <param name="property"> the property metadata </param>
        internal RelPropertyRef(RelProperty property)
        {
            m_property = property;
        }

        #endregion

        #region public apis

        // <summary>
        // Gets the property metadata
        // </summary>
        internal RelProperty Property
        {
            get { return m_property; }
        }

        // <summary>
        // Overrides the default equality function. Two RelPropertyRefs are
        // equal, if they describe the same property
        // </summary>
        // <param name="obj"> the other object to compare to </param>
        // <returns> true, if the objects are equal </returns>
        public override bool Equals(object obj)
        {
            var other = obj as RelPropertyRef;
            return (other != null &&
                    m_property.Equals(other.m_property));
        }

        // <summary>
        // Overrides the default hashcode function.
        // Simply returns the hashcode for the property instead
        // </summary>
        // <returns> hashcode for the relpropertyref </returns>
        public override int GetHashCode()
        {
            return m_property.GetHashCode();
        }

        // <summary>
        // debugging support
        // </summary>
        public override string ToString()
        {
            return m_property.ToString();
        }

        #endregion
    }
}
