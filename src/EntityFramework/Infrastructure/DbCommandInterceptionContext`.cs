// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Utilities;
    using System.Threading.Tasks;

    /// <summary>
    ///     Represents contextual information associated with calls into <see cref="IDbCommandInterceptor" />
    ///     implementations including the result of the operation.
    /// </summary>
    /// <remarks>
    ///     Instances of this class are publicly immutable except that the <see cref="Result" /> property can
    ///     be set. To add other contextual information use one of the With... or As... methods to create a
    ///     new interception context containing the new information.
    /// </remarks>
    public class DbCommandInterceptionContext<TResult> : DbCommandInterceptionContext, IDbInterceptionContextWithResult<TResult>
    {
        private bool _isResultSet;
        private TResult _result;

        /// <summary>
        ///     Constructs a new <see cref="DbCommandInterceptionContext{TResult}" /> with no state.
        /// </summary>
        public DbCommandInterceptionContext()
        {
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandInterceptionContext{TResult}" /> by copying state from the given
        ///     interception context. Also see <see cref="DbCommandInterceptionContext{TResult}.Clone" />
        /// </summary>
        /// <param name="copyFrom">The context from which to copy state.</param>
        public DbCommandInterceptionContext(DbInterceptionContext copyFrom)
            : base(copyFrom)
        {
            var asThisType = copyFrom as DbCommandInterceptionContext<TResult>;
            if (asThisType != null)
            {
                _isResultSet = asThisType._isResultSet;
                _result = asThisType._result;
            }
        }

        /// <summary>
        ///     The result of executing the operation.
        /// </summary>
        /// <remarks>
        ///     If this property is set before the command has been executed (in
        ///     <see cref="IDbCommandInterceptor.NonQueryExecuting" />,
        ///     <see cref="IDbCommandInterceptor.ReaderExecuting" />,  or <see cref="IDbCommandInterceptor.ScalarExecuting" />),
        ///     then the command will not be executed and the set result will be used instead.
        ///     This property is set by EF when the command has been executed and can then be changed (in
        ///     <see cref="IDbCommandInterceptor.NonQueryExecuted" />,
        ///     <see cref="IDbCommandInterceptor.ReaderExecuted" />,  or <see cref="IDbCommandInterceptor.ScalarExecuted" />)
        ///     to change the result that will be used.
        /// </remarks>
        public TResult Result
        {
            get { return _result; }
            set
            {
                _result = value;
                _isResultSet = true;
            }
        }

        internal bool IsResultSet
        {
            get { return ((IDbInterceptionContextWithResult<TResult>)this).IsResultSet; }
        }

        bool IDbInterceptionContextWithResult<TResult>.IsResultSet
        {
            get { return _isResultSet; }
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandInterceptionContext{TResult}" /> that contains all the contextual information in this
        ///     interception context together with the <see cref="DbCommandInterceptionContext.IsAsync" /> flag set to true.
        /// </summary>
        /// <returns>A new interception context associated with the async flag set.</returns>
        public new DbCommandInterceptionContext<TResult> AsAsync()
        {
            return (DbCommandInterceptionContext<TResult>)base.AsAsync();
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandInterceptionContext{TResult}" /> that contains all the contextual information in this
        ///     interception context together with the given <see cref="TaskStatus" />.
        /// </summary>
        /// <param name="taskStatus">The task status to associate.</param>
        /// <returns>A new interception context associated with the given status.</returns>
        public new DbCommandInterceptionContext<TResult> WithTaskStatus(TaskStatus taskStatus)
        {
            return (DbCommandInterceptionContext<TResult>)base.WithTaskStatus(taskStatus);
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandInterceptionContext{TResult}" /> that contains all the contextual information in this
        ///     interception context together with the given <see cref="CommandBehavior" />.
        /// </summary>
        /// <param name="commandBehavior">The command behavior to associate.</param>
        /// <returns>A new interception context associated with the given command behavior.</returns>
        public new DbCommandInterceptionContext<TResult> WithCommandBehavior(CommandBehavior commandBehavior)
        {
            return (DbCommandInterceptionContext<TResult>)base.WithCommandBehavior(commandBehavior);
        }

        private DbCommandInterceptionContext<TResult> TypedClone()
        {
            return (DbCommandInterceptionContext<TResult>)Clone();
        }

        /// <inheritdoc />
        protected override DbInterceptionContext Clone()
        {
            return new DbCommandInterceptionContext<TResult>(this);
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandInterceptionContext{TResult}" /> that contains all the contextual information in this
        ///     interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new DbCommandInterceptionContext<TResult> WithDbContext(DbContext context)
        {
            Check.NotNull(context, "context");

            return (DbCommandInterceptionContext<TResult>)base.WithDbContext(context);
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandInterceptionContext{TResult}" /> that contains all the contextual information in this
        ///     interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new DbCommandInterceptionContext<TResult> WithObjectContext(ObjectContext context)
        {
            Check.NotNull(context, "context");

            return (DbCommandInterceptionContext<TResult>)base.WithObjectContext(context);
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandInterceptionContext{TResult}" /> that contains all the contextual information in this
        ///     interception context with the addition of the given <see cref="Exception" />.
        ///     Note that associating an exception with an interception context indicates that the intercepted
        ///     operation failed.
        /// </summary>
        /// <param name="exception">The exception to associate.</param>
        /// <returns>A new interception context associated with the given exception.</returns>
        public new DbCommandInterceptionContext<TResult> WithException(Exception exception)
        {
            return (DbCommandInterceptionContext<TResult>)base.WithException(exception);
        }
    }
}
