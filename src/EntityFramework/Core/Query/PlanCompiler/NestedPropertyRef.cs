// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     A nested propertyref describes a nested property access - think "a.b.c"
    /// </summary>
    internal class NestedPropertyRef : PropertyRef
    {
        private readonly PropertyRef m_inner;
        private readonly PropertyRef m_outer;

        /// <summary>
        ///     Basic constructor.
        ///     Represents the access of property "propertyRef" within property "property"
        /// </summary>
        /// <param name="innerProperty"> the inner property </param>
        /// <param name="outerProperty"> the outer property </param>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "NestedPropertyRef")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "innerProperty")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        internal NestedPropertyRef(PropertyRef innerProperty, PropertyRef outerProperty)
        {
            PlanCompiler.Assert(!(innerProperty is NestedPropertyRef), "innerProperty cannot be a NestedPropertyRef");
            m_inner = innerProperty;
            m_outer = outerProperty;
        }

        /// <summary>
        ///     the nested property
        /// </summary>
        internal PropertyRef OuterProperty
        {
            get { return m_outer; }
        }

        /// <summary>
        ///     the parent property
        /// </summary>
        internal PropertyRef InnerProperty
        {
            get { return m_inner; }
        }

        /// <summary>
        ///     Overrides the default equality function. Two NestedPropertyRefs are
        ///     equal if the have the same property name, and the types are the same
        /// </summary>
        /// <param name="obj"> </param>
        /// <returns> </returns>
        public override bool Equals(object obj)
        {
            var other = obj as NestedPropertyRef;
            return (other != null &&
                    m_inner.Equals(other.m_inner) &&
                    m_outer.Equals(other.m_outer));
        }

        /// <summary>
        ///     Overrides the default hashcode function. Simply adds the hashcodes
        ///     of the "property" and "propertyRef" fields
        /// </summary>
        /// <returns> </returns>
        public override int GetHashCode()
        {
            return m_inner.GetHashCode() ^ m_outer.GetHashCode();
        }

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        public override string ToString()
        {
            return m_inner + "." + m_outer;
        }
    }
}
