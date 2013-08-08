// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Internal.Validation;
    using System.Data.Entity.Resources;
    using Xunit;

    public class ValidationAttributeValidatorTests : TestBase
    {
        #region Helpers

        private readonly string RegexAttribute_ValidationError = LookupString
            (SystemComponentModelDataAnnotationsAssembly, "System.ComponentModel.DataAnnotations.Resources.DataAnnotationsResources",
                "RegexAttribute_ValidationError");

        private readonly string StringLengthAttribute_ValidationError = LookupString
            (SystemComponentModelDataAnnotationsAssembly, "System.ComponentModel.DataAnnotations.Resources.DataAnnotationsResources",
                "StringLengthAttribute_ValidationError");

        public static ValidationResult FailMiserably(object entity, ValidationContext validationContex)
        {
            return new ValidationResult("The entity is not valid");
        }

        #endregion

        [Fact]
        public void ValidationAttributeValidator_does_not_return_errors_if_property_value_is_valid()
        {
            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(
                new Dictionary<string, object>
                    {
                        { "Name", "abc" }
                    });

            var ValidationAttributeValidator = new ValidationAttributeValidator(new StringLengthAttribute(10), null);

            var results = ValidationAttributeValidator.Validate(
                MockHelper.CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Name"));

            Assert.False(results.Any());
        }

        [Fact]
        public void ValidationAttributeValidator_returns_validation_errors_if_property_value_is_not_valid()
        {
            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(
                new Dictionary<string, object>
                    {
                        { "Name", "abcdefghijklmnopq" }
                    });

            var ValidationAttributeValidator = new ValidationAttributeValidator(new StringLengthAttribute(10), null);

            var results = ValidationAttributeValidator.Validate(
                MockHelper.CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Name"));

            Assert.Equal(1, results.Count());
            var error = results.Single();
            Assert.Equal("Name", error.PropertyName);
            Assert.Equal(string.Format(StringLengthAttribute_ValidationError, "Name", 10), error.ErrorMessage);
        }

        [Fact]
        public void ValidationAttributeValidator_returns_errors_for_invalid_complex_property_child_property_values()
        {
            var entity = new FlightSegmentWithNestedComplexTypes
            {
                Departure = new DepartureArrivalInfoWithNestedComplexType
                {
                    Airport = new AirportDetails
                    {
                        AirportCode = "???",
                    }
                }
            };

            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(entity);

            var validator = new ValidationAttributeValidator(new RegularExpressionAttribute("^[A-Z]{3}$"), null);

            var results = validator.Validate(
                MockHelper.CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Departure").Property("Airport").Property("AirportCode"));

            ValidationErrorHelper.VerifyResults(
                new[]
                    {
                        new Tuple<string, string>(
                            "Departure.Airport.AirportCode",
                            string.Format(RegexAttribute_ValidationError, "Departure.Airport.AirportCode", "^[A-Z]{3}$"))
                    }, results);
        }

        [Fact]
        public void ValidationAttributeValidator_returns_errors_if_complex_property_type_level_validation_fails()
        {
            var entity = new FlightSegmentWithNestedComplexTypes
            {
                Departure = new DepartureArrivalInfoWithNestedComplexType
                {
                    Airport = new AirportDetails
                    {
                        AirportCode = "YVR",
                        CityCode = "YVR",
                        CountryOrRegionCode = "ZZ"
                    }
                }
            };

            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(entity);

            var validator = new ValidationAttributeValidator(new CustomValidationAttribute(typeof(AirportDetails), "ValidateCountryOrRegion"), null);

            var results = validator.Validate(
                MockHelper.CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Departure").Property("Airport"));

            ValidationErrorHelper.VerifyResults(
                new[]
                    {
                        new Tuple<string, string>("Departure.Airport", "City 'YVR' is not located in country or region 'ZZ'.")
                    }, results);
        }

        [Fact]
        public void ValidationAttributeValidator_returns_validation_errors_if_entity_validation_with_type_level_annotation_attributes_fails()
        {
            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(
                new Dictionary<string, object>
                    {
                        { "Name", "abcdefghijklmnopq" }
                    });
            mockInternalEntityEntry.Setup(e => e.Entity).Returns(new object());

            var ValidationAttributeValidator = new ValidationAttributeValidator(
                new CustomValidationAttribute(GetType(), "FailMiserably"), null);

            var results = ValidationAttributeValidator.Validate(
                MockHelper.CreateEntityValidationContext(mockInternalEntityEntry.Object), null);

            Assert.Equal(1, results.Count());
            var error = results.Single();
            Assert.Equal(null, error.PropertyName);
            Assert.Equal("The entity is not valid", error.ErrorMessage);
        }

        [Fact]
        public void ValidationAttributeValidator_wraps_exceptions()
        {
            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(
                new Dictionary<string, object>
                    {
                        { "ID", 1 }
                    });

            var ValidationAttributeValidator = new ValidationAttributeValidator(new StringLengthAttribute(10), null);

            Assert.Equal(
                new DbUnexpectedValidationException(
                    Strings.DbUnexpectedValidationException_ValidationAttribute(
                        "ID", "System.ComponentModel.DataAnnotations.StringLengthAttribute")).Message,
                Assert.Throws<DbUnexpectedValidationException>(
                    () => ValidationAttributeValidator.Validate(
                        MockHelper.CreateEntityValidationContext(mockInternalEntityEntry.Object),
                        mockInternalEntityEntry.Object.Property("ID"))).Message);
        }
    }
}
