// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections;
    using System.ComponentModel;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// This class implements IEnumerable and IDisposable. Instance of this class
    /// is returned from ObjectQuery.Execute method.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public abstract class ObjectResult : IEnumerable, IDisposable, IListSource
#if !NET40
, IDbAsyncEnumerable
#endif

    {
        internal ObjectResult()
        {
        }

#if !NET40

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
        {
            return GetAsyncEnumeratorInternal();
        }

#endif

        /// <summary>Returns an enumerator that iterates through the query results.</summary>
        /// <returns>An enumerator that iterates through the query results.</returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        #region IListSource

        /// <summary>
        /// IListSource.ContainsListCollection implementation. Always returns false.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        bool IListSource.ContainsListCollection
        {
            get
            {
                return false; // this means that the IList we return is the one which contains our actual data, it is not a collection
            }
        }

        /// <summary>Returns the results in a format useful for data binding.</summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IList" /> of entity objects.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IList IListSource.GetList()
        {
            return GetIListSourceListInternal();
        }

        #endregion

        /// <summary>
        /// When overridden in a derived class, gets the type of the generic
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectResult`1" />
        /// .
        /// </summary>
        /// <returns>
        /// The type of the generic <see cref="T:System.Data.Entity.Core.Objects.ObjectResult`1" />.
        /// </returns>
        public abstract Type ElementType { get; }

        /// <summary>Performs tasks associated with freeing, releasing, or resetting resources.</summary>
        public void Dispose()
        {
            Dispose(true);

            // Use SuppressFinalize in case a subclass 
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases the resources used by the object result.</summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected abstract void Dispose(bool disposing);

        /// <summary>Gets the next result set of a stored procedure.</summary>
        /// <returns>An ObjectResult that enumerates the values of the next result set. Null, if there are no more, or if the ObjectResult is not the result of a stored procedure call.</returns>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        public ObjectResult<TElement> GetNextResult<TElement>()
        {
            return GetNextResultInternal<TElement>();
        }

#if !NET40

        internal abstract IDbAsyncEnumerator GetAsyncEnumeratorInternal();

#endif

        internal abstract IEnumerator GetEnumeratorInternal();
        internal abstract IList GetIListSourceListInternal();
        internal abstract ObjectResult<TElement> GetNextResultInternal<TElement>();
    }
}
