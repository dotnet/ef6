// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Query.PlanCompiler
{
    /// <summary>
    /// A TypeId propertyref represents a reference to the TypeId property
    /// of a type (complex type, entity type etc.)
    /// </summary>
    internal class TypeIdPropertyRef : PropertyRef
    {
        private TypeIdPropertyRef()
        {
        }

        /// <summary>
        /// Gets the default instance of this type
        /// </summary>
        internal static TypeIdPropertyRef Instance = new TypeIdPropertyRef();

        /// <summary>
        /// Friendly string for debugging.
        /// </summary>
        public override string ToString()
        {
            return "TYPEID";
        }
    }
}
