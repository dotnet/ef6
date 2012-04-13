namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.Utilities;
    using System.Linq;
    using System.Xml.Linq;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    // TODO: SDE Merge - No CE Provider
    //[Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
        public class EmptyContextTests : DbTestCase
        {
            private static readonly XNamespace CsdlNamespace
                = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edm");

        [MigrationsTheory]
            public void Can_get_empty_model()
            {
                using (var connection = ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = ConnectionString;

                    using (var emptyContext = new EmptyContext(connection))
                    {
                        var model = emptyContext.GetModel();

                        var csdlSchemaNode = model.Descendants(CsdlNamespace + "Schema").Single();
                        var entityContainer = csdlSchemaNode.Descendants(CsdlNamespace + "EntityContainer").Single();
                        Assert.Equal(0, entityContainer.Descendants().Count());
                    }
                }
            }

        [MigrationsTheory]
            public void Getting_model_does_not_create_database()
            {
                using (var connection = ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = ConnectionString;

                    DropDatabase();

                    using (var emptyContext = new EmptyContext(connection))
                    {
                        emptyContext.GetModel();

                        Assert.False(Database.Exists(connection));
                    }
                }
            }
        }
    }
