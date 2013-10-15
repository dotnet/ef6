// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.WrappingProvider
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Functionals.Utilities;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.TestHelpers;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class WrappingProviderTests : FunctionalTestBase, IDisposable
    {
        private const string SqlClientInvariantName = "System.Data.SqlClient";

        private static readonly DataTable _providerTable =
            (DataTable)typeof(DbProviderFactories).GetDeclaredMethod("GetProviderTable")
                                                  .Invoke(null, BindingFlags.NonPublic | BindingFlags.Static, null, null, null);

        public WrappingProviderTests()
        {
            RegisterResolvers();
        }

        public void Dispose()
        {
            MutableResolver.ClearResolvers();
            RegisterAdoNetProvider(typeof(SqlClientFactory));
        }

        [Fact]
        public void Wrapping_provider_can_be_found_using_net40_style_table_lookup_even_after_first_asking_for_non_wrapped_provider()
        {
            MutableResolver.AddResolver<IDbProviderFactoryResolver>(
                new SingletonDependencyResolver<IDbProviderFactoryResolver>(
                    (IDbProviderFactoryResolver)Activator.CreateInstance(
                        typeof(DbContext).Assembly().GetTypes().Single(t => t.Name == "Net40DefaultDbProviderFactoryResolver"), nonPublic: true)));

            Assert.Same(
                SqlClientFactory.Instance,
                DbConfiguration.DependencyResolver.GetService<IDbProviderFactoryResolver>()
                               .ResolveProviderFactory(new SqlConnection()));

            RegisterAdoNetProvider(typeof(WrappingAdoNetProvider<SqlClientFactory>));

            Assert.Same(
                WrappingAdoNetProvider<SqlClientFactory>.Instance,
                DbConfiguration.DependencyResolver.GetService<IDbProviderFactoryResolver>()
                               .ResolveProviderFactory(new WrappingConnection<SqlClientFactory>(new SqlConnection())));
        }

        [Fact]
        public void Correct_services_are_returned_when_setup_by_replacing_ADO_NET_provider()
        {
            RegisterAdoNetProvider(typeof(WrappingAdoNetProvider<SqlClientFactory>));
            MutableResolver.AddResolver<DbProviderServices>(k => WrappingEfProvider<SqlClientFactory, SqlProviderServices>.Instance);

            Assert.Same(
                WrappingAdoNetProvider<SqlClientFactory>.Instance,
                DbProviderFactories.GetFactory(SqlClientInvariantName));

            Assert.Same(
                WrappingAdoNetProvider<SqlClientFactory>.Instance,
                DbConfiguration.DependencyResolver.GetService<DbProviderFactory>(SqlClientInvariantName));

            Assert.Same(
                WrappingEfProvider<SqlClientFactory, SqlProviderServices>.Instance,
                DbConfiguration.DependencyResolver.GetService<DbProviderServices>(SqlClientInvariantName));

            Assert.Equal(
                SqlClientInvariantName,
                DbConfiguration.DependencyResolver.GetService<IProviderInvariantName>(WrappingAdoNetProvider<SqlClientFactory>.Instance).Name);

            Assert.Same(
                WrappingAdoNetProvider<SqlClientFactory>.Instance,
                DbConfiguration.DependencyResolver.GetService<IDbProviderFactoryResolver>()
                               .ResolveProviderFactory(new WrappingConnection<SqlClientFactory>(new SqlConnection())));
        }

        [Fact]
        public void Correct_services_are_returned_when_wrapping_setup_at_EF_level_only()
        {
            WrappingAdoNetProvider<SqlClientFactory>.WrapProviders();

            Assert.Same(
                WrappingAdoNetProvider<SqlClientFactory>.Instance,
                DbConfiguration.DependencyResolver.GetService<DbProviderFactory>(SqlClientInvariantName));

            Assert.Same(
                WrappingEfProvider<SqlClientFactory, SqlProviderServices>.Instance,
                DbConfiguration.DependencyResolver.GetService<DbProviderServices>(SqlClientInvariantName));

            Assert.Equal(
                SqlClientInvariantName,
                DbConfiguration.DependencyResolver.GetService<IProviderInvariantName>(WrappingAdoNetProvider<SqlClientFactory>.Instance).Name);

            Assert.Same(
                WrappingAdoNetProvider<SqlClientFactory>.Instance,
                DbConfiguration.DependencyResolver.GetService<IDbProviderFactoryResolver>()
                               .ResolveProviderFactory(new WrappingConnection<SqlClientFactory>(new SqlConnection())));

            // Should still report what is in the providers table
            Assert.Same(SqlClientFactory.Instance, DbProviderFactories.GetFactory(SqlClientInvariantName));
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Simple_query_and_update_works_with_wrapping_provider_setup_by_replacing_ADO_NET_provider()
        {
            RegisterAdoNetProvider(typeof(WrappingAdoNetProvider<SqlClientFactory>));
            MutableResolver.AddResolver<DbProviderServices>(k => WrappingEfProvider<SqlClientFactory, SqlProviderServices>.Instance);
            MutableResolver.AddResolver<Func<MigrationSqlGenerator>>(WrappingEfProvider<SqlClientFactory, SqlProviderServices>.Instance);
            MutableResolver.AddResolver<IDbProviderFactoryResolver>(k => new WrappingProviderFactoryResolver<SqlClientFactory>());

            var log = WrappingAdoNetProvider<SqlClientFactory>.Instance.Log;
            log.Clear();

            using (var context = new AdoLevelBlogContext())
            {
                var blog = context.Blogs.Single();
                Assert.Equal("Half a Unicorn", blog.Title);
                Assert.Equal("Wrap it up...", blog.Posts.Single().Title);

                using (context.Database.BeginTransaction())
                {
                    blog.Posts.Add(
                        new Post
                            {
                                Title = "Throw it away..."
                            });
                    Assert.Equal(1, context.SaveChanges());
                    Assert.Equal(
                        new[] { "Throw it away...", "Wrap it up..." },
                        context.Posts.AsNoTracking().Select(p => p.Title).OrderBy(t => t));
                }
            }

            // Sanity check that the wrapping provider really did get used
            var methods = log.Select(i => i.Method).ToList();
            Assert.Contains("ExecuteReader", methods);
            Assert.Contains("ExecuteNonQuery", methods);
            Assert.Contains("Open", methods);
            Assert.Contains("Close", methods);
            Assert.Contains("Commit", methods);
            Assert.Contains("Generate", methods);
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Simple_query_and_update_works_with_wrapping_provider_setup_at_EF_level_only()
        {
            WrappingAdoNetProvider<SqlClientFactory>.WrapProviders();

            var log = WrappingAdoNetProvider<SqlClientFactory>.Instance.Log;
            log.Clear();

            using (var context = new EfLevelBlogContext())
            {
                var blog = context.Blogs.Single();
                Assert.Equal("Half a Unicorn", blog.Title);
                Assert.Equal("Wrap it up...", blog.Posts.Single().Title);

                using (context.Database.BeginTransaction())
                {
                    blog.Posts.Add(
                        new Post
                            {
                                Title = "Throw it away..."
                            });
                    Assert.Equal(1, context.SaveChanges());

                    Assert.Equal(
                        new[] { "Throw it away...", "Wrap it up..." },
                        context.Posts.AsNoTracking().Select(p => p.Title).OrderBy(t => t));
                }
            }

            // Sanity check that the wrapping provider really did get used
            var methods = log.Select(i => i.Method).ToList();
            Assert.Contains("ExecuteReader", methods);
            Assert.Contains("ExecuteNonQuery", methods);
            Assert.Contains("Open", methods);
            Assert.Contains("Close", methods);
            Assert.Contains("Commit", methods);
            Assert.Contains("Generate", methods);
        }

        public class Blog
        {
            public int Id { get; set; }
            public string Title { get; set; }

            public virtual ICollection<Post> Posts { get; set; }
        }

        public class Post
        {
            public int Id { get; set; }
            public string Title { get; set; }

            public int BlogId { get; set; }
            public virtual Blog Blog { get; set; }
        }

        public class AdoLevelBlogContext : BlogContext
        {
            static AdoLevelBlogContext()
            {
                Database.SetInitializer<AdoLevelBlogContext>(new BlogInitializer());
            }
        }

        public class EfLevelBlogContext : BlogContext
        {
            static EfLevelBlogContext()
            {
                Database.SetInitializer<EfLevelBlogContext>(new BlogInitializer());
            }
        }

        public abstract class BlogContext : DbContext
        {
            public DbSet<Blog> Blogs { get; set; }
            public DbSet<Post> Posts { get; set; }
        }

        public class BlogInitializer : DropCreateDatabaseAlways<BlogContext>
        {
            protected override void Seed(BlogContext context)
            {
                context.Posts.Add(
                    new Post
                        {
                            Title = "Wrap it up...",
                            Blog = new Blog
                                {
                                    Title = "Half a Unicorn"
                                }
                        });
            }
        }

        private static void RegisterAdoNetProvider(Type providerFactoryType)
        {
            var row = _providerTable.NewRow();
            row["Name"] = "SqlClient Data Provider";
            row["Description"] = ".Net Framework Data Provider for SqlServer";
            row["InvariantName"] = SqlClientInvariantName;
            row["AssemblyQualifiedName"] = providerFactoryType.AssemblyQualifiedName;

            _providerTable.Rows.Remove(_providerTable.Rows.Find(SqlClientInvariantName));
            _providerTable.Rows.Add(row);
        }

        private static void RegisterResolvers()
        {
            // We register new resolvers that match the defaults because the defaults cache values such as
            // the ADO.NET provider registered in the factories table that we wish to temporarily change.

            MutableResolver.AddResolver<DbProviderServices>(
                (IDbDependencyResolver)Activator.CreateInstance(
                    typeof(DbContext).Assembly().GetTypes().Single(t => t.Name == "DefaultProviderServicesResolver"), nonPublic: true));

            MutableResolver.AddResolver<DbProviderFactory>(
                (IDbDependencyResolver)Activator.CreateInstance(
                    typeof(DbContext).Assembly().GetTypes().Single(t => t.Name == "DefaultProviderFactoryResolver"), nonPublic: true));

            MutableResolver.AddResolver<IProviderInvariantName>(
                (IDbDependencyResolver)Activator.CreateInstance(
                    typeof(DbContext).Assembly().GetTypes().Single(t => t.Name == "DefaultInvariantNameResolver"), nonPublic: true));
        }
    }
}
