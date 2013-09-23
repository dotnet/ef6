// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Design
{
    // <summary>
    // A contract handlers can use to accept a single result.
    // </summary>
    // <seealso cref="HandlerBase" />
    internal interface IResultHandler
    {
        // <summary>
        // Sets the result.
        // </summary>
        // <param name="value">The result.</param>
        void SetResult(object value);
    }
}
