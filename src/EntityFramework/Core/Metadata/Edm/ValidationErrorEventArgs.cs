// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    /// <summary>
    ///     Class representing a validtion error event args
    /// </summary>
    internal class ValidationErrorEventArgs : EventArgs
    {
        private readonly EdmItemError _validationError;

        /// <summary>
        ///     Construct the validation error event args with a validation error object
        /// </summary>
        /// <param name="validationError"> The validation error object for this event args </param>
        public ValidationErrorEventArgs(EdmItemError validationError)
        {
            _validationError = validationError;
        }

        /// <summary>
        ///     Gets the validation error object this event args
        /// </summary>
        public EdmItemError ValidationError
        {
            get { return _validationError; }
        }
    }
}
