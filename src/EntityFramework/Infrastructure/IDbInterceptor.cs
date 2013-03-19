// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common.CommandTrees;

    public interface IDbInterceptor
    {
        bool CommandExecuting(DbCommand command);
        bool ConnectionOpening(DbConnection connection);

        DbCommandTree CommandTreeCreated(DbCommandTree commandTree);
    }
}
