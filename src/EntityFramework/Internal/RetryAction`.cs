namespace System.Data.Entity.Internal
{
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Adapted from <see cref = "System.Lazy{T}" /> to allow the initializer to take an input object and
    ///     to do one-time initialization that only has side-effects and doesn't return a value.
    /// </summary>
    /// <typeparam name = "TInput">The type of the input.</typeparam>
    internal class RetryAction<TInput>
    {
        #region Fields and constructors

        private readonly object _lock = new object();
        private Action<TInput> _action;

        /// <summary>
        ///     Initializes a new instance of the <see cref = "RetryAction&lt;TInput&gt;" /> class.
        /// </summary>
        /// <param name = "action">The action.</param>
        public RetryAction(Action<TInput> action)
        {
            Contract.Requires(action != null);

            _action = action;
        }

        #endregion

        #region Lazy initialization

        /// <summary>
        ///     Performs the action unless it has already been successfully performed before.
        /// </summary>
        /// <param name = "input">The input to the action; ignored if the action has already succeeded.</param>
        public void PerformAction(TInput input)
        {
            // This code is taken from System.Lazy with the parts of that class that we are not using removed
            // and with extra logic to allow initialization retry.
            lock (_lock)
            {
                // Note that if the same thread attempts to perform the action again (such as when
                // an initializer creates a new context instance while initializing) then the second
                // run through this code will do nothing because _action will be null.
                if (_action != null)
                {
                    var action = _action;
                    _action = null;
                    try
                    {
                        action(input);
                    }
                    catch (Exception)
                    {
                        // Reset the action so that it will be re-tried.
                        _action = action;
                        throw;
                    }
                }
            }
        }

        #endregion
    }
}