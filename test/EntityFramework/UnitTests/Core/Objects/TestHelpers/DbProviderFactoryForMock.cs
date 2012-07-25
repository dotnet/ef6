// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Objects
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient.Internal;
    using Moq;

    public class DbProviderFactoryForMock : DbProviderFactory, IServiceProvider
    {
        private Mock<IEntityAdapter> entityAdapterMock;
        public DbProviderFactoryForMock()
        {
            entityAdapterMock = new Mock<IEntityAdapter>();
        }

        public virtual object GetService(Type serviceType)
        {
            if (serviceType == typeof(IEntityAdapter))
            {
                return entityAdapterMock.Object;
            }

            return null;
        }
    }
}
