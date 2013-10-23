// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Core.Context
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    ///     The ContextItemCollection class maintains a set of context items.  A context
    ///     item represents a piece of transient state in a designer.
    ///     ContextItems must define an empty constructor.  This empty constructor
    ///     version of a context item represents its default value, and will be the
    ///     value returned from GetItem if the context item manager does not contain
    ///     a context item of the requested type.
    ///     The ContextItemCollection supports context layers.  A context layer is a
    ///     separation in the set of context items and is useful when providing modal
    ///     functions.  For example, when switching modes in the designer to show the
    ///     tab order layout it may be desirable to disable adding items from the
    ///     toolbox and change the user mouse and keyboard gestures to focus on setting
    ///     the tab order.  Rather than grabbing and storing context items before
    ///     replacing them with new values, a developer can simply call CreateLayer.
    ///     Once the layer is created, all subsequent context changes go to that layer.
    ///     When the developer is done with the layer, as would be the case when a user
    ///     switches out of tab order mode, she simply calls Remove on the layer. This
    ///     removes all context items that were added to the layer and restores the context
    ///     to its previous set of values before the layer was created.
    /// </summary>
    public abstract class ContextItemCollection : IEnumerable<ContextItem>
    {
        /// <summary>
        ///     Returns true if the item collection contains an item of the given type.
        /// </summary>
        /// <param name="itemType">The type of item to check.</param>
        /// <returns>True if the context contains an instance of this item type.</returns>
        /// <exception cref="ArgumentNullException">if itemType is null.</exception>
        public abstract bool Contains(Type itemType);

        /// <summary>
        ///     Creates a new editing context layer.  Editing context layers can be used to
        ///     create editing modes.  For example, you may create a layer before starting a
        ///     drag operation on the designer.  Any new context items you add to the layer
        ///     hide context items underneath it.  When the layer is removed, all context
        ///     items under the layer are re-surfaced.  This allows you to create a layer
        ///     and set overrides for context items during operations such as drag and drop.
        /// </summary>
        /// <returns>A new context layer.</returns>
        public abstract ContextLayer CreateLayer();

        /// <summary>
        ///     Enumerates the context items in the editing context.  This enumeration
        ///     includes prior layers unless the enumerator hits an isolated layer.
        ///     Enumeration is typically not useful in most scenarios but it is provided so
        ///     that developers can search in the context and learn what is placed in it.
        /// </summary>
        /// <returns>An enumeration of context items.</returns>
        public abstract IEnumerator<ContextItem> GetEnumerator();

        /// <summary>
        ///     Returns an instance of the requested item type.  If there is no context
        ///     item with the given type, an empty item will be created.
        /// </summary>
        /// <param name="itemType">The type of item to return.</param>
        /// <returns>A context item of the requested type.  If there is no item in the context of this type a default one will be created.</returns>
        /// <exception cref="ArgumentNullException">if itemType is null.</exception>
        public abstract ContextItem GetValue(Type itemType);

        /// <summary>
        ///     Returns an instance of the requested item type.  If there is no context
        ///     item with the given type, an empty item will be created.
        /// </summary>
        /// <typeparam name="ItemType">The type of item to return.</typeparam>
        /// <returns>A context item of the requested type.  If there is no item in the context of this type a default one will be created.</returns>
        public TItemType GetValue<TItemType>() where TItemType : ContextItem
        {
            return (TItemType)GetValue(typeof(TItemType));
        }

        /// <summary>
        ///     This is a helper method that invokes the protected OnItemChanged
        ///     method on ContextItem.
        /// </summary>
        /// <param name="context">The editing context in use.</param>
        /// <param name="item">The new context item.</param>
        /// <param name="previousItem">The previous context item.</param>
        /// <exception cref="ArgumentNullException">if context, item or previousItem is null.</exception>
        protected static void NotifyItemChanged(EditingContext context, ContextItem item, ContextItem previousItem)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            item.InvokeOnItemChanged(context, previousItem);
        }

        /// <summary>
        ///     This sets a context item to the given value.  It is illegal to pass
        ///     null here.  If you want to set a context item to its empty value create
        ///     an instance of the item using a default constructor.
        /// </summary>
        /// <param name="value">The value to set into the context item manager.</param>
        public abstract void SetValue(ContextItem value);

        /// <summary>
        ///     Adds an event callback that will be invoked with a context item of the given item type changes.
        /// </summary>
        /// <param name="contextItemType">The type of item you wish to subscribe to.</param>
        /// <param name="callback">A callback that will be invoked when contextItemType changes.</param>
        /// <exception cref="ArgumentNullException">if contextItemType or callback is null.</exception>
        public abstract void Subscribe(Type contextItemType, SubscribeContextCallback callback);

        /// <summary>
        ///     Adds an event callback that will be invoked with a context item of the given item type changes.
        /// </summary>
        /// <typeparam name="ContextItemType">The type of item you wish to subscribe to.</typeparam>
        /// <param name="callback">A callback that will be invoked when contextItemType changes.</param>
        /// <exception cref="ArgumentNullException">if callback is null.</exception>
        public void Subscribe<TContextItemType>(SubscribeContextCallback<TContextItemType> callback) where TContextItemType : ContextItem
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            var proxy = new SubscribeProxy<TContextItemType>(callback);
            Subscribe(typeof(TContextItemType), proxy.Callback);
        }

        /// <summary>
        ///     Removes a subscription.
        /// </summary>
        /// <typeparam name="ContextItemType">The type of context item to remove the callback from.</typeparam>
        /// <param name="callback">The callback to remove.</param>
        /// <exception cref="ArgumentNullException">if callback is null.</exception>
        public void Unsubscribe<TContextItemType>(SubscribeContextCallback<TContextItemType> callback) where TContextItemType : ContextItem
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            var proxy = new SubscribeProxy<TContextItemType>(callback);
            Unsubscribe(typeof(TContextItemType), proxy.Callback);
        }

        /// <summary>
        ///     Removes a subscription.
        /// </summary>
        /// <param name="contextItemType">The type of context item to remove the callback from.</param>
        /// <param name="callback">The callback to remove.</param>
        /// <exception cref="ArgumentNullException">if contextItemType or callback is null.</exception>
        public abstract void Unsubscribe(Type contextItemType, SubscribeContextCallback callback);

        /// <summary>
        ///     This is a helper method that returns the target object for a delegate.
        ///     If the delegate was created to proxy a generic delegate, this will correctly
        ///     return the original object, not the proxy.
        /// </summary>
        /// <param name="callback">The callback whose target you want.</param>
        /// <exception cref="ArgumentNullException">if callback is null.</exception>
        /// <returns>The target object of the callback.</returns>
        protected static object GetTarget(Delegate callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            var proxy = callback.Target as ICallbackProxy;
            if (proxy != null)
            {
                return proxy.OriginalTarget;
            }

            return callback.Target;
        }

        /// <summary>
        ///     This is a helper method that performs a Delegate.Remove, but knows
        ///     how to unwrap delegates that are proxies to generic callbacks.  Use
        ///     this in your Unsubscribe implementations.
        /// </summary>
        /// <param name="existing">The existing delegate.</param>
        /// <param name="toRemove">The delegate to be removed from existing.</param>
        /// <returns>The new delegate that should be assigned to existing.</returns>
        protected static Delegate RemoveCallback(Delegate existing, Delegate toRemove)
        {
            if (existing == null)
            {
                return null;
            }
            if (toRemove == null)
            {
                return existing;
            }

            var toRemoveProxy = toRemove.Target as ICallbackProxy;
            if (toRemoveProxy == null)
            {
                // The item to be removed is a normal delegate.  Just call
                // Delegate.Remove
                return Delegate.Remove(existing, toRemove);
            }

            toRemove = toRemoveProxy.OriginalDelegate;

            var invocationList = existing.GetInvocationList();
            var removedItems = false;

            for (var idx = 0; idx < invocationList.Length; idx++)
            {
                var item = invocationList[idx];
                var itemProxy = item.Target as ICallbackProxy;
                if (itemProxy != null)
                {
                    item = itemProxy.OriginalDelegate;
                }

                if (item.Equals(toRemove))
                {
                    invocationList[idx] = null;
                    removedItems = true;
                }
            }

            if (removedItems)
            {
                // We must create a new delegate containing the 
                // invocation list that is is left
                existing = null;
                foreach (var d in invocationList)
                {
                    if (d != null)
                    {
                        if (existing == null)
                        {
                            existing = d;
                        }
                        else
                        {
                            existing = Delegate.Combine(existing, d);
                        }
                    }
                }
            }

            return existing;
        }

        /// <summary>
        ///     Implementation of default IEnumerable.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     This is a simple proxy that converts a non-generic subscribe callback to a generic
        ///     one.
        /// </summary>
        /// <typeparam name="ContextItemType"></typeparam>
        private class SubscribeProxy<ContextItemType> : ICallbackProxy
            where ContextItemType : ContextItem
        {
            private readonly SubscribeContextCallback<ContextItemType> _genericCallback;

            internal SubscribeProxy(SubscribeContextCallback<ContextItemType> callback)
            {
                _genericCallback = callback;
            }

            internal SubscribeContextCallback Callback
            {
                get { return SubscribeContext; }
            }

            private void SubscribeContext(ContextItem item)
            {
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }
                _genericCallback((ContextItemType)item);
            }

            Delegate ICallbackProxy.OriginalDelegate
            {
                get { return _genericCallback; }
            }

            object ICallbackProxy.OriginalTarget
            {
                get { return _genericCallback.Target; }
            }
        }

        private interface ICallbackProxy
        {
            Delegate OriginalDelegate { get; }
            object OriginalTarget { get; }
        }
    }
}
