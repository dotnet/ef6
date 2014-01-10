// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;

    /// <summary>
    /// This class is used by <see cref="CommitFailureHandler"/> to write and read transaction tracing information
    /// from the database.
    /// To customize the definition of the transaction table you can derive from
    /// this class and override <see cref="OnModelCreating"/>. Derived classes can be registered
    /// using <see cref="DbConfiguration" />.
    /// </summary>
    /// <remarks>
    /// By default EF will poll the resolved <see cref="TransactionContext"/> to check wether the database schema is compatible and
    /// will try to modify it accordingly if it's not. To disable this check call
    /// <code>Database.SetInitializer&lt;TTransactionContext&gt;(null)</code> where TTransactionContext is the type of the resolved context.
    /// </remarks>
    public class TransactionContext : DbContext
    {
        private const string _defaultTableName = "__TransactionHistory";

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionContext"/> class.
        /// </summary>
        /// <param name="existingConnection">The connection used by the context for which the transactions will be recorded.</param>
        public TransactionContext(DbConnection existingConnection)
            : base(existingConnection, contextOwnsConnection: false)
        {
            Configuration.ValidateOnSaveEnabled = false;
        }

        /// <summary>
        /// Gets or sets a <see cref="DbSet{TEntity}" /> that can be used to read and write <see cref="TransactionRow" /> instances.
        /// </summary>
        public virtual IDbSet<TransactionRow> Transactions { get; set; }

        /// <inheritdoc/>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransactionRow>().ToTable(_defaultTableName);
        }
    }
}
