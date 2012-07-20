// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Migrations.History
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.Edm;
    using System.Data.Entity.Migrations.Extensions;
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
        private bool? _exists;

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

            if (!Exists)
            {
                return null;
            }

            using (var context = CreateContext())
            {
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

            if (!Exists)
            {
                return null;
            }

            using (var context = CreateContext())
            {
                var model = context.History
                    .Where(h => h.MigrationId == migrationId)
                    .Select(h => h.Model)
                    .Single();

                return (model == null)
                           ? null
                           : new ModelCompressor().Decompress(model);
            }
        }

        public virtual IEnumerable<string> GetPendingMigrations(IEnumerable<string> localMigrations)
        {
            Contract.Requires(localMigrations != null);

            if (!Exists)
            {
                return localMigrations;
            }

            using (var context = CreateContext())
            {
                var databaseMigrations = context.History
                    .Select(s => s.MigrationId)
                    .ToList();

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

                if (migrationId != DbMigrator.InitialDatabase)
                {
                    if (!Exists
                        || !context.History.Any(h => h.MigrationId == migrationId))
                    {
                        throw Error.MigrationNotFound(migrationId);
                    }

                    query = query.Where(h => string.Compare(h.MigrationId, migrationId, StringComparison.Ordinal) > 0);
                }
                else if (!Exists)
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

            if (!Exists)
            {
                return null;
            }

            using (var context = CreateContext())
            {
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

        public virtual bool Exists
        {
            get
            {
                if (_exists == null)
                {
                    using (var context = CreateContext())
                    {
                        _exists = QueryExists(context);
                    }
                }

                return _exists.Value;
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

        private bool? ColumnExists<TContext, TResult>(
            Func<HistoryContextBase<TContext>> createContext, Expression<Func<HistoryRow, TResult>> selector)
            where TContext : DbContext
        {
            using (var context = createContext())
            {
                if (Exists)
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

        public virtual MigrationOperation CreateInsertOperation(string migrationId, XDocument model)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(migrationId));
            Contract.Requires(model != null);

            // TODO: Can we somehow use DbInsertCommandTree?
            return new InsertHistoryOperation(HistoryContext.TableName, migrationId, new ModelCompressor().Compress(model));
        }

        public virtual MigrationOperation CreateDeleteOperation(string migrationId)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(migrationId));

            // TODO: Can we somehow use DbInsertCommandTree?
            return new DeleteHistoryOperation(HistoryContext.TableName, migrationId);
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

        public virtual void ResetExists()
        {
            _exists = null;
        }

        private static bool QueryExists<TContext>(HistoryContextBase<TContext> context) where TContext : DbContext
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
            return new HistoryContext(connection ?? CreateConnection(), connection == null, null);
        }

        public virtual void AppendHistoryModel(XDocument model, DbProviderInfo providerInfo)
        {
            Contract.Requires(model != null);
            Contract.Requires(providerInfo != null);

            var csdlNamespace = model.Descendants(EdmXNames.Csdl.SchemaNames).Single().Name.Namespace;
            var mslNamespace = model.Descendants(EdmXNames.Msl.MappingNames).Single().Name.Namespace;
            var ssdlNamespace = model.Descendants(EdmXNames.Ssdl.SchemaNames).Single().Name.Namespace;

            using (var context = CreateContext())
            {
                // prevent having to lookup the provider info.
                context.InternalContext.ModelProviderInfo = providerInfo;

                var historyModel = context.GetModel();

                var entityType = historyModel.Descendants(EdmXNames.Csdl.EntityTypeNames).Single();
                var entitySetMapping = historyModel.Descendants(EdmXNames.Msl.EntitySetMappingNames).Single();
                var storeEntityType = historyModel.Descendants(EdmXNames.Ssdl.EntityTypeNames).Single();
                var storeEntitySet = historyModel.Descendants(EdmXNames.Ssdl.EntitySetNames).Single();

                new[] { entityType, entitySetMapping, storeEntityType, storeEntitySet }
                    .Each(x => x.SetAttributeValue(EdmXNames.IsSystem, true));

                // normalize namespaces
                entityType.DescendantsAndSelf().Each(e => e.Name = csdlNamespace + e.Name.LocalName);
                entitySetMapping.DescendantsAndSelf().Each(e => e.Name = mslNamespace + e.Name.LocalName);
                storeEntityType.DescendantsAndSelf().Each(e => e.Name = ssdlNamespace + e.Name.LocalName);
                storeEntitySet.DescendantsAndSelf().Each(e => e.Name = ssdlNamespace + e.Name.LocalName);

                model.Descendants(EdmXNames.Csdl.SchemaNames).Single().Add(entityType);
                model.Descendants(EdmXNames.Msl.EntityContainerMappingNames).Single().Add(entitySetMapping);
                model.Descendants(EdmXNames.Ssdl.SchemaNames).Single().Add(storeEntityType);
                model.Descendants(EdmXNames.Ssdl.EntityContainerNames).Single().Add(storeEntitySet);
            }
        }
    }
}
