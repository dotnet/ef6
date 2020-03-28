// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using Xunit;

    public class DbEntityValidationResultTests
    {
        [Fact]
        public void DbEntityValidationResult_IsValid_true_if_no_validation_errors_occurred()
        {
            Assert.True(
                new DbEntityValidationResult(
                    new DbEntityEntry(new InternalEntityEntryForMock<object>()),
                    new List<DbValidationError>())
                    .IsValid);
        }

        [Fact]
        public void DbEntityValidationResult_IsValid_false_if_there_were_validation_errors()
        {
            Assert.False(
                new DbEntityValidationResult(
                    new DbEntityEntry(new InternalEntityEntryForMock<object>()),
                    new List<DbValidationError>(
                        new[] { new DbValidationError("property", "errormessage") }))
                    .IsValid);
        }
    }
}
