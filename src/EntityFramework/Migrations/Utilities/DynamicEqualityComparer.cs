namespace System.Data.Entity.Migrations.Utilities
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    internal sealed class DynamicEqualityComparer<T> : IEqualityComparer<T>
        where T : class
    {
        #region Constants and Fields

        private readonly Func<T, T, bool> _func;

        #endregion

        #region Constructors and Destructors

        public DynamicEqualityComparer(Func<T, T, bool> func)
        {
            Contract.Requires(func != null);

            _func = func;
        }

        #endregion

        #region Implemented Interfaces

        #region IEqualityComparer<T>

        public bool Equals(T x, T y)
        {
            return _func(x, y);
        }

        public int GetHashCode(T obj)
        {
            return 0; // force Equals
        }

        #endregion

        #endregion
    }
}
