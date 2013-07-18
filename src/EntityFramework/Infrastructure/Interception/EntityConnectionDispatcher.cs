// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Utilities;

    internal class EntityConnectionDispatcher
    {
        private readonly InternalDispatcher<IEntityConnectionInterceptor> _internalDispatcher
            = new InternalDispatcher<IEntityConnectionInterceptor>();

        public InternalDispatcher<IEntityConnectionInterceptor> InternalDispatcher
        {
            get { return _internalDispatcher; }
        }

        public virtual bool Opening(EntityConnection entityConnection, DbInterceptionContext interceptionContext)
        {
            DebugCheck.NotNull(entityConnection);
            DebugCheck.NotNull(interceptionContext);

            return _internalDispatcher.Dispatch(true, (b, i) => i.ConnectionOpening(entityConnection, interceptionContext) && b);
        }
    }
}
