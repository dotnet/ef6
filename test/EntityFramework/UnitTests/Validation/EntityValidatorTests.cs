// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.Validation;
    using System.Linq;
    using Moq;
    using Xunit;

    public class EntityValidatorTests
    {
        [Fact]
        public void EntityValidator_does_not_return_error_if_entity_is_valid()
        {
            var mockValidator = new Mock<IValidator>();
            mockValidator
                .Setup(v => v.Validate(It.IsAny<EntityValidationContext>(), It.IsAny<InternalMemberEntry>()))
                .Returns(() => Enumerable.Empty<DbValidationError>());

            var entityValidator = new EntityValidator(
                new[]
                    {
                        new PropertyValidator(
                            "Name",
                            new[] { mockValidator.Object })
                    }, new ValidationAttributeValidator[0]);

            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(
                new Dictionary<string, object>
                    {
                        { "Name", "abc" }
                    });

            var validationResult = entityValidator.Validate(MockHelper.CreateEntityValidationContext(mockInternalEntityEntry.Object));
            Assert.NotNull(validationResult);
            Assert.True(validationResult.IsValid);
        }

        [Fact]
        public void EntityValidator_does_not_run_other_validation_if_property_validation_failed()
        {
            var mockValidator = new Mock<IValidator>();
            mockValidator
                .Setup(v => v.Validate(It.IsAny<EntityValidationContext>(), It.IsAny<InternalMemberEntry>()))
                .Returns(() => new[] { new DbValidationError("ID", "error") });

            var mockUncalledValidator = new Mock<IValidator>(MockBehavior.Strict);

            var entityValidator = new EntityValidator(
                new[]
                    {
                        new PropertyValidator(
                            "ID",
                            new[] { mockValidator.Object })
                    },
                new[] { mockUncalledValidator.Object });

            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(
                new Dictionary<string, object>
                    {
                        { "ID", -1 }
                    });

            var entityValidationResult = entityValidator.Validate(MockHelper.CreateEntityValidationContext(mockInternalEntityEntry.Object));

            Assert.NotNull(entityValidationResult);

            ValidationErrorHelper.VerifyResults(
                new[] { new Tuple<string, string>("ID", "error") }, entityValidationResult.ValidationErrors);
        }

        [Fact]
        public void EntityValidator_returns_an_error_if_IValidatableObject_validation_failed()
        {
            var entity = new FlightSegmentWithNestedComplexTypes
            {
            };

            var mockInternalEntityEntry = Internal.MockHelper.CreateMockInternalEntityEntry(entity);

            var entityValidator = new EntityValidator(
                new PropertyValidator[0],
                new[] { MockHelper.CreateValidatableObjectValidator("object", "IValidatableObject is invalid") });

            var entityValidationResult = entityValidator.Validate(MockHelper.CreateEntityValidationContext(mockInternalEntityEntry.Object));

            Assert.NotNull(entityValidationResult);

            ValidationErrorHelper.VerifyResults(
                new[] { new Tuple<string, string>("object", "IValidatableObject is invalid") }, entityValidationResult.ValidationErrors);
        }
    }
}
