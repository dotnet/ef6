// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ProductivityApi
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Threading;
    using SimpleModel;
    using Xunit;

#if !NET40

    /// <summary>
    /// Tests that check that ConfigureAwait(false) is called before each await
    /// </summary>
    public class DeadlockTests : TestBase
    {
        [Fact]
        public void SqlQuery_ToListAsync_does_not_deadlock()
        {
            using (var context = new SimpleModelContext())
            {
                var query = context.Database.SqlQuery<int>("select Id from Products");

                RunDeadlockTest(query.ToListAsync);
            }
        }

        [Fact]
        public void DbQuery_ToListAsync_does_not_deadlock()
        {
            using (var context = new SimpleModelContext())
            {
                RunDeadlockTest(context.Products.ToListAsync);
            }
        }

        [Fact]
        public void DbSet_FindAsync_does_not_deadlock()
        {
            using (var context = new SimpleModelContext())
            {
                RunDeadlockTest(() => context.Products.FindAsync(0));
            }
        }

        [Fact]
        public void Database_ExecuteSqlCommandAsync_does_not_deadlock()
        {
            using (var context = new SimpleModelContext())
            {
                RunDeadlockTest(() => context.Database.ExecuteSqlCommandAsync("select Id from Products"));
            }
        }

        [Fact]
        public void DbEntityEntry_ReloadAsync_does_not_deadlock()
        {
            using (var context = new SimpleModelContext())
            {
                RunDeadlockTest(
                    async () =>
                        {
                            await context.Products.LoadAsync().ConfigureAwait(false);
                            await context.ChangeTracker.Entries().First().ReloadAsync().ConfigureAwait(false);
                        });
            }
        }

        [Fact]
        public void DbContext_SaveChangesAsync_does_not_deadlock()
        {
            using (var context = new SimpleModelContext())
            {
                using (context.Database.BeginTransaction())
                {
                    context.Products.Add(new Product());
                    RunDeadlockTest(context.SaveChangesAsync);
                }
            }
        }

        private void RunDeadlockTest(Func<Task> asyncOperation)
        {
            Assert.Null(SynchronizationContext.Current);
            var dispatcherThread = new Thread(Dispatcher.Run) { Name = "Dispatcher" };
            dispatcherThread.Start();

            Dispatcher dispatcher = null;
            // Wait for dispatcher to start up
            while ((dispatcher = Dispatcher.FromThread(dispatcherThread)) == null)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(15));
            }

            try
            {
                Assert.Equal(
                    DispatcherOperationStatus.Completed,
                    dispatcher.InvokeAsync(
                        () =>
                        Assert.True(asyncOperation().Wait(TimeSpan.FromMinutes(1))))
                        .Wait(TimeSpan.FromMinutes(1.5)));
            }
            catch (TaskCanceledException)
            {
                // Sometimes thrown by the dispatcher, doesn't indicate a deadlock
            }
            finally
            {
                // Do our best to cleanup, but don't fail the test if not possible to do so in the allocated time
                dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
                var startShutdownTime = DateTime.Now;
                while (!dispatcher.HasShutdownFinished
                       && DateTime.Now - startShutdownTime < TimeSpan.FromSeconds(10))
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(100));
                }

                Task.Run(() => dispatcherThread.Abort()).Wait(TimeSpan.FromSeconds(10));
            }
        }
    }

#endif
}
