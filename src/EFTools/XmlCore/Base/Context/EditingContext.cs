// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Context
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model;

    /// <summary>
    ///     The EditingContext class contains contextual state about a designer.  This includes permanent
    ///     state such as list of services running in the designer.
    ///     It also includes transient state consisting of context items.  Examples of transient
    ///     context item state include the set of currently selected objects as well as the editing tool
    ///     being used to manipulate objects on the design surface.
    ///     The editing context is designed to be a concrete class for ease of use.  It does have a protected
    ///     API that can be used to replace its implementation.
    /// </summary>
    internal class EditingContext : IDisposable
    {
        private ContextItemCollection _contextItems;
        private EFArtifactService _efArtifactService;

        /// <summary>
        ///     The Disposing event gets fired just before the context gets disposed.
        /// </summary>
        internal event EventHandler Disposing;

        internal event EventHandler Reloaded;

        internal virtual void OnReloaded(EventArgs args)
        {
            if (Reloaded != null)
            {
                Reloaded(this, args);
            }
        }

        /// <summary>
        ///     Finalizer that implements the IDisposable pattern.
        /// </summary>
        ~EditingContext()
        {
            Dispose(false);
        }

        internal EFArtifactService GetEFArtifactService()
        {
            return _efArtifactService;
        }

        internal void SetEFArtifactService(EFArtifactService artifactService)
        {
            _efArtifactService = artifactService;
        }

        /// <summary>
        ///     Returns the local collection of context items offered by this editing context.
        /// </summary>
        /// <value></value>
        internal ContextItemCollection Items
        {
            get
            {
                if (_contextItems == null)
                {
                    _contextItems = CreateContextItemCollection();
                    if (_contextItems == null)
                    {
                        throw new InvalidOperationException();
                    }
                }

                return _contextItems;
            }
        }

        /// <summary>
        ///     Creates an instance of the context item collection to be returned from
        ///     the ContextItems property.  The default implementation creates a
        ///     ContextItemCollection that supports delayed activation of design editor
        ///     collections through the declaration of a SubscribeContext attribute on
        ///     the design editor manager.
        /// </summary>
        /// <returns>Returns an implementation of the ContextItemCollection class.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected virtual ContextItemCollection CreateContextItemCollection()
        {
            return new DefaultContextItemCollection(this);
        }

        /// <summary>
        ///     Disposes this editing context.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Disposes this editing context.
        /// </summary>
        /// <param name="disposing">True if this object is being disposed, or false if it is finalizing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Let any interested parties know the context is being disposed
                if (Disposing != null)
                {
                    Disposing(this, EventArgs.Empty);
                }

                if (_contextItems != null)
                {
                    foreach (var contextItem in _contextItems)
                    {
                        var d = contextItem as IDisposable;
                        if (d != null)
                        {
                            d.Dispose();
                        }
                    }
                }

                _efArtifactService = null;
                _contextItems = null;
            }
        }

        /// <summary>
        ///     This is here because we end up with multiple commands executing in the same stack, and we only want the first CommandProcessor to
        ///     start/end the parent undo unit.
        /// </summary>
        internal bool ParentUndoUnitStarted { get; set; }

        /// <summary>
        ///     This is the default context item collection for our editing context.
        /// </summary>
        private sealed class DefaultContextItemCollection : ContextItemCollection, IDisposable
        {
            private readonly EditingContext _context;
            private DefaultContextLayer _currentLayer;
            private Dictionary<Type, SubscribeContextCallback> _subscriptions;

            internal DefaultContextItemCollection(EditingContext context)
            {
                _context = context;
                _currentLayer = new DefaultContextLayer(this, null);
            }

            ~DefaultContextItemCollection()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_context != null)
                    {
                        _context.Dispose();
                    }
                    if (_currentLayer != null)
                    {
                        _currentLayer.Dispose();
                    }
                }
            }

            /// <summary>
            ///     This changes a context item to the given value.  It is illegal to pass
            ///     null here.  If you want to set a context item to its empty value create
            ///     an instance of the item using a default constructor.
            /// </summary>
            /// <param name="value"></param>
            internal override void SetValue(ContextItem value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                // The rule for change is that we store the new value,
                // raise a change on the item, and then raise a change
                // to everyone else.  If changing the item fails, we recover
                // the previous item.
                ContextItem existing, existingRawValue;
                existing = existingRawValue = GetValueNull(value.ItemType);

                if (existing == null)
                {
                    existing = GetValue(value.ItemType);
                }

                var success = false;

                try
                {
                    _currentLayer.Items[value.ItemType] = value;
                    NotifyItemChanged(_context, value, existing);
                    success = true;
                }
                finally
                {
                    if (success)
                    {
                        OnItemChanged(value);
                    }
                    else
                    {
                        // The item threw during its transition to 
                        // becoming active.  Put the old one back.
                        // We must put the old one back by re-activating
                        // it.  This could throw a second time, so we
                        // cover this case by removing the value first.
                        // Should it throw again, we won't recurse because
                        // the existing raw value would be null.

                        _currentLayer.Items.Remove(value.ItemType);
                        if (existingRawValue != null)
                        {
                            SetValue(existingRawValue);
                        }
                    }
                }
            }

            /// <summary>
            ///     Returns true if the item collection contains an item of the given type.
            ///     This only looks in the current layer.
            /// </summary>
            /// <param name="itemType"></param>
            /// <returns></returns>
            internal override bool Contains(Type itemType)
            {
                if (itemType == null)
                {
                    throw new ArgumentNullException("itemType");
                }
                if (!typeof(ContextItem).IsAssignableFrom(itemType))
                {
                    throw new ArgumentException("Incorrect Argument Type", "itemType");
                    /*
                    throw new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture, 
                        Resources.Error_ArgIncorrectType, 
                        "itemType", typeof(ContextItem).FullName));
                    */
                }

                return _currentLayer.Items.ContainsKey(itemType);
            }

            internal override bool Remove(Type itemType)
            {
                if (itemType == null)
                {
                    throw new ArgumentNullException("itemType");
                }
                if (!typeof(ContextItem).IsAssignableFrom(itemType))
                {
                    throw new ArgumentException("Incorrect Argument Type", "itemType");
                    /*
                    throw new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture, 
                        Resources.Error_ArgIncorrectType, 
                        "itemType", typeof(ContextItem).FullName));
                    */
                }
                return _currentLayer.Items.Remove(itemType);
            }

            /// <summary>
            ///     This helper function returns the childLayer for the layer that is passed in.
            ///     This function is used in the OnLayerRemoved to link the layers when
            ///     a layer (in the middle) is removed.
            /// </summary>
            /// <param name="layer"></param>
            /// <returns></returns>
            private DefaultContextLayer FindChildLayer(DefaultContextLayer layer)
            {
                var startLayer = _currentLayer;
                while (startLayer.ParentLayer != layer)
                {
                    startLayer = startLayer.ParentLayer;
                }
                return startLayer;
            }

            /// <summary>
            ///     Returns an instance of the requested item type.  If there is no context
            ///     item with the given type, an empty item will be created.
            /// </summary>
            internal override ContextItem GetValue(Type itemType)
            {
                var item = GetValueNull(itemType);

                if (item == null)
                {
                    // Check the default item table and add a new
                    // instance there if we need to
                    if (!_currentLayer.DefaultItems.TryGetValue(itemType, out item))
                    {
                        item = (ContextItem)Activator.CreateInstance(itemType);

                        // Verify that the resulting item has the correct item type
                        // If it doesn't, it means that the user provided a derived
                        // item type
                        if (item.ItemType != itemType)
                        {
                            throw new ArgumentException("Error in DerivedContextItem", itemType.FullName);
                            /*
                            throw new ArgumentException(string.Format(
                                CultureInfo.CurrentCulture, 
                                Resources.Error_DerivedContextItem,
                                itemType.FullName,
                                item.ItemType.FullName));
                            */
                        }

                        // Now push the item in the context so we have
                        // a consistent reference
                        _currentLayer.DefaultItems.Add(item.ItemType, item);
                    }
                }

                return item;
            }

            /// <summary>
            ///     Similar to GetValue, but returns NULL if the item isn't found instead of
            ///     creating an empty item.
            /// </summary>
            private ContextItem GetValueNull(Type itemType)
            {
                if (itemType == null)
                {
                    throw new ArgumentNullException("itemType");
                }
                if (!typeof(ContextItem).IsAssignableFrom(itemType))
                {
                    throw new ArgumentException("Incorrect Type", "itemType");
                    /*                
                    throw new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture, 
                        Resources.Error_ArgIncorrectType, 
                        "itemType", typeof(ContextItem).FullName));
                    */
                }

                ContextItem item = null;
                var layer = _currentLayer;
                while (layer != null
                       && !layer.Items.TryGetValue(itemType, out item))
                {
                    layer = layer.ParentLayer;
                }

                return item;
            }

            /// <summary>
            ///     Creates a new editing context layer.  Editing context layers can be used to
            ///     create editing modes.  For example, you may create a layer before starting a
            ///     drag operation on the designer.  Any new context items you add to the layer
            ///     hide context items underneath it.  When the layer is removed, all context
            ///     items under the layer are re-surfaced.  This allows you to create a layer
            ///     and set overrides for context items during operations such as drag and drop.
            /// </summary>
            internal override ContextLayer CreateLayer()
            {
                _currentLayer = new DefaultContextLayer(this, _currentLayer);
                return _currentLayer;
            }

            /// <summary>
            ///     Enumerates the context items in the editing context.  This enumeration
            ///     includes prior layers unless the enumerator hits an isolated layer.
            ///     Enumeration is typically not useful in most scenarios but it is provided so
            ///     that developers can search in the context and learn what is placed in it.
            /// </summary>
            public override IEnumerator<ContextItem> GetEnumerator()
            {
                return _currentLayer.Items.Values.GetEnumerator();
            }

            /// <summary>
            ///     Called when an item changes value.  This happens in one of two ways:
            ///     either the user has called Change, or the user has removed a layer.
            /// </summary>
            private void OnItemChanged(ContextItem item)
            {
                SubscribeContextCallback callback;

                Debug.Assert(item != null, "You cannot pass a null item here.");

                if (_subscriptions != null
                    && _subscriptions.TryGetValue(item.ItemType, out callback))
                {
                    callback(item);
                }
            }

            /// <summary>
            ///     Called when the user removes a layer.
            /// </summary>
            private void OnLayerRemoved(DefaultContextLayer layer)
            {
                if (_currentLayer == layer)
                {
                    _currentLayer = layer.ParentLayer;
                }
                else
                {
                    var childLayer = FindChildLayer(layer);
                    childLayer.ParentLayer = layer.ParentLayer;
                }

                Debug.Assert(_currentLayer != null, "DefaultContextLayer should not call OnLayerRemoved for top most layer");

                // For each item that was in the layer, raise a changed event for it
                if (_subscriptions != null)
                {
                    foreach (var oldItem in layer.Items.Values)
                    {
                        OnItemChanged(GetValue(oldItem.ItemType));
                    }
                }
            }

            /// <summary>
            ///     Adds an event callback that will be invoked with a context item of the given item type changes.
            /// </summary>
            internal override void Subscribe(Type contextItemType, SubscribeContextCallback callback)
            {
                if (contextItemType == null)
                {
                    throw new ArgumentNullException("contextItemType");
                }
                if (callback == null)
                {
                    throw new ArgumentNullException("callback");
                }
                if (!typeof(ContextItem).IsAssignableFrom(contextItemType))
                {
                    throw new ArgumentException("Argument Incorrect Type", "contextItemType");
                    /*
                    throw new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture,
                        Resources.Error_ArgIncorrectType,
                        "contextItemType", typeof(ContextItem).FullName));
                    */
                }

                if (_subscriptions == null)
                {
                    _subscriptions = new Dictionary<Type, SubscribeContextCallback>();
                }

                SubscribeContextCallback existing = null;

                _subscriptions.TryGetValue(contextItemType, out existing);

                existing = (SubscribeContextCallback)Delegate.Combine(existing, callback);
                _subscriptions[contextItemType] = existing;

                // If the context is already present, invoke the callback.
                var item = GetValueNull(contextItemType);

                if (item != null)
                {
                    callback(item);
                }
            }

            /// <summary>
            ///     Removes a subscription.
            /// </summary>
            internal override void Unsubscribe(Type contextItemType, SubscribeContextCallback callback)
            {
                if (contextItemType == null)
                {
                    throw new ArgumentNullException("contextItemType");
                }
                if (callback == null)
                {
                    throw new ArgumentNullException("callback");
                }
                if (!typeof(ContextItem).IsAssignableFrom(contextItemType))
                {
                    throw new ArgumentException("Argument incorrect type.", "contextItemType");
                    /*
                    throw new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture,
                        Resources.Error_ArgIncorrectType,
                        "contextItemType", typeof(ContextItem).FullName));
                    */
                }
                if (_subscriptions != null)
                {
                    SubscribeContextCallback existing;
                    if (_subscriptions.TryGetValue(contextItemType, out existing))
                    {
                        existing = (SubscribeContextCallback)RemoveCallback(existing, callback);
                        if (existing == null)
                        {
                            _subscriptions.Remove(contextItemType);
                        }
                        else
                        {
                            _subscriptions[contextItemType] = existing;
                        }
                    }
                }
            }

            /// <summary>
            ///     This context layer contains our context items.
            /// </summary>
            private class DefaultContextLayer : ContextLayer
            {
                private readonly DefaultContextItemCollection _collection;
                private DefaultContextLayer _parentLayer;
                private Dictionary<Type, ContextItem> _items;
                private Dictionary<Type, ContextItem> _defaultItems;

                internal DefaultContextLayer(DefaultContextItemCollection collection, DefaultContextLayer parentLayer)
                {
                    _collection = collection;
                    _parentLayer = parentLayer; // can be null
                }

                internal Dictionary<Type, ContextItem> DefaultItems
                {
                    get
                    {
                        if (_defaultItems == null)
                        {
                            _defaultItems = new Dictionary<Type, ContextItem>();
                        }
                        return _defaultItems;
                    }
                }

                internal Dictionary<Type, ContextItem> Items
                {
                    get
                    {
                        if (_items == null)
                        {
                            _items = new Dictionary<Type, ContextItem>();
                        }
                        return _items;
                    }
                }

                internal DefaultContextLayer ParentLayer
                {
                    get { return _parentLayer; }
                    set { _parentLayer = value; }
                }

                internal override void Remove()
                {
                    // Only remove the layer if we have a parent layer.
                    // Also, once we remove the layer make sure we don't
                    // try to remove it if someone else calls Remove.
                    if (_parentLayer != null)
                    {
                        _collection.OnLayerRemoved(this);
                        _parentLayer = null;
                    }
                }
            }
        }
    }
}
