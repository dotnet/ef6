// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    public interface IRetryDelayStrategy
    {
        /// <summary>
        ///     Determines whether the action should be retried and the delay before the next attempt.
        /// </summary>
        /// <param name="lastException">The exception thrown during the last execution attempt.</param>
        /// <returns>
        ///     Returns the delay indicating how long to wait for before the next execution attempt if the action should be retried;
        ///     <c>null</c> otherwise
        /// </returns>
        TimeSpan? GetNextDelay(Exception lastException);
    }
}
