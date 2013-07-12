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
    ///     Instances of this class are publicly immutable for contextual information. To add
    ///     contextual information use one of the With... or As... methods to create a new
    ///     interception context containing the new information.
    /// </remarks>
    public class DbCommandInterceptionContext<TResult> : DbCommandInterceptionContext, IDbMutableInterceptionContext<TResult>
    {
        private readonly InterceptionContextMutableData<TResult> _mutableData
            = new InterceptionContextMutableData<TResult>();

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
        }

        InterceptionContextMutableData IDbMutableInterceptionContext.MutableData
        {
            get { return _mutableData; }
        }

        InterceptionContextMutableData<TResult> IDbMutableInterceptionContext<TResult>.MutableData
        {
            get { return _mutableData; }
        }

        internal override InterceptionContextMutableData MutableData
        {
            get { return _mutableData; }
        }

        /// <summary>
        ///     If execution of the operation completes without throwing, then this property will contain
        ///     the result of the operation. If the operation was suppressed or did not fail, then this property
        ///     will always contain the default value for the generic type.
        /// </summary>
        /// <remarks>
        ///     When an operation operation completes without throwing both this property and the <see cref="Result" />
        ///     property are set. However, the <see cref="Result" /> property can be set or changed by interceptors,
        ///     while this property will always represent the actual result returned by the operation, if any.
        /// </remarks>
        public TResult OriginalResult
        {
            get { return _mutableData.OriginalResult; }
        }

        /// <summary>
        ///     If this property is set before the operation has executed, then execution of the operation will
        ///     be suppressed and the set result will be returned instead. Otherwise, if the operation succeeds, then
        ///     this property will be set to the returned result. In either case, interceptors that run
        ///     after the operation can change this property to change the result that will be returned.
        /// </summary>
        /// <remarks>
        ///     When an operation operation completes without throwing both this property and the <see cref="OriginalResult" />
        ///     property are set. However, this property can be set or changed by interceptors, while the
        ///     <see cref="OriginalResult" /> property will always represent the actual result returned by the
        ///     operation, if any.
        /// </remarks>
        public TResult Result
        {
            get { return _mutableData.Result; }
            set { _mutableData.Result = value; }
        }

        /// <inheritdoc />
        public override bool IsSuppressed
        {
            get { return _mutableData.IsSuppressed; }
        }

        /// <inheritdoc />
        public override void SuppressExecution()
        {
            _mutableData.SuppressExecution();
        }

        /// <inheritdoc />
        public override Exception OriginalException
        {
            get { return _mutableData.OriginalException; }
        }

        /// <inheritdoc />
        public override Exception Exception
        {
            get { return _mutableData.Exception; }
            set { _mutableData.Exception = value; }
        }

        /// <inheritdoc />
        public override TaskStatus TaskStatus
        {
            get { return _mutableData.TaskStatus; }
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandInterceptionContext{TResult}" /> that contains all the contextual information in this
        ///     interception context together with the <see cref="DbInterceptionContext.IsAsync" /> flag set to true.
        /// </summary>
        /// <returns>A new interception context associated with the async flag set.</returns>
        public new DbCommandInterceptionContext<TResult> AsAsync()
        {
            return (DbCommandInterceptionContext<TResult>)base.AsAsync();
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

        /// <inheritdoc />
        protected override DbInterceptionContext Clone()
        {
            return new DbCommandInterceptionContext<TResult>(this);
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandInterceptionContext{TResult}" /> that contains all the contextual information in this
        ///     interception context with the addition of the given <see cref="DbContext" />.
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
    }
}
