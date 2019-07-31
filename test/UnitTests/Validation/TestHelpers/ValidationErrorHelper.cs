// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    internal class ValidationErrorHelper
    {
        internal static void VerifyResults(Tuple<string, string>[] expectedResults, IEnumerable<DbValidationError> actualResults)
        {
            Assert.Equal(expectedResults.Count(), actualResults.Count());

            foreach (var validationError in actualResults)
            {
                Assert.True(
                    expectedResults.SingleOrDefault(
                        r => r.Item1 == validationError.PropertyName && r.Item2 == validationError.ErrorMessage) != null,
                    String.Format(
                        "Unexpected error message '{0}' for property '{1}' not found", validationError.ErrorMessage,
                        validationError.PropertyName));
            }
        }
    }
}
