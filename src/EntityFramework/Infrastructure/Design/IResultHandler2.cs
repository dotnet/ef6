// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Design
{
    public interface IResultHandler2 : IResultHandler
    {
        /// <summary>
        ///     Invoked when an error occurs.
        /// </summary>
        /// <param name="type"> The exception type. </param>
        /// <param name="message"> The error message. </param>
        /// <param name="stackTrace"> The stack trace. </param>
        /// <returns>true if the error was handled; otherwise, false.</returns>
        void SetError(string type, string message, string stackTrace);
    }
}
