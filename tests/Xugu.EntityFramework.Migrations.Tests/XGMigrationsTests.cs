// Copyright (c) 2013, 2020 Oracle and/or its affiliates.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License, version 2.0, as
// published by the Free Software Foundation.
//
// This program is also distributed with certain software (including
// but not limited to OpenSSL) that is licensed under separate terms,
// as designated in a particular file or component or in included license
// documentation.  The authors of MySQL hereby grant you an
// additional permission to link the program and your derivative works
// with the separately licensed software that they have included with
// MySQL.
//
// Without limiting anything contained in the foregoing, this file,
// which is part of MySQL Connector/NET, is also subject to the
// Universal FOSS Exception, version 1.0, a copy of which can be found at
// http://oss.oracle.com/licenses/universal-foss-exception.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License, version 2.0, for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Linq;
using NUnit.Framework;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using XuguClient;
using Xugu.Data.EntityFramework;

namespace Xugu.EntityFramework.Migrations.Tests
{
    public class XGMigrationsTests : SetUpMigrationsTests
    {
        private XGProviderManifest ProviderManifest;

        public XGMigrationsTests()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<BlogContext, EF6Configuration>());
        }

        private XGConnection GetConnectionFromContext(DbContext ctx)
        {
            return (XGConnection)((EntityConnection)(((IObjectContextAdapter)ctx).ObjectContext.Connection)).StoreConnection;
        }

        /// <summary>
        /// Add int32 type column to existing table
        /// </summary>
        [Test]
        public void AddColumnOperationMigration()
        {
            var migrationOperations = new List<MigrationOperation>();

            if (ProviderManifest == null)
                ProviderManifest = new XGProviderManifest(Version.ToString());

            TypeUsage tu = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
            TypeUsage result = ProviderManifest.GetStoreType(tu);

            var intColumn = new ColumnModel(PrimitiveTypeKind.Int32, result)
            {
                Name = "TotalPosts",
                IsNullable = false
            };

            var addColumnMigratioOperation = new AddColumnOperation("Blogs", intColumn);
            migrationOperations.Add(addColumnMigratioOperation);

            using (BlogContext context = new BlogContext())
            {
                //context.Database.Connection.ConnectionString=BlogContext.GetEFConnectionString<BlogContext>();
                if (context.Database.Exists()) context.Database.Delete();
                context.Database.Create();

                using (XGConnection conn = new XGConnection(context.Database.Connection.ConnectionString))
                {
                    conn.ConnectionString=BlogContext.GetEFConnectionString<BlogContext>();
                    if (conn.State == System.Data.ConnectionState.Closed) conn.Open();
                    Assert.AreEqual(true, GenerateAndExecuteXuGuStatements(migrationOperations));

                    XGCommand query = new XGCommand("SELECT COL_NAME AS Column_name,CASE WHEN NOT_NULL THEN FALSE ELSE TRUE END AS Is_Nullable,TYPE_NAME AS Data_Type FROM ALL_Columns WHERE TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='Blogs' LIMIT 1) AND COL_NAME='TotalPosts'", conn);
                    XGDataReader reader = query.ExecuteReader();
                    while (reader.Read())
                    {
                        Assert.AreEqual("TotalPosts", reader.GetString(0));
                        Assert.AreEqual("F", reader.GetString(1));
                        Assert.AreEqual("INTEGER", reader.GetString(2));
                    }
                    reader.Close();
                    conn.Close();
                }
            }
        }

        [Test]
        public void RenameColumnOperationMigration()
        {
            var migrationOperations = new List<MigrationOperation>();

            if (ProviderManifest == null)
                ProviderManifest = new XGProviderManifest(Version.ToString());

            TypeUsage tu = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
            TypeUsage result = ProviderManifest.GetStoreType(tu);

            var addColumnMigratioOperation = new RenameColumnOperation("Blogs", "Title", "NewTitle");
            migrationOperations.Add(addColumnMigratioOperation);

            using (BlogContext context = new BlogContext())
            {
                //context.Database.Connection.ConnectionString=BlogContext.GetEFConnectionString<BlogContext>();
                if (context.Database.Exists()) context.Database.Delete();
                context.Database.Create();

                using (XGConnection conn = new XGConnection(context.Database.Connection.ConnectionString))
                {
                    conn.ConnectionString = BlogContext.GetEFConnectionString<BlogContext>();
                    if (conn.State == System.Data.ConnectionState.Closed) conn.Open();
                    Assert.AreEqual(true, GenerateAndExecuteXuGuStatements(migrationOperations));

                    XGCommand query = new XGCommand("SELECT COL_NAME AS Column_name,CASE WHEN NOT_NULL THEN FALSE ELSE TRUE END AS Is_Nullable,TYPE_NAME AS Data_Type FROM ALL_Columns WHERE TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='Blogs' LIMIT 1) AND COL_NAME='NewTitle'", conn);
                    XGDataReader reader = query.ExecuteReader();
                    while (reader.Read())
                    {
                        Assert.AreEqual("NewTitle", reader.GetString(0));
                        Assert.AreEqual("T", reader.GetString(1));
                        Assert.AreEqual("CLOB", reader.GetString(2));
                    }
                }
            }
        }

        /// <summary>
        /// CreateTable operation
        /// with the following columns int PostId string Title string Body 
        /// </summary>
        [Test]
        public void CreateTableOperationMigration()
        {

            var migrationOperations = new List<MigrationOperation>();
            var createTableOperation = CreateTableOperation();

            migrationOperations.Add(createTableOperation);

            using (BlogContext context = new BlogContext())
            {
                if (context.Database.Exists()) context.Database.Delete();
                context.Database.Create();

                using (var conn = new XGConnection(context.Database.Connection.ConnectionString))
                {
                    if (conn.State == System.Data.ConnectionState.Closed) conn.Open();
                    Assert.True(GenerateAndExecuteXuGuStatements(migrationOperations));
                    using (XGCommand query = new XGCommand($"SELECT COL_NAME AS Column_name FROM ALL_Columns WHERE TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='Posts' LIMIT 1)", conn))
                    {
                        using (XGDataReader reader = query.ExecuteReader())
                        {
                            while (reader.Read())
                                Assert.That(createTableOperation.Columns.Where(t => t.Name.Equals(reader[0].ToString())), Has.One.Items);
                            reader.Close();
                        }


                        query.CommandText = $"SELECT TYPE_NAME AS Data_Type FROM ALL_Columns WHERE TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='Posts' LIMIT 1) AND " +
                          $"COL_NAME = 'Password'";

                        StringAssert.AreEqualIgnoringCase("BINARY", query.ExecuteScalar().ToString());
                    }
                }
            }
        }

        /// <summary>
        /// CreateForeignKey operation
        /// between Blogs and Posts table
        /// </summary>
        [Test]
        public void CreateForeignKeyOperation()
        {
            var migrationOperations = new List<MigrationOperation>();

            // create dependant table Posts
            var createTableOperation = CreateTableOperation();
            migrationOperations.Add(createTableOperation);

            // Add column BlogId to create the constraints

            if (ProviderManifest == null)
                ProviderManifest = new XGProviderManifest(Version.ToString());

            TypeUsage tu = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
            TypeUsage result = ProviderManifest.GetStoreType(tu);

            var intColumn = new ColumnModel(PrimitiveTypeKind.Int32, result)
            {
                Name = "BlogId",
                IsNullable = false
            };

            var addColumnMigratioOperation = new AddColumnOperation("Posts", intColumn);
            migrationOperations.Add(addColumnMigratioOperation);

            // create constrain object
            var createForeignkeyOperation = new AddForeignKeyOperation();

            createForeignkeyOperation.Name = "FKBlogs";
            createForeignkeyOperation.DependentTable = "Posts";
            createForeignkeyOperation.DependentColumns.Add("BlogId");
            createForeignkeyOperation.CascadeDelete = true;
            createForeignkeyOperation.PrincipalTable = "Blogs";
            createForeignkeyOperation.PrincipalColumns.Add("BlogId");

            //create index to use
            migrationOperations.Add(createForeignkeyOperation.CreateCreateIndexOperation());

            migrationOperations.Add(createForeignkeyOperation);


            using (BlogContext context = new BlogContext())
            {

                if (context.Database.Exists()) context.Database.Delete();
                context.Database.Create();

                Assert.AreEqual(true, GenerateAndExecuteXuGuStatements(migrationOperations));

                using (var conn = new XGConnection(context.Database.Connection.ConnectionString))
                {
                    if (conn.State == System.Data.ConnectionState.Closed) conn.Open();
                    // check for foreign key creation
                    XGCommand query = new XGCommand("SELECT COUNT(*) FROM ALL_CONSTRAINTS WHERE CONS_TYPE='F' AND TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='Blogs' LIMIT 1);", conn);
                    int rows = Convert.ToInt32(query.ExecuteScalar());
                    Assert.AreEqual(0, rows);
                    // check for table creation          
                    query = new XGCommand("select Count(*) from all_tables WHERE `table_name` = 'Posts'", conn);
                    rows = Convert.ToInt32(query.ExecuteScalar());
                    Assert.AreEqual(1, rows);
                    conn.Close();
                }

                // Test fix for 
                //XGConnection con = GetConnectionFromContext(context);
                //con.Open();
                //try
                //{
                //    XGCommand cmd = new XGCommand("show create table `Posts`", con);
                //    using (XGDataReader r = cmd.ExecuteReader())
                //    {
                //        r.Read();
                //        string sql = r.GetString(1);
                //        Assert.True(sql.IndexOf(
                //          " CONSTRAINT `FKBlogs` FOREIGN KEY (`BlogId`) REFERENCES `blogs` (`BlogId`) ON DELETE CASCADE ON UPDATE CASCADE",
                //          StringComparison.OrdinalIgnoreCase) != -1);
                //    }
                //}
                //finally
                //{
                //    con.Close();
                //}
            }
        }


        /// <summary>
        /// Remove PK and the autoincrement property for the column
        /// </summary>

        [Test]
        public void DropPrimaryKeyOperationWithAnonymousArguments()
        {

            var migrationOperations = new List<MigrationOperation>();

            // create table where the PK exists
            var createTableOperation = CreateTableOperation();
            migrationOperations.Add(createTableOperation);

            var createDropPKOperation = new DropPrimaryKeyOperation(anonymousArguments: new { DeleteAutoIncrement = true });
            createDropPKOperation.Table = "Posts";
            createDropPKOperation.Columns.Add("PostId");
            migrationOperations.Add(createDropPKOperation);

            using (BlogContext context = new BlogContext())
            {
                if (context.Database.Exists()) context.Database.Delete();
                context.Database.Create();


                Assert.AreEqual(true, GenerateAndExecuteXuGuStatements(migrationOperations));

                using (var conn = new XGConnection(context.Database.Connection.ConnectionString))
                {
                    if (conn.State == System.Data.ConnectionState.Closed) conn.Open();

                    // check for table creation          
                    var query = new XGCommand("select Count(*) from ALL_TABLES WHERE `table_name` = 'Posts'", conn);
                    int rows = Convert.ToInt32(query.ExecuteScalar());
                    Assert.AreEqual(1, rows);

                    // check if PK exists          
                    query = new XGCommand("SELECT COUNT(*) FROM ALL_CONSTRAINTS WHERE CONS_TYPE='P' AND TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='Posts' LIMIT 1);", conn);
                    rows = Convert.ToInt32(query.ExecuteScalar());
                    Assert.AreEqual(0, rows);

                    //check the definition of the column that was PK
                    query = new XGCommand("SELECT COL_NAME AS Column_name,CASE WHEN NOT_NULL THEN FALSE ELSE TRUE END AS Is_Nullable,TYPE_NAME AS Data_Type FROM ALL_Columns WHERE TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='Blogs' LIMIT 1) AND COL_NAME='PostId'", conn);
                    XGDataReader reader = query.ExecuteReader();
                    while (reader.Read())
                    {
                        Assert.AreEqual("PostId", reader[0].ToString());
                        Assert.AreEqual("NO", reader[1].ToString());
                        Assert.AreEqual("int", reader[2].ToString());
                    }
                    reader.Close();
                    conn.Close();
                }
            }
        }


        /// <summary>
        /// Drop primary key. No anonymous arguments
        /// </summary>
        [Test]
        public void DropPrimaryKeyOperation()
        {

            var migrationOperations = new List<MigrationOperation>();

            // create table where the PK exists
            var createTableOperation = CreateTableOperation();
            migrationOperations.Add(createTableOperation);

            var createDropPKOperation = new DropPrimaryKeyOperation(anonymousArguments: new { DeleteAutoIncrement = true });
            createDropPKOperation.Table = "Posts";
            createDropPKOperation.Columns.Add("PostId");
            migrationOperations.Add(createDropPKOperation);

            using (BlogContext context = new BlogContext())
            {
                if (context.Database.Exists()) context.Database.Delete();
                context.Database.Create();


                Assert.AreEqual(true, GenerateAndExecuteXuGuStatements(migrationOperations));

                using (var conn = new XGConnection(context.Database.Connection.ConnectionString))
                {
                    if (conn.State == System.Data.ConnectionState.Closed) conn.Open();

                    // check for table creation          
                    var query = new XGCommand("select Count(*) from ALL_TABLES WHERE `table_name` = 'Posts'", conn);
                    int rows = Convert.ToInt32(query.ExecuteScalar());
                    Assert.AreEqual(1, rows);

                    // check if PK exists          
                    query = new XGCommand("SELECT COUNT(*) FROM ALL_CONSTRAINTS WHERE CONS_TYPE='P' AND TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='Posts' LIMIT 1);", conn);
                    rows = Convert.ToInt32(query.ExecuteScalar());
                    Assert.AreEqual(0, rows);

                    //check the definition of the column that was PK
                    query = new XGCommand("SELECT COL_NAME AS Column_name,CASE WHEN NOT_NULL THEN FALSE ELSE TRUE END AS Is_Nullable,TYPE_NAME AS Data_Type FROM ALL_Columns WHERE TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='Blogs' LIMIT 1) AND COL_NAME='PostId'", conn);
                    XGDataReader reader = query.ExecuteReader();
                    while (reader.Read())
                    {
                        Assert.AreEqual("PostId", reader[0].ToString());
                        Assert.AreEqual("NO", reader[1].ToString());
                        Assert.AreEqual("int", reader[2].ToString());
                    }
                    reader.Close();
                    conn.Close();
                }
            }
        }


        /// <summary>
        /// Creates a table named Posts 
        /// and columns int PostId, string Title, string Body 
        /// </summary>
        /// <returns></returns>

        private CreateTableOperation CreateTableOperation()
        {
            TypeUsage tu;
            TypeUsage result;

            if (ProviderManifest == null)
                ProviderManifest = new XGProviderManifest(Version.ToString());

            var createTableOperation = new CreateTableOperation("Posts");

            //Column model for int IdPost
            tu = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
            result = ProviderManifest.GetStoreType(tu);

            var intColumn = new ColumnModel(PrimitiveTypeKind.Int32, result)
            {
                Name = "PostId",
                IsNullable = false,
                IsIdentity = true
            };

            createTableOperation.Columns.Add(intColumn);

            //Column model for string 
            tu = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            result = ProviderManifest.GetStoreType(tu);

            var stringColumnTitle = new ColumnModel(PrimitiveTypeKind.String, result)
            {
                Name = "Title",
                IsNullable = false
            };

            var stringColumnBody = new ColumnModel(PrimitiveTypeKind.String, result)
            {
                Name = "Body",
                IsNullable = true
            };

            createTableOperation.Columns.Add(stringColumnTitle);
            createTableOperation.Columns.Add(stringColumnBody);

            //Column model for binary 
            tu = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary));
            result = ProviderManifest.GetStoreType(tu);

            var binaryColumn = new ColumnModel(PrimitiveTypeKind.Binary, result)
            {
                Name = "Password",
                MaxLength = 10,
                StoreType = "binary"
            };

            createTableOperation.Columns.Add(binaryColumn);

            var primaryKey = new AddPrimaryKeyOperation();

            primaryKey.Columns.Add("PostId");

            createTableOperation.PrimaryKey = primaryKey;

            return createTableOperation;

        }


        /// <summary>
        /// Generate and apply sql statemens from the
        /// migration operations list
        /// return false is case of fail or if database doesn't exist
        /// </summary>
        private bool GenerateAndExecuteXuGuStatements(List<MigrationOperation> migrationOperations)
        {
            XGProviderServices ProviderServices;

            ProviderServices = new XGProviderServices();

            using (BlogContext context = new BlogContext())
            {
                if (!context.Database.Exists()) return false;

                using (XGConnection conn = new XGConnection(context.Database.Connection.ConnectionString))
                {
                    var migratorGenerator = new XGMigrationSqlGenerator();
                    var Token = ProviderServices.GetProviderManifestToken(conn);
                    var sqlStmts = migratorGenerator.Generate(migrationOperations, providerManifestToken: Token);
                    if (conn.State == System.Data.ConnectionState.Closed) conn.Open();
                    foreach (MigrationStatement stmt in sqlStmts)
                    {
                        try
                        {
                            XGCommand cmd = new XGCommand(stmt.Sql, conn);
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}