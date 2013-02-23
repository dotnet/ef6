// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using System.Data.Entity.Internal.Validation;
    using Xunit;

    public class ComplexTypeValidatorTests
    {
        [Fact]
        public void ComplexTypeValidator_returns_an_error_if_IValidatableObject_validation_failed()
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

            var entityValidator = new ComplexTypeValidator(
                new PropertyValidator[0],
                new[] { MockHelper.CreateValidatableObjectValidator("object", "IValidatableObject is invalid") });

            var entityValidationResult = entityValidator.Validate(
                MockHelper.CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Departure").Property("Airport").Property("AirportCode"));

            Assert.NotNull(entityValidationResult);

            ValidationErrorHelper.VerifyResults(
                new[] { new Tuple<string, string>("object", "IValidatableObject is invalid") }, entityValidationResult);
        }
    }
}
