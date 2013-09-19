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
    /// This class represents the result of the <see cref="ObjectQuery{T}.Execute" /> method.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class ObjectResult<T> : ObjectResult, IEnumerable<T>
#if !NET40
, IDbAsyncEnumerable<T>
#endif
    {
        private Shaper<T> _shaper;
        private DbDataReader _reader;
        private readonly EntitySet _singleEntitySet;
        private readonly TypeUsage _resultItemType;
        private readonly bool _readerOwned;
        private readonly bool _shouldReleaseConnection;
        private IBindingList _cachedBindingList;
        private NextResultGenerator _nextResultGenerator;
        private Action<object, EventArgs> _onReaderDispose;

        internal ObjectResult(Shaper<T> shaper, EntitySet singleEntitySet, TypeUsage resultItemType)
            : this(shaper, singleEntitySet, resultItemType, readerOwned: true, shouldReleaseConnection: true)
        {
        }

        internal ObjectResult(
            Shaper<T> shaper, EntitySet singleEntitySet, TypeUsage resultItemType, bool readerOwned, bool shouldReleaseConnection)
            : this(
                shaper, singleEntitySet, resultItemType, readerOwned, shouldReleaseConnection, nextResultGenerator: null,
                onReaderDispose: null)
        {
        }

        internal ObjectResult(
            Shaper<T> shaper, EntitySet singleEntitySet, TypeUsage resultItemType, bool readerOwned,
            bool shouldReleaseConnection, NextResultGenerator nextResultGenerator, Action<object, EventArgs> onReaderDispose)
        {
            _shaper = shaper;
            _reader = _shaper.Reader;
            _singleEntitySet = singleEntitySet;
            _resultItemType = resultItemType;
            _readerOwned = readerOwned;
            _shouldReleaseConnection = shouldReleaseConnection;
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

        /// <summary>Returns an enumerator that iterates through the query results.</summary>
        /// <returns>An enumerator that iterates through the query results.</returns>
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

#if !NET40

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IDbAsyncEnumerator<T> IDbAsyncEnumerable<T>.GetAsyncEnumerator()
        {
            return GetDbEnumerator();
        }

#endif

        #endregion

        /// <summary>Releases the unmanaged resources used by the <see cref="T:System.Data.Entity.Core.Objects.ObjectResult`1" /> and optionally releases the managed resources.</summary>
        /// <param name="disposing">true to release managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            var reader = _reader;
            _reader = null;
            _nextResultGenerator = null;

            if (reader != null && _readerOwned)
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
                if (_shaper.Context != null
                    && _readerOwned
                    && _shouldReleaseConnection)
                {
                    _shaper.Context.ReleaseConnection();
                }
                _shaper = null;
            }
        }

#if !NET40

        internal override IDbAsyncEnumerator GetAsyncEnumeratorInternal()
        {
            return GetDbEnumerator();
        }

#endif

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

        /// <summary>
        /// Gets the type of the <see cref="T:System.Data.Entity.Core.Objects.ObjectResult`1" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Type" /> that is the type of the <see cref="T:System.Data.Entity.Core.Objects.ObjectResult`1" />.
        /// </returns>
        public override Type ElementType
        {
            get { return typeof(T); }
        }
    }
}
