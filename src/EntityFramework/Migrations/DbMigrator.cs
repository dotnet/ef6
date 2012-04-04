namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.Edm;
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Migrations.Utilities;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using IEnumerableExtensions = System.Data.Entity.Migrations.Extensions.IEnumerableExtensions;

    /// <summary>
    ///     DbMigrator is used to apply existing migrations to a database. 
    ///     DbMigrator can be used to upgrade and downgrade to any given migration.
    ///     To scaffold migrations based on changes to your model use <see cref = "Design.MigrationScaffolder" />
    /// </summary>
    public class DbMigrator : MigratorBase
    {
        /// <summary>
        ///     Migration Id representing the state of the database before any migrations are applied.
        /// </summary>
        public const string InitialDatabase = "0";

        private static readonly MethodInfo _setInitializerMethod
            = typeof(Database).GetMethod("SetInitializer");

        private readonly XDocument _emptyModel;
        private readonly DbMigrationsConfiguration _configuration;
        private readonly XDocument _currentModel;
        private readonly DbProviderFactory _providerFactory;
        private readonly HistoryRepository _historyRepository;
        private readonly MigrationAssembly _migrationAssembly;
        private readonly DbContextInfo _usersContextInfo;
        private readonly bool _calledByCreateDatabase;
        private readonly ModelDiffer _modelDiffer;
        private readonly string _providerManifestToken;
        private readonly bool _hasSeedLogic;
        private readonly string _targetDatabase;

        private MigrationSqlGenerator _sqlGenerator;

        private bool _emptyMigrationNeeded;

        /// <summary>
        ///     Initializes a new instance of the DbMigrator class.
        /// </summary>
        /// <param name = "configuration">Configuration to be used for the migration process.</param>
        public DbMigrator(DbMigrationsConfiguration configuration)
            : this(configuration, null)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(configuration.ContextType != null);
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal DbMigrator(DbMigrationsConfiguration configuration, DbContext usersContext)
            : base(null)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(configuration.ContextType != null);

            _configuration = configuration;
            _calledByCreateDatabase = usersContext != null;

            // If DbContext CreateDatabase is using Migrations then the user has not opted out of initializers
            // and if we disable the initializer here then future calls to Initialize the database (for this or
            // a different connection) will fail. So only disable the initializer if Migrations are being used
            // explicitly.
            if (usersContext == null)
            {
                DisableInitializer(_configuration.ContextType);
            }

            if (_calledByCreateDatabase)
            {
                _usersContextInfo = new DbContextInfo(usersContext);
            }
            else
            {
                _usersContextInfo
                    = configuration.TargetDatabase == null
                          ? new DbContextInfo(configuration.ContextType)
                          : new DbContextInfo(configuration.ContextType, configuration.TargetDatabase);

                if (!_usersContextInfo.IsConstructible)
                {
                    throw Error.ContextNotConstructible(configuration.ContextType);
                }
            }

            _modelDiffer = _configuration.ModelDiffer;

            var context = usersContext ?? _usersContextInfo.CreateInstance();
            try
            {
                _migrationAssembly = new MigrationAssembly(
                    _configuration.MigrationsAssembly, _configuration.MigrationsNamespace);
                _currentModel = context.GetModel();

                var connection = context.Database.Connection;
                _providerFactory = DbProviderServices.GetProviderFactory(connection);
                _historyRepository = new HistoryRepository(_usersContextInfo.ConnectionString, _providerFactory);
                _providerManifestToken = context.InternalContext.ModelProviderInfo != null
                                             ? context.InternalContext.ModelProviderInfo.ProviderManifestToken
                                         // TODO: Not calling using extension method syntax here because of conflicts due to duplicate extension methods
                                         // Should fix this post EF5.
                                             : DbProviderServices.GetProviderServices(connection).
                                                   GetProviderManifestTokenChecked(connection);

                _targetDatabase
                    = Strings.LoggingTargetDatabaseFormat(
                        connection.DataSource,
                        connection.Database,
                        _usersContextInfo.ConnectionProviderName,
                        _usersContextInfo.ConnectionStringOrigin == DbConnectionStringOrigin.DbContextInfo
                            ? Strings.LoggingExplicit
                            : _usersContextInfo.ConnectionStringOrigin.ToString());
            }
            finally
            {
                if (usersContext == null)
                {
                    context.Dispose();
                }
            }

            _emptyModel =
                new DbModelBuilder().Build(
                    new DbProviderInfo(
                        _usersContextInfo.ConnectionProviderName,
                        _providerManifestToken)).GetModel();

            var seedMethod
                = _configuration.GetType()
                    .GetMethod(
                        "Seed",
                        BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance);

            if (seedMethod != null)
            {
                _hasSeedLogic = seedMethod.GetMethodBody().GetILAsByteArray().Length > 2;
            }
        }

        /// <summary>
        ///     Gets the configuration that is being used for the migration process.
        /// </summary>
        public override DbMigrationsConfiguration Configuration
        {
            get { return _configuration; }
        }

        internal virtual void DisableInitializer(Type contextType)
        {
            Contract.Requires(contextType != null);

            _setInitializerMethod
                .MakeGenericMethod(contextType)
                .Invoke(null, new object[] { null });
        }

        internal override string TargetDatabase
        {
            get { return _targetDatabase; }
        }

        private MigrationSqlGenerator SqlGenerator
        {
            get
            {
                return _sqlGenerator
                       ?? (_sqlGenerator = _configuration.GetSqlGenerator(_usersContextInfo.ConnectionProviderName));
            }
        }

        /// <summary>
        ///     Gets all migrations that are defined in the configured migrations assembly.
        /// </summary>
        public override IEnumerable<string> GetLocalMigrations()
        {
            return _migrationAssembly.MigrationIds;
        }

        /// <summary>
        ///     Gets all migrations that have been applied to the target database.
        /// </summary>
        public override IEnumerable<string> GetDatabaseMigrations()
        {
            return _historyRepository.GetMigrationsSince(InitialDatabase);
        }

        internal ScaffoldedMigration ScaffoldInitialCreate(string @namespace)
        {
            string migrationId;
            var databaseModel = _historyRepository.GetLastModel(out migrationId);

            if ((databaseModel == null)
                || !migrationId.MigrationName().Equals(Strings.InitialCreate))
            {
                return null;
            }

            var migrationOperations
                = _modelDiffer.Diff(
                    _emptyModel,
                    databaseModel,
                    _usersContextInfo.ConnectionString)
                    .ToList();

            var generatedMigration
                = _configuration.CodeGenerator.Generate(
                    migrationId,
                    migrationOperations,
                    null,
                    Convert.ToBase64String(new ModelCompressor().Compress(_currentModel)),
                    @namespace,
                    Strings.InitialCreate);

            generatedMigration.MigrationId = migrationId;
            generatedMigration.Directory = _configuration.MigrationsDirectory;

            return generatedMigration;
        }

        internal ScaffoldedMigration Scaffold(string migrationName, string @namespace, bool ignoreChanges)
        {
            XDocument sourceModel = null;
            CheckLegacyCompatibility(() => sourceModel = _currentModel);

            string sourceMigrationId = null;
            sourceModel = sourceModel ?? (_historyRepository.GetLastModel(out sourceMigrationId) ?? _emptyModel);
            var modelCompressor = new ModelCompressor();

            var migrationOperations
                = ignoreChanges
                      ? Enumerable.Empty<MigrationOperation>()
                      : _modelDiffer.Diff(
                          sourceModel,
                          _currentModel,
                          _usersContextInfo.ConnectionString)
                            .ToList();

            string migrationId;

            if (migrationName.IsValidMigrationId())
            {
                migrationId = migrationName;
                migrationName = migrationName.MigrationName();
            }
            else
            {
                migrationName = _migrationAssembly.UniquifyName(migrationName);
                migrationId = MigrationAssembly.CreateMigrationId(migrationName);
            }

            var generatedMigration
                = _configuration.CodeGenerator.Generate(
                    migrationId,
                    migrationOperations,
                    (sourceModel == _emptyModel)
                    || (sourceModel == _currentModel)
                    || !sourceMigrationId.IsAutomaticMigration()
                        ? null
                        : Convert.ToBase64String(modelCompressor.Compress(sourceModel)),
                    Convert.ToBase64String(modelCompressor.Compress(_currentModel)),
                    @namespace,
                    migrationName);

            generatedMigration.MigrationId = migrationId;
            generatedMigration.Directory = _configuration.MigrationsDirectory;

            return generatedMigration;
        }

        /// <summary>
        ///     Gets all migrations that are defined in the assembly but haven't been applied to the target database.
        /// </summary>
        public override IEnumerable<string> GetPendingMigrations()
        {
            return _historyRepository.GetPendingMigrations(_migrationAssembly.MigrationIds);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void CheckLegacyCompatibility(Action onCompatible)
        {
            Contract.Requires(onCompatible != null);

            if (!_calledByCreateDatabase
                && !_historyRepository.Exists())
            {
                using (var context = _usersContextInfo.CreateInstance())
                {
                    bool compatibleWithModel;

                    try
                    {
                        compatibleWithModel
                            = context.Database.CompatibleWithModel(true);
                    }
                    catch
                    {
                        // no EdmMetadata table
                        return;
                    }

                    if (!compatibleWithModel)
                    {
                        throw Error.MetadataOutOfDate();
                    }

                    onCompatible();
                }
            }
        }

        /// <summary>
        ///     Updates the target database to a given migration.
        /// </summary>
        /// <param name = "targetMigration">The migration to upgrade/downgrade to.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        public override void Update(string targetMigration)
        {
            base.EnsureDatabaseExists();

            var upgradeOperations = _historyRepository.GetUpgradeOperations();

            if (upgradeOperations.Any())
            {
                base.UpgradeHistory(upgradeOperations);
            }

            var pendingMigrations = GetPendingMigrations();

            if (!pendingMigrations.Any())
            {
                CheckLegacyCompatibility(
                    () => ExecuteOperations(
                        MigrationAssembly.CreateBootstrapMigrationId(),
                        _currentModel,
                        new List<MigrationOperation>(),
                        false));
            }

            var targetMigrationId = targetMigration;

            if (!string.IsNullOrWhiteSpace(targetMigrationId))
            {
                if (!targetMigrationId.IsValidMigrationId())
                {
                    if (targetMigrationId == Strings.AutomaticMigration)
                    {
                        throw Error.AutoNotValidTarget(Strings.AutomaticMigration);
                    }

                    targetMigrationId = GetMigrationId(targetMigration);
                }

                if (pendingMigrations.Any(m => m.EqualsIgnoreCase(targetMigrationId)))
                {
                    pendingMigrations
                        = pendingMigrations
                            .Where(
                                m =>
                                string.CompareOrdinal(m.ToLowerInvariant(), targetMigrationId.ToLowerInvariant()) <= 0);
                }
                else
                {
                    pendingMigrations
                        = _historyRepository.GetMigrationsSince(targetMigrationId);

                    if (pendingMigrations.Any())
                    {
                        base.Downgrade(pendingMigrations.Concat(new[] { targetMigrationId }));

                        return;
                    }
                }
            }

            base.Upgrade(pendingMigrations, targetMigrationId, null);
        }

        internal override void UpgradeHistory(IEnumerable<MigrationOperation> upgradeOperations)
        {
            var sqlStatements = SqlGenerator.Generate(upgradeOperations, _providerManifestToken);

            base.ExecuteStatements(sqlStatements);
        }

        internal override string GetMigrationId(string migration)
        {
            if (migration.IsValidMigrationId())
            {
                return migration;
            }

            var migrationId
                = GetPendingMigrations()
                      .SingleOrDefault(m => m.MigrationName().EqualsIgnoreCase(migration))
                  ?? _historyRepository.GetMigrationId(migration);

            if (migrationId == null)
            {
                throw Error.MigrationNotFound(migration);
            }

            return migrationId;
        }

        internal override void Upgrade(
            IEnumerable<string> pendingMigrations, string targetMigrationId, string lastMigrationId)
        {
            DbMigration lastMigration = null;

            if (lastMigrationId != null)
            {
                lastMigration = _migrationAssembly.GetMigration(lastMigrationId);
            }

            foreach (var pendingMigration in pendingMigrations)
            {
                var migration = _migrationAssembly.GetMigration(pendingMigration);

                base.ApplyMigration(migration, lastMigration);

                lastMigration = migration;

                _emptyMigrationNeeded = false;

                if (pendingMigration.EqualsIgnoreCase(targetMigrationId))
                {
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(targetMigrationId)
                && ((_emptyMigrationNeeded && _configuration.AutomaticMigrationsEnabled)
                    || IsModelOutOfDate(_currentModel, lastMigration)))
            {
                if (!_configuration.AutomaticMigrationsEnabled)
                {
                    throw Error.AutomaticDisabledException();
                }

                base.AutoMigrate(
                    MigrationAssembly.CreateMigrationId(
                        _calledByCreateDatabase
                            ? Strings.InitialCreate
                            : Strings.AutomaticMigration),
                    GetLastModel(lastMigration),
                    _currentModel,
                    false);
            }

            if (_hasSeedLogic
                && !IsModelOutOfDate(_currentModel, lastMigration))
            {
                base.SeedDatabase();
            }
        }

        internal override void SeedDatabase()
        {
            // Context may not be constructable when Migrations is being called by DbContext CreateDatabase
            // and the config cannot have Seed data anyway, so avoid creating the context to seed.
            if (!_calledByCreateDatabase)
            {
                using (var context = _usersContextInfo.CreateInstance())
                {
                    _configuration.OnSeed(context);

                    context.SaveChanges();
                }
            }
        }

        private bool IsModelOutOfDate(XDocument model, DbMigration lastMigration)
        {
            Contract.Requires(model != null);

            return _modelDiffer.Diff(GetLastModel(lastMigration), model, _usersContextInfo.ConnectionString).Any();
        }

        private XDocument GetLastModel(DbMigration lastMigration, string currentMigrationId = null)
        {
            if (lastMigration != null)
            {
                var migrationMetadata = (IMigrationMetadata)lastMigration;

                return new ModelCompressor().Decompress(Convert.FromBase64String(migrationMetadata.Target));
            }

            string migrationId;
            var lastModel = _historyRepository.GetLastModel(out migrationId);

            if (lastModel != null
                && (currentMigrationId == null || migrationId.ComesBefore(currentMigrationId)))
            {
                return lastModel;
            }

            return _emptyModel;
        }

        internal override void Downgrade(IEnumerable<string> pendingMigrations)
        {
            for (var i = 0; i < pendingMigrations.Count() - 1; i++)
            {
                var migrationId = pendingMigrations.ElementAt(i);
                var migration = _migrationAssembly.GetMigration(migrationId);
                var nextMigrationId = pendingMigrations.ElementAt(i + 1);
                var targetModel = (nextMigrationId != InitialDatabase)
                                      ? _historyRepository.GetModel(nextMigrationId)
                                      : _emptyModel;

                Contract.Assert(targetModel != null);

                if (migration == null)
                {
                    var sourceModel = _historyRepository.GetModel(migrationId);

                    base.AutoMigrate(migrationId, sourceModel, targetModel, downgrading: true);
                }
                else
                {
                    base.RevertMigration(migrationId, migration, targetModel);
                }
            }
        }

        internal override void RevertMigration(string migrationId, DbMigration migration, XDocument targetModel)
        {
            migration.Down();

            ExecuteOperations(migrationId, targetModel, migration.Operations.ToList(), downgrading: true);
        }

        internal override void ApplyMigration(DbMigration migration, DbMigration lastMigration)
        {
            var migrationMetadata = (IMigrationMetadata)migration;
            var compressor = new ModelCompressor();

            if (migrationMetadata.Source != null)
            {
                var sourceModel
                    = compressor.Decompress(Convert.FromBase64String(migrationMetadata.Source));

                if (IsModelOutOfDate(sourceModel, lastMigration))
                {
                    base.AutoMigrate(
                        migrationMetadata.Id + "_" + Strings.AutomaticMigration,
                        GetLastModel(lastMigration, migrationMetadata.Id),
                        sourceModel,
                        downgrading: false);
                }
            }

            migration.Up();

            var targetModel = compressor.Decompress(Convert.FromBase64String(migrationMetadata.Target));

            ExecuteOperations(migrationMetadata.Id, targetModel, migration.Operations.ToList(), false);
        }

        internal override void AutoMigrate(
            string migrationId, XDocument sourceModel, XDocument targetModel, bool downgrading)
        {
            var operations
                = _modelDiffer.Diff(
                    sourceModel,
                    targetModel,
                    _usersContextInfo.ConnectionString)
                    .ToList();

            if (!_configuration.AutomaticMigrationDataLossAllowed
                && operations.Any(o => o.IsDestructiveChange))
            {
                throw Error.AutomaticDataLoss();
            }

            ExecuteOperations(migrationId, targetModel, operations, downgrading, auto: true);
        }

        private void ExecuteOperations(
            string migrationId, XDocument targetModel, IEnumerable<MigrationOperation> operations, bool downgrading, bool auto = false)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(migrationId));
            Contract.Requires(targetModel != null);
            Contract.Requires(operations != null);

            FillInForeignKeyOperations(operations, targetModel);

            var newTableForeignKeys
                = (from ct in operations.OfType<CreateTableOperation>()
                   from afk in operations.OfType<AddForeignKeyOperation>()
                   where ct.Name.EqualsIgnoreCase(afk.DependentTable)
                   select afk)
                    .ToList();

            var orderedOperations
                = operations
                    .Except(newTableForeignKeys)
                    .Concat(newTableForeignKeys)
                    .ToList();

            var isFirstMigration = IsFirstMigration(migrationId, downgrading);

            if (downgrading)
            {
                orderedOperations.Add(_historyRepository.CreateDeleteOperation(migrationId));

                if (isFirstMigration)
                {
                    orderedOperations.Add(_historyRepository.CreateDropTableOperation(_modelDiffer));
                }
            }
            else
            {
                if (isFirstMigration)
                {
                    orderedOperations.Add(_historyRepository.CreateCreateTableOperation(_modelDiffer));
                }

                orderedOperations.Add(_historyRepository.CreateInsertOperation(migrationId, targetModel));
            }

            var migrationStatements = SqlGenerator.Generate(orderedOperations, _providerManifestToken);

            if (auto)
            {
                // Filter duplicates when auto-migrating. Duplicates can be caused by
                // duplicates in the model such as shared FKs.

                migrationStatements 
                    = migrationStatements.Distinct((m1, m2) => string.Equals(m1.Sql, m2.Sql, StringComparison.Ordinal));
            }

            base.ExecuteStatements(migrationStatements);
        }

        internal override void ExecuteStatements(IEnumerable<MigrationStatement> migrationStatements)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    foreach (var migrationStatement in migrationStatements)
                    {
                        base.ExecuteSql(transaction, migrationStatement);
                    }

                    transaction.Commit();
                }
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        internal override void ExecuteSql(DbTransaction transaction, MigrationStatement migrationStatement)
        {
            if (string.IsNullOrWhiteSpace(migrationStatement.Sql))
            {
                return;
            }

            Console.WriteLine(migrationStatement.Sql);

            if (!migrationStatement.SuppressTransaction)
            {
                using (var command = transaction.Connection.CreateCommand())
                {
                    command.CommandText = migrationStatement.Sql;
                    command.Transaction = transaction;
                    command.ExecuteNonQuery();
                }
            }
            else
            {
                using (var connection = CreateConnection())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = migrationStatement.Sql;

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private static void FillInForeignKeyOperations(
            IEnumerable<MigrationOperation> operations, XDocument targetModel)
        {
            Contract.Requires(operations != null);
            Contract.Requires(targetModel != null);

            foreach (var foreignKeyOperation
                in operations.OfType<AddForeignKeyOperation>()
                    .Where(fk => fk.PrincipalTable != null && !fk.PrincipalColumns.Any()))
            {
                var entitySetName
                    = (from es in targetModel.Descendants(EdmXNames.Ssdl.EntitySetNames)
                       where ModelDiffer.GetQualifiedTableName(es.TableAttribute(), es.SchemaAttribute())
                           .EqualsIgnoreCase(foreignKeyOperation.PrincipalTable)
                       select es.NameAttribute()).SingleOrDefault();

                if (entitySetName != null)
                {
                    var entityTypeElement
                        = targetModel.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                            .Single(et => et.NameAttribute().EqualsIgnoreCase(entitySetName));

                    IEnumerableExtensions.Each(
                        entityTypeElement
                            .Descendants(EdmXNames.Ssdl.PropertyRefNames),
                        pr => foreignKeyOperation.PrincipalColumns.Add(pr.NameAttribute()));
                }
                else
                {
                    // try and find the table in the current list of ops
                    var table
                        = operations
                            .OfType<CreateTableOperation>()
                            .SingleOrDefault(ct => ct.Name.EqualsIgnoreCase(foreignKeyOperation.PrincipalTable));

                    if ((table != null)
                        && (table.PrimaryKey != null))
                    {
                        IEnumerableExtensions.Each(
                            table.PrimaryKey.Columns, c => foreignKeyOperation.PrincipalColumns.Add(c));
                    }
                    else
                    {
                        throw Error.PartialFkOperation(
                            foreignKeyOperation.DependentTable, foreignKeyOperation.DependentColumns.Join());
                    }
                }
            }
        }

        internal override void EnsureDatabaseExists()
        {
            using (var connection = CreateConnection())
            {
                if (!Database.Exists(connection))
                {
                    new DatabaseCreator().Create(connection);
                    _emptyMigrationNeeded = true;
                }
                else
                {
                    _emptyMigrationNeeded = false;
                }
            }
        }

        internal override DbMigration GetMigration(string migrationId)
        {
            return _migrationAssembly.GetMigration(migrationId);
        }

        internal override bool IsFirstMigrationIncludingAutomatics(string migrationId)
        {
            var firstMigrationId = _historyRepository.GetMigrationsSince(InitialDatabase).LastOrDefault();

            // If this is the first applied migration, return true; otherwise false
            return firstMigrationId == null
                   || string.Equals(migrationId, firstMigrationId, StringComparison.Ordinal);
        }

        private bool IsFirstMigration(string migrationId, bool downgrading)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(migrationId));
            Contract.Assert(migrationId.IsValidMigrationId());

            if (downgrading)
            {
                var firstMigrationId = _historyRepository.GetMigrationsSince(InitialDatabase).LastOrDefault();
                Contract.Assert(firstMigrationId != null);

                return string.Equals(migrationId, firstMigrationId, StringComparison.Ordinal);
            }

            var firstExplicitMigrationId = _migrationAssembly.MigrationIds.FirstOrDefault();

            // If this comes before or is the first explicit migration...
            if (firstExplicitMigrationId == null
                || string.Equals(migrationId, firstExplicitMigrationId, StringComparison.Ordinal)
                || migrationId.ComesBefore(firstExplicitMigrationId))
            {
                return base.IsFirstMigrationIncludingAutomatics(migrationId);
            }

            return false;
        }

        private DbConnection CreateConnection()
        {
            var connection = _providerFactory.CreateConnection();
            connection.ConnectionString = _usersContextInfo.ConnectionString;

            return connection;
        }
    }
}
