// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Extends <see cref="ObservableCollection{T}" /> and adds an explicit implementation of <see cref="IListSource" />.
    /// The GetList method of IListSource is implemented to return an <see cref="IBindingList" /> implementation that
    /// stays in sync with the ObservableCollection.
    /// This class can be used to implement navigation properties on entities for use in Windows Forms data binding.
    /// For WPF data binding using an ObservableCollection rather than an instance of this class is recommended.
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix",
        Justification = "Adding Collection makes the name too long.")]
    public class ObservableListSource<T> : ObservableCollection<T>, IListSource
        where T : class
    {
        #region Fields and constructors

        private IBindingList _bindingList;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableListSource{T}" /> class.
        /// </summary>
        public ObservableListSource()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableListSource{T}" /> class that
        /// contains elements copied from the specified collection.
        /// </summary>
        /// <param name="collection"> The collection from which the elements are copied. </param>
        public ObservableListSource(IEnumerable<T> collection)
            : base(collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableListSource{T}" /> class that
        /// contains elements copied from the specified list.
        /// </summary>
        /// <param name="list"> The list from which the elements are copied. </param>
        public ObservableListSource(List<T> list)
            : base(list)
        {
        }

        #endregion

        #region IListSource implementation

        /// <summary>
        /// Returns <c>false</c>.
        /// </summary>
        /// <returns> <c>false</c> . </returns>
        bool IListSource.ContainsListCollection
        {
            get { return false; }
        }

        /// <summary>
        /// Returns an <see cref="IBindingList" /> implementation that stays in sync with this <see cref="ObservableCollection{T}" />.
        /// The returned list is cached on this object such that the same list is returned each time this method is called.
        /// </summary>
        /// <returns> An <see cref="IBindingList" /> implementation that stays in sync with this <see
        ///  cref="ObservableCollection{T}" /> . </returns>
        IList IListSource.GetList()
        {
            return _bindingList ?? (_bindingList = this.ToBindingList());
        }

        #endregion
    }
}
