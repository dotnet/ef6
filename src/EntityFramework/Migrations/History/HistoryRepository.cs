// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.History
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Config;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.Edm;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Transactions;
    using System.Xml.Linq;

    internal class HistoryRepository : RepositoryBase
    {
        private static readonly string _productVersion =
            Assembly.GetExecutingAssembly().GetInformationalVersion();

        private readonly string _contextKey;
        private readonly IEnumerable<string> _schemas;
        private readonly IHistoryContextFactory _historyContextFactory;

        private string _currentSchema;
        private bool? _exists;
        private bool _contextKeyColumnExists;

        public HistoryRepository(
            string connectionString,
            DbProviderFactory providerFactory,
            string contextKey,
            IEnumerable<string> schemas = null,
            IHistoryContextFactory historyContextFactory = null)
            : base(connectionString, providerFactory)
        {
            DebugCheck.NotEmpty(contextKey);

            _contextKey = contextKey;

            _schemas
                = new[] { EdmModelExtensions.DefaultSchema }
                    .Concat(schemas ?? Enumerable.Empty<string>())
                    .Distinct();

            _historyContextFactory
                = historyContextFactory
                  ?? DbConfiguration.GetService<IHistoryContextFactory>();
        }

        public string CurrentSchema
        {
            get { return _currentSchema; }
            set
            {
                DebugCheck.NotEmpty(value);

                _currentSchema = value;
            }
        }

        public virtual XDocument GetLastModel()
        {
            string _;
            return GetLastModel(out _);
        }

        public virtual XDocument GetLastModel(out string migrationId, string contextKey = null)
        {
            migrationId = null;

            if (!Exists(contextKey))
            {
                return null;
            }

            using (var context = CreateContext())
            {
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var lastModel
                        = CreateHistoryQuery(context, contextKey)
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
        }

        public virtual XDocument GetModel(string migrationId)
        {
            DebugCheck.NotEmpty(migrationId);

            if (!Exists())
            {
                return null;
            }

            using (var context = CreateContext())
            {
                var model = CreateHistoryQuery(context)
                    .Where(h => h.MigrationId == migrationId)
                    .Select(h => h.Model)
                    .Single();

                return (model == null) ? null : new ModelCompressor().Decompress(model);
            }
        }

        public virtual IEnumerable<string> GetPendingMigrations(IEnumerable<string> localMigrations)
        {
            DebugCheck.NotNull(localMigrations);

            if (!Exists())
            {
                return localMigrations;
            }

            using (var context = CreateContext())
            {
                List<string> databaseMigrations;
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    databaseMigrations = CreateHistoryQuery(context)
                        .Select(h => h.MigrationId)
                        .ToList();
                }

                var pendingMigrations = localMigrations.Except(databaseMigrations);
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
                    Debug.Assert(pendingMigrations.First() == firstLocalMigration);

                    pendingMigrations = pendingMigrations.Skip(1);
                }

                return pendingMigrations.ToList();
            }
        }

        public virtual IEnumerable<string> GetMigrationsSince(string migrationId)
        {
            DebugCheck.NotEmpty(migrationId);

            var exists = Exists();

            using (var context = CreateContext())
            {
                var query = CreateHistoryQuery(context);

                if (migrationId != DbMigrator.InitialDatabase)
                {
                    if (!exists
                        || !query.Any(h => h.MigrationId == migrationId))
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
            DebugCheck.NotEmpty(migrationName);

            if (!Exists())
            {
                return null;
            }

            using (var context = CreateContext())
            {
                var migrationIds
                    = CreateHistoryQuery(context)
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

        private IQueryable<HistoryRow> CreateHistoryQuery(HistoryContext context, string contextKey = null)
        {
            IQueryable<HistoryRow> q = context.History;

            contextKey = contextKey ?? _contextKey;

            if (_contextKeyColumnExists)
            {
                q = q.Where(h => h.ContextKey == contextKey);
            }

            return q;
        }

        public virtual bool IsShared()
        {
            if (!Exists()
                || !_contextKeyColumnExists)
            {
                return false;
            }

            using (var context = CreateContext())
            {
                return context.History.Any(hr => hr.ContextKey != _contextKey);
            }
        }

        public virtual bool HasMigrations()
        {
            if (!Exists())
            {
                return false;
            }

            if (!_contextKeyColumnExists)
            {
                return true;
            }

            using (var context = CreateContext())
            {
                return context.History.Any(hr => hr.ContextKey == _contextKey);
            }
        }

        public virtual bool Exists(string contextKey = null)
        {
            if (_exists == null)
            {
                _exists = QueryExists(contextKey ?? _contextKey);
            }

            return _exists.Value;
        }

        private bool QueryExists(string contextKey)
        {
            using (var connection = CreateConnection())
            {
                using (var context = CreateContext(connection))
                {
                    if (!context.Database.Exists())
                    {
                        return false;
                    }
                }

                foreach (var schema in _schemas.Reverse())
                {
                    using (var context = CreateContext(connection, schema))
                    {
                        try
                        {
                            using (new TransactionScope(TransactionScopeOption.Suppress))
                            {
                                context.History.Count();
                            }

                            _currentSchema = schema;
                            _contextKeyColumnExists = true;

                            try
                            {
                                using (new TransactionScope(TransactionScopeOption.Suppress))
                                {
                                    if (context.History.Any(hr => hr.ContextKey == contextKey))
                                    {
                                        return true;
                                    }
                                }
                            }
                            catch (EntityException)
                            {
                                _contextKeyColumnExists = false;
                            }
                        }
                        catch (EntityException)
                        {
                            _currentSchema = null;
                        }
                    }
                }
            }

            return !string.IsNullOrWhiteSpace(_currentSchema);
        }

        public virtual void ResetExists()
        {
            _exists = null;
        }

        public virtual IEnumerable<MigrationOperation> GetUpgradeOperations()
        {
            if (!Exists())
            {
                yield break;
            }

            using (var connection = CreateConnection())
            {
                const string tableName = "dbo." + HistoryContext.TableName;

                using (var context = CreateContext(connection))
                {
                    var productVersionExists = false;

                    try
                    {
                        using (new TransactionScope(TransactionScopeOption.Suppress))
                        {
                            context.History
                                   .Select(h => h.ProductVersion)
                                   .FirstOrDefault();
                        }

                        productVersionExists = true;
                    }
                    catch (EntityException)
                    {
                    }

                    if (!productVersionExists)
                    {
                        yield return new DropColumnOperation(tableName, "Hash");

                        yield return new AddColumnOperation(
                            tableName,
                            new ColumnModel(PrimitiveTypeKind.String)
                                {
                                    MaxLength = 32,
                                    Name = "ProductVersion",
                                    IsNullable = false,
                                    DefaultValue = "0.7.0.0"
                                });
                    }

                    if (!_contextKeyColumnExists)
                    {
                        yield return new AddColumnOperation(
                            tableName,
                            new ColumnModel(PrimitiveTypeKind.String)
                                {
                                    MaxLength = 512,
                                    Name = "ContextKey",
                                    IsNullable = false,
                                    DefaultValue = _contextKey
                                });

                        var dropPrimaryKeyOperation
                            = new DropPrimaryKeyOperation
                                  {
                                      Table = tableName
                                  };

                        dropPrimaryKeyOperation.Columns.Add("MigrationId");

                        yield return dropPrimaryKeyOperation;

                        var addPrimaryKeyOperation
                            = new AddPrimaryKeyOperation
                                  {
                                      Table = tableName
                                  };

                        addPrimaryKeyOperation.Columns.Add("MigrationId");
                        addPrimaryKeyOperation.Columns.Add("ContextKey");

                        yield return addPrimaryKeyOperation;
                    }
                }

                using (var context = new LegacyHistoryContext(connection))
                {
                    var createdOnExists = false;

                    try
                    {
                        using (new TransactionScope(TransactionScopeOption.Suppress))
                        {
                            context.History
                                   .Select(h => h.CreatedOn)
                                   .FirstOrDefault();
                        }

                        createdOnExists = true;
                    }
                    catch (EntityException)
                    {
                    }

                    if (createdOnExists)
                    {
                        yield return new DropColumnOperation(tableName, "CreatedOn");
                    }
                }
            }
        }

        public virtual MigrationOperation CreateInsertOperation(string migrationId, XDocument model)
        {
            DebugCheck.NotEmpty(migrationId);
            DebugCheck.NotNull(model);

            using (var context = CreateContext())
            {
                context.History.Add(
                    new HistoryRow
                        {
                            MigrationId = migrationId,
                            ContextKey = _contextKey,
                            Model = new ModelCompressor().Compress(model),
                            ProductVersion = _productVersion
                        });

                using (var commandTracer = new CommandTracer())
                {
                    context.SaveChanges();

                    return new HistoryOperation(commandTracer.Commands);
                }
            }
        }

        public virtual MigrationOperation CreateDeleteOperation(string migrationId)
        {
            DebugCheck.NotEmpty(migrationId);

            using (var context = CreateContext())
            {
                var historyRow
                    = new HistoryRow
                          {
                              MigrationId = migrationId,
                              ContextKey = _contextKey
                          };

                context.History.Attach(historyRow);
                context.History.Remove(historyRow);

                using (var commandTracer = new CommandTracer())
                {
                    context.SaveChanges();

                    return new HistoryOperation(commandTracer.Commands);
                }
            }
        }

        public virtual void BootstrapUsingEFProviderDdl(XDocument model)
        {
            DebugCheck.NotNull(model);

            using (var context = CreateContext())
            {
                context.Database.ExecuteSqlCommand(
                    ((IObjectContextAdapter)context).ObjectContext.CreateDatabaseScript());

                context.History.Add(
                    new HistoryRow
                        {
                            MigrationId = MigrationAssembly.CreateMigrationId(Strings.InitialCreate),
                            ContextKey = _contextKey,
                            Model = new ModelCompressor().Compress(model),
                            ProductVersion = Assembly.GetExecutingAssembly().GetInformationalVersion()
                        });

                context.SaveChanges();
            }
        }

        private HistoryContext CreateContext(DbConnection connection = null, string schema = null)
        {
            return _historyContextFactory.Create(connection ?? CreateConnection(), connection == null, schema ?? CurrentSchema);
        }
    }
}
