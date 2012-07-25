// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Internal
{
    using System;
    using System.Collections.Generic;

    internal class LazyEnumerator<T> : IEnumerator<T>
    {
        private readonly Lazy<IEnumerator<T>> _lazyEnumerator;

        // getEnumerator should never return null
        public LazyEnumerator(Func<IEnumerator<T>> getEnumerator)
        {
            _lazyEnumerator = new Lazy<IEnumerator<T>>(getEnumerator);
        }

        public T Current
        {
            get { return _lazyEnumerator.Value.Current; }
        }

        public void Dispose()
        {
            if (_lazyEnumerator.IsValueCreated)
            {
                _lazyEnumerator.Value.Dispose();
            }
        }

        object Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            return _lazyEnumerator.Value.MoveNext();
        }

        public void Reset()
        {
            _lazyEnumerator.Value.Reset();
        }
    }
}
