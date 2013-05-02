// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Utilities;
    using System.Threading.Tasks;

    /// <summary>
    ///     Represents contextual information associated with calls into <see cref="IDbCommandInterceptor" />
    ///     implementations.
    /// </summary>
    /// <remarks>
    ///     Instances of this class are publicly immutable. To add contextual information use one of the
    ///     With... or As... methods to create a new interception context containing the new information.
    /// </remarks>
    public class DbCommandInterceptionContext : DbInterceptionContext
    {
        private bool _isAsync;
        private TaskStatus _taskStatus;
        private CommandBehavior _commandBehavior = CommandBehavior.Default;

        /// <summary>
        ///     Constructs a new <see cref="DbCommandInterceptionContext" /> with no state.
        /// </summary>
        public DbCommandInterceptionContext()
        {
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandInterceptionContext" /> by copying state from the given
        ///     interception context. Also see <see cref="DbCommandInterceptionContext.Clone" />
        /// </summary>
        /// <param name="copyFrom">The context from which to copy state.</param>
        public DbCommandInterceptionContext(DbInterceptionContext copyFrom)
            : base(copyFrom)
        {
            var asThisType = copyFrom as DbCommandInterceptionContext;
            if (asThisType != null)
            {
                _isAsync = asThisType._isAsync;
                _taskStatus = asThisType.TaskStatus;
                _commandBehavior = asThisType._commandBehavior;
            }
        }

        /// <summary>
        ///     True if the operation is being executed asynchronously, otherwise false.
        /// </summary>
        public bool IsAsync
        {
            get { return _isAsync; }
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandInterceptionContext" /> that contains all the contextual information in this
        ///     interception context the <see cref="DbCommandInterceptionContext.IsAsync" /> flag set to true.
        /// </summary>
        /// <returns>A new interception context associated with the async flag set.</returns>
        public DbCommandInterceptionContext AsAsync()
        {
            var copy = TypedClone();
            copy._isAsync = true;
            return copy;
        }

        /// <summary>
        ///     The status of the async <see cref="Task" /> after the operation complete. This property is only
        ///     set after an async operation has either completed, failed, or been canceled.
        /// </summary>
        public TaskStatus TaskStatus
        {
            get { return _taskStatus; }
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandInterceptionContext" /> that contains all the contextual information in this
        ///     interception context together with the given <see cref="TaskStatus" />.
        /// </summary>
        /// <param name="taskStatus">The task status to associate.</param>
        /// <returns>A new interception context associated with the given status.</returns>
        public DbCommandInterceptionContext WithTaskStatus(TaskStatus taskStatus)
        {
            var copy = TypedClone();
            copy._taskStatus = taskStatus;
            return copy;
        }

        /// <summary>
        ///     The <see cref="CommandBehavior" /> that will be used or has been used to execute the command with a
        ///     <see cref="DbDataReader" />. This property is only used for <see cref="DbCommand.ExecuteReader(CommandBehavior)" />
        ///     and its async counterparts.
        /// </summary>
        public CommandBehavior CommandBehavior
        {
            get { return _commandBehavior; }
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandInterceptionContext" /> that contains all the contextual information in this
        ///     interception context together with the given <see cref="CommandBehavior" />.
        /// </summary>
        /// <param name="commandBehavior">The command behavior to associate.</param>
        /// <returns>A new interception context associated with the given command behavior.</returns>
        public DbCommandInterceptionContext WithCommandBehavior(CommandBehavior commandBehavior)
        {
            var copy = TypedClone();
            copy._commandBehavior = commandBehavior;
            return copy;
        }

        private DbCommandInterceptionContext TypedClone()
        {
            return (DbCommandInterceptionContext)Clone();
        }

        /// <inheritdoc />
        protected override DbInterceptionContext Clone()
        {
            return new DbCommandInterceptionContext(this);
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandInterceptionContext" /> that contains all the contextual information in this
        ///     interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new DbCommandInterceptionContext WithDbContext(DbContext context)
        {
            Check.NotNull(context, "context");

            return (DbCommandInterceptionContext)base.WithDbContext(context);
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandInterceptionContext" /> that contains all the contextual information in this
        ///     interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new DbCommandInterceptionContext WithObjectContext(ObjectContext context)
        {
            Check.NotNull(context, "context");

            return (DbCommandInterceptionContext)base.WithObjectContext(context);
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandInterceptionContext" /> that contains all the contextual information in this
        ///     interception context with the addition of the given <see cref="Exception" />.
        ///     Note that associating an exception with an interception context indicates that the intercepted
        ///     operation failed.
        /// </summary>
        /// <param name="exception">The exception to associate.</param>
        /// <returns>A new interception context associated with the given exception.</returns>
        public new DbCommandInterceptionContext WithException(Exception exception)
        {
            return (DbCommandInterceptionContext)base.WithException(exception);
        }
    }
}
