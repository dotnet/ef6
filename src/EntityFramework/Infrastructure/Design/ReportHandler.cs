// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Design
{
    /// <summary>
    /// Used to handle reported design-time activity.
    /// </summary>
    public class ReportHandler : HandlerBase, IReportHandler
    {
        private readonly Action<string> _errorHandler;
        private readonly Action<string> _warningHandler;
        private readonly Action<string> _informationHandler;
        private readonly Action<string> _verboseHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReportHandler" /> class.
        /// </summary>
        /// <param name="errorHandler"> A callback for <see cref="OnError" />. </param>
        /// <param name="warningHandler"> A callback for <see cref="OnWarning" />. </param>
        /// <param name="informationHandler"> A callback for <see cref="OnInformation" />. </param>
        /// <param name="verboseHandler"> A callback for <see cref="OnVerbose" />. </param>
        public ReportHandler(
            Action<string> errorHandler,
            Action<string> warningHandler,
            Action<string> informationHandler,
            Action<string> verboseHandler)
        {
            _errorHandler = errorHandler;
            _warningHandler = warningHandler;
            _informationHandler = informationHandler;
            _verboseHandler = verboseHandler;
        }

        /// <summary>
        /// Invoked when an error is reported.
        /// </summary>
        /// <param name="message">The message.</param>
        public virtual void OnError(string message)
            => _errorHandler?.Invoke(message);

        /// <summary>
        /// Invoked when a warning is reported.
        /// </summary>
        /// <param name="message">The message.</param>
        public virtual void OnWarning(string message)
            => _warningHandler?.Invoke(message);

        /// <summary>
        /// Invoked when information is reported.
        /// </summary>
        /// <param name="message">The message.</param>
        public virtual void OnInformation(string message)
            => _informationHandler?.Invoke(message);

        /// <summary>
        /// Invoked when verbose information is reported.
        /// </summary>
        /// <param name="message">The message.</param>
        public virtual void OnVerbose(string message)
            => _verboseHandler?.Invoke(message);
    }
}
