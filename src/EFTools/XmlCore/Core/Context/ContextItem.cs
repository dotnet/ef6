// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Core.Context
{
    using System;

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public abstract class ContextItem
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <value></value>
        public abstract Type ItemType { get; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="context">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="previousItem">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        protected virtual void OnItemChanged(EditingContext context, ContextItem previousItem)
        {
        }

        //
        // Internal API that calls OnItemChanged.  This is invoked from the
        // abstract ContextItemCollection class so deriving classes can still
        // invoke it.
        //
        internal void InvokeOnItemChanged(EditingContext context, ContextItem previousItem)
        {
            OnItemChanged(context, previousItem);
        }

        /// <summary>
        ///     Indicates if the current ContextItem can be replaced in the EditingContext
        /// </summary>
        internal virtual bool CanBeReplaced
        {
            get { return true; }
        }
    }

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    /// <typeparam name="T">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</typeparam>
    public class ContextItem<T> : ContextItem
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public ContextItem()
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="obj">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        public ContextItem(T obj)
        {
            _object = obj;
        }

        private readonly T _object;

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public T Object
        {
            get { return _object; }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public override Type ItemType
        {
            get { return typeof(ContextItem<T>); }
        }
    }
}
