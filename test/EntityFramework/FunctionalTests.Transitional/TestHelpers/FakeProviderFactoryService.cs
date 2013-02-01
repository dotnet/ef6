// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;

    public class FakeProviderFactoryService : IDbProviderFactoryService
    {
        private IDbProviderFactoryService _originalProviderFactoryService;
        public FakeProviderFactoryService(IDbProviderFactoryService originalProviderFactoryService)
        {
            _originalProviderFactoryService = originalProviderFactoryService;
        }

        public DbProviderFactory GetProviderFactory(DbConnection connection)
        {
            var type = connection.GetType();
            if (type.FullName.StartsWith("Castle.Proxies."))
            {
                return GenericProviderFactory<DbProviderFactory>.Instance;
            }
            else
            {
                return _originalProviderFactoryService.GetProviderFactory(connection);
            }
        }
    }
}
