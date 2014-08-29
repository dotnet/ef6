// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    /// <summary>
    /// A simple logger for logging SQL and other database operations to the console or a file.
    /// A logger can be registered in code or in the application's web.config /app.config file.
    /// </summary>
    public class DatabaseLogger : IDisposable, IDbConfigurationInterceptor
    {
        private TextWriter _writer;
        private DatabaseLogFormatter _formatter;
        private readonly object _lock = new object();

        /// <summary>
        /// Creates a new logger that will send log output to the console.
        /// </summary>
        public DatabaseLogger()
        {
        }

        /// <summary>
        /// Creates a new logger that will send log output to a file. If the file already exists then
        /// it is overwritten.
        /// </summary>
        /// <param name="path">A path to the file to which log output will be written.</param>
        public DatabaseLogger(string path)
            : this(path, append: false)
        {
        }

        /// <summary>
        /// Creates a new logger that will send log output to a file.
        /// </summary>
        /// <param name="path">A path to the file to which log output will be written.</param>
        /// <param name="append">True to append data to the file if it exists; false to overwrite the file.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public DatabaseLogger(string path, bool append)
        {
            Check.NotEmpty(path, "path");

            _writer = new StreamWriter(path, append) { AutoFlush = true };
        }

        /// <summary>
        /// Stops logging and closes the underlying file if output is being written to a file.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Stops logging and closes the underlying file if output is being written to a file.
        /// </summary>
        /// <param name="disposing">
        /// True to release both managed and unmanaged resources; False to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            StopLogging();

            if (disposing && _writer != null)
            {
                _writer.Dispose();
                _writer = null;
            }
        }

        /// <summary>
        /// Starts logging. This method is a no-op if logging is already started.
        /// </summary>
        public void StartLogging()
        {
            StartLogging(DbConfiguration.DependencyResolver);
        }

        /// <summary>
        /// Stops logging. This method is a no-op if logging is not started.
        /// </summary>
        public void StopLogging()
        {
            if (_formatter != null)
            {
                DbInterception.Remove(_formatter);
                _formatter = null;
            }
        }

        /// <summary>
        /// Called to start logging during Entity Framework initialization when this logger is registered.
        /// as an <see cref="IDbInterceptor"/>. 
        /// </summary>
        /// <param name="loadedEventArgs">Arguments to the event that this interceptor mirrors.</param>
        /// <param name="interceptionContext">Contextual information about the event.</param>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void IDbConfigurationInterceptor.Loaded(
            DbConfigurationLoadedEventArgs loadedEventArgs,
            DbConfigurationInterceptionContext interceptionContext)
        {
            Check.NotNull(loadedEventArgs, "loadedEventArgs");
            Check.NotNull(interceptionContext, "interceptionContext");

            StartLogging(loadedEventArgs.DependencyResolver);
        }

        private void StartLogging(IDbDependencyResolver resolver)
        {
            DebugCheck.NotNull(resolver);

            if (_formatter == null)
            {
                _formatter = resolver.GetService<Func<DbContext, Action<string>, DatabaseLogFormatter>>()(
                    null, _writer == null ? (Action<string>)Console.Write : WriteThreadSafe);

                DbInterception.Add(_formatter);
            }
        }

        private void WriteThreadSafe(string value)
        {
            lock (_lock)
            {
                _writer.Write(value);
            }
        }
    }
}
