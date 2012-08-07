// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
#pragma warning disable 3015 // no accessible constructors which use only CLS-compliant types

    /// <summary>
    ///     Base attribute for schematized types
    /// </summary>
    public abstract class EdmTypeAttribute : Attribute
    {
        /// <summary>
        ///     Only allow derived attributes from this assembly
        /// </summary>
        internal EdmTypeAttribute()
        {
        }

        /// <summary>
        ///     Returns the name of the type that this type maps to in the CSpace
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Returns the namespace of the type that this type maps to in the CSpace
        /// </summary>
        public string NamespaceName { get; set; }
    }
}
