// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Utilities;

    internal class EntityConnectionDispatcher : DispatcherBase<IEntityConnectionInterceptor>
    {
        public virtual bool Opening(EntityConnection entityConnection, DbInterceptionContext interceptionContext)
        {
            DebugCheck.NotNull(entityConnection);
            DebugCheck.NotNull(interceptionContext);

            return InternalDispatcher.Dispatch(true, (b, i) => i.ConnectionOpening(entityConnection, interceptionContext) && b);
        }
    }
}
