// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.Validation;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class ValidationProviderTests
    {
        [Fact]
        public void GetEntityValidator_returns_cached_validator()
        {
            var mockInternalEntity = Internal.MockHelper.CreateMockInternalEntityEntry(new object());
            var mockBuilder = MockHelper.CreateMockEntityValidatorBuilder();
            var provider = MockHelper.CreateMockValidationProvider(mockBuilder.Object);

            var expectedValidator = new EntityValidator(new PropertyValidator[0], new IValidator[0]);
            mockBuilder.Setup(b => b.BuildEntityValidator(It.IsAny<InternalEntityEntry>()))
                .Returns<InternalEntityEntry>(e => expectedValidator);

            var entityValidator = provider.Object.GetEntityValidatorBase(mockInternalEntity.Object);
            Assert.Same(expectedValidator, entityValidator);
            mockBuilder.Verify(b => b.BuildEntityValidator(It.IsAny<InternalEntityEntry>()), Times.Once());

            // Now it should get the cached one
            entityValidator = provider.Object.GetEntityValidatorBase(mockInternalEntity.Object);
            Assert.Same(expectedValidator, entityValidator);
            mockBuilder.Verify(b => b.BuildEntityValidator(It.IsAny<InternalEntityEntry>()), Times.Once());
        }

        [Fact]
        public void GetPropertyValidator_returns_correct_validator()
        {
            var entity = new FlightSegmentWithNestedComplexTypes();
            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(entity);
            var propertyEntry = mockInternalEntityEntry.Object.Property("Departure");

            var propertyValidator = new PropertyValidator("Departure", new IValidator[0]);
            var entityValidator = new EntityValidator(new[] { propertyValidator }, new IValidator[0]);

            var mockValidationProvider = MockHelper.CreateMockValidationProvider();
            mockValidationProvider.Setup(p => p.GetEntityValidator(It.IsAny<InternalEntityEntry>()))
                .Returns<InternalEntityEntry>(e => entityValidator);
            mockValidationProvider.Protected()
                .Setup<PropertyValidator>("GetValidatorForProperty", ItExpr.IsAny<EntityValidator>(), ItExpr.IsAny<InternalMemberEntry>())
                .Returns<EntityValidator, InternalMemberEntry>((ev, e) => propertyValidator);

            var actualPropertyValidator = mockValidationProvider.Object.GetPropertyValidatorBase(
                mockInternalEntityEntry.Object, propertyEntry);

            Assert.Same(propertyValidator, actualPropertyValidator);
        }

        [Fact]
        public void GetPropertyValidator_returns_null_if_entity_validator_does_not_exist()
        {
            var entity = new FlightSegmentWithNestedComplexTypes();
            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(entity);
            var propertyEntry = mockInternalEntityEntry.Object.Property("Departure");

            var propertyValidator = MockHelper.CreateMockValidationProvider().Object.GetPropertyValidatorBase(
                mockInternalEntityEntry.Object, propertyEntry);

            Assert.Null(propertyValidator);
        }

        [Fact]
        public void GetEntityValidationContext_returns_correct_context()
        {
            var mockInternalEntity = Internal.MockHelper.CreateMockInternalEntityEntry(new object());
            var items = new Dictionary<object, object>
                            {
                                { "test", 1 }
                            };

            var entityValidationContext = MockHelper.CreateMockValidationProvider().Object.GetEntityValidationContextBase(
                mockInternalEntity.Object, items);

            Assert.Equal(entityValidationContext.InternalEntity, mockInternalEntity.Object);

            var validationContext = entityValidationContext.ExternalValidationContext;
            Assert.NotNull(validationContext);
            Assert.Same(mockInternalEntity.Object.Entity, validationContext.ObjectInstance);
            Assert.Equal(1, validationContext.Items.Count);
            Assert.Equal(1, validationContext.Items["test"]);
        }

        [Fact]
        public void GetValidatorForProperty_returns_correct_validator_for_child_complex_property()
        {
            var entity = new DepartureArrivalInfoWithNestedComplexType
            {
                Airport = new AirportDetails(),
            };

            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(entity);
            var childPropertyEntry = mockInternalEntityEntry.Object.Property("Airport").Property("AirportCode");

            var childPropertyValidator = new PropertyValidator("AirportCode", new IValidator[0]);
            var complexPropertyValidator = new ComplexPropertyValidator(
                "Airport", new IValidator[0],
                new ComplexTypeValidator(new[] { childPropertyValidator }, new IValidator[0]));
            var entityValidator = new EntityValidator(new[] { complexPropertyValidator }, new IValidator[0]);

            var mockValidationProvider = MockHelper.CreateMockValidationProvider();

            mockValidationProvider.Protected()
                .Setup<PropertyValidator>("GetValidatorForProperty", ItExpr.IsAny<EntityValidator>(), ItExpr.IsAny<InternalMemberEntry>())
                .Returns<EntityValidator, InternalMemberEntry>((ev, e) => complexPropertyValidator);

            var actualPropertyValidator = mockValidationProvider.Object.GetValidatorForPropertyBase(entityValidator, childPropertyEntry);

            Assert.Same(childPropertyValidator, actualPropertyValidator);
        }

        [Fact]
        public void GetValidatorForProperty_returns_null_for_child_complex_property_if_no_child_property_validation()
        {
            var entity = new DepartureArrivalInfoWithNestedComplexType
            {
                Airport = new AirportDetails(),
            };

            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(entity);
            var childPropertyEntry = mockInternalEntityEntry.Object.Property("Airport").Property("AirportCode");

            var complexPropertyValidator = new ComplexPropertyValidator(
                "Airport", new IValidator[0],
                null);
            var entityValidator = new EntityValidator(new[] { complexPropertyValidator }, new IValidator[0]);

            var mockValidationProvider = MockHelper.CreateMockValidationProvider();

            mockValidationProvider.Protected()
                .Setup<PropertyValidator>("GetValidatorForProperty", ItExpr.IsAny<EntityValidator>(), ItExpr.IsAny<InternalMemberEntry>())
                .Returns<EntityValidator, InternalMemberEntry>((ev, e) => complexPropertyValidator);

            var actualPropertyValidator = mockValidationProvider.Object.GetValidatorForPropertyBase(entityValidator, childPropertyEntry);

            Assert.Null(actualPropertyValidator);
        }

        [Fact]
        public void GetValidatorForProperty_returns_null_for_child_complex_property_if_no_property_validation()
        {
            var entity = new DepartureArrivalInfoWithNestedComplexType
            {
                Airport = new AirportDetails(),
            };

            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(entity);
            var childPropertyEntry = mockInternalEntityEntry.Object.Property("Airport").Property("AirportCode");

            var entityValidator = new EntityValidator(new PropertyValidator[0], new IValidator[0]);

            var actualPropertyValidator = MockHelper.CreateMockValidationProvider().Object.GetValidatorForPropertyBase(
                entityValidator, childPropertyEntry);

            Assert.Null(actualPropertyValidator);
        }

        [Fact]
        public void GetValidatorForProperty_returns_correct_validator_for_scalar_property()
        {
            var entity = new FlightSegmentWithNestedComplexTypes();
            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(entity);
            var propertyEntry = mockInternalEntityEntry.Object.Property("FlightNumber");

            var propertyValidator = new PropertyValidator("FlightNumber", new IValidator[0]);
            var entityValidator = new EntityValidator(new[] { propertyValidator }, new IValidator[0]);

            var actualPropertyValidator = MockHelper.CreateMockValidationProvider().Object.GetValidatorForPropertyBase(entityValidator, propertyEntry);

            Assert.Same(propertyValidator, actualPropertyValidator);
        }
    }
}
