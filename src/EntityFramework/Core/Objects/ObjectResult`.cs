// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     This class represents the result of the <see cref="ObjectQuery{T}.Execute" /> method.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class ObjectResult<T> : ObjectResult, IEnumerable<T>, IDbAsyncEnumerable<T>
    {
        private Shaper<T> _shaper;
        private DbDataReader _reader;
        private readonly EntitySet _singleEntitySet;
        private readonly TypeUsage _resultItemType;
        private readonly bool _readerOwned;
        private IBindingList _cachedBindingList;
        private NextResultGenerator _nextResultGenerator;
        private Action<object, EventArgs> _onReaderDispose;

        internal ObjectResult(Shaper<T> shaper, EntitySet singleEntitySet, TypeUsage resultItemType)
            : this(shaper, singleEntitySet, resultItemType, true)
        {
        }

        internal ObjectResult(Shaper<T> shaper, EntitySet singleEntitySet, TypeUsage resultItemType, bool readerOwned)
            : this(shaper, singleEntitySet, resultItemType, readerOwned, null, null)
        {
        }

        internal ObjectResult(
            Shaper<T> shaper, EntitySet singleEntitySet, TypeUsage resultItemType, bool readerOwned, NextResultGenerator nextResultGenerator,
            Action<object, EventArgs> onReaderDispose)
        {
            _shaper = shaper;
            _reader = _shaper.Reader;
            _singleEntitySet = singleEntitySet;
            _resultItemType = resultItemType;
            _readerOwned = readerOwned;
            _nextResultGenerator = nextResultGenerator;
            _onReaderDispose = onReaderDispose;
        }

        private void EnsureCanEnumerateResults()
        {
            if (null == _shaper)
            {
                // Enumerating more than once is not allowed.
                throw new InvalidOperationException(Strings.Materializer_CannotReEnumerateQueryResults);
            }
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return GetDbEnumerator();
        }

        internal virtual IDbEnumerator<T> GetDbEnumerator()
        {
            EnsureCanEnumerateResults();

            var shaper = _shaper;
            _shaper = null;
            var result = shaper.GetEnumerator();
            return result;
        }

        #region IDbAsyncEnumerable

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IDbAsyncEnumerator<T> IDbAsyncEnumerable<T>.GetAsyncEnumerator()
        {
            return GetDbEnumerator();
        }

        #endregion

        /// <summary>
        ///     Performs tasks associated with freeing, releasing, or resetting resources.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        public override void Dispose()
        {
            // Technically, calling GC.SuppressFinalize is not required because the class does not
            // have a finalizer, but it does no harm, protects against the case where a finalizer is added
            // in the future, and prevents an FxCop warning.
            GC.SuppressFinalize(this);

            var reader = _reader;
            _reader = null;
            _nextResultGenerator = null;

            if (null != reader && _readerOwned)
            {
                reader.Dispose();
                if (_onReaderDispose != null)
                {
                    _onReaderDispose(this, new EventArgs());
                    _onReaderDispose = null;
                }
            }
            if (_shaper != null)
            {
                // This case includes when the ObjectResult is disposed before it 
                // created an ObjectQueryEnumeration; at this time, the connection can be released
                if (_shaper.Context != null && _readerOwned)
                {
                    _shaper.Context.ReleaseConnection();
                }
                _shaper = null;
            }
        }

        internal override IDbAsyncEnumerator GetAsyncEnumeratorInternal()
        {
            return GetDbEnumerator();
        }

        internal override IEnumerator GetEnumeratorInternal()
        {
            return GetDbEnumerator();
        }

        internal override IList GetIListSourceListInternal()
        {
            // You can only enumerate the query results once, and the creation of an ObjectView consumes this enumeration.
            // However, there are situations where setting the DataSource of a control can result in multiple calls to this method.
            // In order to enable this scenario and allow direct binding to the ObjectResult instance, 
            // the ObjectView is cached and returned on subsequent calls to this method.

            if (_cachedBindingList == null)
            {
                EnsureCanEnumerateResults();

                var forceReadOnly = _shaper.MergeOption == MergeOption.NoTracking;
                _cachedBindingList = ObjectViewFactory.CreateViewForQuery(
                    _resultItemType, this, _shaper.Context, forceReadOnly, _singleEntitySet);
            }

            return _cachedBindingList;
        }

        internal override ObjectResult<TElement> GetNextResultInternal<TElement>()
        {
            return null != _nextResultGenerator ? _nextResultGenerator.GetNextResult<TElement>(_reader) : null;
        }

        public override Type ElementType
        {
            get { return typeof(T); }
        }
    }
}
