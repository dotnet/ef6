// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Objects
{
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Resources;
    using System.Data.SqlClient;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using System.Transactions;
    using Xunit;

    public class TransactionLogEntry
    {
        public int ID { get; set; }
        public int TransactionCount { get; set; }
    }

    public class TransactionContext : ObjectContext
    {
        public TransactionContext(string connectionString)
            :
                base(connectionString)
        {
        }

        public IQueryable<TransactionLogEntry> LogEntries
        {
            get { return CreateObjectSet<TransactionLogEntry>("Entities.TransactionLog"); }
        }
    }

    public class TransactionsTests : IUseFixture<TransactionFixture>
    {
        private string connectionString;
        private string modelDirectory;

        private MetadataWorkspace workspace;
        private SqlConnection globalConnection;

        public void SetFixture(TransactionFixture data)
        {
            globalConnection = data.GlobalConnection;

            connectionString =
                @"metadata=res://EntityFramework.FunctionalTests/System.Data.Entity.Objects.TransactionsModel.csdl|res://EntityFramework.FunctionalTests/System.Data.Entity.Objects.TransactionsModel.ssdl|res://EntityFramework.FunctionalTests/System.Data.Entity.Objects.TransactionsModel.msl;provider=System.Data.SqlClient;provider connection string=""Data Source=.\sqlexpress;Initial Catalog=tempdb;Integrated Security=True""";
        }

        [Fact]
        public void Verify_implicit_transaction_is_created_when_no_transaction_created_by_user_and_connection_is_closed()
        {
            try
            {
                using (var ctx = CreateTransactionContext())
                {
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);
                    Assert.Equal(1, CreateTransactionContext().LogEntries.Count());
                    Assert.Equal(ConnectionState.Closed, ctx.Connection.State);
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_created_when_no_transaction_created_by_user_and_connection_is_open()
        {
            try
            {
                using (var ctx = CreateTransactionContext())
                {
                    ctx.Connection.Open();
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);

                    // Implicit transaction committed. 1 entity expected.
                    Assert.Equal(1, CreateTransactionContext().LogEntries.Count());
                    Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_creates_transaction_using_TransactionScope_and_connection_is_closed
            ()
        {
            try
            {
                using (var transactionScope = new TransactionScope())
                {
                    using (var ctx = CreateTransactionContext())
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Closed, ctx.Connection.State);
                    }
                }

                // Transaction not commited. No entities should be saved to the database.
                Assert.Equal(0, CreateTransactionContext().LogEntries.Count());
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_creates_transaction_using_TransactionScope_and_connection_is_open()
        {
            try
            {
                using (var transactionScope = new TransactionScope())
                {
                    using (var ctx = CreateTransactionContext())
                    {
                        ctx.Connection.Open();
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    }
                }

                // Transaction not commited. No entities should be saved to the database.
                Assert.Equal(0, CreateTransactionContext().LogEntries.Count());
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_executing_multiple_operations_in_the_same_TransactionScope()
        {
            try
            {
                using (var transactionScope = new TransactionScope())
                {
                    using (var ctx = CreateTransactionContext())
                    {
                        ctx.Connection.Open();
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);

                        transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    }
                }

                // "Transaction not commited. No entities should be saved to the database."
                Assert.Equal(0, CreateTransactionContext().LogEntries.Count());
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_creates_transaction_explicitly_using_CommittableTransaction()
        {
            try
            {
                using (var ctx = CreateTransactionContext())
                {
                    ctx.Connection.Open();
                    using (var committableTransaction = new CommittableTransaction())
                    {
                        ctx.Connection.EnlistTransaction(committableTransaction);

                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    }
                }

                // "Transaction not commited. No entities should be saved to the database."
                Assert.Equal(0, CreateTransactionContext().LogEntries.Count());
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_enlists_TransactionScope()
        {
            try
            {
                using (var ctx = CreateTransactionContext())
                {
                    ctx.Connection.Open();
                    using (var transactionScope = new TransactionScope())
                    {
                        ctx.Connection.EnlistTransaction(Transaction.Current);

                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    }
                }

                // "Transaction not commited. No entities should be saved to the database."
                Assert.Equal(0, CreateTransactionContext().LogEntries.Count());
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_creates_ambient_CommittableTransaction()
        {
            try
            {
                using (var ctx = CreateTransactionContext())
                {
                    ctx.Connection.Open();
                    using (var committableTransaction = new CommittableTransaction(new TimeSpan(1, 0, 0)))
                    {
                        ctx.Connection.EnlistTransaction(committableTransaction);
                        Transaction.Current = committableTransaction;

                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);

                        Transaction.Current = null;
                    }
                }

                // Transaction not commited. No entities should be saved to the database.
                Assert.Equal(0, CreateTransactionContext().LogEntries.Count());
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_starts_DbTransaction_explicitly()
        {
            try
            {
                using (var ctx = CreateTransactionContext())
                {
                    ctx.Connection.Open();
                    using (var dbTransaction = ctx.Connection.BeginTransaction())
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    }
                }

                // "Transaction not commited. No entities should be saved to the database."
                Assert.Equal(0, CreateTransactionContext().LogEntries.Count());
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_DbTransaction_cannot_be_mixed_with_TransactionScope()
        {
            try
            {
                using (var ctx = CreateTransactionContext())
                {
                    ctx.Connection.Open();
                    using (var dbTransaction = ctx.Connection.BeginTransaction())
                    {
                        using (var transactionScope = new TransactionScope())
                        {
                            var transactionLogEntry = new TransactionLogEntry();
                            ctx.AddObject("Entities.TransactionLog", transactionLogEntry);

                            Assert.Equal(
                                Strings.EntityClient_ProviderSpecificError("EnlistTransaction"),
                                Assert.Throws<EntityException>(() => ctx.SaveChanges()).Message);
                        }
                    }
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_DbTransaction_cannot_be_mixed_with_CommittableTransaction()
        {
            try
            {
                using (var ctx = CreateTransactionContext())
                {
                    ctx.Connection.Open();
                    using (var dbTransaction = ctx.Connection.BeginTransaction())
                    {
                        using (var committableTransaction = new CommittableTransaction())
                        {
                            Assert.Equal(
                                Strings.EntityClient_ProviderSpecificError("EnlistTransaction"),
                                Assert.Throws<EntityException>(() => ctx.Connection.EnlistTransaction(committableTransaction)).Message);
                        }
                    }
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_using_TransactionScope_with_DbTransaction_results_in_nested_transaction_and_implicit_transaction_not_created()
        {
            try
            {
                using (var ctx = CreateTransactionContext())
                {
                    using (var transactionScope = new TransactionScope())
                    {
                        ctx.Connection.Open();
                        using (var dbTransaction = ctx.Connection.BeginTransaction())
                        {
                            var transactionLogEntry = AddLogEntryToDatabase(ctx);
                            Assert.Equal(2, transactionLogEntry.TransactionCount);
                            Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                        }
                    }
                }

                // "Transaction not commited. No entities should be saved to the database."
                Assert.Equal(0, CreateTransactionContext().LogEntries.Count());
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void
            Verify_using_CommittableTransaction_with_DbTransaction_results_in_nested_transaction_and_implicit_transaction_not_created()
        {
            try
            {
                using (var ctx = CreateTransactionContext())
                {
                    ctx.Connection.Open();
                    using (var committableTransaction = new CommittableTransaction())
                    {
                        ctx.Connection.EnlistTransaction(committableTransaction);

                        using (var dbTransaction = ctx.Connection.BeginTransaction())
                        {
                            var transactionLogEntry = AddLogEntryToDatabase(ctx);
                            Assert.Equal(2, transactionLogEntry.TransactionCount);
                            Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                        }
                    }
                }

                // "Transaction not commited. No entities should be saved to the database."
                Assert.Equal(0, CreateTransactionContext().LogEntries.Count());
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_TransactionScope_cannot_be_mixed_with_CommitableTransaction()
        {
            try
            {
                using (var ctx = CreateTransactionContext())
                {
                    using (var transactionScope = new TransactionScope())
                    {
                        ctx.Connection.Open();
                        using (var committableTransaction = new CommittableTransaction())
                        {
                            Assert.Equal(
                                Strings.EntityClient_ProviderSpecificError("EnlistTransaction"),
                                Assert.Throws<EntityException>(() => ctx.Connection.EnlistTransaction(committableTransaction)).Message);
                        }
                    }
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_CommitableTransaction_cannot_be_mixed_with_TransactionScope()
        {
            try
            {
                using (var ctx = CreateTransactionContext())
                {
                    ctx.Connection.Open();
                    using (var committableTransaction = new CommittableTransaction())
                    {
                        ctx.Connection.EnlistTransaction(committableTransaction);
                        using (var transactionScope = new TransactionScope())
                        {
                            var transactionLogEntry = new TransactionLogEntry();
                            ctx.AddObject("Entities.TransactionLog", transactionLogEntry);

                            Assert.Equal(
                                Strings.EntityClient_ProviderSpecificError("EnlistTransaction"),
                                Assert.Throws<EntityException>(() => ctx.SaveChanges()).Message);
                        }
                    }
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_created_when_transaction_from_previous_operation_disposed_and_connection_opened()
        {
            try
            {
                using (var ctx = CreateObjectContext())
                {
                    ctx.Connection.Open();
                    using (var transactionScope = new TransactionScope())
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    }

                    // "Transaction not commited. No entities should be saved to the database."
                    Assert.Equal(0, CreateTransactionContext().LogEntries.Count());

                    AddLogEntryToDatabase(ctx);

                    // "Implicit transaction committed. 1 entity expected."
                    Assert.Equal(1, CreateTransactionContext().LogEntries.Count());
                    Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void
            Verify_no_implicit_transaction_created_when_if_enlisted_in_explicit_transaction_after_transaction_from_previous_operation_disposed
            ()
        {
            try
            {
                using (var ctx = CreateObjectContext())
                {
                    ctx.Connection.Open();
                    using (var transactionScope = new TransactionScope())
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    }

                    // "Transaction not commited. No entities should be saved to the database."
                    Assert.Equal(0, CreateTransactionContext().LogEntries.Count());

                    using (var committableTransaction = new CommittableTransaction())
                    {
                        ctx.Connection.EnlistTransaction(committableTransaction);
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    }

                    // "Transaction not commited. No entities should be saved to the database."
                    Assert.Equal(0, CreateTransactionContext().LogEntries.Count());
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_created_when_transaction_from_previous_operation_disposed_and_connection_closed()
        {
            try
            {
                using (var ctx = CreateObjectContext())
                {
                    using (var transactionScope = new TransactionScope())
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Closed, ctx.Connection.State);
                    }

                    // "Transaction not commited. No entities should be saved to the database."
                    Assert.Equal(0, CreateTransactionContext().LogEntries.Count());

                    var newTransactionLogEntry = AddLogEntryToDatabase(ctx);

                    // "Implicit transaction committed. 1 entity expected."
                    Assert.Equal(1, CreateTransactionContext().LogEntries.Count());
                    Assert.Equal(ConnectionState.Closed, ctx.Connection.State);
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_explicit_transaction_cleared_after_disposing()
        {
            try
            {
                using (var ctx = CreateObjectContext())
                {
                    ctx.Connection.Open();

                    using (var committableTransaction = new CommittableTransaction())
                    {
                        ctx.Connection.EnlistTransaction(committableTransaction);
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                    }

                    // "Transaction not commited. No entities should be saved to the database."
                    Assert.Equal(0, CreateTransactionContext().LogEntries.Count());

                    var newTransactionLogEntry = AddLogEntryToDatabase(ctx);

                    // "Implicit transaction committed. 1 entity expected."
                    Assert.Equal(1, CreateTransactionContext().LogEntries.Count());
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_no_implicit_transaction_created_when_transactions_change_between_requests()
        {
            try
            {
                using (var ctx = CreateObjectContext())
                {
                    ctx.Connection.Open();

                    using (var transaction1 = new TransactionScope())
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    }

                    using (var transaction2 = new TransactionScope())
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    }
                }

                // "Transaction not commited. No entities should be saved to the database."
                Assert.Equal(0, CreateTransactionContext().LogEntries.Count());
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_no_implicit_transaction_created_when_enlisting_in_transaction_between_requests()
        {
            try
            {
                using (var ctx = CreateObjectContext())
                {
                    ctx.Connection.Open();

                    using (var transaction1 = new TransactionScope())
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    }

                    using (var committableTransaction = new CommittableTransaction())
                    {
                        ctx.Connection.EnlistTransaction(committableTransaction);

                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    }
                }

                // "Transaction not commited. No entities should be saved to the database."
                Assert.Equal(0, CreateTransactionContext().LogEntries.Count());
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_no_implicit_transaction_created_when_transaction_change_and_connection_is_closed_between_requests()
        {
            try
            {
                using (var ctx = CreateObjectContext())
                {
                    ctx.Connection.Open();

                    using (var trx = new TransactionScope())
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        ctx.Connection.Close();
                    }

                    using (var trx = new TransactionScope())
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Closed, ctx.Connection.State);
                    }
                }

                // "Transaction not commited. No entities should be saved to the database."
                Assert.Equal(0, CreateTransactionContext().LogEntries.Count());
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_cannot_enlist_in_more_than_one_active_transaction_on_the_same_opened_connection()
        {
            try
            {
                using (var ctx = CreateTransactionContext())
                {
                    ctx.Connection.Open();
                    using (var committableTransaction = new CommittableTransaction())
                    {
                        using (var newCommittableTransaction = new CommittableTransaction())
                        {
                            ctx.Connection.EnlistTransaction(committableTransaction);

                            Assert.Equal(
                                Strings.EntityClient_ProviderSpecificError("EnlistTransaction"),
                                Assert.Throws<EntityException>(() => ctx.Connection.EnlistTransaction(newCommittableTransaction)).Message);
                        }
                    }
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_closing_connection_invalidates_explicit_transaction()
        {
            try
            {
                using (var ctx = CreateTransactionContext())
                {
                    ctx.Connection.Open();
                    using (var committableTransaction = new CommittableTransaction())
                    {
                        ctx.Connection.EnlistTransaction(committableTransaction);
                        ctx.Connection.Close();

                        ctx.Connection.Open();
                        using (var newCommittableTransaction = new CommittableTransaction())
                        {
                            ctx.Connection.EnlistTransaction(newCommittableTransaction);
                        }
                    }
                }
            }
            finally
            {
                ResetTables();
            }
        }

        private TransactionContext CreateTransactionContext()
        {
            var ctx = new TransactionContext(connectionString);
            ctx.MetadataWorkspace.LoadFromAssembly(Assembly.GetExecutingAssembly());

            return ctx;
        }

        private ObjectContext CreateObjectContext()
        {
            var ctx = new ObjectContext(connectionString);
            ctx.MetadataWorkspace.LoadFromAssembly(Assembly.GetExecutingAssembly());

            return ctx;
        }

        private int LogEntriesCount()
        {
            using (var ctx = CreateTransactionContext())
            {
                return ctx.LogEntries.Count();
            }
        }

        private TransactionLogEntry AddLogEntryToDatabase(ObjectContext ctx)
        {
            var transactionLogEntry = new TransactionLogEntry();
            ctx.AddObject("Entities.TransactionLog", transactionLogEntry);
            ctx.SaveChanges();

            return transactionLogEntry;
        }

        /// <summary>
        ///     Removes all entries from the tables used for tests. Must be called from tests that are committing transactions
        ///     as each test expects that the db will be initially clean.
        /// </summary>
        private void ResetTables()
        {
            new SqlCommand("DELETE FROM ##TransactionLog", globalConnection).ExecuteNonQuery();
            Assert.Equal(0, CreateTransactionContext().LogEntries.Count());
        }
    }

    public class TransactionFixture : FunctionalTestBase, IDisposable
    {
        public SqlConnection GlobalConnection { get; private set; }

        public TransactionFixture()
        {
            if (GlobalConnection != null)
            {
                throw new InvalidOperationException("Database is still in use and cannot be initialized.");
            }

            // we are using tempdb and SQLExpress instance so we don't want this to be configurable
            GlobalConnection = new SqlConnection("Data Source=.\\SQLEXPRESS;Initial Catalog=tempdb;Integrated Security=SSPI;");
            GlobalConnection.Open();

            try
            {
                new SqlCommand(
                    @"
    CREATE TABLE [dbo].[##TransactionLog](
	    [ID] [int] IDENTITY(1,1) NOT NULL,
	    [TransactionCount] [int] NOT NULL,
     CONSTRAINT [PK_TransactionLog] PRIMARY KEY CLUSTERED 
    (
	    [ID] ASC
    )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
    ) ON [PRIMARY]", GlobalConnection).ExecuteNonQuery();

                new SqlCommand(
                    @"
    CREATE PROCEDURE [dbo].[##CreateTransactionLogEntry] 
    AS
    BEGIN
        DECLARE @tranCount AS int = @@TRANCOUNT  -- assigning to a variable prevents from counting an implicit transaction created for insert
	    INSERT INTO ##TransactionLog Values(@tranCount)
	    SELECT ID, TransactionCount 
        FROM ##TransactionLog
        WHERE ID = SCOPE_IDENTITY()
    END", GlobalConnection).ExecuteNonQuery();
            }
            catch
            {
                CleanupDatabase();

                throw;
            }
        }

        private void CleanupDatabase()
        {
            Contract.Assert(GlobalConnection != null);

            GlobalConnection.Close();
            GlobalConnection.Dispose();
            GlobalConnection = null;
        }

        public void Dispose()
        {
            CleanupDatabase();
        }
    }
}
