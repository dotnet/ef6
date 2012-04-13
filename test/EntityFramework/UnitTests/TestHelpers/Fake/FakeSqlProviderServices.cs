namespace System.Data.Entity.ModelConfiguration.Internal.UnitTests
{
    using System;
    using System.Data.Entity.Core.Common;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Objects;

    /// <summary>
    /// Used with the FakeSqlConnection class to fake provider info so that Code First can create SSDL
    /// without having to hit a real store.
    /// </summary>
    public class FakeSqlProviderServices : DbProviderServices
    {
        protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree)
        {
            throw new NotImplementedException();
        }

        protected override string GetDbProviderManifestToken(DbConnection connection)
        {
            return ((FakeSqlConnection)connection).ManifestToken;
        }

        protected override DbProviderManifest GetDbProviderManifest(string manifestToken)
        {
            var manifestType = typeof(ObjectContext).Assembly.GetType("System.Data.Entity.Core.SqlClient.SqlProviderManifest");
            return (DbProviderManifest)Activator.CreateInstance(manifestType, new object[] {manifestToken});
        }
    }
}