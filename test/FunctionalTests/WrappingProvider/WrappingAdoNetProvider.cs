// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.WrappingProvider
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.SqlServer;
    using System.Data.SqlClient;
    using System.Reflection;

    public class WrappingAdoNetProvider<TBase> : DbProviderFactory where TBase : DbProviderFactory
    {
        public static readonly WrappingAdoNetProvider<TBase> Instance = new WrappingAdoNetProvider<TBase>();

        private readonly DbProviderFactory _baseProviderFactory;
        private readonly IList<LogItem> _log = new List<LogItem>();

        private WrappingAdoNetProvider()
        {
            _baseProviderFactory =
                (DbProviderFactory)typeof(TBase).GetField("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        }

        public IList<LogItem> Log
        {
            get { return _log; }
        }

        public static void WrapProviders()
        {
            MutableResolver.AddResolver<DbProviderServices>(k => WrappingEfProvider<SqlClientFactory, SqlProviderServices>.Instance);
            MutableResolver.AddResolver<Func<MigrationSqlGenerator>>(WrappingEfProvider<SqlClientFactory, SqlProviderServices>.Instance);
            MutableResolver.AddResolver<DbProviderFactory>(k => WrappingAdoNetProvider<SqlClientFactory>.Instance);
            MutableResolver.AddResolver<IDbProviderFactoryResolver>(k => new WrappingProviderFactoryResolver<SqlClientFactory>());
            MutableResolver.AddResolver<IProviderInvariantName>(
                k => new WrappingProviderInvariantName
                {
                    Name = "System.Data.SqlClient"
                });
        }
        public override DbCommand CreateCommand()
        {
            return new WrappingCommand<TBase>(_baseProviderFactory.CreateCommand());
        }

        public override DbConnection CreateConnection()
        {
            return new WrappingConnection<TBase>(_baseProviderFactory.CreateConnection());
        }
    }
}
