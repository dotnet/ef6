// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ProductivityApi
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.TestHelpers;
    using System.Data.Entity.WrappingProvider;
    using System.Data.SqlClient;
    using System.Linq;
    using Xunit;

    public class TimeoutTests : FunctionalTestBase, IDisposable
    {
        private readonly IList<LogItem> _log;

        public TimeoutTests()
        {
            WrappingAdoNetProvider<SqlClientFactory>.WrapProviders();

            using (var context = new TimeoutContext())
            {
                context.Database.Initialize(force: false);
                context.Database.CreateIfNotExists();
            }

            _log = WrappingAdoNetProvider<SqlClientFactory>.Instance.Log;
            _log.Clear();
        }

        public void Dispose()
        {
            MutableResolver.ClearResolvers();
        }

        [Fact]
        public void Default_timeout_is_used_if_none_is_set()
        {
            using (var context = new TimeoutContext())
            {
                context.Time.Load();

                Assert.Equal("30", GetLoggedTimeout());
            }
        }

        [Fact]
        public void DbContext_timeout_is_used_for_queries()
        {
            using (var context = new TimeoutContext())
            {
                context.Database.CommandTimeout = 66;
                context.Time.Load();

                Assert.Equal("66", GetLoggedTimeout());
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void DbContext_timeout_is_used_for_updates()
        {
            ExtendedSqlAzureExecutionStrategy.ExecuteNew(
                () =>
                {
                    using (var context = new TimeoutContext())
                    {
                        context.Database.CommandTimeout = 66;

                        using (context.Database.BeginTransaction())
                        {
                            context.Space.Add(new SomeSpace());
                            context.SaveChanges();
                        }

                        Assert.Equal("66", GetLoggedTimeout());
                    }
                });
        }

        [Fact]
        public void DbContext_timeout_is_used_for_lazy_loading()
        {
            using (var context = new TimeoutContext())
            {
                context.Database.CommandTimeout = 66;
                var _ = context.Space.Attach(context.Space.Create()).Time;

                Assert.Equal("66", GetLoggedTimeout());
            }
        }

        [Fact]
        public void DbContext_timeout_is_used_for_explicit_loading()
        {
            using (var context = new TimeoutContext())
            {
                context.Database.CommandTimeout = 66;
                context.Entry(context.Space.Attach(context.Space.Create())).Collection(e => e.Time).Load();

                Assert.Equal("66", GetLoggedTimeout());
            }
        }

        [Fact]
        public void DbContext_timeout_is_used_for_Find()
        {
            using (var context = new TimeoutContext())
            {
                context.Database.CommandTimeout = 66;
                context.Time.Find(0);

                Assert.Equal("66", GetLoggedTimeout());
            }
        }

        [Fact]
        public void DbContext_timeout_is_used_for_generic_DbSet_SqlQuery()
        {
            using (var context = new TimeoutContext())
            {
                context.Database.CommandTimeout = 66;
                context.Time.SqlQuery("select * from SomeTimes").ToList();

                Assert.Equal("66", GetLoggedTimeout());
            }
        }

        [Fact]
        public void DbContext_timeout_is_used_for_non_generic_DbSet_SqlQuery()
        {
            using (var context = new TimeoutContext())
            {
                context.Database.CommandTimeout = 66;
                context.Set(typeof(SomeTime)).SqlQuery("select * from SomeTimes").ToList<SomeTime>();

                Assert.Equal("66", GetLoggedTimeout());
            }
        }

        [Fact]
        public void DbContext_timeout_is_used_for_generic_Database_SqlQuery()
        {
            using (var context = new TimeoutContext())
            {
                context.Database.CommandTimeout = 66;
                context.Database.SqlQuery<SomeTime>("select * from SomeTimes").ToList<SomeTime>();

                Assert.Equal("66", GetLoggedTimeout());
            }
        }

        [Fact]
        public void DbContext_timeout_is_used_for_non_generic_Database_SqlQuery()
        {
            using (var context = new TimeoutContext())
            {
                context.Database.CommandTimeout = 66;
                context.Database.SqlQuery(typeof(SomeTime), "select * from SomeTimes").ToList<SomeTime>();

                Assert.Equal("66", GetLoggedTimeout());
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void DbContext_timeout_is_used_for_Database_ExecuteSqlCommand()
        {
            ExtendedSqlAzureExecutionStrategy.ExecuteNew(
                () =>
                {
                    using (var context = new TimeoutContext())
                    {
                        context.Database.CommandTimeout = 66;

                        using (context.Database.BeginTransaction())
                        {
                            context.Database.ExecuteSqlCommand("update SomeTimes set SpaceId = 1 where Id = 1");
                        }

                        Assert.Equal("66", GetLoggedTimeout());
                    }
                });
        }

        [Fact]
        public void DbContext_timeout_is_used_for_Database_Exists()
        {
            using (var context = new TimeoutContext())
            {
                context.Database.CommandTimeout = 66;
                context.Database.Exists();

                Assert.Equal("66", GetLoggedDdlTimeout("DbDatabaseExists"));
            }
        }

        [Fact]
        public void DbContext_timeout_is_used_for_Database_CompatibleWithModel()
        {
            using (var context = new TimeoutContext())
            {
                context.Database.CommandTimeout = 66;
                context.Database.CompatibleWithModel(throwIfNoMetadata: false);

                Assert.True(GetAllLoggedTimeouts().Any());
                Assert.True(GetAllLoggedTimeouts().All(t => t == "66"));
            }
        }

        [Fact]
        public void DbContext_timeout_is_used_for_Database_Create()
        {
            using (var context = new TimeoutContext())
            {
                context.Database.Delete();
                _log.Clear();

                context.Database.CommandTimeout = 66;
                context.Database.Create();

                Assert.Equal("66", GetLoggedDdlTimeout("DbCreateDatabase"));
            }
        }

        [Fact]
        public void DbContext_timeout_is_used_for_Database_CreateIfNotExists()
        {
            using (var context = new TimeoutContext())
            {
                context.Database.Delete();
                _log.Clear();

                context.Database.CommandTimeout = 66;
                context.Database.CreateIfNotExists();

                Assert.Equal("66", GetLoggedDdlTimeout("DbCreateDatabase"));
                Assert.True(GetAllLoggedTimeouts().All(t => t == "66"));
            }
        }

        [Fact]
        public void DbContext_timeout_is_used_for_Database_Delete()
        {
            using (var context = new TimeoutContext())
            {
                context.Database.CommandTimeout = 66;
                context.Database.Delete();

                Assert.Equal("66", GetLoggedDdlTimeout("DbDeleteDatabase"));
            }
        }

        [Fact]
        public void DbContext_timeout_is_used_for_database_initialization()
        {
            using (var context = new TimeoutContext())
            {
                context.Database.Delete();
                _log.Clear();

                context.Database.CommandTimeout = 66;
                context.Database.Initialize(force: true);

                Assert.True(GetAllLoggedTimeouts().Any());
                Assert.True(GetAllLoggedTimeouts().All(t => t == "66"));
            }
        }

#if !NET40

        [Fact]
        public void DbContext_timeout_is_used_for_async_queries()
        {
            using (var context = new TimeoutContext())
            {
                context.Database.CommandTimeout = 66;
                context.Time.LoadAsync().Wait();

                Assert.Equal("66", GetLoggedTimeout());
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void DbContext_timeout_is_used_for_async_updates()
        {
            ExtendedSqlAzureExecutionStrategy.ExecuteNew(
                () =>
                {
                    using (var context = new TimeoutContext())
                    {
                        context.Database.CommandTimeout = 66;

                        using (context.Database.BeginTransaction())
                        {
                            context.Space.Add(new SomeSpace());
                            context.SaveChangesAsync().Wait();
                        }

                        Assert.Equal("66", GetLoggedTimeout());
                    }
                });
        }

        [Fact]
        public void DbContext_timeout_is_used_for_async_explicit_loading()
        {
            using (var context = new TimeoutContext())
            {
                context.Database.CommandTimeout = 66;
                context.Entry(context.Space.Attach(context.Space.Create())).Collection(e => e.Time).LoadAsync().Wait();

                Assert.Equal("66", GetLoggedTimeout());
            }
        }

        [Fact]
        public void DbContext_timeout_is_used_for_async_Find()
        {
            using (var context = new TimeoutContext())
            {
                context.Database.CommandTimeout = 66;
                context.Time.FindAsync(0).Wait();

                Assert.Equal("66", GetLoggedTimeout());
            }
        }

        [Fact]
        public void DbContext_timeout_is_used_for_async_generic_DbSet_SqlQuery()
        {
            using (var context = new TimeoutContext())
            {
                context.Database.CommandTimeout = 66;
                context.Time.SqlQuery("select * from SomeTimes").ToListAsync().Wait();

                Assert.Equal("66", GetLoggedTimeout());
            }
        }

        [Fact]
        public void DbContext_timeout_is_used_for_async_non_generic_DbSet_SqlQuery()
        {
            using (var context = new TimeoutContext())
            {
                context.Database.CommandTimeout = 66;
                context.Set(typeof(SomeTime)).SqlQuery("select * from SomeTimes").ToListAsync().Wait();

                Assert.Equal("66", GetLoggedTimeout());
            }
        }

        [Fact]
        public void DbContext_timeout_is_used_for_async_generic_Database_SqlQuery()
        {
            using (var context = new TimeoutContext())
            {
                context.Database.CommandTimeout = 66;
                context.Database.SqlQuery<SomeTime>("select * from SomeTimes").ToListAsync().Wait();

                Assert.Equal("66", GetLoggedTimeout());
            }
        }

        [Fact]
        public void DbContext_timeout_is_used_for_async_non_generic_Database_SqlQuery()
        {
            using (var context = new TimeoutContext())
            {
                context.Database.CommandTimeout = 66;
                context.Database.SqlQuery(typeof(SomeTime), "select * from SomeTimes").ToListAsync().Wait();

                Assert.Equal("66", GetLoggedTimeout());
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void DbContext_timeout_is_used_for_async_Database_ExecuteSqlCommand()
        {
            ExtendedSqlAzureExecutionStrategy.ExecuteNew(
                () =>
                {
                    using (var context = new TimeoutContext())
                    {
                        context.Database.CommandTimeout = 66;

                        using (context.Database.BeginTransaction())
                        {
                            context.Database.ExecuteSqlCommandAsync("update SomeTimes set SpaceId = 1 where Id = 1").Wait();
                        }

                        Assert.Equal("66", GetLoggedTimeout());
                    }
                });
        }

#endif

        [Fact]
        public void Command_timeout_can_be_inherited_from_ObjectContext_and_can_be_changed_from_either_context()
        {
            using (var outerContext = new TimeoutContext())
            {
                var objectContext = ((IObjectContextAdapter)outerContext).ObjectContext;
                objectContext.CommandTimeout = 77;
                
                using (var context = new TimeoutContext(objectContext))
                {
                    Assert.Equal(77, context.Database.CommandTimeout);

                    context.Database.CommandTimeout = 88;
                    Assert.Equal(88, context.Database.CommandTimeout);
                    Assert.Equal(88, objectContext.CommandTimeout);

                    objectContext.CommandTimeout = 99;
                    Assert.Equal(99, context.Database.CommandTimeout);
                    Assert.Equal(99, objectContext.CommandTimeout);
                }
            }
        }

        private IEnumerable<string> GetAllLoggedTimeouts()
        {
            return _log.Where(i => i.Method == "Set CommandTimeout")
                       .Select(i => i.Details)
                       .Union(
                           _log.Where(i2 => new[] { "DbCreateDatabase", "DbDatabaseExists", "DbDeleteDatabase" }.Contains(i2.Method))
                               .Select(i2 => ((object[])i2.RawDetails)[0].ToString()));
        }

        private string GetLoggedTimeout()
        {
            return _log.Where(i => i.Method == "Set CommandTimeout").Select(i => i.Details).Single();
        }

        private string GetLoggedDdlTimeout(string method)
        {
            return _log.Where(i => i.Method == method).Select(i => ((object[])i.RawDetails)[0].ToString()).Single();
        }

        public class SomeTime
        {
            public int Id { get; set; }
            public int SpaceId { get; set; }
            public virtual SomeSpace Space { get; set; }
        }

        public class SomeSpace
        {
            public int Id { get; set; }
            public virtual ICollection<SomeTime> Time { get; set; }
        }

        public class TimeoutContext : DbContext
        {
            public TimeoutContext()
            {
            }

            public TimeoutContext(ObjectContext objectContext)
                : base(objectContext, dbContextOwnsObjectContext: false)
            {
            }

            public TimeoutContext(int? commandTimeout)
            {
                Database.CommandTimeout = commandTimeout;
            }

            public DbSet<SomeSpace> Space { get; set; }
            public DbSet<SomeTime> Time { get; set; }
        }
    }
}
