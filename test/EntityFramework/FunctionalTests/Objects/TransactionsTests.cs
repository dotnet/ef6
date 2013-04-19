// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Objects
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Common;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;
    using System.Transactions;
    using Xunit;
    using IsolationLevel = System.Data.IsolationLevel;

    public class TransactionLogEntry
    {
        public int ID { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int TransactionCount { get; set; }
    }

    public class TransactionContext : ObjectContext
    {
        public TransactionContext(string connectionString)
            : base(connectionString)
        {
        }

        public TransactionContext(EntityConnection connection)
            : base(connection)
        {
        }

        public IQueryable<TransactionLogEntry> LogEntries
        {
            get { return CreateObjectSet<TransactionLogEntry>("Entities.TransactionLog"); }
        }
    }

    public class TransactionDbContext : DbContext
    {
        static TransactionDbContext()
        {
            Database.SetInitializer<TransactionDbContext>(null);
        }

        public TransactionDbContext(string connectionString)
            : base(connectionString)
        {
        }

        public TransactionDbContext(EntityConnection connection, bool contextOwnsConnection)
            : base(connection, contextOwnsConnection)
        {
        }

        public TransactionDbContext(SqlConnection connection, DbCompiledModel model, bool contextOwnsConnection)
            : base(connection, model, contextOwnsConnection)
        {
        }

        public DbSet<TransactionLogEntry> LogEntries { get; set; }
    }

    public class TransactionsTests : FunctionalTestBase, IUseFixture<TransactionFixture>
    {
        private string _entityConnectionString;
        private string _connectionString;
        private string _modelDirectory;
        private MetadataWorkspace _workspace;
        private DbCompiledModel _compiledModel;

        public void SetFixture(TransactionFixture data)
        {
            _compiledModel = data.CompiledModel;
            _connectionString = data.ConnectionString;

            _entityConnectionString = string.Format(
                @"metadata=res://EntityFramework.FunctionalTests/System.Data.Entity.Objects.TransactionsModel.csdl|res://EntityFramework.FunctionalTests/System.Data.Entity.Objects.TransactionsModel.ssdl|res://EntityFramework.FunctionalTests/System.Data.Entity.Objects.TransactionsModel.msl;provider=System.Data.SqlClient;provider connection string=""{0}""",
                _connectionString);
        }

        [Fact]
        public void Verify_implicit_transaction_is_created_when_using_DbContext_and_no_transaction_created_by_user_and_connection_is_closed()
        {
            try
            {
                using (var ctx = CreateTransactionDbContext())
                {
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);

                    // Implicit transaction committed. 1 entity expected.
                    Assert.Equal(1, LogEntriesCount());
                    Assert.Equal(ConnectionState.Closed, ctx.Database.Connection.State);
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_created_when_using_DbContext_and_no_transaction_created_by_user_and_EntityConnection_is_opened_and_context_owns_connection()
        {
            try
            {
                using (var entityConnection = new EntityConnection(_entityConnectionString))
                {
                    entityConnection.Open();

                    using (var ctx = CreateTransactionDbContext(entityConnection, contextOwnsConnection: true))
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);

                        // Implicit transaction committed. 1 entity expected.
                        Assert.Equal(1, LogEntriesCount());
                        Assert.Equal(ConnectionState.Open, ctx.Database.Connection.State);
                    }

                    Assert.Equal(ConnectionState.Closed, entityConnection.State);
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_created_when_using_DbContext_and_no_transaction_created_by_user_and_EntityConnection_is_opened_and_context_does_not_own_connection()
        {
            try
            {
                using (var entityConnection = new EntityConnection(_entityConnectionString))
                {
                    entityConnection.Open();
                    using (var ctx = CreateTransactionDbContext(entityConnection, contextOwnsConnection: false))
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);

                        // Implicit transaction committed. 1 entity expected.
                        Assert.Equal(1, LogEntriesCount());
                    }

                    Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State);
                    Assert.Equal(ConnectionState.Open, entityConnection.State);
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_created_when_using_DbContext_and_no_transaction_created_by_user_and_SqlConnection_is_opened_and_context_owns_connection()
        {
            try
            {
                using (var sqlConnection = new SqlConnection(_connectionString))
                {
                    sqlConnection.Open();

                    using (var ctx = CreateTransactionDbContext(sqlConnection, _compiledModel, contextOwnsConnection: true))
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);

                        // Implicit transaction committed. 1 entity expected.
                        Assert.Equal(1, LogEntriesCount());
                        Assert.Equal(ConnectionState.Open, ctx.Database.Connection.State);
                    }

                    Assert.Equal(ConnectionState.Closed, sqlConnection.State);
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_created_when_using_DbContext_and_no_transaction_created_by_user_and_SqlConnection_is_opened_and_context_does_not_own_connection()
        {
            try
            {
                using (var sqlConnection = new SqlConnection(_connectionString))
                {
                    sqlConnection.Open();

                    using (var ctx = CreateTransactionDbContext(sqlConnection, _compiledModel, contextOwnsConnection: false))
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);

                        // Implicit transaction committed. 1 entity expected.
                        Assert.Equal(1, LogEntriesCount());
                    }

                    Assert.Equal(ConnectionState.Open, sqlConnection.State);
                }
            }
            finally
            {
                ResetTables();
            }
        }

        #region Database.Transaction tests

        [Fact]
        public void Passing_null_to_UseStoreTransaction_Clears_Current_Transaction()
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();

                using (var sqlTransaction = sqlConnection.BeginTransaction())
                {
                    using (var ctx = CreateTransactionDbContext(sqlConnection, _compiledModel, contextOwnsConnection: false))
                    {
                        // set up EntityConnection with a transaction which we test we can subsequently clear below
                        ctx.Database.UseTransaction(sqlTransaction);
                        var entityConnection = (EntityConnection)(((IObjectContextAdapter)ctx).ObjectContext).Connection;
                        Assert.NotNull(GetCurrentEntityTransaction(entityConnection));

                        ctx.Database.UseTransaction(null);
                        Assert.Null(GetCurrentEntityTransaction(entityConnection));
                    }
                }
            }
        }

        [Fact]
        public void Entity_Framework_uses_transaction_set_by_using_Database_BeginTransaction()
        {
            using (var ctx = CreateTransactionDbContext())
            {
                var entityConnection = (EntityConnection)(((IObjectContextAdapter)ctx).ObjectContext).Connection;
                Assert.Null(GetCurrentEntityTransaction(entityConnection));
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    Assert.Equal(transaction.StoreTransaction, GetCurrentStoreTransaction(entityConnection));
                }
            }
        }

        [Fact]
        public void Verify_simple_commit_DbContextTransaction_works()
        {
            try
            {
                using (var ctx = CreateTransactionDbContext())
                {
                    Assert.Equal(ConnectionState.Closed, ctx.Database.Connection.State);
                    using (var txn = ctx.Database.BeginTransaction())
                    {
                        Assert.Equal(ConnectionState.Open, ctx.Database.Connection.State);
                        AddLogEntryToDatabase(ctx);
                        txn.Commit();
                    }

                    Assert.Equal(ConnectionState.Closed, ctx.Database.Connection.State);
                    Assert.Equal(1, LogEntriesCount());
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_simple_rollback_DbContextTransaction_works()
        {
            try
            {
                using (var ctx = CreateTransactionDbContext())
                {
                    Assert.Equal(ConnectionState.Closed, ctx.Database.Connection.State);
                    using (var txn = ctx.Database.BeginTransaction())
                    {
                        Assert.Equal(ConnectionState.Open, ctx.Database.Connection.State);
                        AddLogEntryToDatabase(ctx);
                        txn.Rollback();
                    }

                    Assert.Equal(ConnectionState.Closed, ctx.Database.Connection.State);
                    Assert.Equal(0, LogEntriesCount());
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Entity_Framework_uses_transaction_isolation_level_set_by_using_Database_BeginTransaction()
        {
            using (var ctx = CreateTransactionDbContext())
            {
                var entityConnection = (EntityConnection)(((IObjectContextAdapter)ctx).ObjectContext).Connection;
                Assert.Null(GetCurrentEntityTransaction(entityConnection));
                using (var transaction = ctx.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    Assert.Equal(transaction.StoreTransaction, GetCurrentStoreTransaction(entityConnection));
                    Assert.Equal(IsolationLevel.Serializable, GetCurrentStoreTransaction(entityConnection).IsolationLevel);
                }
            }
        }

        [Fact]
        public void Verify_SaveChanges_commits_when_using_UseTransaction_with_open_external_connection()
        {
            try
            {
                using (var sqlConnection = new SqlConnection(_connectionString))
                {
                    sqlConnection.Open();

                    using (var sqlTransaction = sqlConnection.BeginTransaction())
                    {
                        // add entry using the external SqlConnection
                        AddLogEntryUsingAdoNet(sqlConnection, sqlTransaction);

                        using (var ctx = CreateTransactionDbContext(sqlConnection, _compiledModel, contextOwnsConnection: false))
                        {
                            ctx.Database.UseTransaction(sqlTransaction);
                            // add entry using the DbContext
                            AddLogEntryToDatabase(ctx);
                            Assert.Equal<DbTransaction>(
                                sqlTransaction,
                                GetCurrentStoreTransaction(((EntityConnection)(((IObjectContextAdapter)ctx).ObjectContext).Connection)));
                        }

                        sqlTransaction.Commit();
                    }

                    // check that both entries were inserted on the DB
                    Assert.Equal(2, CountLogEntriesUsingAdoNet(sqlConnection));
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_SaveChanges_rolls_back_when_using_UseTransaction_with_open_external_connection()
        {
            try
            {
                using (var sqlConnection = new SqlConnection(_connectionString))
                {
                    sqlConnection.Open();

                    using (var sqlTransaction = sqlConnection.BeginTransaction())
                    {
                        // add entry using the external SqlConnection
                        AddLogEntryUsingAdoNet(sqlConnection, sqlTransaction);

                        using (var ctx = CreateTransactionDbContext(sqlConnection, _compiledModel, contextOwnsConnection: false))
                        {
                            ctx.Database.UseTransaction(sqlTransaction);
                            // add entry using the DbContext
                            AddLogEntryToDatabase(ctx);
                            Assert.Equal<DbTransaction>(
                                sqlTransaction,
                                GetCurrentStoreTransaction(((EntityConnection)(((IObjectContextAdapter)ctx).ObjectContext).Connection)));
                        }

                        sqlTransaction.Rollback();
                    }

                    // check that neither entry was inserted on the DB
                    Assert.Equal(0, CountLogEntriesUsingAdoNet(sqlConnection));
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_ExecuteStoreCommand_works_when_using_UseTransaction_with_open_external_connection()
        {
            try
            {
                using (var sqlConnection = new SqlConnection(_connectionString))
                {
                    sqlConnection.Open();

                    using (var sqlTransaction = sqlConnection.BeginTransaction())
                    {
                        using (var ctx = CreateTransactionDbContext(sqlConnection, _compiledModel, contextOwnsConnection: false))
                        {
                            ctx.Database.UseTransaction(sqlTransaction);

                            var objCtx = ((IObjectContextAdapter)ctx).ObjectContext;
                            var commandText = @"INSERT INTO TransactionLog Values(-1)";
                            Assert.Equal(1, objCtx.ExecuteStoreCommand(commandText));
                            Assert.Equal<DbTransaction>(
                                sqlTransaction,
                                GetCurrentStoreTransaction(((EntityConnection)(((IObjectContextAdapter)ctx).ObjectContext).Connection)));
                        }

                        sqlTransaction.Commit();
                    }

                    Assert.Equal(1, CountLogEntriesUsingAdoNet(sqlConnection));
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_ExecuteStoreCommand_works_if_external_transaction_is_already_committed()
        {
            try
            {
                using (var sqlConnection = new SqlConnection(_connectionString))
                {
                    sqlConnection.Open();

                    using (var sqlTransaction = sqlConnection.BeginTransaction())
                    {
                        using (var ctx = CreateTransactionDbContext(sqlConnection, _compiledModel, contextOwnsConnection: false))
                        {
                            ctx.Database.UseTransaction(sqlTransaction);

                            sqlTransaction.Commit();

                            Assert.Equal(0, CountLogEntriesUsingAdoNet(sqlConnection));

                            Assert.Null(
                                GetCurrentEntityTransaction(((EntityConnection)(((IObjectContextAdapter)ctx).ObjectContext).Connection)));

                            var objCtx = ((IObjectContextAdapter)ctx).ObjectContext;
                            var commandText = @"INSERT INTO TransactionLog Values(-1)";
                            Assert.Equal(1, objCtx.ExecuteStoreCommand(commandText));
                        }
                    }

                    Assert.Equal(1, CountLogEntriesUsingAdoNet(sqlConnection));
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_ExecuteStoreCommand_works_if_external_transaction_is_already_rolled_back()
        {
            try
            {
                using (var sqlConnection = new SqlConnection(_connectionString))
                {
                    sqlConnection.Open();

                    using (var sqlTransaction = sqlConnection.BeginTransaction())
                    {
                        using (var ctx = CreateTransactionDbContext(sqlConnection, _compiledModel, contextOwnsConnection: false))
                        {
                            ctx.Database.UseTransaction(sqlTransaction);

                            sqlTransaction.Rollback();

                            Assert.Equal(0, CountLogEntriesUsingAdoNet(sqlConnection));

                            Assert.Null(
                                GetCurrentEntityTransaction(((EntityConnection)(((IObjectContextAdapter)ctx).ObjectContext).Connection)));

                            var objCtx = ((IObjectContextAdapter)ctx).ObjectContext;
                            var commandText = @"INSERT INTO TransactionLog Values(-1)";
                            Assert.Equal(1, objCtx.ExecuteStoreCommand(commandText));
                        }
                    }

                    Assert.Equal(1, CountLogEntriesUsingAdoNet(sqlConnection));
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_can_commit_a_transaction_and_still_use_connection_without_transaction_afterwards()
        {
            try
            {
                using (var ctx = CreateTransactionDbContext())
                {
                    ctx.Database.Connection.Open();
                    using (var txn = ctx.Database.BeginTransaction())
                    {
                        AddLogEntryToDatabase(ctx);
                        txn.Commit();
                    }

                    Assert.Equal(ConnectionState.Open, ctx.Database.Connection.State);
                    Assert.Equal(1, LogEntriesCount());

                    AddLogEntryToDatabase(ctx);
                    Assert.Equal(ConnectionState.Open, ctx.Database.Connection.State);
                    Assert.Equal(2, LogEntriesCount());
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_can_rollback_a_transaction_and_still_use_connection_without_transaction_afterwards()
        {
            try
            {
                using (var ctx = CreateTransactionDbContext())
                {
                    ctx.Database.Connection.Open();
                    using (var txn = ctx.Database.BeginTransaction())
                    {
                        AddLogEntryToDatabase(ctx);
                        txn.Rollback();
                    }

                    Assert.Equal(ConnectionState.Open, ctx.Database.Connection.State);
                    Assert.Equal(0, LogEntriesCount());

                    AddLogEntryToDatabase(ctx);
                    Assert.Equal(ConnectionState.Open, ctx.Database.Connection.State);
                    Assert.Equal(1, LogEntriesCount());
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_SaveChanges_works_when_using_its_own_connection_to_open_transaction()
        {
            try
            {
                using (var ctx = CreateTransactionDbContext())
                {
                    ctx.Database.Connection.Open();
                    using (var txn = ctx.Database.Connection.BeginTransaction())
                    {
                        ctx.Database.UseTransaction(txn);
                        AddLogEntryToDatabase(ctx);
                        txn.Commit();
                    }

                    Assert.Equal(ConnectionState.Open, ctx.Database.Connection.State);
                    Assert.Equal(1, LogEntriesCount());
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_calling_UseTransaction_throws_when_passed_committed_transaction()
        {
            try
            {
                using (var sqlConnection = new SqlConnection(_connectionString))
                {
                    sqlConnection.Open();

                    var sqlTransaction = sqlConnection.BeginTransaction();
                    AddLogEntryUsingAdoNet(sqlConnection, sqlTransaction);
                    sqlTransaction.Commit();

                    using (var ctx = CreateTransactionDbContext(sqlConnection, _compiledModel, contextOwnsConnection: false))
                    {
                        Assert.Throws<InvalidOperationException>(() => ctx.Database.UseTransaction(sqlTransaction))
                              .ValidateMessage("DbContext_InvalidTransactionNoConnection");
                    }
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_calling_UseTransaction_throws_when_passed_rolled_back_transaction()
        {
            try
            {
                using (var sqlConnection = new SqlConnection(_connectionString))
                {
                    sqlConnection.Open();

                    var sqlTransaction = sqlConnection.BeginTransaction();
                    AddLogEntryUsingAdoNet(sqlConnection, sqlTransaction);
                    sqlTransaction.Rollback();

                    using (var ctx = CreateTransactionDbContext(sqlConnection, _compiledModel, contextOwnsConnection: false))
                    {
                        Assert.Throws<InvalidOperationException>(() => ctx.Database.UseTransaction(sqlTransaction))
                              .ValidateMessage("DbContext_InvalidTransactionNoConnection");
                    }
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_committed_DbContextTransaction_inside_completed_TransactionScope_commits()
        {
            try
            {
                using (var txnScope = new TransactionScope())
                {
                    using (var ctx = CreateTransactionDbContext())
                    {
                        Assert.Equal(ConnectionState.Closed, ctx.Database.Connection.State);
                        using (var txn = ctx.Database.BeginTransaction())
                        {
                            Assert.Equal(ConnectionState.Open, ctx.Database.Connection.State);
                            AddLogEntryToDatabase(ctx);
                            txn.Commit();
                        }

                        Assert.Equal(ConnectionState.Closed, ctx.Database.Connection.State);
                    }

                    txnScope.Complete();
                }

                Assert.Equal(1, LogEntriesCount());
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_committed_DbContextTransaction_inside_uncompleted_TransactionScope_rolls_back()
        {
            try
            {
                using (new TransactionScope())
                {
                    using (var ctx = CreateTransactionDbContext())
                    {
                        Assert.Equal(ConnectionState.Closed, ctx.Database.Connection.State);
                        using (var txn = ctx.Database.BeginTransaction())
                        {
                            Assert.Equal(ConnectionState.Open, ctx.Database.Connection.State);
                            AddLogEntryToDatabase(ctx);
                            txn.Commit();
                        }

                        Assert.Equal(ConnectionState.Closed, ctx.Database.Connection.State);
                    }

                    // here do not call txnScope.Complete() - so TransactionScope will roll back
                }

                Assert.Equal(0, LogEntriesCount());
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_rolled_back_DbContextTransaction_inside_completed_TransactionScope_throws_TransactionAbortedException()
        {
            try
            {
                Assert.Throws<TransactionAbortedException>(
                    () =>
                        {
                            using (var txnScope = new TransactionScope())
                            {
                                using (var ctx = CreateTransactionDbContext())
                                {
                                    Assert.Equal(ConnectionState.Closed, ctx.Database.Connection.State);
                                    using (var txn = ctx.Database.BeginTransaction())
                                    {
                                        Assert.Equal(ConnectionState.Open, ctx.Database.Connection.State);
                                        AddLogEntryToDatabase(ctx);
                                        txn.Rollback();
                                    }

                                    Assert.Equal(ConnectionState.Closed, ctx.Database.Connection.State);
                                }

                                txnScope.Complete();
                            }
                        }
                    );
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_rolled_back_DbContextTransaction_inside_uncompleted_TransactionScope_rolls_back()
        {
            try
            {
                using (new TransactionScope())
                {
                    using (var ctx = CreateTransactionDbContext())
                    {
                        Assert.Equal(ConnectionState.Closed, ctx.Database.Connection.State);
                        using (var txn = ctx.Database.BeginTransaction())
                        {
                            Assert.Equal(ConnectionState.Open, ctx.Database.Connection.State);
                            AddLogEntryToDatabase(ctx);
                            txn.Rollback();
                        }

                        Assert.Equal(ConnectionState.Closed, ctx.Database.Connection.State);
                    }

                    // here do not call txnScope.Complete() - so TransactionScope will roll back
                }

                Assert.Equal(0, LogEntriesCount());
            }
            finally
            {
                ResetTables();
            }
        }

        #endregion

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_using_DbContext_and_user_creates_transaction_using_TransactionScope_and_EntityConnection_is_closed()
        {
            var entityConnection = new EntityConnection(_entityConnectionString);
            using (new TransactionScope())
            {
                using (var ctx = CreateTransactionDbContext(entityConnection, true))
                {
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);
                }
            }

            // "Transaction not commited. No entities should be saved to the database."
            Assert.Equal(0, LogEntriesCount());
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_using_DbContext_and_user_creates_transaction_using_TransactionScope_and_SqlConnection_is_closed()
        {
            var connection = new SqlConnection(_connectionString);
            using (new TransactionScope())
            {
                using (var ctx = CreateTransactionDbContext(connection, _compiledModel, contextOwnsConnection: true))
                {
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);
                }
            }

            // "Transaction not commited. No entities should be saved to the database."
            Assert.Equal(0, LogEntriesCount());
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_using_DbContext_and_user_creates_transaction_using_TransactionScope_and_EntityConnection_is_opened_inside_transaction_scope()
        {
            var connection = new EntityConnection(_entityConnectionString);
            using (new TransactionScope())
            {
                connection.Open();
                using (var ctx = CreateTransactionDbContext(connection, contextOwnsConnection: true))
                {
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);
                }
            }

            // "Transaction not commited. No entities should be saved to the database."
            Assert.Equal(0, LogEntriesCount());
        }

        // What should be the correct behavior for this? See Issue #777 for details
        ////[Fact]
        ////public void Verify_implicit_transaction_is_not_created_when_using_DbContext_and_user_creates_transaction_using_TransactionScope_and_EntityConnection_is_opened_outside_transaction_scope()
        ////{
        ////    try
        ////    {
        ////        var entityConnection = new EntityConnection(connectionString);
        ////        entityConnection.Open();
        ////        using (var transactionScope = new TransactionScope())
        ////        {
        ////            using (var ctx = CreateTransactionDbContext(entityConnection, contextOwnsConnection: true))
        ////            {
        ////                var transactionLogEntry = AddLogEntryToDatabase(ctx);
        ////                Assert.Equal(1, transactionLogEntry.TransactionCount);
        ////            }
        ////        }

        ////        // "Transaction not commited. No entities should be saved to the database."
        ////        Assert.Equal(0, LogEntriesCount());
        ////    }
        ////    finally
        ////    {
        ////        ResetTables();
        ////    }
        ////}

        [Fact]
        public void Verify_implicit_transaction_is_created_when_no_transaction_created_by_user_and_connection_is_closed()
        {
            try
            {
                using (var ctx = CreateTransactionContext())
                {
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);

                    // Implicit transaction committed. 1 entity expected.
                    Assert.Equal(1, LogEntriesCount());
                    Assert.Equal(ConnectionState.Closed, ctx.Connection.State);
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_created_when_no_transaction_created_by_user_and_connection_is_created_outside_context_and_is_closed()
        {
            try
            {
                var connection = new EntityConnection(_entityConnectionString);
                using (var ctx = CreateTransactionContext(connection))
                {
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);

                    // Implicit transaction committed. 1 entity expected.
                    Assert.Equal(1, LogEntriesCount(connection));
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
                    Assert.Equal(1, LogEntriesCount());
                    Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    ctx.Connection.Close();
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_created_when_no_transaction_created_by_user_and_connection_is_created_outside_context_and_is_open()
        {
            try
            {
                using (var connection = new EntityConnection(_entityConnectionString))
                {
                    connection.Open();
                    using (var ctx = CreateTransactionContext(connection))
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);

                        // Implicit transaction committed. 1 entity expected.
                        Assert.Equal(1, LogEntriesCount(connection));
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    }
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_creates_transaction_using_TransactionScope_and_connection_is_closed()
        {
            using (new TransactionScope())
            {
                using (var ctx = CreateTransactionContext())
                {
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);
                    Assert.Equal(ConnectionState.Closed, ctx.Connection.State);
                }
            }

            // Transaction not commited. No entities should be saved to the database.
            Assert.Equal(0, LogEntriesCount());
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_creates_transaction_using_TransactionScope_and_connection_created_outside_context_and_is_closed()
        {
            using (new TransactionScope())
            {
                var connection = new EntityConnection(_entityConnectionString);
                using (var ctx = CreateTransactionContext(connection))
                {
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);
                    Assert.Equal(ConnectionState.Closed, ctx.Connection.State);
                }
            }

            // Transaction not commited. No entities should be saved to the database.
            Assert.Equal(0, LogEntriesCount());
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_creates_transaction_using_TransactionScope_and_connection_created_outside_context_and_is_closed_plus_AdoNet_calls()
        {
            using (new TransactionScope())
            {
                var connection = new EntityConnection(_entityConnectionString);
                AddLogEntryUsingAdoNet((SqlConnection)connection.StoreConnection);

                using (var ctx = CreateTransactionContext(connection))
                {
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);
                    Assert.Equal(ConnectionState.Closed, ctx.Connection.State);
                }
            }

            // Transaction not commited. No entities should be saved to the database.
            Assert.Equal(0, LogEntriesCount());
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_creates_transaction_using_TransactionScope_and_connection_created_outside_transaction_scope_and_is_closed()
        {
            var connection = new EntityConnection(_entityConnectionString);
            using (new TransactionScope())
            {
                using (var ctx = CreateTransactionContext(connection))
                {
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);
                    Assert.Equal(ConnectionState.Closed, ctx.Connection.State);
                }
            }

            // Transaction not commited. No entities should be saved to the database.
            Assert.Equal(0, LogEntriesCount());
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_creates_transaction_using_TransactionScope_and_connection_created_outside_transaction_scope_and_is_closed_plus_AdoNet_calls()
        {
            var connection = new EntityConnection(_entityConnectionString);
            using (new TransactionScope())
            {
                AddLogEntryUsingAdoNet((SqlConnection)connection.StoreConnection);
                using (var ctx = CreateTransactionContext(connection))
                {
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);
                    Assert.Equal(ConnectionState.Closed, ctx.Connection.State);
                }
            }

            // Transaction not commited. No entities should be saved to the database.
            Assert.Equal(0, LogEntriesCount());
        }

        [Fact]
        public void Verify_only_one_transaction_created_when_user_creates_transaction_using_TransactionScope_and_connection_created_outside_transaction_scope_and_is_closed_plus_AdoNet_calls_and_transaction_is_completed()
        {
            try
            {
                var connection = new EntityConnection(_entityConnectionString);
                using (var transactionScope = new TransactionScope())
                {
                    AddLogEntryUsingAdoNet((SqlConnection)connection.StoreConnection);
                    using (var ctx = CreateTransactionContext(connection))
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        transactionScope.Complete();
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Closed, ctx.Connection.State);
                    }
                }

                // verify that there are two entries (one for AdoNet and one for EF, but only one transaction
                using (var ctx = CreateTransactionContext())
                {
                    var transactionCountEntries = ctx.LogEntries.Select(e => e.TransactionCount).OrderBy(c => c).ToList();
                    Assert.Equal(2, transactionCountEntries.Count());
                    Assert.Equal(-1, transactionCountEntries[0]);
                    Assert.Equal(1, transactionCountEntries[1]);
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_creates_transaction_using_TransactionScope_and_connection_is_open()
        {
            using (new TransactionScope())
            {
                using (var ctx = CreateTransactionContext())
                {
                    ctx.Connection.Open();
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);
                    Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    ctx.Connection.Close();
                }
            }

            // Transaction not commited. No entities should be saved to the database.
            Assert.Equal(0, LogEntriesCount());
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_creates_transaction_using_TransactionScope_and_connection_created_outside_context_and_is_open()
        {
            using (new TransactionScope())
            {
                using (var connection = new EntityConnection(_entityConnectionString))
                {
                    connection.Open();
                    using (var ctx = CreateTransactionContext(connection))
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    }
                }
            }

            // Transaction not commited. No entities should be saved to the database.
            Assert.Equal(0, LogEntriesCount());
        }

        // What should be the correct behavior for this? See Issue #777 for details
        ////[Fact]
        ////public void Verify_implicit_transaction_is_not_created_when_user_creates_transaction_using_TransactionScope_and_connection_created_and_opened_outside_transaction_scope()
        ////{
        ////    try
        ////    {
        ////        var connection = new EntityConnection(connectionString);
        ////        connection.Open();
        ////        using (var transactionScope = new TransactionScope())
        ////        {
        ////            using (var ctx = CreateTransactionContext(connection))
        ////            {
        ////                var transactionLogEntry = AddLogEntryToDatabase(ctx);
        ////                Assert.Equal(1, transactionLogEntry.TransactionCount);
        ////                Assert.Equal(ConnectionState.Open, ctx.Connection.State);
        ////            }
        ////        }

        ////        // Transaction not commited. No entities should be saved to the database.
        ////        Assert.Equal(0, CreateTransactionContext().LogEntries.Count());
        ////    }
        ////    finally
        ////    {
        ////        ResetTables();
        ////    }
        ////}

        // What should be the correct behavior for this? See Issue #777 for details
        ////[Fact]
        ////public void Verify_implicit_transaction_is_not_created_when_user_creates_transaction_using_TransactionScope_and_connection_created_and_opened_outside_transaction_scope_plus_AdoNet_calls()
        ////{
        ////    try
        ////    {
        ////        var connection = new EntityConnection(connectionString);
        ////        connection.Open();
        ////        using (var transactionScope = new TransactionScope())
        ////        {
        ////            // need to enlist store connection so that AdoNet calls execute inside a transaction
        ////            connection.StoreConnection.EnlistTransaction(Transaction.Current);
        ////            AddLogEntryUsingAdoNet((SqlConnection)connection.StoreConnection);
        ////            using (var ctx = CreateTransactionContext(connection))
        ////            {
        ////                var transactionLogEntry = AddLogEntryToDatabase(ctx);
        ////                Assert.Equal(1, transactionLogEntry.TransactionCount);
        ////                Assert.Equal(ConnectionState.Open, ctx.Connection.State);
        ////            }
        ////        }

        ////        // Transaction not commited. No entities should be saved to the database.
        ////        Assert.Equal(0, LogEntriesCount());
        ////    }
        ////    finally
        ////    {
        ////        ResetTables();
        ////    }
        ////}

        // What should be the correct behavior for this? See Issue #777 for details
        ////[Fact]
        ////public void Verify_only_one_transaction_created_when_user_creates_transaction_using_TransactionScope_and_connection_created_outside_transaction_scope_and_is_open_plus_AdoNet_calls_and_transaction_is_completed()
        ////{
        ////    try
        ////    {
        ////        var connection = new EntityConnection(connectionString);
        ////        connection.Open();
        ////        using (var transactionScope = new TransactionScope())
        ////        {
        ////            // need to enlist store connection so that AdoNet calls execute inside a transaction
        ////            connection.StoreConnection.EnlistTransaction(Transaction.Current);
        ////            AddLogEntryUsingAdoNet((SqlConnection)connection.StoreConnection);
        ////            using (var ctx = CreateTransactionContext(connection))
        ////            {
        ////                var transactionLogEntry = AddLogEntryToDatabase(ctx);
        ////                transactionScope.Complete();
        ////                Assert.Equal(1, transactionLogEntry.TransactionCount);
        ////                Assert.Equal(ConnectionState.Open, ctx.Connection.State);
        ////            }
        ////        }

        ////        // verify that there are two entries (one for AdoNet and one for EF, but only one transaction
        ////        using (var ctx = CreateTransactionContext())
        ////        {
        ////            var transactionCountEntries = ctx.LogEntries.Select(e => e.TransactionCount).OrderBy(c => c).ToList();
        ////            Assert.Equal(2, transactionCountEntries.Count());
        ////            Assert.Equal(-1, transactionCountEntries[0]);
        ////            Assert.Equal(1, transactionCountEntries[1]);
        ////        }
        ////    }
        ////    finally
        ////    {
        ////        ResetTables();
        ////    }
        ////}

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_executing_multiple_operations_in_the_same_TransactionScope()
        {
            using (new TransactionScope())
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
                    ctx.Connection.Close();
                }
            }

            // "Transaction not commited. No entities should be saved to the database."
            Assert.Equal(0, LogEntriesCount());
        }

        // What should be the correct behavior for this? See Issue #777 for details
        ////[Fact]
        ////public void Verify_implicit_transaction_is_not_created_when_executing_multiple_operations_in_the_same_TransactionScope_and_connection_is_opened_outside_transaction_scope()
        ////{
        ////    try
        ////    {
        ////        var connection = new EntityConnection(connectionString);
        ////        connection.Open();
        ////        using (var transactionScope = new TransactionScope())
        ////        {
        ////            using (var ctx = CreateTransactionContext(connection))
        ////            {
        ////                var transactionLogEntry = AddLogEntryToDatabase(ctx);
        ////                Assert.Equal(1, transactionLogEntry.TransactionCount);
        ////                Assert.Equal(ConnectionState.Open, ctx.Connection.State);

        ////                transactionLogEntry = AddLogEntryToDatabase(ctx);
        ////                Assert.Equal(1, transactionLogEntry.TransactionCount);
        ////                Assert.Equal(ConnectionState.Open, ctx.Connection.State);
        ////            }
        ////        }

        ////        // "Transaction not commited. No entities should be saved to the database."
        ////        Assert.Equal(0, LogEntriesCount());
        ////    }
        ////    finally
        ////    {
        ////        ResetTables();
        ////    }
        ////}

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_creates_transaction_explicitly_using_CommittableTransaction()
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
                ctx.Connection.Close();
            }

            // "Transaction not commited. No entities should be saved to the database."
            Assert.Equal(0, LogEntriesCount());
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_enlists_TransactionScope()
        {
            using (var ctx = CreateTransactionContext())
            {
                ctx.Connection.Open();
                using (new TransactionScope())
                {
                    ctx.Connection.EnlistTransaction(Transaction.Current);

                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);
                    Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                }
                ctx.Connection.Close();
            }

            // "Transaction not commited. No entities should be saved to the database."
            Assert.Equal(0, LogEntriesCount());
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_creates_ambient_CommittableTransaction()
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
                ctx.Connection.Close();
            }

            // Transaction not commited. No entities should be saved to the database.
            Assert.Equal(0, LogEntriesCount());
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_starts_DbTransaction_explicitly()
        {
            using (var ctx = CreateTransactionContext())
            {
                ctx.Connection.Open();
                using (ctx.Connection.BeginTransaction())
                {
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);
                    Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                }
                ctx.Connection.Close();
            }

            // "Transaction not commited. No entities should be saved to the database."
            Assert.Equal(0, LogEntriesCount());
        }

        [Fact]
        public void Verify_DbTransaction_cannot_be_mixed_with_TransactionScope()
        {
            try
            {
                using (var ctx = CreateTransactionContext())
                {
                    ctx.Connection.Open();
                    using (ctx.Connection.BeginTransaction())
                    {
                        using (new TransactionScope())
                        {
                            var transactionLogEntry = new TransactionLogEntry();
                            ctx.AddObject("Entities.TransactionLog", transactionLogEntry);

                            Assert.Throws<EntityException>(() => ctx.SaveChanges())
                                  .ValidateMessage("EntityClient_ProviderSpecificError", "EnlistTransaction");
                        }
                    }
                    ctx.Connection.Close();
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
                    using (ctx.Connection.BeginTransaction())
                    {
                        using (var committableTransaction = new CommittableTransaction())
                        {
                            Assert.Throws<EntityException>(() => ctx.Connection.EnlistTransaction(committableTransaction))
                                  .ValidateMessage("EntityClient_ProviderSpecificError", "EnlistTransaction");
                        }
                    }
                    ctx.Connection.Close();
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
            using (var ctx = CreateTransactionContext())
            {
                using (new TransactionScope())
                {
                    ctx.Connection.Open();
                    using (ctx.Connection.BeginTransaction())
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(2, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    }
                    ctx.Connection.Close();
                }
            }

            // "Transaction not commited. No entities should be saved to the database."
            Assert.Equal(0, LogEntriesCount());
        }

        // What should be the correct behavior for this? See Issue #777 for details
        ////[Fact]
        ////public void Verify_using_TransactionScope_with_DbTransaction_results_in_nested_transaction_and_implicit_transaction_not_created_when_using_context_with_already_opened_connection()
        ////{
        ////    try
        ////    {
        ////        var connection = new EntityConnection(connectionString);
        ////        connection.Open();
        ////        using (var ctx = CreateTransactionContext(connection))
        ////        {
        ////            using (var transactionScope = new TransactionScope())
        ////            {
        ////                // need to enlist connection here, otherwise test will fail
        ////                ctx.Connection.EnlistTransaction(Transaction.Current);
        ////                using (var dbTransaction = ctx.Connection.BeginTransaction())
        ////                {
        ////                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
        ////                    Assert.Equal(2, transactionLogEntry.TransactionCount);
        ////                    Assert.Equal(ConnectionState.Open, ctx.Connection.State);
        ////                }
        ////            }
        ////        }

        ////        // "Transaction not commited. No entities should be saved to the database."
        ////        Assert.Equal(0, LogEntriesCount());
        ////    }
        ////    finally
        ////    {
        ////        ResetTables();
        ////    }
        ////}

        [Fact]
        public void Verify_using_CommittableTransaction_with_DbTransaction_results_in_nested_transaction_and_implicit_transaction_not_created()
        {
            using (var ctx = CreateTransactionContext())
            {
                ctx.Connection.Open();
                using (var committableTransaction = new CommittableTransaction())
                {
                    ctx.Connection.EnlistTransaction(committableTransaction);

                    using (ctx.Connection.BeginTransaction())
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(2, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    }
                }
                ctx.Connection.Close();
            }

            // "Transaction not commited. No entities should be saved to the database."
            Assert.Equal(0, LogEntriesCount());
        }

        [Fact]
        public void Verify_TransactionScope_cannot_be_mixed_with_CommitableTransaction()
        {
            try
            {
                using (var ctx = CreateTransactionContext())
                {
                    using (new TransactionScope())
                    {
                        ctx.Connection.Open();
                        using (var committableTransaction = new CommittableTransaction())
                        {
                            Assert.Throws<EntityException>(() => ctx.Connection.EnlistTransaction(committableTransaction))
                                  .ValidateMessage("EntityClient_ProviderSpecificError", "EnlistTransaction");
                        }
                        ctx.Connection.Close();
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
                        using (new TransactionScope())
                        {
                            var transactionLogEntry = new TransactionLogEntry();
                            ctx.AddObject("Entities.TransactionLog", transactionLogEntry);

                            Assert.Throws<EntityException>(() => ctx.SaveChanges())
                                  .ValidateMessage("EntityClient_ProviderSpecificError", "EnlistTransaction");
                        }
                    }
                    ctx.Connection.Close();
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
                    using (new TransactionScope())
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    }

                    // "Transaction not commited. No entities should be saved to the database."
                    Assert.Equal(0, LogEntriesCount());

                    AddLogEntryToDatabase(ctx);

                    // "Implicit transaction committed. 1 entity expected."
                    Assert.Equal(1, LogEntriesCount());
                    Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    ctx.Connection.Close();
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_no_implicit_transaction_created_when_if_enlisted_in_explicit_transaction_after_transaction_from_previous_operation_disposed()
        {
            using (var ctx = CreateObjectContext())
            {
                ctx.Connection.Open();
                using (new TransactionScope())
                {
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);
                    Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                }

                // "Transaction not commited. No entities should be saved to the database."
                Assert.Equal(0, LogEntriesCount());

                using (var committableTransaction = new CommittableTransaction())
                {
                    ctx.Connection.EnlistTransaction(committableTransaction);
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);
                    Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                }

                // "Transaction not commited. No entities should be saved to the database."
                Assert.Equal(0, LogEntriesCount());
                ctx.Connection.Close();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_created_when_transaction_from_previous_operation_disposed_and_connection_closed()
        {
            try
            {
                using (var ctx = CreateObjectContext())
                {
                    using (new TransactionScope())
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Closed, ctx.Connection.State);
                    }

                    // "Transaction not commited. No entities should be saved to the database."
                    Assert.Equal(0, LogEntriesCount());

                    AddLogEntryToDatabase(ctx);

                    // "Implicit transaction committed. 1 entity expected."
                    Assert.Equal(1, LogEntriesCount());
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

                    AddLogEntryToDatabase(ctx);

                    // "Implicit transaction committed. 1 entity expected."
                    Assert.Equal(1, CreateTransactionContext().LogEntries.Count());
                    ctx.Connection.Close();
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
            using (var ctx = CreateObjectContext())
            {
                ctx.Connection.Open();

                using (new TransactionScope())
                {
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);
                    Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                }

                using (new TransactionScope())
                {
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);
                    Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                }
                ctx.Connection.Close();
            }

            // "Transaction not commited. No entities should be saved to the database."
            Assert.Equal(0, LogEntriesCount());
        }

        [Fact]
        public void Verify_no_implicit_transaction_created_when_enlisting_in_transaction_between_requests()
        {
            using (var ctx = CreateObjectContext())
            {
                ctx.Connection.Open();

                using (new TransactionScope())
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
                ctx.Connection.Close();
            }

            // "Transaction not commited. No entities should be saved to the database."
            Assert.Equal(0, LogEntriesCount());
        }

        [Fact]
        public void Verify_no_implicit_transaction_created_when_transaction_change_and_connection_is_closed_between_requests()
        {
            using (var ctx = CreateObjectContext())
            {
                ctx.Connection.Open();

                using (new TransactionScope())
                {
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);
                    ctx.Connection.Close();
                }

                using (new TransactionScope())
                {
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);
                    Assert.Equal(ConnectionState.Closed, ctx.Connection.State);
                }
                ctx.Connection.Close();
            }

            // "Transaction not commited. No entities should be saved to the database."
            Assert.Equal(0, LogEntriesCount());
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

                            Assert.Throws<EntityException>(() => ctx.Connection.EnlistTransaction(newCommittableTransaction))
                                  .ValidateMessage("EntityClient_ProviderSpecificError", "EnlistTransaction");
                        }
                    }
                    ctx.Connection.Close();
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
                    ctx.Connection.Close();
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void ExecuteSqlCommand_by_default_uses_transaction()
        {
            try
            {
                using (var ctx = CreateTransactionDbContext())
                {
                    ctx.Database.ExecuteSqlCommand("[dbo].[TransactionLogEntry_Insert]");
                    var transactionCount = ctx.LogEntries.Single().TransactionCount;

                    Assert.Equal(1, transactionCount);
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void ExecuteSqlCommand_with_TransactionalBehavior_EnsureTransaction_uses_transaction()
        {
            try
            {
                using (var ctx = CreateTransactionDbContext())
                {
                    ctx.Database.ExecuteSqlCommand(TransactionalBehavior.EnsureTransaction, "[dbo].[TransactionLogEntry_Insert]");
                    var transactionCount = ctx.LogEntries.Single().TransactionCount;

                    Assert.Equal(1, transactionCount);
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void ExecuteSqlCommand_with_TransactionalBehavior_DoNotEnsureTransaction_does_not_use_transaction()
        {
            try
            {
                using (var ctx = CreateTransactionDbContext())
                {
                    ctx.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, "[dbo].[TransactionLogEntry_Insert]");
                    var transactionCount = ctx.LogEntries.Single().TransactionCount;

                    Assert.Equal(0, transactionCount);
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void ExecuteSqlCommand_with_TransactionalBehavior_DoNotEnsureTransaction_still_uses_transaction_if_called_inside_transaction_scope()
        {
            try
            {
                using (new TransactionScope())
                {
                    using (var ctx = CreateTransactionDbContext())
                    {
                        ctx.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, "[dbo].[TransactionLogEntry_Insert]");
                        var transactionCount = ctx.LogEntries.Single().TransactionCount;

                        Assert.Equal(1, transactionCount);
                    }
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void ExecuteSqlCommand_with_TransactionalBehavior_DoNotEnsureTransaction_still_uses_transaction_if_called_inside_user_transaction()
        {
            try
            {
                using (var ctx = CreateTransactionDbContext())
                {
                    ctx.Database.BeginTransaction();
                    ctx.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, "[dbo].[TransactionLogEntry_Insert]");
                    var transactionCount = ctx.LogEntries.Single().TransactionCount;

                    Assert.Equal(1, transactionCount);
                }
            }
            finally
            {
                ResetTables();
            }
        }

#if !NET40
        [Fact]
        public void ExecuteSqlCommandAsync_by_default_uses_transaction()
        {
            try
            {
                using (var ctx = CreateTransactionDbContext())
                {
                    ctx.Database.ExecuteSqlCommandAsync("[dbo].[TransactionLogEntry_Insert]").Wait();
                    var transactionCount = ctx.LogEntries.Single().TransactionCount;

                    Assert.Equal(1, transactionCount);
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void ExecuteSqlCommandAsync_with_TransactionalBehavior_EnsureTransaction_uses_transaction()
        {
            try
            {
                using (var ctx = CreateTransactionDbContext())
                {
                    ctx.Database.ExecuteSqlCommandAsync(TransactionalBehavior.EnsureTransaction, "[dbo].[TransactionLogEntry_Insert]").Wait();
                    var transactionCount = ctx.LogEntries.Single().TransactionCount;

                    Assert.Equal(1, transactionCount);
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void ExecuteSqlCommandAsync_with_TransactionalBehavior_DoNotEnsureTransaction_does_not_use_transaction()
        {
            try
            {
                using (var ctx = CreateTransactionDbContext())
                {
                    ctx.Database.ExecuteSqlCommandAsync(TransactionalBehavior.DoNotEnsureTransaction, "[dbo].[TransactionLogEntry_Insert]").Wait();
                    var transactionCount = ctx.LogEntries.Single().TransactionCount;

                    Assert.Equal(0, transactionCount);
                }
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void ExecuteSqlCommandAsync_with_TransactionalBehavior_DoNotEnsureTransaction_still_uses_transaction_if_called_inside_user_transaction()
        {
            try
            {
                using (var ctx = CreateTransactionDbContext())
                {
                    ctx.Database.BeginTransaction();
                    ctx.Database.ExecuteSqlCommandAsync(TransactionalBehavior.DoNotEnsureTransaction, "[dbo].[TransactionLogEntry_Insert]").Wait();
                    var transactionCount = ctx.LogEntries.Single().TransactionCount;

                    Assert.Equal(1, transactionCount);
                }
            }
            finally
            {
                ResetTables();
            }
        }
#endif

        // TODO: this is temporary, tests that use these internals should be in unit tests
        private EntityTransaction GetCurrentEntityTransaction(EntityConnection connection)
        {
            var propertyInfo = connection.GetType().GetProperty("CurrentTransaction", BindingFlags.Instance | BindingFlags.NonPublic);

            return (EntityTransaction)propertyInfo.GetValue(connection, null);
        }

        // TODO: this is temporary, tests that use these internals should be in unit tests
        private DbTransaction GetCurrentStoreTransaction(EntityConnection connection)
        {
            var entityTransaction = GetCurrentEntityTransaction(connection);
            var propertyInfo = entityTransaction.GetType().GetProperty("StoreTransaction", BindingFlags.Instance | BindingFlags.NonPublic);

            return (DbTransaction)propertyInfo.GetValue(entityTransaction, null);
        }

        private TransactionContext CreateTransactionContext()
        {
            var ctx = new TransactionContext(_entityConnectionString);
            ctx.MetadataWorkspace.LoadFromAssembly(Assembly.GetExecutingAssembly());

            return ctx;
        }

        private TransactionContext CreateTransactionContext(EntityConnection connection)
        {
            var ctx = new TransactionContext(connection);
            ctx.MetadataWorkspace.LoadFromAssembly(Assembly.GetExecutingAssembly());

            return ctx;
        }

        private ObjectContext CreateObjectContext()
        {
            var ctx = new ObjectContext(_entityConnectionString);
            ctx.MetadataWorkspace.LoadFromAssembly(Assembly.GetExecutingAssembly());

            return ctx;
        }

        private TransactionDbContext CreateTransactionDbContext()
        {
            var ctx = new TransactionDbContext(_entityConnectionString);

            return ctx;
        }

        private TransactionDbContext CreateTransactionDbContext(EntityConnection connection, bool contextOwnsConnection)
        {
            var ctx = new TransactionDbContext(connection, contextOwnsConnection);

            return ctx;
        }

        private TransactionDbContext CreateTransactionDbContext(
            SqlConnection connection, DbCompiledModel compiledModel, bool contextOwnsConnection)
        {
            var ctx = new TransactionDbContext(connection, compiledModel, contextOwnsConnection);

            return ctx;
        }

        private int LogEntriesCount()
        {
            using (var ctx = CreateTransactionContext())
            {
                return ctx.LogEntries.Count();
            }
        }

        private int LogEntriesCount(EntityConnection connection)
        {
            using (var ctx = CreateTransactionContext(connection))
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

        private TransactionLogEntry AddLogEntryToDatabase(TransactionDbContext ctx)
        {
            var transactionLogEntry = new TransactionLogEntry();
            ctx.LogEntries.Add(transactionLogEntry);
            ctx.SaveChanges();

            return transactionLogEntry;
        }

        private void AddLogEntryUsingAdoNet(SqlConnection connection, SqlTransaction sqlTransaction = null)
        {
            var shouldCloseConnection = false;
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
                shouldCloseConnection = true;
            }

            var command = new SqlCommand();
            command.Connection = connection;
            if (sqlTransaction != null)
            {
                command.Transaction = sqlTransaction;
            }
            command.CommandText = @"INSERT INTO TransactionLog Values(-1)";
            command.ExecuteNonQuery();

            if (shouldCloseConnection)
            {
                connection.Close();
            }
        }

        private int CountLogEntriesUsingAdoNet(SqlConnection connection, SqlTransaction sqlTransaction = null)
        {
            var shouldCloseConnection = false;
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
                shouldCloseConnection = true;
            }

            var command = new SqlCommand();
            command.Connection = connection;
            if (sqlTransaction != null)
            {
                command.Transaction = sqlTransaction;
            }
            command.CommandText = @"SELECT COUNT(*) FROM TransactionLog";
            var count = -1;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read()
                    && false == reader.IsDBNull(0))
                {
                    count = reader.GetInt32(0);
                }
            }

            if (shouldCloseConnection)
            {
                connection.Close();
            }

            return count;
        }

        /// <summary>
        ///     Removes all entries from the tables used for tests. Must be called from tests that are committing transactions
        ///     as each test expects that the db will be initially clean.
        /// </summary>
        private void ResetTables()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                new SqlCommand("DELETE FROM TransactionLog", connection).ExecuteNonQuery();
            }

            Assert.Equal(0, CreateTransactionContext().LogEntries.Count());
        }
    }

    public class TransactionFixture : FunctionalTestBase
    {
        public DbCompiledModel CompiledModel { get; private set; }
        public string ConnectionString { get; private set; }
        private const string DatabaseName = "TransactionTests";

        public TransactionFixture()
        {
            using (var masterConnection = new SqlConnection(ModelHelpers.SimpleConnectionString("master")))
            {
                masterConnection.Open();
                var createDatabaseScript = string.Format(
                    @"if exists(select * from sys.databases where name = '{0}')
drop database {0}
create database {0}", DatabaseName);
                new SqlCommand(createDatabaseScript, masterConnection).ExecuteNonQuery();
            }

            ConnectionString = ModelHelpers.SimpleConnectionString(DatabaseName);
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                new SqlCommand(
                    @"CREATE TABLE [dbo].[TransactionLog](
  [ID] [int] IDENTITY(1,1) NOT NULL,
  [TransactionCount] [int] NOT NULL,
CONSTRAINT [PK_TransactionLog] PRIMARY KEY CLUSTERED ([ID] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]) 
ON [PRIMARY]", connection).ExecuteNonQuery();

                new SqlCommand(
                    @"CREATE PROCEDURE [dbo].[CreateTransactionLogEntry] 
AS
BEGIN
  DECLARE @tranCount AS int = @@TRANCOUNT  -- assigning to a variable prevents from counting an implicit transaction created for insert
  INSERT INTO TransactionLog Values(@tranCount)
  SELECT ID, TransactionCount 
  FROM TransactionLog
  WHERE ID = SCOPE_IDENTITY()
END", connection).ExecuteNonQuery();

                new SqlCommand(
                    @"CREATE PROCEDURE [dbo].[TransactionLogEntry_Insert] 
AS
BEGIN
  DECLARE @tranCount AS int = @@TRANCOUNT
  INSERT INTO TransactionLog Values(@tranCount)
  SELECT ID, TransactionCount 
  FROM TransactionLog
  WHERE ID = SCOPE_IDENTITY()
END", connection).ExecuteNonQuery();

                var builder = new DbModelBuilder();
                builder.Entity<TransactionLogEntry>().ToTable("TransactionLog");
                builder.HasDefaultSchema("TransactionsModel");
                builder.HasDefaultSchema("Entities");
                builder.Conventions.Remove<ModelContainerConvention>();
                builder.Entity<TransactionLogEntry>()
                       .MapToStoredProcedures(c => c.Insert(i => i.HasName("TransactionLogEntry_Insert", "dbo")));

                var model = builder.Build(connection);
                CompiledModel = model.Compile();
            }
        }
    }
}
