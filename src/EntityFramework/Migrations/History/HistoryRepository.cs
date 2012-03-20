namespace System.Data.Entity.Migrations.History
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.Edm;
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Metadata.Edm;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Transactions;
    using System.Xml.Linq;

    internal class HistoryRepository : RepositoryBase
    {
        public HistoryRepository(string connectionString, DbProviderFactory providerFactory)
            : base(connectionString, providerFactory)
        {
        }

        public virtual XDocument GetLastModel()
        {
            string _;
            return GetLastModel(out _);
        }

        public virtual XDocument GetLastModel(out string migrationId)
        {
            migrationId = null;

            using (var context = new HistoryContext(CreateConnection()))
            {
                if (!Exists(context))
                {
                    return null;
                }

                var lastModel
                    = context.History
                        .OrderByDescending(h => h.MigrationId)
                        .Select(s => new { s.MigrationId, s.Model })
                        .FirstOrDefault();

                if (lastModel == null)
                {
                    return null;
                }

                migrationId = lastModel.MigrationId;

                return new ModelCompressor().Decompress(lastModel.Model);
            }
        }

        public virtual XDocument GetModel(string migrationId)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(migrationId));

            using (var context = new HistoryContext(CreateConnection()))
            {
                if (!Exists(context))
                {
                    return null;
                }

                var model = context.History
                    .Where(h => h.MigrationId == migrationId)
                    .Select(h => h.Model)
                    .Single();

                if (model == null)
                {
                    return null;
                }

                return new ModelCompressor().Decompress(model);
            }
        }

        public virtual IEnumerable<string> GetPendingMigrations(IEnumerable<string> localMigrations)
        {
            Contract.Requires(localMigrations != null);

            using (var context = new HistoryContext(CreateConnection()))
            {
                if (!Exists(context))
                {
                    return localMigrations;
                }

                return localMigrations
                    .Except(context.History.Select(s => s.MigrationId))
                    .ToList();
            }
        }

        public virtual IEnumerable<string> GetMigrationsSince(string migrationId)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(migrationId));

            using (var context = new HistoryContext(CreateConnection()))
            {
                var query = context.History.AsQueryable();
                var exists = Exists(context);

                if (migrationId != DbMigrator.InitialDatabase)
                {
                    if (!exists || !context.History.Any(h => h.MigrationId == migrationId))
                    {
                        throw Error.MigrationNotFound(migrationId);
                    }

                    query = query.Where(h => string.Compare(h.MigrationId, migrationId, StringComparison.Ordinal) > 0);
                }
                else if (!exists)
                {
                    return Enumerable.Empty<string>();
                }

                return query
                    .OrderByDescending(h => h.MigrationId)
                    .Select(h => h.MigrationId)
                    .ToList();
            }
        }

        public virtual string GetMigrationId(string migrationName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(migrationName));

            using (var context = new HistoryContext(CreateConnection()))
            {
                if (!Exists(context))
                {
                    return null;
                }

                var migrationIds
                    = context.History
                        .Select(h => h.MigrationId)
                        .Where(m => m.Substring(16) == migrationName)
                        .ToList();

                if (!migrationIds.Any())
                {
                    return null;
                }

                if (migrationIds.Count() == 1)
                {
                    return migrationIds.Single();
                }

                throw Error.AmbiguousMigrationName(migrationName);
            }
        }

        public virtual bool Exists()
        {
            using (var context = new HistoryContext(CreateConnection()))
            {
                return Exists(context);
            }
        }

        public virtual IEnumerable<MigrationOperation> GetUpgradeOperations()
        {
            if (ColumnExists(() => new HistoryContext(CreateConnection()), h => h.ProductVersion) == false)
            {
                yield return new DropColumnOperation(HistoryContext.TableName, "Hash");

                yield return new AddColumnOperation(
                    HistoryContext.TableName,
                    new ColumnModel(PrimitiveTypeKind.String)
                        {
                            MaxLength = 32,
                            Name = "ProductVersion",
                            IsNullable = false,
                            DefaultValue = "0.7.0.0"
                        });
            }

#pragma warning disable 612,618
            if (ColumnExists(() => new LegacyHistoryContext(CreateConnection()), h => h.CreatedOn) == true)
#pragma warning restore 612,618
            {
                yield return new DropColumnOperation(HistoryContext.TableName, "CreatedOn");
            }
        }

        private bool? ColumnExists<TContext, TResult>(Func<HistoryContextBase<TContext>> createContext, Expression<Func<HistoryRow, TResult>> selector) where TContext : DbContext
        {
            using (var context = createContext())
            {
                if (Exists(context))
                {
                    try
                    {
                        context.History
                            .Select(selector)
                            .FirstOrDefault();
                    }
                    catch (EntityException)
                    {
                        return false;
                    }

                    return true;
                }
            }

            return null;
        }

        public virtual MigrationOperation CreateCreateTableOperation(ModelDiffer modelDiffer)
        {
            return CreateCreateTableOperation(c => new HistoryContext(c, false), modelDiffer);
        }

        public virtual MigrationOperation CreateCreateTableOperation<TContext>(Func<DbConnection, HistoryContextBase<TContext>> createContext, ModelDiffer modelDiffer) where TContext : DbContext
        {
            Contract.Requires(modelDiffer != null);

            using (var connection = CreateConnection())
            {
                using (var context = createContext(connection))
                {
                    using (var emptyContext = new EmptyContext(connection))
                    {
                        var operations
                            = modelDiffer.Diff(emptyContext.GetModel(), context.GetModel(), null);

                        var createTableOperation = operations.OfType<CreateTableOperation>().Single();

                        createTableOperation.AnonymousArguments.Add("IsMSShipped", true);

                        return createTableOperation;
                    }
                }
            }
        }

        public virtual MigrationOperation CreateDropTableOperation(ModelDiffer modelDiffer)
        {
            Contract.Requires(modelDiffer != null);

            using (var connection = CreateConnection())
            {
                using (var context = new HistoryContext(connection, false))
                {
                    using (var emptyContext = new EmptyContext(connection))
                    {
                        var operations
                            = modelDiffer.Diff(context.GetModel(), emptyContext.GetModel(), null);

                        var dropTableOperation = operations.OfType<DropTableOperation>().Single();

                        return dropTableOperation;
                    }
                }
            }
        }

        public virtual MigrationOperation CreateInsertOperation(string migrationId, XDocument model)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(migrationId));
            Contract.Requires(model != null);

            // TODO: Can we somehow use DbInsertCommandTree?
            return new InsertHistoryOperation(
                HistoryContext.TableName,
                migrationId,
                new ModelCompressor().Compress(model));
        }

        public virtual MigrationOperation CreateDeleteOperation(string migrationId)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(migrationId));

            // TODO: Can we somehow use DbInsertCommandTree?
            return new DeleteHistoryOperation(
                HistoryContext.TableName,
                migrationId);
        }

        public virtual void BootstrapUsingEFProviderDdl(XDocument model)
        {
            Contract.Requires(model != null);

            using (var context = new HistoryContext(CreateConnection()))
            {
                context.Database.ExecuteSqlCommand(
                    ((IObjectContextAdapter)context).ObjectContext.CreateDatabaseScript());

                context.History.Add(
                    new HistoryRow
                    {
                        MigrationId = MigrationAssembly.CreateMigrationId(Strings.InitialCreate),
                        Model = new ModelCompressor().Compress(model),
                        ProductVersion = Assembly.GetExecutingAssembly().GetInformationalVersion()
                    });

                context.SaveChanges();
            }
        }

        private static bool Exists<TContext>(HistoryContextBase<TContext> context) where TContext : DbContext
        {
            Contract.Requires(context != null);

            bool databaseExists;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                databaseExists = context.Database.Exists();
            }

            if (databaseExists)
            {
                try
                {
                    context.History.Count();

                    return true;
                }
                catch (EntityException)
                {
                }
            }

            return false;
        }
    }
}