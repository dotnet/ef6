namespace System.Data.Entity.Internal.Validation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Data.Entity.Validation;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     Validates entities or complex types implementing IValidatableObject interface.
    /// </summary>
    internal class ValidatableObjectValidator : IValidator
    {
        /// <summary>
        ///     Display attribute used to specify the display name for an entity or complex property.
        /// </summary>
        private readonly DisplayAttribute _displayAttribute;

        public ValidatableObjectValidator(DisplayAttribute displayAttribute)
        {
            _displayAttribute = displayAttribute;
        }

        /// <summary>
        ///     Validates an entity or a complex type implementing IValidatableObject interface.
        ///     This method is virtual to allow mocking.
        /// </summary>
        /// <param name = "entityValidationContext">Validation context. Never null.</param>
        /// <param name = "property">
        ///     Property to validate. Null if this is the entity that will be validated. Never null if this 
        ///     is the complex type that will be validated.
        /// </param>
        /// <returns>Validation error as <see cref = "IEnumerable{DbValidationError}" />. Empty if no errors. Never null.
        /// </returns>
        /// <remarks>
        ///     Note that <paramref name = "property" /> is used to figure out what needs to be validated. If it not null the complex
        ///     type will be validated otherwise the entity will be validated.
        ///     Also if this is an IValidatableObject complex type but the instance (.CurrentValue) is null we won't validate
        ///     anything and will not return any errors. The reason for this is that Validation is supposed to validate using
        ///     information the user provided and not some additional implicit rules. (ObjectContext will throw for operations
        ///     that involve null complex properties).
        /// </remarks>
        public virtual IEnumerable<DbValidationError> Validate(
            EntityValidationContext entityValidationContext, InternalMemberEntry property)
        {
            Contract.Assert(
                (property == null && entityValidationContext.InternalEntity.Entity is IValidatableObject) ||
                (property != null && (property.CurrentValue == null || property.CurrentValue is IValidatableObject)),
                "Neither entity nor complex type implements IValidatableObject.");

            if (property != null
                && property.CurrentValue == null)
            {
                return Enumerable.Empty<DbValidationError>();
            }

            var validationContext = entityValidationContext.ExternalValidationContext;

            validationContext.SetDisplayName(property, _displayAttribute);

            var validatableObject = (IValidatableObject)(property == null
                                                             ? entityValidationContext.InternalEntity.Entity
                                                             : property.CurrentValue);

            IEnumerable<ValidationResult> validationResults = null;
            try
            {
                validationResults = validatableObject.Validate(validationContext);
            }
            catch (Exception ex)
            {
                throw new DbUnexpectedValidationException(
                    Strings.DbUnexpectedValidationException_IValidatableObject(
                        validationContext.DisplayName, ObjectContextTypeCache.GetObjectType(validatableObject.GetType())),
                    ex);
            }

            return DbHelpers.SplitValidationResults(
                validationContext.MemberName,
                validationResults ?? Enumerable.Empty<ValidationResult>());
        }
    }
}
