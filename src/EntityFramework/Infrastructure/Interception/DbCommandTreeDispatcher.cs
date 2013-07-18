// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Utilities;

    internal class DbCommandTreeDispatcher
    {
        private readonly InternalDispatcher<IDbCommandTreeInterceptor> _internalDispatcher
            = new InternalDispatcher<IDbCommandTreeInterceptor>();

        public InternalDispatcher<IDbCommandTreeInterceptor> InternalDispatcher
        {
            get { return _internalDispatcher; }
        }

        public virtual DbCommandTree Created(DbCommandTree commandTree, DbInterceptionContext interceptionContext)
        {
            DebugCheck.NotNull(commandTree);
            DebugCheck.NotNull(interceptionContext);

            var clonedInterceptionContext = new DbCommandTreeInterceptionContext(interceptionContext);

            return _internalDispatcher.Dispatch(
                commandTree, clonedInterceptionContext, i => i.TreeCreated(clonedInterceptionContext));
        }
    }
}
