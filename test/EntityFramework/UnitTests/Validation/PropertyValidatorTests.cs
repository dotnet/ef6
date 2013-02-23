// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.Validation;
    using System.Linq;
    using Moq;
    using Xunit;

    public class PropertyValidatorTests
    {
        [Fact]
        public void PropertyValidator_does_not_return_errors_if_primitive_property_value_is_valid()
        {
            var mockValidator = new Mock<IValidator>();
            mockValidator
                .Setup(v => v.Validate(It.IsAny<EntityValidationContext>(), It.IsAny<InternalMemberEntry>()))
                .Returns(() => Enumerable.Empty<DbValidationError>());
            var propertyValidator = new PropertyValidator(
                "Name",
                new[] { mockValidator.Object });

            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(
                new Dictionary<string, object>
                    {
                        { "Name", "abc" }
                    });

            var results = propertyValidator.Validate(
                MockHelper.CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Member("Name"));

            Assert.False(results.Any());
        }

        [Fact]
        public void PropertyValidator_returns_validation_errors_if_primitive_property_value_is_not_valid()
        {
            var mockValidator = new Mock<IValidator>();
            mockValidator
                .Setup(v => v.Validate(It.IsAny<EntityValidationContext>(), It.IsAny<InternalMemberEntry>()))
                .Returns(() => new[] { new DbValidationError("Name", "error") });

            var propertyValidator = new PropertyValidator(
                "Name",
                new[] { mockValidator.Object });

            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(
                new Dictionary<string, object>
                    {
                        { "Name", "" }
                    });

            var results = propertyValidator.Validate(
                MockHelper.CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Member("Name"));

            ValidationErrorHelper.VerifyResults(
                new[] { new Tuple<string, string>("Name", "error") },
                results);
        }
    }
}
