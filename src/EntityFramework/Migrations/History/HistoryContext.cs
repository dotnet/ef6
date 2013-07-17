// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.History
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     This class is used by Code First Migrations to read and write migration history
    ///     from the database.
    ///     To customize the definition of the migrations history table you can derive from
    ///     this class and override OnModelCreating. Derived instances can either be registered
    ///     on a per migrations configuration basis using <see cref="DbMigrationsConfiguration.SetHistoryContextFactory" />,
    ///     or globally using <see cref="DbConfiguration" />.
    /// </summary>
    public class HistoryContext : DbContext, IDbModelCacheKeyProvider
    {
        /// <summary>
        ///     The default name used for the migrations history table.
        /// </summary>
        public const string DefaultTableName = "__MigrationHistory";

        internal const int ContextKeyMaxLength = 300;
        internal const int MigrationIdMaxLength = 150;

        private readonly string _defaultSchema;

        internal static readonly Func<DbConnection, string, HistoryContext> DefaultFactory = (e, d) => new HistoryContext(e, d);

        /// <summary>
        ///     For testing
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal HistoryContext()
        {
            InternalContext.InitializerDisabled = true;
        }

        /// <summary>
        ///     Initializes a new instance of the HistoryContext class.
        ///     If you are creating a derived history context you will generally expose a constructor
        ///     that accepts these same three parameters and passes them to this base constructor.
        /// </summary>
        /// <param name="existingConnection">
        ///     An existing connection to use for the new context.
        /// </param>
        /// <param name="defaultSchema">
        ///     The default schema of the model being migrated.
        ///     This schema will be used for the migrations history table unless a different schema is configured in OnModelCreating.
        /// </param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public HistoryContext(DbConnection existingConnection, string defaultSchema)
            : base(existingConnection, contextOwnsConnection: false)
        {
            _defaultSchema = defaultSchema;

            Configuration.ValidateOnSaveEnabled = false;
            InternalContext.InitializerDisabled = true;
        }

        /// <summary>
        ///     Gets the key used to locate a model that was previously built for this context. This is used
        ///     to avoid processing OnModelCreating and calculating the model every time a new context instance is created.
        ///     By default this property returns the default schema.
        ///     In most cases you will not need to override this property. However, if your implementation of OnModelCreating
        ///     contains conditional logic that results in a different model being built for the same database provider and
        ///     default schema you should override this property and calculate an appropriate key.
        /// </summary>
        public virtual string CacheKey
        {
            get { return _defaultSchema; }
        }

        /// <summary>
        ///     Gets the default schema of the model being migrated.
        ///     This schema will be used for the migrations history table unless a different schema is configured in OnModelCreating.
        /// </summary>
        protected string DefaultSchema
        {
            get { return _defaultSchema; }
        }

        /// <summary>
        ///     Gets or sets a <see cref="DbSet{TEntity}" /> that can be used to read and write <see cref="HistoryRow" /> instances.
        /// </summary>
        public virtual IDbSet<HistoryRow> History { get; set; }

        /// <summary>
        ///     Applies the default configuration for the migrations history table. If you override
        ///     this method it is recommended that you call this base implementation before applying your
        ///     custom configuration.
        /// </summary>
        /// <param name="modelBuilder"> The builder that defines the model for the context being created. </param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(_defaultSchema);

            modelBuilder.Entity<HistoryRow>().ToTable(DefaultTableName);
            modelBuilder.Entity<HistoryRow>().HasKey(
                h => new
                         {
                             h.MigrationId,
                             h.ContextKey
                         });
            modelBuilder.Entity<HistoryRow>().Property(h => h.MigrationId).HasMaxLength(MigrationIdMaxLength).IsRequired();
            modelBuilder.Entity<HistoryRow>().Property(h => h.ContextKey).HasMaxLength(ContextKeyMaxLength).IsRequired();
            modelBuilder.Entity<HistoryRow>().Property(h => h.Model).IsRequired().IsMaxLength();
            modelBuilder.Entity<HistoryRow>().Property(h => h.ProductVersion).HasMaxLength(32).IsRequired();
        }
    }
}
