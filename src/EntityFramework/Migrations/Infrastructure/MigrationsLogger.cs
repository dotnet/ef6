// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    /// <summary>
    /// Base class for loggers that can be used for the migrations process.
    /// </summary>
    public abstract class MigrationsLogger : MarshalByRefObject
    {
        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message"> The message to be logged. </param>
        public abstract void Info(string message);

        /// <summary>
        /// Logs a warning that the user should be made aware of.
        /// </summary>
        /// <param name="message"> The message to be logged. </param>
        public abstract void Warning(string message);

        /// <summary>
        /// Logs some additional information that should only be presented to the user if they request verbose output.
        /// </summary>
        /// <param name="message"> The message to be logged. </param>
        public abstract void Verbose(string message);
    }
}
