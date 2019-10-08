// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Design
{
    /// <summary>
    /// Used to handle reported design-time activity.
    /// </summary>
    public interface IReportHandler
    {
        /// <summary>
        /// Invoked when an error is reported.
        /// </summary>
        /// <param name="message">The message.</param>
        void OnError(string message);

        /// <summary>
        /// Invoked when a warning is reported.
        /// </summary>
        /// <param name="message">The message.</param>
        void OnWarning(string message);

        /// <summary>
        /// Invoked when information is reported.
        /// </summary>
        /// <param name="message">The message.</param>
        void OnInformation(string message);

        /// <summary>
        /// Invoked when verbose information is reported.
        /// </summary>
        /// <param name="message">The message.</param>
        void OnVerbose(string message);
    }
}
