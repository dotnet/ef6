// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    internal class DbCommandTreeDispatcher : DispatcherBase<IDbCommandTreeInterceptor>
    {
        public virtual DbCommandTree Created(DbCommandTree commandTree, DbInterceptionContext interceptionContext)
        {
            DebugCheck.NotNull(commandTree);
            DebugCheck.NotNull(interceptionContext);

            var clonedInterceptionContext = new DbCommandTreeInterceptionContext(interceptionContext);

            return InternalDispatcher.Dispatch(
                commandTree, clonedInterceptionContext, i => i.TreeCreated(commandTree, clonedInterceptionContext));
        }
    }
}
