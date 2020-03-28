// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.Validation;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Reflection;

    internal class EntityValidatorBuilderForMock : EntityValidatorBuilder
    {
        public EntityValidatorBuilderForMock(AttributeProvider attributeProvider)
            : base(attributeProvider)
        {
        }

        public EntityValidator BuildEntityValidatorBase(InternalEntityEntry entityEntry)
        {
            return base.BuildEntityValidator(entityEntry);
        }

        public IList<PropertyValidator> BuildValidatorsForPropertiesBase(
            IEnumerable<PropertyInfo> clrProperties, IEnumerable<EdmProperty> edmProperties,
            IEnumerable<NavigationProperty> navigationProperties)
        {
            return base.BuildValidatorsForProperties(clrProperties, edmProperties, navigationProperties);
        }

        public PropertyValidator BuildPropertyValidatorBase(
            PropertyInfo clrProperty, EdmProperty edmProperty, bool buildFacetValidators)
        {
            return base.BuildPropertyValidator(clrProperty, edmProperty, buildFacetValidators);
        }

        public PropertyValidator BuildPropertyValidatorBase(PropertyInfo clrProperty)
        {
            return base.BuildPropertyValidator(clrProperty);
        }

        public ComplexTypeValidator BuildComplexTypeValidatorBase(Type clrType, ComplexType complexType)
        {
            return base.BuildComplexTypeValidator(clrType, complexType);
        }

        public IList<IValidator> BuildValidationAttributeValidatorsBase(IEnumerable<Attribute> attributes)
        {
            return base.BuildValidationAttributeValidators(attributes);
        }

        public IEnumerable<PropertyInfo> GetPublicInstancePropertiesBase(Type type)
        {
            return base.GetPublicInstanceProperties(type);
        }

        public IEnumerable<IValidator> BuildFacetValidatorsBase(
            PropertyInfo clrProperty, EdmMember edmProperty, IEnumerable<Attribute> existingAttributes)
        {
            return base.BuildFacetValidators(clrProperty, edmProperty, existingAttributes);
        }
    }
}
