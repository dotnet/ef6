// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    // <summary>
    // Manages a binding list constructed from query results.
    // </summary>
    // <typeparam name="TElement"> Type of the elements in the binding list. </typeparam>
    // <remarks>
    // The binding list is initialized from query results.
    // If the binding list can be modified,
    // objects are added or removed from the ObjectStateManager (via the ObjectContext).
    // </remarks>
    internal sealed class ObjectViewQueryResultData<TElement> : IObjectViewData<TElement>
    {
        private readonly List<TElement> _bindingList;

        // <summary>
        // ObjectContext used to add or delete objects when the list can be modified.
        // </summary>
        private readonly ObjectContext _objectContext;

        // <summary>
        // If the TElement type is an Entity type of some kind,
        // this field specifies the entity set to add entity objects.
        // </summary>
        private readonly EntitySet _entitySet;

        private readonly bool _canEditItems;
        private readonly bool _canModifyList;

        // <summary>
        // Construct a new instance of the ObjectViewQueryResultData class using the supplied query results.
        // </summary>
        // <param name="queryResults"> Result of object query execution used to populate the binding list. </param>
        // <param name="objectContext"> ObjectContext used to add or remove items. If the binding list can be modified, this parameter should not be null. </param>
        // <param name="forceReadOnlyList">
        // <b>True</b> if items should not be allowed to be added or removed from the binding list. Note that other conditions may prevent the binding list from being modified, so a value of <b>false</b> supplied for this parameter doesn't necessarily mean that the list will be writable.
        // </param>
        // <param name="entitySet"> If the TElement type is an Entity type of some kind, this field specifies the entity set to add entity objects. </param>
        internal ObjectViewQueryResultData(
            IEnumerable queryResults, ObjectContext objectContext, bool forceReadOnlyList, EntitySet entitySet)
        {
            var canTrackItemChanges = IsEditable(typeof(TElement));

            _objectContext = objectContext;
            _entitySet = entitySet;

            _canEditItems = canTrackItemChanges;
            _canModifyList = !forceReadOnlyList && canTrackItemChanges && _objectContext != null;

            _bindingList = new List<TElement>();
            foreach (TElement element in queryResults)
            {
                _bindingList.Add(element);
            }
        }

        // <summary>
        // Cannot be a DbDataRecord or a derivative of DbDataRecord
        // </summary>
        private static bool IsEditable(Type elementType)
        {
            return !((elementType == typeof(DbDataRecord)) ||
                     ((elementType != typeof(DbDataRecord)) && elementType.IsSubclassOf(typeof(DbDataRecord))));
        }

        // <summary>
        // Throw an exception is an entity set was not specified for this instance.
        // </summary>
        private void EnsureEntitySet()
        {
            if (_entitySet == null)
            {
                throw new InvalidOperationException(Strings.ObjectView_CannotResolveTheEntitySet(typeof(TElement).FullName));
            }
        }

        #region IObjectViewData<T> Members

        public IList<TElement> List
        {
            get { return _bindingList; }
        }

        public bool AllowNew
        {
            get { return _canModifyList && _entitySet != null; }
        }

        public bool AllowEdit
        {
            get { return _canEditItems; }
        }

        public bool AllowRemove
        {
            get { return _canModifyList; }
        }

        public bool FiresEventOnAdd
        {
            get { return false; }
        }

        public bool FiresEventOnRemove
        {
            get { return true; }
        }

        public bool FiresEventOnClear
        {
            get { return false; }
        }

        public void EnsureCanAddNew()
        {
            EnsureEntitySet();
        }

        public int Add(TElement item, bool isAddNew)
        {
            EnsureEntitySet();

            Debug.Assert(_objectContext != null, "ObjectContext is null.");

            // If called for AddNew operation, add item to binding list, pending addition to ObjectContext.
            if (!isAddNew)
            {
                _objectContext.AddObject(TypeHelpers.GetFullName(_entitySet.EntityContainer.Name, _entitySet.Name), item);
            }

            _bindingList.Add(item);

            return _bindingList.Count - 1;
        }

        public void CommitItemAt(int index)
        {
            EnsureEntitySet();

            Debug.Assert(_objectContext != null, "ObjectContext is null.");

            var item = _bindingList[index];
            _objectContext.AddObject(TypeHelpers.GetFullName(_entitySet.EntityContainer.Name, _entitySet.Name), item);
        }

        public void Clear()
        {
            while (0 < _bindingList.Count)
            {
                var entity = _bindingList[_bindingList.Count - 1];

                Remove(entity, false);
            }
        }

        public bool Remove(TElement item, bool isCancelNew)
        {
            bool removed;

            Debug.Assert(_objectContext != null, "ObjectContext is null.");

            if (isCancelNew)
            {
                // Item was previously added to binding list, but not ObjectContext.
                removed = _bindingList.Remove(item);
            }
            else
            {
                var stateEntry = _objectContext.ObjectStateManager.FindEntityEntry(item);

                if (stateEntry != null)
                {
                    stateEntry.Delete();
                    // OnCollectionChanged event will be fired, where the binding list will be updated.
                    removed = true;
                }
                else
                {
                    removed = false;
                }
            }

            return removed;
        }

        public ListChangedEventArgs OnCollectionChanged(object sender, CollectionChangeEventArgs e, ObjectViewListener listener)
        {
            ListChangedEventArgs changeArgs = null;

            // Since event is coming from cache and it might be shared amoung different queries
            // we have to check to see if correct event is being handled.
            if (e.Element.GetType().IsAssignableFrom(typeof(TElement))
                &&
                _bindingList.Contains((TElement)(e.Element)))
            {
                var item = (TElement)e.Element;
                var itemIndex = _bindingList.IndexOf(item);

                if (itemIndex >= 0) // Ignore entities that we don't know about.
                {
                    // Only process "remove" events.
                    Debug.Assert(e.Action != CollectionChangeAction.Refresh, "Cache should never fire with refresh, it does not have clear");

                    if (e.Action
                        == CollectionChangeAction.Remove)
                    {
                        _bindingList.Remove(item);

                        listener.UnregisterEntityEvents(item);

                        changeArgs = new ListChangedEventArgs(ListChangedType.ItemDeleted, itemIndex /* newIndex*/, -1 /* oldIndex*/);
                    }
                }
            }

            return changeArgs;
        }

        #endregion
    }
}
