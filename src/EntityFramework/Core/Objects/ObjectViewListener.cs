// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Objects
{
    using System.Collections;
    using System.ComponentModel;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Diagnostics;

    internal sealed class ObjectViewListener
    {
        private readonly WeakReference _viewWeak;
        private readonly object _dataSource;
        private readonly IList _list;

        internal ObjectViewListener(IObjectView view, IList list, object dataSource)
        {
            _viewWeak = new WeakReference(view);
            _dataSource = dataSource;
            _list = list;

            RegisterCollectionEvents();
            RegisterEntityEvents();
        }

        private void CleanUpListener()
        {
            UnregisterCollectionEvents();
            UnregisterEntityEvents();
        }

        private void RegisterCollectionEvents()
        {
            var cache = _dataSource as ObjectStateManager;
            if (cache != null)
            {
                cache.EntityDeleted += CollectionChanged;
            }
            else if (null != _dataSource)
            {
                ((RelatedEnd)_dataSource).AssociationChangedForObjectView += CollectionChanged;
            }
        }

        private void UnregisterCollectionEvents()
        {
            var cache = _dataSource as ObjectStateManager;
            if (cache != null)
            {
                cache.EntityDeleted -= CollectionChanged;
            }
            else if (null != _dataSource)
            {
                ((RelatedEnd)_dataSource).AssociationChangedForObjectView -= CollectionChanged;
            }
        }

        internal void RegisterEntityEvents(object entity)
        {
            Debug.Assert(entity != null, "Entity should not be null");
            var propChanged = entity as INotifyPropertyChanged;
            if (propChanged != null)
            {
                propChanged.PropertyChanged += EntityPropertyChanged;
            }
        }

        private void RegisterEntityEvents()
        {
            if (null != _list)
            {
                foreach (var entityObject in _list)
                {
                    var propChanged = entityObject as INotifyPropertyChanged;
                    if (propChanged != null)
                    {
                        propChanged.PropertyChanged += EntityPropertyChanged;
                    }
                }
            }
        }

        internal void UnregisterEntityEvents(object entity)
        {
            Debug.Assert(entity != null, "entity should not be null");
            var propChanged = entity as INotifyPropertyChanged;
            if (propChanged != null)
            {
                propChanged.PropertyChanged -= EntityPropertyChanged;
            }
        }

        private void UnregisterEntityEvents()
        {
            if (null != _list)
            {
                foreach (var entityObject in _list)
                {
                    var propChanged = entityObject as INotifyPropertyChanged;
                    if (propChanged != null)
                    {
                        propChanged.PropertyChanged -= EntityPropertyChanged;
                    }
                }
            }
        }

        private void EntityPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var view = (IObjectView)_viewWeak.Target;
            if (view != null)
            {
                view.EntityPropertyChanged(sender, e);
            }
            else
            {
                CleanUpListener();
            }
        }

        private void CollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            var view = (IObjectView)_viewWeak.Target;
            if (view != null)
            {
                view.CollectionChanged(sender, e);
            }
            else
            {
                CleanUpListener();
            }
        }
    }
}
