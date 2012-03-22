namespace System.Data.Entity.Internal.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Validation;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Abstracts simple validators used to validate entities and properties.
    /// </summary>
    [ContractClass(typeof(IValidatorContracts))]
    internal interface IValidator
    {
        /// <summary>
        ///     Validates an entity or a property.
        /// </summary>
        /// <param name = "entityValidationContext">Validation context. Never null.</param>
        /// <param name = "property">Property to validate. Can be null for type level validation.</param>
        /// <returns>Validation error as<see cref = "IEnumerable{DbValidationError}" />. Empty if no errors. Never null.
        /// </returns>
        IEnumerable<DbValidationError> Validate(
            EntityValidationContext entityValidationContext, InternalMemberEntry property);
    }

    [ContractClassFor(typeof(IValidator))]
    internal abstract class IValidatorContracts : IValidator
    {
        /// <summary>
        ///     Contract for IValidator.Validate method.
        /// </summary>
        /// <param name = "entityValidationContext">Validation context.</param>
        /// <param name = "property">Property.</param>
        /// <returns>Nothing - always throws.</returns>
        IEnumerable<DbValidationError> IValidator.Validate(
            EntityValidationContext entityValidationContext, InternalMemberEntry property)
        {
            Contract.Requires(entityValidationContext != null);
            Contract.Ensures(Contract.Result<IEnumerable<DbValidationError>>() != null);

            throw new NotImplementedException();
        }
    }
}
