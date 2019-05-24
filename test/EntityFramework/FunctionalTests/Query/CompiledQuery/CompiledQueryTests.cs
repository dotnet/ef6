namespace System.Data.Entity.Query.CompiledQuery
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Functionals.Utilities;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Objects;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Linq.Expressions;
    using Xunit;

    public class CompiledQueryBlog
    {
        public int Id { get; set; }
        public string Title { get; set; }

    }

    public class CompiledQueryContext : ObjectContext
    {
        public CompiledQueryContext(string connectionString)
            : base(connectionString)
        {
        }

        public CompiledQueryContext(EntityConnection connection)
            : base(connection)
        {
        }

        public IQueryable<CompiledQueryBlog> Blogs
        {
            get { return CreateObjectSet<CompiledQueryBlog>("Entities.CompiledQueryBlog"); }
        }
    }

    public class CompiledQueryTests : FunctionalTestBase, IClassFixture<CompiledQueryFixture>
    {
        private string _entityConnectionString;
        private string _connectionString;
        private string _modelDirectory;
        private MetadataWorkspace _workspace;
        private DbCompiledModel _compiledModel;

        public CompiledQueryTests(CompiledQueryFixture data)
        {
            _compiledModel = data.CompiledModel;
            _connectionString = data.ConnectionString;

            _entityConnectionString = string.Format(
                @"metadata=res://EntityFramework.FunctionalTests/System.Data.Entity.Query.CompiledQuery.CompiledQueryModel.csdl|res://EntityFramework.FunctionalTests/System.Data.Entity.Query.CompiledQuery.CompiledQueryModel.ssdl|res://EntityFramework.FunctionalTests/System.Data.Entity.Query.CompiledQuery.CompiledQueryModel.msl;provider=System.Data.SqlClient;provider connection string=""{0}""",
                _connectionString);
        }


        private CompiledQueryContext CreateCompiledQueryContext()
        {
            var ctx = new CompiledQueryContext(_entityConnectionString);
            ctx.MetadataWorkspace.LoadFromAssembly(GetType().Assembly());

            return ctx;
        }

        [Fact(
#if NETCOREAPP3_0
            Skip = "#860"
#endif
            )]
        public void CompiledQuery_with_contains_does_not_hold_reference_to_context()
        {
            WeakReference wr;

            CompiledQuery_with_contains_does_not_hold_reference_to_context_Test(out wr);

            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.False(wr.IsAlive);
        }

        public void CompiledQuery_with_contains_does_not_hold_reference_to_context_Test(out WeakReference wr)
        {
            using (var context = CreateCompiledQueryContext())
            {
                wr = new WeakReference(context);

                Expression<Func<CompiledQueryContext, string, string, IEnumerable<CompiledQueryBlog>>> expression =
                    (ctx, fake, prm) =>
                    (from b in ctx.Blogs
                     where b.Title.Contains(prm)
                     select b);

                var cq = CompiledQuery.Compile(expression);
                var query = cq(context, "What-everrrr", "Foo");
                var result = query.ToList();
            }
        }
    }

    public class CompiledQueryFixture : FunctionalTestBase
    {
        public DbCompiledModel CompiledModel { get; private set; }
        public string ConnectionString { get; private set; }
        private const string DatabaseName = "CompiledQueryTests";

        public CompiledQueryFixture()
        {
            using (var masterConnection = new SqlConnection(ModelHelpers.SimpleConnectionString("master")))
            {
                masterConnection.Open();

                var databaseExistsScript = string.Format(
                    "SELECT COUNT(*) FROM sys.databases where name = '{0}'", DatabaseName);

                var databaseExists = (int)new SqlCommand(databaseExistsScript, masterConnection).ExecuteScalar() == 1;
                if (databaseExists)
                {
                    var dropDatabaseScript = string.Format("drop database {0}", DatabaseName);
                    new SqlCommand(dropDatabaseScript, masterConnection).ExecuteNonQuery();
                }

                var createDatabaseScript = string.Format("create database {0}", DatabaseName);
                new SqlCommand(createDatabaseScript, masterConnection).ExecuteNonQuery();
            }

            ConnectionString = ModelHelpers.SimpleConnectionString(DatabaseName);
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var ddlSql =
                    @"CREATE TABLE [dbo].[CompiledQueryBlog](
  [Id] [int] IDENTITY(1,1) NOT NULL,
  [Title] [nvarchar] (100) NOT NULL,
CONSTRAINT [PK_CompiledQueryBlog] PRIMARY KEY CLUSTERED ([Id] ASC))";

                new SqlCommand(ddlSql, connection).ExecuteNonQuery();

                var seedSql =
                    @"INSERT INTO [dbo].[CompiledQueryBlog](Title) VALUES ('Foo')
INSERT INTO [dbo].[CompiledQueryBlog](Title) VALUES ('Bar')";

                new SqlCommand(seedSql, connection).ExecuteNonQuery();

                var builder = new DbModelBuilder();
                builder.Entity<TransactionLogEntry>().ToTable("CompiledQueryBlog");
                builder.HasDefaultSchema("CompiledQueryModel");
                builder.HasDefaultSchema("Entities");
                builder.Conventions.Remove<ModelContainerConvention>();

                var model = builder.Build(connection);
                CompiledModel = model.Compile();
            }
        }
    }
}
