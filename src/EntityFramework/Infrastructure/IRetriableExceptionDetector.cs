// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    public interface IRetriableExceptionDetector
    {
        /// <summary>
        ///     Determines whether the specified exception represents a transient failure that can be compensated by a retry.
        /// </summary>
        /// <param name="ex">The exception object to be verified.</param>
        /// <returns><c>true</c> if the specified exception is considered as transient, otherwise <c>false</c>.</returns>
        bool ShouldRetryOn(Exception ex);
    }
}
