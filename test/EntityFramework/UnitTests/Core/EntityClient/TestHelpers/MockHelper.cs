// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityClient
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Query.ResultAssembly;
    using System.Linq;
    using Moq;

    internal class MockHelper
    {
        public static EntityConnection InitializeEntityConnection(MetadataWorkspace metadataWorkspace = null)
        {
            var providerFactory = new Mock<DbProviderFactory>(MockBehavior.Strict).Object;
            var dbConnection = new Mock<DbConnection>(MockBehavior.Strict).Object;

            var entityConnectionMock = new Mock<EntityConnection>(MockBehavior.Loose, metadataWorkspace, dbConnection, true)
                                           {
                                               CallBase = true
                                           };
            entityConnectionMock.SetupGet(m => m.StoreProviderFactory).Returns(providerFactory);
            entityConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);
            entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("foo");
            var entityConnection = entityConnectionMock.Object;

            return entityConnection;
        }

        public static EntityCommandDefinition InitializeEntityCommandDefinition()
        {
            var storeDataReader = new Mock<DbDataReader>().Object;
            var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(MockBehavior.Strict, null, null);
            entityCommandDefinitionMock.SetupGet(m => m.Parameters).Returns(Enumerable.Empty<EntityParameter>());
            entityCommandDefinitionMock.Setup(m => m.Execute(It.IsAny<EntityCommand>(), It.IsAny<CommandBehavior>())).
                Returns(storeDataReader);

            return entityCommandDefinitionMock.Object;
        }
    }
}
