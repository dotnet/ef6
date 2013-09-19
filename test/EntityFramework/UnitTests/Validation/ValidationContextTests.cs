// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.Validation;
    using System.Linq;
    using Moq;
    using Xunit;

    public class ValidationContextTests
    {
        [Fact]
        public void Verify_custom_validation_items_dictionary_gets_to_validator()
        {
            var mockIValidator = new Mock<IValidator>();
            mockIValidator.Setup(v => v.Validate(It.IsAny<EntityValidationContext>(), It.IsAny<InternalPropertyEntry>()))
                .Returns(
                    (EntityValidationContext ctx, InternalPropertyEntry property) =>
                    {
                        var validationContext = ctx.ExternalValidationContext;
                        Assert.NotNull(validationContext);
                        Assert.NotNull(validationContext.Items);
                        Assert.Equal(1, validationContext.Items.Count);
                        Assert.Equal(1, validationContext.Items["test"]);

                        return Enumerable.Empty<DbValidationError>();
                    });

            var mockValidationProvider = MockHelper.CreateMockValidationProvider();
            mockValidationProvider.Setup(
                p => p.GetEntityValidator(It.IsAny<InternalEntityEntry>()))
                .Returns(new EntityValidator(Enumerable.Empty<PropertyValidator>(), new[] { mockIValidator.Object }));
            mockValidationProvider.CallBase = true;

            var mockInternalEntity = new Mock<InternalEntityEntryForMock<object>>();
            var mockInternalContext = Mock.Get((InternalContextForMock)mockInternalEntity.Object.InternalContext);
            mockInternalContext.SetupGet(c => c.ValidationProvider).Returns(mockValidationProvider.Object);

            var items = new Dictionary<object, object>
                            {
                                { "test", 1 }
                            };

            // GetValidationResult on entity
            mockInternalEntity.Object.GetValidationResult(items);
        }

        [Fact]
        public void Verify_custom_validation_items_dictionary_is_not_null_by_default()
        {
            var mockInternalEntity = new Mock<InternalEntityEntryForMock<object>>();
            mockInternalEntity.Setup(e => e.GetValidationResult(It.IsAny<Dictionary<object, object>>()))
                .Returns(new DbEntityValidationResult(mockInternalEntity.Object, Enumerable.Empty<DbValidationError>()));
            mockInternalEntity.CallBase = true;
            Mock.Get((InternalContextForMock)mockInternalEntity.Object.InternalContext).CallBase = true;
            Mock.Get(mockInternalEntity.Object.InternalContext.Owner).CallBase = true;

            new DbEntityEntry(mockInternalEntity.Object).GetValidationResult();

            mockInternalEntity.Verify(e => e.GetValidationResult(null), Times.Never());
            mockInternalEntity.Verify(e => e.GetValidationResult(It.IsAny<Dictionary<object, object>>()), Times.Once());
        }
    }
}
