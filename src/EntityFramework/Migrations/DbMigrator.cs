// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.Edm;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Resources;
    using System.Xml.Linq;
    using DatabaseCreator = System.Data.Entity.Migrations.Utilities.DatabaseCreator;

    /// <summary>
    /// DbMigrator is used to apply existing migrations to a database.
    /// DbMigrator can be used to upgrade and downgrade to any given migration.
    /// To scaffold migrations based on changes to your model use <see cref="Design.MigrationScaffolder" />
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class DbMigrator : MigratorBase
    {
        /// <summary>
        /// Migration Id representing the state of the database before any migrations are applied.
        /// </summary>
        public const string InitialDatabase = "0";

        private const string DefaultSchemaResourceKey = "DefaultSchema";

        private readonly Lazy<XDocument> _emptyModel;
        private readonly DbMigrationsConfiguration _configuration;
        private readonly XDocument _currentModel;
        private readonly DbProviderFactory _providerFactory;
        private readonly HistoryRepository _historyRepository;
        private readonly MigrationAssembly _migrationAssembly;
        private readonly DbContextInfo _usersContextInfo;
        private readonly EdmModelDiffer _modelDiffer;
        private readonly Lazy<ModificationCommandTreeGenerator> _modificationCommandTreeGenerator;
        private readonly DbContext _usersContext;
        private readonly Func<DbConnection, string, HistoryContext> _historyContextFactory;
        private readonly DbConnection _connection;

        private readonly bool _calledByCreateDatabase;
        private readonly DatabaseExistenceState _existenceState;

        private readonly string _providerManifestToken;
        private readonly string _targetDatabase;
        private readonly string _legacyContextKey;
        private readonly string _defaultSchema;

        private MigrationSqlGenerator _sqlGenerator;
        private bool _emptyMigrationNeeded;
        private bool _committedStatements;

        // <summary>
        // For testing.
        // </summary>
        internal DbMigrator(
            DbContext usersContext = null,
            DbProviderFactory providerFactory = null,
            MigrationAssembly migrationAssembly = null)
            : base(null)
        {
            _usersContext = usersContext;
            _providerFactory = providerFactory;
            _migrationAssembly = migrationAssembly;
            _usersContextInfo = new DbContextInfo(typeof(DbContext));
            _configuration = new DbMigrationsConfiguration();
            _calledByCreateDatabase = true;
        }

        /// <summary>
        /// Initializes a new instance of the DbMigrator class.
        /// </summary>
        /// <param name="configuration"> Configuration to be used for the migration process. </param>
        public DbMigrator(DbMigrationsConfiguration configuration)
            : this(configuration, null, DatabaseExistenceState.Unknown, calledByCreateDatabase: false)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(configuration.ContextType, "configuration.ContextType");
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal DbMigrator(DbMigrationsConfiguration configuration, DbContext usersContext)
            : this(configuration, usersContext, DatabaseExistenceState.Unknown, calledByCreateDatabase: false)
        {
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal DbMigrator(DbMigrationsConfiguration configuration, DbContext usersContext, DatabaseExistenceState existenceState, bool calledByCreateDatabase)
            : base(null)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(configuration.ContextType, "configuration.ContextType");

            _configuration = configuration;
            _calledByCreateDatabase = calledByCreateDatabase;
            _existenceState = existenceState;

            if (usersContext != null)
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
            _usersContext = context;

            try
            {
                _migrationAssembly
                    = new MigrationAssembly(
                        _configuration.MigrationsAssembly,
                        _configuration.MigrationsNamespace);

                _currentModel = context.GetModel();

                _connection = context.Database.Connection;

                _providerFactory = DbProviderServices.GetProviderFactory(_connection);

                _defaultSchema
                    = context.InternalContext.DefaultSchema
                      ?? EdmModelExtensions.DefaultSchema;

                _historyContextFactory
                    = _configuration
                        .GetHistoryContextFactory(_usersContextInfo.ConnectionProviderName);

                _historyRepository
                    = new HistoryRepository(
                        context.InternalContext,
                        _usersContextInfo.ConnectionString,
                        _providerFactory,
                        _configuration.ContextKey,
                        _configuration.CommandTimeout,
                        _historyContextFactory,
                        schemas: new[] { _defaultSchema }.Concat(GetHistorySchemas()),
                        contextForInterception: _usersContext,
                        initialExistence: _existenceState,
                        permissionDeniedDetector: e => SqlGenerator.IsPermissionDeniedError(e));

                _providerManifestToken
                    = context.InternalContext.ModelProviderInfo != null
                        ? context.InternalContext.ModelProviderInfo.ProviderManifestToken
                        : DbConfiguration
                            .DependencyResolver
                            .GetService<IManifestTokenResolver>()
                            .ResolveManifestToken(_connection);

                var modelBuilder
                    = context.InternalContext.CodeFirstModel.CachedModelBuilder;

                _modificationCommandTreeGenerator
                    = new Lazy<ModificationCommandTreeGenerator>(
                        () =>
                            new ModificationCommandTreeGenerator(
                                modelBuilder.BuildDynamicUpdateModel(
                                    new DbProviderInfo(
                                        _usersContextInfo.ConnectionProviderName,
                                        _providerManifestToken)),
                                CreateConnection()));

                var interceptionContext = new DbInterceptionContext();
                interceptionContext = interceptionContext.WithDbContext(_usersContext);

                _targetDatabase
                    = Strings.LoggingTargetDatabaseFormat(
                        DbInterception.Dispatch.Connection.GetDataSource(_connection, interceptionContext),
                        DbInterception.Dispatch.Connection.GetDatabase(_connection, interceptionContext),
                        _usersContextInfo.ConnectionProviderName,
                        _usersContextInfo.ConnectionStringOrigin == DbConnectionStringOrigin.DbContextInfo
                            ? Strings.LoggingExplicit
                            : _usersContextInfo.ConnectionStringOrigin.ToString());

                _legacyContextKey = context.InternalContext.DefaultContextKey;
                _emptyModel = GetEmptyModel();
            }
            finally
            {
                if (usersContext == null)
                {
                    _usersContext = null;
                    context.Dispose();
                }
            }
        }

        private Lazy<XDocument> GetEmptyModel()
        {
            return new Lazy<XDocument>(
                () => new DbModelBuilder()
                    .Build(new DbProviderInfo(_usersContextInfo.ConnectionProviderName, _providerManifestToken))
                    .GetModel());
        }

        private XDocument GetHistoryModel(string defaultSchema)
        {
            DebugCheck.NotEmpty(defaultSchema);

            DbConnection connection = null;
            try
            {
                connection = CreateConnection();

                using (var historyContext = _historyContextFactory(connection, defaultSchema))
                {
                    return historyContext.GetModel();
                }
            }
            finally
            {
                if (connection != null)
                {
                    DbInterception.Dispatch.Connection.Dispose(connection, new DbInterceptionContext());
                }
            }
        }

        private IEnumerable<string> GetHistorySchemas()
        {
            return
                from migrationId in _migrationAssembly.MigrationIds
                let migration = _migrationAssembly.GetMigration(migrationId)
                select GetDefaultSchema(migration);
        }

        /// <summary>
        /// Gets the configuration that is being used for the migration process.
        /// </summary>
        public override DbMigrationsConfiguration Configuration
        {
            get { return _configuration; }
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
        /// Gets all migrations that are defined in the configured migrations assembly.
        /// </summary>
        /// <returns>The list of migrations.</returns>
        public override IEnumerable<string> GetLocalMigrations()
        {
            return _migrationAssembly.MigrationIds;
        }

        /// <summary>
        /// Gets all migrations that have been applied to the target database.
        /// </summary>
        /// <returns>The list of migrations.</returns>
        public override IEnumerable<string> GetDatabaseMigrations()
        {
            return _historyRepository.GetMigrationsSince(InitialDatabase);
        }

        /// <summary>
        /// Gets all migrations that are defined in the assembly but haven't been applied to the target database.
        /// </summary>
        /// <returns>The list of migrations.</returns>
        public override IEnumerable<string> GetPendingMigrations()
        {
            return _historyRepository.GetPendingMigrations(_migrationAssembly.MigrationIds);
        }

        internal ScaffoldedMigration ScaffoldInitialCreate(string @namespace)
        {
            string migrationId, _;

            var databaseModel 
                = _historyRepository.GetLastModel(out migrationId, out _, contextKey: _legacyContextKey);

            if ((databaseModel == null)
                || !migrationId.MigrationName().Equals(Strings.InitialCreate))
            {
                return null;
            }

            var migrationOperations
                = _modelDiffer
                    .Diff(_emptyModel.Value, databaseModel, _modificationCommandTreeGenerator, SqlGenerator)
                    .ToList();

            var scaffoldedMigration
                = _configuration.CodeGenerator.Generate(
                    migrationId,
                    migrationOperations,
                    null,
                    Convert.ToBase64String(new ModelCompressor().Compress(_currentModel)),
                    @namespace,
                    Strings.InitialCreate);

            scaffoldedMigration.MigrationId = migrationId;
            scaffoldedMigration.Directory = _configuration.MigrationsDirectory;
            scaffoldedMigration.Resources.Add(DefaultSchemaResourceKey, _defaultSchema);

            return scaffoldedMigration;
        }

        internal ScaffoldedMigration Scaffold(string migrationName, string @namespace, bool ignoreChanges)
        {
            string migrationId = null;
            var rescaffolding = false;

            var pendingMigrations = GetPendingMigrations().ToList();

            if (pendingMigrations.Any())
            {
                var lastMigration = pendingMigrations.Last();

                if (!lastMigration.EqualsIgnoreCase(migrationName)
                    && !lastMigration.MigrationName().EqualsIgnoreCase(migrationName))
                {
                    throw Error.MigrationsPendingException(pendingMigrations.Join());
                }

                rescaffolding = true;
                migrationId = lastMigration;
                migrationName = lastMigration.MigrationName();
            }

            XDocument sourceModel = null;
            CheckLegacyCompatibility(() => sourceModel = _currentModel);

            string sourceMigrationId = null, sourceModelVersion = null;

            sourceModel 
                = sourceModel
                    ?? (_historyRepository.GetLastModel(out sourceMigrationId, out sourceModelVersion) 
                        ?? _emptyModel.Value);
            
            var migrationOperations
                = ignoreChanges
                    ? Enumerable.Empty<MigrationOperation>()
                      : _modelDiffer.Diff(
                            sourceModel, 
                            _currentModel, 
                            _modificationCommandTreeGenerator, 
                            SqlGenerator,
                            sourceModelVersion: sourceModelVersion)
                        .ToList();

            if (!rescaffolding)
            {
                migrationName = _migrationAssembly.UniquifyName(migrationName);
                migrationId = MigrationAssembly.CreateMigrationId(migrationName);
            }

            var modelCompressor = new ModelCompressor();

            var scaffoldedMigration
                = _configuration.CodeGenerator.Generate(
                    migrationId,
                    migrationOperations,
                    (sourceModel == _emptyModel.Value)
                    || (sourceModel == _currentModel)
                    || !sourceMigrationId.IsAutomaticMigration()
                        ? null
                        : Convert.ToBase64String(modelCompressor.Compress(sourceModel)),
                    Convert.ToBase64String(modelCompressor.Compress(_currentModel)),
                    @namespace,
                    migrationName);

            scaffoldedMigration.MigrationId = migrationId;
            scaffoldedMigration.Directory = _configuration.MigrationsDirectory;
            scaffoldedMigration.IsRescaffold = rescaffolding;
            scaffoldedMigration.Resources.Add(DefaultSchemaResourceKey, _defaultSchema);

            return scaffoldedMigration;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void CheckLegacyCompatibility(Action onCompatible)
        {
            DebugCheck.NotNull(onCompatible);

            if (!_calledByCreateDatabase
                && !_historyRepository.Exists())
            {
                var context = _usersContext ?? _usersContextInfo.CreateInstance();

                try
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
                finally
                {
                    if (_usersContext == null)
                    {
                        context.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Updates the target database to a given migration.
        /// </summary>
        /// <param name="targetMigration"> The migration to upgrade/downgrade to. </param>
        public override void Update(string targetMigration)
        {
            base.EnsureDatabaseExists(() => UpdateInternal(targetMigration));
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private void UpdateInternal(string targetMigration)
        {
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
                        new VersionedModel(_currentModel),
                        Enumerable.Empty<MigrationOperation>(),
                        _modelDiffer.Diff(
                            _emptyModel.Value,
                            GetHistoryModel(_defaultSchema),
                            _modificationCommandTreeGenerator,
                            SqlGenerator),
                        downgrading: false));
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

            var migrationsApplied = new List<string>();

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

                migrationsApplied.Add(pendingMigration);

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
                    _calledByCreateDatabase
                        ? new VersionedModel(_emptyModel.Value)
                        : GetLastModel(lastMigration),
                    new VersionedModel(_currentModel),
                    false);
            }

            // Context may not be constructable when Migrations is being called by DbContext CreateDatabase
            // and the config cannot have Seed data anyway, so avoid doing model diff and creating the context to seed.
            if (!_calledByCreateDatabase
                && !IsModelOutOfDate(_currentModel, lastMigration))
            {
                base.SeedDatabase(migrationsApplied);
            }
        }

        internal override void SeedDatabase(IEnumerable<string> migrationsApplied)
        {
            Debug.Assert(!_calledByCreateDatabase);

            var context = _usersContext ?? _usersContextInfo.CreateInstance();
            if (_usersContext != null)
            {
                // If we're using the users context then don't pollute the state manager during seed
                context.InternalContext.UseTempObjectContext();
            }

            try
            {
                _configuration.OnSeed(context, migrationsApplied);

                context.SaveChanges();
            }
            finally
            {
                if (_usersContext == null)
                {
                    context.Dispose();
                }
                else
                {
                    context.InternalContext.DisposeTempObjectContext();
                }
            }
        }

        internal virtual bool IsModelOutOfDate(XDocument model, DbMigration lastMigration)
        {
            DebugCheck.NotNull(model);

            var sourceModel = GetLastModel(lastMigration);

            return _modelDiffer.Diff(sourceModel.Model, model, sourceModelVersion: sourceModel.Version).Any();
        }

        private VersionedModel GetLastModel(DbMigration lastMigration, string currentMigrationId = null)
        {
            if (lastMigration != null)
            {
                return lastMigration.GetTargetModel();
            }

            string migrationId, productVersion;
            var lastModel = _historyRepository.GetLastModel(out migrationId, out productVersion);

            if (lastModel != null
                && (currentMigrationId == null || string.CompareOrdinal(migrationId, currentMigrationId) < 0))
            {
                return new VersionedModel(lastModel, productVersion);
            }

            return new VersionedModel(_emptyModel.Value);
        }

        internal override void Downgrade(IEnumerable<string> pendingMigrations)
        {
            for (var i = 0; i < pendingMigrations.Count() - 1; i++)
            {
                var migrationId = pendingMigrations.ElementAt(i);
                var migration = _migrationAssembly.GetMigration(migrationId);
                var nextMigrationId = pendingMigrations.ElementAt(i + 1);

                string targetModelVersion = null;
                var targetModel = (nextMigrationId != InitialDatabase)
                                      ? _historyRepository.GetModel(nextMigrationId, out targetModelVersion)
                    : _emptyModel.Value;

                Debug.Assert(targetModel != null);

                string _;
                var sourceModel = _historyRepository.GetModel(migrationId, out _);

                if (migration == null)
                {
                    base.AutoMigrate(
                        migrationId, 
                        new VersionedModel(sourceModel),
                        new VersionedModel(targetModel, targetModelVersion), 
                        downgrading: true);
                }
                else
                {
                    base.RevertMigration(migrationId, migration, targetModel);
                }
            }
        }

        internal override void RevertMigration(
            string migrationId, DbMigration migration, XDocument targetModel)
        {
            var systemOperations = Enumerable.Empty<MigrationOperation>();

            var migrationSchema = GetDefaultSchema(migration);
            var historyModel = GetHistoryModel(migrationSchema);

            if (ReferenceEquals(targetModel, _emptyModel.Value)
                && !_historyRepository.IsShared())
            {
                systemOperations = _modelDiffer.Diff(historyModel, _emptyModel.Value);
            }
            else
            {
                var lastMigrationSchema = GetLastDefaultSchema(migrationId);

                if (!string.Equals(lastMigrationSchema, migrationSchema, StringComparison.Ordinal))
                {
                    var lastHistoryModel = GetHistoryModel(lastMigrationSchema);

                    systemOperations = _modelDiffer.Diff(historyModel, lastHistoryModel);
                }
            }

            migration.Down();

            ExecuteOperations(migrationId, new VersionedModel(targetModel), migration.Operations, systemOperations, downgrading: true);
        }

        internal override void ApplyMigration(DbMigration migration, DbMigration lastMigration)
        {
            DebugCheck.NotNull(migration);

            var migrationMetadata = (IMigrationMetadata)migration;
            var lastModel = GetLastModel(lastMigration, migrationMetadata.Id);
            var sourceModel = migration.GetSourceModel();
            var targetModel = migration.GetTargetModel();

            if (sourceModel != null
                && IsModelOutOfDate(sourceModel.Model, lastMigration))
            {
                    base.AutoMigrate(
                        migrationMetadata.Id.ToAutomaticMigrationId(),
                        lastModel,
                        sourceModel,
                        downgrading: false);

                    lastModel = sourceModel;
                }

            var migrationSchema = GetDefaultSchema(migration);
            var historyModel = GetHistoryModel(migrationSchema);

            var systemOperations = Enumerable.Empty<MigrationOperation>();

            if (ReferenceEquals(lastModel.Model, _emptyModel.Value)
                && !base.HistoryExists())
            {
                systemOperations = _modelDiffer.Diff(_emptyModel.Value, historyModel);
            }
            else
            {
                var lastMigrationSchema = GetLastDefaultSchema(migrationMetadata.Id);

                if (!string.Equals(lastMigrationSchema, migrationSchema, StringComparison.Ordinal))
                {
                    var lastHistoryModel = GetHistoryModel(lastMigrationSchema);

                    systemOperations = _modelDiffer.Diff(lastHistoryModel, historyModel);
                }
            }

            migration.Up();

            ExecuteOperations(migrationMetadata.Id, targetModel, migration.Operations, systemOperations, false);
        }

        private static string GetDefaultSchema(DbMigration migration)
        {
            DebugCheck.NotNull(migration);

            try
            {
                var defaultSchema = new ResourceManager(migration.GetType()).GetString(DefaultSchemaResourceKey);

                return !string.IsNullOrWhiteSpace(defaultSchema) ? defaultSchema : EdmModelExtensions.DefaultSchema;
            }
            catch (MissingManifestResourceException)
            {
                // Upgrade scenario, no default schema resource found
                return EdmModelExtensions.DefaultSchema;
            }
        }

        private string GetLastDefaultSchema(string migrationId)
        {
            DebugCheck.NotEmpty(migrationId);

            var lastMigrationId
                = _migrationAssembly
                    .MigrationIds
                    .LastOrDefault(m => string.CompareOrdinal(m, migrationId) < 0);

            return (lastMigrationId == null)
                ? EdmModelExtensions.DefaultSchema
                : GetDefaultSchema(_migrationAssembly.GetMigration(lastMigrationId));
        }

        internal override bool HistoryExists()
        {
            return _historyRepository.Exists();
        }

        internal override void AutoMigrate(
            string migrationId, VersionedModel sourceModel, VersionedModel targetModel, bool downgrading)
        {
            var systemOperations = Enumerable.Empty<MigrationOperation>();

            if (!_historyRepository.IsShared())
            {
                if (ReferenceEquals(targetModel.Model, _emptyModel.Value))
                {
                    systemOperations
                        = _modelDiffer.Diff(GetHistoryModel(EdmModelExtensions.DefaultSchema), _emptyModel.Value);
                }
                else if (ReferenceEquals(sourceModel.Model, _emptyModel.Value))
                {
                    systemOperations
                        = _modelDiffer.Diff(
                            _emptyModel.Value,
                            _calledByCreateDatabase
                                ? GetHistoryModel(_defaultSchema)
                                : GetHistoryModel(EdmModelExtensions.DefaultSchema));
                }
            }

            var operations
                = _modelDiffer
                    .Diff(
                        sourceModel.Model,
                        targetModel.Model,
                        targetModel.Model == _currentModel
                            ? _modificationCommandTreeGenerator
                            : null,
                        SqlGenerator,
                        sourceModel.Version,
                        targetModel.Version)
                    .ToList();

            if (!_calledByCreateDatabase
                && ReferenceEquals(targetModel.Model, _currentModel))
            {
                var lastDefaultSchema = GetLastDefaultSchema(migrationId);

                if (!string.Equals(lastDefaultSchema, _defaultSchema, StringComparison.Ordinal))
                {
                    throw Error.UnableToMoveHistoryTableWithAuto();
                }
            }

            if (!_configuration.AutomaticMigrationDataLossAllowed
                && operations.Any(o => o.IsDestructiveChange))
            {
                throw Error.AutomaticDataLoss();
            }

            if ((targetModel.Model != _currentModel)
                && (operations.Any(o => o is ProcedureOperation)))
            {
                throw Error.AutomaticStaleFunctions(migrationId);
            }

            ExecuteOperations(migrationId, targetModel, operations, systemOperations, downgrading, auto: true);
        }

        private void ExecuteOperations(
            string migrationId,
            VersionedModel targetModel,
            IEnumerable<MigrationOperation> operations,
            IEnumerable<MigrationOperation> systemOperations,
            bool downgrading,
            bool auto = false)
        {
            DebugCheck.NotEmpty(migrationId);
            DebugCheck.NotNull(targetModel);
            DebugCheck.NotNull(operations);
            DebugCheck.NotNull(systemOperations);

            FillInForeignKeyOperations(operations, targetModel.Model);

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
                    .Concat(systemOperations)
                    .ToList();

            var createHistoryOperation
                = systemOperations
                    .OfType<CreateTableOperation>()
                    .FirstOrDefault();

            if (createHistoryOperation != null)
            {
                _historyRepository.CurrentSchema
                    = DatabaseName.Parse(createHistoryOperation.Name).Schema;
            }

            var moveHistoryOperation
                = systemOperations
                    .OfType<MoveTableOperation>()
                    .FirstOrDefault();

            if (moveHistoryOperation != null)
            {
                _historyRepository.CurrentSchema = moveHistoryOperation.NewSchema;

                moveHistoryOperation.ContextKey = _configuration.ContextKey;
                moveHistoryOperation.IsSystem = true;
            }

            if (!downgrading)
            {
                orderedOperations.Add(_historyRepository.CreateInsertOperation(migrationId, targetModel));
            }
            else if (!systemOperations.Any(o => o is DropTableOperation))
            {
                orderedOperations.Add(_historyRepository.CreateDeleteOperation(migrationId));
            }

            var migrationStatements
                = base.GenerateStatements(orderedOperations, migrationId);

            if (auto)
            {
                // Filter duplicates when auto-migrating. Duplicates can be caused by
                // duplicates in the model such as shared FKs.
                migrationStatements
                    = migrationStatements
                        .Distinct((m1, m2) => string.Equals(m1.Sql, m2.Sql, StringComparison.Ordinal));
            }

            base.ExecuteStatements(migrationStatements);

            _historyRepository.ResetExists();
        }

        internal override IEnumerable<DbQueryCommandTree> CreateDiscoveryQueryTrees()
        {
            return _historyRepository.CreateDiscoveryQueryTrees();
        }

        internal override IEnumerable<MigrationStatement> GenerateStatements(
            IList<MigrationOperation> operations, string migrationId)
        {
            DebugCheck.NotNull(operations);

            return SqlGenerator.Generate(operations, _providerManifestToken);
        }

        internal override void ExecuteStatements(IEnumerable<MigrationStatement> migrationStatements)
        {
            ExecuteStatements(migrationStatements, null);
        }

        internal void ExecuteStatements(IEnumerable<MigrationStatement> migrationStatements, DbTransaction existingTransaction)
        {
            DebugCheck.NotNull(migrationStatements);

            DbConnection connection = null;
            try
            {
                if (existingTransaction != null)
                {
                    var interceptionContext = new DbInterceptionContext();
                    interceptionContext = interceptionContext.WithDbContext(_usersContext);
                    ExecuteStatementsWithinTransaction(migrationStatements, existingTransaction, interceptionContext);
                }
                else
                {
                    connection = CreateConnection();
                    DbProviderServices.GetExecutionStrategy(connection).Execute(
                        () => ExecuteStatementsInternal(migrationStatements, connection));
                }
            }
            finally
            {
                if (connection != null)
                {
                    DbInterception.Dispatch.Connection.Dispose(connection, new DbInterceptionContext());
                }
            }
        }

        private void ExecuteStatementsInternal(IEnumerable<MigrationStatement> migrationStatements, DbConnection connection)
        {
            DebugCheck.NotNull(migrationStatements);
            DebugCheck.NotNull(connection);

            var context = _usersContext ?? _usersContextInfo.CreateInstance();

            var interceptionContext = new DbInterceptionContext();
            interceptionContext = interceptionContext.WithDbContext(context);

            TransactionHandler transactionHandler = null;
            try
            {
                if (DbInterception.Dispatch.Connection.GetState(connection, interceptionContext) == ConnectionState.Broken)
                {
                    DbInterception.Dispatch.Connection.Close(connection, interceptionContext);
                }

                if (DbInterception.Dispatch.Connection.GetState(connection, interceptionContext) == ConnectionState.Closed)
                {
                    DbInterception.Dispatch.Connection.Open(connection, interceptionContext);
                }

                if (!(context is TransactionContext))
                {
                    var providerInvariantName =
                        DbConfiguration.DependencyResolver.GetService<IProviderInvariantName>(
                            DbProviderServices.GetProviderFactory(connection))
                            .Name;

                    var dataSource = DbInterception.Dispatch.Connection.GetDataSource(connection, interceptionContext);

                    var transactionHandlerFactory = DbConfiguration.DependencyResolver.GetService<Func<TransactionHandler>>(
                        new ExecutionStrategyKey(providerInvariantName, dataSource));

                    if (transactionHandlerFactory != null)
                    {
                        transactionHandler = transactionHandlerFactory();
                        transactionHandler.Initialize(context, connection);
                    }
                }

                ExecuteStatementsInternal(migrationStatements, connection, interceptionContext);

                _committedStatements = true;
            }
            finally
            {
                if (transactionHandler != null)
                {
                    transactionHandler.Dispose();
                }

                if (_usersContext == null)
                {
                    context.Dispose();
                }
            }
        }

        private void ExecuteStatementsInternal(
            IEnumerable<MigrationStatement> migrationStatements,
            DbConnection connection,
            DbTransaction transaction,
            DbInterceptionContext interceptionContext)
        {
            DebugCheck.NotNull(migrationStatements);
            DebugCheck.NotNull(connection);

            foreach (var migrationStatement in migrationStatements)
            {
                base.ExecuteSql(migrationStatement, connection, transaction, interceptionContext);
            }
        }

        private void ExecuteStatementsInternal(
            IEnumerable<MigrationStatement> migrationStatements, DbConnection connection,
            DbInterceptionContext interceptionContext)
        {
            DebugCheck.NotNull(migrationStatements);
            DebugCheck.NotNull(connection);

            var pendingStatements = new List<MigrationStatement>();

            foreach (var statement in migrationStatements.Where(s => !string.IsNullOrEmpty(s.Sql)))
            {
                if (!statement.SuppressTransaction)
                {
                    pendingStatements.Add(statement);

                    continue;
                }

                if (pendingStatements.Any())
                {
                    ExecuteStatementsWithinNewTransaction(pendingStatements, connection, interceptionContext);

                    pendingStatements.Clear();
                }

                base.ExecuteSql(statement, connection, null, interceptionContext);
            }

            if (pendingStatements.Any())
            {
                ExecuteStatementsWithinNewTransaction(pendingStatements, connection, interceptionContext);
            }
        }

        private void ExecuteStatementsWithinTransaction(
            IEnumerable<MigrationStatement> migrationStatements, DbTransaction transaction,
            DbInterceptionContext interceptionContext)
        {
            DebugCheck.NotNull(migrationStatements);
            DebugCheck.NotNull(transaction);

            var connection = DbInterception.Dispatch.Transaction.GetConnection(transaction, interceptionContext);

            ExecuteStatementsInternal(migrationStatements, connection, transaction, interceptionContext);
        }

        private void ExecuteStatementsWithinNewTransaction(
            IEnumerable<MigrationStatement> migrationStatements, DbConnection connection,
            DbInterceptionContext interceptionContext)
        {
            DebugCheck.NotNull(migrationStatements);
            DebugCheck.NotNull(connection);

            var beginTransactionInterceptionContext
                = new BeginTransactionInterceptionContext(interceptionContext)
                    .WithIsolationLevel(IsolationLevel.Serializable);

            DbTransaction transaction = null;
            try
            {
                transaction
                    = DbInterception.Dispatch.Connection.BeginTransaction(
                        connection, beginTransactionInterceptionContext);

                ExecuteStatementsWithinTransaction(migrationStatements, transaction, interceptionContext);

                DbInterception.Dispatch.Transaction.Commit(transaction, interceptionContext);
            }
            finally
            {
                if (transaction != null)
                {
                    DbInterception.Dispatch.Transaction.Dispose(transaction, interceptionContext);
                }
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        internal override void ExecuteSql(
            MigrationStatement migrationStatement, DbConnection connection, DbTransaction transaction,
            DbInterceptionContext interceptionContext)
        {
            DebugCheck.NotNull(migrationStatement);
            DebugCheck.NotNull(connection);

            if (string.IsNullOrWhiteSpace(migrationStatement.Sql))
            {
                return;
            }

            var dbCommand = connection.CreateCommand();

            using (var command = ConfigureCommand(dbCommand, migrationStatement.Sql, interceptionContext))
            {
                if (transaction != null)
                {
                    command.Transaction = transaction;
                }

                command.ExecuteNonQuery();
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private InterceptableDbCommand ConfigureCommand(DbCommand command, string commandText, DbInterceptionContext interceptionContext)
        {
            command.CommandText = commandText;

            if (_configuration.CommandTimeout.HasValue)
            {
                command.CommandTimeout = _configuration.CommandTimeout.Value;
            }

            return new InterceptableDbCommand(command, interceptionContext);
        }

        private void FillInForeignKeyOperations(IEnumerable<MigrationOperation> operations, XDocument targetModel)
        {
            DebugCheck.NotNull(operations);
            DebugCheck.NotNull(targetModel);

            foreach (var foreignKeyOperation
                in operations.OfType<AddForeignKeyOperation>()
                    .Where(fk => fk.PrincipalTable != null && !fk.PrincipalColumns.Any()))
            {
                var principalTable = GetStandardizedTableName(foreignKeyOperation.PrincipalTable);
                var entitySetName
                    = (from es in targetModel.Descendants(EdmXNames.Ssdl.EntitySetNames)
                        where new DatabaseName(es.TableAttribute(), es.SchemaAttribute()).ToString()
                            .EqualsIgnoreCase(principalTable)
                        select es.NameAttribute()).SingleOrDefault();

                if (entitySetName != null)
                {
                    var entityTypeElement
                        = targetModel.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                            .Single(et => et.NameAttribute().EqualsIgnoreCase(entitySetName));

                    entityTypeElement
                        .Descendants(EdmXNames.Ssdl.PropertyRefNames).Each(
                            pr => foreignKeyOperation.PrincipalColumns.Add(pr.NameAttribute()));
                }
                else
                {
                    // try and find the table in the current list of ops
                    var table
                        = operations
                            .OfType<CreateTableOperation>()
                            .SingleOrDefault(ct => GetStandardizedTableName(ct.Name).EqualsIgnoreCase(principalTable));

                    if ((table != null)
                        && (table.PrimaryKey != null))
                    {
                        table.PrimaryKey.Columns.Each(c => foreignKeyOperation.PrincipalColumns.Add(c));
                    }
                    else
                    {
                        throw Error.PartialFkOperation(
                            foreignKeyOperation.DependentTable, foreignKeyOperation.DependentColumns.Join());
                    }
                }
            }
        }

        private string GetStandardizedTableName(string tableName)
        {
            DebugCheck.NotEmpty(tableName);

            var databaseName = DatabaseName.Parse(tableName);

            if (!string.IsNullOrWhiteSpace(databaseName.Schema))
            {
                return tableName;
            }

            return new DatabaseName(tableName, _defaultSchema).ToString();
        }

        // <summary>
        // Ensures that the database exists by creating an empty database if one does not
        // already exist. If a new empty database is created but then the code in mustSucceedToKeepDatabase
        // throws an exception, then an attempt is made to clean up (delete) the new empty database.
        // This avoids leaving an empty database with no or incomplete metadata (e.g. MigrationHistory)
        // which can then cause problems for database initializers that check whether or not a database
        // exists.
        // </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal override void EnsureDatabaseExists(Action mustSucceedToKeepDatabase)
        {
            var databaseCreated = false;
            var databaseCreator = new DatabaseCreator(_configuration.CommandTimeout);

            {
                DbConnection connection = null;
                try
                {
                    connection = CreateConnection();

                    if (_existenceState == DatabaseExistenceState.DoesNotExist
                        || (_existenceState == DatabaseExistenceState.Unknown
                            && !databaseCreator.Exists(connection)))
                    {
                        databaseCreator.Create(connection);

                        databaseCreated = true;
                    }
                }
                finally
                {
                    if (connection != null)
                    {
                        DbInterception.Dispatch.Connection.Dispose(connection, new DbInterceptionContext());
                    }
                }
            }

            _emptyMigrationNeeded = databaseCreated;

            try
            {
                _committedStatements = false;
                mustSucceedToKeepDatabase();
            }
            catch
            {
                if (databaseCreated
                    && !_committedStatements)
                {
                    DbConnection connection = null;
                    try
                    {
                        connection = CreateConnection();

                        databaseCreator.Delete(connection);
                    }
                    catch
                    {
                        // Intentionally swallowing this exception since it is better to throw the
                        // original exception again for the user to see what the real problem is. An
                        // exception here is unlikely and would not be a root cause, but rather a
                        // cleanup issue.
                    }
                    finally
                    {
                        if (connection != null)
                        {
                            DbInterception.Dispatch.Connection.Dispose(connection, new DbInterceptionContext());
                        }
                    }
                }
                throw;
            }
        }

        private DbConnection CreateConnection()
        {
            var connection = _connection == null
                ? _providerFactory.CreateConnection()
                : DbProviderServices.GetProviderServices(_connection).CloneDbConnection(_connection, _providerFactory);

            var interceptionContext = new DbConnectionPropertyInterceptionContext<string>().WithValue(_usersContextInfo.ConnectionString);
            if (_usersContext != null)
            {
                interceptionContext = interceptionContext.WithDbContext(_usersContext);
            }

            DbInterception.Dispatch.Connection.SetConnectionString(connection, interceptionContext);

            return connection;
        }
    }
}
