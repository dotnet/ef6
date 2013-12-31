// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Interception;

    public class RegisterMutableResolver : IDbConfigurationInterceptor
    {
        public void Loaded(
            DbConfigurationLoadedEventArgs loadedEventArgs,
            DbConfigurationInterceptionContext interceptionContext)
        {
            loadedEventArgs.AddDependencyResolver(MutableResolver.Instance, overrideConfigFile: true);
        }
    }
}
