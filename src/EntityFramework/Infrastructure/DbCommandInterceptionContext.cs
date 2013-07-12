// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Utilities;
    using System.Threading.Tasks;

    /// <summary>
    ///     Represents contextual information associated with calls into <see cref="IDbCommandInterceptor" />
    ///     implementations.
    /// </summary>
    /// <remarks>
    ///     Instances of this class are publicly immutable for contextual information. To add
    ///     contextual information use one of the With... or As... methods to create a new
    ///     interception context containing the new information.
    /// </remarks>
    public abstract class DbCommandInterceptionContext : DbCommandBaseInterceptionContext, IDbMutableInterceptionContext
    {
        /// <summary>
        ///     Constructs a new <see cref="DbCommandInterceptionContext" /> with no state.
        /// </summary>
        protected DbCommandInterceptionContext()
        {
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandInterceptionContext" /> by copying state from the given
        ///     interception context. Also see <see cref="DbCommandInterceptionContext{TResult}.Clone" />
        /// </summary>
        /// <param name="copyFrom">The context from which to copy state.</param>
        protected DbCommandInterceptionContext(DbInterceptionContext copyFrom)
            : base(copyFrom)
        {
        }

        /// <summary>
        ///     When true, this flag indicates that that execution of the operation has been suppressed by
        ///     one of the interceptors. This can be done before the operation has executed by calling
        ///     <see cref="SuppressExecution" />, by setting an <see cref="Exception" /> to be thrown, or
        ///     by setting the operation result using <see cref="DbCommandInterceptionContext{TResult}.Result" />.
        /// </summary>
        public abstract bool IsSuppressed { get; }

        /// <summary>
        ///     Prevents the operation from being executed if called before the operation has executed.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if this method is called after the
        ///     operation has already executed.
        /// </exception>
        public abstract void SuppressExecution();

        /// <summary>
        ///     If execution of the operation fails, then this property will contain the exception that was
        ///     thrown. If the operation was suppressed or did not fail, then this property will always be
        ///     null.
        /// </summary>
        /// <remarks>
        ///     When an operation fails both this property and the <see cref="Exception" /> property are set
        ///     to the exception that was thrown. However, the <see cref="Exception" /> property can be set or
        ///     changed by interceptors, while this property will always represent the original exception
        ///     thrown.
        /// </remarks>
        public abstract Exception OriginalException { get; }

        /// <summary>
        ///     If this property is set before the operation has executed, then execution of the operation will
        ///     be suppressed and the set exception will be thrown instead. Otherwise, if the operation fails, then
        ///     this property will be set to the exception that was thrown. In either case, interceptors that run
        ///     after the operation can change this property to change the exception that will be thrown, or set this
        ///     property to null to cause no exception to be thrown at all.
        /// </summary>
        /// <remarks>
        ///     When an operation fails both this property and the <see cref="OriginalException" /> property are set
        ///     to the exception that was thrown. However, the this property can be set or changed by
        ///     interceptors, while the <see cref="OriginalException" /> property will always represent
        ///     the original exception thrown.
        /// </remarks>
        public abstract Exception Exception { get; set; }

        /// <summary>
        ///     Set to the status of the <see cref="Task{TResult}" /> after an async operation has finished. Not used for
        ///     synchronous operations.
        /// </summary>
        public abstract TaskStatus TaskStatus { get; }

        /// <summary>
        ///     Creates a new <see cref="DbCommandInterceptionContext" /> that contains all the contextual information in this
        ///     interception context together with the <see cref="DbInterceptionContext.IsAsync" /> flag set to true.
        /// </summary>
        /// <returns>A new interception context associated with the async flag set.</returns>
        public new DbCommandInterceptionContext AsAsync()
        {
            return (DbCommandInterceptionContext)base.AsAsync();
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandInterceptionContext" /> that contains all the contextual information in this
        ///     interception context together with the given <see cref="CommandBehavior" />.
        /// </summary>
        /// <param name="commandBehavior">The command behavior to associate.</param>
        /// <returns>A new interception context associated with the given command behavior.</returns>
        public new DbCommandInterceptionContext WithCommandBehavior(CommandBehavior commandBehavior)
        {
            return (DbCommandInterceptionContext)base.WithCommandBehavior(commandBehavior);
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandInterceptionContext" /> that contains all the contextual information in this
        ///     interception context with the addition of the given <see cref="DbContext" />.
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
        ///     interception context with the addition of the given <see cref="DbContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new DbCommandInterceptionContext WithObjectContext(ObjectContext context)
        {
            Check.NotNull(context, "context");

            return (DbCommandInterceptionContext)base.WithObjectContext(context);
        }

        InterceptionContextMutableData IDbMutableInterceptionContext.MutableData
        {
            get { return MutableData; }
        }

        internal abstract InterceptionContextMutableData MutableData { get; }
    }
}
