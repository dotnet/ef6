// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Internal.Validation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Used to cache and retrieve generated validators and to create context for validating entities or properties.
    /// </summary>
    internal class ValidationProvider
    {
        /// <summary>
        ///     Collection of validators keyed by the entity CLR type. Note that if there's no validation for a given type
        ///     it will be associated with a null validator.
        /// </summary>
        private readonly Dictionary<Type, EntityValidator> _entityValidators;

        private readonly EntityValidatorBuilder _entityValidatorBuilder;

        /// <summary>
        ///     Initializes a new instance of <see cref = "ValidationProvider" /> class.
        /// </summary>
        public ValidationProvider(EntityValidatorBuilder builder = null)
        {
            _entityValidators = new Dictionary<Type, EntityValidator>();
            _entityValidatorBuilder = builder ?? new EntityValidatorBuilder(new AttributeProvider());
        }

        /// <summary>
        ///     Returns a validator to validate <paramref name = "entityEntry" />.
        /// </summary>
        /// <param name = "entityEntry">Entity the validator is requested for.</param>
        /// <returns>
        ///     <see cref = "EntityValidator" /> to validate <paramref name = "entityEntry" />. Possibly null if no validation 
        ///     has been specified for the entity.
        /// </returns>
        public virtual EntityValidator GetEntityValidator(InternalEntityEntry entityEntry)
        {
            Contract.Requires(entityEntry != null);

            var entityType = entityEntry.EntityType;
            EntityValidator validator = null;
            if (_entityValidators.TryGetValue(entityType, out validator))
            {
                return validator;
            }
            else
            {
                validator = _entityValidatorBuilder.BuildEntityValidator(entityEntry);
                _entityValidators[entityType] = validator;
                return validator;
            }
        }

        /// <summary>
        ///     Returns a validator to validate <paramref name = "property" />.
        /// </summary>
        /// <param name = "property">Navigation property the validator is requested for.</param>
        /// <returns>
        ///     Validator to validate <paramref name = "property" />. Possibly null if no validation 
        ///     has been specified for the requested property.
        /// </returns>
        public virtual PropertyValidator GetPropertyValidator(
            InternalEntityEntry owningEntity, InternalMemberEntry property)
        {
            Contract.Requires(owningEntity != null);
            Contract.Requires(property != null);

            var entityValidator = GetEntityValidator(owningEntity);

            return entityValidator != null ? GetValidatorForProperty(entityValidator, property) : null;
        }

        /// <summary>
        ///     Gets a validator for the <paramref name = "memberEntry" />.
        /// </summary>
        /// <param name = "entityValidator">Entity validator.</param>
        /// <param name = "memberEntry">Property to get a validator for.</param>
        /// <returns>
        ///     Validator to validate <paramref name = "memberEntry" />. Possibly null if there is no validation for the 
        ///     <paramref name = "memberEntry" />.
        /// </returns>
        /// <remarks>
        ///     For complex properties this method walks up the type hierarchy to get to the entity level and then goes down
        ///     and gets a validator for the child property that is an ancestor of the property to validate. If a validator
        ///     returned for an ancestor is null it means that there is no validation defined beneath and the method just 
        ///     propagates (and eventually returns) null.
        /// </remarks>
        protected virtual PropertyValidator GetValidatorForProperty(
            EntityValidator entityValidator, InternalMemberEntry memberEntry)
        {
            var complexPropertyEntry = memberEntry as InternalNestedPropertyEntry;
            if (complexPropertyEntry != null)
            {
                var propertyValidator =
                    GetValidatorForProperty(entityValidator, complexPropertyEntry.ParentPropertyEntry) as
                    ComplexPropertyValidator;
                // if a validator for parent property is null there is no validation for child properties.  
                // just propagate the null.
                return propertyValidator != null && propertyValidator.ComplexTypeValidator != null
                           ? propertyValidator.ComplexTypeValidator.GetPropertyValidator(memberEntry.Name)
                           : null;
            }
            else
            {
                return entityValidator.GetPropertyValidator(memberEntry.Name);
            }
        }

        /// <summary>
        ///     Creates <see cref = "EntityValidationContext" /> for <paramref name = "entityEntry" />.
        /// </summary>
        /// <param name = "entityEntry">Entity entry for which a validation context needs to be created.</param>
        /// <param name = "items">User defined dictionary containing additional info for custom validation. This parameter is optional and can be null.</param>
        /// <returns>An instance of <see cref = "EntityValidationContext" /> class.</returns>
        /// <seealso cref = "DbContext.ValidateEntity" />
        public virtual EntityValidationContext GetEntityValidationContext(
            InternalEntityEntry entityEntry, IDictionary<object, object> items)
        {
            Contract.Requires(entityEntry != null);

            return new EntityValidationContext(entityEntry, new ValidationContext(entityEntry.Entity, null, items));
        }
    }
}
