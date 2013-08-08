// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
    /// <summary>
    /// Attribute for scalar properties in an IEntity.
    /// Implied default AttributeUsage properties Inherited=True, AllowMultiple=False,
    /// The metadata system expects this and will only look at the first of each of these attributes, even if there are more.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EdmScalarPropertyAttribute : EdmPropertyAttribute
    {
        // Private variables corresponding to their properties.
        private bool _isNullable = true;

        /// <summary>Gets or sets the value that indicates whether the property can have a null value.</summary>
        /// <returns>The value that indicates whether the property can have a null value.</returns>
        public bool IsNullable
        {
            get { return _isNullable; }
            set { _isNullable = value; }
        }

        /// <summary>Gets or sets the value that indicates whether the property is part of the entity key.</summary>
        /// <returns>The value that indicates whether the property is part of the entity key.</returns>
        public bool EntityKeyProperty { get; set; }
    }
}
