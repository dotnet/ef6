// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;

    internal interface IDbEnumerator<out T> : IDbAsyncEnumerator<T>, IEnumerator<T>
    {
        new T Current { get; }
    }
}
