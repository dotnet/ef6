// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.ComponentModel;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents contextual information associated with calls to property setters of type <typeparamref name="TValue"/>.
    /// </summary>
    /// <remarks>
    /// An instance of this class is passed to the dispatch methods and does not contain mutable information such as
    /// the result of the operation. This mutable information is obtained from the <see cref="PropertyInterceptionContext{TValue}"/>
    /// that is passed to the interceptors. Instances of this class are publicly immutable. To add contextual information
    /// use one of the With... or As... methods to create a new interception context containing the new information.
    /// </remarks>
    /// <typeparam name="TValue">The type of the target property.</typeparam>
    public class PropertyInterceptionContext<TValue> : DbInterceptionContext, IDbMutableInterceptionContext
    {
        private readonly InterceptionContextMutableData _mutableData
            = new InterceptionContextMutableData();

        private TValue _value;

        /// <summary>
        /// Constructs a new <see cref="PropertyInterceptionContext{TValue}" /> with no state.
        /// </summary>
        public PropertyInterceptionContext()
        {
        }

        /// <summary>
        /// Creates a new <see cref="PropertyInterceptionContext{TValue}" /> by copying immutable state from the given
        /// interception context. Also see <see cref="Clone" />
        /// </summary>
        /// <param name="copyFrom">The context from which to copy state.</param>
        public PropertyInterceptionContext(DbInterceptionContext copyFrom)
            : base(copyFrom)
        {
            Check.NotNull(copyFrom, "copyFrom");

            var asThisType = copyFrom as PropertyInterceptionContext<TValue>;
            if (asThisType != null)
            {
                _value = asThisType._value;
            }
        }

        InterceptionContextMutableData IDbMutableInterceptionContext.MutableData
        {
            get { return _mutableData; }
        }

        /// <summary>
        /// The value that will be assigned to the target property.
        /// </summary>
        public TValue Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Creates a new <see cref="BeginTransactionInterceptionContext" /> that contains all the contextual information in this
        /// interception context together with the given property value.
        /// </summary>
        /// <param name="value">The value that will be assigned to the target property.</param>
        /// <returns>A new interception context associated with the given property value.</returns>
        public PropertyInterceptionContext<TValue> WithValue(TValue value)
        {
            var copy = TypedClone();
            copy._value = value;
            return copy;
        }

        private PropertyInterceptionContext<TValue> TypedClone()
        {
            return (PropertyInterceptionContext<TValue>)Clone();
        }

        /// <inheritdoc />
        protected override DbInterceptionContext Clone()
        {
            return new PropertyInterceptionContext<TValue>(this);
        }

        /// <summary>
        /// When true, this flag indicates that that execution of the operation has been suppressed by
        /// one of the interceptors. This can be done before the operation has executed by calling
        /// <see cref="SuppressExecution" /> or by setting an <see cref="Exception" /> to be thrown
        /// </summary>
        public bool IsExecutionSuppressed
        {
            get { return _mutableData.IsExecutionSuppressed; }
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
        /// Set to the status of the <see cref="Task" /> after an async operation has finished. Not used for
        /// synchronous operations.
        /// </summary>
        public TaskStatus TaskStatus
        {
            get { return _mutableData.TaskStatus; }
        }

        /// <summary>
        /// Creates a new <see cref="PropertyInterceptionContext{TValue}" /> that contains all the contextual information in this
        /// interception context together with the <see cref="DbInterceptionContext.IsAsync" /> flag set to true.
        /// </summary>
        /// <returns>A new interception context associated with the async flag set.</returns>
        public new PropertyInterceptionContext<TValue> AsAsync()
        {
            return (PropertyInterceptionContext<TValue>)base.AsAsync();
        }

        /// <summary>
        /// Creates a new <see cref="PropertyInterceptionContext{TValue}" /> that contains all the contextual information in this
        /// interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new PropertyInterceptionContext<TValue> WithDbContext(DbContext context)
        {
            Check.NotNull(context, "context");

            return (PropertyInterceptionContext<TValue>)base.WithDbContext(context);
        }

        /// <summary>
        /// Creates a new <see cref="PropertyInterceptionContext{TValue}" /> that contains all the contextual information in this
        /// interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new PropertyInterceptionContext<TValue> WithObjectContext(ObjectContext context)
        {
            Check.NotNull(context, "context");

            return (PropertyInterceptionContext<TValue>)base.WithObjectContext(context);
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
