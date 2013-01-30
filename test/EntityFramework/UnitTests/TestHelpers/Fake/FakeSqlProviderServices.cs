// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Internal.UnitTests
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.SqlServer;
    using Moq;
    using Moq.Protected;

    /// <summary>
    ///     Used with the FakeSqlConnection class to fake provider info so that Code First can create SSDL
    ///     without having to hit a real store.
    /// </summary>
    public class FakeSqlProviderServices : DbProviderServices
    {
        public static readonly FakeSqlProviderServices Instance = new FakeSqlProviderServices();

        internal EntityCommandDefinition EntityCommandDefinition;
        
        protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree)
        {
            var entityCommandDefinition = EntityCommandDefinition;
            if (entityCommandDefinition == null)
            {
                entityCommandDefinition = new Mock<EntityCommandDefinition>(MockBehavior.Loose, null, null, null).Object;
            }

            return entityCommandDefinition;
        }

        protected override string GetDbProviderManifestToken(DbConnection connection)
        {
            return ((FakeSqlConnection)connection).ManifestToken;
        }

        protected override DbProviderManifest GetDbProviderManifest(string manifestToken)
        {
            return new SqlProviderManifest(manifestToken);
        }
    }
}
