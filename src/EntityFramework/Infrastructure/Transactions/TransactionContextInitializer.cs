// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Transactions;

    internal class TransactionContextInitializer<TContext> : IDatabaseInitializer<TContext>
        where TContext : TransactionContext
    {
        public void InitializeDatabase(TContext context)
        {
            var entityConnection = (EntityConnection)((IObjectContextAdapter)context).ObjectContext.Connection;
            // We don't need to initialize the TransactionContext if there's no transaction yet
            if (entityConnection.State == ConnectionState.Open
                && entityConnection.CurrentTransaction != null)
            {
                try
                {
                    using (new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        context.Transactions
                            .AsNoTracking()
                            .WithExecutionStrategy(new DefaultExecutionStrategy())
                            .Count();
                    }
                }
                catch (EntityException)
                {
                    var currentInfo = DbContextInfo.CurrentInfo;
                    DbContextInfo.CurrentInfo = null;
                    try
                    {
                        var sqlStatements = GenerateMigrationStatements(context);
                        var migrator = new DbMigrator(
                            context.InternalContext.MigrationsConfiguration, context, DatabaseExistenceState.Exists,
                            calledByCreateDatabase: true);
                        using (new TransactionScope(TransactionScopeOption.Suppress))
                        {
                            migrator.ExecuteStatements(sqlStatements, entityConnection.CurrentTransaction.StoreTransaction);
                        }
                    }
                    finally
                    {
                        DbContextInfo.CurrentInfo = currentInfo;
                    }
                }
            }
        }
        
        internal static IEnumerable<MigrationStatement> GenerateMigrationStatements(TransactionContext context)
        {
            if (DbConfiguration.DependencyResolver.GetService<Func<MigrationSqlGenerator>>(context.InternalContext.ProviderName) != null)
            {
                var migrationSqlGenerator =
                    context.InternalContext.MigrationsConfiguration.GetSqlGenerator(context.InternalContext.ProviderName);

                var connection = context.Database.Connection;
                var emptyModel = new DbModelBuilder().Build(connection).GetModel();
                var createTableOperation = (CreateTableOperation)
                    new EdmModelDiffer().Diff(emptyModel, context.GetModel()).Single();

                var providerManifestToken
                    = context.InternalContext.ModelProviderInfo != null
                        ? context.InternalContext.ModelProviderInfo.ProviderManifestToken
                        : DbConfiguration
                            .DependencyResolver
                            .GetService<IManifestTokenResolver>()
                            .ResolveManifestToken(connection);

                return migrationSqlGenerator.Generate(new[] { createTableOperation }, providerManifestToken);
            }
            else
            {
                return new[]
                {
                    new MigrationStatement
                    {
                        Sql = ((IObjectContextAdapter)context).ObjectContext.CreateDatabaseScript(),
                        SuppressTransaction = true
                    }
                };
            }
        }
    }
}
