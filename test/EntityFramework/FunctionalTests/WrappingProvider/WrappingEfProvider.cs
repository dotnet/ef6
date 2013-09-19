// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.WrappingProvider
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Migrations.Sql;
    using System.Reflection;

    public class WrappingEfProvider<TAdoNetBase, TEfBase> : DbProviderServices
        where TAdoNetBase : DbProviderFactory
        where TEfBase : DbProviderServices
    {
        public static readonly WrappingEfProvider<TAdoNetBase, TEfBase> Instance = new WrappingEfProvider<TAdoNetBase, TEfBase>();

        private readonly DbProviderServices _baseServices;

        private WrappingEfProvider()
        {
            _baseServices =
                (DbProviderServices)typeof(TEfBase).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
        }

        protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree)
        {
            return new WrappingCommandDefinition<TAdoNetBase>(_baseServices.CreateCommandDefinition(providerManifest, commandTree));
        }

        protected override string GetDbProviderManifestToken(DbConnection connection)
        {
            return _baseServices.GetProviderManifestToken(((WrappingConnection<TAdoNetBase>)connection).BaseConnection);
        }

        protected override DbProviderManifest GetDbProviderManifest(string manifestToken)
        {
            return _baseServices.GetProviderManifest(manifestToken);
        }

        protected override void DbCreateDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            WrappingAdoNetProvider<TAdoNetBase>.Instance.Log.Add(
                new LogItem("DbCreateDatabase", connection, new object[] { commandTimeout, storeItemCollection }));

            _baseServices.CreateDatabase(((WrappingConnection<TAdoNetBase>)connection).BaseConnection, commandTimeout, storeItemCollection);
        }

        protected override bool DbDatabaseExists(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            WrappingAdoNetProvider<TAdoNetBase>.Instance.Log.Add(
                new LogItem("DbDatabaseExists", connection, new object[] { commandTimeout, storeItemCollection }));

            return _baseServices.DatabaseExists(
                ((WrappingConnection<TAdoNetBase>)connection).BaseConnection, commandTimeout, storeItemCollection);
        }

        protected override string DbCreateDatabaseScript(string providerManifestToken, StoreItemCollection storeItemCollection)
        {
            return _baseServices.CreateDatabaseScript(providerManifestToken, storeItemCollection);
        }

        protected override void DbDeleteDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            WrappingAdoNetProvider<TAdoNetBase>.Instance.Log.Add(
                new LogItem("DbDeleteDatabase", connection, new object[] { commandTimeout, storeItemCollection }));

            _baseServices.DeleteDatabase(((WrappingConnection<TAdoNetBase>)connection).BaseConnection, commandTimeout, storeItemCollection);
        }

        public override object GetService(Type type, object key)
        {
            var service = _baseServices.GetService(type, key);

            var asSqlGenerator = service as Func<MigrationSqlGenerator>;
            return asSqlGenerator != null ? (Func<MigrationSqlGenerator>)(() => new WrappingSqlGenerator<TAdoNetBase>(asSqlGenerator)) : service;
        }
    }
}
