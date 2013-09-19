// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
#pragma warning disable 3015 // no accessible constructors which use only CLS-compliant types

    /// <summary>
    /// Base attribute for schematized types
    /// </summary>
    public abstract class EdmTypeAttribute : Attribute
    {
        /// <summary>
        /// Only allow derived attributes from this assembly
        /// </summary>
        internal EdmTypeAttribute()
        {
        }

        /// <summary>The name of the type in the conceptual schema that maps to the class to which this attribute is applied.</summary>
        /// <returns>
        /// A <see cref="T:System.String" /> that is the name.
        /// </returns>
        public string Name { get; set; }

        /// <summary>The namespace name of the entity object type or complex type in the conceptual schema that maps to this type.</summary>
        /// <returns>
        /// A <see cref="T:System.String" /> that is the namespace name.
        /// </returns>
        public string NamespaceName { get; set; }
    }
}
