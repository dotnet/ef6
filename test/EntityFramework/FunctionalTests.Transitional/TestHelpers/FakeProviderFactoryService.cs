// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Reflection;

    public class FakeProviderFactoryService : IDbProviderFactoryService
    {
        private static readonly PropertyInfo _factoryProperty 
            = typeof(DbConnection).GetProperty("ProviderFactory", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        private readonly IDbProviderFactoryService _originalProviderFactoryService;
        
        public FakeProviderFactoryService(IDbProviderFactoryService originalProviderFactoryService)
        {
            _originalProviderFactoryService = originalProviderFactoryService;
        }

        public DbProviderFactory GetProviderFactory(DbConnection connection)
        {
            return connection.GetType().FullName.StartsWith("Castle.Proxies.")
                       ? (DbProviderFactory)_factoryProperty.GetValue(connection, null)
                       : _originalProviderFactoryService.GetProviderFactory(connection);
        }
    }
}
