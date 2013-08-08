// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    /// <summary>
    /// A reference to "all" properties of a type
    /// </summary>
    internal class AllPropertyRef : PropertyRef
    {
        private AllPropertyRef()
        {
        }

        /// <summary>
        /// Get the singleton instance
        /// </summary>
        internal static AllPropertyRef Instance = new AllPropertyRef();

        /// <summary>
        /// Create a nested property ref, with "p" as the prefix
        /// </summary>
        /// <param name="p"> the property to prefix with </param>
        /// <returns> the nested property reference </returns>
        internal override PropertyRef CreateNestedPropertyRef(PropertyRef p)
        {
            return p;
        }

        public override string ToString()
        {
            return "ALL";
        }
    }
}
