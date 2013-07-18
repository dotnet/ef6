// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    internal interface IDbMutableInterceptionContext<TResult> : IDbMutableInterceptionContext
    {
        new InterceptionContextMutableData<TResult> MutableData { get; }
    }
}
