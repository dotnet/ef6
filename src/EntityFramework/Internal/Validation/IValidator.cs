// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Validation;

    /// <summary>
    ///     Abstracts simple validators used to validate entities and properties.
    /// </summary>
    internal interface IValidator
    {
        /// <summary>
        ///     Validates an entity or a property.
        /// </summary>
        /// <param name="entityValidationContext"> Validation context. Never null. </param>
        /// <param name="property"> Property to validate. Can be null for type level validation. </param>
        /// <returns>
        ///     Validation error as <see cref="IEnumerable{DbValidationError}" /> . Empty if no errors. Never null.
        /// </returns>
        IEnumerable<DbValidationError> Validate(
            EntityValidationContext entityValidationContext, InternalMemberEntry property);
    }
}
