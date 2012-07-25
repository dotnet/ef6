// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace ProductivityApiTests
{
    using System.Data.Entity;
    using System.Linq;
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
        public void Insert_In_SystemTransaction_without_commit_works_and_transaction_count_is_correct()
        {
            EnsureDatabaseInitialized(() => new SimpleModelContext());

            using (new TransactionScope())
            {
                using (var context = new SimpleModelContext())
                {
                    var product = new Product { Name = "Fanta" };
                    context.Products.Add(product);
                    context.SaveChanges();

                    Assert.Equal(1, GetTransactionCount(context.Database.Connection));
                    Assert.True(context.Products.Where(p => p.Name == "Fanta").AsNoTracking().Any());
                }
            }
        }

        [Fact]
        public void Explicit_rollback_can_be_used_to_rollback_a_transaction()
        {
            EnsureDatabaseInitialized(() => new SimpleModelContext());

            using (var tx = new TransactionScope())
            {
                using (var context = new SimpleModelContext())
                {
                    var product = new Product() { Name = "BestTea" };
                    context.Products.Add(product);
                    context.SaveChanges();

                    Assert.Equal(1, GetTransactionCount(context.Database.Connection));
                    Assert.True(context.Products.Where(p => p.Name == "BestTea").AsNoTracking().Any());

                    // Rollback System Transaction
                    tx.Dispose();

                    Assert.False(context.Products.Where(p => p.Name == "BestTea").AsNoTracking().Any());
                }
            }
        }

        public class SimpleModelContextForCommit : SimpleModelContext
        {
        }

        [Fact]
        public void Transaction_with_explicit_commit_on_a_local_transaction_can_be_used()
        {
            // Since this test actually mutates the database outside of a transaction it needs
            // to use a special context and ensure that the database is deleted and created
            // each time the test is run.
            using (var context = new SimpleModelContextForCommit())
            {
                context.Database.Delete();
                context.Database.Create();

                // Begin a local transaction
                var transaction = BeginLocalTransaction(context);

                var product = new Product { Name = "Fanta" };
                context.Products.Add(product);
                context.SaveChanges();

                // Commit local transaction
                transaction.Commit();
                CloseEntityConnection(context);
            }

            using (var context = new SimpleModelContextForCommit())
            {
                Assert.True(context.Products.Where(p => p.Name == "Fanta").Any());
            }
        }

        [Fact]
        public void Explicit_rollback_on_a_local_transaction_can_be_used_to_rollback_a_transaction()
        {
            EnsureDatabaseInitialized(() => new SimpleModelContext());

            using (var context = new SimpleModelContext())
            {
                // Begin a local transaction
                var transaction = BeginLocalTransaction(context);

                var product = new Product() { Name = "New Tea" };
                context.Products.Add(product);
                context.SaveChanges();

                Assert.True(context.Products.Where(p => p.Name == "New Tea").AsNoTracking().Any());

                // Rollback local transaction
                transaction.Rollback();
                CloseEntityConnection(context);

                Assert.False(context.Products.Where(p => p.Name == "New Tea").AsNoTracking().Any());
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

                var product = new Product { Name = "Fanta" };
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

        #endregion Transaction scenarios
    }
}