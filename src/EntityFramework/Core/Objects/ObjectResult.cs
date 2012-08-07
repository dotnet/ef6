// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections;
    using System.ComponentModel;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     This class implements IEnumerable and IDisposable. Instance of this class
    ///     is returned from ObjectQuery.Execute method.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public abstract class ObjectResult : IEnumerable, IDbAsyncEnumerable, IDisposable, IListSource
    {
        internal ObjectResult()
        {
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
        {
            return GetAsyncEnumeratorInternal();
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        #region IListSource

        /// <summary>
        ///     IListSource.ContainsListCollection implementation. Always returns false.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        bool IListSource.ContainsListCollection
        {
            get { return false; // this means that the IList we return is the one which contains our actual data, it is not a collection
            }
        }

        /// <summary>
        ///     IListSource.GetList implementation
        /// </summary>
        /// <returns> IList interface over the data to bind </returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IList IListSource.GetList()
        {
            return GetIListSourceListInternal();
        }

        #endregion

        public abstract Type ElementType { get; }

        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        public abstract void Dispose();

        /// <summary>
        ///     Get the next result set of a stored procedure.
        /// </summary>
        /// <returns> An ObjectResult that enumerates the values of the next result set. null, if there are no more, or if the the ObjectResult is not the result of a stored procedure call. </returns>
        public ObjectResult<TElement> GetNextResult<TElement>()
        {
            return GetNextResultInternal<TElement>();
        }

        internal abstract IDbAsyncEnumerator GetAsyncEnumeratorInternal();
        internal abstract IEnumerator GetEnumeratorInternal();
        internal abstract IList GetIListSourceListInternal();
        internal abstract ObjectResult<TElement> GetNextResultInternal<TElement>();
    }
}
