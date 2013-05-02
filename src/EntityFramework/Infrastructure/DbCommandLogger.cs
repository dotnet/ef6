// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Config;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    ///     This is the default logger used when some <see cref="TextWriter" /> is set onto the <see cref="Database.Log" />
    ///     property. A different logger can be used by creating a class that inherits from this class and overrides
    ///     some or all methods to change behavior.
    /// </summary>
    /// <remarks>
    ///     To set the new logger create a code-based configuration for EF using <see cref="DbConfiguration" /> and then
    ///     set the logger class to use with <see cref="DbConfiguration.SetCommandLogger" />.
    ///     Note that setting the type of logger to use with this method does change the way command are
    ///     logged when <see cref="Database.Log" />is used. It is still necessary to set a <see cref="TextWriter" />
    ///     instance onto <see cref="Database.Log" /> before any commands will be logged.
    ///     For more low-level control over logging/interception see <see cref="IDbCommandInterceptor" /> and
    ///     <see cref="Interception" />.
    /// </remarks>
    public class DbCommandLogger : IDbCommandInterceptor
    {
        private readonly DbContext _context;
        private readonly TextWriter _writer;

        /// <summary>
        ///     Creates a logger that will not filter by any <see cref="DbContext" /> and will instead log every command
        ///     from any context and also commands that do not originate from a context.
        /// </summary>
        /// <remarks>
        ///     This constructor is not used when a writer is set on <see cref="Database.Log" />. Instead it can be
        ///     used by setting the logger directly using <see cref="Interception.AddInterceptor" />.
        /// </remarks>
        /// <param name="writer">The writer to which commands will be logged.</param>
        public DbCommandLogger(TextWriter writer)
        {
            Check.NotNull(writer, "writer");

            _writer = writer;
        }

        /// <summary>
        ///     Creates a logger that will only log commands the come from the given <see cref="DbContext" /> instance.
        /// </summary>
        /// <remarks>
        ///     This constructor must be called by a class that inherits from this class to override the behavior
        ///     of <see cref="Database.Log" />.
        /// </remarks>
        /// <param name="context">The context for which commands should be logged.</param>
        /// <param name="writer">The writer to which commands will be logged.</param>
        public DbCommandLogger(DbContext context, TextWriter writer)
        {
            Check.NotNull(context, "context");
            Check.NotNull(writer, "writer");

            _context = context;
            _writer = writer;
        }

        /// <summary>
        ///     The context for which commands are being logged, or null if commands from all contexts are
        ///     being logged.
        /// </summary>
        public DbContext Context
        {
            get { return _context; }
        }

        /// <summary>
        ///     The writer to which commands are being logged.
        /// </summary>
        public TextWriter Writer
        {
            get { return _writer; }
        }

        /// <summary>
        ///     This method is called before a call to <see cref="DbCommand.ExecuteNonQuery" /> or
        ///     one of its async counterparts is made.
        ///     The default implementation calls <see cref="Executing" />
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        public virtual void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            Executing(command, interceptionContext);
        }

        /// <summary>
        ///     This method is called after a call to <see cref="DbCommand.ExecuteNonQuery" />  or
        ///     one of its async counterparts is made.
        ///     The default implementation calls <see cref="Executed" />
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="result">The result of the command execution.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <returns>The result to be used by Entity Framework.</returns>
        public virtual int NonQueryExecuted(DbCommand command, int result, DbCommandInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            Executed(command, result, interceptionContext);
            return result;
        }

        /// <summary>
        ///     This method is called before a call to <see cref="DbCommand.ExecuteReader(CommandBehavior)" />  or
        ///     one of its async counterparts is made.
        ///     The default implementation calls <see cref="Executing" />
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        public virtual void ReaderExecuting(DbCommand command, DbCommandInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            Executing(command, interceptionContext);
        }

        /// <summary>
        ///     This method is called after a call to <see cref="DbCommand.ExecuteReader(CommandBehavior)" />  or
        ///     one of its async counterparts is made.
        ///     The default implementation calls <see cref="Executed" />
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="result">The result of the command execution.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <returns>The result to be used by Entity Framework.</returns>
        public virtual DbDataReader ReaderExecuted(DbCommand command, DbDataReader result, DbCommandInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            Executed(command, result, interceptionContext);
            return result;
        }

        /// <summary>
        ///     This method is called before a call to <see cref="DbCommand.ExecuteScalar" />  or
        ///     one of its async counterparts is made.
        ///     The default implementation calls <see cref="Executing" />
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        public virtual void ScalarExecuting(DbCommand command, DbCommandInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            Executing(command, interceptionContext);
        }

        /// <summary>
        ///     This method is called after a call to <see cref="DbCommand.ExecuteScalar" />  or
        ///     one of its async counterparts is made.
        ///     The default implementation calls <see cref="Executed" />
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="result">The result of the command execution.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <returns>The result to be used by Entity Framework.</returns>
        public virtual object ScalarExecuted(DbCommand command, object result, DbCommandInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            Executed(command, result, interceptionContext);
            return result;
        }

        /// <summary>
        ///     Called whenever a command is about to be executed. The default implementation of this method
        ///     filters by <see cref="DbContext" /> set into <see cref="Context" />, if any, and then calls
        ///     <see cref="LogCommand" />. This method would typically only be overridden to change the
        ///     context filtering behavior.
        /// </summary>
        /// <param name="command">The command that will be executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the command.</param>
        public virtual void Executing(DbCommand command, DbCommandInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            if (_context == null
                || interceptionContext.DbContexts.Contains(_context, ReferenceEquals))
            {
                LogCommand(command, interceptionContext);
            }
        }

        /// <summary>
        ///     Called whenever a command has completed executing. The default implementation of this method
        ///     filters by <see cref="DbContext" /> set into <see cref="Context" />, if any, and then calls
        ///     <see cref="LogResult" />. This method would typically only be overridden to change the context
        ///     filtering behavior.
        /// </summary>
        /// <param name="command">The command that was executed.</param>
        /// <param name="result">The result of executing the command.</param>
        /// <param name="interceptionContext">Contextual information associated with the command.</param>
        public virtual void Executed(DbCommand command, object result, DbCommandInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            if (_context == null
                || interceptionContext.DbContexts.Contains(_context, ReferenceEquals))
            {
                LogResult(command, result, interceptionContext);
            }
        }

        /// <summary>
        ///     Called to log a command that is about to be executed. Override this method to change how the
        ///     command is logged to <see cref="Writer" />.
        /// </summary>
        /// <param name="command">The command to be logged.</param>
        /// <param name="interceptionContext">Contextual information associated with the command.</param>
        public virtual void LogCommand(DbCommand command, DbCommandInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            if (command.CommandText.EndsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                _writer.Write(command.CommandText);
            }
            else
            {
                _writer.WriteLine(command.CommandText);
            }

            foreach (var parameter in command.Parameters.OfType<DbParameter>())
            {
                LogParameter(command, interceptionContext, parameter);
            }

            if (interceptionContext.IsAsync)
            {
                _writer.WriteLine(Strings.CommandLogAsync);
            }
        }

        /// <summary>
        ///     Called by <see cref="LogCommand" /> to log each parameter. This method can be called from an overridden
        ///     implementation of <see cref="LogCommand" /> to log parameters, and/or can be overridden to
        ///     change the way that parameters are logged to <see cref="Writer" />.
        /// </summary>
        /// <param name="command">The command being logged.</param>
        /// <param name="interceptionContext">Contextual information associated with the command.</param>
        /// <param name="parameter">The parameter to log.</param>
        public virtual void LogParameter(DbCommand command, DbCommandInterceptionContext interceptionContext, DbParameter parameter)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");
            Check.NotNull(parameter, "parameter");

            // This is not a resource string because nothing in this string should be localized.
            const string parameterFormat = "-- {0}: {1} {2}{3} (Size = {4}; Precision = {5}; Scale = {6}) [{7}]";
            _writer.WriteLine(
                parameterFormat,
                parameter.ParameterName,
                parameter.Direction,
                (parameter.IsNullable ? "Nullable " : ""),
                parameter.DbType,
                parameter.Size,
                ((IDbDataParameter)parameter).Precision,
                ((IDbDataParameter)parameter).Scale,
                (parameter.Value == null || parameter.Value == DBNull.Value ? "null" : parameter.Value));
        }

        /// <summary>
        ///     Called to log the result of executing a command. Override this method to change how results are
        ///     logged to <see cref="Writer" />.
        /// </summary>
        /// <param name="command">The command being logged.</param>
        /// <param name="result">The result returned when the command was executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the command.</param>
        public virtual void LogResult(DbCommand command, object result, DbCommandInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            if (interceptionContext.Exception != null)
            {
                _writer.WriteLine(Strings.CommandLogFailed(interceptionContext.Exception.Message));
            }
            else if (interceptionContext.TaskStatus.HasFlag(TaskStatus.Canceled))
            {
                _writer.WriteLine(Strings.CommandLogCanceled);
            }
            else
            {
                var resultString = result == null
                                       ? "null"
                                       : (result is DbDataReader)
                                             ? result.GetType().Name
                                             : result.ToString();
                _writer.WriteLine(Strings.CommandLogComplete(resultString));
            }
            _writer.WriteLine();
        }
    }
}
