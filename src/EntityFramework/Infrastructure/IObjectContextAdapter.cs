// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core.Objects;

    /// <summary>
    ///     Interface implemented by objects that can provide an <see cref = "ObjectContext" /> instance.
    ///     The <see cref = "DbContext" /> class implements this interface to provide access to the underlying
    ///     ObjectContext.
    /// </summary>
    public interface IObjectContextAdapter
    {
        /// <summary>
        ///     Gets the object context.
        /// </summary>
        /// <value>The object context.</value>
        ObjectContext ObjectContext { get; }
    }
}
