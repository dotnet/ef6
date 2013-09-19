// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Common;

    internal interface ICancelableDbCommandInterceptor : IDbInterceptor
    {
        bool CommandExecuting(DbCommand command, DbInterceptionContext interceptionContext);
    }
}
