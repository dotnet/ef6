// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;

    /// <summary>
    /// Extends <see cref="SortableBindingList{T}" /> to create a sortable binding list that stays in
    /// sync with an underlying <see cref="ObservableCollection{T}" />.  That is, when items are added
    /// or removed from the binding list, they are added or removed from the ObservableCollecion, and
    /// vice-versa.
    /// </summary>
    /// <typeparam name="T"> The list element type. </typeparam>
    internal class ObservableBackedBindingList<T> : SortableBindingList<T>
    {
        #region Fields and constructors

        private bool _addingNewInstance;
        private T _addNewInstance;
        private T _cancelNewInstance;

        private readonly ObservableCollection<T> _obervableCollection;
        private bool _inCollectionChanged;
        private bool _changingObservableCollection;

        /// <summary>
        /// Initializes a new instance of a binding list backed by the given <see cref="ObservableCollection{T}" />
        /// </summary>
        /// <param name="obervableCollection"> The obervable collection. </param>
        public ObservableBackedBindingList(ObservableCollection<T> obervableCollection)
            : base(obervableCollection.ToList())
        {
            _obervableCollection = obervableCollection;
            _obervableCollection.CollectionChanged += ObservableCollectionChanged;
        }

        #endregion

        #region BindingList overrides

        /// <summary>
        /// Creates a new item to be added to the binding list.
        /// </summary>
        /// <returns> The new item. </returns>
        protected override object AddNewCore()
        {
            _addingNewInstance = true;
            _addNewInstance = (T)base.AddNewCore();
            return _addNewInstance;
        }

        /// <summary>
        /// Cancels adding of a new item that was started with AddNew.
        /// </summary>
        /// <param name="itemIndex"> Index of the item. </param>
        public override void CancelNew(int itemIndex)
        {
            if (itemIndex >= 0
                && itemIndex < Count
                && Equals(base[itemIndex], _addNewInstance))
            {
                _cancelNewInstance = _addNewInstance;
                _addNewInstance = default(T);
                _addingNewInstance = false;
            }
            base.CancelNew(itemIndex);
        }

        /// <summary>
        /// Removes all items from the binding list and underlying ObservableCollection.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (var entity in Items)
            {
                RemoveFromObservableCollection(entity);
            }
            base.ClearItems();
        }

        /// <summary>
        /// Ends the process of adding a new item that was started with AddNew.
        /// </summary>
        /// <param name="itemIndex"> Index of the item. </param>
        public override void EndNew(int itemIndex)
        {
            if (itemIndex >= 0
                && itemIndex < Count
                && Equals(base[itemIndex], _addNewInstance))
            {
                AddToObservableCollection(_addNewInstance);
                _addNewInstance = default(T);
                _addingNewInstance = false;
            }
            base.EndNew(itemIndex);
        }

        /// <summary>
        /// Inserts the item into the binding list at the given index.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <param name="item"> The item. </param>
        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            if (!_addingNewInstance
                && index >= 0
                && index <= Count)
            {
                AddToObservableCollection(item);
            }
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index"> The index. </param>
        protected override void RemoveItem(int index)
        {
            if (index >= 0
                && index < Count
                && Equals(base[index], _cancelNewInstance))
            {
                _cancelNewInstance = default(T);
            }
            else
            {
                RemoveFromObservableCollection(base[index]);
            }
            base.RemoveItem(index);
        }

        /// <summary>
        /// Sets the item into the list at the given position.
        /// </summary>
        /// <param name="index"> The index to insert at. </param>
        /// <param name="item"> The item. </param>
        protected override void SetItem(int index, T item)
        {
            var entity = base[index];
            base.SetItem(index, item);

            if (index >= 0
                && index < Count)
            {
                // Check to see if the user is trying to set an item that is currently being added via AddNew
                // If so then the list should not continue the AddNew; but instead add the item
                // that is being passed in.
                if (Equals(entity, _addNewInstance))
                {
                    _addNewInstance = default(T);
                    _addingNewInstance = false;
                }
                else
                {
                    RemoveFromObservableCollection(entity);
                }
                AddToObservableCollection(item);
            }
        }

        #endregion

        #region ObservaleCollection management

        /// <summary>
        /// Event handler to update the binding list when the underlying observable collection changes.
        /// </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> Data indicating how the collection has changed. </param>
        private void ObservableCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Don't try to change the binding list if the original change came from the binding list
            // and the ObervableCollection is just being changed to match it.
            if (!_changingObservableCollection)
            {
                try
                {
                    // We are about to change the underlying binding list.  We want to prevent those
                    // changes trying to go back into the ObservableCollection, so we set a flag
                    // to prevent that.
                    _inCollectionChanged = true;

                    if (e.Action
                        == NotifyCollectionChangedAction.Reset)
                    {
                        Clear();
                    }

                    if (e.Action == NotifyCollectionChangedAction.Remove
                        ||
                        e.Action == NotifyCollectionChangedAction.Replace)
                    {
                        foreach (T entity in e.OldItems)
                        {
                            Remove(entity);
                        }
                    }

                    if (e.Action == NotifyCollectionChangedAction.Add
                        ||
                        e.Action == NotifyCollectionChangedAction.Replace)
                    {
                        foreach (T entity in e.NewItems)
                        {
                            Add(entity);
                        }
                    }
                }
                finally
                {
                    _inCollectionChanged = false;
                }
            }
        }

        /// <summary>
        /// Adds the item to the underlying observable collection.
        /// </summary>
        /// <param name="item"> The item. </param>
        private void AddToObservableCollection(T item)
        {
            // Don't try to change the ObervableCollection if the original change
            // came from the ObservableCollection
            if (!_inCollectionChanged)
            {
                try
                {
                    // We are about to change the ObservableCollection based on the binding list.
                    // We don't want to try to put that change into the ObservableCollection again,
                    // so we set a flag to prevent this.
                    _changingObservableCollection = true;
                    _obervableCollection.Add(item);
                }
                finally
                {
                    _changingObservableCollection = false;
                }
            }
        }

        /// <summary>
        /// Removes the item from the underlying from observable collection.
        /// </summary>
        /// <param name="item"> The item. </param>
        private void RemoveFromObservableCollection(T item)
        {
            // Don't try to change the ObervableCollection if the original change
            // came from the ObservableCollection
            if (!_inCollectionChanged)
            {
                try
                {
                    // We are about to change the ObservableCollection based on the binding list.
                    // We don't want to try to put that change into the ObservableCollection again,
                    // so we set a flag to prevent this.
                    _changingObservableCollection = true;
                    _obervableCollection.Remove(item);
                }
                finally
                {
                    _changingObservableCollection = false;
                }
            }
        }

        #endregion
    }
}
