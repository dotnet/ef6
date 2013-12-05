// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.ComponentModel.DataAnnotations.Schema
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Returned by <see cref="IndexAttribute.IsCompatibleWith"/> and related methods to indicate whether or
    /// not one object does not conflict with another such that the two can be combined into one.
    /// </summary>
    /// <remarks>
    /// If the two objects are not compatible then information about why they are not compatible is contained
    /// in the <see cref="ErrorMessage"/> property.
    /// </remarks>
    public sealed class CompatibilityResult
    {
        private readonly bool _isCompatible;
        private readonly string _errorMessage;

        /// <summary>
        /// Creates a new <see cref="CompatibilityResult"/> instance.
        /// </summary>
        /// <param name="isCompatible">Indicates whether or not the two tested objects are compatible.</param>
        /// <param name="errorMessage">
        /// An error message indicating how the objects are not compatible. Expected to be null if isCompatible is true.
        /// </param>
        public CompatibilityResult(bool isCompatible, string errorMessage)
        {
            _isCompatible = isCompatible;
            _errorMessage = errorMessage;

            if (!isCompatible)
            {
                Check.NotEmpty(errorMessage, "errorMessage");
            }

            Debug.Assert((isCompatible && errorMessage == null) || (!isCompatible && !string.IsNullOrWhiteSpace(errorMessage)));
        }

        /// <summary>
        /// True if the two tested objects are compatible; otherwise false.
        /// </summary>
        public bool IsCompatible
        {
            get { return _isCompatible; }
        }

        /// <summary>
        /// If <see cref="IsCompatible"/> is true, then returns an error message indicating how the two tested objects
        /// are incompatible.
        /// </summary>
        public string ErrorMessage
        {
            get { return _errorMessage; }
        }

        /// <summary>
        /// Implicit conversion to a bool to allow the result object to be used directly in checks.
        /// </summary>
        /// <param name="result">The object to convert.</param>
        /// <returns>True if the result is compatible; false otherwise.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static implicit operator bool(CompatibilityResult result)
        {
            Check.NotNull(result, "result");

            return result._isCompatible;
        }
    }
}
