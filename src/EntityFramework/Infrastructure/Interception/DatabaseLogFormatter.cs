// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    ///     This is the default log formatter used when some <see cref="Action{String}" /> is set onto the <see cref="Database.Log" />
    ///     property. A different formatter can be used by creating a class that inherits from this class and overrides
    ///     some or all methods to change behavior.
    /// </summary>
    /// <remarks>
    ///     To set the new formatter create a code-based configuration for EF using <see cref="DbConfiguration" /> and then
    ///     set the formatter class to use with <see cref="DbConfiguration.SetDatabaseLogFormatter" />.
    ///     Note that setting the type of formatter to use with this method does change the way command are
    ///     logged when <see cref="Database.Log" /> is used. It is still necessary to set a <see cref="Action{String}" />
    ///     onto <see cref="Database.Log" /> before any commands will be logged.
    ///     For more low-level control over logging/interception see <see cref="IDbCommandInterceptor" /> and
    ///     <see cref="DbInterception" />.
    /// </remarks>
    public class DatabaseLogFormatter : IDbCommandInterceptor
    {
        private readonly DbContext _context;
        private readonly Action<string> _writeAction;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        ///     Creates a formatter that will not filter by any <see cref="DbContext" /> and will instead log every command
        ///     from any context and also commands that do not originate from a context.
        /// </summary>
        /// <remarks>
        ///     This constructor is not used when a delegate is set on <see cref="Database.Log" />. Instead it can be
        ///     used by setting the formatter directly using <see cref="DbInterception.Add" />.
        /// </remarks>
        /// <param name="writeAction">The delegate to which output will be sent.</param>
        public DatabaseLogFormatter(Action<string> writeAction)
        {
            Check.NotNull(writeAction, "writeAction");

            _writeAction = writeAction;
        }

        /// <summary>
        ///     Creates a formatter that will only log commands the come from the given <see cref="DbContext" /> instance.
        /// </summary>
        /// <remarks>
        ///     This constructor must be called by a class that inherits from this class to override the behavior
        ///     of <see cref="Database.Log" />.
        /// </remarks>
        /// <param name="context">The context for which commands should be logged.</param>
        /// <param name="writeAction">The delegate to which output will be sent.</param>
        public DatabaseLogFormatter(DbContext context, Action<string> writeAction)
        {
            Check.NotNull(context, "context");
            Check.NotNull(writeAction, "writeAction");

            _context = context;
            _writeAction = writeAction;
        }

        /// <summary>
        ///     The context for which commands are being logged, or null if commands from all contexts are
        ///     being logged.
        /// </summary>
        protected internal DbContext Context
        {
            get { return _context; }
        }

        internal Action<string> WriteAction
        {
            get { return _writeAction; }
        }

        /// <summary>
        ///     Writes the given string to the underlying write delegate.
        /// </summary>
        /// <param name="output">The string to write.</param>
        protected virtual void Write(string output)
        {
            _writeAction(output);
        }

        /// <summary>
        ///     The stop watch used to time executions. This stop watch is started at the end of
        ///     <see cref="NonQueryExecuting" />, <see cref="ScalarExecuting" />, and <see cref="ReaderExecuting" />
        ///     methods and is stopped at the beginning of the <see cref="NonQueryExecuted" />, <see cref="ScalarExecuted" />,
        ///     and <see cref="ReaderExecuted" /> methods. If these methods are overridden and the stop watch is being used
        ///     then the overrides should either call the base method or start/stop the watch themselves.
        /// </summary>
        protected internal Stopwatch Stopwatch
        {
            get { return _stopwatch; }
        }

        /// <summary>
        ///     This method is called before a call to <see cref="DbCommand.ExecuteNonQuery" /> or
        ///     one of its async counterparts is made.
        ///     The default implementation calls <see cref="Executing" /> and starts <see cref="Stopwatch"/>.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        public virtual void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            Executing(command, interceptionContext);
            Stopwatch.Restart();
        }

        /// <summary>
        ///     This method is called after a call to <see cref="DbCommand.ExecuteNonQuery" /> or
        ///     one of its async counterparts is made.
        ///     The default implementation stops <see cref="Stopwatch"/> and calls <see cref="Executed" />.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        public virtual void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            Stopwatch.Stop();
            Executed(command, interceptionContext);
        }

        /// <summary>
        ///     This method is called before a call to <see cref="DbCommand.ExecuteReader(CommandBehavior)" /> or
        ///     one of its async counterparts is made.
        ///     The default implementation calls <see cref="Executing" /> and starts <see cref="Stopwatch"/>.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        public virtual void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            Executing(command, interceptionContext);
            Stopwatch.Restart();
        }

        /// <summary>
        ///     This method is called after a call to <see cref="DbCommand.ExecuteReader(CommandBehavior)" /> or
        ///     one of its async counterparts is made.
        ///     The default implementation stops <see cref="Stopwatch"/> and calls <see cref="Executed" />.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        public virtual void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            Stopwatch.Stop();
            Executed(command, interceptionContext);
        }

        /// <summary>
        ///     This method is called before a call to <see cref="DbCommand.ExecuteScalar" />  or
        ///     one of its async counterparts is made.
        ///     The default implementation calls <see cref="Executing" /> and starts <see cref="Stopwatch"/>.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        public virtual void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            Executing(command, interceptionContext);
            Stopwatch.Restart();
        }

        /// <summary>
        ///     This method is called after a call to <see cref="DbCommand.ExecuteScalar" />  or
        ///     one of its async counterparts is made.
        ///     The default implementation stops <see cref="Stopwatch"/> and calls <see cref="Executed" />.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        public virtual void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            Stopwatch.Stop();
            Executed(command, interceptionContext);
        }

        /// <summary>
        ///     Called whenever a command is about to be executed. The default implementation of this method
        ///     filters by <see cref="DbContext" /> set into <see cref="Context" />, if any, and then calls
        ///     <see cref="LogCommand" />. This method would typically only be overridden to change the
        ///     context filtering behavior.
        /// </summary>
        /// <param name="command">The command that will be executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the command.</param>
        public virtual void Executing<TResult>(DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            if (Context == null
                || interceptionContext.DbContexts.Contains(Context, ReferenceEquals))
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
        public virtual void Executed<TResult>(DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            if (Context == null
                || interceptionContext.DbContexts.Contains(Context, ReferenceEquals))
            {
                LogResult(command, interceptionContext);
            }
        }

        /// <summary>
        ///     Called to log a command that is about to be executed. Override this method to change how the
        ///     command is logged to <see cref="WriteAction" />.
        /// </summary>
        /// <param name="command">The command to be logged.</param>
        /// <param name="interceptionContext">Contextual information associated with the command.</param>
        public virtual void LogCommand<TResult>(DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            var commandText = command.CommandText ?? "<null>";
            if (commandText.EndsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                Write(commandText);
            }
            else
            {
                Write(commandText);
                Write(Environment.NewLine);
            }

            if (command.Parameters != null)
            {
                foreach (var parameter in command.Parameters.OfType<DbParameter>())
                {
                    LogParameter(command, interceptionContext, parameter);
                }
            }

            Write(interceptionContext.IsAsync
                      ? Strings.CommandLogAsync(DateTimeOffset.Now, Environment.NewLine)
                      : Strings.CommandLogNonAsync(DateTimeOffset.Now, Environment.NewLine));
        }

        /// <summary>
        ///     Called by <see cref="LogCommand" /> to log each parameter. This method can be called from an overridden
        ///     implementation of <see cref="LogCommand" /> to log parameters, and/or can be overridden to
        ///     change the way that parameters are logged to <see cref="WriteAction" />.
        /// </summary>
        /// <param name="command">The command being logged.</param>
        /// <param name="interceptionContext">Contextual information associated with the command.</param>
        /// <param name="parameter">The parameter to log.</param>
        public virtual void LogParameter<TResult>(DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext, DbParameter parameter)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");
            Check.NotNull(parameter, "parameter");

            // -- Name: [Value] (Type = {}, Direction = {}, IsNullable = {}, Size = {}, Precision = {} Scale = {})
            var builder = new StringBuilder();
            builder.Append("-- ")
                .Append(parameter.ParameterName)
                .Append(": '")
                .Append((parameter.Value == null || parameter.Value == DBNull.Value) ? "null" : parameter.Value)
                .Append("' (Type = ")
                .Append(parameter.DbType);

            if (parameter.Direction != ParameterDirection.Input)
            {
                builder.Append(", Direction = ").Append(parameter.Direction);
            }

            if (!parameter.IsNullable)
            {
                builder.Append(", IsNullable = false");
            }

            if (parameter.Size != 0)
            {
                builder.Append(", Size = ").Append(parameter.Size);
            }

            if (((IDbDataParameter)parameter).Precision != 0)
            {
                builder.Append(", Precision = ").Append(((IDbDataParameter)parameter).Precision);
            }

            if (((IDbDataParameter)parameter).Scale != 0)
            {
                builder.Append(", Scale = ").Append(((IDbDataParameter)parameter).Scale);
            }

            builder.Append(")").Append(Environment.NewLine);

            Write(builder.ToString());
        }

        /// <summary>
        ///     Called to log the result of executing a command. Override this method to change how results are
        ///     logged to <see cref="WriteAction" />.
        /// </summary>
        /// <param name="command">The command being logged.</param>
        /// <param name="result">The result returned when the command was executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the command.</param>
        public virtual void LogResult<TResult>(DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            if (interceptionContext.Exception != null)
            {
                Write(Strings.CommandLogFailed(
                    Stopwatch.ElapsedMilliseconds, interceptionContext.Exception.Message, Environment.NewLine));
            }
            else if (interceptionContext.TaskStatus.HasFlag(TaskStatus.Canceled))
            {
                Write(Strings.CommandLogCanceled(Stopwatch.ElapsedMilliseconds, Environment.NewLine));
            }
            else
            {
                var result = interceptionContext.Result;
                var resultString = (object)result == null
                                       ? "null"
                                       : (result is DbDataReader)
                                             ? result.GetType().Name
                                             : result.ToString();
                Write(Strings.CommandLogComplete(Stopwatch.ElapsedMilliseconds, resultString, Environment.NewLine));
            }
            Write(Environment.NewLine);
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
