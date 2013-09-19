// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.Validation;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Linq;
    using System.Reflection;
    using Moq;
    using Moq.Protected;

    internal static class MockHelper
    {

        public static ValidatableObjectValidator CreateValidatableObjectValidator(string propertyName, string errorMessage)
        {
            var validetableObjectValidator = new Mock<ValidatableObjectValidator>(null);
            validetableObjectValidator
                .Setup(
                    v => v.Validate(It.IsAny<EntityValidationContext>(), It.IsAny<InternalMemberEntry>()))
                .Returns<EntityValidationContext, InternalMemberEntry>(
                    (c, e) =>
                    new[] { new DbValidationError(propertyName, errorMessage) });
            return validetableObjectValidator.Object;
        }

        public static ValidationAttribute CreateValidationAttribute(string errorMessage)
        {
            var validationAttribute = new Mock<ValidationAttribute>();
            validationAttribute.CallBase = true;
            validationAttribute.Setup(a => a.IsValid(It.IsAny<object>()))
                               .Returns<object>(o => false);
            validationAttribute.Setup(a => a.FormatErrorMessage(It.IsAny<string>()))
                               .Returns<string>(n => errorMessage);
            return validationAttribute.Object;
        }

        public static Mock<ValidationProviderForMock> CreateMockValidationProvider(EntityValidatorBuilderForMock builder = null)
        {
            builder = builder ?? CreateMockEntityValidatorBuilder().Object;

            var validationProvider = new Mock<ValidationProviderForMock>(builder);

            return validationProvider;
        }

        public static Mock<EntityValidatorBuilderForMock> CreateMockEntityValidatorBuilder(
            Mock<AttributeProvider> attributeProvider = null)
        {
            attributeProvider = attributeProvider ?? new Mock<AttributeProvider>();
            var builder = new Mock<EntityValidatorBuilderForMock>(attributeProvider.Object);
            builder.Protected()
                   .Setup<IList<PropertyValidator>>(
                       "BuildValidatorsForProperties", ItExpr.IsAny<IEnumerable<PropertyInfo>>(),
                       ItExpr.IsAny<IEnumerable<EdmProperty>>(), ItExpr.IsAny<IEnumerable<NavigationProperty>>())
                   .Returns<IEnumerable<PropertyInfo>, IEnumerable<EdmProperty>, IEnumerable<NavigationProperty>>(
                       (pi, e, n) => new List<PropertyValidator>());

            builder.Protected()
                   .Setup<IList<IValidator>>("BuildValidationAttributeValidators", ItExpr.IsAny<IEnumerable<Attribute>>())
                   .Returns<IEnumerable<Attribute>>(a => new List<IValidator>());

            builder.Protected()
                   .Setup<IEnumerable<IValidator>>(
                       "BuildFacetValidators", ItExpr.IsAny<PropertyInfo>(), ItExpr.IsAny<EdmMember>(), ItExpr.IsAny<IEnumerable<Attribute>>())
                   .Returns<PropertyInfo, EdmMember, IEnumerable<Attribute>>((pi, e, a) => Enumerable.Empty<IValidator>());

            return builder;
        }

        public static EntityValidationContext CreateEntityValidationContext(InternalEntityEntry entityEntry)
        {
            return new EntityValidationContext(entityEntry, new ValidationContext(entityEntry.Entity, null, null));
        }
    }
}
