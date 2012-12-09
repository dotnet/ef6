// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityClient
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
#if !NET40
#endif

    internal class MockHelper
    {
        public static EntityConnection CreateEntityConnection(MetadataWorkspace metadataWorkspace = null)
        {
            var providerFactory = new Mock<DbProviderFactory>(MockBehavior.Strict).Object;
            var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
            dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);
            var dbConnection = dbConnectionMock.Object;

            var entityConnectionMock = new Mock<EntityConnection>(MockBehavior.Loose, metadataWorkspace, dbConnection, true, true)
                                           {
                                               CallBase = true
                                           };
            entityConnectionMock.SetupGet(m => m.StoreProviderFactory).Returns(providerFactory);
            entityConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);
            entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("foo");
            var entityConnection = entityConnectionMock.Object;

            return entityConnection;
        }

        public static EntityCommandDefinition CreateEntityCommandDefinition()
        {
            var storeDataReaderMock = new Mock<DbDataReader>();
#if !NET40
            storeDataReaderMock.Setup(m => m.NextResultAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(false));
#endif
            var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(MockBehavior.Strict, null, null, null);
            entityCommandDefinitionMock.SetupGet(m => m.Parameters).Returns(Enumerable.Empty<EntityParameter>());
            entityCommandDefinitionMock.Setup(m => m.Execute(It.IsAny<EntityCommand>(), It.IsAny<CommandBehavior>())).
                Returns(storeDataReaderMock.Object);
#if !NET40
            entityCommandDefinitionMock.Setup(
                m => m.ExecuteAsync(It.IsAny<EntityCommand>(), It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>())).
                Returns((EntityCommand ec, CommandBehavior cb, CancellationToken ct) => Task.FromResult(storeDataReaderMock.Object));
#endif
            return entityCommandDefinitionMock.Object;
        }
    }
}
