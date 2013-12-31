// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Utilities;

    internal class DbConfigurationDispatcher
    {
        private readonly InternalDispatcher<IDbConfigurationInterceptor> _internalDispatcher
            = new InternalDispatcher<IDbConfigurationInterceptor>();

        public InternalDispatcher<IDbConfigurationInterceptor> InternalDispatcher
        {
            get { return _internalDispatcher; }
        }

        public virtual void Loaded(DbConfigurationLoadedEventArgs loadedEventArgs, DbInterceptionContext interceptionContext)
        {
            DebugCheck.NotNull(loadedEventArgs);
            DebugCheck.NotNull(interceptionContext);

            var clonedInterceptionContext = new DbConfigurationInterceptionContext(interceptionContext);

            _internalDispatcher.Dispatch(i => i.Loaded(loadedEventArgs, clonedInterceptionContext));
        }
    }
}
