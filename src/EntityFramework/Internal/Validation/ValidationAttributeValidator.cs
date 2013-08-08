// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.Validation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Data.Entity.Validation;
    using System.Linq;

    /// <summary>
    /// Validates a property, complex property or an entity using validation attributes the property
    /// or the complex/entity type is decorated with.
    /// </summary>
    /// <remarks>
    /// Note that this class is used for validating primitive properties using attributes declared on the property
    /// (property level validation) and complex properties and entities using attributes declared on the type
    /// (type level validation).
    /// </remarks>
    internal class ValidationAttributeValidator : IValidator
    {
        /// <summary>
        /// Display attribute used to specify the display name for a property or entity.
        /// </summary>
        private readonly DisplayAttribute _displayAttribute;

        /// <summary>
        /// Validation attribute used to validate a property or an entity.
        /// </summary>
        private readonly ValidationAttribute _validationAttribute;

        /// <summary>
        /// Creates an instance of <see cref="ValidationAttributeValidator" /> class.
        /// </summary>
        /// <param name="validationAttribute"> Validation attribute used to validate a property or an entity. </param>
        public ValidationAttributeValidator(ValidationAttribute validationAttribute, DisplayAttribute displayAttribute)
        {
            DebugCheck.NotNull(validationAttribute);

            _validationAttribute = validationAttribute;
            _displayAttribute = displayAttribute;
        }

        /// <summary>
        /// Validates a property or an entity.
        /// </summary>
        /// <param name="entityValidationContext"> Validation context. Never null. </param>
        /// <param name="property"> Property to validate. Null for entity validation. Not null for property validation. </param>
        /// <returns>
        /// Validation errors as <see cref="IEnumerable{DbValidationError}" /> . Empty if no errors, never null.
        /// </returns>
        public virtual IEnumerable<DbValidationError> Validate(
            EntityValidationContext entityValidationContext, InternalMemberEntry property)
        {
            DebugCheck.NotNull(entityValidationContext);

            var validationContext = entityValidationContext.ExternalValidationContext;

            validationContext.SetDisplayName(property, _displayAttribute);

            var objectToValidate = property == null
                                       ? entityValidationContext.InternalEntity.Entity
                                       : property.CurrentValue;

            ValidationResult validationResult = null;

            try
            {
                validationResult = _validationAttribute.GetValidationResult(objectToValidate, validationContext);
            }
            catch (Exception ex)
            {
                throw new DbUnexpectedValidationException(
                    Strings.DbUnexpectedValidationException_ValidationAttribute(
                        validationContext.DisplayName, _validationAttribute.GetType()),
                    ex);
            }

            return validationResult != ValidationResult.Success
                       ? DbHelpers.SplitValidationResults(validationContext.MemberName, new[] { validationResult })
                       : Enumerable.Empty<DbValidationError>();
        }
    }
}
