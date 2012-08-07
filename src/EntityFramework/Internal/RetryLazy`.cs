// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Diagnostics;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Adapted from <see cref="System.Lazy{T}" /> to allow the initializer to take an input object and
    ///     to retry initialization if it has previously failed.
    /// </summary>
    /// <remarks>
    ///     This class can only be used to initialize reference types that will not be null when
    ///     initialized.
    /// </remarks>
    /// <typeparam name="TInput"> The type of the input. </typeparam>
    /// <typeparam name="TResult"> The type of the result. </typeparam>
    internal class RetryLazy<TInput, TResult>
        where TResult : class
    {
        #region Fields and constructors

        private readonly object _lock = new object();
        private Func<TInput, TResult> _valueFactory;
        private TResult _value;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RetryLazy&lt;TInput, TResult&gt;" /> class.
        /// </summary>
        /// <param name="valueFactory"> The value factory. </param>
        public RetryLazy(Func<TInput, TResult> valueFactory)
        {
            Contract.Requires(valueFactory != null);

            _valueFactory = valueFactory;
        }

        #endregion

        #region Lazy initialization

        /// <summary>
        ///     Gets the value, possibly by running the initializer if it has not been run before or
        ///     if all previous times it ran resulted in exceptions.
        /// </summary>
        /// <param name="input"> The input to the initializer; ignored if initialization has already succeeded. </param>
        /// <returns> The initialized object. </returns>
        [DebuggerStepThrough]
        public TResult GetValue(TInput input)
        {
            // This code is taken from System.Lazy with the parts of that class that we are not using removed
            // and with extra logic to allow initialization retry.
            lock (_lock)
            {
                if (_value == null)
                {
                    Contract.Assert(_valueFactory != null, "Same thread called Value while already calculating Value.");

                    var valueFactory = _valueFactory;
                    try
                    {
                        _valueFactory = null;
                        _value = valueFactory(input);
                    }
                    catch (Exception)
                    {
                        Contract.Assert(_value == null, "_value should only be set if no exception is thrown.");

                        // Reset the value factory so that the value creation will be retried.
                        _valueFactory = valueFactory;
                        throw;
                    }
                }

                Contract.Assert(_value != null, "This class needs modification if it should ever return null.");
                return _value;
            }
        }

        #endregion
    }
}
