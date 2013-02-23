// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Internal.Validation;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class ValidatableObjectValidatorTests
    {
        [Fact]
        public void ValidatableObjectValidator_returns_errors_if_entity_IValidatableObject_validation_fails()
        {
            var entity = new FlightSegmentWithNestedComplexTypesWithTypeLevelValidation
            {
                Aircraft = new AircraftInfo
                {
                    Code = "A380"
                },
                FlightNumber = "QF0006"
            };

            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(entity);

            var validator = new ValidatableObjectValidator(null);

            var results = validator.Validate(MockHelper.CreateEntityValidationContext(mockInternalEntityEntry.Object), null);

            ValidationErrorHelper.VerifyResults(
                new[]
                    {
                        new Tuple<string, string>("Aircraft.Code", "Your trip may end in Singapore."),
                        new Tuple<string, string>("FlightNumber", "Your trip may end in Singapore.")
                    }, results);
        }

        [Fact]
        public void ValidatableObjectValidator_returns_errors_if_complex_property_IValidatableObject_validation_fails()
        {
            var entity = new FlightSegmentWithNestedComplexTypes
            {
                Departure = new DepartureArrivalInfoWithNestedComplexType
                {
                    Time = DateTime.MinValue
                }
            };

            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(entity);

            var validator = new ValidatableObjectValidator(null);

            var results = validator.Validate(
                MockHelper.CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Departure"));

            ValidationErrorHelper.VerifyResults(new[] { new Tuple<string, string>("Departure", "Date cannot be in the past.") }, results);
        }

        [Fact]
        public void ValidatableObjectValidator_returns_empty_enumerator_if_complex_property_IValidatableObject_validation_returns_null()
        {
            var entity = new FlightSegmentWithNestedComplexTypes
            {
                Departure = new DepartureArrivalInfoWithNestedComplexType
                {
                    ValidationResults = new[] { ValidationResult.Success, null }
                }
            };

            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(entity);

            var validator = new ValidatableObjectValidator(null);

            var results = validator.Validate(
                MockHelper.CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Departure"));

            ValidationErrorHelper.VerifyResults(new Tuple<string, string>[0], results);
        }

        [Fact]
        public void ValidatableObjectValidator_does_not_return_errors_for_null_complex_property_with_IValidatableObject_validation()
        {
            var entity = new FlightSegmentWithNestedComplexTypes
            {
            };

            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(entity);

            var validator = new ValidatableObjectValidator(null);

            var results = validator.Validate(
                MockHelper.CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Departure"));

            Assert.False(results.Any());
        }

        [Fact]
        public void ValidatableObjectValidator_wraps_exceptions()
        {
            var entity = new DepartureArrivalInfoWithNestedComplexType
            {
                Airport = new AirportDetails(),
            };
            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(entity);

            var validator = new ValidatableObjectValidator(
                new DisplayAttribute
                {
                    Name = "Airport information"
                });

            Assert.Equal(
                    Strings.DbUnexpectedValidationException_IValidatableObject("Airport information", typeof(AirportDetails).FullName),
                    Assert.Throws<DbUnexpectedValidationException>(
                        () => validator.Validate(
                            MockHelper.CreateEntityValidationContext(mockInternalEntityEntry.Object),
                            mockInternalEntityEntry.Object.Property("Airport"))).Message);
        }
    }
}
