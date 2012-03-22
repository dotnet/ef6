namespace System.Data.Entity.Edm.Internal
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal sealed class BackingList<TElement> : IEnumerable<TElement>
    {
        private IList<TElement> instance;

        internal bool HasValue
        {
            get { return instance != null; }
        }

        internal void SetValue(IList<TElement> value)
        {
            Contract.Requires(value != null);

            instance = value;
        }

        internal IList<TElement> EnsureValue()
        {
            if (instance == null)
            {
                instance = new List<TElement>();
            }
            return instance;
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return (instance ?? Enumerable.Empty<TElement>()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (instance ?? Enumerable.Empty<TElement>()).GetEnumerator();
        }
    }
}
