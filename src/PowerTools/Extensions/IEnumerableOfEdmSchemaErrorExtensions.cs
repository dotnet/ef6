// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Extensions
{
    using System.Collections.Generic;
    using System.Data.Metadata.Edm;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static class IEnumerableOfEdmSchemaErrorExtensions
    {
        public static void HandleErrors(this IEnumerable<EdmSchemaError> errors, string message)
        {
            Contract.Requires(errors != null);

            if (errors.HasErrors())
            {
                throw new EdmSchemaErrorException(message, errors);
            }
        }

        private static bool HasErrors(this IEnumerable<EdmSchemaError> errors)
        {
            Contract.Requires(errors != null);

            return errors.Any(e => e.Severity == EdmSchemaErrorSeverity.Error);
        }
    }
}
