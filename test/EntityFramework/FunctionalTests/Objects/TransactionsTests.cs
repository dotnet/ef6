// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Objects
{
    using System.Data.Common;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.SqlClient;
    using System.Diagnostics;
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

    public class TransactionsTests : IUseFixture<TransactionFixture>
    {
        private string connectionString;
        private string modelDirectory;

        private MetadataWorkspace workspace;
        private SqlConnection globalConnection;
        private DbCompiledModel compiledModel;

        public void SetFixture(TransactionFixture data)
        {
            globalConnection = data.GlobalConnection;
            compiledModel = data.CompiledModel;

            connectionString = string.Format(
                @"metadata=res://EntityFramework.FunctionalTests/System.Data.Entity.Objects.TransactionsModel.csdl|res://EntityFramework.FunctionalTests/System.Data.Entity.Objects.TransactionsModel.ssdl|res://EntityFramework.FunctionalTests/System.Data.Entity.Objects.TransactionsModel.msl;provider=System.Data.SqlClient;provider connection string=""{0}""",
                ModelHelpers.SimpleConnectionString("tempdb"));
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
                var entityConnection = new EntityConnection(connectionString);
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
                var entityConnection = new EntityConnection(connectionString);
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
                var sqlConnection = new SqlConnection(globalConnection.ConnectionString);
                sqlConnection.Open();

                using (var ctx = CreateTransactionDbContext(sqlConnection, compiledModel, contextOwnsConnection: true))
                {
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);

                    // Implicit transaction committed. 1 entity expected.
                    Assert.Equal(1, LogEntriesCount());
                    Assert.Equal(ConnectionState.Open, ctx.Database.Connection.State);
                }

                Assert.Equal(ConnectionState.Closed, sqlConnection.State);
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
                var sqlConnection = new SqlConnection(globalConnection.ConnectionString);
                sqlConnection.Open();

                using (var ctx = CreateTransactionDbContext(sqlConnection, compiledModel, contextOwnsConnection: false))
                {
                    var transactionLogEntry = AddLogEntryToDatabase(ctx);
                    Assert.Equal(1, transactionLogEntry.TransactionCount);

                    // Implicit transaction committed. 1 entity expected.
                    Assert.Equal(1, LogEntriesCount());
                }

                Assert.Equal(ConnectionState.Open, sqlConnection.State);
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_using_DbContext_and_user_creates_transaction_using_TransactionScope_and_EntityConnection_is_closed()
        {
            try
            {
                var entityConnection = new EntityConnection(connectionString);
                using (var transactionScope = new TransactionScope())
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
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_using_DbContext_and_user_creates_transaction_using_TransactionScope_and_SqlConnection_is_closed()
        {
            try
            {
                var connection = new SqlConnection(globalConnection.ConnectionString);
                using (var transactionScope = new TransactionScope())
                {
                    using (var ctx = CreateTransactionDbContext(connection, compiledModel, contextOwnsConnection: true))
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                    }
                }

                // "Transaction not commited. No entities should be saved to the database."
                Assert.Equal(0, LogEntriesCount());
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_using_DbContext_and_user_creates_transaction_using_TransactionScope_and_EntityConnection_is_opened_inside_transaction_scope()
        {
            try
            {
                var connection = new EntityConnection(connectionString);
                using (var transactionScope = new TransactionScope())
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
            finally
            {
                ResetTables();
            }
        }

        // Fails for now - MSDTC on server 'MAUMARDEV\SQLEXPRESS' is unavailable. See Issue #771 for details
        ////[Fact]
        ////public void Verify_implicit_transaction_is_not_created_when_using_DbContext_and_user_creates_transaction_using_TransactionScope_and_connection_is_opened_inside_transaction_scope()
        ////{
        ////    try
        ////    {
        ////        var connection = new SqlConnection(globalConnection.ConnectionString);
        ////        using (var transactionScope = new TransactionScope())
        ////        {
        ////            connection.Open();
        ////            using (var ctx = CreateTransactionDbContext(connection, compiledModel, contextOwnsConnection: true))
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
        public void Verify_implicit_transaction_is_not_created_when_using_DbContext_and_user_creates_transaction_using_TransactionScope_and_EntityConnection_is_opened_outside_transaction_scope()
        {
            try
            {
                var entityConnection = new EntityConnection(connectionString);
                entityConnection.Open();
                using (var transactionScope = new TransactionScope())
                {
                    using (var ctx = CreateTransactionDbContext(entityConnection, contextOwnsConnection: true))
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                    }
                }

                // "Transaction not commited. No entities should be saved to the database."
                Assert.Equal(0, LogEntriesCount());
            }
            finally
            {
                ResetTables();
            }
        }

        // Fails for now - MSDTC on server 'MAUMARDEV\SQLEXPRESS' is unavailable. See Issue #771 for details
        ////[Fact]
        ////public void Verify_implicit_transaction_is_not_created_when_using_DbContext_and_user_creates_transaction_using_TransactionScope_and_connection_is_opened_outside_transaction_scope()
        ////{
        ////    try
        ////    {
        ////        var connection = new SqlConnection(globalConnection.ConnectionString);

        ////        connection.Open();
        ////        using (var transactionScope = new TransactionScope())
        ////        {
        ////            using (var ctx = CreateTransactionDbContext(connection, compiledModel, contextOwnsConnection: true))
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
                var connection = new EntityConnection(connectionString);
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
                var connection = new EntityConnection(connectionString);
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
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_creates_transaction_using_TransactionScope_and_connection_is_closed()
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
                Assert.Equal(0, LogEntriesCount());
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_creates_transaction_using_TransactionScope_and_connection_created_outside_context_and_is_closed()
        {
            try
            {
                using (var transactionScope = new TransactionScope())
                {
                    var connection = new EntityConnection(connectionString);
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
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_creates_transaction_using_TransactionScope_and_connection_created_outside_context_and_is_closed_plus_AdoNet_calls()
        {
            try
            {
                using (var transactionScope = new TransactionScope())
                {
                    var connection = new EntityConnection(connectionString);
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
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_creates_transaction_using_TransactionScope_and_connection_created_outside_transaction_scope_and_is_closed()
        {
            try
            {
                var connection = new EntityConnection(connectionString);
                using (var transactionScope = new TransactionScope())
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
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_user_creates_transaction_using_TransactionScope_and_connection_created_outside_transaction_scope_and_is_closed_plus_AdoNet_calls()
        {
            try
            {
                var connection = new EntityConnection(connectionString);
                using (var transactionScope = new TransactionScope())
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
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_only_one_transaction_created_when_user_creates_transaction_using_TransactionScope_and_connection_created_outside_transaction_scope_and_is_closed_plus_AdoNet_calls_and_transaction_is_completed()
        {
            try
            {
                var connection = new EntityConnection(connectionString);
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
        public void Verify_implicit_transaction_is_not_created_when_user_creates_transaction_using_TransactionScope_and_connection_created_outside_context_and_is_open()
        {
            try
            {
                using (var transactionScope = new TransactionScope())
                {
                    var connection = new EntityConnection(connectionString);
                    connection.Open();
                    using (var ctx = CreateTransactionContext(connection))
                    {
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
        public void Verify_implicit_transaction_is_not_created_when_user_creates_transaction_using_TransactionScope_and_connection_created_and_opened_outside_transaction_scope()
        {
            try
            {
                var connection = new EntityConnection(connectionString);
                connection.Open();
                using (var transactionScope = new TransactionScope())
                {
                    using (var ctx = CreateTransactionContext(connection))
                    {
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
        public void Verify_implicit_transaction_is_not_created_when_user_creates_transaction_using_TransactionScope_and_connection_created_and_opened_outside_transaction_scope_plus_AdoNet_calls()
        {
            try
            {
                var connection = new EntityConnection(connectionString);
                connection.Open();
                using (var transactionScope = new TransactionScope())
                {
                    // need to enlist store connection so that AdoNet calls execute inside a transaction
                    connection.StoreConnection.EnlistTransaction(Transaction.Current);
                    AddLogEntryUsingAdoNet((SqlConnection)connection.StoreConnection);
                    using (var ctx = CreateTransactionContext(connection))
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    }
                }

                // Transaction not commited. No entities should be saved to the database.
                Assert.Equal(0, LogEntriesCount());
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_only_one_transaction_created_when_user_creates_transaction_using_TransactionScope_and_connection_created_outside_transaction_scope_and_is_open_plus_AdoNet_calls_and_transaction_is_completed()
        {
            try
            {
                var connection = new EntityConnection(connectionString);
                connection.Open();
                using (var transactionScope = new TransactionScope())
                {
                    // need to enlist store connection so that AdoNet calls execute inside a transaction
                    connection.StoreConnection.EnlistTransaction(Transaction.Current);
                    AddLogEntryUsingAdoNet((SqlConnection)connection.StoreConnection);
                    using (var ctx = CreateTransactionContext(connection))
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        transactionScope.Complete();
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);
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
                Assert.Equal(0, LogEntriesCount());
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_implicit_transaction_is_not_created_when_executing_multiple_operations_in_the_same_TransactionScope_and_connection_is_opened_outside_transaction_scope()
        {
            try
            {
                var connection = new EntityConnection(connectionString);
                connection.Open();
                using (var transactionScope = new TransactionScope())
                {
                    using (var ctx = CreateTransactionContext(connection))
                    {
                        var transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);

                        transactionLogEntry = AddLogEntryToDatabase(ctx);
                        Assert.Equal(1, transactionLogEntry.TransactionCount);
                        Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                    }
                }

                // "Transaction not commited. No entities should be saved to the database."
                Assert.Equal(0, LogEntriesCount());
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
                Assert.Equal(0, LogEntriesCount());
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
                Assert.Equal(0, LogEntriesCount());
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
                Assert.Equal(0, LogEntriesCount());
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
                Assert.Equal(0, LogEntriesCount());
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
                Assert.Equal(0, LogEntriesCount());
            }
            finally
            {
                ResetTables();
            }
        }

        [Fact]
        public void Verify_using_TransactionScope_with_DbTransaction_results_in_nested_transaction_and_implicit_transaction_not_created_when_using_context_with_already_opened_connection()
        {
            try
            {
                var connection = new EntityConnection(connectionString);
                connection.Open();
                using (var ctx = CreateTransactionContext(connection))
                {
                    using (var transactionScope = new TransactionScope())
                    {
                        // need to enlist connection here, otherwise test will fail
                        ctx.Connection.EnlistTransaction(Transaction.Current);
                        using (var dbTransaction = ctx.Connection.BeginTransaction())
                        {
                            var transactionLogEntry = AddLogEntryToDatabase(ctx);
                            Assert.Equal(2, transactionLogEntry.TransactionCount);
                            Assert.Equal(ConnectionState.Open, ctx.Connection.State);
                        }
                    }
                }

                // "Transaction not commited. No entities should be saved to the database."
                Assert.Equal(0, LogEntriesCount());
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
                Assert.Equal(0, LogEntriesCount());
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
                    Assert.Equal(0, LogEntriesCount());

                    AddLogEntryToDatabase(ctx);

                    // "Implicit transaction committed. 1 entity expected."
                    Assert.Equal(1, LogEntriesCount());
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
                    Assert.Equal(0, LogEntriesCount());

                    var newTransactionLogEntry = AddLogEntryToDatabase(ctx);

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

        private TransactionContext CreateTransactionContext(EntityConnection connection)
        {
            var ctx = new TransactionContext(connection);
            ctx.MetadataWorkspace.LoadFromAssembly(Assembly.GetExecutingAssembly());

            return ctx;
        }

        private ObjectContext CreateObjectContext()
        {
            var ctx = new ObjectContext(connectionString);
            ctx.MetadataWorkspace.LoadFromAssembly(Assembly.GetExecutingAssembly());

            return ctx;
        }

        private ObjectContext CreateObjectContext(EntityConnection connection)
        {
            var ctx = new ObjectContext(connection);
            ctx.MetadataWorkspace.LoadFromAssembly(Assembly.GetExecutingAssembly());

            return ctx;
        }

        private TransactionDbContext CreateTransactionDbContext()
        {
            var ctx = new TransactionDbContext(connectionString);

            return ctx;
        }

        private TransactionDbContext CreateTransactionDbContext(EntityConnection connection, bool contextOwnsConnection)
        {
            var ctx = new TransactionDbContext(connection, contextOwnsConnection);

            return ctx;
        }

        private TransactionDbContext CreateTransactionDbContext(SqlConnection connection, DbCompiledModel compiledModel, bool contextOwnsConnection)
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

        private void AddLogEntryUsingAdoNet(SqlConnection connection)
        {
            bool shouldCloseConnection = false;
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
                shouldCloseConnection = true;
            }

            var command = new SqlCommand();
            command.Connection = connection;
            command.CommandText = @"INSERT INTO ##TransactionLog Values(-1)";
            command.ExecuteNonQuery();

            if (shouldCloseConnection)
            {
                connection.Close();
            }
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
        public DbCompiledModel CompiledModel { get; private set; }

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

                CompiledModel = BuildCompiledModel();
            }
            catch
            {
                CleanupDatabase();

                throw;
            }
        }

        private DbCompiledModel BuildCompiledModel()
        {
            // hack - we have to create context that uses StoredProcedure to for inserts. DbModelBuilder does not support that.
            // Instead, we use reflection to inject MetadataWorkspace created using csdl/ssdl/msl into a DbCompiledModel
            var connectionString = string.Format(
                @"metadata=res://EntityFramework.FunctionalTests/System.Data.Entity.Objects.TransactionsModel.csdl|res://EntityFramework.FunctionalTests/System.Data.Entity.Objects.TransactionsModel.ssdl|res://EntityFramework.FunctionalTests/System.Data.Entity.Objects.TransactionsModel.msl;provider=System.Data.SqlClient;provider connection string=""{0}""",
                ModelHelpers.SimpleConnectionString("tempdb"));

            var ctx = new TransactionDbContext(connectionString);
            ctx.LogEntries.Count();
            var objectContext = ((IObjectContextAdapter)ctx).ObjectContext;

            DbModelBuilder builder = new DbModelBuilder();
            builder.Entity(typeof(TransactionLogEntry)).ToTable("##TransactionLog");
            builder.HasDefaultSchema("tempdbModel");
            builder.Conventions.Add(new ModelContainerConvention("Entities"));
                

            var model = builder.Build(ctx.Database.Connection);
            var compiledModel = model.Compile();
            var cachedMetadataWorkspaceFieldInfo = compiledModel.GetType().GetField("_workspace", BindingFlags.Instance | BindingFlags.NonPublic);
            var cachedMetadataWorkspace = cachedMetadataWorkspaceFieldInfo.GetValue(compiledModel);
            var metadataWorkspaceFieldInfo = cachedMetadataWorkspace.GetType().GetField("_metadataWorkspace", BindingFlags.Instance | BindingFlags.NonPublic);
            metadataWorkspaceFieldInfo.SetValue(cachedMetadataWorkspace, objectContext.MetadataWorkspace);

            return compiledModel;
        }

        private void CleanupDatabase()
        {
            Debug.Assert(GlobalConnection != null);

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
