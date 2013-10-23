// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Core.Context
{
    using System;

    /// <summary>
    ///     A delegate that is called back when an object should publish an instance of a
    ///     service.
    /// </summary>
    /// <param name="serviceType">The type of service to be published.</param>
    /// <returns>An instance of serviceType.</returns>
    public delegate object PublishServiceCallback(Type serviceType);

    /// <summary>
    ///     A generic delegate that is called back when an object should publish an
    ///     instance of a service.
    /// </summary>
    /// <typeparam name="ServiceType">The type of service to be published.</typeparam>
    /// <returns>An instance of ServiceType.</returns>
    public delegate TServiceType PublishServiceCallback<TServiceType>();
}
