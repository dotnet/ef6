// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.ComponentModel;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents contextual information associated with calls with return type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of the target method.</typeparam>
    public abstract class MutableInterceptionContext<TResult> : DbInterceptionContext, IDbMutableInterceptionContext<TResult>
    {
        private readonly InterceptionContextMutableData<TResult> _mutableData
            = new InterceptionContextMutableData<TResult>();

        /// <summary>
        /// Constructs a new <see cref="MutableInterceptionContext{TResult}" /> with no state.
        /// </summary>
        protected MutableInterceptionContext()
        {
        }

        /// <summary>
        /// Creates a new <see cref="MutableInterceptionContext{TResult}" /> by copying immutable state from the given
        /// interception context. Also see <see cref="DbInterceptionContext.Clone" />
        /// </summary>
        /// <param name="copyFrom">The context from which to copy state.</param>
        protected MutableInterceptionContext(DbInterceptionContext copyFrom)
            : base(copyFrom)
        {
            Check.NotNull(copyFrom, "copyFrom");
        }

        InterceptionContextMutableData<TResult> IDbMutableInterceptionContext<TResult>.MutableData
        {
            get { return _mutableData; }
        }

        InterceptionContextMutableData IDbMutableInterceptionContext.MutableData
        {
            get { return _mutableData; }
        }

        /// <summary>
        /// If execution of the operation completes without throwing, then this property will contain
        /// the result of the operation. If the operation was suppressed or did not fail, then this property
        /// will always contain the default value for the generic type.
        /// </summary>
        /// <remarks>
        /// When an operation operation completes without throwing both this property and the <see cref="Result" />
        /// property are set. However, the <see cref="Result" /> property can be set or changed by interceptors,
        /// while this property will always represent the actual result returned by the operation, if any.
        /// </remarks>
        public TResult OriginalResult
        {
            get { return _mutableData.OriginalResult; }
        }

        /// <summary>
        /// If this property is set before the operation has executed, then execution of the operation will
        /// be suppressed and the set result will be returned instead. Otherwise, if the operation succeeds, then
        /// this property will be set to the returned result. In either case, interceptors that run
        /// after the operation can change this property to change the result that will be returned.
        /// </summary>
        /// <remarks>
        /// When an operation operation completes without throwing both this property and the <see cref="OriginalResult" />
        /// property are set. However, this property can be set or changed by interceptors, while the
        /// <see cref="OriginalResult" /> property will always represent the actual result returned by the
        /// operation, if any.
        /// </remarks>
        public TResult Result
        {
            get { return _mutableData.Result; }
            set { _mutableData.Result = value; }
        }

        /// <summary>
        /// When true, this flag indicates that that execution of the operation has been suppressed by
        /// one of the interceptors. This can be done before the operation has executed by calling
        /// <see cref="SuppressExecution" />, by setting an <see cref="Exception" /> to be thrown, or
        /// by setting the operation result using <see cref="Result" />.
        /// </summary>
        public bool IsExecutionSuppressed
        {
            get { return _mutableData.IsExecutionSuppressed; }
        }

        /// <summary>
        /// Gets or sets a value containing arbitrary user-specified state information associated with the operation.
        /// </summary>
        public object UserState
        {
            get { return _mutableData.UserState; }
            set { _mutableData.UserState = value; }
        }

        /// <summary>
        /// Prevents the operation from being executed if called before the operation has executed.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this method is called after the operation has already executed.
        /// </exception>
        public void SuppressExecution()
        {
            _mutableData.SuppressExecution();
        }

        /// <summary>
        /// If execution of the operation fails, then this property will contain the exception that was
        /// thrown. If the operation was suppressed or did not fail, then this property will always be null.
        /// </summary>
        /// <remarks>
        /// When an operation fails both this property and the <see cref="Exception" /> property are set
        /// to the exception that was thrown. However, the <see cref="Exception" /> property can be set or
        /// changed by interceptors, while this property will always represent the original exception thrown.
        /// </remarks>
        public Exception OriginalException
        {
            get { return _mutableData.OriginalException; }
        }

        /// <summary>
        /// If this property is set before the operation has executed, then execution of the operation will
        /// be suppressed and the set exception will be thrown instead. Otherwise, if the operation fails, then
        /// this property will be set to the exception that was thrown. In either case, interceptors that run
        /// after the operation can change this property to change the exception that will be thrown, or set this
        /// property to null to cause no exception to be thrown at all.
        /// </summary>
        /// <remarks>
        /// When an operation fails both this property and the <see cref="OriginalException" /> property are set
        /// to the exception that was thrown. However, the this property can be set or changed by
        /// interceptors, while the <see cref="OriginalException" /> property will always represent
        /// the original exception thrown.
        /// </remarks>
        public Exception Exception
        {
            get { return _mutableData.Exception; }
            set { _mutableData.Exception = value; }
        }

        /// <summary>
        /// Set to the status of the <see cref="Task{TResult}" /> after an async operation has finished. Not used for
        /// synchronous operations.
        /// </summary>
        public TaskStatus TaskStatus
        {
            get { return _mutableData.TaskStatus; }
        }

        /// <summary>
        /// Creates a new <see cref="MutableInterceptionContext{TResult}" /> that contains all the contextual information in this
        /// interception context together with the <see cref="DbInterceptionContext.IsAsync" /> flag set to true.
        /// </summary>
        /// <returns>A new interception context associated with the async flag set.</returns>
        public new MutableInterceptionContext<TResult> AsAsync()
        {
            return (MutableInterceptionContext<TResult>)base.AsAsync();
        }

        /// <summary>
        /// Creates a new <see cref="MutableInterceptionContext{TResult}" /> that contains all the contextual information in this
        /// interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new MutableInterceptionContext<TResult> WithDbContext(DbContext context)
        {
            Check.NotNull(context, "context");

            return (MutableInterceptionContext<TResult>)base.WithDbContext(context);
        }

        /// <summary>
        /// Creates a new <see cref="MutableInterceptionContext{TResult}" /> that contains all the contextual information in this
        /// interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new MutableInterceptionContext<TResult> WithObjectContext(ObjectContext context)
        {
            Check.NotNull(context, "context");

            return (MutableInterceptionContext<TResult>)base.WithObjectContext(context);
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
