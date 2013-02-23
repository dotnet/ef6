// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.Validation;
    using System.Linq;
    using Moq;
    using Xunit;

    public class ComplexPropertyValidatorTests
    {
        [Fact]
        public void ComplexPropertyValidator_does_not_return_errors_if_complex_property_value_is_valid()
        {
            var entity = new FlightSegmentWithNestedComplexTypes
            {
                Departure = new DepartureArrivalInfoWithNestedComplexType
                {
                    Airport = new AirportDetails
                    {
                    },
                }
            };

            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(entity);
            var propertyValidator = new ComplexPropertyValidator(
                "Departure", new ValidationAttributeValidator[0],
                new ComplexTypeValidator(new PropertyValidator[0], new ValidationAttributeValidator[0]));

            var results = propertyValidator.Validate(
                MockHelper.CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Departure"));

            Assert.False(results.Any());
        }

        [Fact]
        public void ComplexPropertyValidator_does_not_return_errors_if_complex_type_validator_is_null()
        {
            var entity = new FlightSegmentWithNestedComplexTypesWithTypeLevelValidation
            {
                Departure = new DepartureArrivalInfoWithNestedComplexType
                {
                    Airport = new AirportDetails
                    {
                    },
                },
            };

            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(entity);
            var propertyValidator = new ComplexPropertyValidator(
                "Departure", new ValidationAttributeValidator[0],
                null);

            var results = propertyValidator.Validate(
                MockHelper.CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Departure"));

            Assert.False(results.Any());
        }

        [Fact]
        public void ComplexPropertyValidator_does_not_run_complex_type_validation_if_property_validation_failed()
        {
            var entity = new FlightSegmentWithNestedComplexTypes
            {
                Departure = new DepartureArrivalInfoWithNestedComplexType
                {
                    Airport = new AirportDetails
                    {
                        AirportCode = null,
                    },
                }
            };

            var mockValidator = new Mock<IValidator>();
            mockValidator
                .Setup(v => v.Validate(It.IsAny<EntityValidationContext>(), It.IsAny<InternalMemberEntry>()))
                .Returns(() => new[] { new DbValidationError("Airport", "error") });

            var mockComplexValidator = new Mock<IValidator>(MockBehavior.Strict);

            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(entity);
            var propertyValidator = new ComplexPropertyValidator(
                "Airport",
                new[]
                    {
                        mockValidator.Object
                    },
                new ComplexTypeValidator(
                    new[]
                        {
                            new PropertyValidator(
                                "AirportCode",
                                new[] { mockComplexValidator.Object })
                        }, new ValidationAttributeValidator[0]));

            var results = propertyValidator.Validate(
                MockHelper.CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Departure").Property("Airport"));

            Assert.Equal(1, results.Count());

            ValidationErrorHelper.VerifyResults(
                new[]
                    {
                        new Tuple<string, string>("Airport", "error")
                    }, results);
        }

        [Fact]
        public void ComplexPropertyValidator_does_not_run_complex_type_validation_if_property_is_null()
        {
            var entity = new EntityWithOptionalNestedComplexType
            {
                ID = 1
            };

            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(entity);
            var propertyValidator = new ComplexPropertyValidator(
                "AirportDetails", new ValidationAttributeValidator[0],
                new ComplexTypeValidator(
                    new[]
                        {
                            new PropertyValidator(
                                "AirportCode",
                                new[] { new ValidationAttributeValidator(new RequiredAttribute(), null) })
                        }, new ValidationAttributeValidator[0]));

            var results = propertyValidator.Validate(
                MockHelper.CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("AirportDetails"));

            Assert.False(results.Any());
        }
    }
}
