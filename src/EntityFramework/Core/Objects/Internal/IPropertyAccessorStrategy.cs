// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Data.Entity.Core.Objects.DataClasses;

    /// <summary>
    ///     A strategy interface that defines methods used for setting and getting values of
    ///     properties and collections on entities.
    ///     Implementors of this interface are used by the EntityWrapper class.
    /// </summary>
    internal interface IPropertyAccessorStrategy
    {
        /// <summary>
        ///     Gets the value of a navigation property for the given related end.
        /// </summary>
        /// <param name="relatedEnd"> Specifies the related end for which a value is required </param>
        /// <returns> The property value </returns>
        object GetNavigationPropertyValue(RelatedEnd relatedEnd);

        /// <summary>
        ///     Sets the value of a navigation property for the given related end.
        /// </summary>
        /// <param name="relatedEnd"> Specifies the related end for which a value should be set </param>
        /// <param name="value"> The value to set </param>
        void SetNavigationPropertyValue(RelatedEnd relatedEnd, object value);

        /// <summary>
        ///     Adds a value to the collection represented by the given related end.
        /// </summary>
        /// <param name="relatedEnd"> The related end for the collection to use </param>
        /// <param name="value"> The value to add to the collection </param>
        void CollectionAdd(RelatedEnd relatedEnd, object value);

        /// <summary>
        ///     Removes a value from the collection represented by the given related end.
        /// </summary>
        /// <param name="relatedEnd"> The related end for the collection to use </param>
        /// <param name="value"> The value to remove from the collection </param>
        /// <returns> True if a value was found and removed; false otherwise </returns>
        bool CollectionRemove(RelatedEnd relatedEnd, object value);

        /// <summary>
        ///     Creates a new collection for the given related end.
        /// </summary>
        /// <param name="relatedEnd"> The related end for which a collection should be created </param>
        /// <returns> The new collection </returns>
        object CollectionCreate(RelatedEnd relatedEnd);
    }
}
