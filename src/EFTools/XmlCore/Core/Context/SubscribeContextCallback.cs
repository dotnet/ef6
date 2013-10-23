// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Core.Context
{
    /// <summary>
    ///     Defines a callback method that will be invoked when a context item
    ///     changes.
    /// </summary>
    /// <param name="item">The context item that has changed.</param>
    public delegate void SubscribeContextCallback(ContextItem item);

    /// <summary>
    ///     Defines a callback method that will be invoked when a context item
    ///     changes.
    /// </summary>
    /// <typeparam name="ContextItemType">The type of context item this subscription is for.</typeparam>
    /// <param name="item">The context item that has changed.</param>
    public delegate void SubscribeContextCallback<TContextItemType>(
        TContextItemType item) where TContextItemType : ContextItem;
}
