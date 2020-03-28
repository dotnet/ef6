// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Collections.Concurrent;
    using System.Data.Entity;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using SimpleModel;
    using Xunit;

    /// <summary>
    /// Functional tests that do various things with a <see cref="DbContext" /> using multiple threads
    /// such that we have at least some chance of finding issues in this code. As with any test of this
    /// type just because these tests pass does not mean that the code is correct. On the other hand,
    /// if any test ever fails (EVEN ONCE) then we know there is a problem to be investigated.
    /// </summary>
    public class MultiThreadingTests : FunctionalTestBase
    {
        #region Context initialization by multiple threads

        public class MultiInitContext1A : MultiInitContext1<MultiInitContext1A>
        {
        }

        public class MultiInitContext1B : MultiInitContext1<MultiInitContext1A>
        {
        }

        public class MultiInitContext1C : MultiInitContext1<MultiInitContext1A>
        {
        }

        public class MultiInitContext1D : MultiInitContext1<MultiInitContext1A>
        {
        }

        public class MultiInitContext1<TContext> : DbContext
            where TContext : DbContext
        {
            public MultiInitContext1()
            {
                Database.SetInitializer<TContext>(null);
            }

            public DbSet<Product> Products { get; set; }
        }

        [Fact]
        public void DbContext_initialization_should_work_when_called_concurrently_from_multiple_threads()
        {
            // This used to throw consistently when context initialization/model creation
            // was not thread safe. This test verifies that it does not throw anymore.
            ExecuteInParallel(() => new MultiInitContext1A().Products.Add(new Product()));
            ExecuteInParallel(() => new MultiInitContext1B().Products.Add(new Product()));
            ExecuteInParallel(() => new MultiInitContext1C().Products.Add(new Product()));
            ExecuteInParallel(() => new MultiInitContext1D().Products.Add(new Product()));
        }

        public class MultiInitContext2 : DbContext
        {
            public MultiInitContext2()
            {
                Database.SetInitializer<MultiInitContext2>(null);
            }

            public DbSet<Product> Products { get; set; }
        }

        [Fact]
        public void DbContext_initialization_using_Database_Initialize_should_work_when_called_concurrently_from_multiple_threads()
        {
            // This used to throw consistently when context initialization/model creation
            // was not thread safe. This test verifies that it does not throw anymore.
            ExecuteInParallel(() => new MultiInitContext2().Database.Initialize(force: false));
        }

        public class MultiInitContext3 : DbContext
        {
            public MultiInitContext3()
                : base(CreateConnection(), contextOwnsConnection: true)
            {
                Database.SetInitializer<MultiInitContext3>(null);
            }

            public DbSet<Product> Products { get; set; }

            private static int _count;
            private static readonly object _lock = new object();

            private static SqlConnection CreateConnection()
            {
                var connection = SimpleConnection<MultiInitContext3>();
                lock (_lock)
                {
                    _count++;
                    if (_count <= 5)
                    {
                        connection.Dispose();
                    }
                }
                return connection;
            }
        }

        [Fact]
        public void DbContext_initialization_should_work_when_called_concurrently_from_multiple_threads_even_if_first_the_first_few_fail()
        {
            try
            {
                ExecuteInParallel(() => new MultiInitContext3().Products.Add(new Product()));
            }
            catch (AggregateException ex)
            {
                // The number of times that the initialization fails is dependent on which of the threads with
                // bad connections actually attempt initialization before one with a good connection makes it work.
                Assert.True(ex.InnerExceptions.Count <= 5);
                foreach (var innerException in ex.InnerExceptions)
                {
                    Assert.IsType<ProviderIncompatibleException>(innerException);
                }
            }
        }

        public class MultiInitContextForCrud : DbContext
        {
            public MultiInitContextForCrud()
            {
                Database.SetInitializer(new DropCreateDatabaseIfModelChanges<MultiInitContextForCrud>());
            }

            public DbSet<Product> Products { get; set; }
        }

        [Fact]
        public void A_context_type_can_go_through_initialization_and_be_used_for_CRUD_from_multiple_threads()
        {
            // This test, unusually, modifies the database without being inside a transaction.
            // This is intentional since there are lots of threads and lots of connections working at the same
            // time here. The test is written in such a way that even if cleanup fails it should not impact
            // the test running again.
            ExecuteInParallel(
                () =>
                    {
                        int id;
                        var name = Thread.CurrentThread.ManagedThreadId.ToString();

                        using (var context = new MultiInitContextForCrud())
                        {
                            var product = context.Products.Add(
                                new Product
                                    {
                                        Name = name
                                    });
                            context.SaveChanges();
                            id = product.Id;
                            Assert.NotEqual(0, id);
                        }

                        using (var context = new MultiInitContextForCrud())
                        {
                            var product = context.Products.Find(id);
                            Assert.Equal(name, product.Name);
                            product.Name += "_Updated!";
                            context.SaveChanges();
                        }

                        using (var context = new MultiInitContextForCrud())
                        {
                            var product = context.Products.Where(p => p.Id == id).Single();
                            Assert.Equal(name + "_Updated!", product.Name);
                            context.Entry(product).State = EntityState.Deleted;
                            context.SaveChanges();
                        }

                        using (var context = new MultiInitContextForCrud())
                        {
                            Assert.Null(context.Products.Find(id));
                        }
                    });
        }

        public class SimpleModelContextForThreads : SimpleModelContext
        {
            public SimpleModelContextForThreads()
                : base(SimpleModelEntityConnectionString)
            {
            }
        }

        [Fact]
        public void EDMX_based_DbContext_initialization_should_work_when_called_concurrently_from_multiple_threads()
        {
            ExecuteInParallel(
                () =>
                    {
                        using (var context = new SimpleModelContextForThreads())
                        {
                            context.Products.ToString(); // Causes s-space and c/s loading

                            var workspace = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;
                            Assert.NotNull(workspace.GetItemCollection(DataSpace.OCSpace));
                            Assert.NotNull(workspace.GetItemCollection(DataSpace.CSSpace));
                            Assert.NotNull(workspace.GetItemCollection(DataSpace.SSpace));
                            Assert.NotNull(workspace.GetItemCollection(DataSpace.CSpace));
                        }
                    });
        }

        #endregion

        #region WriteEdmx

        [Fact]
        public void EDMX_can_be_written_from_multiple_threads_using_a_single_DbCompiledModel()
        {
            var edmxs = new ConcurrentBag<string>();
            ExecuteInParallel(
                () =>
                    {
                        var edmxBuilder = new StringBuilder();
                        using (var context = new SimpleModelContext())
                        {
                            // Cached DbCompiledModel will be used each time
                            EdmxWriter.WriteEdmx(context, XmlWriter.Create(edmxBuilder));
                        }

                        var edmx = edmxBuilder.ToString();

                        Assert.True(edmx.Contains("EntitySet Name=\"Products\""));
                        Assert.True(edmx.Contains("EntitySet Name=\"Categories\""));

                        edmxs.Add(edmx);
                    });

            Assert.True(edmxs.All(m => edmxs.First() == m));
        }

        #endregion

        #region Model hash

        [Fact]
        public void Model_hash_can_be_calculated_from_multiple_threads_using_a_single_DbCompiledModel()
        {
            var hashes = new ConcurrentBag<string>();
            ExecuteInParallel(
                () =>
                    {
                        using (var context = new SimpleModelContext())
                        {
#pragma warning disable 612,618
                            var hash = EdmMetadata.TryGetModelHash(context);
#pragma warning restore 612,618

                            Assert.NotNull(hash);
                            hashes.Add(hash);
                        }
                    });

            Assert.True(hashes.All(h => hashes.First() == h));
        }

        #endregion
    }
}
