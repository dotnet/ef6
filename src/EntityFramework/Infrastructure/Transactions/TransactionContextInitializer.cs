// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Transactions;

    internal class TransactionContextInitializer<TContext> : IDatabaseInitializer<TContext>
        where TContext : TransactionContext
    {
        public void InitializeDatabase(TContext context)
        {
            // We shouldn't create the database here as it might interfere with the parent context initialization.
            if (context.Database.Exists())
            {
                try
                {
                    using (new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        context.Transactions.AsNoTracking().Count();
                    }
                }
                catch (EntityException)
                {
                    var connection = context.Database.Connection;
                    var emptyModel = new DbModelBuilder().Build(connection).GetModel();
                    var createTableOperation = (CreateTableOperation)
                        new EdmModelDiffer().Diff(emptyModel, context.GetModel()).Single();

                    var migrationsConfiguration = context.InternalContext.MigrationsConfiguration;
                    var migrationSqlGenerator = migrationsConfiguration.GetSqlGenerator(context.InternalContext.ProviderName);

                    var providerManifestToken
                        = context.InternalContext.ModelProviderInfo != null
                            ? context.InternalContext.ModelProviderInfo.ProviderManifestToken
                            : DbConfiguration
                                .DependencyResolver
                                .GetService<IManifestTokenResolver>()
                                .ResolveManifestToken(connection);

                    var sqlStatements = migrationSqlGenerator.Generate(new[] { createTableOperation }, providerManifestToken);
                    var migrator = new DbMigrator(migrationsConfiguration, context, DatabaseExistenceState.Exists);
                    using (new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        migrator.ExecuteStatements(sqlStatements);
                    }
                }
            }
        }
    }
}
