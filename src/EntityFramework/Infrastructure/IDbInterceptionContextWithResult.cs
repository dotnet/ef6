// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    internal interface IDbInterceptionContextWithResult<TResult>
    {
        TResult Result { get; set; }
        bool IsResultSet { get; }
    }
}
