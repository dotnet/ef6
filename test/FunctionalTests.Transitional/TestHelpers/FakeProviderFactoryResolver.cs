// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Utilities;
    using System.Reflection;

    public class FakeProviderFactoryResolver : IDbProviderFactoryResolver
    {
        private static readonly PropertyInfo _factoryProperty = typeof(DbConnection).GetDeclaredProperty("ProviderFactory");

        private readonly IDbProviderFactoryResolver _originalProviderFactoryResolver;
        
        public FakeProviderFactoryResolver(IDbProviderFactoryResolver originalProviderFactoryResolver)
        {
            _originalProviderFactoryResolver = originalProviderFactoryResolver;
        }

        public DbProviderFactory ResolveProviderFactory(DbConnection connection)
        {
            return connection.GetType().FullName.StartsWith("Castle.Proxies.")
                       ? (DbProviderFactory)_factoryProperty.GetValue(connection, null)
                       : _originalProviderFactoryResolver.ResolveProviderFactory(connection);
        }
    }
}
