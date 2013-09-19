// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Utilities;

    /// <summary>
    /// This class encapsulates the error information for a generic EDM error.
    /// </summary>
    [Serializable]
    public abstract class EdmError
    {
        private readonly string _message;

        /// <summary>
        /// Constructs a EdmSchemaError object.
        /// </summary>
        /// <param name="message"> The explanation of the error. </param>
        internal EdmError(string message)
        {
            Check.NotEmpty(message, "message");
            _message = message;
        }

        /// <summary>Gets the error message.</summary>
        /// <returns>The error message.</returns>
        public string Message
        {
            get { return _message; }
        }
    }
}
