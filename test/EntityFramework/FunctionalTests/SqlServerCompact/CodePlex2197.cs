namespace System.Data.Entity.SqlServerCompact
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.SqlServerCe;
    using Xunit;

    public class CodePlex2197 : TestBase
    {
        private const string TableDetailSql = @"
              SELECT 
                  t.CatalogName
              ,   t.SchemaName                           
              ,   t.Name
              ,   t.ColumnName
              ,   t.Ordinal
              ,   t.IsNullable
              ,   t.TypeName
              ,   t.MaxLength
              ,   t.Precision
              ,   t.DateTimePrecision
              ,   t.Scale
              ,   t.IsIdentity
              ,   t.IsStoreGenerated
              ,   CASE WHEN pk.IsPrimaryKey IS NULL THEN false ELSE pk.IsPrimaryKey END as IsPrimaryKey
            FROM (
              SELECT
                  t.CatalogName
              ,   t.SchemaName                           
              ,   t.Name
              ,   c.Id as ColumnId
              ,   c.Name as ColumnName
              ,   c.Ordinal
              ,   c.IsNullable
              ,   c.ColumnType.TypeName as TypeName
              ,   c.ColumnType.MaxLength as MaxLength
              ,   c.ColumnType.Precision as Precision
              ,   c.ColumnType.DateTimePrecision as DateTimePrecision
              ,   c.ColumnType.Scale as Scale
              ,   c.IsIdentity
              ,   c.IsStoreGenerated
              FROM
                  SchemaInformation.Tables as t 
                  cross apply 
                  t.Columns as c ) as t 
            LEFT OUTER JOIN (
              SELECT 
                  true as IsPrimaryKey
                , pkc.Id
              FROM
                  OfType(SchemaInformation.TableConstraints, Store.PrimaryKeyConstraint) as pk
                  CROSS APPLY pk.Columns as pkc) as pk
            ON t.ColumnId = pk.Id                   
            ";

        [Fact]
        public void When_Same_Primary_Key_Name_Is_Used_For_Two_Tables_Correct_Number_Of_MetaData_Rows_Is_Returned()
        {
            this.CreateIfNotExists();
            var workspace = CreateMetadataWorkspace();            
            var connection = new EntityConnection(workspace, new SqlCeConnection(dbConnectionString));
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = TableDetailSql;
            var rowCount = 0;

            using (var reader = command.ExecuteReader(CommandBehavior.SequentialAccess))
            {
                while (reader.Read())
                {
                    rowCount++;
                }
            }

            Assert.Equal(4, rowCount);
        }

        private const string dbFileName = "2197.sdf";
        private const string dbConnectionString = @"Data Source=|DataDirectory|\2197.sdf;";
        
        private void CreateIfNotExists()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", AppDomain.CurrentDomain.GetData("APPBASE"));
            var path = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();

            if (!System.IO.File.Exists(System.IO.Path.Combine(path, dbFileName)))    
            {   
                using (var engine = new SqlCeEngine(dbConnectionString))
                {
                    engine.CreateDatabase();
                }
                using (var conn = new SqlCeConnection(dbConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCeCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = @"CREATE TABLE [Categories] (
                          [Category ID] int IDENTITY (1,1) NOT NULL
                        , [Category Name] nvarchar(15) NOT NULL
                        );";
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = @"ALTER TABLE [Categories] ADD CONSTRAINT [Categories_PK] PRIMARY KEY ([Category ID]);";
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = @"CREATE TABLE [CategoriesTest] (
                          [Category ID] int IDENTITY (1,1) NOT NULL
                        , [Category Name] nvarchar(15) NOT NULL
                        );";
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = @"ALTER TABLE [CategoriesTest] ADD CONSTRAINT [Categories_PK] PRIMARY KEY ([Category ID]);";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private static MetadataWorkspace CreateMetadataWorkspace()
        {
            var sqlCeProviderServices = DbProviderServices.GetProviderServices(new SqlCeConnection());
            var sqlCeProviderManifest = sqlCeProviderServices.GetProviderManifest("4.0");

            var edmItemCollection = new EdmItemCollection(
                new[] { DbProviderServices.GetConceptualSchemaDefinition("ConceptualSchemaDefinition") });
            var storeItemCollection = new StoreItemCollection(
                new[] { sqlCeProviderManifest.GetInformation("StoreSchemaDefinition") });
            var mappingItemCollection = new StorageMappingItemCollection(edmItemCollection, storeItemCollection,
                new[] { sqlCeProviderManifest.GetInformation("StoreSchemaMapping") });

            return new MetadataWorkspace(() => edmItemCollection, () => storeItemCollection, () => mappingItemCollection);
        }
    }
}
