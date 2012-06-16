namespace System.Data.Entity.Migrations.History
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.Edm;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Transactions;
    using System.Xml.Linq;

    internal class HistoryRepository : RepositoryBase
    {
        private readonly string _defaultSchema;

        public HistoryRepository(string connectionString, DbProviderFactory providerFactory, string defaultSchema = null)
            : base(connectionString, providerFactory)
        {
            _defaultSchema = defaultSchema;
        }

        public virtual XDocument GetLastModel()
        {
            string _;
            return GetLastModel(out _);
        }

        public virtual XDocument GetLastModel(out string migrationId)
        {
            migrationId = null;

            using (var context = CreateContext())
            {
                if (!Exists(context))
                {
                    return null;
                }

                var lastModel
                    = context.History
                        .OrderByDescending(h => h.MigrationId)
                        .Select(
                            s => new
                                     {
                                         s.MigrationId,
                                         s.Model
                                     })
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

            using (var context = CreateContext())
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

            using (var context = CreateContext())
            {
                if (!Exists(context))
                {
                    return localMigrations;
                }

                var databaseMigrations = context.History
                    .Select(s => s.MigrationId)
                    .ToList();
                var pendingMigrations = localMigrations
                    .Except(databaseMigrations);

                var firstDatabaseMigration = databaseMigrations.FirstOrDefault();
                var firstLocalMigration = localMigrations.FirstOrDefault();

                // If the first database migration and the first local migration don't match,
                // but both are named InitialCreate then treat it as already applied. This can
                // happen when trying to migrate a database that was created using initializers
                if (firstDatabaseMigration != firstLocalMigration
                    && firstDatabaseMigration != null
                    && firstDatabaseMigration.MigrationName() == Strings.InitialCreate
                    && firstLocalMigration != null
                    && firstLocalMigration.MigrationName() == Strings.InitialCreate)
                {
                    Contract.Assert(pendingMigrations.First() == firstLocalMigration);

                    pendingMigrations = pendingMigrations.Skip(1);
                }

                return pendingMigrations
                    .ToList();
            }
        }

        public virtual IEnumerable<string> GetMigrationsSince(string migrationId)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(migrationId));

            using (var context = CreateContext())
            {
                var query = context.History.AsQueryable();
                var exists = Exists(context);

                if (migrationId != DbMigrator.InitialDatabase)
                {
                    if (!exists
                        || !context.History.Any(h => h.MigrationId == migrationId))
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

            using (var context = CreateContext())
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
            using (var context = CreateContext())
            {
                return Exists(context);
            }
        }

        public virtual IEnumerable<MigrationOperation> GetUpgradeOperations()
        {
            if (ColumnExists(() => CreateContext(), h => h.ProductVersion) == false)
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

        private static bool? ColumnExists<TContext, TResult>(
            Func<HistoryContextBase<TContext>> createContext, Expression<Func<HistoryRow, TResult>> selector)
            where TContext : DbContext
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

        public virtual MigrationOperation CreateCreateTableOperation(EdmModelDiffer modelDiffer)
        {
            return CreateCreateTableOperation(CreateContext, modelDiffer);
        }

        public virtual MigrationOperation CreateCreateTableOperation<TContext>(
            Func<DbConnection, HistoryContextBase<TContext>> createContext, EdmModelDiffer modelDiffer)
            where TContext : DbContext
        {
            Contract.Requires(modelDiffer != null);

            using (var connection = CreateConnection())
            {
                using (var context = createContext(connection))
                {
                    using (var emptyContext = new EmptyContext(connection))
                    {
                        var operations
                            = modelDiffer.Diff(emptyContext.GetModel(), context.GetModel());

                        var createTableOperation = operations.OfType<CreateTableOperation>().Single();

                        createTableOperation.AnonymousArguments.Add("IsMSShipped", true);

                        return createTableOperation;
                    }
                }
            }
        }

        public virtual MigrationOperation CreateDropTableOperation(EdmModelDiffer modelDiffer)
        {
            Contract.Requires(modelDiffer != null);

            using (var connection = CreateConnection())
            {
                using (var context = CreateContext(connection))
                {
                    using (var emptyContext = new EmptyContext(connection))
                    {
                        var operations
                            = modelDiffer.Diff(context.GetModel(), emptyContext.GetModel());

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
            return new InsertHistoryOperation(TableName, migrationId, new ModelCompressor().Compress(model));
        }

        public virtual MigrationOperation CreateDeleteOperation(string migrationId)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(migrationId));

            // TODO: Can we somehow use DbInsertCommandTree?
            return new DeleteHistoryOperation(TableName, migrationId);
        }

        private string TableName
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_defaultSchema)
                           ? _defaultSchema + "." + HistoryContext.TableName
                           : HistoryContext.TableName;
            }
        }

        public virtual void BootstrapUsingEFProviderDdl(XDocument model)
        {
            Contract.Requires(model != null);

            using (var context = CreateContext())
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

        private HistoryContext CreateContext(DbConnection connection = null)
        {
            return new HistoryContext(connection ?? CreateConnection(), connection == null, _defaultSchema);
        }
    }
}
