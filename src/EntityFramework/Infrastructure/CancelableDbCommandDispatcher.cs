// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Utilities;

    internal class CancelableDbCommandDispatcher : DispatcherBase<ICancelableDbCommandInterceptor>
    {
        public virtual bool Executing(DbCommand command, DbInterceptionContext interceptionContext)
        {
            DebugCheck.NotNull(command);
            DebugCheck.NotNull(interceptionContext);

            return InternalDispatcher.Dispatch(true, (b, i) => i.CommandExecuting(command, interceptionContext) && b);
        }
    }
}
