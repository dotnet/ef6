// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.Validation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Builds validators based on <see cref="ValidationAttribute" />s specified on entity CLR types and properties
    /// as well as based on presence of <see cref="IValidatableObject" /> implementation on entity and complex
    /// type CLR types. It's not sealed and not static for mocking purposes.
    /// </summary>
    internal class EntityValidatorBuilder
    {
        private readonly AttributeProvider _attributeProvider;

        public EntityValidatorBuilder(AttributeProvider attributeProvider)
        {
            DebugCheck.NotNull(attributeProvider);

            _attributeProvider = attributeProvider;
        }

        /// <summary>
        /// Builds an <see cref="EntityValidator" /> for the given <paramref name="entityEntry" />.
        /// </summary>
        /// <param name="entityEntry"> The entity entry to build the validator for. </param>
        /// <returns>
        /// <see cref="EntityValidator" /> for the given <paramref name="entityEntry" /> . Possibly null if no validation has been specified for this entity type.
        /// </returns>
        public virtual EntityValidator BuildEntityValidator(InternalEntityEntry entityEntry)
        {
            DebugCheck.NotNull(entityEntry);

            return BuildTypeValidator(
                entityEntry.EntityType,
                entityEntry.EdmEntityType.Properties,
                entityEntry.EdmEntityType.NavigationProperties,
                (propertyValidators, typeLevelValidators) =>
                new EntityValidator(propertyValidators, typeLevelValidators));
        }

        /// <summary>
        /// Builds the validator for a given <paramref name="complexType" /> and the corresponding
        /// <paramref name="clrType" />.
        /// </summary>
        /// <param name="clrType"> The CLR type that corresponds to the EDM complex type. </param>
        /// <param name="complexType"> The EDM complex type that type level validation is built for. </param>
        /// <returns>
        /// A <see cref="ComplexTypeValidator" /> for the given complex type. May be null if no validation specified.
        /// </returns>
        protected virtual ComplexTypeValidator BuildComplexTypeValidator(Type clrType, ComplexType complexType)
        {
            DebugCheck.NotNull(complexType);
            DebugCheck.NotNull(clrType);
            Debug.Assert(complexType.Name == clrType.Name);

            return BuildTypeValidator(
                clrType,
                complexType.Properties,
                Enumerable.Empty<NavigationProperty>(),
                (propertyValidators, typeLevelValidators) =>
                new ComplexTypeValidator(propertyValidators, typeLevelValidators));
        }

        /// <summary>
        /// Extracted method from BuildEntityValidator and BuildComplexTypeValidator
        /// </summary>
        private T BuildTypeValidator<T>(
            Type clrType,
            IEnumerable<EdmProperty> edmProperties,
            IEnumerable<NavigationProperty> navigationProperties,
            Func<IEnumerable<PropertyValidator>, IEnumerable<IValidator>, T> validatorFactoryFunc)
            where T : TypeValidator
        {
            var propertyValidators = BuildValidatorsForProperties(
                GetPublicInstanceProperties(clrType), edmProperties, navigationProperties);

            var attributes = _attributeProvider.GetAttributes(clrType);

            var typeLevelValidators = BuildValidationAttributeValidators(attributes);

            if (typeof(IValidatableObject).IsAssignableFrom(clrType))
            {
                typeLevelValidators.Add(
                    new ValidatableObjectValidator(attributes.OfType<DisplayAttribute>().SingleOrDefault()));
            }

            return propertyValidators.Any() || typeLevelValidators.Any()
                       ? validatorFactoryFunc(propertyValidators, typeLevelValidators)
                       : null;
        }

        /// <summary>
        /// Build validators for the <paramref name="clrProperties" /> and the corresponding <paramref name="edmProperties" />
        /// or <paramref name="navigationProperties" />.
        /// </summary>
        /// <param name="clrProperties"> Properties to build validators for. </param>
        /// <param name="edmProperties"> Non-navigation EDM properties. </param>
        /// <param name="navigationProperties"> Navigation EDM properties. </param>
        /// <returns> A list of validators. Possibly empty, never null. </returns>
        protected virtual IList<PropertyValidator> BuildValidatorsForProperties(
            IEnumerable<PropertyInfo> clrProperties,
            IEnumerable<EdmProperty> edmProperties,
            IEnumerable<NavigationProperty> navigationProperties)
        {
            DebugCheck.NotNull(edmProperties);
            DebugCheck.NotNull(navigationProperties);
            DebugCheck.NotNull(clrProperties);

            var validators = new List<PropertyValidator>();

            foreach (var property in clrProperties)
            {
                PropertyValidator propertyValidator = null;

                var edmProperty = edmProperties
                    .Where(p => p.Name == property.Name)
                    .SingleOrDefault();

                if (edmProperty != null)
                {
                    var referencingAssociations = from navigationProperty in navigationProperties
                                                  let associationType =
                                                      navigationProperty.RelationshipType as AssociationType
                                                  where associationType != null
                                                  from constraint in associationType.ReferentialConstraints
                                                  where constraint.ToProperties.Contains(edmProperty)
                                                  select constraint;

                    propertyValidator = BuildPropertyValidator(
                        property, edmProperty, buildFacetValidators: !referencingAssociations.Any());
                }
                else
                {
                    // Currently we don't use facets to build validators for navigation properties,
                    // if this changes in the future we would need to implement and call a different overload
                    // of BuildPropertyValidator here

                    propertyValidator = BuildPropertyValidator(property);
                }

                if (propertyValidator != null)
                {
                    validators.Add(propertyValidator);
                }
            }

            return validators;
        }

        /// <summary>
        /// Builds a <see cref="PropertyValidator" /> for the given <paramref name="edmProperty" /> and the corresponding
        /// <paramref name="clrProperty" />. If the property is a complex type, type level validators will be built here as
        /// well.
        /// </summary>
        /// <param name="clrProperty"> The CLR property to build the validator for. </param>
        /// <param name="edmProperty"> The EDM property to build the validator for. </param>
        /// <returns>
        /// <see cref="PropertyValidator" /> for the given <paramref name="edmProperty" /> . Possibly null if no validation has been specified for this property.
        /// </returns>
        protected virtual PropertyValidator BuildPropertyValidator(
            PropertyInfo clrProperty, EdmProperty edmProperty, bool buildFacetValidators)
        {
            DebugCheck.NotNull(clrProperty);
            DebugCheck.NotNull(edmProperty);
            Debug.Assert(clrProperty.Name == edmProperty.Name);

            var propertyAttributeValidators = new List<IValidator>();

            var attributes = _attributeProvider.GetAttributes(clrProperty);

            propertyAttributeValidators.AddRange(BuildValidationAttributeValidators(attributes));

            if (edmProperty.TypeUsage.EdmType.BuiltInTypeKind
                == BuiltInTypeKind.ComplexType)
            {
                // this is a complex type so build validators for child properties
                var complexType = (ComplexType)edmProperty.TypeUsage.EdmType;

                // finally build validators for type level validation mechanisms defined for this complex type
                var complexTypeValidator = BuildComplexTypeValidator(clrProperty.PropertyType, complexType);
                return propertyAttributeValidators.Any() || complexTypeValidator != null
                           ? new ComplexPropertyValidator(
                                 clrProperty.Name, propertyAttributeValidators, complexTypeValidator)
                           : null;
            }
            else if (buildFacetValidators)
            {
                propertyAttributeValidators.AddRange(BuildFacetValidators(clrProperty, edmProperty, attributes));
            }

            return propertyAttributeValidators.Any()
                       ? new PropertyValidator(clrProperty.Name, propertyAttributeValidators)
                       : null;
        }

        /// <summary>
        /// Builds a <see cref="PropertyValidator" /> for the given transient <paramref name="clrProperty" />.
        /// </summary>
        /// <param name="clrProperty"> The CLR property to build the validator for. </param>
        /// <returns>
        /// <see cref="PropertyValidator" /> for the given <paramref name="clrProperty" /> . Possibly null if no validation has been specified for this property.
        /// </returns>
        protected virtual PropertyValidator BuildPropertyValidator(PropertyInfo clrProperty)
        {
            DebugCheck.NotNull(clrProperty);

            var propertyValidators = BuildValidationAttributeValidators(_attributeProvider.GetAttributes(clrProperty));

            return propertyValidators.Count > 0
                       ? new PropertyValidator(clrProperty.Name, propertyValidators)
                       : null;
        }

        /// <summary>
        /// Builds <see cref="ValidationAttributeValidator" />s for given <paramref name="attributes" /> that derive from
        /// <see cref="ValidationAttribute" />.
        /// </summary>
        /// <param name="attributes"> Attributes used to build validators. </param>
        /// <returns>
        /// A list of <see cref="ValidationAttributeValidator" /> s built from <paramref name="attributes" /> . Possibly empty, never null.
        /// </returns>
        protected virtual IList<IValidator> BuildValidationAttributeValidators(IEnumerable<Attribute> attributes)
        {
            DebugCheck.NotNull(attributes);

            return (from validationAttribute in attributes
                    where validationAttribute is ValidationAttribute
                    select new ValidationAttributeValidator(
                        (ValidationAttribute)validationAttribute,
                        attributes.OfType<DisplayAttribute>().SingleOrDefault()))
                .ToList<IValidator>();
        }

        /// <summary>
        /// Returns all non-static non-indexed CLR properties from the <paramref name="type" />.
        /// </summary>
        /// <param name="type">
        /// The CLR <see cref="Type" /> to get the properties from.
        /// </param>
        /// <returns> A collection of CLR properties. Possibly empty, never null. </returns>
        protected virtual IEnumerable<PropertyInfo> GetPublicInstanceProperties(Type type)
        {
            DebugCheck.NotNull(type);

            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                       .Where(p => p.GetIndexParameters().Length == 0 && p.GetGetMethod() != null);
        }

        /// <summary>
        /// Builds validators based on the facets of <paramref name="edmProperty" />:
        /// * If .Nullable facet set to false adds a validator equivalent to the RequiredAttribute
        /// * If the .MaxLength facet is specified adds a validator equivalent to the MaxLengthAttribute.
        /// However the validator isn't added if .IsMaxLength has been set to true.
        /// </summary>
        /// <param name="clrProperty"> The CLR property to build the facet validators for. </param>
        /// <param name="edmProperty"> The property for which facet validators will be created </param>
        /// <returns> A collection of validators. </returns>
        protected virtual IEnumerable<IValidator> BuildFacetValidators(
            PropertyInfo clrProperty, EdmMember edmProperty, IEnumerable<Attribute> existingAttributes)
        {
            DebugCheck.NotNull(clrProperty);
            DebugCheck.NotNull(edmProperty);
            DebugCheck.NotNull(existingAttributes);

            var facetDerivedAttributes = new List<ValidationAttribute>();

            MetadataProperty storeGeneratedItem;

            edmProperty.MetadataProperties.TryGetValue(
                XmlConstants.AnnotationNamespace + ":" + XmlConstants.StoreGeneratedPattern,
                false,
                out storeGeneratedItem);

            var propertyIsStoreGenerated = storeGeneratedItem != null && storeGeneratedItem.Value != null;

            Facet nullable;
            edmProperty.TypeUsage.Facets.TryGetValue(EdmConstants.Nullable, false, out nullable);

            var nullableFacetIsFalse = nullable != null && nullable.Value != null && !(bool)nullable.Value;

            if (nullableFacetIsFalse
                && !propertyIsStoreGenerated
                && clrProperty.PropertyType.IsNullable()
                &&
                !existingAttributes.Any(a => a is RequiredAttribute))
            {
                facetDerivedAttributes.Add(
                    new RequiredAttribute
                        {
                            AllowEmptyStrings = true
                        });
            }

            Facet MaxLength;
            edmProperty.TypeUsage.Facets.TryGetValue(XmlConstants.MaxLengthElement, false, out MaxLength);
            if (MaxLength != null
                && MaxLength.Value != null
                && MaxLength.Value is int
                &&
                !existingAttributes.Any(a => a is MaxLengthAttribute)
                &&
                !existingAttributes.Any(a => a is StringLengthAttribute))
            {
                facetDerivedAttributes.Add(new MaxLengthAttribute((int)MaxLength.Value));
            }

            return from attribute in facetDerivedAttributes
                   select
                       new ValidationAttributeValidator(
                       attribute, existingAttributes.OfType<DisplayAttribute>().SingleOrDefault());
        }
    }
}
