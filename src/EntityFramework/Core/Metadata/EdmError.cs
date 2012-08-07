// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    /// <summary>
    ///     This class encapsulates the error information for a generic EDM error.
    /// </summary>
    [Serializable]
    public abstract class EdmError
    {
        #region Instance Fields

        private readonly string _message;

        #endregion

        #region Constructors

        /// <summary>
        ///     Constructs a EdmSchemaError object.
        /// </summary>
        /// <param name="message"> The explanation of the error. </param>
        /// <param name="errorCode"> The code associated with this error. </param>
        /// <param name="severity"> The severity of the error. </param>
        internal EdmError(string message)
        {
            EntityUtil.CheckStringArgument(message, "message");
            _message = message;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the error message.
        /// </summary>
        public string Message
        {
            get { return _message; }
        }

        #endregion
    }
}
