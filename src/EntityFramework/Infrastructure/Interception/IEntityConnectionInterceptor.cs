// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Entity.Core.EntityClient;

    internal interface IEntityConnectionInterceptor : IDbInterceptor
    {
        bool ConnectionOpening(EntityConnection connection, DbInterceptionContext interceptionContext);
    }
}
