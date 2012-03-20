namespace System.Data.Entity.Internal
{
    /// <summary>
    ///     Helper class that extends Tuple to give the Item1 and Item2 properties more meaningful names.
    /// </summary>
    internal class InitializerLockPair : Tuple<Action<DbContext>, bool>
    {
        #region Constructor

        /// <summary>
        ///     Creates a new pair of the given database initializer delegate and a flag
        ///     indicating whether or not it is locked.
        /// </summary>
        public InitializerLockPair(Action<DbContext> initializerDelegate, bool isLocked)
            : base(initializerDelegate, isLocked)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///     The initializer delegate.
        /// </summary>
        public Action<DbContext> InitializerDelegate
        {
            get { return Item1; }
        }

        /// <summary>
        ///     A flag indicating whether or not the initializer is locked and should not be changed.
        /// </summary>
        public bool IsLocked
        {
            get { return Item2; }
        }

        #endregion
    }
}