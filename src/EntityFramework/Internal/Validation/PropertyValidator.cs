namespace System.Data.Entity.Internal.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Validation;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Validates a property of a given EDM property type.
    /// </summary>
    /// <remarks>
    ///     This is a composite validator for a property of an entity or a complex type.
    /// </remarks>
    internal class PropertyValidator
    {
        /// <summary>
        ///     Simple validators for the corresponding property.
        /// </summary>
        private readonly IEnumerable<IValidator> _propertyValidators;

        /// <summary>
        ///     Name of the property the validator was created for.
        /// </summary>
        private readonly string _propertyName;

        /// <summary>
        ///     Creates an instance of <see cref = "PropertyValidator" /> for a given EDM property.
        /// </summary>
        /// <param name = "propertyName">The EDM property name.</param>
        /// <param name = "propertyValidators">Validators used to validate the given property.</param>
        public PropertyValidator(string propertyName, IEnumerable<IValidator> propertyValidators)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));

            Contract.Requires(propertyValidators != null);

            _propertyValidators = propertyValidators;
            _propertyName = propertyName;
        }

        /// <summary>
        ///     Simple validators for the corresponding property.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public IEnumerable<IValidator> PropertyAttributeValidators
        {
            get { return _propertyValidators; }
        }

        /// <summary>
        ///     Gets the name of the property the validator was created for.
        /// </summary>
        public string PropertyName
        {
            get { return _propertyName; }
        }

        /// <summary>
        ///     Validates a property.
        /// </summary>
        /// <param name = "entityValidationContext">Validation context. Never null.</param>
        /// <param name = "property">Property to validate. Never null.</param>
        /// <returns>Validation errors as <see cref = "IEnumerable{DbValidationError}" />. Empty if no errors. Never null.
        /// </returns>
        public virtual IEnumerable<DbValidationError> Validate(
            EntityValidationContext entityValidationContext, InternalMemberEntry property)
        {
            Contract.Requires(entityValidationContext != null);
            Contract.Requires(property != null);

            var validationErrors = new List<DbValidationError>();

            foreach (var validator in _propertyValidators)
            {
                validationErrors.AddRange(validator.Validate(entityValidationContext, property));
            }

            return validationErrors;
        }
    }
}
