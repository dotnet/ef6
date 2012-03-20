namespace System.Data.Entity.Internal
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    ///     Generic wrapper around <see cref = "InternalSqlQuery" /> to allow results to be
    ///     returned as generic <see cref = "IEnumerable{T}" />
    /// </summary>
    /// <typeparam name = "TElement">The type of the element.</typeparam>
    internal class InternalSqlQuery<TElement> : IEnumerable<TElement>, IListSource
    {
        #region Constructors and fields

        private readonly InternalSqlQuery _internalQuery;

        internal InternalSqlQuery(InternalSqlQuery internalQuery)
        {
            _internalQuery = internalQuery;
        }

        #endregion

        #region IEnumerable implementation

        /// <summary>
        ///     Executes the query and returns an enumerator for the elements.
        /// </summary>
        /// An
        /// <see cref = "IEnumerator{T}" />
        /// object that can be used to iterate through the elements.
        public IEnumerator<TElement> GetEnumerator()
        {
            return (IEnumerator<TElement>)_internalQuery.GetEnumerator();
        }

        /// <summary>
        ///     Executes the query and returns an enumerator for the elements.
        /// </summary>
        /// <returns>
        ///     An <see cref = "T:System.Collections.IEnumerator" /> object that can be used to iterate through the elements.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region ToString

        /// <summary>
        ///     Returns a <see cref = "System.String" /> that contains the SQL string that was set
        ///     when the query was created.  The parameters are not included.
        /// </summary>
        /// <returns>
        ///     A <see cref = "System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return _internalQuery.ToString();
        }

        #endregion

        #region IListSource implementation

        /// <summary>
        ///     Returns <c>false</c>.
        /// </summary>
        /// <returns><c>false</c>.</returns>
        bool IListSource.ContainsListCollection
        {
            get { return _internalQuery.ContainsListCollection; }
        }

        /// <summary>
        ///     Throws an exception indicating that binding directly to a store query is not supported.
        /// </summary>
        /// <returns>
        ///     Never returns; always throws.
        /// </returns>
        IList IListSource.GetList()
        {
            return _internalQuery.GetList();
        }

        #endregion
    }
}