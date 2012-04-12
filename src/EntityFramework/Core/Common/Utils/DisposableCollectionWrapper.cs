namespace System.Data.Entity.Core.Common.Utils
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    internal class DisposableCollectionWrapper<T> : IDisposable, IEnumerable<T>
        where T : IDisposable
    {
        private readonly IEnumerable<T> _enumerable;

        internal DisposableCollectionWrapper(IEnumerable<T> enumerable)
        {
            Debug.Assert(enumerable != null, "don't pass in a null enumerable");
            _enumerable = enumerable;
        }

        public void Dispose()
        {
            // Technically, calling GC.SuppressFinalize is not required because the class does not
            // have a finalizer, but it does no harm, protects against the case where a finalizer is added
            // in the future, and prevents an FxCop warning.
            GC.SuppressFinalize(this);
            if (_enumerable != null)
            {
                foreach (var item in _enumerable)
                {
                    if (item != null)
                    {
                        item.Dispose();
                    }
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_enumerable).GetEnumerator();
        }
    }
}
