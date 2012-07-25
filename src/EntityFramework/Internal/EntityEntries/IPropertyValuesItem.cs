// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Internal
{
    /// <summary>
    ///     Represents an item in an <see cref = "InternalPropertyValues" /> representing a property name/value.
    /// </summary>
    internal interface IPropertyValuesItem
    {
        /// <summary>
        ///     Gets or sets the value of the property represented by this item.
        /// </summary>
        /// <value>The value.</value>
        object Value { get; set; }

        /// <summary>
        ///     Gets the name of the property.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        ///     Gets a value indicating whether this item represents a complex property.
        /// </summary>
        /// <value><c>true</c> If this instance represents a complex property; otherwise, <c>false</c>.</value>
        bool IsComplex { get; }

        /// <summary>
        ///     Gets the type of the underlying property.
        /// </summary>
        /// <value>The property type.</value>
        Type Type { get; }
    }
}
