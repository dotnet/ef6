// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
#pragma warning disable 3015 // no accessible constructors which use only CLS-compliant types

    /// <summary>
    /// Base attribute for properties mapped to store elements.
    /// Implied default AttributeUsage properties Inherited=True, AllowMultiple=False,
    /// The metadata system expects this and will only look at the first of each of these attributes, even if there are more.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class EdmPropertyAttribute : Attribute
    {
        /// <summary>
        /// Only allow derived attributes from this assembly
        /// </summary>
        internal EdmPropertyAttribute()
        {
        }
    }
}
