// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Minimizing changes, bugs tracking to refactor these.")]
    [Serializable]
    internal class CommandValidationFailedException : Exception
    {
        internal CommandValidationFailedException(string message)
            : base(message)
        {
        }
    }
}
