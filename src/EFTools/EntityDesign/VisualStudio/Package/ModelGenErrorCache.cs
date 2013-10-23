// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;

    internal class ModelGenErrorCache
    {
        private readonly Dictionary<string, List<EdmSchemaError>> _errors;

        internal ModelGenErrorCache()
        {
            _errors = new Dictionary<string, List<EdmSchemaError>>();
        }

        // virtual to allow mocking
        internal virtual void AddErrors(string fileName, List<EdmSchemaError> errors)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(fileName), "invalid file name");
            Debug.Assert(errors != null && errors.Any(), "expected non-empty error collection");

            _errors[fileName] = errors;
        }

        // virtual to allow mocking
        internal virtual void RemoveErrors(string fileName)
        {
            _errors.Remove(fileName);
        }

        internal List<EdmSchemaError> GetErrors(string fileName)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(fileName), "invalid file name");

            List<EdmSchemaError> errors;
            _errors.TryGetValue(fileName, out errors);
            return errors;
        }
    }
}
