// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System.Data;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.TestHelpers;
    using System.Linq;
    using System.Reflection;
    using System.Transactions;
    using SimpleModel;
    using Xunit;
    
    /// <summary>
    /// Tests for simple uses of transactions with DbContext.
    /// </summary>
    public class TransactionTests : FunctionalTestBase
    {
        #region Transaction scenarios

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Insert_In_SystemTransaction_without_commit_works_and_transaction_count_is_correct()
        {
            EnsureDatabaseInitialized(() => new SimpleModelContext());

            ExtendedSqlAzureExecutionStrategy.ExecuteNew(
                () =>
                {
                    using (new TransactionScope())
                    {
                        using (var context = new SimpleModelContext())
                        {
                            var product = new Product
                            {
                                Name = "Fanta"
                            };
                            context.Products.Add(product);
                            context.SaveChanges();

                            Assert.Equal(1, GetTransactionCount(context.Database.Connection));
                            Assert.True(context.Products.Where(p => p.Name == "Fanta").AsNoTracking().Any());
                        }
                    }
                });
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Explicit_rollback_can_be_used_to_rollback_a_transaction()
        {
            EnsureDatabaseInitialized(() => new SimpleModelContext());

            ExtendedSqlAzureExecutionStrategy.ExecuteNew(
                () =>
                {
                    using (var tx = new TransactionScope())
                    {
                        using (var context = new SimpleModelContext())
                        {
                            var product = new Product
                            {
                                Name = "BestTea"
                            };
                            context.Products.Add(product);
                            context.SaveChanges();

                            Assert.Equal(1, GetTransactionCount(context.Database.Connection));
                            Assert.True(context.Products.Where(p => p.Name == "BestTea").AsNoTracking().Any());

                            // Rollback System Transaction
                            tx.Dispose();

                            Assert.False(context.Products.Where(p => p.Name == "BestTea").AsNoTracking().Any());
                        }
                    }
                });
        }

        public class SimpleModelContextForCommit : SimpleModelContext
        {
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Transaction_with_explicit_commit_on_a_local_transaction_can_be_used()
        {
            // Since this test actually mutates the database outside of a transaction it needs
            // to use a special context and ensure that the database is deleted and created
            // each time the test is run.
            ExtendedSqlAzureExecutionStrategy.ExecuteNew(
                () =>
                {
                    using (var context = new SimpleModelContextForCommit())
                    {
                        context.Database.Delete();
                        context.Database.Create();

                        // Begin a local transaction
                        var transaction = BeginLocalTransaction(context);

                        var product = new Product
                        {
                            Name = "Fanta"
                        };
                        context.Products.Add(product);
                        context.SaveChanges();

                        // Commit local transaction
                        transaction.Commit();
                        CloseEntityConnection(context);
                    }
                });

            using (var context = new SimpleModelContextForCommit())
            {
                ExtendedSqlAzureExecutionStrategy.ExecuteNew(
                    () =>
                    {
                        Assert.True(context.Products.Any(p => p.Name == "Fanta"));
                    });
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Explicit_rollback_on_a_local_transaction_can_be_used_to_rollback_a_transaction()
        {
            EnsureDatabaseInitialized(() => new SimpleModelContext());

            using (var context = new SimpleModelContext())
            {
                ExtendedSqlAzureExecutionStrategy.ExecuteNew(
                    () =>
                    {
                        // Begin a local transaction
                        var transaction = BeginLocalTransaction(context);

                        var product = new Product
                        {
                            Name = "New Tea"
                        };
                        context.Products.Add(product);
                        context.SaveChanges();

                        Assert.True(context.Products.Where(p => p.Name == "New Tea").AsNoTracking().Any());

                        // Rollback local transaction
                        transaction.Rollback();
                        CloseEntityConnection(context);

                        Assert.False(context.Products.Where(p => p.Name == "New Tea").AsNoTracking().Any());

                    });
            }
        }

        public class SimpleModelContextForCommit2 : SimpleModelContext
        {
        }

        [Fact]
        public void Transaction_with_explicit_commit_on_commitable_transaction_can_be_used()
        {
            // Since this test actually mutates the database outside of a transaction it needs
            // to use a special context and ensure that the database is deleted and created
            // each time the test is run.
            using (var context = new SimpleModelContextForCommit2())
            {
                context.Database.Delete();
                context.Database.Create();

                var transaction = BeginCommittableTransaction(context);

                var product = new Product
                                  {
                                      Name = "Fanta"
                                  };
                context.Products.Add(product);
                context.SaveChanges();

                // Commit local transaction
                transaction.Commit();
                CloseEntityConnection(context);
            }

            using (var context = new SimpleModelContextForCommit2())
            {
                Assert.True(context.Products.Where(p => p.Name == "Fanta").Any());
            }
        }

        public class Issue1805Context : SimpleModelContext
        {
            public Issue1805Context(DbConnection connection, bool contextOwnsConnection)
                : base(connection, contextOwnsConnection)
            { }
        }

        [ExtendedFact(SkipForSqlAzure = true, Justification = "Unable to connect to master database with open connection on sqlAzure")]
        public void Issue1805_EntityConnection_is_not_subscribed_to_its_underlying_store_connection_event_after_it_has_been_disposed()
        {
            using (var ctx1 = new SimpleModelContext())
            {
                var connection = ctx1.Database.Connection;

                connection.Open();
                using (var txn = connection.BeginTransaction())
                {
                    var stateChangeEventHandlerField = typeof(DbConnection).GetFields(
                        BindingFlags.NonPublic | BindingFlags.Instance).SingleOrDefault(fi => fi.Name == "_stateChangeEventHandler");

                    using (var ctx2 = new Issue1805Context(connection, false))
                    {
                        ctx2.Database.UseTransaction(txn);
                        ctx2.Database.Initialize(force: false);

                        // Now look up by reflection the event handler on the initial connection.
                        // That event handler's invocation list should contain a delegate to EntityConnection's
                        // StoreConnectionStateChangeHandler method because it subscribed to that
                        // event when it was created.
                        var stateChangeEventHandler = (StateChangeEventHandler)stateChangeEventHandlerField.GetValue(connection);
                        Assert.NotEmpty(stateChangeEventHandler.GetInvocationList().Where(
                                    del => del.Target != null
                                           && del.Target.ToString() == "System.Data.Entity.Core.EntityClient.EntityConnection"
                                           && del.Method != null
                                           && del.Method.ToString().StartsWith("Void StoreConnectionStateChangeHandler")));
                    }

                    Assert.Equal(ConnectionState.Open, connection.State);

                    // Now look up the event handler on the initial connection again.
                    // That event handler should be null as the subscription to EntityConnection's
                    // StoreConnectionStateChangeHandler method (which would keep the EntityConnection
                    // alive which in turn would prevent the Issue1805Context from being garbage-collected)
                    // should have been unsubscribed during Dispose() on the EntityConnection.
                    Assert.Null(stateChangeEventHandlerField.GetValue(connection));
                }
            }
        }

        #endregion Transaction scenarios
    }
}
